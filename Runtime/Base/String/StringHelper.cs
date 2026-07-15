using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace ArkSharp
{
	public static class StringHelper
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNullOrEmpty(this string s)
		{
			if (s == null)
				return true;

			return s.Length == 0;	
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNullOrWhiteSpace(this string s)
		{
			return string.IsNullOrWhiteSpace(s);
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

		/// <summary>
		/// 匹配 CJK 统一表意文字扩展 A 区（U+3400-U+4DBF）和基本区（U+4E00-U+9FFF），
		/// 不包含扩展 B 及后续扩展区，也不包含兼容表意文字。
		/// </summary>
		private static readonly Regex CHINESE_CHARACTER_REGEX = new(@"[\u3400-\u4DBF\u4E00-\u9FFF]");

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ContainsChinese(this string text)
		{
			if (string.IsNullOrEmpty(text))
				return false;

			return CHINESE_CHARACTER_REGEX.IsMatch(text);
		}
	}
}
