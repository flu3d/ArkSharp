using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ArkSharp
{
	/// <summary>
	/// 枚举工具箱
	/// </summary>
	public static class EnumHelper
	{
		/// <summary>
		/// 枚举是否忽略大小写
		/// </summary>
		public static bool EnumIgnoreCase = false;

		/// <summary>
		/// 组合枚举分割字符
		/// </summary>
		public static char[] EnumSplitChars = new char[] { '|', ',' };

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Combine<T>(IReadOnlyList<T> enumFlags) where T : Enum
		{
			int result = 0;

			for (int i = 0; i < enumFlags.Count; i++)
				result |= (int)(object)enumFlags[i];

			return (T)(object)result;
		}

		/// <summary>
		/// 字符串转换为枚举，支持用|分割多个Flags型枚举组合结果
		/// </summary>
		/// <param name="checkAlias">是否先尝试别名，再尝试枚举名</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T To<T>(this string text, bool checkAlias = true) where T : Enum
		{
			return (T)Parse(text, typeof(T), checkAlias);
		}

		/// <summary>
		/// 字符串转换为枚举，支持用|分割多个Flags型枚举组合结果
		/// </summary>
		/// <param name="checkAlias">是否先尝试别名，再尝试枚举名</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T To<T>(this ReadOnlySpan<char> text, bool checkAlias = true) where T : Enum
		{
			return (T)Parse(text, typeof(T), checkAlias);
		}

		/// <summary>
		/// 字符串转换为枚举，支持用|分割多个Flags型枚举组合结果
		/// </summary>
		/// <param name="checkAlias">是否先尝试别名，再尝试枚举名</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static object Parse(ReadOnlySpan<char> text, Type enumType, bool checkAlias = true)
		{
			if (text.IsEmpty)
				return Enum.ToObject(enumType, 0);

			// 单个枚举
			if (text.IndexOfAny(EnumSplitChars) < 0)
				return ParseOne(text, enumType, checkAlias);

			// 组合枚举
			var result = 0L;

			var enumList = text.Split(EnumSplitChars, StringSplitOptions.RemoveEmptyEntries);
			foreach (var enumItem in enumList)
			{
				if (enumItem.IsEmpty)
					continue;

				var d = ParseOne(enumItem, enumType, checkAlias);
				if (d != null)
					result |= Convert.ToInt64(d);
			}

			return Enum.ToObject(enumType, result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static object ParseOne(ReadOnlySpan<char> text, Type enumType, bool checkAlias)
		{
			if (text.IsEmpty)
				return Enum.ToObject(enumType, 0);

			// 如果是数字格式先尝试转换
			var ch = text[0];
			if (char.IsDigit(ch) || ch == '-' || ch == '+')
			{
				long.TryParse(text, out var val2);
				return Enum.ToObject(enumType, val2);
			}

			var comparision = EnumIgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

			// 遍历所有成员逐一检查
			var members = GetMembers(enumType);
			for (int i = 0; i < members.Count; i++)
			{
				var d = members[i];

				// 先查询别名
				if (checkAlias && d.alias != null &&
					text.Equals(d.alias, comparision))
					return d.value;

				// 再查询名字
				if (text.Equals(d.name, comparision))
					return d.value;
			}

			// 查找不到返回0
			return Enum.ToObject(enumType, 0);
		}

		/// <summary>
		/// 获取缓存的枚举成员信息
		/// </summary>
		public static IReadOnlyList<EnumMember> GetMembers(Type enumType)
		{
			if (!enumType.IsEnum)
				return null;

			if (_cacheEnumMembers.TryGetValue(enumType, out var members))
				return members;

			var names = Enum.GetNames(enumType);
			var values = Enum.GetValues(enumType);

			var array = new EnumMember[names.Length];
			for (int i = 0; i < array.Length; i++)
			{
				string name = names[i];
				string alias = null;

#if UNITY_5_3_OR_NEWER
				var field = enumType.GetField(name, BindingFlags.Static | BindingFlags.Public);
				alias = field?.GetCustomAttribute<UnityEngine.InspectorNameAttribute>()?.displayName;
#else
				// TODO
#endif

				array[i] = new EnumMember(name, alias, values.GetValue(i));
			}

			_cacheEnumMembers[enumType] = array;
			return array;
		}

		/// <summary>
		/// 缓存枚举成员信息：名字、数值、别名
		/// </summary>
		public class EnumMember
		{
			internal EnumMember(string name, string alias, object value)
			{
				this.name = name;
				this.alias = alias;
				this.value = value;
			}

			public readonly string name;
			public readonly string alias;
			public readonly object value;
		}

		static readonly Dictionary<Type, IReadOnlyList<EnumMember>> _cacheEnumMembers = new Dictionary<Type, IReadOnlyList<EnumMember>>();
	}
}
