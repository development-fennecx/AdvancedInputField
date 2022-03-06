package com.jeroenvanpienbroek.nativekeyboard;

import android.util.Log;

import com.jeroenvanpienbroek.nativekeyboard.textvalidator.CharacterValidator;

import org.json.JSONArray;
import org.json.JSONObject;

public class NativeKeyboardConfiguration
{
    private final NativeKeyboard.KeyboardType[] keyboardTypeValues = NativeKeyboard.KeyboardType.values();
    private final NativeKeyboard.CharacterValidation[] characterValidationValues = NativeKeyboard.CharacterValidation.values();
    private final NativeKeyboard.LineType[] lineTypeValues = NativeKeyboard.LineType.values();
    private final NativeKeyboard.AutocapitalizationType[] autocapitalizationTypeValues = NativeKeyboard.AutocapitalizationType.values();
    private final NativeKeyboard.AutofillType[] autofillTypeValues = NativeKeyboard.AutofillType.values();
    private final NativeKeyboard.ReturnKeyType[] returnKeyTypeValues = NativeKeyboard.ReturnKeyType.values();

    public NativeKeyboard.KeyboardType keyboardType;
    public NativeKeyboard.CharacterValidation characterValidation;
    public NativeKeyboard.LineType lineType;
    public NativeKeyboard.AutocapitalizationType autocapitalizationType;
    public NativeKeyboard.AutofillType autofillType;
    public NativeKeyboard.ReturnKeyType returnKeyType;
    public boolean autocorrection;
    public boolean secure;
    public boolean richTextEditing;
    public boolean emojisAllowed;
    public boolean hasNext;
    public int characterLimit;
    public CharacterValidator characterValidator;

    public NativeKeyboardConfiguration(JSONObject jsonObject)
    {
        parseJSON(jsonObject);
    }

    private void parseJSON(JSONObject jsonObject)
    {
        try
        {
            keyboardType = keyboardTypeValues[jsonObject.getInt("keyboardType")];
            characterValidation = characterValidationValues[jsonObject.getInt("characterValidation")];
            lineType = lineTypeValues[jsonObject.getInt("lineType")];
            autocapitalizationType = autocapitalizationTypeValues[jsonObject.getInt("autocapitalizationType")];
            autofillType = autofillTypeValues[jsonObject.getInt("autofillType")];
            returnKeyType = returnKeyTypeValues[jsonObject.getInt("returnKeyType")];
            autocorrection = jsonObject.getBoolean("autocorrection");
            secure = jsonObject.getBoolean("secure");
            richTextEditing = jsonObject.getBoolean("richTextEditing");
            emojisAllowed = jsonObject.getBoolean("emojisAllowed");
            hasNext = jsonObject.getBoolean("hasNext");
            characterLimit = jsonObject.getInt("characterLimit");

            String characterValidatorJSON = jsonObject.getString("characterValidatorJSON");
            if(characterValidatorJSON != null && characterValidatorJSON.length() > 0)
            {
                try
                {
                    JSONObject characterValidatorJsonObject = new JSONObject(characterValidatorJSON);
                    characterValidator = new CharacterValidator(characterValidatorJsonObject);
                }
                catch (Exception e)
                {
                    e.printStackTrace();
                }
            }
        }
        catch (Exception e)
        {
            e.printStackTrace();
        }
    }
}
