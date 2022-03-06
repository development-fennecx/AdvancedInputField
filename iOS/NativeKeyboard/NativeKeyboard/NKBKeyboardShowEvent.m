//
//  KeyboardShowEvent.m
//  TouchScreenKeyboard
//
//  Created by Jeroen Pienbroek on 28/01/2018.
//  Copyright © 2018 Jeroen van Pienbroek. All rights reserved.
//

#import "NKBKeyboardShowEvent.h"
#import "NKBNativeKeyboard.h"
#import <Foundation/Foundation.h>

@implementation NKBKeyboardShowEvent

-(id)initWithText:(NSString*) text_
selectionStartPosition:(int)selectionStartPosition_
selectionEndPosition:(int)selectionEndPosition_
     configuration:(NKBNativeKeyboardConfiguration*)configuration_
{
    self = [super init];
    
    if(self)
    {
        self.text = text_;
        self.selectionStartPosition = selectionStartPosition_;
        self.selectionEndPosition = selectionEndPosition_;
        self.configuration = configuration_;
    }
    
    return self;
}

-(NKBEventType) getType
{
    return ET_KEYBOARD_SHOW;
}

@end
