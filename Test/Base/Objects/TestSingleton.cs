using NUnit.Framework;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace ArkSharp.Test.Objects
{
	[TestFixture]
	public class TestSingleton
	{
		[SetUp]
		public void Init()
		{
			Singleton.autoSetDontDestroyOnLoad = true;
			Singleton.Clear();
		}

		[Test]
		public void Test1()
		{
			var d1 = Singleton.Get<TestClass1>();
			var d2 = Singleton.Get(typeof(TestClass1));

			Assert.NotNull(d1);
			Assert.AreSame(d1, d2);

			var d3 = new TestClass1();

			Assert.NotNull(d3);
			Assert.AreNotSame(d1, d3);
		}

		public class TestClass1
		{
		}

#if UNITY_5_3_OR_NEWER
		[Test]
		public void Test2()
		{
			Singleton.autoSetDontDestroyOnLoad = true;

			var d1 = Singleton.Get<TestClass2>();
			var d2 = Singleton.Get(typeof(TestClass2));

			Assert.NotNull(d1);
			Assert.AreSame(d1, d2);

			if (Application.isPlaying)
				Assert.AreEqual("DontDestroyOnLoad", d1.gameObject.scene.name);
			else
				Assert.AreNotEqual("DontDestroyOnLoad", d1.gameObject.scene.name);

			Singleton.autoSetDontDestroyOnLoad = false;
			Singleton.Clear();

			var d3 = Singleton.Get<TestClass2>();

			Assert.NotNull(d3);
			Assert.AreNotSame(d1, d3);
			Assert.AreNotEqual("DontDestroyOnLoad", d3.gameObject.scene.name);
		}

		public class TestClass2 : MonoBehaviour
		{
		}
#endif
	}
}
