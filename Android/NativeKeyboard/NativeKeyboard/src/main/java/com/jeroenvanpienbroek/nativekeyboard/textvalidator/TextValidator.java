//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

package com.jeroenvanpienbroek.nativekeyboard.textvalidator;

import android.util.Log;

import com.jeroenvanpienbroek.nativekeyboard.NativeKeyboard;
import com.jeroenvanpienbroek.nativekeyboard.Util;

public class TextValidator
{
    private final String EMAIL_SPECIAL_CHARACTERS = "!#$%&'*+-/=?^_`{|}~";

    private NativeKeyboard.CharacterValidation validation;
    private CharacterValidator validator;
    private NativeKeyboard.LineType lineType;
    private String resultText;
    private int resultCaretPosition;

    public NativeKeyboard.CharacterValidation getValidation()
    {
        return validation;
    }
    public NativeKeyboard.LineType getLineType()
    {
        return lineType;
    }
    public String getResultText(){ return resultText; }
    public int getResultCaretPosition(){ return resultCaretPosition; }

    public  void setValidation(NativeKeyboard.CharacterValidation validation)
    {
        this.validation = validation;
    }

    public void setValidator(CharacterValidator validator)
    {
        this.validator = validator;
    }

    public void setLineType(NativeKeyboard.LineType lineType)
    {
        this.lineType = lineType;
    }

    public void validate(String text, String textToAppend, int caretPosition, int selectionStartPosition)
    {
        int textLength = text.length();
        int textToAppendLength = textToAppend.length();

        int startCaretPosition = caretPosition;
        char[] buffer = new char[textLength + textToAppendLength];
        Util.stringCopy(buffer, text);

        int position = 0;
        for(int i = 0; i < textToAppendLength; i++)
        {
            char ch = textToAppend.charAt(i);
            char result = validateChar(ch, buffer, position, position, caretPosition, selectionStartPosition);
            if(result != 0)
            {
                buffer[position] = result;
                position++;
                caretPosition++;
            }
        }

        if(startCaretPosition < textLength)
        {
            for(int i = startCaretPosition; i < textLength; i++)
            {
                char ch = text.charAt(i);
                char result = validateChar(ch, buffer, position, position, caretPosition, selectionStartPosition);
                if(result != 0)
                {
                    buffer[position] = result;
                    position++;
                }
            }
        }

        textLength = position;

        resultText = new String(buffer, 0, textLength);
        resultCaretPosition = caretPosition;
    }

