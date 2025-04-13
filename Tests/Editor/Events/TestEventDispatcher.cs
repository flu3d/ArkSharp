using System;
using NUnit.Framework;

namespace ArkSharp.Test
{
	public class TestEventDispatcher
	{
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
	}
}
