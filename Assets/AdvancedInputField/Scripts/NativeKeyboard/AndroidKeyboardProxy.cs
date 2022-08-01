#if !UNITY_EDITOR && UNITY_ANDROID
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	public class AndroidKeyboardProxy: AndroidJavaProxy
	{
		private AndroidKeyboard keyboard;

		public AndroidKeyboardProxy(AndroidKeyboard keyboard) : base("com.jeroenvanpienbroek.nativekeyboard.INativeKeyboardCallback")
		{
			this.keyboard = keyboard;
		}

		public void OnTextEditUpdate(string text, int selectionStartPosition, int selectionEndPosition)
		{
			ThreadHelper.ScheduleActionOnUnityThread(() => { keyboard.OnTextEditUpdate(text, selectionStartPosition, selectionEndPosition); });
		}

		public void OnAutofillUpdate(string text, int autofillType)
		{
			ThreadHelper.ScheduleActionOnUnityThread(() => { keyboard.OnAutofillUpdate(text, (AutofillType)autofillType); });
		}

		public void OnKeyboardShow()
		{
			ThreadHelper.ScheduleActionOnUnityThread(() => { keyboard.OnKeyboardShow(); });
		}

		public void OnKeyboardHide()
		{
			ThreadHelper.ScheduleActionOnUnityThread(() => { keyboard.OnKeyboardHide(); });
		}

		public void OnKeyboardDone()
		{
			ThreadHelper.ScheduleActionOnUnityThread(() => { keyboard.OnKeyboardDone(); });
		}

		public void OnKeyboardNext()
		{
			ThreadHelper.ScheduleActionOnUnityThread(() => { keyboard.OnKeyboardNext(); });
		}

		public void OnKeyboardCancel()
		{
			ThreadHelper.ScheduleActionOnUnityThread(() => { keyboard.OnKeyboardCancel(); });
		}

		public void OnSpecialKeyPressed(int specialKeyCode)
		{
			ThreadHelper.ScheduleActionOnUnityThread(() => { keyboard.OnSpecialKeyPressed((SpecialKeyCode)specialKeyCode); });
		}

		public void OnKeyboardHeightChanged(int height)
		{
			ThreadHelper.ScheduleActionOnUnityThread(() => { keyboard.OnKeyboardHeightChanged(height); });
		}

		public void OnHardwareKeyboardChanged(bool connected)
		{
			ThreadHelper.ScheduleActionOnUnityThread(() => { keyboard.OnHardwareKeyboardChanged(connected); });
		}
	}
}
#endif