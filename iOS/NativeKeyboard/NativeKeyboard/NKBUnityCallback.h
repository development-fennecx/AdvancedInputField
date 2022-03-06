//
//  UnityCallback.h
//  TouchScreenKeyboard
//
//  Created by Jeroen Pienbroek on 27/01/2018.
//  Copyright © 2018 Jeroen van Pienbroek. All rights reserved.
//

#ifndef NKBUnityCallback_h
#define NKBUnityCallback_h

#import <Foundation/Foundation.h>

typedef void (*NKBOnTextEditUpdate)(const char* text, int selectionStartPosition, int selectionEndPosition);
typedef void (*NKBOnAutofillUpdate)(const char* text, int autofillType);
typedef void (*NKBOnKeyboardShow)(void);
typedef void (*NKBOnKeyboardHide)(void);
typedef void (*NKBOnKeyboardDone)(void);
typedef void (*NKBOnKeyboardNext)(void);
typedef void (*NKBOnKeyboardCancel)(void);
typedef void (*NKBOnSpecialKeyPressed)(int specialKeyCode);
typedef void (*NKBOnKeyboardHeightChanged)(int height);
typedef void (*NKBOnHardwareKeyboardChanged)(bool connected);
typedef NS_ENUM(int, NKBSpecialKeyCode);

@interface NKBUnityCallback : NSObject

-(id)initWithOnTextEditUpdate:(NKBOnTextEditUpdate)onTextEditUpdate_
             onAutofillUpdate:(NKBOnAutofillUpdate)onAutofillUpdate_
               onKeyboardShow:(NKBOnKeyboardShow)onKeyboardShow_
               onKeyboardHide:(NKBOnKeyboardHide)onKeyboardHide_
               onKeyboardDone:(NKBOnKeyboardDone)onKeyboardDone_
               onKeyboardNext:(NKBOnKeyboardNext)onKeyboardNext_
             onKeyboardCancel:(NKBOnKeyboardCancel)onKeyboardCancel_
               onKeyboardDelete:(NKBOnSpecialKeyPressed)onSpecialKeyPressed_
      onKeyboardHeightChanged:(NKBOnKeyboardHeightChanged)onKeyboardHeightChanged_
    onHardwareKeyboardChanged:(NKBOnHardwareKeyboardChanged)onHardwareKeyboardChanged_;

-(void) onTextEditUpdate:(NSString*) text_
                   start:(int) start_
                     end:(int) end_;

-(void) onAutofillUpdate:(NSString*) text_
            autofillType:(int) autofillType_;

-(void) onKeyboardShow;

-(void) onKeyboardHide;

-(void) onKeyboardDone;

-(void) onKeyboardNext;

-(void) onKeyboardCancel;

-(void) onSpecialKeyPressed:(NKBSpecialKeyCode)specialKeyCode_;

-(void) onKeyboardHeightChanged:(NSUInteger) height_;

-(void) onHardwareKeyboardChanged:(BOOL) connected_;

@end
#endif /* UnityCallback_h */
