//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Base class for text string manipulation</summary>
	public class TextManipulator
	{
		/// <summary>The TextValidator</summary>
		private TextValidator textValidator;

		public InputFieldEngine Engine { get; private set; }

		/// <summary>The InputField</summary>
		public AdvancedInputField InputField { get { return Engine.InputField; } }

		/// <summary>The TextNavigator/summary>
		public virtual TextNavigator TextNavigator { get; protected set; }

		/// <summary>The main renderer for text</summary>
		public TextRenderer TextRenderer { get; private set; }

		/// <summary>The renderer for processed text</summary>
		public TextRenderer ProcessedTextRenderer { get; private set; }

		/// <summary>The main text string</summary>
		public virtual string Text
		{
			get { return Engine.Text; }
			set
			{
				Engine.SetText(value);
			}
		}

		/// <summary>The text in the clipboard</summary>
		public static string Clipboard
		{
			get
			{
				return GUIUtility.systemCopyBuffer;
			}
			set
			{
				GUIUtility.systemCopyBuffer = value;
			}
		}

		/// <summary>Indicates whether to currrently block text change events from being send to the native bindings</summary>
		public bool BlockNativeTextChange { get; set; }

		/// <summary>Initializes the class</summary>
		internal virtual void Initialize(InputFieldEngine engine, TextNavigator textNavigator, TextRenderer textRenderer, TextRenderer processedTextRenderer)
		{
			Engine = engine;
			TextNavigator = textNavigator;
			TextRenderer = textRenderer;
			ProcessedTextRenderer = processedTextRenderer;
			textValidator = new TextValidator(InputField.CharacterValidation, InputField.LineType, InputField.CharacterValidator);
		}

		internal void RefreshTextValidator()
		{
			textValidator = new TextValidator(InputField.CharacterValidation, InputField.LineType, InputField.CharacterValidator);
		}

		/// <summary>Begins the edit mode</summary>
		internal void BeginEditMode()
		{
			Engine.RefreshRenderedText();
			textValidator.Validator = InputField.CharacterValidator;
			if(InputField.LiveDecoration)
			{
				LiveDecorationFilter liveProcessingFilter = InputField.LiveDecorationFilter;
				string text = Engine.Text;
				int caretPosition = Engine.CaretPosition;
				string processedText = liveProcessingFilter.ProcessText(text, caretPosition);
				if(processedText != null)
				{
					Engine.SetProcessedText(processedText);

					int processedCaretPosition = liveProcessingFilter.DetermineProcessedCaret(text, caretPosition, processedText);
					Engine.SetProcessedSelection(processedCaretPosition, processedCaretPosition);
				}
			}
		}

		/// <summary>Ends the edit mode</summary>
		internal void EndEditMode()
		{
			if(InputField.PostDecorationFilter != null)
			{
				if(string.IsNullOrEmpty(Text)) //Empty
				{
					Engine.SetProcessedText(string.Empty);
				}
				else if(InputField.PostDecorationFilter.ProcessText(Text, out string processedText)) //Try to process text
				{
					Engine.SetProcessedText(processedText);
				}
				else //Couldn't be parsed, use original value
				{
					Engine.SetProcessedText(Text);
				}
			}
		}

		/// <summary>Checks if character is valid</summary>
		/// <param name="c">The character to check</param>
		internal bool IsValidChar(char c)
		{
			if((int)c == 127) //Delete key on mac
			{
				return false;
			}

			if(c == '\t' || c == '\n') // Accept newline and tab
			{
				return true;
			}

			return TextRenderer.FontHasCharacter(c);
		}

		/// <summary>Insert a string at caret position</summary>
		/// <param name="input">the string to insert</param>
		internal virtual void Insert(string input)
		{
			if(InputField.ReadOnly)
			{
				return;
			}

			if(Engine.HasSelection)
			{
				DeleteSelection();
			}

			string resultText;
			int resultCaretPosition;
			if(InputField.EmojisAllowed) //Not validating individual characters when using emojis, because that could break the character sequences
			{
				resultText = Text.Insert(Engine.CaretPosition, input);
				resultCaretPosition = Engine.CaretPosition + input.Length;
			}
			else
			{
				if(InputField.CharacterLimit > 0 && Text.Length + input.Length > InputField.CharacterLimit)
				{
					if(Text.Length < InputField.CharacterLimit)
					{
						int amountAllowed = InputField.CharacterLimit - Text.Length;
						input = input.Substring(0, amountAllowed);
					}
					else
					{
						return;
					}
				}

				int selectionStartPosition = -1;
				if(Engine.HasSelection)
				{
					selectionStartPosition = Engine.SelectionStartPosition;
				}
				textValidator.Validate(Text, input, Engine.CaretPosition, selectionStartPosition);
				resultText = textValidator.ResultText;
				resultCaretPosition = textValidator.ResultCaretPosition;

				ApplyCharacterLimit(ref resultText, ref resultCaretPosition);
			}

			TextEditFrame textEditFrame = new TextEditFrame(resultText, resultCaretPosition, resultCaretPosition);
			Engine.ApplyTextEditFrame(textEditFrame);
		}

		public void ApplyCharacterLimit(ref string text, ref int caretPosition)
		{
			if(InputField.CharacterLimit != 0 && text.Length > InputField.CharacterLimit)
			{
				text = text.Substring(0, InputField.CharacterLimit);
				caretPosition = Mathf.Clamp(caretPosition, 0, Text.Length);
			}
		}

		/// <summary>Deletes current text selection</summary>
		internal virtual void DeleteSelection()
		{
			if(InputField.ReadOnly)
			{
				return;
			}

			string text = Text.Remove(Engine.SelectionStartPosition, Engine.SelectionEndPosition - Engine.SelectionStartPosition);
			int caretPosition = Engine.SelectionStartPosition;
			TextEditFrame textEditFrame = new TextEditFrame(text, caretPosition, caretPosition);
			Engine.ApplyTextEditFrame(textEditFrame);

			Engine.ResetDragStartPosition(Engine.CaretPosition);
		}

		/// <summary>Copies current text selection</summary>
		internal virtual void Copy()
		{
			if(!InputField.IsPasswordField)
			{
				Clipboard = Engine.SelectedText;
			}
			else
			{
				Clipboard = string.Empty;
			}

			TextNavigator.HideActionBar();
		}

		/// <summary>Pastes clipboard text</summary>
		internal virtual void Paste()
		{
			if(InputField.ReadOnly)
			{
				return;
			}

			string input = Clipboard;
			string processedInput = string.Empty;

			int length = input.Length;
			for(int i = 0; i < length; i++)
			{
				char c = input[i];

				if(c >= ' ' || c == '\t' || c == '\r' || c == 10 || c == '\n')
				{
					processedInput += c;
				}
			}

			if(!string.IsNullOrEmpty(processedInput))
			{
				Insert(processedInput);
			}
		}

		/// <summary>Cuts current text selection</summary>
		internal virtual void Cut()
		{
			if(!InputField.IsPasswordField)
			{
				Clipboard = Engine.SelectedText;
			}
			else
			{
				Clipboard = string.Empty;
			}

			if(InputField.ReadOnly)
			{
				return;
			}

			if(Engine.HasSelection)
			{
				DeleteSelection();
			}
		}

		/// <summary>Replaces current word with given text</summary>
		internal virtual void Replace(string text)
		{
			if(InputField.ReadOnly)
			{
				return;
			}

			TextRange range;
			if(Engine.HasSelection)
			{
				Insert(text);
			}
			else if(TryGetWordRange(Engine.Text, Engine.CaretPosition, out range))
			{
				Engine.SetSelection(range.start, range.end + 1);
				Insert(text);
			}
		}

		public bool TryGetWordRange(string text, int position, out TextRange range)
		{
			if(position >= text.Length || !char.IsLetter(text[position])) //Not in a word
			{
				range = default(TextRange);
				return false;
			}

			int wordStart = position;
			int wordEnd = position;

			int length = text.Length;
			for(int i = position - 1; i >= 0; i--)
			{
				if(!char.IsLetter(text[i]))
				{
					break;
				}

				wordStart = i;
			}
			for(int i = position + 1; i < length; i++)
			{
				if(!char.IsLetter(text[i]))
				{
					break;
				}

				wordEnd = i;
			}

			if(wordEnd > wordStart)
			{
				range = new TextRange(wordStart, wordEnd);
				return true;
			}
			else
			{
				range = default(TextRange);
				return false;
			}
		}
	}
}
