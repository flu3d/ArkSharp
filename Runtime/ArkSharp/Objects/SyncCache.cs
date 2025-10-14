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

		public void Clear(Action<TKey, TValue> disposer = null)
		{
			_lock.EnterWriteLock();
			try
			{
				if (disposer != null)
				{
					foreach (var kv in _dict)
						disposer.Invoke(kv.Key, kv.Value);
				}

				_dict.Clear();
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}

        public void Dispose()
		{
			_lock.Dispose();
		}
	}
}
