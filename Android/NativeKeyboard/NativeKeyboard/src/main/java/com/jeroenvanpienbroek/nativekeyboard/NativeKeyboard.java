//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

package com.jeroenvanpienbroek.nativekeyboard;

import android.app.Activity;
import android.app.Fragment;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.res.Configuration;
import android.graphics.Rect;
import android.os.Build;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.os.SystemClock;
import android.text.Editable;
import android.text.InputFilter;
import android.text.TextWatcher;
import android.text.method.DigitsKeyListener;
import android.util.Log;
import android.view.KeyEvent;
import android.view.View;
import android.view.Window;
import android.view.WindowManager;
import android.view.autofill.AutofillManager;
import android.view.inputmethod.EditorInfo;
import android.view.inputmethod.InputMethodManager;
import android.view.inputmethod.InputMethodSubtype;
import android.widget.LinearLayout;
import android.widget.TabHost;
import android.widget.TextView;

import androidx.autofill.HintConstants;

import com.google.android.gms.auth.api.phone.SmsRetriever;
import com.google.android.gms.auth.api.phone.SmsRetrieverClient;
import com.google.android.gms.tasks.OnFailureListener;
import com.google.android.gms.tasks.OnSuccessListener;
import com.jeroenvanpienbroek.nativekeyboard.textvalidator.CharacterValidator;
import com.jeroenvanpienbroek.nativekeyboard.textvalidator.TextValidator;
import com.unity3d.player.UnityPlayer;

import org.json.JSONObject;

import java.util.HashMap;
import java.util.Map;

import static android.text.InputType.TYPE_CLASS_NUMBER;
import static android.text.InputType.TYPE_CLASS_PHONE;
import static android.text.InputType.TYPE_CLASS_TEXT;
import static android.text.InputType.TYPE_NUMBER_FLAG_DECIMAL;
import static android.text.InputType.TYPE_NUMBER_FLAG_SIGNED;
import static android.text.InputType.TYPE_NUMBER_VARIATION_PASSWORD;
import static android.text.InputType.TYPE_TEXT_FLAG_CAP_CHARACTERS;
import static android.text.InputType.TYPE_TEXT_FLAG_CAP_SENTENCES;
import static android.text.InputType.TYPE_TEXT_FLAG_CAP_WORDS;
import static android.text.InputType.TYPE_TEXT_FLAG_MULTI_LINE;
import static android.text.InputType.TYPE_TEXT_FLAG_NO_SUGGESTIONS;
import static android.text.InputType.TYPE_TEXT_VARIATION_EMAIL_ADDRESS;
import static android.text.InputType.TYPE_TEXT_VARIATION_FILTER;
import static android.text.InputType.TYPE_TEXT_VARIATION_PASSWORD;
import static android.text.InputType.TYPE_TEXT_VARIATION_URI;
import static android.text.InputType.TYPE_TEXT_VARIATION_VISIBLE_PASSWORD;

// Unity.
// Debug.

/** Fragment class that manages the TouchScreenKeyboard */
public class NativeKeyboard extends Fragment implements TextWatcher, TextView.OnEditorActionListener, DummyView.OnSpecialKeyPressedListener, DummyView.OnSelectionChangedListener
{
    //region CONSTANTS
    public enum KeyboardState
    {
        HIDDEN,
        PENDING_SHOW,
        VISIBLE,
        PENDING_HIDE,
        PENDING_RELOAD
    }

    public enum KeyboardType
    {
        DEFAULT,
        ASCII_CAPABLE,
        DECIMAL_PAD,
        URL,
        NUMBER_PAD,
        PHONE_PAD,
        EMAIL_ADDRESS,
        NUMBERS_AND_PUNCTUATION
    }

    public enum CharacterValidation
    {
        NONE,
        INTEGER,
        DECIMAL,
        ALPHANUMERIC,
        NAME,
        EMAIL_ADDRESS,
        IP_ADDRESS,
        SENTENCE,
        CUSTOM,
        DECIMAL_FORCE_POINT,
    }

    public enum LineType
    {
        SINGLE_LINE,
        MULTI_LINE_SUBMIT,
        MULTI_LINE_NEWLINE
    }

    public enum AutocapitalizationType
    {
        NONE,
        CHARACTERS,
        WORDS,
        SENTENCES
    }

    public enum AutofillType
    {
        NONE,
        USERNAME,
        PASSWORD,
        NEW_PASSWORD,
        ONE_TIME_CODE,
        NAME,
        GIVEN_NAME,
        MIDDLE_NAME,
        FAMILY_NAME,
        LOCATION,
        FULL_STREET_ADDRESS,
        STREET_ADDRESS_LINE_1,
        STREET_ADDRESS_LINE_2,
        ADDRESS_CITY,
        ADDRESS_STATE,
        ADDRESS_CITY_AND_STATE,
        COUNTRY_NAME,
        POSTAL_CODE,
        TELEPHONE_NUMBER
    }

    public enum ReturnKeyType
    {
        DEFAULT,
        GO,
        SEND,
        SEARCH
    }

    public enum EventType
    {
        TEXT_EDIT_UPDATE,
        TEXT_CHANGE,
        SELECTION_CHANGE,
        KEYBOARD_SHOW,
        KEYBOARD_HIDE
    }

    public enum SpecialKeyCode
    {
        BACK,
        BACKSPACE,
        ESCAPE
    }

    /** Reference array to convert int to KeyboardType */
    private final KeyboardType[] keyboardTypeValues = KeyboardType.values();

    /** Reference array to convert int to CharacterValidation */
    private final CharacterValidation[] characterValidationValues = CharacterValidation.values();

    /** Reference array to convert int to LineType */
    private final LineType[] lineTypeValues = LineType.values();

    /** Reference array to convert int to AutocapitalizationType */
    private final AutocapitalizationType[] autocapitalizationTypeValues = AutocapitalizationType.values();

    /** Reference array to convert int to ReturnKeyType */
    private final ReturnKeyType[] returnKeyTypeValues = ReturnKeyType.values();

    /** Determines how often to update */
    private final int UPDATE_FREQUENCY = 100;

    /** Determines how often to check for hardware keyboard connectivity */
    private final int HARDWARE_KEYBOARD_FREQUENCY = 3000;

