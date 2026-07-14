using System;
using NUnit.Framework;
using ArkSharp;

namespace ArkSharp.Test.Collections
{
    [TestFixture]
    public class TestRefStructList
    {
        private struct TestStruct
        {
            public int Value;
            public string Name;
        }

        [Test]
        public void Constructor_WithNegativeCapacity_CreatesEmptyList()
        {
            var list = new RefStructList<TestStruct>(-1);
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void Constructor_WithValidCapacity_CreatesEmptyList()
        {
            var list = new RefStructList<TestStruct>(10);
            Assert.AreEqual(0, list.Count);
            Assert.AreEqual(10, list.GetRawBuffer().Length);
        }

        [Test]
        public void Add_WithValue_AddsToList()
        {
            var list = new RefStructList<TestStruct>(2);
            var item = new TestStruct { Value = 42, Name = "test" };

            list.Add(item);

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(42, list[0].Value);
            Assert.AreEqual("test", list[0].Name);
        }

        [Test]
        public void AddRef_ReturnsRefToNewItem()
        {
            var list = new RefStructList<TestStruct>(2);
            ref var item = ref list.AddRef();

            item.Value = 42;
            item.Name = "test";

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(42, list[0].Value);
            Assert.AreEqual("test", list[0].Name);
        }

        [Test]
        public void Ref_ModifiesItemByReference()
        {
            var list = new RefStructList<TestStruct>(2);
            list.Add(new TestStruct { Value = 1, Name = "old" });

            ref var item = ref list.Ref(0);
            item.Value = 42;
            item.Name = "new";

            Assert.AreEqual(42, list[0].Value);
            Assert.AreEqual("new", list[0].Name);
        }

        [Test]
        public void Enlarge_WithSmallerCount_DoesNothing()
        {
            var list = new RefStructList<TestStruct>(5);
            list.Add(new TestStruct { Value = 1 });
            list.Add(new TestStruct { Value = 2 });

            list.Enlarge(1);

            Assert.AreEqual(2, list.Count);
        }

        [Test]
        public void Enlarge_WithLargerCount_IncreasesSize()
        {
            var list = new RefStructList<TestStruct>(2);
            list.Add(new TestStruct { Value = 1 });

            list.Enlarge(3);

            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(1, list[0].Value);
            Assert.AreEqual(0, list[1].Value); // 新增的元素应该是默认值
            Assert.AreEqual(0, list[2].Value);
        }

        [Test]
        public void RemoveAt_WithValidIndex_RemovesAndSwapsWithLast()
        {
            var list = new RefStructList<TestStruct>(3);
            list.Add(new TestStruct { Value = 1 });
            list.Add(new TestStruct { Value = 2 });
            list.Add(new TestStruct { Value = 3 });

            list.RemoveAt(1);

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(1, list[0].Value);
            Assert.AreEqual(3, list[1].Value); // 最后一个元素被交换到了删除位置
        }

        [Test]
        public void RemoveAt_WithInvalidIndex_DoesNothing()
        {
            var list = new RefStructList<TestStruct>(2);
            list.Add(new TestStruct { Value = 1 });

            list.RemoveAt(-1);
            list.RemoveAt(1);

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(1, list[0].Value);
        }

        [Test]
        public void Clear_WithoutResetFunc_SetsDefaultValues()
        {
            var list = new RefStructList<TestStruct>(2);
            list.Add(new TestStruct { Value = 1, Name = "test1" });
            list.Add(new TestStruct { Value = 2, Name = "test2" });

            list.Clear();

            Assert.AreEqual(0, list.Count);
            // 验证底层数组已被清空
            var buffer = list.GetRawBuffer();
            Assert.AreEqual(0, buffer[0].Value);
            Assert.AreEqual(null, buffer[0].Name);
        }

        [Test]
        public void Clear_WithResetFunc_CallsResetFunc()
        {
            int resetCount = 0;
            void ResetFunc(ref TestStruct item)
            {
                item.Value = -1;
                item.Name = "reset";
                resetCount++;
            }

            var list = new RefStructList<TestStruct>(2, ResetFunc);
            list.Add(new TestStruct { Value = 1, Name = "test1" });
            list.Add(new TestStruct { Value = 2, Name = "test2" });

            list.Clear();

            Assert.AreEqual(0, list.Count);
            Assert.AreEqual(2, resetCount);
            // 验证重置函数被调用
            var buffer = list.GetRawBuffer();
            Assert.AreEqual(-1, buffer[0].Value);
            Assert.AreEqual("reset", buffer[0].Name);
        }

        [Test]
        public void RemoveAt_WithResetFunc_CallsResetFunc()
        {
            int resetCount = 0;
            void ResetFunc(ref TestStruct item)
            {
                item.Value = -1;
                item.Name = "reset";
                resetCount++;
            }

            var list = new RefStructList<TestStruct>(2, ResetFunc);
            list.Add(new TestStruct { Value = 1, Name = "test1" });
            list.Add(new TestStruct { Value = 2, Name = "test2" });

            list.RemoveAt(0);

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(1, resetCount);
            Assert.AreEqual(2, list[0].Value); // 验证最后一个元素被交换到了位置0
        }
    }
}
