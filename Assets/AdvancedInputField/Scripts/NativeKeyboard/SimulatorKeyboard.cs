// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using UnityEngine;
using UnityEngine.UI;

namespace AdvancedInputFieldPlugin
{
	public class SimulatorKeyboard: NativeKeyboard
	{
		public const float TRANSITION_TIME = 0.5f;
		public readonly Color DEFAULT_COLOR = Color.black;
		public readonly Color ACTIVE_COLOR = new Color(0, 0.5f, 1f);

		[SerializeField]
		private Button characterButtonPrefab;

		[SerializeField]
		private float xSpacing = 16;

		[SerializeField]
		private float ySpacing = 16;

		[SerializeField]
		private RectTransform[] characterRows;

		[SerializeField]
		private string[] mainPageValues;

		[SerializeField]
		private string[] symbolPage1Values;

		[SerializeField]
		private string[] symbolPage2Values;

		[SerializeField]
		private Button shiftButton;

		private RectTransform rectTransform;
		private Canvas canvas;
		private Vector2 buttonSize;
		private bool mainPageActive;
		private int pageNr;
		private bool uppercaseActive;

		private string text;
		private int selectionStartPosition;
		private int selectionEndPosition;

		private LineType lineType;
		private int characterLimit;
		private bool emojisAllowed;
		private TextValidator textValidator;

		private float currentTransitionTime;
		private bool processHardwareKeyboardEvents;

		public bool ShouldSubmit
		{
			get { return (lineType != LineType.MULTILINE_NEWLINE); }
		}

		public bool Multiline { get { return lineType != LineType.SINGLE_LINE; } }

		private void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
			buttonSize = DetermineSmallestButtonSize();
			ConfigureCharacterRows(mainPageValues);
			textValidator = new TextValidator(CharacterValidation.NONE, LineType.SINGLE_LINE);
		}

		public override void EnableUpdates()
		{
			processHardwareKeyboardEvents = true;
			InputMethodManager.ClearEventQueue();
		}

		public override void DisableUpdates()
		{
			processHardwareKeyboardEvents = false;
		}

		private void Update()
		{
			if(State == KeyboardState.PENDING_SHOW)
			{
				currentTransitionTime += Time.deltaTime;
				if(currentTransitionTime >= TRANSITION_TIME)
				{
					currentTransitionTime = TRANSITION_TIME;
					OnKeyboardShow();
					int keyboardHeight = Mathf.RoundToInt(rectTransform.rect.height * canvas.scaleFactor); //Convert to screen pixels
					OnKeyboardHeightChanged(keyboardHeight); //Fully shown
				}

				float progress = currentTransitionTime / TRANSITION_TIME;
				Vector2 anchoredPositon = rectTransform.anchoredPosition;
				anchoredPositon.y = -((1 - progress) * rectTransform.rect.height);
				rectTransform.anchoredPosition = anchoredPositon;
			}
			else if(State == KeyboardState.PENDING_HIDE)
			{
				currentTransitionTime += Time.deltaTime;
				if(currentTransitionTime >= TRANSITION_TIME)
				{
					currentTransitionTime = TRANSITION_TIME;
					OnKeyboardHide();
					OnKeyboardHeightChanged(0); //Fully hidden
				}

				float progress = currentTransitionTime / TRANSITION_TIME;
				Vector2 anchoredPositon = rectTransform.anchoredPosition;
				anchoredPositon.y = -(progress * rectTransform.rect.height);
				rectTransform.anchoredPosition = anchoredPositon;
			}

			if(processHardwareKeyboardEvents)
			{
				UpdateHardwareKeyboardInput();
			}
		}

