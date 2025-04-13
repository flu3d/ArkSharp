using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ArkSharp
{
	public static class EnumHelper
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Combine<T>(IReadOnlyList<T> enumFlags) where T: Enum
		{
			int result = 0;

			for (int i = 0; i < enumFlags.Count; i++)
				result |= (int)(object)enumFlags[i];

			return (T)(object)result;
		}
	}
}
