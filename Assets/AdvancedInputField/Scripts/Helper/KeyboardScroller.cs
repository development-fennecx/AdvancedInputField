// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
#if(UNITY_ANDROID || UNITY_IOS)
using UnityEngine.EventSystems;
using System.Collections;
#endif

namespace AdvancedInputFieldPlugin
{
	/// <summary>Determines when to scroll when keyboard appears</summary>
	public enum KeyboardScrollMode { ONLY_SCROLL_IF_INPUTFIELD_BLOCKED, ALWAYS_SCROLL }

	/// <summary>The state of the keyboard scroller</summary>
	public enum KeyboardScrollerState { IDLE, SCROLLING_TO_INPUTFIELD, FOCUSED_ON_INPUTFIELD, SCROLLING_FROM_INPUTFIELD }

	/// <summary>Class to scroll to the currently selected InputField when te keyboard appears & hides, making sure that the InputField is not behind the keyboard</summary>
	[RequireComponent(typeof(ScrollRect))]
	public class KeyboardScroller: MonoBehaviour
	{
		/// <summary>Event used when the keyboard scroller state changes</summary>
		[Serializable]
		public class KeyboardScrollStateChangedEvent: UnityEvent<KeyboardScrollerState> { }

		private const float SCROLL_FIX_SPEED = 0.1f;

		/// <summary>The KeyboardScrollMode</summary>
		[Tooltip("Determines when to scroll when keyboard appears")]
		[SerializeField]
		private KeyboardScrollMode keyboardScrollMode;

		/// <summary>The transition time to scroll to the target InputField</summary>
		[SerializeField]
		private float transitionTime = 0.5f;

		/// <summary>The normalized offset y used to add additional spacing between keyboard and target</summary>
		[Tooltip("The normalized offset y used to add additional spacing between keyboard and target")]
		[SerializeField]
		[Range(0f, 1f)]
		private float normalizedOffsetY = 0.025f;

		/// <summary>The multiplier to use when resizing the content transform (for enough scrolling space)</summary>
		[Tooltip("The multiplier to use when resizing the content transform (for enough scrolling space)")]
		[SerializeField]
		[Range(3f, 10f)]
		private float scrollSpaceMultiplier = 3f;

		[Tooltip("Event used when the keyboard scroller state changes")]
		[SerializeField]
		private KeyboardScrollStateChangedEvent onKeyboardScrollerStateChanged = new KeyboardScrollStateChangedEvent();

		/// <summary>Event used when the keyboard scroller state changes</summary>
		public KeyboardScrollStateChangedEvent OnKeyboardScrollerStateChanged
		{
			get { return onKeyboardScrollerStateChanged; }
			set { onKeyboardScrollerStateChanged = value; }
		}

#if(UNITY_ANDROID || UNITY_IOS)
		/// <summary>The ScrollRect</summary>
		private ScrollRect scrollRect;

		/// <summary>The Canvas</summary>
		private Canvas canvas;

		/// <summary>The original size of the scrollable content</summary>
		private Vector2 originalContentSize;

		/// <summary>The start size of the scrollable content when starting the transition</summary>
		private Vector2 startContentSize;

		/// <summary>The end size of the scrollable content when starting the transition</summary>
		private Vector2 endContentSize;

		/// <summary>The start position of the scrollable content when starting the transition</summary>
		private Vector2 startContentPosition;

		/// <summary>The end position of the scrollable content when starting the transition</summary>
		private Vector2 endContentPosition;

		/// <summary>The current time in the transition</summary>
		private float currentTime;

		/// <summary>The last selected object</summary>
		private GameObject lastSelectedObject;

		/// <summary>The last known keyboard height</summary>
		private int lastKeyboardHeight;

		/// <summary>The current KeyboardScrollState</summary>
		private KeyboardScrollerState state;

		private bool initialized;

		/// <summary>The Canvas</summary>
		public Canvas Canvas
		{
			get
			{
				if(canvas == null)
				{
					canvas = GetComponentInParent<Canvas>();
				}

				return canvas;
			}
		}

		/// <summary>The current KeyboardScrollState</summary>
		public KeyboardScrollerState State
		{
			get
			{
				return state;
			}
			private set
			{
				if(state != value)
				{
					state = value;
					onKeyboardScrollerStateChanged?.Invoke(state);
				}
			}
		}

		#region UNITY
		private void Awake()
		{
			EnsureInitialization();
		}

