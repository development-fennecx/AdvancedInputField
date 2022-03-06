using BetterJSON;

namespace NativeKeyboardUWP
{
	public enum CharacterAction
	{
		ALLOW,
		BLOCK,
		TO_UPPERCASE,
		TO_LOWERCASE,
		REPLACE
	}

	public class CharacterRule
	{
		public CharacterCondition[] conditions;
		public CharacterAction action;
		public int actionIntValue;

		public CharacterRule(JSONObject jsonObject)
		{
			ParseJSON(jsonObject);
		}

		private void ParseJSON(JSONObject jsonObject)
		{
			JSONArray conditionsJSON = jsonObject["conditions"].Array;
			int length = conditionsJSON.Length;
			conditions = new CharacterCondition[length];
			for(int i = 0; i < length; i++)
			{
				JSONObject conditionJSON = conditionsJSON[i].Object;
				conditions[i] = new CharacterCondition(conditionJSON);
			}

			action = (CharacterAction)jsonObject["action"].Integer;
			actionIntValue = jsonObject["actionIntValue"].Integer;
		}

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
