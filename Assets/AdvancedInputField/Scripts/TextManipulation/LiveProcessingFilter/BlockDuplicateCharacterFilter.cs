using System.Collections.Generic;

namespace AdvancedInputFieldPlugin
{
	public class BlockDuplicateCharacterFilter: LiveProcessingFilter
	{
		public override TextEditFrame ProcessTextEditUpdate(TextEditFrame textEditFrame, TextEditFrame lastTextEditFrame)
		{
			if(textEditFrame.text == lastTextEditFrame.text) //No text change
			{
				return textEditFrame; //No processing needed, so allow change by returning current frame
			}
			else //Text change
			{
				List<char> characters = new List<char>();

				int length = textEditFrame.text.Length;
				for(int i = 0; i < length; i++)
				{
					char c = textEditFrame.text[i];
					if(characters.Contains(c))
					{
						return lastTextEditFrame; //Found duplicate, so block this change by returning last frame
					}

					characters.Add(c);
				}

				return textEditFrame; //No duplicates detected, so allow change by returning current frame
			}
		}
	}
}
