//
//  CharacterValidator.m
//  NativeKeyboard
//
//  Created by Jeroen Pienbroek on 30/10/2018.
//  Copyright © 2018 Jeroen van Pienbroek. All rights reserved.
//

#import "NKBCharacterValidator.h"

@implementation NKBCharacterValidator

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
    NSArray* rulesJSON = (NSArray*)[jsonObject_ objectForKey:@"rules"];
    _rules = [[NSMutableArray alloc]init];
    
    NSUInteger length = rulesJSON.count;
    for(int i = 0; i < length; i++)
    {
        NSDictionary* ruleJSON = rulesJSON[i];
        NKBCharacterRule* rule = [[NKBCharacterRule alloc]initWithJSON:ruleJSON];
        
        [_rules addObject:rule];
    }
    
    _otherCharacterAction = [((NSNumber*)[jsonObject_ objectForKey:@"otherCharacterAction"])intValue];
    _otherCharacterActionIntValue = [((NSNumber*)[jsonObject_ objectForKey:@"otherCharacterActionIntValue"])intValue];
}

-(unichar)validate:(unichar)ch_
              text:(unichar*)text_
        textLength:(int)textLength_
               pos:(int)pos_
selectionStartPosition:(int)selectionStartPosition_;
{
    NSUInteger length = _rules.count;
    for(int i = 0; i < length; i++)
    {
        NKBCharacterRule* rule = _rules[i];
        if([rule areConditionsMet:ch_ text:text_ textLength:textLength_ pos:pos_ selectionStartPosition:selectionStartPosition_])
        {
            return [self executeAction:ch_ action:rule.action actionValue:rule.actionIntValue];
        }
    }
    
    return [self executeAction:ch_ action:_otherCharacterAction actionValue:_otherCharacterActionIntValue];
}

-(unichar)executeAction:(unichar)ch_ action:(NKBCharacterAction)action_ actionValue:(int)actionValue_
{
    switch(action_)
    {
        case NKB_ALLOW: return ch_;
        case NKB_BLOCK: return (unichar)0;
        case NKB_TO_LOWERCASE:
            if([[NSCharacterSet lowercaseLetterCharacterSet] characterIsMember: ch_]) { return ch_; }
            else { return tolower(ch_); }
        case NKB_TO_UPPERCASE:
            if([[NSCharacterSet uppercaseLetterCharacterSet] characterIsMember: ch_]) { return ch_; }
            else { return toupper(ch_); }
        case NKB_REPLACE: return (unichar)actionValue_;
    }
    
    return ch_;
}

@end
