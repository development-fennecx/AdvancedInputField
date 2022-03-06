// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AdvancedInputFieldPlugin
{
	public enum MovementType
	{
		Unrestricted, // Unrestricted movement -- can scroll forever
		Elastic, // Restricted but flexible -- can go past the edges, but springs back in place
		Clamped, // Restricted movement where it's not possible to go past the edges
	}

	public enum ScrollbarVisibility
	{
		ALWAYS_HIDDEN,
		ALWAYS_VISIBLE,
		AUTOMATIC,
	}

	/// <summary>Custom ScrollRect class to handle smooth text scrolling</summary>
	[SelectionBase]
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RectTransform))]
	public class ScrollArea: UIBehaviour, ICanvasElement, ILayoutElement, ILayoutGroup, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
	{
		[Serializable]
		public class ScrollRectEvent: UnityEvent<Vector2> { }

		[SerializeField]
		private RectTransform content;
		public RectTransform Content { get { return content; } set { content = value; } }

		[SerializeField]
		private bool horizontal = true;
		public bool Horizontal { get { return horizontal; } set { horizontal = value; } }

		[SerializeField]
		private bool m_Vertical = true;
		public bool Vertical { get { return m_Vertical; } set { m_Vertical = value; } }

		[SerializeField]
		private MovementType movementType = MovementType.Elastic;
		public MovementType MovementType { get { return movementType; } set { movementType = value; } }

		[SerializeField]
		private float elasticity = 0.1f; // Only used for MovementType.Elastic
		public float Elasticity { get { return elasticity; } set { elasticity = value; } }

		[SerializeField]
		private bool inertia = true;
		public bool Inertia { get { return inertia; } set { inertia = value; } }

		[SerializeField]
		private float decelerationRate = 0.135f; // Only used when inertia is enabled
		public float DecelerationRate { get { return decelerationRate; } set { decelerationRate = value; } }

		[SerializeField]
		private float scrollSensitivity = 1.0f;
		public float ScrollSensitivity { get { return scrollSensitivity; } set { scrollSensitivity = value; } }

		[SerializeField]
		private RectTransform viewport;
		public RectTransform Viewport { get { return viewport; } set { viewport = value; SetDirtyCaching(); } }

		[SerializeField]
		private Scrollbar horizontalScrollbar;
		public Scrollbar HorizontalScrollbar
		{
			get
			{
				return horizontalScrollbar;
			}
			set
			{
				if(horizontalScrollbar)
				{
					horizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
				}
				horizontalScrollbar = value;
				if(horizontalScrollbar)
				{
					horizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
				}
				SetDirtyCaching();
			}
		}

		[SerializeField]
		private Scrollbar verticalScrollbar;
		public Scrollbar VerticalScrollbar
		{
			get
			{
				return verticalScrollbar;
			}
			set
			{
				if(verticalScrollbar)
				{
					verticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);
				}
				verticalScrollbar = value;
				if(verticalScrollbar)
				{
					verticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);
				}
				SetDirtyCaching();
			}
		}

		[SerializeField]
		private ScrollBarVisibilityMode horizontalScrollbarVisibility;
		public ScrollBarVisibilityMode HorizontalScrollbarVisibility { get { return horizontalScrollbarVisibility; } set { horizontalScrollbarVisibility = value; SetDirtyCaching(); } }

		[SerializeField]
		private ScrollBarVisibilityMode verticalScrollbarVisibility;
		public ScrollBarVisibilityMode VerticalScrollbarVisibility { get { return verticalScrollbarVisibility; } set { verticalScrollbarVisibility = value; SetDirtyCaching(); } }

		[SerializeField]
		private float horizontalScrollbarSpacing;
		public float HorizontalScrollbarSpacing { get { return horizontalScrollbarSpacing; } set { horizontalScrollbarSpacing = value; SetDirty(); } }

		[SerializeField]
		private float verticalScrollbarSpacing;
		public float VerticalScrollbarSpacing { get { return verticalScrollbarSpacing; } set { verticalScrollbarSpacing = value; SetDirty(); } }

		[SerializeField]
		private ScrollRectEvent onValueChanged = new ScrollRectEvent();
		public ScrollRectEvent OnValueChanged { get { return onValueChanged; } set { onValueChanged = value; } }

		// The offset from handle position to mouse down position
		private Vector2 m_PointerStartLocalCursor = Vector2.zero;
		protected Vector2 m_ContentStartPosition = Vector2.zero;

		private RectTransform m_ViewRect;

		protected RectTransform viewRect
		{
			get
			{
				if(m_ViewRect == null)
				{
					m_ViewRect = viewport;
				}
				if(m_ViewRect == null)
				{
					m_ViewRect = (RectTransform)transform;
				}
				return m_ViewRect;
			}
		}

		protected Bounds m_ContentBounds;
		private Bounds m_ViewBounds;

		private Vector2 m_Velocity;
		public Vector2 velocity { get { return m_Velocity; } set { m_Velocity = value; } }

		private bool m_Dragging;

		private Vector2 m_PrevPosition = Vector2.zero;
		private Bounds m_PrevContentBounds;
		private Bounds m_PrevViewBounds;
		[NonSerialized]
		private bool m_HasRebuiltLayout = false;

		private bool m_HSliderExpand;
		private bool m_VSliderExpand;
		private float m_HSliderHeight;
		private float m_VSliderWidth;

		[System.NonSerialized] private RectTransform m_Rect;
		private RectTransform rectTransform
		{
			get
			{
				if(m_Rect == null)
				{
					m_Rect = GetComponent<RectTransform>();
				}
				return m_Rect;
			}
		}

		private RectTransform m_HorizontalScrollbarRect;
		private RectTransform m_VerticalScrollbarRect;

		private DrivenRectTransformTracker m_Tracker;


		//Custom
		private float transitionTime;
		private Vector2 contentStartPosition;
		private Vector2 contentEndPosition;
		private float currentTransitionTime;
		private bool editMode;
		private DragMode dragMode;

		protected ScrollArea()
		{ }

		public bool EditMode
		{
			get { return editMode; }
			set
			{
				editMode = value;
				UpdateScrollbarVisibility();
			}
		}

		public DragMode DragMode
		{
			get { return dragMode; }
			set { dragMode = value; }
		}

		public void MoveContentHorizontally(Vector2 position, float xDistancePerSecond, float maxTransitionTime)
		{
			contentStartPosition = Content.anchoredPosition;
			contentEndPosition = position;
			float xDistance = Mathf.Abs(contentEndPosition.x - contentStartPosition.x);

			transitionTime = Mathf.Min(xDistance / xDistancePerSecond, maxTransitionTime);
			currentTransitionTime = 0;
		}

		public void MoveContentVertically(Vector2 position, float yDistancePerSecond, float maxTransitionTime)
		{
			contentStartPosition = Content.anchoredPosition;
			contentEndPosition = position;
			float yDistance = Mathf.Abs(contentEndPosition.y - contentStartPosition.y);

			transitionTime = Mathf.Min(yDistance / yDistancePerSecond, maxTransitionTime);
			currentTransitionTime = 0;
		}

		public void MoveContentImmediately(Vector2 position)
		{
			Content.anchoredPosition = position;
			currentTransitionTime = transitionTime; //Cancel transition if any
		}

		public void StopMoveContent()
		{
			currentTransitionTime = transitionTime; //Cancel transition if any
		}

		private void Update()
		{
			if(currentTransitionTime < transitionTime)
			{
				currentTransitionTime += Time.deltaTime;
				if(currentTransitionTime >= transitionTime)
				{
					currentTransitionTime = transitionTime;
				}

				float progress = currentTransitionTime / transitionTime;
				Content.anchoredPosition = Vector2.Lerp(contentStartPosition, contentEndPosition, progress);
			}
		}

		public virtual void Rebuild(CanvasUpdate executing)
		{
			if(executing == CanvasUpdate.Prelayout)
			{
				UpdateCachedData();
			}

			if(executing == CanvasUpdate.PostLayout)
			{
				UpdateBounds();
				UpdateScrollbars(Vector2.zero);
				UpdatePrevData();

				m_HasRebuiltLayout = true;
			}
		}

		public virtual void LayoutComplete()
		{ }

		public virtual void GraphicUpdateComplete()
		{ }

		void UpdateCachedData()
		{
			Transform transform = this.transform;
			m_HorizontalScrollbarRect = horizontalScrollbar == null ? null : horizontalScrollbar.transform as RectTransform;
			m_VerticalScrollbarRect = verticalScrollbar == null ? null : verticalScrollbar.transform as RectTransform;

			// These are true if either the elements are children, or they don't exist at all.
			bool viewIsChild = (viewRect.parent == transform);
			bool hScrollbarIsChild = (!m_HorizontalScrollbarRect || m_HorizontalScrollbarRect.parent == transform);
			bool vScrollbarIsChild = (!m_VerticalScrollbarRect || m_VerticalScrollbarRect.parent == transform);
			bool allAreChildren = (viewIsChild && hScrollbarIsChild && vScrollbarIsChild);

			//m_HSliderExpand = allAreChildren && m_HorizontalScrollbarRect && horizontalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
			//m_VSliderExpand = allAreChildren && m_VerticalScrollbarRect && verticalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
			m_HSliderExpand = false;
			m_VSliderExpand = false;
			m_HSliderHeight = (m_HorizontalScrollbarRect == null ? 0 : m_HorizontalScrollbarRect.rect.height);
			m_VSliderWidth = (m_VerticalScrollbarRect == null ? 0 : m_VerticalScrollbarRect.rect.width);
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			if(horizontalScrollbar)
			{
				horizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
			}
			if(verticalScrollbar)
			{
				verticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);
			}

			CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
			UpdateScrollbarVisibility();
		}

		protected override void OnDisable()
		{
			CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

			if(horizontalScrollbar)
			{
				horizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
			}
			if(verticalScrollbar)
			{
				verticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);
			}

			m_HasRebuiltLayout = false;
			m_Tracker.Clear();
			m_Velocity = Vector2.zero;
			LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
			base.OnDisable();
			UpdateScrollbarVisibility();
		}

		public override bool IsActive()
		{
			return base.IsActive() && content != null;
		}

		private void EnsureLayoutHasRebuilt()
		{
			if(!m_HasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
				Canvas.ForceUpdateCanvases();
		}

		public virtual void StopMovement()
		{
			m_Velocity = Vector2.zero;
		}

		public virtual void OnScroll(PointerEventData data)
		{
			if(dragMode == DragMode.UPDATE_TEXT_SELECTION)
			{
				IScrollHandler scrollHandler = transform.parent.GetComponentInParent<IScrollHandler>();
				if(scrollHandler != null)
				{
					scrollHandler.OnScroll(data);
				}
				return;
			}

			if(!IsActive())
			{
				return;
			}

			EnsureLayoutHasRebuilt();
			UpdateBounds();

			Vector2 delta = data.scrollDelta;
			// Down is positive for scroll events, while in UI system up is positive.
			delta.y *= -1;
			if(Vertical && !Horizontal)
			{
				if(Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
				{
					delta.y = delta.x;
				}
				delta.x = 0;
			}
			if(Horizontal && !Vertical)
			{
				if(Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
				{
					delta.x = delta.y;
				}
				delta.y = 0;
			}

			Vector2 position = content.anchoredPosition;
			position += delta * scrollSensitivity;
			if(movementType == MovementType.Clamped)
			{
				position += CalculateOffset(position - content.anchoredPosition);
			}

			SetContentAnchoredPosition(position);
			UpdateBounds();
		}

		public virtual void OnInitializePotentialDrag(PointerEventData eventData)
		{
			if(dragMode == DragMode.UPDATE_TEXT_SELECTION)
			{
				IInitializePotentialDragHandler initializePotentialDragHandler = transform.parent.GetComponentInParent<IInitializePotentialDragHandler>();
				if(initializePotentialDragHandler != null)
				{
					initializePotentialDragHandler.OnInitializePotentialDrag(eventData);
				}
				return;
			}

			if(eventData.button != PointerEventData.InputButton.Left)
			{
				return;
			}

			m_Velocity = Vector2.zero;
		}

		public virtual void OnBeginDrag(PointerEventData eventData)
		{
			if(dragMode == DragMode.UPDATE_TEXT_SELECTION)
			{
				IBeginDragHandler beginDragHandler = transform.parent.GetComponentInParent<IBeginDragHandler>();
				if(beginDragHandler != null)
				{
					beginDragHandler.OnBeginDrag(eventData);
				}
				return;
			}

			if(eventData.button != PointerEventData.InputButton.Left)
			{
				return;
			}

			if(!IsActive())
			{
				return;
			}

			UpdateBounds();

			m_PointerStartLocalCursor = Vector2.zero;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out m_PointerStartLocalCursor);
			m_ContentStartPosition = content.anchoredPosition;
			m_Dragging = true;
		}

		public virtual void OnEndDrag(PointerEventData eventData)
		{
			if(dragMode == DragMode.UPDATE_TEXT_SELECTION)
			{
				IEndDragHandler endDragHandler = transform.parent.GetComponentInParent<IEndDragHandler>();
				if(endDragHandler != null)
				{
					endDragHandler.OnEndDrag(eventData);
				}
				return;
			}

			if(eventData.button != PointerEventData.InputButton.Left)
			{
				return;
			}

			m_Dragging = false;
		}

		public virtual void OnDrag(PointerEventData eventData)
		{
			if(dragMode == DragMode.UPDATE_TEXT_SELECTION)
			{
				IDragHandler dragHandler = transform.parent.GetComponentInParent<IDragHandler>();
				if(dragHandler != null)
				{
					dragHandler.OnDrag(eventData);
				}
				return;
			}

			if(eventData.button != PointerEventData.InputButton.Left)
			{
				return;
			}

			if(!IsActive())
			{
				return;
			}

			Vector2 localCursor;
			if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out localCursor))
			{
				return;
			}

			UpdateBounds();

			var pointerDelta = localCursor - m_PointerStartLocalCursor;
			Vector2 position = m_ContentStartPosition + pointerDelta;

			// Offset to get content into place in the view.
			Vector2 offset = CalculateOffset(position - content.anchoredPosition);
			position += offset;
			if(movementType == MovementType.Elastic)
			{
				if(offset.x != 0)
				{
					position.x = position.x - RubberDelta(offset.x, m_ViewBounds.size.x);
				}
				if(offset.y != 0)
				{
					position.y = position.y - RubberDelta(offset.y, m_ViewBounds.size.y);
				}
			}

			SetContentAnchoredPosition(position);
		}

		protected virtual void SetContentAnchoredPosition(Vector2 position)
		{
			if(!horizontal)
			{
				position.x = content.anchoredPosition.x;
			}
			if(!m_Vertical)
			{
				position.y = content.anchoredPosition.y;
			}

			if(position != content.anchoredPosition)
			{
				content.anchoredPosition = position;
				UpdateBounds();
			}
		}

		protected virtual void LateUpdate()
		{
			if(!content)
			{
				return;
			}

			EnsureLayoutHasRebuilt();
			UpdateBounds();
			float deltaTime = Time.unscaledDeltaTime;
			Vector2 offset = CalculateOffset(Vector2.zero);
			if(!m_Dragging && (offset != Vector2.zero || m_Velocity != Vector2.zero))
			{
				Vector2 position = content.anchoredPosition;
				for(int axis = 0; axis < 2; axis++)
				{
					// Apply spring physics if movement is elastic and content has an offset from the view.
					if(movementType == MovementType.Elastic && offset[axis] != 0)
					{
						float speed = m_Velocity[axis];
						position[axis] = Mathf.SmoothDamp(content.anchoredPosition[axis], content.anchoredPosition[axis] + offset[axis], ref speed, elasticity, Mathf.Infinity, deltaTime);
						if(Mathf.Abs(speed) < 1)
						{
							speed = 0;
						}
						m_Velocity[axis] = speed;
					}
					// Else move content according to velocity with deceleration applied.
					else if(inertia)
					{
						m_Velocity[axis] *= Mathf.Pow(decelerationRate, deltaTime);
						if(Mathf.Abs(m_Velocity[axis]) < 1)
						{
							m_Velocity[axis] = 0;
						}
						position[axis] += m_Velocity[axis] * deltaTime;
					}
					// If we have neither elaticity or friction, there shouldn't be any velocity.
					else
					{
						m_Velocity[axis] = 0;
					}
				}

				if(movementType == MovementType.Clamped)
				{
					offset = CalculateOffset(position - content.anchoredPosition);
					position += offset;
				}

				SetContentAnchoredPosition(position);
			}

			if(m_Dragging && inertia)
			{
				Vector3 newVelocity = (content.anchoredPosition - m_PrevPosition) / deltaTime;
				m_Velocity = Vector3.Lerp(m_Velocity, newVelocity, deltaTime * 10);
			}

			if(m_ViewBounds != m_PrevViewBounds || m_ContentBounds != m_PrevContentBounds || content.anchoredPosition != m_PrevPosition)
			{
				UpdateScrollbars(offset);
				UISystemProfilerApi.AddMarker("ScrollRect.value", this);
				onValueChanged.Invoke(normalizedPosition);
				UpdatePrevData();
			}
			UpdateScrollbarVisibility();
		}

		protected void UpdatePrevData()
		{
			if(content == null)
			{
				m_PrevPosition = Vector2.zero;
			}
			else
			{
				m_PrevPosition = content.anchoredPosition;
			}
			m_PrevViewBounds = m_ViewBounds;
			m_PrevContentBounds = m_ContentBounds;
		}

		private void UpdateScrollbars(Vector2 offset)
		{
			if(horizontalScrollbar)
			{
				if(m_ContentBounds.size.x > 0)
				{
					horizontalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.x - Mathf.Abs(offset.x)) / m_ContentBounds.size.x);
				}
				else
				{
					horizontalScrollbar.size = 1;
				}

				horizontalScrollbar.value = horizontalNormalizedPosition;
			}

			if(verticalScrollbar)
			{
				if(m_ContentBounds.size.y > 0)
				{
					verticalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.y - Mathf.Abs(offset.y)) / m_ContentBounds.size.y);
				}
				else
				{
					verticalScrollbar.size = 1;
				}

				verticalScrollbar.value = verticalNormalizedPosition;
			}
		}

		public Vector2 normalizedPosition
		{
			get
			{
				return new Vector2(horizontalNormalizedPosition, verticalNormalizedPosition);
			}
			set
			{
				SetNormalizedPosition(value.x, 0);
				SetNormalizedPosition(value.y, 1);
			}
		}

		public float horizontalNormalizedPosition
		{
			get
			{
				UpdateBounds();
				if(m_ContentBounds.size.x <= m_ViewBounds.size.x)
				{
					return (m_ViewBounds.min.x > m_ContentBounds.min.x) ? 1 : 0;
				}

				return (m_ViewBounds.min.x - m_ContentBounds.min.x) / (m_ContentBounds.size.x - m_ViewBounds.size.x);
			}
			set
			{
				SetNormalizedPosition(value, 0);
			}
		}

		public float verticalNormalizedPosition
		{
			get
			{
				UpdateBounds();
				if(m_ContentBounds.size.y <= m_ViewBounds.size.y)
				{
					return (m_ViewBounds.min.y > m_ContentBounds.min.y) ? 1 : 0;
				}

				return (m_ViewBounds.min.y - m_ContentBounds.min.y) / (m_ContentBounds.size.y - m_ViewBounds.size.y);
			}
			set
			{
				SetNormalizedPosition(value, 1);
			}
		}

		private void SetHorizontalNormalizedPosition(float value) { SetNormalizedPosition(value, 0); }
		private void SetVerticalNormalizedPosition(float value) { SetNormalizedPosition(value, 1); }

		protected virtual void SetNormalizedPosition(float value, int axis)
		{
			EnsureLayoutHasRebuilt();
			UpdateBounds();
			// How much the content is larger than the view.
			float hiddenLength = m_ContentBounds.size[axis] - m_ViewBounds.size[axis];
			// Where the position of the lower left corner of the content bounds should be, in the space of the view.
			float contentBoundsMinPosition = m_ViewBounds.min[axis] - value * hiddenLength;
			// The new content localPosition, in the space of the view.
			float newLocalPosition = content.localPosition[axis] + contentBoundsMinPosition - m_ContentBounds.min[axis];

			Vector3 localPosition = content.localPosition;
			if(Mathf.Abs(localPosition[axis] - newLocalPosition) > 0.01f)
			{
				localPosition[axis] = newLocalPosition;
				content.localPosition = localPosition;
				m_Velocity[axis] = 0;
				UpdateBounds();
			}
		}

		private static float RubberDelta(float overStretching, float viewSize)
		{
			return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
		}

		protected override void OnRectTransformDimensionsChange()
		{
			SetDirty();
		}

		private bool hScrollingNeeded
		{
			get
			{
				if(Application.isPlaying)
				{
					return (m_ContentBounds.size.x > m_ViewBounds.size.x + 0.01f) && enabled;
				}
				return true;
			}
		}
		private bool vScrollingNeeded
		{
			get
			{
				if(Application.isPlaying)
				{
					return (m_ContentBounds.size.y > m_ViewBounds.size.y + 0.01f) && enabled;
				}
				return true;
			}
		}

		public virtual void CalculateLayoutInputHorizontal() { }
		public virtual void CalculateLayoutInputVertical() { }

		public virtual float minWidth { get { return -1; } }
		public virtual float preferredWidth { get { return -1; } }
		public virtual float flexibleWidth { get { return -1; } }

		public virtual float minHeight { get { return -1; } }
		public virtual float preferredHeight { get { return -1; } }
		public virtual float flexibleHeight { get { return -1; } }

		public virtual int layoutPriority { get { return -1; } }

		public virtual void SetLayoutHorizontal()
		{
			m_Tracker.Clear();

			if(m_HSliderExpand || m_VSliderExpand)
			{
				m_Tracker.Add(this, viewRect,
					DrivenTransformProperties.Anchors |
					DrivenTransformProperties.SizeDelta |
					DrivenTransformProperties.AnchoredPosition);

				// Make view full size to see if content fits.
				viewRect.anchorMin = Vector2.zero;
				viewRect.anchorMax = Vector2.one;
				viewRect.sizeDelta = Vector2.zero;
				viewRect.anchoredPosition = Vector2.zero;

				// Recalculate content layout with this size to see if it fits when there are no scrollbars.
				LayoutRebuilder.ForceRebuildLayoutImmediate(Content);
				m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
				m_ContentBounds = GetBounds();
			}

			// If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
			if(m_VSliderExpand && vScrollingNeeded)
			{
				viewRect.sizeDelta = new Vector2(-(m_VSliderWidth + verticalScrollbarSpacing), viewRect.sizeDelta.y);

				// Recalculate content layout with this size to see if it fits vertically
				// when there is a vertical scrollbar (which may reflowed the content to make it taller).
				LayoutRebuilder.ForceRebuildLayoutImmediate(Content);
				m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
				m_ContentBounds = GetBounds();
			}

			// If it doesn't fit horizontally, enable horizontal scrollbar and shrink view vertically to make room for it.
			if(m_HSliderExpand && hScrollingNeeded)
			{
				viewRect.sizeDelta = new Vector2(viewRect.sizeDelta.x, -(m_HSliderHeight + horizontalScrollbarSpacing));
				m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
				m_ContentBounds = GetBounds();
			}

			// If the vertical slider didn't kick in the first time, and the horizontal one did,
			// we need to check again if the vertical slider now needs to kick in.
			// If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
			if(m_VSliderExpand && vScrollingNeeded && viewRect.sizeDelta.x == 0 && viewRect.sizeDelta.y < 0)
			{
				viewRect.sizeDelta = new Vector2(-(m_VSliderWidth + verticalScrollbarSpacing), viewRect.sizeDelta.y);
			}
		}

		public virtual void SetLayoutVertical()
		{
			UpdateScrollbarLayout();
			m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
			m_ContentBounds = GetBounds();
		}

		void UpdateScrollbarVisibility()
		{
			UpdateOneScrollbarVisibility(vScrollingNeeded, m_Vertical, verticalScrollbarVisibility, verticalScrollbar);
			UpdateOneScrollbarVisibility(hScrollingNeeded, horizontal, horizontalScrollbarVisibility, horizontalScrollbar);
		}

		private void UpdateOneScrollbarVisibility(bool xScrollingNeeded, bool xAxisEnabled, ScrollBarVisibilityMode scrollbarVisibilityMode, Scrollbar scrollbar)
		{
			if(scrollbar)
			{
				switch(scrollbarVisibilityMode)
				{
					case ScrollBarVisibilityMode.ALWAYS_HIDDEN: scrollbar.gameObject.SetActive(false); break;
					case ScrollBarVisibilityMode.ALWAYS_VISIBLE: scrollbar.gameObject.SetActive(true); break;
					case ScrollBarVisibilityMode.IN_EDIT_MODE_WHEN_NEEDED:
						if(EditMode)
						{
							scrollbar.gameObject.SetActive(xScrollingNeeded);
						}
						else
						{
							scrollbar.gameObject.SetActive(false);
						}
						break;
					case ScrollBarVisibilityMode.ALWAYS_WHEN_NEEDED: scrollbar.gameObject.SetActive(xScrollingNeeded); break;
				}
			}
		}

		void UpdateScrollbarLayout()
		{
			if(m_VSliderExpand && horizontalScrollbar)
			{
				m_Tracker.Add(this, m_HorizontalScrollbarRect,
					DrivenTransformProperties.AnchorMinX |
					DrivenTransformProperties.AnchorMaxX |
					DrivenTransformProperties.SizeDeltaX |
					DrivenTransformProperties.AnchoredPositionX);
				m_HorizontalScrollbarRect.anchorMin = new Vector2(0, m_HorizontalScrollbarRect.anchorMin.y);
				m_HorizontalScrollbarRect.anchorMax = new Vector2(1, m_HorizontalScrollbarRect.anchorMax.y);
				m_HorizontalScrollbarRect.anchoredPosition = new Vector2(0, m_HorizontalScrollbarRect.anchoredPosition.y);
				if(vScrollingNeeded)
				{
					m_HorizontalScrollbarRect.sizeDelta = new Vector2(-(m_VSliderWidth + verticalScrollbarSpacing), m_HorizontalScrollbarRect.sizeDelta.y);
				}
				else
				{
					m_HorizontalScrollbarRect.sizeDelta = new Vector2(0, m_HorizontalScrollbarRect.sizeDelta.y);
				}
			}

			if(m_HSliderExpand && verticalScrollbar)
			{
				m_Tracker.Add(this, m_VerticalScrollbarRect,
					DrivenTransformProperties.AnchorMinY |
					DrivenTransformProperties.AnchorMaxY |
					DrivenTransformProperties.SizeDeltaY |
					DrivenTransformProperties.AnchoredPositionY);
				m_VerticalScrollbarRect.anchorMin = new Vector2(m_VerticalScrollbarRect.anchorMin.x, 0);
				m_VerticalScrollbarRect.anchorMax = new Vector2(m_VerticalScrollbarRect.anchorMax.x, 1);
				m_VerticalScrollbarRect.anchoredPosition = new Vector2(m_VerticalScrollbarRect.anchoredPosition.x, 0);
				if(hScrollingNeeded)
				{
					m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x, -(m_HSliderHeight + horizontalScrollbarSpacing));
				}
				else
				{
					m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x, 0);
				}
			}
		}

		protected void UpdateBounds()
		{
			m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
			m_ContentBounds = GetBounds();

			if(content == null)
			{
				return;
			}

			Vector3 contentSize = m_ContentBounds.size;
			Vector3 contentPos = m_ContentBounds.center;
			var contentPivot = content.pivot;
			AdjustBounds(ref m_ViewBounds, ref contentPivot, ref contentSize, ref contentPos);
			m_ContentBounds.size = contentSize;
			m_ContentBounds.center = contentPos;

			if(MovementType == MovementType.Clamped)
			{
				// Adjust content so that content bounds bottom (right side) is never higher (to the left) than the view bounds bottom (right side).
				// top (left side) is never lower (to the right) than the view bounds top (left side).
				// All this can happen if content has shrunk.
				// This works because content size is at least as big as view size (because of the call to InternalUpdateBounds above).
				Vector2 delta = Vector2.zero;
				if(m_ViewBounds.max.x > m_ContentBounds.max.x)
				{
					delta.x = Math.Min(m_ViewBounds.min.x - m_ContentBounds.min.x, m_ViewBounds.max.x - m_ContentBounds.max.x);
				}
				else if(m_ViewBounds.min.x < m_ContentBounds.min.x)
				{
					delta.x = Math.Max(m_ViewBounds.min.x - m_ContentBounds.min.x, m_ViewBounds.max.x - m_ContentBounds.max.x);
				}

				if(m_ViewBounds.min.y < m_ContentBounds.min.y)
				{
					delta.y = Math.Max(m_ViewBounds.min.y - m_ContentBounds.min.y, m_ViewBounds.max.y - m_ContentBounds.max.y);
				}
				else if(m_ViewBounds.max.y > m_ContentBounds.max.y)
				{
					delta.y = Math.Min(m_ViewBounds.min.y - m_ContentBounds.min.y, m_ViewBounds.max.y - m_ContentBounds.max.y);
				}
				if(delta.sqrMagnitude > float.Epsilon)
				{
					contentPos = content.anchoredPosition + delta;
					if(!horizontal)
					{
						contentPos.x = content.anchoredPosition.x;
					}
					if(!m_Vertical)
					{
						contentPos.y = content.anchoredPosition.y;
					}
					AdjustBounds(ref m_ViewBounds, ref contentPivot, ref contentSize, ref contentPos);
				}
			}
		}

		internal static void AdjustBounds(ref Bounds viewBounds, ref Vector2 contentPivot, ref Vector3 contentSize, ref Vector3 contentPos)
		{
			// Make sure content bounds are at least as large as view by adding padding if not.
			// One might think at first that if the content is smaller than the view, scrolling should be allowed.
			// However, that's not how scroll views normally work.
			// Scrolling is *only* possible when content is *larger* than view.
			// We use the pivot of the content rect to decide in which directions the content bounds should be expanded.
			// E.g. if pivot is at top, bounds are expanded downwards.
			// This also works nicely when ContentSizeFitter is used on the content.
			Vector3 excess = viewBounds.size - contentSize;
			if(excess.x > 0)
			{
				contentPos.x -= excess.x * (contentPivot.x - 0.5f);
				contentSize.x = viewBounds.size.x;
			}
			if(excess.y > 0)
			{
				contentPos.y -= excess.y * (contentPivot.y - 0.5f);
				contentSize.y = viewBounds.size.y;
			}
		}

		private readonly Vector3[] m_Corners = new Vector3[4];
		private Bounds GetBounds()
		{
			if(content == null)
			{
				return new Bounds();
			}
			content.GetWorldCorners(m_Corners);
			var viewWorldToLocalMatrix = viewRect.worldToLocalMatrix;
			return InternalGetBounds(m_Corners, ref viewWorldToLocalMatrix);
		}

		internal static Bounds InternalGetBounds(Vector3[] corners, ref Matrix4x4 viewWorldToLocalMatrix)
		{
			var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

			for(int j = 0; j < 4; j++)
			{
				Vector3 v = viewWorldToLocalMatrix.MultiplyPoint3x4(corners[j]);
				vMin = Vector3.Min(v, vMin);
				vMax = Vector3.Max(v, vMax);
			}

			var bounds = new Bounds(vMin, Vector3.zero);
			bounds.Encapsulate(vMax);
			return bounds;
		}

		private Vector2 CalculateOffset(Vector2 delta)
		{
			return InternalCalculateOffset(ref m_ViewBounds, ref m_ContentBounds, horizontal, m_Vertical, movementType, ref delta);
		}

		internal static Vector2 InternalCalculateOffset(ref Bounds viewBounds, ref Bounds contentBounds, bool horizontal, bool vertical, MovementType movementType, ref Vector2 delta)
		{
			Vector2 offset = Vector2.zero;
			if(movementType == MovementType.Unrestricted)
			{
				return offset;
			}

			Vector2 min = contentBounds.min;
			Vector2 max = contentBounds.max;

			if(horizontal)
			{
				min.x += delta.x;
				max.x += delta.x;
				if(min.x > viewBounds.min.x)
				{
					offset.x = viewBounds.min.x - min.x;
				}
				else if(max.x < viewBounds.max.x)
				{
					offset.x = viewBounds.max.x - max.x;
				}
			}

			if(vertical)
			{
				min.y += delta.y;
				max.y += delta.y;
				if(max.y < viewBounds.max.y)
				{
					offset.y = viewBounds.max.y - max.y;
				}
				else if(min.y > viewBounds.min.y)
				{
					offset.y = viewBounds.min.y - min.y;
				}
			}

			return offset;
		}

		protected void SetDirty()
		{
			if(!IsActive())
			{
				return;
			}

			LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		}

		protected void SetDirtyCaching()
		{
			if(!IsActive())
			{
				return;
			}

			CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
			LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		}

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			SetDirtyCaching();
		}
#endif

		public void OnPointerDown(PointerEventData eventData)
		{
			IPointerDownHandler pointerDownHandler = transform.parent.GetComponentInParent<IPointerDownHandler>();
			if(pointerDownHandler != null)
			{
				pointerDownHandler.OnPointerDown(eventData);
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			IPointerUpHandler pointerUpHandler = transform.parent.GetComponentInParent<IPointerUpHandler>();
			if(pointerUpHandler != null)
			{
				pointerUpHandler.OnPointerUp(eventData);
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			IPointerClickHandler pointerClickHandler = transform.parent.GetComponentInParent<IPointerClickHandler>();
			if(pointerClickHandler != null)
			{
				pointerClickHandler.OnPointerClick(eventData);
			}
		}
	}
}
