using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace ArkSharp.Test.Concurrent
{
    [RequiresPlayMode]
    [TestFixture]
    public class TestCircularQueueRuntime
    {
        [Test]
		public void ThreadSafety_SingleProducerSingleConsumer_WorksCorrectly()
		{
			var queue = new CircularQueue<int>(8);
			var count = 1000;
			var received = new int[count];

            // 生产者任务
            var producerTask = Task.Run(() =>
            {
                for (int i = 0; i < count; i++)
                {
                    while (!queue.TryEnqueue(i))
                    {
                        Thread.Sleep(1); // 队列满时等待
                    }
                }
			});

            // 消费者任务
            var consumerTask = Task.Run(() =>
            {
				int index = 0;
				while (index < count)
				{
					if (queue.TryDequeue(out var item))
					{
						received[index++] = item;
                    }
                    else
                    {
                        Thread.Sleep(1); // 队列空时等待
                    }
                }
            });

			var allTasks = Task.WhenAll(producerTask, consumerTask);
			Assert.IsTrue(allTasks.Wait(System.TimeSpan.FromSeconds(5)), "生产者和消费者未能在限定时间内完成");
			allTasks.GetAwaiter().GetResult();

			for (int i = 0; i < count; i++)
				Assert.AreEqual(i, received[i]);
		}
    }
}
