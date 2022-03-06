//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

#import "NKBNativeKeyboard.h"
#import <string.h>
#import <ctype.h>
#import <math.h>
#import <objc/message.h>
#import "NKBUtil.h"
#import "NKBIUnityEvent.h"
#import "NKBKeyboardHideEvent.h"
#import "NKBKeyboardShowEvent.h"
#import "NKBTextEditUpdateEvent.h"
#import "NKBNSString+Extensions.h"
#import "NKBCharacterValidator.h"

const float UPDATE_FREQUENCY = 0.1f;
const float HARDWARE_KEYBOARD_FREQUENCY = 3.0f;
const float MIN_VISIBLE_TIME_FOR_CANCEL = 0.5f;
const float MAX_PENDING_TIME = 1.5f;

__strong NKBNativeKeyboard* _nativeKeyboard_instance = nil;

@implementation NKBNativeKeyboard

//region LIFECYCLE
+(void)initialize:(NKBOnTextEditUpdate)onTextEditUpdate_
            onAutofillUpdate:(NKBOnAutofillUpdate)onAutofillUpdate_
               onKeyboardShow:(NKBOnKeyboardShow)onKeyboardShow_
               onKeyboardHide:(NKBOnKeyboardHide)onKeyboardHide_
               onKeyboardDone:(NKBOnKeyboardDone)onKeyboardDone_
               onKeyboardNext:(NKBOnKeyboardNext)onKeyboardNext_
             onKeyboardCancel:(NKBOnKeyboardCancel)onKeyboardCancel_
 onKeyboardDelete:(NKBOnSpecialKeyPressed)onKeyboardDelete_
      onKeyboardHeightChanged:(NKBOnKeyboardHeightChanged)onKeyboardHeightChanged_
    onHardwareKeyboardChanged:(NKBOnHardwareKeyboardChanged)onHardwareKeyboardChanged_
{
    _nativeKeyboard_instance = [self new];
    [_nativeKeyboard_instance initWithOnTextEditUpdate:onTextEditUpdate_ onAutofillUpdate:onAutofillUpdate_ onKeyboardShow:onKeyboardShow_ onKeyboardHide:onKeyboardHide_ onKeyboardDone:onKeyboardDone_ onKeyboardNext:onKeyboardNext_ onKeyboardCancel:onKeyboardCancel_ onKeyboardDelete:onKeyboardDelete_ onKeyboardHeightChanged:onKeyboardHeightChanged_ onHardwareKeyboardChanged:onHardwareKeyboardChanged_];
}

+(NKBNativeKeyboard*)getInstance
{
    return _nativeKeyboard_instance;
}

-(void)initWithOnTextEditUpdate:(NKBOnTextEditUpdate)onTextEditUpdate_
               onAutofillUpdate:(NKBOnAutofillUpdate)onAutofillUpdate_
                 onKeyboardShow:(NKBOnKeyboardShow)onKeyboardShow_
                 onKeyboardHide:(NKBOnKeyboardHide)onKeyboardHide_
                 onKeyboardDone:(NKBOnKeyboardDone)onKeyboardDone_
                 onKeyboardNext:(NKBOnKeyboardNext)onKeyboardNext_
               onKeyboardCancel:(NKBOnKeyboardCancel)onKeyboardCancel_
               onKeyboardDelete:(NKBOnSpecialKeyPressed)onKeyboardDelete_
        onKeyboardHeightChanged:(NKBOnKeyboardHeightChanged)onKeyboardHeightChanged_
      onHardwareKeyboardChanged:(NKBOnHardwareKeyboardChanged)onHardwareKeyboardChanged_
{
    _unityEventQueue = [[NKBThreadsafeQueue<NKBIUnityEvent> alloc]init];
    _unityCallback = [[NKBUnityCallback alloc]initWithOnTextEditUpdate:onTextEditUpdate_ onAutofillUpdate:onAutofillUpdate_ onKeyboardShow:onKeyboardShow_ onKeyboardHide:onKeyboardHide_ onKeyboardDone:onKeyboardDone_ onKeyboardNext:onKeyboardNext_ onKeyboardCancel:onKeyboardCancel_ onKeyboardDelete:onKeyboardDelete_ onKeyboardHeightChanged:onKeyboardHeightChanged_ onHardwareKeyboardChanged:onHardwareKeyboardChanged_];
    _textValidator = [[NKBTextValidator alloc]init];
    _state = KS_HIDDEN;
    
    int width = 250;
    int height = 50;
    _viewY = 0;
    
    _window = [UIApplication sharedApplication].keyWindow;
    
    _singleLineView = [[NKBSingleLineView alloc] initWithFrame: CGRectMake ( -width * 2, _viewY, width, height)];
    _singleLineView.delegate = self;
    _singleLineView.myDelegate = self;
    [_singleLineView addTarget:self
              action:@selector(textFieldDidChange:)
    forControlEvents:UIControlEventEditingChanged];
    [_window addSubview:_singleLineView];
    _viewY -= height;
    
    _multilineView = [[NKBMultilineView alloc] initWithFrame: CGRectMake ( -width * 2, _viewY, width, height)];
    _multilineView.delegate = self;
    _multilineView.myDelegate = self;
    [_window addSubview:_multilineView];
    _viewY -= height;
    
    _currentView = _singleLineView;
    
    _autofillViews = [[NSMutableDictionary alloc]init];
    [self getViewForAutofill:AFT_USERNAME];
    [self getViewForAutofill:AFT_PASSWORD];
    
    UITextInputAssistantItem* shortcut = [_singleLineView inputAssistantItem];
    _originalLeadingBarButtonGroups = shortcut.leadingBarButtonGroups;
    _originalTrailingBarButtonGroups = shortcut.trailingBarButtonGroups;
    
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onKeyboardDidShow:) name:UIKeyboardDidShowNotification object:nil];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onKeyboardDidHide:) name:UIKeyboardDidHideNotification object:nil];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onKeyboardDidChangeFrame:) name:UIKeyboardDidChangeFrameNotification object:nil];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onKeyboardWillHide:) name:UIKeyboardWillHideNotification object:nil];
    
    _updateHandler = ^
    {
        [_nativeKeyboard_instance update];
        
        if(_nativeKeyboard_instance.cancelUpdateWhenDone && [_nativeKeyboard_instance.unityEventQueue getCount] == 0 && _nativeKeyboard_instance.currentEvent == nil)
        {
            _nativeKeyboard_instance.cancelUpdateWhenDone = false; //Don't repeat action anymore
            return;
        }
        else
        {
            [NKBUtil runOnMainThread:_nativeKeyboard_instance.updateHandler delay:UPDATE_FREQUENCY];
        }
    };
    
    _hardwareKeyboardUpdateHandler = ^
    {
        [_nativeKeyboard_instance updateHardwareKeyboardConnectivity];
        
        if(_nativeKeyboard_instance.cancelHardwareKeyboardUpdateWhenDone)
        {
            _nativeKeyboard_instance.cancelHardwareKeyboardUpdateWhenDone = false; //Don't repeat action anymore
            return;
        }
        else
        {
            [NKBUtil runOnMainThread:_nativeKeyboard_instance.updateHandler delay:HARDWARE_KEYBOARD_FREQUENCY];
        }
    };
    
    [_nativeKeyboard_instance updateHardwareKeyboardConnectivity];
}
//endregion LIFCYCLE

