// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using UnityEditor;
using UnityEngine;

namespace AdvancedInputFieldPlugin.Editor
{
	[CustomEditor(typeof(SettingsData), true)]
	public class SettingsDataEditor: UnityEditor.Editor
	{
		private readonly Platform[] SUPPORTED_PLATFORMS = new Platform[]
		{
			Platform.STANDALONE,
			Platform.ANDROID,
			Platform.IOS,
			Platform.UWP
		};

		private SerializedProperty localizationsProperty;
		private SerializedProperty platformSettingsProperty;
		private SerializedProperty simulateMobileBehaviourInEditorProperty;
		private SerializedProperty portraitKeyboardCanvasPrefabProperty;
		private SerializedProperty landscapeKeyboardCanvasPrefabProperty;
		private SerializedProperty doubleTapThresholdProperty;
		private SerializedProperty holdThresholdProperty;
		private SerializedProperty passwordMaskingCharacterProperty;
		private int platformTab;

		private void OnEnable()
		{
			localizationsProperty = serializedObject.FindProperty("localizations");
			platformSettingsProperty = serializedObject.FindProperty("platformSettings");
			simulateMobileBehaviourInEditorProperty = serializedObject.FindProperty("simulateMobileBehaviourInEditor");
			portraitKeyboardCanvasPrefabProperty = serializedObject.FindProperty("portraitKeyboardCanvasPrefab");
			landscapeKeyboardCanvasPrefabProperty = serializedObject.FindProperty("landscapeKeyboardCanvasPrefab");
			doubleTapThresholdProperty = serializedObject.FindProperty("doubleTapThreshold");
			holdThresholdProperty = serializedObject.FindProperty("holdThreshold");
			passwordMaskingCharacterProperty = serializedObject.FindProperty("passwordMaskingCharacter");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			SettingsData settingsData = (SettingsData)target;
			VerifyPlatformSettings(settingsData);

			EditorGUILayout.LabelField("General:", EditorStyles.boldLabel);
			DrawLocalizationsProperty();
			EditorGUILayout.PropertyField(simulateMobileBehaviourInEditorProperty);
			EditorGUILayout.PropertyField(portraitKeyboardCanvasPrefabProperty);
			EditorGUILayout.PropertyField(landscapeKeyboardCanvasPrefabProperty);
			EditorGUILayout.PropertyField(doubleTapThresholdProperty);
			EditorGUILayout.PropertyField(holdThresholdProperty);
			EditorGUILayout.PropertyField(passwordMaskingCharacterProperty);
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Platform Specific:", EditorStyles.boldLabel);
			DrawPlatformSpecificProperties();

			serializedObject.ApplyModifiedProperties();
		}

		public void DrawLocalizationsProperty()
		{
			localizationsProperty.isExpanded = EditorGUILayout.Foldout(localizationsProperty.isExpanded, "Localizations");
			if(localizationsProperty.isExpanded)
			{
				EditorGUI.indentLevel = 1;
				int length = localizationsProperty.arraySize;
				length = EditorGUILayout.IntField("Size", length);

				if(length != localizationsProperty.arraySize)
				{
					while(length > localizationsProperty.arraySize)
					{
						localizationsProperty.InsertArrayElementAtIndex(localizationsProperty.arraySize);
					}
					while(length < localizationsProperty.arraySize)
					{
						localizationsProperty.DeleteArrayElementAtIndex(localizationsProperty.arraySize - 1);
					}
					serializedObject.ApplyModifiedProperties();
				}

				for(int i = 0; i < length; i++)
				{
					SerializedProperty localizationProperty = localizationsProperty.GetArrayElementAtIndex(i);
					EditorGUILayout.ObjectField(localizationProperty, new GUIContent("Element " + i));
				}
				EditorGUI.indentLevel = 0;
			}
		}

		public void VerifyPlatformSettings(SettingsData settingsData)
		{
			int platformsLength = SUPPORTED_PLATFORMS.Length;
			int platformSettingsLength = platformSettingsProperty.arraySize;

			PlatformSettingsData[] platformSettingsDatas = new PlatformSettingsData[platformsLength];
			for(int i = 0; i < platformsLength; i++)
			{
				Platform platform = SUPPORTED_PLATFORMS[i];
				if(i < platformSettingsLength)
				{
					PlatformSettingsData platformSettingsData = settingsData.PlatformSettings[i];
					if(platformSettingsData.Platform != platform)
					{
						platformSettingsData.Platform = platform;
					}

					platformSettingsDatas[i] = platformSettingsData;
				}
			}

			settingsData.PlatformSettings = platformSettingsDatas;
		}

		public void DrawPlatformSpecificProperties()
		{
			platformTab = GUILayout.Toolbar(platformTab, new string[] { "Standalone", "Android", "iOS", "UWP" });
			SerializedProperty selectedPlatformSettingsProperty = platformSettingsProperty.GetArrayElementAtIndex(platformTab);
			SerializedProperty platformProperty = selectedPlatformSettingsProperty.FindPropertyRelative("platform");
			SerializedProperty actionBarAllowedProperty = selectedPlatformSettingsProperty.FindPropertyRelative("actionBarAllowed");
			SerializedProperty actionBarPrefabProperty = selectedPlatformSettingsProperty.FindPropertyRelative("actionBarPrefab");
			SerializedProperty basicTextSelectionPrefabProperty = selectedPlatformSettingsProperty.FindPropertyRelative("basicTextSelectionPrefab");
			SerializedProperty touchTextSelectionAllowedProperty = selectedPlatformSettingsProperty.FindPropertyRelative("touchTextSelectionAllowed");
			SerializedProperty touchTextSelectionPrefabProperty = selectedPlatformSettingsProperty.FindPropertyRelative("touchTextSelectionPrefab");
			SerializedProperty mobileKeyboardBehaviourProperty = selectedPlatformSettingsProperty.FindPropertyRelative("mobileKeyboardBehaviour");

			Platform platform = (Platform)platformProperty.enumValueIndex;

			EditorGUILayout.PropertyField(actionBarAllowedProperty);
			if(actionBarAllowedProperty.boolValue)
			{
				EditorGUILayout.PropertyField(actionBarPrefabProperty);
			}

			EditorGUILayout.PropertyField(basicTextSelectionPrefabProperty);

			EditorGUILayout.PropertyField(touchTextSelectionAllowedProperty);
			if(touchTextSelectionAllowedProperty.boolValue)
			{
				EditorGUILayout.PropertyField(touchTextSelectionPrefabProperty);
			}

			if(platform == Platform.ANDROID || platform == Platform.IOS)
			{
				EditorGUILayout.PropertyField(mobileKeyboardBehaviourProperty);
			}
		}
	}
}