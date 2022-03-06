// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AdvancedInputFieldPlugin
{
	/// <summary>Access point for the NativeKeyboard for current platform</summary>
	public class NativeKeyboardManager: MonoBehaviour
	{
		/// <summary>The singleton instance of NativeKeyboardManager</summary>
		private static NativeKeyboardManager instance;

		/// <summary>The NativeKeyboard instance of current platform</summary>
		private NativeKeyboard keyboard;

		private EmojiEngine emojiEngine;
		private RichTextBindingEngine richTextBindingEngine;

		/// <summary>The last selected AdvancedInputField instance (if any)</summary>
		public AdvancedInputField lastSelectedInputField;

		/// <summary>The active AdvancedInputField instance (if any) before the app got paused</summary>
		private AdvancedInputField activeInputFieldBeforePause;

#if UNITY_EDITOR
		private static bool editorInPlayMode = true;
#endif

		/// <summary>The singleton instance of NativeKeyboardManager</summary>
		public static NativeKeyboardManager Instance
		{
			get
			{
				ValidateInstance();
				return instance;
			}
		}

		internal static void CreateInstance()
		{
			instance = FindObjectOfType<NativeKeyboardManager>();
			if(instance == null)
			{
				GameObject gameObject = new GameObject("NativeKeyboardManager");
				DontDestroyOnLoad(gameObject);
				instance = gameObject.AddComponent<NativeKeyboardManager>();
			}
		}

		/// <summary>The NativeKeyboard instance of current platform</summary>
		public static NativeKeyboard Keyboard
		{
			get
			{
				if(!ValidateInstance()) { return null; }
				return Instance.keyboard;
			}
		}

		public static EmojiEngine EmojiEngine
		{
			get
			{
				if(!ValidateInstance()) { return null; }
				return Instance.emojiEngine;
			}
		}

		public static RichTextBindingEngine RichTextBindingEngine
		{
			get
			{
				if(!ValidateInstance()) { return null; }
				return Instance.richTextBindingEngine;
			}
		}

		/// <summary>The last selected AdvancedInputField instance</summary>
		public static AdvancedInputField LastSelectedInputField
		{
			get { return Instance.lastSelectedInputField; }
			set { Instance.lastSelectedInputField = value; }
		}

		/// <summary>The active AdvancedInputField instance (if any) before the app got paused</summary>
		public static AdvancedInputField ActiveInputFieldBeforePause
		{
			get { return Instance.activeInputFieldBeforePause; }
			set { Instance.activeInputFieldBeforePause = value; }
		}

		/// <summary>Indicates whether a hardware keyboard is connected</summary>
		public static bool HardwareKeyboardConnected
		{
			get { return Instance.keyboard.HardwareKeyboardConnected; }
		}

		public static bool InstanceValid
		{
			get
			{
				if(instance == null) { return false; }
				else if(instance.gameObject == null && !ReferenceEquals(instance.gameObject, null)) { return false; } //Pending destruction
				return true;
			}
		}

		#region UNITY
		private void Awake()
		{
#if UNITY_EDITOR
#if(UNITY_ANDROID || UNITY_IOS)
			if(Settings.SimulateMobileBehaviourInEditor)
			{
				Canvas mobileKeyboardCanvas = null;
				if(Screen.height > Screen.width)
				{
					mobileKeyboardCanvas = GameObject.Instantiate(Settings.PortraitKeyboardCanvasPrefab);
				}
				else
				{
					mobileKeyboardCanvas = GameObject.Instantiate(Settings.LandscapeKeyboardCanvasPrefab);
				}
				DontDestroyOnLoad(mobileKeyboardCanvas.gameObject);
				keyboard = mobileKeyboardCanvas.GetComponentInChildren<SimulatorKeyboard>();
				keyboard.Init(name);
			}
			else
			{
				keyboard = gameObject.AddComponent<StandaloneKeyboard>();
				keyboard.Init(name);
			}
#elif UNITY_STANDALONE || UNITY_WSA || UNITY_WEBGL
			keyboard = gameObject.AddComponent<StandaloneKeyboard>();
			keyboard.Init(name);
#endif
#elif UNITY_ANDROID
			keyboard = gameObject.AddComponent<AndroidKeyboard>();
			keyboard.Init(name);
#elif UNITY_IOS
			keyboard = gameObject.AddComponent<IOSKeyboard>();
			keyboard.Init(name);
#elif UNITY_WSA || UNITY_STANDALONE || UNITY_WEBGL
			keyboard = gameObject.AddComponent<StandaloneKeyboard>();
			keyboard.Init(name);
#else
			Debug.LogWarning("Native Keyboard is not supported on this platform");
#endif
			emojiEngine = gameObject.AddComponent<EmojiEngine>();
			richTextBindingEngine = gameObject.AddComponent<RichTextBindingEngine>();
		}

#if UNITY_EDITOR
		private void OnEnable()
		{
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}

		private void OnDisable()
		{
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		}

		private void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
		{
			switch(playModeStateChange)
			{
				case PlayModeStateChange.EnteredPlayMode: editorInPlayMode = true; break;
				case PlayModeStateChange.ExitingPlayMode: editorInPlayMode = false; break;
			}
		}
#endif

		private void OnDestroy()
		{
			instance = null;
		}

		private void OnApplicationPause(bool pause)
		{
			if(!pause)
			{
				if(activeInputFieldBeforePause != null)
				{
					activeInputFieldBeforePause.ManualDeselect();
					StartCoroutine(DelayedRestore());
				}
			}
		}

		private IEnumerator DelayedRestore()
		{
			float totalWaitTime = 0;
			while(Keyboard.State != KeyboardState.HIDDEN)
			{
				yield return new WaitForSeconds(0.1f);
				totalWaitTime += 0.1f;
				if(totalWaitTime > 1f)
				{
					break;
				}
			}
			activeInputFieldBeforePause.ManualSelect();
		}
		#endregion

		/// <summary>Manually preload the plugin resources (Normally the plugin resources are automatically loaded the first time an inputfield gets focused)</summary>
		public static void Initialize()
		{
			ValidateInstance();
		}

		public static void TryDestroy()
		{
			if(instance != null && instance.gameObject != null)
			{
				Destroy(instance.gameObject);
			}
		}

		/// <summary>Check if current instance is valid or can be created</summary>
		/// <returns>true if there is a valid instance</returns>
		public static bool ValidateInstance()
		{
			if(InstanceValid) { return true; } //Instance is still valid
#if UNITY_EDITOR
			if(!editorInPlayMode) { return false; } //Not going to recreate instance when leaving play mode
#endif

			if(instance == null)
			{
				CreateInstance();
			}
			return true;
		}

		/// <summary>Checks whether the native binding should be active or not</summary>
		public static void UpdateKeyboardActiveState()
		{
			if(!ValidateInstance()) { return; }
			Keyboard.UpdateActiveState();
		}

		/// <summary>
		/// Enables hardware keyboard connectivity checks in the native binding.
		/// Use this when you want connectivity checks when no inputfield is selected.
		/// </summary>
		public static void EnableHardwareKeyboardUpdates()
		{
			if(!ValidateInstance()) { return; }
			Keyboard.EnableHardwareKeyboardUpdates();
		}

		/// <summary>
		/// Disables hardware keyboard connectivity checks in the native binding.
		/// Use this when you want to disable connectivity checks after using EnableHardwareKeyboardUpdates.
		/// </summary>
		public static void DisableHardwareKeyboardUpdates()
		{
			if(!ValidateInstance()) { return; }
			Keyboard.DisableHardwareKeyboardUpdates();
		}

		/// <summary>Updates the native text and selection</summary>
		public static void UpdateTextEdit(string text, int selectionStartPosition, int selectionEndPosition)
		{
			if(!ValidateInstance()) { return; }
			Keyboard.UpdateTextEdit(text, selectionStartPosition, selectionEndPosition);
		}

		/// <summary>Shows the TouchScreenKeyboard for current platform</summary>
		/// <param name="keyboardType">The keyboard type to use</param>
		/// <param name="characterValidation">The characterValidation to use</param>
		/// <param name="lineType">The lineType to use</param>
		/// <param name="autocorrection">Indicates whether autocorrection is enabled</param>
		/// <param name="characterLimit">The character limit for the text</param>
		/// <param name="secure">Indicates whether input should be secure</param>
		public static void ShowKeyboard(string text, int selectionStartPosition, int selectionEndPosition, NativeKeyboardConfiguration configuration)
		{
			if(!ValidateInstance()) { return; }
			Keyboard.ShowKeyboard(text, selectionStartPosition, selectionEndPosition, configuration);
		}

		/// <summary>Shows the TouchScreenKeyboard for current platform without changing settings</summary>
		public static void RestoreKeyboard()
		{
			if(!ValidateInstance()) { return; }
			Keyboard.RestoreKeyboard();
		}

		/// <summary>Hides the TouchScreenKeyboard for current platform</summary>
		public static void HideKeyboard()
		{
			if(!ValidateInstance()) { return; }
			Keyboard.HideKeyboard();
		}

		/// <summary>(Android only) Starts listening for sms messages with one time codes until timeout (5 minutes)</summary>
		public static void StartListeningForOneTimeCodes()
		{
			if(!ValidateInstance()) { return; }
			Keyboard.StartListeningForOneTimeCodes();
		}

		/// <summary>Resets the autofill service for current platform (Android only)</summary>
		public static void ResetAutofill()
		{
			if(!ValidateInstance()) { return; }
			Keyboard.ResetAutofill();
		}

		/// <summary>Resets the autofill service for current platform (Android & iOS only)</summary>
		/// <param name="domainName">The domain name of your website, only needed for iOS</param>
		public static void SaveCredentials(string domainName)
		{
			if(!ValidateInstance()) { return; }
			Keyboard.SaveCredentials(domainName);
		}

		/// <summary>Adds a KeyboardHeightChangedListener</summary>
		/// <param name="listener">The KeyboardHeightChangedListener to add</param>
		public static void AddKeyboardHeightChangedListener(OnKeyboardHeightChangedHandler listener)
		{
			if(!ValidateInstance()) { return; }
			Keyboard.AddKeyboardHeightChangedListener(listener);
		}

		/// <summary>Removes a KeyboardHeightChangdeListener</summary>
		/// <param name="listener">The KeyboardHeightChangedListener to remove</param>
		public static void RemoveKeyboardHeightChangedListener(OnKeyboardHeightChangedHandler listener)
		{
			if(!ValidateInstance()) { return; } //No need to remove event listener if instance is null
			Keyboard.RemoveKeyboardHeightChangedListener(listener);
		}

		/// <summary>Adds a KeyboardHeightChangedListener</summary>
		/// <param name="listener">The HardwareKeyboardChangedListener to add</param>
		public static void AddHardwareKeyboardChangedListener(OnHardwareKeyboardChangedHandler listener)
		{
			if(!ValidateInstance()) { return; }
			Keyboard.AddHardwareKeyboardChangedListener(listener);
		}

		/// <summary>Removes a KeyboardHeightChangedListener</summary>
		/// <param name="listener">The KeyboardHeightChangedListener to remove</param>
		public static void RemoveHardwareKeyboardChangedListener(OnHardwareKeyboardChangedHandler listener)
		{
			if(!ValidateInstance()) { return; } //No need to remove event listener if instance is null
			Keyboard.RemoveHardwareKeyboardChangedListener(listener);
		}
	}
}
