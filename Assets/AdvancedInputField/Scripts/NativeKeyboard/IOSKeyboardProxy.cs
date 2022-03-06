// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#if !UNITY_EDITOR && UNITY_IOS
using System.Runtime.InteropServices;
using System;
using AOT;
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	public class IOSKeyboardProxy
	{
		[DllImport("__Internal")]
		private static extern void _nativeKeyboard_initialize(NKBOnTextEditUpdate onTextEditUpdate, NKBOnAutofillUpdate onAutofillUpdate, NKBOnKeyboardShow onKeyboardShow, NKBOnKeyboardHide onKeyboardHide, NKBOnKeyboardDone onKeyboardDone, NKBOnKeyboardNext onKeyboardNext, NKBOnKeyboardCancel onKeyboardCancel, NKBOnSpecialKeyPressed onSpecialKeyPressed, NKBOnKeyboardHeightChanged onKeyboardHeightChanged, NKBOnHardwareKeyboardChanged onHardwareKeyboardChanged);

		public delegate void NKBOnTextEditUpdate(string text, int selectionStartPosition, int selectionEndPosition);
		public delegate void NKBOnAutofillUpdate(string text, int autofillType);
		public delegate void NKBOnKeyboardShow();
		public delegate void NKBOnKeyboardHide();
		public delegate void NKBOnKeyboardDone();
		public delegate void NKBOnKeyboardNext();
		public delegate void NKBOnKeyboardCancel();
		public delegate void NKBOnSpecialKeyPressed(SpecialKeyCode specialKeyCode);
		public delegate void NKBOnKeyboardHeightChanged(int height);
		public delegate void NKBOnHardwareKeyboardChanged(bool connected);

		private static IOSKeyboard keyboard;

		public IOSKeyboardProxy(IOSKeyboard keyboard)
		{
			IOSKeyboardProxy.keyboard = keyboard;
			_nativeKeyboard_initialize(OnTextEditUpdate, OnAutofillUpdate, OnKeyboardShow, OnKeyboardHide, OnKeyboardDone, OnKeyboardNext, OnKeyboardCancel, OnSpecialKeyPressed, OnKeyboardHeightChanged, OnHardwareKeyboardChanged);
		}

		[MonoPInvokeCallback(typeof(NKBOnTextEditUpdate))]
		public static void OnTextEditUpdate(string text, int selectionStartPosition, int selectionEndPosition)
		{
			ThreadHelper.ScheduleActionOnUnityThread(() => { keyboard.OnTextEditUpdate(text, selectionStartPosition, selectionEndPosition); });
		}

		[MonoPInvokeCallback(typeof(NKBOnAutofillUpdate))]
		public static void OnAutofillUpdate(string text, int autofillType)
		{
			ThreadHelper.ScheduleActionOnUnityThread(() => { keyboard.OnAutofillUpdate(text, (AutofillType)autofillType); });
		}

		[MonoPInvokeCallback(typeof(NKBOnKeyboardShow))]
		public static void OnKeyboardShow()
		{
			ThreadHelper.ScheduleActionOnUnityThread(() => { keyboard.OnKeyboardShow(); });
		}

		[MonoPInvokeCallback(typeof(NKBOnKeyboardHide))]
		public static void OnKeyboardHide()
		{
			ThreadHelper.ScheduleActionOnUnityThread(() => { keyboard.OnKeyboardHide(); });
		}

		[MonoPInvokeCallback(typeof(NKBOnKeyboardDone))]
		public static void OnKeyboardDone()
		{
			ThreadHelper.ScheduleActionOnUnityThread(() => { keyboard.OnKeyboardDone(); });
		}

		[MonoPInvokeCallback(typeof(NKBOnKeyboardNext))]
		public static void OnKeyboardNext()
		{
			ThreadHelper.ScheduleActionOnUnityThread(() => { keyboard.OnKeyboardNext(); });
		}

		[MonoPInvokeCallback(typeof(NKBOnKeyboardCancel))]
		public static void OnKeyboardCancel()
		{
			ThreadHelper.ScheduleActionOnUnityThread(() => { keyboard.OnKeyboardCancel(); });
		}

		[MonoPInvokeCallback(typeof(NKBOnSpecialKeyPressed))]
		public static void OnSpecialKeyPressed(SpecialKeyCode specialKeyCode)
		{
			ThreadHelper.ScheduleActionOnUnityThread(() => { keyboard.OnSpecialKeyPressed(specialKeyCode); });
		}

		[MonoPInvokeCallback(typeof(NKBOnKeyboardHeightChanged))]
		public static void OnKeyboardHeightChanged(int height)
		{
			ThreadHelper.ScheduleActionOnUnityThread(() => { keyboard.OnKeyboardHeightChanged(height); });
		}

		[MonoPInvokeCallback(typeof(NKBOnHardwareKeyboardChanged))]
		public static void OnHardwareKeyboardChanged(bool connected)
		{
			ThreadHelper.ScheduleActionOnUnityThread(() => { keyboard.OnHardwareKeyboardChanged(connected); });
		}
	}
}
#endif