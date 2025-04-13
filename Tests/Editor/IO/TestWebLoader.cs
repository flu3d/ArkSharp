using NUnit.Framework;
using System.IO;
using System.Text;

namespace ArkSharp.Test
{
	[TestFixture]
	public class TestWebLoader
	{
		private CoroutineScheduler _scheduler = new CoroutineScheduler();

		private const string _stringData01 = "Hello, Ark.\n你好，方舟。\nこんにちは、アーク。";

		[Test]
		public void TestGetLocal()
		{
			var filePath = Path.GetTempFileName();
			var fileContentText = _stringData01;
			var fileContentBytes = Encoding.UTF8.GetBytes(_stringData01);
			File.WriteAllBytes(filePath, fileContentBytes);

			var url = PathHelper.Path2URL(filePath);
			var loader = new WebLoader(_scheduler);
			var req = loader.Get(url);
			while (!req.IsCompleted)
				_scheduler.Poll();

			Assert.IsTrue(req.IsCompleted);
			Assert.IsNull(req.Error);
			Assert.NotNull(req.Result);
			Assert.AreEqual(fileContentBytes.Length, req.Result.Length);

			for (int i = 0; i < fileContentBytes.Length; i++)
				Assert.AreEqual(fileContentBytes[i], req.Result[i]);

			Assert.AreEqual(fileContentText, req.GetResultAsText());
		}

		[Test]
		public void TestGetLocalNotFound()
		{
			var filePath = Path.GetTempFileName();
			File.Delete(filePath);

			var url = PathHelper.Path2URL(filePath);
			var loader = new WebLoader(_scheduler);
			var req = loader.Get(url);
			while (!req.IsCompleted)
				_scheduler.Poll();

			Assert.IsTrue(req.IsCompleted);
			Assert.NotNull(req.Error);
			Assert.IsNull(req.Result);
		}

		[Test]
		public void TestGetLocalNotFoundWithRetry()
		{
			var filePath = Path.GetTempFileName();
			File.Delete(filePath);

			var time = RealTime.unixTimeMS;
			var url = PathHelper.Path2URL(filePath);
			var loader = new WebLoader(_scheduler);
			var req = loader.Get(url, 3, 200);
			while (!req.IsCompleted)
				_scheduler.Poll();

			Assert.GreaterOrEqual(RealTime.unixTimeMS - time, 3*200);
			Assert.IsTrue(req.IsCompleted);
			Assert.NotNull(req.Error);
			Assert.IsNull(req.Result);
		}
	}
}