		private void EnsureInitialization()
		{
			if(!initialized) { Initialize(); }
		}

		private void Initialize()
		{
			scrollRect = GetComponent<ScrollRect>();
			initialized = true;
		}

		private void Start()
		{
			originalContentSize = scrollRect.content.sizeDelta;
			currentTime = transitionTime;

#if UNITY_EDITOR
			if(!Settings.SimulateMobileBehaviourInEditor)
			{
				enabled = false;
			}
#endif
		}

		private void OnEnable()
		{
			NativeKeyboardManager.AddKeyboardHeightChangedListener(OnKeyboardHeightChanged);
		}

		private void OnDisable()
		{
			NativeKeyboardManager.RemoveKeyboardHeightChangedListener(OnKeyboardHeightChanged);
		}

		private void Update()
		{
			if(currentTime < transitionTime)
			{
				currentTime += Time.deltaTime;
				if(currentTime >= transitionTime)
				{
					currentTime = transitionTime;
					scrollRect.vertical = true; //Enable user scroll again
					if(lastKeyboardHeight == 0)
					{
						StartCoroutine(EnsureScrollWithinRange());
						State = KeyboardScrollerState.IDLE;
					}
					else
					{
						State = KeyboardScrollerState.FOCUSED_ON_INPUTFIELD;
					}
				}

				float progress = currentTime / transitionTime;
				scrollRect.content.sizeDelta = Vector2.Lerp(startContentSize, endContentSize, progress);
				scrollRect.content.anchoredPosition = Vector2.Lerp(startContentPosition, endContentPosition, progress);
				TryUpdateCaretPosition();
			}

			if(lastKeyboardHeight > 0)
			{
				GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
				if(selectedObject != null && selectedObject != lastSelectedObject)
				{
					lastSelectedObject = selectedObject;

					if(selectedObject.GetComponent<AdvancedInputField>()) //Handle Next on mobile keyboard
					{
						ScrollContent(lastKeyboardHeight);
					}
				}
			}
		}
		#endregion

		private void TryUpdateCaretPosition()
		{
			if(lastSelectedObject == null) { return; }

			AdvancedInputField inputField = lastSelectedObject.GetComponent<AdvancedInputField>();
			if(inputField != null)
			{
				inputField.UpdateCaretPosition();
			}
		}

		private IEnumerator EnsureScrollWithinRange()
		{
			if(scrollRect.verticalNormalizedPosition < 0)
			{
				float scrollDifference = -scrollRect.verticalNormalizedPosition;
				while(scrollRect.verticalNormalizedPosition < 0)
				{
					float scrollMove = (scrollDifference * (Time.deltaTime / SCROLL_FIX_SPEED));
					scrollRect.verticalNormalizedPosition = scrollRect.verticalNormalizedPosition + scrollMove;
					yield return null;
				}

				scrollRect.verticalNormalizedPosition = 0; //Final scroll update
			}
			else if(scrollRect.verticalNormalizedPosition > 1)
			{
				float scrollDifference = scrollRect.verticalNormalizedPosition - 1;
				while(scrollRect.verticalNormalizedPosition > 1)
				{
					float scrollMove = (scrollDifference * (Time.deltaTime / SCROLL_FIX_SPEED));
					scrollRect.verticalNormalizedPosition = scrollRect.verticalNormalizedPosition - scrollMove;
					yield return null;
				}

				scrollRect.verticalNormalizedPosition = 1; //Final scroll update
			}


			while(scrollRect.verticalNormalizedPosition > 1)
			{
				scrollRect.verticalNormalizedPosition = scrollRect.verticalNormalizedPosition;
				yield return null;
			}
		}

		/// <summary>Sets current content size as the original content size</summary>
		public void RefreshOriginalContentSize()
		{
			EnsureInitialization();
			originalContentSize = scrollRect.content.sizeDelta;
		}

		/// <summary>Force update the original content size (when the keyboard is already visible)</summary>
		public void ForceScrollUpdate(float originalContentHeight)
		{
			EnsureInitialization();
			originalContentSize.y = originalContentHeight;
			ScrollContent(lastKeyboardHeight);
		}

