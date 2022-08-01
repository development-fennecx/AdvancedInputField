#if ADVANCEDINPUTFIELD_TEXTMESHPRO
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;

namespace AdvancedInputFieldPlugin.Editor
{
	[CustomEditor(typeof(TMProEmojiAsset), true), CanEditMultipleObjects]
	public class TMProEmojiAssetEditor: TMP_SpriteAssetEditor
	{
		private readonly Platform[] SUPPORTED_PLATFORMS = new Platform[]
		{
			Platform.STANDALONE,
			Platform.ANDROID,
			Platform.IOS,
			Platform.UWP
		};

		private const int EMOJIS_PER_PAGE = 10;

		private SerializedProperty emojisProperty;
		private SerializedProperty platformSettingsProperty;
		private int emojiPageIndex;
		private int platformTab;

		public new void OnEnable()
		{
			base.OnEnable();

			emojisProperty = serializedObject.FindProperty("emojis");
			platformSettingsProperty = serializedObject.FindProperty("platformSettings");
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			serializedObject.Update();

			TMProEmojiAsset emojiAsset = (TMProEmojiAsset)target;
			VerifyPlatformSettings(emojiAsset);
			serializedObject.Update();

			DrawEmojisProperty();
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Platform Specific:", EditorStyles.boldLabel);
			DrawPlatformSpecificProperties();

			serializedObject.ApplyModifiedProperties();
		}

		public void DrawEmojisProperty()
		{
			emojisProperty.isExpanded = EditorGUILayout.Foldout(emojisProperty.isExpanded, "Emojis");
			if(emojisProperty.isExpanded)
			{
				GUI.enabled = false;
				EditorGUI.indentLevel = 1;
				int length = emojisProperty.arraySize;
				EditorGUILayout.IntField("Size", length);

				int startIndex = (emojiPageIndex * EMOJIS_PER_PAGE);
				for(int i = startIndex; i < startIndex + EMOJIS_PER_PAGE; i++)
				{
					if(i >= length) { break; }

					SerializedProperty emojiProperty = emojisProperty.GetArrayElementAtIndex(i);
					DrawEmojiProperty(emojiProperty, i);
				}

				GUI.enabled = true;
				EditorGUILayout.BeginHorizontal();
				if(GUILayout.Button("Previous"))
				{
					emojiPageIndex = Mathf.Max(emojiPageIndex - 1, 0);
				}
				if(GUILayout.Button("Next"))
				{
					if((emojiPageIndex * EMOJIS_PER_PAGE) < length)
					{
						emojiPageIndex++;
					}
				}
				EditorGUILayout.EndHorizontal();
				EditorGUI.indentLevel = 0;
			}
		}

		public void DrawEmojiProperty(SerializedProperty emojiProperty, int emojiIndex)
		{
			SerializedProperty nameProperty = emojiProperty.FindPropertyRelative("name");
			SerializedProperty textProperty = emojiProperty.FindPropertyRelative("text");
			SerializedProperty richTextProperty = emojiProperty.FindPropertyRelative("richText");

			emojiProperty.isExpanded = EditorGUILayout.Foldout(emojiProperty.isExpanded, "Emoji " + emojiIndex);
			if(emojiProperty.isExpanded)
			{
				EditorGUI.indentLevel = 2;
				EditorGUILayout.PropertyField(nameProperty);
				EditorGUILayout.PropertyField(textProperty);
				EditorGUILayout.PropertyField(richTextProperty);
				EditorGUI.indentLevel = 1;
			}
		}

		public void VerifyPlatformSettings(TMProEmojiAsset emojiAsset)
		{
			int platformsLength = SUPPORTED_PLATFORMS.Length;
			int platformSettingsLength = platformSettingsProperty.arraySize;

			TMProEmojiPlatformSettingsData[] platformSettingsDatas = new TMProEmojiPlatformSettingsData[platformsLength];
			for(int i = 0; i < platformsLength; i++)
			{
				Platform platform = SUPPORTED_PLATFORMS[i];
				if(i < platformSettingsLength)
				{
					TMProEmojiPlatformSettingsData platformSettingsData = emojiAsset.PlatformSettings[i];
					if(platformSettingsData.Platform != platform)
					{
						platformSettingsData.Platform = platform;
					}

					platformSettingsDatas[i] = platformSettingsData;
				}
			}

			emojiAsset.PlatformSettings = platformSettingsDatas;
		}

		public void DrawPlatformSpecificProperties()
		{
			platformTab = GUILayout.Toolbar(platformTab, new string[] { "Standalone", "Android", "iOS", "UWP" });
			SerializedProperty selectedPlatformSettingsProperty = platformSettingsProperty.GetArrayElementAtIndex(platformTab);
			SerializedProperty spriteAtlasProperty = selectedPlatformSettingsProperty.FindPropertyRelative("spriteAtlas");
			EditorGUILayout.PropertyField(spriteAtlasProperty);
		}
	}
}
#endif