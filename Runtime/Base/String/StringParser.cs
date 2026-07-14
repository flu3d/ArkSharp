using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace ArkSharp
{
	/// <summary>
	/// 字符串转换工具箱
	/// </summary>
	public static partial class StringParser
	{
		public static readonly char[] DefaultListSeparators = new char[] { ',', ';', '|' };
		public static readonly char[] DefaultDictSeparators = new char[] { ':' };
		public static readonly char[] DefaultListStartEnd = new char[] { '[', ']' };
		public static readonly char[] DefaultDictStartEnd = new char[] { '{', '}' };

		/// <summary>
		/// 通用类型转换，支持容器类型，如果转换失败则返回默认值
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static object To(this string text, Type type)
		{
			if (type == typeof(string))
				return text;

			if (TryParse(text, type, out var result))
				return result;

			// 回退系统类型转换
			return Convert.ChangeType(text, type, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// 通用类型转换，支持容器类型，如果转换失败则返回默认值
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static object To(this ReadOnlySpan<char> text, Type type)
		{
			if (type == typeof(string))
				return text.ToString();

			if (TryParse(text, type, out var result))
				return result;

			// 回退系统类型转换
			return Convert.ChangeType(text.ToString(), type, CultureInfo.InvariantCulture);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool TryParse(this ReadOnlySpan<char> text, Type type, out object result)
		{
			// 自定义转换器
			var parser = _parsers.GetValueOrDefault(type);
			if (parser != null)
			{
				result = parser(text);
				return true;
			}

			// 处理枚举
			if (type.IsEnum)
			{
				result = EnumHelper.Parse(text, type);
				return true;
			}

			// 处理容器类型
			if (type.IsArray)
			{
				result = ToArray(text, type);
				return true;
			}
			else if (type.IsList())
			{
				result = ToList(text, type);
				return true;
			}
			else if (type.IsDictionary())
			{
				result = ToDictionary(text, type);
				return true;
			}

			// 无自定义转换器，应回退到系统类型转换
			result = null;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Array ToArray(this ReadOnlySpan<char> text, Type type)
		{
			// 先转为List再转数组
            var listType = typeof(List<>).MakeGenericType(type.GetElementType());
			var list = ToList(text, listType);

			var elementType = type.GetElementType();
			var result = Array.CreateInstance(elementType, list.Count);

			for (int i = 0; i < list.Count; i++)
				result.SetValue(list[i], i);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IList ToList(this ReadOnlySpan<char> text, Type type)
		{
			text = TrimStartEnd(text, DefaultListStartEnd);

			var vals = text.Split(DefaultListSeparators);

			var elementType = type.GetGenericArg0();
			var result = (IList)Activator.CreateInstance(type);

			foreach (var val in vals)
				result.Add(To(val, elementType));

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IDictionary ToDictionary(this ReadOnlySpan<char> text, Type type)
		{
			text = TrimStartEnd(text, DefaultDictStartEnd);

			var keyType = type.GetGenericArg0();
			var valType = type.GetGenericArg1();
			var result = (IDictionary)Activator.CreateInstance(type);

			var pairs = text.Split(DefaultListSeparators);
			foreach (var pair in pairs)
			{
				var kvs = pair.Split(DefaultDictSeparators, 2);

				object key = null, val = null;
				int i = 0;
				foreach (var kv in kvs)
				{
					if (i == 0)
					{
						key = To(kv, keyType);
						i++;
					}
					else // i == 1
					{
						val = To(kv, valType);
						break;
					}
				}

				result.Add(key, val);
			}

			return result;
		}

		/// <summary>
		/// 移除头尾的特定标记，如数组的[]、字典的{}等
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ReadOnlySpan<char> TrimStartEnd(ReadOnlySpan<char> s, char[] startEndChars)
		{
			if (s.IsEmpty)
				return s;

			int start = 0;
			int end = s.Length - 1;

			if (s[start] == startEndChars[0])
				start++;
			if (s[end] == startEndChars[1])
				end--;

			var len = end - start + 1;
			if (len > 0)
				return s.Slice(start, len);
			
			return ReadOnlySpan<char>.Empty;
		}

		static StringParser()
		{
			SetParser<sbyte>(sbyte.TryParse);
			SetParser<byte>(byte.TryParse);
			SetParser<short>(short.TryParse);
			SetParser<ushort>(ushort.TryParse);
			SetParser<int>(int.TryParse);
			SetParser<uint>(uint.TryParse);
			SetParser<long>(long.TryParse);
			SetParser<ulong>(ulong.TryParse);

			SetParser<float>(float.TryParse);
			SetParser<double>(double.TryParse);

			SetParser<char>(TryParseChar);
			SetParser<bool>(TryParseBool);
		}

		private static bool TryParseChar(ReadOnlySpan<char> s, out char result)
		{
			if (s.IsEmpty)
			{
				result = default;
				return false;
			}

			result = s[0];
			return true;
		}

		private static bool TryParseBool(ReadOnlySpan<char> s, out bool result)
		{
			result = default;

			if (s.IsEmpty)
				return false;

			s = s.Trim();

			if (s.IsEmpty)
				return false;

			if (s.Length == 1)
			{
				switch (s[0])
				{
					case '1':
						result = true;
						return true;
					case '0':
						result = false;
						return true;
				}
			}

			if (s.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase))
			{
				result = true;
				return true;
			}

			if (s.Equals(bool.FalseString, StringComparison.OrdinalIgnoreCase))
			{
				result = false;
				return true;
			}

			return false;
		}

		public delegate bool ParseFunc<T>(ReadOnlySpan<char> s, out T result);

		/// <summary>
		/// 添加自定义类型转换器
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetParser<T>(ParseFunc<T> func)
		{
			ParserHolder<T>.func = func;
			_parsers[typeof(T)] = ParserHolder<T>.ConvertToObject;
		}

		static class ParserHolder<T>
		{
			public static ParseFunc<T> func;

			public static object ConvertToObject(ReadOnlySpan<char> s)
			{
				func.Invoke(s, out var result);
				return result;
			}
		}

		private delegate object ParseObjFunc(ReadOnlySpan<char> s);
		private static readonly Dictionary<Type, ParseObjFunc> _parsers = new Dictionary<Type, ParseObjFunc>();
	}
}
