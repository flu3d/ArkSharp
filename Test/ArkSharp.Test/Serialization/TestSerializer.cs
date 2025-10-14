using System;
using System.Text;
using NUnit.Framework;

namespace ArkSharp.Test.Serialization
{
	[TestFixture]
	public class TestSerializer
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

		/// <summary>
		/// 测试定长缓冲区
		/// </summary>
		[Test]
		public void SerializeToFixedBuffer()
		{
			var buffer = new byte[32];
			var bs = new Serializer(buffer);

			TestWriteRaw(ref bs);
			Assert.AreEqual(32, bs.GetResult().Length);

			Exception exception = null;
			try
			{
				// buffer长度不足溢出
				bs.WriteRaw(ulong.MinValue);
			}
			catch (Exception e)
			{
				exception = e;
			}

			Assert.IsTrue(bs.Capacity == 32);

			Assert.IsNotNull(exception);
			Assert.AreEqual(32, bs.GetResult().Length);
		}

		/// <summary>
		/// 测试变长缓冲区
		/// </summary>
		[Test]
		public void SerializeToExpandableBuffer()
		{
			var bs = new Serializer(32);

			TestWriteRaw(ref bs);
			Assert.AreEqual(32, bs.GetResult().Length);

			// buffer可以自动扩展长度
			bs.WriteRaw(ulong.MinValue);
			Assert.IsTrue(bs.Capacity > 32);

			Assert.AreEqual(40, bs.GetResult().Length);
		}

		// 写入非托管类型（数值和枚举）
		private void TestWriteRaw(ref Serializer bs)
		{
			Assert.AreEqual(0, bs.Position);

			bs.WriteRaw(true);
			Assert.AreEqual(1, bs.Position);

			bs.WriteRaw((byte)42);
			Assert.AreEqual(2, bs.Position);

			bs.WriteRaw((short)352);
			Assert.AreEqual(4, bs.Position);

			bs.WriteRaw(999);
			Assert.AreEqual(8, bs.Position);

			bs.WriteRaw(1.0f);
			Assert.AreEqual(12, bs.Position);

			bs.WriteRaw(3.1416);
			Assert.AreEqual(20, bs.Position);

			bs.WriteRaw(long.MaxValue);
			Assert.AreEqual(28, bs.Position);

			bs.WriteRaw(ColorType.Red);
			Assert.AreEqual(32, bs.Position);
		}

		/// <summary>
		/// 测试写入字符串
		/// </summary>
		[Test]
		public void TestWriteString()
		{
			var bs = new Serializer(32, SerializeOptions.VarLen);

			bs.Write("hello");
			Assert.AreEqual(6, bs.Position);
		}

		/// <summary>
		/// 测试写入字符串
		/// </summary>
		[Test]
		public void TestWriteString2()
		{
			var bs = new Serializer(32, SerializeOptions.FixedLen2);

			bs.Write("world");
			Assert.AreEqual(7, bs.Position);
		}

		/// <summary>
		/// 测试写入字符串
		/// </summary>
		[Test]
		public void TestWriteLongString()
		{
			var bs = new Serializer(32, SerializeOptions.VarLen);

			var s = _stringData02;
			var bytes = Encoding.UTF8.GetBytes(s);

			bs.Write(s);
			Assert.AreEqual(bytes.Length + 2, bs.Position);
		}

		/// <summary>
		/// 测试写入字符串
		/// </summary>
		[Test]
		public void TestWriteLongString2()
		{
			var bs = new Serializer(32, SerializeOptions.FixedLen2);

			var s = _stringData02;
			var bytes = Encoding.UTF8.GetBytes(s);

			bs.Write(s);
			Assert.AreEqual(bytes.Length + 2, bs.Position);
		}

		/// <summary>
		/// 测试写入字符流
		/// </summary>
		[Test]
		public void TestWriteBytes()
		{
			var buffer = new byte[16];
			var bs = new Serializer(buffer);

			var bytes = new byte[8];

			bs.WriteRaw(bytes, 0, 6);
			Assert.AreEqual(6, bs.Position);

			bs.WriteRaw(bytes.AsSpan().Slice(6));
			Assert.AreEqual(8, bs.Position);

			bs.WriteRaw(bytes, 1, 7);
			Assert.AreEqual(15, bs.Position);
		}

