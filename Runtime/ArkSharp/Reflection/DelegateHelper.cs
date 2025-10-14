using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ArkSharp
{
    /// <summary>
    /// 委托相关工具
    /// </summary>
    public static class DelegateHelper
    {
        /// <summary>
        /// 创建委托
        /// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T CreateDelegate<T>(this MethodInfo method, object target) where T : Delegate
        {
            return (T)method.CreateDelegate(typeof(T), target);
        }

        /// <summary>
        /// 创建委托
        /// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T CreateDelegate<T>(this MethodInfo method) where T : Delegate
        {
            return (T)method.CreateDelegate(typeof(T));
        }

        /// <summary>
        /// 创建委托，创建失败不会抛出异常
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Delegate CreateDelegateNoThrow(this MethodInfo method, Type type, object target)
        {
            return Delegate.CreateDelegate(type, target, method, false);
        }

        /// <summary>
        /// 创建委托，创建失败不会抛出异常
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T CreateDelegateNoThrow<T>(this MethodInfo method, object target) where T : Delegate
        {
            return Delegate.CreateDelegate(typeof(T), target, method, false) as T;
        }
    }
}
