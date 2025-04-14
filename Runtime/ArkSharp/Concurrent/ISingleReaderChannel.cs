using System;
using System.Threading;

namespace ArkSharp
{
	/// <summary>
	/// 线程安全的单消费者队列接口
	/// </summary>
	public interface ISingleReaderChannel<T> : IDisposable
	{
		/// <summary>
		/// 尝试写入队列，如果队列关闭或队列满则返回false
		/// </summary>
		bool Write(T item);

		/// <summary>
		/// 阻塞读取数据，直到队列关闭且无数据则返回false
		/// </summary>
		bool Read(out T item);

		/// <summary>
		/// 关闭队列，不再接收新数据，已有数据可以继续读取直到队列空
		/// </summary>
		void Close();

		/// <summary>
		/// 队列是否已关闭
		/// </summary>
		bool IsClosed { get; }
	}
}
