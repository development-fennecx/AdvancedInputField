//
//  KeyboardShowEvent.h
//  TouchScreenKeyboard
//
//  Created by Jeroen Pienbroek on 28/01/2018.
//  Copyright © 2018 Jeroen van Pienbroek. All rights reserved.
//

#ifndef NKBKeyboardShowEvent_h
#define NKBKeyboardShowEvent_h

#import "NKBIUnityEvent.h"
#import "NKBNativeKeyboardConfiguration.h"

@interface NKBKeyboardShowEvent : NSObject<NKBIUnityEvent>

@property (nonatomic, strong) NSString* text;
@property (nonatomic, assign) int selectionStartPosition;
@property (nonatomic, assign) int selectionEndPosition;
@property (nonatomic, strong) NKBNativeKeyboardConfiguration* configuration;

-(id)initWithText:(NSString*) text_
selectionStartPosition:(int)selectionStartPosition_
selectionEndPosition:(int)selectionEndPosition_
     configuration:(NKBNativeKeyboardConfiguration*)configuration_;

@end

#endif /* KeyboardShowEvent_h */
