// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	[ExecuteInEditMode]
	public abstract class TextSelectionHandler: MonoBehaviour
	{
		private DrivenRectTransformTracker transformTracker;

		public TextNavigator TextNavigator { get; private set; }
		public InputFieldEngine Engine { get { return TextNavigator.Engine; } }
		public AdvancedInputField InputField { get { return Engine.InputField; } }

		private bool initialized;

		public virtual void Initialize()
		{
			initialized = true;
		}

		public void EnsureInitialization()
		{
			if(!initialized) { Initialize(); }
		}

		protected virtual void OnEnable()
		{
			RectTransform rectTransform = GetComponent<RectTransform>();
			rectTransform.anchorMin = new Vector2(0, 0);
			rectTransform.anchorMax = new Vector2(1, 1);
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
			rectTransform.offsetMin = new Vector2(0, 0);
			rectTransform.offsetMax = new Vector2(0, 0);
			rectTransform.localScale = Vector3.one;
			rectTransform.localRotation = Quaternion.identity;

			transformTracker.Add(this, rectTransform, DrivenTransformProperties.All);
		}

		protected virtual void OnDisable()
		{
			transformTracker.Clear();
		}

		public virtual void Setup(Transform parent, TextNavigator textNavigator)
		{
			TextNavigator = textNavigator;
			EnsureInitialization();

			transform.SetParent(parent);
			transform.localScale = Vector3.one;
			transform.localPosition = Vector3.zero;
			transform.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;

			transformTracker.Clear();
			RectTransform rectTransform = GetComponent<RectTransform>();
			rectTransform.anchorMin = new Vector2(0, 0);
			rectTransform.anchorMax = new Vector2(1, 1);
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
			rectTransform.offsetMin = new Vector2(0, 0);
			rectTransform.offsetMax = new Vector2(0, 0);
			rectTransform.localScale = Vector3.one;
			rectTransform.localRotation = Quaternion.identity;

			transformTracker.Add(this, rectTransform, DrivenTransformProperties.All);
		}

		public virtual void OnUpdate() { }
		public virtual void OnCanvasScaleChanged(float canvasScaleFactor) { }
		public abstract void OnSelectionUpdate(int selectionStartPosition, int selectionEndPosition);
	}
}
