namespace AdvancedInputFieldPlugin
{
	public interface INativeKeyboardCallback
	{
		void OnTextEditUpdate(string text, int selectionStartPosition, int selectionEndPosition);
		void OnKeyboardShow();
		void OnKeyboardHide();
		void OnKeyboardDone();
		void OnKeyboardNext();
		void OnKeyboardCancel();
		void OnKeyboardHeightChanged(int height);
		void OnHardwareKeyboardChanged(bool connected);
	}
}
