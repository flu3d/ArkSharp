using System;
using NUnit.Framework;

namespace ArkSharp.Test.Events
{
	public class TestEventDispatcher
	{
		static int _staticCallbackCount;

		static void InvokeStaticCallback() => _staticCallbackCount++;

		[Test]
		public void TestEvent0()
		{
			var d = new TestEventData();

			var dispatcher = new EventDispatcher();

			dispatcher.Add("e0", d.Inc0);

			dispatcher.Dispatch("e0");
			Assert.AreEqual(1, d.iVal);
			dispatcher.Dispatch("e0");
			Assert.AreEqual(2, d.iVal);
			dispatcher.Dispatch("e0");
			Assert.AreEqual(3, d.iVal);

			dispatcher.Add("e0", d.Dec0);

			dispatcher.DispatchWith("e0", null);
			Assert.AreEqual(3, d.iVal);
			dispatcher.DispatchWith("e0", null);
			Assert.AreEqual(3, d.iVal);

			dispatcher.Remove("e0", (Action)d.Inc0); // TODO

			dispatcher.DispatchWith("e0", null);
			Assert.AreEqual(2, d.iVal);

			dispatcher.Remove("e0");

			dispatcher.Dispatch("e0");
			Assert.AreEqual(2, d.iVal);
		}

		[Test]
		public void TestEvent012()
		{
			var d = new TestEventData();

			var dispatcher = new EventDispatcher();

			dispatcher.Add("e0", d.Inc0);
			dispatcher.Add("e1", (Action<int>)d.Inc1);
			dispatcher.Add("e2", (Action<int, string>)d.Inc2);

			dispatcher.Dispatch("e0");
			Assert.AreEqual(1, d.iVal);
			Assert.AreEqual("", d.sVal);

			dispatcher.Dispatch("e1", 2);
			Assert.AreEqual(3, d.iVal);
			Assert.AreEqual("", d.sVal);

			dispatcher.Dispatch("e2", 3, "hello");
			Assert.AreEqual(6, d.iVal);
			Assert.AreEqual("hello", d.sVal);
		}

		[Test]
		public void TestEvent345()
		{
			var dispatcher = new EventDispatcher();
			var value3 = 0;
			var value4 = 0;
			var value5 = 0;

			dispatcher.Add("e3", (Action<int, int, int>)((a, b, c) => value3 = a + b + c));
			dispatcher.Add("e4", (Action<int, int, int, int>)((a, b, c, d) => value4 = a + b + c + d));
			dispatcher.Add("e5", (Action<int, int, int, int, int>)((a, b, c, d, e) => value5 = a + b + c + d + e));

			dispatcher.Dispatch("e3", 1, 2, 3);
			dispatcher.Dispatch("e4", 1, 2, 3, 4);
			dispatcher.Dispatch("e5", 1, 2, 3, 4, 5);

			Assert.AreEqual(6, value3);
			Assert.AreEqual(10, value4);
			Assert.AreEqual(15, value5);
		}

		[Test]
		public void TestRemoveTargetAndRemoveAll()
		{
			var first = new TestEventData();
			var second = new TestEventData();
			var dispatcher = new EventDispatcher();

			dispatcher.Add("e0", (Action<int>)first.Inc1);
			dispatcher.Add("e1", (Action<int>)first.Inc1);
			dispatcher.Add("e0", (Action<int>)second.Inc1);
			dispatcher.RemoveTarget(first);
			dispatcher.Dispatch("e0", 1);
			dispatcher.Dispatch("e1", 1);

			Assert.AreEqual(0, first.iVal);
			Assert.AreEqual(1, second.iVal);

			dispatcher.RemoveAll();
			dispatcher.Dispatch("e0", 1);
			Assert.AreEqual(1, second.iVal);
		}

		[Test]
		public void TestRemoveNullTargetDoesNotRemoveStaticCallback()
		{
			_staticCallbackCount = 0;

			var dispatcher = new EventDispatcher();
			dispatcher.Add("event", InvokeStaticCallback);
			dispatcher.RemoveTarget(null);
			dispatcher.Dispatch("event");

			Assert.AreEqual(1, _staticCallbackCount);
		}
	}
}
