using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Ark.Data
{
	internal class MemberInfoEx
	{
		public readonly PropertyInfo prop;
		public readonly FieldInfo field;
		public readonly Type memberType;
		public readonly Type originType;
		public readonly Type elementType;
		public readonly bool isList;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemberInfoEx(MemberInfo member)
		{
			// 属性或字段
			switch (member.MemberType)
			{
				case MemberTypes.Property:
					prop = (PropertyInfo)member;
					memberType = prop.PropertyType;
					break;
				case MemberTypes.Field:
					field = (FieldInfo)member;
					memberType = field.FieldType;
					break;
			}

			// 枚举类型转换
			var attrib = member.GetCustomAttribute<SerializeFieldAttribute>();
			if (attrib != null && attrib.originType != null)
				originType = attrib.originType;

			// 列表
			if (memberType.IsGenericType &&
				(typeof(IList).IsAssignableFrom(memberType) || memberType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)))
			{
				isList = true;
				elementType = Reflect.GetGenericArg0(memberType);
				memberType = typeof(List<>).MakeGenericType(elementType);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetValue(object instanceObj, string value, bool stringPooling)
		{
			object result;

			if (isList)
			{
				// 处理列表
				var currentValue = GetValue(instanceObj);

				var list = currentValue as IList;
				if (list == null)
					list = Activator.CreateInstance(memberType) as IList;
				else 
					list.Clear();

				var vals = value.Split(Table.Config.ListSeparators);

				int count = vals.Length;
				if (count == 1)
				{
					var s0 = vals[0];
					if (string.IsNullOrEmpty(s0) ||  // 分割后仅有一个空元素
						(string.IsNullOrWhiteSpace(s0) && elementType != typeof(string)))   // 分割后仅有一个空白元素，且转换目标不是字符串
					{
						count = 0; // 忽略整个列表
					}
				}

				for (int i = 0; i < count; i++)
				{
					object val = null;
					if (originType != null)
					{
						val = vals[i].To(originType);
						val = Convert.ChangeType(val, elementType);
					}
					else
					{
						val = vals[i].To(elementType);
					}

					if (stringPooling)
						val = Table.StringPool.Intern(val);

					list.Add(val);
				}

				result = list;
			}
			else
			{
				// 处理普通成员
				if (originType != null)
				{
					var val = value.To(originType);
					result = Convert.ChangeType(val, memberType);
				}
				else
				{
					result = value.To(memberType);
				}

				if (stringPooling)
					result = Table.StringPool.Intern(result);
			}

			SetValue(instanceObj, result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetValue(object instanceObj, object value)
		{
			prop?.SetValue(instanceObj, value);
			field?.SetValue(instanceObj, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public object GetValue(object instanceObj)
		{
			if (prop != null)
				return prop.GetValue(instanceObj);
			if (field != null)
				return field.GetValue(instanceObj);

			return null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static MemberInfoEx[] GetMembers(Type objType, string[] names)
		{
			if (objType == null || names == null)
				return null;

			int count = names.Length;
			var result = new MemberInfoEx[count];

			for (int i = 0; i < count; ++i)
				result[i] = GetMember(objType, names[i]);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static MemberInfoEx GetMember(Type objType, string name)
		{
			if (objType == null || name == null)
				return null;

			// 去掉字段名前缀特殊字符
			var ch = name[0];
			if (ch == Record.PrefixComment || 
				ch == Record.PrefixL10n ||
				ch == Record.PrefixDataSlot)
				name = name.Substring(1);

			name = name.Trim();

			var pubFlags = BindingFlags.Public | BindingFlags.Instance;

			// 先搜索公开属性
			var prop = objType.GetProperty(name, pubFlags);
			if (prop != null && prop.CanRead && prop.CanWrite)
				return new MemberInfoEx(prop);

			// 再搜索公开字段
			var field = objType.GetField(name, pubFlags);
			if (field != null)
				return new MemberInfoEx(field);

			// 搜索私有字段，允许带下划线前缀
			var pfields = objType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (var pfield in pfields)
			{
				int startPos = pfield.Name[0] == '_' ? 1 : 0;
				if (string.Compare(pfield.Name, startPos, name, 0, name.Length, true) == 0)
					return new MemberInfoEx(pfield);
			}

			return null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IReadOnlyList<MemberInfoEx> GetSerializeFields(Type objType)
		{
			if (objType == null)
				return null;

			var result = new List<MemberInfoEx>();

			var members = objType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			for (int i = 0; i < members.Length; i++)
			{
				var member = members[i];
				if (member.MemberType != MemberTypes.Property &&
					member.MemberType != MemberTypes.Field)
					continue;

				var attrib = member.GetCustomAttribute<SerializeFieldAttribute>();
				if (attrib != null)
					result.Add(new MemberInfoEx(member));
			}	

			return result;
		}
	}
}
