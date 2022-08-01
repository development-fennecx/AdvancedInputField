using System.Text;

namespace AdvancedInputFieldPlugin
{
	public class BulletPointFilter: LiveProcessingFilter
	{
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
						return ApplyBulletPoints(textEditFrame);
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
						return ApplyBulletPoints(textEditFrame);
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

		public TextEditFrame ApplyBulletPoints(TextEditFrame textEditFrame)
		{
			StringBuilder.Clear();
			StringBuilder.Append(textEditFrame.text);

			int length = textEditFrame.text.Length;
			for(int i = 0; i < length; i++)
			{
				char c = textEditFrame.text[i];
				if(c == '\n')
				{
					if(i + 1 < length)
					{
						char nextChar = textEditFrame.text[i + 1];
						if(nextChar != '•')
						{
							StringBuilder.Insert(i + 1, "•");
							if(textEditFrame.selectionStartPosition == i + 1) //Check if we need to move the caret
							{
								textEditFrame.selectionStartPosition = i + 2;
								textEditFrame.selectionEndPosition = i + 2;
							}
						}
					}
					else
					{
						StringBuilder.Insert(i + 1, "•");
						if(textEditFrame.selectionStartPosition == i + 1) //Check if we need to move the caret
						{
							textEditFrame.selectionStartPosition = i + 2;
							textEditFrame.selectionEndPosition = i + 2;
						}
					}
				}
			}
			textEditFrame.text = StringBuilder.ToString();

			return textEditFrame;
		}
	}
}
