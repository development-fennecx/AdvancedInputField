using System.Text;
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Class to show last inserted character while leaving rest of password as hidden</summary>
	public class PasswordCharacterFilter: LiveDecorationFilter
	{
		[SerializeField]
		private float characterVisibleTime = 1;

		private string lastText;
		private bool characterVisible;
		private float lastEditTime;

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
			int length = text.Length;
			if(length == 0)
			{
				lastText = text;
				return string.Empty;
			}

			bool textAdded = false;
			if(lastText == null) { lastText = text; }

			if(text.Length > lastText.Length)
			{
				textAdded = true;
			}
			lastText = text;

			characterVisible = false;
			StringBuilder.Length = 0; //Clears the contents of the StringBuilder

			for(int i = 0; i < length; i++)
			{
				char c = text[i];
				if(i == caretPosition - 1 && textAdded)
				{
					StringBuilder.Append(c);
					characterVisible = true;
					lastEditTime = Time.realtimeSinceStartup;
				}
				else
				{
					StringBuilder.Append('*');
				}
			}

			return StringBuilder.ToString();
		}

		public override int DetermineProcessedCaret(string text, int caretPosition, string processedText)
		{
			return caretPosition; //This filter doesn't add more characters, so just return caret position
		}

		public override int DetermineCaret(string text, string processedText, int processedCaretPosition)
		{
			return processedCaretPosition; //This filter doesn't add more characters, so just return caret position
		}

		public override bool UpdateFilter(out string processedText, bool lastUpdate = false)
		{
			if(characterVisible && ((Time.realtimeSinceStartup - lastEditTime) > characterVisibleTime || lastUpdate))
			{
				characterVisible = false;
				int length = lastText.Length;
				processedText = new string('*', length);
				return true;
			}
			else
			{
				processedText = null;
				return false;
			}
		}
	}
}
