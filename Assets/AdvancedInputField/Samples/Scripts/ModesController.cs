//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using AdvancedInputFieldPlugin;
using UnityEngine;

namespace AdvancedInputFieldSamples
{
	public class ModesController: MonoBehaviour
	{
		[SerializeField]
		private AdvancedInputField resizeVerticalInputField;

		private Canvas canvas;
		private Vector2 originalResizeVerticalPosition;
		private float keyboardHeight;

		public Canvas Canvas
		{
			get
			{
				if(canvas == null)
				{
					canvas = resizeVerticalInputField.GetComponentInParent<Canvas>();
				}

				return canvas;
			}
		}

		private void Start()
		{
#if(UNITY_ANDROID || UNITY_IOS)
			if(!Application.isEditor || Settings.SimulateMobileBehaviourInEditor)
			{
				NativeKeyboardManager.AddKeyboardHeightChangedListener(OnKeyboardHeightChanged);
			}
#endif
		}

		public void OnResizeHorizontalSizeChanged(Vector2 size)
		{
			Debug.Log("OnResizeHorizontalSizeChanged: " + size);
		}

		public void OnResizeVerticalBeginEdit(BeginEditReason reason)
		{
			Debug.Log("OnResizeVerticalBeginEdit");
			originalResizeVerticalPosition = resizeVerticalInputField.RectTransform.anchoredPosition;

#if(UNITY_ANDROID || UNITY_IOS)
			if(!Application.isEditor || Settings.SimulateMobileBehaviourInEditor)
			{
				OnResizeVerticalSizeChanged(resizeVerticalInputField.Size); //Move to top of keyboard on mobile on begin edit
			}
#endif
		}

		public void OnResizeVerticalEndEdit(string result, EndEditReason reason)
		{
			Debug.Log("OnResizeVerticalEndEdit");
			resizeVerticalInputField.RectTransform.anchoredPosition = originalResizeVerticalPosition;
		}

		public void OnResizeVerticalSizeChanged(Vector2 size)
		{
			Debug.Log("OnResizeVerticalSizeChanged: " + size);
			if(!resizeVerticalInputField.Selected) { return; }

#if(UNITY_ANDROID || UNITY_IOS)
			if(!Application.isEditor || Settings.SimulateMobileBehaviourInEditor)
			{
				Vector2 position = resizeVerticalInputField.RectTransform.anchoredPosition;
				float currentBottomY = GetAbsoluteBottomY(resizeVerticalInputField.RectTransform);
				float targetBottomY = keyboardHeight / Canvas.scaleFactor;
				position.y += (targetBottomY - currentBottomY);
				resizeVerticalInputField.RectTransform.anchoredPosition = position;
			}
#endif
		}

		public float GetAbsoluteBottomY(RectTransform rectTransform)
		{
			Vector3[] corners = new Vector3[4];
			rectTransform.GetWorldCorners(corners);

			float bottomY = corners[0].y;
			float normalizedBottomY = 0;
			if(Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
			{
				normalizedBottomY = bottomY / Screen.height;
			}
			else
			{
				Camera camera = Canvas.worldCamera;
				normalizedBottomY = (bottomY + camera.orthographicSize) / (camera.orthographicSize * 2);
			}

			return (normalizedBottomY * Canvas.pixelRect.height) / Canvas.scaleFactor;
		}

		public void OnKeyboardHeightChanged(int keyboardHeight)
		{
			Debug.Log("OnKeyboardHeightChanged: " + keyboardHeight);
			this.keyboardHeight = keyboardHeight;

			if(resizeVerticalInputField.Selected)
			{
				Vector2 position = resizeVerticalInputField.RectTransform.anchoredPosition;
				float currentBottomY = GetAbsoluteBottomY(resizeVerticalInputField.RectTransform);
				float targetBottomY = keyboardHeight / Canvas.scaleFactor;
				position.y += (targetBottomY - currentBottomY);
				resizeVerticalInputField.RectTransform.anchoredPosition = position;
			}
		}
	}
}
