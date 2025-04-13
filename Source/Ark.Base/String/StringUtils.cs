using System.Runtime.CompilerServices;

namespace Ark
{
	public static class StringUtils
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNullOrEmpty(this string s)
		{
			if (s == null)
				return true;

			return s.Length == 0;	
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string EmptyToNull(this string s)
		{
			if (s == null || s.Length == 0)
				return null;

			return s;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string NullToEmpty(this string s)
		{
			if (s == null || s.Length == 0)
				return string.Empty;

			return s;
		}
	}
}
