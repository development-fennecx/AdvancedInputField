package com.jeroenvanpienbroek.nativekeyboard;

import java.util.Queue;
import java.util.concurrent.LinkedBlockingQueue;

/** Class that wraps Queue in a threadsafe way */
public class ThreadsafeQueue<T>
{
    private Queue<T> queue;
    private Object queueLock = new Object();

    public ThreadsafeQueue()
    {
        this.queue = new LinkedBlockingQueue<T>();
    }

    public void enqueue(T item)
    {
        synchronized (queueLock)
        {
            this.queue.add(item);
        }
    }

    public T dequeue()
    {
        synchronized (queueLock)
        {
            return this.queue.poll();
        }
    }

    public boolean contains(T item)
    {
        synchronized (queueLock)
        {
            return this.queue.contains(item);
        }
    }

    public int getCount()
    {
        synchronized (queueLock)
        {
            return this.queue.size();
        }
    }

    public void clear()
    {
        synchronized (queueLock)
        {
            this.queue.clear();
        }
    }
}