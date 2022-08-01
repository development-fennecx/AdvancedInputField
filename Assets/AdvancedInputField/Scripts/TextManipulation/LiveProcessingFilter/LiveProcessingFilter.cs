//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Class to format text whenever the text of caret position changes</summary>
	public abstract class LiveProcessingFilter: MonoBehaviour
	{
		public abstract TextEditFrame ProcessTextEditUpdate(TextEditFrame textEditFrame, TextEditFrame lastTextEditFrame);
	}
}
