//
//  NKBNativeKeyboardConfiguration.m
//  NativeKeyboard
//
//  Created by Jeroen Pienbroek on 05/09/2019.
//  Copyright © 2019 Jeroen van Pienbroek. All rights reserved.
//

#import "NKBNativeKeyboardConfiguration.h"
#import "NKBNativeKeyboard.h"

@implementation NKBNativeKeyboardConfiguration

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
    _keyboardType = [((NSNumber*)[jsonObject_ objectForKey:@"keyboardType"])intValue];
    _characterValidation = [((NSNumber*)[jsonObject_ objectForKey:@"characterValidation"])intValue];
    _lineType = [((NSNumber*)[jsonObject_ objectForKey:@"lineType"])intValue];
    _autocapitalizationType = [((NSNumber*)[jsonObject_ objectForKey:@"autocapitalizationType"])intValue];
    _autofillType = [((NSNumber*)[jsonObject_ objectForKey:@"autofillType"])intValue];
    _returnKeyType = [((NSNumber*)[jsonObject_ objectForKey:@"returnKeyType"])intValue];
    _autocorrection = [((NSNumber*)[jsonObject_ objectForKey:@"autocorrection"])boolValue];
    _secure = [((NSNumber*)[jsonObject_ objectForKey:@"secure"])boolValue];
    _richTextEditing = [((NSNumber*)[jsonObject_ objectForKey:@"richTextEditing"])boolValue];
    _emojisAllowed = [((NSNumber*)[jsonObject_ objectForKey:@"emojisAllowed"])boolValue];
    _hasNext = [((NSNumber*)[jsonObject_ objectForKey:@"hasNext"])boolValue];
    _characterLimit = [((NSNumber*)[jsonObject_ objectForKey:@"characterLimit"])intValue];
    
    NSString* characterValidatorValue = (NSString*)[jsonObject_ objectForKey:@"characterValidatorJSON"];
    _characterValidator = nil;
    if(characterValidatorValue != nil)
    {
        NSError *jsonError;
        NSData *characterValidatorData = [characterValidatorValue dataUsingEncoding:NSUTF8StringEncoding];
        NSDictionary *jsonObject = [NSJSONSerialization JSONObjectWithData:characterValidatorData
                                                         options:NSJSONReadingMutableContainers
                                                           error:&jsonError];
        if(jsonError == nil)
        {
            _characterValidator = [[NKBCharacterValidator alloc]initWithJSON:jsonObject];
        }
    }
}

@end
