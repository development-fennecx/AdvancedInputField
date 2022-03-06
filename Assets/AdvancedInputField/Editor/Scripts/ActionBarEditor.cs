// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using UnityEditor;

namespace AdvancedInputFieldPlugin.Editor
{
	[CustomEditor(typeof(ActionBar), true)]
	public class ActionBarEditor: UnityEditor.Editor
	{
		private SerializedProperty actionBarRendererProperty;
		private SerializedProperty previousPageButtonProperty;
		private SerializedProperty nextPageButtonProperty;
		private SerializedProperty iconButtonPaddingProperty;
		private SerializedProperty optimizeItemSizesProperty;
		private SerializedProperty preferredFontSizeProperty;
		private SerializedProperty minItemWidthProperty;
		private SerializedProperty horizontalTextPaddingProperty;
		private SerializedProperty verticalTextPaddingProperty;
		private SerializedProperty showDividersProperty;
		private SerializedProperty defaultActionsFirstProperty;
		private SerializedProperty showSoloReplaceActionImmediatelyProperty;

		private void OnEnable()
		{
			actionBarRendererProperty = serializedObject.FindProperty("actionBarRenderer");
			previousPageButtonProperty = serializedObject.FindProperty("previousPageButton");
			nextPageButtonProperty = serializedObject.FindProperty("nextPageButton");
			iconButtonPaddingProperty = serializedObject.FindProperty("iconButtonPadding");
			optimizeItemSizesProperty = serializedObject.FindProperty("optimizeItemSizes");
			preferredFontSizeProperty = serializedObject.FindProperty("preferredFontSize");
			minItemWidthProperty = serializedObject.FindProperty("minItemWidth");
			horizontalTextPaddingProperty = serializedObject.FindProperty("horizontalTextPadding");
			verticalTextPaddingProperty = serializedObject.FindProperty("verticalTextPadding");
			showDividersProperty = serializedObject.FindProperty("showDividers");
			defaultActionsFirstProperty = serializedObject.FindProperty("defaultActionsFirst");
			showSoloReplaceActionImmediatelyProperty = serializedObject.FindProperty("showSoloReplaceActionImmediately");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(actionBarRendererProperty);
			EditorGUILayout.PropertyField(previousPageButtonProperty);
			EditorGUILayout.PropertyField(nextPageButtonProperty);
			EditorGUILayout.PropertyField(showDividersProperty);
			EditorGUILayout.PropertyField(defaultActionsFirstProperty);
			EditorGUILayout.PropertyField(showSoloReplaceActionImmediatelyProperty);

			EditorGUILayout.PropertyField(optimizeItemSizesProperty);
			if(optimizeItemSizesProperty.boolValue)
			{
				EditorGUILayout.PropertyField(preferredFontSizeProperty);
				EditorGUILayout.PropertyField(minItemWidthProperty);
				EditorGUILayout.PropertyField(horizontalTextPaddingProperty);
				EditorGUILayout.PropertyField(verticalTextPaddingProperty);
			}
			EditorGUILayout.PropertyField(iconButtonPaddingProperty);

			serializedObject.ApplyModifiedProperties();
		}
	}
}