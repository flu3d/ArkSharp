using System.Collections;

namespace ArkSharp
{
	/// <summary>
	/// 通用异步接口，不支持Unity协程
	/// </summary>
	public interface IAsyncOp
	{
		bool IsCompleted { get; }
	}

	/// <summary>
	/// 通用异步接口，不支持Unity协程
	/// </summary>
	public interface IAsyncOp<T> : IAsyncOp
	{
		T Result { get; }
	}

	/// <summary>
	/// 通用异步结果基类，可同时支持Unity及独立的协程管理
	/// </summary>
	public abstract class AsyncOp : IAsyncOp, IEnumerator
	{
		public abstract bool IsCompleted { get; }
		
		object IEnumerator.Current => null;
		bool IEnumerator.MoveNext() => !IsCompleted;
		void IEnumerator.Reset() { }

		public static AsyncOp Default { get; } = new DummyClass();
		class DummyClass : AsyncOp
		{
			public override bool IsCompleted => true;
		}
	}

	/// <summary>
	/// 通用异步结果基类（带返回值），可同时支持Unity及独立的协程管理
	/// </summary>
	public abstract class AsyncOp<T> : IAsyncOp<T>, IEnumerator
	{
		public abstract T Result { get; }
		public abstract bool IsCompleted { get; }
		
		object IEnumerator.Current => null;
		bool IEnumerator.MoveNext() => !IsCompleted;
		void IEnumerator.Reset() { }

		public static AsyncOp<T> Default { get; } = new DummyClass();
		class DummyClass : AsyncOp<T>
		{
			public override bool IsCompleted => true;
			public override T Result => default;
		}
	}
}
