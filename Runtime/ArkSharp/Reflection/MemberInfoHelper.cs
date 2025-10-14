using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ArkSharp
{
    /// <summary>
    /// 成员信息相关工具
    /// </summary>
    public static class MemberInfoHelper
    {
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
                    prop.SetValue(instanceObj, TypeHelper.ChangeType(value, prop.PropertyType), null);
                    break;
                case MemberTypes.Field:
                    var field = (FieldInfo)member;
                    field.SetValue(instanceObj, TypeHelper.ChangeType(value, field.FieldType));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 获取成员类型，支持属性和字段
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type GetPropertyOrFieldType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
            }

            return null;
        }

		/// <summary>
		/// 获取首个指定类型Attribute
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetFirstAttribute<T>(this MemberInfo member, bool inherit = false) where T : Attribute
		{
			var attributes = member.GetCustomAttributes(typeof(T), inherit);
			if (attributes == null || attributes.Length <= 0)
				return null;

			return (T)attributes[0];
		}

        /// <summary>
        /// 检查是否有某个指定类型Attribute
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAttribute<T>(this MemberInfo member, bool inherit = false) where T : Attribute
        {
            return member.IsDefined(typeof(T), inherit);
        }
    }
}
