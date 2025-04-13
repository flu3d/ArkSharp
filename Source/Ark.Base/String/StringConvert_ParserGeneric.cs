using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ark
{
	public static partial class StringConvert
	{
		/// <summary>
		/// 通用类型转换，支持容器类型，如果转换失败则返回默认值
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T To<T>(this string text)
		{
			if (ParserHolder<T>.func != null)
			{
				ParserHolder<T>.func(text, out var result);
				return result;
			}

			return (T)text.To(typeof(T));
		}

		/// <summary>
		/// 字符串转数组，如果result为空或长度不匹配则新建
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void To<T>(this string text, ref T[] result, char[] separator = null)
		{
			if (string.IsNullOrEmpty(text))
			{
				result = new T[0];
				return;
			}

			var vals = text.Split(separator ?? DefaultListSeparators);
			result = new T[vals.Length];

			for (int i = 0; i < vals.Length; i++)
				result[i] = vals[i].To<T>();
		}

		/// <summary>
		/// 字符串转List，如果result为空则新建
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void To<T>(this string text, ref List<T> result, char[] separator = null)
		{
			if (result == null)
				result = new List<T>();
			else
				result.Clear();

			if (string.IsNullOrEmpty(text))
				return;

			var vals = text.Split(separator ?? DefaultListSeparators);

			result.Capacity = vals.Length;
			for (int i = 0; i < vals.Length; i++)
				result.Add(vals[i].To<T>());
		}

		/// <summary>
		/// 字符串转Dictionary，如果result为空则新建
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void To<K, V>(this string text, ref Dictionary<K, V> result, char[] separator = null, char[] separatorKV = null)
		{
			if (result == null)
				result = new Dictionary<K, V>();
			else
				result.Clear();

			ToDictionaryImpl<K, V>(text, result.Add, separator, separatorKV);
		}

		/// <summary>
		/// 字符串转SortedDictionary，如果result为空则新建
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void To<K, V>(this string text, ref SortedDictionary<K, V> result, char[] separator = null, char[] separatorKV = null)
		{
			if (result == null)
				result = new SortedDictionary<K, V>();
			else
				result.Clear();

			ToDictionaryImpl<K, V>(text, result.Add, separator, separatorKV);
		}

		/// <summary>
		/// 字符串转MultiDictionary，如果result为空则新建
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void To<K, V>(this string text, ref MultiDictionary<K, V> result, char[] separator = null, char[] separatorKV = null)
		{
			if (result == null)
				result = new MultiDictionary<K, V>();
			else
				result.Clear();

			ToDictionaryImpl<K, V>(text, result.Add, separator, separatorKV);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ToDictionaryImpl<K, V>(string text, Action<K, V> addDictFunc, char[] separator, char[] separatorKV)
		{
			if (string.IsNullOrEmpty(text))
				return;

			if (separatorKV == null)
				separatorKV = DefaultDictSeparators;

			var pairs = text.Split(separator ?? DefaultListSeparators);
			foreach (var pair in pairs)
			{
				string[] kv = pair.Split(separatorKV, 2);
				if (kv.Length < 2)
					continue;

				var key = kv[0].To<K>();
				var val = kv[1].To<V>();
				addDictFunc(key, val);
			}
		}

		#region Tuples

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void To<T1>(this string text, out T1 p1)
		{
			p1 = To<T1>(text);
		}

		public static void To<T1, T2>(this string text, out T1 p1, out T2 p2)
		{
			var values = SplitValues(text);

			ArrayValueTo(values, 0, out p1);
			ArrayValueTo(values, 1, out p2);
		}

		public static void To<T1, T2, T3>(this string text, out T1 p1, out T2 p2, out T3 p3)
		{
			var values = SplitValues(text);

			ArrayValueTo(values, 0, out p1);
			ArrayValueTo(values, 1, out p2);
			ArrayValueTo(values, 2, out p3);
		}

		public static void To<T1, T2, T3, T4>(this string text, out T1 p1, out T2 p2, out T3 p3, out T4 p4)
		{
			var values = SplitValues(text);

			ArrayValueTo(values, 0, out p1);
			ArrayValueTo(values, 1, out p2);
			ArrayValueTo(values, 2, out p3);
			ArrayValueTo(values, 3, out p4);
		}

		public static void To<T1, T2, T3, T4, T5>(this string text, out T1 p1, out T2 p2, out T3 p3, out T4 p4, out T5 p5)
		{
			var values = SplitValues(text);

			ArrayValueTo(values, 0, out p1);
			ArrayValueTo(values, 1, out p2);
			ArrayValueTo(values, 2, out p3);
			ArrayValueTo(values, 3, out p4);
			ArrayValueTo(values, 4, out p5);
		}

		public static void To<T1, T2, T3, T4, T5, T6>(this string text, out T1 p1, out T2 p2, out T3 p3, out T4 p4, out T5 p5, out T6 p6)
		{
			var values = SplitValues(text);

			ArrayValueTo(values, 0, out p1);
			ArrayValueTo(values, 1, out p2);
			ArrayValueTo(values, 2, out p3);
			ArrayValueTo(values, 3, out p4);
			ArrayValueTo(values, 4, out p5);
			ArrayValueTo(values, 5, out p6);
		}

		public static void To<T1, T2, T3, T4, T5, T6, T7>(this string text, out T1 p1, out T2 p2, out T3 p3, out T4 p4, out T5 p5, out T6 p6, out T7 p7)
		{
			var values = SplitValues(text);

			ArrayValueTo(values, 0, out p1);
			ArrayValueTo(values, 1, out p2);
			ArrayValueTo(values, 2, out p3);
			ArrayValueTo(values, 3, out p4);
			ArrayValueTo(values, 4, out p5);
			ArrayValueTo(values, 5, out p6);
			ArrayValueTo(values, 6, out p7);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ArrayValueTo<T>(string[] values, int index, out T val)
		{
			if (values == null || index >= values.Length)
				val = default;
			else
				val = values[index].To<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static string[] SplitValues(string text)
		{
			return string.IsNullOrEmpty(text)
				? null : text.Split(DefaultListSeparators);
		}

		#endregion
	}
}
