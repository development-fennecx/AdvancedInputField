using System;
using System.Collections.Generic;
using System.Text;
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
using TMPro;
#endif
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	[Serializable]
	public class EmojiData
	{
		public string name;
		public string text;
		public string richText;

		public EmojiData(string name, string text, string richText = null)
		{
			this.name = name;
			this.text = text;
			this.richText = richText;
		}
	}

	public class EmojiEngine: MonoBehaviour
	{
		private Dictionary<string, EmojiData> textEmojiDictionary;
		private Dictionary<string, EmojiData> richTextEmojiDictionary;
		private int maxEmojiLength;

		private void Awake()
		{
			textEmojiDictionary = new Dictionary<string, EmojiData>();
			richTextEmojiDictionary = new Dictionary<string, EmojiData>();
			maxEmojiLength = 0;
			RefreshEmojis();
		}

		public void RefreshEmojis()
		{
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
			TMProEmojiAsset emojiAsset = TMP_Settings.defaultSpriteAsset as TMProEmojiAsset;
			if(emojiAsset != null)
			{
				ConfigureEmojis(emojiAsset);

				List<TMP_SpriteAsset> fallbackSpriteAssets = emojiAsset.fallbackSpriteAssets;
				int length = fallbackSpriteAssets.Count;
				for(int i = 0; i < length; i++)
				{
					TMProEmojiAsset fallbackEmojiAsset = fallbackSpriteAssets[i] as TMProEmojiAsset;
					if(fallbackEmojiAsset != null)
					{
						ConfigureEmojis(fallbackEmojiAsset);
					}
				}
			}
			else
#endif
			{
				Debug.LogWarning("No emoji asset assigned as default sprite asset in TextMeshPro settings. See the Documentation on how to create an emoji asset file.");
			}
		}

#if ADVANCEDINPUTFIELD_TEXTMESHPRO
		public void ConfigureEmojis(TMProEmojiAsset emojiAsset)
		{
			List<EmojiData> emojis = emojiAsset.Emojis;

			int length = emojis.Count;
			for(int i = 0; i < length; i++)
			{
				EmojiData emoji = emojis[i];
				if(!textEmojiDictionary.ContainsKey(emoji.text))
				{
					textEmojiDictionary.Add(emoji.text, emoji);
					richTextEmojiDictionary.Add(emoji.richText, emoji);
					maxEmojiLength = Mathf.Max(emoji.text.Length, maxEmojiLength);
				}
			}
		}
#endif

		public bool TryGetEmoji(string key, out EmojiData emojiData)
		{
			return textEmojiDictionary.TryGetValue(key, out emojiData);
		}

		public bool TryGetSprite(string key, out EmojiData emojiData)
		{
			return richTextEmojiDictionary.TryGetValue(key, out emojiData);
		}

		public bool TryFindPreviousEmojiInText(string text, int endPosition, out EmojiData emojiData)
		{
			StringBuilder stringBuilder = new StringBuilder();
			emojiData = default;

			for(int i = endPosition; i >= 0; i--)
			{
				char c = text[i];
				stringBuilder.Insert(0, c);

				string emojiText = stringBuilder.ToString();
				if(TryGetEmoji(emojiText, out emojiData))
				{
					return true;
				}

				if(emojiText.Length == maxEmojiLength) //Probably not an emoji if longer than longest emoji in list
				{
					return false;
				}
			}

			return false;
		}

		public bool TryFindNextEmojiInText(string text, int startPosition, out EmojiData emojiData)
		{
			StringBuilder stringBuilder = new StringBuilder();
			emojiData = default;
			bool foundEmoji = false;

			int length = text.Length;
			for(int i = startPosition; i < length; i++)
			{
				char c = text[i];
				stringBuilder.Append(c);

				string emojiText = stringBuilder.ToString();
				if(TryGetEmoji(emojiText, out EmojiData result))
				{
					emojiData = result;
					foundEmoji = true;
				}

				if(emojiText.Length == maxEmojiLength) //Probably not an emoji if longer than longest emoji in list
				{
					break;
				}
			}

			if(foundEmoji)
			{
				if(startPosition + emojiData.text.Length < length)
				{
					char nextChar = text[startPosition + emojiData.text.Length];
					if(nextChar >= 0xFE00 && nextChar <= 0xFE0F) //On Android some emojis end with a variant selector
					{
						emojiData = ExtendEmoji(emojiData, nextChar);
					}
				}
			}

			return foundEmoji;
		}

		private EmojiData ExtendEmoji(EmojiData emojiData, char extensionChar)
		{
			string extendedEmojiText = emojiData.text + extensionChar;

			EmojiData extendedEmojiData = new EmojiData(emojiData.name, extendedEmojiText, emojiData.richText);
			textEmojiDictionary.Remove(emojiData.text);
			textEmojiDictionary.Add(extendedEmojiText, extendedEmojiData);
			richTextEmojiDictionary[emojiData.richText] = extendedEmojiData;

			return extendedEmojiData;
		}
	}
}