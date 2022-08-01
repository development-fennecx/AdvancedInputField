//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using System.Text;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Class to format text as password (can be used to hide text when done editing)</summary>
	public class PasswordFilter: PostDecorationFilter
	{
		/// <summary>The StringBuilder</summary>
		private StringBuilder stringBuilder;

		public PasswordFilter()
		{
			stringBuilder = new StringBuilder();
		}

		/// <summary>Formats text as password</summary>
		/// <param name="text">The input text</param>
		/// <param name="filteredText">The output text</param>
		public override bool ProcessText(string text, out string filteredText)
		{
			stringBuilder.Length = 0; //Clears the contents of the StringBuilder

			int length = text.Length;
			for(int i = 0; i < length; i++)
			{
				stringBuilder.Append('*');
			}

			filteredText = stringBuilder.ToString();
			return true;
		}
	}
}
