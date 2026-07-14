using System.Runtime.InteropServices;

namespace ArkSharp
{
	/// <summary>
	/// 缓存行对齐的数据字段
	/// 通过强制数据结构在内存中的布局，使其关键字段位于独立的缓存行(Cache Line)中，从而避免多线程环境下的伪共享(False Sharing)问题。
	///
	///   缓存行大小：现代CPU通常使用64字节的缓存行(x86/x64)或128字节缓存行（ARM/ARM64）；
	///   伪共享：当两个线程频繁修改位于同一缓存行的不同变量时，会触发缓存一致性协议(如MESI)，导致缓存行无效化，产生不必要的内存访问；
	/// </summary>
	[StructLayout(LayoutKind.Explicit, Size = 128)]
	public struct PaddedInt
	{
		/// <summary>
		/// 经过缓存行对齐的真正数据，建议用Volatile.Read/Write读写
		/// </summary>
		[FieldOffset(64)]
		public int Value;
	}

	[StructLayout(LayoutKind.Explicit, Size = 128)]
	public struct PaddedInt64
	{
		/// <summary>
		/// 经过缓存行对齐的真正数据，建议用Volatile.Read/Write读写
		/// </summary>
		[FieldOffset(64)]
		public long Value;
	}
}
