//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

package com.jeroenvanpienbroek.nativekeyboard;

import android.content.Context;
import android.graphics.Canvas;
import android.util.Log;
import android.view.KeyEvent;
import android.view.MotionEvent;
import android.view.inputmethod.EditorInfo;
import android.view.inputmethod.InputConnection;
import android.view.inputmethod.InputConnectionWrapper;
import android.widget.EditText;

/** Class the acts a Dummy for text interaction and manipulation */
public class DummyView extends EditText
{
    public static final String TAG = "DummyView";
    public NativeKeyboard.AutofillType autofillType;

    private class DummyViewInputConnection extends InputConnectionWrapper
    {
        public DummyViewInputConnection(InputConnection target, boolean mutable)
        {
            super(target, mutable);
        }

        @Override
        public boolean sendKeyEvent(KeyEvent event)
        {
            if (event.getAction() == KeyEvent.ACTION_DOWN && event.getKeyCode() == KeyEvent.KEYCODE_DEL)
            {
                if(onSpecialKeyPressedListener != null)
                {
                    onSpecialKeyPressedListener.onSpecialKeyPressed(NativeKeyboard.SpecialKeyCode.BACKSPACE);
                }
            }
            return super.sendKeyEvent(event);
        }

        @Override
        public boolean deleteSurroundingText(int beforeLength, int afterLength)
        {
            // magic: in latest Android, deleteSurroundingText(1, 0) will be called for backspace
            if (beforeLength == 1 && afterLength == 0)
            {
                if(onSpecialKeyPressedListener != null)
                {
                    onSpecialKeyPressedListener.onSpecialKeyPressed(NativeKeyboard.SpecialKeyCode.BACKSPACE);
                }
            }
            return super.deleteSurroundingText(beforeLength, afterLength);
        }
    }

    public interface OnSpecialKeyPressedListener
    {
        public void onSpecialKeyPressed(NativeKeyboard.SpecialKeyCode specialKeyCode);
    }

    public interface OnSelectionChangedListener
    {
        public void onSelectionChanged(int selectionStart, int selectionEnd);
    }

    private OnSpecialKeyPressedListener onSpecialKeyPressedListener;
    private OnSelectionChangedListener onSelectionChangedListener;

    public DummyView(Context context)
    {
        super(context);
    }

    public void setOnSpecialKeyPressedListener(OnSpecialKeyPressedListener listener)
    {
        onSpecialKeyPressedListener = listener;
    }

    public void setOnSelectionChangedListener(OnSelectionChangedListener listener)
    {
        onSelectionChangedListener = listener;
    }

    @Override
    public InputConnection onCreateInputConnection(EditorInfo outAttrs)
    {
        return new DummyViewInputConnection(super.onCreateInputConnection(outAttrs), true);
    }

    @Override
    public boolean onTouchEvent (MotionEvent event)
    {
        return false; //Ignore touch events
    }

    @Override
    public boolean onKeyPreIme(int keyCode, KeyEvent event)
    {
        if (event.getAction() == KeyEvent.ACTION_UP && event.getKeyCode() == KeyEvent.KEYCODE_BACK)
        {
            if(onSpecialKeyPressedListener != null)
            {
                onSpecialKeyPressedListener.onSpecialKeyPressed(NativeKeyboard.SpecialKeyCode.BACK);
            }
        }
        return super.onKeyPreIme(keyCode, event);
    }

    @Override
    protected void onSelectionChanged(int selStart, int selEnd)
    {
        if(onSelectionChangedListener != null)
        {
            onSelectionChangedListener.onSelectionChanged(selStart, selEnd);
        }

        super.onSelectionChanged(selStart, selEnd);
    }

    @Override
    protected void onDraw(Canvas canvas)
    {
        //Don't draw anything
        //super.onDraw(canvas);
    }

    @SuppressWarnings("all")
    @Override
    public void draw(Canvas canvas)
    {
        //Don't draw anything
        //super.draw(canvas);
    }
}
