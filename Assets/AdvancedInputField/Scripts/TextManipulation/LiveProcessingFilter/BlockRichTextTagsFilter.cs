// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	public class BlockRichTextTagsFilter: LiveProcessingFilter
	{
		[SerializeField]
		private RichTextData richTextData;

		public override TextEditFrame ProcessTextEditUpdate(TextEditFrame textEditFrame, TextEditFrame lastTextEditFrame)
		{
			if(textEditFrame.text == lastTextEditFrame.text) //No text change
			{
				return textEditFrame; //No processing needed, so allow change by returning current frame
			}
			else //Text change
			{
				int tagStartIndex = -1;

				int length = textEditFrame.text.Length;
				for(int i = 0; i < length; i++)
				{
					char c = textEditFrame.text[i];
					if(c == '>' && tagStartIndex != -1)
					{
						int tagLength = (i - tagStartIndex) + 1;
						string tagText = textEditFrame.text.Substring(tagStartIndex, tagLength);
						if(IsRichTextTag(tagText))
						{
							textEditFrame.text = textEditFrame.text.Remove(tagStartIndex, tagLength);
							int caretPosition = Mathf.Max(textEditFrame.selectionStartPosition - tagLength, 0);
							textEditFrame.selectionStartPosition = caretPosition;
							textEditFrame.selectionEndPosition = caretPosition;

							return textEditFrame;
						}

						tagStartIndex = -1;
					}
					else if(c == '<')
					{
						tagStartIndex = i;
					}
				}

				return textEditFrame; //No rich text tags detected, so allow change by returning current frame
			}
		}

		public bool IsRichTextTag(string tagText)
		{
			RichTextTagInfo[] tagInfos = richTextData.GetSupportedTags();
			int length = tagInfos.Length;
			for(int i = 0; i < length; i++)
			{
				RichTextTagInfo tagInfo = tagInfos[i];
				switch(tagInfo.type)
				{
					case RichTextTagType.BASIC_TAG_PAIR:
						if(tagInfo.startTag == tagText || tagInfo.endTag == tagText)
						{
							return true;
						}
						break;
					case RichTextTagType.SINGLE_PARAMETER_TAG_PAIR:
						if(tagText.StartsWith(tagInfo.startTagStart) || tagInfo.endTag == tagText)
						{
							return true;
						}
						break;
					case RichTextTagType.SINGLE_PARAMETER_SINGLE_TAG:
						if(tagText.StartsWith(tagInfo.startTagStart))
						{
							return true;
						}
						break;
				}
			}

			return false;
		}
	}
}
