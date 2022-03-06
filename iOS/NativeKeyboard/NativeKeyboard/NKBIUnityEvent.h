//
//  IUnityEvent.h
//  TouchScreenKeyboard
//
//  Created by Jeroen Pienbroek on 28/01/2018.
//  Copyright © 2018 Jeroen van Pienbroek. All rights reserved.
//

#ifndef NKBIUnityEvent_h
#define NKBIUnityEvent_h

#import <Foundation/Foundation.h>

typedef NS_ENUM(int, NKBEventType);

@protocol NKBIUnityEvent

-(NKBEventType)getType;

@end

#endif /* IUnityEvent_h */
