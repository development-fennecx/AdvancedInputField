//
//  CharacterCondition.h
//  NativeKeyboard
//
//  Created by Jeroen Pienbroek on 30/10/2018.
//  Copyright © 2018 Jeroen van Pienbroek. All rights reserved.
//

#ifndef NKBCharacterCondition_h
#define NKBCharacterCondition_h

#import <Foundation/Foundation.h>

typedef NS_ENUM(int, NKBCharacterConditionOperator)
{
    NKB_VALUE_EQUALS = 0,
    NKB_VALUE_SMALLER_THAN = 1,
    NKB_VALUE_SMALLER_THAN_OR_EQUALS = 2,
    NKB_VALUE_GREATER_THAN = 3,
    NKB_VALUE_GREATER_THAN_OR_EQUALS = 4,
    NKB_VALUE_BETWEEN_INCLUSIVE = 5,
    NKB_VALUE_BETWEEN_EXCLUSIVE = 6,
    NKB_VALUE_IN_STRING = 7,
    NKB_INDEX_EQUALS = 8,
    NKB_INDEX_SMALLER_THAN = 9,
    NKB_INDEX_SMALLER_THAN_OR_EQUALS = 10,
    NKB_INDEX_GREATER_THAN = 11,
    NKB_INDEX_GREATER_THAN_OR_EQUALS = 12,
    NKB_INDEX_BETWEEN_INCLUSIVE = 13,
    NKB_INDEX_BETWEEN_EXCLUSIVE = 14,
    NKB_OCCURENCES_SMALLER_THAN = 15,
    NKB_OCCURENCES_SMALLER_THAN_OR_EQUALS = 16,
    NKB_OCCURENCES_GREATER_THAN = 17,
    NKB_OCCURENCES_GREATER_THAN_OR_EQUALS = 18,
    NKB_VALUE_SAME_AS_PREVIOUS = 19
};

@interface NKBCharacterCondition : NSObject

@property(nonatomic, assign) NKBCharacterConditionOperator conditionOperator;
@property(nonatomic, assign) int conditionIntValue1;
@property(nonatomic, assign) int conditionIntValue2;
@property(nonatomic, strong) NSString* conditionStringValue;

-(id)initWithJSON:(NSDictionary*)jsonObject_;

-(void)parseJSON:(NSDictionary*)jsonObject_;

-(bool)isConditionMet:(unichar)ch_
                 text:(unichar*)text_
           textLength:(int)textLength_
                  pos:(int)pos_
selectionStartPosition:(int)selectionStartPosition_;

@end
#endif
