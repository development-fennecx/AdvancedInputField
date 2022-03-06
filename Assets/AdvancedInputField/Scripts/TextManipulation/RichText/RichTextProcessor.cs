// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using AIFLogger;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	public class RichTextProcessor
	{
		public RichTextTagInfo[] supportedTags;
		public bool emojisAllowed;
		public bool richTextBindingsAllowed;

		public TextEditFrame LastRichTextEditFrame { get; private set; }
		public TextEditFrame LastTextEditFrame { get; private set; }

		private List<TextRegion> textRegions;
		private StringBuilder stringBuilder;
		public string RichText { get { return LastRichTextEditFrame.text; } }
		public string Text { get { return LastTextEditFrame.text; } }
		public List<TextRegion> TextRegions { get { return textRegions; } }

		public RichTextProcessor(RichTextTagInfo[] supportedTags, bool emojisAllowed, bool richTextBindingsAllowed)
		{
			this.supportedTags = supportedTags;
			this.emojisAllowed = emojisAllowed;
			this.richTextBindingsAllowed = richTextBindingsAllowed;
			this.textRegions = new List<TextRegion>();
			this.stringBuilder = new StringBuilder();
		}

		public void SetupRichText(string richText)
		{
			textRegions.Clear();
			List<RichTextRegion> richTextRegions = ParseRichTextRegions(richText);
			List<TextRegion> parsedTextRegions = new List<TextRegion>();

			int length = richTextRegions.Count;
			for(int i = 0; i < length; i++)
			{
				RichTextRegion richTextRegion = richTextRegions[i];
				List<TextRegion> currentTextRegions = ParseTextRegions(richTextRegion.Text);
				foreach(TextRegion currentTextRegion in currentTextRegions)
				{
					currentTextRegion.ConfigureRichTextRegion(richTextRegion);
					parsedTextRegions.Add(currentTextRegion);
				}
			}

			length = parsedTextRegions.Count;
			if(length == 1)
			{
				textRegions.Add(parsedTextRegions[0]);
			}
			else
			{
				for(int i = 0; i < length; i++)
				{
					TextRegion region = parsedTextRegions[i];
					if(region.isSymbol) //Symbols can't be merged
					{
						region.ConfigureSymbol();
						textRegions.Add(region);
						continue;
					}

					int mergedIndex = i;
					for(int ni = i + 1; ni < length; ni++)
					{
						TextRegion nextRegion = parsedTextRegions[ni];
						if(nextRegion.isSymbol) //Symbols can't be merged
						{
							break;
						}
						else
						{
							region.Content = region.content + nextRegion.content;
							region.richTextRegions.Add(nextRegion.richTextRegions[0]);
							mergedIndex = ni;
						}
					}

					textRegions.Add(region);
					i = mergedIndex;
				}
			}

			string resultRichText = RebuildRichTextString();
			string resultText = RebuildTextString();
			LastRichTextEditFrame = new TextEditFrame(resultRichText, 0, 0);
			LastTextEditFrame = new TextEditFrame(resultText, 0, 0);
		}

		public string RebuildRichTextString()
		{
			stringBuilder.Clear();

			int length = textRegions.Count;
			for(int i = 0; i < length; i++)
			{
				textRegions[i].BuildRichTextString(stringBuilder);
			}

			return stringBuilder.ToString();
		}

		public string RebuildTextString()
		{
			stringBuilder.Clear();

			int length = textRegions.Count;
			for(int i = 0; i < length; i++)
			{
				stringBuilder.Append(textRegions[i].content);
			}

			return stringBuilder.ToString();
		}

		public List<RichTextRegion> ParseRichTextRegions(string richText)
		{
			if(emojisAllowed || richTextBindingsAllowed)
			{
				richText = ConvertSpecialTags(richText);
			}
			List<RichTextRegion> regions = new List<RichTextRegion>();
			if(string.IsNullOrEmpty(richText)) { return regions; }

			Stack<string> startTagsStack = new Stack<string>();
			Stack<string> endTagsStack = new Stack<string>();
			int tagStartIndex = -1;
			int tagEndIndex = -1;

			int length = richText.Length;
			for(int i = 0; i < length; i++)
			{
				char c = richText[i];
				if(c == '<')
				{
					if(tagStartIndex != -1) //Wasn't a tag
					{
						if(tagEndIndex == -1)
						{
							int amount = i - tagStartIndex;
							string content = richText.Substring(0, amount);
							regions.Add(new RichTextRegion(content));
							tagEndIndex += amount;
						}
						else
						{
							int amount = tagStartIndex - tagEndIndex;
							string content = richText.Substring(tagEndIndex + 1, amount);
							regions.Add(new RichTextRegion(content, new List<string>(startTagsStack), new List<string>(endTagsStack)));
							tagEndIndex += amount;
						}
					}
					tagStartIndex = i;
				}
				else if(c == '>' && tagStartIndex != -1)
				{
					string tagText = richText.Substring(tagStartIndex, (i - tagStartIndex) + 1);

					bool isStartTag = IsValidStartTagText(tagText, out int si);
					if(isStartTag)
					{
						if(tagEndIndex == -1 && tagStartIndex != 0)
						{
							string content = richText.Substring(0, tagStartIndex);
							regions.Add(new RichTextRegion(content));
						}
						else if(tagEndIndex + 1 != tagStartIndex)
						{
							int amount = tagStartIndex - (tagEndIndex + 1);
							string content = richText.Substring(tagEndIndex + 1, amount);
							regions.Add(new RichTextRegion(content, new List<string>(startTagsStack), new List<string>(endTagsStack)));
						}

						startTagsStack.Push(tagText); //Use tag text here to preserve the parameter value
						endTagsStack.Push(supportedTags[si].endTag);

						tagStartIndex = -1;
						tagEndIndex = i;
						continue;
					}

					bool isEndTag = IsValidEndTagText(tagText, out int ei);
					if(isEndTag)
					{
						if(tagEndIndex + 1 != tagStartIndex)
						{
							int amount = tagStartIndex - (tagEndIndex + 1);
							string content = richText.Substring(tagEndIndex + 1, amount);
							regions.Add(new RichTextRegion(content, new List<string>(startTagsStack), new List<string>(endTagsStack)));
						}

						startTagsStack.Pop();
						endTagsStack.Pop();

						tagStartIndex = -1;
						tagEndIndex = i;
						continue;
					}
				}
			}

			if(tagEndIndex != -1)
			{
				if(tagEndIndex < length - 1)
				{
					int amount = length - (tagEndIndex + 1);
					string content = richText.Substring(tagEndIndex + 1, amount);
					regions.Add(new RichTextRegion(content, null, null));
				}
			}
			else
			{
				regions.Add(new RichTextRegion(richText, null, null));
			}

			return regions;
		}

		private string ConvertSpecialTags(string richText)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int tagStartIndex = -1;

			int length = richText.Length;
			for(int i = 0; i < length; i++)
			{
				char c = richText[i];
				if(c == '<')
				{
					if(tagStartIndex != -1)
					{
						string text = richText.Substring(tagStartIndex, i - tagStartIndex);
						stringBuilder.Append(text);
					}
					tagStartIndex = i;
				}
				else if(c == '>' && tagStartIndex != -1)
				{
					string tagText = richText.Substring(tagStartIndex, (i - tagStartIndex) + 1);

					if(emojisAllowed && IsValidSingleTagText(tagText) && NativeKeyboardManager.EmojiEngine.TryGetSprite(tagText, out EmojiData emojiData))
					{
						stringBuilder.Append(emojiData.text);
					}
					else if(richTextBindingsAllowed && NativeKeyboardManager.RichTextBindingEngine.TryFindNextBindingInRichText(richText, tagStartIndex, out RichTextBindingData tagData))
					{
						stringBuilder.Append(tagData.codePoint);
						i = tagStartIndex + tagData.richText.Length - 1;
					}
					else
					{
						stringBuilder.Append(tagText);
					}

					tagStartIndex = -1;
				}
				else if(tagStartIndex == -1)
				{
					stringBuilder.Append(c);
				}
			}

			if(tagStartIndex != -1)
			{
				stringBuilder.Append(richText.Substring(tagStartIndex));
			}

			return stringBuilder.ToString();
		}

		public bool IsValidSingleTagText(string tagText)
		{
			int length = supportedTags.Length;
			for(int i = 0; i < length; i++)
			{
				RichTextTagInfo tagInfo = supportedTags[i];
				switch(tagInfo.type)
				{
					case RichTextTagType.BASIC_SINGLE_TAG:
						if(tagInfo.startTag == tagText)
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

		public List<TextRegion> ParseTextRegions(string text)
		{
			List<TextRegion> regions = new List<TextRegion>();
			StringBuilder stringBuilder = new StringBuilder();

			int length = text.Length;
			for(int i = 0; i < length; i++)
			{
				char c = text[i];
				if(richTextBindingsAllowed && NativeKeyboardManager.RichTextBindingEngine.TryGetBindingFromCodePoint(c, out RichTextBindingData tagData))
				{
					if(stringBuilder.Length > 0)
					{
						string content = stringBuilder.ToString();
						regions.Add(new TextRegion(content));
						stringBuilder.Clear();
					}

					regions.Add(new TextRegion(tagData));
				}
				else if(emojisAllowed && NativeKeyboardManager.EmojiEngine.TryFindNextEmojiInText(text, i, out EmojiData emojiData))
				{
					if(stringBuilder.Length > 0)
					{
						string content = stringBuilder.ToString();
						regions.Add(new TextRegion(content));
						stringBuilder.Clear();
					}

					regions.Add(new TextRegion(emojiData));
					i += (emojiData.text.Length - 1);
				}
				else
				{
					stringBuilder.Append(c);
				}
			}

			if(stringBuilder.Length > 0)
			{
				string content = stringBuilder.ToString();
				regions.Add(new TextRegion(content));
			}

			return regions;
		}

		public bool IsValidTagText(string tagText)
		{
			int length = supportedTags.Length;
			for(int i = 0; i < length; i++)
			{
				RichTextTagInfo tagInfo = supportedTags[i];
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

		public bool IsValidStartTagText(string tagText, out int index)
		{
			int length = supportedTags.Length;
			for(int i = 0; i < length; i++)
			{
				RichTextTagInfo tagInfo = supportedTags[i];
				switch(tagInfo.type)
				{
					case RichTextTagType.BASIC_TAG_PAIR:
						if(tagInfo.startTag == tagText)
						{
							index = i;
							return true;
						}
						break;
					case RichTextTagType.SINGLE_PARAMETER_TAG_PAIR:
						if(tagText.StartsWith(tagInfo.startTagStart))
						{
							index = i;
							return true;
						}
						break;

				}
			}

			index = -1;
			return false;
		}

		public bool IsValidEndTagText(string tagText, out int index)
		{
			int length = supportedTags.Length;
			for(int i = 0; i < length; i++)
			{
				RichTextTagInfo tagInfo = supportedTags[i];
				if(tagInfo.endTag == tagText)
				{
					index = i;
					return true;
				}
			}

			index = -1;
			return false;
		}

		/// <summary>Processes a TextEditFrame in rich text to a TextEditFrame in original text</summary>
		public TextEditFrame ProcessRichTextEditFrame(TextEditFrame richTextEditFrame)
		{
			TextEditFrame textEditFrame = new TextEditFrame();
			if(LastRichTextEditFrame.text == richTextEditFrame.text) //No text change
			{
				textEditFrame.text = LastTextEditFrame.text;

				if(richTextEditFrame.selectionStartPosition == LastRichTextEditFrame.selectionStartPosition) //No change
				{
					textEditFrame.selectionStartPosition = LastTextEditFrame.selectionStartPosition;
				}
				else //Selection start position changed
				{
					textEditFrame.selectionStartPosition = DeterminePositionInText(richTextEditFrame.selectionStartPosition, textEditFrame.text);
				}

				if(richTextEditFrame.selectionEndPosition == LastRichTextEditFrame.selectionEndPosition) //No change
				{
					textEditFrame.selectionEndPosition = LastTextEditFrame.selectionEndPosition;
				}
				else //Selection end position changed
				{
					if(richTextEditFrame.selectionStartPosition == richTextEditFrame.selectionEndPosition) //No selection
					{
						textEditFrame.selectionEndPosition = textEditFrame.selectionStartPosition;
					}
					else
					{
						textEditFrame.selectionEndPosition = DeterminePositionInText(richTextEditFrame.selectionEndPosition, textEditFrame.text);
					}
				}
			}
			else
			{
				Debug.LogWarning("Shouldn't be executed here. Rich Text changes should call SetupRichText()");
			}

			LastTextEditFrame = textEditFrame;
			LastRichTextEditFrame = richTextEditFrame;
			return textEditFrame;
		}

		/// <summary>Processes a TextEditFrame in original text to a TextEditFrame in rich text</summary>
		public TextEditFrame ProcessTextEditFrame(TextEditFrame textEditFrame)
		{
			TextEditFrame richTextEditFrame = new TextEditFrame();
			if(textEditFrame.text == LastTextEditFrame.text) //No text change
			{
				richTextEditFrame.text = LastRichTextEditFrame.text;

				if(textEditFrame.selectionStartPosition == LastTextEditFrame.selectionStartPosition) //No selection start position change
				{
					richTextEditFrame.selectionStartPosition = LastRichTextEditFrame.selectionStartPosition;
				}
				else //Selection start position changed
				{
					richTextEditFrame.selectionStartPosition = DeterminePositionInRichText(textEditFrame.selectionStartPosition, richTextEditFrame.text);
				}

				if(textEditFrame.selectionEndPosition == LastTextEditFrame.selectionEndPosition) //No selection end position change
				{
					richTextEditFrame.selectionEndPosition = LastRichTextEditFrame.selectionEndPosition;
				}
				else //Selection end position changed
				{
					if(textEditFrame.selectionStartPosition == textEditFrame.selectionEndPosition) //No selection
					{
						richTextEditFrame.selectionEndPosition = richTextEditFrame.selectionStartPosition;
					}
					else //Has selection
					{
						richTextEditFrame.selectionEndPosition = DeterminePositionInRichText(textEditFrame.selectionEndPosition, richTextEditFrame.text);
					}
				}
			}
			else //Text change
			{
				if(textEditFrame.selectionStartPosition == textEditFrame.selectionEndPosition && LastTextEditFrame.selectionStartPosition != LastTextEditFrame.selectionEndPosition) //Selection cleared
				{
					int previousSelectionAmount = LastTextEditFrame.selectionEndPosition - LastTextEditFrame.selectionStartPosition;
					int insertAmount = textEditFrame.text.Length - (LastTextEditFrame.text.Length - previousSelectionAmount);
					if(insertAmount > 0) //Clear & insert
					{
						DeleteInText(textEditFrame.selectionStartPosition - insertAmount, previousSelectionAmount); //Forward delete from current position
						string textToInsert = textEditFrame.text.Substring(textEditFrame.selectionStartPosition - insertAmount, insertAmount);
						InsertInText(textToInsert, textEditFrame.selectionStartPosition - insertAmount);
					}
					else //Only clear
					{
						DeleteInText(textEditFrame.selectionStartPosition, previousSelectionAmount); //Forward delete from current position
					}
				}
				else //No selection change
				{
					int amount = Mathf.Abs(textEditFrame.text.Length - LastTextEditFrame.text.Length);
					if(textEditFrame.selectionStartPosition > LastTextEditFrame.selectionStartPosition) //Text insert
					{
						if(CheckWordReplaced(textEditFrame.text, LastTextEditFrame.text, LastTextEditFrame.selectionStartPosition, out int wordStartPosition))
						{
							int deleteAmount = (LastTextEditFrame.selectionStartPosition - wordStartPosition);
							DeleteInText(LastTextEditFrame.selectionStartPosition - deleteAmount, deleteAmount);
							amount += deleteAmount;
							string textToInsert = textEditFrame.text.Substring(LastTextEditFrame.selectionStartPosition - deleteAmount, amount);
							InsertInText(textToInsert, LastTextEditFrame.selectionStartPosition - deleteAmount);
						}
						else
						{
							string textToInsert = textEditFrame.text.Substring(LastTextEditFrame.selectionStartPosition, amount);
							InsertInText(textToInsert, LastTextEditFrame.selectionStartPosition);
						}
					}
					else if(textEditFrame.selectionStartPosition < LastTextEditFrame.selectionStartPosition) //Backwards delete
					{
						DeleteInText(textEditFrame.selectionStartPosition, amount); //Forward delete from current position
						if(CheckWordReplaced(textEditFrame.text, LastTextEditFrame.text, textEditFrame.selectionStartPosition, out int wordStartPosition))
						{
							int deleteAmount = (textEditFrame.selectionStartPosition - wordStartPosition);
							DeleteInText(textEditFrame.selectionStartPosition - deleteAmount, deleteAmount);
							amount = deleteAmount;
							string textToInsert = textEditFrame.text.Substring(textEditFrame.selectionStartPosition - deleteAmount, amount);
							InsertInText(textToInsert, textEditFrame.selectionStartPosition - deleteAmount);
						}
					}
					else if(amount > 0) //Forward delete
					{
						DeleteInText(textEditFrame.selectionStartPosition, amount); //Forward delete from current position
					}
					else if(CheckWordReplaced(textEditFrame.text, LastTextEditFrame.text, textEditFrame.selectionStartPosition, out int wordStartPosition))
					{
						int deleteAmount = (textEditFrame.selectionStartPosition - wordStartPosition);
						DeleteInText(textEditFrame.selectionStartPosition - deleteAmount, deleteAmount);
						amount = deleteAmount;
						string textToInsert = textEditFrame.text.Substring(textEditFrame.selectionStartPosition - deleteAmount, amount);
						InsertInText(textToInsert, textEditFrame.selectionStartPosition - deleteAmount);
					}
				}

				richTextEditFrame.text = RebuildRichTextString();
				richTextEditFrame.selectionStartPosition = DeterminePositionInRichText(textEditFrame.selectionStartPosition, richTextEditFrame.text);
				richTextEditFrame.selectionEndPosition = richTextEditFrame.selectionStartPosition;
			}

			LastTextEditFrame = textEditFrame;
			LastRichTextEditFrame = richTextEditFrame;

			return richTextEditFrame;
		}

		public bool CheckWordReplaced(string currentText, string lastText, int textPosition, out int wordStartPosition)
		{
			int currentLength = currentText.Length;
			int lastLength = lastText.Length;
			wordStartPosition = -1;
			bool detectedWordChange = false;

			for(int i = textPosition - 1; i >= 0; i--)
			{
				if(i < currentLength && i < lastLength)
				{
					char currentChar = currentText[i];
					if(currentChar == ' ' || currentChar == '\n')
					{
						break;
					}

					char lastChar = lastText[i];
					if(currentChar == ' ' || currentChar == '\n')
					{
						break;
					}

					if(currentChar != lastChar)
					{
						wordStartPosition = i;
						detectedWordChange = true;
					}
				}
				else
				{
					break;
				}
			}

			return detectedWordChange;
		}

		public void InsertInText(string textToInsert, int textPosition)
		{
			List<TextRegion> insertRegions = ParseTextRegions(textToInsert);

			int length = insertRegions.Count;
			for(int i = 0; i < length; i++)
			{
				TextRegion insertRegion = insertRegions[i];
				string contentToInsert = insertRegion.content;
				bool hasInsertedRegion = false;
				int textOffset = 0;
				int previousTextOffset = 0;

				int regionsLength = textRegions.Count;
				if(regionsLength == 0) //Just insert
				{
					insertRegion.richTextRegions.Add(new RichTextRegion(contentToInsert));
					if(insertRegion.isSymbol)
					{
						insertRegion.ConfigureSymbol();
					}
					textRegions.Add(insertRegion);
					textPosition += contentToInsert.Length;
					continue;
				}
				else
				{
					for(int ri = 0; ri < regionsLength; ri++)
					{
						TextRegion textRegion = textRegions[ri];

						if(textRegion.PositionWithinRegion(textOffset, textPosition, out bool startOfRegion))
						{
							if(startOfRegion) //Add after previous text region
							{
								if(ri == 0)
								{
									insertRegion.richTextRegions.Add(new RichTextRegion(contentToInsert));
									if(insertRegion.isSymbol)
									{
										insertRegion.ConfigureSymbol();
									}

									textRegions.Insert(0, insertRegion);
									textPosition += contentToInsert.Length;
									hasInsertedRegion = true;
									break;
								}

								TextRegion previousRegion = textRegions[ri - 1];
								if(previousRegion.isSymbol) //Add after symbol
								{
									RichTextRegion richTextRegion = new RichTextRegion(contentToInsert);
									List<RichTextRegion> richTextRegions = previousRegion.richTextRegions;
									richTextRegion.CopyTags(richTextRegions[richTextRegions.Count - 1]);
									insertRegion.richTextRegions.Add(richTextRegion);

									if(insertRegion.isSymbol)
									{
										insertRegion.ConfigureSymbol();
									}

									textRegions.Insert(ri, insertRegion);
									textPosition += contentToInsert.Length;
								}
								else
								{
									if(insertRegion.isSymbol)
									{
										RichTextRegion richTextRegion = new RichTextRegion(contentToInsert);
										List<RichTextRegion> richTextRegions = previousRegion.richTextRegions;
										richTextRegion.CopyTags(richTextRegions[richTextRegions.Count - 1]);
										insertRegion.richTextRegions.Add(richTextRegion);

										insertRegion.ConfigureSymbol();

										textRegions.Insert(ri, insertRegion);
										textPosition += contentToInsert.Length;
									}
									else if(previousRegion.TryInsertInText(contentToInsert, previousTextOffset, textPosition))
									{
										textPosition += contentToInsert.Length;
									}
								}
							}
							else //Add in current text region
							{
								if(textRegion.isSymbol) //Add before symbol
								{
									RichTextRegion richTextRegion = new RichTextRegion(contentToInsert);
									if(ri > 0)
									{
										TextRegion previousRegion = textRegions[ri - 1];
										List<RichTextRegion> richTextRegions = previousRegion.richTextRegions;
										richTextRegion.CopyTags(richTextRegions[richTextRegions.Count - 1]);
									}
									insertRegion.richTextRegions.Add(richTextRegion);

									if(insertRegion.isSymbol)
									{
										insertRegion.ConfigureSymbol();
									}

									textRegions.Insert(ri, insertRegion);
									textPosition += contentToInsert.Length;
								}
								else
								{
									if(insertRegion.isSymbol)
									{
										TextRegion[] splitRegions = textRegion.SplitRegion(textOffset, textPosition);
										textRegions.RemoveAt(ri);
										textRegions.Insert(ri, splitRegions[0]);

										RichTextRegion richTextRegion = new RichTextRegion(contentToInsert);
										richTextRegion.CopyTags(splitRegions[0].richTextRegions[0]);
										insertRegion.richTextRegions.Add(richTextRegion);
										insertRegion.ConfigureSymbol();
										textRegions.Insert(ri + 1, insertRegion);
										textRegions.Insert(ri + 2, splitRegions[1]);

										textPosition += contentToInsert.Length;
									}
									else if(textRegion.TryInsertInText(contentToInsert, textOffset, textPosition))
									{
										textPosition += contentToInsert.Length;
									}
								}
							}
							hasInsertedRegion = true;
							break;
						}
						else
						{
							previousTextOffset = textOffset;
							textOffset += textRegion.content.Length;
						}
					}
				}

				if(!hasInsertedRegion)
				{
					if(regionsLength == 0)
					{
						insertRegion.richTextRegions.Add(new RichTextRegion(textToInsert));
						if(insertRegion.isSymbol)
						{
							insertRegion.ConfigureSymbol();
						}

						textRegions.Insert(0, insertRegion);
						textPosition += contentToInsert.Length;
						continue;
					}

					TextRegion previousRegion = textRegions[regionsLength - 1];
					if(previousRegion.isSymbol) //Add after symbol
					{
						RichTextRegion richTextRegion = new RichTextRegion(contentToInsert);
						List<RichTextRegion> richTextRegions = previousRegion.richTextRegions;
						richTextRegion.CopyTags(richTextRegions[richTextRegions.Count - 1]);
						insertRegion.richTextRegions.Add(richTextRegion);

						if(insertRegion.isSymbol)
						{
							insertRegion.ConfigureSymbol();
						}

						textRegions.Insert(regionsLength, insertRegion);
						textPosition += contentToInsert.Length;
					}
					else
					{
						if(insertRegion.isSymbol)
						{
							RichTextRegion richTextRegion = new RichTextRegion(contentToInsert);
							List<RichTextRegion> richTextRegions = previousRegion.richTextRegions;
							richTextRegion.CopyTags(richTextRegions[richTextRegions.Count - 1]);
							insertRegion.richTextRegions.Add(richTextRegion);

							insertRegion.ConfigureSymbol();

							textRegions.Insert(regionsLength, insertRegion);
							textPosition += contentToInsert.Length;
						}
						else if(previousRegion.TryInsertInText(contentToInsert, previousTextOffset, textPosition))
						{
							textPosition += contentToInsert.Length;
						}
					}
				}
			}

			//PrintTextRegions();
		}

		public void PrintTextRegions()
		{
			foreach(TextRegion textRegion in textRegions)
			{
				this.Log("TextRegion: " + textRegion);
				foreach(RichTextRegion richTextRegion in textRegion.richTextRegions)
				{
					this.Log("RichTextRegion: " + richTextRegion);
				}
			}
		}

		public void DeleteInText(int textPosition, int amount)
		{
			int textOffset = 0;
			int richTextOffset = 0;

			int length = textRegions.Count;
			for(int i = 0; i < length; i++)
			{
				TextRegion textRegion = textRegions[i];
				int startTextOffset = textOffset;
				int startRichTextOffset = richTextOffset;
				if(textRegion.TryDeleteInText(ref amount, ref richTextOffset, ref textOffset, textPosition))
				{
					if(textRegion.content.Length == 0)
					{
						textRegions.RemoveAt(i);
						length--;
					}

					if(amount == 0)
					{
						break;
					}
					else //Recheck to make sure that is there no more content to delete
					{
						textOffset = startTextOffset;
						richTextOffset = startRichTextOffset;
						i--;
					}
				}
			}
		}

		public TextEditFrame ToggleTagPair(string startTag, string endTag)
		{
			int start = LastTextEditFrame.selectionStartPosition;
			int end = LastTextEditFrame.selectionEndPosition;
			bool startFound = false;
			bool foundStart = false;
			bool toggleON = false;
			int textOffset = 0;

			int length = textRegions.Count;
			for(int i = 0; i < length; i++)
			{
				TextRegion textRegion = textRegions[i];
				if(startFound)
				{
					bool endFound = textRegion.ToggleTagPairInRichText(start, end, textOffset, startTag, endTag, ref foundStart, ref toggleON);
					if(endFound)
					{
						break;
					}
					else
					{
						textOffset += textRegion.content.Length;
					}
				}
				else
				{
					if(textRegion.PositionWithinRegion(textOffset, start, out bool startOfRegion))
					{
						startFound = true;
						i--; //Recheck
					}
					else
					{
						textOffset += textRegion.content.Length;
					}
				}
			}

			TextEditFrame textEditFrame = LastTextEditFrame;
			TextEditFrame richTextEditFrame = new TextEditFrame();
			richTextEditFrame.text = RebuildRichTextString();
			richTextEditFrame.selectionStartPosition = DeterminePositionInRichText(textEditFrame.selectionStartPosition, richTextEditFrame.text);
			richTextEditFrame.selectionEndPosition = DeterminePositionInRichText(textEditFrame.selectionEndPosition, richTextEditFrame.text); ;

			LastTextEditFrame = textEditFrame;
			LastRichTextEditFrame = richTextEditFrame;
			return richTextEditFrame;
		}

		public int DeterminePositionInText(int richTextPosition, string text)
		{
			int richTextOffset = 0;
			int textOffset = 0;

			int length = textRegions.Count;
			for(int i = 0; i < length; i++)
			{
				if(textRegions[i].TryDeterminePositionInText(richTextPosition, ref richTextOffset, ref textOffset, out int textPosition))
				{
					return textPosition;
				}
			}

			return text.Length;
		}

		public int DeterminePositionInRichText(int textPosition, string richText)
		{
			int richTextOffset = 0;
			int textOffset = 0;

			int length = textRegions.Count;
			for(int i = 0; i < length; i++)
			{
				if(textRegions[i].TryDeterminePositionInRichText(textPosition, ref richTextOffset, ref textOffset, out int richTextPosition))
				{
					return richTextPosition;
				}
			}

			return richText.Length;
		}
	}
}
