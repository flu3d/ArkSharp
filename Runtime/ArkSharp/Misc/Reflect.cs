using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ArkSharp
{
	/// <summary>
	/// 反射工具箱
	/// </summary>
	public static class Reflect
	{
		#region 类型判断和转换

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsNullable(this Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

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

		#endregion

		#region 字段/属性查找和读写

		/// <summary>
		/// 过滤并获取属性或字段
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IReadOnlyList<MemberInfo> GetPropertyOrFields(this Type objType, BindingFlags flags, bool inherit = false, Func<MemberInfo, bool> predicate = null)
		{
			var result = new List<MemberInfo>();
			GetPropertyOrFields(objType, flags, ref result, inherit, predicate);
			return result;
		}

		/// <summary>
		/// 过滤并获取属性或字段
		/// </summary>
		public static void GetPropertyOrFields(this Type objType, BindingFlags flags, ref List<MemberInfo> result, bool inherit = false, Func<MemberInfo, bool> predicate = null)
		{
			flags |= BindingFlags.DeclaredOnly;

			if (result == null)
				result = new List<MemberInfo>();

			// 获取并过滤所有成员
			var members = objType.GetMembers(flags);
			for (int i = 0; i < members.Length; i++)
			{
				var member = members[i];
				if (member == null)
					continue;
				if (member.MemberType != MemberTypes.Property && member.MemberType != MemberTypes.Field)
					continue;
				if (predicate != null && !predicate(member))
					continue;

				result.Add(member);
			}

			// 向上查找父类型的属性和字段，尾递归直接用循环处理
			if (inherit)
			{
				objType = objType.BaseType;

				while (objType != null)
				{
					GetPropertyOrFields(objType, flags, ref result, false, predicate);
					objType = objType.BaseType;
				}
			}
		}

		/// <summary>
		/// 获取指定名字的属性或字段
		/// </summary>
		public static MemberInfo GetPropOrField(this Type objType, string name, BindingFlags flags, bool inherit = false)
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
					var result = GetPropOrField(objType, name, flags, false);
					if (result != null)
						return result;

					objType = objType.BaseType;
				}
			}

			return null;
		}

		/// <summary>
		/// 通过反射为数据变量赋值，支持属性和字段
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetValue(this MemberInfo member, object instanceObj, object value)
		{
			if (member == null)
				return;

			switch (member.MemberType)
			{
				case MemberTypes.Property:
					var prop = (PropertyInfo)member;
					prop.SetValue(instanceObj, ChangeType(value, prop.PropertyType), null);
					break;
				case MemberTypes.Field:
					var field = (FieldInfo)member;
					field.SetValue(instanceObj, ChangeType(value, field.FieldType));
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// 获取首个指定类型Attribute
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetFirstAttribute<T>(this MemberInfo element, bool inherit = false) where T : Attribute
		{
			var attributes = element.GetCustomAttributes(typeof(T), inherit);
			if (attributes == null || attributes.Length <= 0)
				return null;

			return (T)attributes[0];
		}

		/// <summary>
		/// 检查是否有某个指定类型Attribute
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasAttribute<T>(this MemberInfo element, bool inherit = false) where T : Attribute
		{
			var attributes = element.GetCustomAttributes(typeof(T), inherit);
			return attributes != null && attributes.Length > 0;
		}

		#endregion

		#region 程序集和类型遍历相关接口

		/// <summary>
		/// 跨程序集查找指定类型
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Type FindType(string fullName, bool ignoreCase = false)
		{
			return GetAllTypes().FirstOrDefault(t =>
					string.Compare(t.FullName, fullName, ignoreCase) == 0);
		}

		/// <summary>
		/// 检查是否用户程序集
		/// (简单过滤是否在SystemAssemblyPrefixList内)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsUserAssembly(Assembly assembly)
		{
			var name = assembly.FullName;

			foreach (var prefix in SystemAssemblyPrefixList)
			{
				if (name.StartsWith(prefix))
					return false;
			}

			return true;
		}

		/// <summary>
		/// 获取可加载类型
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
		{
			try
			{
				return assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				// Gets the array of classes that were defined in the module and loaded.
				return e.Types.Where(t => t != null);
			}
		}

		/// <summary>
		/// 获取所有类型定义，允许指定只收集用户程序级
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<Type> GetAllTypes(bool userAssemblyOnly = true)
		{
			return AppDomain.CurrentDomain.GetAssemblies()
					.Where(asm => !userAssemblyOnly || IsUserAssembly(asm))
					.SelectMany(asm => asm.GetLoadableTypes());
		}

		public static readonly string[] SystemAssemblyPrefixList = new string[] {
			"mscorlib","netstandard","System.", "Mono.","Microsoft.",
			"Unity.","UnityEngine.","UnityEditor.", "AssetStoreTools",
			"I18N", "ICSharpCode.", "nunit.", "SyntaxTree.",
		};

		#endregion
	}
}
