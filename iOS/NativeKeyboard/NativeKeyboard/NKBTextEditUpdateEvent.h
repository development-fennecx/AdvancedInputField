//
//  NKBTextEditUpdateEvent.h
//  TouchScreenKeyboard
//
//  Created by Jeroen Pienbroek on 28/01/2018.
//  Copyright © 2018 Jeroen van Pienbroek. All rights reserved.
//

#ifndef NKBSelectionChangeEvent_h
#define NKBSelectionChangeEvent_h

#import "NKBIUnityEvent.h"

@interface NKBTextEditUpdateEvent : NSObject<NKBIUnityEvent>

@property (nonatomic, strong) NSString* text;
@property (nonatomic, assign) int selectionStartPosition;
@property (nonatomic, assign) int selectionEndPosition;

-(id)initWithText:(NSString*)text_
selectionStartPosition:(int)selectionStartPosition_
selectionEndPosition:(int)selectionEndPosition_;

@end

#endif /* SelectionChangeEvent_h */
