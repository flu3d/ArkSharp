using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

#if UNITY_5_3_OR_NEWER
using UnityEngine.TestTools;
#endif

namespace ArkSharp.Test.Concurrent
{
#if UNITY_5_3_OR_NEWER
    [RequiresPlayMode]
#endif
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
        [Test]
		public void ThreadSafety_SingleProducerSingleConsumer_WorksCorrectly()
		{
			var channel = new SingleReaderUnboundedChannel<int>();
			var count = 1000;
			var received = new int[count];
			int writeFailureCount = 0;

            // 生产者任务
            var producerTask = Task.Run(() =>
			{
				for (int i = 0; i < count; i++)
				{
					if (!channel.Write(i))
						Interlocked.Increment(ref writeFailureCount);
				}
				channel.Close();
			});

            // 消费者任务
            var consumerTask = Task.Run(() =>
			{
				int index = 0;
				while (channel.Read(out var item))
				{
					received[index++] = item;
				}
			});

			try
			{
				WaitForCompletion(Task.WhenAll(producerTask, consumerTask));

				Assert.Zero(writeFailureCount);
				for (int i = 0; i < count; i++)
					Assert.AreEqual(i, received[i]);
			}
			finally
			{
				channel.Close();
				WaitForCompletion(Task.WhenAll(producerTask, consumerTask));
			}
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
			var received = new bool[totalItems];
			int receivedCount = 0;
			int invalidItemCount = 0;
			int writeFailureCount = 0;
			var remainingProducers = producerCount;

            // 多个生产者任务
			var producerTasks = new Task[producerCount];
			for (int p = 0; p < producerCount; p++)
			{
				int producerIndex = p;
				producerTasks[p] = Task.Run(() =>
				{
					for (int i = 0; i < itemsPerProducer; i++)
					{
						if (!channel.Write(producerIndex * itemsPerProducer + i))
							Interlocked.Increment(ref writeFailureCount);
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
					if (item < 0 || item >= totalItems || received[item])
					{
						invalidItemCount++;
						continue;
					}

					received[item] = true;
					receivedCount++;
				}
			});

			try
			{
				WaitForCompletion(Task.WhenAll(producerTasks.Concat(new[] { consumerTask })));
				Assert.Zero(writeFailureCount);
				Assert.Zero(invalidItemCount);
				Assert.AreEqual(totalItems, receivedCount);
				Assert.IsTrue(received.All(x => x));
			}
			finally
			{
				channel.Close();
				WaitForCompletion(Task.WhenAll(producerTasks.Concat(new[] { consumerTask })));
			}
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

		[Test]
		public void Read_WhenEmpty_IsUnblockedByWrite()
		{
			var channel = new SingleReaderUnboundedChannel<int>();
			using var readerStarted = new ManualResetEventSlim();
			var readTask = Task.Run(() =>
			{
				readerStarted.Set();
				return channel.Read(out var item) ? item : -1;
			});

			try
			{
				Assert.IsTrue(readerStarted.Wait(TimeSpan.FromSeconds(1)));
				Assert.IsTrue(channel.Write(42));
				Assert.IsTrue(readTask.Wait(TimeSpan.FromSeconds(1)), "阻塞读取未能在限定时间内完成");
				Assert.AreEqual(42, readTask.GetAwaiter().GetResult());
			}
			finally
			{
				channel.Close();
				WaitForCompletion(readTask);
			}
		}

		[Test]
		public void Read_WhenEmpty_IsUnblockedByClose()
		{
			var channel = new SingleReaderUnboundedChannel<int>();
			using var readerStarted = new ManualResetEventSlim();
			var readTask = Task.Run(() =>
			{
				readerStarted.Set();
				return channel.Read(out _);
			});

			try
			{
				Assert.IsTrue(readerStarted.Wait(TimeSpan.FromSeconds(1)));
				channel.Close();
				Assert.IsTrue(readTask.Wait(TimeSpan.FromSeconds(1)), "阻塞读取未能在限定时间内完成");
				Assert.IsFalse(readTask.GetAwaiter().GetResult());
			}
			finally
			{
				channel.Close();
				WaitForCompletion(readTask);
			}
		}

		[Test]
		public void SuccessfulWrites_AreAllReadable_WhenCloseRacesWithWriters()
		{
			const int producerCount = 4;
			const int itemsPerProducer = 1000;

			for (int attempt = 0; attempt < 20; attempt++)
			{
				var channel = new SingleReaderUnboundedChannel<int>();
				using var start = new ManualResetEventSlim();
				int startedProducerCount = 0;
				int successfulWriteCount = 0;
				var producerTasks = new Task[producerCount];

				for (int producerIndex = 0; producerIndex < producerCount; producerIndex++)
				{
					producerTasks[producerIndex] = Task.Run(() =>
					{
						start.Wait();
						Interlocked.Increment(ref startedProducerCount);

						for (int item = 0; item < itemsPerProducer; item++)
						{
							if (!channel.Write(item))
								break;

							Interlocked.Increment(ref successfulWriteCount);
						}
					});
				}

				try
				{
					start.Set();
					Assert.IsTrue(SpinWait.SpinUntil(() =>
						Volatile.Read(ref startedProducerCount) == producerCount && Volatile.Read(ref successfulWriteCount) > 0,
						TimeSpan.FromSeconds(1)));
					channel.Close();

					WaitForCompletion(Task.WhenAll(producerTasks));

					int readCount = 0;
					while (channel.Read(out _))
						readCount++;

					Assert.Greater(successfulWriteCount, 0);
					Assert.AreEqual(successfulWriteCount, readCount);
				}
				finally
				{
					start.Set();
					channel.Close();
					WaitForCompletion(Task.WhenAll(producerTasks));
				}
			}
		}

		[Test]
		public void Close_CalledRepeatedly_RemainsClosed()
		{
			var channel = new SingleReaderUnboundedChannel<int>();
			channel.Close();
			channel.Close();

			Assert.IsTrue(channel.IsClosed);
			Assert.IsFalse(channel.Write(1));
			Assert.IsFalse(channel.Read(out _));
		}

		private static void WaitForCompletion(Task task)
		{
			Assert.IsTrue(task.Wait(TimeSpan.FromSeconds(5)), "并发任务未能在限定时间内完成");
			task.GetAwaiter().GetResult();
		}

    }
}
