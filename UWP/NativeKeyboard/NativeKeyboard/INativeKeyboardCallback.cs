namespace NativeKeyboardUWP
{
	public interface INativeKeyboardCallback
	{
		void OnTextEditUpdate(string text, int selectionStartPosition, int selectionEndPosition);
		void OnKeyboardHeightChanged(int height);
		void OnHardwareKeyboardChanged(bool connected);
		void OnKeyboardShow();
		void OnKeyboardHide();
		void OnKeyboardDone();
		void OnKeyboardNext();
		void OnKeyboardCancel();
		void DebugLine(string message);
	}
}
