using System;

namespace AdvancedInputFieldPlugin
{
	public enum CharacterAction
	{
		ALLOW,
		BLOCK,
		TO_UPPERCASE,
		TO_LOWERCASE,
		REPLACE
	}

	[Serializable]
	public class CharacterRule
	{
		public CharacterCondition[] conditions;
		public CharacterAction action;
		public int actionIntValue;

		public bool AreConditionsMet(char ch, char[] text, int textLength, int pos, int selectionStartPosition)
		{
			int length = conditions.Length;
			for(int i = 0; i < length; i++)
			{
				if(!conditions[i].IsConditionMet(ch, text, textLength, pos, selectionStartPosition)) { return false; }
			}

			return (length > 0);
		}
	}
}
