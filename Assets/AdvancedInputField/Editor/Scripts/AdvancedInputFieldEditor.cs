// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.UI;
using UnityEngine;

namespace AdvancedInputFieldPlugin.Editor
{
	[CustomEditor(typeof(AdvancedInputField), true)]
	public class AdvancedInputFieldEditor: SelectableEditor
	{
		private const string DEPRECATED_FORMAT_RESOURCE_PATH = "AdvancedInputField/Images/deprecated_format";
		private const string UNITY_FORMAT_IMAGE_PATH = "AdvancedInputField/Images/unity_format";
		private const string TEXTMESHPRO_FORMAT_IMAGE_PATH = "AdvancedInputField/Images/textmeshpro_format";

		private static bool inheritedFoldout;
		private SerializedProperty interactableProperty; //Property from Selectable base class
		private SerializedProperty targetGraphicProperty;
		private SerializedProperty transitionProperty;
		private SerializedProperty colorBlockProperty;
		private SerializedProperty spriteStateProperty;
		private SerializedProperty animTriggerProperty;
		private AnimBool showColorTint = new AnimBool();
		private AnimBool showSpriteTrasition = new AnimBool();
		private AnimBool showAnimTransition = new AnimBool();

		private static bool modeFoldout;
		private SerializedProperty readOnlyProperty;
		private SerializedProperty modeProperty;
		private SerializedProperty resizeMinWidthProperty;
		private SerializedProperty resizeMaxWidthProperty;
		private SerializedProperty resizeMinHeightProperty;
		private SerializedProperty resizeMaxHeightProperty;
		private SerializedProperty caretOnBeginEditProperty;
		private SerializedProperty selectionModeProperty;
		private SerializedProperty dragModeProperty;
		private SerializedProperty scrollBehaviourOnEndEditProperty;
		private SerializedProperty scrollBarsVisibilityModeProperty;
		private SerializedProperty scrollSpeedProperty;
		private SerializedProperty maxScrollTransitionTimeProperty;
		private SerializedProperty fastScrollSensitivityProperty;

		private static bool appearanceFoldout;
		private SerializedProperty caretBlinkRateProperty;
		private SerializedProperty caretWidthProperty;
		private SerializedProperty caretColorProperty;
		private SerializedProperty selectionColorProperty;
		private SerializedProperty selectionBehindTextProperty;

		private static bool generalFoldout;
		private SerializedProperty textProperty;
		private SerializedProperty placeholderTextProperty;
		private SerializedProperty richTextEditingProperty;
		private SerializedProperty richTextConfigProperty;
		private SerializedProperty characterLimitProperty;
		private SerializedProperty lineLimitProperty;
		private SerializedProperty contentTypeProperty;
		private SerializedProperty lineTypeProperty;
		private SerializedProperty inputTypeProperty;
		private SerializedProperty visiblePasswordProperty;
		private SerializedProperty keyboardTypeProperty;
		private SerializedProperty characterValidationProperty;
		private SerializedProperty characterValidatorProperty;
		private SerializedProperty emojisAllowedProperty;
		private SerializedProperty richTextBindingsAllowedProperty;

		private static bool processingFoldout;
		private SerializedProperty liveProcessingFilterProperty;
		private SerializedProperty liveDecorationFilterProperty;
		private SerializedProperty postDecorationFilterProperty;

		private static bool eventsFoldout;
		private SerializedProperty onSelectionChangedProperty;
		private SerializedProperty onBeginEditProperty;
		private SerializedProperty onEndEditProperty;
		private SerializedProperty onValueChangedProperty;
		private SerializedProperty onCaretPositionChangedProperty;
		private SerializedProperty onTextSelectionChangedProperty;
		private SerializedProperty onSizeChangedProperty;
		private SerializedProperty onSpecialKeyPressedProperty;
		private SerializedProperty onTextTapProperty;
		private SerializedProperty onActionBarActionProperty;

