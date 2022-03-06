// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	public class Settings
	{
		private const string SETTINGS_PATH = "AdvancedInputField/Settings";

		private static SettingsData data;

		private static SettingsData Data
		{
			get
			{
				if(data == null)
				{
					data = Resources.Load(SETTINGS_PATH) as SettingsData;
				}

				return data;
			}
		}

		public static LocalizationData[] Localizations { get { return Data.Localizations; } }
		public static bool SimulateMobileBehaviourInEditor { get { return Data.SimulateMobileBehaviourInEditor; } }
		public static Canvas PortraitKeyboardCanvasPrefab { get { return Data.PortraitKeyboardCanvasPrefab; } }
		public static Canvas LandscapeKeyboardCanvasPrefab { get { return Data.LandscapeKeyboardCanvasPrefab; } }
		public static float DoubleTapThreshold { get { return Data.DoubleTapThreshold; } }
		public static float HoldThreshold { get { return Data.HoldThreshold; } }
		public static char PasswordMaskingCharacter { get { return Data.PasswordMaskingCharacter; } }

		public static PlatformSettingsData CurrentPlatformSettings
		{
			get
			{
#if UNITY_STANDALONE
				return Data.PlatformSettings[0];
#elif UNITY_ANDROID
				return Data.PlatformSettings[1];
#elif UNITY_IOS
				return Data.PlatformSettings[2];
#elif UNITY_WSA
				return Data.PlatformSettings[3];
#else
				return Data.PlatformSettings[0];
#endif
			}
		}

		public static bool ActionBarAllowed { get { return CurrentPlatformSettings.ActionBarAllowed; } }
		public static ActionBar ActionBarPrefab { get { return CurrentPlatformSettings.ActionBarPrefab; } }
		public static BasicTextSelectionHandler BasicTextSelectionPrefab { get { return CurrentPlatformSettings.BasicTextSelectionPrefab; } }
		public static bool TouchTextSelectionAllowed { get { return CurrentPlatformSettings.TouchTextSelectionAllowed; } }
		public static TouchTextSelectionHandler TouchTextSelectionPrefab { get { return CurrentPlatformSettings.TouchTextSelectionPrefab; } }
		public static float TouchSelectionCursorsScale { get { return CurrentPlatformSettings.TouchSelectionCursorsScale; } }
		public static MobileKeyboardBehaviour MobileKeyboardBehaviour { get { return CurrentPlatformSettings.MobileKeyboardBehaviour; } }
	}
}
