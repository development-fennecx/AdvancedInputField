//----------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

#ifndef NKBNativeKeyboard_h
#define NKBNativeKeyboard_h

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "NKBIUnityEvent.h"
#import "NKBThreadsafeQueue.h"
#import "NKBUnityCallback.h"
#import "TextValidator/NKBTextValidator.h"
#import "NKBSingleLineView.h"
#import "NKBMultilineView.h"
#import "NKBCharacterValidator.h"
#import "NKBNativeKeyboardConfiguration.h"

@class NKBTextEditUpdateEvent;
@class NKBKeyboardShowEvent;
@class NKBKeyboardHideEvent;
@class NKBTextValidator;

typedef NS_ENUM(int, NKBKeyboardState)
{
    KS_HIDDEN = 0,
    KS_PENDING_SHOW = 1,
    KS_VISIBLE = 2,
    KS_PENDING_HIDE = 3,
    KS_PENDING_RELOAD = 4
};

typedef NS_ENUM(int, NKBKeyboardType)
{
    KT_DEFAULT = 0,
    KT_ASCII_CAPABLE = 1,
    KT_DECIMAL_PAD = 2,
    KT_URL = 3,
    KT_NUMBER_PAD = 4,
    KT_PHONE_PAD = 5,
    KT_EMAIL_ADDRESS = 6,
    KT_NUMBERS_AND_PUNCTUATION = 7,
};

typedef NS_ENUM(int, NKBCharacterValidation)
{
    CV_NONE = 0,
    CV_INTEGER = 1,
    CV_DECIMAL = 2,
    CV_ALPHANUMERIC = 3,
    CV_NAME = 4,
    CV_EMAIL_ADDRESS = 5,
    CV_IP_ADDRESS = 6,
    CV_SENTENCE = 7,
    CV_CUSTOM = 8,
    CV_DECIMAL_FORCE_POINT = 9
};

typedef NS_ENUM(int, NKBLineType)
{
    LT_SINGLE_LINE = 0,
    LT_MULTI_LINE_SUBMIT = 1,
    LT_MULTI_LINE_NEWLINE = 2
};

typedef NS_ENUM(int, NKBAutocapitalizationType)
{
    AT_NO_AUTOCAPITALIZATION = 0,
    AT_CHARACTERS = 1,
    AT_WORDS = 2,
    AT_SENTENCES = 3
};

typedef NS_ENUM(int, NKBReturnKeyType)
{
    RT_DEFAULT = 0,
    RT_GO = 1,
    RT_SEND = 2,
    RT_SEARCH = 3
};

typedef NS_ENUM(NSInteger, NKBAutofillType)
{
    AFT_NONE = 0,
    AFT_USERNAME = 1,
    AFT_PASSWORD = 2,
    AFT_NEW_PASSWORD = 3,
    AFT_ONE_TIME_CODE = 4,
    AFT_NAME = 5,
    AFT_GIVEN_NAME = 6,
    AFT_MIDDLE_NAME = 7,
    AFT_FAMILY_NAME = 8,
    AFT_LOCATION = 9,
    AFT_FULL_STREET_ADDRESS = 10,
    AFT_STREET_ADDRESS_LINE_1 = 11,
    AFT_STREET_ADDRESS_LINE_2 = 12,
    AFT_ADDRESS_CITY = 13,
    AFT_ADDRESS_STATE = 14,
    AFT_ADDRESS_CITY_AND_STATE = 15,
    AFT_COUNTRY_NAME = 16,
    AFT_POSTAL_CODE = 17,
    AFT_TELEPHONE_NUMBER = 18
};

typedef NS_ENUM(int, NKBEventType)
{
    ET_TEXT_EDIT_UPDATE = 0,
    ET_KEYBOARD_SHOW = 1,
    ET_KEYBOARD_HIDE = 2
};

typedef NS_ENUM(int, NKBSpecialKeyCode)
{
    SKC_BACK = 0,
    SKC_BACKSPACE = 1,
    SKC_ESCAPE = 2
};

extern void UnitySendMessage(const char *, const char *, const char *);

@interface NKBNativeKeyboard : NSObject<UITextFieldDelegate, UITextViewDelegate, NKBMultilineViewDelegate, NKBSingleLineViewDelegate>

@property (nonatomic, strong) NKBThreadsafeQueue<NKBIUnityEvent>* unityEventQueue;
@property (nonatomic, strong) id<NKBIUnityEvent> currentEvent;
@property (nonatomic, strong) NKBUnityCallback* unityCallback;
@property (nonatomic, assign) BOOL updatesEnabled;
@property (nonatomic, assign) BOOL cancelUpdateWhenDone;
@property (nonatomic, assign) BOOL hardwareKeyboardUpdatesEnabled;
@property (nonatomic, assign) BOOL cancelHardwareKeyboardUpdateWhenDone;

