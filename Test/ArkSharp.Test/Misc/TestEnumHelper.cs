using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace ArkSharp.Test.Misc
{
    // 测试用枚举
    public enum TestEnum
    {
        None = 0,
        First = 1,
        Second = 2,
        Third = 4,
        Fourth = 8
    }

    [Flags]
    public enum TestFlagsEnum
    {
        None = 0,
        Flag1 = 1,
        Flag2 = 2,
        Flag3 = 4,
        Flag4 = 8,
        Combined = Flag1 | Flag2
    }

#if UNITY_5_3_OR_NEWER
    public enum TestEnumWithAlias
    {
        [InspectorName("别名1")]
        Item1 = 1,

        [InspectorName("别名2")]
        Item2 = 2,

        Item3 = 3
    }
#endif

    [TestFixture]
    public class TestEnumHelper
    {
        [SetUp]
        public void Setup()
        {
            EnumHelper.EnumIgnoreCase = false;
            EnumHelper.EnumSplitChars = new char[] { '|', ',' };
        }

        [Test]
        public void Combine_WithValidList_ShouldCombineFlags()
        {
            var enumList = new List<TestFlagsEnum> { TestFlagsEnum.Flag1, TestFlagsEnum.Flag2, TestFlagsEnum.Flag3 };
            var result = EnumHelper.Combine(enumList);

            Assert.AreEqual(TestFlagsEnum.Flag1 | TestFlagsEnum.Flag2 | TestFlagsEnum.Flag3, result);
        }

        [Test]
        public void Combine_WithEmptyList_ShouldReturnNone()
        {
            var enumList = new List<TestFlagsEnum>();
            var result = EnumHelper.Combine(enumList);

            Assert.AreEqual(TestFlagsEnum.None, result);
        }

        [Test]
        public void To_WithValidEnumName_ShouldReturnEnum()
        {
            var result = "First".To<TestEnum>();
            Assert.AreEqual(TestEnum.First, result);
        }

        [Test]
        public void To_WithValidEnumNumber_ShouldReturnEnum()
        {
            var result = "2".To<TestEnum>();
            Assert.AreEqual(TestEnum.Second, result);
        }

        [Test]
        public void To_WithInvalidEnumName_ShouldReturnDefault()
        {
            var result = "Invalid".To<TestEnum>();
            Assert.AreEqual(TestEnum.None, result);
        }

        [Test]
        public void To_WithEmptyString_ShouldReturnDefault()
        {
            var result = "".To<TestEnum>();
            Assert.AreEqual(TestEnum.None, result);
        }

        [Test]
        public void To_WithCombinedFlags_ShouldReturnCombinedEnum()
        {
            var result = "Flag1|Flag2".To<TestFlagsEnum>();
            Assert.AreEqual(TestFlagsEnum.Flag1 | TestFlagsEnum.Flag2, result);
        }

        [Test]
        public void To_WithCombinedFlagsCommaSeparated_ShouldReturnCombinedEnum()
        {
            var result = "Flag1,Flag2".To<TestFlagsEnum>();
            Assert.AreEqual(TestFlagsEnum.Flag1 | TestFlagsEnum.Flag2, result);
        }

        [Test]
        public void To_WithMixedValidAndInvalidNames_ShouldIgnoreInvalid()
        {
            var result = "Flag1|Invalid|Flag2".To<TestFlagsEnum>();
            Assert.AreEqual(TestFlagsEnum.Flag1 | TestFlagsEnum.Flag2, result);
        }

        [Test]
        public void To_WithIgnoreCase_ShouldBeCaseInsensitive()
        {
            EnumHelper.EnumIgnoreCase = true;
            var result = "first".To<TestEnum>();
            Assert.AreEqual(TestEnum.First, result);
        }

        [Test]
        public void To_WithCaseSensitive_ShouldBeCaseSensitive()
        {
            EnumHelper.EnumIgnoreCase = false;
            var result = "first".To<TestEnum>();
            Assert.AreEqual(TestEnum.None, result);
        }

        [Test]
        public void To_ReadOnlySpan_ShouldWork()
        {
            ReadOnlySpan<char> span = "First".AsSpan();
            var result = span.To<TestEnum>();
            Assert.AreEqual(TestEnum.First, result);
        }

        [Test]
        public void Parse_WithValidType_ShouldReturnCorrectObject()
        {
            var result = EnumHelper.Parse("First".AsSpan(), typeof(TestEnum));
            Assert.AreEqual(TestEnum.First, result);
        }

        [Test]
        public void Parse_WithNumericString_ShouldParseCorrectly()
        {
            var result = EnumHelper.Parse("2".AsSpan(), typeof(TestEnum));
            Assert.AreEqual(TestEnum.Second, result);
        }

        [Test]
        public void Parse_WithNegativeNumber_ShouldParseCorrectly()
        {
            var result = EnumHelper.Parse("-1".AsSpan(), typeof(TestEnum));
            Assert.AreEqual((TestEnum)(-1), result);
        }

        [Test]
        public void Parse_WithPlusSign_ShouldParseCorrectly()
        {
            var result = EnumHelper.Parse("+2".AsSpan(), typeof(TestEnum));
            Assert.AreEqual(TestEnum.Second, result);
        }

        [Test]
        public void GetMembers_WithValidEnumType_ShouldReturnMembers()
        {
            var members = EnumHelper.GetMembers(typeof(TestEnum));

            Assert.IsNotNull(members);
            Assert.AreEqual(5, members.Count); // None, First, Second, Third, Fourth

            Assert.AreEqual("None", members[0].name);
            Assert.AreEqual(TestEnum.None, members[0].value);
        }

        [Test]
        public void GetMembers_WithNonEnumType_ShouldReturnNull()
        {
            var members = EnumHelper.GetMembers(typeof(string));
            Assert.IsNull(members);
        }

        [Test]
        public void GetMembers_ShouldCacheResults()
        {
            var members1 = EnumHelper.GetMembers(typeof(TestEnum));
            var members2 = EnumHelper.GetMembers(typeof(TestEnum));

            Assert.AreSame(members1, members2);
        }

#if UNITY_5_3_OR_NEWER
        [Test]
        public void To_WithAlias_ShouldUseAlias()
        {
            var result = "别名1".To<TestEnumWithAlias>();
            Assert.AreEqual(TestEnumWithAlias.Item1, result);
        }

        [Test]
        public void To_WithAliasDisabled_ShouldUseEnumName()
        {
            var result = "Item1".To<TestEnumWithAlias>(checkAlias: false);
            Assert.AreEqual(TestEnumWithAlias.Item1, result);
        }

        [Test]
        public void To_WithAliasDisabled_ShouldIgnoreAlias()
        {
            var result = "别名1".To<TestEnumWithAlias>(checkAlias: false);
            Assert.AreEqual((TestEnumWithAlias)0, result); // 应该返回默认值，因为别名被忽略
        }

        [Test]
        public void GetMembers_WithAlias_ShouldIncludeAlias()
        {
            var members = EnumHelper.GetMembers(typeof(TestEnumWithAlias));

            Assert.IsNotNull(members);
            var item1Member = members[0]; // Item1
            Assert.AreEqual("Item1", item1Member.name);
            Assert.AreEqual("别名1", item1Member.alias);
            Assert.AreEqual(TestEnumWithAlias.Item1, item1Member.value);
        }
#endif

        [Test]
        public void To_WithWhitespaceInCombination_ShouldNotMatchWithSpaces()
        {
            // EnumHelper不会自动trim空格，所以包含空格的枚举名称无法匹配
            var result = "Flag1 | Flag2 , Flag3".To<TestFlagsEnum>();
            Assert.AreEqual(TestFlagsEnum.None, result);
        }

        [Test]
        public void To_WithNoWhitespaceInCombination_ShouldWork()
        {
            // 验证没有空格时正常工作
            var result = "Flag1|Flag2,Flag3".To<TestFlagsEnum>();
            Assert.AreEqual(TestFlagsEnum.Flag1 | TestFlagsEnum.Flag2 | TestFlagsEnum.Flag3, result);
        }

        [Test]
        public void To_WithEmptyParts_ShouldIgnoreEmptyParts()
        {
            var result = "Flag1||,Flag2".To<TestFlagsEnum>();
            Assert.AreEqual(TestFlagsEnum.Flag1 | TestFlagsEnum.Flag2, result);
        }

        [Test]
        public void Parse_WithLargeNumber_ShouldHandleCorrectly()
        {
            var result = EnumHelper.Parse("999999".AsSpan(), typeof(TestEnum));
            Assert.AreEqual((TestEnum)999999, result);
        }

        [Test]
        public void EnumMember_Properties_ShouldBeReadonly()
        {
            var members = EnumHelper.GetMembers(typeof(TestEnum));
            var member = members[1]; // First

            Assert.AreEqual("First", member.name);
            Assert.AreEqual(TestEnum.First, member.value);
            Assert.IsNull(member.alias); // 没有别名
        }

        [Test]
        public void To_WithCustomSplitChars_ShouldWork()
        {
            EnumHelper.EnumSplitChars = new char[] { ';', '&' };
            var result = "Flag1;Flag2&Flag3".To<TestFlagsEnum>();
            Assert.AreEqual(TestFlagsEnum.Flag1 | TestFlagsEnum.Flag2 | TestFlagsEnum.Flag3, result);
        }

        [Test]
        public void GetMembers_MixedReadAndWrite_ShouldNotCorrupt_InMultiThread()
        {
            // 测试混合场景：一些线程首次访问新类型，一些线程重复访问已缓存类型
            const int repeatCount = 50;
            const int threadCount = 300;
            const int iterationsPerThread = 1000;

            for (int repeat = 0; repeat < repeatCount; repeat++)
            {
                // 清除缓存
                var cacheField = typeof(EnumHelper).GetField("_cacheEnumMembers",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                if (cacheField != null)
                {
                    var cache = cacheField.GetValue(null) as System.Collections.IDictionary;
                    cache?.Clear();
                }

                var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();
                var threads = new System.Threading.Thread[threadCount];
                var startSignal = new System.Threading.ManualResetEventSlim(false);

                for (int i = 0; i < threadCount; i++)
                {
                    int threadIndex = i;
                    threads[i] = new System.Threading.Thread(() =>
                    {
                        try
                        {
                            startSignal.Wait();

                            // 前10个线程访问 TestEnum，中间10个访问 TestFlagsEnum，后10个再访问 TestEnum
                            // 这样可以让一些线程在读取 TestEnum 时，其他线程正在写入 TestFlagsEnum
                            Type enumType;
                            int expectedCount;

                            if (threadIndex < 10)
                            {
                                enumType = typeof(TestEnum);
                                expectedCount = 5;
                            }
                            else if (threadIndex < 20)
                            {
                                enumType = typeof(TestFlagsEnum);
                                expectedCount = 6;
                            }
                            else
                            {
                                enumType = typeof(TestEnum);
                                expectedCount = 5;
                            }

                            // 多次调用以增加触发问题的概率
                            for (int j = 0; j < iterationsPerThread; j++)
                            {
                                var members = EnumHelper.GetMembers(enumType);

                                if (members == null)
                                {
                                    throw new InvalidOperationException($"GetMembers({enumType.Name}) 返回 null");
                                }

                                if (members.Count != expectedCount)
                                {
                                    throw new InvalidOperationException(
                                        $"GetMembers({enumType.Name}) 返回的成员数量错误: {members.Count}，期望: {expectedCount}");
                                }

                                // 验证成员完整性
                                foreach (var member in members)
                                {
                                    if (member == null || member.name == null || member.value == null)
                                    {
                                        throw new InvalidOperationException($"GetMembers({enumType.Name}) 返回的成员数据不完整");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    });
                }

                // 启动所有线程
                foreach (var thread in threads)
                {
                    thread.Start();
                }

                // 发送信号
                startSignal.Set();

                // 等待所有线程完成
                foreach (var thread in threads)
                {
                    thread.Join();
                }

                // 检查是否有异常
                if (!exceptions.IsEmpty)
                {
                    var firstException = exceptions.FirstOrDefault();
                    Assert.Fail($"第 {repeat + 1} 次迭代中发生 {exceptions.Count} 个异常。第一个异常: {firstException?.Message}\n堆栈: {firstException?.StackTrace}");
                }
            }
        }


    }
}
