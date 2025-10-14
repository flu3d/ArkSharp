#if UNITY_5_3_OR_NEWER

using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ArkSharp
{
	/// <summary>
	/// Unity Object接口扩展
	/// </summary>
	public static class UnityObjectHelper
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
		{
			if (obj == null)
				return null;

			var comp = obj.GetComponent<T>() ?? 
					obj.AddComponent<T>();

			return comp;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveComponent<T>(this GameObject obj) where T : Component
		{
			if (obj == null)
				return;

			var comp = obj.GetComponent<T>();
			if (comp != null)
				Object.Destroy(comp);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static GameObject FindChild(this GameObject obj, string childPath)
		{
			if (obj == null || string.IsNullOrEmpty(childPath))
				return null;

			var child = obj.transform.Find(childPath);
			if (child == null)
				return null;

			return child.gameObject;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T FindChild<T>(this GameObject obj, string childPath) where T : Component
		{
			if (obj == null || string.IsNullOrEmpty(childPath))
				return null;

			var child = obj.transform.Find(childPath);
			if (child == null)
				return null;

			return child.GetComponent<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Destroy(this GameObject obj)
		{
			if (obj == null)
				return;

			if (Application.isPlaying)
				GameObject.Destroy(obj);
			else
				GameObject.DestroyImmediate(obj);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void DestroyChildren(this GameObject obj)
		{
			obj?.transform.DestroyChildren();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void DestroyChildren(this Transform parent)
		{
			if (parent == null)
				return;

			for (int i = parent.transform.childCount - 1; i >= 0; i--)
			{
				var child = parent.transform.GetChild(i);
				child?.gameObject.Destroy();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ResetLocal(this Transform transform)
		{
			if (transform == null)
				return;

			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;
		}

		// Unity bugfix: a Collider's underlying PhysX Transfrom did not update correctly when reparenting.
		// https://issuetracker.unity3d.com/issues/colliders-are-not-updating-to-match-attached-gameobject-location
		public static void ForceUpdateCollider(this GameObject obj)
		{
			var colliders = obj.GetComponentsInChildren<Collider>();

			foreach (var collider in colliders)
			{
				collider.enabled = false;
				collider.enabled = true;
			}
		}

		/// <summary>
		/// Determines whether a Unity object is null or "fake null", without ever calling
		/// Unity's own equality operators. This method is useful for checking if a Unity
		/// object is null, destroyed or missing at times when it is not allowed to call
		/// Unity's own equality operators, for example when not running on the main thread.
		/// </summary>
		/// <returns>True if the object is null, missing or destroyed; otherwise false.</returns>
		public static bool IsNullPtr(this Object obj)
		{
			if ((object)obj == null)
				return true;

			if (unityObjectGetCachedPtr == null)
				throw new System.NotSupportedException("Could not find method UnityEngine.Object.GetCachedPtr(); cannot perform a special null check.");

			var intPtr = unityObjectGetCachedPtr(obj);
			return intPtr == System.IntPtr.Zero;
		}

		private delegate System.IntPtr GetCachePtrDelagate(Object obj);
		private static readonly GetCachePtrDelagate unityObjectGetCachedPtr;

		static UnityObjectHelper()
		{
			var methodInfo = typeof(Object).GetMethod("GetCachedPtr", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			if (methodInfo != null)
				unityObjectGetCachedPtr = (GetCachePtrDelagate)System.Delegate.CreateDelegate(typeof(GetCachePtrDelagate), methodInfo);
		}
	}
}
#endif
