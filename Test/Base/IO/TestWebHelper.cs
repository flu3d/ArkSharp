using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

#if UNITY_5_3_OR_NEWER
using UnityEngine.TestTools;
#endif

namespace ArkSharp.Test.IO
{
#if UNITY_5_3_OR_NEWER
	[RequiresPlayMode]
#endif
	[TestFixture]
	public class TestWebHelper
	{
		private const string _stringData01 = "Hello, Ark.\n你好，方舟。\nこんにちは、アーク。";

		[Test]
		public async Task TestFetchLocal()
		{
			var filePath = Path.GetTempFileName();
			var fileContent = Encoding.UTF8.GetBytes(_stringData01);

			try
			{
				File.WriteAllBytes(filePath, fileContent);

				var url = PathHelper.Path2URL(filePath);
				var result = await WebHelper.Fetch(url, false);

				Assert.True(result.IsSuccess);
				Assert.IsNull(result.Error);
				Assert.AreEqual(_stringData01, result.Text);
				CollectionAssert.AreEqual(fileContent, result.Bytes);
			}
			finally
			{
				File.Delete(filePath);
			}
		}

		[Test]
		public async Task TestGetLocalWithUtf8Bom()
		{
			var filePath = Path.GetTempFileName();
			var preamble = Encoding.UTF8.GetPreamble();
			var content = Encoding.UTF8.GetBytes(_stringData01);
			var bytes = new byte[preamble.Length + content.Length];

			try
			{
				Buffer.BlockCopy(preamble, 0, bytes, 0, preamble.Length);
				Buffer.BlockCopy(content, 0, bytes, preamble.Length, content.Length);
				File.WriteAllBytes(filePath, bytes);

				Assert.AreEqual(_stringData01, await WebHelper.Get(PathHelper.Path2URL(filePath)));
			}
			finally
			{
				File.Delete(filePath);
			}
		}

		[Test]
		public void TestWebResultText()
		{
			var text = _stringData01 + "\0\r\n";
			var bom = Encoding.UTF8.GetPreamble();

			Assert.IsNull(new WebResult(null, null).Text);
			Assert.AreEqual(string.Empty, new WebResult(Array.Empty<byte>(), null).Text);
			Assert.AreEqual(string.Empty, new WebResult(bom, null).Text);
			Assert.AreEqual(text, new WebResult(Encoding.UTF8.GetBytes(text), null).Text);
		}

		[Test]
		public async Task TestFetchLocalNotFound()
		{
			var filePath = Path.GetTempFileName();
			File.Delete(filePath);

			try
			{
				var url = PathHelper.Path2URL(filePath);
				var result = await WebHelper.Fetch(url, false);

				Assert.False(result.IsSuccess);
				Assert.IsNotEmpty(result.Error);
				Assert.IsNull(result.Bytes);
				Assert.IsNull(result.Text);
			}
			finally
			{
				File.Delete(filePath);
			}
		}

		[Test]
		public async Task TestFetchWithRetryLocalNotFound()
		{
			var filePath = Path.GetTempFileName();
			File.Delete(filePath);

			try
			{
				const int MAX_ATTEMPT_COUNT = 3;
				const int RETRY_INTERVAL_MS = 50;
				var stopwatch = Stopwatch.StartNew();
				var url = PathHelper.Path2URL(filePath);
				var result = await WebHelper.FetchWithRetry(url, false, null, MAX_ATTEMPT_COUNT, RETRY_INTERVAL_MS);

				Assert.GreaterOrEqual(stopwatch.ElapsedMilliseconds, (MAX_ATTEMPT_COUNT - 1) * RETRY_INTERVAL_MS);
				Assert.False(result.IsSuccess);
				Assert.IsNotEmpty(result.Error);
				Assert.IsNull(result.Text);
			}
			finally
			{
				File.Delete(filePath);
			}
		}

		[Test]
		public async Task TestGetWithRetryLocalNotFound()
		{
			var filePath = Path.GetTempFileName();
			File.Delete(filePath);

			try
			{
				var url = PathHelper.Path2URL(filePath);
				var result = await WebHelper.GetWithRetry(url, 1, 0);

				Assert.IsNull(result);
			}
			finally
			{
				File.Delete(filePath);
			}
		}

		[Test]
		public void TestBuildQueryString()
		{
			Assert.AreEqual(string.Empty, WebHelper.BuildQueryString(null));
			Assert.AreEqual(string.Empty, WebHelper.BuildQueryString(new Dictionary<string, string>()));
			Assert.AreEqual("value=c%3Dd%26f", WebHelper.BuildQueryString(new Dictionary<string, string> {
				["value"] = "c=d&f",
			}));
			Assert.AreEqual("empty=", WebHelper.BuildQueryString(new Dictionary<string, string> {
				["empty"] = null,
			}));
			Assert.Throws<ArgumentException>(() => WebHelper.BuildQueryString(new NullKeyDictionary()));
			Assert.Throws<ArgumentException>(() => WebHelper.BuildQueryString(new Dictionary<string, string> {
				[string.Empty] = "value",
			}));
		}

		private sealed class NullKeyDictionary : IReadOnlyDictionary<string, string>
		{
			public string this[string key] => null;
			public IEnumerable<string> Keys => Array.Empty<string>();
			public IEnumerable<string> Values => Array.Empty<string>();
			public int Count => 1;

			public bool ContainsKey(string key) => false;
			public bool TryGetValue(string key, out string value)
			{
				value = null;
				return false;
			}

			public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
			{
				yield return new KeyValuePair<string, string>(null, "value");
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}
