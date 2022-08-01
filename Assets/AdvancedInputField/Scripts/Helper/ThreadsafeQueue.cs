using System.Collections.Generic;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Class that wraps ThreadsafeQueue in a threadsafe way</summary>
	public class ThreadsafeQueue<T>
	{
		private Queue<T> queue;
		private object queueLock = new object();

		public ThreadsafeQueue()
		{
			this.queue = new Queue<T>();
		}

		public ThreadsafeQueue(int capacity)
		{
			this.queue = new Queue<T>(capacity);
		}

		public void Enqueue(T item)
		{
			lock(queueLock)
			{
				this.queue.Enqueue(item);
			}
		}

		public T Dequeue()
		{
			lock(queueLock)
			{
				return this.queue.Dequeue();
			}
		}

		public bool Contains(T item)
		{
			lock(queueLock)
			{
				return this.queue.Contains(item);
			}
		}

		public int Count
		{
			get
			{
				lock(queueLock)
				{
					return this.queue.Count;
				}
			}
		}

		public void Clear()
		{
			lock(queueLock)
			{
				this.queue.Clear();
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			lock(queueLock)
			{
				return this.queue.GetEnumerator();
			}
		}
	}
}
