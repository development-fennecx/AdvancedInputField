using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
using TMPro;
#endif

namespace AdvancedInputFieldPlugin.Editor
{
	public static class AdvancedInputFieldExtensions
	{
		public static RectTransform ExtractData(this AdvancedInputField inputField, ref InputFieldData inputFieldData, ref TextRendererData textRendererData, ref TextRendererData placeholderRendererData)
		{
			RectTransform rectTransform = inputField.GetComponent<RectTransform>();
			inputFieldData = inputField.ExtractInputFieldData();

			bool deprecated = (inputField.GetComponentInChildren<ScrollArea>() == null);

			Transform textTransform = null;
			if(deprecated)
			{
				textTransform = rectTransform.Find("Text");
			}
			else
			{
				textTransform = rectTransform.Find("TextArea/Content/Text");
			}

			if(textTransform != null)
			{
				Text text = textTransform.GetComponent<Text>();
				if(text != null)
				{
					textRendererData = text.ExtractTextRendererData();
				}
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
				TextMeshProUGUI tmpText = textTransform.GetComponent<TextMeshProUGUI>();
				if(tmpText != null)
				{
					textRendererData = tmpText.ExtractTextRendererData();
				}
#endif
			}

			Transform placeholderTransform = null;
			if(deprecated)
			{
				placeholderTransform = rectTransform.Find("Text/Processed"); //Deprecated format used ProcessedRenderer for Placeholder
			}
			else
			{
				placeholderTransform = rectTransform.Find("TextArea/Content/Placeholder");
			}

			if(placeholderTransform != null)
			{
				Text placeholderText = placeholderTransform.GetComponent<Text>();
				if(placeholderText != null)
				{
					placeholderRendererData = placeholderText.ExtractTextRendererData();
				}
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
				TextMeshProUGUI tmpPlaceholderText = placeholderTransform.GetComponent<TextMeshProUGUI>();
				if(tmpPlaceholderText != null)
				{
					placeholderRendererData = tmpPlaceholderText.ExtractTextRendererData();
				}
#endif
			}

			return rectTransform;
		}

		public static void ApplyData(this AdvancedInputField inputField, RectTransform sourceTransform, InputFieldData inputFieldData, TextRendererData textRendererData, TextRendererData placeholderRendererData)
		{
			RectTransform rectTransform = inputField.GetComponent<RectTransform>();
			rectTransform.CopyValues(sourceTransform);
			inputField.name = sourceTransform.name;
			inputField.ApplyInputFieldData(inputFieldData);

			bool deprecated = (inputField.GetComponentInChildren<ScrollArea>() == null);

			Transform textTransform = null;
			if(deprecated)
			{
				textTransform = rectTransform.Find("Text");
			}
			else
			{
				textTransform = rectTransform.Find("TextArea/Content/Text");
			}

			if(textTransform != null)
			{
				Text text = textTransform.GetComponent<Text>();
				if(text != null)
				{
					text.ApplyTextRendererData(textRendererData);
				}
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
				TextMeshProUGUI tmpText = textTransform.GetComponent<TextMeshProUGUI>();
				if(tmpText != null)
				{
					tmpText.ApplyTextRendererData(textRendererData);
				}
#endif
			}

			Transform processedTextTransform = null;
			if(deprecated)
			{
				textTransform = rectTransform.Find("Text/Processed");
			}
			else
			{
				textTransform = rectTransform.Find("TextArea/Content/Processed");
			}

			if(processedTextTransform != null)
			{
				Text processedText = processedTextTransform.GetComponent<Text>();
				if(processedText != null)
				{
					processedText.ApplyTextRendererData(textRendererData); //Use settings from main text renderer
					processedText.text = string.Empty; //Default empty
				}
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
				TextMeshProUGUI tmpProcessedText = processedTextTransform.GetComponent<TextMeshProUGUI>();
				if(tmpProcessedText != null)
				{
					tmpProcessedText.ApplyTextRendererData(textRendererData); //Use settings from main text renderer
					tmpProcessedText.text = string.Empty; //Default empty
				}
#endif
			}

			Transform placeholderTransform = null;
			if(deprecated)
			{
				placeholderTransform = rectTransform.Find("Text/Processed"); //Deprecated format used ProcessedRenderer for Placeholder
			}
			else
			{
				placeholderTransform = rectTransform.Find("TextArea/Content/Placeholder");
			}

			if(placeholderTransform != null)
			{
				Text placeholderText = placeholderTransform.GetComponent<Text>();
				if(placeholderText != null && placeholderRendererData != null)
				{
					placeholderText.ApplyTextRendererData(placeholderRendererData);
					placeholderText.text = inputFieldData.placeholder;
				}
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
				TextMeshProUGUI tmpPlaceholderText = placeholderTransform.GetComponent<TextMeshProUGUI>();
				if(tmpPlaceholderText != null && placeholderRendererData != null)
				{
					tmpPlaceholderText.ApplyTextRendererData(placeholderRendererData);
					tmpPlaceholderText.text = inputFieldData.placeholder;
				}
#endif
			}
		}