-(NKBSingleLineView*) getViewForAutofill:(NKBAutofillType) type_
{
    NKBSingleLineView* autofillView = [_autofillViews objectForKey:@(type_)];
    if(autofillView == nil)
    {
        int width = 250;
        int height = 50;
        
        bool secure = (type_ == AFT_PASSWORD || type_ == AFT_NEW_PASSWORD);
        if(secure) //Workaround for issue that previous TextView will also have third party keyboards blocked
        {
            NKBSingleLineView* extraView = [[NKBSingleLineView alloc] initWithFrame: CGRectMake ( -width * 2, _viewY, width, height)];
            extraView.delegate = self;
            extraView.myDelegate = self;
            extraView.autofillType = type_;
            [extraView addTarget:self
                      action:@selector(textFieldDidChange:)
            forControlEvents:UIControlEventEditingChanged];
            [_window addSubview:extraView];
            _viewY -= height;
        }

        autofillView = [[NKBSingleLineView alloc] initWithFrame: CGRectMake ( -width * 2, _viewY, width, height)];
        autofillView.delegate = self;
        autofillView.myDelegate = self;
        autofillView.autofillType = type_;
        [autofillView addTarget:self
                  action:@selector(textFieldDidChange:)
        forControlEvents:UIControlEventEditingChanged];
        if (@available(iOS 12, *))
        {
            switch (type_)
            {
                case AFT_USERNAME: autofillView.textContentType = UITextContentTypeUsername;
                    break;
                case AFT_PASSWORD: autofillView.textContentType = UITextContentTypePassword;
                    break;
                case AFT_NEW_PASSWORD: autofillView.textContentType = UITextContentTypeNewPassword;
                    break;
                case AFT_ONE_TIME_CODE: autofillView.textContentType = UITextContentTypeOneTimeCode;
                    break;
                case AFT_NAME: autofillView.textContentType = UITextContentTypeName;
                    break;
                case AFT_GIVEN_NAME: autofillView.textContentType = UITextContentTypeGivenName;
                    break;
                case AFT_MIDDLE_NAME: autofillView.textContentType = UITextContentTypeMiddleName;
                    break;
                case AFT_FAMILY_NAME: autofillView.textContentType = UITextContentTypeFamilyName;
                    break;
                case AFT_LOCATION: autofillView.textContentType = UITextContentTypeLocation;
                    break;
                case AFT_FULL_STREET_ADDRESS: autofillView.textContentType = UITextContentTypeFullStreetAddress;
                    break;
                case AFT_STREET_ADDRESS_LINE_1: autofillView.textContentType = UITextContentTypeStreetAddressLine1;
                    break;
                case AFT_STREET_ADDRESS_LINE_2: autofillView.textContentType = UITextContentTypeStreetAddressLine2;
                    break;
                case AFT_ADDRESS_CITY: autofillView.textContentType = UITextContentTypeAddressCity;
                    break;
                case AFT_ADDRESS_STATE: autofillView.textContentType = UITextContentTypeAddressState;
                    break;
                case AFT_ADDRESS_CITY_AND_STATE: autofillView.textContentType = UITextContentTypeAddressCityAndState;
                    break;
                case AFT_COUNTRY_NAME: autofillView.textContentType = UITextContentTypeCountryName;
                    break;
                case AFT_POSTAL_CODE: autofillView.textContentType = UITextContentTypePostalCode;
                    break;
                case AFT_TELEPHONE_NUMBER: autofillView.textContentType = UITextContentTypeTelephoneNumber;
                    break;
                default:
                    NSLog(@"Unknown type: %ld", (long)type_);
                    break;
            }
        }
        autofillView.secureTextEntry = secure;
        
        [_window addSubview:autofillView];
        _viewY -= height;
        
        _autofillViews[@(type_)] = autofillView;
    }
    
    return autofillView;
}

