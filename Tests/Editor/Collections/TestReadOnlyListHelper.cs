using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace ArkSharp.Test.Collections
{
	[TestFixture]
    public class TestReadOnlyListHelper
    {
        private IReadOnlyList<int> _emptyList;
        private IReadOnlyList<string> _stringList;
        private IReadOnlyList<int> _numberList;

        [SetUp]
        public void Setup()
        {
            _emptyList = Array.Empty<int>();
            _stringList = new List<string> { "hello", "world", "test" }.AsReadOnly();
            _numberList = new List<int> { 1, 2, 3, 2, 1 }.AsReadOnly();
        }

        [Test]
        public void Contains_WithExistingValue_ReturnsTrue()
        {
            Assert.IsTrue(_numberList.Contains(2));
            Assert.IsTrue(_stringList.Contains("world"));
        }

        [Test]
        public void Contains_WithNonExistingValue_ReturnsFalse()
        {
            Assert.IsFalse(_numberList.Contains(42));
            Assert.IsFalse(_stringList.Contains("missing"));
        }

        [Test]
        public void IndexOf_WithExistingValue_ReturnsFirstIndex()
        {
            Assert.AreEqual(1, _numberList.IndexOf(2));
            Assert.AreEqual(1, _stringList.IndexOf("world"));
        }

        [Test]
        public void IndexOf_WithNonExistingValue_ReturnsMinusOne()
        {
            Assert.AreEqual(-1, _numberList.IndexOf(42));
            Assert.AreEqual(-1, _stringList.IndexOf("missing"));
        }

        [Test]
        public void Exists_WithMatchingPredicate_ReturnsTrue()
        {
            Assert.IsTrue(_numberList.Exists(x => x > 2));
            Assert.IsTrue(_stringList.Exists(s => s.Length == 4));
        }

        [Test]
        public void Exists_WithNonMatchingPredicate_ReturnsFalse()
        {
            Assert.IsFalse(_numberList.Exists(x => x > 10));
            Assert.IsFalse(_stringList.Exists(s => s.Length > 10));
        }

        [Test]
        public void Find_WithMatchingPredicate_ReturnsFirstMatch()
        {
            Assert.AreEqual(2, _numberList.Find(x => x > 1));
            Assert.AreEqual("test", _stringList.Find(s => s.Length == 4));
        }

        [Test]
        public void Find_WithNonMatchingPredicate_ReturnsDefault()
        {
            Assert.AreEqual(0, _numberList.Find(x => x > 10));
            Assert.IsNull(_stringList.Find(s => s.Length > 10));
        }

        [Test]
        public void FindIndex_WithMatchingPredicate_ReturnsFirstMatchIndex()
        {
            Assert.AreEqual(1, _numberList.FindIndex(x => x > 1));
            Assert.AreEqual(2, _stringList.FindIndex(s => s.Length == 4));
        }

        [Test]
        public void FindIndex_WithNonMatchingPredicate_ReturnsMinusOne()
        {
            Assert.AreEqual(-1, _numberList.FindIndex(x => x > 10));
            Assert.AreEqual(-1, _stringList.FindIndex(s => s.Length > 10));
        }

        [Test]
        public void ConvertAll_WithValidConverter_TransformsAllItems()
        {
            var result = _numberList.ConvertAll(x => x.ToString());
            CollectionAssert.AreEqual(new[] { "1", "2", "3", "2", "1" }, result);
        }

        [Test]
        public void ConvertAll_WithExistingList_AppendsToList()
        {
            var output = new List<string> { "existing" };
            var result = _numberList.ConvertAll(x => x.ToString(), output);

            Assert.AreSame(output, result);
            CollectionAssert.AreEqual(new[] { "existing", "1", "2", "3", "2", "1" }, result);
        }

        [Test]
        public void SequenceEqual_WithCustomComparer_ComparesCorrectly()
        {
            var other = new List<string> { "HELLO", "WORLD", "TEST" }.AsReadOnly();
            Assert.IsTrue(_stringList.SequenceEqual(other, (a, b) => a.Equals(b, StringComparison.OrdinalIgnoreCase)));
        }

        [Test]
        public void SequenceEqual_WithEqualityComparer_ComparesCorrectly()
        {
            var other = new List<string> { "HELLO", "WORLD", "TEST" }.AsReadOnly();
            Assert.IsTrue(_stringList.SequenceEqual(other, StringComparer.OrdinalIgnoreCase));
        }

        [Test]
        public void EmptyToNull_WithEmptyList_ReturnsNull()
        {
            Assert.IsNull(_emptyList.EmptyToNull());
        }

        [Test]
        public void EmptyToNull_WithNonEmptyList_ReturnsSameList()
        {
            var result = _numberList.EmptyToNull();
            Assert.AreSame(_numberList, result);
        }

        [Test]
        public void GetValueOrDefault_WithValidIndex_ReturnsValue()
        {
            Assert.AreEqual(2, _numberList.GetValueOrDefault(1));
            Assert.AreEqual("world", _stringList.GetValueOrDefault(1));
        }

        [Test]
        public void GetValueOrDefault_WithInvalidIndex_ReturnsDefault()
        {
            Assert.AreEqual(0, _numberList.GetValueOrDefault(10));
            Assert.IsNull(_stringList.GetValueOrDefault(10));
        }

        [Test]
        public void GetValueOrDefault_WithCustomDefault_ReturnsCustomValue()
        {
            Assert.AreEqual(-1, _numberList.GetValueOrDefault(10, -1));
            Assert.AreEqual("default", _stringList.GetValueOrDefault(10, "default"));
        }

        [Test]
        public void GetValueOrFirst_WithValidIndex_ReturnsValue()
        {
            Assert.AreEqual(2, _numberList.GetValueOrFirst(1));
        }

        [Test]
        public void GetValueOrFirst_WithInvalidIndex_ReturnsFirstValue()
        {
            Assert.AreEqual(1, _numberList.GetValueOrFirst(10));
        }

        [Test]
        public void GetValueOrFirst_WithEmptyList_ReturnsDefault()
        {
            Assert.AreEqual(0, _emptyList.GetValueOrFirst(0));
        }

        [Test]
        public void GetValueOrLast_WithValidIndex_ReturnsValue()
        {
            Assert.AreEqual(2, _numberList.GetValueOrLast(1));
        }

        [Test]
        public void GetValueOrLast_WithInvalidIndex_ReturnsLastValue()
        {
            Assert.AreEqual(1, _numberList.GetValueOrLast(10));
        }

        [Test]
        public void GetValueOrLast_WithEmptyList_ReturnsDefault()
        {
            Assert.AreEqual(0, _emptyList.GetValueOrLast(0));
        }

        [Test]
        public void IsNullOrEmpty_WithEmptyList_ReturnsTrue()
        {
            Assert.IsTrue(_emptyList.IsNullOrEmpty());
        }

        [Test]
        public void IsNullOrEmpty_WithNonEmptyList_ReturnsFalse()
        {
            Assert.IsFalse(_numberList.IsNullOrEmpty());
        }

        [Test]
        public void IsNullOrEmpty_WithNullList_ReturnsTrue()
        {
            IReadOnlyList<int> nullList = null;
            Assert.IsTrue(nullList.IsNullOrEmpty());
        }
    }
}
