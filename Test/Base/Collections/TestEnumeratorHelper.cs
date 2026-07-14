using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ArkSharp;

namespace ArkSharp.Test.Collections
{
    [TestFixture]
    public class TestEnumeratorHelper
    {
        private List<int> _sourceList;
        private IEnumerator<int> _enumerator;

        [SetUp]
        public void Setup()
        {
            _sourceList = new List<int> { 1, 2, 3, 4, 5 };
            _enumerator = _sourceList.GetEnumerator();
        }

        [TearDown]
        public void Cleanup()
        {
            _enumerator?.Dispose();
            _enumerator = null;
            _sourceList = null;
        }

        [Test]
        public void GetEnumerator_WithForeach_IteratesAllItems()
        {
            var result = new List<int>();

            // 测试直接对 IEnumerator 使用 foreach
            foreach (var item in _enumerator)
            {
                result.Add(item);
            }

            Assert.AreEqual(_sourceList.Count, result.Count);
            CollectionAssert.AreEqual(_sourceList, result);
        }

        [Test]
        public void GetEnumerator_WithMultipleForeach_OnlyIteratesOnce()
        {
            var result1 = new List<int>();
            var result2 = new List<int>();

            // 第一次遍历
            foreach (var item in _enumerator)
            {
                result1.Add(item);
            }

            // 第二次遍历应该没有元素了，因为迭代器已经到达末尾
            foreach (var item in _enumerator)
            {
                result2.Add(item);
            }

            Assert.AreEqual(_sourceList.Count, result1.Count);
            Assert.AreEqual(0, result2.Count);
            CollectionAssert.AreEqual(_sourceList, result1);
        }

        [Test]
        public void AsEnumerable_WithLinq_SupportsLinqOperations()
        {
            var enumerable = _enumerator.AsEnumerable();

            // 测试是否支持 LINQ 操作
            var result = enumerable
                .Where(x => x % 2 == 0)  // 筛选偶数
                .Select(x => x * 2)      // 每个数乘以2
                .ToList();

            CollectionAssert.AreEqual(new[] { 4, 8 }, result);
        }

        [Test]
        public void AsEnumerable_WithMultipleEnumeration_ReusesEnumerator()
        {
            var enumerable = _enumerator.AsEnumerable();

            // 第一次遍历
            var result1 = enumerable.ToList();

            // 第二次遍历应该没有元素，因为使用的是同一个迭代器且已经到达末尾
            var result2 = enumerable.ToList();

            Assert.AreEqual(_sourceList.Count, result1.Count);  // 第一次遍历可以获取所有元素
            Assert.AreEqual(0, result2.Count);  // 第二次遍历无法获取元素，因为迭代器已经到达末尾
            CollectionAssert.AreEqual(_sourceList, result1);
        }

        [Test]
        public void AsEnumerable_WithNullEnumerator_ThrowsNullReferenceException()
        {
            IEnumerator<int> nullEnumerator = null;
            var enumerable = nullEnumerator.AsEnumerable();

            // 这里应该抛出 NullReferenceException
            Assert.Catch<NullReferenceException>(() => enumerable.ToList());
        }
    }
}
