using NUnit.Framework;

namespace ArkSharp.Test.Serialization
{
    [TestFixture]
    public class TestDeserializer
    {
        private const string _stringData02 = @"Hello, Ark
你好, 方舟
こんにちは、アーク
안녕, 아크
¡Hola, Ark
Bonjour, Ark
Привет, Арк
Hallo, Ark
Olá, Ark
Ciao, Ark
Chào Ark
Hei, Ark
Hej, Ark";

        public enum ColorType
        {
            None = 0,
            White = 1,
            Green = 2,
            Blue = 3,
            Purple = 4,
            Red = 5,
            Gold = 6,
        }

        [Test]
        public void TestRaw()
        {
            var bs = new Serializer(128);

            bs.WriteRaw(true);
            bs.WriteRaw((byte)42);
            bs.WriteRaw((short)352);
            bs.WriteRaw(999);
            bs.WriteRaw(1.0f);
            bs.WriteRaw(3.1416);
            bs.WriteRaw(long.MaxValue);
            bs.WriteRaw(ColorType.Red);

            bs.WriteVarIntZg(0);
            bs.WriteVarIntZg(1);
            bs.WriteVarIntZg(-1);
            bs.WriteVarIntZg(127);
            bs.WriteVarIntZg(-127);
            bs.WriteVarIntZg(int.MinValue);
            bs.WriteVarIntZg(long.MaxValue);

            var buffer = bs.GetResult().ToArray();
            var ds = new Deserializer(buffer);

            Assert.AreEqual(true, ds.ReadRaw<bool>());
            Assert.AreEqual(42, ds.ReadRaw<byte>());
            Assert.AreEqual(352, ds.ReadRaw<short>());
            Assert.AreEqual(999, ds.ReadRaw<int>());
            Assert.AreEqual(1.0f, ds.ReadRaw<float>(), 0.00001f);
            Assert.AreEqual(3.1416, ds.ReadRaw<double>(), 0.00001);
            Assert.AreEqual(long.MaxValue, ds.ReadRaw<long>());
            Assert.AreEqual(ColorType.Red, ds.ReadRaw<ColorType>());

            Assert.AreEqual(0, ds.ReadVarIntZg());
            Assert.AreEqual(1, ds.ReadVarIntZg());
            Assert.AreEqual(-1, ds.ReadVarIntZg());
            Assert.AreEqual(127, ds.ReadVarIntZg());
            Assert.AreEqual(-127, ds.ReadVarIntZg());
            Assert.AreEqual(int.MinValue, ds.ReadVarIntZg());
            Assert.AreEqual(long.MaxValue, ds.ReadVarIntZg());

            Assert.AreEqual(ds.Length, ds.Position);
        }

        [Test]
        public void Test0()
        {
            // 变长容器 + 变长整数
            TestRWImpl(SerializeOptions.VarInt | SerializeOptions.VarLen);
        }

        [Test]
        public void Test1()
        {
            // 变长容器 + 定长整数
            TestRWImpl(SerializeOptions.VarLen);
        }

        [Test]
        public void Test2()
        {
            // 定长容器 + 变长整数
            TestRWImpl(SerializeOptions.VarInt | SerializeOptions.FixedLen2);
        }

        void TestRWImpl(SerializeOptions options)
        {
            var bs = new Serializer(128, options);

            bs.Write(true);
            bs.Write((short)352);
            bs.Write(-42);
            bs.Write((uint)999999);
            bs.Write(long.MaxValue);
            bs.Write(ColorType.Red);

            bs.Write("hello");
            bs.Write(_stringData02);

            bs.Write(1.0f);
            bs.Write(3.1416);

            var buffer = bs.GetResult().ToArray();
            var ds = new Deserializer(buffer, options);

            ds.Read(out bool bVal);
            Assert.AreEqual(true, bVal);
            ds.Read(out short sVal);
            Assert.AreEqual(352, sVal);
            ds.Read(out int iVal);
            Assert.AreEqual(-42, iVal);
            ds.Read(out uint uVal);
            Assert.AreEqual(999999, uVal);
            ds.Read(out long lVal);
            Assert.AreEqual(long.MaxValue, lVal);
            ds.Read(out ColorType eVal);
            Assert.AreEqual(ColorType.Red, eVal);

            Assert.AreEqual("hello", ds.ReadString());
            Assert.AreEqual(_stringData02, ds.ReadString());

            ds.Read(out float fVal); Assert.AreEqual(1.0f, fVal, 0.00001f);
            ds.Read(out double dVal); Assert.AreEqual(3.1416, dVal, 0.00001);

            Assert.AreEqual(ds.Length, ds.Position);
        }
    }
}