		public static InputFieldData ExtractInputFieldData(this AdvancedInputField advancedInputField)
		{
			InputFieldData data = new InputFieldData();

			data.navigation = advancedInputField.navigation;
			data.transition = advancedInputField.transition;
			data.colors = advancedInputField.colors;
			data.spriteState = advancedInputField.spriteState;
			data.animationTriggers = advancedInputField.animationTriggers;
			data.interactable = advancedInputField.interactable;

			data.text = advancedInputField.Text;
			data.placeholder = advancedInputField.PlaceHolderText;
			data.characterLimit = advancedInputField.CharacterLimit;
			data.contentType = advancedInputField.ContentType;
			data.lineType = advancedInputField.LineType;
			data.inputType = advancedInputField.InputType;
			data.keyboardType = advancedInputField.KeyboardType;
			data.characterValidation = advancedInputField.CharacterValidation;
			data.emojisAllowed = advancedInputField.EmojisAllowed;
			data.liveProcessingFilter = advancedInputField.LiveDecorationFilter;
			data.postProcessingFilter = advancedInputField.PostDecorationFilter;
			data.caretOnBeginEdit = advancedInputField.CaretOnBeginEdit;
			data.caretBlinkRate = advancedInputField.CaretBlinkRate;
			data.caretWidth = advancedInputField.CaretWidth;
			data.caretColor = advancedInputField.CaretColor;
			data.selectionColor = advancedInputField.SelectionColor;
			data.readOnly = advancedInputField.ReadOnly;
			data.scrollBehaviourOnEndEdit = advancedInputField.ScrollBehaviourOnEndEdit;
			data.scrollBarsVisiblityMode = advancedInputField.ScrollBarsVisibilityMode;
			data.scrollSpeed = advancedInputField.ScrollSpeed;
			data.fastScrollSensitivity = advancedInputField.FastScrollSensitivity;
			data.nextInputField = advancedInputField.NextInputField;
			data.onSelectionChangedEvent = advancedInputField.OnSelectionChanged;
			data.onBeginEditEvent = advancedInputField.OnBeginEdit;
			data.onEndEditEvent = advancedInputField.OnEndEdit;
			data.onValueChangedEvent = advancedInputField.OnValueChanged;
			data.onCaretPositionChangedEvent = advancedInputField.OnCaretPositionChanged;
			data.onTextSelectionChangedEvent = advancedInputField.OnTextSelectionChanged;
			data.actionBarEnabled = advancedInputField.ActionBarEnabled;
			data.actionBarCut = advancedInputField.ActionBarCut;
			data.actionBarCopy = advancedInputField.ActionBarCopy;
			data.actionBarPaste = advancedInputField.ActionBarPaste;
			data.actionBarSelectAll = advancedInputField.ActionBarSelectAll;
			data.selectionCursorsEnabled = advancedInputField.TouchSelectionCursorsEnabled;
			data.autocapitalizationType = advancedInputField.AutocapitalizationType;
			data.autofillType = advancedInputField.AutofillType;

			return data;
		}