		private static bool otherFoldout;
		private SerializedProperty actionBarEnabledProperty;
		private SerializedProperty actionBarCutProperty;
		private SerializedProperty actionBarCopyProperty;
		private SerializedProperty actionBarPasteProperty;
		private SerializedProperty actionBarSelectAllProperty;

		private static bool mobileFoldout;
		private SerializedProperty touchSelectionCursorsEnabledProperty;
		private SerializedProperty cursorClampModeProperty;
		private SerializedProperty autocapitalizationTypeProperty;
		private SerializedProperty autofillTypeProperty;
		private SerializedProperty returnKeyTypeProperty;

		private SerializedProperty nextInputFieldProperty;

		private GUIStyle foldoutStyle;

		protected override void OnEnable()
		{
			base.OnEnable();

			interactableProperty = serializedObject.FindProperty("m_Interactable");
			targetGraphicProperty = serializedObject.FindProperty("m_TargetGraphic");
			transitionProperty = serializedObject.FindProperty("m_Transition");
			colorBlockProperty = serializedObject.FindProperty("m_Colors");
			spriteStateProperty = serializedObject.FindProperty("m_SpriteState");
			animTriggerProperty = serializedObject.FindProperty("m_AnimationTriggers");
			showColorTint.valueChanged.AddListener(Repaint);
			showSpriteTrasition.valueChanged.AddListener(Repaint);

			modeProperty = serializedObject.FindProperty("mode");
			textProperty = serializedObject.FindProperty("text");
			placeholderTextProperty = serializedObject.FindProperty("placeholderText");
			richTextEditingProperty = serializedObject.FindProperty("richTextEditing");
			richTextConfigProperty = serializedObject.FindProperty("richTextConfig");
			characterLimitProperty = serializedObject.FindProperty("characterLimit");
			lineLimitProperty = serializedObject.FindProperty("lineLimit");
			contentTypeProperty = serializedObject.FindProperty("contentType");
			lineTypeProperty = serializedObject.FindProperty("lineType");
			inputTypeProperty = serializedObject.FindProperty("inputType");
			visiblePasswordProperty = serializedObject.FindProperty("visiblePassword");
			keyboardTypeProperty = serializedObject.FindProperty("keyboardType");
			characterValidationProperty = serializedObject.FindProperty("characterValidation");
			characterValidatorProperty = serializedObject.FindProperty("characterValidator");
			emojisAllowedProperty = serializedObject.FindProperty("emojisAllowed");
			richTextBindingsAllowedProperty = serializedObject.FindProperty("richTextBindingsAllowed");
			liveProcessingFilterProperty = serializedObject.FindProperty("liveProcessingFilter");
			liveDecorationFilterProperty = serializedObject.FindProperty("liveDecorationFilter");
			postDecorationFilterProperty = serializedObject.FindProperty("postDecorationFilter");
			selectionModeProperty = serializedObject.FindProperty("selectionMode");
			dragModeProperty = serializedObject.FindProperty("dragMode");
			caretOnBeginEditProperty = serializedObject.FindProperty("caretOnBeginEdit");
			caretBlinkRateProperty = serializedObject.FindProperty("caretBlinkRate");
			caretWidthProperty = serializedObject.FindProperty("caretWidth");
			caretColorProperty = serializedObject.FindProperty("caretColor");
			selectionColorProperty = serializedObject.FindProperty("selectionColor");
			selectionBehindTextProperty = serializedObject.FindProperty("selectionBehindText");
			readOnlyProperty = serializedObject.FindProperty("readOnly");
			scrollBehaviourOnEndEditProperty = serializedObject.FindProperty("scrollBehaviourOnEndEdit");
			scrollBarsVisibilityModeProperty = serializedObject.FindProperty("scrollBarsVisibilityMode");
			scrollSpeedProperty = serializedObject.FindProperty("scrollSpeed");
			maxScrollTransitionTimeProperty = serializedObject.FindProperty("maxScrollTransitionTime");
			fastScrollSensitivityProperty = serializedObject.FindProperty("fastScrollSensitivity");
			resizeMinWidthProperty = serializedObject.FindProperty("resizeMinWidth");
			resizeMaxWidthProperty = serializedObject.FindProperty("resizeMaxWidth");
			resizeMinHeightProperty = serializedObject.FindProperty("resizeMinHeight");
			resizeMaxHeightProperty = serializedObject.FindProperty("resizeMaxHeight");
			onSelectionChangedProperty = serializedObject.FindProperty("onSelectionChanged");
			onBeginEditProperty = serializedObject.FindProperty("onBeginEdit");
			onEndEditProperty = serializedObject.FindProperty("onEndEdit");
			onValueChangedProperty = serializedObject.FindProperty("onValueChanged");
			onCaretPositionChangedProperty = serializedObject.FindProperty("onCaretPositionChanged");
			onTextSelectionChangedProperty = serializedObject.FindProperty("onTextSelectionChanged");
			onSizeChangedProperty = serializedObject.FindProperty("onSizeChanged");
			onSpecialKeyPressedProperty = serializedObject.FindProperty("onSpecialKeyPressed");
			onTextTapProperty = serializedObject.FindProperty("onTextTap");
			onActionBarActionProperty = serializedObject.FindProperty("onActionBarAction");
			actionBarEnabledProperty = serializedObject.FindProperty("actionBarEnabled");
			actionBarCutProperty = serializedObject.FindProperty("actionBarCut");
			actionBarCopyProperty = serializedObject.FindProperty("actionBarCopy");
			actionBarPasteProperty = serializedObject.FindProperty("actionBarPaste");
			actionBarSelectAllProperty = serializedObject.FindProperty("actionBarSelectAll");
			touchSelectionCursorsEnabledProperty = serializedObject.FindProperty("touchSelectionCursorsEnabled");
			cursorClampModeProperty = serializedObject.FindProperty("cursorClampMode");
			autocapitalizationTypeProperty = serializedObject.FindProperty("autocapitalizationType");
			autofillTypeProperty = serializedObject.FindProperty("autofillType");
			returnKeyTypeProperty = serializedObject.FindProperty("returnKeyType");
			nextInputFieldProperty = serializedObject.FindProperty("nextInputField");
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			showColorTint.valueChanged.RemoveListener(Repaint);
			showSpriteTrasition.valueChanged.RemoveListener(Repaint);
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			AdvancedInputField inputField = (AdvancedInputField)target;
			foldoutStyle = new GUIStyle(EditorStyles.foldout);
			foldoutStyle.fontStyle = FontStyle.Bold;

			DrawHeader(inputField);
			DrawInheritedProperties(inputField);
			DrawModeProperties(inputField);
			DrawAppearanceProperties(inputField);
			DrawGeneralProperties(inputField);
			DrawProcessingProperties(inputField);
			DrawEventProperties(inputField);
			DrawOtherProperties(inputField);
			DrawMobileProperties(inputField);

			serializedObject.ApplyModifiedProperties();
		}

