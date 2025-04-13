using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Ark
{
	/// <summary>
	/// 字符串转换工具箱
	/// </summary>
	public static partial class StringConvert
	{
		/// <summary>
		/// 枚举忽略大小写
		/// </summary>
		public static bool EnumIgnoreCase = false;

		public static readonly char[] DefaultListSeparators = new char[] { ',', ';', '|' };
		public static readonly char[] DefaultDictSeparators = new char[] { ':', '=' };

		/// <summary>
		/// 通用类型转换，支持容器类型，如果转换失败则返回默认值
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static object To(this string text, Type type)
		{
			if (type == typeof(string))
				return text;

			// 自定义转换器
			var parser = _parsers.GetValueOrDefault(type);
			if (parser != null)
				return parser(text);

			// 处理枚举
			if (type.IsEnum)
				return ToEnum(text, type);

			// 处理容器类型
			if (type.IsArray)
				return ToArray(text, type);
			else if (type.IsList())
				return ToList(text, type);
			else if (type.IsDictionary())
				return ToDictionary(text, type);

			// 回退系统类型转换
			return Convert.ChangeType(text, type, CultureInfo.InvariantCulture);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Array ToArray(this string text, Type type)
		{
			var vals = text.Split(DefaultListSeparators);

			var elementType = type.GetElementType();
			var result = Array.CreateInstance(elementType, vals.Length);

			for (int i = 0; i < vals.Length; i++)
				result.SetValue(To(vals[i], elementType), i);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static IList ToList(this string text, Type type)
		{
			var vals = text.Split(DefaultListSeparators);

			var elementType = Reflect.GetGenericArg0(type);
			var result = (IList)Activator.CreateInstance(type);

			for (int i = 0; i < vals.Length; i++)
				result.Add(To(vals[i], elementType));

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static IDictionary ToDictionary(this string text, Type type)
		{
			var vals = text.Split(DefaultListSeparators);

			var keyType = Reflect.GetGenericArg0(type);
			var valType = Reflect.GetGenericArg1(type);
			var result = (IDictionary)Activator.CreateInstance(type);

			foreach (var pair in vals)
			{
				var kv = pair.Split(DefaultDictSeparators, 2);
				if (kv.Length < 2)
					continue;

				var key = To(kv[0], keyType);
				var val = To(kv[1], valType);
				result.Add(key, val);
			}

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static object ToEnum(this string text, Type type)
		{
			object result = null;

			// use Enum.TryParse
			if (!Enum.TryParse(type, text, out result) || !Enum.IsDefined(type, result))
				result = null;
	
			// try XXXX.XXXX or XXXXType.XXXX
			if (result == null)
			{
				var enumTypeName = type.Name;

				if (enumTypeName.EndsWith("Type"))
					enumTypeName = enumTypeName.Substring(0, enumTypeName.Length - 4);

				try
				{
					result = Enum.Parse(type, enumTypeName + text, EnumIgnoreCase);
				}
				catch (Exception)
				{
					result = null;
				}
			}

			// try text => int => Enum
			if (result == null)
			{
				if (int.TryParse(text, out var val))
				{
					var enumVal = Enum.ToObject(type, val);

					if (Enum.IsDefined(type, enumVal))
						result = enumVal;
				}
			}

			if (result == null)
				return Activator.CreateInstance(type);

			return result;
		}

		static StringConvert()
		{
			AddParser<char>(char.TryParse);
			AddParser<sbyte>(sbyte.TryParse);
			AddParser<byte>(byte.TryParse);

			AddParser<short>(short.TryParse);
			AddParser<ushort>(ushort.TryParse);
			AddParser<int>(int.TryParse);
			AddParser<uint>(uint.TryParse);
			AddParser<long>(long.TryParse);
			AddParser<ulong>(ulong.TryParse);

			AddParser<float>(float.TryParse);
			AddParser<double>(double.TryParse);

			AddParser<bool>(TryParseBool);
		}

		public delegate bool ParseFunc<T>(string text, out T result);

		/// <summary>
		/// 添加自定义类型转换器
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AddParser<T>(ParseFunc<T> func)
		{
			ParserHolder<T>.func = func;
			_parsers[typeof(T)] = ParserHolder<T>.ConvertToObject;
		}

		private static bool TryParseBool(string s, out bool result)
		{
			result = default;

			if (string.IsNullOrEmpty(s))
				return false;

			s = s.Trim();

			if (s == "1")
			{
				result = true;
				return true;
			}

			if (s == "0")
			{
				result = false;
				return true;
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

		static class ParserHolder<T>
		{
			public static ParseFunc<T> func;

			public static object ConvertToObject(string text)
			{
				if (func == null)
					return default;

				func.Invoke(text, out var result);
				return result;
			}
		}

		private static readonly Dictionary<Type, Func<string, object>> _parsers = new Dictionary<Type, Func<string, object>>();
	}
}
