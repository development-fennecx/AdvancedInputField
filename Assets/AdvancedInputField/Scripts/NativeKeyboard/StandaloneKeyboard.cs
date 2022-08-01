using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	public class StandaloneKeyboard: NativeKeyboard
	{
		private TextValidator textValidator;
		private bool emojisAllowed;
		private bool secure;
		private LineType lineType;
		private int characterLimit;

		private string text;
		private int selectionStartPosition;
		private int selectionEndPosition;

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

		public bool ShouldSubmit
		{
			get { return (lineType != LineType.MULTILINE_NEWLINE); }
		}

		public bool Multiline { get { return lineType != LineType.SINGLE_LINE; } }

		/// <summary>Indicates if some text is currently selected</summary>
		public bool HasSelection
		{
			get { return (selectionEndPosition > selectionStartPosition); }
		}

		private void Awake()
		{
			textValidator = new TextValidator();
			text = string.Empty;
			enabled = false;
		}

		public override void UpdateTextEdit(string text, int selectionStartPosition, int selectionEndPosition)
		{
			this.text = text;
			this.selectionStartPosition = selectionStartPosition;
			this.selectionEndPosition = selectionEndPosition;
		}

		public override void ShowKeyboard(string text, int selectionStartPosition, int selectionEndPosition, NativeKeyboardConfiguration configuration)
		{
			this.text = text;
			this.selectionStartPosition = selectionStartPosition;
			this.selectionEndPosition = selectionEndPosition;

			this.characterLimit = configuration.characterLimit;
			this.emojisAllowed = configuration.emojisAllowed;
			this.secure = configuration.secure;
			this.lineType = configuration.lineType;
			textValidator.Validation = configuration.characterValidation;
			textValidator.LineType = configuration.lineType;

			CharacterValidator characterValidator = null;
			if(!string.IsNullOrEmpty(configuration.characterValidatorJSON))
			{
				characterValidator = ScriptableObject.CreateInstance<CharacterValidator>();
				JsonUtility.FromJsonOverwrite(configuration.characterValidatorJSON, characterValidator);
			}
			textValidator.Validator = characterValidator;

			OnKeyboardShow();
		}

		public override void HideKeyboard()
		{
			OnKeyboardHide();
		}

		public override void EnableUpdates()
		{
			enabled = true;
			InputMethodManager.ClearEventQueue();
		}

		public override void DisableUpdates()
		{
			enabled = false;
		}

		private void Update()
		{
			Event keyboardEvent = new Event();
			while(Event.PopEvent(keyboardEvent))
			{
				if(keyboardEvent.rawType == EventType.KeyDown)
				{
					bool shouldContinue = ProcessKeyboardEvent(keyboardEvent);
					if(!shouldContinue)
					{
						return;
					}
				}

				if((keyboardEvent.type == EventType.ValidateCommand || keyboardEvent.type == EventType.ExecuteCommand)
					&& keyboardEvent.commandName == "SelectAll")
				{
					SelectAll();
				}
			}

			InputEvent inputEvent;
			while(InputMethodManager.PopEvent(out inputEvent))
			{
				switch(inputEvent.Type)
				{
					case InputEventType.CHARACTER:
						CharacterInputEvent characterInputEvent = (CharacterInputEvent)inputEvent;
						TryInsertChar(characterInputEvent.character);
						break;
					case InputEventType.TEXT:
						TextInputEvent textInputEvent = (TextInputEvent)inputEvent;
						Insert(textInputEvent.text);
						break;
				}
			}
		}

		/// <summary>Processes a keyboard event</summary>
		/// <param name="keyboardEvent">The keyboard event to process</param>
		internal bool ProcessKeyboardEvent(Event keyboardEvent)
		{
			EventModifiers currentEventModifiers = keyboardEvent.modifiers;
			bool ctrl = SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX ? (currentEventModifiers & EventModifiers.Command) != 0 : (currentEventModifiers & EventModifiers.Control) != 0;
			bool shift = (currentEventModifiers & EventModifiers.Shift) != 0;
			bool alt = (currentEventModifiers & EventModifiers.Alt) != 0;
			bool ctrlOnly = ctrl && !alt && !shift;

			switch(keyboardEvent.keyCode)
			{
				case KeyCode.Backspace:
					OnSpecialKeyPressed(SpecialKeyCode.BACKSPACE);
					DeletePreviousChar();
					return true;
				case KeyCode.Delete:
					DeleteNextChar();
					return true;
				case KeyCode.Home:
					MoveToStart();
					return true;
				case KeyCode.End:
					MoveToEnd();
					return true;
				case KeyCode.A: //Select All
					if(ctrlOnly)
					{
						SelectAll();
						return true;
					}
					break;
				case KeyCode.C: //Copy
					if(ctrlOnly)
					{
						Copy();
						return true;
					}
					break;
				case KeyCode.V: //Paste
					if(ctrlOnly)
					{
						Paste();
						return true;
					}
					break;
				case KeyCode.X: //Cut
					if(ctrlOnly)
					{
						Cut();
						return true;
					}
					break;
				case KeyCode.LeftArrow:
					OnMoveLeft(shift, ctrl);
					return true;
				case KeyCode.RightArrow:
					OnMoveRight(shift, ctrl);
					return true;
				case KeyCode.DownArrow:
					OnMoveDown(shift, ctrl);
					return true;
				case KeyCode.UpArrow:
					OnMoveUp(shift, ctrl);
					return true;
				case KeyCode.Return: //Submit
				case KeyCode.KeypadEnter: //Submit
					if(ShouldSubmit)
					{
						OnKeyboardDone();
						return false;
					}
					break;
				case KeyCode.Escape:
					OnSpecialKeyPressed(SpecialKeyCode.ESCAPE);
					OnKeyboardCancel();
					return false;
				case KeyCode.Tab:
					OnKeyboardNext();
					return false;
			}

			char c = keyboardEvent.character;
			if(!Multiline && (c == '\t' || c == '\r' || c == 10)) //Don't allow return chars or tabulator key to be entered into single line fields.
			{
				return true;
			}

			if(c == '\r' || (int)c == 3) //Convert carriage return and end-of-text characters to newline.
			{
				c = '\n';
			}

			TryInsertChar(c);

			return true;
		}

		/// <summary>Copies current text selection</summary>
		internal virtual void Copy()
		{
			if(!secure)
			{
				Clipboard = text.Substring(selectionStartPosition, selectionEndPosition - selectionStartPosition);
			}
			else
			{
				Clipboard = string.Empty;
			}
		}

		/// <summary>Pastes clipboard text</summary>
		internal virtual void Paste()
		{
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
			if(!secure)
			{
				Clipboard = text.Substring(selectionStartPosition, selectionEndPosition - selectionStartPosition);
			}
			else
			{
				Clipboard = string.Empty;
			}

			if(selectionEndPosition > selectionStartPosition)
			{
				DeleteSelection();
			}
		}

		internal void SelectAll()
		{
			selectionStartPosition = 0;
			selectionEndPosition = text.Length;
			OnTextEditUpdate(text, selectionStartPosition, selectionEndPosition);
		}

		/// <summary>Moves caret to start of the text</summary>
		internal void MoveToStart()
		{
			selectionStartPosition = 0;
			selectionEndPosition = selectionStartPosition;
			OnTextEditUpdate(text, selectionStartPosition, selectionEndPosition);
		}

		/// <summary>Moves caret to end of the text</summary>
		internal void MoveToEnd()
		{
			selectionStartPosition = text.Length;
			selectionEndPosition = selectionStartPosition;
			OnTextEditUpdate(text, selectionStartPosition, selectionEndPosition);
		}

		/// <summary>Tries to insert a character</summary>
		/// <param name="c">The character to insert</param>
		internal void TryInsertChar(char c)
		{
			if(!IsValidChar(c))
			{
				return;
			}

			Insert(c.ToString());
		}

		/// <summary>Checks if character is valid</summary>
		/// <param name="c">The character to check</param>
		internal bool IsValidChar(char c)
		{
			if((int)c == 127 || (int)c == 0) //Delete key on mac and zero char
			{
				return false;
			}

			return true;
		}

		/// <summary>Insert a string at caret position</summary>
		/// <param name="input">the string to insert</param>
		internal virtual void Insert(string input)
		{
			if(selectionEndPosition > selectionStartPosition)
			{
				text = text.Remove(selectionStartPosition, selectionEndPosition - selectionStartPosition);
				selectionEndPosition = selectionStartPosition;
			}

			string resultText;
			int resultCaretPosition;
			if(emojisAllowed) //Not validating individual characters when using emojis, because that could break the character sequences
			{
				int caretPosition = selectionStartPosition;
				resultText = text.Insert(caretPosition, input);
				resultCaretPosition = caretPosition + input.Length;
			}
			else
			{
				if(characterLimit > 0 && text.Length + input.Length > characterLimit)
				{
					if(text.Length < characterLimit)
					{
						int amountAllowed = characterLimit - text.Length;
						input = input.Substring(0, amountAllowed);
					}
					else
					{
						return;
					}
				}

				int caretPosition = selectionStartPosition;
				textValidator.Validate(text, input, caretPosition, selectionStartPosition);
				resultText = textValidator.ResultText;
				resultCaretPosition = textValidator.ResultCaretPosition;

				ApplyCharacterLimit(ref resultText, ref resultCaretPosition);
			}

			text = resultText;
			selectionStartPosition = resultCaretPosition;
			selectionEndPosition = resultCaretPosition;
			OnTextEditUpdate(text, selectionStartPosition, selectionEndPosition);
		}

		public void ApplyCharacterLimit(ref string text, ref int caretPosition)
		{
			if(characterLimit != 0 && text.Length > characterLimit)
			{
				text = text.Substring(0, characterLimit);
				caretPosition = Mathf.Clamp(caretPosition, 0, text.Length);
			}
		}

		/// <summary>Deletes previous character</summary>
		internal void DeletePreviousChar()
		{
			if(selectionEndPosition > selectionStartPosition)
			{
				DeleteSelection();
			}
			else if(selectionStartPosition > 0) //Backwards delete
			{
				selectionStartPosition--;

				EmojiData emojiData;
				if(emojisAllowed && NativeKeyboardManager.EmojiEngine.TryFindPreviousEmojiInText(text, selectionStartPosition, out emojiData))
				{
					int count = emojiData.text.Length;
					text = text.Remove(selectionStartPosition + 1 - count, count);
					selectionStartPosition -= (count - 1);
				}
				else
				{
					text = text.Remove(selectionStartPosition, 1);
				}
				selectionEndPosition = selectionStartPosition;

				OnTextEditUpdate(text, selectionStartPosition, selectionEndPosition);
			}
		}

		/// <summary>Deletes next character</summary>
		internal void DeleteNextChar()
		{
			if(selectionEndPosition > selectionStartPosition)
			{
				DeleteSelection();
			}
			else if(selectionStartPosition < text.Length) //Forward delete
			{
				EmojiData emojiData;
				if(emojisAllowed && NativeKeyboardManager.EmojiEngine.TryFindNextEmojiInText(text, selectionStartPosition, out emojiData))
				{
					int count = emojiData.text.Length;
					text = text.Remove(selectionStartPosition, count);
				}
				else
				{
					text = text.Remove(selectionStartPosition, 1);
				}
				selectionEndPosition = selectionStartPosition;

				OnTextEditUpdate(text, selectionStartPosition, selectionEndPosition);
			}
		}

		/// <summary>Deletes current text selection</summary>
		internal virtual void DeleteSelection()
		{
			text = text.Remove(selectionStartPosition, selectionEndPosition - selectionStartPosition);
			selectionEndPosition = selectionStartPosition;

			OnTextEditUpdate(text, selectionStartPosition, selectionEndPosition);
		}
	}
}
