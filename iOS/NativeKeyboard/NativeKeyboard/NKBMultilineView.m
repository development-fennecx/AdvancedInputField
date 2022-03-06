//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

#import "NKBMultilineView.h"
#import "NKBNativeKeyboard.h"

@implementation NKBMultilineView

- (id)initWithFrame:(CGRect)frame
{
    self = [super initWithFrame:frame];
    return self;
}

- (id)initWithCoder:(NSCoder *)aDecoder
{
    self = [super initWithCoder:aDecoder];
    return self;
}

- (void)drawRect:(CGRect)rect
{
    // Don't draw
}

- (void)deleteBackward
{
    [super deleteBackward];
    [_myDelegate textFieldDidDelete];
}

- (void)touchesBegan:(NSSet *)touches withEvent:(UIEvent *)event
{
}

- (void)touchesMoved:(NSSet *)touches withEvent:(UIEvent *)event
{
}

- (void)touchesEnded:(NSSet *)touches withEvent:(UIEvent *)event
{
}

-(void)changeKeyboardType:(UIKeyboardType)keyboardType_
{
    [self setKeyboardType:keyboardType_];
}

-(void)changeAutocorrectionType:(UITextAutocorrectionType)autocorrectionType_
{
    [self setAutocorrectionType:autocorrectionType_];
}

-(void)changeSecureTextEntry:(bool)secure_
{
    [self setSecureTextEntry:secure_];
}

-(void)changeAutocapitalizationType:(UITextAutocapitalizationType)autocapitalizationType_
{
    [self setAutocapitalizationType:autocapitalizationType_];
}

-(void)changeReturnKeyType:(UIReturnKeyType)returnKeyType_
{
    [self setReturnKeyType:returnKeyType_];
}

-(void)changeText:(NSString *)text_
{
    [self setText:text_];
}

-(void)changeSelection:(int)start_ end:(int)end_
{
    UITextPosition *startPosition = [self positionFromPosition:self.beginningOfDocument offset:start_];
    UITextPosition *endPosition = [self positionFromPosition:self.beginningOfDocument offset:end_];
    self.selectedTextRange = [self textRangeFromPosition:startPosition toPosition:endPosition];
}

-(void)requestFocus
{
    [self becomeFirstResponder];
}

-(void)loseFocus
{
    [self resignFirstResponder];
}

-(void)refreshInputViews
{
    [self reloadInputViews];
}

-(UITextInputAssistantItem*)getShortcut
{
    return [self inputAssistantItem];
}

-(UIView*)getAccesoryView
{
    return self.inputAccessoryView;
}

-(NSString*)getText
{
    return self.text;
}

-(NKBAutofillType)getAutofillType
{
    return AFT_NONE;
}

-(NSUInteger) getHashCode
{
    return self.hash;
}
@end
