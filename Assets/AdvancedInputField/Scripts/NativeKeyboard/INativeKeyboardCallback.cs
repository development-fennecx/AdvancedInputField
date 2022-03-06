// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

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
