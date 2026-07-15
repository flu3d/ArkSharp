using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace ArkSharp.Test.IO
{
	[RequiresPlayMode]
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
		public async Task TestFetchLocalNotFound()
		{
			var filePath = Path.GetTempFileName();
			File.Delete(filePath);

			try
			{
				var url = PathHelper.Path2URL(filePath);
				var result = await WebHelper.Fetch(url, false);

				Assert.False(result.IsSuccess);
				Assert.NotEmpty(result.Error);
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
				Assert.NotEmpty(result.Error);
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
	}
}
