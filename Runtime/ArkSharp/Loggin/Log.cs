using System.Runtime.CompilerServices;

namespace ArkSharp
{
	/// <summary>
	/// 日志等级
	/// </summary>
	public enum LogLevel
	{
		Trace,	// 调试追踪
		Info,	// 常规
		Warn,	// 警告
		Error,	// 错误或异常
	}

	/// <summary>
	/// 通用日志接口
	/// </summary>
	public static class Log
	{
		public static Logger logger = new Logger();

		public static LogLevel level 
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => logger.level;
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => logger.level = value;
		}
		
		public static bool timePrefix 
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => logger.timePrefix;
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => logger.timePrefix = value; 
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsEnabled(LogLevel logLevel) => logger.IsEnabled(logLevel);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Trace(object message) => logger.Write(LogLevel.Trace, message);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Info(object message) => logger.Write(LogLevel.Info, message);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Warn(object message) => logger.Write(LogLevel.Warn, message);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Error(object message) => logger.Write(LogLevel.Error, message);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Write(LogLevel level, object message) => logger.Write(level, message);
	}
}