-(NKBAutofillType) getAutofillTypeForHashCode:(NSUInteger) hashCode_
{
    for (id key in _autofillViews)
    {
        id<NKBInputView> autofillView = _autofillViews[key];
        if([autofillView getHashCode] == hashCode_)
        {
            return [autofillView getAutofillType];
        }
    }
    
    return AFT_NONE;
}

-(void)setNewestTextEditUpdateEventSafe:(NKBTextEditUpdateEvent*) textEditUpdateEvent_
{
    @synchronized (_newestTextEditUpdateLock)
    {
        _newestTextEditUpdateEventSafe = textEditUpdateEvent_;
    }
}

-(NKBTextEditUpdateEvent*)getNewestTextEditUpdateEventSafe
{
    @synchronized (_newestTextEditUpdateLock)
    {
        return _newestTextEditUpdateEventSafe;
    }}

//region PROCESS
-(void)update
{
    if(_state == KS_PENDING_RELOAD)
    {
        [self updateKeyboardVisibility];
        
        if(!_keyboardVisible)
        {
            _state = KS_PENDING_SHOW;
            [_currentView requestFocus];
            [_currentView refreshInputViews];
        }
        return;
    }
    else if(_state == KS_PENDING_SHOW)
    {
        [self updateKeyboardVisibility];
        
        if(!_keyboardVisible && ([[NSProcessInfo processInfo] systemUptime] - _pendingStartTime) <= MAX_PENDING_TIME)
        {
            [_currentView requestFocus];
            return;
        }
        
        _state = KS_VISIBLE;
        [_unityCallback onKeyboardShow];
    }
    else if(_state == KS_PENDING_HIDE)
    {
        [self updateKeyboardVisibility];
        
        if(_keyboardVisible && ([[NSProcessInfo processInfo] systemUptime] - _pendingStartTime) <= MAX_PENDING_TIME)
        {
            [_currentView loseFocus];
            return;
        }
        
        _visibleStartTime = [[NSProcessInfo processInfo] systemUptime];
        _state = KS_HIDDEN;
        [_unityCallback onKeyboardHide];
    }
    
    _currentEvent = nil;
    id<NKBIUnityEvent> unityEvent = [self popEvent];
    while(unityEvent != nil)
    {
        _currentEvent = unityEvent;
        switch ([_currentEvent getType])
        {
            case ET_TEXT_EDIT_UPDATE: [self processTextEditUpdateEvent:(NKBTextEditUpdateEvent*)unityEvent];
                _currentEvent = nil; //Clear it immediately to avoid race condition with a native text edit event
                break;
            case ET_KEYBOARD_SHOW: [self processKeyboardShowEvent:(NKBKeyboardShowEvent*)unityEvent]; break;
            case ET_KEYBOARD_HIDE: [self processKeyboardHideEvent:(NKBKeyboardHideEvent*)unityEvent]; break;
        }
        
        unityEvent = [self popEvent];
    }
    
    [self updateKeyboardHeight];
}

-(id<NKBIUnityEvent>) popEvent
{
    if([_unityEventQueue getCount] == 0)
    {
        return nil;
    }
    
    return [_unityEventQueue dequeue];
}

-(void)processTextEditUpdateEvent:(NKBTextEditUpdateEvent*) textEditUpdateEvent_
{
    if([self getNewestTextEditUpdateEventSafe] != textEditUpdateEvent_)
    {
        return;
    }
    
    NSString* text = textEditUpdateEvent_.text;
    int selectionStartPosition = textEditUpdateEvent_.selectionStartPosition;
    int selectionEndPosition = textEditUpdateEvent_.selectionEndPosition;
    [self applyTextEditUpdate:text selectionStartPosition:selectionStartPosition selectionEndPosition:selectionEndPosition forced:false];
}

-(void)applyTextEditUpdate:(NSString*) text_
    selectionStartPosition:(int) selectionStartPosition_
      selectionEndPosition:(int) selectionEndPosition_
                    forced:(bool) forced_
{
    @try
    {
        if(![text_ isEqualToString:_lastText] || forced_)
        {
            [_currentView changeText:text_];
            _lastText = text_;

            [_currentView changeSelection:selectionStartPosition_ end:selectionEndPosition_]; //Always update selection after text change
            _lastSelectionStartPosition = selectionStartPosition_;
            _lastSelectionEndPosition = selectionEndPosition_;
        }
        else if(selectionStartPosition_ != _lastSelectionStartPosition || selectionEndPosition_ != _lastSelectionEndPosition)
        {
            [_currentView changeSelection:selectionStartPosition_ end:selectionEndPosition_];
            _lastSelectionStartPosition = selectionStartPosition_;
            _lastSelectionEndPosition = selectionEndPosition_;
        }
    }
    @catch (NSException *exception){}
}

