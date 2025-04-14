using System.Collections.Generic;
using System.Threading;

namespace ArkSharp
{
	/// <summary>
	/// 日志持久化组
	/// </summary>
	public interface ILogSinkGroup
	{
		void Add(ILogSink sink);
		void Remove(ILogSink sink);
		void RemoveAll();
	}

	public abstract class LogSinkGroup : ILogSink, ILogSinkGroup
	{
		public abstract void Write(LogLevel level, string message);

		public virtual void Dispose() => RemoveAll();

		protected void WriteSink(LogLevel level, string message)
		{
			lock (_sinks)
			{
				for (int i = 0; i < _sinks.Count; i++)
				{
					try
					{
						_sinks[i]?.Write(level, message);
					}
					catch
					{
						// 忽略持久化异常
					}
				}
			}
		}

		public virtual void Add(ILogSink sink)
		{
			if (sink == null)
				return;

			lock (_sinks)
			{
				_sinks.AddUnique(sink);
			}
		}

		public virtual void Remove(ILogSink sink)
		{
			if (sink == null)
				return;

			lock (_sinks)
			{
				if (_sinks.Remove(sink))
					sink.Dispose();
			}
		}

		public virtual void RemoveAll()
		{
			lock (_sinks)
			{
				for (int i = 0; i < _sinks.Count; i++)
					_sinks[i]?.Dispose();

				_sinks.Clear();
			}
		}

		protected readonly List<ILogSink> _sinks = new List<ILogSink>();
	}

	/// <summary>
	/// 同步日志持久化组
	/// </summary>
	public class SyncLogSinkGroup : LogSinkGroup
	{
		public override void Write(LogLevel level, string message)
		{
			// 直接复用_sink锁，支持多线程写入
			WriteSink(level, message);
		}
	}

	/// <summary>
	/// 异步日志持久化组
	/// </summary>
	public class AsyncLogSinkGroup : LogSinkGroup
	{
		struct LogItem
		{
			public LogLevel level;
			public string message;
		}

		private readonly SingleReaderBoundedChannel<LogItem> _channel;
		private readonly Thread _workerThread;

		private long _discardedCount;

		/// <summary>
		/// 获取被丢弃消息数量的属性
		/// </summary>
		public long DiscardedCount => Interlocked.Read(ref _discardedCount);

		public AsyncLogSinkGroup(int capacity = 1024)
		{
			_channel = new SingleReaderBoundedChannel<LogItem>(capacity);

			_workerThread = new Thread(ProcessItems) { IsBackground = true };
			_workerThread.Start();
		}

		public override void Write(LogLevel level, string message)
		{
			if (!_channel.Write(new LogItem { level = level, message = message }))
			{
				// 记录丢弃数量
				Interlocked.Increment(ref _discardedCount);
			}
		}

		// 后台线程从日志队列取出并做持久化
		private void ProcessItems()
		{
			while (true)
			{
				// 阻塞读取数据，如果队列关闭则结束
				if (!_channel.Read(out var m))
					break; 

				// 日志持久化
				WriteSink(m.level, m.message);
			}
		}

		public override void Dispose()
		{
			_channel.Close();
			_workerThread.Join();

			RemoveAll();
		}
	}
}
