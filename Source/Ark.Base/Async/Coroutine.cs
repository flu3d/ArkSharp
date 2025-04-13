using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace Ark
{
	/// <summary>
	/// 协程包装器，支持AsyncOp及大部分Unity异步对象
	/// </summary>
	public sealed class Coroutine : AsyncOp
	{
		private IEnumerator _routine;
		private bool _completed;

		public override bool IsCompleted => _completed;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Coroutine(IEnumerator routine)
		{
			_routine = routine;

			if (routine == null)
				_completed = true;
		}

		/// <summary>
		/// 停止协程，释放迭代器引用
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Stop()
		{
			_routine = null;
			_completed = true;
		}

		/// <summary>
		/// 检查协程当前状态对象，如果不处于等待则步进
		/// </summary>
		public void Poll()
		{
			if (_completed || _routine == null)
				return;

			if (!DoPoll(_routine))
				_completed = true;
		}

		/// <summary>
		/// 检查协程当前状态对象，如果不处于等待则步进
		/// 返回true表示继续等待或步进成功，false表示协程结束
		/// </summary>
		private static bool DoPoll(IEnumerator routine)
		{
			var current = routine.Current;
			if (current != null)
			{
				switch (current)
				{
					case IAsyncOp asyncOp:
						if (!asyncOp.IsCompleted)
							return true;
						break;
					case Task task:
						if (!task.IsCompleted)
							return true;
						break;
#if UNITY_5_3_OR_NEWER
					// 检查Unity异步对象
					case AsyncOperation asyncOpU:
						if (!asyncOpU.isDone)
							return true;
						break;
					case CustomYieldInstruction customYieldOp:
						if (customYieldOp.keepWaiting)
							return true;
						break;
#if UNITY_EDITOR
					case YieldInstruction yieldOp:
						// 暂不支持YieldInstruction
						if (current is WaitForSeconds ws)
							Debug.LogWarning("WaitForSeconds is not supported in Ark.Coroutine. Using WaitForSecondsRealtime instead.");
						else
							Debug.LogWarning($"{current.GetType().Name} is not supported in Ark.Coroutine.");
						break;
#endif
#endif
					case IEnumerator innerRoutine:
						//  递归检查步进内嵌迭代器
						if (DoPoll(innerRoutine))
							return true;
						break;
					default:
						break;
				}
			}

			return DoMoveNext(routine);
		}


		/// <summary>
		/// 迭代器步进，如果下一个状态对象为内嵌协程则迭代执行步进
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool DoMoveNext(IEnumerator routine)
		{
			if (!routine.MoveNext())
				return false;

			var current = routine.Current;
			while (current is IEnumerator innerRoutine)
			{
				if (!innerRoutine.MoveNext())
					break;

				current = innerRoutine.Current;
			}

			return true;
		}
	}
}