		internal void UpdateHardwareKeyboardInput()
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
			switch(keyboardEvent.keyCode)
			{
				case KeyCode.Backspace:
					OnBackspaceClick();
					return true;
				case KeyCode.Delete:
					OnDeleteClick();
					return true;
				case KeyCode.LeftArrow:
					return true;
				case KeyCode.RightArrow:
					return true;
				case KeyCode.DownArrow:
					return true;
				case KeyCode.UpArrow:
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
					OnSpecialKeyPressed(SpecialKeyCode.BACK); //Escape key acts a Back key in this simulated keyboard
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

		internal override void Setup()
		{
			LoadMainPage();
			mainPageActive = true;
			uppercaseActive = false;
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

			canvas = GetComponentInParent<Canvas>(); //Update current Canvas

			if(State == KeyboardState.HIDDEN)
			{
				currentTransitionTime = 0;
				State = KeyboardState.PENDING_SHOW;
			}

			LoadMainPage();
			mainPageActive = true;
			uppercaseActive = false;
		}

		public override void HideKeyboard()
		{
			if(gameObject == null) { return; }
			canvas = GetComponentInParent<Canvas>(); //Update current Canvas

			if(State == KeyboardState.VISIBLE || State == KeyboardState.PENDING_HIDE || State == KeyboardState.PENDING_SHOW)
			{
				currentTransitionTime = 0;
				State = KeyboardState.PENDING_HIDE;
			}
		}

		public void ConfigureCharacterRows(string[] characterRowValues)
		{
			int length = characterRows.Length;
			for(int i = 0; i < length; i++)
			{
				ConfigureCharacterRow(characterRows[i], characterRowValues[i]);
			}
		}

		private void ConfigureCharacterRow(RectTransform characterRow, string characterRowValue)
		{
			CleanCharacterRow(characterRow);

			Vector2 boundsSize = characterRow.rect.size;
			int length = characterRowValue.Length;
			float x = (-((length - 1) * 0.5f) * (buttonSize.x + xSpacing));
			float y = 0;

			for(int i = 0; i < length; i++)
			{
				Button characterButton = CreateCharacterButton(characterRow);
				characterButton.onClick.AddListener(() => OnCharacterButtonClick(characterButton));
				RectTransform rectTransform = characterButton.GetComponent<RectTransform>();
				rectTransform.anchoredPosition = new Vector2(x, y);
				rectTransform.sizeDelta = buttonSize;

				x += (buttonSize.x + xSpacing);
			}
		}

		private void CleanCharacterRow(RectTransform characterRow)
		{
			while(characterRow.childCount > 0)
			{
				DestroyImmediate(characterRow.GetChild(0).gameObject);
			}
		}

		private Vector2 DetermineSmallestButtonSize()
		{
			Vector2 smallestButtonSize = new Vector2(float.MaxValue, float.MaxValue);

			int length = characterRows.Length;
			for(int i = 0; i < length; i++)
			{
				Vector2 boundsSize = characterRows[i].rect.size;
				float buttonWidth = ((boundsSize.x - xSpacing) / mainPageValues[i].Length) - xSpacing;
				float buttonHeight = boundsSize.y - ySpacing;

				smallestButtonSize.x = Mathf.Min(buttonWidth, smallestButtonSize.x);
				smallestButtonSize.y = Mathf.Min(buttonHeight, smallestButtonSize.y);
			}

			return smallestButtonSize;
		}

		private Button CreateCharacterButton(Transform parentTransform)
		{
			Button characterButton = Instantiate(characterButtonPrefab);
			RectTransform rectTransform = characterButton.GetComponent<RectTransform>();
			Vector2 size = rectTransform.sizeDelta;
			rectTransform.SetParent(parentTransform);
			rectTransform.localScale = Vector3.one;
			rectTransform.localRotation = Quaternion.identity;
			rectTransform.localPosition = Vector3.zero;
			rectTransform.sizeDelta = size;

			return characterButton;
		}

		private void LoadMainPage()
		{
			int length = characterRows.Length;
			for(int i = 0; i < length; i++)
			{
				UpdateCharacterRow(characterRows[i], mainPageValues[i]);
			}

			Image iconRenderer = shiftButton.transform.Find("Icon").GetComponent<Image>();
			Text label = shiftButton.transform.Find("Label").GetComponent<Text>();
			iconRenderer.enabled = true;
			iconRenderer.color = DEFAULT_COLOR;
			label.enabled = false;
		}

		private void LoadSymbolsPage1()
		{
			int length = characterRows.Length;
			for(int i = 0; i < length; i++)
			{
				UpdateCharacterRow(characterRows[i], symbolPage1Values[i]);
			}

			Image iconRenderer = shiftButton.transform.Find("Icon").GetComponent<Image>();
			Text label = shiftButton.transform.Find("Label").GetComponent<Text>();
			iconRenderer.enabled = false;
			label.enabled = true;
			label.text = "1/2";
		}

		private void LoadSymbolsPage2()
		{
			int length = characterRows.Length;
			for(int i = 0; i < length; i++)
			{
				UpdateCharacterRow(characterRows[i], symbolPage2Values[i]);
			}


			Image iconRenderer = shiftButton.transform.Find("Icon").GetComponent<Image>();
			Text label = shiftButton.transform.Find("Label").GetComponent<Text>();
			iconRenderer.enabled = false;
			label.enabled = true;
			label.text = "2/2";
		}

		private void UpdateCharacterRow(RectTransform characterRow, string characterRowValue)
		{
			int length = characterRow.childCount;
			for(int i = 0; i < length; i++)
			{
				if(i >= characterRowValue.Length) { break; }

				Button characterButton = characterRow.GetChild(i).GetComponent<Button>();
				Text label = characterButton.GetComponentInChildren<Text>();
				label.text = characterRowValue[i].ToString();
			}
		}

		private void UpdateMainPageCase()
		{
			int length = characterRows.Length;
			for(int i = 0; i < length; i++)
			{
				UpdateCharacterRowCase(characterRows[i]);
			}
		}

		private void UpdateCharacterRowCase(RectTransform characterRow)
		{
			int length = characterRow.childCount;
			for(int i = 0; i < length; i++)
			{
				Button characterButton = characterRow.GetChild(i).GetComponent<Button>();
				Text label = characterButton.GetComponentInChildren<Text>();
				if(uppercaseActive)
				{
					label.text = label.text.ToUpper();
				}
				else
				{
					label.text = label.text.ToLower();
				}
			}
		}

		public void OnCharacterButtonClick(Button characterButton)
		{
			Text label = characterButton.GetComponentInChildren<Text>();
			string input = label.text;

			Insert(input);
		}

		public void Insert(string input)
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

			if(uppercaseActive)
			{
				OnShiftClick();
			}
		}

