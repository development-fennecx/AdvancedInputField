//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

package com.jeroenvanpienbroek.nativekeyboard;

import com.jeroenvanpienbroek.nativekeyboard.textvalidator.CharacterValidator;

public class KeyboardShowEvent implements IUnityEvent
{
    public NativeKeyboard.EventType getType() { return NativeKeyboard.EventType.KEYBOARD_SHOW; }

    public String text;
    public int selectionStartPosition;
    public int selectionEndPosition;
    public NativeKeyboardConfiguration configuration;

    public KeyboardShowEvent(String text, int selectionStartPosition, int selectionEndPosition, NativeKeyboardConfiguration configuration)
    {
        this.text = text;
        this.selectionStartPosition = selectionStartPosition;
        this.selectionEndPosition = selectionEndPosition;
        this.configuration = configuration;
    }
}