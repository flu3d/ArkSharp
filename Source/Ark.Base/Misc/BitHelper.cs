using System.Runtime.CompilerServices;

namespace Ark
{
	/// <summary>
	/// 位运算工具箱
	/// </summary>
	public sealed class BitHelper
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static int SetMask(int value, int bitmask, bool enabled) => enabled ? (value | bitmask) : (value & ~bitmask);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static uint SetMask(uint value, uint bitmask, bool enabled) => enabled ? (value | bitmask) : (value & ~bitmask);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static long SetMask(long value, long bitmask, bool enabled) => enabled ? (value | bitmask) : (value & ~bitmask);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static ulong SetMask(ulong value, ulong bitmask, bool enabled) => enabled ? (value | bitmask) : (value & ~bitmask);

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool TestMask(int value, int bitmask) => (value & bitmask) == bitmask;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool TestMask(uint value, uint bitmask) => (value & bitmask) == bitmask;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool TestMask(long value, long bitmask) => (value & bitmask) == bitmask;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool TestMask(ulong value, ulong bitmask) => (value & bitmask) == bitmask;

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static int SetAt(int value, int index, bool enabled) => SetMask(value, 1 << index, enabled);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static uint SetAt(uint value, int index, bool enabled) => SetMask(value, 1U << index, enabled);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static long SetAt(long value, int index, bool enabled) => SetMask(value, 1L << index, enabled);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static ulong SetAt(ulong value, int index, bool enabled) => SetMask(value, 1UL << index, enabled);

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool TestAt(int value, int index) => TestMask(value, 1 << index);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool TestAt(uint value, int index) => TestMask(value, 1U << index);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool TestAt(long value, int index) => TestMask(value, 1L << index);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool TestAt(ulong value, int index) => TestMask(value, 1UL << index);
	}
}
