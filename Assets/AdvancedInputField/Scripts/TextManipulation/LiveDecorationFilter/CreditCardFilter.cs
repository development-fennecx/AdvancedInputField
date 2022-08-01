using System.Text;
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Class to format text as credit card number separated by spaces every 4 numbers</summary>
	public class CreditCardFilter: LiveDecorationFilter
	{
		/// <summary>The maximum amount of separator characters to use</summary>
		private const int MAX_SEPARATOR_CHARACTERS = 3;

		/// <summary>The character used to separate groups of 4 numbers</summary>
		[SerializeField]
		private char separatorCharacter = ' ';

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
			int separatorCount = 0;

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

					if(numberCount == 5 && separatorCount < MAX_SEPARATOR_CHARACTERS) //This is the 5th number found, so add separator char first
					{
						numberCount = 1;
						StringBuilder.Append(separatorCharacter);
						separatorCount++;
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
