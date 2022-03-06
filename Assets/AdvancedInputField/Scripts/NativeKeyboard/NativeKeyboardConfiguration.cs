// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;

namespace AdvancedInputFieldPlugin
{
	[Serializable]
	public class NativeKeyboardConfiguration
	{
		public KeyboardType keyboardType;
		public CharacterValidation characterValidation;
		public LineType lineType;
		public AutocapitalizationType autocapitalizationType;
		public AutofillType autofillType;
		public ReturnKeyType returnKeyType;
		public bool autocorrection;
		public bool secure;
		public bool richTextEditing;
		public bool emojisAllowed;
		public bool hasNext;
		public int characterLimit;
		public string characterValidatorJSON;
	}
}
