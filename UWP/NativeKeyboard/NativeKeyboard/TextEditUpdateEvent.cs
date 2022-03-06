namespace NativeKeyboardUWP
{
	public class TextEditUpdateEvent: IUnityEvent
	{
		public EventType Type { get { return EventType.TEXT_EDIT_UPDATE; } }
		public string text;
		public int selectionStartPosition;
		public int selectionEndPosition;

		public TextEditUpdateEvent(string text, int selectionStartPosition, int selectionEndPosition)
		{
			this.text = text;
			this.selectionStartPosition = selectionStartPosition;
			this.selectionEndPosition = selectionEndPosition;
		}
	}
}
