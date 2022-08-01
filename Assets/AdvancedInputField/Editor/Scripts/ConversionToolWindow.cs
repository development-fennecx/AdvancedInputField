using System;
using System.Collections.Generic;
using System.Reflection;
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
using TMPro;
#endif
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedInputFieldPlugin.Editor
{
	public enum InputFieldFormat { DEPRECATED_ADVANCEDINPUTFIELD, UNITY_INPUTFIELD, TEXTMESHPRO_INPUTFIELD, ADVANCEDINPUTFIELD_UNITY_TEXT, ADVANCEDINPUTFIELD_TEXTMESHPRO_TEXT }

	public class InputFieldData
	{
		public Navigation navigation;
		public Selectable.Transition transition;
		public ColorBlock colors;
		public SpriteState spriteState;
		public AnimationTriggers animationTriggers;
		public bool? interactable;

		public string text;
		public string placeholder;
		public int? characterLimit;
		public ContentType? contentType;
		public LineType? lineType;
		public InputType? inputType;
		public KeyboardType? keyboardType;
		public CharacterValidation? characterValidation;
		public bool? emojisAllowed;
		public LiveDecorationFilter liveProcessingFilter;
		public PostDecorationFilter postProcessingFilter;
		public CaretOnBeginEdit? caretOnBeginEdit;
		public float? caretBlinkRate;
		public float? caretWidth;
		public Color? caretColor;
		public Color? selectionColor;
		public bool? readOnly;
		public ScrollBehaviourOnEndEdit? scrollBehaviourOnEndEdit;
		public ScrollBarVisibilityMode? scrollBarsVisiblityMode;
		public float? scrollSpeed;
		public float? fastScrollSensitivity;
		public AdvancedInputField nextInputField;
		public AdvancedInputField.SelectionChangedEvent onSelectionChangedEvent;
		public AdvancedInputField.BeginEditEvent onBeginEditEvent;
		public AdvancedInputField.EndEditEvent onEndEditEvent;
		public AdvancedInputField.ValueChangedEvent onValueChangedEvent;
		public AdvancedInputField.CaretPositionChangedEvent onCaretPositionChangedEvent;
		public AdvancedInputField.TextSelectionChangedEvent onTextSelectionChangedEvent;
		public bool? actionBarEnabled;
		public bool? actionBarCut;
		public bool? actionBarCopy;
		public bool? actionBarPaste;
		public bool? actionBarSelectAll;
		public bool? selectionCursorsEnabled;
		public AutocapitalizationType? autocapitalizationType;
		public AutofillType? autofillType;
	}

	public class TextRendererData
	{
		public string text;
		public Color color;
		public Font font;
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
		public TMP_FontAsset tmpFont;
#endif
		public FontStyle? fontStyle;
		public float? fontSize;
		public float? lineSpacing;
		public TextAnchor? textAlignment;
		public bool? autoSize;
		public float? minFontSize;
		public float? maxFontSize;
	}

	public class ConversionToolWindow: EditorWindow
	{
		private const string UNITY_INPUTFIELD_PREFAB_PATH = "AdvancedInputField/Prefabs/Unity_InputField";
		private const string TEXTMESHPRO_INPUTFIELD_PREFAB_PATH = "AdvancedInputField/Prefabs/TextMeshPro_InputField";
		private const string ADVANCEDINPUTFIELD_UNITY_TEXT_PREFAB_PATH = "AdvancedInputField/Prefabs/AdvancedInputField_Unity_Text";
		private const string ADVANCEDINPUTFIELD_TEXTMESHPRO_TEXT_PREFAB_PATH = "AdvancedInputField/Prefabs/AdvancedInputField_TextMeshPro_Text";

		private InputFieldFormat from;
		private InputFieldFormat to;
		private bool destroyOriginalInputFields = true;
		private bool preserveSiblingIndexes = true;
		private bool textMeshProAvailable = false;

		[MenuItem("Tools/Advanced Input Field/ConversionTool", false, 101)]
		public static void Init()
		{
			ConversionToolWindow window = (ConversionToolWindow)EditorWindow.GetWindowWithRect(typeof(ConversionToolWindow), new Rect(0, 0, 360, 260), true, "Conversion Tool");
			window.Show();
		}

		private void OnGUI()
		{
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
			textMeshProAvailable = true;
#endif

			EditorGUILayout.LabelField("Convert InputFields", EditorStyles.boldLabel);
			EditorStyles.label.wordWrap = true;
			EditorGUILayout.LabelField("You can use this tool to convert all InputFields in the scene to another format.\n\'Deprecated\' format means AdvancedInputFields made prior to version 1.6.", EditorStyles.label);
			from = (InputFieldFormat)EditorGUILayout.EnumPopup("From:", from);
			to = (InputFieldFormat)EditorGUILayout.EnumPopup("To:", to);
			destroyOriginalInputFields = EditorGUILayout.Toggle("Destroy original InputFields when done", destroyOriginalInputFields);
			preserveSiblingIndexes = EditorGUILayout.Toggle("Preserve sibling indexes", preserveSiblingIndexes);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("NOTE: some properties can't be transferred between formats, because no equivalent property exists or the types differ too much. \nIn these cases the property will have the default value.", EditorStyles.label);
			EditorGUILayout.Space();

			if(GUILayout.Button("Convert all InputFields in current Scene"))
			{
				if(from == to)
				{
					EditorUtility.DisplayDialog("Error", "\'From\' and \'to\' can't be the same!", "OK");
				}
				else if(to == InputFieldFormat.DEPRECATED_ADVANCEDINPUTFIELD)
				{
					EditorUtility.DisplayDialog("Error", "You can't convert to the deprecated format", "OK");
				}
				else if(!textMeshProAvailable && (from == InputFieldFormat.TEXTMESHPRO_INPUTFIELD || to == InputFieldFormat.TEXTMESHPRO_INPUTFIELD || from == InputFieldFormat.ADVANCEDINPUTFIELD_TEXTMESHPRO_TEXT || to == InputFieldFormat.ADVANCEDINPUTFIELD_TEXTMESHPRO_TEXT))
				{
					EditorUtility.DisplayDialog("Error", "TextMeshPro hasn't been added to the project yet.\nPlease install it using the Package Manager and add \'ADVANCEDINPUTFIELD_TEXTMESHPRO\' to the \'Scripting Define Symbols\' in the Player Settings.", "OK");
				}
				else
				{
					int result = ConvertInputFields();
					EditorUtility.DisplayDialog("Info", "Converted " + result + " Input Fields.", "OK");
				}
			}
		}

		private int ConvertInputFields()
		{
			int result = -1;

			if(from == InputFieldFormat.UNITY_INPUTFIELD)
			{
				switch(to)
				{
					case InputFieldFormat.ADVANCEDINPUTFIELD_UNITY_TEXT: result = ConvertUnityInputFieldsToAdvancedInputField(ADVANCEDINPUTFIELD_UNITY_TEXT_PREFAB_PATH); break;
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
					case InputFieldFormat.ADVANCEDINPUTFIELD_TEXTMESHPRO_TEXT: result = ConvertUnityInputFieldsToAdvancedInputField(ADVANCEDINPUTFIELD_TEXTMESHPRO_TEXT_PREFAB_PATH); break;
					case InputFieldFormat.TEXTMESHPRO_INPUTFIELD: result = ConvertUnityInputFieldsToTMProInputField(); break;
#endif
				}
			}
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
			else if(from == InputFieldFormat.TEXTMESHPRO_INPUTFIELD)
			{
				switch(to)
				{
					case InputFieldFormat.UNITY_INPUTFIELD: result = ConvertTMProInputFieldsToUnityInputField(); break;
					case InputFieldFormat.ADVANCEDINPUTFIELD_UNITY_TEXT: result = ConvertTMProInputFieldsToAdvancedInputField(ADVANCEDINPUTFIELD_UNITY_TEXT_PREFAB_PATH); break;
					case InputFieldFormat.ADVANCEDINPUTFIELD_TEXTMESHPRO_TEXT: result = ConvertTMProInputFieldsToAdvancedInputField(ADVANCEDINPUTFIELD_TEXTMESHPRO_TEXT_PREFAB_PATH); break;
				}
			}
#endif
			else if(from == InputFieldFormat.ADVANCEDINPUTFIELD_UNITY_TEXT)
			{
				switch(to)
				{
					case InputFieldFormat.UNITY_INPUTFIELD: result = ConvertAdvancedInputFieldsUnityTextToUnityInputField(); break;
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
					case InputFieldFormat.TEXTMESHPRO_INPUTFIELD: result = ConvertAdvancedInputFieldsUnityTextToTMProInputField(); break;
					case InputFieldFormat.ADVANCEDINPUTFIELD_TEXTMESHPRO_TEXT: result = ConvertAdvancedInputFieldsUnityTextToAdvancedInputField(ADVANCEDINPUTFIELD_TEXTMESHPRO_TEXT_PREFAB_PATH); break;
#endif
				}
			}
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
			else if(from == InputFieldFormat.ADVANCEDINPUTFIELD_TEXTMESHPRO_TEXT)
			{
				switch(to)
				{
					case InputFieldFormat.UNITY_INPUTFIELD: result = ConvertAdvancedInputFieldsTMProTextToUnityInputField(); break;
					case InputFieldFormat.TEXTMESHPRO_INPUTFIELD: result = ConvertAdvancedInputFieldsTMProTextToTextMeshProInputField(); break;
					case InputFieldFormat.ADVANCEDINPUTFIELD_UNITY_TEXT: result = ConvertAdvancedInputFieldsTMProTextToAdvancedInputField(ADVANCEDINPUTFIELD_UNITY_TEXT_PREFAB_PATH); break;
				}
			}
#endif
			else if(from == InputFieldFormat.DEPRECATED_ADVANCEDINPUTFIELD)
			{
				switch(to)
				{
					case InputFieldFormat.UNITY_INPUTFIELD: result = ConvertDeprecatedAdvancedInputFieldsToUnityInputField(); break;
					case InputFieldFormat.ADVANCEDINPUTFIELD_UNITY_TEXT: result = ConvertDeprecatedAdvancedInputFieldsToAdvancedInputField(ADVANCEDINPUTFIELD_UNITY_TEXT_PREFAB_PATH); break;
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
					case InputFieldFormat.TEXTMESHPRO_INPUTFIELD: result = ConvertDeprecatedAdvancedInputFieldsToTMProInputField(); break;
					case InputFieldFormat.ADVANCEDINPUTFIELD_TEXTMESHPRO_TEXT: result = ConvertDeprecatedAdvancedInputFieldsToAdvancedInputField(ADVANCEDINPUTFIELD_TEXTMESHPRO_TEXT_PREFAB_PATH); break;
#endif
				}
			}

			return result;
		}

		#region UNITY_INPUTFIELD
		private int ConvertUnityInputFieldsToAdvancedInputField(string prefabPath)
		{
			List<InputField> inputFields = EditorUtil.FindObjectsOfTypeAll<InputField>(true);
			GameObject targetPrefab = Resources.Load<GameObject>(prefabPath);

			int length = inputFields.Count;
			for(int i = 0; i < length; i++)
			{
				InputField fromInputField = inputFields[i];
				InputFieldData inputFieldData = null;
				TextRendererData textRendererData = null;
				TextRendererData placeholderRendererData = null;
				RectTransform fromTransform = null;

				//From
				fromTransform = fromInputField.ExtractData(ref inputFieldData, ref textRendererData, ref placeholderRendererData);
				int siblingIndex = fromTransform.GetSiblingIndex();

				//To
				GameObject targetObject = Instantiate(targetPrefab);
				AdvancedInputField toInputField = targetObject.GetComponent<AdvancedInputField>();
				toInputField.ApplyData(fromTransform, inputFieldData, textRendererData, placeholderRendererData);

				TryCopyComponents(fromInputField.gameObject, toInputField.gameObject);

				if(destroyOriginalInputFields)
				{
					DestroyImmediate(fromInputField.gameObject);
				}
				else
				{
					fromInputField.name = fromInputField.name + "(Original)";
				}

				if(preserveSiblingIndexes)
				{
					toInputField.transform.SetSiblingIndex(siblingIndex);
				}
			}

			return length;
		}

#if ADVANCEDINPUTFIELD_TEXTMESHPRO
		private int ConvertUnityInputFieldsToTMProInputField()
		{
			List<InputField> inputFields = EditorUtil.FindObjectsOfTypeAll<InputField>(true);
			GameObject targetPrefab = Resources.Load<GameObject>(TEXTMESHPRO_INPUTFIELD_PREFAB_PATH);

			int length = inputFields.Count;
			for(int i = 0; i < length; i++)
			{
				InputField fromInputField = inputFields[i];
				InputFieldData inputFieldData = null;
				TextRendererData textRendererData = null;
				TextRendererData placeholderRendererData = null;
				RectTransform fromTransform = null;

				//From
				fromTransform = fromInputField.ExtractData(ref inputFieldData, ref textRendererData, ref placeholderRendererData);
				int siblingIndex = fromTransform.GetSiblingIndex();

				//To
				GameObject targetObject = Instantiate(targetPrefab);
				TMP_InputField toInputField = targetObject.GetComponent<TMP_InputField>();
				toInputField.ApplyData(fromTransform, inputFieldData, textRendererData, placeholderRendererData);

				TryCopyComponents(fromInputField.gameObject, toInputField.gameObject);

				if(destroyOriginalInputFields)
				{
					DestroyImmediate(fromInputField.gameObject);
				}
				else
				{
					fromInputField.name = fromInputField.name + "(Original)";
				}

				if(preserveSiblingIndexes)
				{
					toInputField.transform.SetSiblingIndex(siblingIndex);
				}
			}

			return length;
		}
#endif
		#endregion

#if ADVANCEDINPUTFIELD_TEXTMESHPRO
		#region TEXTMESHPRO_INPUTFIELD
		private int ConvertTMProInputFieldsToUnityInputField()
		{
			List<TMP_InputField> inputFields = EditorUtil.FindObjectsOfTypeAll<TMP_InputField>(true);
			GameObject targetPrefab = Resources.Load<GameObject>(UNITY_INPUTFIELD_PREFAB_PATH);

			int length = inputFields.Count;
			for(int i = 0; i < length; i++)
			{
				TMP_InputField fromInputField = inputFields[i];
				InputFieldData inputFieldData = null;
				TextRendererData textRendererData = null;
				TextRendererData placeholderRendererData = null;
				RectTransform fromTransform = null;

				//From
				fromTransform = fromInputField.ExtractData(ref inputFieldData, ref textRendererData, ref placeholderRendererData);
				int siblingIndex = fromTransform.GetSiblingIndex();

				//To
				GameObject targetObject = Instantiate(targetPrefab);
				InputField toInputField = targetObject.GetComponent<InputField>();
				toInputField.ApplyData(fromTransform, inputFieldData, textRendererData, placeholderRendererData);

				TryCopyComponents(fromInputField.gameObject, toInputField.gameObject);

				if(destroyOriginalInputFields)
				{
					DestroyImmediate(fromInputField.gameObject);
				}
				else
				{
					fromInputField.name = fromInputField.name + "(Original)";
				}

				if(preserveSiblingIndexes)
				{
					toInputField.transform.SetSiblingIndex(siblingIndex);
				}
			}

			return length;
		}

		private int ConvertTMProInputFieldsToAdvancedInputField(string prefabPath)
		{
			List<TMP_InputField> inputFields = EditorUtil.FindObjectsOfTypeAll<TMP_InputField>(true);
			GameObject targetPrefab = Resources.Load<GameObject>(prefabPath);

			int length = inputFields.Count;
			for(int i = 0; i < length; i++)
			{
				TMP_InputField fromInputField = inputFields[i];
				InputFieldData inputFieldData = null;
				TextRendererData textRendererData = null;
				TextRendererData placeholderRendererData = null;
				RectTransform fromTransform = null;

				//From
				fromTransform = fromInputField.ExtractData(ref inputFieldData, ref textRendererData, ref placeholderRendererData);
				int siblingIndex = fromTransform.GetSiblingIndex();

				//To
				GameObject targetObject = Instantiate(targetPrefab);
				AdvancedInputField toInputField = targetObject.GetComponent<AdvancedInputField>();
				toInputField.ApplyData(fromTransform, inputFieldData, textRendererData, placeholderRendererData);

				TryCopyComponents(fromInputField.gameObject, toInputField.gameObject);

				if(destroyOriginalInputFields)
				{
					DestroyImmediate(fromInputField.gameObject);
				}
				else
				{
					fromInputField.name = fromInputField.name + "(Original)";
				}

				if(preserveSiblingIndexes)
				{
					toInputField.transform.SetSiblingIndex(siblingIndex);
				}
			}

			return length;
		}
		#endregion
#endif

		#region ADVANCEDINPUTFIELD_UNITY_TEXT
		private List<AdvancedInputField> GetAdvancedInputFieldsUnityText()
		{
			List<AdvancedInputField> inputFields = EditorUtil.FindObjectsOfTypeAll<AdvancedInputField>(true);
			List<AdvancedInputField> advancedInputFields = new List<AdvancedInputField>();

			int length = inputFields.Count;
			for(int i = 0; i < length; i++)
			{
				AdvancedInputField inputField = inputFields[i];
				if(inputField.GetComponentInChildren<UnityTextRenderer>() != null)
				{
					advancedInputFields.Add(inputField);
				}
			}

			return advancedInputFields;
		}

		private int ConvertAdvancedInputFieldsUnityTextToUnityInputField()
		{
			List<AdvancedInputField> inputFields = GetAdvancedInputFieldsUnityText();
			GameObject targetPrefab = Resources.Load<GameObject>(UNITY_INPUTFIELD_PREFAB_PATH);

			int length = inputFields.Count;
			for(int i = 0; i < length; i++)
			{
				AdvancedInputField fromInputField = inputFields[i];
				InputFieldData inputFieldData = null;
				TextRendererData textRendererData = null;
				TextRendererData placeholderRendererData = null;
				RectTransform fromTransform = null;

				//From
				fromTransform = fromInputField.ExtractData(ref inputFieldData, ref textRendererData, ref placeholderRendererData);
				int siblingIndex = fromTransform.GetSiblingIndex();

				//To
				GameObject targetObject = Instantiate(targetPrefab);
				InputField toInputField = targetObject.GetComponent<InputField>();
				toInputField.ApplyData(fromTransform, inputFieldData, textRendererData, placeholderRendererData);

				TryCopyComponents(fromInputField.gameObject, toInputField.gameObject);

				if(destroyOriginalInputFields)
				{
					DestroyImmediate(fromInputField.gameObject);
				}
				else
				{
					fromInputField.name = fromInputField.name + "(Original)";
				}

				if(preserveSiblingIndexes)
				{
					toInputField.transform.SetSiblingIndex(siblingIndex);
				}
			}

			return length;
		}

#if ADVANCEDINPUTFIELD_TEXTMESHPRO
		private int ConvertAdvancedInputFieldsUnityTextToTMProInputField()
		{
			List<AdvancedInputField> inputFields = GetAdvancedInputFieldsUnityText();
			GameObject targetPrefab = Resources.Load<GameObject>(TEXTMESHPRO_INPUTFIELD_PREFAB_PATH);

			int length = inputFields.Count;
			for(int i = 0; i < length; i++)
			{
				AdvancedInputField fromInputField = inputFields[i];
				InputFieldData inputFieldData = null;
				TextRendererData textRendererData = null;
				TextRendererData placeholderRendererData = null;
				RectTransform fromTransform = null;

				//From
				fromTransform = fromInputField.ExtractData(ref inputFieldData, ref textRendererData, ref placeholderRendererData);
				int siblingIndex = fromTransform.GetSiblingIndex();

				//To
				GameObject targetObject = Instantiate(targetPrefab);
				TMP_InputField toInputField = targetObject.GetComponent<TMP_InputField>();
				toInputField.ApplyData(fromTransform, inputFieldData, textRendererData, placeholderRendererData);

				TryCopyComponents(fromInputField.gameObject, toInputField.gameObject);

				if(destroyOriginalInputFields)
				{
					DestroyImmediate(fromInputField.gameObject);
				}
				else
				{
					fromInputField.name = fromInputField.name + "(Original)";
				}

				if(preserveSiblingIndexes)
				{
					toInputField.transform.SetSiblingIndex(siblingIndex);
				}
			}

			return length;
		}
#endif

		private int ConvertAdvancedInputFieldsUnityTextToAdvancedInputField(string prefabPath)
		{
			List<AdvancedInputField> inputFields = GetAdvancedInputFieldsUnityText();
			GameObject targetPrefab = Resources.Load<GameObject>(prefabPath);

			int length = inputFields.Count;
			for(int i = 0; i < length; i++)
			{
				AdvancedInputField fromInputField = inputFields[i];
				InputFieldData inputFieldData = null;
				TextRendererData textRendererData = null;
				TextRendererData placeholderRendererData = null;
				RectTransform fromTransform = null;

				//From
				fromTransform = fromInputField.ExtractData(ref inputFieldData, ref textRendererData, ref placeholderRendererData);
				int siblingIndex = fromTransform.GetSiblingIndex();

				//To
				GameObject targetObject = Instantiate(targetPrefab);
				AdvancedInputField toInputField = targetObject.GetComponent<AdvancedInputField>();
				toInputField.ApplyData(fromTransform, inputFieldData, textRendererData, placeholderRendererData);

				TryCopyComponents(fromInputField.gameObject, toInputField.gameObject);

				if(destroyOriginalInputFields)
				{
					DestroyImmediate(fromInputField.gameObject);
				}
				else
				{
					fromInputField.name = fromInputField.name + "(Original)";
				}

				if(preserveSiblingIndexes)
				{
					toInputField.transform.SetSiblingIndex(siblingIndex);
				}
			}

			return length;
		}
		#endregion

#if ADVANCEDINPUTFIELD_TEXTMESHPRO
		#region ADVANCEDINPUTFIELD_TEXTMESHPRO_TEXT
		private List<AdvancedInputField> GetAdvancedInputFieldsTextMeshProText()
		{
			List<AdvancedInputField> inputFields = EditorUtil.FindObjectsOfTypeAll<AdvancedInputField>(true);
			List<AdvancedInputField> advancedInputFields = new List<AdvancedInputField>();

			int length = inputFields.Count;
			for(int i = 0; i < length; i++)
			{
				AdvancedInputField inputField = inputFields[i];
				if(inputField.GetComponentInChildren<TMProTextRenderer>() != null)
				{
					advancedInputFields.Add(inputField);
				}
			}

			return advancedInputFields;
		}

		private int ConvertAdvancedInputFieldsTMProTextToUnityInputField()
		{
			List<AdvancedInputField> inputFields = GetAdvancedInputFieldsTextMeshProText();
			GameObject targetPrefab = Resources.Load<GameObject>(UNITY_INPUTFIELD_PREFAB_PATH);

			int length = inputFields.Count;
			for(int i = 0; i < length; i++)
			{
				AdvancedInputField fromInputField = inputFields[i];
				InputFieldData inputFieldData = null;
				TextRendererData textRendererData = null;
				TextRendererData placeholderRendererData = null;
				RectTransform fromTransform = null;

				//From
				fromTransform = fromInputField.ExtractData(ref inputFieldData, ref textRendererData, ref placeholderRendererData);
				int siblingIndex = fromTransform.GetSiblingIndex();

				//To
				GameObject targetObject = Instantiate(targetPrefab);
				InputField toInputField = targetObject.GetComponent<InputField>();
				toInputField.ApplyData(fromTransform, inputFieldData, textRendererData, placeholderRendererData);

				TryCopyComponents(fromInputField.gameObject, toInputField.gameObject);

				if(destroyOriginalInputFields)
				{
					DestroyImmediate(fromInputField.gameObject);
				}
				else
				{
					fromInputField.name = fromInputField.name + "(Original)";
				}

				if(preserveSiblingIndexes)
				{
					toInputField.transform.SetSiblingIndex(siblingIndex);
				}
			}

			return length;
		}

		private int ConvertAdvancedInputFieldsTMProTextToTextMeshProInputField()
		{
			List<AdvancedInputField> inputFields = GetAdvancedInputFieldsTextMeshProText();
			GameObject targetPrefab = Resources.Load<GameObject>(TEXTMESHPRO_INPUTFIELD_PREFAB_PATH);

			int length = inputFields.Count;
			for(int i = 0; i < length; i++)
			{
				AdvancedInputField fromInputField = inputFields[i];
				InputFieldData inputFieldData = null;
				TextRendererData textRendererData = null;
				TextRendererData placeholderRendererData = null;
				RectTransform fromTransform = null;

				//From
				fromTransform = fromInputField.ExtractData(ref inputFieldData, ref textRendererData, ref placeholderRendererData);
				int siblingIndex = fromTransform.GetSiblingIndex();

				//To
				GameObject targetObject = Instantiate(targetPrefab);
				TMP_InputField toInputField = targetObject.GetComponent<TMP_InputField>();
				toInputField.ApplyData(fromTransform, inputFieldData, textRendererData, placeholderRendererData);

				TryCopyComponents(fromInputField.gameObject, toInputField.gameObject);

				if(destroyOriginalInputFields)
				{
					DestroyImmediate(fromInputField.gameObject);
				}
				else
				{
					fromInputField.name = fromInputField.name + "(Original)";
				}

				if(preserveSiblingIndexes)
				{
					toInputField.transform.SetSiblingIndex(siblingIndex);
				}
			}

			return length;
		}

		private int ConvertAdvancedInputFieldsTMProTextToAdvancedInputField(string prefabPath)
		{
			List<AdvancedInputField> inputFields = GetAdvancedInputFieldsTextMeshProText();
			GameObject targetPrefab = Resources.Load<GameObject>(prefabPath);

			int length = inputFields.Count;
			for(int i = 0; i < length; i++)
			{
				AdvancedInputField fromInputField = inputFields[i];
				InputFieldData inputFieldData = null;
				TextRendererData textRendererData = null;
				TextRendererData placeholderRendererData = null;
				RectTransform fromTransform = null;

				//From
				fromTransform = fromInputField.ExtractData(ref inputFieldData, ref textRendererData, ref placeholderRendererData);
				int siblingIndex = fromTransform.GetSiblingIndex();

				//To
				GameObject targetObject = Instantiate(targetPrefab);
				AdvancedInputField toInputField = targetObject.GetComponent<AdvancedInputField>();
				toInputField.ApplyData(fromTransform, inputFieldData, textRendererData, placeholderRendererData);

				TryCopyComponents(fromInputField.gameObject, toInputField.gameObject);

				if(destroyOriginalInputFields)
				{
					DestroyImmediate(fromInputField.gameObject);
				}
				else
				{
					fromInputField.name = fromInputField.name + "(Original)";
				}

				if(preserveSiblingIndexes)
				{
					toInputField.transform.SetSiblingIndex(siblingIndex);
				}
			}

			return length;
		}
		#endregion
#endif

		#region DEPRECATED_ADVANCEDINPUTFIELD
		private List<AdvancedInputField> GetDeprecatedAdvancedInputFields()
		{
			List<AdvancedInputField> inputFields = EditorUtil.FindObjectsOfTypeAll<AdvancedInputField>(true);
			List<AdvancedInputField> deprecatedInputFields = new List<AdvancedInputField>();

			int length = inputFields.Count;
			for(int i = 0; i < length; i++)
			{
				AdvancedInputField inputField = inputFields[i];
				if(inputField.GetComponentInChildren<ScrollArea>() == null) //Deprecated format didn't have a ScrollArea component yet
				{
					deprecatedInputFields.Add(inputField);
				}
			}

			return deprecatedInputFields;
		}

		private int ConvertDeprecatedAdvancedInputFieldsToUnityInputField()
		{
			List<AdvancedInputField> inputFields = GetDeprecatedAdvancedInputFields();
			GameObject targetPrefab = Resources.Load<GameObject>(UNITY_INPUTFIELD_PREFAB_PATH);

			int length = inputFields.Count;
			for(int i = 0; i < length; i++)
			{
				AdvancedInputField fromInputField = inputFields[i];
				InputFieldData inputFieldData = null;
				TextRendererData textRendererData = null;
				TextRendererData placeholderRendererData = null;
				RectTransform fromTransform = null;

				//From
				fromTransform = fromInputField.ExtractData(ref inputFieldData, ref textRendererData, ref placeholderRendererData);
				int siblingIndex = fromTransform.GetSiblingIndex();

				//To
				GameObject targetObject = Instantiate(targetPrefab);
				InputField toInputField = targetObject.GetComponent<InputField>();
				toInputField.ApplyData(fromTransform, inputFieldData, textRendererData, placeholderRendererData);

				TryCopyComponents(fromInputField.gameObject, toInputField.gameObject);

				if(destroyOriginalInputFields)
				{
					DestroyImmediate(fromInputField.gameObject);
				}
				else
				{
					fromInputField.name = fromInputField.name + "(Original)";
				}

				if(preserveSiblingIndexes)
				{
					toInputField.transform.SetSiblingIndex(siblingIndex);
				}
			}

			return length;
		}

#if ADVANCEDINPUTFIELD_TEXTMESHPRO
		private int ConvertDeprecatedAdvancedInputFieldsToTMProInputField()
		{
			List<AdvancedInputField> inputFields = GetDeprecatedAdvancedInputFields();
			GameObject targetPrefab = Resources.Load<GameObject>(TEXTMESHPRO_INPUTFIELD_PREFAB_PATH);

			int length = inputFields.Count;
			for(int i = 0; i < length; i++)
			{
				AdvancedInputField fromInputField = inputFields[i];
				InputFieldData inputFieldData = null;
				TextRendererData textRendererData = null;
				TextRendererData placeholderRendererData = null;
				RectTransform fromTransform = null;

				//From
				fromTransform = fromInputField.ExtractData(ref inputFieldData, ref textRendererData, ref placeholderRendererData);
				int siblingIndex = fromTransform.GetSiblingIndex();

				//To
				GameObject targetObject = Instantiate(targetPrefab);
				TMP_InputField toInputField = targetObject.GetComponent<TMP_InputField>();
				toInputField.ApplyData(fromTransform, inputFieldData, textRendererData, placeholderRendererData);

				TryCopyComponents(fromInputField.gameObject, toInputField.gameObject);

				if(destroyOriginalInputFields)
				{
					DestroyImmediate(fromInputField.gameObject);
				}
				else
				{
					fromInputField.name = fromInputField.name + "(Original)";
				}

				if(preserveSiblingIndexes)
				{
					toInputField.transform.SetSiblingIndex(siblingIndex);
				}
			}

			return length;
		}
#endif

		private int ConvertDeprecatedAdvancedInputFieldsToAdvancedInputField(string prefabPath)
		{
			List<AdvancedInputField> inputFields = GetDeprecatedAdvancedInputFields();
			GameObject targetPrefab = Resources.Load<GameObject>(prefabPath);

			int length = inputFields.Count;
			for(int i = 0; i < length; i++)
			{
				AdvancedInputField fromInputField = inputFields[i];
				InputFieldData inputFieldData = null;
				TextRendererData textRendererData = null;
				TextRendererData placeholderRendererData = null;
				RectTransform fromTransform = null;

				//From
				fromTransform = fromInputField.ExtractData(ref inputFieldData, ref textRendererData, ref placeholderRendererData);
				int siblingIndex = fromTransform.GetSiblingIndex();

				//To
				GameObject targetObject = Instantiate(targetPrefab);
				AdvancedInputField toInputField = targetObject.GetComponent<AdvancedInputField>();
				toInputField.ApplyData(fromTransform, inputFieldData, textRendererData, placeholderRendererData);

				TryCopyComponents(fromInputField.gameObject, toInputField.gameObject);

				if(destroyOriginalInputFields)
				{
					DestroyImmediate(fromInputField.gameObject);
				}
				else
				{
					fromInputField.name = fromInputField.name + "(Original)";
				}

				if(preserveSiblingIndexes)
				{
					toInputField.transform.SetSiblingIndex(siblingIndex);
				}
			}

			return length;
		}
		#endregion

		public void TryCopyComponents(GameObject from, GameObject to)
		{
			Component[] components = from.GetComponents(typeof(Component));
			MethodInfo methodInfo = typeof(GameObjectExtensions).GetMethod("AddComponent");

			try
			{
				int length = components.Length;
				for(int i = 0; i < length; i++)
				{
					Component component = components[i];
					if(component is Transform) { continue; }
					if(component is InputField) { continue; }
					if(component is AdvancedInputField) { continue; }
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
					if(component is TMP_InputField) { continue; }
#endif

					Type type = component.GetType();
					methodInfo.MakeGenericMethod(type).Invoke(to, new object[] { to, component });
				}
			}
			catch(Exception e) { Debug.LogWarning("Couldn't copy components: " + e.Message); }
		}
	}
}