using System;
using System.Collections.Generic;
using NUnit.Framework;
using ArkSharp;

namespace ArkSharp.Test.Collections
{
    [TestFixture]
    public class TestDictionaryHelper
    {
        [Test]
        public void RemoveAll_WithEmptyDictionary_DoesNothing()
        {
            var dict = new Dictionary<string, int>();
            dict.RemoveAll(kv => true);

            Assert.AreEqual(0, dict.Count);
        }

        [Test]
        public void RemoveAll_WithPredicate_RemovesMatchingItems()
        {
            var dict = new Dictionary<string, int>
            {
                { "one", 1 },
                { "two", 2 },
                { "three", 3 },
                { "four", 4 }
            };

            dict.RemoveAll(kv => kv.Value % 2 == 0); // 移除所有偶数值

            Assert.AreEqual(2, dict.Count);
            Assert.IsTrue(dict.ContainsKey("one"));
            Assert.IsTrue(dict.ContainsKey("three"));
            Assert.IsFalse(dict.ContainsKey("two"));
            Assert.IsFalse(dict.ContainsKey("four"));
        }

        [Test]
        public void RemoveAll_WithCustomTempList_ReusesList()
        {
            var dict = new Dictionary<string, int>
            {
                { "one", 1 },
                { "two", 2 }
            };
            var tempList = new List<string>();

            dict.RemoveAll(kv => kv.Value > 1, tempList);

            Assert.AreEqual(1, dict.Count);
            Assert.IsTrue(dict.ContainsKey("one"));
            Assert.IsFalse(dict.ContainsKey("two"));
            Assert.AreEqual(0, tempList.Count); // 确保临时列表被清空
        }

        [Test]
        public void RemoveAll_WithNonEmptyTempList_DoesNotRemoveKeysFromPreviousUse()
        {
            var dict = new Dictionary<string, int>
            {
                { "keep", 1 },
                { "remove", 2 }
            };
            var tempList = new List<string> { "keep" };

            dict.RemoveAll(kv => kv.Value == 2, tempList);

            Assert.IsTrue(dict.ContainsKey("keep"));
            Assert.IsFalse(dict.ContainsKey("remove"));
            Assert.AreEqual(0, tempList.Count);
        }

        [Test]
        public void RemoveAll_WhenPredicateThrows_ClearsTempList()
        {
            var dict = new Dictionary<string, int> { { "key", 1 } };
            var tempList = new List<string>();

            Assert.Throws<InvalidOperationException>(() => dict.RemoveAll(_ => throw new InvalidOperationException(), tempList));
            Assert.AreEqual(0, tempList.Count);
        }

        [Test]
        public void IsNullOrEmpty_WithNullDictionary_ReturnsTrue()
        {
            IReadOnlyDictionary<int, string> dict = null;
            Assert.IsTrue(dict.IsNullOrEmpty());
        }

        [Test]
        public void IsNullOrEmpty_WithEmptyDictionary_ReturnsTrue()
        {
            var dict = new Dictionary<int, string>();
            Assert.IsTrue(dict.IsNullOrEmpty());
        }

        [Test]
        public void IsNullOrEmpty_WithNonEmptyDictionary_ReturnsFalse()
        {
            var dict = new Dictionary<int, string> { { 1, "one" } };
            Assert.IsFalse(dict.IsNullOrEmpty());
        }

        [Test]
        public void GetOrAdd_WithValue_AddsAndReturnsValueWhenKeyIsMissing()
        {
            var dict = new Dictionary<string, string>();

            var result = dict.GetOrAdd("key", "value");

            Assert.AreEqual("value", result);
            Assert.AreEqual("value", dict["key"]);
        }

        [Test]
        public void GetOrAdd_WithValue_ReturnsExistingValueWhenKeyExists()
        {
            var existing = new object();
            var dict = new Dictionary<string, object> { { "key", existing } };

            var result = dict.GetOrAdd("key", new object());

            Assert.AreSame(existing, result);
            Assert.AreEqual(1, dict.Count);
        }

        [Test]
        public void GetOrAdd_WithFactory_CreatesValueOnlyWhenKeyIsMissing()
        {
            var dict = new Dictionary<string, List<int>>();
            var factoryCallCount = 0;

            var added = dict.GetOrAdd("key", () =>
            {
                factoryCallCount++;
                return new List<int>();
            });
            var existing = dict.GetOrAdd("key", () =>
            {
                factoryCallCount++;
                return new List<int>();
            });

            Assert.AreSame(added, existing);
            Assert.AreEqual(1, factoryCallCount);
        }

        [Test]
        public void GetValueOrDefault_TwoLevelDictionary_ReturnsCorrectValue()
        {
            var innerDict = new Dictionary<int, string>
            {
                { 1, "one" },
                { 2, "two" }
            };

            var dict = new Dictionary<string, IReadOnlyDictionary<int, string>>
            {
                { "first", innerDict }
            };

            var result1 = dict.GetValueOrDefault("first", 1);
            var result2 = dict.GetValueOrDefault("first", 3);
            var result3 = dict.GetValueOrDefault("second", 1);

            Assert.AreEqual("one", result1);
            Assert.AreEqual(default(string), result2);
            Assert.AreEqual(default(string), result3);
        }

        [Test]
        public void GetValueOrDefault_ThreeLevelDictionary_ReturnsCorrectValue()
        {
            var innerMostDict = new Dictionary<char, bool>
            {
                { 'a', true },
                { 'b', false }
            };

            var innerDict = new Dictionary<int, IReadOnlyDictionary<char, bool>>
            {
                { 1, innerMostDict }
            };

            var dict = new Dictionary<string, IReadOnlyDictionary<int, IReadOnlyDictionary<char, bool>>>
            {
                { "first", innerDict }
            };

            var result1 = dict.GetValueOrDefault("first", 1, 'a');
            var result2 = dict.GetValueOrDefault("first", 1, 'c');
            var result3 = dict.GetValueOrDefault("first", 2, 'a');
            var result4 = dict.GetValueOrDefault("second", 1, 'a');

            Assert.IsTrue(result1);
            Assert.AreEqual(default(bool), result2);
            Assert.AreEqual(default(bool), result3);
            Assert.AreEqual(default(bool), result4);
        }

        [Test]
        public void GetValueOrDefault_ThreeLevelDictionary_WithDefaultValue_ReturnsCorrectValue()
        {
            var innerMostDict = new Dictionary<char, string>
            {
                { 'a', "value" }
            };

            var innerDict = new Dictionary<int, IReadOnlyDictionary<char, string>>
            {
                { 1, innerMostDict }
            };

            var dict = new Dictionary<string, IReadOnlyDictionary<int, IReadOnlyDictionary<char, string>>>
            {
                { "first", innerDict }
            };

            var defaultValue = "default";
            var result1 = dict.GetValueOrDefault("first", 1, 'a', defaultValue);
            var result2 = dict.GetValueOrDefault("first", 1, 'b', defaultValue);
            var result3 = dict.GetValueOrDefault("first", 2, 'a', defaultValue);
            var result4 = dict.GetValueOrDefault("second", 1, 'a', defaultValue);

            Assert.AreEqual("value", result1);
            Assert.AreEqual(defaultValue, result2);
            Assert.AreEqual(defaultValue, result3);
            Assert.AreEqual(defaultValue, result4);
        }
    }
}
