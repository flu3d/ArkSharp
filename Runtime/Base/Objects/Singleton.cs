using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace ArkSharp
{
	/// <summary>
	/// 单例工具箱，提供统一的原生对象/Unity行为对象单件获取接口
	/// </summary>
	public static class Singleton
	{
		public static bool autoSetDontDestroyOnLoad { get; set; } = true;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Get<T>(Func<Type, object> createFunc = null) where T : class
		{
			return TypeHolder<T>.Get(createFunc);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static object Get(Type type, Func<Type, object> createFunc = null)
		{
			return GetOrCreate(type, createFunc);
		}

		/// <summary>
		/// 清理所有缓存的单例，便于进行单元测试
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Clear()
		{
			foreach (var clearFunc in _clearFuncs)
				clearFunc?.Invoke();

			_cache.Clear(_destroyInstanceFunc);
		}

		private static readonly SyncCache<Type, object> _cache = new SyncCache<Type, object>();
		private static readonly List<Action> _clearFuncs = new List<Action>();

		static class TypeHolder<T> where T:class
		{
			static volatile T _instance = null;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static T Get(Func<Type, object> createFunc)
			{
				if (IsNullOrDestroyed(_instance))
					_instance = (T)GetOrCreate(typeof(T), createFunc);

				return _instance;
			}

			static void Clear()
			{
				_instance = null;
			}

			static TypeHolder() => _clearFuncs.Add(Clear);
		}


		private static readonly Func<Type, object> _createInstanceFunc = type => CreateInstance(type, null);
		private static readonly Action<Type, object> _destroyInstanceFunc = DestroyInstance;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static object GetOrCreate(Type type, Func<Type, object> createFunc)
		{
			var factory = _createInstanceFunc;
			if (createFunc != null)
				factory = requestedType => CreateInstance(requestedType, createFunc);

			var instance = _cache.GetOrAdd(type, factory);

			if (IsNullOrDestroyed(instance))
			{
				_cache.Remove(type);
				instance = _cache.GetOrAdd(type, factory);
			}

			return instance;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsNullOrDestroyed(object instance)
		{
			if (instance == null)
				return true;

#if UNITY_5_3_OR_NEWER
			return instance is UnityEngine.Object unityObject && !unityObject;
#else
			return false;
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static object CreateInstance(Type type, Func<Type, object> createFunc)
		{
			object instance;
			if (createFunc != null)
			{
				// 使用传入的工厂函数创建实例
				instance = createFunc(type);
			}

#if UNITY_5_3_OR_NEWER
			else if (typeof(MonoBehaviour).IsAssignableFrom(type))
			{
				// Unity 环境下使用 FindObjectOfType 查找已存在的实例，不存在则创建新的实例
				var behaviour = GameObject.FindObjectOfType(type);
				if (behaviour == null && !_isAppQuitting)
					behaviour = new GameObject(type.Name).AddComponent(type);

				instance = behaviour;
			}
#endif
			else
			{
				// 非 Unity 环境下使用反射创建实例
				instance = Activator.CreateInstance(type, true);
			}

#if UNITY_5_3_OR_NEWER
			if (autoSetDontDestroyOnLoad && !_isAppQuitting && Application.isPlaying)
			{
				if (instance is MonoBehaviour behaviour && behaviour)
					GameObject.DontDestroyOnLoad(behaviour.gameObject);
			}
#endif

			return instance;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void DestroyInstance(Type type, object instance)
		{
			if (instance == null)
				return;

#if UNITY_5_3_OR_NEWER
			if (instance is MonoBehaviour behaviour)
			{
				if (!behaviour)
					return;

				var gameObj = behaviour.gameObject;
				GameObject.DestroyImmediate(behaviour);

				if (Application.isPlaying)
					GameObject.Destroy(gameObj);
				else
					GameObject.DestroyImmediate(gameObj);
			}
#endif
		}

#if UNITY_5_3_OR_NEWER
        static bool _isAppQuitting = false;

        static Singleton()
		{
			Application.quitting += () => _isAppQuitting = true;
		}
#endif
	}
}
