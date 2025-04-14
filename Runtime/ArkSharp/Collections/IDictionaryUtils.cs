using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ArkSharp
{
	public static class IDictionaryUtils
	{
#if false //NET_STANDARD
		// Using System.Collections.Generic.CollectionExtensions.GetValueOrDefault() after .NETStandard 2.1

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict, TKey key)
		{
			if (dict.TryGetValue(key, out var result))
				return result;

			return default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
		{
			if (dict.TryGetValue(key, out var result))
				return result;

			return defaultValue;
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> dict, Func<KeyValuePair<TKey, TValue>, bool> predicate, IList<TKey> tempKeyList = null)
		{
			if (dict.Count <= 0)
				return;

			if (tempKeyList == null)
				tempKeyList = new List<TKey>(dict.Count);

			foreach (var kv in dict)
			{
				if (predicate(kv))
					tempKeyList.Add(kv.Key);
			}

			for (int i = 0; i < tempKeyList.Count; i++)
			{
				dict.Remove(tempKeyList[i]);
			}

			tempKeyList.Clear();
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict)
        {
            return dict == null || dict.Count == 0;
        }
    }
}
