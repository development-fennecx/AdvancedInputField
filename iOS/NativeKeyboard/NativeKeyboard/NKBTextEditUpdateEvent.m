//
//  NKBTextEditUpdateEvent.m
//  TouchScreenKeyboard
//
//  Created by Jeroen Pienbroek on 28/01/2018.
//  Copyright © 2018 Jeroen van Pienbroek. All rights reserved.
//

#import "NKBTextEditUpdateEvent.h"
#import "NKBNativeKeyboard.h"
#import <Foundation/Foundation.h>

@implementation NKBTextEditUpdateEvent

-(id)initWithText:(NSString*)text_
selectionStartPosition:(int)selectionStartPosition_
selectionEndPosition:(int)selectionEndPosition_
{
    self = [super init];
    
    if(self)
    {
        self.text = text_;
        self.selectionStartPosition = selectionStartPosition_;
        self.selectionEndPosition = selectionEndPosition_;
    }
    
    return self;
}

-(NKBEventType) getType
{
    return ET_TEXT_EDIT_UPDATE;
}

@end

