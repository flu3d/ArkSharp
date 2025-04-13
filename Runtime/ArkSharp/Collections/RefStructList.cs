using System;
using System.Runtime.CompilerServices;

namespace Game.Battle
{
	/// <summary>
	/// 结构体变长线性容器，比普通List增加ref引用访问支持
	/// Linear collection for structs, with ref accessing over normal List
	/// </summary>
	public class RefStructList<T> where T : struct
	{
		private const int DefaultCapacity = 64;
		private const int MaxCapacity = 0x7FEFFFFF;

		private T[] _items;
		private int _size;

		private ResetFunc _resetFunc;
		public delegate void ResetFunc(ref T item);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RefStructList(int capacity, ResetFunc resetFunc = null)
		{
			if (capacity < 0)
				capacity = 0;
			else if ((uint)capacity > MaxCapacity)
				capacity = MaxCapacity;

			_items = new T[capacity];
			_size = 0;

			_resetFunc = resetFunc;
		}

		public int Count {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _size;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] GetRawBuffer() => _items;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Ref(int index) => ref _items[index];

		public T this[int index] {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _items[index];

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _items[index] = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(in T value)
		{
			int index = _size;

			_size++;
			EnsureCapacity(_size);

			_items[index] = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T AddRef()
		{
			int index = _size;

			_size++;
			EnsureCapacity(_size);

			return ref _items[index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Enlarge(int count)
		{
			if (count <= _size)
				return;

			_size = count;
			EnsureCapacity(_size);
		}

		/// <summary>
		/// 重置指定位置元素并与最后一位交换。在循环中使用需要注意。
		/// Reset and swap with last one, using carefully in loops
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveAt(int index)
		{
			if (index < 0 || index >= _size)
				return;

			if (_resetFunc != null)
				_resetFunc.Invoke(ref _items[index]);
			else
				_items[index] = default;

			var lastIndex = _size - 1;
			if (lastIndex != index)
			{
				var temp = _items[index];
				_items[index] = _items[lastIndex];
				_items[lastIndex] = temp;
			}

			_size--;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			if (_resetFunc != null)
			{
				for (int i = 0; i < _size; i++)
					_resetFunc.Invoke(ref _items[i]);
			}
			else
			{
				Array.Clear(_items, 0, _size);
			}

			_size = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void EnsureCapacity(int count)
		{
			int capacity = _items.Length;
			if (count <= capacity)
				return;

			while (count > capacity)
			{
				if (capacity <= 0)
					capacity = DefaultCapacity;
				else
					capacity *= 2;

				if ((uint)capacity > MaxCapacity)
				{
					capacity = MaxCapacity;
					break;
				}
			}

			if (capacity > _items.Length)
				Array.Resize(ref _items, capacity);
		}
	}
}
