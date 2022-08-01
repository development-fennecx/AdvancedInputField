using System;
using System.Linq;

namespace AdvancedInputFieldPlugin
{
	public enum CharacterConditionOperator
	{
		VALUE_EQUALS,
		VALUE_SMALLER_THAN,
		VALUE_SMALLER_THAN_OR_EQUALS,
		VALUE_GREATER_THAN,
		VALUE_GREATER_THAN_OR_EQUALS,
		VALUE_BETWEEN_INCLUSIVE,
		VALUE_BETWEEN_EXCLUSIVE,
		VALUE_IN_STRING,
		INDEX_EQUALS,
		INDEX_SMALLER_THAN,
		INDEX_SMALLER_THAN_OR_EQUALS,
		INDEX_GREATER_THAN,
		INDEX_GREATER_THAN_OR_EQUALS,
		INDEX_BETWEEN_INCLUSIVE,
		INDEX_BETWEEN_EXCLUSIVE,
		OCCURENCES_SMALLER_THAN,
		OCCURENCES_SMALLER_THAN_OR_EQUALS,
		OCCURENCES_GREATER_THAN,
		OCCURENCES_GREATER_THAN_OR_EQUALS
	}

	[Serializable]
	public class CharacterCondition
	{
		public CharacterConditionOperator conditionOperator;
		public int conditionIntValue1;
		public int conditionIntValue2;
		public string conditionStringValue;

		public bool IsConditionMet(char ch, char[] text, int textLength, int pos, int selectionStartPosition)
		{
			switch(conditionOperator)
			{
				case CharacterConditionOperator.VALUE_EQUALS:
					if((int)ch == conditionIntValue1) { return true; }
					break;
				case CharacterConditionOperator.VALUE_SMALLER_THAN:
					if((int)ch < conditionIntValue1) { return true; }
					break;
				case CharacterConditionOperator.VALUE_SMALLER_THAN_OR_EQUALS:
					if((int)ch <= conditionIntValue1) { return true; }
					break;
				case CharacterConditionOperator.VALUE_GREATER_THAN:
					if((int)ch > conditionIntValue1) { return true; }
					break;
				case CharacterConditionOperator.VALUE_GREATER_THAN_OR_EQUALS:
					if((int)ch >= conditionIntValue1) { return true; }
					break;
				case CharacterConditionOperator.VALUE_BETWEEN_EXCLUSIVE:
					if((int)ch > conditionIntValue1 && (int)ch < conditionIntValue2) { return true; }
					break;
				case CharacterConditionOperator.VALUE_BETWEEN_INCLUSIVE:
					if((int)ch >= conditionIntValue1 && (int)ch <= conditionIntValue2) { return true; }
					break;
				case CharacterConditionOperator.VALUE_IN_STRING:
					if(conditionStringValue.Contains(ch)) { return true; }
					break;
				case CharacterConditionOperator.INDEX_EQUALS:
					if(pos == conditionIntValue1) { return true; }
					break;
				case CharacterConditionOperator.INDEX_SMALLER_THAN:
					if(pos < conditionIntValue1) { return true; }
					break;
				case CharacterConditionOperator.INDEX_SMALLER_THAN_OR_EQUALS:
					if(pos <= conditionIntValue1) { return true; }
					break;
				case CharacterConditionOperator.INDEX_GREATER_THAN:
					if(pos > conditionIntValue1) { return true; }
					break;
				case CharacterConditionOperator.INDEX_GREATER_THAN_OR_EQUALS:
					if(pos >= conditionIntValue1) { return true; }
					break;
				case CharacterConditionOperator.INDEX_BETWEEN_EXCLUSIVE:
					if(pos > conditionIntValue1 && pos < conditionIntValue2) { return true; }
					break;
				case CharacterConditionOperator.INDEX_BETWEEN_INCLUSIVE:
					if(pos >= conditionIntValue1 && pos <= conditionIntValue2) { return true; }
					break;
				case CharacterConditionOperator.OCCURENCES_SMALLER_THAN:
					if(Util.CountOccurences(ch, text, textLength) < conditionIntValue2) { return true; }
					break;
				case CharacterConditionOperator.OCCURENCES_SMALLER_THAN_OR_EQUALS:
					if(Util.CountOccurences(ch, text, textLength) <= conditionIntValue2) { return true; }
					break;
				case CharacterConditionOperator.OCCURENCES_GREATER_THAN:
					if(Util.CountOccurences(ch, text, textLength) > conditionIntValue2) { return true; }
					break;
				case CharacterConditionOperator.OCCURENCES_GREATER_THAN_OR_EQUALS:
					if(Util.CountOccurences(ch, text, textLength) >= conditionIntValue2) { return true; }
					break;
			}

			return false;
		}
	}
}
