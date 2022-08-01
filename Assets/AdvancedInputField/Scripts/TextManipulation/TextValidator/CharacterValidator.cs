using System;
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	[Serializable]
	public class CharacterValidator: ScriptableObject
	{
		public CharacterRule[] rules;
		public CharacterAction otherCharacterAction;
		public int otherCharacterActionIntValue;

		public char Validate(char ch, char[] text, int textLength, int pos, int selectionStartPosition)
		{
			int length = rules.Length;
			for(int i = 0; i < length; i++)
			{
				CharacterRule rule = rules[i];
				if(rule.AreConditionsMet(ch, text, textLength, pos, selectionStartPosition))
				{
					return ExecuteAction(ch, rule.action, rule.actionIntValue);
				}
			}

			return ExecuteAction(ch, otherCharacterAction, otherCharacterActionIntValue);
		}

		private char ExecuteAction(char ch, CharacterAction action, int actionValue)
		{
			switch(action)
			{
				case CharacterAction.ALLOW: return ch;
				case CharacterAction.BLOCK: return (char)0;
				case CharacterAction.TO_LOWERCASE:
					if(char.IsLower(ch)) { return ch; }
					else { return char.ToLower(ch); }
				case CharacterAction.TO_UPPERCASE:
					if(char.IsUpper(ch)) { return ch; }
					else { return char.ToUpper(ch); }
				case CharacterAction.REPLACE: return (char)actionValue;
			}

			return ch;
		}
	}
}
