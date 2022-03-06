// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace AdvancedInputFieldPlugin
{
	public class InputFieldEngine
	{
		private InputFieldKeyboardClient keyboardClient;
		private InputFieldEventHandler eventHandler;
		private TextInputHandler textInputHandler;
		private TextNavigator textNavigator;
		private TextManipulator textManipulator;
		private RichTextProcessor richTextProcessor;

		private string text;

		private string placeholder;

		/// <summary>The text value for processed text (or the placeholder when text is empty)</summary>
		private string processedText;

		private string richText;

		/// <summary>The last known canvas scale factor</summary>
		private float lastCanvasScaleFactor;

		/// <summary>The press position</summary>
		private Vector2 pressPosition;

		/// <summary>The text content position when press started</summary>
		private Vector2 pressTextContentPosition;

		/// <summary>The start position on the drag (as character index)</summary>
		private int dragStartPosition;

		/// <summary>Indicates if input field is currently in edit mode</summary>
		private bool editMode;

		/// <summary>The reason for beginning edit mode</summary>
		private BeginEditReason beginEditReason;

		/// <summary>The reason for ending edit mode</summary>
		private EndEditReason endEditReason;

		/// <summary>The last text edit frame</summary>
		private TextEditFrame lastTextEditFrame;

		/// <summary>Indicates if input field is selected</summary>
		private bool selected;

		/// <summary>The last time this input field was selected</summary>
		private float lastTimeSelected;

		/// <summary>Indicates if drag position is out of bounds</summary>
		private bool dragOutOfBounds;

		/// <summary>Indicates if the drag state should keep updating (for drag out of bounds)</summary>
		private bool updateDrag;

		/// <summary>The offset to use when determining drag position</summary>
		private Vector2 dragOffset;

		/// <summary>The start position of the text selection</summary>
		private int selectionStartPosition;

		/// <summary>The start position of the processed text selection when using live processing filters</summary>
		private int processedSelectionStartPosition;

		/// <summary>The start position of the rich text selection</summary
		private int richTextSelectionStartPosition;

		/// <summary>The end position of the text selection</summary>
		private int selectionEndPosition;

		/// <summary>The end position of the processed text selection when using live processing filters</summary>
		private int processedSelectionEndPosition;

		/// <summary>The end position of the rich text selection</summary
		private int richTextSelectionEndPosition;

		/// <summary>The root transform for text selection</summary>
		private RectTransform selectionTransform;

		private bool textDirty;
		private bool selectionDirty;
		private bool hasModifiedTextAfterClick;
		private bool caretIsStart;
		private bool initialized;

		public AdvancedInputField InputField { get; private set; }

		/// <summary>Indicates if the user is currently pressing this input field</summary>
		public bool UserPressing { get; private set; }

		public RectTransform SelectionTransform { get { return selectionTransform; } }
		public string Text { get { return text; } }
		public string Placeholder { get { return placeholder; } }
		public bool HasSelection { get { return (selectionEndPosition > selectionStartPosition); } }
		public bool CaretIsStart { get { return caretIsStart; } set { caretIsStart = value; } }
		public bool Initialized { get { return initialized; } }

		/// <summary>The currently selected text</summary>
		public string SelectedText
		{
			get
			{
				if(HasSelection)
				{
					return text.Substring(selectionStartPosition, selectionEndPosition - selectionStartPosition);
				}
				return string.Empty;
			}
		}

		public ActionBar ActionBar { get { return textNavigator.ActionBar; } }
		public InputFieldEventHandler EventHandler { get { return eventHandler; } }
		public RichTextProcessor RichTextProcessor { get { return richTextProcessor; } }
		public InputFieldKeyboardClient KeyboardClient { get { return keyboardClient; } }

		public bool EditMode { get { return editMode; } }
		public bool HasModifiedTextAfterClick { get { return hasModifiedTextAfterClick; } set { hasModifiedTextAfterClick = value; } }

		/// <summary>The caret position</summary>
		public virtual int CaretPosition
		{
			get
			{
				if(caretIsStart)
				{
					return selectionStartPosition;
				}
				else
				{
					return selectionEndPosition;
				}
			}
		}

		/// <summary>The processed caret position (for live processing)</summary>
		public virtual int ProcessedCaretPosition
		{
			get
			{
				if(caretIsStart)
				{
					return processedSelectionStartPosition;
				}
				else
				{
					return processedSelectionEndPosition;
				}
			}
		}

		public virtual int RichTextCaretPosition
		{
			get
			{
				if(caretIsStart)
				{
					return richTextSelectionStartPosition;
				}
				else
				{
					return richTextSelectionEndPosition;
				}
			}
		}

		/// <summary>The caret position in currently rendered text</summary>
		public virtual int VisibleCaretPosition
		{
			get
			{
				if(InputField.ShouldUseRichText)
				{
					return RichTextCaretPosition;
				}
				else if(InputField.LiveDecoration)
				{
					return ProcessedCaretPosition;
				}
				else
				{
					return CaretPosition;
				}
			}
		}

		/// <summary>The start position of the text selection</summary>
		public virtual int SelectionStartPosition
		{
			get { return selectionStartPosition; }
		}

		/// <summary>The start position of the rich text selection</summary>
		public virtual int RichTextSelectionStartPosition
		{
			get { return richTextSelectionStartPosition; }
		}

		/// <summary>The start position of the processed text selection (for live processing)</summary>
		public virtual int ProcessedSelectionStartPosition
		{
			get { return processedSelectionStartPosition; }
		}

		/// <summary>The start position of the text selection in currently rendered text</summary>
		public virtual int VisibleSelectionStartPosition
		{
			get
			{
				if(InputField.ShouldUseRichText)
				{
					return RichTextSelectionStartPosition;
				}
				else if(InputField.LiveDecoration)
				{
					return ProcessedSelectionStartPosition;
				}
				else
				{
					return SelectionStartPosition;
				}
			}
		}

		/// <summary>The end position of the text selection</summary>
		public virtual int SelectionEndPosition
		{
			get { return selectionEndPosition; }
		}

		/// <summary>The end position of the rich text selection</summary>
		public virtual int RichTextSelectionEndPosition
		{
			get { return richTextSelectionEndPosition; }
		}

		/// <summary>The end position of the processed text selection (for live processing)</summary>
		public virtual int ProcessedSelectionEndPosition
		{
			get { return processedSelectionEndPosition; }
		}

		/// <summary>The end position of the text selection in currently rendered text</summary>
		public virtual int VisibleSelectionEndPosition
		{
			get
			{
				if(InputField.ShouldUseRichText)
				{
					return RichTextSelectionEndPosition;
				}
				else if(InputField.LiveDecoration)
				{
					return ProcessedSelectionEndPosition;
				}
				else
				{
					return SelectionEndPosition;
				}
			}
		}

		/// <summary>The text value for processed text</summary>
		internal string ProcessedText
		{
			get { return processedText; }
		}

		/// <summary>The rich text string</summary>
		public string RichText
		{
			get
			{
				return richText;
			}
		}

		public bool UsingTouchSelectionCursors
		{
			get
			{
				if((!Application.isEditor || Settings.SimulateMobileBehaviourInEditor) && InputField.CanUseTouchSelectionCursors)
				{
					TouchTextSelectionHandler touchTextSelectionHandler = textNavigator.GetTextSelectionHandler<TouchTextSelectionHandler>();
					if(touchTextSelectionHandler.StartCursor.Selected || touchTextSelectionHandler.EndCursor.Selected)
					{
						return true;
					}
				}

				return false;
			}
		}

		public MonoBehaviour ActiveBehaviour
		{
			get
			{
				if(NativeKeyboardManager.InstanceValid) //Normally this behaviour should always be active (except when closing application)
				{
					return NativeKeyboardManager.Instance;
				}

				return null;
			}
		}

		/// <summary>Indicates if input field is selected</summary>
		public bool Selected
		{
			get
			{
				return selected;
			}
		}

		public InputFieldEngine(AdvancedInputField inputField)
		{
			InputField = inputField;
		}

		public void Initialize()
		{
			this.text = InputField.Text;
			Canvas.ForceUpdateCanvases(); //Make sure the layout information is correct

			InitializeTextRenderers();
			InitializeComponents();

			if(InputField.LiveDecoration)
			{
				InitializeLiveProcessing();
			}

			if(InputField.ShouldUseRichText)
			{
				InitializeRichTextEditing(InputField.Text);
			}

			textNavigator.EndEditMode();
			textManipulator.EndEditMode();
			RefreshTextSelectionRenderOrder();
			RefreshRenderedText();

			ScrollArea scrollArea = InputField.ScrollArea;
			if(InputField.Multiline)
			{
				scrollArea.Horizontal = false;
				scrollArea.Vertical = true;
			}
			else
			{
				scrollArea.Horizontal = true;
				scrollArea.Vertical = false;
			}
			scrollArea.DragMode = InputField.DragMode;

			ActiveBehaviour?.StartCoroutine(InitialTextRendererFix());
			NativeKeyboardManager.AddHardwareKeyboardChangedListener(OnHardwareKeyboardChanged);

			initialized = true;
		}

		private IEnumerator InitialTextRendererFix()
		{
			UpdateActiveTextRenderer();
			yield return null;
			UpdateActiveTextRenderer();
		}

		internal void InitializeTextRenderers()
		{
			float caretWidth = InputField.CaretWidth;
			bool shouldUseRichText = InputField.ShouldUseRichText;
			bool multiline = InputField.Multiline;

			TextRenderer textRenderer = InputField.TextRenderer;
			textRenderer.CaretWidth = caretWidth;
			textRenderer.Multiline = multiline;
			textRenderer.RichTextEnabled = shouldUseRichText;
			textRenderer.ViewportTransform = InputField.TextAreaTransform;

			TextRenderer processedTextRenderer = InputField.ProcessedTextRenderer;
			processedTextRenderer.CaretWidth = caretWidth;
			processedTextRenderer.Multiline = multiline;
			processedTextRenderer.RichTextEnabled = shouldUseRichText;
			processedTextRenderer.ViewportTransform = InputField.TextAreaTransform;

			TextRenderer placeholderTextRenderer = InputField.PlaceholderTextRenderer;
			placeholderTextRenderer.CaretWidth = caretWidth;
			placeholderTextRenderer.Multiline = multiline;
			placeholderTextRenderer.RichTextEnabled = shouldUseRichText;
			placeholderTextRenderer.ViewportTransform = InputField.TextAreaTransform;

#if ADVANCEDINPUTFIELD_TEXTMESHPRO
			if(placeholderTextRenderer is TMProTextRenderer && string.IsNullOrEmpty(placeholderTextRenderer.Text))
			{
				placeholderTextRenderer.Text = " "; //Workaround for TextMeshPro character data issue with empty text
			}
#endif
		}

		private void InitializeComponents()
		{
			keyboardClient = InputField.gameObject.GetComponent<InputFieldKeyboardClient>();
			if(keyboardClient == null)
			{
				keyboardClient = InputField.gameObject.AddComponent<InputFieldKeyboardClient>();
			}
			keyboardClient.Initialize(this);
			eventHandler = new InputFieldEventHandler(this);
			textInputHandler = new TextInputHandler();
			textNavigator = new TextNavigator();
			textManipulator = new TextManipulator();
			textInputHandler.Initialize(this, textNavigator, textManipulator);
			textNavigator.Initialize(this);
			textManipulator.Initialize(this, textNavigator, InputField.TextRenderer, InputField.ProcessedTextRenderer);
		}

		internal void InitializeLiveProcessing()
		{
			string processedText = InputField.LiveDecorationFilter.ProcessText(Text, CaretPosition);
			if(processedText != null)
			{
				SetProcessedText(processedText);
			}
		}

		internal void InitializeRichTextEditing(string richText)
		{
			if(InputField.RichTextConfig != null)
			{
				richTextProcessor = new RichTextProcessor(InputField.RichTextConfig.GetSupportedTags(), InputField.EmojisAllowed, InputField.RichTextBindingsAllowed);
			}
			else
			{
				richTextProcessor = new RichTextProcessor(new RichTextTagInfo[0], InputField.EmojisAllowed, InputField.RichTextBindingsAllowed);
			}

			richTextProcessor.SetupRichText(richText);
			this.richText = richTextProcessor.LastRichTextEditFrame.text;
			this.text = richTextProcessor.LastTextEditFrame.text;
			InputField.UpdateTextProperty(this.text);
			textDirty = true;
		}

		internal void SetText(string text, bool invokeTextChangeEvent = true)
		{
			if(this.text != text)
			{
				this.text = text;
				textDirty = true;
				hasModifiedTextAfterClick = true;

				InputField.UpdateTextProperty(this.text);
				if(invokeTextChangeEvent)
				{
					eventHandler?.InvokeValueChanged(text);
				}

				if(!Application.isPlaying)
				{
					InputField.TextRenderer.Text = text;
					UpdateActiveTextRenderer();
				}
			}
		}

		internal void SetSelection(int selectionStartPosition, int selectionEndPosition, bool caretIsStart = false, bool invokeTextSelectionChangeEvent = true)
		{
			if(this.selectionStartPosition != selectionStartPosition || this.selectionEndPosition != selectionEndPosition)
			{
				this.selectionStartPosition = selectionStartPosition;
				this.selectionEndPosition = selectionEndPosition;
				selectionDirty = true;

				if(invokeTextSelectionChangeEvent)
				{
					eventHandler?.InvokeTextSelectionChanged(selectionStartPosition, selectionEndPosition);
				}
			}

			this.caretIsStart = caretIsStart;
			if(selectionDirty)
			{
				if(invokeTextSelectionChangeEvent)
				{
					eventHandler?.InvokeCaretPositionChanged(CaretPosition);
				}
			}
		}

		internal void SetProcessedText(string processedText)
		{
			if(this.processedText != processedText)
			{
				this.processedText = processedText;
				textDirty = true;
			}
		}

		internal void SetProcessedSelection(int processedSelectionStartPosition, int processedSelectionEndPosition, bool caretIsStart = false)
		{
			if(this.processedSelectionStartPosition != processedSelectionStartPosition || this.processedSelectionEndPosition != processedSelectionEndPosition)
			{
				this.processedSelectionStartPosition = processedSelectionStartPosition;
				this.processedSelectionEndPosition = processedSelectionEndPosition;
				this.selectionStartPosition = InputField.LiveDecorationFilter.DetermineCaret(Text, ProcessedText, processedSelectionStartPosition);
				this.selectionEndPosition = InputField.LiveDecorationFilter.DetermineCaret(Text, ProcessedText, processedSelectionEndPosition);
				selectionDirty = true;
			}

			this.caretIsStart = caretIsStart;
		}

		internal void SetRichText(string richText)
		{
			if(this.richText != richText)
			{
				this.richText = richText;
				textDirty = true;
				hasModifiedTextAfterClick = true;

				InputField.UpdateTextProperty(this.text);
			}
		}

		internal void SetRichTextSelection(int richTextSelectionStartPosition, int richTextSelectionEndPosition, bool caretIsStart = false)
		{
			if(this.richTextSelectionStartPosition != richTextSelectionStartPosition || this.richTextSelectionEndPosition != richTextSelectionEndPosition)
			{
				this.richTextSelectionStartPosition = richTextSelectionStartPosition;
				this.richTextSelectionEndPosition = richTextSelectionEndPosition;
				selectionDirty = true;
			}

			this.caretIsStart = caretIsStart;
		}

		internal void SetVisibleSelection(int visibleSelectionStartPosition, int visibleSelectionEndPosition, bool caretIsStart = false)
		{
			if(InputField.ShouldUseRichText)
			{
				SetRichTextSelection(visibleSelectionStartPosition, visibleSelectionEndPosition, caretIsStart);

				TextEditFrame richTextEditFrame = new TextEditFrame(richText, richTextSelectionStartPosition, richTextSelectionEndPosition);
				ApplyRichTextEditFrame(richTextEditFrame);

				this.caretIsStart = caretIsStart;
			}
			else if(InputField.LiveDecoration)
			{
				SetProcessedSelection(visibleSelectionStartPosition, visibleSelectionEndPosition, caretIsStart);
			}
			else
			{
				SetSelection(visibleSelectionStartPosition, visibleSelectionEndPosition, caretIsStart);
			}
		}

		internal void RefreshBasicTextSelectionHandlerAppearance()
		{
			BasicTextSelectionHandler basicTextSelectionHandler = textNavigator.GetTextSelectionHandler<BasicTextSelectionHandler>();
			if(basicTextSelectionHandler != null)
			{
				basicTextSelectionHandler.RefreshAppearance();
				selectionDirty = true;
			}
		}

		internal void RefreshTextSelectionRenderOrder()
		{
			if(selectionTransform == null)
			{
				selectionTransform = InputField.TextContentTransform.Find("Selection") as RectTransform;
				if(selectionTransform == null)
				{
					GameObject selectionObject = new GameObject("Selection");
					selectionObject.transform.SetParent(InputField.TextContentTransform);
					selectionTransform = selectionObject.AddComponent<RectTransform>();
					selectionTransform.localPosition = Vector3.zero;
					selectionTransform.anchorMin = new Vector2(0, 0);
					selectionTransform.anchorMax = new Vector2(1, 1);
					selectionTransform.pivot = new Vector2(0.5f, 0.5f);
					selectionTransform.offsetMin = new Vector2(0, 0);
					selectionTransform.offsetMax = new Vector2(0, 0);
					selectionTransform.localScale = Vector3.one;
					selectionTransform.localRotation = Quaternion.identity;
				}
			}

			if(InputField.SelectionBehindText)
			{
				selectionTransform.SetAsFirstSibling();
			}
			else
			{
				selectionTransform.SetAsLastSibling();
			}
		}

		/// <summary>Refreshes the rendered text</summary>
		internal void RefreshRenderedText()
		{
			TextRenderer textRenderer = InputField.TextRenderer;
			TextRenderer processedTextRenderer = InputField.ProcessedTextRenderer;

			if(InputField.ShouldUseRichText)
			{
				textRenderer.Text = richText;
			}
			else if(InputField.LiveDecoration || (!InputField.Selected && InputField.PostDecoration))
			{
				processedTextRenderer.Text = processedText;
			}
			else
			{
				string text;
				if(Text.Length > 0)
				{
					text = Text;

					if(InputField.Secure && !InputField.VisiblePassword)
					{
						text = new string(Settings.PasswordMaskingCharacter, text.Length);
					}
				}
				else
				{
					text = Text;
				}

				textRenderer.Text = text;
			}

			UpdateActiveTextRenderer();
		}

		internal void ProcessDone()
		{
			if(InputField.NextInputField != null)
			{
				if(!InputField.NextInputField.ReadOnly)
				{
					KeyboardClient.Keyboard.State = KeyboardState.HIDDEN; //Flag keyboard as inactive, so next inputfield will load it's settings
				}
				Deselect(EndEditReason.KEYBOARD_NEXT);
				InputField.NextInputField.ManualSelect(BeginEditReason.KEYBOARD_NEXT);
			}
			else
			{
				Deselect(EndEditReason.KEYBOARD_DONE);
			}
		}

		internal void MoveLeft(bool shift, bool ctrl)
		{
			textNavigator.MoveLeft(shift, ctrl);
		}

		internal void MoveRight(bool shift, bool ctrl)
		{
			textNavigator.MoveRight(shift, ctrl);
		}

		internal void MoveUp(bool shift, bool ctrl)
		{
			textNavigator.MoveUp(shift, ctrl);
		}

		internal void MoveDown(bool shift, bool ctrl)
		{
			textNavigator.MoveDown(shift, ctrl);
		}

		internal void OnEnable()
		{
			ScrollArea scrollArea = InputField.ScrollArea;
			scrollArea.OnValueChanged.AddListener(OnTextScrollChanged);
		}

		internal void OnDisable()
		{
			ScrollArea scrollArea = InputField.ScrollArea;
			scrollArea.OnValueChanged.RemoveListener(OnTextScrollChanged);

			if(!NativeKeyboardManager.InstanceValid) { return; }
			if(Selected)
			{
				Deselect(EndEditReason.PROGRAMMATIC_DESELECT);
				EndEditMode();
				DisableSelection();
			}
		}

		internal void OnDestroy()
		{
			NativeKeyboardManager.RemoveHardwareKeyboardChangedListener(OnHardwareKeyboardChanged);
		}

		internal void OnRectTransformDimensionsChange()
		{
			Canvas canvas = InputField.Canvas;
			if(Selected && canvas != null && lastCanvasScaleFactor != canvas.scaleFactor)
			{
				textInputHandler.OnCanvasScaleChanged(canvas.scaleFactor);
				textNavigator.OnCanvasScaleChanged(canvas.scaleFactor);

				lastCanvasScaleFactor = canvas.scaleFactor;
			}

			MarkDirty();
		}

		internal void OnApplicationPause(bool pause)
		{
			if(pause)
			{
				if(Time.realtimeSinceStartup - lastTimeSelected <= 1) //Check if this was inputfield was selected within last second
				{
					NativeKeyboardManager.ActiveInputFieldBeforePause = InputField;
				}
			}
		}

		internal void SetSize(Vector2 value)
		{
			RectTransform rectTransform = InputField.RectTransform;
			Vector2 size = rectTransform.rect.size;
			Vector2 sizeDifference = value - size;
			if(sizeDifference != Vector2.zero)
			{
				Vector2 sizeDelta = rectTransform.sizeDelta;
				sizeDelta += sizeDifference;
				rectTransform.sizeDelta = sizeDelta;

				eventHandler?.InvokeSizeChanged(value);
			}
		}

		/// <summary>Updates the Size based on given Text Renderer</summary>
		internal void UpdateSize(TextRenderer textRenderer)
		{
			RectTransform rectTransform = InputField.RectTransform;
			Vector2 size = InputField.Size;
			Vector2 preferredSize = textRenderer.PreferredSize;
			RectTransform textAreaTransform = InputField.TextAreaTransform;
			RectTransform textContentTransform = InputField.TextContentTransform;
			InputFieldMode mode = InputField.Mode;
			float resizeMinWidth = InputField.ResizeMinWidth;
			float resizeMaxWidth = InputField.ResizeMaxWidth;
			float resizeMinHeight = InputField.ResizeMinHeight;
			float resizeMaxHeight = InputField.ResizeMaxHeight;

			textContentTransform.sizeDelta = preferredSize;

			if(mode != InputFieldMode.SCROLL_TEXT)
			{
				switch(mode)
				{
					case InputFieldMode.HORIZONTAL_RESIZE_FIT_TEXT:
						float marginX = (rectTransform.rect.width - textAreaTransform.rect.width);
						size.x = Mathf.Max(preferredSize.x, resizeMinWidth - marginX);
						if(resizeMaxWidth > 0)
						{
							ScrollArea scrollArea = textAreaTransform.GetComponent<ScrollArea>();
							if(size.x > resizeMaxWidth - marginX)
							{
								size.x = resizeMaxWidth - marginX;
								scrollArea.enabled = true;
							}
							else
							{
								if(scrollArea.enabled)
								{
									scrollArea.MoveContentImmediately(Vector2.zero);
								}
								scrollArea.enabled = false;
							}
						}
						size.x += marginX;
						InputField.Size = size;
						break;
					case InputFieldMode.VERTICAL_RESIZE_FIT_TEXT:
						float marginY = (rectTransform.rect.height - textAreaTransform.rect.height);
						size.y = Mathf.Max(preferredSize.y, resizeMinHeight - marginY);
						if(resizeMaxHeight > 0)
						{
							ScrollArea scrollArea = textAreaTransform.GetComponent<ScrollArea>();
							if(size.y > resizeMaxHeight - marginY)
							{
								size.y = resizeMaxHeight - marginY;
								scrollArea.enabled = true;
							}
							else
							{
								if(scrollArea.enabled)
								{
									scrollArea.MoveContentImmediately(Vector2.zero);
								}
								scrollArea.enabled = false;
							}
						}
						size.y += marginY;
						InputField.Size = size;
						break;
				}
			}
		}

		/// <summary>Updates which Text Renderer that should be visible and which should be hidden</summary>
		internal void UpdateVisibleTextRenderers()
		{
			bool shouldUseRichText = InputField.ShouldUseRichText;
			bool liveProcessing = InputField.LiveDecoration;
			bool postProcessing = InputField.PostDecoration;
			TextRenderer textRenderer = InputField.TextRenderer;
			TextRenderer placeholderTextRenderer = InputField.PlaceholderTextRenderer;
			TextRenderer processedTextRenderer = InputField.ProcessedTextRenderer;
			string richText = InputField.RichText;
			if(!Application.isPlaying)
			{
				richText = InputField.Text;
			}

			if(Selected)
			{
				if(liveProcessing)
				{
					if(string.IsNullOrEmpty(processedText))
					{
						placeholderTextRenderer.Show();
						textRenderer.Hide();
						processedTextRenderer.Hide();
					}
					else
					{
						placeholderTextRenderer.Hide();
						textRenderer.Hide();
						processedTextRenderer.Show();
					}
				}
				else if(shouldUseRichText)
				{
					if(string.IsNullOrEmpty(richText))
					{
						placeholderTextRenderer.Show();
						textRenderer.Hide();
						processedTextRenderer.Hide();
					}
					else
					{
						placeholderTextRenderer.Hide();
						textRenderer.Show();
						processedTextRenderer.Hide();
					}
				}
				else
				{
					if(string.IsNullOrEmpty(text))
					{
						placeholderTextRenderer.Show();
						textRenderer.Hide();
						processedTextRenderer.Hide();
					}
					else
					{
						placeholderTextRenderer.Hide();
						textRenderer.Show();
						processedTextRenderer.Hide();
					}
				}
			}
			else
			{
				if(liveProcessing || postProcessing)
				{
					if(string.IsNullOrEmpty(processedText))
					{
						placeholderTextRenderer.Show();
						textRenderer.Hide();
						processedTextRenderer.Hide();
					}
					else
					{
						placeholderTextRenderer.Hide();
						textRenderer.Hide();
						processedTextRenderer.Show();
					}
				}
				else if(shouldUseRichText)
				{
					if(string.IsNullOrEmpty(richText))
					{
						placeholderTextRenderer.Show();
						textRenderer.Hide();
						processedTextRenderer.Hide();
					}
					else
					{
						placeholderTextRenderer.Hide();
						textRenderer.Show();
						processedTextRenderer.Hide();
					}
				}
				else
				{
					if(string.IsNullOrEmpty(text))
					{
						placeholderTextRenderer.Show();
						textRenderer.Hide();
						processedTextRenderer.Hide();
					}
					else
					{
						placeholderTextRenderer.Hide();
						textRenderer.Show();
						processedTextRenderer.Hide();
					}
				}
			}
		}

		/// <summary>Gets the currently active/visible Text Renderer</summary>
		internal TextRenderer GetActiveTextRenderer()
		{
			UpdateVisibleTextRenderers();

			if(InputField.PlaceholderTextRenderer.Visible)
			{
				return InputField.PlaceholderTextRenderer;
			}
			else if(InputField.ProcessedTextRenderer.Visible)
			{
				return InputField.ProcessedTextRenderer;
			}
			else
			{
				return InputField.TextRenderer;
			}
		}

		public void MarkDirty()
		{
			textDirty = true;
			selectionDirty = true;
		}

		/// <summary>Updates text and caret of current active Text Renderer</summary>
		internal void UpdateActiveTextRenderer()
		{
			TextRenderer activeTextRenderer = GetActiveTextRenderer();
			if(activeTextRenderer.UpdateImmediately())
			{
				Util.UpdateTextAlignment(activeTextRenderer, InputField.TextContentTransform);
				UpdateSize(activeTextRenderer);
				UpdateCaretPosition();
				activeTextRenderer.UpdateImmediately();
			}
			else if(InputField.gameObject.activeInHierarchy)
			{
				InputField.StartCoroutine(DelayedUpdateActiveTextRenderer());
			}
		}

		internal IEnumerator DelayedUpdateActiveTextRenderer()
		{
			yield return null;
			UpdateActiveTextRenderer();
		}

		internal void UpdateSettings()
		{
			if(Application.isEditor)
			{
				textManipulator?.RefreshTextValidator();

				if(!Settings.SimulateMobileBehaviourInEditor) { return; }
			}

			if(Selected)
			{
				LoadKeyboard(); //Reload keyboard to apply settings
			}

			MarkDirty();
		}

		/// <summary>Updates the visual position of the caret</summary>
		internal void UpdateCaretPosition()
		{
			if(Application.isPlaying && selected)
			{
				if(InputField.CanUseActionBar && ActionBar == null)
				{
					textInputHandler.InitActionBar(this);
				}
				textNavigator.UpdateRendering(false);
			}
		}

		internal RectTransform GetCaretTransform()
		{
			if(!selected)
			{
				Debug.LogWarning("This input field is not selected");
				return null;
			}

			BasicTextSelectionHandler basicTextSelectionHandler = textNavigator.GetTextSelectionHandler<BasicTextSelectionHandler>();
			if(basicTextSelectionHandler != null)
			{
				return basicTextSelectionHandler.CaretTransform;
			}

			Debug.LogWarning("Didn't find a BasicTextSelectionHandler. This input field is probably not in edit mode");
			return null;
		}

		public void OnTextScrollChanged(Vector2 scroll)
		{
			if(textInputHandler != null && ActionBar != null)
			{
				textInputHandler.BreakHold();
				if(InputField.CanUseActionBar)
				{
					textNavigator.UpdateActionBarPosition();
				}
			}
		}

		/// <summary>Event callback when a hardware keyboard has been connected/disconnected</summary>
		/// <param name="connected">Indicates whether a hardware keyboard has been connected</param>
		internal void OnHardwareKeyboardChanged(bool connected)
		{
			int currentSelectionStartPosition = selectionStartPosition;
			int currentSelectionEndPosition = selectionEndPosition;
			NativeKeyboardManager.RemoveHardwareKeyboardChangedListener(OnHardwareKeyboardChanged); //Will be readded in Initialize()
			Initialize();
			if(editMode)
			{
				textNavigator.BeginEditMode();
				textManipulator.BeginEditMode();

				InputField.SetTextSelection(currentSelectionStartPosition, currentSelectionEndPosition);

				if(!ShouldUseHardwareKeyboard()) //Reload soft keyboard
				{
					keyboardClient.HideKeyboard();
					LoadKeyboard();
				}
			}
		}

		internal bool ShouldUseHardwareKeyboard()
		{
			bool shouldUseHardwareKeyboard = false;

			switch(Settings.MobileKeyboardBehaviour)
			{
				case MobileKeyboardBehaviour.USE_HARDWARE_KEYBOARD_WHEN_AVAILABLE:
					shouldUseHardwareKeyboard = NativeKeyboardManager.HardwareKeyboardConnected;
					break;
				case MobileKeyboardBehaviour.ALWAYS_USE_TOUCHSCREENKEYBOARD:
					shouldUseHardwareKeyboard = false;
					break;
				case MobileKeyboardBehaviour.ALWAYS_USE_HARDWAREKEYBOARD:
					shouldUseHardwareKeyboard = true;
					break;
			}

			return shouldUseHardwareKeyboard;
		}

		/// <summary>Toggles the tag pair defined by given start and end tag in current text selection</summary>
		internal void ToggleTagPair(string startTag, string endTag)
		{
			if(InputField.ShouldUseRichText)
			{
				if(!HasSelection) { return; }

				TextEditFrame richTextEditFrame = richTextProcessor.ToggleTagPair(startTag, endTag);
				if(InputField.RichText != richTextEditFrame.text)
				{
					SetRichText(richTextEditFrame.text);
					SetRichTextSelection(richTextEditFrame.selectionStartPosition, richTextEditFrame.selectionEndPosition);
				}
			}
			else
			{
				Debug.LogWarning("Rich text editing is not enabled on this input field");
			}
		}

		public void ReplaceSelectedTextInRichText(string textToInsert)
		{
			if(InputField.ShouldUseRichText)
			{
				if(!HasSelection) { return; }

				TextEditFrame textEditFrame = lastTextEditFrame;
				int start = textEditFrame.selectionStartPosition;
				int end = textEditFrame.selectionEndPosition;
				textEditFrame.text = textEditFrame.text.Remove(start, (end - start));
				textEditFrame.text = textEditFrame.text.Insert(start, textToInsert);
				int caretPosition = start + textToInsert.Length;
				textEditFrame.selectionStartPosition = caretPosition;
				textEditFrame.selectionEndPosition = caretPosition;
				ApplyTextEditFrame(textEditFrame);
			}
			else
			{
				Debug.LogWarning("Rich text editing is not enabled on this input field");
			}
		}

		internal void CheckClick(PointerEventData eventData)
		{
			if(!InputField.interactable) { return; }

			SelectionMode selectionMode = InputField.SelectionMode;
			Canvas canvas = InputField.Canvas;
			RectTransform textAreaTransform = InputField.TextAreaTransform;
			CaretOnBeginEdit caretOnBeginEdit = InputField.CaretOnBeginEdit;

			if(!Selected && selectionMode == SelectionMode.SELECT_ON_RELEASE)
			{
				Vector2 currentPosition = eventData.position;
				Vector2 move = (currentPosition - pressPosition) / canvas.scaleFactor;
				float fontSize = GetActiveTextRenderer().FontSize;
				if(Mathf.Abs(move.x) > fontSize || Mathf.Abs(move.y) > fontSize) //Using font size as the tap threshold
				{
					EventSystem.current.SetSelectedGameObject(null);
					return;
				}

				beginEditReason = BeginEditReason.USER_SELECT;
				if(EventSystem.current.currentSelectedGameObject != InputField.gameObject)
				{
					EventSystem.current.SetSelectedGameObject(InputField.gameObject);
				}
				EnableSelection();
				BeginEditMode();

				Vector2 localMousePosition;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(textAreaTransform, eventData.position, eventData.pressEventCamera, out localMousePosition);
				localMousePosition.x += (textAreaTransform.rect.width * 0.5f);
				localMousePosition.y -= (textAreaTransform.rect.height * 0.5f);

				if(caretOnBeginEdit == CaretOnBeginEdit.LOCATION_OF_CLICK)
				{
					textInputHandler.OnPress(localMousePosition);
					textInputHandler.OnRelease(localMousePosition);
				}
				else if(caretOnBeginEdit == CaretOnBeginEdit.START_OF_TEXT)
				{
					SetSelection(0, 0);
				}
				else if(caretOnBeginEdit == CaretOnBeginEdit.END_OF_TEXT)
				{
					int caretPosition = Text.Length;
					SetSelection(caretPosition, caretPosition);
				}
				else if(caretOnBeginEdit == CaretOnBeginEdit.SELECT_ALL)
				{
					SetSelection(0, Text.Length);
				}
			}
			else if(selected && selectionMode == SelectionMode.SELECT_ON_PRESS && !editMode)
			{
				Vector2 currentPosition = eventData.position;
				Vector2 move = (currentPosition - pressPosition) / canvas.scaleFactor;
				float fontSize = GetActiveTextRenderer().FontSize;
				if(Mathf.Abs(move.x) > fontSize || Mathf.Abs(move.y) > fontSize) //Using font size as the tap threshold
				{
					return;
				}

				beginEditReason = BeginEditReason.USER_SELECT;
				if(EventSystem.current.currentSelectedGameObject != InputField.gameObject)
				{
					EventSystem.current.SetSelectedGameObject(InputField.gameObject);
				}
				EnableSelection();
				BeginEditMode();
			}
		}

		/// <summary>Resets the drag start position to given position (after text delete)</summary>
		internal void ResetDragStartPosition(int position)
		{
			dragStartPosition = position;
		}

		internal void OnBeginDrag(PointerEventData eventData)
		{
			DragMode dragMode = InputField.DragMode;
			Transform transform = InputField.transform;

			if(!Selected || dragMode == DragMode.MOVE_TEXT)
			{
				IBeginDragHandler beginDragHandler = transform.parent.GetComponentInParent<IBeginDragHandler>();
				if(beginDragHandler != null)
				{
					beginDragHandler.OnBeginDrag(eventData);
				}

				updateDrag = false;
				return;
			}

			updateDrag = true;
			dragStartPosition = textInputHandler.PressCharPosition;

			eventData.Use();
		}

		internal void OnDrag(PointerEventData eventData)
		{
			DragMode dragMode = InputField.DragMode;
			Transform transform = InputField.transform;
			RectTransform textAreaTransform = InputField.TextAreaTransform;
			TextRenderer processedTextRenderer = InputField.ProcessedTextRenderer;
			TextRenderer textRenderer = InputField.TextRenderer;
			bool liveProcessing = InputField.LiveDecoration;

			if(!Selected || dragMode == DragMode.MOVE_TEXT)
			{
				IDragHandler dragHandler = transform.parent.GetComponentInParent<IDragHandler>();
				if(dragHandler != null)
				{
					dragHandler.OnDrag(eventData);
				}

				updateDrag = false;
				return;
			}


			if(PositionOutOfBounds(eventData))
			{
				if(!dragOutOfBounds)
				{
					dragOutOfBounds = true;
				}
			}
			else
			{
				dragOutOfBounds = false;
			}


			Vector2 localMousePosition;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(textAreaTransform, eventData.position, eventData.pressEventCamera, out localMousePosition);
			localMousePosition.x += (textAreaTransform.rect.width * 0.5f);
			localMousePosition.y -= (textAreaTransform.rect.height * 0.5f);

			if(dragMode == DragMode.UPDATE_TEXT_SELECTION && !string.IsNullOrEmpty(text))
			{
				if(!UsingTouchSelectionCursors)
				{
					int position;
					if(liveProcessing)
					{
						position = textNavigator.GetCharacterIndexFromPosition(processedTextRenderer, localMousePosition);
					}
					else
					{
						position = textNavigator.GetCharacterIndexFromPosition(textRenderer, localMousePosition);
					}

					textNavigator.UpdateSelectionArea(position, dragStartPosition);
				}
			}

			textInputHandler.OnDrag(localMousePosition);

			eventData.Use();
		}

		internal void OnEndDrag(PointerEventData eventData)
		{
			DragMode dragMode = InputField.DragMode;
			Transform transform = InputField.transform;

			if(!Selected || dragMode == DragMode.MOVE_TEXT)
			{
				IEndDragHandler endDragHandler = transform.parent.GetComponentInParent<IEndDragHandler>();
				if(endDragHandler != null)
				{
					endDragHandler.OnEndDrag(eventData);
				}

				updateDrag = false;
			}

			dragStartPosition = -1;
			updateDrag = false;

			eventData.Use();
		}

		internal void OnPointerDown(PointerEventData eventData)
		{
			RectTransform textContentTransform = InputField.TextContentTransform;
			RectTransform textAreaTransform = InputField.TextAreaTransform;
			SelectionMode selectionMode = InputField.SelectionMode;

			UserPressing = true;
			pressPosition = eventData.position;
			pressTextContentPosition = textContentTransform.anchoredPosition;

			Vector2 localMousePosition;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(textAreaTransform, eventData.position, eventData.pressEventCamera, out localMousePosition);
			localMousePosition.x += (textAreaTransform.rect.width * 0.5f);
			localMousePosition.y -= (textAreaTransform.rect.height * 0.5f);

			if(!selected)
			{
				if(selectionMode == SelectionMode.SELECT_ON_PRESS)
				{
					beginEditReason = BeginEditReason.USER_SELECT;
					EnableSelection();
				}
				else if(selectionMode == SelectionMode.SELECT_ON_RELEASE)
				{
					textInputHandler.LastPosition = localMousePosition;
					return;
				}
			}

			textInputHandler.OnPress(localMousePosition);
		}

		internal void OnPointerUp(PointerEventData eventData)
		{
			RectTransform textAreaTransform = InputField.TextAreaTransform;

			UserPressing = false;

			if(!selected)
			{
				CheckClick(eventData);
				return;
			}

			Vector2 localMousePosition;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(textAreaTransform, eventData.position, eventData.pressEventCamera, out localMousePosition);
			localMousePosition.x += (textAreaTransform.rect.width * 0.5f);
			localMousePosition.y -= (textAreaTransform.rect.height * 0.5f);

			textInputHandler.OnRelease(localMousePosition);
			CheckClick(eventData);
		}

		internal void OnDeselect(BaseEventData eventData)
		{
			ActiveBehaviour?.StartCoroutine(DelayedDeselect());
		}

		internal void OnUpdateSelected(BaseEventData eventData)
		{
			if(textNavigator.EditMode)
			{
				if(InputField.LiveDecoration)
				{
					LiveDecorationFilter liveDecorationFilter = InputField.LiveDecorationFilter;
					string processedText;
					if(liveDecorationFilter.UpdateFilter(out processedText))
					{
						SetProcessedText(processedText);

						if(Selected)
						{
							int caretPosition = CaretPosition;
							int processedCaretPosition = liveDecorationFilter.DetermineProcessedCaret(text, caretPosition, processedText);
							SetProcessedSelection(processedCaretPosition, processedCaretPosition);
						}
					}
				}

				textInputHandler.Process();

				if(InputField.CanUseTouchSelectionCursors)
				{
					TouchTextSelectionHandler touchTextSelectionHandler = textNavigator.GetTextSelectionHandler<TouchTextSelectionHandler>();
					if(touchTextSelectionHandler.StartCursor.Selected)
					{
						updateDrag = true;
						dragOutOfBounds = touchTextSelectionHandler.StartCursor.OutOfBounds;
					}
					else if(touchTextSelectionHandler.EndCursor.Selected)
					{
						updateDrag = true;
						dragOutOfBounds = touchTextSelectionHandler.EndCursor.OutOfBounds;
					}
					else
					{
						updateDrag = false;
						dragOutOfBounds = false;
					}
				}

				if(updateDrag)
				{
					UpdateDrag();
				}
			}
			else if(textInputHandler != null)
			{
				textInputHandler.UpdateHold();
			}

			eventData.Use();
			lastTimeSelected = Time.realtimeSinceStartup;
		}

		/// <summary>Updates the drag for out of bounds text scroll</summary>
		internal void UpdateDrag()
		{
			RectTransform textAreaTransform = InputField.TextAreaTransform;
			TextRenderer processedTextRenderer = InputField.ProcessedTextRenderer;
			TextRenderer textRenderer = InputField.TextRenderer;
			Canvas canvas = InputField.Canvas;
			float fastScrollSensitivity = InputField.FastScrollSensitivity;

			if(dragOutOfBounds)
			{
				Vector2 localMousePosition;
#if ENABLE_INPUT_SYSTEM
				RectTransformUtility.ScreenPointToLocalPointInRectangle(textAreaTransform, Mouse.current.position.ReadValue(), InputField.GetComponentInParent<Canvas>().worldCamera, out localMousePosition);
#else
				RectTransformUtility.ScreenPointToLocalPointInRectangle(textAreaTransform, Input.mousePosition, InputField.GetComponentInParent<Canvas>().worldCamera, out localMousePosition);
#endif
				localMousePosition.x += (textAreaTransform.rect.width * 0.5f);
				localMousePosition.y -= (textAreaTransform.rect.height * 0.5f);

				if(InputField.DragMode == DragMode.UPDATE_TEXT_SELECTION && !string.IsNullOrEmpty(text))
				{
					if(!UsingTouchSelectionCursors)
					{
						int position;
						if(InputField.LiveDecoration)
						{
							position = textNavigator.GetCharacterIndexFromPosition(processedTextRenderer, localMousePosition);
						}
						else
						{
							position = textNavigator.GetCharacterIndexFromPosition(textRenderer, localMousePosition);
						}

						textNavigator.UpdateSelectionArea(position, dragStartPosition);
					}

					BasicTextSelectionHandler basicTextSelectionHandler = textNavigator.GetTextSelectionHandler<BasicTextSelectionHandler>();
					float fullSize = Mathf.Min(canvas.pixelRect.width, canvas.pixelRect.height); //Use same reference size for horizontal and vertical scroll sensitivity calculation
					if(InputField.Multiline)
					{
						float scrollSensitivity = 1;
						if(localMousePosition.y > 0)
						{
							scrollSensitivity += fastScrollSensitivity * Mathf.Abs(localMousePosition.y / fullSize);
						}
						else if(localMousePosition.y < -textAreaTransform.rect.height)
						{
							scrollSensitivity += fastScrollSensitivity * Mathf.Abs((-textAreaTransform.rect.height - localMousePosition.y) / fullSize);
						}

						basicTextSelectionHandler.UpdateVerticalScrollPosition(textRenderer, scrollSensitivity);
					}
					else
					{
						float scrollSensitivity = 1;
						if(localMousePosition.x < 0)
						{
							scrollSensitivity += fastScrollSensitivity * Mathf.Abs(localMousePosition.x / fullSize);
						}
						else if(localMousePosition.x > textAreaTransform.rect.width)
						{
							scrollSensitivity += fastScrollSensitivity * Mathf.Abs((localMousePosition.x - textAreaTransform.rect.width) / fullSize);
						}

						basicTextSelectionHandler.UpdateHorizontalScrollPosition(textRenderer, scrollSensitivity);
					}
				}
			}
		}

		internal void OnSelect()
		{
			ManualSelect();
		}

		internal void Deselect(EndEditReason reason)
		{
			endEditReason = reason;
			EventSystem.current?.SetSelectedGameObject(null);
		}

		/// <summary>Checks if position is out of bounds</summary>
		/// <param name="eventData">The event data to check</param>
		/// <returns>true is position is out of bounds</returns>
		internal bool PositionOutOfBounds(PointerEventData eventData)
		{
			RectTransform textAreaTransform = InputField.TextAreaTransform;
			return !RectTransformUtility.RectangleContainsScreenPoint(textAreaTransform, eventData.position, eventData.pressEventCamera);
		}

		/// <summary>Marks as selected</summary>
		internal void EnableSelection()
		{
			AdvancedInputField lastSelectedInputField = NativeKeyboardManager.LastSelectedInputField;
			if(lastSelectedInputField != null && NativeKeyboardManager.LastSelectedInputField != InputField)
			{
				if(lastSelectedInputField.ShouldBlockDeselect)
				{
					return;
				}
			}

			selected = true;
			NativeKeyboardManager.LastSelectedInputField = InputField;
			if(InputField.ReadOnly)
			{
				keyboardClient.ClearEventQueue();
				CloseKeyboard();
				keyboardClient.Deactivate();
			}
			else
			{
				if(InputField == NativeKeyboardManager.ActiveInputFieldBeforePause)
				{
					NativeKeyboardManager.ActiveInputFieldBeforePause = null;
					KeyboardClient.Keyboard.RestoreKeyboard();
					KeyboardClient.Keyboard.State = KeyboardState.PENDING_SHOW;
				}
				else
				{
					keyboardClient.ClearEventQueue();
					LoadKeyboard();
				}
				keyboardClient.Activate();
			}
			eventHandler?.InvokeSelectionChanged(true);
		}

		/// <summary>Marks as deselected</summary>
		internal void DisableSelection()
		{
			if(Selected)
			{
				lastTimeSelected = Time.realtimeSinceStartup;
				selected = false;
				keyboardClient.Deactivate();
				eventHandler?.InvokeSelectionChanged(false);
			}
		}

		public void LoadKeyboard()
		{
			if(InputField.ReadOnly) { return; }

			string text = InputField.Text;

			if(InputField.ShouldUseRichText)
			{
				TextEditFrame textEditFrame = richTextProcessor.LastTextEditFrame;
				text = textEditFrame.text;
				selectionStartPosition = textEditFrame.selectionStartPosition;
				selectionEndPosition = textEditFrame.selectionEndPosition;
			}

			NativeKeyboardConfiguration configuration = new NativeKeyboardConfiguration()
			{
				keyboardType = InputField.KeyboardType,
				characterValidation = InputField.CharacterValidation,
				lineType = InputField.LineType,
				autocapitalizationType = InputField.AutocapitalizationType,
				autofillType = InputField.AutofillType,
				returnKeyType = InputField.ReturnKeyType,
				autocorrection = InputField.AutoCorrection,
				secure = InputField.Secure,
				richTextEditing = InputField.RichTextEditing,
				emojisAllowed = InputField.EmojisAllowed,
				hasNext = InputField.HasNext,
				characterLimit = InputField.CharacterLimit
			};

			string characterValidatorJSON = null;
			if(InputField.CharacterValidation == CharacterValidation.CUSTOM && InputField.CharacterValidator != null)
			{
				characterValidatorJSON = JsonUtility.ToJson(InputField.CharacterValidator);
			}
			configuration.characterValidatorJSON = characterValidatorJSON;

#if !UNITY_EDITOR
			KeyboardClient.Keyboard.State = KeyboardState.PENDING_SHOW;
#endif
			KeyboardClient.ShowKeyboard(text, selectionStartPosition, selectionEndPosition, configuration);
		}

		public void CloseKeyboard()
		{
			if(KeyboardClient.Keyboard.State != KeyboardState.HIDDEN)
			{
				KeyboardClient.Keyboard.State = KeyboardState.PENDING_HIDE;
				KeyboardClient.HideKeyboard();
			}
		}

		internal void ConfigureActionBar()
		{
			if(InputField.CanUseActionBar)
			{
				textInputHandler.InitActionBar(this);
			}
		}

		/// <summary>Enables text editing</summary>
		internal void BeginEditMode()
		{
			editMode = true;
			if(InputField.CanUseActionBar)
			{
				textInputHandler.InitActionBar(this);
			}
			textInputHandler.BeginEditMode();
			textNavigator.BeginEditMode();
			textManipulator.BeginEditMode();
			lastTextEditFrame = new TextEditFrame(Text, 0, 0);
			keyboardClient.ClearLastTextEditFrame();
			textDirty = true;
			selectionDirty = true;
			RefreshRendering();
			textNavigator.KeepActionBarVisible = false;
			eventHandler?.InvokeBeginEdit(beginEditReason);
		}

		/// <summary>Disables text editing</summary>
		internal void EndEditMode()
		{
			editMode = false;
			if(ActionBar != null)
			{
				Object.Destroy(ActionBar.gameObject);
			}
			textNavigator.EndEditMode();
			textManipulator.EndEditMode();
			textInputHandler.CancelInput();

			if(InputField.LiveDecoration)
			{
				LiveDecorationFilter liveDecorationFilter = InputField.LiveDecorationFilter;
				string processedText;
				if(liveDecorationFilter.UpdateFilter(out processedText, true))
				{
					SetProcessedText(processedText);
				}
			}

			MarkDirty();

			eventHandler?.InvokeEndEdit(text, endEditReason);
		}

		internal void ApplyTextEditFrame(TextEditFrame textEditFrame)
		{
			if(InputField.LineLimit > 0 && !InputField.ShouldUseRichText)
			{
				if(ApplyLineLimit(textEditFrame, out TextEditFrame resultTextEditFrame))
				{
					ApplyTextEditFrame(resultTextEditFrame); //Retry
					return;
				}
			}

			if(InputField.LiveProcessing)
			{
				TextEditFrame processedFrame = InputField.LiveProcessingFilter.ProcessTextEditUpdate(textEditFrame, lastTextEditFrame);
				if(processedFrame.text != textEditFrame.text || processedFrame.selectionStartPosition != textEditFrame.selectionStartPosition
					|| processedFrame.selectionEndPosition != textEditFrame.selectionEndPosition)
				{
					textEditFrame = processedFrame;
					textDirty = true;
					selectionDirty = true;
				}
			}
			SetText(textEditFrame.text);
			SetSelection(textEditFrame.selectionStartPosition, textEditFrame.selectionEndPosition);

			if(InputField.LiveDecoration)
			{
				LiveDecorationFilter liveDecorationFilter = InputField.LiveDecorationFilter;
				if(textEditFrame.text != lastTextEditFrame.text)
				{
					string processedText = liveDecorationFilter.ProcessText(textEditFrame.text, textEditFrame.selectionStartPosition);
					SetProcessedText(processedText);
				}

				if(textEditFrame.selectionStartPosition != lastTextEditFrame.selectionStartPosition || textEditFrame.selectionEndPosition != lastTextEditFrame.selectionEndPosition)
				{
					int processedSelectionStartPosition = liveDecorationFilter.DetermineProcessedCaret(textEditFrame.text, textEditFrame.selectionStartPosition, processedText);
					int processedSelectionEndPosition = liveDecorationFilter.DetermineProcessedCaret(textEditFrame.text, textEditFrame.selectionEndPosition, processedText);
					SetProcessedSelection(processedSelectionStartPosition, processedSelectionEndPosition);
				}
			}

			if(InputField.ShouldUseRichText)
			{
				TextEditFrame richTextEditFrame = richTextProcessor.ProcessTextEditFrame(textEditFrame);
				if(InputField.LineLimit > 0)
				{
					if(ApplyLineLimit(textEditFrame, out TextEditFrame resultTextEditFrame))
					{
						ApplyTextEditFrame(resultTextEditFrame); //Retry
						return;
					}
				}
				SetRichText(richTextEditFrame.text);
				SetRichTextSelection(richTextEditFrame.selectionStartPosition, richTextEditFrame.selectionEndPosition);
			}

			lastTextEditFrame = textEditFrame;
		}

		internal void ApplyRichTextEditFrame(TextEditFrame richTextEditFrame)
		{
			if(InputField.LiveProcessing)
			{
				InputField.LiveProcessingFilter.OnRichTextEditUpdate(richTextEditFrame, richTextProcessor.LastRichTextEditFrame);
			}

			TextEditFrame textEditFrame = richTextProcessor.ProcessRichTextEditFrame(richTextEditFrame);
			SetText(textEditFrame.text);
			SetSelection(textEditFrame.selectionStartPosition, textEditFrame.selectionEndPosition);
			MarkDirty();
		}

		internal bool ApplyLineLimit(TextEditFrame textEditFrame, out TextEditFrame resultTextEditFrame)
		{
			resultTextEditFrame = textEditFrame;

			TextRenderer activeTextRenderer;
			if(InputField.LiveDecoration || (!InputField.Selected && InputField.PostDecoration))
			{
				activeTextRenderer = InputField.ProcessedTextRenderer;
			}
			else
			{
				activeTextRenderer = InputField.TextRenderer;
			}


			string text = textEditFrame.text;
			activeTextRenderer.Text = text;
			activeTextRenderer.UpdateImmediately();

			if(activeTextRenderer.LineCount > InputField.LineLimit && text.Length > 1)
			{
				int selectionStartPosition = textEditFrame.selectionStartPosition;
				selectionStartPosition--;

				EmojiData emojiData;
				if(InputField.EmojisAllowed && NativeKeyboardManager.EmojiEngine.TryFindPreviousEmojiInText(text, selectionStartPosition, out emojiData))
				{
					int count = emojiData.text.Length;
					text = text.Remove(selectionStartPosition + 1 - count, count);
					selectionStartPosition -= (count - 1);
				}
				else
				{
					text = text.Remove(selectionStartPosition, 1);
				}

				textEditFrame.text = text;
				textEditFrame.selectionStartPosition = selectionStartPosition;
				textEditFrame.selectionEndPosition = textEditFrame.selectionStartPosition;
				ApplyLineLimit(textEditFrame, out resultTextEditFrame); //Try again to stay within line limit
				return true;
			}

			return false;
		}

		internal void OnUpdate()
		{
			if(selected)
			{
				textNavigator.OnUpdate();
			}
		}

		internal void OnLateUpdate()
		{
			RefreshRendering();
		}

		internal void RefreshRendering()
		{
			if(textDirty || selectionDirty)
			{
				if(selected)
				{
					TextEditFrame? lastNativeTextEditFrame = keyboardClient.LastTextEditFrame;
					if(lastNativeTextEditFrame == null)
					{
						keyboardClient.UpdateTextEdit(text, selectionStartPosition, selectionEndPosition); //Sync state with native code
					}
					else
					{
						TextEditFrame lastNativeFrame = lastNativeTextEditFrame.Value;
						if(lastNativeFrame.text != text
							|| lastNativeFrame.selectionStartPosition != selectionStartPosition
							|| lastNativeFrame.selectionEndPosition != selectionEndPosition)
						{
							keyboardClient.UpdateTextEdit(text, selectionStartPosition, selectionEndPosition); //Sync state with native code
						}
					}
				}

				if(textDirty) //Always update selection if text changed
				{
					RefreshRenderedText();
					textNavigator.UpdateRendering();
					textDirty = false;
					selectionDirty = false;
				}
				else if(selectionDirty) //Only update selection if only selection changed
				{
					textNavigator.UpdateRendering();
					selectionDirty = false;
				}

				lastTextEditFrame.text = text;
				lastTextEditFrame.selectionStartPosition = selectionStartPosition;
				lastTextEditFrame.selectionEndPosition = selectionEndPosition;
			}
		}

		/// <summary>Checks if a CanvasFrontRenderer is selected</summary>
		/// <returns>true if CanvasFrontRenderer is selected</returns>
		internal bool IsCanvasFrontRendererSelected()
		{
			GameObject currentSelection = EventSystem.current.currentSelectedGameObject;
			if(currentSelection != null)
			{
				bool selected = (currentSelection.GetComponentInParent<CanvasFrontRenderer>() != null);
				return selected;
			}

			return false;
		}

		internal bool IsInputFieldChildSelected()
		{
			GameObject currentSelection = EventSystem.current.currentSelectedGameObject;
			if(currentSelection != null)
			{
				return (currentSelection.GetComponentInParent<AdvancedInputField>() == InputField);
			}

			return false;
		}

		internal bool IsInputFieldRefocusableSelected()
		{
			GameObject currentSelection = EventSystem.current.currentSelectedGameObject;
			if(currentSelection != null)
			{
				if(currentSelection.GetComponentInParent<InputFieldRefocusable>() != null) { return true; }
				if(currentSelection.GetComponentInParent<InputFieldButton>() != null) { return true; }
			}

			return false;
		}

		/// <summary>Delayed deselect method to check for valid deselects</summary>
		internal IEnumerator DelayedDeselect()
		{
			yield return null;
			if(this == null) { yield break; }

			if(IsCanvasFrontRendererSelected() || IsInputFieldChildSelected() || IsInputFieldRefocusableSelected()) //Invalid deselect
			{
				Reselect();
			}
			else if(InputField.ShouldBlockDeselect)
			{
				eventHandler?.InvokeEndEdit(text, endEditReason);

				if(InputField.ShouldBlockDeselect) //Second check, because flag could have been changed when above event got called
				{
					Reselect();
				}
				else //Valid deselect
				{
					EndEditMode();
					DisableSelection();
					UpdateActiveTextRenderer();
				}
			}
			else //Valid deselect
			{
				EndEditMode();
				DisableSelection();
			}

			endEditReason = EndEditReason.USER_DESELECT; //Reset to default reason (deselection)
		}

		/// <summary>(Re)selects the InputField</summary>
		internal void Reselect()
		{
			if(EventSystem.current != null)
			{
				EventSystem.current.SetSelectedGameObject(InputField.gameObject);
				textInputHandler.OnSelect();
			}
			else
			{
				ActiveBehaviour?.StartCoroutine(DelayedReselect());
			}
		}

		internal IEnumerator DelayedReselect()
		{
			yield return null;
			if(EventSystem.current != null)
			{
				EventSystem.current.SetSelectedGameObject(InputField.gameObject);
				textInputHandler.OnSelect();
				keyboardClient.Activate();
			}
		}

		internal void ManualSelect(BeginEditReason beginEditReason = BeginEditReason.PROGRAMMATIC_SELECT)
		{
			this.beginEditReason = beginEditReason;
			Reselect();
			EnableSelection();
			BeginEditMode();
			switch(InputField.CaretOnBeginEdit)
			{
				case CaretOnBeginEdit.START_OF_TEXT: textNavigator.MoveToStart(); break;
				case CaretOnBeginEdit.END_OF_TEXT: textNavigator.MoveToEnd(); break;
				case CaretOnBeginEdit.LOCATION_OF_CLICK:
					if(beginEditReason == BeginEditReason.USER_SELECT)
					{
						textNavigator.ResetCaret(textInputHandler.LastPosition);
					}
					else
					{
						textNavigator.MoveToEnd();
					}
					break;
				case CaretOnBeginEdit.SELECT_ALL: textNavigator.SelectAll(); break;
			}
			textInputHandler.Process();
		}
	}
}
