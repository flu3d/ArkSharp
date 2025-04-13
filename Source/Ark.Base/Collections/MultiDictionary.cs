using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Ark
{
	/// <summary>
	/// 单Key对多Value的字典容器
	/// Dictionary with Key->(multi)Values
	/// </summary>
	[Serializable]
	public class MultiDictionary<TKey, TValue> : Dictionary<TKey, List<TValue>>
	{
		public MultiDictionary() {}
		public MultiDictionary(int capacity) : base(capacity) {}
		protected MultiDictionary(SerializationInfo info, StreamingContext context) : base(info, context) {}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(TKey key, TValue value)
		{
			this.TryGetValue(key, out var list);

			if (list == null)
				this[key] = list = new List<TValue>();

			list.Add(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddUnique(TKey key, TValue value, Func<TValue, TValue, bool> comparer)
		{
			this.TryGetValue(key, out var list);

			if (list == null)
				this[key] = list = new List<TValue>();

			list.AddUnique(value, comparer);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddUnique(TKey key, TValue value, IEqualityComparer<TValue> comparer = null)
		{
			this.TryGetValue(key, out var list);

			if (list == null)
				this[key] = list = new List<TValue>();

			list.AddUnique(value, comparer);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(TKey key, TValue value)
		{
			this.TryGetValue(key, out var list);

			if (list == null)
				return false;

			return list.Remove(value);
		}
	}
}
