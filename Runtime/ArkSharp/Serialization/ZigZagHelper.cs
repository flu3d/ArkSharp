namespace ArkSharp
{
	/// <summary>
	/// ZigZag编码转换整数和非负整数
	/// 让绝对值小的负数在进行变长编码时长度缩短
	/// 仅支持小端序
	/// </summary>
	public static class ZigZagHelper
	{
		public static ulong Encode(long value) => (ulong)((value << 1) ^ (value >> 63));
		public static long Decode(ulong value) => (long)(value >> 1) ^ -((long)value & 1);

		public static uint Encode(int value) => (uint)((value << 1) ^ (value >> 31));
		public static int Decode(uint value) => (int)(value >> 1) ^ -((int)value & 1);
	}
}
