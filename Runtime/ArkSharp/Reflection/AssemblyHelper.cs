using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ArkSharp
{
    /// <summary>
    /// 程序集相关工具
    /// </summary>
    public static partial class AssemblyHelper
    {
        /// <summary>
        /// 用户程序集名称前缀列表
        /// </summary>
        public static string[] UserAssemblyNamePrefixList;

        /// <summary>
        /// 系统程序集前缀
        /// </summary>
        public static readonly string[] SystemAssemblyPrefixList = new string[] {
			"mscorlib","netstandard","System.", "Mono.","Microsoft.",
			"Unity.","UnityEngine.","UnityEditor.", "AssetStoreTools",
			"I18N", "ICSharpCode.", "nunit.", "SyntaxTree.",
		};

        /// <summary>
		/// 跨程序集查找指定类型
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Type FindUserType(string fullName, bool ignoreCase = false)
		{
			return GetAllUserTypes().FirstOrDefault(t =>
					string.Compare(t.FullName, fullName, ignoreCase) == 0);
		}

		/// <summary>
		/// 检查是否用户程序集
        /// 通过<see cref="UserAssemblyNamePrefixList"/>指定程序集前缀名字（如果有）或简单过滤是否在<see cref="SystemAssemblyPrefixList"/>内
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsUserAssembly(Assembly assembly)
		{
            // 判断是否在指定程序集内
            if (UserAssemblyNamePrefixList != null && UserAssemblyNamePrefixList.Length > 0)
            {
                var assName = assembly.GetName().Name;
                return UserAssemblyNamePrefixList.FirstOrDefault(x => assName.StartsWith(x)) != null;
            }

			var name = assembly.FullName;

            // 简单过滤是否在SystemAssemblyPrefixList内
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
        /// <param name="assembly">程序集</param>
        /// <param name="nonPublic">是否包含非public类型，默认false（能过滤将近一半的类型，很多都是编译器生成的）</param>
        /// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly, bool nonPublic = false)
		{
			try
			{
				return nonPublic ? assembly.GetTypes() : assembly.GetExportedTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				// Gets the array of classes that were defined in the module and loaded.
				return e.Types.Where(t => t != null);
			}
		}

        /// <summary>
        /// 获取所有类型定义，只包含过滤的程序集
        /// </summary>
        /// <param name="predicateAssembly">程序集过滤器</param>
        /// <param name="nonPublicTypes">是否包含非public类型，默认false（能过滤将近一半的类型，很多都是编译器生成的）</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<Type> GetAllUserTypes(Predicate<Assembly> predicateAssembly, bool nonPublicTypes = false)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.IsDynamic && predicateAssembly(x))
                .SelectMany(x => GetLoadableTypes(x, nonPublicTypes));
        }

		/// <summary>
        /// 获取所有类型定义，只包含用户程序集
        /// </summary>
        /// <param name="nonPublicTypes">是否包含非public类型，默认false（能过滤将近一半的类型，很多都是编译器生成的）</param>
        /// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<Type> GetAllUserTypes(bool nonPublicTypes = false)
		{
            return GetAllUserTypes(IsUserAssembly, nonPublicTypes);
		}
    }
}
