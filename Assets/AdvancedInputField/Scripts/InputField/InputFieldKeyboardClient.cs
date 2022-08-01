namespace AdvancedInputFieldPlugin
{
	public class InputFieldKeyboardClient: KeyboardClient
	{
		public InputFieldEngine Engine { get; private set; }
		public TextEditFrame? LastTextEditFrame { get; private set; }

		public void Initialize(InputFieldEngine engine)
		{
			Engine = engine;
			LastTextEditFrame = null;
		}

		public void ClearLastTextEditFrame()
		{
			LastTextEditFrame = null;
		}

		public override void UpdateTextEdit(string text, int selectionStartPosition, int selectionEndPosition)
		{
			base.UpdateTextEdit(text, selectionStartPosition, selectionEndPosition);
			LastTextEditFrame = new TextEditFrame(text, selectionStartPosition, selectionEndPosition);
		}

		public override void OnTextEditUpdate(NativeKeyboardEvent keyboardEvent)
		{
			LastTextEditFrame = keyboardEvent.textEditFrame;
			Engine.ApplyTextEditFrame(keyboardEvent.textEditFrame);
		}

		public override void OnDone(NativeKeyboardEvent keyboardEvent)
		{
			Keyboard.ClearEventQueue(); //Should be last event to process, so clear queue
			Engine.ProcessDone();
		}

		public override void OnNext(NativeKeyboardEvent keyboardEvent)
		{
			Keyboard.ClearEventQueue(); //Should be last event to process, so clear queue
			Engine.ProcessDone();
		}

		public override void OnCancel(NativeKeyboardEvent keyboardEvent)
		{
			Keyboard.ClearEventQueue(); //Should be last event to process, so clear queue
			Engine.Deselect(EndEditReason.KEYBOARD_CANCEL);
		}

		public override void OnSpecialKeyPressed(NativeKeyboardEvent keyboardEvent)
		{
			Engine.EventHandler.InvokeSpecialKeyPressed(keyboardEvent.specialKeyCode);
		}

		public override void OnMoveLeft(NativeKeyboardEvent keyboardEvent)
		{
			Engine.MoveLeft(keyboardEvent.shift, keyboardEvent.ctrl);
		}

		public override void OnMoveRight(NativeKeyboardEvent keyboardEvent)
		{
			Engine.MoveRight(keyboardEvent.shift, keyboardEvent.ctrl);
		}

		public override void OnMoveUp(NativeKeyboardEvent keyboardEvent)
		{
			Engine.MoveUp(keyboardEvent.shift, keyboardEvent.ctrl);
		}

		public override void OnMoveDown(NativeKeyboardEvent keyboardEvent)
		{
			Engine.MoveDown(keyboardEvent.shift, keyboardEvent.ctrl);
		}
	}
}
