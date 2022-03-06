//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

package com.jeroenvanpienbroek.nativekeyboard;

public class KeyboardHideEvent implements IUnityEvent
{
    public NativeKeyboard.EventType getType() { return NativeKeyboard.EventType.KEYBOARD_HIDE; }
}