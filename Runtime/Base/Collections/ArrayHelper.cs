using System;
using System.Runtime.CompilerServices;

namespace ArkSharp
{
	public static class ArrayHelper
	{
		public static int MaxLength => 0x7FFFFFC7;

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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool EnsureCapacity<T>(ref T[] array, int count, int minCapacity = 16)
		{
			if (count > MaxLength)
				return false;
				
			long capacity = array.Length;
			if (count <= capacity)
				return true;

			if (capacity <= 0)
			{
				capacity = minCapacity;
				if (capacity <= 0)
					capacity = 1;
			}

			if (capacity > MaxLength)
				capacity = MaxLength;

			while (count > capacity)
			{
				capacity *= 2;

				if (capacity > MaxLength)
				{
					capacity = MaxLength;
					break;
				}
			}

			Array.Resize(ref array, (int)capacity);
			return true;
		}
	}
}
