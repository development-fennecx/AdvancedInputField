// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	/// <summary>The base class for text navigation using the caret and selecting text</summary>
	public class TextNavigator
	{
		/// <summary>The min ActionBar horizontal offset from the sides</summary>
		private const float ACTION_BAR_MARGIN_X = 0.05f;

		private List<TextSelectionHandler> textSelectionHandlers;

		public RectTransform TextAreaTransform { get { return InputField.TextAreaTransform; } }

		public RectTransform TextContentTransform { get { return InputField.TextContentTransform; } }

		/// <summary>The main renderer of the text</summary>
		public TextRenderer TextRenderer { get { return InputField.TextRenderer; } }

		/// <summary>The text renderer for processed text</summary>
		public TextRenderer ProcessedTextRenderer { get { return InputField.ProcessedTextRenderer; } }

		/// <summary>The InputField</summary>
		internal InputFieldEngine Engine { get; private set; }

		/// <summary>The current time for caret blink</summary>
		protected float caretBlinkTime;

		/// <summary>Indicates if edit mode is enabled</summary>
		public bool EditMode { get; protected set; }

		/// <summary>The ActionBar</summary>
		public ActionBar ActionBar { get; set; }

		/// <summary>Keeps the ActionBar visible when showing ActionBar by press holding an empty input field</summary>
		public bool KeepActionBarVisible { get; set; }

		/// <summary>The main text string</summary>
		public string Text { get { return Engine.Text; } }

		/// <summary>The processed text string (for live processing)</summary>
		public string ProcessedText { get { return Engine.ProcessedText; } }

		/// <summary>The text currently being rendered</summary>
		public string RenderedText { get { return TextRenderer.Text; } }

		/// <summary>The processed text currently being rendered (for live processing)</summary>
		public string ProcessedRenderedText { get { return ProcessedTextRenderer.Text; } }

		/// <summary>The Canvas</summary>
		public Canvas Canvas { get { return InputField.Canvas; } }

		public AdvancedInputField InputField { get { return Engine.InputField; } }

		/// <summary>The character count of the rendered text</summary>
		public int CharacterCount
		{
			get { return TextRenderer.CharacterCount; }
		}

		/// <summary>The character count of the whole text</summary>
		public int TotalCharacterCount
		{
			get { return Engine.Text.Length; }
		}

		/// <summary>The line count</summary>
		public int LineCount
		{
			get { return TextRenderer.LineCount; }
		}

		/// <summary>Indicates whether to currrently block selection change events from being send to the native bindings</summary>
		public bool BlockNativeSelectionChange { get; set; }

		/// <summary>Initializes this class</summary>
		internal virtual void Initialize(InputFieldEngine engine)
		{
			Engine = engine;
			textSelectionHandlers = new List<TextSelectionHandler>();
		}

		internal virtual void OnCanvasScaleChanged(float canvasScaleFactor)
		{
			int length = textSelectionHandlers.Count;
			for(int i = 0; i < length; i++)
			{
				textSelectionHandlers[i].OnCanvasScaleChanged(canvasScaleFactor);
			}
		}

		public T GetTextSelectionHandler<T>() where T : TextSelectionHandler
		{
			int length = textSelectionHandlers.Count;
			for(int i = 0; i < length; i++)
			{
				T textSelectionHandler = textSelectionHandlers[i] as T;
				if(textSelectionHandler != null)
				{
					return textSelectionHandler;
				}
			}

			return null;
		}

		/// <summary>Gets the character index from position</summary>
		/// <param name="position">The position to use</param>
		internal int GetCharacterIndexFromPosition(TextRenderer textRenderer, Vector2 position)
		{
			position.x -= TextContentTransform.anchoredPosition.x;

			TextAlignment alignment = textRenderer.TextAlignment;
			if(alignment == TextAlignment.BOTTOM || alignment == TextAlignment.CENTER || alignment == TextAlignment.TOP)
			{
				position.x -= (TextAreaTransform.rect.width * 0.5f);
				position.x += (TextContentTransform.rect.width * 0.5f);
			}
			else if(alignment == TextAlignment.BOTTOM_RIGHT || alignment == TextAlignment.RIGHT || alignment == TextAlignment.TOP_RIGHT)
			{
				position.x -= TextAreaTransform.rect.width;
				position.x += TextContentTransform.rect.width;
			}

			if(alignment == TextAlignment.LEFT || alignment == TextAlignment.CENTER || alignment == TextAlignment.RIGHT)
			{
				position.y += (TextAreaTransform.rect.height * 0.5f);
				position.y -= (TextContentTransform.rect.height * 0.5f);
			}
			else if(alignment == TextAlignment.BOTTOM_LEFT || alignment == TextAlignment.BOTTOM || alignment == TextAlignment.BOTTOM_RIGHT)
			{
				position.y += (TextAreaTransform.rect.height * 1f);
				position.y -= (TextContentTransform.rect.height * 1f);
			}

			if(textRenderer.LineCount == 0)
			{
				return 0;
			}

			int line = GetUnclampedCharacterLineFromPosition(textRenderer, position);
			if(line < 0)
			{
				return 0;
			}
			else if(line >= textRenderer.LineCount)
			{
				return textRenderer.CharacterCountVisible;
			}

			int startCharIndex = textRenderer.GetLineInfo(line).startCharIdx;
			int endCharIndex = GetLineEndCharIndex(textRenderer, line);

			for(int i = startCharIndex; i <= endCharIndex; i++)
			{
				if(i >= textRenderer.CharacterCountVisible)
				{
					break;
				}

				CharacterInfo charInfo = textRenderer.GetCharacterInfo(i);

				float distToCharStart = position.x - charInfo.position.x;
				float distToCharEnd = charInfo.position.x + charInfo.width - position.x;
				if(distToCharStart < distToCharEnd)
				{
					return i;
				}
			}

			return endCharIndex;
		}

		/// <summary>Gets the unclamped character line from position</summary>
		/// <param name="position">The position to use</param>
		/// <param name="textGenerator">The text generator to use</param>
		internal int GetUnclampedCharacterLineFromPosition(TextRenderer textRenderer, Vector2 position)
		{
			position.y -= TextContentTransform.anchoredPosition.y;
			if(!InputField.Multiline)
			{
				return 0;
			}

			float y = position.y;
			float lastBottomY = 0.0f;

			for(int i = 0; i < textRenderer.LineCount; ++i)
			{
				LineInfo lineInfo = textRenderer.GetLineInfo(i);
				float topY = lineInfo.topY;
				float bottomY = topY - lineInfo.height;

				if(y > topY)
				{
					float leading = topY - lastBottomY;
					if(y > topY - (0.5f * leading))
					{
						return i - 1;
					}
					else
					{
						return i;
					}
				}

				if(y > bottomY)
				{
					return i;
				}

				lastBottomY = bottomY;
			}

			return textRenderer.LineCount;
		}

		/// <summary>Gets the character index  of the line start</summary>
		/// <param name="line">The line to check</param>
		internal int GetLineStartCharIndex(TextRenderer textRenderer, int line)
		{
			line = Mathf.Clamp(line, 0, textRenderer.LineCount - 1);
			return textRenderer.GetLineInfo(line).startCharIdx;
		}

		/// <summary>Gets the character index  of the line end</summary>
		/// <param name="line">The line to check</param>
		internal int GetLineEndCharIndex(TextRenderer textRenderer, int line)
		{
			line = Mathf.Clamp(line, 0, textRenderer.LineCount - 1);
			return textRenderer.GetLineEndCharIndex(line);
		}

		internal virtual void OnUpdate()
		{
			int length = textSelectionHandlers.Count;
			for(int i = 0; i < length; i++)
			{
				textSelectionHandlers[i].OnUpdate();
			}
		}

		internal virtual void UpdateRendering(bool canHideActionBar = true)
		{
			int length = textSelectionHandlers.Count;
			for(int i = 0; i < length; i++)
			{
				textSelectionHandlers[i].OnSelectionUpdate(Engine.SelectionStartPosition, Engine.SelectionEndPosition);
			}

			if(InputField.ActionBarEnabled && InputField.ActionBar != null)
			{
				UpdateSelectionCursorsActionBar(canHideActionBar);
			}
		}

		/// <summary>Begins Edit mode</summary>
		internal virtual void BeginEditMode()
		{
			if(!EditMode)
			{
				EditMode = true;
				caretBlinkTime = 0;
				CleanupTextSelectionHandlers();

				BasicTextSelectionHandler basicTextSelectionHandler = Object.Instantiate(Settings.BasicTextSelectionPrefab);
				basicTextSelectionHandler.gameObject.SetActive(true);
				basicTextSelectionHandler.Setup(Engine.SelectionTransform, this);
				textSelectionHandlers.Add(basicTextSelectionHandler);

				if(InputField.CanUseTouchSelectionCursors || InputField.CanUseActionBar)
				{
					TouchTextSelectionHandler touchTextSelectionHandler = Object.Instantiate(Settings.TouchTextSelectionPrefab);
					touchTextSelectionHandler.gameObject.SetActive(true);
					touchTextSelectionHandler.Setup(Engine.SelectionTransform, this);
					textSelectionHandlers.Add(touchTextSelectionHandler);
				}

				ScrollArea scrollArea = TextAreaTransform.GetComponent<ScrollArea>();
				scrollArea.EditMode = true;

				if(Canvas != null)
				{
					OnCanvasScaleChanged(Canvas.scaleFactor);
				}
			}
		}

		/// <summary>Ends Edit mode</summary>
		internal virtual void EndEditMode()
		{
			EditMode = false;
			caretBlinkTime = InputField.CaretBlinkRate;
			Engine.SetSelection(0, 0);

			ScrollArea scrollArea = TextAreaTransform.GetComponent<ScrollArea>();
			BasicTextSelectionHandler basicTextSelectionHandler = GetTextSelectionHandler<BasicTextSelectionHandler>();
			if(basicTextSelectionHandler != null)
			{
				switch(InputField.ScrollBehaviourOnEndEdit)
				{
					case ScrollBehaviourOnEndEdit.START_OF_TEXT:
						Engine.SetSelection(0, 0);
						basicTextSelectionHandler.UpdateCaret(true);
						break;
					case ScrollBehaviourOnEndEdit.END_OF_TEXT:
						Engine.SetSelection(Text.Length, Text.Length);
						basicTextSelectionHandler.UpdateCaret(true);
						Engine.SetSelection(0, 0);
						break;
				}
			}
			scrollArea.EditMode = false;

			CleanupTextSelectionHandlers();
		}

		internal void CleanupTextSelectionHandlers()
		{
			int length = textSelectionHandlers.Count;
			for(int i = 0; i < length; i++)
			{
				Object.Destroy(textSelectionHandlers[i].gameObject);
			}
			textSelectionHandlers.Clear();

			TextSelectionHandler[] otherHandlers = InputField.TextRenderer.transform.GetComponentsInChildren<TextSelectionHandler>(); //Extra check to make sure all handlers are destroyed
			length = otherHandlers.Length;
			for(int i = 0; i < length; i++)
			{
				if(InputField.WithinAwake)
				{
					Object.DestroyImmediate(otherHandlers[i].gameObject);
				}
				else
				{
					Object.Destroy(otherHandlers[i].gameObject);
				}
			}
		}

		/// <summary>Moves caret to start of the text</summary>
		internal void MoveToStart()
		{
			Engine.SetSelection(0, 0);
		}

		/// <summary>Moves caret to end of the text</summary>
		internal void MoveToEnd()
		{
			int length = Text.Length;
			Engine.SetSelection(length, length);
		}

		/// <summary>Select current word at caret position</summary>
		internal virtual void SelectCurrentWord()
		{
			string renderedText;
			if(InputField.LiveDecoration)
			{
				renderedText = ProcessedRenderedText;
			}
			else
			{
				renderedText = RenderedText;
			}

			Util.DetermineCurrentWordRegion(renderedText, Engine.VisibleCaretPosition, out int startPosition, out int endPosition);
			Engine.SetVisibleSelection(startPosition, endPosition);
		}

		/// <summary>Selects all text</summary>
		internal virtual void SelectAll()
		{
			TextEditFrame textEditFrame = new TextEditFrame(Engine.Text, 0, Text.Length);
			Engine.ApplyTextEditFrame(textEditFrame);
		}

		/// <summary>Resets the caret based on position</summary>
		/// <param name="position">The position to check</param>
		internal virtual void ResetCaret(Vector2 position)
		{
			int visibleCaretPosition = DetermineVisibleCaretPosition(position);
			Engine.SetVisibleSelection(visibleCaretPosition, visibleCaretPosition);
		}

		/// <summary>Determine visible caret position based on given UI position</summary>
		/// <param name="position">The position to check</param>
		internal int DetermineVisibleCaretPosition(Vector2 position)
		{
			string text = Engine.Text;
			if(InputField.ShouldUseRichText)
			{
				text = InputField.RichText;
			}

			if(CharacterCount > text.Length + 1) //Text hasn't been updated yet, probably was disabled
			{
				return 0;
			}
			else
			{
				if(Engine.GetActiveTextRenderer() == InputField.PlaceholderTextRenderer)
				{
					return 0;
				}
				else if(InputField.LiveDecoration)
				{
					return GetCharacterIndexFromPosition(ProcessedTextRenderer, position);
				}
				else
				{
					return GetCharacterIndexFromPosition(TextRenderer, position);
				}
			}
		}

		/// <summary>Updates the selection area when dragging</summary>
		/// <param name="currentPosition">The current position</param>
		/// <param name="pressPosition">The position when press started</param>
		/// <param name="autoSelectWord">Indicates whether current word should be selected automatically</param>
		internal virtual void UpdateSelectionArea(int currentPosition, int pressPosition)
		{
			if(currentPosition < pressPosition)
			{
				Engine.SetVisibleSelection(currentPosition, pressPosition, true);
			}
			else if(currentPosition > pressPosition)
			{
				Engine.SetVisibleSelection(pressPosition, currentPosition, false);
			}
			else
			{
				Engine.SetVisibleSelection(currentPosition, currentPosition, false);
			}
		}

		internal void ToggleActionBar()
		{
			if(ActionBar.Visible)
			{
				HideActionBar();
			}
			else
			{
				ShowActionBar();
			}
		}

		internal void UpdateSelectionCursorsActionBar(bool canHideActionBar = true)
		{
			if(Engine.SelectionEndPosition > Engine.SelectionStartPosition)
			{
				ActionBar.transform.SetParent(InputField.transform);
				ActionBar.transform.localScale = Vector3.one;
				ActionBar.CheckInputFieldScale();
				bool cut = !InputField.Secure && !InputField.ReadOnly && InputField.ActionBarCut;
				bool copy = !InputField.Secure && InputField.ActionBarCopy;
				bool paste = !InputField.ReadOnly && InputField.ActionBarPaste;
				bool selectAll = InputField.ActionBarSelectAll;

#if !(UNITY_STANDALONE || UNITY_WSA)
				ActionBar.Show(cut, copy, paste, selectAll);
#endif

				UpdateActionBarPosition();
			}
			else
			{
#if !(UNITY_STANDALONE || UNITY_WSA)
				if(KeepActionBarVisible)
				{
					UpdateActionBarPosition();
					ActionBar.UpdateButtons();
				}
				else if(canHideActionBar)
				{
					ActionBar.Hide();
				}

#endif
			}
		}

		internal void HideActionBar()
		{
			ActionBar.Hide();
		}

		internal void ShowActionBar()
		{
			bool cut = !InputField.Secure && !InputField.ReadOnly && InputField.ActionBarCut && Engine.HasSelection;
			bool copy = !InputField.Secure && InputField.ActionBarCopy && Engine.HasSelection;
			bool paste = !InputField.ReadOnly && InputField.ActionBarPaste;
			bool selectAll = InputField.ActionBarSelectAll && Engine.Text.Length > 0;
			ActionBar.Show(cut, copy, paste, selectAll);
			UpdateActionBarPosition();
		}

		internal void UpdateActionBarPosition()
		{
			int maxCharIndex = Mathf.Max(TextRenderer.CharacterCount - 1, 0);
			Vector2 actionBarPosition;

			if(Engine.SelectionEndPosition > Engine.SelectionStartPosition)
			{
				int startCharIndex = Mathf.Clamp(Engine.VisibleSelectionStartPosition, 0, maxCharIndex);
				int endCharIndex = Mathf.Clamp(Engine.VisibleSelectionEndPosition, 0, maxCharIndex);
				Vector3 startPosition = TextRenderer.GetCharacterInfo(startCharIndex).position;
				Vector3 endPosition = TextRenderer.GetCharacterInfo(endCharIndex).position;

				actionBarPosition = startPosition;
				actionBarPosition.x = (startPosition.x + endPosition.x) / 2f;
			}
			else
			{
				int charIndex = Mathf.Clamp(Engine.VisibleCaretPosition, 0, maxCharIndex);
				actionBarPosition = TextRenderer.GetCharacterInfo(charIndex).position;
			}

			actionBarPosition += TextContentTransform.anchoredPosition;
			actionBarPosition.x -= (TextAreaTransform.rect.width * 0.5f);

			TextAlignment alignment = TextRenderer.TextAlignment;
			if(alignment == TextAlignment.BOTTOM || alignment == TextAlignment.CENTER || alignment == TextAlignment.TOP)
			{
				actionBarPosition.x += (TextAreaTransform.rect.width * 0.5f);
				actionBarPosition.x -= (TextContentTransform.rect.width * 0.5f);
			}
			else if(alignment == TextAlignment.BOTTOM_RIGHT || alignment == TextAlignment.RIGHT || alignment == TextAlignment.TOP_RIGHT)
			{
				actionBarPosition.x += TextAreaTransform.rect.width;
				actionBarPosition.x -= TextContentTransform.rect.width;
			}

			if(alignment == TextAlignment.LEFT || alignment == TextAlignment.CENTER || alignment == TextAlignment.RIGHT)
			{
				actionBarPosition.y -= (TextAreaTransform.rect.height * 0.5f);
				actionBarPosition.y += (TextContentTransform.rect.height * 0.5f);
			}
			else if(alignment == TextAlignment.BOTTOM_LEFT || alignment == TextAlignment.BOTTOM || alignment == TextAlignment.BOTTOM_RIGHT)
			{
				actionBarPosition.y -= (TextAreaTransform.rect.height * 1f);
				actionBarPosition.y += (TextContentTransform.rect.height * 1f);
			}

			actionBarPosition.y = Mathf.Min(actionBarPosition.y, 0);
			ActionBar.UpdatePosition(actionBarPosition);

			KeepActionBarWithinBounds();
		}

		internal void KeepActionBarWithinBounds()
		{
			Camera camera = Canvas.worldCamera;
			RectTransform rectTransform = ActionBar.RectTransform;
			Vector3[] corners = new Vector3[4]; //BottomLeft, TopLeft, TopRight, BottomRight
			rectTransform.GetWorldCorners(corners);
			Vector2 screenBottomLeft = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
			Vector2 screenTopRight = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);

			Vector3[] canvasCorners = new Vector3[4]; //BottomLeft, TopLeft, TopRight, BottomRight
			RectTransform canvasTransform = Canvas.GetComponent<RectTransform>();
			canvasTransform.GetWorldCorners(canvasCorners);
			Vector2 canvasScreenBottomLeft = RectTransformUtility.WorldToScreenPoint(camera, canvasCorners[0]);
			Vector2 canvasScreenTopRight = RectTransformUtility.WorldToScreenPoint(camera, canvasCorners[2]);
			Vector2 canvasScreenSize = canvasScreenTopRight - canvasScreenBottomLeft;

			float normalizedLeftX = (screenBottomLeft.x / canvasScreenSize.x);
			float normalizedRightX = (screenTopRight.x / canvasScreenSize.x);
			float normalizedTopY = (screenTopRight.y / canvasScreenSize.y);

			float normalizedNotchArea = 0;
			if(Screen.safeArea.yMax > 0)
			{
				normalizedNotchArea = (Screen.height - Screen.safeArea.yMax) / (float)Screen.height;
			}

			if(normalizedTopY + normalizedNotchArea > 1) //Out of bounds, move to bottom of InputField
			{
				Vector2 actionBarPosition = rectTransform.anchoredPosition;
				actionBarPosition.y -= (InputField.Size.y + ActionBar.RectTransform.rect.height);
				ActionBar.UpdatePosition(actionBarPosition);
			}

			if(normalizedLeftX < ACTION_BAR_MARGIN_X)
			{
				Vector2 actionBarPosition = rectTransform.anchoredPosition;
				actionBarPosition.x += (ACTION_BAR_MARGIN_X - normalizedLeftX) * (Canvas.pixelRect.width / Canvas.scaleFactor);
				ActionBar.UpdatePosition(actionBarPosition);
			}
			else if(normalizedRightX > 1 - ACTION_BAR_MARGIN_X)
			{
				Vector2 actionBarPosition = rectTransform.anchoredPosition;
				actionBarPosition.x += ((1 - ACTION_BAR_MARGIN_X) - normalizedRightX) * (Canvas.pixelRect.width / Canvas.scaleFactor);
				ActionBar.UpdatePosition(actionBarPosition);
			}
		}

		public void UpdateSelectionStart(Vector2 position, out Vector2 cursorPosition, out bool switchToEnd)
		{
			TextRenderer activeTextRenderer = Engine.GetActiveTextRenderer();
			int charIndex = GetCharacterIndexFromPosition(activeTextRenderer, position);
			if(charIndex <= Engine.VisibleSelectionEndPosition)
			{
				Engine.SetVisibleSelection(charIndex, Engine.VisibleSelectionEndPosition, true);
				switchToEnd = false;
			}
			else
			{
				Engine.SetVisibleSelection(Engine.VisibleSelectionEndPosition, charIndex, false);
				switchToEnd = true;
			}

			CharacterInfo charInfo = activeTextRenderer.GetCharacterInfo(charIndex);
			int lineIndex = activeTextRenderer.DetermineCharacterLine(charIndex);
			LineInfo lineInfo = activeTextRenderer.GetLineInfo(lineIndex);

			cursorPosition = new Vector2(charInfo.position.x, lineInfo.topY - lineInfo.height);
		}

		public void UpdateSelectionEnd(Vector2 position, out Vector2 cursorPosition, out bool switchToStart)
		{
			TextRenderer activeTextRenderer = Engine.GetActiveTextRenderer();
			int charIndex = GetCharacterIndexFromPosition(activeTextRenderer, position);
			if(charIndex >= Engine.VisibleSelectionStartPosition)
			{
				Engine.SetVisibleSelection(Engine.VisibleSelectionStartPosition, charIndex, false);
				switchToStart = false;
			}
			else
			{
				Engine.SetVisibleSelection(charIndex, Engine.VisibleSelectionStartPosition, true);
				switchToStart = true;
			}

			CharacterInfo charInfo = activeTextRenderer.GetCharacterInfo(charIndex);
			int lineIndex = activeTextRenderer.DetermineCharacterLine(charIndex);
			LineInfo lineInfo = activeTextRenderer.GetLineInfo(lineIndex);

			cursorPosition = new Vector2(charInfo.position.x, lineInfo.topY - lineInfo.height);
		}

		public void UpdateCurrentCursor(Vector2 position, out Vector2 cursorPosition)
		{
			TextRenderer activeTextRenderer = Engine.GetActiveTextRenderer();
			int charIndex = GetCharacterIndexFromPosition(activeTextRenderer, position);
			Engine.SetVisibleSelection(charIndex, charIndex);

			CharacterInfo charInfo = activeTextRenderer.GetCharacterInfo(charIndex);
			int lineIndex = activeTextRenderer.DetermineCharacterLine(charIndex);
			LineInfo lineInfo = activeTextRenderer.GetLineInfo(lineIndex);

			cursorPosition = new Vector2(charInfo.position.x, lineInfo.topY - lineInfo.height);
		}

		/// <summary>Handles the left arrow key</summary>
		/// <param name="shift">Indicates if shift is pressed</param>
		/// <param name="ctrl">Indicates if ctrl is pressed</param>
		internal void MoveLeft(bool shift, bool ctrl)
		{
			if(ctrl)
			{
				MoveCtrlLeft();
			}
			else if(shift)
			{
				MoveShiftLeft();
			}
			else
			{
				MoveLeft();
			}
		}

		/// <summary>Handles the left arrow + ctrl key combination</summary>
		private void MoveCtrlLeft()
		{
			int result = Util.FindPreviousWordStart(Text, Engine.SelectionStartPosition);
			TextEditFrame textEditFrame = new TextEditFrame(Engine.Text, result, result);
			Engine.ApplyTextEditFrame(textEditFrame);
			Engine.CaretIsStart = true;
		}

		/// <summary>Handles the left arrow + shift key combination</summary>
		private void MoveShiftLeft()
		{
			if(Engine.HasSelection)
			{
				if(Engine.CaretIsStart)
				{
					int result = GetPreviousCharacterIndex();
					TextEditFrame textEditFrame = new TextEditFrame(Engine.Text, result, Engine.SelectionEndPosition);
					Engine.ApplyTextEditFrame(textEditFrame);
					Engine.CaretIsStart = true;
				}
				else
				{
					int result = GetPreviousCharacterIndex();
					TextEditFrame textEditFrame = new TextEditFrame(Engine.Text, Engine.SelectionStartPosition, result);
					Engine.ApplyTextEditFrame(textEditFrame);
					Engine.CaretIsStart = false;
				}
			}
			else
			{
				int result = GetPreviousCharacterIndex();
				TextEditFrame textEditFrame = new TextEditFrame(Engine.Text, result, Engine.SelectionEndPosition);
				Engine.ApplyTextEditFrame(textEditFrame);
				Engine.CaretIsStart = true;
			}
		}

		/// <summary>Handles the left arrow (without ctrl and shift)</summary>
		private void MoveLeft()
		{
			if(Engine.HasSelection)
			{
				Engine.SetVisibleSelection(Engine.VisibleCaretPosition, Engine.VisibleCaretPosition);
			}
			else
			{
				int result = GetPreviousCharacterIndex();
				TextEditFrame textEditFrame = new TextEditFrame(Engine.Text, result, result);
				Engine.ApplyTextEditFrame(textEditFrame);
			}
		}

		internal int GetPreviousCharacterIndex()
		{
			int amount = 1;
			if(InputField.EmojisAllowed)
			{
				EmojiData emojiData;
				if(NativeKeyboardManager.EmojiEngine.TryFindPreviousEmojiInText(Text, Engine.CaretPosition - 1, out emojiData))
				{
					amount = emojiData.text.Length;
				}
			}
			return Mathf.Max(0, Engine.CaretPosition - amount);
		}

		/// <summary>Handles the right arrow key</summary>
		/// <param name="shift">Indicates if shift is pressed</param>
		/// <param name="ctrl">Indicates if ctrl is pressed</param>
		internal void MoveRight(bool shift, bool ctrl)
		{
			if(ctrl)
			{
				MoveCtrlRight();
			}
			else if(shift)
			{
				MoveShiftRight();
			}
			else
			{
				MoveRight();
			}
		}

		/// <summary>Handles the right arrow + ctrl key combination</summary>
		private void MoveCtrlRight()
		{
			int result = Util.FindNextWordStart(Text, Engine.SelectionEndPosition);
			TextEditFrame textEditFrame = new TextEditFrame(Engine.Text, result, result);
			Engine.ApplyTextEditFrame(textEditFrame);
			Engine.CaretIsStart = false;
		}

		/// <summary>Handles the right arrow + shift key combination</summary>
		private void MoveShiftRight()
		{
			if(Engine.HasSelection)
			{
				if(Engine.CaretIsStart)
				{
					int result = GetNextCharacterIndex();
					TextEditFrame textEditFrame = new TextEditFrame(Engine.Text, result, Engine.SelectionEndPosition);
					Engine.ApplyTextEditFrame(textEditFrame);
					Engine.CaretIsStart = true;
				}
				else
				{
					int result = GetNextCharacterIndex();
					TextEditFrame textEditFrame = new TextEditFrame(Engine.Text, Engine.SelectionStartPosition, result);
					Engine.ApplyTextEditFrame(textEditFrame);
					Engine.CaretIsStart = false;
				}
			}
			else
			{
				int result = GetNextCharacterIndex();
				TextEditFrame textEditFrame = new TextEditFrame(Engine.Text, Engine.SelectionStartPosition, result);
				Engine.ApplyTextEditFrame(textEditFrame);
				Engine.CaretIsStart = false;
			}
		}

		/// <summary>Handles the right arrow (without ctrl and shift)</summary>
		private void MoveRight()
		{
			if(Engine.HasSelection)
			{
				Engine.SetVisibleSelection(Engine.VisibleCaretPosition, Engine.VisibleCaretPosition);
			}
			else
			{
				int result = GetNextCharacterIndex();
				TextEditFrame textEditFrame = new TextEditFrame(Engine.Text, result, result);
				Engine.ApplyTextEditFrame(textEditFrame);
			}
		}

		internal int GetNextCharacterIndex()
		{
			int amount = 1;
			if(InputField.EmojisAllowed)
			{
				EmojiData emojiData;
				if(NativeKeyboardManager.EmojiEngine.TryFindNextEmojiInText(Text, Engine.CaretPosition, out emojiData))
				{
					amount = emojiData.text.Length;
				}
			}
			return Mathf.Min(TotalCharacterCount, Engine.CaretPosition + amount);
		}

		/// <summary>Handles the down arrow key</summary>
		/// <param name="shift">Indicates if shift is pressed</param>
		/// <param name="ctrl">Indicates if ctrl is pressed</param>
		internal void MoveDown(bool shift, bool ctrl)
		{
			if(ctrl)
			{
				MoveCtrlDown();
			}
			else if(shift)
			{
				MoveShiftDown();
			}
			else
			{
				MoveDown();
			}
		}

		/// <summary>Handles the down arrow + ctrl key combination</summary>
		private void MoveCtrlDown()
		{
			int result = Util.NewLineDownPosition(Text, InputField.Multiline, Engine.CaretPosition);
			TextEditFrame textEditFrame = new TextEditFrame(Engine.Text, result, result);
			Engine.ApplyTextEditFrame(textEditFrame);
		}

		/// <summary>Handles the down arrow + shift key combination</summary>
		private void MoveShiftDown()
		{
			if(Engine.HasSelection)
			{
				if(Engine.CaretIsStart)
				{
					int result = GetLineDownCharacterIndex();
					if(result > Engine.SelectionEndPosition) //Invert
					{
						TextEditFrame textEditFrame = new TextEditFrame(Engine.Text, Engine.SelectionEndPosition, result);
						Engine.ApplyTextEditFrame(textEditFrame);
						Engine.CaretIsStart = false;
					}
					else
					{
						TextEditFrame textEditFrame = new TextEditFrame(Engine.Text, result, Engine.SelectionEndPosition);
						Engine.ApplyTextEditFrame(textEditFrame);
						Engine.CaretIsStart = true;
					}
				}
				else
				{
					int result = GetLineDownCharacterIndex();
					TextEditFrame textEditFrame = new TextEditFrame(Engine.Text, Engine.SelectionStartPosition, result);
					Engine.ApplyTextEditFrame(textEditFrame);
					Engine.CaretIsStart = false;
				}
			}
			else
			{
				int result = GetLineDownCharacterIndex();
				TextEditFrame textEditFrame = new TextEditFrame(Engine.Text, Engine.SelectionStartPosition, result);
				Engine.ApplyTextEditFrame(textEditFrame);
				Engine.CaretIsStart = false;
			}
		}

		/// <summary>Handles the down arrow (without ctrl and shift)</summary>
		private void MoveDown()
		{
			if(Engine.HasSelection)
			{
				Engine.SetVisibleSelection(Engine.VisibleCaretPosition, Engine.VisibleCaretPosition);
			}
			else
			{
				int result = GetLineDownCharacterIndex();
				TextEditFrame textEditFrame = new TextEditFrame(Engine.Text, result, result);
				Engine.ApplyTextEditFrame(textEditFrame);
			}
		}

		internal int GetLineDownCharacterIndex()
		{
			TextRenderer textRenderer = Engine.GetActiveTextRenderer();
			int result = textRenderer.LineDownPosition(Engine.VisibleCaretPosition, InputField.Multiline);
			if(InputField.ShouldUseRichText)
			{
				result = Engine.RichTextProcessor.DeterminePositionInText(result, InputField.Text);
			}

			return result;
		}

		/// <summary>Handles the up arrow key</summary>
		/// <param name="shift">Indicates if shift is pressed</param>
		/// <param name="ctrl">Indicates if ctrl is pressed</param>
		internal void MoveUp(bool shift, bool ctrl)
		{
			if(ctrl)
			{
				MoveCtrlUp();
			}
			else if(shift)
			{
				MoveShiftUp();
			}
			else
			{
				MoveUp();
			}
		}

		/// <summary>Handles the up arrow + ctrl key combination</summary>
		private void MoveCtrlUp()
		{
			int result = Util.NewLineUpPosition(Text, InputField.Multiline, Engine.CaretPosition);
			TextEditFrame textEditFrame = new TextEditFrame(Engine.Text, result, result);
			Engine.ApplyTextEditFrame(textEditFrame);
		}

		/// <summary>Handles the up arrow + shift key combination</summary>
		private void MoveShiftUp()
		{
			if(Engine.HasSelection)
			{
				if(Engine.CaretIsStart)
				{
					int result = GetLineUpCharacterIndex();
					TextEditFrame textEditFrame = new TextEditFrame(Engine.Text, result, Engine.SelectionEndPosition);
					Engine.ApplyTextEditFrame(textEditFrame);
					Engine.CaretIsStart = true;
				}
				else
				{
					int result = GetLineUpCharacterIndex();
					if(result < Engine.SelectionStartPosition) //Invert
					{
						TextEditFrame textEditFrame = new TextEditFrame(Engine.Text, result, Engine.SelectionStartPosition);
						Engine.ApplyTextEditFrame(textEditFrame);
						Engine.CaretIsStart = true;
					}
					else
					{
						TextEditFrame textEditFrame = new TextEditFrame(Engine.Text, Engine.SelectionStartPosition, result);
						Engine.ApplyTextEditFrame(textEditFrame);
						Engine.CaretIsStart = false;
					}
				}
			}
			else
			{
				int result = GetLineUpCharacterIndex();
				TextEditFrame textEditFrame = new TextEditFrame(Engine.Text, result, Engine.SelectionEndPosition);
				Engine.ApplyTextEditFrame(textEditFrame);
				Engine.CaretIsStart = true;
			}
		}

		/// <summary>Handles the up arrow (without ctrl and shift)</summary>
		private void MoveUp()
		{
			if(Engine.HasSelection)
			{
				Engine.SetVisibleSelection(Engine.VisibleCaretPosition, Engine.VisibleCaretPosition);
			}
			else
			{
				int result = GetLineUpCharacterIndex();
				TextEditFrame textEditFrame = new TextEditFrame(Engine.Text, result, result);
				Engine.ApplyTextEditFrame(textEditFrame);
			}
		}

		internal int GetLineUpCharacterIndex()
		{
			TextRenderer textRenderer = Engine.GetActiveTextRenderer();
			int result = textRenderer.LineUpPosition(Engine.VisibleCaretPosition, InputField.Multiline);
			if(InputField.ShouldUseRichText)
			{
				result = Engine.RichTextProcessor.DeterminePositionInText(result, InputField.Text);
			}

			return result;
		}
	}
}
