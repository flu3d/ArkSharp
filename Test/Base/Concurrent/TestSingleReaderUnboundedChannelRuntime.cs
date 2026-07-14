using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace ArkSharp.Test.Concurrent
{
    [TestFixture]
    public class TestSingleReaderUnboundedChannelRuntime
    {
        /// <summary>
        /// 测试目的：验证在单生产者-单消费者场景下的线程安全性和数据完整性
        /// 1. 验证大量数据（10000个）能被正确写入和读取
        /// 2. 验证写入操作总是成功（无界队列特性）
        /// 3. 验证数据顺序和完整性（通过求和验证）
        /// 4. 验证通道关闭后消费者能正确退出
        /// </summary>
        [RequiresPlayMode]
        [Test]
        public void ThreadSafety_SingleProducerSingleConsumer_WorksCorrectly()
        {
            var channel = new SingleReaderUnboundedChannel<int>();
            var count = 1000;
            var producerDone = false;
            var consumerSum = 0;
            var expectedSum = (count * (count - 1)) / 2;

            // 生产者任务
            var producerTask = Task.Run(() =>
            {
                for (int i = 0; i < count; i++)
                {
                    Assert.IsTrue(channel.Write(i), "写入应该总是成功的，因为是无界队列");
                }
                producerDone = true;
                channel.Close();
            });

            // 消费者任务
            var consumerTask = Task.Run(() =>
            {
                while (channel.Read(out var item))
                {
                    consumerSum += item;
                }
            });

            Task.WhenAll(producerTask, consumerTask).GetAwaiter().GetResult();

            Assert.IsTrue(producerDone);
            Assert.AreEqual(expectedSum, consumerSum);
        }

        /// <summary>
        /// 测试目的：验证多生产者-单消费者场景下的并发安全性
        /// 1. 验证多个生产者（4个）能同时向通道写入数据
        /// 2. 验证所有生产者的数据都能被正确消费
        /// 3. 验证最后一个生产者能正确关闭通道
        /// 4. 验证数据完整性（通过求和验证）
        /// </summary>
        [Test]
        public void MultipleProducers_SingleConsumer_WorksCorrectly()
        {
            var channel = new SingleReaderUnboundedChannel<int>();
            var producerCount = 4;
            var itemsPerProducer = 1000;
            var totalItems = producerCount * itemsPerProducer;
            var expectedSum = (itemsPerProducer * (itemsPerProducer - 1) / 2) * producerCount;
            var consumerSum = 0;
            var remainingProducers = producerCount;

            // 多个生产者任务
            var producerTasks = new Task[producerCount];
            for (int p = 0; p < producerCount; p++)
            {
                producerTasks[p] = Task.Run(() =>
                {
                    for (int i = 0; i < itemsPerProducer; i++)
                    {
                        Assert.IsTrue(channel.Write(i));
                    }
                    if (Interlocked.Decrement(ref remainingProducers) == 0)
                    {
                        channel.Close();
                    }
                });
            }

            // 消费者任务
            var consumerTask = Task.Run(() =>
            {
                while (channel.Read(out var item))
                {
                    consumerSum += item;
                }
            });

            Task.WhenAll(producerTasks.Concat(new[] { consumerTask })).GetAwaiter().GetResult();
            Assert.AreEqual(expectedSum, consumerSum);
        }

        /// <summary>
        /// 测试目的：验证向已关闭的通道写入数据的行为
        /// 1. 验证向已关闭通道写入数据会返回false
        /// 2. 确保通道关闭状态的正确性
        /// </summary>
        [Test]
        public void Write_ToClosedChannel_ReturnsFalse()
        {
            var channel = new SingleReaderUnboundedChannel<int>();
            channel.Close();
            Assert.IsFalse(channel.Write(1));
        }

        /// <summary>
        /// 测试目的：验证从空的且已关闭的通道读取数据的行为
        /// 1. 验证从空的已关闭通道读取数据会返回false
        /// 2. 确保通道在关闭且为空时的读取行为符合预期
        /// </summary>
        [Test]
        public void Read_FromEmptyAndClosedChannel_ReturnsFalse()
        {
            var channel = new SingleReaderUnboundedChannel<int>();
            channel.Close();
            Assert.IsFalse(channel.Read(out _));
        }

        /// <summary>
        /// 测试目的：验证从非空但已关闭的通道读取数据的行为
        /// 1. 验证已关闭通道中的现有数据仍然可以被读取
        /// 2. 验证数据按照写入顺序被正确读取
        /// 3. 验证读取完所有数据后返回false
        /// </summary>
        [Test]
        public void Read_FromNonEmptyAndClosedChannel_ReturnsAllItems()
        {
            var channel = new SingleReaderUnboundedChannel<int>();
            var items = new[] { 1, 2, 3, 4, 5 };

            foreach (var item in items)
            {
                Assert.IsTrue(channel.Write(item));
            }
            channel.Close();

            for (int i = 0; i < items.Length; i++)
            {
                Assert.IsTrue(channel.Read(out var item));
                Assert.AreEqual(items[i], item);
            }
            Assert.IsFalse(channel.Read(out _));
        }

        /// <summary>
        /// 测试目的：验证通道的Dispose行为
        /// 1. 验证调用Dispose会关闭通道
        /// 2. 验证通道关闭后的状态正确性
        /// 3. 验证关闭后的写入操作会返回false
        /// </summary>
        [Test]
        public void Dispose_ClosesChannel()
        {
            var channel = new SingleReaderUnboundedChannel<int>();
            channel.Dispose();
            Assert.IsTrue(channel.IsClosed);
            Assert.IsFalse(channel.Write(1));
        }

        /// <summary>
        /// 测试目的：验证并发操作下的死锁防护
        /// 1. 验证并发的读取、写入和关闭操作不会导致死锁
        /// 2. 使用CancellationTokenSource确保测试在5秒内完成
        /// 3. 验证通道最终能被正确关闭
        /// 4. 验证所有并发操作能正常结束
        /// </summary>
        [Test]
        public void ConcurrentOperations_DoNotDeadlock()
        {
            var channel = new SingleReaderUnboundedChannel<int>();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var writeTask = Task.Run(() =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    channel.Write(1);
                    Thread.Sleep(1);
                }
            });

            var readTask = Task.Run(() =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    channel.Read(out _);
                    Thread.Sleep(1);
                }
            });

            var closeTask = Task.Run(() =>
            {
                Thread.Sleep(100);
                channel.Close();
            });

            Task.WhenAll(writeTask, readTask, closeTask).GetAwaiter().GetResult();
            Assert.IsTrue(channel.IsClosed);
        }
    }
}
