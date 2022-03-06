// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	[Serializable]
	public class RichTextBindingData
	{
		public string name;
		public char codePoint;
		public string richText;

		public RichTextBindingData(string name, string richText)
		{
			this.name = name;
			this.codePoint = default;
			this.richText = richText;
		}
	}

	public class RichTextBindingEngine: MonoBehaviour
	{
		private const int MAX_CODE_POINTS = 6400;
		private const int START_CODE_POINT = 57344;

		private List<RichTextBindingData> bindings;
		private Dictionary<char, RichTextBindingData> codePointBindingDictionary;
		private Dictionary<string, RichTextBindingData> richTextBindingDictionary;
		private Dictionary<string, RichTextBindingData> nameBindingDictionary;
		private int maxTagLength;
		private bool initialized;

		public List<RichTextBindingData> Bindings { get { return bindings; } }

		private void Awake()
		{
			if(!initialized) { Initialize(); }
		}

		public void Initialize()
		{
			bindings = new List<RichTextBindingData>();
			codePointBindingDictionary = new Dictionary<char, RichTextBindingData>();
			richTextBindingDictionary = new Dictionary<string, RichTextBindingData>();
			nameBindingDictionary = new Dictionary<string, RichTextBindingData>();

			initialized = true;
		}

		public void InitializeBindings(List<RichTextBindingData> bindings)
		{
			if(!initialized) { Initialize(); }

			this.bindings = bindings;
			if(bindings.Count > MAX_CODE_POINTS)
			{
				Debug.LogWarning("Tags size exceeds the maximum private usable code points. This will cause weird effects");
			}

			codePointBindingDictionary.Clear();
			richTextBindingDictionary.Clear();
			nameBindingDictionary.Clear();

			int length = bindings.Count;
			for(int i = 0; i < length; i++)
			{
				RichTextBindingData tagData = bindings[i];
				char codePoint = (char)(START_CODE_POINT + i);
				tagData.codePoint = codePoint;
				maxTagLength = Mathf.Max(tagData.richText.Length, maxTagLength);

				codePointBindingDictionary.Add(codePoint, tagData);
				richTextBindingDictionary.Add(tagData.richText, tagData);
				nameBindingDictionary.Add(tagData.name, tagData);
			}
		}

		public bool TryGetBindingFromRichText(string key, out RichTextBindingData tagData)
		{
			return richTextBindingDictionary.TryGetValue(key, out tagData);
		}

		public bool TryGetBindingFromCodePoint(char key, out RichTextBindingData tagData)
		{
			return codePointBindingDictionary.TryGetValue(key, out tagData);
		}

		public bool TryGetBindingFromName(string name, out RichTextBindingData tagData)
		{
			return nameBindingDictionary.TryGetValue(name, out tagData);
		}

		public bool TryFindNextBindingInRichText(string richText, int startPosition, out RichTextBindingData tagData)
		{
			StringBuilder stringBuilder = new StringBuilder();
			tagData = default;
			bool foundTag = false;

			int length = richText.Length;
			for(int i = startPosition; i < length; i++)
			{
				char c = richText[i];
				stringBuilder.Append(c);

				string emojiText = stringBuilder.ToString();
				if(TryGetBindingFromRichText(emojiText, out RichTextBindingData result))
				{
					tagData = result;
					foundTag = true;
				}

				if(emojiText.Length == maxTagLength) //Probably not an tag if longer than longest tag in list
				{
					break;
				}
			}

			return foundTag;
		}
	}
}
