//
//  UnityCallback.m
//  TouchScreenKeyboard
//
//  Created by Jeroen Pienbroek on 27/01/2018.
//  Copyright © 2018 Jeroen van Pienbroek. All rights reserved.
//

#import "NKBUnityCallback.h"
#import "NKBUtil.h"
#import "NKBNSString+Extensions.h"

@interface NKBUnityCallback()
@property (nonatomic, assign) NKBOnTextEditUpdate textEditUpdate;
@property (nonatomic, assign) NKBOnAutofillUpdate autofillUpdate;
@property (nonatomic, assign) NKBOnKeyboardShow keyboardShow;
@property (nonatomic, assign) NKBOnKeyboardHide keyboardHide;
@property (nonatomic, assign) NKBOnKeyboardDone keyboardDone;
@property (nonatomic, assign) NKBOnKeyboardNext keyboardNext;
@property (nonatomic, assign) NKBOnKeyboardCancel keyboardCancel;
@property (nonatomic, assign) NKBOnSpecialKeyPressed specialKeyPressed;
@property (nonatomic, assign) NKBOnKeyboardHeightChanged keyboardHeightChanged;
@property (nonatomic, assign) NKBOnHardwareKeyboardChanged hardwareKeyboardChanged;
@end

@implementation NKBUnityCallback


-(id)initWithOnTextEditUpdate:(NKBOnTextEditUpdate)onTextEditUpdate_
             onAutofillUpdate:(NKBOnAutofillUpdate)onAutofillUpdate_
               onKeyboardShow:(NKBOnKeyboardShow)onKeyboardShow_
               onKeyboardHide:(NKBOnKeyboardHide)onKeyboardHide_
               onKeyboardDone:(NKBOnKeyboardDone)onKeyboardDone_
               onKeyboardNext:(NKBOnKeyboardNext)onKeyboardNext_
             onKeyboardCancel:(NKBOnKeyboardCancel)onKeyboardCancel_
             onKeyboardDelete:(NKBOnSpecialKeyPressed)onSpecialKeyPressed_
      onKeyboardHeightChanged:(NKBOnKeyboardHeightChanged)onKeyboardHeightChanged_
    onHardwareKeyboardChanged:(NKBOnHardwareKeyboardChanged)onHardwareKeyboardChanged_
{
    self = [super init];
    
    if(self)
    {
        self.textEditUpdate = onTextEditUpdate_;
        self.autofillUpdate = onAutofillUpdate_;
        self.keyboardShow = onKeyboardShow_;
        self.keyboardHide = onKeyboardHide_;
        self.keyboardDone = onKeyboardDone_;
        self.keyboardNext = onKeyboardNext_;
        self.keyboardCancel = onKeyboardCancel_;
        self.specialKeyPressed = onSpecialKeyPressed_;
        self.keyboardHeightChanged = onKeyboardHeightChanged_;
        self.hardwareKeyboardChanged = onHardwareKeyboardChanged_;
    }
    
    return self;
}

-(void) onTextEditUpdate:(NSString*) text_
                     start:(int) start_
                       end:(int) end_
{
    _textEditUpdate([text_ toUnityString], (int)start_, (int)end_);
}

-(void) onAutofillUpdate:(NSString*) text_
            autofillType:(int) autofillType_
{
    _autofillUpdate([text_ toUnityString], autofillType_);
}

-(void) onKeyboardShow
{
    _keyboardShow();
}

-(void) onKeyboardHide
{
    _keyboardHide();
}

-(void) onKeyboardDone
{
    _keyboardDone();
}

-(void) onKeyboardNext
{
    _keyboardNext();
}

-(void) onKeyboardCancel
{
    _keyboardCancel();
}

-(void) onSpecialKeyPressed:(NKBSpecialKeyCode)specialKeyCode_
{
    _specialKeyPressed(specialKeyCode_);
}

-(void) onKeyboardHeightChanged:(NSUInteger) height_
{
    _keyboardHeightChanged((int)height_);
}

-(void) onHardwareKeyboardChanged:(BOOL) connected_
{
    _hardwareKeyboardChanged(connected_);
}

@end
