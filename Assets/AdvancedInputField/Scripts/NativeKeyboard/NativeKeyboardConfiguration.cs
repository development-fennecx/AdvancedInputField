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
