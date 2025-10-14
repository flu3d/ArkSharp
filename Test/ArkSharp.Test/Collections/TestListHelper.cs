using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ArkSharp;

namespace ArkSharp.Test.Collections
{
    [TestFixture]
    public class TestListHelper
    {
        [Test]
        public void AddUnique_WithCustomComparer_AddsUniqueItems()
        {
            var list = new List<string> { "hello", "WORLD" };
            list.AddUnique("Hello", (a, b) => a.Equals(b, StringComparison.OrdinalIgnoreCase));
            list.AddUnique("world", (a, b) => a.Equals(b, StringComparison.OrdinalIgnoreCase));

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual("hello", list[0]);
            Assert.AreEqual("WORLD", list[1]);
        }

        [Test]
        public void AddUnique_WithEqualityComparer_AddsUniqueItems()
        {
            var list = new List<string> { "hello", "WORLD" };
            var comparer = StringComparer.OrdinalIgnoreCase;

            list.AddUnique("Hello", comparer);
            list.AddUnique("world", comparer);

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual("hello", list[0]);
            Assert.AreEqual("WORLD", list[1]);
        }

        [Test]
        public void AddRange_WithNullCollection_DoesNothing()
        {
            IList<int> list = new List<int> { 1, 2, 3 };
            list.AddRange((IEnumerable<int>)null);

            Assert.AreEqual(3, list.Count);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list);
        }

        [Test]
        public void AddRange_WithReadOnlyList_AddsAllItems()
        {
            var list = new List<int> { 1, 2 };
            var source = new List<int> { 3, 4, 5 }.AsReadOnly();

            list.AddRange(source);

            Assert.AreEqual(5, list.Count);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, list);
        }

        [Test]
        public void AddRange_WithEnumerable_AddsAllItems()
        {
            var list = new List<int> { 1, 2 };
            var source = Enumerable.Range(3, 3); // 生成 3,4,5

            list.AddRange(source);

            Assert.AreEqual(5, list.Count);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, list);
        }

        [Test]
        public void Enlarge_WithSmallerCount_DoesNothing()
        {
            var list = new List<int> { 1, 2, 3 };
            list.Enlarge(2);

            Assert.AreEqual(3, list.Count);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list);
        }

        [Test]
        public void Enlarge_WithLargerCount_FillsWithDefaultValue()
        {
            var list = new List<int> { 1, 2 };
            list.Enlarge(4, -1);

            Assert.AreEqual(4, list.Count);
            CollectionAssert.AreEqual(new[] { 1, 2, -1, -1 }, list);
        }

        [Test]
        public void Resize_WithSmallerCount_RemovesExcessItems()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            list.Resize(3);

            Assert.AreEqual(3, list.Count);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list);
        }

        [Test]
        public void Resize_WithLargerCount_FillsWithDefaultValue()
        {
            var list = new List<int> { 1, 2 };
            list.Resize(4, -1);

            Assert.AreEqual(4, list.Count);
            CollectionAssert.AreEqual(new[] { 1, 2, -1, -1 }, list);
        }

        [Test]
        public void Resize_WithNegativeCount_ClearsTheList()
        {
            var list = new List<int> { 1, 2, 3 };
            list.Resize(-1);

            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void Shuffle_WithRandomSeed_ShufflesItems()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var original = new List<int>(list);
            var random = new Random(42); // 使用固定种子以使测试可重复

            list.Shuffle(random);

            Assert.AreEqual(original.Count, list.Count);
            Assert.IsFalse(Enumerable.SequenceEqual(original, list)); // 确保列表被打乱
            CollectionAssert.AreEquivalent(original, list); // 确保元素相同，只是顺序不同
        }

        [Test]
        public void Shuffle_WithCustomRandomFunction_ShufflesItems()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var original = new List<int>(list);
            int counter = 0;

            // 使用一个简单的自定义随机函数
            list.Shuffle(n => counter++ % n);

            Assert.AreEqual(original.Count, list.Count);
            Assert.IsFalse(Enumerable.SequenceEqual(original, list));
            CollectionAssert.AreEquivalent(original, list);
        }
    }
}