		private void DrawHeader(AdvancedInputField inputField)
		{
			if(inputField.GetComponentInChildren<ScrollArea>() != null) //New format
			{
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
				if(inputField.GetComponentInChildren<TMPro.TextMeshProUGUI>() != null)
				{
					Texture2D texture = Resources.Load<Texture2D>(TEXTMESHPRO_FORMAT_IMAGE_PATH);
					GUILayout.Box(texture);
				}
				else
#endif
				{
					Texture2D texture = Resources.Load<Texture2D>(UNITY_FORMAT_IMAGE_PATH);
					GUILayout.Box(texture);
				}
			}
			else //Old format
			{
				Texture2D texture = Resources.Load<Texture2D>(DEPRECATED_FORMAT_RESOURCE_PATH);
				GUILayout.Box(texture);
				GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
				labelStyle.normal.textColor = new Color(0.5f, 0, 0);
				labelStyle.fontSize = 12;
				string message = "This format is deprecated, please use the ConversionTool to convert the InputField(s) to the newer format." +
					"\n(TopBar: Advanced Input Field => ConversionTool)" +
					 "\nSet \'from\' to \'DEPRECATED_ADVANCEDINPUTFIELD\' and \'to\' to either \'ADVANCEDINPUTFIELD_UNITY_TEXT\' or \'ADVANCEDINPUTFIELD_TEXTMESHPRO_TEXT\'";
				GUILayout.Label(message, labelStyle);
				EditorGUILayout.Space();
			}
		}

