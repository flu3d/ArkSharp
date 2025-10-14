using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace ArkSharp.Test.Misc
{
    [TestFixture]
    public class TestHashHelper
    {
        [Test]
        public void MD5_WithValidString_ShouldReturnCorrectHash()
        {
            string input = "hello world";
            string result = HashHelper.MD5(input);

            Assert.IsNotNull(result);
            Assert.AreEqual(32, result.Length);
            Assert.AreEqual("5eb63bbbe01eeed093cb22bb8f5acdc3", result);
        }

        [Test]
        public void MD5_WithNullString_ShouldReturnEmptyHash()
        {
            string result = HashHelper.MD5((string)null);
            string emptyResult = HashHelper.MD5("");

            Assert.IsNotNull(result);
            Assert.AreEqual(emptyResult, result);
        }

        [Test]
        public void MD5_WithEmptyString_ShouldReturnConsistentHash()
        {
            string result1 = HashHelper.MD5("");
            string result2 = HashHelper.MD5("");

            Assert.AreEqual(result1, result2);
            Assert.AreEqual("d41d8cd98f00b204e9800998ecf8427e", result1);
        }

        [Test]
        public void MD5_WithByteArray_ShouldReturnCorrectHash()
        {
            byte[] input = Encoding.UTF8.GetBytes("hello world");
            string result = HashHelper.MD5(input);

            Assert.AreEqual("5eb63bbbe01eeed093cb22bb8f5acdc3", result);
        }

        [Test]
        public void MD5_WithNullByteArray_ShouldReturnEmptyHash()
        {
            string result = HashHelper.MD5((byte[])null);
            string emptyResult = HashHelper.MD5(new byte[0]);

            Assert.AreEqual(emptyResult, result);
        }

        [Test]
        public void MD5_WithByteArrayAndRange_ShouldReturnCorrectHash()
        {
            byte[] input = Encoding.UTF8.GetBytes("hello world test");
            string result = HashHelper.MD5(input, 0, 11); // "hello world"

            Assert.AreEqual("5eb63bbbe01eeed093cb22bb8f5acdc3", result);
        }

        [Test]
        public void GetHash32_WithSameString_ShouldReturnSameHash()
        {
            string input = "test string";
            int hash1 = HashHelper.GetHash32(input);
            int hash2 = HashHelper.GetHash32(input);

            Assert.AreEqual(hash1, hash2);
        }

        [Test]
        public void GetHash32_WithDifferentStrings_ShouldReturnDifferentHashes()
        {
            int hash1 = HashHelper.GetHash32("string1");
            int hash2 = HashHelper.GetHash32("string2");

            Assert.AreNotEqual(hash1, hash2);
        }

        [Test]
        public void GetHash16_ShouldReturnLower16Bits()
        {
            string input = "test";
            int hash32 = HashHelper.GetHash32(input);
            ushort hash16 = HashHelper.GetHash16(input);

            Assert.AreEqual((ushort)(hash32 & 0xffff), hash16);
        }

        [Test]
        public void CRC32_WithValidString_ShouldReturnCorrectHash()
        {
            string input = "hello world";
            uint result = HashHelper.CRC32(input);

            Assert.AreNotEqual(0u, result);
        }

        [Test]
        public void CRC32_WithNullOrEmptyString_ShouldReturnZero()
        {
            Assert.AreEqual(0u, HashHelper.CRC32((string)null));
            Assert.AreEqual(0u, HashHelper.CRC32(""));
        }

        [Test]
        public void CRC32_WithByteArray_ShouldReturnCorrectHash()
        {
            byte[] input = Encoding.UTF8.GetBytes("hello world");
            uint result = HashHelper.CRC32(input);

            Assert.AreNotEqual(0u, result);
        }

        [Test]
        public void CRC32_WithNullOrEmptyByteArray_ShouldReturnZero()
        {
            Assert.AreEqual(0u, HashHelper.CRC32((byte[])null));
            Assert.AreEqual(0u, HashHelper.CRC32(new byte[0]));
        }

        [Test]
        public void CRC32_WithByteArrayAndRange_ShouldReturnCorrectHash()
        {
            byte[] input = Encoding.UTF8.GetBytes("hello world test");
            uint result = HashHelper.CRC32(input, 0, 11); // "hello world"
            uint expected = HashHelper.CRC32("hello world");

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void CRC32_WithStream_ShouldReturnCorrectHash()
        {
            byte[] data = Encoding.UTF8.GetBytes("hello world");
            using var stream = new MemoryStream(data);

            uint result = HashHelper.CRC32(stream);
            uint expected = HashHelper.CRC32("hello world");

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void CRC32_WithNullStream_ShouldReturnZero()
        {
            uint result = HashHelper.CRC32((Stream)null);
            Assert.AreEqual(0u, result);
        }

        [Test]
        public void CRC32_WithEmptyStream_ShouldReturnZero()
        {
            using var stream = new MemoryStream();
            uint result = HashHelper.CRC32(stream);
            Assert.AreEqual(0u, result);
        }

        [Test]
        public void CRC32_SameInputDifferentMethods_ShouldReturnSameHash()
        {
            string input = "test data";
            byte[] bytes = Encoding.UTF8.GetBytes(input);

            uint hashFromString = HashHelper.CRC32(input);
            uint hashFromBytes = HashHelper.CRC32(bytes);

            using var stream = new MemoryStream(bytes);
            uint hashFromStream = HashHelper.CRC32(stream);

            Assert.AreEqual(hashFromString, hashFromBytes);
            Assert.AreEqual(hashFromString, hashFromStream);
        }

        [Test]
        public void Hash_WithConsistentInput_ShouldReturnConsistentResults()
        {
            // 测试相同输入多次调用应该返回相同结果
            string input = "consistency test";

            string md5_1 = HashHelper.MD5(input);
            string md5_2 = HashHelper.MD5(input);

            int hash32_1 = HashHelper.GetHash32(input);
            int hash32_2 = HashHelper.GetHash32(input);

            uint crc32_1 = HashHelper.CRC32(input);
            uint crc32_2 = HashHelper.CRC32(input);

            Assert.AreEqual(md5_1, md5_2);
            Assert.AreEqual(hash32_1, hash32_2);
            Assert.AreEqual(crc32_1, crc32_2);
        }

        [Test]
        public void MD5_WithSpecialCharacters_ShouldHandleCorrectly()
        {
            string input = "测试中文字符!@#$%^&*()";
            string result = HashHelper.MD5(input);

            Assert.IsNotNull(result);
            Assert.AreEqual(32, result.Length);
        }

        [Test]
        public void CRC32_WithSpecialCharacters_ShouldHandleCorrectly()
        {
            string input = "测试中文字符!@#$%^&*()";
            uint result = HashHelper.CRC32(input);

            Assert.AreNotEqual(0u, result);
        }
    }
}
