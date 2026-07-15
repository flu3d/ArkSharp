using System;
using System.Runtime.CompilerServices;

namespace ArkSharp
{
	public static class Defer
	{
		/// <summary>
		/// 离开代码块时自动执行；多个守卫按声明的逆序执行。
		/// 示例：<c>using var _ = Defer.OnLeave(CleanUp);</c>
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Defer0 OnLeave(Action action)
		{
			return new Defer0(action);
		}

		/// <summary>
		/// 离开代码块时自动执行；多个守卫按声明的逆序执行。
		/// 示例：<c>using var _ = Defer.OnLeave(CleanUp, state);</c>
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Defer1<TState> OnLeave<TState>(Action<TState> action, TState state)
		{
			return new Defer1<TState>(action, state);
		}

		public struct Defer0 : IDisposable
		{
			private Action _action;

			public Defer0(Action action)
			{
				_action = action;
			}

			public void Dispose()
			{
				var action = _action;
				_action = null;
				action?.Invoke();
			}
		}

		public struct Defer1<TState> : IDisposable
		{
			private Action<TState> _action;
			private TState _state;

			public Defer1(Action<TState> action, TState state)
			{
				_action = action;
				_state = state;
			}

			public void Dispose()
			{
				var action = _action;
				var state = _state;

				_action = null;
				_state = default;

				action?.Invoke(state);
			}
		}
	}
}
