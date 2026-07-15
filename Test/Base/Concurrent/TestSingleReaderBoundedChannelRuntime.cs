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
    public class TestSingleReaderBoundedChannelRuntime
    {
        /// <summary>
        /// 测试目的：验证在单生产者-单消费者场景下的线程安全性和数据完整性
        /// 1. 验证在有界队列（容量8）下的生产者-消费者模式
        /// 2. 验证当队列满时生产者会等待
        /// 3. 验证数据的完整性和FIFO顺序
        /// 4. 验证通道关闭后消费者能正确退出
        /// </summary>
        [Test]
		public void ThreadSafety_SingleProducerSingleConsumer_WorksCorrectly()
		{
			var channel = new SingleReaderBoundedChannel<int>(8);
			var count = 1000;
            var received = new int[count];

            // 生产者任务
            var producerTask = Task.Run(() =>
            {
				for (int i = 0; i < count; i++)
				{
					while (!channel.Write(i))
					{
						if (channel.IsClosed)
							return;

						Thread.Sleep(1);
					}
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
        /// 测试目的：验证向已关闭的通道写入数据的行为
        /// 1. 验证向已关闭通道写入数据会返回false
        /// 2. 确保通道关闭状态的正确性
        /// </summary>
        [Test]
        public void Write_ToClosedChannel_ReturnsFalse()
        {
            var channel = new SingleReaderBoundedChannel<int>(2);
            channel.Close();
            Assert.IsFalse(channel.Write(1));
        }

        /// <summary>
        /// 测试目的：验证向已满通道写入数据的行为
        /// 1. 验证向已满通道写入数据会返回false
        /// 2. 验证通道容量限制的正确实现
        /// 3. 确保第一次写入成功，第二次写入失败（由于循环队列实现需要保留一个空位）
        /// </summary>
        [Test]
        public void Write_ToFullChannel_ReturnsFalse()
        {
            var channel = new SingleReaderBoundedChannel<int>(2);
            Assert.IsTrue(channel.Write(1), "第一次写入应该成功");
            Assert.IsFalse(channel.Write(2), "第二次写入应该失败，因为循环队列需要保留一个空位");
        }

        /// <summary>
        /// 测试目的：验证从空的且已关闭的通道读取数据的行为
        /// 1. 验证从空的已关闭通道读取数据会返回false
        /// 2. 确保通道在关闭且为空时的读取行为符合预期
        /// </summary>
        [Test]
        public void Read_FromEmptyAndClosedChannel_ReturnsFalse()
        {
            var channel = new SingleReaderBoundedChannel<int>(2);
            channel.Close();
            Assert.IsFalse(channel.Read(out _));
        }

        /// <summary>
        /// 测试目的：验证从非空但已关闭的通道读取数据的行为
        /// 1. 验证已关闭通道中的现有数据仍然可以被读取
        /// 2. 验证数据按照写入顺序被正确读取（FIFO特性）
        /// 3. 验证读取完所有数据后返回false
        /// 4. 验证有界队列在部分填充状态下的正确性
        /// </summary>
        [Test]
        public void Read_FromNonEmptyAndClosedChannel_ReturnsAllItems()
        {
            var channel = new SingleReaderBoundedChannel<int>(2);
            Assert.IsTrue(channel.Write(1), "写入第一个元素应该成功");
            channel.Close();

            Assert.IsTrue(channel.Read(out var item1), "应该能读取到写入的元素");
            Assert.AreEqual(1, item1, "读取的元素值应该正确");
            Assert.IsFalse(channel.Read(out _), "通道为空时应该返回false");
        }

        /// <summary>
        /// 测试目的：验证通道的Dispose行为
        /// 1. 验证调用Dispose会关闭通道
        /// 2. 验证通道关闭后的状态正确性
        /// 3. 验证关闭后的写入操作会返回false
        /// 4. 确保资源正确释放
        /// </summary>
        [Test]
		public void Dispose_ClosesChannel()
		{
            var channel = new SingleReaderBoundedChannel<int>(2);
            channel.Dispose();
            Assert.IsTrue(channel.IsClosed);
			Assert.IsFalse(channel.Write(1));
		}

		[Test]
		public void Read_WhenEmpty_IsUnblockedByWrite()
		{
			var channel = new SingleReaderBoundedChannel<int>(2);
			using var readerStarted = new ManualResetEventSlim();
			var readTask = Task.Run(() =>
			{
				readerStarted.Set();
				return channel.Read(out var item) ? item : -1;
			});

			try
			{
				Assert.IsTrue(readerStarted.Wait(System.TimeSpan.FromSeconds(1)));
				Assert.IsTrue(channel.Write(42));
				Assert.IsTrue(readTask.Wait(System.TimeSpan.FromSeconds(1)), "阻塞读取未能在限定时间内完成");
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
			var channel = new SingleReaderBoundedChannel<int>(2);
			using var readerStarted = new ManualResetEventSlim();
			var readTask = Task.Run(() =>
			{
				readerStarted.Set();
				return channel.Read(out _);
			});

			try
			{
				Assert.IsTrue(readerStarted.Wait(System.TimeSpan.FromSeconds(1)));
				channel.Close();
				Assert.IsTrue(readTask.Wait(System.TimeSpan.FromSeconds(1)), "阻塞读取未能在限定时间内完成");
				Assert.IsFalse(readTask.GetAwaiter().GetResult());
			}
			finally
			{
				channel.Close();
				WaitForCompletion(readTask);
			}
		}

		[Test]
		public void MultipleProducers_SingleConsumer_ReceivesAllItems()
		{
			const int producerCount = 4;
			const int itemsPerProducer = 1000;
			const int totalItems = producerCount * itemsPerProducer;
			var channel = new SingleReaderBoundedChannel<int>(64);
			var received = new bool[totalItems];
			int receivedCount = 0;
			int invalidItemCount = 0;
			int remainingProducers = producerCount;
			var producerTasks = new Task[producerCount];

			for (int producerIndex = 0; producerIndex < producerCount; producerIndex++)
			{
				int index = producerIndex;
				producerTasks[producerIndex] = Task.Run(() =>
				{
					for (int sequence = 0; sequence < itemsPerProducer; sequence++)
					{
						int item = index * itemsPerProducer + sequence;
						while (!channel.Write(item))
						{
							if (channel.IsClosed)
								return;

							Thread.Yield();
						}
					}

					if (Interlocked.Decrement(ref remainingProducers) == 0)
						channel.Close();
				});
			}

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
				WaitForCompletion(Task.WhenAll(producerTasks));
				WaitForCompletion(consumerTask);
				Assert.Zero(invalidItemCount);
				Assert.AreEqual(totalItems, receivedCount);
				for (int i = 0; i < received.Length; i++)
					Assert.IsTrue(received[i], $"未收到元素{i}");
			}
			finally
			{
				channel.Close();
				WaitForCompletion(Task.WhenAll(producerTasks));
				WaitForCompletion(consumerTask);
			}
		}

		[Test]
		public void SuccessfulWrites_AreAllReadable_WhenCloseRacesWithWriters()
		{
			const int producerCount = 4;
			const int itemsPerProducer = 1000;

			for (int attempt = 0; attempt < 20; attempt++)
			{
				var channel = new SingleReaderBoundedChannel<int>(1024);
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
						System.TimeSpan.FromSeconds(1)));
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
			var channel = new SingleReaderBoundedChannel<int>(2);
			channel.Close();
			channel.Close();

			Assert.IsTrue(channel.IsClosed);
			Assert.IsFalse(channel.Write(1));
			Assert.IsFalse(channel.Read(out _));
		}

		private static void WaitForCompletion(Task task)
		{
			Assert.IsTrue(task.Wait(System.TimeSpan.FromSeconds(5)), "并发任务未能在限定时间内完成");
			task.GetAwaiter().GetResult();
		}

	}
}
