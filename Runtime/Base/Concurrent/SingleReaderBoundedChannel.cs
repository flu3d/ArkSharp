using System;
using System.Threading;

namespace ArkSharp
{
	/// <summary>
	/// 线程安全的单消费者定长队列
	/// </summary>
	public sealed class SingleReaderBoundedChannel<T> : ISingleReaderChannel<T>, IDisposable
	{
		private readonly CircularQueue<T> _queue;
		private readonly object _syncRoot = new object();
		private volatile bool _isClosed;

		public bool IsClosed => _isClosed;

		public SingleReaderBoundedChannel(int capacity)
		{
			_queue = new CircularQueue<T>(capacity);
		}

		/// <summary>
		/// 尝试写入队列，如果队列关闭或队列满则返回false
		/// </summary>
		public bool Write(T item)
		{
			lock (_syncRoot)
			{
				if (_isClosed)
					return false;

				if (!_queue.TryEnqueue(item))
					return false;

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
				while (true)
				{
					if (_queue.TryDequeue(out item))
						return true;

					if (_isClosed)
						return false;

					Monitor.Wait(_syncRoot);
				}
			}
		}

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
