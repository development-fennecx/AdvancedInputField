// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AdvancedInputFieldPlugin
{
	public struct TextEditFrame
	{
		public string text;
		public int selectionStartPosition;
		public int selectionEndPosition;

		public TextEditFrame(string text, int selectionStartPosition, int selectionEndPosition)
		{
			this.text = text;
			this.selectionStartPosition = selectionStartPosition;
			this.selectionEndPosition = selectionEndPosition;
		}

		public override string ToString()
		{
			return string.Format("Text: {0}, Selection: {1} -> {2}", text, selectionStartPosition, selectionEndPosition);
		}
	}

	public struct TextRange
	{
		public int start;
		public int end;

		public TextRange(int start, int end)
		{
			this.start = start;
			this.end = end;
		}
	}

	/// <summary>The mode of the InputField. Determines how changes in the text bounds should be handled.</summary>
	public enum InputFieldMode { SCROLL_TEXT, HORIZONTAL_RESIZE_FIT_TEXT, VERTICAL_RESIZE_FIT_TEXT }

	/// <summary>Configuration preset for the content of this InputField</summary>
	public enum ContentType { STANDARD, AUTOCORRECTED, INTEGER_NUMBER, DECIMAL_NUMBER, ALPHANUMERIC, NAME, EMAIL_ADDRESS, PASSWORD, PIN, CUSTOM, IP_ADDRESS, SENTENCE, DECIMAL_NUMBER_FORCE_POINT }

	/// <summary>The type of input</summary>
	public enum InputType { STANDARD, AUTOCORRECT, PASSWORD }

	/// <summary>The keyboard on mobile to use</summary>
	public enum KeyboardType { DEFAULT, ASCII_CAPABLE, DECIMAL_PAD, URL, NUMBER_PAD, PHONE_PAD, EMAIL_ADDRESS, NUMBERS_AND_PUNCTUATION }

	/// <summary>The validation to use for the text</summary>
	public enum CharacterValidation { NONE, INTEGER, DECIMAL, ALPHANUMERIC, NAME, EMAIL_ADDRESS, IP_ADDRESS, SENTENCE, CUSTOM, DECIMAL_FORCE_POINT }

	/// <summary>The type of line</summary>
	public enum LineType { SINGLE_LINE, MULTILINE_SUBMIT, MULTILINE_NEWLINE }

	/// <summary>The type of autocapitalization</summary>
	public enum AutocapitalizationType { NONE, CHARACTERS, WORDS, SENTENCES }

	/// <summary>The type of autofill</summary>
	public enum AutofillType
	{
		NONE,
		USERNAME,
		PASSWORD,
		NEW_PASSWORD,
		ONE_TIME_CODE,
		NAME,
		GIVEN_NAME,
		MIDDLE_NAME,
		FAMILY_NAME,
		LOCATION,
		FULL_STREET_ADDRESS,
		STREET_ADDRESS_LINE_1,
		STREET_ADDRESS_LINE_2,
		ADDRESS_CITY,
		ADDRESS_STATE,
		ADDRESS_CITY_AND_STATE,
		COUNTRY_NAME,
		POSTAL_CODE,
		TELEPHONE_NUMBER
	}

	/// <summary>The type of return key to display on mobile</summary>
	public enum ReturnKeyType { DEFAULT, GO, SEND, SEARCH }

	/// <summary>Determines which input event to use to select the inputfield</summary>
	public enum SelectionMode { SELECT_ON_RELEASE, SELECT_ON_PRESS }

	/// <summary>Determines how to use drag events</summary>
	public enum DragMode { UPDATE_TEXT_SELECTION, MOVE_TEXT }

	/// <summary>Determines what to use as start of the drag</summary>
	public enum StartDragMode { FROM_CURRENT_POSITION, FROM_SELECTION_START, FROM_SELECTION_END }

	/// <summary>The reason for beginning edit mode</summary>
	public enum BeginEditReason { USER_SELECT, KEYBOARD_NEXT, PROGRAMMATIC_SELECT }

	/// <summary>The reason for ending edit mode</summary>
	public enum EndEditReason { USER_DESELECT, KEYBOARD_CANCEL, KEYBOARD_DONE, KEYBOARD_NEXT, PROGRAMMATIC_DESELECT }

	/// <summary>The caret position in the text when beginning edit mode</summary>
	public enum CaretOnBeginEdit { LOCATION_OF_CLICK, START_OF_TEXT, END_OF_TEXT, SELECT_ALL }

	/// <summary>The clamp mode of the mobile cursors</summary>
	public enum CursorClampMode { NONE, TEXT_BOUNDS, INPUTFIELD_BOUNDS }

	/// <summary>The scroll behaviour when ending edit mode</summary>
	public enum ScrollBehaviourOnEndEdit { START_OF_TEXT, NO_SCROLL, END_OF_TEXT }

	/// <summary>The visibility mode for the scrollbars</summary>
	public enum ScrollBarVisibilityMode { ALWAYS_HIDDEN, ALWAYS_VISIBLE, IN_EDIT_MODE_WHEN_NEEDED, ALWAYS_WHEN_NEEDED }

	public enum SpecialKeyCode { BACK, BACKSPACE, ESCAPE }

	/// <summary>The main class of Advanced Input Field</summary>
	[RequireComponent(typeof(RectTransform))]
	public class AdvancedInputField: Selectable, IPointerClickHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IUpdateSelectedHandler
	{
		#region EVENT_CLASSES
		/// <summary>Event used for selection change</summary>
		[Serializable]
		public class SelectionChangedEvent: UnityEvent<bool> { }

		/// <summary>Event used for edit begin</summary>
		[Serializable]
		public class BeginEditEvent: UnityEvent<BeginEditReason> { }

		/// <summary>Event used for edit end</summary>
		[Serializable]
		public class EndEditEvent: UnityEvent<string, EndEditReason> { }

		/// <summary>Event used for text value change</summary>
		[Serializable]
		public class ValueChangedEvent: UnityEvent<string> { }

		/// <summary>Event used for caret position change</summary>
		[Serializable]
		public class CaretPositionChangedEvent: UnityEvent<int> { }

		/// <summary>Event used for selection change</summary>
		[Serializable]
		public class TextSelectionChangedEvent: UnityEvent<int, int> { }

		/// <summary>Event used for size change</summary>
		[Serializable]
		public class SizeChangedEvent: UnityEvent<Vector2> { }

		/// <summary>Event used when a special key has been pressed</summary>
		[Serializable]
		public class SpecialKeyPressedEvent: UnityEvent<SpecialKeyCode> { }

		/// <summary>Event used when the text has been tapped</summary>
		[Serializable]
		public class TextTapEvent: UnityEvent<int, Vector2> { }

		/// <summary>Event used when an ActionBar action has been triggered</summary>
		[Serializable]
		public class ActionBarActionEvent: UnityEvent<ActionBarAction> { }
		#endregion

		#region SERIALIZED_FIELDS
		[Tooltip("The mode of the InputField. Determines how changes in the text bounds should be handled.")]
		[SerializeField]
		private InputFieldMode mode;

		[Tooltip("The main text renderer")]
		[SerializeField]
		private TextRenderer textRenderer;

		[Tooltip("The text renderer used for text that has been processed")]
		[SerializeField]
		private TextRenderer processedTextRenderer;

		[Tooltip("The text renderer used as placeholder")]
		[SerializeField]
		private TextRenderer placeholderTextRenderer;

		[Tooltip("The caret renderer")]
		[SerializeField]
		private Image caretRenderer;

		[Tooltip("The renderer for text selection")]
		[SerializeField]
		private TextSelectionRenderer selectionRenderer;

		[Tooltip("The main text string")]
		[TextArea(3, 10)]
		[SerializeField]
		private string text;

		[Tooltip("The placeholder text string")]
		[SerializeField]
		private string placeholderText;

		[Tooltip("Enables editing of text with rich text tags")]
		[SerializeField]
		private bool richTextEditing;

		[Tooltip("The rich text configuration")]
		[SerializeField]
		private RichTextData richTextConfig;

		[Tooltip("The maximum amount of characters allowed, zero means infinite")]
		[SerializeField]
		private int characterLimit;

		[Tooltip("The maximum amount of lines allowed, zero means infinite")]
		[SerializeField]
		private int lineLimit;

		[Tooltip("Configuration preset for the content of this InputField")]
		[SerializeField]
		[EnumOrder(new int[] { 0, 1, 2, 3, 12, 4, 5, 6, 7, 8, 10, 11, 9 })] //Custom should be displayed last
		private ContentType contentType;

		[Tooltip("The type of line")]
		[SerializeField]
		private LineType lineType;

		[Tooltip("The type of input")]
		[SerializeField]
		private InputType inputType;

		[Tooltip("Indicates whether the password should be visible")]
		[SerializeField]
		private bool visiblePassword;

		[Tooltip("The keyboard on mobile to use")]
		[EnumOrder(new int[] { 0, 1, 7, 3, 4, 5, 2, 6 })]
		[SerializeField]
		private KeyboardType keyboardType;

		[Tooltip("The validation to use for the text")]
		[SerializeField]
		[EnumOrder(new int[] { 0, 1, 2, 9, 3, 4, 5, 6, 7, 8 })] //Custom should be displayed last
		private CharacterValidation characterValidation;

		[Tooltip("The custom character validator to use for the text")]
		[SerializeField]
		private CharacterValidator characterValidator;

		[Tooltip("Indicates whether emojis should be allowed, requires TextMeshPro Text Renderers")]
		[SerializeField]
		private bool emojisAllowed;

		[Tooltip("Indicates whether rich text bindings should be allowed")]
		[SerializeField]
		private bool richTextBindingsAllowed;

		[Tooltip("The filter (if any) to use whenever text or caret position changes")]
		[SerializeField]
		private LiveProcessingFilter liveProcessingFilter;

		[Tooltip("The filter (if any) to use whenever text or caret position changes")]
		[SerializeField]
		private LiveDecorationFilter liveDecorationFilter;

		[Tooltip("The filter (if any) to use when input field has been deselected")]
		[SerializeField]
		private PostDecorationFilter postDecorationFilter;

		[Tooltip("Determines which input event to use to select the inputfield")]
		[SerializeField]
		private SelectionMode selectionMode;

		[Tooltip("Determines how to use drag events")]
		[SerializeField]
		private DragMode dragMode;

		[Tooltip("Indicates where to position the caret when beginning edit mode")]
		[SerializeField]
		private CaretOnBeginEdit caretOnBeginEdit;

		[Tooltip("The blink rate of the caret")]
		[SerializeField]
		[Range(0.1f, 4f)]
		private float caretBlinkRate = 0.85f;

		[Tooltip("The width of the caret")]
		[SerializeField]
		[Range(0.01f, 10)]
		private float caretWidth = 2;

		[Tooltip("The color of the caret")]
		[SerializeField]
		private Color caretColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

		[Tooltip("The color of the text selection")]
		[SerializeField]
		private Color selectionColor = new Color(168f / 255f, 206f / 255f, 255f / 255f, 192f / 255f);

		[Tooltip("Indicates whether to draw the text selection behind the text")]
		[SerializeField]
		private bool selectionBehindText = true;

		[Tooltip("Indicates if this input field is read only")]
		[SerializeField]
		private bool readOnly = false;

		[Tooltip("The scroll behaviour when ending edit mode")]
		[SerializeField]
		private ScrollBehaviourOnEndEdit scrollBehaviourOnEndEdit;

		[Tooltip("The visibility mode for the scrollbars")]
		[SerializeField]
		private ScrollBarVisibilityMode scrollBarsVisibilityMode;

		[Tooltip("The speed to scroll text (Canvas pixels per second)")]
		[SerializeField]
		private float scrollSpeed = 250;

		[Tooltip("The maximum time it can take to scroll to caret position (in seconds)")]
		[SerializeField]
		private float maxScrollTransitionTime = 3;

		[Tooltip("The additional scroll sensitivity when dragging out of bounds")]
		[SerializeField]
		private float fastScrollSensitivity = 5;

		[Tooltip("The minimal width of the InputField")]
		[SerializeField]
		private float resizeMinWidth = 0;

		[Tooltip("The maximum width of the InputField, zero means infinite")]
		[SerializeField]
		private float resizeMaxWidth = 0;

		[Tooltip("The minimal height of the InputField")]
		[SerializeField]
		private float resizeMinHeight = 0;

		[Tooltip("The maximum height of the InputField, zero means infinite")]
		[SerializeField]
		private float resizeMaxHeight = 0;

		[Tooltip("Event used for selection change")]
		[SerializeField]
		private SelectionChangedEvent onSelectionChanged = new SelectionChangedEvent();

		[Tooltip("Event used for edit begin")]
		[SerializeField]
		private BeginEditEvent onBeginEdit = new BeginEditEvent();

		[Tooltip("Event used for edit end")]
		[SerializeField]
		private EndEditEvent onEndEdit = new EndEditEvent();

		[Tooltip("Event used for text value change")]
		[SerializeField]
		private ValueChangedEvent onValueChanged = new ValueChangedEvent();

		[Tooltip("Event used for caret position change")]
		[SerializeField]
		private CaretPositionChangedEvent onCaretPositionChanged = new CaretPositionChangedEvent();

		[Tooltip("Event used for text selection change")]
		[SerializeField]
		private TextSelectionChangedEvent onTextSelectionChanged = new TextSelectionChangedEvent();

		[Tooltip("Event used for size change")]
		[SerializeField]
		private SizeChangedEvent onSizeChanged = new SizeChangedEvent();

		[Tooltip("Event used when a special key has been pressed")]
		[SerializeField]
		private SpecialKeyPressedEvent onSpecialKeyPressed = new SpecialKeyPressedEvent();

		[Tooltip("Event used when the text has been tapped")]
		[SerializeField]
		private TextTapEvent onTextTap = new TextTapEvent();

		[Tooltip("Event used when an ActionBar action has been triggered")]
		[SerializeField]
		private ActionBarActionEvent onActionBarAction = new ActionBarActionEvent();

		[Tooltip("Indicates if the ActionBar should be used")]
		[SerializeField]
		[FormerlySerializedAs("actionBar")]
		private bool actionBarEnabled = false;

		[Tooltip("Indicates if the cut option should be enabled in the ActionBar")]
		[SerializeField]
		private bool actionBarCut = true;

		[Tooltip("Indicates if the copy option should be enabled in the ActionBar")]
		[SerializeField]
		private bool actionBarCopy = true;

		[Tooltip("Indicates if the paste option should be enabled in the ActionBar")]
		[SerializeField]
		private bool actionBarPaste = true;

		[Tooltip("Indicates if the select all option should be enabled in the ActionBar")]
		[SerializeField]
		private bool actionBarSelectAll = true;

		[Tooltip("Indicates if the Touch Selection Cursors (handles for selection start and end) should be used")]
		[SerializeField]
		[FormerlySerializedAs("selectionCursors")]
		private bool touchSelectionCursorsEnabled = false;

		[Tooltip("The clamp mode of the Touch Selection Cursors")]
		public CursorClampMode cursorClampMode;

		[Tooltip("The type of autocapitalization")]
		[SerializeField]
		private AutocapitalizationType autocapitalizationType;

		[Tooltip("The type of autofill")]
		public AutofillType autofillType;

		[Tooltip("The type of return key, default is done/next key")]
		[SerializeField]
		private ReturnKeyType returnKeyType;

		[Tooltip("The next input field (if any) to switch to when pressing the done button on the TouchScreenKeyboard")]
		[SerializeField]
		private AdvancedInputField nextInputField;
		#endregion

		#region FIELDS
		private InputFieldEngine engine;

		private RectTransform rectTransform;

		/// <summary>The transform containing the TextArea</summary>
		private RectTransform textAreaTransform;

		private ScrollArea scrollArea;

		/// <summary>The scrollable ContentTransform</summary>
		private RectTransform textContentTransform;

		/// <summary>The Canvas</summary>
		private Canvas canvas;

		/// <summary>Indicates if this input field is initialized</summary>
		private bool initialized;
		#endregion

		#region PROPERTIES
		internal InputFieldEngine Engine
		{
			get
			{
				if(engine == null)
				{
					Initialize();
				}
				return engine;
			}
		}

		/// <summary>The RectTransform of the InputField</summary>
		public RectTransform RectTransform
		{
			get
			{
				if(rectTransform == null)
				{
					rectTransform = GetComponent<RectTransform>();
				}

				return rectTransform;
			}
		}

		/// <summary>The size of the InputField RectTransform</summary>
		public Vector2 Size
		{
			get { return RectTransform.rect.size; }
			set
			{
				Engine.SetSize(value);
			}
		}

		/// <summary>The mode of the InputField. Determines how changes in the text bounds should be handled.</summary>
		public InputFieldMode Mode
		{
			get { return mode; }
			set
			{
				mode = value;

				ScrollArea scrollArea = TextAreaTransform.GetComponent<ScrollArea>();
				switch(mode)
				{
					case InputFieldMode.SCROLL_TEXT: scrollArea.enabled = true; break;
					case InputFieldMode.HORIZONTAL_RESIZE_FIT_TEXT: scrollArea.enabled = false; break;
					case InputFieldMode.VERTICAL_RESIZE_FIT_TEXT: scrollArea.enabled = false; break;
				}

				Engine.MarkDirty();
			}
		}

		/// <summary>The RectTransform of the TextArea. The TextArea is the viewport of the actual rendered text.</summary>
		public RectTransform TextAreaTransform
		{
			get
			{
				if(textAreaTransform == null)
				{
					textAreaTransform = transform.Find("TextArea").GetComponent<RectTransform>();
				}
				return textAreaTransform;
			}
		}

		/// <summary>The ScrollArea of the TextArea</summary>
		internal ScrollArea ScrollArea
		{
			get
			{
				if(scrollArea == null)
				{
					scrollArea = TextAreaTransform.GetComponent<ScrollArea>();
				}
				return scrollArea;
			}
		}

		/// <summary>The scrollable content RectTransform of the actual text rendered text.</summary>
		public RectTransform TextContentTransform
		{
			get
			{
				if(textContentTransform == null)
				{
					textContentTransform = transform.Find("TextArea/Content").GetComponent<RectTransform>();
				}
				return textContentTransform;
			}
		}

		/// <summary>The main text renderer</summary>
		public TextRenderer TextRenderer
		{
			get
			{
				if(textRenderer == null)
				{
					textRenderer = transform.Find("TextArea/Content/Text").GetComponent<TextRenderer>();
				}
				return textRenderer;
			}
		}

		/// <summary>The text renderer for processed text (when using Live or PostDecorationFilters)</summary>
		public TextRenderer ProcessedTextRenderer
		{
			get
			{
				if(processedTextRenderer == null)
				{
					processedTextRenderer = transform.Find("TextArea/Content/Processed").GetComponent<TextRenderer>();
				}
				return processedTextRenderer;
			}
		}

		/// <summary>The text renderer used as placeholder</summary>
		public TextRenderer PlaceholderTextRenderer
		{
			get
			{
				if(placeholderTextRenderer == null)
				{
					placeholderTextRenderer = transform.Find("TextArea/Content/Placeholder").GetComponent<TextRenderer>();
				}
				return placeholderTextRenderer;
			}
		}

		/// <summary>Enables editing of text with rich text tags</summary>
		public bool RichTextEditing
		{
			get { return richTextEditing; }
			set
			{
				if(richTextEditing != value)
				{
					richTextEditing = value;
					Engine.InitializeTextRenderers();

					if(ShouldUseRichText)
					{
						Engine.InitializeRichTextEditing(text);
					}
				}
			}
		}

		/// <summary>Indicates if the Rich Text should be used on this inputfield/summary>
		public bool ShouldUseRichText
		{
			get { return (richTextEditing || emojisAllowed); }
		}

		/// <summary>The rich text configuration</summary>
		public RichTextData RichTextConfig
		{
			get { return richTextConfig; }
			set
			{
				richTextConfig = value;

				if(ShouldUseRichText)
				{
					Engine.InitializeRichTextEditing(text);
				}
			}
		}

		/// <summary>The maximum amount of characters allowed, zero means infinite</summary>
		public int CharacterLimit
		{
			get { return characterLimit; }
			set
			{
				characterLimit = value;
				ApplyCharacterLimit(characterLimit);
				Engine.UpdateSettings();
			}
		}

		/// <summary>The maximum amount of lines allowed, zero means infinite</summary>
		public int LineLimit
		{
			get { return lineLimit; }
			set { lineLimit = value; }
		}

		/// <summary>Configuration preset for the content of this InputField</summary>
		public ContentType ContentType
		{
			get { return contentType; }
			set
			{
				contentType = value;
				UpdateContentType(contentType);
				Engine.UpdateSettings();

			}
		}

		/// <summary>The type of line</summary>
		public LineType LineType
		{
			get { return lineType; }
			set
			{
				lineType = value;
				Engine.InitializeTextRenderers();
				Engine.UpdateSettings();
			}
		}

		/// <summary>The type of autocapitalization</summary>
		public AutocapitalizationType AutocapitalizationType
		{
			get { return autocapitalizationType; }
			set
			{
				autocapitalizationType = value;
				Engine.UpdateSettings();
			}
		}

		/// <summary>The type of autofill</summary>
		public AutofillType AutofillType
		{
			get { return autofillType; }
			set
			{
				autofillType = value;
				Engine.UpdateSettings();
			}
		}

		/// <summary>The type of return key</summary>
		public ReturnKeyType ReturnKeyType
		{
			get { return returnKeyType; }
			set
			{
				returnKeyType = value;
				Engine.UpdateSettings();
			}
		}

		/// <summary>The type of input</summary>
		public InputType InputType
		{
			get { return inputType; }
			set
			{
				inputType = value;
				Engine.UpdateSettings();
			}
		}

		/// <summary>Indicates whether the password should be visible</summary>
		public bool VisiblePassword
		{
			get { return visiblePassword; }
			set
			{
				visiblePassword = value;
				Engine.MarkDirty();
			}
		}

		/// <summary>The keyboard on mobile to use</summary>
		public KeyboardType KeyboardType
		{
			get { return keyboardType; }
			set
			{
				keyboardType = value;
				Engine.UpdateSettings();
			}
		}

		/// <summary>The validation to use for the text</summary>
		public CharacterValidation CharacterValidation
		{
			get { return characterValidation; }
			set
			{
				characterValidation = value;
				Engine.UpdateSettings();
			}
		}

		/// <summary>The custom character validator to use for the text</summary>
		public CharacterValidator CharacterValidator
		{
			get { return characterValidator; }
			set
			{
				characterValidator = value;
				Engine.UpdateSettings();
			}
		}

		/// <summary>Indicates whether emojis should be allowed</summary>
		public bool EmojisAllowed
		{
			get { return emojisAllowed; }
			set
			{
				emojisAllowed = value;
				Engine.InitializeTextRenderers();

				if(ShouldUseRichText)
				{
					Engine.InitializeRichTextEditing(text);
				}

				Engine.UpdateSettings();
			}
		}

		/// <summary>Indicates whether rich text bindings should be allowed</summary>
		public bool RichTextBindingsAllowed
		{
			get { return richTextBindingsAllowed; }
			set
			{
				richTextBindingsAllowed = value;
				Engine.InitializeTextRenderers();

				if(ShouldUseRichText)
				{
					Engine.InitializeRichTextEditing(text);
				}

				Engine.UpdateSettings();
			}
		}

		/// <summary>The filter (if any) to use whenever text or caret position changes</summary>
		public LiveProcessingFilter LiveProcessingFilter
		{
			get { return liveProcessingFilter; }
			set { liveProcessingFilter = value; }
		}

		/// <summary>The filter (if any) to use whenever text or caret position changes</summary>
		public LiveDecorationFilter LiveDecorationFilter
		{
			get { return liveDecorationFilter; }
			set { liveDecorationFilter = value; }
		}

		/// <summary>The filter (if any) to use when input field has been deselected</summary>
		public PostDecorationFilter PostDecorationFilter
		{
			get { return postDecorationFilter; }
			set { postDecorationFilter = value; }
		}

		/// <summary>Determines which input event to use to select the inputfield</summary>
		public SelectionMode SelectionMode
		{
			get { return selectionMode; }
			set { selectionMode = value; }
		}

		/// <summary>Determines how to use drag events</summary>
		public DragMode DragMode
		{
			get { return dragMode; }
			set
			{
				if(dragMode != value)
				{
					dragMode = value;

					if(scrollArea != null)
					{
						scrollArea.DragMode = dragMode;
					}
				}
			}
		}

		/// <summary>Indicates where to position the caret when beginning edit mode</summary>
		public CaretOnBeginEdit CaretOnBeginEdit
		{
			get { return caretOnBeginEdit; }
			set { caretOnBeginEdit = value; }
		}

		/// <summary>The blink rate of the caret</summary>
		public float CaretBlinkRate
		{
			get { return caretBlinkRate; }
			set { caretBlinkRate = value; }
		}

		/// <summary>The width of the caret</summary>
		public float CaretWidth
		{
			get { return caretWidth; }
			set
			{
				caretWidth = value;

				if(Application.isPlaying)
				{
					Engine.RefreshBasicTextSelectionHandlerAppearance();
				}
			}
		}

		/// <summary>The color of the caret</summary>
		public Color CaretColor
		{
			get { return caretColor; }
			set
			{
				caretColor = value;

				if(Application.isPlaying)
				{
					Engine.RefreshBasicTextSelectionHandlerAppearance();
				}
			}
		}

		/// <summary>The color of the text selection</summary>
		public Color SelectionColor
		{
			get { return selectionColor; }
			set
			{
				selectionColor = value;

				if(Application.isPlaying)
				{
					Engine.RefreshBasicTextSelectionHandlerAppearance();
				}
			}
		}

		/// <summary>Indicates whether to draw the text selection behind the text</summary>
		public bool SelectionBehindText
		{
			get { return selectionBehindText; }
			set
			{
				selectionBehindText = value;

				if(Application.isPlaying)
				{
					Engine.RefreshTextSelectionRenderOrder();
				}
			}
		}

		/// <summary>The scroll behaviour when ending edit mode</summary>
		public ScrollBehaviourOnEndEdit ScrollBehaviourOnEndEdit
		{
			get { return scrollBehaviourOnEndEdit; }
			set { scrollBehaviourOnEndEdit = value; }
		}

		/// <summary>The speed to scroll text (Canvas pixels per second)</summary>
		public float ScrollSpeed
		{
			get { return scrollSpeed; }
			set { scrollSpeed = value; }
		}

		// <summary>The maximum time it can take to scroll to caret position (in seconds)
		public float MaxScrollTransitionTime
		{
			get { return maxScrollTransitionTime; }
			set { maxScrollTransitionTime = value; }
		}

		/// <summary>The additional scroll sensitivity when dragging out of bounds</summary>
		public float FastScrollSensitivity
		{
			get { return fastScrollSensitivity; }
			set { fastScrollSensitivity = value; }
		}

		/// <summary>The minimal width of the InputField</summary>
		public float ResizeMinWidth
		{
			get { return resizeMinWidth; }
			set { resizeMinWidth = value; }
		}

		/// <summary>The maximum width of the InputField, zero means infinite</summary>
		public float ResizeMaxWidth
		{
			get { return resizeMaxWidth; }
			set { resizeMaxWidth = value; }
		}

		/// <summary>The minimal height of the InputField</summary>
		public float ResizeMinHeight
		{
			get { return resizeMinHeight; }
			set { resizeMinHeight = value; }
		}

		/// <summary>The maximum height of the InputField, zero means infinite</summary>
		public float ResizeMaxHeight
		{
			get { return resizeMaxHeight; }
			set { resizeMaxHeight = value; }
		}

		/// <summary>Event used for selection change</summary>
		public SelectionChangedEvent OnSelectionChanged
		{
			get { return onSelectionChanged; }
			set { onSelectionChanged = value; }
		}

		/// <summary>Event used for begin edit</summary>
		public BeginEditEvent OnBeginEdit
		{
			get { return onBeginEdit; }
			set { onBeginEdit = value; }
		}

		/// <summary>Event used for end edit</summary>
		public EndEditEvent OnEndEdit
		{
			get { return onEndEdit; }
			set { onEndEdit = value; }
		}

		/// <summary>Event used for value change</summary>
		public ValueChangedEvent OnValueChanged
		{
			get { return onValueChanged; }
			set { onValueChanged = value; }
		}

		/// <summary>Event used for value change</summary>
		public CaretPositionChangedEvent OnCaretPositionChanged
		{
			get { return onCaretPositionChanged; }
			set { onCaretPositionChanged = value; }
		}

		/// <summary>Event used for text selection change</summary>
		public TextSelectionChangedEvent OnTextSelectionChanged
		{
			get { return onTextSelectionChanged; }
			set { onTextSelectionChanged = value; }
		}

		/// <summary>Event used for size change</summary>
		public SizeChangedEvent OnSizeChanged
		{
			get { return onSizeChanged; }
			set { onSizeChanged = value; }
		}

		/// <summary>Event used when a special key has been pressed</summary>
		public SpecialKeyPressedEvent OnSpecialKeyPressed
		{
			get { return onSpecialKeyPressed; }
			set { onSpecialKeyPressed = value; }
		}

		/// <summary>Event used when the text has been tapped</summary>
		public TextTapEvent OnTextTap
		{
			get { return onTextTap; }
			set { onTextTap = value; }
		}

		/// <summary>Event used when an ActionBar action has been triggered</summary>
		public ActionBarActionEvent OnActionBarAction
		{
			get { return onActionBarAction; }
			set { onActionBarAction = value; }
		}

		/// <summary>Indicates if the ActionBar should be used</summary>
		public bool ActionBarEnabled
		{
			get { return actionBarEnabled; }
			set { actionBarEnabled = value; }
		}

		/// <summary>Indicates if the ActionBar can be used on this inputfield on this platform</summary>
		public bool CanUseActionBar
		{
			get { return (actionBarEnabled && Settings.ActionBarAllowed); }
		}

		/// <summary>Indicates if the cut option should be enabled in the ActionBar</summary>
		public bool ActionBarCut
		{
			get { return actionBarCut; }
			set { actionBarCut = value; }
		}

		/// <summary>Indicates if the copy option should be enabled in the ActionBar</summary>
		public bool ActionBarCopy
		{
			get { return actionBarCopy; }
			set { actionBarCopy = value; }
		}

		/// <summary>Indicates if the paste option should be enabled in the ActionBar</summary>
		public bool ActionBarPaste
		{
			get { return actionBarPaste; }
			set { actionBarPaste = value; }
		}

		/// <summary>Indicates if the select all option should be enabled in the ActionBar</summary>
		public bool ActionBarSelectAll
		{
			get { return actionBarSelectAll; }
			set { actionBarSelectAll = value; }
		}

		/// <summary>Indicates if the Selection Cursors (handles for selection start and end) should be used</summary>
		public bool TouchSelectionCursorsEnabled
		{
			get { return touchSelectionCursorsEnabled; }
			set { touchSelectionCursorsEnabled = value; }
		}

		/// <summary>Indicates if the Touch Selection Cursors can be used on this inputfield on this platform</summary>
		public bool CanUseTouchSelectionCursors
		{
			get { return (touchSelectionCursorsEnabled && Settings.TouchTextSelectionAllowed); }
		}

		/// <summary>The clamp mode of the mobile cursors</summary>
		public CursorClampMode CursorClampMode
		{
			get { return cursorClampMode; }
			set { cursorClampMode = value; }
		}

		/// <summary>The next input field (if any) to switch to when pressing the done button on the TouchScreenKeyboard</summary>
		public AdvancedInputField NextInputField
		{
			get { return nextInputField; }
			set { nextInputField = value; }
		}

		/// <summary>Indicates if this input field is initialized</summary>
		public bool Initialized { get { return initialized; } }

		/// <summary>Indicates whether next deselect event should be blocked</summary>
		public bool ShouldBlockDeselect { get; set; }

		/// <summary>Indicates if the Awake() method is currently being called on this instance</summary>
		internal bool WithinAwake { get; set; }

		/// <summary>The visibility mode for the scroll bars</summary>
		public ScrollBarVisibilityMode ScrollBarsVisibilityMode
		{
			get { return scrollBarsVisibilityMode; }
			set
			{
				scrollBarsVisibilityMode = value;

				ScrollArea scrollArea = TextAreaTransform.GetComponent<ScrollArea>();
				scrollArea.HorizontalScrollbarVisibility = value;
				scrollArea.VerticalScrollbarVisibility = value;
			}
		}

		/// <summary>The Canvas</summary>
		internal Canvas Canvas
		{
			get
			{
				if(canvas == null)
				{
					canvas = GetComponentInParent<Canvas>();
				}

				return canvas;
			}
		}

		/// <summary>Indicates if the user is currently pressing this input field</summary>
		internal bool UserPressing { get; private set; }

		/// <summary>Indicates if this input field is read only</summary>
		public bool ReadOnly
		{
			get { return readOnly; }
			set
			{
				if(readOnly != value)
				{
					readOnly = value;

					if(Selected)
					{
						if(readOnly)
						{
							Engine.CloseKeyboard();
						}
						else
						{
							Engine.LoadKeyboard();
						}
					}
				}
			}
		}

		/// <summary>The main text string</summary>
		public string Text
		{
			get
			{
				if(text == null) { text = string.Empty; }
				return text;
			}
			set
			{
				if(value == null) { value = string.Empty; }
				SetText(value);
			}
		}

		internal void UpdateTextProperty(string text)
		{
			this.text = text;
		}

		/// <summary>The caret position</summary>
		public int CaretPosition
		{
			get
			{
				return GetTextCaretPosition();
			}
			set
			{
				SetTextCaretPosition(value);
			}
		}

		/// <summary>The text selection start position</summary>
		public int TextSelectionStartPosition
		{
			get
			{
				return GetTextSelectionStartPosition();
			}
			set
			{
				SetTextSelectionStartPosition(value);
			}
		}

		/// <summary>The text selection end position</summary>
		public int TextSelectionEndPosition
		{
			get
			{
				return GetTextSelectionEndPosition();
			}
			set
			{
				SetTextSelectionEndPosition(value);
			}
		}

		/// <summary>The rich text string</summary>
		public string RichText
		{
			get
			{
				return Engine.RichText;
			}
			set
			{
				if(value == null) { value = string.Empty; }
				SetRichText(value);
			}
		}

		/// <summary>The placeholder text string</summary>
		public string PlaceHolderText
		{
			get { return placeholderText; }
			set
			{
				placeholderText = value;
				PlaceholderTextRenderer.Text = placeholderText;
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
				if(placeholderTextRenderer is TMProTextRenderer && string.IsNullOrEmpty(placeholderText))
				{
					placeholderTextRenderer.Text = " "; //Workaround for TextMeshPro character data issue with empty text
				}
#endif
				if(initialized)
				{
					Engine.MarkDirty();
				}
			}
		}

		/// <summary>Indicates if some text is currently selected</summary>
		public bool HasSelection
		{
			get
			{
				return Engine.HasSelection;
			}
		}

		/// <summary>The selected text string</summary>
		public string SelectedText
		{
			get
			{
				return Engine.SelectedText;
			}
		}

		/// <summary>The text that is actually rendered</summary>
		public string RenderedText
		{
			get
			{
				return Engine.GetActiveTextRenderer().Text;
			}
		}

		/// <summary>The ActionBar if enabled</summary>
		public ActionBar ActionBar
		{
			get
			{
				return Engine.ActionBar;
			}
		}

		/// <summary>Indicates if the input field is currently selected/focused</summary>
		public bool Selected { get { return Engine.Selected; } }

		/// <summary>Indicates if autocorrection should be used</summary>
		public bool AutoCorrection { get { return inputType == InputType.AUTOCORRECT; } }

		/// <summary>Indicates is input should be secure</summary>
		public bool Secure { get { return inputType == InputType.PASSWORD; } }

		/// <summary>Indicates if line type is multiline</summary>
		public bool Multiline { get { return lineType != LineType.SINGLE_LINE; } }

		/// <summary>Indicates if a next inputfield has been set</summary>
		public bool HasNext { get { return nextInputField != null; } }

		/// <summary>Indicates if LiveProcessing is active</summary>
		public bool LiveProcessing
		{
			get { return liveProcessingFilter != null; }
		}

		/// <summary>Indicates if LiveDecoration is active</summary>
		public bool LiveDecoration
		{
			get { return liveDecorationFilter != null; }
		}

		/// <summary>Indicates if PostDecoration is active</summary>
		public bool PostDecoration
		{
			get { return postDecorationFilter != null; }
		}

		/// <summary>Indicates if the Enter/Done key should submit</summary>
		public bool ShouldSubmit
		{
			get { return (lineType != LineType.MULTILINE_NEWLINE); }
		}

		/// <summary>Indicates if input field is a password field</summary>
		public bool IsPasswordField
		{
			get { return inputType == InputType.PASSWORD; }
		}
		#endregion

		#region UNITY_METHODS
		protected override void Awake()
		{
			WithinAwake = true;
			base.Awake();
			EnsureInitialization();
			WithinAwake = false;
		}

		private void OnApplicationPause(bool pause)
		{
			if(!Application.isPlaying) { return; }
			Engine.OnApplicationPause(pause);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			if(!Application.isPlaying) { return; }
			EnsureInitialization();
			Engine.OnEnable();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			if(!Application.isPlaying) { return; }
			Engine.OnDisable();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if(!Application.isPlaying) { return; }
			Engine.OnDestroy();
		}

		private void Update()
		{
			if(!Application.isPlaying) { return; }
			Engine.OnUpdate();
		}

		private void LateUpdate()
		{
			if(!Application.isPlaying) { return; }
			Engine.OnLateUpdate();
		}

		protected override void OnRectTransformDimensionsChange()
		{
			base.OnRectTransformDimensionsChange();
			if(!Application.isPlaying) { return; }
			Engine.OnRectTransformDimensionsChange();
		}
		#endregion

		#region PUBLIC_METHODS
		/// <summary>Gets the text of this input field</summary>
		/// <returns>The current text value of this input field</returns>
		public string GetText()
		{
			return text;
		}

		/// <summary>Sets the text of this input field</summary>
		/// <param name="text">The text value</param>
		/// <param name="invokeTextChangeEvent">Indicates whether this method should invoke the Text Change event</param>
		public void SetText(string text, bool invokeTextChangeEvent = false)
		{
			if(text == null) { text = string.Empty; }

			if(this.text != text)
			{
				Engine.SetText(text, invokeTextChangeEvent);
				Engine.SetSelection(0, 0);

				if(Application.isPlaying)
				{
					if(ShouldUseRichText)
					{
						Engine.RichTextProcessor.SetupRichText(text);
						Engine.SetRichText(Engine.RichTextProcessor.RichText);
						Engine.SetRichTextSelection(0, 0);
					}
					else if(LiveDecoration)
					{
						string processedText = LiveDecorationFilter.ProcessText(text, CaretPosition);
						if(processedText != null)
						{
							Engine.SetProcessedText(processedText);
							Engine.SetProcessedSelection(0, 0);
						}
					}
					else if(PostDecoration && !Selected)
					{
						if(PostDecorationFilter.ProcessText(text, out string processedText))
						{
							Engine.SetProcessedText(processedText);
							Engine.SetProcessedSelection(0, 0);
						}
					}
				}
			}
		}

		/// <summary>Sets the text of this input field while trying to preserve current caret/selection positions</summary>
		/// <param name="text">The text value</param>
		/// <param name="invokeTextChangeEvent">Indicates whether this method should invoke the Text Change event</param>
		public void SetTextAndPreserveSelection(string text, bool invokeTextChangeEvent = false)
		{
			if(text == null) { text = string.Empty; }

			if(this.text != text)
			{
				int selectionStartPosition = Engine.SelectionStartPosition;
				int selectionEndPosition = Engine.SelectionEndPosition;
				Engine.SetText(text, invokeTextChangeEvent);

				int length = text.Length;
				selectionStartPosition = Mathf.Min(selectionStartPosition, length);
				selectionEndPosition = Mathf.Min(selectionEndPosition, length);
				Engine.SetSelection(selectionStartPosition, selectionEndPosition);

				if(Application.isPlaying)
				{
					if(ShouldUseRichText)
					{
						Engine.RichTextProcessor.SetupRichText(text);
						Engine.SetRichText(Engine.RichTextProcessor.RichText);

						int richTextSelectionStartPosition = DeterminePositionInRichText(selectionStartPosition);
						int richTextSelectionEndPosition = DeterminePositionInRichText(selectionEndPosition);
						Engine.SetRichTextSelection(richTextSelectionStartPosition, richTextSelectionEndPosition);
					}
					else if(LiveDecoration)
					{
						string processedText = LiveDecorationFilter.ProcessText(text, CaretPosition);
						if(processedText != null)
						{
							Engine.SetProcessedText(processedText);
							int processedSelectionStartPosition = liveDecorationFilter.DetermineProcessedCaret(text, selectionStartPosition, processedText);
							int processedSelectionEndPosition = liveDecorationFilter.DetermineProcessedCaret(text, selectionEndPosition, processedText);
							Engine.SetProcessedSelection(processedSelectionStartPosition, processedSelectionEndPosition);
						}
					}
					else if(PostDecoration && !Selected)
					{
						if(PostDecorationFilter.ProcessText(Text, out string processedText))
						{
							Engine.SetProcessedText(processedText);
							Engine.SetProcessedSelection(0, 0);
						}
					}
				}
			}
		}

		/// <summary>Gets current caret position</summary>
		/// <returns>The caret position</returns>
		public int GetTextCaretPosition()
		{
			return Engine.CaretPosition;
		}

		/// <summary>Sets the caret position</summary>
		/// <param name="caretPosition">The caret position value</param>
		/// <param name="invokeCaretPositionChangedEvent">Indicates whether this method should invoke the Caret Position Change event</param>
		public void SetTextCaretPosition(int caretPosition, bool invokeCaretPositionChangedEvent = false)
		{
			SetTextSelection(caretPosition, caretPosition, invokeCaretPositionChangedEvent);
		}

		/// <summary>Gets current text selection start position</summary>
		/// <returns>The selection start position</returns>
		public int GetTextSelectionStartPosition()
		{
			return Engine.SelectionStartPosition;
		}

		/// <summary>Sets the text selection start position</summary>
		/// <param name="selectionEndPosition">The selection start position value</param>
		/// <param name="invokeTextSelectionChangedEvent">Indicates whether this method should invoke the Text Selection Change event</param>
		public void SetTextSelectionStartPosition(int selectionStartPosition, bool invokeTextSelectionChangedEvent = false)
		{
			if(selectionStartPosition < Engine.SelectionEndPosition)
			{
				SetTextSelection(selectionStartPosition, Engine.SelectionEndPosition, invokeTextSelectionChangedEvent);
			}
			else
			{
				SetTextSelection(selectionStartPosition, selectionStartPosition, invokeTextSelectionChangedEvent);
			}
		}

		/// <summary>Gets current text selection end position</summary>
		/// <returns>The selection end position</returns>
		public int GetTextSelectionEndPosition()
		{
			return Engine.SelectionEndPosition;
		}

		/// <summary>Sets the text selection end position</summary>
		/// <param name="selectionEndPosition">The selection end position value</param>
		/// <param name="invokeTextSelectionChangedEvent">Indicates whether this method should invoke the Text Selection Change event</param>
		public void SetTextSelectionEndPosition(int selectionEndPosition, bool invokeTextSelectionChangedEvent = false)
		{
			if(selectionEndPosition > Engine.SelectionStartPosition)
			{
				SetTextSelection(Engine.SelectionStartPosition, selectionEndPosition, invokeTextSelectionChangedEvent);
			}
			else
			{
				SetTextSelection(selectionEndPosition, selectionEndPosition, invokeTextSelectionChangedEvent);
			}
		}

		/// <summary>Sets the text selection</summary>
		/// <param name="selectionStartPosition">The selection start position value</param>
		/// <param name="selectionEndPosition">The selection end position value</param>
		/// <param name="invokeTextSelectionChangedEvent">Indicates whether this method should invoke the Text Selection Change event</param>
		public void SetTextSelection(int selectionStartPosition, int selectionEndPosition, bool invokeTextSelectionChangdeEvent = false)
		{
			selectionStartPosition = Mathf.Clamp(selectionStartPosition, 0, text.Length);
			selectionEndPosition = Mathf.Clamp(selectionEndPosition, 0, text.Length);
			Engine.SetSelection(selectionStartPosition, selectionEndPosition, false, invokeTextSelectionChangdeEvent);

			if(ShouldUseRichText)
			{
				TextEditFrame textEditFrame = new TextEditFrame(Engine.Text, selectionStartPosition, selectionEndPosition);
				Engine.ApplyTextEditFrame(textEditFrame);
			}
		}

		/// <summary>Sets the rich text of this input field</summary>
		/// <param name="richText">The text value</param>
		public void SetRichText(string richText)
		{
			if(richText == null) { richText = string.Empty; }

			if(ShouldUseRichText)
			{
				Engine.InitializeRichTextEditing(richText);
			}
			else
			{
				Debug.LogWarning("Rich Text Editing is not enabled");
			}
		}

		/// <summary>Sets the caret position to the start of text</summary>
		/// <param name="invokeCaretPositonChangeEvent">Indicates whether this method should invoke the Caret Position Change event</param>
		public void SetCaretToTextStart(bool invokeCaretPositonChangeEvent = false)
		{
			if(text != null)
			{
				SetTextCaretPosition(0, invokeCaretPositonChangeEvent);
			}
			else
			{
				Debug.LogWarning("Couldn't set caret to text start, text is null");
			}
		}

		/// <summary>Sets the caret position to the end of text</summary>
		/// <param name="invokeCaretPositonChangeEvent">Indicates whether this method should invoke the Caret Position Change event</param>
		public void SetCaretToTextEnd(bool invokeCaretPositonChangeEvent = false)
		{
			if(text != null)
			{
				SetTextCaretPosition(text.Length, invokeCaretPositonChangeEvent);
			}
			else
			{
				Debug.LogWarning("Couldn't set caret to text end, text is null");
			}
		}

		/// <summary>Clears the InputField text</summary>
		public void Clear()
		{
			Text = string.Empty;
		}

		/// <summary>Manually selects this inputfield (call this to select this inputfield programmatically)</summary>
		public void ManualSelect(BeginEditReason beginEditReason = BeginEditReason.PROGRAMMATIC_SELECT)
		{
			if(!interactable)
			{
				Debug.LogWarningFormat("InputField is not interactable, it won't be selected");
			}
			else if(initialized)
			{
				Engine.ManualSelect(beginEditReason);
			}
			else
			{
				Debug.LogWarningFormat("Couldn't select input field, the input field is not initialized yet");
			}
		}

		/// <summary>Manually deselects this inputfield (call this to deselect this inputfield programmatically)</summary>
		public void ManualDeselect(EndEditReason endEditReason = EndEditReason.PROGRAMMATIC_DESELECT)
		{
			if(initialized)
			{
				Engine.Deselect(endEditReason);
			}
			else
			{
				Debug.LogWarningFormat("Couldn't deselect input field, the input field is not initialized yet");
			}
		}

		/// <summary>Toggles bold in current text selection</summary>
		public void ToggleBold()
		{
			Engine.ToggleTagPair("<b>", "</b>");
		}

		/// <summary>Toggles italic in current text selection</summary>
		public void ToggleItalic()
		{
			Engine.ToggleTagPair("<i>", "</i>");
		}

		/// <summary>Toggles lowercase in current text selection</summary>
		public void ToggleLowercase()
		{
			Engine.ToggleTagPair("<lowercase>", "</lowercase>");
		}

		/// <summary>Toggles non-breaking spaces in current text selection</summary>
		public void ToggleNonBreakingSpaces()
		{
			Engine.ToggleTagPair("<nobr>", "</nobr>");
		}

		/// <summary>Toggles no parse in current text selection</summary>
		public void ToggleNoParse()
		{
			Engine.ToggleTagPair("<noparse>", "</noparse>");
		}

		/// <summary>Toggles strikethrough in current text selection</summary>
		public void ToggleStrikethrough()
		{
			Engine.ToggleTagPair("<s>", "</s>");
		}

		/// <summary>Toggles small caps in current text selection</summary>
		public void ToggleSmallCaps()
		{
			Engine.ToggleTagPair("<smallcaps>", "</smallcaps>");
		}

		/// <summary>Toggles subscript in current text selection</summary>
		public void ToggleSubscript()
		{
			Engine.ToggleTagPair("<sub>", "</sub>");
		}

		/// <summary>Toggles superscript in current text selection</summary>
		public void ToggleSuperscript()
		{
			Engine.ToggleTagPair("<sup>", "</sup>");
		}

		/// <summary>Toggles underline in current text selection</summary>
		public void ToggleUnderline()
		{
			Engine.ToggleTagPair("<u>", "</u>");
		}

		/// <summary>Toggles uppercase in current text selection</summary>
		public void ToggleUppercase()
		{
			Engine.ToggleTagPair("<uppercase>", "</uppercase>");
		}

		/// <summary>Toggles align with given parameter in current text selection</summary>
		public void ToggleAlign(string parameter)
		{
			Engine.ToggleTagPair(string.Format("<align={0}>", parameter), "</align>");
		}

		/// <summary>Toggles alpha with given parameter in current text selection</summary>
		public void ToggleAlpha(string parameter)
		{
			Engine.ToggleTagPair(string.Format("<alpha={0}>", parameter), "</alpha>");
		}

		/// <summary>Toggles color with given parameter in current text selection</summary>
		public void ToggleColor(string parameter)
		{
			Engine.ToggleTagPair(string.Format("<color={0}>", parameter), "</color>");
		}

		/// <summary>Toggles character space with given parameter in current text selection</summary>
		public void ToggleCharacterSpace(string parameter)
		{
			Engine.ToggleTagPair(string.Format("<cspace={0}>", parameter), "</cspace>");
		}

		/// <summary>Toggles font with given parameter in current text selection</summary>
		public void ToggleFont(string parameter)
		{
			Engine.ToggleTagPair(string.Format("<font={0}>", parameter), "</font>");
		}

		/// <summary>Toggles indent with given parameter in current text selection</summary>
		public void ToggleIndent(string parameter)
		{
			Engine.ToggleTagPair(string.Format("<indent={0}>", parameter), "</indent>");
		}

		/// <summary>Toggles line height with given parameter in current text selection</summary>
		public void ToggleLineHeight(string parameter)
		{
			Engine.ToggleTagPair(string.Format("<line-height={0}>", parameter), "</line-height>");
		}

		/// <summary>Toggles line indent with given parameter in current text selection</summary>
		public void ToggleLineIndent(string parameter)
		{
			Engine.ToggleTagPair(string.Format("<line-indent={0}>", parameter), "</line-indent>");
		}

		/// <summary>Toggles link with given parameter in current text selection</summary>
		public void ToggleLink(string parameter)
		{
			Engine.ToggleTagPair(string.Format("<link={0}>", parameter), "</link>");
		}

		/// <summary>Toggles margin with given parameter in current text selection</summary>
		public void ToggleMargin(string parameter)
		{
			Engine.ToggleTagPair(string.Format("<margin={0}>", parameter), "</margin>");
		}

		/// <summary>Toggles mark with given parameter in current text selection</summary>
		public void ToggleMark(string parameter)
		{
			Engine.ToggleTagPair(string.Format("<mark={0}>", parameter), "</mark>");
		}

		/// <summary>Toggles material with given parameter in current text selection</summary>
		public void ToggleMaterial(string parameter)
		{
			Engine.ToggleTagPair(string.Format("<material={0}>", parameter), "</material>");
		}

		/// <summary>Toggles monospace with given parameter in current text selection</summary>
		public void ToggleMonospace(string parameter)
		{
			Engine.ToggleTagPair(string.Format("<mspace={0}>", parameter), "</mspace>");
		}

		/// <summary>Toggles position with given parameter in current text selection</summary>
		public void TogglePosition(string parameter)
		{
			Engine.ToggleTagPair(string.Format("<pos={0}>", parameter), "</pos>");
		}

		/// <summary>Toggles size with given parameter in current text selection</summary>
		public void ToggleSize(string parameter)
		{
			Engine.ToggleTagPair(string.Format("<size={0}>", parameter), "</size>");
		}

		/// <summary>Toggles style with given parameter in current text selection</summary>
		public void ToggleStyle(string parameter)
		{
			Engine.ToggleTagPair(string.Format("<style={0}>", parameter), "</style>");
		}

		/// <summary>Toggles vertical offset with given parameter in current text selection</summary>
		public void ToggleVerticalOffset(string parameter)
		{
			Engine.ToggleTagPair(string.Format("<voffset={0}>", parameter), "</voffset>");
		}

		/// <summary>Toggles width with given parameter in current text selection</summary>
		public void ToggleWidth(string parameter)
		{
			Engine.ToggleTagPair(string.Format("<width={0}>", parameter), "</width>");
		}

		/// <summary>Updates the visual position of the caret</summary>
		public void UpdateCaretPosition()
		{
			Engine.UpdateCaretPosition();
		}

		/// <summary>Gets the RectTransform of the caret (when the input field is selected)</summary>
		public RectTransform GetCaretTransform()
		{
			return Engine.GetCaretTransform();
		}

		/// <summary>Refreshes the canvas reference (for example when then input field has been moved to another Canvas)</summary>
		public void RefreshCanvas()
		{
			canvas = GetComponentInParent<Canvas>();
		}

		/// <summary>Determines the position in rich text for given position in raw text</summary>
		/// <param name="textPosition">The position in raw text</param>
		/// <returns>The position in rich text</returns>
		public int DeterminePositionInRichText(int textPosition)
		{
			return Engine.RichTextProcessor.DeterminePositionInRichText(textPosition, RichText);
		}

		/// <summary>Replaces current text selection in rich text with given text</summary>
		/// <param name="textToInsert">The text to insert</param>
		public void ReplaceSelectedTextInRichText(string textToInsert)
		{
			Engine.ReplaceSelectedTextInRichText(textToInsert);
		}
		#endregion

		#region EDITOR_METHODS
		/// <summary>Editor method: Applies the character limit change. Don't call this method directly, use CharacterLimit property instead.</summary>
		/// <param name="characterLimit">The new character limit value</param>
		public void ApplyCharacterLimit(int characterLimit)
		{
			if(characterLimit > 0 && text.Length > characterLimit)
			{
				text = text.Substring(0, characterLimit);
				TextRenderer.Text = text;
				Text = text;
			}
		}

		/// <summary>Editor method: Applies the line limit change. Don't call this method directly, use LineLimit property instead.</summary></summary>
		/// <param name="lineLimit">The new line limit value</param>
		public void ApplyLineLimit(int lineLimit)
		{
			if(lineLimit > 0 && text.Length > 0)
			{
				TextRenderer.Text = text;
				while(TextRenderer.LineCount > lineLimit && text.Length > 0)
				{
					text = text.Substring(0, text.Length - 1);
					TextRenderer.Text = text;
				}
			}
		}

		/// <summary>Editor method: Applies the content type change. Don't call this method directly, use ContentType property instead.</summary>
		/// <param name="contentType">The new content type value</param>
		public void UpdateContentType(ContentType contentType)
		{
			switch(contentType)
			{
				case ContentType.STANDARD:
					{
						// Don't enforce line type for this content type.
						inputType = InputType.STANDARD;
						keyboardType = KeyboardType.DEFAULT;
						characterValidation = CharacterValidation.NONE;
						break;
					}
				case ContentType.AUTOCORRECTED:
					{
						// Don't enforce line type for this content type.
						inputType = InputType.AUTOCORRECT;
						keyboardType = KeyboardType.DEFAULT;
						characterValidation = CharacterValidation.NONE;
						break;
					}
				case ContentType.INTEGER_NUMBER:
					{
						lineType = LineType.SINGLE_LINE;
						inputType = InputType.STANDARD;
						keyboardType = KeyboardType.NUMBER_PAD;
						characterValidation = CharacterValidation.INTEGER;
						break;
					}
				case ContentType.DECIMAL_NUMBER:
					{
						lineType = LineType.SINGLE_LINE;
						inputType = InputType.STANDARD;
						keyboardType = KeyboardType.NUMBERS_AND_PUNCTUATION;
						characterValidation = CharacterValidation.DECIMAL;
						break;
					}
				case ContentType.DECIMAL_NUMBER_FORCE_POINT:
					{
						lineType = LineType.SINGLE_LINE;
						inputType = InputType.STANDARD;
						keyboardType = KeyboardType.NUMBERS_AND_PUNCTUATION;
						characterValidation = CharacterValidation.DECIMAL_FORCE_POINT;
						break;
					}
				case ContentType.ALPHANUMERIC:
					{
						lineType = LineType.SINGLE_LINE;
						inputType = InputType.STANDARD;
						keyboardType = KeyboardType.ASCII_CAPABLE;
						characterValidation = CharacterValidation.ALPHANUMERIC;
						break;
					}
				case ContentType.NAME:
					{
						lineType = LineType.SINGLE_LINE;
						inputType = InputType.STANDARD;
						keyboardType = KeyboardType.DEFAULT;
						characterValidation = CharacterValidation.NAME;
						break;
					}
				case ContentType.EMAIL_ADDRESS:
					{
						lineType = LineType.SINGLE_LINE;
						inputType = InputType.STANDARD;
						keyboardType = KeyboardType.EMAIL_ADDRESS;
						characterValidation = CharacterValidation.EMAIL_ADDRESS;
						break;
					}
				case ContentType.PASSWORD:
					{
						lineType = LineType.SINGLE_LINE;
						inputType = InputType.PASSWORD;
						keyboardType = KeyboardType.DEFAULT;
						characterValidation = CharacterValidation.NONE;
						break;
					}
				case ContentType.PIN:
					{
						lineType = LineType.SINGLE_LINE;
						inputType = InputType.PASSWORD;
						keyboardType = KeyboardType.NUMBER_PAD;
						characterValidation = CharacterValidation.INTEGER;
						break;
					}
				case ContentType.IP_ADDRESS:
					{
						lineType = LineType.SINGLE_LINE;
						inputType = InputType.STANDARD;
						keyboardType = KeyboardType.PHONE_PAD;
						characterValidation = CharacterValidation.IP_ADDRESS;
						break;
					}
				case ContentType.SENTENCE:
					{
						lineType = LineType.MULTILINE_NEWLINE;
						inputType = InputType.STANDARD;
						keyboardType = KeyboardType.DEFAULT;
						characterValidation = CharacterValidation.SENTENCE;
						break;
					}
				default:
					{
						// Includes Custom type. Nothing should be enforced.
						break;
					}
			}
		}
		#endregion

		#region INITIALIZATION
		/// <summary>Initializes the InputField</summary>
		internal void Initialize()
		{
			if(Application.isPlaying)
			{
				if(!NativeKeyboardManager.InstanceValid) //Create global instance if not created yet
				{
					NativeKeyboardManager.CreateInstance();
				}

				engine = new InputFieldEngine(this);
				engine.Initialize();
			}
			else
			{
				engine = new InputFieldEngine(this);
			}

			initialized = true;
		}

		internal void EnsureInitialization()
		{
			if(!initialized) { Initialize(); }
#if UNITY_EDITOR && UNITY_2019_3_OR_NEWER
			else if(UnityEditor.EditorSettings.enterPlayModeOptionsEnabled && (engine == null || !engine.Initialized))
			{
				Initialize();
			}
#endif
		}
		#endregion

		#region INTERFACE_METHODS
		public void OnPointerClick(PointerEventData eventData) { }

		public void OnBeginDrag(PointerEventData eventData)
		{
			if(!Application.isPlaying) { return; }
			Engine.OnBeginDrag(eventData);
		}

		public void OnDrag(PointerEventData eventData)
		{
			if(!Application.isPlaying) { return; }
			Engine.OnDrag(eventData);
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if(!Application.isPlaying) { return; }
			Engine.OnEndDrag(eventData);
		}

		public override void OnPointerDown(PointerEventData eventData)
		{
			base.OnPointerDown(eventData);
			if(!Application.isPlaying) { return; }
			Engine.OnPointerDown(eventData);
			UserPressing = true;
		}

		public override void OnPointerUp(PointerEventData eventData)
		{
			base.OnPointerUp(eventData);
			if(!Application.isPlaying) { return; }
			Engine.OnPointerUp(eventData);
			UserPressing = false;
		}

		public override void OnDeselect(BaseEventData eventData)
		{
			base.OnDeselect(eventData);
			if(!Application.isPlaying) { return; }
			Engine.OnDeselect(eventData);
		}

		public void OnUpdateSelected(BaseEventData eventData)
		{
			if(!Application.isPlaying) { return; }
			Engine.OnUpdateSelected(eventData);
		}

		public override void Select()
		{
			if(!Application.isPlaying) { return; }
			Engine.OnSelect();
		}
		#endregion
	}
}
