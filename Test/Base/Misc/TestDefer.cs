using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ArkSharp.Test.Misc
{
	public class TestDefer
	{
		[Test]
		public void OnLeave_ShouldInvokeWhenDisposed()
		{
			bool invoked = false;

			using (Defer.OnLeave(() => invoked = true))
			{
				Assert.IsFalse(invoked);
			}

			Assert.IsTrue(invoked);
		}

		[Test]
		public void OnLeave_WithState_ShouldInvokeCallbackWithState()
		{
			string receivedValue = null;

			using (Defer.OnLeave(value => receivedValue = value, "ArkSharp"))
			{
			}

			Assert.AreEqual("ArkSharp", receivedValue);
		}

		[Test]
		public void Dispose_ShouldInvokeOnlyOnceWhenCallbackThrows()
		{
			int invokeCount = 0;
			var defer = Defer.OnLeave(() =>
			{
				invokeCount++;
				throw new InvalidOperationException();
			});

			Assert.Throws<InvalidOperationException>(() => defer.Dispose());
			Assert.DoesNotThrow(() => defer.Dispose());
			Assert.AreEqual(1, invokeCount);
		}

		[Test]
		public void OnLeave_ShouldInvokeForEachStructCopy()
		{
			int invokeCount = 0;
			var defer = Defer.OnLeave(() => invokeCount++);
			var copy = defer;

			defer.Dispose();
			copy.Dispose();

			Assert.AreEqual(2, invokeCount);
		}

		[Test]
		public void Dispose_WithState_ShouldInvokeOnlyOnceWhenCallbackThrows()
		{
			int invokeCount = 0;
			var defer = Defer.OnLeave<int>(value =>
			{
				invokeCount += value;
				throw new InvalidOperationException();
			}, 1);

			Assert.Throws<InvalidOperationException>(() => defer.Dispose());
			Assert.DoesNotThrow(() => defer.Dispose());
			Assert.AreEqual(1, invokeCount);
		}

		[Test]
		public void OnLeave_ShouldDisposeInReverseDeclarationOrder()
		{
			var executionOrder = new List<int>();

			using (Defer.OnLeave(() => executionOrder.Add(1)))
			using (Defer.OnLeave(() => executionOrder.Add(2)))
			using (Defer.OnLeave(() => executionOrder.Add(3)))
			{
			}

			CollectionAssert.AreEqual(new[] { 3, 2, 1 }, executionOrder);
		}

		[Test]
		public void OnLeave_NullCallback_ShouldNotThrow()
		{
			Assert.DoesNotThrow(() => Defer.OnLeave(null).Dispose());
			Assert.DoesNotThrow(() => Defer.OnLeave<string>(null, "state").Dispose());
		}
	}
}
