// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	public class SyncContentTransform: MonoBehaviour
	{
		[SerializeField]
		private RectTransform contentTransform;

		private RectTransform rectTransform;
#if(UNITY_ANDROID || UNITY_IOS)
		private KeyboardScroller keyboardScroller;
#endif

		private void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
#if(UNITY_ANDROID || UNITY_IOS)
			keyboardScroller = GetComponentInParent<KeyboardScroller>();
#endif
		}

		private void Start()
		{
			UpdateSize();
		}

		private void OnRectTransformDimensionsChange()
		{
			if(rectTransform == null) { return; }

			UpdateSize();
		}

		public void UpdateSize()
		{
#if(UNITY_ANDROID || UNITY_IOS)
			if(keyboardScroller.State != KeyboardScrollerState.IDLE)
			{
				keyboardScroller.ForceScrollUpdate(rectTransform.rect.height);
				return;
			}
#endif

			UpdateContentSize();
		}

		private void UpdateContentSize()
		{
			float targetHeight = rectTransform.rect.height;
			Vector2 size = contentTransform.sizeDelta;
			size.y = targetHeight;
			contentTransform.sizeDelta = size;

#if(UNITY_ANDROID || UNITY_IOS)
			keyboardScroller.RefreshOriginalContentSize(); //Make sure the KeyboardScroller knows that the content size got changed
#endif
		}
	}
}