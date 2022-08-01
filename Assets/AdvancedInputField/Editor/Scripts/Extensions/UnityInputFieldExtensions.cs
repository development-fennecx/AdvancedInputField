using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedInputFieldPlugin.Editor
{
	public static class UnityInputFieldExtensions
	{
		public static RectTransform ExtractData(this InputField inputField, ref InputFieldData inputFieldData, ref TextRendererData textRendererData, ref TextRendererData placeholderRendererData)
		{
			RectTransform rectTransform = inputField.GetComponent<RectTransform>();
			inputFieldData = inputField.ExtractInputFieldData();

			Text fromText = inputField.textComponent;
			if(fromText != null)
			{
				textRendererData = fromText.ExtractTextRendererData();
			}

			Graphic fromPlaceholder = inputField.placeholder;
			if(fromPlaceholder != null && fromPlaceholder.GetComponent<Text>() != null)
			{
				placeholderRendererData = fromPlaceholder.GetComponent<Text>().ExtractTextRendererData();
			}

			return rectTransform;
		}

		public static void ApplyData(this InputField inputField, RectTransform sourceTransform, InputFieldData inputFieldData, TextRendererData textRendererData, TextRendererData placeholderRendererData)
		{
			RectTransform rectTransform = inputField.GetComponent<RectTransform>();
			rectTransform.CopyValues(sourceTransform);
			inputField.name = sourceTransform.name;
			inputField.ApplyInputFieldData(inputFieldData);

			Text text = inputField.textComponent;
			if(text != null && textRendererData != null)
			{
				text.ApplyTextRendererData(textRendererData);
			}

			Graphic placeholder = inputField.placeholder;
			if(placeholder != null)
			{
				Text placeholderText = placeholder.GetComponent<Text>();
				if(placeholderText != null && placeholderRendererData != null)
				{
					placeholderText.ApplyTextRendererData(placeholderRendererData);
				}
			}
		}

		public static InputFieldData ExtractInputFieldData(this InputField inputField)
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

		public static void ApplyInputFieldData(this InputField inputField, InputFieldData data)
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

		public static string ExtractPlaceholder(this InputField inputField)
		{
			if(inputField.placeholder != null)
			{
				Text text = inputField.placeholder.GetComponent<Text>();
				if(text != null)
				{
					return text.text;
				}
			}
			return null;
		}

		public static void ApplyPlaceHolder(this InputField inputField, InputFieldData data)
		{
			if(inputField.placeholder != null)
			{
				Text text = inputField.placeholder.GetComponent<Text>();
				if(text != null)
				{
					text.text = data.placeholder;
				}
			}
		}

		public static ContentType ExtractContentType(this InputField inputField)
		{
			switch(inputField.contentType)
			{
				case InputField.ContentType.Alphanumeric: return ContentType.ALPHANUMERIC;
				case InputField.ContentType.Autocorrected: return ContentType.AUTOCORRECTED;
				case InputField.ContentType.Custom: return ContentType.CUSTOM;
				case InputField.ContentType.DecimalNumber: return ContentType.DECIMAL_NUMBER;
				case InputField.ContentType.EmailAddress: return ContentType.EMAIL_ADDRESS;
				case InputField.ContentType.IntegerNumber: return ContentType.INTEGER_NUMBER;
				case InputField.ContentType.Name: return ContentType.NAME;
				case InputField.ContentType.Password: return ContentType.PASSWORD;
				case InputField.ContentType.Pin: return ContentType.PIN;
				case InputField.ContentType.Standard: return ContentType.STANDARD;
				default: return ContentType.STANDARD;
			}
		}

		public static void ApplyContentType(this InputField inputField, InputFieldData data)
		{
			ContentType contentType = data.contentType.GetValueOrDefault();

			switch(contentType)
			{
				case ContentType.ALPHANUMERIC: inputField.contentType = InputField.ContentType.Alphanumeric; break;
				case ContentType.AUTOCORRECTED: inputField.contentType = InputField.ContentType.Autocorrected; break;
				case ContentType.CUSTOM: inputField.contentType = InputField.ContentType.Custom; break;
				case ContentType.DECIMAL_NUMBER: inputField.contentType = InputField.ContentType.DecimalNumber; break;
				case ContentType.EMAIL_ADDRESS: inputField.contentType = InputField.ContentType.EmailAddress; break;
				case ContentType.INTEGER_NUMBER: inputField.contentType = InputField.ContentType.IntegerNumber; break;
				case ContentType.NAME: inputField.contentType = InputField.ContentType.Name; break;
				case ContentType.PASSWORD: inputField.contentType = InputField.ContentType.Password; break;
				case ContentType.PIN: inputField.contentType = InputField.ContentType.Pin; break;
				case ContentType.STANDARD: inputField.contentType = InputField.ContentType.Standard; break;
			}
		}

		public static LineType ExtractLineType(this InputField inputField)
		{
			switch(inputField.lineType)
			{
				case InputField.LineType.MultiLineNewline: return LineType.MULTILINE_NEWLINE;
				case InputField.LineType.MultiLineSubmit: return LineType.MULTILINE_SUBMIT;
				case InputField.LineType.SingleLine: return LineType.SINGLE_LINE;
				default: return LineType.SINGLE_LINE;
			}
		}

		public static void ApplyLineType(this InputField inputField, InputFieldData data)
		{
			LineType lineType = data.lineType.GetValueOrDefault();

			switch(lineType)
			{
				case LineType.MULTILINE_NEWLINE: inputField.lineType = InputField.LineType.MultiLineNewline; break;
				case LineType.MULTILINE_SUBMIT: inputField.lineType = InputField.LineType.MultiLineSubmit; break;
				case LineType.SINGLE_LINE: inputField.lineType = InputField.LineType.SingleLine; break;
			}
		}

		public static InputType ExtractInputType(this InputField inputField)
		{
			switch(inputField.inputType)
			{
				case InputField.InputType.AutoCorrect: return InputType.AUTOCORRECT;
				case InputField.InputType.Password: return InputType.PASSWORD;
				case InputField.InputType.Standard: return InputType.STANDARD;
				default: return InputType.STANDARD;
			}
		}

		public static void ApplyInputType(this InputField inputField, InputFieldData data)
		{
			InputType inputType = data.inputType.GetValueOrDefault();

			switch(inputType)
			{
				case InputType.AUTOCORRECT: inputField.inputType = InputField.InputType.AutoCorrect; break;
				case InputType.PASSWORD: inputField.inputType = InputField.InputType.Password; break;
				case InputType.STANDARD: inputField.inputType = InputField.InputType.Standard; break;
			}
		}

		public static KeyboardType ExtractKeyboardType(this InputField inputField)
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

		public static void ApplyKeyboardType(this InputField inputField, InputFieldData data)
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

		public static CharacterValidation ExtractCharacterValidation(this InputField inputField)
		{
			switch(inputField.characterValidation)
			{
				case InputField.CharacterValidation.Alphanumeric: return CharacterValidation.ALPHANUMERIC;
				case InputField.CharacterValidation.Decimal: return CharacterValidation.DECIMAL;
				case InputField.CharacterValidation.EmailAddress: return CharacterValidation.EMAIL_ADDRESS;
				case InputField.CharacterValidation.Integer: return CharacterValidation.INTEGER;
				case InputField.CharacterValidation.Name: return CharacterValidation.NAME;
				case InputField.CharacterValidation.None: return CharacterValidation.NONE;
				default: return CharacterValidation.NONE;
			}
		}

		public static void ApplyCharacterValidation(this InputField inputField, InputFieldData data)
		{
			CharacterValidation characterValidation = data.characterValidation.GetValueOrDefault();

			switch(characterValidation)
			{
				case CharacterValidation.ALPHANUMERIC: inputField.characterValidation = InputField.CharacterValidation.Alphanumeric; break;
				case CharacterValidation.DECIMAL: inputField.characterValidation = InputField.CharacterValidation.Decimal; break;
				case CharacterValidation.EMAIL_ADDRESS: inputField.characterValidation = InputField.CharacterValidation.EmailAddress; break;
				case CharacterValidation.INTEGER: inputField.characterValidation = InputField.CharacterValidation.Integer; break;
				case CharacterValidation.NAME: inputField.characterValidation = InputField.CharacterValidation.Name; break;
				case CharacterValidation.NONE: inputField.characterValidation = InputField.CharacterValidation.None; break;
			}
		}
	}
}
