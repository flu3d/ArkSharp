using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ArkSharp
{
	/// <summary>
	/// 简单对象缓存表
	/// </summary>
	public class SimpleCache<TKey, TValue>
	{
		private readonly Dictionary<TKey, TValue> _dict = new Dictionary<TKey, TValue>();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TValue Get(TKey key)
		{
			_dict.TryGetValue(key, out var result);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(TKey key, TValue value)
		{
			_dict[key] = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(TKey key, Func<TKey, TValue> valueFactory)
		{
			_dict[key] = valueFactory.Invoke(key);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Remove(TKey key)
		{
			_dict.Remove(key);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TValue GetOrAdd(TKey key, TValue value)
		{
			if (!_dict.TryGetValue(key, out var result))
				_dict[key] = result = value;

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
		{
			if (!_dict.TryGetValue(key, out var result))
				_dict[key] = result = valueFactory.Invoke(key);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			_dict.Clear();
		}
	}
}