		#region INHERITED
		private void DrawInheritedProperties(AdvancedInputField inputField)
		{
			inheritedFoldout = EditorGUILayout.Foldout(inheritedFoldout, "Inherited", foldoutStyle);
			if(inheritedFoldout)
			{
				base.OnInspectorGUI();
			}
		}
		#endregion

		#region MODE
		private void DrawModeProperties(AdvancedInputField inputField)
		{
			modeFoldout = EditorGUILayout.Foldout(modeFoldout, new GUIContent("Mode"), foldoutStyle);
			if(modeFoldout)
			{
				EditorGUILayout.PropertyField(readOnlyProperty);

				InputFieldMode mode = (InputFieldMode)modeProperty.enumValueIndex;

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(modeProperty);
				if(EditorGUI.EndChangeCheck())
				{
					mode = (InputFieldMode)modeProperty.enumValueIndex;
					inputField.Mode = mode;
				}

				if(mode == InputFieldMode.SCROLL_TEXT)
				{
					DrawTextScrollProperties(inputField);
				}
				else if(mode == InputFieldMode.HORIZONTAL_RESIZE_FIT_TEXT)
				{
					EditorGUILayout.PropertyField(resizeMinWidthProperty);
					EditorGUILayout.PropertyField(resizeMaxWidthProperty);
				}
				else if(mode == InputFieldMode.VERTICAL_RESIZE_FIT_TEXT)
				{
					EditorGUILayout.PropertyField(resizeMinHeightProperty);
					EditorGUILayout.PropertyField(resizeMaxHeightProperty);
				}
				EditorGUILayout.PropertyField(caretOnBeginEditProperty);
				EditorGUILayout.PropertyField(selectionModeProperty);
				EditorGUILayout.PropertyField(dragModeProperty);
			}
		}

		private void DrawTextScrollProperties(AdvancedInputField inputField)
		{
			EditorGUILayout.PropertyField(scrollBehaviourOnEndEditProperty);

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(scrollBarsVisibilityModeProperty);
			if(EditorGUI.EndChangeCheck())
			{
				ScrollArea scrollArea = inputField.TextAreaTransform.GetComponent<ScrollArea>();
				Undo.RecordObject(scrollArea, "Undo " + scrollArea.GetInstanceID());
				inputField.ScrollBarsVisibilityMode = (ScrollBarVisibilityMode)scrollBarsVisibilityModeProperty.enumValueIndex;
			}

			EditorGUILayout.PropertyField(scrollSpeedProperty);
			EditorGUILayout.PropertyField(maxScrollTransitionTimeProperty);
			EditorGUILayout.PropertyField(fastScrollSensitivityProperty);
		}
		#endregion

		#region APPEARANCE
		private void DrawAppearanceProperties(AdvancedInputField inputField)
		{
			appearanceFoldout = EditorGUILayout.Foldout(appearanceFoldout, "Appearance", foldoutStyle);
			if(appearanceFoldout)
			{
				EditorGUILayout.PropertyField(caretBlinkRateProperty);
				EditorGUILayout.PropertyField(caretWidthProperty);
				EditorGUILayout.PropertyField(caretColorProperty);
				EditorGUILayout.PropertyField(selectionColorProperty);

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(selectionBehindTextProperty);
				if(EditorGUI.EndChangeCheck())
				{
					inputField.SelectionBehindText = selectionBehindTextProperty.boolValue;
				}
			}
		}
		#endregion