-(void)processKeyboardShowEvent:(NKBKeyboardShowEvent*) keyboardShowEvent_
{
    NKBNativeKeyboardConfiguration* configuration = keyboardShowEvent_.configuration;
    _characterValidation = configuration.characterValidation;
    _emojisAllowed = configuration.emojisAllowed;
    NKBLineType lineType = configuration.lineType;
    NKBCharacterValidator* characterValidator = configuration.characterValidator;
    _characterLimit = configuration.characterLimit;
    [_textValidator setValidation:_characterValidation];
    [_textValidator setLineType:lineType];
    [_textValidator setValidator:characterValidator];
    
    _lineType = configuration.lineType;
    [self determineCurrentView:configuration.autofillType];
    [self configureKeyboardType:keyboardShowEvent_];
    
    NSString* text = keyboardShowEvent_.text;
    int selectionStartPosition = keyboardShowEvent_.selectionStartPosition;
    int selectionEndPosition = keyboardShowEvent_.selectionEndPosition;
    [self applyTextEditUpdate:text selectionStartPosition:selectionStartPosition selectionEndPosition:selectionEndPosition forced:true];
    
    _state = KS_PENDING_SHOW;
    _pendingStartTime = [[NSProcessInfo processInfo] systemUptime];
    _visibleStartTime = [[NSProcessInfo processInfo] systemUptime];
    if(_state != KS_PENDING_RELOAD)
    {
        [_currentView requestFocus];
        [_currentView refreshInputViews];
    }
}

-(void)determineCurrentView:(NKBAutofillType) autofillType_
{
    _multiline = (_lineType != LT_SINGLE_LINE);
    
    id<NKBInputView> nextView = nil;
    if(autofillType_ == AFT_NONE)
    {
        if(_multiline)
        {
            nextView = _multilineView;
        }
        else
        {
            nextView = _singleLineView;
        }
    }
    else
    {
        nextView = [self getViewForAutofill:autofillType_];
    }
        
    if(_currentView != nextView)
    {
        _state = KS_PENDING_RELOAD;
        [_currentView loseFocus];
        _currentView = nextView;
    }
}

-(void)configureKeyboardType:(NKBKeyboardShowEvent*) keyboardShowEvent_
{
    NKBNativeKeyboardConfiguration* configuration = keyboardShowEvent_.configuration;
    NKBKeyboardType keyboardType = configuration.keyboardType;
    NKBAutocapitalizationType autocapitalizationType = configuration.autocapitalizationType;
    BOOL autocorrection = configuration.autocorrection;
    BOOL secure = configuration.secure;
    bool hasNext = configuration.hasNext;
    NKBLineType lineType = configuration.lineType;
    NKBReturnKeyType returnKeyType = configuration.returnKeyType;
    
    switch (keyboardType)
    {
        case KT_DEFAULT:
            [_currentView changeKeyboardType: UIKeyboardTypeDefault];
            break;
        case KT_ASCII_CAPABLE:
            [_currentView changeKeyboardType: UIKeyboardTypeASCIICapable];
            break;
        case KT_DECIMAL_PAD:
            [_currentView changeKeyboardType: UIKeyboardTypeDecimalPad];
            break;
        case KT_URL:
            [_currentView changeKeyboardType: UIKeyboardTypeURL];
            break;
        case KT_NUMBER_PAD:
            [_currentView changeKeyboardType: UIKeyboardTypeNumberPad];
            break;
        case KT_PHONE_PAD:
            [_currentView changeKeyboardType: UIKeyboardTypePhonePad];
            break;
        case KT_EMAIL_ADDRESS:
            [_currentView changeKeyboardType: UIKeyboardTypeEmailAddress];
            break;
        case KT_NUMBERS_AND_PUNCTUATION:
            [_currentView changeKeyboardType: UIKeyboardTypeNumbersAndPunctuation];
            break;
    }
    
    if(autocorrection && !secure)
    {
        [_currentView changeAutocorrectionType:UITextAutocorrectionTypeYes];
        UITextInputAssistantItem* shortcut = [_currentView getShortcut];
        shortcut.leadingBarButtonGroups = _originalLeadingBarButtonGroups;
        shortcut.trailingBarButtonGroups = _originalTrailingBarButtonGroups;
    }
    else
    {
        [_currentView changeAutocorrectionType:UITextAutocorrectionTypeNo];
        UITextInputAssistantItem* shortcut = [_currentView getShortcut];
        shortcut.leadingBarButtonGroups = @[];
        shortcut.trailingBarButtonGroups = @[];
    }
    
    if(secure)
    {
        [_currentView changeSecureTextEntry:true];
    }
    else
    {
        [_currentView changeSecureTextEntry:false];
    }
    
    switch (autocapitalizationType)
    {
        case AT_NO_AUTOCAPITALIZATION:
            [_currentView changeAutocapitalizationType:UITextAutocapitalizationTypeNone];
            break;
        case AT_CHARACTERS:
            [_currentView changeAutocapitalizationType:UITextAutocapitalizationTypeAllCharacters];
            break;
        case AT_WORDS:
            [_currentView changeAutocapitalizationType:UITextAutocapitalizationTypeWords];
            break;
        case AT_SENTENCES:
            [_currentView changeAutocapitalizationType:UITextAutocapitalizationTypeSentences];
            break;
    }
    
    if(returnKeyType == RT_DEFAULT)
    {
        if(lineType == LT_MULTI_LINE_NEWLINE)
        {
            [_currentView changeReturnKeyType:UIReturnKeyDefault];
        }
        else if(hasNext)
        {
            [_currentView changeReturnKeyType:UIReturnKeyNext];
        }
        else
        {
            [_currentView changeReturnKeyType:UIReturnKeyDone];
        }
    }
    else
    {
        switch (returnKeyType)
        {
            case RT_GO:
                [_currentView changeReturnKeyType:UIReturnKeyGo];
                break;
            case RT_SEND:
                [_currentView changeReturnKeyType:UIReturnKeySend];
                break;
            case RT_SEARCH:
                [_currentView changeReturnKeyType:UIReturnKeySearch];
                break;
            default:
                [_currentView changeReturnKeyType:UIReturnKeyDefault];
                break;
        }
    }
}


