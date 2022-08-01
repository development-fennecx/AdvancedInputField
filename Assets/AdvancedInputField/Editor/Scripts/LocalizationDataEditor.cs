using UnityEditor;

namespace AdvancedInputFieldPlugin.Editor
{
	[CustomEditor(typeof(LocalizationData), true)]
	public class LocalizationDataEditor: UnityEditor.Editor
	{
		private SerializedProperty languageProperty;
		private SerializedProperty localizedStringsProperty;

		private void OnEnable()
		{
			languageProperty = serializedObject.FindProperty("language");
			localizedStringsProperty = serializedObject.FindProperty("localizedStrings");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(languageProperty);
			DrawLocalizedStringsProperty();

			serializedObject.ApplyModifiedProperties();
		}

		public void DrawLocalizedStringsProperty()
		{
			localizedStringsProperty.isExpanded = EditorGUILayout.Foldout(localizedStringsProperty.isExpanded, "Localized Strings");
			if(localizedStringsProperty.isExpanded)
			{
				EditorGUI.indentLevel = 1;
				int length = localizedStringsProperty.arraySize;
				length = EditorGUILayout.IntField("Size", length);

				if(length != localizedStringsProperty.arraySize)
				{
					while(length > localizedStringsProperty.arraySize)
					{
						localizedStringsProperty.InsertArrayElementAtIndex(localizedStringsProperty.arraySize);
					}
					while(length < localizedStringsProperty.arraySize)
					{
						localizedStringsProperty.DeleteArrayElementAtIndex(localizedStringsProperty.arraySize - 1);
					}
					serializedObject.ApplyModifiedProperties();
				}

				for(int i = 0; i < length; i++)
				{
					SerializedProperty localizedStringProperty = localizedStringsProperty.GetArrayElementAtIndex(i);
					DrawLocalizedStringProperty(localizedStringProperty, i);
				}
				EditorGUI.indentLevel = 0;
			}
		}

		public void DrawLocalizedStringProperty(SerializedProperty localizedStringProperty, int ruleIndex)
		{
			SerializedProperty keyProperty = localizedStringProperty.FindPropertyRelative("key");
			SerializedProperty valueProperty = localizedStringProperty.FindPropertyRelative("value");

			EditorGUILayout.PropertyField(keyProperty);
			EditorGUILayout.PropertyField(valueProperty);
		}
	}
}
