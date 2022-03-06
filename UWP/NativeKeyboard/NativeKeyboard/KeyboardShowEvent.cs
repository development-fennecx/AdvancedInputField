namespace NativeKeyboardUWP
{
	public class KeyboardShowEvent: IUnityEvent
	{
		public EventType Type { get { return EventType.KEYBOARD_SHOW; } }
		public string text;
		public int selectionStartPosition;
		public int selectionEndPosition;
		public NativeKeyboardConfiguration configuration;

		public KeyboardShowEvent(string text, int selectionStartPosition, int selectionEndPosition, NativeKeyboardConfiguration configuration)
		{
			this.text = text;
			this.selectionStartPosition = selectionStartPosition;
			this.selectionEndPosition = selectionEndPosition;
			this.configuration = configuration;
		}
	}
}