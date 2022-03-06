//
//  NSString+Extensions.h
//  NativeKeyboard
//
//  Created by Jeroen Pienbroek on 19/03/2018.
//  Copyright © 2018 Jeroen van Pienbroek. All rights reserved.
//

#ifndef NKBNSString_Extensions_h
#define NKBNSString_Extensions_h
#import <Foundation/Foundation.h>

@interface NSString (Extensions)

+(NSString*) fromUnityString:(const char*) unityString_;
-(char*) toUnityString;
-(unichar) charAt:(int) index_;
-(int) indexOf:(unichar) c_;
-(int) lastIndexOf:(unichar) c_;
-(int) countOccurences:(unichar) c_;

@end
#endif
