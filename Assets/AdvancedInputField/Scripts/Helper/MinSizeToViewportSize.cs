using UnityEngine;
using UnityEngine.UI;

namespace AdvancedInputFieldPlugin
{
	public class MinSizeToViewportSize: MonoBehaviour
	{
		[SerializeField]
		private RectTransform viewport;

		private LayoutElement layoutElement;

		private void Awake()
		{
			layoutElement = GetComponent<LayoutElement>();
		}

		private void Start()
		{
			layoutElement.minHeight = viewport.rect.height;
		}
	}
}
