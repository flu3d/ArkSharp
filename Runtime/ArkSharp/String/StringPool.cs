using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace ArkSharp
{
	/// <summary>
	/// 通用字符串池
	/// </summary>
	public class StringPool
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string Intern(string str)
		{
			if (str == null)
				return null;

			return _pool.GetOrAdd(str, str);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string IsInterned(string str)
		{
			if (str == null)
				return null;

			if (_pool.TryGetValue(str, out string result))
				return result;

			return null;
		}

		public void Clear()
		{
			_pool.Clear();
		}

		// Dictionary vs HashSet (less than .Net 4.7.2/Core 2.0)
		// http://stackoverflow.com/questions/7760364/how-to-retrieve-actual-item-from-hashsett
		private readonly ConcurrentDictionary<string, string> _pool = new ConcurrentDictionary<string, string>();

		public static StringPool Default { get; } = new StringPool();

		static StringPool() { }
	}
}
