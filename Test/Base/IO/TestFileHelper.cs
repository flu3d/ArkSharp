using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

#if UNITY_5_3_OR_NEWER
using UnityEngine.TestTools;
#endif

namespace ArkSharp.Test.IO
{
#if UNITY_5_3_OR_NEWER
	[RequiresPlayMode]
#endif
	[TestFixture]
	public class TestFileHelper
	{
		private const string _stringData01 = "Hello, Ark.\n你好，方舟。\nこんにちは、アーク。";

		[Test]
		public async Task TestReadText()
		{
			var filePath = Path.GetTempFileName();
			var fileContent = _stringData01;

			try
			{
				File.WriteAllText(filePath, fileContent);
				var result = await FileHelper.ReadText(filePath);
				Assert.AreEqual(fileContent, result);
			}
			finally
			{
				File.Delete(filePath);
			}
		}

		[Test]
		public async Task TestReadTextWithUtf8Bom()
		{
			var filePath = Path.GetTempFileName();
			var fileContent = _stringData01;
			var preamble = Encoding.UTF8.GetPreamble();
			var content = Encoding.UTF8.GetBytes(fileContent);
			var bytes = new byte[preamble.Length + content.Length];

			try
			{
				Buffer.BlockCopy(preamble, 0, bytes, 0, preamble.Length);
				Buffer.BlockCopy(content, 0, bytes, preamble.Length, content.Length);
				File.WriteAllBytes(filePath, bytes);

				var result = await FileHelper.ReadText(filePath);
				Assert.AreEqual(fileContent, result);
			}
			finally
			{
				File.Delete(filePath);
			}
		}

		[Test]
		public async Task TestReadTextNotFound()
		{
			var filePath = Path.GetTempFileName();
			File.Delete(filePath);
			Exception exception = null;

			try
			{
				await FileHelper.ReadText(filePath);
			}
			catch (Exception e)
			{
				exception = e;
			}
			finally
			{
				File.Delete(filePath);
			}

			Assert.IsInstanceOf<FileNotFoundException>(exception);
		}

		[Test]
		public async Task TestReadBytes()
		{
			var filePath = Path.GetTempFileName();
			var fileContent = Encoding.UTF8.GetBytes(_stringData01);

			try
			{
				File.WriteAllBytes(filePath, fileContent);
				var result = await FileHelper.ReadBytes(filePath);
				CollectionAssert.AreEqual(fileContent, result);
			}
			finally
			{
				File.Delete(filePath);
			}
		}

		[Test]
		public async Task TestReadBytesNotFound()
		{
			var filePath = Path.GetTempFileName();
			File.Delete(filePath);
			Exception exception = null;

			try
			{
				await FileHelper.ReadBytes(filePath);
			}
			catch (Exception e)
			{
				exception = e;
			}
			finally
			{
				File.Delete(filePath);
			}

			Assert.IsInstanceOf<FileNotFoundException>(exception);
		}
	}
}
