using BetterJSON;

namespace NativeKeyboardUWP
{
	public class NativeKeyboardConfiguration
	{
		public KeyboardType keyboardType;
		public CharacterValidation characterValidation;
		public LineType lineType;
		public AutocapitalizationType autocapitalizationType;
		public ReturnKeyType returnKeyType;
		public bool autocorrection;
		public bool secure;
		public bool emojisAllowed;
		public bool hasNext;
		public int characterLimit;
		public CharacterValidator characterValidator;

		public NativeKeyboardConfiguration(JSONObject jsonObject)
		{
			ParseJSON(jsonObject);
		}

		private void ParseJSON(JSONObject jsonObject)
		{
			keyboardType = (KeyboardType)jsonObject["keyboardType"].Integer;
			characterValidation = (CharacterValidation)jsonObject["characterValidation"].Integer;
			lineType = (LineType)jsonObject["lineType"].Integer;
			autocapitalizationType = (AutocapitalizationType)jsonObject["autocapitalizationType"].Integer;
			returnKeyType = (ReturnKeyType)jsonObject["returnKeyType"].Integer;
			autocorrection = jsonObject["autocorrection"].Boolean;
			secure = jsonObject["secure"].Boolean;
			emojisAllowed = jsonObject["emojisAllowed"].Boolean;
			hasNext = jsonObject["hasNext"].Boolean;
			characterLimit = jsonObject["characterLimit"].Integer;

			string characterValidatorValue = jsonObject["characterValidatorJSON"].String;
			if(!string.IsNullOrEmpty(characterValidatorValue))
			{
				JSONObject characterValidatorJSON = new JSONObject(characterValidatorValue);
				characterValidator = new CharacterValidator(characterValidatorJSON);
			}
		}
	}
}
