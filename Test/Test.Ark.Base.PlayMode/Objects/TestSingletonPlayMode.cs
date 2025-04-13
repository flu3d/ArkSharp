#if UNITY_EDITOR

using NUnit.Framework;
using System.Collections;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace Ark.Test
{
	[TestFixture]
	public class TestSingletonPlayMode
	{
		[SetUp]
		public void Init()
		{
			Singleton.autoSetDontDestroyOnLoad = true;
			Singleton.Clear();
		}

#if UNITY_5_3_OR_NEWER
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
#endif
	}
}

#endif
