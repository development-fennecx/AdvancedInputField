// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
using TMPro;
using System.Collections.Generic;
#endif
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
	public class TMProEmojiAsset: TMP_SpriteAsset
	{
		[SerializeField]
		[CustomName("Emojis")]
		private List<EmojiData> emojis;

		public List<EmojiData> Emojis { get { return emojis; } }

#if UNITY_EDITOR
		[Tooltip("The platform specific settings")]
		[SerializeField, CustomName("Platform Settings")]
		private TMProEmojiPlatformSettingsData[] platformSettings;

		public TMProEmojiPlatformSettingsData[] PlatformSettings { get { return platformSettings; } set { platformSettings = value; } }

		private void OnValidate()
		{
			ApplyPlatformSettings();
		}

		public void ApplyPlatformSettings()
		{
			int length = platformSettings.Length;
			int index = -1;
#if UNITY_STANDALONE
			index = 0;
#elif UNITY_ANDROID
			index = 1;
#elif UNITY_IOS
			index = 2;
#elif UNITY_WSA
			index = 3;
#else
			index = 0;
#endif
			if(index != -1 && index < length)
			{
				TMProEmojiPlatformSettingsData currentPlatformSettings = platformSettings[index];
				if(currentPlatformSettings.SpriteAtlas != null)
				{
					spriteSheet = currentPlatformSettings.SpriteAtlas;
					if(material != null)
					{
						material.SetTexture(ShaderUtilities.ID_MainTex, spriteSheet);
					}
				}
			}
		}
#endif
	}

	[Serializable]
	public class TMProEmojiPlatformSettingsData
	{
		[Tooltip("The platform")]
		[SerializeField, CustomName("Platform")]
		private Platform platform;

		[Tooltip("The sprite atlas to use")]
		[SerializeField, CustomName("Sprite Atlas")]
		private Texture spriteAtlas;

		public Platform Platform { get { return platform; } set { platform = value; } }
		public Texture SpriteAtlas { get { return spriteAtlas; } set { spriteAtlas = value; } }
	}
#endif
}
