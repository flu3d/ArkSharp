using NUnit.Framework;

namespace ArkSharp.Test.Misc
{
    [TestFixture]
    public class TestBitHelper
    {
        [Test]
        public void SetMask_Int_EnabledTrue_ShouldSetBits()
        {
            int value = 0b1010;
            int bitmask = 0b0101;
            int result = BitHelper.SetMask(value, bitmask, true);
            Assert.AreEqual(0b1111, result);
        }

        [Test]
        public void SetMask_Int_EnabledFalse_ShouldClearBits()
        {
            int value = 0b1111;
            int bitmask = 0b0101;
            int result = BitHelper.SetMask(value, bitmask, false);
            Assert.AreEqual(0b1010, result);
        }

        [Test]
        public void SetMask_UInt_EnabledTrue_ShouldSetBits()
        {
            uint value = 0b1010;
            uint bitmask = 0b0101;
            uint result = BitHelper.SetMask(value, bitmask, true);
            Assert.AreEqual(0b1111u, result);
        }

        [Test]
        public void SetMask_UInt_EnabledFalse_ShouldClearBits()
        {
            uint value = 0b1111;
            uint bitmask = 0b0101;
            uint result = BitHelper.SetMask(value, bitmask, false);
            Assert.AreEqual(0b1010u, result);
        }

        [Test]
        public void SetMask_Long_EnabledTrue_ShouldSetBits()
        {
            long value = 0b1010;
            long bitmask = 0b0101;
            long result = BitHelper.SetMask(value, bitmask, true);
            Assert.AreEqual(0b1111L, result);
        }

        [Test]
        public void SetMask_Long_EnabledFalse_ShouldClearBits()
        {
            long value = 0b1111;
            long bitmask = 0b0101;
            long result = BitHelper.SetMask(value, bitmask, false);
            Assert.AreEqual(0b1010L, result);
        }

        [Test]
        public void SetMask_ULong_EnabledTrue_ShouldSetBits()
        {
            ulong value = 0b1010;
            ulong bitmask = 0b0101;
            ulong result = BitHelper.SetMask(value, bitmask, true);
            Assert.AreEqual(0b1111UL, result);
        }

        [Test]
        public void SetMask_ULong_EnabledFalse_ShouldClearBits()
        {
            ulong value = 0b1111;
            ulong bitmask = 0b0101;
            ulong result = BitHelper.SetMask(value, bitmask, false);
            Assert.AreEqual(0b1010UL, result);
        }

        [Test]
        public void TestMask_Int_AllBitsSet_ReturnsTrue()
        {
            int value = 0b1111;
            int bitmask = 0b0101;
            bool result = BitHelper.TestMask(value, bitmask);
            Assert.IsTrue(result);
        }

        [Test]
        public void TestMask_Int_NotAllBitsSet_ReturnsFalse()
        {
            int value = 0b1010;
            int bitmask = 0b0101;
            bool result = BitHelper.TestMask(value, bitmask);
            Assert.IsFalse(result);
        }

        [Test]
        public void TestMask_UInt_AllBitsSet_ReturnsTrue()
        {
            uint value = 0b1111;
            uint bitmask = 0b0101;
            bool result = BitHelper.TestMask(value, bitmask);
            Assert.IsTrue(result);
        }

        [Test]
        public void TestMask_UInt_NotAllBitsSet_ReturnsFalse()
        {
            uint value = 0b1010;
            uint bitmask = 0b0101;
            bool result = BitHelper.TestMask(value, bitmask);
            Assert.IsFalse(result);
        }

        [Test]
        public void TestMask_Long_AllBitsSet_ReturnsTrue()
        {
            long value = 0b1111;
            long bitmask = 0b0101;
            bool result = BitHelper.TestMask(value, bitmask);
            Assert.IsTrue(result);
        }

        [Test]
        public void TestMask_Long_NotAllBitsSet_ReturnsFalse()
        {
            long value = 0b1010;
            long bitmask = 0b0101;
            bool result = BitHelper.TestMask(value, bitmask);
            Assert.IsFalse(result);
        }

        [Test]
        public void TestMask_ULong_AllBitsSet_ReturnsTrue()
        {
            ulong value = 0b1111;
            ulong bitmask = 0b0101;
            bool result = BitHelper.TestMask(value, bitmask);
            Assert.IsTrue(result);
        }

