using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ArkSharp
{
	/// <summary>
	/// 自动挂载提供者
	/// </summary>
	public interface IAutoDelegateProvider
	{
		void OnCreate(object obj);
		void OnDestroy(object obj);

		void ProcessStatic(Type objType);
	}

	/// <summary>
	/// 自动挂载管理器，通过注册的提供者自动挂在目标对象的委托回调函数
	/// </summary>
	public class AutoDelegate
	{
		public static AutoDelegate Default => Singleton.Get<AutoDelegate>();

		/*
		public void Setup()
		{
			_providers.Clear();

			var allTypes = Reflect.GetAllTypes();
			foreach (var type in allTypes)
			{
				if (!type.IsClass || type.IsAbstract)
					continue;

				if (!typeof(IAutoDelegateProvider).IsAssignableFrom(type))
					continue;

				AddProvider(type);
			}
		}
		*/

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddProvider(IAutoDelegateProvider provider)
		{
			if (provider != null)
				_providers.AddUnique(provider);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveProvider(IAutoDelegateProvider provider)
		{
			if (provider != null)
				_providers.Remove(provider);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveAllProviders()
		{
			_providers.Clear();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void OnCreate(object obj)
		{
			if (obj == null)
				return;

			for (int i = 0; i < _providers.Count; i++)
				_providers[i]?.OnCreate(obj);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void OnDestroy(object obj)
		{
			if (obj == null)
				return;

			for (int i = 0; i < _providers.Count; i++)
				_providers[i]?.OnDestroy(obj);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ProcessStatic(Type objType)
		{
			for (int i = 0; i < _providers.Count; i++)
				_providers[i]?.ProcessStatic(objType);
		}

		private readonly List<IAutoDelegateProvider> _providers = new List<IAutoDelegateProvider>();
	}
}
