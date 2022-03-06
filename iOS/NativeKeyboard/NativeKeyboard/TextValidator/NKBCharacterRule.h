//
//  CharacterRule.h
//  NativeKeyboard
//
//  Created by Jeroen Pienbroek on 30/10/2018.
//  Copyright © 2018 Jeroen van Pienbroek. All rights reserved.
//

#ifndef NKBCharacterRule_h
#define NKBCharacterRule_h

#import <Foundation/Foundation.h>

typedef NS_ENUM(int, NKBCharacterAction)
{
    NKB_ALLOW = 0,
    NKB_BLOCK = 1,
    NKB_TO_UPPERCASE = 2,
    NKB_TO_LOWERCASE = 3,
    NKB_REPLACE = 4
};

@interface NKBCharacterRule : NSObject

@property(nonatomic, strong) NSMutableArray* conditions;
@property(nonatomic, assign) NKBCharacterAction action;
@property(nonatomic, assign) int actionIntValue;

-(id)initWithJSON:(NSDictionary*)jsonObject_;

-(void)parseJSON:(NSDictionary*)jsonObject_;

-(bool)areConditionsMet:(unichar)ch_
                   text:(unichar*)text_
             textLength:(int)textLength_
                  pos:(int)pos_
selectionStartPosition:(int)selectionStartPosition_;

@end
#endif
