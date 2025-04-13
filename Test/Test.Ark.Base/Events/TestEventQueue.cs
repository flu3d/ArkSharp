using System;
using NUnit.Framework;

namespace Ark.Test
{
	public class TestEventQueue
	{
		[Test]
		public void TestEvent0()
		{
			var d = new TestEventData();

			var dispatcher = new EventDispatcher();
			var queue = new EventQueue(dispatcher);

			dispatcher.Add("e0", d.Inc0);

			queue.Enqueue("e0");
			queue.Enqueue("e0");
			queue.Enqueue("e0");
			queue.PollAll();
			Assert.AreEqual(3, d.iVal);

			dispatcher.Add("e0", d.Dec0);

			queue.Enqueue("e0");
			queue.Enqueue("e0");
			queue.PollAll();
			Assert.AreEqual(3, d.iVal);

			dispatcher.Remove("e0", (Action)d.Inc0); // TODO

			queue.Enqueue("e0");
			queue.PollAll();
			Assert.AreEqual(2, d.iVal);

			dispatcher.Remove("e0");

			queue.Enqueue("e0");
			queue.PollAll();
			Assert.AreEqual(2, d.iVal);
		}

		[Test]
		public void TestEvent012()
		{
			var d = new TestEventData();

			var dispatcher = new EventDispatcher();
			var queue = new EventQueue(dispatcher);

			dispatcher.Add("e0", d.Inc0);
			dispatcher.Add("e1", (Action<int>)d.Inc1);
			dispatcher.Add("e2", (Action<int, string>)d.Inc2);

			queue.Enqueue("e0");
			queue.PollAll();
			Assert.AreEqual(1, d.iVal);
			Assert.AreEqual("", d.sVal);

			queue.Enqueue("e1", 2);
			queue.PollAll();
			Assert.AreEqual(3, d.iVal);
			Assert.AreEqual("", d.sVal);

			queue.Enqueue("e2", 3, "hello");
			queue.PollAll();
			Assert.AreEqual(6, d.iVal);
			Assert.AreEqual("hello", d.sVal);
		}
	}
}
