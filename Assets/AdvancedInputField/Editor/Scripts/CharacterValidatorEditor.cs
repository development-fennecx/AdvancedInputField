using UnityEditor;
using UnityEngine;

namespace AdvancedInputFieldPlugin.Editor
{
	[CustomEditor(typeof(CharacterValidator), true)]
	public class CharacterValidatorEditor: UnityEditor.Editor
	{
		private SerializedProperty rulesProperty;
		private SerializedProperty otherCharacterActionProperty;
		private SerializedProperty otherCharacterActionIntValueProperty;

		private void OnEnable()
		{
			rulesProperty = serializedObject.FindProperty("rules");
			otherCharacterActionProperty = serializedObject.FindProperty("otherCharacterAction");
			otherCharacterActionIntValueProperty = serializedObject.FindProperty("otherCharacterActionIntValue");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			DrawRulesProperty();
			EditorGUILayout.Space();
			DrawOtherCharactersProperty();

			serializedObject.ApplyModifiedProperties();
		}

		public void DrawRulesProperty()
		{
			EditorGUILayout.LabelField("Character Rules:", EditorStyles.boldLabel);
			rulesProperty.isExpanded = EditorGUILayout.Foldout(rulesProperty.isExpanded, "Rules");
			if(rulesProperty.isExpanded)
			{
				EditorGUI.indentLevel = 1;
				int length = rulesProperty.arraySize;
				length = EditorGUILayout.IntField("Size", length);

				if(length != rulesProperty.arraySize)
				{
					while(length > rulesProperty.arraySize)
					{
						rulesProperty.InsertArrayElementAtIndex(rulesProperty.arraySize);
					}
					while(length < rulesProperty.arraySize)
					{
						rulesProperty.DeleteArrayElementAtIndex(rulesProperty.arraySize - 1);
					}
					serializedObject.ApplyModifiedProperties();
				}

				for(int i = 0; i < length; i++)
				{
					SerializedProperty charRuleProperty = rulesProperty.GetArrayElementAtIndex(i);
					DrawRuleProperty(charRuleProperty, i);
				}
				EditorGUI.indentLevel = 0;
			}
		}

		public void DrawRuleProperty(SerializedProperty ruleProperty, int ruleIndex)
		{
			SerializedProperty conditionsProperty = ruleProperty.FindPropertyRelative("conditions");

			ruleProperty.isExpanded = EditorGUILayout.Foldout(ruleProperty.isExpanded, "Rule " + ruleIndex);
			if(ruleProperty.isExpanded)
			{
				EditorGUI.indentLevel = 2;
				int length = conditionsProperty.arraySize;
				length = EditorGUILayout.IntField("Size", length);

				if(length != conditionsProperty.arraySize)
				{
					while(length > conditionsProperty.arraySize)
					{
						conditionsProperty.InsertArrayElementAtIndex(conditionsProperty.arraySize);
					}
					while(length < conditionsProperty.arraySize)
					{
						conditionsProperty.DeleteArrayElementAtIndex(conditionsProperty.arraySize - 1);
					}
					serializedObject.ApplyModifiedProperties();
				}

				for(int i = 0; i < length; i++)
				{
					SerializedProperty conditionProperty = conditionsProperty.GetArrayElementAtIndex(i);
					DrawCharacterConditionProperty(conditionProperty, i);
				}
				EditorGUI.indentLevel = 1;
				EditorGUILayout.Space();

				SerializedProperty actionProperty = ruleProperty.FindPropertyRelative("action");
				SerializedProperty actionIntValueProperty = ruleProperty.FindPropertyRelative("actionIntValue");
				EditorGUILayout.PropertyField(actionProperty);
				CharacterAction action = (CharacterAction)actionProperty.enumValueIndex;
				if(action == CharacterAction.REPLACE)
				{
					DrawCharacterIntValueProperty(actionIntValueProperty, "Character Value");
				}
			}
		}