		public static void ApplyInputFieldData(this AdvancedInputField inputField, InputFieldData data)
		{
			Undo.RecordObject(inputField, "Undo " + inputField.GetInstanceID());

			inputField.navigation = data.navigation;
			inputField.transition = data.transition;
			inputField.colors = data.colors;
			inputField.spriteState = data.spriteState;
			inputField.animationTriggers = data.animationTriggers;
			inputField.interactable = data.interactable.GetValueOrDefault();
			inputField.targetGraphic = inputField.GetComponentInChildren<Graphic>(true);

			if(data.text != null)
			{
				inputField.Text = data.text;
			}
			inputField.PlaceHolderText = data.placeholder;
			inputField.CharacterLimit = data.characterLimit.GetValueOrDefault();
			inputField.ContentType = data.contentType.GetValueOrDefault();
			inputField.LineType = data.lineType.GetValueOrDefault();
			inputField.InputType = data.inputType.GetValueOrDefault();
			inputField.KeyboardType = data.keyboardType.GetValueOrDefault();
			inputField.CharacterValidation = data.characterValidation.GetValueOrDefault();
			inputField.EmojisAllowed = data.emojisAllowed.GetValueOrDefault();
			inputField.LiveDecorationFilter = data.liveProcessingFilter;
			inputField.PostDecorationFilter = data.postProcessingFilter;
			inputField.CaretOnBeginEdit = data.caretOnBeginEdit.GetValueOrDefault();
			inputField.CaretBlinkRate = data.caretBlinkRate.GetValueOrDefault();
			inputField.CaretWidth = data.caretWidth.GetValueOrDefault();
			inputField.CaretColor = data.caretColor.GetValueOrDefault();
			inputField.SelectionColor = data.selectionColor.GetValueOrDefault();
			inputField.ReadOnly = data.readOnly.GetValueOrDefault();
			inputField.ScrollBehaviourOnEndEdit = data.scrollBehaviourOnEndEdit.GetValueOrDefault();
			inputField.ScrollBarsVisibilityMode = data.scrollBarsVisiblityMode.GetValueOrDefault();
			inputField.ScrollSpeed = data.scrollSpeed.GetValueOrDefault();
			inputField.FastScrollSensitivity = data.fastScrollSensitivity.GetValueOrDefault();
			inputField.NextInputField = data.nextInputField;
			inputField.OnSelectionChanged = data.onSelectionChangedEvent;
			inputField.OnBeginEdit = data.onBeginEditEvent;
			inputField.OnEndEdit = data.onEndEditEvent;
			inputField.OnValueChanged = data.onValueChangedEvent;
			inputField.OnCaretPositionChanged = data.onCaretPositionChangedEvent;
			inputField.OnTextSelectionChanged = data.onTextSelectionChangedEvent;
			inputField.ActionBarEnabled = data.actionBarEnabled.GetValueOrDefault();
			inputField.ActionBarCut = data.actionBarCut.GetValueOrDefault();
			inputField.ActionBarCopy = data.actionBarCopy.GetValueOrDefault();
			inputField.ActionBarPaste = data.actionBarPaste.GetValueOrDefault();
			inputField.ActionBarSelectAll = data.actionBarSelectAll.GetValueOrDefault();
			inputField.TouchSelectionCursorsEnabled = data.selectionCursorsEnabled.GetValueOrDefault();
			inputField.AutocapitalizationType = data.autocapitalizationType.GetValueOrDefault();
			inputField.AutofillType = data.autofillType.GetValueOrDefault();
		}
	}
}
