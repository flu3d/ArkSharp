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
    public class TestCircularQueueRuntime
    {
        [Test]
		public void ThreadSafety_SingleProducerSingleConsumer_WorksCorrectly()
		{
			var queue = new CircularQueue<int>(8);
			var count = 1000;
			var received = new int[count];
			int isStopped = 0;

            // 生产者任务
            var producerTask = Task.Run(() =>
            {
				for (int i = 0; i < count; i++)
				{
					while (!queue.TryEnqueue(i))
					{
						if (Volatile.Read(ref isStopped) != 0)
							return;

						Thread.Sleep(1); // 队列满时等待
					}
                }
			});

            // 消费者任务
            var consumerTask = Task.Run(() =>
            {
				int index = 0;
				while (index < count && Volatile.Read(ref isStopped) == 0)
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
			try
			{
				WaitForCompletion(allTasks);

				for (int i = 0; i < count; i++)
					Assert.AreEqual(i, received[i]);
			}
			finally
			{
				Volatile.Write(ref isStopped, 1);
				WaitForCompletion(allTasks);
			}
		}

		private static void WaitForCompletion(Task task)
		{
			Assert.IsTrue(task.Wait(System.TimeSpan.FromSeconds(5)), "生产者和消费者未能在限定时间内完成");
			task.GetAwaiter().GetResult();
		}
    }
}
