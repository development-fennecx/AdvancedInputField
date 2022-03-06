//
//  NKBInputView.h
//  NativeKeyboard
//
//  Created by Jeroen Pienbroek on 05/01/2020.
//  Copyright © 2020 Jeroen van Pienbroek. All rights reserved.
//

#ifndef NKBInputView_h
#define NKBInputView_h

#import <Foundation/Foundation.h>

typedef NS_ENUM(NSInteger, NKBAutofillType);
 
@protocol NKBInputView
-(void)changeKeyboardType:(UIKeyboardType)keyboardType_;
-(void)changeAutocorrectionType:(UITextAutocorrectionType)autocorrectionType_;
-(void)changeSecureTextEntry:(bool)secure_;
-(void)changeAutocapitalizationType:(UITextAutocapitalizationType)autocapitalizationType_;
-(void)changeReturnKeyType:(UIReturnKeyType)returnKeyType_;
-(void)changeText:(NSString*)text_;
-(void)changeSelection:(int)start_ end:(int)end_;
-(void)requestFocus;
-(void)loseFocus;
-(void)refreshInputViews;
-(UITextInputAssistantItem*)getShortcut;
-(UIView*)getAccesoryView;
-(NSString*)getText;
-(NKBAutofillType)getAutofillType;
-(NSUInteger) getHashCode;
@end
 
@interface NKBInputView : NSObject
 
@end

#endif /* NKBInputView_h */
