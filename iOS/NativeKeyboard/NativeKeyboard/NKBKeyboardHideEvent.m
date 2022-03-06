//
//  KeyboardHideEvent.m
//  TouchScreenKeyboard
//
//  Created by Jeroen Pienbroek on 28/01/2018.
//  Copyright © 2018 Jeroen van Pienbroek. All rights reserved.
//

#import "NKBKeyboardHideEvent.h"
#import "NKBNativeKeyboard.h"
#import <Foundation/Foundation.h>

@implementation NKBKeyboardHideEvent

-(NKBEventType) getType
{
    return ET_KEYBOARD_HIDE;
}

@end
