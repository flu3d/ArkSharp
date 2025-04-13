using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ArkSharp
{
	/// <summary>
	/// 通用对象池
	/// </summary>
	public class ObjectPool<T> where T:class
	{
		protected readonly List<T> _data;
		protected int _limitCount;

		public ObjectPool(Func<T> createFunc, int limitCount = 0)
		{
			_createFunc = createFunc;
			_limitCount = limitCount;

			if (_limitCount > 0)
				_data = new List<T>(_limitCount);
			else
				_data = new List<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ObjectPool(int limitCount = 0) : this(Activator.CreateInstance<T>, limitCount) {}
		
		public int Count {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _data.Count;
		}
		
		public bool IsEmpty {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _data.Count == 0;
		}
		
		public bool IsFull {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _limitCount > 0 && _data.Count >= _limitCount;
		}

		public int LimitCount {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _limitCount;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetLimitCount(int limitCount) => _limitCount = limitCount;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Alloc()
		{
			if (IsEmpty)
				return _createFunc.Invoke();

			int last = _data.Count - 1;
			var obj = _data[last];
			_data.RemoveAt(last);

			return obj;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Release(T obj)
		{
			if (obj == null)
				return;

			// check if obj is already in pool
			if (_data.IndexOf(obj) >= 0)
				return;

			if (IsFull)
			{
				_disposeFunc?.Invoke(obj);
				return;
			}

			_resetFunc?.Invoke(obj);

			_data.Add(obj);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WarmUp(int count)
		{
			for (int i = 0; i < count && !IsFull; i++)
				_data.Add(_createFunc.Invoke());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			if (IsEmpty)
				return;

			for (int i = 0; i < _data.Count; i++)
				_disposeFunc?.Invoke(_data[i]);

			_data.Clear();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ObjectPool<T> CreateWith(Func<T> func) { _createFunc = func; return this; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ObjectPool<T> ResetWith(Action<T> func) { _resetFunc = func; return this; }
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ObjectPool<T> DisposeWith(Action<T> func) { _disposeFunc = func; return this; }

		protected Func<T> _createFunc;
		protected Action<T> _resetFunc;
		protected Action<T> _disposeFunc;
	}
}
