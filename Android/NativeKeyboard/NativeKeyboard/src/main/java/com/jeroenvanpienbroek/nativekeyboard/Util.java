//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

package com.jeroenvanpienbroek.nativekeyboard;

public class Util
{
    public static int clamp(int value, int min, int max)
    {
        value = Math.max(value, min);
        value = Math.min(value, max);

        return  value;
    }

    public static float clamp(float value, float min, float max)
    {
        value = Math.max(value, min);
        value = Math.min(value, max);

        return  value;
    }

    public static boolean contains(char ch, char[] text, int textLength)
    {
        for(int i = 0; i < textLength; i++)
        {
            if(text[i] == ch) { return true; }
        }

        return false;
    }

    public static int indexOf(char ch, char[] text, int textLength)
    {
        for(int i = 0; i < textLength; i++)
        {
            if(text[i] == ch) { return i; }
        }

        return -1;
    }

    public static int lastIndexOf(char ch, char[] text, int textLength)
    {
        for(int i = textLength - 1; i >= 0; i--)
        {
            if(text[i] == ch) { return i; }
        }

        return -1;
    }

    public static int countOccurences(char ch, char[] text, int textLength)
    {
        int occurences = 0;

        for(int i = 0; i < textLength; i++)
        {
            if(text[i] == ch)
            {
                occurences++;
            }
        }

        return occurences;
    }

    public static void stringCopy(char[] destination, String source)
    {
        int length = source.length();
        for(int i = 0; i < length; i++)
        {
            destination[i] = source.charAt(i);
        }
    }
}