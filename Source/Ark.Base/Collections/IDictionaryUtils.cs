using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ark
{
	public static class IDictionaryUtils
	{
#if false
		/// Use System.Collections.Generic.CollectionExtensions.GetValueOrDefault() after .NETStandard 2.1
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict, TKey key)
		{
			dict.TryGetValue(key, out var result);
			return result;
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
	}
}
