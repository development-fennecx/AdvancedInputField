// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text;

namespace AdvancedInputFieldPlugin
{
	public class RichTextRegion
	{
		private string text;
		private string richText;

		public string symbolText;
		public bool isSymbol;
		public bool isModifiable;

		public int startContentPosition;
		public int endContentPosition;
		public int startRichTextContentPosition;
		public int endRichTextContentPosition;
		public List<string> startTags;
		public List<string> endTags;

		public string Text
		{
			get { return text; }
			set
			{
				text = value;
				startContentPosition = 0;
				endContentPosition = text.Length - 1;
			}
		}

		public string RichText { get { return richText; } }

		public RichTextRegion(string text, List<string> startTags = null, List<string> endTags = null, bool isModifiable = true)
		{
			Text = text;
			if(startTags != null)
			{
				this.startTags = startTags;
				this.endTags = endTags;
			}
			else
			{
				this.startTags = new List<string>();
				this.endTags = new List<string>();
			}

			this.isModifiable = isModifiable;
		}

		public void RemoveTag(string endTag)
		{
			if(!isModifiable) { return; }

			int index = endTags.IndexOf(endTag);
			if(index != -1) //Found tags, so safe to remove
			{
				startTags.RemoveAt(index);
				endTags.RemoveAt(index);
			}
		}

		public void UpdateTag(string startTag, string endTag)
		{
			if(!isModifiable) { return; }

			int index = endTags.IndexOf(endTag);
			if(index != -1) //Found tags, so safe to update
			{
				startTags[index] = startTag;
			}
		}

		public void AddTag(string startTag, string endTag)
		{
			if(!isModifiable) { return; }

			int index = endTags.IndexOf(endTag);
			if(index == -1) //No tags found, so safe to add
			{
				startTags.Add(startTag);
				endTags.Add(endTag);
			}
			else
			{
				startTags[index] = startTag; //Replacing start tag, because end tag is the same
			}
		}

		public void CopyTags(RichTextRegion otherRegion)
		{
			if(!isModifiable) { return; }

			startTags.Clear();
			startTags.AddRange(otherRegion.startTags);
			endTags.Clear();
			endTags.AddRange(otherRegion.endTags);
		}

		public RichTextRegion[] Split(int index)
		{
			string content1 = text.Substring(0, index);
			string content2 = text.Substring(index);

			RichTextRegion[] regions = new RichTextRegion[2];
			if(startTags != null)
			{
				regions[0] = new RichTextRegion(content1, new List<string>(startTags), new List<string>(endTags));
				regions[1] = new RichTextRegion(content2, new List<string>(startTags), new List<string>(endTags));
			}
			else
			{
				regions[0] = new RichTextRegion(content1, null, null);
				regions[1] = new RichTextRegion(content2, null, null);
			}

			return regions;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("RichTextRegion: " + text + ": " + startContentPosition + " -> " + endContentPosition);
			int length = startTags.Count;
			for(int i = 0; i < length; i++)
			{
				stringBuilder.AppendLine("Tag: " + startTags[i]);
			}

			return stringBuilder.ToString();
		}

		public void RebuildRichTextString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			int charIndex = 0;

			int startTagsLength = startTags.Count;
			for(int si = 0; si < startTagsLength; si++)
			{
				string startTag = startTags[si];
				stringBuilder.Append(startTag);
				charIndex += startTag.Length;
			}

			startRichTextContentPosition = charIndex;
			if(isSymbol)
			{
				stringBuilder.Append(symbolText);
				charIndex += symbolText.Length;
			}
			else
			{
				stringBuilder.Append(text);
				charIndex += text.Length;
			}

			endRichTextContentPosition = charIndex;

			int endTagsLength = endTags.Count;
			for(int ei = endTagsLength - 1; ei >= 0; ei--)
			{
				string endTag = endTags[ei];
				stringBuilder.Append(endTag);
				charIndex += endTag.Length;
			}

			richText = stringBuilder.ToString();
		}
	}
}
