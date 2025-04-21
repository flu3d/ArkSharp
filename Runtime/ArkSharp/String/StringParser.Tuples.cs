using System;
using System.Runtime.CompilerServices;

namespace ArkSharp
{
	public static partial class StringParser
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void To<T1>(this string text, out T1 p1)
			=> To(text.AsSpan(), out p1);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void To<T1, T2>(this string text, out T1 p1, out T2 p2)
			=> To(text.AsSpan(), out p1, out p2);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void To<T1, T2, T3>(this string text, out T1 p1, out T2 p2, out T3 p3)
			=> To(text.AsSpan(), out p1, out p2, out p3);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void To<T1, T2, T3, T4>(this string text, out T1 p1, out T2 p2, out T3 p3, out T4 p4)
			=> To(text.AsSpan(), out p1, out p2, out p3, out p4);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void To<T1, T2, T3, T4, T5>(this string text, out T1 p1, out T2 p2, out T3 p3, out T4 p4, out T5 p5)
			=> To(text.AsSpan(), out p1, out p2, out p3, out p4, out p5);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void To<T1>(this ReadOnlySpan<char> text, out T1 p1)
		{
			p1 = To<T1>(text);
		}

		public static void To<T1, T2>(this ReadOnlySpan<char> text, out T1 p1, out T2 p2)
		{
			var values = text.Split(DefaultListSeparators);

			p1 = default;
			p2 = default;

			int i = 0;
			foreach (var value in values)
			{
				switch (i)
				{
					case 0:
						To(value, out p1);
						break;
					case 1:
						To(value, out p2);
						break;
					default:
						return;
				}
				i++;
			}
		}

		public static void To<T1, T2, T3>(this ReadOnlySpan<char> text, out T1 p1, out T2 p2, out T3 p3)
		{
			var values = text.Split(DefaultListSeparators);

			p1 = default;
			p2 = default;
			p3 = default;

			int i = 0;
			foreach (var value in values)
			{
				switch (i)
				{
					case 0:
						To(value, out p1);
						break;
					case 1:
						To(value, out p2);
						break;
					case 2:
						To(value, out p3);
						break;
					default:
						return;
				}
				i++;
			}
		}

		public static void To<T1, T2, T3, T4>(this ReadOnlySpan<char> text, out T1 p1, out T2 p2, out T3 p3, out T4 p4)
		{
			var values = text.Split(DefaultListSeparators);

			p1 = default;
			p2 = default;
			p3 = default;
			p4 = default;

			int i = 0;
			foreach (var value in values)
			{
				switch (i)
				{
					case 0:
						To(value, out p1);
						break;
					case 1:
						To(value, out p2);
						break;
					case 2:
						To(value, out p3);
						break;
					case 3:
						To(value, out p4);
						break;
					default:
						return;
				}
				i++;
			}
		}

		public static void To<T1, T2, T3, T4, T5>(this ReadOnlySpan<char> text, out T1 p1, out T2 p2, out T3 p3, out T4 p4, out T5 p5)
		{
			var values = text.Split(DefaultListSeparators);

			p1 = default;
			p2 = default;
			p3 = default;
			p4 = default;
			p5 = default;

			int i = 0;
			foreach (var value in values)
			{
				switch (i)
				{
					case 0:
						To(value, out p1);
						break;
					case 1:
						To(value, out p2);
						break;
					case 2:
						To(value, out p3);
						break;
					case 3:
						To(value, out p4);
						break;
					case 4:
						To(value, out p5);
						break;
					default:
						return;
				}
				i++;
			}
		}
	}
}
