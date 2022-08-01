using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AdvancedInputFieldPlugin.Editor
{
	public class EditorUtil
	{
		public static List<T> FindObjectsOfTypeAll<T>(bool includeInactive = false)
		{
			List<T> results = new List<T>();
			SceneManager.GetActiveScene().GetRootGameObjects().ToList().ForEach(g => results.AddRange(g.GetComponentsInChildren<T>(includeInactive)));
			return results;
		}

		public static string GetCurrentDirectoryPath()
		{
			string path = "Assets";
			foreach(UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), UnityEditor.SelectionMode.Assets))
			{
				path = AssetDatabase.GetAssetPath(obj);
				if(File.Exists(path))
				{
					path = Path.GetDirectoryName(path);
				}
				break;
			}

			return path;
		}

		public static TexturePackerData.SpriteDataObject ConvertToTexturePackerFormat(EmojiSpritesheetData spritesheetData, Vector2Int gridSize, Vector2Int padding, Vector2Int spacing)
		{
			EmojiSpriteData[] sprites = spritesheetData.Sprites;
			TexturePackerData.SpriteDataObject spriteDataObject = new TexturePackerData.SpriteDataObject();
			spriteDataObject.frames = new List<TexturePackerData.Frame>();

			int length = sprites.Length;
			for(int i = 0; i < length; i++)
			{
				EmojiSpriteData sprite = sprites[i];
				TexturePackerData.Frame frame = ConvertToTexturePackerFrame(sprite, gridSize, padding, spacing);
				spriteDataObject.frames.Add(frame);

				if(sprite.SkinVariations.Length > 0)
				{
					EmojiSpriteData[] skinVariations = sprite.SkinVariations;
					int skinVariationsLength = skinVariations.Length;
					for(int si = 0; si < skinVariationsLength; si++)
					{
						EmojiSpriteData skinVariation = skinVariations[si];
						TexturePackerData.Frame skinVariationFrame = ConvertToTexturePackerFrame(skinVariation, gridSize, padding, spacing);
						spriteDataObject.frames.Add(skinVariationFrame);
					}

				}
			}

			return spriteDataObject;
		}

		public static TexturePackerData.Frame ConvertToTexturePackerFrame(EmojiSpriteData sprite, Vector2Int gridSize, Vector2Int padding, Vector2Int spacing)
		{
			TexturePackerData.Frame frame = new TexturePackerData.Frame();
			frame.filename = sprite.Image;
			frame.rotated = false;
			frame.trimmed = false;
			frame.sourceSize = new TexturePackerData.SpriteSize() { w = gridSize.x, h = gridSize.y };
			frame.spriteSourceSize = new TexturePackerData.SpriteFrame() { x = 0, y = 0, w = gridSize.x, h = gridSize.y };
			frame.frame = new TexturePackerData.SpriteFrame()
			{
				x = (sprite.SheetX * (gridSize.x + spacing.x)) + padding.x,
				y = (sprite.SheetY * (gridSize.y + spacing.y)) + padding.y,
				w = gridSize.x,
				h = gridSize.y
			};
			frame.pivot = new Vector2(0f, 0f);

			return frame;
		}
	}
}
