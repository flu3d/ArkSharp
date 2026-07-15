using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ArkSharp.Test.String
{
	[TestFixture]
	public class TestSpanCharHelper
	{
		[Test]
		public void Split_CountReturnsRemainingTextInLastEntry()
		{
			var result = new List<string>();
			foreach (var value in "name:value:with:colon".AsSpan().Split(new[] { ':' }, 2))
				result.Add(value.ToString());

			CollectionAssert.AreEqual(new[] { "name", "value:with:colon" }, result);
		}

		[Test]
		public void Split_CountWithRemoveEmptyEntries_SkipsLeadingSeparatorsFromRemainingText()
		{
			var result = new List<string>();
			foreach (var value in "a,,b,c".AsSpan().Split(new[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries))
				result.Add(value.ToString());

			CollectionAssert.AreEqual(new[] { "a", "b,c" }, result);
		}
	}
}
