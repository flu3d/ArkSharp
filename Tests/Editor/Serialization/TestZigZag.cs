using NUnit.Framework;

namespace ArkSharp.Test
{
	[TestFixture]
	public class TestZigZag
	{
		[Test]
		public void Test1()
		{
			Assert.AreEqual(0, ZigZag.Encode(0));
			Assert.AreEqual(2, ZigZag.Encode(1));
			Assert.AreEqual(1, ZigZag.Encode(-1));
			Assert.AreEqual(254, ZigZag.Encode(127));
			Assert.AreEqual(253, ZigZag.Encode(-127));
			Assert.AreEqual(256, ZigZag.Encode(128));
			Assert.AreEqual(255, ZigZag.Encode(-128));

			Assert.AreEqual(4294967294uL, ZigZag.Encode(int.MaxValue));
			Assert.AreEqual(4294967295uL, ZigZag.Encode(int.MinValue));
			Assert.AreEqual(18446744073709551614uL, ZigZag.Encode(long.MaxValue));
			Assert.AreEqual(18446744073709551615uL, ZigZag.Encode(long.MinValue));
		}

		[Test]
		public void Test2()
		{
			Assert.AreEqual(0, ZigZag.Decode(ZigZag.Encode(0)));
			Assert.AreEqual(1, ZigZag.Decode(ZigZag.Encode(1)));
			Assert.AreEqual(-1, ZigZag.Decode(ZigZag.Encode(-1)));
			Assert.AreEqual(127, ZigZag.Decode(ZigZag.Encode(127)));
			Assert.AreEqual(-127, ZigZag.Decode(ZigZag.Encode(-127)));
			Assert.AreEqual(128, ZigZag.Decode(ZigZag.Encode(128)));
			Assert.AreEqual(-128, ZigZag.Decode(ZigZag.Encode(-128)));

			Assert.AreEqual(int.MaxValue, ZigZag.Decode(ZigZag.Encode(int.MaxValue)));
			Assert.AreEqual(int.MinValue, ZigZag.Decode(ZigZag.Encode(int.MinValue)));
			Assert.AreEqual(long.MaxValue, ZigZag.Decode(ZigZag.Encode(long.MaxValue)));
			Assert.AreEqual(long.MinValue, ZigZag.Decode(ZigZag.Encode(long.MinValue)));
		}
	}
}
