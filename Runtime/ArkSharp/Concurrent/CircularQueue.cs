using System.Threading;
using System;

namespace ArkSharp
{
	/// <summary>
	/// 定长循环队列
	/// 支持单生产者-单消费者的线程安全
	/// </summary>
	public sealed class CircularQueue<T>
	{
		private readonly T[] _buffer;
		private readonly int _mask;

		private PaddedInt _writePos;
		private PaddedInt _readPos;

		/// <summary>
		/// 定长循环队列
		/// </summary>
		/// <param name="capacity">队列长度，必须是2的幂次方</param>
		public CircularQueue(int capacity)
		{
			if (capacity < 2 || (capacity & (capacity - 1)) != 0)
				throw new ArgumentException("Capacity must be >=2 and a power of two");

			_buffer = new T[capacity];
			_mask = capacity - 1;
		}

		/// <summary>
		/// 队列判空
		/// </summary>
		public bool IsEmpty => Volatile.Read(ref _readPos.Value) == Volatile.Read(ref _writePos.Value);

		/// <summary>
		/// 入队操作，如果队伍满则返回false
		/// </summary>
		public bool TryEnqueue(T item)
		{
			int nextPos = (_writePos.Value + 1) & _mask;
			if (nextPos == Volatile.Read(ref _readPos.Value))
				return false;

			_buffer[_writePos.Value] = item;
			Volatile.Write(ref _writePos.Value, nextPos);

			return true;
		}

		/// <summary>
		/// 出队操作，如果队伍空则返回false
		/// </summary>
		public bool TryDequeue(out T item)
		{
			int readPos = _readPos.Value;
			if (readPos == Volatile.Read(ref _writePos.Value))
			{
				item = default;
				return false;
			}

			item = _buffer[readPos];

			int nextPos = (readPos + 1) & _mask;
			Volatile.Write(ref _readPos.Value, nextPos);

			return true;
		}
	}

}
