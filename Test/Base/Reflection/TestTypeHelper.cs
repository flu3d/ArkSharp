using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace ArkSharp.Test.Reflection
{
	public class TestTypeHelper
	{
		private const BindingFlags MEMBER_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		[Test]
		public void TestGetGenericArgAtInvalidInput()
		{
			var type = typeof(Dictionary<int, string>);

			Assert.AreEqual(typeof(int), type.GetGenericArgAt(0));
			Assert.AreEqual(typeof(string), type.GetGenericArgAt(1));
			Assert.Throws<NullReferenceException>(() => TypeHelper.GetGenericArgAt(null, 0));
			Assert.IsNull(typeof(string).GetGenericArgAt(0));
			Assert.IsNull(type.GetGenericArgAt(-1));
			Assert.IsNull(type.GetGenericArgAt(2));
		}

		[Test]
		public void TestEnumerateTypedMembersWithInheritance()
		{
			var type = typeof(DerivedMemberType);
			var fields = type.EnumerateFields(MEMBER_FLAGS, true).ToList();
			var properties = type.EnumerateProperties(MEMBER_FLAGS, true).ToList();
			var methods = type.EnumerateMethods(MEMBER_FLAGS, true).ToList();

			Assert.AreEqual(typeof(DerivedMemberType), fields.Single(x => x.Name == nameof(DerivedMemberType.DerivedField)).DeclaringType);
			Assert.AreEqual(typeof(BaseMemberType), fields.Single(x => x.Name == nameof(BaseMemberType.BaseField)).DeclaringType);
			Assert.Less(fields.FindIndex(x => x.Name == nameof(DerivedMemberType.DerivedField)), fields.FindIndex(x => x.Name == nameof(BaseMemberType.BaseField)));
			Assert.AreEqual(typeof(DerivedMemberType), properties.Single(x => x.Name == nameof(DerivedMemberType.DerivedProperty)).DeclaringType);
			Assert.AreEqual(typeof(BaseMemberType), properties.Single(x => x.Name == nameof(BaseMemberType.BaseProperty)).DeclaringType);
			Assert.AreEqual(typeof(DerivedMemberType), methods.Single(x => x.Name == nameof(DerivedMemberType.DerivedMethod)).DeclaringType);
			Assert.AreEqual(typeof(BaseMemberType), methods.Single(x => x.Name == nameof(BaseMemberType.BaseMethod)).DeclaringType);
		}

		[Test]
		public void TestGetPropertyOrFieldPrefersDerivedProperty()
		{
			var member = typeof(DerivedMemberType).GetPropertyOrField(nameof(DerivedMemberType.Value), MEMBER_FLAGS, true);

			Assert.IsInstanceOf<PropertyInfo>(member);
			Assert.AreEqual(typeof(DerivedMemberType), member.DeclaringType);
		}

		[Test]
		public void TestEnumerateMembersWithPredicate()
		{
			var members = typeof(DerivedMemberType)
				.EnumerateMembers(MemberTypes.Field, MEMBER_FLAGS, true, x => x.Name.StartsWith("Derived", StringComparison.Ordinal))
				.ToList();

			Assert.AreEqual(1, members.Count);
			Assert.AreEqual(nameof(DerivedMemberType.DerivedField), members[0].Name);
		}

		private class BaseMemberType
		{
			public int BaseField = default;
			public int Value = default;
			public string BaseProperty { get; set; }

			public void BaseMethod()
			{
			}
		}

		private class DerivedMemberType : BaseMemberType
		{
			public int DerivedField = default;
			public new string Value { get; set; }
			public string DerivedProperty { get; set; }

			public void DerivedMethod()
			{
			}
		}
	}
}
