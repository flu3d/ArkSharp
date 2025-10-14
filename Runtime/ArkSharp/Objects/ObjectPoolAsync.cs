using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ArkSharp
{
    /// <summary>
    /// 通用异步对象池
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

        public bool IsFull
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _limitCount > 0 && _data.Count >= _limitCount;
        }

        public int LimitCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _limitCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLimitCount(int limitCount) => _limitCount = limitCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual async UniTask<T> Alloc(CancellationToken token = default)
        {
            if (IsEmpty)
                return await _createFunc.Invoke(token);

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
                _onDisposeFunc?.Invoke(obj);
                return;
            }

            _onReleaseFunc?.Invoke(obj);

            _data.Add(obj);
        }

		/// <summary>预热对象池，提前创建指定数量的对象并加入池中</summary>
		/// <param name="withRelease">是否在对象加入池前调用 Release 回调函数</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async UniTask WarmUp(int count, bool withRelease = false, CancellationToken token = default)
		{
			for (int i = 0; i < count && !IsFull; i++)
            {
				var (isCanceled, obj) = await _createFunc.Invoke(token).SuppressCancellationThrow();

				// 已取消或创建失败
				if (isCanceled || obj == null)
					return;

				if (withRelease)
					_onReleaseFunc?.Invoke(obj);

                _data.Add(obj);
            }
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			if (IsEmpty)
				return;

			for (int i = 0; i < _data.Count; i++)
				_onDisposeFunc?.Invoke(_data[i]);

			_data.Clear();
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
