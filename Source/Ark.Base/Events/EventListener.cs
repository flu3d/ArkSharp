using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ark
{
	/// <summary>
	/// 事件监听器接口
	/// </summary>
	public interface IEventListener
	{
		void Add(Delegate callback);

		void Remove(Delegate callback);
		void RemoveTarget(object target);
		void RemoveAll();

		int Count { get; }

		/// <summary>
		/// 通用回调触发
		/// 无参数: args==null
		/// 1个参数：args==参数值
		/// 2个或更多参数: args==Tuple<T1,T2,T3...>
		/// </summary>
		void InvokeWith(object args);
	}

	public class EventListener : EventListenerBase<Action>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override void InvokeWith(object args) => Invoke();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Invoke()
		{
			var invokeList = GetInvocationList();
			for (int i = 0; i < invokeList.Count; i++)
				invokeList[i]?.Invoke();
		}
	}

	public class EventListener<T1> : EventListenerBase<Action<T1>>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override void InvokeWith(object args) => Invoke((T1)args);  // 单个参数直接存args，不放Tuple

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Invoke(T1 p1)
		{
			var invokeList = GetInvocationList();
			for (int i = 0; i < invokeList.Count; i++)
				invokeList[i]?.Invoke(p1);
		}
	}

	public class EventListener<T1, T2> : EventListenerBase<Action<T1, T2>>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override void InvokeWith(object args)
		{
			var p = (Tuple<T1, T2>)args;
			Invoke(p.Item1, p.Item2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Invoke(T1 p1, T2 p2)
		{
			var invokeList = GetInvocationList();
			for (int i = 0; i < invokeList.Count; i++)
				invokeList[i]?.Invoke(p1, p2);
		}
	}

	public class EventListener<T1, T2, T3> : EventListenerBase<Action<T1, T2, T3>>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override void InvokeWith(object args)
		{
			var p = (Tuple<T1, T2, T3>)args;
			Invoke(p.Item1, p.Item2, p.Item3);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Invoke(T1 p1, T2 p2, T3 p3)
		{
			var invokeList = GetInvocationList();
			for (int i = 0; i < invokeList.Count; i++)
				invokeList[i]?.Invoke(p1, p2, p3);
		}
	}

	public class EventListener<T1, T2, T3, T4> : EventListenerBase<Action<T1, T2, T3, T4>>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override void InvokeWith(object args)
		{
			var p = (Tuple<T1, T2, T3, T4>)args;
			Invoke(p.Item1, p.Item2, p.Item3, p.Item4);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Invoke(T1 p1, T2 p2, T3 p3, T4 p4)
		{
			var invokeList = GetInvocationList();
			for (int i = 0; i < invokeList.Count; i++)
				invokeList[i]?.Invoke(p1, p2, p3, p4);
		}
	}

	public class EventListener<T1, T2, T3, T4, T5> : EventListenerBase<Action<T1, T2, T3, T4, T5>>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override void InvokeWith(object args)
		{
			var p = (Tuple<T1, T2, T3, T4, T5>)args;
			Invoke(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Invoke(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
		{
			var invokeList = GetInvocationList();
			for (int i = 0; i < invokeList.Count; i++)
				invokeList[i]?.Invoke(p1, p2, p3, p4, p5);
		}
	}

	public abstract class EventListenerBase<DelegateType> : IEventListener where DelegateType : Delegate
	{
		private readonly List<DelegateType> _delegateList = new List<DelegateType>();
		private DelegateType[] _invocationList;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(DelegateType callback)
		{
			if (callback == null)
				return;

			_delegateList.AddUnique(callback);
			_invocationList = null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Remove(DelegateType callback)
		{
			if (callback == null)
				return;

			_delegateList.Remove(callback);
			_invocationList = null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveTarget(object target)
		{
			_delegateList.RemoveAll(c => c.Target == target);
			_invocationList = null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveAll()
		{
			_delegateList.Clear();
			_invocationList = null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IEventListener.Add(Delegate callback)
		{
			if (callback == null)
				return;

			Add((DelegateType)callback);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IEventListener.Remove(Delegate callback)
		{
			if (callback == null)
				return;

			Remove((DelegateType)callback);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected IReadOnlyList<DelegateType> GetInvocationList()
		{
			// 为了防止在Invoke执行阶段时再次调用Add/Remove改变回调列表，需要缓存一份当前的回调清单
			if (_invocationList == null)
				_invocationList = _delegateList.ToArray();

			return _invocationList;
		}

		public int Count => _delegateList.Count;

		public abstract void InvokeWith(object args);

		protected EventListenerBase() { }
	}
}
