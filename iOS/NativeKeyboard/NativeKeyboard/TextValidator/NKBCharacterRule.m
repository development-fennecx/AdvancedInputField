//
//  CharacterRule.m
//  NativeKeyboard
//
//  Created by Jeroen Pienbroek on 30/10/2018.
//  Copyright © 2018 Jeroen van Pienbroek. All rights reserved.
//

#import "NKBCharacterRule.h"
#import "NKBCharacterCondition.h"

@implementation NKBCharacterRule

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
    NSArray* conditionsJSON = (NSArray*)[jsonObject_ objectForKey:@"conditions"];
    _conditions = [[NSMutableArray alloc]init];
    
    NSUInteger length = conditionsJSON.count;
    for(int i = 0; i < length; i++)
    {
        NSDictionary* conditionJSON = conditionsJSON[i];
        NKBCharacterCondition* condition = [[NKBCharacterCondition alloc]initWithJSON:conditionJSON];

        [_conditions addObject:condition];
    }
    
    _action = [((NSNumber*)[jsonObject_ objectForKey:@"action"])intValue];
    _actionIntValue = [((NSNumber*)[jsonObject_ objectForKey:@"actionIntValue"])intValue];
}

-(bool)areConditionsMet:(unichar)ch_
                   text:(unichar*)text_
             textLength:(int)textLength_
                    pos:(int)pos_
 selectionStartPosition:(int)selectionStartPosition_
{
    NSUInteger length = _conditions.count;
    for(int i = 0; i < length; i++)
    {
        if(![_conditions[i] isConditionMet:ch_ text:text_ textLength:textLength_ pos:pos_ selectionStartPosition:selectionStartPosition_]) { return false; }
    }
    
    return (length > 0);
}

@end
