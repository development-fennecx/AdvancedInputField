using UnityEngine;

namespace AdvancedInputFieldPlugin.Editor
{
	public static class RectTransformExtensions
	{
		public static void CopyValues(this RectTransform targetTransform, RectTransform sourceTransform)
		{
			targetTransform.SetParent(sourceTransform.parent);
			targetTransform.localScale = sourceTransform.localScale;
			targetTransform.localPosition = sourceTransform.localPosition;
			targetTransform.localRotation = sourceTransform.localRotation;

			targetTransform.anchorMax = sourceTransform.anchorMax;
			targetTransform.anchorMin = sourceTransform.anchorMin;
			targetTransform.offsetMax = sourceTransform.offsetMax;
			targetTransform.offsetMin = sourceTransform.offsetMin;
			targetTransform.pivot = sourceTransform.pivot;
			targetTransform.sizeDelta = sourceTransform.sizeDelta;
			targetTransform.anchoredPosition = sourceTransform.anchoredPosition;
		}
	}
}