    private final int MIN_VISIBLE_TIME_FOR_CANCEL = 1500;

    private final int MAX_PENDING_TIME = 1500;

    /** The tag of this Fragment */
    public static final String TAG = "NativeKeyboardFragment";

    public static final int REQUEST_SMS_USER_CONSENT = 143;
    //endregion

    /** The globally accessible instance of this class */
    public static NativeKeyboard instance;

    public ThreadsafeQueue<IUnityEvent> unityEventQueue;
    public IUnityEvent currentEvent;
    public INativeKeyboardCallback unityCallback;
    public boolean updatesEnabled;
    public boolean cancelUpdateWhenDone;
    public boolean hardwareKeyboardUpdatesEnabled;
    public boolean cancelHardwareKeyboardUpdateWhenDone;

    public HashMap<AutofillType, DummyView> autofillViews;
    public int viewY;
    public DummyView defaultView;
    public DummyView currentView;
    public CharacterValidation characterValidation;
    public boolean emojisAllowed;
    public boolean hasNext;
    public KeyboardState state;
    public TextValidator textValidator;

    public Handler handler;
    public Runnable updateRunnable;
    public Runnable hardwareKeyboardUpdateRunnable;
    public InputMethodManager inputMethodManager;
    public boolean keyboardVisible;
    public boolean ignoreTextChange;
    public boolean hardwareKeyboardConnected;
    public long visibleStartTime;
    public int bottomOffset;

    private String lastText;
    private int lastSelectionStartPosition;
    private int lastSelectionEndPosition;
    public int lastKeyboardHeight;
    private SMSBroadcastReceiver smsBroadcastReceiver;
    private String longestNumberSequence;

    private long pendingStartTime;
    private boolean navigationBarWasVisible;
    private boolean initialized;

    private TextEditUpdateEvent newestTextEditUpdateEvent;
    private Object newestTextEditUpdateLock = new Object();

    //region LIFECYLCE
    /** Initializes this class with given gameObjectName
     * @param unityCallback The name of the Unity gameobject used to send event callbacks to
     * */
    public static void initialize(INativeKeyboardCallback unityCallback)
    {
        instance = new NativeKeyboard();
        instance.unityEventQueue = new ThreadsafeQueue<IUnityEvent>();
        instance.unityCallback = unityCallback;
        instance.textValidator = new TextValidator();
        instance.state = KeyboardState.HIDDEN;
        UnityPlayer.currentActivity.getFragmentManager().beginTransaction().add(instance, TAG).commit();
    }

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        if(instance == null) //You shouldn't be here if the instance is NULL
        {
            return;
        }

        setRetainInstance(true); // Retain between configuration changes (like device rotation)

        int width = 250;
        int height = 50;
        viewY = 0;

        autofillViews = new HashMap<AutofillType, DummyView>();

        defaultView = new DummyView(UnityPlayer.currentActivity);
        defaultView.setLayoutParams(new LinearLayout.LayoutParams(-width * 2, viewY));
        defaultView.addTextChangedListener(this);
        defaultView.setOnEditorActionListener(this);
        defaultView.setOnSpecialKeyPressedListener(this);
        defaultView.setOnSelectionChangedListener(this);
        defaultView.setImeOptions(EditorInfo.IME_FLAG_NO_EXTRACT_UI);
        defaultView.autofillType = AutofillType.NONE;
        if (Build.VERSION.SDK_INT >= 26)
        {
            defaultView.setImportantForAutofill(View.IMPORTANT_FOR_AUTOFILL_NO);
        }
        UnityPlayer.currentActivity.addContentView(defaultView, new LinearLayout.LayoutParams(width, height));
        viewY -= height;

        getViewForAutofill(AutofillType.USERNAME);
        getViewForAutofill(AutofillType.PASSWORD);

        currentView = defaultView;

        initNavigationBar();

        handler = new Handler();
        updateRunnable = new Runnable()
        {
            public void run()
            {
                update();

                if(cancelUpdateWhenDone && unityEventQueue.getCount() == 0 && currentEvent == null)
                {
                    cancelUpdateWhenDone = false; //Don't repeat action anymore
                    return;
                }
                else
                {
                    handler.postDelayed(this, UPDATE_FREQUENCY);
                }
            }
        };

        hardwareKeyboardUpdateRunnable = new Runnable()
        {
            public void run()
            {
                updateHardwareKeyboardConnectivity();

                if(cancelHardwareKeyboardUpdateWhenDone)
                {
                    cancelHardwareKeyboardUpdateWhenDone = false; //Don't repeat action anymore
                    return;
                }
                else
                {
                    handler.postDelayed(this, HARDWARE_KEYBOARD_FREQUENCY);
                }
            }
        };

