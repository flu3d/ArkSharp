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
            var producerDone = false;
            var consumerSum = 0;
            var expectedSum = (count * (count - 1)) / 2; // 0到count-1的和

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
                producerDone = true;
            });

            // 消费者任务
            var consumerTask = Task.Run(() =>
            {
                while (!producerDone || !queue.IsEmpty)
                {
                    if (queue.TryDequeue(out var item))
                    {
                        consumerSum += item;
                    }
                    else
                    {
                        Thread.Sleep(1); // 队列空时等待
                    }
                }
            });

            Task.WhenAll(producerTask, consumerTask).GetAwaiter().GetResult();
            Assert.AreEqual(expectedSum, consumerSum);
        }
    }
}
