using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ArkSharp
{
    /// <summary>
    /// 通用异步对象池。
    /// 此类不保证线程安全，应由单一逻辑线程使用。
    /// </summary>
    public class ObjectPoolAsync<T> where T : class
    {
        protected readonly List<T> _data;
        protected int _limitCount;

        public ObjectPoolAsync(Func<CancellationToken, UniTask<T>> createFunc, int limitCount = 0)
        {
            _createFunc = createFunc;
            _limitCount = limitCount;

            if (_limitCount > 0)
                _data = new List<T>(_limitCount);
            else
                _data = new List<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ObjectPoolAsync(int limitCount = 0) : this(_ => UniTask.FromResult(Activator.CreateInstance<T>()), limitCount) {}

        /// <summary>
        /// 当前保留的闲置对象数量。
        /// </summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.Count;
        }

        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.Count == 0;
        }

        /// <summary>
        /// 闲置对象数量是否达到保留上限。无限制时始终为 false。
        /// </summary>
        public bool IsFull
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _limitCount > 0 && _data.Count >= _limitCount;
        }

        /// <summary>
        /// 最多保留的闲置对象数量，非正数表示无限制。
        /// </summary>
        public int LimitCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _limitCount;
        }

        /// <summary>
        /// 设置闲置对象保留上限。
        /// 此方法不会主动裁剪已有闲置对象；缩小上限后，仅影响后续预热和归还行为。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLimitCount(int limitCount)
        {
            _limitCount = limitCount;
        }

        /// <summary>
        /// 从池中分配对象。取消时返回 null，不会消耗池中的闲置对象。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual async UniTask<T> Alloc(CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return null;

            if (IsEmpty)
            {
                try
                {
                    return await _createFunc.Invoke(token);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    return null;
                }
            }

            int last = _data.Count - 1;
            var obj = _data[last];
            _data.RemoveAt(last);

            return obj;
        }

        /// <summary>
        /// 将对象归还到池中。重复归还同一实例时忽略；池满时直接销毁对象。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release(T obj)
        {
            if (obj == null)
                return;

            for (int i = 0; i < _data.Count; i++)
            {
                if (ReferenceEquals(_data[i], obj))
                    return;
            }

            if (IsFull)
            {
                _onDisposeFunc?.Invoke(obj);
                return;
            }

            _onReleaseFunc?.Invoke(obj);

            _data.Add(obj);
        }

		/// <summary>
		/// 额外预创建指定数量的闲置对象，不超过闲置对象保留上限。
		/// 此方法顺序创建对象；取消会安静结束，工厂异常会直接向上传播，已经成功加入池的对象会保留。
		/// </summary>
		/// <param name="count">本次最多新增的闲置对象数量。</param>
		/// <param name="withRelease">对象加入池前是否执行归还回调。</param>
		/// <param name="token">取消令牌。</param>
		public async UniTask WarmUp(int count, bool withRelease = false, CancellationToken token = default)
		{
			for (int i = 0; i < count && !IsFull; i++)
			{
				if (token.IsCancellationRequested)
					return;

				T obj;
				try
				{
					obj = await _createFunc.Invoke(token);
				}
				catch (OperationCanceledException) when (token.IsCancellationRequested)
				{
					return;
				}

				if (obj == null)
					continue;

				if (withRelease)
				{
					try
					{
						_onReleaseFunc?.Invoke(obj);
					}
					catch (Exception releaseException)
					{
						try
						{
							_onDisposeFunc?.Invoke(obj);
						}
						catch (Exception disposeException)
						{
							throw new AggregateException(releaseException, disposeException);
						}

						throw;
					}
				}

				_data.Add(obj);
			}
		}

		/// <summary>
		/// 销毁并移除调用开始时池中已有的所有闲置对象。
		/// 销毁回调期间新归还的对象视为后续操作，不属于本次清理范围。
		/// 销毁回调异常不会阻止其他对象清理，完成清理后会重新抛出。
		/// </summary>
		public void Clear()
		{
			if (IsEmpty)
				return;

			var objects = _data.ToArray();
			_data.Clear();

			List<Exception> exceptions = null;
			for (int i = 0; i < objects.Length; i++)
			{
				try
				{
					_onDisposeFunc?.Invoke(objects[i]);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ObjectPoolAsync<T> CreateWith(Func<CancellationToken, UniTask<T>> func) { _createFunc = func; return this; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ObjectPoolAsync<T> OnReleaseWith(Action<T> func) { _onReleaseFunc = func; return this; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ObjectPoolAsync<T> OnDisposeWith(Action<T> func) { _onDisposeFunc = func; return this; }

        protected Func<CancellationToken, UniTask<T>> _createFunc;
        protected Action<T> _onReleaseFunc;
        protected Action<T> _onDisposeFunc;
    }
}