@property (nonatomic, strong) UIWindow* window;
@property (nonatomic, strong) NSMutableDictionary* autofillViews;
@property (nonatomic, assign) int viewY;
@property (nonatomic, strong) NKBSingleLineView* singleLineView;
@property (nonatomic, strong) NKBMultilineView* multilineView;
@property (nonatomic, strong) id<NKBInputView> currentView;
@property (nonatomic, strong) NSArray<UIBarButtonItemGroup*>* originalLeadingBarButtonGroups;
@property (nonatomic, strong) NSArray<UIBarButtonItemGroup*>* originalTrailingBarButtonGroups;
@property (nonatomic, assign) NKBCharacterValidation characterValidation;
@property (nonatomic, assign) BOOL emojisAllowed;
@property (nonatomic, assign) BOOL multiline;
@property (nonatomic, assign) NKBKeyboardState state;
@property (nonatomic, assign) NSUInteger lastKeyboardHeight;
@property (nonatomic, assign) BOOL keyboardVisible;
@property (nonatomic, assign) BOOL hardwareKeyboardConnected;
@property (nonatomic, strong) NKBTextValidator* textValidator;

@property (nonatomic, weak) UIViewController *presentingViewController;
@property (nonatomic, strong) id updateHandler;
@property (nonatomic, strong) id hardwareKeyboardUpdateHandler;
@property (nonatomic, assign) NSUInteger characterLimit;
@property (nonatomic, assign) NKBLineType lineType;
@property (nonatomic, assign) NSUInteger currentKeyboardHeight;
@property (nonatomic, assign) NSTimeInterval visibleStartTime;
@property (nonatomic, assign) NSTimeInterval pendingStartTime;

@property (nonatomic, strong) NSString* lastText;
@property (nonatomic, assign) int lastSelectionStartPosition;
@property (nonatomic, assign) int lastSelectionEndPosition;

@property (nonatomic, strong) NKBTextEditUpdateEvent* newestTextEditUpdateEventSafe;
@property (nonatomic, strong) NSObject* newestTextEditUpdateLock;

+(void)initialize:(NKBOnTextEditUpdate)onTextEditUpdate_
            onAutofillUpdate:(NKBOnAutofillUpdate)onAutofillUpdate_
               onKeyboardShow:(NKBOnKeyboardShow)onKeyboardShow_
               onKeyboardHide:(NKBOnKeyboardHide)onKeyboardHide_
               onKeyboardDone:(NKBOnKeyboardDone)onKeyboardDone_
               onKeyboardNext:(NKBOnKeyboardNext)onKeyboardNext_
             onKeyboardCancel:(NKBOnKeyboardCancel)onKeyboardCancel_
            onKeyboardDelete:(NKBOnSpecialKeyPressed)onSpecialKeyPressed_
      onKeyboardHeightChanged:(NKBOnKeyboardHeightChanged)onKeyboardHeightChanged_
    onHardwareKeyboardChanged:(NKBOnHardwareKeyboardChanged)onHardwareKeyboardChanged_;

+(NKBNativeKeyboard*)getInstance;

-(void)initWithOnTextEditUpdate:(NKBOnTextEditUpdate)onTextEditUpdate_
               onAutofillUpdate:(NKBOnAutofillUpdate)onAutofillUpdate_
               onKeyboardShow:(NKBOnKeyboardShow)onKeyboardShow_
               onKeyboardHide:(NKBOnKeyboardHide)onKeyboardHide_
               onKeyboardDone:(NKBOnKeyboardDone)onKeyboardDone_
               onKeyboardNext:(NKBOnKeyboardNext)onKeyboardNext_
             onKeyboardCancel:(NKBOnKeyboardCancel)onKeyboardCancel_
               onKeyboardDelete:(NKBOnSpecialKeyPressed)onSpecialKeyPressed_
      onKeyboardHeightChanged:(NKBOnKeyboardHeightChanged)onKeyboardHeightChanged_
    onHardwareKeyboardChanged:(NKBOnHardwareKeyboardChanged)onHardwareKeyboardChanged_;

-(void)update;

-(id<NKBIUnityEvent>) popEvent;

-(void)processTextEditUpdateEvent:(NKBTextEditUpdateEvent*) textEditUpdateEvent_;

-(void)processKeyboardShowEvent:(NKBKeyboardShowEvent*) keyboardShowEvent_;

-(void)configureKeyboardType:(NKBKeyboardShowEvent*) keyboardShowEvent_;

-(void)processKeyboardHideEvent:(NKBKeyboardHideEvent*) keyboardHideEvent_;

-(void)updateKeyboardHeight;

-(void)updateKeyboardVisibility;

-(void)updateHardwareKeyboardConnectivity;

-(BOOL)isHardwareKeyboardConnected;

-(void)enableUpdates;

-(void)disableUpdates;

-(void)enableHardwareKeyboardUpdates;

-(void)disableHardwareKeyboardUpdates;

-(void)updateTextEdit:(NSString*)text_
selectionStartPosition:(int)selectionStartPosition_
 selectionEndPosition:(int)selectionEndPosition_;

-(void)requestTextEditUpdate;

-(void)showKeyboard:(NSString*)text_
selectionStartPosition:(int)selectionStartPosition_
selectionEndPosition:(int)selectionEndPosition_
configurationJSON:(NSString*)configurationJSON_;

-(void)restoreKeyboard;

-(void)hideKeyboard;

-(void)saveCredentials:(NSString*)domainName_;

@end

#endif