    private char validateChar(char ch, char[] text, int textLength, int pos, int caretPosition, int selectionStartPosition)
    {
        if(lineType != NativeKeyboard.LineType.MULTI_LINE_NEWLINE && (ch == '\r' || ch == '\n'))
        {
            return (char)0;
        }

        // Validation is disabled
        if(validation == NativeKeyboard.CharacterValidation.NONE)
        {
            return ch;
        }

        if(validation == NativeKeyboard.CharacterValidation.CUSTOM)
        {
            if(validator == null) { return ch; }
            else
            {
                return validator.validate(ch, text, textLength, pos, selectionStartPosition);
            }
        }

        if(validation == NativeKeyboard.CharacterValidation.INTEGER || validation == NativeKeyboard.CharacterValidation.DECIMAL || validation == NativeKeyboard.CharacterValidation.DECIMAL_FORCE_POINT)
        {
            // Integer and decimal
            boolean cursorBeforeDash = (pos == 0 && textLength > 0 && text[0] == '-');
            boolean dashInSelection = textLength > 0 && text[0] == '-' && ((caretPosition == 0 && selectionStartPosition > 0) || (selectionStartPosition == 0 && caretPosition > 0));
            boolean selectionAtStart = caretPosition == 0 || selectionStartPosition == 0;
            if(!cursorBeforeDash || dashInSelection)
            {
                if(ch >= '0' && ch <= '9') return ch;
                if(ch == '-' && (pos == 0 || selectionAtStart)) return ch;
                if(validation == NativeKeyboard.CharacterValidation.DECIMAL)
                {
                    if(ch == '.' || ch == ',')
                    {
                        if(!Util.contains('.', text, textLength) && !Util.contains(',', text, textLength)) return ch;
                    }
                }
                else if(validation == NativeKeyboard.CharacterValidation.DECIMAL_FORCE_POINT)
                {
                    if (ch == '.' && !Util.contains('.', text, textLength)) return ch;
                    if (ch == ',' && !Util.contains('.', text, textLength)) return '.';
                }
            }
        }
        else if(validation == NativeKeyboard.CharacterValidation.ALPHANUMERIC)
        {
            // All alphanumeric characters
            if(ch >= 'A' && ch <= 'Z') return ch;
            if(ch >= 'a' && ch <= 'z') return ch;
            if(ch >= '0' && ch <= '9') return ch;
        }
        else if(validation == NativeKeyboard.CharacterValidation.NAME)
        {
            // FIXME: some actions still lead to invalid input:
            //        - Hitting delete in front of an uppercase letter
            //        - Selecting an uppercase letter and deleting it
            //        - Typing some text, hitting Home and typing more text (we then have an uppercase letter in the middle of a word)
            //        - Typing some text, hitting Home and typing a space (we then have a leading space)
            //        - Erasing a space between two words (we then have an uppercase letter in the middle of a word)
            //        - We accept a trailing space
            //        - We accept the insertion of a space between two lowercase letters.
            //        - Typing text in front of an existing uppercase letter
            //        - ... and certainly more
            //
            // The rule we try to implement are too complex for this kind of verification.

            if(Character.isLetter(ch))
            {
                // Character following a space should be in uppercase.
                if(Character.isLowerCase(ch) && ((pos == 0) || (text[pos - 1] == ' ')))
                {
                    return Character.toUpperCase(ch);
                }

                // Character not following a space or an apostrophe should be in lowercase.
                if(Character.isUpperCase(ch) && (pos > 0) && (text[pos - 1] != ' ') && (text[pos - 1] != '\''))
                {
                    return Character.toLowerCase(ch);
                }

                return ch;
            }

            if(ch == '\'')
            {
                // Don't allow more than one apostrophe
                if(!Util.contains('\'', text, textLength))
                {
                    // Don't allow consecutive spaces and apostrophes.
                    if(!(((pos > 0) && ((text[pos - 1] == ' ') || (text[pos - 1] == '\''))) ||
                            ((pos < textLength) && ((text[pos] == ' ') || (text[pos] == '\'')))))
                    {
                        return ch;
                    }
                }
            }

            if(ch == ' ')
            {
                // Don't allow consecutive spaces and apostrophes.
                if(!(((pos > 0) && ((text[pos - 1] == ' ') || (text[pos - 1] == '\''))) ||
                        ((pos < textLength) && ((text[pos] == ' ') || (text[pos] == '\'')))))
                {
                    return ch;
                }
            }
        }
        else if(validation == NativeKeyboard.CharacterValidation.EMAIL_ADDRESS)
        {
            // From StackOverflow about allowed characters in email addresses:
            // Uppercase and lowercase English letters (a-z, A-Z)
            // Digits 0 to 9
            // Characters ! # $ % & ' * + - / = ? ^ _ ` { | } ~
            // Character . (dot, period, full stop) provided that it is not the first or last character,
            // and provided also that it does not appear two or more times consecutively.

            if(Character.isLetterOrDigit(ch)) return ch;
            if(ch == '@' && Util.indexOf('@', text, textLength) == -1) return ch;
            if(EMAIL_SPECIAL_CHARACTERS.indexOf(ch) != -1) return ch;
            if(ch == '.')
            {
                char lastChar = (textLength > 0) ? text[Util.clamp(pos, 0, textLength - 1)] : ' ';
                char nextChar = (textLength > 0) ? text[Util.clamp(pos + 1, 0, textLength - 1)] : '\n';
                if(lastChar != '.' && nextChar != '.')
                {
                    return ch;
                }
            }
        }
        else if(validation == NativeKeyboard.CharacterValidation.IP_ADDRESS)
        {
            int lastDotIndex = Util.lastIndexOf('.', text, textLength);
            if(lastDotIndex == -1)
            {
                int numbersInSection = textLength;
                if(numbersInSection < 3 && ch >= '0' && ch <= '9') return ch; //Less than 3 numbers, so number add allowed
                if(ch == '.' && textLength > 0) { return ch; } //Don't start with dot
            }
            else
            {
                if(ch >= '0' && ch <= '9')
                {
                    int numbersInSection = (textLength - 1) - lastDotIndex;
                    if(numbersInSection < 3 && ch >= '0' && ch <= '9') return ch; //Less than 3 numbers, so number add allowed
                }
                if(ch == '.' && lastDotIndex != textLength - 1 && Util.countOccurences('.', text, textLength) < 3) { return ch; } //Max 4 sections (3 dot characters)
            }
        }
        else if(validation == NativeKeyboard.CharacterValidation.SENTENCE)
        {
            if(Character.isLetter(ch) && Character.isLowerCase(ch))
            {
                if(pos == 0) { return Character.toUpperCase(ch); }

                if(pos > 1 && text[pos - 1] == ' ' && text[pos - 2] == '.')
                {
                    return Character.toUpperCase(ch);
                }
            }

            return ch;
        }
        return (char)0;
    }
}