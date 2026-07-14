using System;
using System.Collections.Generic;
using System.Threading;

namespace ArkSharp
{
	/// <summary>
	/// 线程安全的单消费者不定长队列
	/// </summary>
	public sealed class SingleReaderUnboundedChannel<T> : ISingleReaderChannel<T>, IDisposable
	{
		private readonly Queue<T> _queue = new Queue<T>();
		private readonly object _syncRoot = new object();
		private volatile bool _isClosed;

		public bool IsClosed => _isClosed;

		/// <summary>
		/// 尝试写入队列，如果队列关闭则返回false
		/// </summary>
		public bool Write(T item)
		{
			lock (_syncRoot)
			{
				if (_isClosed)
					return false;

				_queue.Enqueue(item);
				Monitor.Pulse(_syncRoot);
				return true;
			}
		}

		/// <summary>
		/// 阻塞读取数据，直到队列关闭且无数据则返回false
		/// </summary>
		public bool Read(out T item)
		{
			lock (_syncRoot)
			{
				while (_queue.Count == 0)
				{
					if (_isClosed)
					{
						item = default;
						return false;
					}

					Monitor.Wait(_syncRoot);
				}

				item = _queue.Dequeue();
				return true;
			}
		}

		/// <summary>
		/// 关闭队列，不再接收新数据，已有数据可以继续读取直到队列空。
		/// </summary>
		public void Close()
		{
			lock (_syncRoot)
			{
				if (_isClosed)
					return;

				_isClosed = true;
				Monitor.PulseAll(_syncRoot);
			}
		}

		public void Dispose() => Close();
	}
}
