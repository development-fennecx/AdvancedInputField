using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Custom Button subclass to be used as a child and on top of an AdvancedInputField instance</summary>
	public class InputFieldButton: Button
	{
		private bool selected;

		protected override void Awake()
		{
			base.Awake();
		}

		public bool IsButtonPressed()
		{
			return IsPressed();
		}

		public bool IsButtonSelected()
		{
			return selected;
		}

		public override void OnPointerDown(PointerEventData eventData)
		{
			base.OnPointerDown(eventData);
			eventData.Use();
		}

		public override void OnPointerUp(PointerEventData eventData)
		{
			base.OnPointerUp(eventData);
			eventData.Use();
		}

		public override void OnPointerClick(PointerEventData eventData)
		{
			base.OnPointerClick(eventData);
			eventData.Use();
		}

		public override void OnSelect(BaseEventData eventData)
		{
			base.OnSelect(eventData);
			selected = true;
		}

		public override void OnDeselect(BaseEventData eventData)
		{
			base.OnDeselect(eventData);
			selected = false;
		}
	}
}
