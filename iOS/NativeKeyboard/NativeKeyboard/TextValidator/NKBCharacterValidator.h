//
//  CharacterValidator.h
//  NativeKeyboard
//
//  Created by Jeroen Pienbroek on 30/10/2018.
//  Copyright © 2018 Jeroen van Pienbroek. All rights reserved.
//

#ifndef NKBCharacterValidator_h
#define NKBCharacterValidator_h

#import <Foundation/Foundation.h>
#import "NKBCharacterRule.h"

@interface NKBCharacterValidator : NSObject

@property(nonatomic, strong) NSMutableArray* rules;
@property(nonatomic, assign) NKBCharacterAction otherCharacterAction;
@property(nonatomic, assign) int otherCharacterActionIntValue;

-(id)initWithJSON:(NSDictionary*)jsonObject_;

-(void)parseJSON:(NSDictionary*)jsonObject_;

-(unichar)validate:(unichar)ch_
              text:(unichar*)text_
        textLength:(int)textLength_
               pos:(int)pos_
 selectionStartPosition:(int)selectionStartPosition_;

-(unichar)executeAction:(unichar)ch_ action:(NKBCharacterAction)action_ actionValue:(int)actionValue_;

@end
#endif
