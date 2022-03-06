//
//  TextValidator.h
//  TouchScreenKeyboard
//
//  Created by Jeroen Pienbroek on 25/01/2018.
//  Copyright © 2018 Jeroen van Pienbroek. All rights reserved.
//

#ifndef NKBTextValidator_h
#define NKBTextValidator_h

#import "NKBNativeKeyboard.h"
#import "NKBCharacterValidator.h"

typedef NS_ENUM(int, NKBCharacterValidation);
typedef NS_ENUM(int, NKBLineType);

@interface NKBTextValidator : NSObject

@property (nonatomic, assign) NKBCharacterValidation validation;
@property (nonatomic, assign) NKBLineType lineType;
@property (nonatomic, strong) NKBCharacterValidator* validator;
@property (nonatomic, strong) NSString* resultText;
@property (nonatomic, assign) int resultCaretPosition;

-(void)validate:(NSString*) text_
   textToAppend:(NSString*) textToAppend_
  caretPosition:(int) caretPosition_
selectionStartPosition:(int) selectionStartPosition_;

-(unichar)validateChar:(unichar) ch_
              text:(unichar*) text_
        textLength:(int) textLength_
               pos:(int) pos_
     caretPosition:(int) caretPosition_
selectionStartPosition:(int) selectionStartPosition_;

@end

#endif /* TextValidator_h */
