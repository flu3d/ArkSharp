using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ArkSharp
{
	/// <summary>
	/// 通用字符串格式化
	/// 提供比默认object.ToString更定制化的实现
	/// </summary>
	public static partial class StringFormatter
	{
		public const string DefaultListFormatingSeparator = ",";
		public const string DefaultDictFormatingSeparator = ":";

		/// <summary>
		/// 通用类型转字符串，支持容器类型，值类型会触发装箱操作
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToString(object value)
		{
			if (value == null)
				return null;

			// 字符串无需转换
			var type = value.GetType();
			if (type == typeof(string))
				return (string)value;

			// 自定义转换器
			var formatter = _formatters.GetValueOrDefault(type);
			if (formatter != null)
				return formatter(value);

			// 容器类型
			if (type.IsArray || type.IsList())
				return ToString((IList)value);
			if (type.IsDictionary())
				return ToString((IDictionary)value);

			// 回退到系统字符串转换
			return value.ToString();
		}

		/// <summary>
		/// bool型转字符串，默认转为1和0
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToString(bool val)
		{
			return val ? "1" : "0";
		}

		/// <summary>
		/// 数组和List转换为字符串
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToString<T>(IReadOnlyList<T> list, string separator = DefaultListFormatingSeparator)
		{
			if (list == null || list.Count == 0)
				return string.Empty;

			var sb = new StringBuilder();
			for (int i = 0; i < list.Count; i++)
			{
				sb.Append(ToString(list[i]));
				sb.Append(separator);
			}

			return sb.ToString(0, sb.Length - 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToString(IList list, string separator = DefaultListFormatingSeparator)
		{
			if (list == null || list.Count == 0)
				return string.Empty;

			var sb = new StringBuilder();
			for (int i = 0; i < list.Count; i++)
			{
				sb.Append(ToString(list[i]));
				sb.Append(separator);
			}

			return sb.ToString(0, sb.Length - 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToString<K, V>(IReadOnlyDictionary<K, V> dict, string separator = DefaultListFormatingSeparator, string separatorKV = DefaultDictFormatingSeparator)
		{
			if (dict == null || dict.Count == 0)
				return string.Empty;

			var sb = new StringBuilder();
			foreach (var kv in dict)
			{
				sb.Append(ToString(kv.Key));
				sb.Append(separatorKV);
				sb.Append(ToString(kv.Value));
				sb.Append(separator);
			}

			return sb.ToString(0, sb.Length - 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToString(IDictionary dict, string separator = DefaultListFormatingSeparator, string separatorKV = DefaultDictFormatingSeparator)
		{
			if (dict == null || dict.Count == 0)
				return string.Empty;

			var sb = new StringBuilder();
			var kv = dict.GetEnumerator();
			while (kv.MoveNext())
			{
				sb.Append(ToString(kv.Key));
				sb.Append(separatorKV);
				sb.Append(ToString(kv.Value));
				sb.Append(separator);
			}

			return sb.ToString(0, sb.Length - 1);
		}

		static StringFormatter()
		{
			SetFormatter<bool>(ToString);
		}

		/// <summary>
		/// 添加自定义字符串格式化
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetFormatter<T>(Func<T, string> func)
		{
			FormaterHolder<T>.func = func;
			_formatters[typeof(T)] = FormaterHolder<T>.ConvertToString;
		}

		static class FormaterHolder<T>
		{
			public static Func<T, string> func;

			public static string ConvertToString(object value) => func.Invoke((T)value);
		}

		private static readonly Dictionary<Type, Func<object, string>> _formatters = new Dictionary<Type, Func<object, string>>();
	}
}
