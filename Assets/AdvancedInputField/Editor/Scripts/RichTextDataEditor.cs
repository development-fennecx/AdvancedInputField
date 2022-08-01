//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using UnityEditor;
using UnityEngine;

namespace AdvancedInputFieldPlugin.Editor
{
	[CustomEditor(typeof(RichTextData), true)]
	public class RichTextDataEditor: UnityEditor.Editor
	{
		private SerializedProperty supportedTagsProperty;

		private void OnEnable()
		{
			supportedTagsProperty = serializedObject.FindProperty("tags");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			RichTextData settingsData = (RichTextData)target;

			EditorGUILayout.LabelField("General:", EditorStyles.boldLabel);
			EditorGUILayout.LabelField("The Rich Text tags you want to support.");
			EditorGUILayout.LabelField("Less is faster, so only use the ones you need.");
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("NOTE: Unity Text Renderer doesn't support all tags listed. Only a few basic ones.");
			EditorGUILayout.LabelField("(See: https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/StyledText.html)");
			EditorGUILayout.LabelField("Use the TextMeshPro Text Renderer instead when needed.");
			EditorGUILayout.LabelField("(See: http://digitalnativestudios.com/textmeshpro/docs/rich-text/)");
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			DrawSupportedTagsProperty();

			serializedObject.ApplyModifiedProperties();
		}

		public void DrawSupportedTagsProperty()
		{
			supportedTagsProperty.isExpanded = EditorGUILayout.Foldout(supportedTagsProperty.isExpanded, "Tags");
			if(supportedTagsProperty.isExpanded)
			{
				EditorGUI.indentLevel = 1;
				int length = supportedTagsProperty.arraySize;
				length = EditorGUILayout.IntField("Size", length);

				if(length != supportedTagsProperty.arraySize)
				{
					while(length > supportedTagsProperty.arraySize)
					{
						supportedTagsProperty.InsertArrayElementAtIndex(supportedTagsProperty.arraySize);
					}
					while(length < supportedTagsProperty.arraySize)
					{
						supportedTagsProperty.DeleteArrayElementAtIndex(supportedTagsProperty.arraySize - 1);
					}
					serializedObject.ApplyModifiedProperties();
				}

				for(int i = 0; i < length; i++)
				{
					SerializedProperty supportedTagProperty = supportedTagsProperty.GetArrayElementAtIndex(i);
					EditorGUILayout.PropertyField(supportedTagProperty, new GUIContent("Element " + i));
				}
				EditorGUI.indentLevel = 0;
			}
		}
	}
}