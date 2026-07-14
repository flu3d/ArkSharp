using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ArkSharp
{
	/// <summary>
	/// 线程安全的单消费者不定长队列
	/// </summary>
	public sealed class SingleReaderUnboundedChannel<T> : ISingleReaderChannel<T>, IDisposable
	{
		private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
		private readonly object _syncRoot = new object();
		private volatile bool _isClosed;

		public bool IsClosed => _isClosed;

		/// <summary>
		/// 尝试写入队列，如果队列关闭则返回false
		/// </summary>
		public bool Write(T item)
		{
			if (_isClosed)
				return false;

			_queue.Enqueue(item);

			lock (_syncRoot)
			{
				Monitor.Pulse(_syncRoot);
			}

			return true;
		}

		/// <summary>
		/// 阻塞读取数据，直到队列关闭且无数据则返回false
		/// </summary>
		public bool Read(out T item)
		{
			while (true)
			{
				if (_queue.TryDequeue(out item))
					return true;

				if (_isClosed)
					return false;

				lock (_syncRoot)
				{
					Monitor.Wait(_syncRoot);
				}
			}
		}

		public void Close()
		{
			if (_isClosed)
				return;

			_isClosed = true;

			lock (_syncRoot)
			{
				Monitor.Pulse(_syncRoot);
			}
		}

		public void Dispose() => Close();
	}
}
