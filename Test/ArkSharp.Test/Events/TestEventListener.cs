using System;
using NUnit.Framework;

namespace ArkSharp.Test.Events
{
	public class TestEventListener
	{
		[Test]
		public void TestAddRemove()
		{
			var d = new TestEventData();

			var listener = new EventListener();
			Assert.AreEqual(0, listener.Count);

			listener.Add(d.Inc0);
			Assert.AreEqual(1, listener.Count);

			listener.Add(d.Inc0);
			Assert.AreEqual(1, listener.Count); // 不会出现重复添加

			listener.Add(d.Dec0);
			Assert.AreEqual(2, listener.Count);

			listener.Remove(d.Inc0);
			Assert.AreEqual(1, listener.Count);

			listener.Remove(d.Dec0);
			Assert.AreEqual(0, listener.Count);

			listener.Add(d.Inc0);
			listener.Add(d.Dec0);
			Assert.AreEqual(2, listener.Count);

			listener.RemoveTarget(d); // 移除对象目标
			Assert.AreEqual(0, listener.Count);

			listener.Add(d.Inc0);
			listener.Add(d.Dec0);
			Assert.AreEqual(2, listener.Count);
			listener.RemoveAll();
			Assert.AreEqual(0, listener.Count);
		}

		[Test]
		public void TestEvent0()
		{
			var d = new TestEventData();

			var listener = new EventListener();

			listener.Add(d.Inc0);

			listener.Invoke();
			Assert.AreEqual(1, d.iVal);
			listener.Invoke();
			Assert.AreEqual(2, d.iVal);
			listener.Invoke();
			Assert.AreEqual(3, d.iVal);

			listener.Add(d.Dec0);

			listener.InvokeWith(null);
			Assert.AreEqual(3, d.iVal);
			listener.Invoke();
			Assert.AreEqual(3, d.iVal);

			listener.Remove(d.Inc0);

			listener.InvokeWith(null);
			Assert.AreEqual(2, d.iVal);

			listener.RemoveAll();

			listener.Invoke();
			Assert.AreEqual(2, d.iVal);
		}

		[Test]
		public void TestEvent1()
		{
			var d = new TestEventData();

			var listener = new EventListener<int>();

			listener.Add(d.Inc1);

			listener.Invoke(1);
			Assert.AreEqual(1, d.iVal);
			listener.Invoke(2);
			Assert.AreEqual(3, d.iVal);
			listener.Invoke(3);
			Assert.AreEqual(6, d.iVal);

			listener.Add(d.Dec1);

			listener.InvokeWith(4);
			Assert.AreEqual(6, d.iVal);
			listener.Invoke(4);
			Assert.AreEqual(6, d.iVal);

			listener.Remove(d.Inc1);

			listener.InvokeWith(3);
			Assert.AreEqual(3, d.iVal);

			listener.RemoveAll();

			listener.Invoke(9);
			Assert.AreEqual(3, d.iVal);

			listener.Add(d.Dec1);

			Assert.Catch(() => listener.InvokeWith(null));
			Assert.Catch(() => listener.InvokeWith("hello"));
			Assert.Catch(() => listener.InvokeWith(Tuple.Create(1, 2)));
		}

		[Test]
		public void TestEvent2()
		{
			var d = new TestEventData();

			var listener = new EventListener<int, string>();

			listener.Add(d.Inc2);

			listener.Invoke(1, "hello ");
			Assert.AreEqual(1, d.iVal);
			Assert.AreEqual("hello ", d.sVal);

			listener.Invoke(2, "world ");
			Assert.AreEqual(3, d.iVal);
			Assert.AreEqual("hello world ", d.sVal);

			listener.Add(d.Dec2);

			listener.Invoke(4, "c");
			Assert.AreEqual(3, d.iVal);
			Assert.AreEqual("hello world cc", d.sVal);

			listener.Remove(d.Inc2);

			listener.Invoke(4, "d");
			Assert.AreEqual(-1, d.iVal);
			Assert.AreEqual("hello world ccd", d.sVal);

			Assert.Catch(() => listener.InvokeWith(null));
			Assert.Catch(() => listener.InvokeWith("hello"));
			Assert.Catch(() => listener.InvokeWith(Tuple.Create(1, 2)));
		}
	}
}
