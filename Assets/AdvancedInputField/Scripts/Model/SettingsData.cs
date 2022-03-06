// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	public enum Platform { STANDALONE, ANDROID, IOS, UWP }

	/// <summary>The behaviour to determine which keyboard type to use</summary>
	public enum MobileKeyboardBehaviour { USE_HARDWARE_KEYBOARD_WHEN_AVAILABLE, ALWAYS_USE_TOUCHSCREENKEYBOARD, ALWAYS_USE_HARDWAREKEYBOARD }

	[Serializable]
	public class PlatformSettingsData
	{
		[Tooltip("The platform")]
		[SerializeField, CustomName("Platform")]
		private Platform platform;

		[Tooltip("Allow the Action Bar to be used")]
		[SerializeField, CustomName("ActionBar Allowed")]
		private bool actionBarAllowed = true;

		[Tooltip("The prefab to use for Action Bar")]
		[SerializeField, CustomName("ActionBar")]
		private ActionBar actionBarPrefab;

		[Tooltip("The prefab to use for Basic Text Selection")]
		[SerializeField, CustomName("Basic Text Selection Prefab")]
		private BasicTextSelectionHandler basicTextSelectionPrefab;

		[Tooltip("Allow the Touch Selection Cursors to be used")]
		[SerializeField, CustomName("Touch Text Selection Allowed")]
		private bool touchTextSelectionAllowed = true;

		[Tooltip("The prefab to use for Touch Text Selection")]
		[SerializeField, CustomName("Touch Text Selection Prefab")]
		private TouchTextSelectionHandler touchTextSelectionPrefab;

		[Tooltip("The scale of the selection cursors (1 is default)")]
		[SerializeField, CustomName("Selection Cursors Scale")]
		[Range(0.01f, 10)]
		private float touchSelectionCursorsScale = 1;

		[Tooltip("The behaviour to determine which keyboard type to use")]
		[SerializeField, CustomName("Keyboard Behaviour")]
		private MobileKeyboardBehaviour mobileKeyboardBehaviour;

		public Platform Platform { get { return platform; } set { platform = value; } }
		public bool ActionBarAllowed { get { return actionBarAllowed; } }
		public ActionBar ActionBarPrefab { get { return actionBarPrefab; } }
		public BasicTextSelectionHandler BasicTextSelectionPrefab { get { return basicTextSelectionPrefab; } }
		public bool TouchTextSelectionAllowed { get { return touchTextSelectionAllowed; } }
		public TouchTextSelectionHandler TouchTextSelectionPrefab { get { return touchTextSelectionPrefab; } }
		public float TouchSelectionCursorsScale { get { return touchSelectionCursorsScale; } }
		public MobileKeyboardBehaviour MobileKeyboardBehaviour { get { return mobileKeyboardBehaviour; } }
	}

	public class SettingsData: ScriptableObject
	{
		[Tooltip("The LocalizationData assets for plugin specific strings (ex. ActionBar buttons)")]
		[SerializeField, CustomName("Localizations")]
		private LocalizationData[] localizations;

		[Tooltip("The platform specific settings")]
		[SerializeField, CustomName("Platform Settings")]
		private PlatformSettingsData[] platformSettings;

		[Tooltip("Indicates whether the plugin should behave like a Mobile Device in the Editor")]
		[SerializeField, CustomName("Simulate Mobile Behaviour In Editor")]
		private bool simulateMobileBehaviourInEditor = true;

		[Tooltip("The prefab to use for simulating the Mobile Keyboard in Portrait mode")]
		[SerializeField, CustomName("Portrait Keyboard Canvas Prefab")]
		private Canvas portraitKeyboardCanvasPrefab;

		[Tooltip("The prefab to use for simulating the Mobile Keyboard in Landscape mode")]
		[SerializeField, CustomName("Landscape Keyboard Canvas Prefab")]
		private Canvas landscapeKeyboardCanvasPrefab;

		[Tooltip("Threshold between 2 taps to count as Double Tap")]
		[SerializeField, CustomName("Double Tap Threshold")]
		private float doubleTapThreshold = 0.5f;

		[Tooltip("The time input needs to be pressed and be kept at same position to count as Hold")]
		[SerializeField, CustomName("Hold Threshold")]
		private float holdThreshold = 1.0f;

		[Tooltip("The character to use when masking text for password input")]
		[SerializeField, CustomName("Password Masking character")]
		private char passwordMaskingCharacter = '*';

		public LocalizationData[] Localizations { get { return localizations; } }
		public PlatformSettingsData[] PlatformSettings { get { return platformSettings; } set { platformSettings = value; } }
		public bool SimulateMobileBehaviourInEditor { get { return simulateMobileBehaviourInEditor; } }
		public Canvas PortraitKeyboardCanvasPrefab { get { return portraitKeyboardCanvasPrefab; } }
		public Canvas LandscapeKeyboardCanvasPrefab { get { return landscapeKeyboardCanvasPrefab; } }
		public float DoubleTapThreshold { get { return doubleTapThreshold; } }
		public float HoldThreshold { get { return holdThreshold; } }
		public char PasswordMaskingCharacter { get { return passwordMaskingCharacter; } }
	}
}