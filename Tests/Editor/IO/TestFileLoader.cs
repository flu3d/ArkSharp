using NUnit.Framework;
using System.IO;
using System.Text;

namespace ArkSharp.Test
{
	[TestFixture]
	public class TestFileLoader
	{
		private CoroutineScheduler _scheduler = new CoroutineScheduler();

		private const string _stringData01 = "Hello, Ark.\n你好，方舟。\nこんにちは、アーク。";

		[Test]
		public void TestLoadText()
		{
			var filePath = Path.GetTempFileName();
			var fileContent = _stringData01;
			File.WriteAllText(filePath, fileContent);

			var loader = new FileLoader(_scheduler);
			var req = loader.LoadText(filePath);
			while (!req.IsCompleted)
				_scheduler.Poll();
			
			Assert.IsTrue(req.IsCompleted);
			Assert.IsNull(req.Error);
			Assert.AreEqual(fileContent, req.Result);
		}

		[Test]
		public void TestLoadTextNotFound()
		{
			var filePath = Path.GetTempFileName();
			File.Delete(filePath);

			var loader = new FileLoader(_scheduler);
			var req = loader.LoadText(filePath);
			while (!req.IsCompleted)
				_scheduler.Poll();

			Assert.IsTrue(req.IsCompleted);
			Assert.NotNull(req.Error);
			Assert.IsNull(req.Result);
		}

		[Test]
		public void TestLoadBytes()
		{
			var filePath = Path.GetTempFileName();
			var fileContent = Encoding.UTF8.GetBytes(_stringData01);
			File.WriteAllBytes(filePath, fileContent);

			var loader = new FileLoader(_scheduler);
			var req = loader.LoadBytes(filePath);
			while (!req.IsCompleted)
				_scheduler.Poll();

			Assert.IsTrue(req.IsCompleted);
			Assert.IsNull(req.Error);
			Assert.NotNull(req.Result);
			Assert.AreEqual(fileContent.Length, req.Result.Length);

			for (int i = 0; i < fileContent.Length; i++)
				Assert.AreEqual(fileContent[i], req.Result[i]);
		}

		[Test]
		public void TestLoadBytesNotFound()
		{
			var filePath = Path.GetTempFileName();
			File.Delete(filePath);

			var loader = new FileLoader(_scheduler);
			var req = loader.LoadBytes(filePath);
			while (!req.IsCompleted)
				_scheduler.Poll();

			Assert.IsTrue(req.IsCompleted);
			Assert.NotNull(req.Error);
			Assert.IsNull(req.Result);
		}
	}
}
