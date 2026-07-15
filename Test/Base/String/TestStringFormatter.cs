using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace ArkSharp.Test.String
{
	[TestFixture]
	public class TestStringFormatter
	{
		[Test]
		public void ToString_UsesFullSeparatorLength()
		{
			Assert.AreEqual("a||b", StringFormatter.ToString(new List<string> { "a", "b" }, "||"));
			Assert.AreEqual("ab", StringFormatter.ToString(new List<string> { "a", "b" }, string.Empty));
			Assert.AreEqual("ab", StringFormatter.ToString(new List<string> { "a", "b" }, null));

			var genericDict = new Dictionary<string, int> { { "a", 1 } };
			var dict = new Hashtable { { "a", 1 } };
			Assert.AreEqual("a=1", StringFormatter.ToString(genericDict, "||", "="));
			Assert.AreEqual("a,b", StringFormatter.ToString(new ArrayList { "a", "b" }));
			Assert.AreEqual("a:1", StringFormatter.ToString(dict));
		}
	}
}