-(void)processKeyboardHideEvent:(NKBKeyboardHideEvent*) keyboardHideEvent_
{
    _state = KS_PENDING_HIDE;
    _pendingStartTime = [[NSProcessInfo processInfo] systemUptime];
    [_currentView loseFocus];
}

-(void)updateKeyboardHeight
{
    if(_currentKeyboardHeight != _lastKeyboardHeight)
    {
        if(_currentKeyboardHeight == 0 && (_state == KS_PENDING_SHOW || _state == KS_VISIBLE))
        {
            return;
        }
        else if(_currentKeyboardHeight > 0 && (_state == KS_PENDING_HIDE || _state == KS_HIDDEN))
        {
            return;
        }
        
        float keyboardHeight = _currentKeyboardHeight * [UIScreen mainScreen].nativeScale;
        [_unityCallback onKeyboardHeightChanged:keyboardHeight];
    }
    
    _lastKeyboardHeight = _currentKeyboardHeight;
}

-(void)updateKeyboardVisibility
{
    if(_currentKeyboardHeight > 0 && _state == KS_PENDING_SHOW)
    {
        _keyboardVisible = true;
    }
    else if(_currentKeyboardHeight == 0 && (_state == KS_PENDING_HIDE || _state == KS_PENDING_RELOAD))
    {
        _keyboardVisible = false;
    }
}

-(void)updateHardwareKeyboardConnectivity
{
    BOOL connected = [self isHardwareKeyboardConnected];
    
    if(_hardwareKeyboardConnected != connected)
    {
        _hardwareKeyboardConnected = connected;
        [_unityCallback onHardwareKeyboardChanged:_hardwareKeyboardConnected];
    }
}

//Based on snippet from myyell0w: https://gist.github.com/myell0w/d8dfabde43f8da543f9c
-(BOOL) isHardwareKeyboardConnected
{
    BOOL hardwareKeyboardAttached = NO;
    
    @try
    {
        NSString *keyboardClassName = [@[@"UI", @"Key", @"boa", @"rd", @"Im", @"pl"] componentsJoinedByString:@""];
        Class c = NSClassFromString(keyboardClassName);
        SEL sharedInstanceSEL = NSSelectorFromString(@"sharedInstance");
        if (c == Nil || ![c respondsToSelector:sharedInstanceSEL])
        {
            return NO;
        }
        
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Warc-performSelector-leaks"
        id sharedKeyboardInstance = [c performSelector:sharedInstanceSEL];
#pragma clang diagnostic pop
        
        if (![sharedKeyboardInstance isKindOfClass:NSClassFromString(keyboardClassName)])
        {
            return NO;
        }
        
        NSString *hardwareKeyboardSelectorName = [@[@"is", @"InH", @"ardw", @"areK", @"eyb", @"oard", @"Mode"] componentsJoinedByString:@""];
        SEL hardwareKeyboardSEL = NSSelectorFromString(hardwareKeyboardSelectorName);
        if (![sharedKeyboardInstance respondsToSelector:hardwareKeyboardSEL])
        {
            return NO;
        }
        
        hardwareKeyboardAttached = ((BOOL ( *)(id, SEL))objc_msgSend)(sharedKeyboardInstance, hardwareKeyboardSEL);
    }
    @catch(__unused NSException *ex)
    {
        hardwareKeyboardAttached = NO;
    }
    
    return hardwareKeyboardAttached;
}
//endregion PROCESS

//region INPUTFIELD_EVENTS
-(void)onKeyboardDidHide:(NSNotification *)notification
{
    _currentKeyboardHeight = 0; //For some reason the keyboard rect is never zero
}

-(void)onKeyboardDidShow:(NSNotification *)notification
{
    _currentKeyboardHeight = [self getKeyboardHeight: notification];
}

-(void)onKeyboardDidChangeFrame:(NSNotification *)notification
{
    _currentKeyboardHeight = [self getKeyboardHeight: notification];
    
    [self checkKeyboardVisible: notification]; //Workaround for split keyboard
}

