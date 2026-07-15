using NUnit.Framework;

namespace ArkSharp.Test.IO
{
	[TestFixture]
	public class TestPathHelper
	{
		[Test]
		public void TestNormalize()
		{
			Assert.AreEqual("Assets/ArkSharp/file.txt", PathHelper.Normalize("Assets\\ArkSharp/file.txt"));
			Assert.IsNull(PathHelper.Normalize(null));
			Assert.AreEqual(string.Empty, PathHelper.Normalize(string.Empty));
		}

		[Test]
		public void TestPath2URL()
		{
			Assert.AreEqual("https://example.com/file.txt", PathHelper.Path2URL("https://example.com/file.txt"));
			Assert.AreEqual("file:///C:/ArkSharp/file.txt", PathHelper.Path2URL("C:\\ArkSharp\\file.txt"));
			Assert.AreEqual("file:///tmp/ArkSharp/file.txt", PathHelper.Path2URL("/tmp/ArkSharp/file.txt"));
		}

		[Test]
		public void TestPath2URLApk()
		{
			Assert.AreEqual("jar:file:///data/app/base.apk!/assets/file.txt", PathHelper.Path2URL("/data/app/base.apk!assets/file.txt"));
			Assert.AreEqual("jar:file:///data/app/base.apk!/assets/file.txt", PathHelper.Path2URL("/data/app/base.apk!/assets/file.txt"));
		}

		[Test]
		public void TestIsURL()
		{
			Assert.True(PathHelper.IsURL("jar:file:///data/app/base.apk!/assets/file.txt"));
			Assert.True(PathHelper.IsURL("http://example.com"));
			Assert.True(PathHelper.IsURL("https://example.com"));
			Assert.True(PathHelper.IsURL("file:///tmp/file.txt"));
			Assert.False(PathHelper.IsURL("Assets/file.txt"));
			Assert.False(PathHelper.IsURL(null));
		}
	}
}
