//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using System.IO;
using UnityEditor;
using UnityEngine;

namespace AdvancedInputFieldPlugin.Editor
{
	public class EditorExtensions
	{
		private const string INPUT_FIELD_UNITY_TEXT_PREFAB_PATH = "AdvancedInputField/Prefabs/AdvancedInputField_Unity_Text";
		private const string INPUT_FIELD_TMPRO_TEXT_PREFAB_PATH = "AdvancedInputField/Prefabs/AdvancedInputField_TextMeshPro_Text";
		private const string SETTINGS_PATH = "AdvancedInputField/Settings";

		[MenuItem("Tools/Advanced Input Field/Create/InputField with Unity Text", false, 1)]
		public static void CreateInputFieldUnityText()
		{
			GameObject prefab = Resources.Load(INPUT_FIELD_UNITY_TEXT_PREFAB_PATH) as GameObject;
			CreateInstance(prefab);
		}

		[MenuItem("Tools/Advanced Input Field/Create/InputField with TextMeshPro Text", false, 2)]
		public static void CreateInputFieldTMProText()
		{
			GameObject prefab = Resources.Load(INPUT_FIELD_TMPRO_TEXT_PREFAB_PATH) as GameObject;
			CreateInstance(prefab);
		}

		[MenuItem("Tools/Advanced Input Field/Create/Character Validator", false, 3)]
		public static void CreateCharacterValidator()
		{
			CharacterValidator asset = ScriptableObject.CreateInstance<CharacterValidator>();

			string currentDirectoryPath = EditorUtil.GetCurrentDirectoryPath();
			string outputFilePath = Path.Combine(currentDirectoryPath, "CharacterValidator.asset");
			AssetDatabase.CreateAsset(asset, outputFilePath);
			AssetDatabase.SaveAssets();

			Selection.activeObject = asset;
		}

		[MenuItem("Tools/Advanced Input Field/Create/LocalizationData", false, 4)]
		public static void CreateLocalizationData()
		{
			LocalizationData asset = ScriptableObject.CreateInstance<LocalizationData>();

			string currentDirectoryPath = EditorUtil.GetCurrentDirectoryPath();
			string outputFilePath = Path.Combine(currentDirectoryPath, "LocalizationData.asset");
			AssetDatabase.CreateAsset(asset, outputFilePath);
			AssetDatabase.SaveAssets();

			Selection.activeObject = asset;
		}

		[MenuItem("Tools/Advanced Input Field/Create/RichTextData", false, 5)]
		public static void CreateRichTextSettingsData()
		{
			RichTextData asset = ScriptableObject.CreateInstance<RichTextData>();

			string currentDirectoryPath = EditorUtil.GetCurrentDirectoryPath();
			string outputFilePath = Path.Combine(currentDirectoryPath, "RichTextData.asset");
			AssetDatabase.CreateAsset(asset, outputFilePath);
			AssetDatabase.SaveAssets();

			Selection.activeObject = asset;
		}

		[MenuItem("Tools/Advanced Input Field/Global Settings", false, 202)]
		public static void OpenSettings()
		{
			Object settings = Resources.Load(SETTINGS_PATH);
			Selection.activeObject = settings;
		}

		[MenuItem("Tools/Advanced Input Field/About", false, 201)]
		public static void OpenVersionInfo()
		{
			VersionInfoWindow window = (VersionInfoWindow)EditorWindow.GetWindowWithRect(typeof(VersionInfoWindow), new Rect(0, 0, 640, 480), true, VersionInfoWindow.TITLE);
			window.Initialize();
		}

		private static void CreateInstance(GameObject prefab)
		{
			GameObject instance = GameObject.Instantiate(prefab);

			Transform parentTransform = Selection.activeTransform;
			instance.transform.SetParent(parentTransform);
			instance.transform.localPosition = Vector3.zero;
			instance.transform.localScale = Vector3.one;

			string name = instance.name;
			name = name.Substring(0, name.Length - "(Clone)".Length);
			instance.name = name;
		}
	}
}