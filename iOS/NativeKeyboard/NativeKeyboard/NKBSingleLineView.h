//
//  NKBSingleLineView.h
//  NativeKeyboard
//
//  Created by Jeroen Pienbroek on 05/01/2020.
//  Copyright © 2020 Jeroen van Pienbroek. All rights reserved.
//

#ifndef NKBSingleLineView_h
#define NKBSingleLineView_h

#import <UIKit/UIKit.h>
#import "NKBInputView.h"

@protocol NKBSingleLineViewDelegate <NSObject>

-(void)textFieldDidDelete;

@end

@interface NKBSingleLineView : UITextField<NKBInputView, UIKeyInput>

@property(nonatomic, assign) id<NKBSingleLineViewDelegate> myDelegate;
@property(nonatomic, assign) NKBAutofillType autofillType;

@end
#endif
