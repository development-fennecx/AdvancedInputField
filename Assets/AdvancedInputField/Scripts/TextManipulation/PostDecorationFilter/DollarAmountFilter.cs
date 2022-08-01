//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using System.Globalization;
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Class to format text as dollar amount</summary>
	public class DollarAmountFilter: PostDecorationFilter
	{
		public DollarAmountFilter()
		{
		}

		/// <summary>Formats text as dollar amount</summary>
		/// <param name="text">The input text</param>
		/// <param name="filteredText">The output text</param>
		public override bool ProcessText(string text, out string filteredText)
		{
			long number = 0;
			if(long.TryParse(text, out number))
			{
				filteredText = '$' + number.ToString("N0", CultureInfo.CurrentCulture);
				return true;
			}
			else
			{
				if(!string.IsNullOrEmpty(text))
				{
					Debug.LogWarningFormat("Couldn't filter \'{0}\'. It's not a valid number string or number is too big", text);
				}
				filteredText = null;
				return false;
			}
		}
	}
}
