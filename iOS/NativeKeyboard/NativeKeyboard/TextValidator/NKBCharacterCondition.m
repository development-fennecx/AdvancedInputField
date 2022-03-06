//
//  CharacterCondition.m
//  NativeKeyboard
//
//  Created by Jeroen Pienbroek on 30/10/2018.
//  Copyright © 2018 Jeroen van Pienbroek. All rights reserved.
//

#import "NKBCharacterCondition.h"
#import "NKBUtil.h"

@implementation NKBCharacterCondition

-(id)initWithJSON:(NSDictionary*)jsonObject_
{
    if(self = [super init])
    {
        [self parseJSON:jsonObject_];
    }
    return self;
}

-(void)parseJSON:(NSDictionary*)jsonObject_
{   
    _conditionOperator = [((NSNumber*)[jsonObject_ objectForKey:@"conditionOperator"])intValue];
    _conditionIntValue1 = [((NSNumber*)[jsonObject_ objectForKey:@"conditionIntValue1"])intValue];
    _conditionIntValue2 = [((NSNumber*)[jsonObject_ objectForKey:@"conditionIntValue2"])intValue];
    _conditionStringValue = (NSString*)[jsonObject_ objectForKey:@"conditionStringValue"];
}

-(bool)isConditionMet:(unichar)ch_
                 text:(unichar*)text_
           textLength:(int)textLength_
                  pos:(int)pos_
selectionStartPosition:(int)selectionStartPosition_;
{
    switch(_conditionOperator)
    {
        case NKB_VALUE_EQUALS:
            if((int)ch_ == _conditionIntValue1) { return true; }
            break;
        case NKB_VALUE_SMALLER_THAN:
            if((int)ch_ < _conditionIntValue1) { return true; }
            break;
        case NKB_VALUE_SMALLER_THAN_OR_EQUALS:
            if((int)ch_ <= _conditionIntValue1) { return true; }
            break;
        case NKB_VALUE_GREATER_THAN:
            if((int)ch_ > _conditionIntValue1) { return true; }
            break;
        case NKB_VALUE_GREATER_THAN_OR_EQUALS:
            if((int)ch_ >= _conditionIntValue1) { return true; }
            break;
        case NKB_VALUE_BETWEEN_EXCLUSIVE:
            if((int)ch_ > _conditionIntValue1 && (int)ch_ < _conditionIntValue2) { return true; }
            break;
        case NKB_VALUE_BETWEEN_INCLUSIVE:
            if((int)ch_ >= _conditionIntValue1 && (int)ch_ <= _conditionIntValue2) { return true; }
            break;
        case NKB_VALUE_IN_STRING:
            if([_conditionStringValue containsString:[NSString stringWithFormat:@"%C", ch_]]) { return true; }
            break;
        case NKB_INDEX_EQUALS:
            if(pos_ == _conditionIntValue1) { return true; }
            break;
        case NKB_INDEX_SMALLER_THAN:
            if(pos_ < _conditionIntValue1) { return true; }
            break;
        case NKB_INDEX_SMALLER_THAN_OR_EQUALS:
            if(pos_ <= _conditionIntValue1) { return true; }
            break;
        case NKB_INDEX_GREATER_THAN:
            if(pos_ > _conditionIntValue1) { return true; }
            break;
        case NKB_INDEX_GREATER_THAN_OR_EQUALS:
            if(pos_ >= _conditionIntValue1) { return true; }
            break;
        case NKB_INDEX_BETWEEN_EXCLUSIVE:
            if(pos_ > _conditionIntValue1 && pos_ < _conditionIntValue2) { return true; }
            break;
        case NKB_INDEX_BETWEEN_INCLUSIVE:
            if(pos_ >= _conditionIntValue1 && pos_ <= _conditionIntValue2) { return true; }
            break;
        case NKB_OCCURENCES_SMALLER_THAN:
            if([NKBUtil countOccurences:ch_ text:text_ textLength:textLength_] < _conditionIntValue2) { return true; }
            break;
        case NKB_OCCURENCES_SMALLER_THAN_OR_EQUALS:
            if([NKBUtil countOccurences:ch_ text:text_ textLength:textLength_] <= _conditionIntValue2) { return true; }
            break;
        case NKB_OCCURENCES_GREATER_THAN:
            if([NKBUtil countOccurences:ch_ text:text_ textLength:textLength_] > _conditionIntValue2) { return true; }
            break;
        case NKB_OCCURENCES_GREATER_THAN_OR_EQUALS:
            if([NKBUtil countOccurences:ch_ text:text_ textLength:textLength_] >= _conditionIntValue2) { return true; }
            break;
        case NKB_VALUE_SAME_AS_PREVIOUS:
            if(pos_ == 0) { return false; }
            if(ch_ == text_[pos_ - 1]) { return true; }
            break;
    }
    
    return false;
}

@end