		/// <summary>Gets the normalized vertical values of a RectTransform</summary>
		/// <param name="rectTransform">The RectTransform to use</param>
		/// <param name="normalizedY">The output normalized y</param>
		/// <param name="normalizedHeight">The output normalized height</param>
		private void GetNormalizedVertical(RectTransform rectTransform, out float normalizedY, out float normalizedHeight)
		{
			Vector3[] corners = new Vector3[4];
			rectTransform.GetWorldCorners(corners);

			float bottomY = corners[1].y;
			if(Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
			{
				normalizedY = bottomY / Screen.height;
			}
			else
			{
				Camera camera = Canvas.worldCamera;
				bottomY -= camera.transform.position.y;
				normalizedY = (bottomY + camera.orthographicSize) / (camera.orthographicSize * 2);
			}
			normalizedY -= normalizedOffsetY;

			Vector2 size = new Vector2(Mathf.Abs(corners[3].x - corners[1].x), Mathf.Abs(corners[3].y - corners[1].y));
			if(Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
			{
				normalizedHeight = size.y / Screen.height;
			}
			else
			{
				Camera camera = Canvas.worldCamera;
				normalizedHeight = size.y / (camera.orthographicSize * 2);
			}
		}

		/// <summary>Event callback when the keyboard height has changed</summary>
		/// <param name="keyboardHeight">The keyboard height</param>
		public void OnKeyboardHeightChanged(int keyboardHeight)
		{
			if(keyboardHeight > 0)
			{
				ScrollContent(keyboardHeight);
			}
			else
			{
				ScrollBack();
			}

			lastKeyboardHeight = keyboardHeight;
		}

		/// <summary>Start the scroll to the current InputField using keyboardHeight as the offset</summary>
		/// <param name="keyboardHeight">The keyboard height</param>
		public void ScrollContent(int keyboardHeight)
		{
			GameObject targetObject = EventSystem.current.currentSelectedGameObject;
			if(targetObject == null || targetObject.GetComponent<AdvancedInputField>() == null)
			{
				return;
			}

			if(targetObject.GetComponent<AdvancedInputField>() != null)
			{
				RectTransform scrollTarget = targetObject.GetComponent<RectTransform>();
				float normalizedY, normalizedHeight;
				GetNormalizedVertical(scrollTarget, out normalizedY, out normalizedHeight);
				float normalizedKeyboardHeight = keyboardHeight / (float)Screen.height;

				float moveY;
				if(Canvas.renderMode == RenderMode.WorldSpace)
				{
					RectTransform canvasTransform = Canvas.GetComponent<RectTransform>();
					Vector3[] canvasCorners = new Vector3[4];
					canvasTransform.GetWorldCorners(canvasCorners);
					Vector2 canvasWorldSize = new Vector2(Mathf.Abs(canvasCorners[3].x - canvasCorners[1].x), Mathf.Abs(canvasCorners[3].y - canvasCorners[1].y));
					float ratioY = canvasWorldSize.y / (canvas.worldCamera.orthographicSize * 2);
					moveY = (normalizedKeyboardHeight - (normalizedY - normalizedHeight)) * (canvasTransform.rect.height / ratioY / Canvas.scaleFactor);
				}
				else
				{
					moveY = (normalizedKeyboardHeight - (normalizedY - normalizedHeight)) * (Canvas.pixelRect.height / Canvas.scaleFactor);
				}
				if(Math.Abs(moveY) < 0.001f) { return; }

				if(keyboardScrollMode == KeyboardScrollMode.ONLY_SCROLL_IF_INPUTFIELD_BLOCKED && moveY < 0)
				{
					return;
				}

				startContentPosition = scrollRect.content.anchoredPosition;
				endContentPosition = startContentPosition;
				endContentPosition.y += moveY;

				startContentSize = scrollRect.content.sizeDelta;
				endContentSize = originalContentSize * scrollSpaceMultiplier; //Multiply it so we have enough scroll space at the top and bottom
				currentTime = 0;
				scrollRect.vertical = false; //Disable user scroll when transitioning

				State = KeyboardScrollerState.SCROLLING_TO_INPUTFIELD;
			}
		}

		/// <summary>Scrolls and resizes content size to their original state</summary>
		public void ScrollBack()
		{
			startContentSize = scrollRect.content.sizeDelta;
			endContentSize = originalContentSize;
			startContentPosition = scrollRect.content.anchoredPosition;
			endContentPosition = scrollRect.content.anchoredPosition; //Keep forcing current position while scrollrect resizes
			currentTime = 0;
			scrollRect.vertical = false; //Disable user scroll when transitioning

			State = KeyboardScrollerState.SCROLLING_FROM_INPUTFIELD;
		}
#endif
	}
}
