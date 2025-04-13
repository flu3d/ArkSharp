using System;
using System.Collections.Generic;

namespace Ark
{
	/// <summary>
	/// 异步等待直到结果满足，同Unity同名类
	/// </summary>
	public sealed class WaitUntil : AsyncOp
	{
		private Func<bool> _predicate;

		public WaitUntil(Func<bool> predicate) { _predicate = predicate; }

		public override bool IsCompleted => _predicate();
	}

	/// <summary>
	/// 异步等待直到结果不满足，同Unity同名类
	/// </summary>
	public sealed class WaitWhile : AsyncOp
	{
		private Func<bool> _predicate;

		public WaitWhile(Func<bool> predicate) { _predicate = predicate; }

		public override bool IsCompleted => !_predicate();
	}

	/// <summary>
	/// 异步等待一定秒数（系统UTC时钟） 
	/// </summary>
	public sealed class WaitForSecondsRealtime : AsyncOp
	{
		private readonly long _endTicks;

		public WaitForSecondsRealtime(double seconds) { _endTicks = DateTime.UtcNow.Ticks + (long)(seconds * 1000L) * 10000L; }

		public override bool IsCompleted => DateTime.UtcNow.Ticks >= _endTicks;
	}

	/// <summary>
	/// 异步等待直到所有异步结果完成
	/// </summary>
	public sealed class WaitForAll : AsyncOp
	{
		private readonly List<IAsyncOp> _opList;

		public WaitForAll(params IAsyncOp[] opList) { _opList = new List<IAsyncOp>(opList); }
		public WaitForAll(IEnumerable<IAsyncOp> opList) { _opList = new List<IAsyncOp>(opList); }

		public override bool IsCompleted { 
			get {
				if (_opList == null || _opList.Count <= 0)
					return true;

				for (int i = 0; i < _opList.Count; ++i)
				{
					var op = _opList[i];
					if (op != null && !op.IsCompleted)
						return false;
				}
				return true;
			}
		}

		public float Progress {
			get {
				if (_opList == null || _opList.Count <= 0)
					return 1.0f;

				int count = 0;
				for (int i = 0; i < _opList.Count; ++i)
				{
					var op = _opList[i];
					if (op == null || op.IsCompleted)
						count++;
				}

				return (float)((double)count / _opList.Count);
			}
		}
	}

	/// <summary>
	/// 异步等待直到其中一个异步结果完成 
	/// </summary>
	public sealed class WaitForAny : AsyncOp
	{
		private readonly List<IAsyncOp> _opList;

		public WaitForAny(params IAsyncOp[] opList) { _opList = new List<IAsyncOp>(opList); }
		public WaitForAny(IEnumerable<IAsyncOp> opList) { _opList = new List<IAsyncOp>(opList); }

		public override bool IsCompleted {
			get {
				if (_opList == null || _opList.Count <= 0)
					return true;

				for (int i = 0; i < _opList.Count; ++i)
				{
					var op = _opList[i];
					if (op != null && op.IsCompleted)
						return true;
				}
				return false;
			}
		}
	}
}
