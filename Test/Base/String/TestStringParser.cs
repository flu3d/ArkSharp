using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ArkSharp.Test.String
{
    [TestFixture]
    public class TestStringParser
    {
        [Test]
        public void Test01()
        {
            var dict = new Dictionary<string, List<int>>();

            var s = @"{tom:10,jerry:1,jerry:-1}";

            s.AsSpan().To(ref dict);

            Assert.AreEqual(2, dict.Count);

            var v1 = dict.GetValueOrDefault("tom");
            Assert.NotNull(v1);
            Assert.AreEqual(1, v1.Count);
            Assert.AreEqual(10, v1[0]);

            var v2 = dict.GetValueOrDefault("jerry");
            Assert.NotNull(v2);
            Assert.AreEqual(2, v2.Count);
            Assert.AreEqual(1, v2[0]);
            Assert.AreEqual(-1, v2[1]);
        }

		[Test]
		public void ToDictionary_UsesCustomKeyValueSeparatorAndPreservesRemainingValue()
		{
			var dict = new Dictionary<string, string>();

			"name=value:with:colon".AsSpan().To(ref dict, separatorKV: new[] { '=' });

			Assert.AreEqual("value:with:colon", dict["name"]);
		}

		[Test]
		public void ToArray_ReusesExistingArrayWhenLengthMatches()
		{
			var result = new int[2];
			var original = result;

			"1,2".To(ref result);

			Assert.AreSame(original, result);
			CollectionAssert.AreEqual(new[] { 1, 2 }, result);
		}

		[Test]
		public void ToArray_ReplacesExistingArrayWhenLengthDoesNotMatch()
		{
			var result = new int[1];
			var original = result;

			"1,2".To(ref result);

			Assert.AreNotSame(original, result);
			CollectionAssert.AreEqual(new[] { 1, 2 }, result);
		}

		[Test]
		public void ToArray_ReusesExistingEmptyArrayWhenInputIsEmpty()
		{
			var result = Array.Empty<int>();
			var original = result;

			"".To(ref result);

			Assert.AreSame(original, result);
			Assert.AreEqual(0, result.Length);
		}

		[Test]
		public void ToNonGenericContainers_EmptyTextReturnsEmptyContainers()
		{
			var text = ReadOnlySpan<char>.Empty;

			var list = text.ToList(typeof(List<int>));
			var array = text.ToArray(typeof(int[]));
			var dict = text.ToDictionary(typeof(Dictionary<string, int>));
			var bracketedList = "[]".AsSpan().ToList(typeof(List<int>));
			var bracketedArray = "[]".AsSpan().ToArray(typeof(int[]));
			var bracketedDict = "{}".AsSpan().ToDictionary(typeof(Dictionary<string, int>));

			Assert.AreEqual(0, list.Count);
			Assert.AreEqual(0, array.Length);
			Assert.AreEqual(0, dict.Count);
			Assert.AreEqual(0, bracketedList.Count);
			Assert.AreEqual(0, bracketedArray.Length);
			Assert.AreEqual(0, bracketedDict.Count);
		}
    }
}
