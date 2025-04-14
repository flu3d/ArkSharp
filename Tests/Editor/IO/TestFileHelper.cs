using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System;
using System.IO;
using System.Text;

namespace ArkSharp.Test
{
	[TestFixture]
	public class TestFileHelper
	{
		private const string _stringData01 = "Hello, Ark.\n你好，方舟。\nこんにちは、アーク。";

		[Test]
		public async void TestLoadText()
		{
			var filePath = Path.GetTempFileName();
			var fileContent = _stringData01;
			File.WriteAllText(filePath, fileContent);

			var req = FileHelper.Get(filePath);
			var result = await req;
			
			//Assert.AreEqual(UniTaskStatus.Succeeded, req.Status);
			Assert.AreEqual(fileContent, result);
		}

		[Test]
		public async void TestLoadTextNotFound()
		{
			var filePath = Path.GetTempFileName();
			File.Delete(filePath);

			Exception error = null;
			string result = null;
			var req = new UniTask<string>();

			try
			{
				req = FileHelper.Get(filePath);
				result = await req;
			}
			catch (Exception e)
			{
				error = e;
			}

			//Assert.AreEqual(UniTaskStatus.Faulted, req.Status);
			Assert.NotNull(error);
			Assert.IsNull(result);
		}

		[Test]
		public async void TestLoadBytes()
		{
			var filePath = Path.GetTempFileName();
			var fileContent = Encoding.UTF8.GetBytes(_stringData01);
			File.WriteAllBytes(filePath, fileContent);

			var req = FileHelper.GetBytes(filePath);
			var result = await req;

			//Assert.AreEqual(UniTaskStatus.Succeeded, req.Status);
			Assert.AreEqual(fileContent.Length, result.Length);

			for (int i = 0; i < fileContent.Length; i++)
				Assert.AreEqual(fileContent[i], result[i]);
		}

		[Test]
		public async void TestLoadBytesNotFound()
		{
			var filePath = Path.GetTempFileName();
			File.Delete(filePath);

			Exception error = null;
			byte[] result = null;
			var req = new UniTask<byte[]>();

			try
			{
				req = FileHelper.GetBytes(filePath);
				result = await req;
			}
			catch (Exception e)
			{
				error = e;
			}

			//Assert.AreEqual(UniTaskStatus.Faulted, req.Status);
			Assert.NotNull(error);
			Assert.IsNull(result);
		}
	}
}
