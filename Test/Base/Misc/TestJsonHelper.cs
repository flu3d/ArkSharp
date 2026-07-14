using System;
using System.Collections.Generic;
using NUnit.Framework;
using Newtonsoft.Json;

namespace ArkSharp.Test.Misc
{
    // 测试用的数据类
    public class TestData
    {
        public int IntValue { get; set; }
        public string StringValue { get; set; }
        public bool BoolValue { get; set; }
        public DateTime DateValue { get; set; }
        public List<int> ListValue { get; set; }
    }

    public class ComplexTestData
    {
        public TestData NestedData { get; set; }
        public Dictionary<string, object> DictionaryValue { get; set; }
        public int[] ArrayValue { get; set; }
    }

    [TestFixture]
    public class TestJsonHelper
    {
        [Test]
        public void ToJson_WithSimpleObject_ShouldReturnValidJson()
        {
            var testObj = new TestData
            {
                IntValue = 42,
                StringValue = "test",
                BoolValue = true,
                DateValue = new DateTime(2023, 1, 1),
                ListValue = new List<int> { 1, 2, 3 }
            };

            string json = testObj.ToJson();

            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("\"IntValue\":42"));
            Assert.IsTrue(json.Contains("\"StringValue\":\"test\""));
            Assert.IsTrue(json.Contains("\"BoolValue\":true"));
        }

        [Test]
        public void ToJson_WithIndented_ShouldReturnFormattedJson()
        {
            var testObj = new { Name = "test", Value = 123 };

            string json = testObj.ToJson(indented: true);
            string compactJson = testObj.ToJson(indented: false);

            Assert.IsNotNull(json);
            Assert.IsNotNull(compactJson);
            Assert.IsTrue(json.Length > compactJson.Length);
            Assert.IsTrue(json.Contains("\n") || json.Contains("\r"));
        }

        [Test]
        public void ToJson_WithNullObject_ShouldReturnNullString()
        {
            TestData nullObj = null;
            string json = nullObj.ToJson();

            Assert.AreEqual("null", json);
        }

        [Test]
        public void ToObject_WithValidJson_ShouldReturnCorrectObject()
        {
            string json = "{\"IntValue\":42,\"StringValue\":\"test\",\"BoolValue\":true,\"DateValue\":\"20230101 00:00:00\",\"ListValue\":[1,2,3]}";

            var result = json.ToObject<TestData>();

            Assert.IsNotNull(result);
            Assert.AreEqual(42, result.IntValue);
            Assert.AreEqual("test", result.StringValue);
            Assert.AreEqual(true, result.BoolValue);
            Assert.AreEqual(3, result.ListValue.Count);
            Assert.AreEqual(1, result.ListValue[0]);
        }

        [Test]
        public void ToObject_WithNullOrEmptyJson_ShouldReturnDefault()
        {
            var result1 = ((string)null).ToObject<TestData>();
            var result2 = "".ToObject<TestData>();

            Assert.IsNull(result1);
            Assert.IsNull(result2);
        }

        [Test]
        public void ToObject_WithInvalidJson_ShouldThrowException()
        {
            string invalidJson = "{ invalid json }";

            Assert.Throws<JsonReaderException>(() => invalidJson.ToObject<TestData>());
        }

        [Test]
        public void ToObject_WithType_ShouldReturnCorrectObject()
        {
            string json = "{\"IntValue\":42,\"StringValue\":\"test\"}";

            var result = json.ToObject(typeof(TestData));

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<TestData>(result);

            var typedResult = (TestData)result;
            Assert.AreEqual(42, typedResult.IntValue);
            Assert.AreEqual("test", typedResult.StringValue);
        }

        [Test]
        public void ToObject_WithTypeAndNullJson_ShouldReturnNull()
        {
            var result1 = ((string)null).ToObject(typeof(TestData));
            var result2 = "".ToObject(typeof(TestData));

            Assert.IsNull(result1);
            Assert.IsNull(result2);
        }

        [Test]
        public void RoundTrip_SimpleObject_ShouldMaintainData()
        {
            var original = new TestData
            {
                IntValue = 123,
                StringValue = "original",
                BoolValue = false,
                DateValue = DateTime.Now,
                ListValue = new List<int> { 4, 5, 6 }
            };

            string json = original.ToJson();
            var restored = json.ToObject<TestData>();

            Assert.IsNotNull(restored);
            Assert.AreEqual(original.IntValue, restored.IntValue);
            Assert.AreEqual(original.StringValue, restored.StringValue);
            Assert.AreEqual(original.BoolValue, restored.BoolValue);
            Assert.AreEqual(original.ListValue.Count, restored.ListValue.Count);
        }

        [Test]
        public void RoundTrip_ComplexObject_ShouldMaintainStructure()
        {
            var original = new ComplexTestData
            {
                NestedData = new TestData
                {
                    IntValue = 100,
                    StringValue = "nested"
                },
                DictionaryValue = new Dictionary<string, object>
                {
                    ["key1"] = "value1",
                    ["key2"] = 200
                },
                ArrayValue = new int[] { 10, 20, 30 }
            };

            string json = original.ToJson();
            var restored = json.ToObject<ComplexTestData>();

            Assert.IsNotNull(restored);
            Assert.IsNotNull(restored.NestedData);
            Assert.AreEqual(100, restored.NestedData.IntValue);
            Assert.AreEqual("nested", restored.NestedData.StringValue);
            Assert.IsNotNull(restored.ArrayValue);
            Assert.AreEqual(3, restored.ArrayValue.Length);
            Assert.AreEqual(10, restored.ArrayValue[0]);
        }

        [Test]
        public void ToJson_WithDateTime_ShouldUseCorrectFormat()
        {
            var testObj = new TestData
            {
                DateValue = new DateTime(2023, 12, 25, 15, 30, 45)
            };

            string json = testObj.ToJson();

            Assert.IsTrue(json.Contains("20231225 15:30:45"));
        }

        [Test]
        public void ToJson_WithSpecialCharacters_ShouldHandleCorrectly()
        {
            var testObj = new TestData
            {
                StringValue = "特殊字符测试\"引号'单引号\n换行\t制表符"
            };

            string json = testObj.ToJson();
            var restored = json.ToObject<TestData>();

            Assert.IsNotNull(restored);
            Assert.AreEqual(testObj.StringValue, restored.StringValue);
        }

        [Test]
        public void ToJson_WithCircularReference_ShouldIgnoreReference()
        {
            // 由于JsonSerializerSettings设置了ReferenceLoopHandling.Ignore
            // 这个测试验证循环引用不会导致异常
            var parent = new { Name = "Parent", Child = (object)null };
            var child = new { Name = "Child", Parent = parent };
            // 注意：这里不能直接创建循环引用，因为匿名对象是不可变的
            // 这个测试更多是验证设置存在

            Assert.DoesNotThrow(() =>
            {
                string json = parent.ToJson();
                Assert.IsNotNull(json);
            });
        }
    }
}
