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
			var factory = (createFunc == null)
				? _createInstanceFunc
				: type => {
					var instance = createFunc(type);
#if UNITY_5_3_OR_NEWER
					if (autoSetDontDestroyOnLoad /*&& Application.isPlaying*/ && instance is MonoBehaviour behaviour)
						GameObject.DontDestroyOnLoad(behaviour.gameObject);
#endif
					return instance;
				};

			return _cache.GetOrAdd(type, factory);
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
				if (_instance == null)
					_instance = (T)_cache.GetOrAdd(typeof(T), createFunc ?? _createInstanceFunc);

				return _instance;
			}

			static void Clear()
			{
				_instance = null;
			}

			static TypeHolder() => _clearFuncs.Add(Clear);
		}


		private static readonly Func<Type, object> _createInstanceFunc = CreateInstance;
		private static readonly Action<Type, object> _destroyInstanceFunc = DestroyInstance;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static object CreateInstance(Type type)
		{
#if UNITY_5_3_OR_NEWER
			if (typeof(MonoBehaviour).IsAssignableFrom(type))
			{
				var behaviour = GameObject.FindObjectOfType(type);
				if (behaviour == null)
				{
					var gameObj = new GameObject(type.Name);
					behaviour = gameObj.AddComponent(type);

					if (autoSetDontDestroyOnLoad && Application.isPlaying)
						GameObject.DontDestroyOnLoad(gameObj);
				}

				return behaviour;
			}
			/*
			if (typeof(ScriptableObject).IsAssignableFrom(type))
			{
			}
			*/
#endif
			return Activator.CreateInstance(type);
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
	}
}
