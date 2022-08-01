//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

#if !UNITY_EDITOR && UNITY_IOS
using System.Runtime.InteropServices;
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Class that acts as a bridge for the Native iOS Keyboard</summary>
	public class IOSKeyboard: NativeKeyboard
	{
		[DllImport("__Internal")]
		private static extern void _nativeKeyboard_updateTextEdit(string text, int selectionStartPosition, int selectionEndPosition);
		[DllImport("__Internal")]
		private static extern void _nativeKeyboard_requestTextEditUpdate();
		[DllImport("__Internal")]
		private static extern void _nativeKeyboard_showKeyboard(string text, int selectionStartPosition, int selectionEndPosition, string configurationJSON);
		[DllImport("__Internal")]
		private static extern void _nativeKeyboard_restoreKeyboard();
		[DllImport("__Internal")]
		private static extern void _nativeKeyboard_hideKeyboard();
		[DllImport("__Internal")]
		private static extern void _nativeKeyboard_enableUpdates();
		[DllImport("__Internal")]
		private static extern void _nativeKeyboard_disableUpdates();
		[DllImport("__Internal")]
		private static extern void _nativeKeyboard_enableHardwareKeyboardUpdates();
		[DllImport("__Internal")]
		private static extern void _nativeKeyboard_disableHardwareKeyboardUpdates();
		[DllImport("__Internal")]
		private static extern void _nativeKeyboard_saveCredentials(string domainName);

		/// <summary>The proxy iOS class</summary>
		private IOSKeyboardProxy proxy;

		internal override void Setup()
		{
			if(ThreadHelper.Instance == null){ ThreadHelper.CreateInstance(); }
			proxy = new IOSKeyboardProxy(this);
		}

		public override void EnableUpdates()
		{	
			_nativeKeyboard_enableUpdates();
		}

		public override void DisableUpdates()
		{
			_nativeKeyboard_disableUpdates();
		}
	
		public override void EnableHardwareKeyboardUpdates()
		{
			_nativeKeyboard_enableHardwareKeyboardUpdates();
		}

		public override void DisableHardwareKeyboardUpdates()
		{
			_nativeKeyboard_disableHardwareKeyboardUpdates();
		}

		public override void UpdateTextEdit(string text, int selectionStartPosition, int selectionEndPosition)
		{
			_nativeKeyboard_updateTextEdit(text, selectionStartPosition, selectionEndPosition);
		}

		public override void RequestTextEditUpdate()
		{
			_nativeKeyboard_requestTextEditUpdate();
		}

		public override void ShowKeyboard(string text, int selectionStartPosition, int selectionEndPosition, NativeKeyboardConfiguration configuration)
		{
			string configurationJSON = JsonUtility.ToJson(configuration);
			_nativeKeyboard_showKeyboard(text, selectionStartPosition, selectionEndPosition, configurationJSON);
		}

		public override void RestoreKeyboard()
		{
			_nativeKeyboard_restoreKeyboard();
		}

		public override void HideKeyboard()
		{
			_nativeKeyboard_hideKeyboard();
		}

		public override void SaveCredentials(string domainName)
		{
			_nativeKeyboard_saveCredentials(domainName);
		}
	}
}
#endif