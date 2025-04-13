using System;
using System.Runtime.CompilerServices;

namespace Ark
{
	public static class ArrayUtils
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Clear(this Array array, int index, int length)
		{
			Array.Clear(array, index, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Clear(this Array array)
		{
			Array.Clear(array, 0, array.Length);
		}
	}
}
