using BetterJSON;
using System.Collections.Generic;

namespace AdvancedInputFieldPlugin
{
	public class EmojiSpritesheetData
	{
		private EmojiSpriteData[] sprites;

		public EmojiSpriteData[] Sprites { get { return sprites; } }

		public EmojiSpritesheetData(JSONArray emojisJSON)
		{
			int length = emojisJSON.Length;
			sprites = new EmojiSpriteData[length];
			for(int i = 0; i < length; i++)
			{
				JSONObject emojiJSON = emojisJSON[i].Object;
				sprites[i] = new EmojiSpriteData(emojiJSON);
			}
		}
	}

	public class EmojiSpriteData
	{
		private string name;
		private string unified;
		private string image;
		private int sheetX;
		private int sheetY;
		private string shortName;
		private EmojiSpriteData[] skinVariations;

		public string Name { get { return name; } }
		public string Unified { get { return unified; } }
		public string Image { get { return image; } }
		public int SheetX { get { return sheetX; } }
		public int SheetY { get { return sheetY; } }
		public string ShortName { get { return shortName; } }
		public EmojiSpriteData[] SkinVariations { get { return skinVariations; } }

		public EmojiSpriteData(JSONObject jsonObject)
		{
			jsonObject.TryGetString("name", out name);
			jsonObject.TryGetString("unified", out unified);
			jsonObject.TryGetString("image", out image);
			jsonObject.TryGetInteger("sheet_x", out sheetX);
			jsonObject.TryGetInteger("sheet_y", out sheetY);
			jsonObject.TryGetString("short_name", out shortName);

			if(jsonObject.TryGetObject("skin_variations", out JSONObject skinVariationsValue))
			{
				List<JSONValue> skinVariationsJSON = skinVariationsValue.GetValues();
				int length = skinVariationsJSON.Count;
				skinVariations = new EmojiSpriteData[length];
				for(int i = 0; i < length; i++)
				{
					JSONObject skinVariationJSON = skinVariationsJSON[i].Object;
					skinVariations[i] = new EmojiSpriteData(skinVariationJSON);
				}
			}
			else
			{
				skinVariations = new EmojiSpriteData[0];
			}
		}
	}
}
