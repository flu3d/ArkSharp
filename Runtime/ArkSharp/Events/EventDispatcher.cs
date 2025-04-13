using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ArkSharp
{
	/// <summary>
	/// 基于事件名的多路事件派发管理
	/// </summary>
	public class EventDispatcher
	{
		private readonly object _syncRoot = new object();
		private readonly Dictionary<string, IEventListener> _listeners = new Dictionary<string, IEventListener>();


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Remove(string eventName, Delegate callback)
		{
			if (eventName == null || callback == null)
				return;

			lock (_syncRoot)
			{
				_listeners.TryGetValue(eventName, out var listener);
				listener?.Remove(callback);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveTarget(object target)
		{
			lock (_syncRoot)
			{
				foreach (var kv in _listeners)
					kv.Value.RemoveTarget(target);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Remove(string eventName)
		{
			if (eventName == null)
				return;

			lock (_syncRoot)
			{
				_listeners.TryGetValue(eventName, out var listener);
				listener?.RemoveAll();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveAll()
		{
			lock (_syncRoot)
			{
				foreach (var kv in _listeners)
					kv.Value.RemoveAll();

				_listeners.Clear();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(string eventName, Action callback)
		{
			if (eventName == null || callback == null)
				return;

			lock (_syncRoot)
			{
				_listeners.TryGetValue(eventName, out var listener);
				if (listener == null)
					_listeners[eventName] = listener = new EventListener();

				listener.Add(callback);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add<T1>(string eventName, Action<T1> callback)
		{
			if (eventName == null || callback == null)
				return;

			lock (_syncRoot)
			{
				_listeners.TryGetValue(eventName, out var listener);
				if (listener == null)
					_listeners[eventName] = listener = new EventListener<T1>();

				listener.Add(callback);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add<T1, T2>(string eventName, Action<T1, T2> callback)
		{
			if (eventName == null || callback == null)
				return;

			lock (_syncRoot)
			{
				_listeners.TryGetValue(eventName, out var listener);
				if (listener == null)
					_listeners[eventName] = listener = new EventListener<T1, T2>();

				listener.Add(callback);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add<T1, T2, T3>(string eventName, Action<T1, T2, T3> callback)
		{
			if (eventName == null || callback == null)
				return;

			lock (_syncRoot)
			{
				_listeners.TryGetValue(eventName, out var listener);
				if (listener == null)
					_listeners[eventName] = listener = new EventListener<T1, T2, T3>();

				listener.Add(callback);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add<T1, T2, T3, T4>(string eventName, Action<T1, T2, T3, T4> callback)
		{
			if (eventName == null || callback == null)
				return;

			lock (_syncRoot)
			{
				_listeners.TryGetValue(eventName, out var listener);
				if (listener == null)
					_listeners[eventName] = listener = new EventListener<T1, T2, T3, T4>();

				listener.Add(callback);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add<T1, T2, T3, T4, T5>(string eventName, Action<T1, T2, T3, T4, T5> callback)
		{
			if (eventName == null || callback == null)
				return;

			lock (_syncRoot)
			{
				_listeners.TryGetValue(eventName, out var listener);
				if (listener == null)
					_listeners[eventName] = listener = new EventListener<T1, T2, T3, T4, T5>();

				listener.Add(callback);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispatch(string eventName)
		{
			if (eventName == null)
				return;

			var listener = GetListenerSync(eventName);
			if (listener != null)
				((EventListener)listener).Invoke();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispatch<T1>(string eventName, T1 p1)
		{
			if (eventName == null)
				return;

			var listener = GetListenerSync(eventName);
			if (listener != null)
				((EventListener<T1>)listener).Invoke(p1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispatch<T1, T2>(string eventName, T1 p1, T2 p2)
		{
			if (eventName == null)
				return;

			var listener = GetListenerSync(eventName);
			if (listener != null)
				((EventListener<T1, T2>)listener).Invoke(p1, p2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispatch<T1, T2, T3>(string eventName, T1 p1, T2 p2, T3 p3)
		{
			if (eventName == null)
				return;

			var listener = GetListenerSync(eventName);
			if (listener != null)
				((EventListener<T1, T2, T3>)listener).Invoke(p1, p2, p3);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispatch<T1, T2, T3, T4>(string eventName, T1 p1, T2 p2, T3 p3, T4 p4)
		{
			if (eventName == null)
				return;

			var listener = GetListenerSync(eventName);
			if (listener != null)
				((EventListener<T1, T2, T3, T4>)listener).Invoke(p1, p2, p3, p4);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispatch<T1, T2, T3, T4, T5>(string eventName, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
		{
			if (eventName == null)
				return;

			var listener = GetListenerSync(eventName);
			if (listener != null)
				((EventListener<T1, T2, T3, T4, T5>)listener).Invoke(p1, p2, p3, p4, p5);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DispatchWith(string eventName, object args)
		{
			if (eventName == null)
				return;

			var listener = GetListenerSync(eventName);
			listener?.InvokeWith(args);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private IEventListener GetListenerSync(string eventName)
		{
			lock (_syncRoot)
			{
				_listeners.TryGetValue(eventName, out var listener);
				return listener;
			}
		}

		public static EventDispatcher Default { get; } = new EventDispatcher();

		static EventDispatcher() { }
	}
}
