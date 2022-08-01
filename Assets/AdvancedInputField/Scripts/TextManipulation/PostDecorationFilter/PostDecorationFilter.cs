//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using System;
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Class to format text when InputField get deselected</summary>
	[Serializable]
	public abstract class PostDecorationFilter: MonoBehaviour
	{
		/// <summary>Formats text in a specific way</summary>
		/// <param name="text">The input text</param>
		/// <param name="filteredText">The output text</param>
		public abstract bool ProcessText(string text, out string filteredText);
	}
}