		#region GENERAL
		private void DrawGeneralProperties(AdvancedInputField inputField)
		{
			LineType lineType = ((LineType)lineTypeProperty.enumValueIndex);

			generalFoldout = EditorGUILayout.Foldout(generalFoldout, "General", foldoutStyle);
			if(generalFoldout)
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(textProperty);
				if(EditorGUI.EndChangeCheck())
				{
					if(string.IsNullOrEmpty(textProperty.stringValue))
					{
						MarkTextRendererDirty(inputField.PlaceholderTextRenderer);
					}
					else
					{
						MarkTextRendererDirty(inputField.TextRenderer);
					}

					inputField.Text = textProperty.stringValue;
				}

				if(textProperty.stringValue.Contains("\n") && lineType != LineType.MULTILINE_NEWLINE)
				{
					GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
					labelStyle.normal.textColor = Color.yellow;
					EditorGUILayout.LabelField("WARNING: Newline character detected in text,", labelStyle);
					EditorGUILayout.LabelField("but lineType is not set MULTILINE_NEWLINE", labelStyle);
				}

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(placeholderTextProperty);
				if(EditorGUI.EndChangeCheck())
				{
					MarkTextRendererDirty(inputField.PlaceholderTextRenderer);
					inputField.PlaceHolderText = placeholderTextProperty.stringValue;
				}

				EditorGUILayout.PropertyField(richTextEditingProperty);
				if(richTextEditingProperty.boolValue)
				{
					EditorGUILayout.PropertyField(richTextConfigProperty);
				}

				EditorGUILayout.PropertyField(emojisAllowedProperty, new GUIContent("Emojis Allowed"));
				EditorGUILayout.PropertyField(richTextBindingsAllowedProperty, new GUIContent("Rich Text Bindings Allowed"));
				DrawCharacterLimitProperty(inputField);
				DrawLineLimitProperty(inputField);
				DrawContentTypeProperties(inputField);
				EditorGUILayout.PropertyField(nextInputFieldProperty);
			}
		}

		private void MarkTextRendererDirty(TextRenderer textRenderer)
		{
			if(textRenderer is UnityTextRenderer)
			{
				Undo.RecordObject(((UnityTextRenderer)textRenderer).Renderer, "Undo " + textRenderer.GetInstanceID());
			}
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
			else if(textRenderer is TMProTextRenderer)
			{
				Undo.RecordObject(((TMProTextRenderer)textRenderer).Renderer, "Undo " + textRenderer.GetInstanceID());
			}
#endif
		}