-(void)checkKeyboardVisible:(NSNotification *)notification
{
    CGRect keyboardEndFrame;
    [[notification.userInfo objectForKey:UIKeyboardFrameEndUserInfoKey] getValue:&keyboardEndFrame];
    CGRect keyboardFrame = [_window convertRect:keyboardEndFrame fromView:nil];
    
    if (!CGRectIntersectsRect(keyboardFrame, _window.frame)) //Keyboard is hidden
    {
        _currentKeyboardHeight = 0;
    }
}

- (void)onKeyboardWillHide:(NSNotification *)notification
{
    if(_state == KS_VISIBLE)
    {
        NSTimeInterval timeElapsed = [[NSProcessInfo processInfo] systemUptime] - _visibleStartTime;
        if(timeElapsed >= MIN_VISIBLE_TIME_FOR_CANCEL)
        {
            if([_currentView getAutofillType] == AFT_NONE)
            {
                [_unityCallback onKeyboardCancel];
                _state = KS_HIDDEN;
            }
        }
    }
    else if(_state == KS_PENDING_SHOW)
    {
        if(_currentKeyboardHeight > 0)
        {
            [_unityCallback onKeyboardCancel];
            _state = KS_HIDDEN;
        }
    }
}

-(int)getKeyboardHeight:(NSNotification *)notification
{
    CGRect keyboardRect = [notification.userInfo[UIKeyboardFrameEndUserInfoKey] CGRectValue];
    
    if([_currentView getAccesoryView] != nil)
    {
        CGRect inputAccessoryViewRect = [_currentView getAccesoryView].bounds;
        return keyboardRect.size.height + inputAccessoryViewRect.size.height;
    }
    else
    {
        return keyboardRect.size.height;
    }
}

-(BOOL)textField:(UITextField *)textField shouldChangeCharactersInRange:(NSRange)range replacementString:(NSString *)text
{
    /* for backspace */
    if(text.length==0)
    {
        return YES;
    }
    
    if (_lineType != LT_MULTI_LINE_NEWLINE && [text isEqualToString:@"\n"])
    {
        [_unityCallback onKeyboardDone];
        return NO;
    }
    
    if(!_emojisAllowed && _characterLimit > 0 && [_currentView getText].length >= _characterLimit)
    {
        NSString* currentText = textField.text;
        NSString* newText = [currentText stringByReplacingCharactersInRange:range withString:text];
        if(newText.length > _characterLimit)
        {
            return NO;
        }
    }
    
    if ([textField isFirstResponder])
    {
        if(_emojisAllowed)
        {
            return YES;
        }
        else
        {
            if([NKBUtil containsEmoji:text])
            {
                return NO;
            }
        }
    }
    
    return YES;
}

- (BOOL)textView:(UITextView *)textView shouldChangeTextInRange:(NSRange)range replacementText:(NSString *)text
{
    /* for backspace */
    if(text.length==0)
    {
        return YES;
    }
    
    if (_lineType != LT_MULTI_LINE_NEWLINE && [text isEqualToString:@"\n"])
    {
        [_unityCallback onKeyboardDone];
        return NO;
    }
    
    if(!_emojisAllowed && _characterLimit > 0 && [_currentView getText].length >= _characterLimit)
    {
        NSString* currentText = textView.text;
        NSString* newText = [currentText stringByReplacingCharactersInRange:range withString:text];
        if(newText.length > _characterLimit)
        {
            return NO;
        }
    }
    
    if ([textView isFirstResponder])
    {
        if(_emojisAllowed)
        {
            return YES;
        }
        else
        {
            if([NKBUtil containsEmoji:text])
            {
                return NO;
            }
        }
    }
    
    return YES;
}

-(void)textFieldDidBeginEditing:(UITextField *)textField
{
    //NSLog(@"TextFieldDidBeginEditing: %@", textField.text);
}

-(void)textViewDidBeginEditing:(UITextView *)textView
{
    //NSLog(@"TextViewDidBeginEditing: %@", textView.text);
}

-(void)textFieldDidEndEditing:(UITextField *)textField
{
    //NSLog(@"TextFieldDidEndEditing: %@", textField.text);
    NKBSingleLineView* singleLineView = (NKBSingleLineView*)textField;
    if([singleLineView getAutofillType] != AFT_NONE && _currentEvent == nil) //Workaround for keyboard dismiss keyboard issue on autofill view
    {
        [_unityCallback onKeyboardCancel];
    }
}

- (void)textViewDidEndEditing:(UITextView *)textView
{
    //NSLog(@"TextViewDidEndEditing: %@", textView.text);
}

