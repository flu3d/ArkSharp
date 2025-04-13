using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ark.Data
{
	public abstract class TableDictBase<T, K> : Table<T> where T : Record, IKeyBase<K>
	{
		protected TableDictBase() { }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected T GetRecord(K key) => _lookup.GetValueOrDefault(key);

		public override void Unload()
		{
			_lookup.Clear();
		
			base.Unload();
		}

		protected override void OnLoaded()
		{
			_lookup.Clear();

			foreach (var d in _data)
				_lookup.Add(d.GetKey(), d);
		}

		protected readonly Dictionary<K, T> _lookup = new Dictionary<K, T>();
	}

	public abstract class TableDictRepeatableBase<T, K> : Table<T> where T : Record, IKeyBase<K>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected IReadOnlyList<T> GetRecords(K key)
		{
			_lookup.TryGetValue(key, out var list);
			
			return list ?? _emptyList;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void GetRecords(IEnumerable<K> keys, IList<T> result)
		{
			foreach (var k in keys)
			{
				_lookup.TryGetValue(k, out var values);

				if (values != null)
				{
					for (int i = 0; i < values.Count; i++)
						result.Add(values[i]);
				}
			}
		}

		public override void Unload()
		{
			_lookup.Clear();

			base.Unload();
		}

		protected override void OnLoaded()
		{
			_lookup.Clear();

			foreach (var d in _data)
				_lookup.Add(d.GetKey(), d);
		}

		protected MultiDictionary<K, T> _lookup = new MultiDictionary<K, T>();
		protected IReadOnlyList<T> _emptyList = new List<T>();
	}

	/// <summary>
	/// 键值唯一的数据表
	/// </summary>
	public abstract class TableDict<T, K> : TableDictBase<T, K> where T : Record, IKey<K>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual T Get(K key)
		{
			return GetRecord(key);
		}
	}

	public abstract class TableDict<T, K, K1, K2> : TableDictBase<T, K> where T : Record, IKeyCombine<K, K1, K2>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual T Get(K1 key1, K2 key2)
		{
			var combineKey = _emptyInstance.CombineKey(key1, key2);
			return GetRecord(combineKey);
		}

		protected static T _emptyInstance = Activator.CreateInstance<T>();
	}

	public abstract class TableDict<T, K, K1, K2, K3> : TableDictBase<T, K> where T : Record, IKeyCombine<K, K1, K2, K3>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual T Get(K1 key1, K2 key2, K3 key3)
		{
			var combineKey = _emptyInstance.CombineKey(key1, key2, key3);
			return GetRecord(combineKey);
		}

		protected static T _emptyInstance = Activator.CreateInstance<T>();
	}

	/// <summary>
	/// 允许键值重复的数据表
	/// </summary>
	public abstract class TableDictRepeatable<T, K> : TableDictRepeatableBase<T, K> where T : Record, IKeyRepeatable<K>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual IReadOnlyList<T> Get(K key)
		{
			return GetRecords(key);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual T GetFirst(K key)
		{
			var list = GetRecords(key);
			if (list.Count > 0)
				return list[0];

			return default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerable<T> Fetch(IReadOnlyList<K> keys)
		{
			return FetchImpl(keys).AsEnumerable();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected IEnumerator<T> FetchImpl(IReadOnlyList<K> keys)
		{
			for (int i = 0; i < keys.Count; i++)
			{
				var valueList = GetRecords(keys[i]);
				for (int j = 0; j < valueList.Count; j++)
					yield return valueList[j];
			}
		}
	}

	public abstract class TableDictRepeatable<T, K, K1, K2> : TableDictRepeatableBase<T, K> where T : Record, IKeyCombine<K, K1, K2>, IKeyRepeatable<K>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual IReadOnlyList<T> Get(K1 key1, K2 key2)
		{
			var combineKey = _emptyInstance.CombineKey(key1, key2);
			return GetRecords(combineKey);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual T GetFirst(K1 key1, K2 key2)
		{
			var combineKey = _emptyInstance.CombineKey(key1, key2);
			var list = GetRecords(combineKey);
			if (list.Count > 0)
				return list[0];

			return default;
		}

		protected static T _emptyInstance = Activator.CreateInstance<T>();
	}

	public abstract class TableDictRepeatable<T, K, K1, K2, K3> : TableDictRepeatableBase<T, K> where T : Record, IKeyCombine<K, K1, K2, K3>, IKeyRepeatable<K>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual IReadOnlyList<T> Get(K1 key1, K2 key2, K3 key3)
		{
			var combineKey = _emptyInstance.CombineKey(key1, key2, key3);
			return GetRecords(combineKey);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual T GetFirst(K1 key1, K2 key2, K3 key3)
		{
			var combineKey = _emptyInstance.CombineKey(key1, key2, key3);
			var list = GetRecords(combineKey);
			if (list.Count > 0)
				return list[0];

			return default;
		}

		protected static T _emptyInstance = Activator.CreateInstance<T>();
	}
}
