package com.jeroenvanpienbroek.nativekeyboard.textvalidator;

import android.util.Log;

import com.jeroenvanpienbroek.nativekeyboard.Util;

import org.json.JSONObject;

public class CharacterCondition
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
        OCCURENCES_GREATER_THAN_OR_EQUALS,
        VALUE_SAME_AS_PREVIOUS
    }

    public static final CharacterConditionOperator[] CHARACTER_CONDITION_OPERATOR_VALUES = CharacterConditionOperator.values();

    public CharacterConditionOperator conditionOperator;
    public int conditionIntValue1;
    public int conditionIntValue2;
    public String conditionStringValue;

    public CharacterCondition(JSONObject jsonObject)
    {
        parseJSON(jsonObject);
    }

    private void parseJSON(JSONObject jsonObject)
    {
        try
        {
            conditionOperator = CHARACTER_CONDITION_OPERATOR_VALUES[jsonObject.getInt("conditionOperator")];
            conditionIntValue1 = jsonObject.getInt("conditionIntValue1");
            conditionIntValue2 = jsonObject.getInt("conditionIntValue2");
            conditionStringValue = jsonObject.getString("conditionStringValue");
        }
        catch (Exception e)
        {
            e.printStackTrace();
        }
    }

    public boolean isConditionMet(char ch, char[] text, int textLength, int pos, int selectionStartPosition)
    {
        switch (conditionOperator)
        {
            case VALUE_EQUALS:
                if ((int) ch == conditionIntValue1)
                {
                    return true;
                }
                break;
            case VALUE_SMALLER_THAN:
                if ((int) ch < conditionIntValue1)
                {
                    return true;
                }
                break;
            case VALUE_SMALLER_THAN_OR_EQUALS:
                if ((int) ch <= conditionIntValue1)
                {
                    return true;
                }
                break;
            case VALUE_GREATER_THAN:
                if ((int) ch > conditionIntValue1)
                {
                    return true;
                }
                break;
            case VALUE_GREATER_THAN_OR_EQUALS:
                if ((int) ch >= conditionIntValue1)
                {
                    return true;
                }
                break;
            case VALUE_BETWEEN_EXCLUSIVE:
                if ((int) ch > conditionIntValue1 && (int) ch < conditionIntValue2)
                {
                    return true;
                }
                break;
            case VALUE_BETWEEN_INCLUSIVE:
                if ((int) ch >= conditionIntValue1 && (int) ch <= conditionIntValue2)
                {
                    return true;
                }
                break;
            case VALUE_IN_STRING:
                if (conditionStringValue.contains(ch + ""))
                {
                    return true;
                }
                break;
            case INDEX_EQUALS:
                if (pos == conditionIntValue1)
                {
                    return true;
                }
                break;
            case INDEX_SMALLER_THAN:
                if (pos < conditionIntValue1)
                {
                    return true;
                }
                break;
            case INDEX_SMALLER_THAN_OR_EQUALS:
                if (pos <= conditionIntValue1)
                {
                    return true;
                }
                break;
            case INDEX_GREATER_THAN:
                if (pos > conditionIntValue1)
                {
                    return true;
                }
                break;
            case INDEX_GREATER_THAN_OR_EQUALS:
                if (pos >= conditionIntValue1)
                {
                    return true;
                }
                break;
            case INDEX_BETWEEN_EXCLUSIVE:
                if (pos > conditionIntValue1 && pos < conditionIntValue2)
                {
                    return true;
                }
                break;
            case INDEX_BETWEEN_INCLUSIVE:
                if (pos >= conditionIntValue1 && pos <= conditionIntValue2)
                {
                    return true;
                }
                break;
            case OCCURENCES_SMALLER_THAN:
                if (Util.countOccurences(ch, text, textLength) < conditionIntValue2)
                {
                    return true;
                }
                break;
            case OCCURENCES_SMALLER_THAN_OR_EQUALS:
                if (Util.countOccurences(ch, text, textLength) <= conditionIntValue2)
                {
                    return true;
                }
                break;
            case OCCURENCES_GREATER_THAN:
                if (Util.countOccurences(ch, text, textLength) > conditionIntValue2)
                {
                    return true;
                }
                break;
            case OCCURENCES_GREATER_THAN_OR_EQUALS:
                if (Util.countOccurences(ch, text, textLength) >= conditionIntValue2)
                {
                    return true;
                }
                break;
            case VALUE_SAME_AS_PREVIOUS:
                if(pos == 0) { return false; }
                if(ch == text[pos - 1]) { return true; }
                break;
        }

        return false;
    }
}