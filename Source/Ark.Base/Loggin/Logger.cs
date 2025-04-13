using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace Ark
{
	public interface ILogTarget
	{
		void Write(LogLevel level, string message);
		void Close();
	}

	public class Logger
	{
		public bool timePrefix { get; set; } = false;

		private LogLevel _level = LogLevel.Info;
		public LogLevel level 
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _level;
			set {
				_level = value;

#if UNITY_5_3_OR_NEWER
				switch (value)
				{
					case LogLevel.Trace: Debug.unityLogger.filterLogType = LogType.Log; break;
					case LogLevel.Info: Debug.unityLogger.filterLogType = LogType.Log; break;
					case LogLevel.Warn: Debug.unityLogger.filterLogType = LogType.Warning; break;
					case LogLevel.Error: Debug.unityLogger.filterLogType = LogType.Error; break;
					default:
						break;
				}
#endif
			}
		}

		private readonly object _syncObj = new object();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsEnabled(LogLevel level) => _level <= level;

		public Logger()
		{
#if UNITY_5_3_OR_NEWER
			Application.logMessageReceivedThreaded += UnityLogCallback;
#endif
		}

		public void Close()
		{
#if UNITY_5_3_OR_NEWER
			Application.logMessageReceivedThreaded -= UnityLogCallback;
#endif
			RemoveAllTargets();
		}

		public virtual void Write(LogLevel level, object messageObj)
		{
			if (messageObj == null || this.level > level)
				return; ;

			var message = timePrefix 
				? $"[{DateTime.Now:HH:mm:ss.fff}] {messageObj}"
				: messageObj.ToString();

#if UNITY_5_3_OR_NEWER
			switch (level)
			{
				case LogLevel.Trace: Debug.Log(message); break;
				case LogLevel.Info: Debug.Log(message); break;
				case LogLevel.Warn: Debug.LogWarning(message); break;
				case LogLevel.Error: Debug.LogError(message); break;
				default:
					break;
			}
#else
			WriteTargets(level, message);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void WriteTargets(LogLevel level, string message)
		{
			lock (_syncObj)
			{
				foreach (var target in _targets)
					target?.Write(level, message);
			}
		}

#if UNITY_5_3_OR_NEWER
		private void UnityLogCallback(string message, string stackTrace, LogType type)
		{
			switch (type)
			{
				case LogType.Error:
				case LogType.Assert:
				case LogType.Exception:
					if (string.IsNullOrEmpty(stackTrace))
						WriteTargets(LogLevel.Error, message);
					else
						WriteTargets(LogLevel.Error, $"{message}\n{stackTrace}");
					break;
				case LogType.Warning:
					WriteTargets(LogLevel.Warn, message);
					break;
				case LogType.Log:
					WriteTargets(LogLevel.Info, message);
					break;
				default:
					break;
			}
		}
#endif

#region LogTargets
		protected readonly List<ILogTarget> _targets = new List<ILogTarget>();

		public void AddTarget(ILogTarget target)
		{
			if (target == null)
				return;

			lock (_syncObj)
			{
				_targets.AddUnique(target);
			}
		}

		public void RemoveTarget(ILogTarget target)
		{
			if (target == null)
				return;

			lock (_syncObj)
			{
				target.Close();
				_targets.Remove(target);
			}
		}

		public void RemoveAllTargets()
		{
			lock (_syncObj)
			{
				foreach (var target in _targets)
					target?.Close();

				_targets.Clear();
			}
		}
#endregion
	}
}
