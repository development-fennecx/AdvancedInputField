//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

#import "NKBNativeKeyboard.h"
#import "NKBUtil.h"
#import "NKBNSString+Extensions.h"
#import "NKBUnityCallback.h"

extern "C"
{
    void _nativeKeyboard_initialize(NKBOnTextEditUpdate onTextEditUpdate_,
                                    NKBOnAutofillUpdate onAutofillUpdate_,
                                    NKBOnKeyboardShow onKeyboardShow_,
                                    NKBOnKeyboardHide onKeyboardHide_,
                                    NKBOnKeyboardDone onKeyboardDone_,
                                    NKBOnKeyboardNext onKeyboardNext_,
                                    NKBOnKeyboardCancel onKeyboardCancel_,
                                    NKBOnSpecialKeyPressed onSpecialKeyPressed_,
                                    NKBOnKeyboardHeightChanged onKeyboardHeightChanged_,
                                    NKBOnHardwareKeyboardChanged onHardwareKeyboardChanged_)
    {
        [NKBNativeKeyboard initialize:onTextEditUpdate_ onAutofillUpdate:onAutofillUpdate_ onKeyboardShow:onKeyboardShow_ onKeyboardHide:onKeyboardHide_ onKeyboardDone:onKeyboardDone_ onKeyboardNext:onKeyboardNext_ onKeyboardCancel:onKeyboardCancel_ onKeyboardDelete: onSpecialKeyPressed_ onKeyboardHeightChanged:onKeyboardHeightChanged_ onHardwareKeyboardChanged:onHardwareKeyboardChanged_];
    }
    
    void _nativeKeyboard_enableUpdates()
    {
        NKBNativeKeyboard* instance = [NKBNativeKeyboard getInstance];
        [instance enableUpdates];
    }
    
    void _nativeKeyboard_disableUpdates()
    {
        NKBNativeKeyboard* instance = [NKBNativeKeyboard getInstance];
        [instance disableUpdates];
    }
    
    void _nativeKeyboard_enableHardwareKeyboardUpdates()
    {
        NKBNativeKeyboard* instance = [NKBNativeKeyboard getInstance];
        [instance enableHardwareKeyboardUpdates];
    }
    
    void _nativeKeyboard_disableHardwareKeyboardUpdates()
    {
        NKBNativeKeyboard* instance = [NKBNativeKeyboard getInstance];
        [instance disableHardwareKeyboardUpdates];
    }
    
    void _nativeKeyboard_updateTextEdit(const char* text_, const int start_, const int end_)
    {
        NKBNativeKeyboard* instance = [NKBNativeKeyboard getInstance];
        NSString* nativeText = [NSString fromUnityString:text_];
        [instance updateTextEdit:nativeText selectionStartPosition:start_ selectionEndPosition:end_];
    }

    void _nativeKeyboard_requestTextEditUpdate()
    {
        NKBNativeKeyboard* instance = [NKBNativeKeyboard getInstance];
        [instance requestTextEditUpdate];
    }
    
    void _nativeKeyboard_showKeyboard(const char* text_, const int selectionStartPosition_, const int selectionEndPosition_, const char* configurationJSON_)
    {
        NKBNativeKeyboard* instance = [NKBNativeKeyboard getInstance];
        NSString* nativeText = [NSString fromUnityString:text_];
        NSString* nativeConfigurationJSON = [NSString fromUnityString:configurationJSON_];
        [instance showKeyboard: nativeText selectionStartPosition:selectionStartPosition_ selectionEndPosition:selectionEndPosition_ configurationJSON:nativeConfigurationJSON];
    }

    void _nativeKeyboard_restoreKeyboard()
    {
        NKBNativeKeyboard* instance = [NKBNativeKeyboard getInstance];
        [instance restoreKeyboard];
    }
    
    void _nativeKeyboard_hideKeyboard()
    {
        NKBNativeKeyboard* instance = [NKBNativeKeyboard getInstance];
        [instance hideKeyboard];
    }

    void _nativeKeyboard_saveCredentials(const char* domainName_)
    {
        NKBNativeKeyboard* instance = [NKBNativeKeyboard getInstance];
        NSString* nativeDomainName = [NSString fromUnityString:domainName_];
        [instance saveCredentials: nativeDomainName];
    }
}
