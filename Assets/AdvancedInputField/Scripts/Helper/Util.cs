//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Utility class with helper methods</summary>
	public class Util
	{
		/// <summary>The characters used for word separation</summary>
		private static readonly char[] WORD_SEPARATOR_CHARS = { ' ', '.', ',', '\t', '\r', '\n' };

		/// <summary>The average thumb size of the user in inches</summary>
		private const float PHYSICAL_THUMB_SIZE = 1;

		public static float DeterminePhysicalScreenSize()
		{
			if(Screen.dpi <= 0)
			{
				return -1;
			}

			float width = Screen.width / Screen.dpi;
			float height = Screen.height / Screen.dpi;
			float screenSize = Mathf.Sqrt(Mathf.Pow(width, 2) + Mathf.Pow(height, 2));

			return screenSize;
		}

		/// <summary>The thumb size in screen pixels (diagonal)</summary>
		public static int DetermineThumbSize()
		{
			float physicalScreenSize = DeterminePhysicalScreenSize();
			if(physicalScreenSize <= 0)
			{
				return -1;
			}
			else
			{
				float normalizedThumbSize = (PHYSICAL_THUMB_SIZE / physicalScreenSize);
				float pixelScreenSize = Mathf.Sqrt(Mathf.Pow(Screen.width, 2) + Mathf.Pow(Screen.height, 2));
				float pixels = (pixelScreenSize * normalizedThumbSize) / 2f;

				return Mathf.RoundToInt(pixels);
			}
		}

		public static bool RectTransformIntersects(RectTransform rectTransform1, RectTransform rectTransform2)
		{
			Vector3[] corners1 = new Vector3[4]; //BottomLeft, TopLeft, TopRight, BottomRight
			rectTransform1.GetWorldCorners(corners1);

			Vector3[] corners2 = new Vector3[4]; //BottomLeft, TopLeft, TopRight, BottomRight
			rectTransform2.GetWorldCorners(corners2);

			Vector2 min = corners1[0];
			Vector2 max = corners1[2];

			int length = corners2.Length;
			int outOfBoundsCount = 0;
			for(int i = 0; i < length; i++)
			{
				Vector2 point = corners2[i];

				if(point.x < min.x || point.x > max.x || point.y < min.y || point.y > max.y)
				{
					outOfBoundsCount++;
				}
				else
				{
					break;
				}
			}

			return (outOfBoundsCount != length);
		}

		public static bool Contains(char ch, char[] text, int textLength)
		{
			for(int i = 0; i < textLength; i++)
			{
				if(text[i] == ch) { return true; }
			}

			return false;
		}

		public static int IndexOf(char ch, char[] text, int textLength)
		{
			for(int i = 0; i < textLength; i++)
			{
				if(text[i] == ch) { return i; }
			}

			return -1;
		}

		public static int LastIndexOf(char ch, char[] text, int textLength)
		{
			for(int i = textLength - 1; i >= 0; i--)
			{
				if(text[i] == ch) { return i; }
			}

			return -1;
		}

		public static int CountOccurences(char ch, char[] text, int textLength)
		{
			int occurences = 0;

			for(int i = 0; i < textLength; i++)
			{
				if(text[i] == ch)
				{
					occurences++;
				}
			}

			return occurences;
		}

		public static void StringCopy(ref char[] destination, string source)
		{
			int length = source.Length;
			for(int i = 0; i < length; i++)
			{
				destination[i] = source[i];
			}
		}

		/// <summary>Select current word at caret position</summary>
		public static void DetermineCurrentWordRegion(string text, int caretPosition, out int startPosition, out int endPosition)
		{
			startPosition = FindPreviousWordStart(text, caretPosition);
			endPosition = FindNextWordStart(text, caretPosition);

			string wordRegionString = text.Substring(startPosition, endPosition - startPosition);
			int separatorIndex = wordRegionString.IndexOfAny(WORD_SEPARATOR_CHARS);
			if(separatorIndex != -1) //There a 2 words in word region
			{
				int word1EndIndex = startPosition + separatorIndex;
				int word2StartIndex = startPosition + 1 + wordRegionString.LastIndexOfAny(WORD_SEPARATOR_CHARS);

				if(caretPosition - word1EndIndex < word2StartIndex - caretPosition) //Previous word is closer
				{
					endPosition = word1EndIndex;
				}
				else //Next word is closer
				{
					startPosition = word2StartIndex;
				}
			}
		}

		/// <summary>Finds the start of previous word</summary>
		/// <param name="position">The character position to start checking from</param>
		/// <param name="text">The text to use</param>
		/// <returns>The start position of previous word</returns>
		public static int FindPreviousWordStart(string text, int position)
		{
			if(position - 2 < 0)
			{
				return 0;
			}

			int wordSeparatorPosition = text.LastIndexOfAny(WORD_SEPARATOR_CHARS, position - 2);
			if(wordSeparatorPosition == -1)
			{
				wordSeparatorPosition = 0;
			}
			else
			{
				wordSeparatorPosition++;
			}

			return wordSeparatorPosition;
		}

		/// <summary>Finds the start of next word</summary>
		/// <param name="position">The character position to start checking from</param>
		/// <param name="text">The text to use</param>
		/// <returns>The start position of next word</returns>
		public static int FindNextWordStart(string text, int position)
		{
			if(position + 1 >= text.Length)
			{
				return text.Length;
			}

			int wordSeparatorPosition = text.IndexOfAny(WORD_SEPARATOR_CHARS, position + 1);
			if(wordSeparatorPosition == -1)
			{
				wordSeparatorPosition = text.Length;
			}
			else
			{
				wordSeparatorPosition++;
			}

			return wordSeparatorPosition;
		}

		/// <summary>Finds the character position of next new line</summary>
		/// <param name="position">The character position to start checking from</param>
		/// <returns>The character position of next new line</returns>
		public static int NewLineDownPosition(string text, bool multiline, int position)
		{
			if(!multiline)
			{
				return text.Length;
			}

			if(position + 1 >= text.Length)
			{
				return text.Length - 1;
			}

			int newLinePosition = text.IndexOf('\n', position + 1);
			if(newLinePosition == -1)
			{
				return text.Length - 1;
			}

			return newLinePosition;
		}

		/// <summary>Finds the character position of previous new line</summary>
		/// <param name="position">The character position to start checking from</param>
		/// <returns>The character position of previous new line</returns>
		public static int NewLineUpPosition(string text, bool multiline, int position)
		{
			if(!multiline)
			{
				return 0;
			}

			if(position - 1 <= 0)
			{
				return 0;
			}

			int newLinePosition = text.LastIndexOf('\n', position - 1, position);
			if(newLinePosition == -1)
			{
				return 0;
			}

			return newLinePosition;
		}

		/// <summary>Updates the Text Alignment based on given Text Renderer</summary>
		public static void UpdateTextAlignment(TextRenderer textRenderer, RectTransform contentTransform)
		{
			Vector2 anchor = new Vector2();
			switch(textRenderer.TextAlignment)
			{
				case TextAlignment.TOP_LEFT: anchor = new Vector2(0, 1); break;
				case TextAlignment.TOP: anchor = new Vector2(0.5f, 1); break;
				case TextAlignment.TOP_RIGHT: anchor = new Vector2(1, 1); break;
				case TextAlignment.LEFT: anchor = new Vector2(0, 0.5f); break;
				case TextAlignment.CENTER: anchor = new Vector2(0.5f, 0.5f); break;
				case TextAlignment.RIGHT: anchor = new Vector2(1, 0.5f); break;
				case TextAlignment.BOTTOM_LEFT: anchor = new Vector2(0, 0); break;
				case TextAlignment.BOTTOM: anchor = new Vector2(0.5f, 0); break;
				case TextAlignment.BOTTOM_RIGHT: anchor = new Vector2(1, 0); break;
			}

			contentTransform.anchorMin = anchor;
			contentTransform.anchorMax = anchor;
			contentTransform.pivot = anchor;
		}
	}
}
