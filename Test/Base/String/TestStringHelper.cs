using NUnit.Framework;

namespace ArkSharp.Test.String
{
	[TestFixture]
	public class TestStringHelper
	{
		[TestCase(null, false)]
		[TestCase("", false)]
		[TestCase("ArkSharp 2026", false)]
		[TestCase("ArkSharp工具箱", true)]
		public void ContainsChinese_ReturnsExpectedResult(string text, bool expected)
		{
			Assert.AreEqual(expected, text.ContainsChinese());
		}

		[Test]
		public void ContainsChinese_OnlyMatchesSupportedRanges()
		{
			Assert.IsTrue("\u3400".ContainsChinese());
			Assert.IsTrue("\u9FFF".ContainsChinese());
			Assert.IsFalse("\uF900".ContainsChinese());
			Assert.IsFalse(char.ConvertFromUtf32(0x20000).ContainsChinese());
		}
	}
}
