using System;
using NUnit.Framework;
using ArkSharp;

namespace ArkSharp.Test.Collections
{
    [TestFixture]
    public class TestArrayHelper
    {
        [Test]
        public void MaxLength_ReturnsCorrectValue()
        {
            Assert.AreEqual(0x7FFFFFC7, ArrayHelper.MaxLength);
        }

        [Test]
        public void Clear_WithValidRange_ClearsArray()
        {
            var array = new int[] { 1, 2, 3, 4, 5 };
            array.Clear(1, 3);
            Assert.AreEqual(1, array[0]);
            Assert.AreEqual(0, array[1]);
            Assert.AreEqual(0, array[2]);
            Assert.AreEqual(0, array[3]);
            Assert.AreEqual(5, array[4]);
        }

        [Test]
        public void Clear_WithFullRange_ClearsEntireArray()
        {
            var array = new int[] { 1, 2, 3, 4, 5 };
            array.Clear();
            Assert.AreEqual(0, array[0]);
            Assert.AreEqual(0, array[1]);
            Assert.AreEqual(0, array[2]);
            Assert.AreEqual(0, array[3]);
            Assert.AreEqual(0, array[4]);
        }

        [Test]
        public void EnsureCapacity_WithSmallerCount_ReturnsTrue()
        {
            var array = new int[10];
            Assert.IsTrue(ArrayHelper.EnsureCapacity(ref array, 5));
            Assert.AreEqual(10, array.Length);
        }

        [Test]
        public void EnsureCapacity_WithLargerCount_ExpandsArray()
        {
            var array = new int[10];
            Assert.IsTrue(ArrayHelper.EnsureCapacity(ref array, 20));
            Assert.AreEqual(20, array.Length);
        }

        [Test]
        public void EnsureCapacity_WithEmptyArray_UsesMinCapacity()
        {
            var array = new int[0];
            Assert.IsTrue(ArrayHelper.EnsureCapacity(ref array, 5, 16));
            Assert.AreEqual(16, array.Length);
        }

        [Test]
        public void EnsureCapacity_WithMaxLength_ResizesToMaxLength()
        {
            var array = new byte[10];
            Assert.IsTrue(ArrayHelper.EnsureCapacity(ref array, ArrayHelper.MaxLength + 1));
            Assert.AreEqual(ArrayHelper.MaxLength, array.Length);
        }

        [Test]
        public void EnsureCapacity_WithDoubling_ExpandsArrayCorrectly()
        {
            var array = new int[10];
            Assert.IsTrue(ArrayHelper.EnsureCapacity(ref array, 15));
            Assert.AreEqual(20, array.Length);
        }

        [Test]
        public void EnsureCapacity_WithMultipleDoubling_ExpandsArrayCorrectly()
        {
            var array = new int[10];
            Assert.IsTrue(ArrayHelper.EnsureCapacity(ref array, 30));
            Assert.AreEqual(40, array.Length);
        }
    }
}
