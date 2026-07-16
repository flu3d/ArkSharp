using System;
using NUnit.Framework;

namespace ArkSharp.Test.Reflection
{
	public class TestDelegateHelper
	{
		[Test]
		public void TestGetInvokeMethod()
		{
			var actionMethod = typeof(Action<int>).GetInvokeMethod();
			var customMethod = typeof(TestDelegate).GetInvokeMethod();

			Assert.AreEqual("Invoke", actionMethod.Name);
			Assert.AreEqual(typeof(void), actionMethod.ReturnType);
			Assert.AreEqual(typeof(int), actionMethod.GetParameters()[0].ParameterType);

			Assert.AreEqual("Invoke", customMethod.Name);
			Assert.AreEqual(typeof(bool), customMethod.ReturnType);
			Assert.AreEqual(typeof(string), customMethod.GetParameters()[0].ParameterType);
		}

		[Test]
		public void TestGetInvokeMethodInvalidType()
		{
			Assert.IsNull(DelegateHelper.GetInvokeMethod(null));
			Assert.IsNull(typeof(string).GetInvokeMethod());
		}

		private delegate bool TestDelegate(string value);
	}
}
