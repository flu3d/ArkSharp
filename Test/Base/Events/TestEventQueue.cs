using System;
using NUnit.Framework;

namespace ArkSharp.Test.Events
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

		[Test]
		public void TestEvent345()
		{
			var dispatcher = new EventDispatcher();
			var queue = new EventQueue(dispatcher);
			var value3 = 0;
			var value4 = 0;
			var value5 = 0;

			dispatcher.Add("e3", (Action<int, int, int>)((a, b, c) => value3 = a + b + c));
			dispatcher.Add("e4", (Action<int, int, int, int>)((a, b, c, d) => value4 = a + b + c + d));
			dispatcher.Add("e5", (Action<int, int, int, int, int>)((a, b, c, d, e) => value5 = a + b + c + d + e));

			queue.Enqueue("e3", 1, 2, 3);
			queue.Enqueue("e4", 1, 2, 3, 4);
			queue.Enqueue("e5", 1, 2, 3, 4, 5);
			queue.PollAll();

			Assert.AreEqual(6, value3);
			Assert.AreEqual(10, value4);
			Assert.AreEqual(15, value5);
		}

		[Test]
		public void TestPollOneAndClear()
		{
			var dispatcher = new EventDispatcher();
			var queue = new EventQueue(dispatcher);
			var values = "";

			dispatcher.Add("first", (Action)(() => values += "A"));
			dispatcher.Add("second", (Action)(() => values += "B"));

			queue.Enqueue("first");
			queue.Enqueue("second");
			Assert.IsTrue(queue.PollOne());
			Assert.AreEqual("A", values);
			Assert.IsTrue(queue.PollOne());
			Assert.AreEqual("AB", values);
			Assert.IsFalse(queue.PollOne());

			queue.Enqueue("first");
			queue.Clear();
			Assert.IsFalse(queue.PollOne());

			queue.Enqueue("second");
			queue.PollAll();
			Assert.AreEqual("ABB", values);
		}

		[Test]
		public void TestPollAllDefersEventsAddedDuringDispatch()
		{
			var d = new TestEventData();
			var dispatcher = new EventDispatcher();
			var queue = new EventQueue(dispatcher);

			dispatcher.Add("event", (Action)(() =>
			{
				d.Inc0();
				if (d.iVal == 1)
					queue.Enqueue("event");
			}));

			queue.Enqueue("event");
			queue.PollAll();
			Assert.AreEqual(1, d.iVal);

			queue.PollAll();
			Assert.AreEqual(2, d.iVal);
		}
	}
}