		private void DrawCharacterLimitProperty(AdvancedInputField inputField)
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(characterLimitProperty);
			if(EditorGUI.EndChangeCheck())
			{
				inputField.ApplyCharacterLimit(characterLimitProperty.intValue);
				textProperty.stringValue = inputField.Text;
			}
		}

		private void DrawLineLimitProperty(AdvancedInputField inputField)
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(lineLimitProperty);
			if(EditorGUI.EndChangeCheck())
			{
				inputField.ApplyLineLimit(lineLimitProperty.intValue);
				textProperty.stringValue = inputField.Text;
			}
		}

		private void DrawContentTypeProperties(AdvancedInputField inputField)
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(contentTypeProperty);
			if(EditorGUI.EndChangeCheck())
			{
				ContentType contentType = ((ContentType)contentTypeProperty.enumValueIndex);
				inputField.UpdateContentType(contentType);
				lineTypeProperty.enumValueIndex = (int)inputField.LineType;
				inputTypeProperty.enumValueIndex = (int)inputField.InputType;
				keyboardTypeProperty.enumValueIndex = (int)inputField.KeyboardType;
				characterValidationProperty.enumValueIndex = (int)inputField.CharacterValidation;
			}

			EditorGUI.indentLevel = 1;
			EditorGUILayout.PropertyField(lineTypeProperty);
			if(((ContentType)contentTypeProperty.enumValueIndex) == ContentType.CUSTOM)
			{
				EditorGUILayout.PropertyField(inputTypeProperty);
				if(((InputType)inputTypeProperty.enumValueIndex) == InputType.PASSWORD)
				{
					EditorGUILayout.PropertyField(visiblePasswordProperty);
				}

				EditorGUILayout.PropertyField(keyboardTypeProperty);
				EditorGUILayout.PropertyField(characterValidationProperty);
				if(((CharacterValidation)characterValidationProperty.enumValueIndex) == CharacterValidation.CUSTOM)
				{
					EditorGUILayout.PropertyField(characterValidatorProperty);
				}
			}
			EditorGUI.indentLevel = 0;
		}
		#endregion

		#region PROCESSING
		private void DrawProcessingProperties(AdvancedInputField inputField)
		{
			processingFoldout = EditorGUILayout.Foldout(processingFoldout, "Processing", foldoutStyle);
			if(processingFoldout)
			{
				EditorGUILayout.PropertyField(liveProcessingFilterProperty);
				EditorGUILayout.PropertyField(liveDecorationFilterProperty);
				EditorGUILayout.PropertyField(postDecorationFilterProperty);
			}
		}
		#endregion

		#region EVENTS
		private void DrawEventProperties(AdvancedInputField inputField)
		{
			eventsFoldout = EditorGUILayout.Foldout(eventsFoldout, "Events", foldoutStyle);
			if(eventsFoldout)
			{
				EditorGUILayout.PropertyField(onSelectionChangedProperty);
				EditorGUILayout.PropertyField(onBeginEditProperty);
				EditorGUILayout.PropertyField(onEndEditProperty);
				EditorGUILayout.PropertyField(onValueChangedProperty);
				EditorGUILayout.PropertyField(onCaretPositionChangedProperty);
				EditorGUILayout.PropertyField(onTextSelectionChangedProperty);
				EditorGUILayout.PropertyField(onSizeChangedProperty);
				EditorGUILayout.PropertyField(onSpecialKeyPressedProperty);
				EditorGUILayout.PropertyField(onTextTapProperty);
				EditorGUILayout.PropertyField(onActionBarActionProperty);
			}
		}
		#endregion

		#region OTHER
		private void DrawOtherProperties(AdvancedInputField inputField)
		{
			otherFoldout = EditorGUILayout.Foldout(otherFoldout, "Other", foldoutStyle);
			if(otherFoldout)
			{
				EditorGUILayout.LabelField("NOTE: These settings could be blocked for current platform in the Global Settings");
				EditorGUILayout.LabelField("(Topbar: Advanced Input Field => Global Settings)");
				EditorGUILayout.PropertyField(actionBarEnabledProperty);
				if(actionBarEnabledProperty.boolValue)
				{
					EditorGUILayout.PropertyField(actionBarCutProperty);
					EditorGUILayout.PropertyField(actionBarCopyProperty);
					EditorGUILayout.PropertyField(actionBarPasteProperty);
					EditorGUILayout.PropertyField(actionBarSelectAllProperty);
				}

				EditorGUILayout.PropertyField(touchSelectionCursorsEnabledProperty);
				if(actionBarEnabledProperty.boolValue || touchSelectionCursorsEnabledProperty.boolValue)
				{
					EditorGUILayout.PropertyField(cursorClampModeProperty);
				}
			}
		}
		#endregion

		#region MOBILE
		private void DrawMobileProperties(AdvancedInputField inputField)
		{
			mobileFoldout = EditorGUILayout.Foldout(mobileFoldout, "Mobile", foldoutStyle);
			if(mobileFoldout)
			{
				EditorGUILayout.PropertyField(autocapitalizationTypeProperty);
				EditorGUILayout.PropertyField(autofillTypeProperty);
				EditorGUILayout.PropertyField(returnKeyTypeProperty);
			}
		}
		#endregion
	}
}