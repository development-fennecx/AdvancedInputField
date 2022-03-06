// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	public class EmojiCharacterLimitFilter: LiveProcessingFilter
	{
		[SerializeField]
		private int characterLimit = 20;

		private StringBuilder stringBuilder;

		public StringBuilder StringBuilder
		{
			get
			{
				if(stringBuilder == null)
				{
					stringBuilder = new StringBuilder();
				}

				return stringBuilder;
			}
		}

		public override TextEditFrame ProcessTextEditUpdate(TextEditFrame textEditFrame, TextEditFrame lastTextEditFrame)
		{
			if(textEditFrame.text == lastTextEditFrame.text) //No text change
			{
				return textEditFrame; //No processing needed, so allow change by returning current frame
			}
			else //Text change
			{
				if(textEditFrame.selectionStartPosition == textEditFrame.selectionEndPosition && lastTextEditFrame.selectionStartPosition != lastTextEditFrame.selectionEndPosition) //Selection cleared
				{
					int previousSelectionAmount = lastTextEditFrame.selectionEndPosition - lastTextEditFrame.selectionStartPosition;
					int insertAmount = textEditFrame.text.Length - (lastTextEditFrame.text.Length - previousSelectionAmount);
					if(insertAmount > 0) //Clear & insert
					{
						return ApplyCharacterLimit(textEditFrame);
					}
					else //Only clear
					{
						return textEditFrame; //No processing needed, so allow change by returning current frame
					}
				}
				else //No selection change
				{
					if(textEditFrame.selectionStartPosition > lastTextEditFrame.selectionStartPosition) //Text insert
					{
						return ApplyCharacterLimit(textEditFrame);
					}
					else if(textEditFrame.selectionStartPosition < lastTextEditFrame.selectionStartPosition) //Backwards delete
					{
						return textEditFrame; //No processing needed, so allow change by returning current frame
					}
					else //Forward delete
					{
						return textEditFrame; //No processing needed, so allow change by returning current frame
					}
				}
			}
		}

		public TextEditFrame ApplyCharacterLimit(TextEditFrame textEditFrame)
		{
			string text = textEditFrame.text;
			int position = textEditFrame.selectionStartPosition;

			while(GetCharacterCount(text) > characterLimit) //Need to recalculate character count each time here, because some emojis can consist out of multiple emojis which can exist independently
			{
				RemovePreviousCharacter(ref text, ref position);
				textEditFrame.text = text;
				textEditFrame.selectionStartPosition = position;
				textEditFrame.selectionEndPosition = position;
			}

			return textEditFrame;
		}

		/// <summary>Calculates the total character count, counting each emoji as a single character</summary>
		public int GetCharacterCount(string text)
		{
			int characterCount = 0;
			StringBuilder.Clear();

			int length = text.Length;
			for(int i = 0; i < length; i++)
			{
				char c = text[i];
				if(NativeKeyboardManager.EmojiEngine.TryFindNextEmojiInText(text, i, out EmojiData emojiData))
				{
					if(StringBuilder.Length > 0)
					{
						characterCount += StringBuilder.Length;
						StringBuilder.Clear();
					}

					characterCount++; //Count emoji as 1 character
					i += (emojiData.text.Length - 1);
				}
				else
				{
					StringBuilder.Append(c);
				}
			}

			if(StringBuilder.Length > 0)
			{
				characterCount += StringBuilder.Length;
				StringBuilder.Clear();
			}

			return characterCount;
		}

		public void RemovePreviousCharacter(ref string text, ref int position)
		{
			if(NativeKeyboardManager.EmojiEngine.TryFindPreviousEmojiInText(text, position - 1, out EmojiData emojiData))
			{
				int emojiLength = emojiData.text.Length;
				position -= emojiLength;
				text = text.Remove(position, emojiLength);
			}
			else
			{
				position--;
				text = text.Remove(position, 1);
			}
		}
	}
}