        Activity activity = UnityPlayer.currentActivity;
        inputMethodManager = (InputMethodManager) activity.getSystemService(Context.INPUT_METHOD_SERVICE);
        initialized = true;
    }

    DummyView getViewForAutofill(AutofillType type)
    {
        DummyView autofillView = null;
        if(autofillViews.containsKey(type))
        {
            autofillView = autofillViews.get(type);
        }

        if(autofillView == null)
        {
            int width = 250;
            int height = 50;

            autofillView = new DummyView(UnityPlayer.currentActivity);
            autofillView.setLayoutParams(new LinearLayout.LayoutParams(-width * 2, viewY));
            autofillView.addTextChangedListener(this);
            autofillView.setOnEditorActionListener(this);
            autofillView.setOnSpecialKeyPressedListener(this);
            autofillView.setOnSelectionChangedListener(this);
            autofillView.setImeOptions(EditorInfo.IME_FLAG_NO_EXTRACT_UI);
            if (Build.VERSION.SDK_INT >= 26)
            {
                autofillView.setImportantForAutofill(View.IMPORTANT_FOR_AUTOFILL_YES);
                autofillView.autofillType = type;
                switch (type)
                {
                    case USERNAME: autofillView.setAutofillHints(HintConstants.AUTOFILL_HINT_USERNAME);
                        break;
                    case PASSWORD: autofillView.setAutofillHints(HintConstants.AUTOFILL_HINT_PASSWORD);
                        break;
                    case NEW_PASSWORD: autofillView.setAutofillHints(HintConstants.AUTOFILL_HINT_NEW_PASSWORD);
                        break;
                    case ONE_TIME_CODE: autofillView.setAutofillHints(HintConstants.AUTOFILL_HINT_SMS_OTP);
                        break;
                    case NAME: autofillView.setAutofillHints(HintConstants.AUTOFILL_HINT_PERSON_NAME);
                        break;
                    case GIVEN_NAME: autofillView.setAutofillHints(HintConstants.AUTOFILL_HINT_PERSON_NAME_GIVEN);
                        break;
                    case MIDDLE_NAME: autofillView.setAutofillHints(HintConstants.AUTOFILL_HINT_PERSON_NAME_MIDDLE);
                        break;
                    case FAMILY_NAME: autofillView.setAutofillHints(HintConstants.AUTOFILL_HINT_PERSON_NAME_FAMILY);
                        break;
                    case LOCATION: autofillView.setAutofillHints(HintConstants.AUTOFILL_HINT_POSTAL_ADDRESS_LOCALITY);
                        break;
                    case FULL_STREET_ADDRESS: autofillView.setAutofillHints(HintConstants.AUTOFILL_HINT_POSTAL_ADDRESS_STREET_ADDRESS);
                        break;
                    case STREET_ADDRESS_LINE_1: autofillView.setAutofillHints(HintConstants.AUTOFILL_HINT_POSTAL_ADDRESS_STREET_ADDRESS);
                        break;
                    case STREET_ADDRESS_LINE_2: autofillView.setAutofillHints(HintConstants.AUTOFILL_HINT_POSTAL_ADDRESS_EXTENDED_ADDRESS);
                        break;
                    case ADDRESS_CITY: autofillView.setAutofillHints(HintConstants.AUTOFILL_HINT_POSTAL_ADDRESS_LOCALITY);
                        break;
                    case ADDRESS_STATE: autofillView.setAutofillHints(HintConstants.AUTOFILL_HINT_POSTAL_ADDRESS_REGION);
                        break;
                    case ADDRESS_CITY_AND_STATE: autofillView.setAutofillHints(HintConstants.AUTOFILL_HINT_POSTAL_ADDRESS_LOCALITY);
                        break;
                    case COUNTRY_NAME: autofillView.setAutofillHints(HintConstants.AUTOFILL_HINT_POSTAL_ADDRESS_COUNTRY);
                        break;
                    case POSTAL_CODE: autofillView.setAutofillHints(HintConstants.AUTOFILL_HINT_POSTAL_CODE);
                        break;
                    case TELEPHONE_NUMBER: autofillView.setAutofillHints(HintConstants.AUTOFILL_HINT_PHONE_NUMBER);
                        break;
                    default:
                        Log.d(TAG, "Unknown type: " + type);
                        break;
                }
            }
            UnityPlayer.currentActivity.addContentView(autofillView, new LinearLayout.LayoutParams(width, height));
            viewY -= height;

            autofillViews.put(type, autofillView);
        }

        return  autofillView;
    }

    @Override
    public void onActivityCreated (Bundle savedInstanceState)
    {
        super.onActivityCreated(savedInstanceState);
        if(instance == null) //You shouldn't be here if the instance is NULL
        {
            getFragmentManager().beginTransaction().remove(this).commit();
        }
    }

    @Override
    public void onPause()
    {
        if(inputMethodManager != null && currentView != null)
        {
            if(currentView.autofillType == AutofillType.NONE)
            {
                inputMethodManager.hideSoftInputFromWindow(currentView.getWindowToken(), 0);
                state = KeyboardState.HIDDEN;
                unityCallback.OnKeyboardCancel();
                hideNavigationBar();
            }
        }

        super.onPause();
    }

    @Override
    public void onDestroy()
    {
        if(inputMethodManager != null && currentView != null)
        {
            inputMethodManager.hideSoftInputFromWindow(currentView.getWindowToken(), 0);
            state = KeyboardState.HIDDEN;
            unityCallback.OnKeyboardCancel();
            hideNavigationBar();
        }

        super.onDestroy();
    }

    @Override
    public void onActivityResult(int requestCode, int resultCode, Intent data)
    {
        if(requestCode == REQUEST_SMS_USER_CONSENT)
        {
            if (resultCode == Activity.RESULT_OK && data != null)
            {
                String message = data.getStringExtra(SmsRetriever.EXTRA_SMS_MESSAGE);
                if(message != null)
                {
                    parseSMSMessage(message);
                }
            }
        }

        super.onActivityResult(requestCode, resultCode, data);
    }
    //endregion

    private void setNewestTextEditUpdateEvent(TextEditUpdateEvent textEditUpdateEvent)
    {
        synchronized (newestTextEditUpdateLock)
        {
            newestTextEditUpdateEvent = textEditUpdateEvent;
        }
    }

    private TextEditUpdateEvent getNewestTextEditUpdateEvent()
    {
        synchronized (newestTextEditUpdateLock)
        {
            return newestTextEditUpdateEvent;
        }
    }

    private void startSmsUserConsent()
    {
        SmsRetrieverClient client = SmsRetriever.getClient(getActivity());
        if(client != null)
        {
            //We can add user phone number or leave it blank
            client.startSmsUserConsent(null).addOnSuccessListener(new OnSuccessListener<Void>()
            {
                @Override
                public void onSuccess(Void aVoid)
                {
                    //Log.d(TAG, "LISTENING_SUCCESS");
                }
            });
            client.startSmsUserConsent(null).addOnFailureListener(new OnFailureListener()
            {
                @Override
                public void onFailure(Exception exception)
                {
                    //Log.d(TAG, "LISTENING_FAILURE");
                }
            });
        }
    }

    private void registerToSmsBroadcastReceiver()
    {
        smsBroadcastReceiver = new SMSBroadcastReceiver();
        smsBroadcastReceiver.smsBroadcastReceiverListener = new SMSBroadcastReceiverListener() {
            @Override
            public void onSuccess(Intent intent)
            {
                if(intent != null)
                {
                    startActivityForResult(intent, REQUEST_SMS_USER_CONSENT);
                }
            }

            @Override
            public void onFailure()
            {
                //Log.d(TAG, "Register failed");
            }
        };

        IntentFilter intentFilter = new IntentFilter(SmsRetriever.SMS_RETRIEVED_ACTION);
        getActivity().registerReceiver(smsBroadcastReceiver, intentFilter);
    }

    public void parseSMSMessage(String message)
    {
        longestNumberSequence = "";
        String currentNumberSequence = "";

        int length = message.length();
        for(int i = 0; i < length; i++)
        {
            char c = message.charAt(i);
            if(Character.isDigit(c))
            {
                currentNumberSequence += c;
                if(currentNumberSequence.length() > longestNumberSequence.length())
                {
                    longestNumberSequence = currentNumberSequence;
                }
            }
            else
            {
                currentNumberSequence = "";
            }
        }

        if(longestNumberSequence.length() > 0)
        {
            DummyView oneTimeCodeView = getViewForAutofill(AutofillType.ONE_TIME_CODE);
            oneTimeCodeView.setText(longestNumberSequence);
            unityCallback.OnAutofillUpdate(longestNumberSequence, AutofillType.ONE_TIME_CODE.ordinal());
        }
    }

    //For some reason this makes sure the navigation bar stays visible when shown
    private void initNavigationBar()
    {
        View decorView = getActivity().getWindow().getDecorView();
        decorView.setOnSystemUiVisibilityChangeListener(new View.OnSystemUiVisibilityChangeListener()
        {
            @Override
            public void onSystemUiVisibilityChange(int i)
            {
                //Log.d("NativeKeyboard", "VisibilityChanged: " + i);
            }
        });
    }

    public boolean isStatusBarVisible()
    {
        Rect rectangle = new Rect();
        Window window = getActivity().getWindow();
        window.getDecorView().getWindowVisibleDisplayFrame(rectangle);
        int statusBarHeight = rectangle.top;
        return statusBarHeight != 0;
    }

    private void showNavigationBar()
    {
        Window window = getActivity().getWindow();
        boolean fullScreen = (window.getAttributes().flags & WindowManager.LayoutParams.FLAG_FULLSCREEN) != 0;
        boolean forceNotFullScreen = (window.getAttributes().flags & WindowManager.LayoutParams.FLAG_FORCE_NOT_FULLSCREEN) != 0;

        if(fullScreen && !forceNotFullScreen && !navigationBarWasVisible)
        {
            View decorView = window.getDecorView();
            int flags = View.SYSTEM_UI_FLAG_LAYOUT_STABLE
                    | View.SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION
                    | View.SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN;
            decorView.setSystemUiVisibility(flags);
        }
    }

    private boolean isLandscape()
    {
        return (getOrientation() == Configuration.ORIENTATION_LANDSCAPE);
    }

    private int getOrientation()
    {
        Activity activity = getActivity();
        if(activity == null){activity = UnityPlayer.currentActivity;}

        try
        {
            return activity.getResources().getConfiguration().orientation;
        }
        catch (Exception e){ e.printStackTrace(); }

        return  -1;
    }

    private void hideNavigationBar()
    {
        Window window = getActivity().getWindow();
        boolean fullScreen = (window.getAttributes().flags & WindowManager.LayoutParams.FLAG_FULLSCREEN) != 0;
        boolean forceNotFullScreen = (window.getAttributes().flags & WindowManager.LayoutParams.FLAG_FORCE_NOT_FULLSCREEN) != 0;

        if(fullScreen && !forceNotFullScreen && !navigationBarWasVisible)
        {
            View decorView = window.getDecorView();
            int flags = View.SYSTEM_UI_FLAG_LAYOUT_STABLE
                    | View.SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION
                    | View.SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN
                    | View.SYSTEM_UI_FLAG_HIDE_NAVIGATION
                    | View.SYSTEM_UI_FLAG_FULLSCREEN
                    | View.SYSTEM_UI_FLAG_IMMERSIVE;
            decorView.setSystemUiVisibility(flags);
        }
    }

    //region PROCESS
    private void update()
    {
        if(state == KeyboardState.PENDING_RELOAD)
        {
            updateKeyboardVisibility();

            if(!keyboardVisible)
            {
                state = KeyboardState.PENDING_SHOW;
                currentView.requestFocus();
                inputMethodManager.showSoftInput(currentView, InputMethodManager.SHOW_FORCED);
            }
            return;
        }
        else if(state == KeyboardState.PENDING_SHOW)
        {
            updateKeyboardVisibility();

            if(!keyboardVisible && SystemClock.elapsedRealtime() - pendingStartTime <= MAX_PENDING_TIME)
            {
                currentView.requestFocus();
                inputMethodManager.showSoftInput(currentView, InputMethodManager.SHOW_FORCED);
                return;
            }

            state = KeyboardState.VISIBLE;
            visibleStartTime = SystemClock.elapsedRealtime();
            unityCallback.OnKeyboardShow();
            if(isLandscape())
            {
                hideNavigationBar();
            }
            else
            {
                showNavigationBar();
            }
        }
        else if(state == KeyboardState.PENDING_HIDE)
        {
            updateKeyboardVisibility();

            if(keyboardVisible && SystemClock.elapsedRealtime() - pendingStartTime <= MAX_PENDING_TIME)
            {
                inputMethodManager.hideSoftInputFromWindow(currentView.getWindowToken(), 0);
                return;
            }

            currentView.clearFocus();
            state = KeyboardState.HIDDEN;
            unityCallback.OnKeyboardHide();
            hideNavigationBar();
        }

        if(state == KeyboardState.VISIBLE)
        {
            if(!inputMethodManager.isActive())
            {
                currentView.requestFocus();
                inputMethodManager.showSoftInput(currentView, InputMethodManager.SHOW_IMPLICIT);
            }
        }

        currentEvent = null;
        IUnityEvent unityEvent = popEvent();
        while(unityEvent != null)
        {
            currentEvent = unityEvent;
            switch(currentEvent.getType())
            {
                case TEXT_EDIT_UPDATE:
                    processTextEditUpdateEvent((TextEditUpdateEvent)currentEvent);
                    currentEvent = null; //Clear it immediately to avoid race condition with a native text edit event
                    break;
                case KEYBOARD_SHOW: processKeyboardShowEvent((KeyboardShowEvent) currentEvent); break;
                case KEYBOARD_HIDE: processKeyboardHideEvent((KeyboardHideEvent) currentEvent); break;
            }

            unityEvent = popEvent();
        }

        updateKeyboardHeight();
    }

    private IUnityEvent popEvent()
    {
        if(unityEventQueue.getCount() == 0)
        {
            return null;
        }

        return unityEventQueue.dequeue();
    }

    private void processTextEditUpdateEvent(TextEditUpdateEvent textEditUpdateEvent)
    {
        if(getNewestTextEditUpdateEvent() != textEditUpdateEvent)
        {
            return;
        }

        String text = textEditUpdateEvent.text;
        int selectionStartPosition = textEditUpdateEvent.selectionStartPosition;
        int selectionEndPosition = textEditUpdateEvent.selectionEndPosition;
        applyTextEditUpdate(text, selectionStartPosition, selectionEndPosition, false);
    }

    private void applyTextEditUpdate(String text, int selectionStartPosition, int selectionEndPosition, boolean forced)
    {
        try
        {
            if(!text.equals(lastText) || forced)
            {
                currentView.setText(text);
                lastText = text;

                currentView.setSelection(selectionStartPosition, selectionEndPosition); //Always update selection after text change
                lastSelectionStartPosition = selectionStartPosition;
                lastSelectionEndPosition = selectionEndPosition;
            }
            else if(selectionStartPosition != lastSelectionStartPosition || selectionEndPosition != lastSelectionEndPosition)
            {
                currentView.setSelection(selectionStartPosition, selectionEndPosition);
                lastSelectionStartPosition = selectionStartPosition;
                lastSelectionEndPosition = selectionEndPosition;
            }
        }
        catch(Exception e) { e.printStackTrace(); }
    }

    private void processKeyboardShowEvent(KeyboardShowEvent keyboardShowEvent)
    {
        if(state == KeyboardState.HIDDEN)
        {
            bottomOffset = getKeyboardHeight(); //Get the offset caused by the navigation bar if any
            Window window = getActivity().getWindow();
            View decorView = window.getDecorView();
            navigationBarWasVisible = (decorView.getSystemUiVisibility() & View.SYSTEM_UI_FLAG_HIDE_NAVIGATION) == 0;

            if(isStatusBarVisible())
            {
                window.setSoftInputMode(WindowManager.LayoutParams.SOFT_INPUT_ADJUST_RESIZE);
            }
        }

        NativeKeyboardConfiguration configuration = keyboardShowEvent.configuration;
        characterValidation = configuration.characterValidation;
        emojisAllowed = configuration.emojisAllowed;
        LineType lineType = configuration.lineType;
        CharacterValidator characterValidator = configuration.characterValidator;
        textValidator.setValidation(characterValidation);
        textValidator.setLineType(lineType);
        textValidator.setValidator(characterValidator);
        determineCurrentView(configuration.autofillType);
        configureInputFilters(configuration.characterLimit);
        configureLineType(configuration.lineType);
        configureKeyboardType(keyboardShowEvent);

        String text = keyboardShowEvent.text;
        int selectionStartPosition = keyboardShowEvent.selectionStartPosition;
        int selectionEndPosition = keyboardShowEvent.selectionEndPosition;
        applyTextEditUpdate(text, selectionStartPosition, selectionEndPosition, true);

        state = KeyboardState.PENDING_SHOW;
        pendingStartTime = SystemClock.elapsedRealtime();
        visibleStartTime = SystemClock.elapsedRealtime();
        currentView.requestFocus();
        inputMethodManager.showSoftInput(currentView, InputMethodManager.SHOW_FORCED);
    }

    private void determineCurrentView(AutofillType autofillType)
    {
        if (Build.VERSION.SDK_INT >= 26)
        {
            DummyView nextView = null;
            if(autofillType == AutofillType.NONE)
            {
                nextView = defaultView;
            }
            else
            {
                nextView = getViewForAutofill(autofillType);
            }

            if(currentView != nextView)
            {
                state = KeyboardState.PENDING_RELOAD;
                inputMethodManager.hideSoftInputFromWindow(currentView.getWindowToken(), 0);
                currentView = nextView;
            }
        }
    }

    private void configureInputFilters(int characterLimit)
    {
        if (characterLimit > 0)
        {
            if(emojisAllowed)
            {
                currentView.setFilters(new InputFilter[]{});
            }
            else
            {
                currentView.setFilters(new InputFilter[]{new EmojiExcludeFilter(), new InputFilter.LengthFilter(characterLimit)});
            }
        }
        else
        {
            if(emojisAllowed)
            {
                currentView.setFilters(new InputFilter[]{});
            }
            else
            {
                currentView.setFilters(new InputFilter[]{new EmojiExcludeFilter()});
            }
        }
    }

    private void configureLineType(LineType lineType)
    {
        switch (lineType)
        {
            case SINGLE_LINE:
                currentView.setSingleLine(true);
                currentView.setMaxLines(1);
                break;
            case MULTI_LINE_SUBMIT:
                currentView.setSingleLine(true);
                currentView.setMaxLines(1);
                break;
            case MULTI_LINE_NEWLINE:
                currentView.setSingleLine(false);
                currentView.setMaxLines(Integer.MAX_VALUE);
                break;
        }
    }

    private void configureKeyboardType(KeyboardShowEvent keyboardShowEvent)
    {
        NativeKeyboardConfiguration configuration = keyboardShowEvent.configuration;
        LineType lineType = configuration.lineType;
        KeyboardType keyboardType = configuration.keyboardType;
        AutocapitalizationType autocapitalizationType = configuration.autocapitalizationType;
        ReturnKeyType returnKeyType = configuration.returnKeyType;
        boolean autocorrection = configuration.autocorrection;
        boolean secure = configuration.secure;
        hasNext = configuration.hasNext;
        int inputType = 0;

        if(lineType == LineType.MULTI_LINE_NEWLINE)
        {
            currentView.setImeOptions(EditorInfo.IME_ACTION_UNSPECIFIED | EditorInfo.IME_FLAG_NO_EXTRACT_UI);

            inputType = TYPE_CLASS_TEXT | TYPE_TEXT_FLAG_MULTI_LINE;
            if (!autocorrection || secure)
            {
                inputType += TYPE_TEXT_FLAG_NO_SUGGESTIONS;
                inputType += TYPE_TEXT_VARIATION_FILTER;
            }

            if(autocapitalizationType != AutocapitalizationType.NONE)
            {
                switch (autocapitalizationType)
                {
                    case CHARACTERS:
                        inputType += TYPE_TEXT_FLAG_CAP_CHARACTERS;
                        break;
                    case WORDS:
                        inputType += TYPE_TEXT_FLAG_CAP_WORDS;
                        break;
                    case SENTENCES:
                        inputType += TYPE_TEXT_FLAG_CAP_SENTENCES;
                        break;
                }
            }

            currentView.setInputType(inputType);
            return;
        }

        if(returnKeyType == ReturnKeyType.DEFAULT)
        {
            if (hasNext)
            {
                currentView.setImeOptions(EditorInfo.IME_ACTION_NEXT | EditorInfo.IME_FLAG_NO_EXTRACT_UI);
            }
            else
            {
                currentView.setImeOptions(EditorInfo.IME_ACTION_DONE | EditorInfo.IME_FLAG_NO_EXTRACT_UI);
            }
        }
        else
        {
            switch (returnKeyType)
            {
                case GO:
                    currentView.setImeOptions(EditorInfo.IME_ACTION_GO | EditorInfo.IME_FLAG_NO_EXTRACT_UI);
                    break;
                case SEND:
                    currentView.setImeOptions(EditorInfo.IME_ACTION_SEND | EditorInfo.IME_FLAG_NO_EXTRACT_UI);
                    break;
                case SEARCH:
                    currentView.setImeOptions(EditorInfo.IME_ACTION_SEARCH | EditorInfo.IME_FLAG_NO_EXTRACT_UI);
                    break;
            }
        }

        switch (configuration.keyboardType)
        {
            case DEFAULT:
                inputType = TYPE_CLASS_TEXT;
                break;
            case ASCII_CAPABLE:
                inputType = TYPE_CLASS_TEXT;
                break;
            case NUMBERS_AND_PUNCTUATION:
                inputType = TYPE_CLASS_NUMBER | TYPE_NUMBER_FLAG_DECIMAL | TYPE_NUMBER_FLAG_SIGNED;
                break;
            case URL:
                inputType = TYPE_CLASS_TEXT | TYPE_TEXT_VARIATION_URI;
                break;
            case NUMBER_PAD:
                inputType = TYPE_CLASS_NUMBER | TYPE_NUMBER_FLAG_SIGNED;
                break;
            case PHONE_PAD:
                inputType = TYPE_CLASS_PHONE;
                break;
            case EMAIL_ADDRESS:
                inputType = TYPE_CLASS_TEXT | TYPE_TEXT_VARIATION_EMAIL_ADDRESS;
                break;
        }

        if (!autocorrection || secure)
        {
            inputType += TYPE_TEXT_FLAG_NO_SUGGESTIONS;
            inputType += TYPE_TEXT_VARIATION_FILTER;
        }

        if (secure)
        {
            if (keyboardType == KeyboardType.NUMBERS_AND_PUNCTUATION || keyboardType == KeyboardType.DECIMAL_PAD || keyboardType == KeyboardType.NUMBER_PAD)
            {
                inputType = TYPE_CLASS_NUMBER | TYPE_NUMBER_VARIATION_PASSWORD;
            }
            else
            {
                inputType = TYPE_CLASS_TEXT | TYPE_TEXT_VARIATION_PASSWORD;
            }
        }

        if(autocapitalizationType != AutocapitalizationType.NONE)
        {
            switch (autocapitalizationType)
            {
                case CHARACTERS:
                    inputType += TYPE_TEXT_FLAG_CAP_CHARACTERS;
                    break;
                case WORDS:
                    inputType += TYPE_TEXT_FLAG_CAP_WORDS;
                    break;
                case SENTENCES:
                    inputType += TYPE_TEXT_FLAG_CAP_SENTENCES;
                    break;
            }
        }

        currentView.setInputType(inputType);

        if(characterValidation == CharacterValidation.DECIMAL || characterValidation == CharacterValidation.DECIMAL_FORCE_POINT)
        {
            currentView.setKeyListener(DigitsKeyListener.getInstance("0123456789.,-"));
        }
    }

    private void processKeyboardHideEvent(KeyboardHideEvent keyboardHideEvent)
    {
        state = KeyboardState.PENDING_HIDE;
        pendingStartTime = SystemClock.elapsedRealtime();
        inputMethodManager.hideSoftInputFromWindow(currentView.getWindowToken(), 0);
        hideNavigationBar();
        if(!updatesEnabled)
        {
            lastKeyboardHeight = 0; //Immediately report zero if we don't have updates enabled any more
            unityCallback.OnKeyboardHeightChanged(lastKeyboardHeight);
        }
    }

    private void updateKeyboardHeight()
    {
        int keyboardHeight = getKeyboardHeight();
        if(keyboardHeight != lastKeyboardHeight)
        {
            if(keyboardHeight < lastKeyboardHeight && state == KeyboardState.VISIBLE)
            {
                long timeElapsed = SystemClock.elapsedRealtime() - visibleStartTime;
                InputMethodSubtype subType = inputMethodManager.getCurrentInputMethodSubtype();
                boolean subTypeActive = (subType != null && subType.getMode() != null && subType.getMode().length() > 0);
                if(timeElapsed >= MIN_VISIBLE_TIME_FOR_CANCEL && !subTypeActive)
                {
                    state = KeyboardState.HIDDEN;
                    unityCallback.OnKeyboardCancel();
                    hideNavigationBar();
                }
            }

            if(keyboardHeight == bottomOffset && (state == KeyboardState.PENDING_SHOW || state == KeyboardState.VISIBLE))
            {
                return;
            }
            else if(keyboardHeight > bottomOffset && (state == KeyboardState.PENDING_HIDE || state == KeyboardState.HIDDEN))
            {
                return;
            }

            unityCallback.OnKeyboardHeightChanged(keyboardHeight - bottomOffset);
        }

        lastKeyboardHeight = keyboardHeight;
    }

    public void updateKeyboardVisibility()
    {
        int keyboardHeight = getKeyboardHeight();
        if(keyboardHeight > bottomOffset && state == KeyboardState.PENDING_SHOW)
        {
            keyboardVisible = true;
        }
        else if(keyboardHeight == bottomOffset && state == KeyboardState.PENDING_HIDE)
        {
            keyboardVisible = false;
        }
    }

    /** Gets current keyboard height */
    private int getKeyboardHeight()
    {
        View view = UnityPlayer.currentActivity.getWindow().getDecorView();
        Rect rect = new Rect();
        view.getWindowVisibleDisplayFrame(rect);

        return Math.round(view.getHeight() - rect.height());
    }

    private void updateHardwareKeyboardConnectivity()
    {
        boolean connected = isHardwareKeyboardConnected();

        if(hardwareKeyboardConnected != connected)
        {
            hardwareKeyboardConnected = connected;
            unityCallback.OnHardwareKeyboardChanged(hardwareKeyboardConnected);
        }
    }

    public boolean isHardwareKeyboardConnected()
    {
        try
        {
            Activity activity = getActivity();
            if(activity == null){ return false; }

            int keyboardConfiguration = activity.getResources().getConfiguration().keyboard;
            return (keyboardConfiguration != Configuration.KEYBOARD_NOKEYS);
        }
        catch (Exception e)
        {
            return  false;
        }
    }
    //endregion

    //region INPUTFIELD_EVENTS
    @Override
    public void beforeTextChanged(CharSequence s, int start, int count, int after)
    {
    }

    @Override
    public void onTextChanged(CharSequence charSequence, int start, int before, int count)
    {
    }

    @Override
    public void afterTextChanged(Editable editable)
    {
        if(editable.hashCode() != currentView.getText().hashCode())
        {
            AutofillType autofillType = getAutofillTypeForHashCode(editable.hashCode());
            if(autofillType != AutofillType.NONE)
            {
                unityCallback.OnAutofillUpdate(editable.toString(), autofillType.ordinal());
            }
            return;
        }

        if(currentEvent != null || ignoreTextChange)
        {
            return;
        }

        if(characterValidation != CharacterValidation.NONE)
        {
            String lastText = currentView.getText().toString();
            String text = "";

            int caretPosition = currentView.getSelectionStart();
            int selectionStartPosition = -1;
            if(currentView.getSelectionEnd() - currentView.getSelectionStart() > 0)
            {
                selectionStartPosition = currentView.getSelectionStart();
            }

            textValidator.validate(text, lastText, caretPosition, selectionStartPosition);
            text = textValidator.getResultText();

            if(!lastText.equals(text))
            {
                int lastSelectionStart = currentView.getSelectionStart();

                ignoreTextChange = true;
                currentView.setText(text);
                ignoreTextChange = false;

                int amountChanged = text.length() - lastText.length();
                try
                {
                    caretPosition = lastSelectionStart + amountChanged;
                    currentView.setSelection(caretPosition, caretPosition);
                }
                catch(Exception e) { e.printStackTrace(); };
            }
        }

        String text = currentView.getText().toString();
        int selectionStartPosition = currentView.getSelectionStart();
        int selectionEndPosition = currentView.getSelectionEnd();
        if(selectionStartPosition > selectionEndPosition) //Check if they are swapped
        {
            selectionStartPosition = currentView.getSelectionEnd();
            selectionEndPosition = currentView.getSelectionStart();
        }
        unityCallback.OnTextEditUpdate(text, selectionStartPosition, selectionEndPosition);

        lastText = text;
        lastSelectionStartPosition = selectionStartPosition;
        lastSelectionEndPosition = selectionEndPosition;
    }

    public AutofillType getAutofillTypeForHashCode(int hashCode)
    {
        for(Map.Entry<AutofillType, DummyView> entry: autofillViews.entrySet())
        {
            DummyView autofillView = entry.getValue();
            if(autofillView.getText().hashCode() == hashCode)
            {
                return autofillView.autofillType;
            }
        }

        return AutofillType.NONE;
    }

    @Override
    public boolean onEditorAction(TextView v, int actionId, KeyEvent event)
    {
        if (actionId == EditorInfo.IME_ACTION_DONE || actionId == EditorInfo.IME_ACTION_NEXT || actionId == EditorInfo.IME_ACTION_GO || actionId == EditorInfo.IME_ACTION_SEND || actionId == EditorInfo.IME_ACTION_SEARCH)
        {
            if (hasNext)
            {
                unityCallback.OnKeyboardNext();
                return true;
            }
            else
            {
                unityCallback.OnKeyboardDone();
                return true;
            }
        }
        return false;
    }

    @Override
    public void onSpecialKeyPressed(SpecialKeyCode specialKeyCode)
    {
        unityCallback.OnSpecialKeyPressed(specialKeyCode.ordinal());

        if(specialKeyCode == SpecialKeyCode.BACK)
        {
            state = KeyboardState.HIDDEN;
            unityCallback.OnKeyboardCancel();
            hideNavigationBar();
        }
    }

    @Override
    public void onSelectionChanged(int selectionStart, int selectionEnd)
    {
        if(currentEvent != null)
        {
            return;
        }

        if(selectionStart != lastSelectionStartPosition || selectionEnd != lastSelectionEndPosition)
        {
            String text = currentView.getText().toString();
            int selectionStartPosition = currentView.getSelectionStart();
            int selectionEndPosition = currentView.getSelectionEnd();
            if(selectionStartPosition > selectionEndPosition) //Check if they are swapped
            {
                selectionStartPosition = currentView.getSelectionEnd();
                selectionEndPosition = currentView.getSelectionStart();
            }
            unityCallback.OnTextEditUpdate(text, selectionStartPosition, selectionEndPosition);

            lastSelectionStartPosition = selectionStartPosition;
            lastSelectionEndPosition = selectionEndPosition;
        }
    }
    //endregion

    //region PUBLIC_METHODS
    public static void enableUpdates()
    {
        ensureInitialization();
        ensureHandler();

        if(!instance.updatesEnabled)
        {
            instance.updatesEnabled = true;

            if(instance.cancelUpdateWhenDone) //Thread action haven't been cancelled yet
            {
                instance.cancelUpdateWhenDone = false;
            }
            else
            {
                instance.handler.post(instance.updateRunnable);
            }
        }
    }

    private static void ensureInitialization()
    {
        if(instance.initialized){ return; } //OK

        try
        {
            for(int i = 0; i < 30; i++) //Try wait (max 30 times)
            {
                Log.d("NativeKeyboard", "NativeKeyboard not initialized yet, waiting...");
                Thread.sleep(100);
                if(instance.initialized){ return; } //OK
            }
        }
        catch (Exception e) { e.printStackTrace(); }
    }

    private static void ensureHandler()
    {
        if(instance.handler != null){ return; } //OK

        try
        {
            for(int i = 0; i < 10; i++) //Try wait (max 10 times)
            {
                Log.d("NativeKeyboard", "Thread handler not initialized yet, waiting...");
                Thread.sleep(100);
                if(instance.handler != null){ return; } //OK
            }
        }
        catch (Exception e) { e.printStackTrace(); }

        if(instance.handler == null) //Still not ready, create it now
        {
            try { instance.handler = new Handler(); }
            catch (Exception e) { e.printStackTrace(); }
        }
    }

    public static void disableUpdates()
    {
        ensureInitialization();
        ensureHandler();

        if(instance.updatesEnabled)
        {
            instance.updatesEnabled = false;
            instance.cancelUpdateWhenDone = true;
        }
    }

    public static void enableHardwareKeyboardUpdates()
    {
        ensureInitialization();
        ensureHandler();

        if(!instance.hardwareKeyboardUpdatesEnabled)
        {
            instance.hardwareKeyboardUpdatesEnabled = true;

            if(instance.cancelHardwareKeyboardUpdateWhenDone) //Thread action haven't been cancelled yet
            {
                instance.cancelHardwareKeyboardUpdateWhenDone = false;
            }
            else
            {
                instance.handler.post(instance.hardwareKeyboardUpdateRunnable);
            }
        }
    }

    public static void disableHardwareKeyboardUpdates()
    {
        ensureInitialization();
        ensureHandler();

        if(instance.hardwareKeyboardUpdatesEnabled)
        {
            instance.hardwareKeyboardUpdatesEnabled = false;
            instance.cancelHardwareKeyboardUpdateWhenDone = true;
        }
    }

    public static void updateTextEdit(String text, int selectionStartPosition, int selectionEndPosition)
    {
        TextEditUpdateEvent textEditUpdateEvent = new TextEditUpdateEvent(text, selectionStartPosition, selectionEndPosition);
        instance.unityEventQueue.enqueue(textEditUpdateEvent);
        instance.setNewestTextEditUpdateEvent(textEditUpdateEvent);
    }

    public static void showKeyboard(String text, int selectionStartPosition, int selectionEndPosition, String configurationJSON)
    {
        NativeKeyboardConfiguration configuration = null;
        if(configurationJSON != null && configurationJSON.length() > 0)
        {
            try
            {
                JSONObject jsonObject = new JSONObject(configurationJSON);
                configuration = new NativeKeyboardConfiguration(jsonObject);
            }
            catch (Exception e)
            {
                e.printStackTrace();
            }
        }
        KeyboardShowEvent keyboardShowEvent = new KeyboardShowEvent(text, selectionStartPosition, selectionEndPosition, configuration);

        instance.unityEventQueue.enqueue(keyboardShowEvent);
    }

    public static void restoreKeyboard()
    {
        try
        {
            Handler mainHandler = new Handler(Looper.getMainLooper());
            Runnable runnable = new Runnable()
            {
                @Override
                public void run()
                {
                    instance.state = KeyboardState.PENDING_SHOW;
                    instance.pendingStartTime = SystemClock.elapsedRealtime();
                    instance.visibleStartTime = SystemClock.elapsedRealtime();
                    instance.currentView.requestFocus();
                    instance.inputMethodManager.showSoftInput(instance.currentView, InputMethodManager.SHOW_FORCED);
                }
            };
            mainHandler.post(runnable);
        }
        catch(Exception e) {}
    }

    public static void hideKeyboard()
    {
        KeyboardHideEvent keyboardHideEvent = new KeyboardHideEvent();
        instance.unityEventQueue.enqueue(keyboardHideEvent);
    }

    public static void resetAutofill()
    {
        try
        {
            Handler mainHandler = new Handler(Looper.getMainLooper());
            Runnable runnable = new Runnable()
            {
                @Override
                public void run()
                {
                    if (Build.VERSION.SDK_INT >= 26)
                    {
                        Activity activity = instance.getActivity();
                        if(activity == null){ return; }

                        AutofillManager afm = activity.getSystemService(AutofillManager.class);
                        if (afm != null)
                        {
                            afm.cancel();
                        }
                    }
                }
            };
            mainHandler.post(runnable);
        }
        catch(Exception e) {}
    }

    public static void startListeningForOneTimeCodes()
    {
        try
        {
            Handler mainHandler = new Handler(Looper.getMainLooper());
            Runnable runnable = new Runnable()
            {
                @Override
                public void run()
                {
                    if (Build.VERSION.SDK_INT >= 26)
                    {
                        try
                        {
                            if(instance.smsBroadcastReceiver == null)
                            {
                                instance.registerToSmsBroadcastReceiver();
                            }
                            instance.startSmsUserConsent();
                        }
                        catch (Exception e)
                        {
                            Log.d(TAG, "Failed to start listening for one time codes: " + e.getMessage());
                        }
                    }
                }
            };
            mainHandler.post(runnable);
        }
        catch(Exception e) {}
    }

    public static void saveCredentials()
    {
        try
        {
            Handler mainHandler = new Handler(Looper.getMainLooper());
            Runnable runnable = new Runnable()
            {
                @Override
                public void run()
                {
                    if (Build.VERSION.SDK_INT >= 26)
                    {
                        AutofillManager autofillManager = instance.getActivity().getSystemService(AutofillManager.class);
                        autofillManager.commit();
                    }
                }
            };
            mainHandler.post(runnable);
        }
        catch (Exception e) { }
    }
    //endregion
}