		public void DrawCharacterConditionProperty(SerializedProperty conditionProperty, int conditionIndex)
		{
			SerializedProperty conditionOperatorProperty = conditionProperty.FindPropertyRelative("conditionOperator");
			SerializedProperty conditionIntValue1Property = conditionProperty.FindPropertyRelative("conditionIntValue1");
			SerializedProperty conditionIntValue2Property = conditionProperty.FindPropertyRelative("conditionIntValue2");
			SerializedProperty conditionStringValueProperty = conditionProperty.FindPropertyRelative("conditionStringValue");

			EditorGUILayout.PropertyField(conditionOperatorProperty);
			CharacterConditionOperator condition = (CharacterConditionOperator)conditionOperatorProperty.enumValueIndex;
			if(condition == CharacterConditionOperator.VALUE_EQUALS || condition == CharacterConditionOperator.VALUE_SMALLER_THAN || condition == CharacterConditionOperator.VALUE_SMALLER_THAN_OR_EQUALS
				|| condition == CharacterConditionOperator.VALUE_GREATER_THAN || condition == CharacterConditionOperator.VALUE_GREATER_THAN_OR_EQUALS)
			{
				DrawCharacterIntValueProperty(conditionIntValue1Property, "Character Value");
			}
			else if(condition == CharacterConditionOperator.VALUE_BETWEEN_INCLUSIVE || condition == CharacterConditionOperator.VALUE_BETWEEN_EXCLUSIVE)
			{
				DrawCharacterIntValueProperty(conditionIntValue1Property, "Min Character Value");
				DrawCharacterIntValueProperty(conditionIntValue2Property, "Max Character Value");
			}
			else if(condition == CharacterConditionOperator.VALUE_IN_STRING)
			{
				DrawCharacterOtherValueProperty(conditionStringValueProperty, "String value");
			}

			if(condition == CharacterConditionOperator.INDEX_EQUALS || condition == CharacterConditionOperator.INDEX_SMALLER_THAN || condition == CharacterConditionOperator.INDEX_SMALLER_THAN_OR_EQUALS
			|| condition == CharacterConditionOperator.INDEX_GREATER_THAN || condition == CharacterConditionOperator.INDEX_GREATER_THAN_OR_EQUALS)
			{
				DrawCharacterOtherValueProperty(conditionIntValue1Property, "Character Index");
			}
			else if(condition == CharacterConditionOperator.INDEX_BETWEEN_INCLUSIVE || condition == CharacterConditionOperator.INDEX_BETWEEN_EXCLUSIVE)
			{
				DrawCharacterOtherValueProperty(conditionIntValue1Property, "Min Character Index");
				DrawCharacterOtherValueProperty(conditionIntValue2Property, "Max Character Index");
			}

			if(condition == CharacterConditionOperator.OCCURENCES_SMALLER_THAN || condition == CharacterConditionOperator.OCCURENCES_SMALLER_THAN_OR_EQUALS
				|| condition == CharacterConditionOperator.OCCURENCES_GREATER_THAN || condition == CharacterConditionOperator.OCCURENCES_GREATER_THAN_OR_EQUALS)
			{
				DrawCharacterIntValueProperty(conditionIntValue1Property, "Character Value");
				DrawCharacterOtherValueProperty(conditionIntValue2Property, "Amount");
			}


		}

		public void DrawOtherCharactersProperty()
		{
			EditorGUILayout.LabelField("Rule for other characters:", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(otherCharacterActionProperty, new GUIContent("Action"));
			CharacterAction action = (CharacterAction)otherCharacterActionProperty.enumValueIndex;
			if(action == CharacterAction.REPLACE)
			{
				DrawCharacterIntValueProperty(otherCharacterActionIntValueProperty, "Character Value");
			}
		}

		public void DrawCharacterIntValueProperty(SerializedProperty property, string name)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(property, new GUIContent(name));
			EditorGUILayout.LabelField(string.Format("Character: {0}", (char)property.intValue));
			EditorGUILayout.EndHorizontal();
		}

		public void DrawCharacterOtherValueProperty(SerializedProperty property, string name)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(property, new GUIContent(name));
			EditorGUILayout.EndHorizontal();
		}
	}
}
