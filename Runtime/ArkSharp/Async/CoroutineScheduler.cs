using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ArkSharp
{
	/// <summary>
	/// 协程管理器，支持AsyncOp以及Unity大部分异步对象
	/// 支持非运行的编辑器模式
	/// </summary>
	public sealed class CoroutineScheduler
	{
		private readonly List<Coroutine> _coroutines = new List<Coroutine>(128);

		public int Count {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _coroutines.Count;
		}

		/// <summary>
		/// 创建受统一管理的协程
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Coroutine Start(IEnumerator routine)
		{
			if (routine == null)
				return null;

			var co = new Coroutine(routine);	
			_coroutines.Add(co);
			
			return co;
		}

		/// <summary>
		/// 停止并移除协程
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Stop(Coroutine co)
		{
			if (co == null)
				return;

			co.Stop();
			_coroutines.Remove(co);
		}

		/// <summary>
		/// 停止并移除所有协程
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void StopAll()
		{
			for (int i = 0; i < _coroutines.Count; i++)
				_coroutines[i].Stop();

			_coroutines.Clear();
		}

		/// <summary>
		/// 步进所有协程，并移除已经结束的协程
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Poll()
		{
			for (int i = 0; i < _coroutines.Count; )
			{
				var co = _coroutines[i];
				co.Poll();

				if (co.IsCompleted)
					_coroutines.RemoveAt(i);
				else
					i++;
			}
		}

#if UNITY_5_3_OR_NEWER
		class CoroutineRunner : MonoBehaviour
		{
			internal readonly CoroutineScheduler scheduler = new CoroutineScheduler();

			void LateUpdate() => scheduler.Poll();
		}

#if UNITY_EDITOR
		class EditorCoroutineRunner
		{
			internal readonly CoroutineScheduler scheduler = new CoroutineScheduler();

			public EditorCoroutineRunner() => EditorApplication.update += scheduler.Poll;
		}
#endif

		public static CoroutineScheduler Default {
			get {
#if UNITY_EDITOR
				return !Application.isPlaying ? Singleton.Get<EditorCoroutineRunner>().scheduler : Singleton.Get<CoroutineRunner>().scheduler;
#else
				return Singleton.Get<CoroutineRunner>().scheduler;
#endif
			}
		}
#else
		public static CoroutineScheduler Default => Singleton.Get<CoroutineScheduler>();
#endif
	}
}
