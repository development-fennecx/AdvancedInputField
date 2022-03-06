// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

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
