//
//  NSString+Extensions.m
//  NativeKeyboard
//
//  Created by Jeroen Pienbroek on 19/03/2018.
//  Copyright © 2018 Jeroen van Pienbroek. All rights reserved.
//

#import "NKBNSString+Extensions.h"

@implementation NSString (Extensions)

+(NSString*) fromUnityString:(const char*) unityString_
{
    if (unityString_ == nil) return [NSString new];
    return [NSString stringWithUTF8String:unityString_];
}

-(char*) toUnityString
{
    const char* cString = self.UTF8String;
    char* _unityString = (char*)malloc(strlen(cString) + 1);
    strcpy(_unityString, cString);
    return _unityString;
}

-(unichar) charAt:(int) index_
{
    return [self characterAtIndex:index_];
}

-(int) indexOf:(unichar) c_
{
    NSString* charString = [NSString stringWithFormat:@"%C" , c_];
    NSUInteger location = [self rangeOfString:charString].location;
    if(location == NSNotFound){ return -1; }
    return (int)location;
}

-(int) lastIndexOf:(unichar) c_
{
    NSString* charString = [NSString stringWithFormat:@"%C" , c_];
    NSUInteger location = [self rangeOfString:charString options:NSBackwardsSearch].location;
    if(location == NSNotFound){ return -1; }
    return (int)location;
}

-(int) countOccurences:(unichar) c_
{
    int occurences = 0;
    
    NSUInteger length = self.length;
    for(int i = 0; i < length; i++)
    {
        if([self characterAtIndex:i] == c_)
        {
            occurences++;
        }
    }
    
    return occurences;
}

@end
