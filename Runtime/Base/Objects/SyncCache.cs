using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ArkSharp
{
	/// <summary>
	/// 线程安全的KeyValue对象缓存表
	/// </summary>
	public class SyncCache<TKey, TValue> : IDisposable
	{
		private readonly Dictionary<TKey, TValue> _dict = new Dictionary<TKey, TValue>();
		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TValue Get(TKey key)
		{
			var result = default(TValue);

			_lock.EnterReadLock();
			try
			{
				_dict.TryGetValue(key, out result);
			}
			finally
			{
				_lock.ExitReadLock();
			}

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(TKey key, TValue value)
		{
			_lock.EnterWriteLock();
			try
			{
				_dict[key] = value;
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(TKey key, Func<TKey, TValue> creator)
		{
			_lock.EnterWriteLock();
			try
			{
				_dict[key] = creator.Invoke(key);
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Remove(TKey key, Action<TKey, TValue> disposer = null)
		{
			_lock.EnterWriteLock();
			try
			{
				if (disposer != null)
				{
					if (_dict.TryGetValue(key, out var value))
						disposer.Invoke(key, value);
				}

				_dict.Remove(key);
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}

		/// Return existing value if key already exists.
		/// Otherwise, add new key/value and return the new value.
		public TValue GetOrAdd(TKey key, TValue value)
		{
			var result = default(TValue);

			_lock.EnterUpgradeableReadLock();
			try
			{

				if (!_dict.TryGetValue(key, out result))
				{
					_lock.EnterWriteLock();
					try
					{

						// double-check locking pattern
						if (!_dict.TryGetValue(key, out result))
							_dict[key] = result = value;
					}
					finally
					{
						_lock.ExitWriteLock();
					}
				}
			}
			finally
			{
				_lock.ExitUpgradeableReadLock();
			}

			return result;
		}

		/// Return existing value if key already exists.
		/// Otherwise, add new key/value and return the new value created by valueFactory.
		public TValue GetOrAdd(TKey key, Func<TKey, TValue> creator)
		{
			var result = default(TValue);

			_lock.EnterUpgradeableReadLock();
			try
			{
				if (!_dict.TryGetValue(key, out result))
				{
					_lock.EnterWriteLock();
					try
					{
						// double-check locking pattern
						if (!_dict.TryGetValue(key, out result))
							_dict[key] = result = creator.Invoke(key);
					}
					finally
					{
						_lock.ExitWriteLock();
					}
				}
			}
			finally
			{
				_lock.ExitUpgradeableReadLock();
			}

			return result;
		}

		/// <summary>
		/// 清除调用开始时缓存中的所有条目。
		/// disposer 在锁外执行；执行期间新写入的条目视为后续操作，不属于本次清理范围。
		/// disposer 异常不会阻止其他条目清理，完成后会统一抛出。
		/// </summary>
		public void Clear(Action<TKey, TValue> disposer = null)
		{
			KeyValuePair<TKey, TValue>[] entries = null;

			_lock.EnterWriteLock();
			try
			{
				if (_dict.Count == 0)
					return;

				if (disposer != null)
				{
					entries = new KeyValuePair<TKey, TValue>[_dict.Count];
					int index = 0;
					foreach (var entry in _dict)
						entries[index++] = entry;
				}

				_dict.Clear();
			}
			finally
			{
				_lock.ExitWriteLock();
			}

			if (entries == null)
				return;

			List<Exception> exceptions = null;
			for (int i = 0; i < entries.Length; i++)
			{
				try
				{
					var entry = entries[i];
					disposer.Invoke(entry.Key, entry.Value);
				}
				catch (Exception e)
				{
					exceptions ??= new List<Exception>();
					exceptions.Add(e);
				}
			}

			if (exceptions != null)
				throw new AggregateException(exceptions);
		}

        public void Dispose()
		{
			_lock.Dispose();
		}
	}
}
