//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

#import "NKBThreadsafeQueue.h"

@interface NKBQueueNode()
@property (nonatomic, strong) id value;
@property (nonatomic, strong) NKBQueueNode* next;
@property (nonatomic, strong) NKBQueueNode* previous;
@end

@implementation NKBQueueNode

-(id)initWithValue:(id) value_
{
    self = [super init];
    
    if(self)
    {
        self.value = value_;
    }
    
    return self;}

@end

@interface NKBThreadsafeQueue ()

@property (nonatomic, strong) NKBQueueNode* node;
@property (nonatomic, strong) NSObject* queueLock;
@property (nonatomic, strong) NKBQueueNode* firstNode;
@property (nonatomic, strong) NKBQueueNode* lastNode;
@property (nonatomic, assign) NSUInteger count;


@end

@implementation NKBThreadsafeQueue

@synthesize queueLock = _queueLock;
@synthesize firstNode = _firstNode;
@synthesize lastNode = _lastNode;
@synthesize count = _count;

-(void) enqueue:(id) item_
{
    @synchronized (_queueLock)
    {
        NKBQueueNode* itemNode = [[NKBQueueNode alloc] initWithValue:item_];
        
        if(_firstNode == nil)
        {
            _firstNode = itemNode;
            _lastNode = itemNode;
            _count = 1;
        }
        else
        {
            _lastNode.next = itemNode;
            itemNode.previous = _lastNode;
            
            _lastNode = itemNode;
            _count++;
        }
    }
}

-(id) dequeue
{
    @synchronized (_queueLock)
    {
        if(_firstNode == nil){ return nil; }
        
        NKBQueueNode* itemNode = _firstNode;
        if(_firstNode.next == nil)
        {
            _firstNode = nil;
            _lastNode = nil;
            _count = 0;
        }
        else
        {
            _firstNode = _firstNode.next;
            _firstNode.previous = nil;
            _count--;
        }
        return itemNode.value;
    }
}

-(BOOL) contains:(id) item_
{
    @synchronized (_queueLock)
    {
        if(_count == 0){ return false; }
        
        NKBQueueNode* node = _firstNode;
        while(node != nil)
        {
            if(node.value == item_)
            {
                return true;
            }
            
            node = node.next;
        }
        
        return false;
    }
}

-(NSUInteger) getCount
{
    @synchronized (_queueLock)
    {
        return _count;
    }
}

-(void) clear
{
    @synchronized (_queueLock)
    {
        for(int i = 0; i < _count; i++)
        {
            NKBQueueNode* previousNode = _lastNode.previous;
            _lastNode.previous = nil;
            
            if(previousNode == nil)
            {
                _lastNode = nil;
            }
            else
            {
                previousNode.next = nil;
                _lastNode = previousNode;
            }
        }
        
        _firstNode = nil;
        _count = 0;
    }
}


@end
