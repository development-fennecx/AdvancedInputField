//
//  ThreadsafeQueue.h
//  TouchScreenKeyboard
//
//  Created by Jeroen Pienbroek on 25/01/2018.
//  Copyright © 2018 Jeroen van Pienbroek. All rights reserved.
//

#ifndef NKBThreadsafeQueue_h
#define NKBThreadsafeQueue_h

#import <Foundation/Foundation.h>

@interface NKBQueueNode<T> : NSObject

-(id)initWithValue:(T) value_;

@end

@interface NKBThreadsafeQueue<T> : NSObject

-(void) enqueue:(T) item_;

-(T) dequeue;

-(BOOL) contains:(T) item_;

-(NSUInteger) getCount;

-(void) clear;

@end
#endif /* ThreadsafeQueue_h */
