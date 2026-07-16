using System;
using NUnit.Framework;

namespace ArkSharp.Test.Objects
{
	public class TestSyncCache
	{
		[Test]
		public void Clear_WhenDisposerThrows_RemovesAllEntriesAndAttemptsEveryDisposer()
		{
			using var cache = new SyncCache<int, string>();
			cache.Set(1, "first");
			cache.Set(2, "second");

			int disposeCount = 0;
			var exception = Assert.Throws<AggregateException>(() => cache.Clear((key, _) =>
			{
				disposeCount++;
				if (key == 1)
					throw new InvalidOperationException();
			}));

			Assert.AreEqual(1, exception.InnerExceptions.Count);
			Assert.AreEqual(2, disposeCount);
			Assert.IsNull(cache.Get(1));
			Assert.IsNull(cache.Get(2));

			cache.Set(3, "new");
			Assert.AreEqual("new", cache.Get(3));
		}

		[Test]
		public void Clear_WhenDisposerWritesToCache_PreservesNewEntry()
		{
			using var cache = new SyncCache<int, string>();
			cache.Set(1, "first");

			cache.Clear((key, value) => cache.Set(key + 10, value + "-new"));

			Assert.IsNull(cache.Get(1));
			Assert.AreEqual("first-new", cache.Get(11));
		}
	}
}
