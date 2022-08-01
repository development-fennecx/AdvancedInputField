using System.Text;
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Class to format text as date (00/00/0000)</summary>
	public class DateFilter: LiveDecorationFilter
	{
		/// <summary>The maximum amount of number characters</summary>
		private const int MAX_NUMBERS = 8;

		/// <summary>The character used to separate numbers</summary>
		[SerializeField]
		private char separatorCharacter = '/';

		/// <summary>The StringBuilder</summary>
		private StringBuilder stringBuilder;

		/// <summary>The StringBuilder</summary>
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

		public override string ProcessText(string text, int caretPosition)
		{
			StringBuilder.Length = 0; //Clears the contents of the StringBuilder
			int numberCount = 0;

			int length = text.Length;
			if(length == 0)
			{
				return string.Empty;
			}

			for(int i = 0; i < length; i++)
			{
				char c = text[i];

				if(IsNumber(c))
				{
					numberCount++;
					if(numberCount > MAX_NUMBERS)
					{
						Debug.LogWarning("There are more than 8 number characters. Please set the character limit to 8 to support the date format (00/00/0000)");
						return null;
					}

					if(numberCount == 3 || numberCount == 5) //3th or 5th number found, so add separator char first
					{
						StringBuilder.Append(separatorCharacter);
					}

					StringBuilder.Append(c);
				}
				else
				{
					Debug.LogWarning("Unexpected character: " + c);
					return string.Empty;
				}
			}

			return StringBuilder.ToString();
		}

		public override int DetermineProcessedCaret(string text, int caretPosition, string processedText)
		{
			if(caretPosition == 0)
			{
				return 0;
			}

			int length = processedText.Length;
			if(length == 0)
			{
				return 0;
			}

			int numberCount = 0;
			for(int i = 0; i < length; i++)
			{
				char c = processedText[i];

				if(IsNumber(c))
				{
					numberCount++;

					if(numberCount == caretPosition + 1)
					{
						return i;
					}
				}
			}

			return length;
		}

		public override int DetermineCaret(string text, string processedText, int processedCaretPosition)
		{
			if(processedCaretPosition == 0)
			{
				return 0;
			}

			int length = processedText.Length;
			if(length == 0)
			{
				return 0;
			}

			if(processedCaretPosition == length)
			{
				return text.Length;
			}

			int numberCount = 0;
			for(int i = 0; i < processedCaretPosition; i++)
			{
				char c = processedText[i];

				if(IsNumber(c))
				{
					numberCount++;
				}
			}

			return numberCount;
		}

		private bool IsNumber(char c)
		{
			return (c >= '0' && c <= '9');
		}
	}
}
