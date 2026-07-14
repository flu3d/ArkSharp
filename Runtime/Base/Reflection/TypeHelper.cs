using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace ArkSharp
{
	/// <summary>
	/// 类型相关工具
	/// </summary>
	public static class TypeHelper
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsNullable(this Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsStatic(this Type type) => type.IsAbstract && type.IsSealed;

		// Type.IsArray已经内置，无需定义扩展函数

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsList(this Type type) => typeof(IList).IsAssignableFrom(type);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsDictionary(this Type type) => typeof(IDictionary).IsAssignableFrom(type);

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Type GetGenericArg0(this Type type) => GetGenericArgAt(type, 0);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Type GetGenericArg1(this Type type) => GetGenericArgAt(type, 1);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Type GetGenericArg2(this Type type) => GetGenericArgAt(type, 2);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Type GetGenericArgAt(this Type type, int index)
		{
			if (!type.IsGenericType)
				return null;

			var arguments = type.GetGenericArguments();
			if (arguments == null || arguments.Length <= 0 || index >= arguments.Length)
				return null;

			return arguments[index];
		}

		/// 类型转换，支持Nullable
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T ChangeType<T>(object value, IFormatProvider provider = null) => (T)ChangeType(value, typeof(T), provider);

		/// 类型转换，支持Nullable
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static object ChangeType(object value, Type conversionType, IFormatProvider provider = null)
		{
			if (value == null || value.GetType() == conversionType)
				return value;

			if (IsNullable(conversionType))
				conversionType = GetGenericArg0(conversionType);

			// TODO 特化枚举类型

			return Convert.ChangeType(value, conversionType, provider ?? CultureInfo.InvariantCulture);
		}

		private static readonly Dictionary<Type, string> _friendlyTypeNames = new()
		{
			{ typeof(byte), "byte" },
			{ typeof(sbyte), "sbyte" },
			{ typeof(bool), "bool" },
			{ typeof(short), "short" },
			{ typeof(ushort), "ushort" },
			{ typeof(int), "int" },
			{ typeof(uint), "uint" },
			{ typeof(long), "long" },
			{ typeof(ulong), "ulong" },
			{ typeof(float), "float" },
			{ typeof(double), "double" },
			{ typeof(decimal), "decimal" },
			{ typeof(char), "char" },
			{ typeof(string), "string" },
			{ typeof(object), "object" },
			{ typeof(void), "void" }
		};

		/// <summary>
		/// 获取更可读的类型名称
		/// </summary>
		public static string GetFriendlyName(this Type type)
		{
			if (type == null)
				return "null";

			if (_friendlyTypeNames.TryGetValue(type, out var friendlyName))
				return friendlyName;

			if (type.IsGenericType)
				return $"{type.Name[..type.Name.IndexOf('`')]}<{string.Join(", ", type.GetGenericArguments().Select(x => GetFriendlyName(x)))}>";

			return type.Name;
		}

		/// <summary>
		/// 过滤并获取属性或字段
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IReadOnlyList<MemberInfo> GetPropertyOrFields(this Type objType, BindingFlags flags, bool inherit = false, Func<MemberInfo, bool> predicate = null)
		{
			return EnumerateMembers(objType, MemberTypes.Property | MemberTypes.Field, flags, inherit, predicate).ToArray();
		}

		/// <summary>
		/// 获取指定名字的属性或字段
		/// </summary>
		public static MemberInfo GetPropertyOrField(this Type objType, string name, BindingFlags flags, bool inherit = false)
		{
			if (objType == null || string.IsNullOrEmpty(name))
				return null;

			flags |= BindingFlags.DeclaredOnly;

			// 优先查找属性
			var prop = objType.GetProperty(name, flags);
			if (prop != null)
				return prop;

			// 属性找不到才找字段
			var field = objType.GetField(name, flags);
			if (field != null)
				return field;

			// 向上查找父类型的属性和字段，尾递归直接用循环处理
			if (inherit)
			{
				objType = objType.BaseType;

				while (objType != null)
				{
					var result = GetPropertyOrField(objType, name, flags, false);
					if (result != null)
						return result;

					objType = objType.BaseType;
				}
			}

			return null;
		}

		/// <summary>
		/// 枚举类型成员（属性、字段、方法等）
		/// </summary>
		public static IEnumerable<MemberInfo> EnumerateMembers(this Type objType, MemberTypes memberType, BindingFlags flags, bool inherit = false, Func<MemberInfo, bool> predicate = null)
		{
			if (objType == null)
				yield break;

			flags |= BindingFlags.DeclaredOnly;

			// 获取并过滤所有成员
			var members = objType.GetMembers(flags);
			for (int i = 0; i < members.Length; i++)
			{
				var member = members[i];

				if (member == null)
					continue;
				if (!memberType.HasFlag(member.MemberType))
					continue;
				if (predicate != null && !predicate(member))
					continue;

				yield return member;
			}

			// 向上查找父类型，尾递归直接用循环处理
			if (inherit)
			{
				objType = objType.BaseType;

				while (objType != null)
				{
					var parentMembers = EnumerateMembers(objType, memberType, flags, false, predicate);
					foreach (var member in parentMembers)
						yield return member;

					objType = objType.BaseType;
				}
			}
		}
	}
}
