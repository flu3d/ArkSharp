using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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
		public async Task TestGetLocal()
		{
			var filePath = Path.GetTempFileName();
			var fileContent = Encoding.UTF8.GetBytes(_stringData01);
			File.WriteAllBytes(filePath, fileContent);

			var url = PathHelper.Path2URL(filePath);
			var req = WebHelper.GetBytes(url);
			var result = await req;

			//Assert.AreEqual(UniTaskStatus.Succeeded, req.Status);
			Assert.AreEqual(fileContent.Length, result.Length);

			for (int i = 0; i < fileContent.Length; i++)
				Assert.AreEqual(fileContent[i], result[i]);
		}

		[Test]
		public async Task TestGetLocalNotFound()
		{
			var filePath = Path.GetTempFileName();
			File.Delete(filePath);

			Exception error = null;
			byte[] result = null;
			var req = new UniTask<byte[]>();

			try
			{
				var url = PathHelper.Path2URL(filePath);
				req = WebHelper.GetBytes(url);
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
		public async Task TestGetLocalNotFoundWithRetry()
		{
			var filePath = Path.GetTempFileName();
			File.Delete(filePath);

			Exception error = null;
			string result = null;
			var req = new UniTask<string>();

			var time = RealTime.unixTimeMS;

			try
			{
				var url = PathHelper.Path2URL(filePath);
				req = WebHelper.TryGet(url, 3, 200);
				result = await req;
			}
			catch (Exception e)
			{
				error = e;
			}

			//Assert.AreEqual(UniTaskStatus.Faulted, req.Status);
			Assert.GreaterOrEqual(RealTime.unixTimeMS - time, 2 * 200); // 尝试3次都失败，中间经历2次延迟
			Assert.NotNull(error);
			Assert.IsNull(result);
		}
	}
}