- (void)textFieldDidChange:(UITextField *)textField
{
    //NSLog(@"TextFieldDidEndEditing: %@", textField.text);
    if(textField.hash != [_currentView getHashCode])
    {
        NKBAutofillType autofillType = [self getAutofillTypeForHashCode: textField.hash];
        if(autofillType != AFT_NONE)
        {
            //NSLog(@"Detected autofill: %ld", (long)autofillType);
            [_unityCallback onAutofillUpdate:textField.text autofillType:(int)autofillType];
        }
        return;
    }
    
    UITextRange* selectionRange = textField.selectedTextRange;
    UITextPosition* selectionStartPos = selectionRange.start;
    int selectionStart = (int)[textField offsetFromPosition:textField.beginningOfDocument toPosition:selectionStartPos];
    UITextPosition* selectionEndPos = selectionRange.end;
    int selectionEnd = (int)[textField offsetFromPosition:textField.beginningOfDocument toPosition:selectionEndPos];
    
    if(_currentEvent != NULL)
    {
        //NSLog(@"Early return, event is not null");
        return;
    }
    
    if(_characterValidation != CV_NONE)
    {
        NSString* lastText = textField.text;
        NSString* text = @"";
    
        int caretPosition = selectionStart;
        int selectionStartPosition = -1;
        if(selectionEnd - selectionStart > 0)
        {
            selectionStartPosition = selectionStart;
        }
        
        [_textValidator validate:text textToAppend:lastText caretPosition:caretPosition selectionStartPosition:selectionStartPosition];
        text = _textValidator.resultText;
        
        if(![lastText isEqualToString:text])
        {
            int lastSelectionStart = selectionStart;
            [textField.undoManager removeAllActions];
            [textField setText: text];
            
            int amountChanged = (int)text.length - (int)lastText.length;
            @try
            {
                caretPosition = lastSelectionStart + amountChanged;
                
                UITextPosition *caretTextPosition = [textField positionFromPosition:textField.beginningOfDocument offset:caretPosition];
                textField.selectedTextRange = [textField textRangeFromPosition:caretTextPosition toPosition:caretTextPosition];;
            }
            @catch(NSException* e) { NSLog(@"iOS: Failed to change caret: %d", caretPosition); }
        }
    }
    
    NSString* text = textField.text;
    selectionRange = textField.selectedTextRange;
    selectionStartPos = selectionRange.start;
    selectionStart = (int)[textField offsetFromPosition:textField.beginningOfDocument toPosition:selectionStartPos];
    selectionEndPos = selectionRange.end;
    selectionEnd = (int)[textField offsetFromPosition:textField.beginningOfDocument toPosition:selectionEndPos];
    [_unityCallback onTextEditUpdate:text start:selectionStart end:selectionEnd];
    
    NKBAutofillType autofillType = [self getAutofillTypeForHashCode: textField.hash];
    if(autofillType != AFT_NONE)
    {
        //NSLog(@"Detected autofill: %ld", (long)autofillType);
        [_unityCallback onAutofillUpdate:textField.text autofillType:(int)autofillType];
    }
    
    _lastText = text;
    _lastSelectionStartPosition = selectionStart;
    _lastSelectionEndPosition = selectionEnd;
}

- (void)textViewDidChange:(UITextView *)textView
{
    UITextRange* selectionRange = textView.selectedTextRange;
    UITextPosition* selectionStartPos = selectionRange.start;
    int selectionStart = (int)[textView offsetFromPosition:textView.beginningOfDocument toPosition:selectionStartPos];
    UITextPosition* selectionEndPos = selectionRange.end;
    int selectionEnd = (int)[textView offsetFromPosition:textView.beginningOfDocument toPosition:selectionEndPos];
    
    if(_currentEvent != NULL)
    {
        return;
    }
    
    if(_characterValidation != CV_NONE)
    {
        NSString* lastText = textView.text;
        NSString* text = @"";
    
        int caretPosition = selectionStart;
        int selectionStartPosition = -1;
        if(selectionEnd - selectionStart > 0)
        {
            selectionStartPosition = selectionStart;
        }
        
        [_textValidator validate:text textToAppend:lastText caretPosition:caretPosition selectionStartPosition:selectionStartPosition];
        text = _textValidator.resultText;
        
        if(![lastText isEqualToString:text])
        {
            int lastSelectionStart = selectionStart;
            [textView.undoManager removeAllActions];
            [textView setText: text];
            
            int amountChanged = (int)text.length - (int)lastText.length;
            @try
            {
                caretPosition = lastSelectionStart + amountChanged;
                [textView setSelectedRange:NSMakeRange(caretPosition, 0)];
            }
            @catch(NSException* e) { NSLog(@"iOS: Failed to change caret: %d", caretPosition); }
        }
    }
    
    NSString* text = textView.text;
    selectionRange = textView.selectedTextRange;
    selectionStartPos = selectionRange.start;
    selectionStart = (int)[textView offsetFromPosition:textView.beginningOfDocument toPosition:selectionStartPos];
    selectionEndPos = selectionRange.end;
    selectionEnd = (int)[textView offsetFromPosition:textView.beginningOfDocument toPosition:selectionEndPos];
    [_unityCallback onTextEditUpdate:text start:selectionStart end:selectionEnd];
    
    _lastText = text;
    _lastSelectionStartPosition = selectionStart;
    _lastSelectionEndPosition = selectionEnd;
}

- (void)textFieldDidDelete
{
    [_unityCallback onSpecialKeyPressed: SKC_BACKSPACE];
}
//endregion INPUTFIELD_EVENTS

//region PUBLIC_METHODS
-(void)enableUpdates
{
    if(!_updatesEnabled)
    {
        _updatesEnabled = true;
        
        if(_cancelUpdateWhenDone) //Thread action haven't been cancelled yet
        {
            _cancelUpdateWhenDone = false;
        }
        else
        {
            [NKBUtil runOnMainThread:_nativeKeyboard_instance.updateHandler delay:0];
        }
    }
}

