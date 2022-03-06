// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Class to verify text (applies CharacterValidation)</summary>
	public class TextValidator
	{
		private const string EMAIL_SPECIAL_CHARACTERS = "!#$%&'*+-/=?^_`{|}~";

		public CharacterValidation Validation { get; set; }
		public LineType LineType { get; set; }
		public CharacterValidator Validator { get; set; }
		public string ResultText { get; private set; }
		public int ResultCaretPosition { get; private set; }

		public TextValidator()
		{
		}

		public TextValidator(CharacterValidation validation, LineType lineType, CharacterValidator validator = null)
		{
			Validation = validation;
			LineType = lineType;
			Validator = validator;
		}

		/// <summary>Validates the text</summary>
		/// <param name="text">The text to check</param>
		/// <param name="pos">The current char position</param>
		/// <param name="ch">The next character</param>
		/// <param name="selectionStartPosition">Current selection start position</param>
		internal void Validate(string text, string textToAppend, int caretPosition, int selectionStartPosition)
		{
			int textLength = text.Length;
			int textToAppendLength = textToAppend.Length;

			int startCaretPosition = caretPosition;
			char[] buffer = new char[textLength + textToAppendLength];
			Util.StringCopy(ref buffer, text);

			int position = caretPosition;
			for(int i = 0; i < textToAppendLength; i++)
			{
				char ch = textToAppend[i];
				char result = ValidateChar(ch, buffer, position, position, caretPosition, selectionStartPosition);
				if(result != 0)
				{
					buffer[position] = result;
					position++;
					caretPosition++;
				}
			}

			if(startCaretPosition < textLength)
			{
				for(int i = startCaretPosition; i < textLength; i++)
				{
					char ch = text[i];
					char result = ValidateChar(ch, buffer, position, position, caretPosition, selectionStartPosition);
					if(result != 0)
					{
						buffer[position] = result;
						position++;
					}
				}
			}

			textLength = position;

			ResultText = new string(buffer, 0, textLength);
			ResultCaretPosition = caretPosition;
		}

		internal char ValidateChar(char ch, char[] text, int textLength, int pos, int caretPosition, int selectionStartPosition)
		{
			if(LineType != LineType.MULTILINE_NEWLINE && (ch == '\r' || ch == '\n'))
			{
				return (char)0;
			}

			// Validation is disabled
			if(Validation == CharacterValidation.NONE)
			{
				return ch;
			}

			if(Validation == CharacterValidation.CUSTOM)
			{
				if(Validator == null) { return ch; }
				else
				{
					return Validator.Validate(ch, text, textLength, pos, selectionStartPosition);
				}
			}

			if(Validation == CharacterValidation.INTEGER || Validation == CharacterValidation.DECIMAL || Validation == CharacterValidation.DECIMAL_FORCE_POINT)
			{
				// Integer and decimal
				bool cursorBeforeDash = (pos == 0 && textLength > 0 && text[0] == '-');
				bool dashInSelection = textLength > 0 && text[0] == '-' && ((caretPosition == 0 && selectionStartPosition > 0) || (selectionStartPosition == 0 && caretPosition > 0));
				bool selectionAtStart = caretPosition == 0 || selectionStartPosition == 0;
				if(!cursorBeforeDash || dashInSelection)
				{
					if(ch >= '0' && ch <= '9') return ch;
					if(ch == '-' && (pos == 0 || selectionAtStart)) return ch;
					if(Validation == CharacterValidation.DECIMAL)
					{
						if(ch == '.' || ch == ',')
						{
							if(!Util.Contains('.', text, textLength) && !Util.Contains(',', text, textLength)) return ch;
						}
					}
					else if(Validation == CharacterValidation.DECIMAL_FORCE_POINT)
					{
						if(ch == '.' && !Util.Contains('.', text, textLength)) return ch;
						if(ch == ',' && !Util.Contains('.', text, textLength)) return '.';
					}
				}
			}
			else if(Validation == CharacterValidation.ALPHANUMERIC)
			{
				// All alphanumeric characters
				if(ch >= 'A' && ch <= 'Z') return ch;
				if(ch >= 'a' && ch <= 'z') return ch;
				if(ch >= '0' && ch <= '9') return ch;
			}
			else if(Validation == CharacterValidation.NAME)
			{
				// FIXME: some actions still lead to invalid input:
				//        - Hitting delete in front of an uppercase letter
				//        - Selecting an uppercase letter and deleting it
				//        - Typing some text, hitting Home and typing more text (we then have an uppercase letter in the middle of a word)
				//        - Typing some text, hitting Home and typing a space (we then have a leading space)
				//        - Erasing a space between two words (we then have an uppercase letter in the middle of a word)
				//        - We accept a trailing space
				//        - We accept the insertion of a space between two lowercase letters.
				//        - Typing text in front of an existing uppercase letter
				//        - ... and certainly more
				//
				// The rule we try to implement are too complex for this kind of verification.

				if(char.IsLetter(ch))
				{
					// Character following a space should be in uppercase.
					if(char.IsLower(ch) && ((pos == 0) || (text[pos - 1] == ' ')))
					{
						return char.ToUpper(ch);
					}

					// Character not following a space or an apostrophe should be in lowercase.
					if(char.IsUpper(ch) && (pos > 0) && (text[pos - 1] != ' ') && (text[pos - 1] != '\''))
					{
						return char.ToLower(ch);
					}

					return ch;
				}

				if(ch == '\'')
				{
					// Don't allow more than one apostrophe
					if(!Util.Contains('\'', text, textLength))
					{
						// Don't allow consecutive spaces and apostrophes.
						if(!(((pos > 0) && ((text[pos - 1] == ' ') || (text[pos - 1] == '\''))) ||
							((pos < textLength) && ((text[pos] == ' ') || (text[pos] == '\'')))))
						{
							return ch;
						}
					}
				}

				if(ch == ' ')
				{
					// Don't allow consecutive spaces and apostrophes.
					if(!(((pos > 0) && ((text[pos - 1] == ' ') || (text[pos - 1] == '\''))) ||
						((pos < textLength) && ((text[pos] == ' ') || (text[pos] == '\'')))))
					{
						return ch;
					}
				}
			}
			else if(Validation == CharacterValidation.EMAIL_ADDRESS)
			{
				// From StackOverflow about allowed characters in email addresses:
				// Uppercase and lowercase English letters (a-z, A-Z)
				// Digits 0 to 9
				// Characters ! # $ % & ' * + - / = ? ^ _ ` { | } ~
				// Character . (dot, period, full stop) provided that it is not the first or last character,
				// and provided also that it does not appear two or more times consecutively.

				if(char.IsLetterOrDigit(ch)) return ch;
				if(ch == '@' && Util.IndexOf('@', text, textLength) == -1) return ch;
				if(EMAIL_SPECIAL_CHARACTERS.IndexOf(ch) != -1) return ch;
				if(ch == '.')
				{
					char lastChar = (textLength > 0) ? text[Mathf.Clamp(pos, 0, textLength - 1)] : ' ';
					char nextChar = (textLength > 0) ? text[Mathf.Clamp(pos + 1, 0, textLength - 1)] : '\n';
					if(lastChar != '.' && nextChar != '.')
					{
						return ch;
					}
				}
			}
			else if(Validation == CharacterValidation.IP_ADDRESS)
			{
				int lastDotIndex = Util.LastIndexOf('.', text, textLength);
				if(lastDotIndex == -1)
				{
					int numbersInSection = textLength;
					if(numbersInSection < 3 && ch >= '0' && ch <= '9') return ch; //Less than 3 numbers, so number add allowed
					if(ch == '.' && textLength > 0) { return ch; } //Don't start with dot
				}
				else
				{
					if(ch >= '0' && ch <= '9')
					{
						int numbersInSection = (textLength - 1) - lastDotIndex;
						if(numbersInSection < 3 && ch >= '0' && ch <= '9') return ch; //Less than 3 numbers, so number add allowed
					}
					if(ch == '.' && lastDotIndex != textLength - 1 && Util.CountOccurences('.', text, textLength) < 3) { return ch; } //Max 4 sections (3 dot characters)
				}
			}
			else if(Validation == CharacterValidation.SENTENCE)
			{
				if(char.IsLetter(ch) && char.IsLower(ch))
				{
					if(pos == 0) { return char.ToUpper(ch); }

					if(pos > 1 && text[pos - 1] == ' ' && text[pos - 2] == '.')
					{
						return char.ToUpper(ch);
					}
				}

				return ch;
			}
			return (char)0;
		}
	}
}