        [Test]
        public void TestMask_ULong_NotAllBitsSet_ReturnsFalse()
        {
            ulong value = 0b1010;
            ulong bitmask = 0b0101;
            bool result = BitHelper.TestMask(value, bitmask);
            Assert.IsFalse(result);
        }

        [Test]
        public void SetAt_Int_EnabledTrue_ShouldSetBit()
        {
            int value = 0b1010;
            int result = BitHelper.SetAt(value, 0, true);
            Assert.AreEqual(0b1011, result);
        }

        [Test]
        public void SetAt_Int_EnabledFalse_ShouldClearBit()
        {
            int value = 0b1011;
            int result = BitHelper.SetAt(value, 0, false);
            Assert.AreEqual(0b1010, result);
        }

        [Test]
        public void SetAt_UInt_EnabledTrue_ShouldSetBit()
        {
            uint value = 0b1010;
            uint result = BitHelper.SetAt(value, 0, true);
            Assert.AreEqual(0b1011u, result);
        }

        [Test]
        public void SetAt_UInt_EnabledFalse_ShouldClearBit()
        {
            uint value = 0b1011;
            uint result = BitHelper.SetAt(value, 0, false);
            Assert.AreEqual(0b1010u, result);
        }

        [Test]
        public void SetAt_Long_EnabledTrue_ShouldSetBit()
        {
            long value = 0b1010;
            long result = BitHelper.SetAt(value, 0, true);
            Assert.AreEqual(0b1011L, result);
        }

        [Test]
        public void SetAt_Long_EnabledFalse_ShouldClearBit()
        {
            long value = 0b1011;
            long result = BitHelper.SetAt(value, 0, false);
            Assert.AreEqual(0b1010L, result);
        }

        [Test]
        public void SetAt_ULong_EnabledTrue_ShouldSetBit()
        {
            ulong value = 0b1010;
            ulong result = BitHelper.SetAt(value, 0, true);
            Assert.AreEqual(0b1011UL, result);
        }

        [Test]
        public void SetAt_ULong_EnabledFalse_ShouldClearBit()
        {
            ulong value = 0b1011;
            ulong result = BitHelper.SetAt(value, 0, false);
            Assert.AreEqual(0b1010UL, result);
        }

        [Test]
        public void TestAt_Int_BitSet_ReturnsTrue()
        {
            int value = 0b1011;
            bool result = BitHelper.TestAt(value, 0);
            Assert.IsTrue(result);
        }

        [Test]
        public void TestAt_Int_BitNotSet_ReturnsFalse()
        {
            int value = 0b1010;
            bool result = BitHelper.TestAt(value, 0);
            Assert.IsFalse(result);
        }

        [Test]
        public void TestAt_UInt_BitSet_ReturnsTrue()
        {
            uint value = 0b1011;
            bool result = BitHelper.TestAt(value, 0);
            Assert.IsTrue(result);
        }

        [Test]
        public void TestAt_UInt_BitNotSet_ReturnsFalse()
        {
            uint value = 0b1010;
            bool result = BitHelper.TestAt(value, 0);
            Assert.IsFalse(result);
        }

        [Test]
        public void TestAt_Long_BitSet_ReturnsTrue()
        {
            long value = 0b1011;
            bool result = BitHelper.TestAt(value, 0);
            Assert.IsTrue(result);
        }

        [Test]
        public void TestAt_Long_BitNotSet_ReturnsFalse()
        {
            long value = 0b1010;
            bool result = BitHelper.TestAt(value, 0);
            Assert.IsFalse(result);
        }

        [Test]
        public void TestAt_ULong_BitSet_ReturnsTrue()
        {
            ulong value = 0b1011;
            bool result = BitHelper.TestAt(value, 0);
            Assert.IsTrue(result);
        }

        [Test]
        public void TestAt_ULong_BitNotSet_ReturnsFalse()
        {
            ulong value = 0b1010;
            bool result = BitHelper.TestAt(value, 0);
            Assert.IsFalse(result);
        }

        [Test]
        public void TestAt_HighBitIndex_WorksCorrectly()
        {
            int value = 1 << 31;
            bool result = BitHelper.TestAt(value, 31);
            Assert.IsTrue(result);
        }

        [Test]
        public void SetAt_HighBitIndex_WorksCorrectly()
        {
            int value = 0;
            int result = BitHelper.SetAt(value, 31, true);
            Assert.AreEqual(1 << 31, result);
        }
    }
}
