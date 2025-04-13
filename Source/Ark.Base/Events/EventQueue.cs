using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Ark
{
	/// <summary>
	/// 事件队列
	/// </summary>
	public class EventQueue
	{
		readonly struct EventData 
		{ 
			public readonly string name;
			public readonly object args;

			public EventData(string name, object args)
			{
				this.name = name;
				this.args = args;
			}
		}

		private ConcurrentQueue<EventData> _queue = new ConcurrentQueue<EventData>();
		private readonly EventDispatcher _dispatcher;

		public EventQueue(EventDispatcher dispatcher) => _dispatcher = dispatcher;


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Enqueue(string eventName)
		{
			if (eventName == null)
				return;

			_queue.Enqueue(new EventData(eventName, null));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Enqueue<T1>(string eventName, T1 p1)
		{
			if (eventName == null)
				return;

			_queue.Enqueue(new EventData(eventName, p1)); // 单个参数直接存args，不放Tuple
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Enqueue<T1, T2>(string eventName, T1 p1, T2 p2)
		{
			if (eventName == null)
				return;

			_queue.Enqueue(new EventData(eventName, Tuple.Create(p1, p2)));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Enqueue<T1, T2, T3>(string eventName, T1 p1, T2 p2, T3 p3)
		{
			if (eventName == null)
				return;

			_queue.Enqueue(new EventData(eventName, Tuple.Create(p1, p2, p3)));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Enqueue<T1, T2, T3, T4>(string eventName, T1 p1, T2 p2, T3 p3, T4 p4)
		{
			if (eventName == null)
				return;

			_queue.Enqueue(new EventData(eventName, Tuple.Create(p1, p2, p3, p4)));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Enqueue<T1, T2, T3, T4, T5>(string eventName, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
		{
			if (eventName == null)
				return;

			_queue.Enqueue(new EventData(eventName, Tuple.Create(p1, p2, p3, p4, p5)));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PollAll()
		{
			while (_queue.TryDequeue(out var e))
				_dispatcher.DispatchWith(e.name, e.args);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool PollOne()
		{
			if (!_queue.TryDequeue(out var e))
				return false;

			_dispatcher.DispatchWith(e.name, e.args);
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			_queue.Clear();
		}

		public static EventQueue Default { get; } = new EventQueue(EventDispatcher.Default);

		static EventQueue() { }
	}
}
