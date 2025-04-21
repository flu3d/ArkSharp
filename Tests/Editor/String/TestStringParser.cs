using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ArkSharp.Test
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
    }
}
