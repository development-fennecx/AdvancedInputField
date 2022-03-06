package com.jeroenvanpienbroek.nativekeyboard.textvalidator;

import android.util.Log;

import org.json.JSONArray;
import org.json.JSONObject;

public class CharacterRule
{
    public enum CharacterAction
    {
        ALLOW,
        BLOCK,
        TO_UPPERCASE,
        TO_LOWERCASE,
        REPLACE
    }

    public static final CharacterAction[] CHARACTER_ACTION_VALUES = CharacterAction.values();

    public CharacterCondition[] conditions;
    public CharacterAction action;
    public int actionIntValue;

    public CharacterRule(JSONObject jsonObject)
    {
        parseJSON(jsonObject);
    }

    private void parseJSON(JSONObject jsonObject)
    {
        try
        {
            JSONArray conditionsJSON = jsonObject.getJSONArray("conditions");
            int length = conditionsJSON.length();
            conditions = new CharacterCondition[length];
            for(int i = 0; i < length; i++)
            {
                JSONObject conditionJSON = conditionsJSON.getJSONObject(i);
                conditions[i] = new CharacterCondition(conditionJSON);
            }

            action = CHARACTER_ACTION_VALUES[jsonObject.getInt("action")];
            actionIntValue = jsonObject.getInt("actionIntValue");
        }
        catch (Exception e)
        {
            e.printStackTrace();
        }
    }

    public boolean areConditionsMet(char ch, char[] text, int textLength, int pos, int selectionStartPosition)
    {
        int length = conditions.length;
        for(int i = 0; i < length; i++)
        {
            if(!conditions[i].isConditionMet(ch, text, textLength, pos, selectionStartPosition)) { return false; }
        }

        return (length > 0);
    }
}
