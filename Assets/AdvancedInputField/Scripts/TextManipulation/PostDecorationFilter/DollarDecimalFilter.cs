//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using System.Globalization;
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Class to format text as dollar decimal amount</summary>
	public class DollarDecimalFilter: PostDecorationFilter
	{
		/// <summary>CultureInfo that uses a dot for decimals</summary>
		private CultureInfo decimalDotCulture; //Workaround for broken CultureInfo.CurrentCulture: https://feedback.unity3d.com/suggestions/fix-localization-issues-with-cor

		/// <summary>CultureInfo that uses a comma for decimals</summary>
		private CultureInfo decimalCommaCulture; //Workaround for broken CultureInfo.CurrentCulture: https://feedback.unity3d.com/suggestions/fix-localization-issues-with-cor

		public DollarDecimalFilter()
		{
			decimalDotCulture = new CultureInfo("en-US"); //Just pick a culture that uses a point for decimal values
			decimalCommaCulture = new CultureInfo("nl-NL"); //Just pick a culture that uses a comma for decimal values
		}

		/// <summary>Formats text as dollar decimal amount</summary>
		/// <param name="text">The input text</param>
		/// <param name="filteredText">The output text</param>
		public override bool ProcessText(string text, out string filteredText)
		{
			double decimalValue = 0;
			if(double.TryParse(text, NumberStyles.Any ^ NumberStyles.AllowThousands, decimalDotCulture, out decimalValue))
			{
				filteredText = '$' + decimalValue.ToString("0.00", decimalDotCulture);
				return true;
			}
			else if(double.TryParse(text, NumberStyles.Any ^ NumberStyles.AllowThousands, decimalCommaCulture, out decimalValue))
			{
				filteredText = '$' + decimalValue.ToString("0.00", decimalCommaCulture);
				return true;
			}
			else
			{
				if(!string.IsNullOrEmpty(text))
				{
					Debug.LogWarningFormat("Couldn't filter \'{0}\'. It's not a valid decimal string or decimal is too big", text);
				}
				filteredText = null;
				return false;
			}
		}
	}
}
