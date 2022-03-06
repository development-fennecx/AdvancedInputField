//
//  Util.m
//  NativeKeyboard
//
//  Created by Jeroen Pienbroek on 19/03/2018.
//  Copyright © 2018 Jeroen van Pienbroek. All rights reserved.
//

#import "NKBUtil.h"
#import <Foundation/Foundation.h>
#import <CoreFoundation/CoreFoundation.h>
#import <CoreGraphics/CoreGraphics.h>
#import <CoreText/CoreText.h>

@implementation NKBUtil

+(CFMutableCharacterSetRef)emojiCharacterSet
{
    static CFMutableCharacterSetRef set = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        set = CFCharacterSetCreateMutableCopy(kCFAllocatorDefault, CTFontCopyCharacterSet(CTFontCreateWithName(CFSTR("AppleColorEmoji"), 0.0, NULL)));
        CFCharacterSetRemoveCharactersInString(set, CFSTR(" 0123456789#*"));
    });
    return set;
}

+(bool)containsEmoji:(NSString *)emoji
{
    if([emoji isEqualToString:@"\n"]){return false;}
    return CFStringFindCharacterFromSet((CFStringRef)emoji, [self emojiCharacterSet], CFRangeMake(0, emoji.length), 0, NULL);
}

+(CGFloat) pixelToPoints:(CGFloat) px_
{
    CGFloat pointsPerInch = 72.0; // see: http://en.wikipedia.org/wiki/Point%5Fsize#Current%5FDTP%5Fpoint%5Fsystem
    CGFloat scale = [UIScreen mainScreen].scale;
    float pixelPerInch; // DPI
    if (UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPad)
    {
        pixelPerInch = 132 * scale;
    }
    else if (UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPhone)
    {
        pixelPerInch = 163 * scale;
    }
    else
    {
        pixelPerInch = 160 * scale;
    }
    CGFloat points = (px_ / pixelPerInch * pointsPerInch) / scale;
    return points;
}

+(CGFloat) pointsToPixels:(CGFloat) points_
{
    CGFloat pointsPerInch = 72.0; // see: http://en.wikipedia.org/wiki/Point%5Fsize#Current%5FDTP%5Fpoint%5Fsystem
    CGFloat scale = [UIScreen mainScreen].scale;
    float pixelPerInch; // DPI
    if (UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPad)
    {
        pixelPerInch = 132 * scale;
    }
    else if (UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPhone)
    {
        pixelPerInch = 163 * scale;
    }
    else
    {
        pixelPerInch = 160 * scale;
    }
    CGFloat px = (points_ / pointsPerInch * pixelPerInch) / scale;
    return px;
}

+(NSInteger) clamp:(NSInteger) value_
min:(NSInteger) min_
max:(NSInteger) max_
{
    value_ = fmax(value_, min_);
    value_ = fmin(value_, max_);
    
    return value_;
}

+(void) runOnMainThread:(void (^)(void)) block_
                  delay:(float) delay_;
{
    dispatch_after(dispatch_time(DISPATCH_TIME_NOW, delay_ * NSEC_PER_SEC), dispatch_get_main_queue(), block_);
}

+(bool) contains:(unichar)ch_
            text:(unichar*) text_
      textLength:(int)textLength_
{
    for(int i = 0; i < textLength_; i++)
    {
        if(text_[i] == ch_) { return true; }
    }
    
    return false;
}

+(int) indexOf:(unichar)ch_
          text:(unichar*)text_
    textLength:(int)textLength_
{
    for(int i = 0; i < textLength_; i++)
    {
        if(text_[i] == ch_) { return i; }
    }
    
    return -1;
}

+(int) lastIndexOf:(unichar)ch_
              text:(unichar*)text_
        textLength:(int)textLength_
{
    for(int i = textLength_ - 1; i >= 0; i--)
    {
        if(text_[i] == ch_) { return i; }
    }
    
    return -1;
}

+(int) countOccurences:(unichar)ch_
                  text:(unichar*)text_
            textLength:(int) textLength_
{
    int occurences = 0;
    
    for(int i = 0; i < textLength_; i++)
    {
        if(text_[i] == ch_)
        {
            occurences++;
        }
    }
    
    return occurences;
}

@end
