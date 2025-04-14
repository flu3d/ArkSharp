using System.Runtime.CompilerServices;
using ConditionalAttribute = System.Diagnostics.ConditionalAttribute;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace ArkSharp
{
	/// <summary>
	/// 日志等级
	/// </summary>
	public enum LogLevel
	{
		Trace,  // 调试追踪，默认仅在编辑器或开发环境生效
		Info,   // 常规
		Warn,   // 警告
		Error,  // 错误或异常
	}

	/// <summary>
	/// 全局通用日志接口
	/// Unity下直接转发给UnityLogger，再由默认Logger统一捕获
	/// 非Unity环境直接转发给默认Logger
	/// </summary>
	public static class Log
	{
		/// <summary>
		/// 默认为异步日志器，队列容量为1024
		/// </summary>
		public static readonly Logger Default = new Logger(1024);

		/// <summary>
		/// 日志等级，设置时会与Unity日志保持同步
		/// </summary>
		public static LogLevel Level
		{
			get => Default.Level;
			set
			{
				Default.Level = value;

#if UNITY_5_3_OR_NEWER
				switch (value)
				{
					case LogLevel.Trace:
						Debug.unityLogger.filterLogType = LogType.Log;
						break;
					case LogLevel.Info:
						Debug.unityLogger.filterLogType = LogType.Log;
						break;
					case LogLevel.Warn:
						Debug.unityLogger.filterLogType = LogType.Warning;
						break;
					case LogLevel.Error:
						Debug.unityLogger.filterLogType = LogType.Error;
						break;
					default:
						break;
				}
#endif
			}
		}

		/// <summary>
		/// 日志持久化组
		/// Log.Sink.ToFile("xxxx.log").ToConsole();
		/// </summary>
		public static ILogSinkGroup Sink => Default.Sink;

		/// <summary>
		/// 判断日志等级是否需要写入
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsEnabled(LogLevel logLevel) => Default.IsEnabled(logLevel);

#if UNITY_5_3_OR_NEWER
		// Unity下直接转发给UnityLogger，再统一捕获交给默认Logger

		[Conditional("UNITY_EDITOR")]
		[Conditional("DEVELOPMENT_BUILD")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static void Trace(object message) => Debug.Log(message);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static void Info(object message) => Debug.Log(message);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static void Warn(object message) => Debug.LogWarning(message);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static void Error(object message) => Debug.LogError(message);

		static Log()
		{
			Application.logMessageReceivedThreaded += UnityLogCallback;
		}

		// 捕获Unity日志输出到默认日志实例
		private static void UnityLogCallback(string message, string stackTrace, LogType type)
		{
			switch (type)
			{
				case LogType.Error:
				case LogType.Assert:
				case LogType.Exception:
					if (string.IsNullOrEmpty(stackTrace))
						Default.Write(LogLevel.Error, message);
					else
						Default.Write(LogLevel.Error, $"{message}\n{stackTrace}");
					break;
				case LogType.Warning:
					Default.Write(LogLevel.Warn, message);
					break;
				case LogType.Log:
					Default.Write(LogLevel.Info, message);
					break;
				default:
					break;
			}
		}
#else
		// 非Unity环境直接转发给默认Logger

		[Conditional("DEBUG")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static void Trace(object message) => Default.Write(LogLevel.Trace, message);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static void Info(object message) => Default.Write(LogLevel.Info, message);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static void Warn(object message) => Default.Write(LogLevel.Warn, message);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static void Error(object message) => Default.Write(LogLevel.Error, message);
#endif
	}
}
