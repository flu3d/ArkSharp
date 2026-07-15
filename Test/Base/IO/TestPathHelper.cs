using System;
using System.IO;
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
		public void TestGetFullPath()
		{
			const string RELATIVE_PATH = "Temp\\ArkSharp/file.txt";

			Assert.AreEqual(PathHelper.Normalize(Path.GetFullPath(RELATIVE_PATH)), PathHelper.GetFullPath(RELATIVE_PATH));
			Assert.IsNull(PathHelper.GetFullPath(null));
			Assert.AreEqual(string.Empty, PathHelper.GetFullPath(string.Empty));
		}

		[Test]
		public void TestPath2URL()
		{
			Assert.AreEqual("https://example.com/file.txt", PathHelper.Path2URL("https://example.com/file.txt"));
			Assert.AreEqual("file:///C:/ArkSharp/file.txt", PathHelper.Path2URL("C:\\ArkSharp\\file.txt"));
			Assert.AreEqual("file:///tmp/ArkSharp/file.txt", PathHelper.Path2URL("/tmp/ArkSharp/file.txt"));
			Assert.AreEqual("file:///C:/ArkSharp/a%23b%3F.txt", PathHelper.Path2URL("C:\\ArkSharp\\a#b?.txt"));
			Assert.AreEqual("file:///tmp/ArkSharp/a%23b%3F.txt", PathHelper.Path2URL("/tmp/ArkSharp/a#b?.txt"));
			Assert.AreEqual("file://server/share/a%20b.txt", PathHelper.Path2URL("\\\\server\\share\\a b.txt"));
			Assert.IsNull(PathHelper.Path2URL(null));
			Assert.AreEqual(string.Empty, PathHelper.Path2URL(string.Empty));
		}

		[Test]
		public void TestPath2URLRelative()
		{
			const string RELATIVE_PATH = "Temp/ArkSharp/a#b?.txt";
			var uri = new Uri(PathHelper.Path2URL(RELATIVE_PATH));

			Assert.AreEqual(Path.GetFullPath(RELATIVE_PATH), uri.LocalPath);
		}

		[Test]
		public void TestPath2URLApk()
		{
			Assert.AreEqual("jar:file:///data/app/base.apk!/assets/file.txt", PathHelper.Path2URL("/data/app/base.apk!assets/file.txt"));
			Assert.AreEqual("jar:file:///data/app/base.apk!/assets/file.txt", PathHelper.Path2URL("/data/app/base.apk!/assets/file.txt"));
			Assert.AreEqual("jar:file:///data/app/base.apk!/", PathHelper.Path2URL("/data/app/base.apk!"));
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
			Assert.False(PathHelper.IsURL(string.Empty));
		}
	}
}
