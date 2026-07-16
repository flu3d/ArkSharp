using NUnit.Framework;

namespace ArkSharp.Test.Reflection
{
	public class TestAssemblyHelper
	{
		[Test]
		public void TestFindUserTypeByFullName()
		{
			var type = typeof(TestAssemblyHelper);

			Assert.AreEqual(type, AssemblyHelper.FindUserType(type.FullName));
		}

		[Test]
		public void TestFindUserTypeByQualifiedNameSuffix()
		{
			Assert.AreEqual(typeof(TestAssemblyHelper), AssemblyHelper.FindUserType("Test.Reflection.TestAssemblyHelper"));
		}

		[Test]
		public void TestFindUserTypeDoesNotMatchPartialNamespaceSegment()
		{
			Assert.IsNull(AssemblyHelper.FindUserType("Framework.TestAssemblyHelperFake"));
		}

		[Test]
		public void TestFindUserTypeIgnoreCase()
		{
			Assert.AreEqual(typeof(TestAssemblyHelper), AssemblyHelper.FindUserType("test.reflection.testassemblyhelper", true));
		}

	}
}

namespace ArkSharp.Test.MyFramework
{
	public class TestAssemblyHelperFake
	{
	}
}
