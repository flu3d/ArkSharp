using NUnit.Framework;

namespace ArkSharp.Test.Reflection
{
	public class TestMemberInfoHelper
	{
		[Test]
		public void TestGetValueType()
		{
			var type = typeof(TestData);
			var field = type.GetField(nameof(TestData.Number));
			var property = type.GetProperty(nameof(TestData.Name));

			Assert.AreEqual(typeof(int), field.GetValueType());
			Assert.AreEqual(typeof(string), property.GetValueType());
		}

		private class TestData
		{
			public int Number = default;
			public string Name { get; set; }
		}
	}
}
