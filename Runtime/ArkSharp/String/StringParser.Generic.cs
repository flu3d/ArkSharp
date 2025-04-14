using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ArkSharp
{
	public static partial class StringParser
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
		/// 通用类型转换，支持容器类型，如果转换失败则返回默认值
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T To<T>(this ReadOnlySpan<char> text)
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
		public static void To<T>(this ReadOnlySpan<char> text, ref T[] result, char[] separator = null)
		{
			// 先转为List再转数组
			var list = new List<T>();
			To(text, ref list, separator);

			result = list.ToArray();
		}

		/// <summary>
		/// 字符串转List，如果result为空则新建
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void To<T>(this ReadOnlySpan<char> text, ref List<T> result, char[] separator = null)
		{
			if (result == null)
				result = new List<T>();
			else
				result.Clear();

			text = TrimStartEnd(text, DefaultListStartEnd);

			if (text.IsEmpty)
				return;

			var vals = text.Split(separator ?? DefaultListSeparators);
			foreach (var val in vals)
				result.Add(val.To<T>());
		}

		/// <summary>
		/// 字符串转Dictionary，如果result为空则新建
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void To<K, V>(this ReadOnlySpan<char> text, ref Dictionary<K, V> result, char[] separator = null, char[] separatorKV = null)
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
		public static void To<K, V>(this ReadOnlySpan<char> text, ref SortedDictionary<K, V> result, char[] separator = null, char[] separatorKV = null)
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
		public static void To<K, V>(this ReadOnlySpan<char> text, ref MultiDictionary<K, V> result, char[] separator = null, char[] separatorKV = null)
		{
			if (result == null)
				result = new MultiDictionary<K, V>();
			else
				result.Clear();

			ToDictionaryImpl<K, V>(text, result.Add, separator, separatorKV);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ToDictionaryImpl<K, V>(ReadOnlySpan<char> text, Action<K, V> addDictFunc, char[] separator, char[] separatorKV)
		{
			text = TrimStartEnd(text, DefaultDictStartEnd);

			if (text.IsEmpty)
				return;

			if (separatorKV == null)
				separatorKV = DefaultDictSeparators;

			var pairs = text.Split(separator ?? DefaultListSeparators);
			foreach (var pair in pairs)
			{
				var kvs = pair.Split(DefaultDictSeparators, 2);

				var key = default(K);
				var val = default(V);

				int i = 0;
				foreach (var kv in kvs)
				{
					if (i == 0)
					{
						key = To<K>(kv);
						i++;
					}
					else // i == 1
					{
						val = To<V>(kv);
						break;
					}
				}

				addDictFunc(key, val);
			}
		}
	}
}
