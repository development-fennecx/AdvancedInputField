package com.jeroenvanpienbroek.nativekeyboard;

public class TextEditUpdateEvent implements IUnityEvent
{
    public NativeKeyboard.EventType getType() { return NativeKeyboard.EventType.TEXT_EDIT_UPDATE; }

    public String text;
    public int selectionStartPosition;
    public int selectionEndPosition;

    public TextEditUpdateEvent(String text, int selectionStartPosition, int selectionEndPosition)
    {
        this.text = text;
        this.selectionStartPosition = selectionStartPosition;
        this.selectionEndPosition = selectionEndPosition;
    }
}
