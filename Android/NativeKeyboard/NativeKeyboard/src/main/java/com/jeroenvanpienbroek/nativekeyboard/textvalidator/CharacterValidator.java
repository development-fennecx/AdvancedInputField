package com.jeroenvanpienbroek.nativekeyboard.textvalidator;

import org.json.JSONArray;
import org.json.JSONObject;

import static com.jeroenvanpienbroek.nativekeyboard.textvalidator.CharacterRule.CHARACTER_ACTION_VALUES;

public class CharacterValidator
{
    public CharacterRule[] rules;
    public CharacterRule.CharacterAction otherCharacterAction;
    public int otherCharacterActionIntValue;

    public CharacterValidator(JSONObject jsonObject)
    {
        parseJSON(jsonObject);
    }

    private void parseJSON(JSONObject jsonObject)
    {
        try
        {
            JSONArray rulesJSON = jsonObject.getJSONArray("rules");
            int length = rulesJSON.length();
            rules = new CharacterRule[length];
            for(int i = 0; i < length; i++)
            {
                JSONObject ruleJSON = rulesJSON.getJSONObject(i);
                rules[i] = new CharacterRule(ruleJSON);
            }

            otherCharacterAction= CHARACTER_ACTION_VALUES[jsonObject.getInt("otherCharacterAction")];
            otherCharacterActionIntValue = jsonObject.getInt("otherCharacterActionIntValue");
        }
        catch (Exception e)
        {
            e.printStackTrace();
        }
    }

    public char validate(char ch, char[] text, int textLength, int pos, int selectionStartPosition)
    {
        int length = rules.length;
        for(int i = 0; i < length; i++)
        {
            CharacterRule rule = rules[i];
            if(rule.areConditionsMet(ch, text, textLength, pos, selectionStartPosition))
            {
                return executeAction(ch, rule.action, rule.actionIntValue);
            }
        }

        return executeAction(ch, otherCharacterAction, otherCharacterActionIntValue);
    }

    private char executeAction(char ch, CharacterRule.CharacterAction action, int actionValue)
    {
        switch(action)
        {
            case ALLOW: return ch;
            case BLOCK: return (char)0;
            case TO_LOWERCASE:
            if(Character.isLowerCase(ch)) { return ch; }
            else { return Character.toLowerCase(ch); }
            case TO_UPPERCASE:
            if(Character.isUpperCase(ch)) { return ch; }
            else { return Character.toUpperCase(ch); }
            case REPLACE: return (char)actionValue;
        }

        return ch;
    }
}