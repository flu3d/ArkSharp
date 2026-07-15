using NUnit.Framework;
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
		public async Task TestReadTextNotFound()
		{
			var filePath = Path.GetTempFileName();
			File.Delete(filePath);

			try
			{
				Assert.ThrowsAsync<FileNotFoundException>(async () => await FileHelper.ReadText(filePath));
			}
			finally
			{
				File.Delete(filePath);
			}
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

			try
			{
				Assert.ThrowsAsync<FileNotFoundException>(async () => await FileHelper.ReadBytes(filePath));
			}
			finally
			{
				File.Delete(filePath);
			}
		}
	}
}
