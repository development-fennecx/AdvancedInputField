#if ADVANCEDINPUTFIELD_TEXTMESHPRO
using BetterJSON;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using TMPro;
using TMPro.SpriteAssetUtilities;
#endif
using UnityEditor;
using UnityEngine;

namespace AdvancedInputFieldPlugin.Editor
{
	/// <summary>Tool to create a TextMeshPro Sprite asset configured for emoji support, based on the implementation of the package "com.kyub.emojisearch"</summary>
	public class EmojiAssetGenerator: EditorWindow
	{
		private const string RICH_TEXT_SPRITE_FORMAT = "<sprite name=\"{0}\">";

		[MenuItem("Tools/Advanced Input Field/Emoji Asset Generator", false, 102)]
		public static void ShowWindow()
		{
			EmojiAssetGenerator window = GetWindow<EmojiAssetGenerator>();
			window.titleContent = new GUIContent("Emoji Asset Generator");
			window.Focus();
		}

#if ADVANCEDINPUTFIELD_TEXTMESHPRO
		[SerializeField]
		private Vector2Int gridSize = new Vector2Int(32, 32);

		[SerializeField]
		private Vector2Int padding = new Vector2Int(1, 1);

		[SerializeField]
		private Vector2Int spacing = new Vector2Int(2, 2);

		[SerializeField]
		private Texture2D spritesheet;

		[SerializeField]
		private TextAsset emojiListAsset;

		private TMProEmojiAsset emojiAsset;
		private List<EmojiData> emojis = new List<EmojiData>();
		private List<TMP_Sprite> spriteList = new List<TMP_Sprite>();
#endif

		private void OnEnable()
		{
			this.minSize = new Vector2(Mathf.Max(230, this.minSize.x), Mathf.Max(300, this.minSize.y));
		}

		private void OnGUI()
		{
			DrawEditor();
		}

		private void DrawEditor()
		{
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
			GUILayout.BeginVertical();
			GUILayout.Label("Import Settings", EditorStyles.boldLabel);
			emojiListAsset = EditorGUILayout.ObjectField("Emoji list", emojiListAsset, typeof(TextAsset), false) as TextAsset;

			GUILayout.Space(4);
			gridSize = EditorGUILayout.Vector2IntField("Grid Size", gridSize);
			padding = EditorGUILayout.Vector2IntField("Padding", padding);
			spacing = EditorGUILayout.Vector2IntField("Spacing", spacing);
			GUILayout.Space(4);

			spritesheet = EditorGUILayout.ObjectField("Spritesheet", spritesheet, typeof(Texture2D), false) as Texture2D;

			GUILayout.Space(10);
			GUI.enabled = (emojiListAsset != null && spritesheet != null);

			if(GUILayout.Button("Generate Emoji Asset"))
			{
				string json = emojiListAsset.text;
				//json = "{\"sprites\":" + json + "}";
				EmojiSpritesheetData spritesheetData = new EmojiSpritesheetData(new JSONArray(json));
				ReadEmojis(spritesheetData);

				TexturePackerData.SpriteDataObject spriteDataObject = EditorUtil.ConvertToTexturePackerFormat(spritesheetData, gridSize, padding, spacing);
				json = JsonUtility.ToJson(spriteDataObject);

#if(TEXTMESHPRO_1_5 || TEXTMESHPRO_2_1 || TEXTMESHPRO_3_0)
				TexturePacker_JsonArray.SpriteDataObject sprites = JsonUtility.FromJson<TexturePacker_JsonArray.SpriteDataObject>(json);
#else
				TexturePacker.SpriteDataObject sprites = JsonUtility.FromJson<TexturePacker.SpriteDataObject>(json);
#endif
				if(sprites != null && sprites.frames != null && sprites.frames.Count > 0)
				{
					spriteList = CreateSpriteInfoList(sprites);
				}

				if(spriteList.Count > 0)
				{
					string filePath = EditorUtility.SaveFilePanel("Save Emoji Asset File", new FileInfo(AssetDatabase.GetAssetPath(emojiListAsset)).DirectoryName, emojiListAsset.name, "asset");
					if(filePath.Length == 0) { return; }

					SaveSpriteAsset(filePath);
				}
				else
				{
					Debug.LogWarning("No sprites found");
				}
			}

			GUI.enabled = true;
			GUILayout.EndVertical();
#else
			GUILayout.Label("TextMeshPro not configured yet, please see the Documentation for instructions", EditorStyles.boldLabel);
#endif
		}

#if ADVANCEDINPUTFIELD_TEXTMESHPRO
		private void ReadEmojis(EmojiSpritesheetData spritesheetData)
		{
			EmojiSpriteData[] emojiSpriteDatas = spritesheetData.Sprites;
			emojis.Clear();

			int length = emojiSpriteDatas.Length;
			for(int i = 0; i < length; i++)
			{
				EmojiSpriteData emojiSpriteData = emojiSpriteDatas[i];
				EmojiData emoji = CreateEmojiData(emojiSpriteData);
				emojis.Add(emoji);

				if(emojiSpriteData.SkinVariations.Length > 0)
				{
					EmojiSpriteData[] skinVariations = emojiSpriteData.SkinVariations;
					int skinVariationsLength = skinVariations.Length;
					for(int si = 0; si < skinVariationsLength; si++)
					{
						EmojiSpriteData skinVariation = skinVariations[si];
						EmojiData skinVariationEmoji = CreateEmojiData(skinVariation);
						emojis.Add(skinVariationEmoji);
					}
				}
			}
		}

