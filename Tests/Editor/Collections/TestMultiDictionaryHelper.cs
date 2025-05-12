using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace ArkSharp.Test.Collections
{
	[TestFixture]
    public class TestMultiDictionaryHelper
    {
        [Test]
        public void Add_WithNewKey_CreatesListAndAddsValue()
        {
            var dict = new Dictionary<string, List<int>>();
            dict.Add("numbers", 1);

            Assert.IsTrue(dict.ContainsKey("numbers"));
            Assert.AreEqual(1, dict["numbers"].Count);
            Assert.AreEqual(1, dict["numbers"][0]);
        }

        [Test]
        public void Add_WithExistingKey_AppendsToList()
        {
            var dict = new Dictionary<string, List<int>>();
            dict.Add("numbers", 1);
            dict.Add("numbers", 2);
            dict.Add("numbers", 3);

            Assert.AreEqual(3, dict["numbers"].Count);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, dict["numbers"]);
        }

        [Test]
        public void AddUnique_WithCustomComparer_AddsUniqueItems()
        {
            var dict = new Dictionary<string, List<string>>();
            bool CompareStrings(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

            dict.AddUnique("words", "hello", CompareStrings);
            dict.AddUnique("words", "HELLO", CompareStrings);
            dict.AddUnique("words", "world", CompareStrings);

            Assert.AreEqual(2, dict["words"].Count);
            CollectionAssert.AreEqual(new[] { "hello", "world" }, dict["words"]);
        }

        [Test]
        public void AddUnique_WithEqualityComparer_AddsUniqueItems()
        {
            var dict = new Dictionary<string, List<string>>();
            var comparer = StringComparer.OrdinalIgnoreCase;

            dict.AddUnique("words", "hello", comparer);
            dict.AddUnique("words", "HELLO", comparer);
            dict.AddUnique("words", "world", comparer);

            Assert.AreEqual(2, dict["words"].Count);
            CollectionAssert.AreEqual(new[] { "hello", "world" }, dict["words"]);
        }

        [Test]
        public void AddUnique_WithDefaultComparer_AddsUniqueItems()
        {
            var dict = new Dictionary<string, List<int>>();

            dict.AddUnique("numbers", 1);
            dict.AddUnique("numbers", 1);
            dict.AddUnique("numbers", 2);

            Assert.AreEqual(2, dict["numbers"].Count);
            CollectionAssert.AreEqual(new[] { 1, 2 }, dict["numbers"]);
        }

        [Test]
        public void Remove_WithExistingKeyAndValue_RemovesValue()
        {
            var dict = new Dictionary<string, List<int>>();
            dict.Add("numbers", 1);
            dict.Add("numbers", 2);
            dict.Add("numbers", 3);

            var result = dict.Remove("numbers", 2);

            Assert.IsTrue(result);
            Assert.AreEqual(2, dict["numbers"].Count);
            CollectionAssert.AreEqual(new[] { 1, 3 }, dict["numbers"]);
        }

        [Test]
        public void Remove_WithNonExistingKey_ReturnsFalse()
        {
            var dict = new Dictionary<string, List<int>>();
            dict.Add("numbers", 1);

            var result = dict.Remove("nonexistent", 1);

            Assert.IsFalse(result);
            Assert.AreEqual(1, dict["numbers"].Count);
        }

        [Test]
        public void Remove_WithNonExistingValue_ReturnsFalse()
        {
            var dict = new Dictionary<string, List<int>>();
            dict.Add("numbers", 1);

            var result = dict.Remove("numbers", 2);

            Assert.IsFalse(result);
            Assert.AreEqual(1, dict["numbers"].Count);
            Assert.AreEqual(1, dict["numbers"][0]);
        }

        [Test]
        public void MultiOperation_ComplexScenario()
        {
            var dict = new Dictionary<string, List<string>>();
            var comparer = StringComparer.OrdinalIgnoreCase;

            // 添加一些值
            dict.Add("fruits", "apple");
            dict.Add("fruits", "banana");
            dict.AddUnique("fruits", "APPLE", comparer);  // 不应该添加，因为已经有 "apple"

            // 添加到另一个键
            dict.AddUnique("vegetables", "carrot", comparer);
            dict.AddUnique("vegetables", "CARROT", comparer);  // 不应该添加

            // 删除一些值
            dict.Remove("fruits", "banana");

            // 验证最终状态
            Assert.AreEqual(1, dict["fruits"].Count);
            Assert.AreEqual(1, dict["vegetables"].Count);
            Assert.AreEqual("apple", dict["fruits"][0]);
            Assert.AreEqual("carrot", dict["vegetables"][0]);
        }
    }
}
