using NUnit.Framework;

namespace ArkSharp.Test.Misc
{
    [TestFixture]
    public class TestUnsafeHelper
    {
        [Test]
        public void SizeOf_WithInt_ShouldReturn4()
        {
            int size = UnsafeHelper.SizeOf<int>();
            Assert.AreEqual(4, size);
        }

        [Test]
        public void SizeOf_WithByte_ShouldReturn1()
        {
            int size = UnsafeHelper.SizeOf<byte>();
            Assert.AreEqual(1, size);
        }

        [Test]
        public void SizeOf_WithLong_ShouldReturn8()
        {
            int size = UnsafeHelper.SizeOf<long>();
            Assert.AreEqual(8, size);
        }

        [Test]
        public void SizeOf_WithFloat_ShouldReturn4()
        {
            int size = UnsafeHelper.SizeOf<float>();
            Assert.AreEqual(4, size);
        }

        [Test]
        public void SizeOf_WithDouble_ShouldReturn8()
        {
            int size = UnsafeHelper.SizeOf<double>();
            Assert.AreEqual(8, size);
        }

        [Test]
        public void SizeOf_WithBool_ShouldReturn1()
        {
            int size = UnsafeHelper.SizeOf<bool>();
            Assert.AreEqual(1, size);
        }

        [Test]
        public void SizeOf_WithChar_ShouldReturn2()
        {
            int size = UnsafeHelper.SizeOf<char>();
            Assert.AreEqual(2, size);
        }

        [Test]
        public void SizeOf_WithShort_ShouldReturn2()
        {
            int size = UnsafeHelper.SizeOf<short>();
            Assert.AreEqual(2, size);
        }

        [Test]
        public void SizeOf_WithUShort_ShouldReturn2()
        {
            int size = UnsafeHelper.SizeOf<ushort>();
            Assert.AreEqual(2, size);
        }

        [Test]
        public void SizeOf_WithUInt_ShouldReturn4()
        {
            int size = UnsafeHelper.SizeOf<uint>();
            Assert.AreEqual(4, size);
        }

        [Test]
        public void SizeOf_WithULong_ShouldReturn8()
        {
            int size = UnsafeHelper.SizeOf<ulong>();
            Assert.AreEqual(8, size);
        }

        [Test]
        public void SizeOf_WithSByte_ShouldReturn1()
        {
            int size = UnsafeHelper.SizeOf<sbyte>();
            Assert.AreEqual(1, size);
        }

        [Test]
        public void SizeOf_WithDecimal_ShouldReturn16()
        {
            int size = UnsafeHelper.SizeOf<decimal>();
            Assert.AreEqual(16, size);
        }

        // 测试自定义结构体
        public struct TestStruct
        {
            public int IntField;
            public byte ByteField;
        }

        [Test]
        public void SizeOf_WithCustomStruct_ShouldReturnCorrectSize()
        {
            int size = UnsafeHelper.SizeOf<TestStruct>();
            // 由于内存对齐，实际大小可能不是简单的4+1=5
            // 通常是8字节（4字节int + 1字节byte + 3字节填充）
            Assert.IsTrue(size >= 5);
            Assert.IsTrue(size <= 8);
        }

        public struct PackedStruct
        {
            public byte Byte1;
            public byte Byte2;
            public byte Byte3;
            public byte Byte4;
        }

        [Test]
        public void SizeOf_WithPackedStruct_ShouldReturnCorrectSize()
        {
            int size = UnsafeHelper.SizeOf<PackedStruct>();
            Assert.AreEqual(4, size);
        }

        public struct LargeStruct
        {
            public long Long1;
            public long Long2;
            public int Int1;
            public float Float1;
        }

        [Test]
        public void SizeOf_WithLargeStruct_ShouldReturnCorrectSize()
        {
            int size = UnsafeHelper.SizeOf<LargeStruct>();
            // 2个long(8*2) + 1个int(4) + 1个float(4) = 24字节
            Assert.AreEqual(24, size);
        }

        public enum TestEnum : byte
        {
            Value1 = 1,
            Value2 = 2
        }

        [Test]
        public void SizeOf_WithByteEnum_ShouldReturn1()
        {
            int size = UnsafeHelper.SizeOf<TestEnum>();
            Assert.AreEqual(1, size);
        }

        public enum TestIntEnum : int
        {
            Value1 = 1,
            Value2 = 2
        }

        [Test]
        public void SizeOf_WithIntEnum_ShouldReturn4()
        {
            int size = UnsafeHelper.SizeOf<TestIntEnum>();
            Assert.AreEqual(4, size);
        }

        [Test]
        public void SizeOf_ConsistentResults_ShouldReturnSameValue()
        {
            // 多次调用应该返回相同的结果
            int size1 = UnsafeHelper.SizeOf<int>();
            int size2 = UnsafeHelper.SizeOf<int>();
            int size3 = UnsafeHelper.SizeOf<int>();

            Assert.AreEqual(size1, size2);
            Assert.AreEqual(size2, size3);
        }

        [Test]
        public void SizeOf_AllPrimitiveTypes_ShouldReturnPositiveValues()
        {
            // 确保所有基本类型都返回正数
            Assert.IsTrue(UnsafeHelper.SizeOf<byte>() > 0);
            Assert.IsTrue(UnsafeHelper.SizeOf<sbyte>() > 0);
            Assert.IsTrue(UnsafeHelper.SizeOf<short>() > 0);
            Assert.IsTrue(UnsafeHelper.SizeOf<ushort>() > 0);
            Assert.IsTrue(UnsafeHelper.SizeOf<int>() > 0);
            Assert.IsTrue(UnsafeHelper.SizeOf<uint>() > 0);
            Assert.IsTrue(UnsafeHelper.SizeOf<long>() > 0);
            Assert.IsTrue(UnsafeHelper.SizeOf<ulong>() > 0);
            Assert.IsTrue(UnsafeHelper.SizeOf<float>() > 0);
            Assert.IsTrue(UnsafeHelper.SizeOf<double>() > 0);
            Assert.IsTrue(UnsafeHelper.SizeOf<decimal>() > 0);
            Assert.IsTrue(UnsafeHelper.SizeOf<bool>() > 0);
            Assert.IsTrue(UnsafeHelper.SizeOf<char>() > 0);
        }
    }
}
