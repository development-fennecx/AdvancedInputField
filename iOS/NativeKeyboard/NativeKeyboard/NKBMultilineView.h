//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

#ifndef NKBMultilineView_h
#define NKBMultilineView_h

#import <UIKit/UIKit.h>
#import "NKBInputView.h"

@protocol NKBMultilineViewDelegate <NSObject>

-(void)textFieldDidDelete;

@end

@interface NKBMultilineView : UITextView<NKBInputView, UIKeyInput>

@property(nonatomic, assign) id<NKBMultilineViewDelegate> myDelegate;

@end
#endif