		[Test]
		public void TestWriteVarUInt()
		{
			var bs = new Serializer(64);

			bs.WriteVarUInt(0);
			Assert.AreEqual(1, bs.Position);

			bs.WriteVarUInt(1);
			Assert.AreEqual(2, bs.Position);

			bs.WriteVarUInt(127);
			Assert.AreEqual(2 + 1, bs.Position);

			bs.WriteVarUInt(128);
			Assert.AreEqual(2 + 1 + 2, bs.Position);

			bs.WriteVarUInt(uint.MaxValue);
			Assert.AreEqual(2 + 1 + 2 + 5, bs.Position);

			bs.WriteVarUInt(ulong.MaxValue);
			Assert.AreEqual(2 + 1 + 2 + 5 + 10, bs.Position);
		}

		[Test]
		public void TestWriteVarIntZg()
		{
			var bs = new Serializer(64);

			bs.WriteVarIntZg(0);
			Assert.AreEqual(1, bs.Position);

			bs.WriteVarIntZg(1);
			Assert.AreEqual(2, bs.Position);

			bs.WriteVarIntZg(-1);
			Assert.AreEqual(2 + 1, bs.Position);

			bs.WriteVarIntZg(127);
			Assert.AreEqual(2 + 1 + 2, bs.Position);

			bs.WriteVarIntZg(-127);
			Assert.AreEqual(2 + 1 + 2 + 2, bs.Position);

			bs.WriteVarIntZg(int.MaxValue);
			Assert.AreEqual(2 + 1 + 2 + 2 + 5, bs.Position);

			bs.WriteVarIntZg(int.MinValue);
			Assert.AreEqual(2 + 1 + 2 + 2 + 5 + 5, bs.Position);

			bs.WriteVarIntZg(long.MaxValue);
			Assert.AreEqual(2 + 1 + 2 + 2 + 5 + 5 + 10, bs.Position);

			bs.WriteVarIntZg(long.MinValue);
			Assert.AreEqual(2 + 1 + 2 + 2 + 5 + 5 + 10 + 10, bs.Position);
		}

		/// <summary>
		/// 测试大端序列化
		/// </summary>
		[Test]
		public void TestBigEndianSerialization()
		{
			var bs = new Serializer(32, SerializeOptions.BigEndian);

			// 测试 short (2字节)
			bs.WriteRaw((short)0x1234);
			var result = bs.GetResult();
			Assert.AreEqual(0x12, result[0]);
			Assert.AreEqual(0x34, result[1]);

			// 测试 int (4字节)
			bs.WriteRaw(0x12345678);
			result = bs.GetResult();
			Assert.AreEqual(0x12, result[2]);
			Assert.AreEqual(0x34, result[3]);
			Assert.AreEqual(0x56, result[4]);
			Assert.AreEqual(0x78, result[5]);

			// 测试 long (8字节)
			bs.WriteRaw(0x123456789ABCDEF0L);
			result = bs.GetResult();
			Assert.AreEqual(0x12, result[6]);
			Assert.AreEqual(0x34, result[7]);
			Assert.AreEqual(0x56, result[8]);
			Assert.AreEqual(0x78, result[9]);
			Assert.AreEqual(0x9A, result[10]);
			Assert.AreEqual(0xBC, result[11]);
			Assert.AreEqual(0xDE, result[12]);
			Assert.AreEqual(0xF0, result[13]);

			// 测试 float (4字节)
			bs.WriteRaw(3.14f);
			result = bs.GetResult();
			// float 3.14 的大端序字节表示为: 40 48 F5 C3
			Assert.AreEqual(0x40, result[14]);
			Assert.AreEqual(0x48, result[15]);
			Assert.AreEqual(0xF5, result[16]);
			Assert.AreEqual(0xC3, result[17]);
		}
	}
}
