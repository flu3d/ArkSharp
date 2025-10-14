using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ArkSharp.Test.Concurrent
{
    [TestFixture]
    public class TestCircularQueue
    {
        [Test]
        public void Constructor_WithInvalidCapacity_ThrowsArgumentException()
        {
            // 测试非2的幂次方容量
            Assert.Throws<ArgumentException>(() => new CircularQueue<int>(3));
            Assert.Throws<ArgumentException>(() => new CircularQueue<int>(1));

            // 测试有效容量不应抛出异常
            Assert.DoesNotThrow(() => new CircularQueue<int>(2));
            Assert.DoesNotThrow(() => new CircularQueue<int>(4));
        }

        [Test]
        public void IsEmpty_NewQueue_ReturnsTrue()
        {
            var queue = new CircularQueue<int>(4);
            Assert.IsTrue(queue.IsEmpty);
        }

        [Test]
        public void TryEnqueue_EmptyQueue_Success()
        {
            var queue = new CircularQueue<int>(4);
            Assert.IsTrue(queue.TryEnqueue(1));
            Assert.IsFalse(queue.IsEmpty);
        }

        [Test]
        public void TryEnqueue_FullQueue_ReturnsFalse()
        {
            var queue = new CircularQueue<int>(2);
            Assert.IsTrue(queue.TryEnqueue(1));
            Assert.IsFalse(queue.TryEnqueue(2)); // 由于内部实现需要保留一个空位，所以这里就会满
        }

        [Test]
        public void TryDequeue_EmptyQueue_ReturnsFalse()
        {
            var queue = new CircularQueue<int>(4);
            Assert.IsFalse(queue.TryDequeue(out var _));
        }

        [Test]
        public void TryDequeue_NonEmptyQueue_ReturnsItemInOrder()
        {
            var queue = new CircularQueue<int>(4);
            queue.TryEnqueue(1);
            queue.TryEnqueue(2);

            Assert.IsTrue(queue.TryDequeue(out var item1));
            Assert.AreEqual(1, item1);
            Assert.IsTrue(queue.TryDequeue(out var item2));
            Assert.AreEqual(2, item2);
            Assert.IsTrue(queue.IsEmpty);
        }

        [Test]
        public void EnqueueDequeue_CircularBehavior_WorksCorrectly()
        {
            var queue = new CircularQueue<int>(4);

            // 填充队列
            Assert.IsTrue(queue.TryEnqueue(1));
            Assert.IsTrue(queue.TryEnqueue(2));
            Assert.IsTrue(queue.TryEnqueue(3));

            // 取出两个元素
            Assert.IsTrue(queue.TryDequeue(out var item1));
            Assert.AreEqual(1, item1);
            Assert.IsTrue(queue.TryDequeue(out var item2));
            Assert.AreEqual(2, item2);

            // 再次入队，测试循环行为
            Assert.IsTrue(queue.TryEnqueue(4));
            Assert.IsTrue(queue.TryEnqueue(5));

            // 验证出队顺序
            Assert.IsTrue(queue.TryDequeue(out var item3));
            Assert.AreEqual(3, item3);
            Assert.IsTrue(queue.TryDequeue(out var item4));
            Assert.AreEqual(4, item4);
            Assert.IsTrue(queue.TryDequeue(out var item5));
            Assert.AreEqual(5, item5);
        }
    }
}
