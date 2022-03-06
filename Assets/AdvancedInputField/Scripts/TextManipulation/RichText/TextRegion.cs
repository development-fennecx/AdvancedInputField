// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	public class TextRegion
	{
		public string content;
		public int startPosition;
		public int endPosition;
		public bool isSymbol;

		public string richTextContent;
		public List<RichTextRegion> richTextRegions;

		public string Content
		{
			get { return content; }
			set
			{
				content = value;
				startPosition = 0;
				endPosition = content.Length - 1;
			}
		}

		public TextRegion(string content)
		{
			Content = content;
			this.isSymbol = false;

			this.richTextContent = content;
			this.richTextRegions = new List<RichTextRegion>();
		}

		public TextRegion(EmojiData emojiData)
		{
			Content = emojiData.text;
			this.isSymbol = true;

			this.richTextContent = emojiData.richText;
			this.richTextRegions = new List<RichTextRegion>();
		}

		public TextRegion(RichTextBindingData tagData)
		{
			Content = tagData.codePoint.ToString();
			this.isSymbol = true;

			this.richTextContent = tagData.richText;
			this.richTextRegions = new List<RichTextRegion>();
		}

		public override string ToString()
		{
			return string.Format("Content: {0}, Length: {1}, Start: {2}, End: {3}, Symbol: {4}",
				content, content.Length, startPosition, endPosition, isSymbol);
		}

		public void ConfigureRichTextRegion(RichTextRegion richTextRegion)
		{
			List<string> startTags = null;
			if(richTextRegion.startTags != null)
			{
				startTags = new List<string>(richTextRegion.startTags);
			}

			List<string> endTags = new List<string>();
			if(richTextRegion.endTags != null)
			{
				endTags = new List<string>(richTextRegion.endTags);
			}

			richTextRegions.Clear();
			richTextRegions.Add(new RichTextRegion(content, startTags, endTags));
		}

		public void ConfigureSymbol()
		{
			if(NativeKeyboardManager.RichTextBindingEngine.TryGetBindingFromCodePoint(content[0], out RichTextBindingData tagData))
			{
				RichTextRegion richTextRegion = richTextRegions[0];
				richTextRegion.isSymbol = true;
				richTextRegion.symbolText = tagData.richText;
				richTextRegion.isModifiable = false;
			}
			else if(NativeKeyboardManager.EmojiEngine.TryGetEmoji(content, out EmojiData emojiData))
			{
				RichTextRegion richTextRegion = richTextRegions[0];
				richTextRegion.isSymbol = true;
				richTextRegion.symbolText = emojiData.richText;
			}
		}

		public void BuildRichTextString(StringBuilder stringBuilder)
		{
			int length = richTextRegions.Count;
			for(int i = 0; i < length; i++)
			{
				RichTextRegion richTextRegion = richTextRegions[i];
				richTextRegion.RebuildRichTextString(); //TODO: implement isDirty flag
				stringBuilder.Append(richTextRegion.RichText);
			}
		}

		public bool TryDeterminePositionInText(int richTextPosition, ref int richTextOffset, ref int textOffset, out int textPosition)
		{
			textPosition = -1;

			int length = richTextRegions.Count;
			for(int i = 0; i < length; i++)
			{
				RichTextRegion richTextRegion = richTextRegions[i];
				int startRichTextPosition = richTextOffset + richTextRegion.startRichTextContentPosition;
				int endRichTextPosition = richTextOffset + richTextRegion.endRichTextContentPosition;

				if(richTextPosition >= startRichTextPosition && richTextPosition <= endRichTextPosition)
				{
					if(richTextRegion.isSymbol)
					{
						if(richTextPosition == startRichTextPosition)
						{
							textPosition = textOffset;
						}
						else
						{
							textPosition = textOffset + richTextRegion.Text.Length;
						}
					}
					else
					{
						textPosition = textOffset + (richTextPosition - startRichTextPosition);
					}
					return true;
				}

				richTextOffset += richTextRegion.RichText.Length;
				textOffset += richTextRegion.Text.Length;
			}

			return false;
		}

		public bool TryDeterminePositionInRichText(int textPosition, ref int richTextOffset, ref int textOffset, out int richTextPosition)
		{
			richTextPosition = -1;

			int length = richTextRegions.Count;
			for(int i = 0; i < length; i++)
			{
				RichTextRegion richTextRegion = richTextRegions[i];
				int startTextPosition = textOffset + richTextRegion.startContentPosition;
				int endTextPosition = textOffset + richTextRegion.endContentPosition;

				if(textPosition >= startTextPosition && textPosition <= endTextPosition)
				{
					richTextPosition = (richTextOffset + richTextRegion.startRichTextContentPosition) + (textPosition - startTextPosition);
					return true;
				}

				richTextOffset += richTextRegion.RichText.Length;
				textOffset += richTextRegion.Text.Length;
			}

			return false;
		}

		public bool TryInsertInText(string textToInsert, int textOffset, int textPosition)
		{
			int startPosition = textOffset;

			int length = richTextRegions.Count;
			for(int i = 0; i < length; i++)
			{
				RichTextRegion richTextRegion = richTextRegions[i];
				int startTextPosition = textOffset + richTextRegion.startContentPosition;
				int endTextPosition = textOffset + richTextRegion.endContentPosition;

				if(textPosition == endTextPosition + 1)
				{
					richTextRegion.Text += textToInsert;
					Content = content.Insert(textPosition - startPosition, textToInsert);
					return true;
				}
				else if(textPosition >= startTextPosition && textPosition <= endTextPosition)
				{
					richTextRegion.Text = richTextRegion.Text.Insert(textPosition - startTextPosition, textToInsert);
					Content = content.Insert(textPosition - startPosition, textToInsert);
					return true;
				}

				textOffset += richTextRegion.Text.Length;
			}

			Debug.LogWarningFormat("Couldn't insert {0}", textToInsert);
			return false;
		}

		public bool PositionWithinRegion(int textOffset, int textPosition, out bool startOfRegion)
		{
			int startTextPosition = textOffset + startPosition;
			int endTextPosition = textOffset + endPosition;

			if(textPosition >= startTextPosition && textPosition <= endTextPosition)
			{
				startOfRegion = (textPosition == startTextPosition);
				return true;
			}

			startOfRegion = false;
			return false;
		}

		public bool ToggleTagPairInRichText(int start, int end, int textOffset, string startTag, string endTag, ref bool foundStart, ref bool toggleON)
		{
			int length = richTextRegions.Count;
			for(int i = 0; i < length; i++)
			{
				RichTextRegion richTextRegion = richTextRegions[i];
				int startTextPosition = textOffset + richTextRegion.startContentPosition;
				int endTextPosition = textOffset + richTextRegion.endContentPosition;

				if((start >= startTextPosition && start <= endTextPosition) || foundStart)
				{
					if(!foundStart)
					{
						foundStart = true;
						if(richTextRegion.endTags.Contains(endTag))
						{
							if(richTextRegion.startTags.Contains(startTag)) //Has tag, so toggle it OFF
							{
								toggleON = false;
							}
							else //Has same tag pair, but with different parameter, so toggle it ON
							{
								toggleON = true;
							}
						}
						else //Doesn't have tag, so toggle it ON
						{
							toggleON = true;
						}
					}
					if(start <= startTextPosition)
					{
						if(end >= endTextPosition) //Toggle whole region
						{
							if(richTextRegion.endTags.Contains(endTag))
							{
								if(richTextRegion.startTags.Contains(startTag))
								{
									if(toggleON)
									{
										//Already has tag
									}
									else
									{
										richTextRegion.RemoveTag(endTag);
									}
								}
								else //Has same tag pair, but with different parameter
								{
									if(toggleON)
									{
										richTextRegion.UpdateTag(startTag, endTag);
									}
									else
									{
										richTextRegion.RemoveTag(endTag);
									}
								}
							}
							else
							{
								if(toggleON)
								{
									richTextRegion.AddTag(startTag, endTag);
								}
								else
								{
									//Doesn't have tag, so no need to remove
								}
							}
						}
						else
						{
							if(richTextRegion.isModifiable)
							{
								int splitIndex = end - startTextPosition;
								RichTextRegion[] splitRegions = richTextRegion.Split(splitIndex);
								richTextRegions.RemoveAt(i);
								richTextRegions.Insert(i, splitRegions[0]);
								richTextRegions.Insert(i + 1, splitRegions[1]);
								i--;
								length++;
								continue;
							}
						}
					}
					else //Split
					{
						if(richTextRegion.isModifiable)
						{
							int splitIndex = start - startTextPosition;
							RichTextRegion[] splitRegions = richTextRegion.Split(splitIndex);
							richTextRegions.RemoveAt(i);
							richTextRegions.Insert(i, splitRegions[0]);
							richTextRegions.Insert(i + 1, splitRegions[1]);
							textOffset += splitIndex;
							length++;
							continue;
						}
					}
				}

				if(foundStart && end <= endTextPosition + 1)
				{
					return true; //Found end
				}

				textOffset += richTextRegion.Text.Length;
			}
			return false;
		}

		public bool TryDeleteInText(ref int amount, ref int richTextOffset, ref int textOffset, int textPosition)
		{
			int startPosition = textOffset;

			int length = richTextRegions.Count;
			for(int i = 0; i < length; i++)
			{
				RichTextRegion richTextRegion = richTextRegions[i];
				int startTextPosition = textOffset + richTextRegion.startContentPosition;
				int endTextPosition = textOffset + richTextRegion.endContentPosition;

				if(textPosition >= startTextPosition && textPosition <= endTextPosition)
				{
					int amountToDelete = Mathf.Min((endTextPosition - textPosition) + 1, amount);
					richTextRegion.Text = richTextRegion.Text.Remove(textPosition - startTextPosition, amountToDelete);
					Content = content.Remove(textPosition - startPosition, amountToDelete);

					if(richTextRegion.Text.Length == 0)
					{
						richTextRegions.RemoveAt(i);
					}

					amount -= amountToDelete;
					return true;
				}

				richTextOffset += richTextRegion.RichText.Length;
				textOffset += richTextRegion.Text.Length;
			}

			return false;
		}

		public TextRegion[] SplitRegion(int textOffset, int textPosition)
		{
			int startPosition = textOffset;

			int length = richTextRegions.Count;
			for(int i = 0; i < length; i++)
			{
				RichTextRegion richTextRegion = richTextRegions[i];
				int startTextPosition = textOffset + richTextRegion.startContentPosition;
				int endTextPosition = textOffset + richTextRegion.endContentPosition;

				if(textPosition >= startTextPosition && textPosition <= endTextPosition)
				{
					int splitIndex = textPosition - startPosition;
					TextRegion[] splitRegions = Split(startPosition, textPosition, splitIndex);

					return splitRegions;
				}

				textOffset += richTextRegion.Text.Length;
			}

			Debug.LogWarningFormat("Couldn't split regions at " + textPosition);
			return null;
		}

		private TextRegion[] Split(int textOffset, int textPosition, int splitIndex)
		{
			TextRegion[] splitRegions = new TextRegion[2];
			splitRegions[0] = new TextRegion(content.Substring(0, splitIndex));
			splitRegions[1] = new TextRegion(content.Substring(splitIndex));

			int length = richTextRegions.Count;
			for(int i = 0; i < length; i++)
			{
				RichTextRegion richTextRegion = richTextRegions[i];
				int startTextPosition = textOffset + richTextRegion.startContentPosition;
				int endTextPosition = textOffset + richTextRegion.endContentPosition;

				if(textPosition > endTextPosition)
				{
					splitRegions[0].richTextRegions.Add(richTextRegion);
				}
				else
				{
					if(textPosition > startTextPosition && textPosition <= endTextPosition)
					{
						string text = richTextRegion.Text;
						int splitRichTextIndex = textPosition - startTextPosition;

						RichTextRegion previousRichTextRegion = new RichTextRegion(text.Substring(0, splitRichTextIndex));
						previousRichTextRegion.startTags.AddRange(richTextRegion.startTags);
						previousRichTextRegion.endTags.AddRange(richTextRegion.endTags);

						RichTextRegion nextRichTextRegion = new RichTextRegion(text.Substring(splitRichTextIndex));
						nextRichTextRegion.startTags.AddRange(richTextRegion.startTags);
						nextRichTextRegion.endTags.AddRange(richTextRegion.endTags);

						splitRegions[0].richTextRegions.Add(previousRichTextRegion);
						splitRegions[1].richTextRegions.Add(nextRichTextRegion);
					}
					else
					{
						splitRegions[1].richTextRegions.Add(richTextRegion);
					}
				}

				textOffset += richTextRegion.Text.Length;
			}

			return splitRegions;
		}
	}
}