-(void)disableUpdates
{
    if(_updatesEnabled)
    {
        _updatesEnabled = false;
        _cancelUpdateWhenDone = true;
    }
}

-(void)enableHardwareKeyboardUpdates
{
    if(!_hardwareKeyboardUpdatesEnabled)
    {
        _hardwareKeyboardUpdatesEnabled = true;
        
        if(_cancelHardwareKeyboardUpdateWhenDone) //Thread action haven't been cancelled yet
        {
            _cancelHardwareKeyboardUpdateWhenDone = false;
        }
        else
        {
            [NKBUtil runOnMainThread:_nativeKeyboard_instance.hardwareKeyboardUpdateHandler delay:0];
        }
    }
}

-(void)disableHardwareKeyboardUpdates
{
    if(_hardwareKeyboardUpdatesEnabled)
    {
        _hardwareKeyboardUpdatesEnabled = false;
        _cancelHardwareKeyboardUpdateWhenDone = true;
    }
}

-(void)updateTextEdit:(NSString*)text_
selectionStartPosition:(int)selectionStartPosition_
 selectionEndPosition:(int)selectionEndPosition_
{
    NKBTextEditUpdateEvent* textEditUpdateEvent = [[NKBTextEditUpdateEvent alloc]initWithText:text_ selectionStartPosition:selectionStartPosition_ selectionEndPosition:selectionEndPosition_];
    
    [_unityEventQueue enqueue:textEditUpdateEvent];
    [self setNewestTextEditUpdateEventSafe:textEditUpdateEvent];
}

-(void)requestTextEditUpdate
{
    [NKBUtil runOnMainThread:^
    {
        [_currentView requestFocus];
        [_currentView refreshInputViews];
        
        [NKBUtil runOnMainThread:^
        {
            [_currentView loseFocus];
        }
        delay:3.5f];
    }
    delay:0.1f];
}

-(void)showKeyboard:(NSString*)text_
selectionStartPosition:(int)selectionStartPosition_
selectionEndPosition:(int)selectionEndPosition_
configurationJSON:(NSString*)configurationJSON_
{
    NKBNativeKeyboardConfiguration* configuration = nil;
    NSError *jsonError;
    NSData *configurationData = [configurationJSON_ dataUsingEncoding:NSUTF8StringEncoding];
    NSDictionary *jsonObject = [NSJSONSerialization JSONObjectWithData:configurationData
                                                         options:NSJSONReadingMutableContainers
                                                           error:&jsonError];
    if(jsonError == nil)
    {
        configuration = [[NKBNativeKeyboardConfiguration alloc]initWithJSON:jsonObject];
    }
    
    NKBKeyboardShowEvent* keyboardShowEvent = [[NKBKeyboardShowEvent alloc]initWithText:text_ selectionStartPosition:selectionStartPosition_ selectionEndPosition:selectionEndPosition_ configuration:configuration];
    
    [_unityEventQueue enqueue:keyboardShowEvent];
}

-(void)restoreKeyboard
{
     [NKBUtil runOnMainThread:^
       {
           _state = KS_PENDING_SHOW;
           _pendingStartTime = [[NSProcessInfo processInfo] systemUptime];
           _visibleStartTime = [[NSProcessInfo processInfo] systemUptime];
           [_currentView requestFocus];
           [_currentView refreshInputViews];
       }
       delay:0.1f];
}

-(void)hideKeyboard
{
    NKBKeyboardHideEvent* keyboardHideEvent = [[NKBKeyboardHideEvent alloc]init];
    
    [_unityEventQueue enqueue:keyboardHideEvent];
}

-(void)saveCredentials:(NSString*)domainName_
{
    NKBSingleLineView* usernameView = [self getViewForAutofill:AFT_USERNAME];
    NKBSingleLineView* passwordView = [self getViewForAutofill:AFT_PASSWORD];
    
    if(usernameView == nil || usernameView.text.length == 0)
    {
        NSLog(@"Username not filled in");
        return;
    }
    
    if(passwordView == nil || passwordView.text.length == 0)
    {
        NSLog(@"Password not filled in, checking new password instead");
        passwordView = [self getViewForAutofill:AFT_NEW_PASSWORD];
        if(passwordView == nil || passwordView.text.length == 0)
        {
            NSLog(@"New password not filled in");
            return;
        }
    }
    
    NSString* domainNameString = [[NSString alloc] initWithString:domainName_];
    NSString* usernameString = [[NSString alloc] initWithString:usernameView.text];
    NSString* passwordString = [[NSString alloc] initWithString:passwordView.text];
    CFStringRef domainName = (__bridge_retained CFStringRef)domainNameString;
    CFStringRef username = (__bridge_retained CFStringRef)usernameString;
    CFStringRef password = (__bridge_retained CFStringRef)passwordString;
    
    [NKBUtil runOnMainThread:^
      {
        SecAddSharedWebCredential(domainName, username, password,
        ^(CFErrorRef error)
        {
            if(error == nil)
            {
                NSLog(@"Saved credentials successfully");
            }
            else
            {
                NSLog(@"Save credentials failed with error: %@", error);
            }
            CFRelease(domainName);
            CFRelease(username);
            CFRelease(password);
        });
      }
      delay:0.1f];
}
//endregion PUBLIC_METHODS
@end
