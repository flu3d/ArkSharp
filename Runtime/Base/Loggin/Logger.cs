using System;
using System.Runtime.CompilerServices;

namespace ArkSharp
{
	public interface ILogSink : IDisposable
	{
		void Write(LogLevel level, string message);
	}

	public class Logger : IDisposable
	{
		/// <summary>
		/// 默认时间戳格式，和消息间隔2个空格
		/// </summary>
		public const string DefaultTimeFormat = "HH:mm:ss.fff  ";

		/// <summary>
		/// 时间戳前缀格式
		/// </summary>
		public string TimeFormat { get; set; } = DefaultTimeFormat;

		/// <summary>
		/// 日志等级
		/// </summary>
		public LogLevel Level { get; set; }

		/// <summary>
		/// 判断日志等级是否需要写入
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsEnabled(LogLevel level) => level >= Level;

		/// <summary>
		/// 创建Logger
		/// </summary>
		/// <param name="asyncCapacity">异步日志队列容量，容量要求是2的幂次方，0则表示同步阻塞模式</param>
		public Logger(int asyncCapacity)
		{
			_sinkGroup = asyncCapacity > 0
				? new AsyncLogSinkGroup(asyncCapacity)
				: new SyncLogSinkGroup();
		}

		/// <summary>
		/// 日志持久化组
		/// </summary>
		public ILogSinkGroup Sink => _sinkGroup;
		private readonly LogSinkGroup _sinkGroup;

		public void Write(LogLevel level, object messageObj)
		{
			if (messageObj == null || level < Level)
				return;

			var message = !string.IsNullOrEmpty(TimeFormat)
				? DateTime.Now.ToString(TimeFormat) + messageObj.ToString()
				: messageObj.ToString();

			_sinkGroup?.Write(level, message);
		}

		public void Dispose()
		{
			_sinkGroup.Dispose();
		}
	}
}
