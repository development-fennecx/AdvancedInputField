//
//  NKBNativeKeyboardConfiguration.h
//  NativeKeyboard
//
//  Created by Jeroen Pienbroek on 05/09/2019.
//  Copyright © 2019 Jeroen van Pienbroek. All rights reserved.
//

#ifndef NKBNativeKeyboardConfiguration_h
#define NKBNativeKeyboardConfiguration_h

#import <Foundation/Foundation.h>
#import "NKBCharacterValidator.h"

typedef NS_ENUM(int, NKBKeyboardType);
typedef NS_ENUM(int, NKBCharacterValidation);
typedef NS_ENUM(int, NKBLineType);
typedef NS_ENUM(int, NKBAutocapitalizationType);
typedef NS_ENUM(NSInteger, NKBAutofillType);
typedef NS_ENUM(int, NKBReturnKeyType);

@interface NKBNativeKeyboardConfiguration : NSObject

@property(nonatomic, assign) NKBKeyboardType keyboardType;
@property(nonatomic, assign) NKBCharacterValidation characterValidation;
@property(nonatomic, assign) NKBLineType lineType;
@property(nonatomic, assign) NKBAutocapitalizationType autocapitalizationType;
@property(nonatomic, assign) NKBAutofillType autofillType;
@property(nonatomic, assign) NKBReturnKeyType returnKeyType;
@property(nonatomic, assign) bool autocorrection;
@property(nonatomic, assign) bool secure;
@property(nonatomic, assign) bool richTextEditing;
@property(nonatomic, assign) bool emojisAllowed;
@property(nonatomic, assign) bool hasNext;
@property(nonatomic, assign) int characterLimit;
@property(nonatomic, strong) NKBCharacterValidator* characterValidator;

-(id)initWithJSON:(NSDictionary*)jsonObject_;

-(void)parseJSON:(NSDictionary*)jsonObject_;

@end
#endif
