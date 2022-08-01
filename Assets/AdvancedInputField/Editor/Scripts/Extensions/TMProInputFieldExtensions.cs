#if ADVANCEDINPUTFIELD_TEXTMESHPRO
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedInputFieldPlugin.Editor
{
	public static class TMProInputFieldExtensions
	{
		public static RectTransform ExtractData(this TMP_InputField inputField, ref InputFieldData inputFieldData, ref TextRendererData textRendererData, ref TextRendererData placeholderRendererData)
		{
			RectTransform rectTransform = inputField.GetComponent<RectTransform>();
			inputFieldData = inputField.ExtractInputFieldData();

			TMP_Text fromText = inputField.textComponent;
			if(fromText != null && fromText.GetComponent<TextMeshProUGUI>() != null)
			{
				textRendererData = fromText.GetComponent<TextMeshProUGUI>().ExtractTextRendererData();
			}

			Graphic fromPlaceholder = inputField.placeholder;
			if(fromPlaceholder != null && fromPlaceholder.GetComponent<TextMeshProUGUI>() != null)
			{
				placeholderRendererData = fromPlaceholder.GetComponent<TextMeshProUGUI>().ExtractTextRendererData();
			}

			return rectTransform;
		}

		public static void ApplyData(this TMP_InputField inputField, RectTransform sourceTransform, InputFieldData inputFieldData, TextRendererData textRendererData, TextRendererData placeholderRendererData)
		{
			RectTransform rectTransform = inputField.GetComponent<RectTransform>();
			rectTransform.CopyValues(sourceTransform);
			inputField.name = sourceTransform.name;
			inputField.ApplyInputFieldData(inputFieldData);

			TMP_Text tmpText = inputField.textComponent;
			if(tmpText != null)
			{
				TextMeshProUGUI text = tmpText.GetComponent<TextMeshProUGUI>();
				if(text != null && textRendererData != null)
				{
					text.ApplyTextRendererData(textRendererData);
				}
			}

			Graphic placeholder = inputField.placeholder;
			if(placeholder != null)
			{
				TextMeshProUGUI placeholderText = placeholder.GetComponent<TextMeshProUGUI>();
				if(placeholderText != null && placeholderRendererData != null)
				{
					placeholderText.ApplyTextRendererData(placeholderRendererData);
				}
			}
		}

		public static InputFieldData ExtractInputFieldData(this TMP_InputField inputField)
		{
			InputFieldData data = new InputFieldData();

			data.navigation = inputField.navigation;
			data.transition = inputField.transition;
			data.colors = inputField.colors;
			data.spriteState = inputField.spriteState;
			data.animationTriggers = inputField.animationTriggers;
			data.interactable = inputField.interactable;

			data.text = inputField.text;
			data.placeholder = ExtractPlaceholder(inputField);
			data.characterLimit = inputField.characterLimit;
			data.contentType = ExtractContentType(inputField);
			data.lineType = ExtractLineType(inputField);
			data.inputType = ExtractInputType(inputField);
			data.keyboardType = ExtractKeyboardType(inputField);
			data.characterValidation = ExtractCharacterValidation(inputField);
			data.caretBlinkRate = inputField.caretBlinkRate;
			data.caretWidth = inputField.caretWidth;
			data.caretColor = inputField.caretColor;
			data.selectionColor = inputField.selectionColor;
			data.readOnly = inputField.readOnly;

			return data;
		}

		public static void ApplyInputFieldData(this TMP_InputField inputField, InputFieldData data)
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
				inputField.text = data.text;
			}
			inputField.ApplyPlaceHolder(data);
			inputField.characterLimit = data.characterLimit.GetValueOrDefault();
			inputField.ApplyContentType(data);
			inputField.ApplyLineType(data);
			inputField.ApplyInputType(data);
			inputField.ApplyKeyboardType(data);
			inputField.ApplyCharacterValidation(data);
			inputField.caretBlinkRate = data.caretBlinkRate.GetValueOrDefault();
			inputField.caretWidth = Mathf.RoundToInt(data.caretWidth.GetValueOrDefault());
			inputField.caretColor = data.caretColor.GetValueOrDefault();
			inputField.selectionColor = data.selectionColor.GetValueOrDefault();
			inputField.readOnly = data.readOnly.GetValueOrDefault();
		}

		public static string ExtractPlaceholder(this TMP_InputField inputField)
		{
			TextMeshProUGUI text = inputField.placeholder.GetComponent<TextMeshProUGUI>();
			if(text != null)
			{
				return text.text;
			}

			return null;
		}

		public static void ApplyPlaceHolder(this TMP_InputField inputField, InputFieldData data)
		{
			TextMeshProUGUI text = inputField.placeholder.GetComponent<TextMeshProUGUI>();
			if(text != null)
			{
				text.text = data.placeholder;
			}
		}

		public static ContentType ExtractContentType(this TMP_InputField inputField)
		{
			switch(inputField.contentType)
			{
				case TMP_InputField.ContentType.Alphanumeric: return ContentType.ALPHANUMERIC;
				case TMP_InputField.ContentType.Autocorrected: return ContentType.AUTOCORRECTED;
				case TMP_InputField.ContentType.Custom: return ContentType.CUSTOM;
				case TMP_InputField.ContentType.DecimalNumber: return ContentType.DECIMAL_NUMBER;
				case TMP_InputField.ContentType.EmailAddress: return ContentType.EMAIL_ADDRESS;
				case TMP_InputField.ContentType.IntegerNumber: return ContentType.INTEGER_NUMBER;
				case TMP_InputField.ContentType.Name: return ContentType.NAME;
				case TMP_InputField.ContentType.Password: return ContentType.PASSWORD;
				case TMP_InputField.ContentType.Pin: return ContentType.PIN;
				case TMP_InputField.ContentType.Standard: return ContentType.STANDARD;
				default: return ContentType.STANDARD;
			}
		}

		public static void ApplyContentType(this TMP_InputField inputField, InputFieldData data)
		{
			ContentType contentType = data.contentType.GetValueOrDefault();

			switch(contentType)
			{
				case ContentType.ALPHANUMERIC: inputField.contentType = TMP_InputField.ContentType.Alphanumeric; break;
				case ContentType.AUTOCORRECTED: inputField.contentType = TMP_InputField.ContentType.Autocorrected; break;
				case ContentType.CUSTOM: inputField.contentType = TMP_InputField.ContentType.Custom; break;
				case ContentType.DECIMAL_NUMBER: inputField.contentType = TMP_InputField.ContentType.DecimalNumber; break;
				case ContentType.EMAIL_ADDRESS: inputField.contentType = TMP_InputField.ContentType.EmailAddress; break;
				case ContentType.INTEGER_NUMBER: inputField.contentType = TMP_InputField.ContentType.IntegerNumber; break;
				case ContentType.NAME: inputField.contentType = TMP_InputField.ContentType.Name; break;
				case ContentType.PASSWORD: inputField.contentType = TMP_InputField.ContentType.Password; break;
				case ContentType.PIN: inputField.contentType = TMP_InputField.ContentType.Pin; break;
				case ContentType.STANDARD: inputField.contentType = TMP_InputField.ContentType.Standard; break;
			}
		}

		public static LineType ExtractLineType(this TMP_InputField inputField)
		{
			switch(inputField.lineType)
			{
				case TMP_InputField.LineType.MultiLineNewline: return LineType.MULTILINE_NEWLINE;
				case TMP_InputField.LineType.MultiLineSubmit: return LineType.MULTILINE_SUBMIT;
				case TMP_InputField.LineType.SingleLine: return LineType.SINGLE_LINE;
				default: return LineType.SINGLE_LINE;
			}
		}

		public static void ApplyLineType(this TMP_InputField inputField, InputFieldData data)
		{
			LineType lineType = data.lineType.GetValueOrDefault();

			switch(lineType)
			{
				case LineType.MULTILINE_NEWLINE: inputField.lineType = TMP_InputField.LineType.MultiLineNewline; break;
				case LineType.MULTILINE_SUBMIT: inputField.lineType = TMP_InputField.LineType.MultiLineSubmit; break;
				case LineType.SINGLE_LINE: inputField.lineType = TMP_InputField.LineType.SingleLine; break;
			}
		}

		public static InputType ExtractInputType(this TMP_InputField inputField)
		{
			switch(inputField.inputType)
			{
				case TMP_InputField.InputType.AutoCorrect: return InputType.AUTOCORRECT;
				case TMP_InputField.InputType.Password: return InputType.PASSWORD;
				case TMP_InputField.InputType.Standard: return InputType.STANDARD;
				default: return InputType.STANDARD;
			}
		}

		public static void ApplyInputType(this TMP_InputField inputField, InputFieldData data)
		{
			InputType inputType = data.inputType.GetValueOrDefault();

			switch(inputType)
			{
				case InputType.AUTOCORRECT: inputField.inputType = TMP_InputField.InputType.AutoCorrect; break;
				case InputType.PASSWORD: inputField.inputType = TMP_InputField.InputType.Password; break;
				case InputType.STANDARD: inputField.inputType = TMP_InputField.InputType.Standard; break;
			}
		}

		public static KeyboardType ExtractKeyboardType(this TMP_InputField inputField)
		{
			switch(inputField.keyboardType)
			{
				case TouchScreenKeyboardType.ASCIICapable: return KeyboardType.ASCII_CAPABLE;
				case TouchScreenKeyboardType.Default: return KeyboardType.DEFAULT;
				case TouchScreenKeyboardType.EmailAddress: return KeyboardType.EMAIL_ADDRESS;
				case TouchScreenKeyboardType.NumberPad: return KeyboardType.NUMBER_PAD;
				case TouchScreenKeyboardType.NumbersAndPunctuation: return KeyboardType.NUMBERS_AND_PUNCTUATION;
				case TouchScreenKeyboardType.PhonePad: return KeyboardType.PHONE_PAD;
				case TouchScreenKeyboardType.URL: return KeyboardType.URL;
				default: return KeyboardType.DEFAULT;
			}
		}

		public static void ApplyKeyboardType(this TMP_InputField inputField, InputFieldData data)
		{
			KeyboardType keyboardType = data.keyboardType.GetValueOrDefault();

			switch(keyboardType)
			{
				case KeyboardType.ASCII_CAPABLE: inputField.keyboardType = TouchScreenKeyboardType.ASCIICapable; break;
				case KeyboardType.DEFAULT: inputField.keyboardType = TouchScreenKeyboardType.Default; break;
				case KeyboardType.EMAIL_ADDRESS: inputField.keyboardType = TouchScreenKeyboardType.EmailAddress; break;
				case KeyboardType.NUMBER_PAD: inputField.keyboardType = TouchScreenKeyboardType.NumberPad; break;
				case KeyboardType.NUMBERS_AND_PUNCTUATION: inputField.keyboardType = TouchScreenKeyboardType.NumbersAndPunctuation; break;
				case KeyboardType.PHONE_PAD: inputField.keyboardType = TouchScreenKeyboardType.PhonePad; break;
				case KeyboardType.URL: inputField.keyboardType = TouchScreenKeyboardType.URL; break;
			}
		}

		public static CharacterValidation ExtractCharacterValidation(this TMP_InputField inputField)
		{
			switch(inputField.characterValidation)
			{
				case TMP_InputField.CharacterValidation.Alphanumeric: return CharacterValidation.ALPHANUMERIC;
				case TMP_InputField.CharacterValidation.Decimal: return CharacterValidation.DECIMAL;
				case TMP_InputField.CharacterValidation.EmailAddress: return CharacterValidation.EMAIL_ADDRESS;
				case TMP_InputField.CharacterValidation.Integer: return CharacterValidation.INTEGER;
				case TMP_InputField.CharacterValidation.Name: return CharacterValidation.NAME;
				case TMP_InputField.CharacterValidation.None: return CharacterValidation.NONE;
				default: return CharacterValidation.NONE;
			}
		}

		public static void ApplyCharacterValidation(this TMP_InputField inputField, InputFieldData data)
		{
			CharacterValidation characterValidation = data.characterValidation.GetValueOrDefault();

			switch(characterValidation)
			{
				case CharacterValidation.ALPHANUMERIC: inputField.characterValidation = TMP_InputField.CharacterValidation.Alphanumeric; break;
				case CharacterValidation.DECIMAL: inputField.characterValidation = TMP_InputField.CharacterValidation.Decimal; break;
				case CharacterValidation.EMAIL_ADDRESS: inputField.characterValidation = TMP_InputField.CharacterValidation.EmailAddress; break;
				case CharacterValidation.INTEGER: inputField.characterValidation = TMP_InputField.CharacterValidation.Integer; break;
				case CharacterValidation.NAME: inputField.characterValidation = TMP_InputField.CharacterValidation.Name; break;
				case CharacterValidation.NONE: inputField.characterValidation = TMP_InputField.CharacterValidation.None; break;
			}
		}
	}
}
#endif