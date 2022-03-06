//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

#ifndef NKBUtil_h
#define NKBUtil_h
#import <Foundation/Foundation.h>
#import <CoreGraphics/Coregraphics.h>
#import <UIKit/UIKit.h>
#import <math.h>

@interface NKBUtil: NSObject

+(CFMutableCharacterSetRef)emojiCharacterSet;
+(bool)containsEmoji:(NSString *)emoji;
+(CGFloat) pixelToPoints:(CGFloat) px_;
+(CGFloat) pointsToPixels:(CGFloat) points_;
+(NSInteger) clamp:(NSInteger) value_
               min:(NSInteger) min_
               max:(NSInteger) max_;
+(void) runOnMainThread:(void (^)(void)) block_
                        delay:(float) delay_;
+(bool) contains:(unichar)ch_
            text:(unichar*) text_
      textLength:(int)textLength_;
+(int) indexOf:(unichar)ch_
          text:(unichar*)text_
    textLength:(int)textLength_;
+(int) lastIndexOf:(unichar)ch_
              text:(unichar*)text_
        textLength:(int)textLength_;
+(int) countOccurences:(unichar)ch_
                  text:(unichar*)text_
            textLength:(int) textLength_;

@end
#endif
