using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ArkSharp.Test.Misc
{
	public class TestActionHelper
	{
		[Test]
		public void Take_ShouldClearActionBeforeInvocation()
		{
			Action callback = null;
			bool secondCallbackCalled = false;

			callback = () => callback = () => secondCallbackCalled = true;

			ActionHelper.Take(ref callback)?.Invoke();
			Assert.IsNotNull(callback);

			ActionHelper.Take(ref callback)?.Invoke();
			Assert.IsTrue(secondCallbackCalled);
			Assert.IsNull(callback);
		}

		[Test]
		public void Take_GenericAction_ShouldReturnCallbackAndClearSource()
		{
			int receivedValue = 0;
			Action<int> callback = value => receivedValue = value;

			ActionHelper.Take(ref callback)?.Invoke(42);

			Assert.AreEqual(42, receivedValue);
			Assert.IsNull(callback);
		}

		[Test]
		public void Take_TwoParameterAction_ShouldReturnCallbackAndClearSource()
		{
			int twoParameterResult = 0;
			Action<int, int> twoParameterCallback = (value1, value2) => twoParameterResult = value1 + value2;

			ActionHelper.Take(ref twoParameterCallback)?.Invoke(40, 2);

			Assert.AreEqual(42, twoParameterResult);
			Assert.IsNull(twoParameterCallback);
		}

		[Test]
		public void Take_ShouldAllowOnlyOneConcurrentConsumer()
		{
			int invokeCount = 0;
			Action callback = () => Interlocked.Increment(ref invokeCount);

			Parallel.For(0, 32, _ => ActionHelper.Take(ref callback)?.Invoke());

			Assert.AreEqual(1, invokeCount);
			Assert.IsNull(callback);
		}
	}
}
