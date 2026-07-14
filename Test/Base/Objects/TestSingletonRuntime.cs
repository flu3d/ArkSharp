#if UNITY_5_3_OR_NEWER

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ArkSharp.Test.Objects
{
	[RequiresPlayMode]
	[TestFixture]
	public class TestSingletonRuntime
	{
		[SetUp]
		public void Init()
		{
			Singleton.autoSetDontDestroyOnLoad = true;
			Singleton.Clear();
		}

		[Test]
		public void Test3()
		{
			Singleton.autoSetDontDestroyOnLoad = true;

			var d1 = Singleton.Get<TestClass2>();
			var d2 = Singleton.Get(typeof(TestClass2));

			Assert.NotNull(d1);
			Assert.AreSame(d1, d2);

			Assert.IsTrue(Application.isPlaying);
			Assert.AreEqual("DontDestroyOnLoad", d1.gameObject.scene.name);

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
	}
}
#endif
