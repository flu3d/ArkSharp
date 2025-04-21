using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ArkSharp
{
	/// <summary>
	/// 单Key对多Value的字典容器辅助类
	/// Helper for Dictionary with (one)Key->(multi)Values
	/// </summary>
	public static class MultiDictionaryHelper
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Add<TKey, TValue>(this IDictionary<TKey, List<TValue>> dict, TKey key, TValue value)
		{
			dict.TryGetValue(key, out var list);

			if (list == null)
				dict[key] = list = new List<TValue>();

			list.Add(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AddUnique<TKey, TValue>(this IDictionary<TKey, List<TValue>> dict, TKey key, TValue value, Func<TValue, TValue, bool> comparer)
		{
			dict.TryGetValue(key, out var list);

			if (list == null)
				dict[key] = list = new List<TValue>();

			list.AddUnique(value, comparer);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AddUnique<TKey, TValue>(this IDictionary<TKey, List<TValue>> dict, TKey key, TValue value, IEqualityComparer<TValue> comparer = null)
		{
			dict.TryGetValue(key, out var list);

			if (list == null)
				dict[key] = list = new List<TValue>();

			list.AddUnique(value, comparer);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Remove<TKey, TValue>(this IDictionary<TKey, List<TValue>> dict, TKey key, TValue value)
		{
			dict.TryGetValue(key, out var list);

			if (list == null)
				return false;

			return list.Remove(value);
		}
	}
}
