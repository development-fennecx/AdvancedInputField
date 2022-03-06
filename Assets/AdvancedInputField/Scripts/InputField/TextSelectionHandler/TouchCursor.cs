// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AdvancedInputFieldPlugin
{
	public delegate void OnTouchCursorSelected();
	public delegate void OnTouchCursorMoved(TouchCursor touchCursor, PointerEventData eventData);
	public delegate void OnTouchCursorMoveFinished();

	public enum MobileCursorType { START_CURSOR, END_CURSOR, CURRENT_CURSOR }

	[RequireComponent(typeof(RectTransform))]
	public class TouchCursor: Selectable
	{
		private const float MIN_SIZE_RATIO = 0.6f;
		private const float MAX_SIZE_RATIO = 1f;
		private const float TRANSITION_TIME = 0.33f;

		[SerializeField]
		private CanvasFrontRenderer frontRenderer;

		private RectTransform rectTransform;
		private RectTransform backImageTransform;
		private RectTransform frontBoundsTransform;
		private RectTransform frontImageTransform;

		private event OnTouchCursorSelected onTouchCursorSelected;
		private event OnTouchCursorMoved onTouchCursorMoved;
		private event OnTouchCursorMoveFinished onTouchCursorMoveFinished;
		private float currentTransitionTime;
		private Vector2 startTransitionPosition;

		public RectTransform RectTransform { get { return rectTransform; } }
		public CanvasFrontRenderer FrontRenderer { get { return frontRenderer; } }
		public MobileCursorType Type { get; set; }
		public bool Selected { get; private set; }
		public bool OutOfBounds { get; set; }
		public Vector2 TargetPosition { get; set; }
		public Vector2 PressOffset { get; set; }

		public event OnTouchCursorSelected TouchCursorSelected
		{
			add { onTouchCursorSelected += value; }
			remove { onTouchCursorSelected -= value; }
		}

		public event OnTouchCursorMoved TouchCursorMoved
		{
			add { onTouchCursorMoved += value; }
			remove { onTouchCursorMoved -= value; }
		}

		public event OnTouchCursorMoveFinished TouchCursorMoveFinished
		{
			add { onTouchCursorMoveFinished += value; }
			remove { onTouchCursorMoveFinished -= value; }
		}

		public Vector2 Offset
		{
			get
			{
				Vector2 anchorOffset = frontImageTransform.anchorMax - new Vector2(0.5f, 0.5f);
#if UNITY_2018_1_OR_NEWER
				return anchorOffset * frontImageTransform.rect.size;
#else
				return new Vector2(anchorOffset.x * frontImageTransform.rect.size.x, anchorOffset.y * frontImageTransform.rect.size.y);
#endif
			}
		}

		protected override void Awake()
		{
			base.Awake();

			rectTransform = GetComponent<RectTransform>();
			backImageTransform = rectTransform.Find("Image").GetComponent<RectTransform>();
			frontBoundsTransform = frontRenderer.GetComponent<RectTransform>();
			frontImageTransform = frontBoundsTransform.Find("Image").GetComponent<RectTransform>();
			GetComponent<Image>().enabled = false;
			enabled = false;
		}

		protected override void Start()
		{
			base.Start();

			PointerHandler backPointerHandler = GetComponent<PointerHandler>();
			backPointerHandler.Press += OnPress;
			backPointerHandler.Release += OnRelease;
			backPointerHandler.BeginDrag += OnBeginDrag;
			backPointerHandler.Drag += OnDrag;
			backPointerHandler.EndDrag += OnEndDrag;

			PointerHandler frontPointerHandler = FrontRenderer.GetComponent<PointerHandler>();
			frontPointerHandler.Press += OnPress;
			frontPointerHandler.Release += OnRelease;
			frontPointerHandler.BeginDrag += OnBeginDrag;
			frontPointerHandler.Drag += OnDrag;
			frontPointerHandler.EndDrag += OnEndDrag;
		}

		private void Update()
		{
			if(currentTransitionTime < TRANSITION_TIME)
			{
				currentTransitionTime += Time.deltaTime;
				if(currentTransitionTime >= TRANSITION_TIME)
				{
					currentTransitionTime = TRANSITION_TIME;

					if(!Selected)
					{
						if(onTouchCursorMoveFinished != null)
						{
							onTouchCursorMoveFinished();
						}
					}
				}

				float progress = currentTransitionTime / TRANSITION_TIME;
				float sizeRatio;
				if(Selected)
				{
					sizeRatio = MIN_SIZE_RATIO + (progress * (MAX_SIZE_RATIO - MIN_SIZE_RATIO));
				}
				else
				{
					sizeRatio = MIN_SIZE_RATIO + ((1 - progress) * (MAX_SIZE_RATIO - MIN_SIZE_RATIO));
					rectTransform.anchoredPosition = Vector2.Lerp(startTransitionPosition, TargetPosition, progress);
					Sync();
				}

				backImageTransform.sizeDelta = rectTransform.rect.size * sizeRatio;
				frontImageTransform.sizeDelta = frontBoundsTransform.rect.size * sizeRatio;
			}
			else if(frontRenderer.gameObject.activeInHierarchy)
			{
				frontRenderer.SyncTransform(rectTransform, false);
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			ShowBack();
			ShowFront();
			Selected = false;
			backImageTransform.sizeDelta = rectTransform.rect.size * MIN_SIZE_RATIO;
			frontImageTransform.sizeDelta = frontBoundsTransform.rect.size * MIN_SIZE_RATIO;
			currentTransitionTime = TRANSITION_TIME;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			HideBack();
			HideFront();
			Selected = false;
			backImageTransform.sizeDelta = rectTransform.rect.size * MIN_SIZE_RATIO;
			frontImageTransform.sizeDelta = frontBoundsTransform.rect.size * MIN_SIZE_RATIO;
			currentTransitionTime = TRANSITION_TIME;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if(frontRenderer != null)
			{
				AdvancedInputField inputField = GetComponentInParent<AdvancedInputField>();
				if(inputField == null || !inputField.WithinAwake)
				{
					Destroy(frontRenderer.gameObject);
				}
			}
		}

		public void ShowBack()
		{
			backImageTransform.gameObject.SetActive(true);
			GetComponent<Image>().enabled = true;
		}

		public void HideBack()
		{
			backImageTransform.gameObject.SetActive(false);
			GetComponent<Image>().enabled = false;
		}

		public void ShowFront()
		{
			if(!enabled || !gameObject.activeInHierarchy) { return; }
			FrontRenderer.Show();
		}

		public void HideFront()
		{
			FrontRenderer.Hide();
		}

		public void OnTextScrollChanged(Vector2 scroll)
		{
			if(!Selected)
			{
				frontRenderer.SyncTransform(rectTransform, false);
			}
		}

		public void OnPress(PointerEventData eventData)
		{
			Selected = true;
			ShowFront();
			currentTransitionTime = 0;

			if(onTouchCursorSelected != null)
			{
				onTouchCursorSelected();
			}
		}

		public void OnRelease(PointerEventData eventData)
		{
			Selected = false;
			currentTransitionTime = 0;
			startTransitionPosition = rectTransform.anchoredPosition;
		}

		public void OnDrag(PointerEventData eventData)
		{
			if(onTouchCursorMoved != null)
			{
				onTouchCursorMoved(this, eventData);
			}
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
		}

		public void OnEndDrag(PointerEventData eventData)
		{
		}

		public void UpdateSize(Vector2 size)
		{
			rectTransform.sizeDelta = size;
			Sync();

			backImageTransform.sizeDelta = rectTransform.rect.size * MIN_SIZE_RATIO;
			frontImageTransform.sizeDelta = frontBoundsTransform.rect.size * MIN_SIZE_RATIO;
		}

		public void UpdatePosition(Vector2 position, bool resetTarget = false)
		{
			rectTransform.anchoredPosition = position;
			if(resetTarget)
			{
				TargetPosition = position;
			}
			Sync();
		}

		private void Sync()
		{
			frontRenderer.RefreshCanvas(GetComponentInParent<Canvas>());
			frontRenderer.SyncTransform(rectTransform);
		}
	}
}