		public void ApplyCharacterLimit(ref string text, ref int caretPosition)
		{
			if(characterLimit != 0 && text.Length > characterLimit)
			{
				text = text.Substring(0, characterLimit);
				caretPosition = Mathf.Clamp(caretPosition, 0, text.Length);
			}
		}

		public void OnShiftClick()
		{
			if(mainPageActive)
			{
				Image iconRenderer = shiftButton.transform.Find("Icon").GetComponent<Image>();
				iconRenderer.enabled = true;
				uppercaseActive = !uppercaseActive;
				if(uppercaseActive)
				{
					iconRenderer.color = ACTIVE_COLOR;
				}
				else
				{
					iconRenderer.color = DEFAULT_COLOR;
				}
				UpdateMainPageCase();
			}
			else
			{
				if(pageNr == 1)
				{
					pageNr = 2;
					LoadSymbolsPage2();
				}
				else
				{
					pageNr = 1;
					LoadSymbolsPage1();
				}
			}
		}

		public void OnBackspaceClick()
		{
			OnSpecialKeyPressed(SpecialKeyCode.BACKSPACE);
			DeletePreviousChar();

			if(uppercaseActive)
			{
				OnShiftClick();
			}
		}

		public void OnDeleteClick()
		{
			DeleteNextChar();

			if(uppercaseActive)
			{
				OnShiftClick();
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

		public void OnSymbolsClick()
		{
			mainPageActive = !mainPageActive;
			pageNr = 1;

			if(mainPageActive)
			{
				ConfigureCharacterRows(mainPageValues);
				LoadMainPage();
				uppercaseActive = false;
				UpdateMainPageCase();
			}
			else
			{
				ConfigureCharacterRows(symbolPage1Values);
				LoadSymbolsPage1();
			}
		}

		public void OnCommaClick()
		{
			Insert(",");
		}

		public void OnSpaceClick()
		{
			Insert(" ");
		}

		public void OnDotClick()
		{
			Insert(".");
		}

		public void OnDoneClick()
		{
			if(lineType == LineType.SINGLE_LINE || lineType == LineType.MULTILINE_SUBMIT)
			{
				OnKeyboardDone();
			}
			else if(lineType == LineType.MULTILINE_NEWLINE)
			{
				Insert("\n");
			}
		}
	}
}