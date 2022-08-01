
#if !UNITY_EDITOR && UNITY_ANDROID
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Class that acts as a bridge for the Native Android Keyboard</summary>
	public class AndroidKeyboard: NativeKeyboard
	{
		/// <summary>The main Android class</summary>
		private AndroidJavaClass mainClass;

		/// <summary>The proxy Android class</summary>
		private AndroidKeyboardProxy proxy;

		internal override void Setup()
		{
			if(ThreadHelper.Instance == null) { ThreadHelper.CreateInstance(); }
			proxy = new AndroidKeyboardProxy(this);
			mainClass = new AndroidJavaClass("com.jeroenvanpienbroek.nativekeyboard.NativeKeyboard");
			mainClass.CallStatic("initialize", proxy);
		}

		public override void EnableUpdates()
		{
			mainClass.CallStatic("enableUpdates");
		}

		public override void DisableUpdates()
		{
			mainClass.CallStatic("disableUpdates");
		}

		public override void EnableHardwareKeyboardUpdates()
		{
			mainClass.CallStatic("enableHardwareKeyboardUpdates");
		}

		public override void DisableHardwareKeyboardUpdates()
		{
			mainClass.CallStatic("disableHardwareKeyboardUpdates");
		}

		public override void UpdateTextEdit(string text, int selectionStartPosition, int selectionEndPosition)
		{
			mainClass.CallStatic("updateTextEdit", text, selectionStartPosition, selectionEndPosition);
		}

		public override void ShowKeyboard(string text, int selectionStartPosition, int selectionEndPosition, NativeKeyboardConfiguration configuration)
		{
			string configurationJSON = JsonUtility.ToJson(configuration);
			mainClass.CallStatic("showKeyboard", text, selectionStartPosition, selectionEndPosition, configurationJSON);
		}

		public override void RestoreKeyboard()
		{
			mainClass.CallStatic("restoreKeyboard");
		}

		public override void HideKeyboard()
		{
			mainClass.CallStatic("hideKeyboard");
		}

		public override void ResetAutofill()
		{ 
			mainClass.CallStatic("resetAutofill");
		}

		public override void SaveCredentials(string domainName) 
		{ 
			mainClass.CallStatic("saveCredentials");
		}

		public override void StartListeningForOneTimeCodes()
		{ 
			mainClass.CallStatic("startListeningForOneTimeCodes");
		}
	}
}
#endif