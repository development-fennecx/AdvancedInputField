using UnityEngine;
using UnityEngine.EventSystems;

namespace AdvancedInputFieldPlugin
{
	public delegate void OnPress(PointerEventData eventData);
	public delegate void OnRelease(PointerEventData eventData);
	public delegate void OnBeginDrag(PointerEventData eventData);
	public delegate void OnDrag(PointerEventData eventData);
	public delegate void OnEndDrag(PointerEventData eventData);

	public class PointerHandler: MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
	{
		private event OnPress onPress;
		private event OnRelease onRelease;
		private event OnBeginDrag onBeginDrag;
		private event OnDrag onDrag;
		private event OnEndDrag onEndDrag;

		public event OnPress Press
		{
			add { onPress += value; }
			remove { onPress -= value; }
		}

		public event OnRelease Release
		{
			add { onRelease += value; }
			remove { onRelease -= value; }
		}

		public event OnBeginDrag BeginDrag
		{
			add { onBeginDrag += value; }
			remove { onBeginDrag -= value; }
		}

		public event OnDrag Drag
		{
			add { onDrag += value; }
			remove { onDrag -= value; }
		}

		public event OnEndDrag EndDrag
		{
			add { onEndDrag += value; }
			remove { onEndDrag -= value; }
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if(onPress != null)
			{
				onPress(eventData);
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if(onRelease != null)
			{
				onRelease(eventData);
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			if(onDrag != null)
			{
				onDrag(eventData);
			}
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			if(onBeginDrag != null)
			{
				onBeginDrag(eventData);
			}
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if(onEndDrag != null)
			{
				onEndDrag(eventData);
			}
		}

		public void ClearEvents()
		{
			onPress = null;
			onRelease = null;
			onBeginDrag = null;
			onDrag = null;
			onEndDrag = null;
		}
	}
}