using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ArkSharp
{
	public static class ActionHelper
	{
		/// <summary>
		/// 取走并清空无参数回调。
		/// 示例：<c>ActionHelper.Take(ref callback)?.Invoke();</c>
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Action Take(ref Action action)
		{
			return Interlocked.Exchange(ref action, null);
		}

		/// <summary>
		/// 取走并清空单参数回调。
		/// 示例：<c>ActionHelper.Take(ref callback)?.Invoke(arg);</c>
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Action<T> Take<T>(ref Action<T> action)
		{
			return Interlocked.Exchange(ref action, null);
		}

		/// <summary>
		/// 取走并清空双参数回调。
		/// 示例：<c>ActionHelper.Take(ref callback)?.Invoke(arg1, arg2);</c>
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Action<T1, T2> Take<T1, T2>(ref Action<T1, T2> action)
		{
			return Interlocked.Exchange(ref action, null);
		}
	}
}