		private EmojiData CreateEmojiData(EmojiSpriteData emojiSpriteData)
		{
			string name = emojiSpriteData.Name;
			if(string.IsNullOrEmpty(name))
			{
				name = emojiSpriteData.ShortName;
			}

			string emojiName = Path.GetFileNameWithoutExtension(emojiSpriteData.Image);
			string[] values = emojiName.Split('-');
			StringBuilder stringBuilder = new StringBuilder();
			int valuesLength = values.Length;
			for(int vi = 0; vi < valuesLength; vi++)
			{
				string part = values[vi].Trim();
				string utf16String = char.ConvertFromUtf32(int.Parse(part, NumberStyles.HexNumber));
				stringBuilder.Append(utf16String);
			}
			string emojiText = stringBuilder.ToString();

			return new EmojiData(name, emojiText);
		}

#if(TEXTMESHPRO_1_5 || TEXTMESHPRO_2_1 || TEXTMESHPRO_3_0)
		private List<TMP_Sprite> CreateSpriteInfoList(TexturePacker_JsonArray.SpriteDataObject spriteDataObject)
#else
		private List<TMP_Sprite> CreateSpriteInfoList(TexturePacker.SpriteDataObject spriteDataObject)
#endif
		{
#if(TEXTMESHPRO_1_5 || TEXTMESHPRO_2_1 || TEXTMESHPRO_3_0)
			List<TexturePacker_JsonArray.Frame> importedSprites = spriteDataObject.frames;
#else
			List<TexturePacker.SpriteData> importedSprites = spriteDataObject.frames;
#endif
			List<TMP_Sprite> spriteInfoList = new List<TMP_Sprite>();

			for(int i = 0; i < importedSprites.Count; i++)
			{
#if(TEXTMESHPRO_1_5 || TEXTMESHPRO_2_1 || TEXTMESHPRO_3_0)
				TexturePacker_JsonArray.Frame spriteData = importedSprites[i];
#else
				TexturePacker.SpriteData spriteData = importedSprites[i];
#endif
				EmojiData emojiData = emojis[i];
				TMP_Sprite sprite = new TMP_Sprite();

				sprite.id = i;
				sprite.name = Path.GetFileNameWithoutExtension(spriteData.filename);
				emojiData.richText = string.Format(RICH_TEXT_SPRITE_FORMAT, sprite.name);
				sprite.hashCode = TMP_TextUtilities.GetSimpleHashCode(sprite.name);

				int unicode;
				int indexOfSeparator = sprite.name.IndexOf('-');
				if(indexOfSeparator != -1)
				{
					string substring = sprite.name.Substring(0, indexOfSeparator);
					unicode = TMP_TextUtilities.StringHexToInt(substring);
				}
				else
				{
					unicode = TMP_TextUtilities.StringHexToInt(sprite.name);
				}

				sprite.unicode = unicode;
				sprite.x = spriteData.frame.x;
				sprite.y = spritesheet.height - (spriteData.frame.y + spriteData.frame.h);
				sprite.width = spriteData.frame.w;
				sprite.height = spriteData.frame.h;
				sprite.pivot = spriteData.pivot;
				sprite.xAdvance = sprite.width;
				sprite.scale = 1.0f;
				sprite.xOffset = -(sprite.width * sprite.pivot.x);
				sprite.yOffset = sprite.height - (sprite.height * sprite.pivot.y);

				spriteInfoList.Add(sprite);
			}

			return spriteInfoList;
		}

		private void SaveSpriteAsset(string filePath)
		{
			filePath = filePath.Substring(0, filePath.LastIndexOf('.'));

			string dataPath = Application.dataPath;

			if(filePath.IndexOf(dataPath, System.StringComparison.InvariantCultureIgnoreCase) == -1)
			{
				Debug.LogWarning("You're trying to save the emoji asset in a directory outside of this project folder. This is not supported.");
				return;
			}

			string relativeAssetPath = filePath.Substring(dataPath.Length - 6);
			string directoryName = Path.GetDirectoryName(relativeAssetPath);
			string fileName = Path.GetFileNameWithoutExtension(relativeAssetPath);
			string pathNoExt = directoryName + "/" + fileName;

			emojiAsset = CreateInstance<TMProEmojiAsset>();
			AssetDatabase.CreateAsset(emojiAsset, pathNoExt + ".asset");

			emojiAsset.hashCode = TMP_TextUtilities.GetSimpleHashCode(emojiAsset.name);
			emojiAsset.spriteSheet = spritesheet;
			emojiAsset.spriteInfoList = spriteList;
			AddDefaultMaterial(emojiAsset);

			FieldInfo versionField = typeof(TMP_SpriteAsset).GetField("m_Version", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			if(versionField != null)
			{
				versionField.SetValue(emojiAsset, string.Empty);
			}

			FieldInfo emojisField = typeof(TMProEmojiAsset).GetField("emojis", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			if(emojisField != null)
			{
				emojisField.SetValue(emojiAsset, emojis);
			}

			emojiAsset.UpdateLookupTables();

			List<TMP_SpriteCharacter> characters = emojiAsset.spriteCharacterTable;
			int length = characters.Count;
			for(int i = 0; i < length; i++)
			{
				characters[i].glyphIndex = (uint)i;
			}
		}

		private void AddDefaultMaterial(TMP_SpriteAsset spriteAsset)
		{
			Shader shader = Shader.Find("TextMeshPro/Sprite");
			Material material = new Material(shader);
			material.SetTexture(ShaderUtilities.ID_MainTex, spriteAsset.spriteSheet);

			spriteAsset.material = material;
			material.hideFlags = HideFlags.HideInHierarchy;
			AssetDatabase.AddObjectToAsset(material, spriteAsset);
		}
#endif
	}
}