using System;
using NUnit.Framework;

namespace ArkSharp.Test.Misc
{
    [TestFixture]
    public class TestRealTime
    {
        [Test]
        public void Now_ShouldReturnValidTime()
        {
            double time1 = RealTime.now;
            double time2 = RealTime.now;

            Assert.IsTrue(time1 > 0);
            Assert.IsTrue(time2 >= time1);
        }

        [Test]
        public void Ticks_ShouldReturnValidTicks()
        {
            long ticks1 = RealTime.ticks;
            long ticks2 = RealTime.ticks;

            Assert.IsTrue(ticks1 > 0);
            Assert.IsTrue(ticks2 >= ticks1);
        }

        [Test]
        public void UnixTime_ShouldReturnValidUnixTime()
        {
            long unixTime1 = RealTime.unixTime;
            long unixTime2 = RealTime.unixTime;

            Assert.IsTrue(unixTime1 > 0);
            Assert.IsTrue(unixTime2 >= unixTime1);

            // Unix时间应该是合理的值（大于2020年1月1日）
            long year2020UnixTime = 1577836800; // 2020-01-01 00:00:00 UTC
            Assert.IsTrue(unixTime1 > year2020UnixTime);
        }

        [Test]
        public void UnixTimeMS_ShouldReturnValidUnixTimeInMilliseconds()
        {
            long unixTimeMS1 = RealTime.unixTimeMS;
            long unixTimeMS2 = RealTime.unixTimeMS;

            Assert.IsTrue(unixTimeMS1 > 0);
            Assert.IsTrue(unixTimeMS2 >= unixTimeMS1);

            // 毫秒时间应该比秒时间大约大1000倍
            long unixTimeS = RealTime.unixTime;
            Assert.IsTrue(Math.Abs(unixTimeMS1 / 1000 - unixTimeS) <= 1);
        }

        [Test]
        public void UnixTimeNS_ShouldReturnValidUnixTimeInNanoseconds()
        {
            long unixTimeNS1 = RealTime.unixTimeNS;
            long unixTimeNS2 = RealTime.unixTimeNS;

            Assert.IsTrue(unixTimeNS1 > 0);
            Assert.IsTrue(unixTimeNS2 >= unixTimeNS1);

            // 纳秒时间应该比毫秒时间大约大1,000,000倍
            long unixTimeMS = RealTime.unixTimeMS;
            Assert.IsTrue(Math.Abs(unixTimeNS1 / 1_000_000 - unixTimeMS) <= 1);
        }

        [Test]
        public void EpochTime_ShouldBeCorrectValue()
        {
            DateTime expectedEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            Assert.AreEqual(expectedEpoch, RealTime.epochTime);
        }

        [Test]
        public void TimeValues_ShouldBeConsistent()
        {
            // 获取所有时间值
            double now = RealTime.now;
            long ticks = RealTime.ticks;
            long unixTime = RealTime.unixTime;
            long unixTimeMS = RealTime.unixTimeMS;
            long unixTimeNS = RealTime.unixTimeNS;

            // 验证它们之间的关系
            // now 应该约等于当前UTC时间的秒数
            DateTime utcNow = DateTime.UtcNow;
            double expectedNow = (double)utcNow.Ticks / TimeSpan.TicksPerSecond;
            Assert.IsTrue(Math.Abs(now - expectedNow) < 1.0); // 1秒误差范围内

            // ticks 应该约等于当前UTC时间的毫秒数
            long expectedTicks = utcNow.Ticks / TimeSpan.TicksPerMillisecond;
            Assert.IsTrue(Math.Abs(ticks - expectedTicks) < 1000); // 1秒误差范围内

            // unixTime 和 unixTimeMS 的关系
            Assert.IsTrue(Math.Abs(unixTimeMS / 1000 - unixTime) <= 1);

            // unixTimeMS 和 unixTimeNS 的关系
            Assert.IsTrue(Math.Abs(unixTimeNS / 1_000_000 - unixTimeMS) <= 1);
        }

        [Test]
        public void UnixTime_ShouldMatchDateTimeCalculation()
        {
            long unixTime = RealTime.unixTime;
            DateTime calculatedTime = RealTime.epochTime.AddSeconds(unixTime);
            DateTime utcNow = DateTime.UtcNow;

            // 计算的时间应该与当前UTC时间相近（1秒误差范围内）
            Assert.IsTrue(Math.Abs((calculatedTime - utcNow).TotalSeconds) < 1.0);
        }

        [Test]
        public void UnixTimeMS_ShouldMatchDateTimeCalculation()
        {
            long unixTimeMS = RealTime.unixTimeMS;
            DateTime calculatedTime = RealTime.epochTime.AddMilliseconds(unixTimeMS);
            DateTime utcNow = DateTime.UtcNow;

            // 计算的时间应该与当前UTC时间相近（1秒误差范围内）
            Assert.IsTrue(Math.Abs((calculatedTime - utcNow).TotalSeconds) < 1.0);
        }

        [Test]
        public void TimeProgression_ShouldBeMonotonic()
        {
            // 连续获取时间值，应该是单调递增的
            double[] nowValues = new double[10];
            long[] tickValues = new long[10];
            long[] unixTimeValues = new long[10];

            for (int i = 0; i < 10; i++)
            {
                nowValues[i] = RealTime.now;
                tickValues[i] = RealTime.ticks;
                unixTimeValues[i] = RealTime.unixTime;

                // 短暂延迟以确保时间推进
                System.Threading.Thread.Sleep(1);
            }

            // 验证单调性
            for (int i = 1; i < 10; i++)
            {
                Assert.IsTrue(nowValues[i] >= nowValues[i-1]);
                Assert.IsTrue(tickValues[i] >= tickValues[i-1]);
                Assert.IsTrue(unixTimeValues[i] >= unixTimeValues[i-1]);
            }
        }

        [Test]
        public void EpochTime_ShouldBeUtcKind()
        {
            Assert.AreEqual(DateTimeKind.Utc, RealTime.epochTime.Kind);
        }

        [Test]
        public void TimeValues_ShouldBeReasonable()
        {
            // 验证时间值在合理范围内
            double now = RealTime.now;
            long ticks = RealTime.ticks;
            long unixTime = RealTime.unixTime;

            // 现在的时间应该大于2020年1月1日
            double year2020Seconds = (new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc) - new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            Assert.IsTrue(now > year2020Seconds);

            // Unix时间应该大于2020年
            Assert.IsTrue(unixTime > 1577836800); // 2020-01-01 00:00:00 UTC

            // 时间值应该小于2050年（防止异常大的值）
            double year2050Seconds = (new DateTime(2050, 1, 1, 0, 0, 0, DateTimeKind.Utc) - new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            Assert.IsTrue(now < year2050Seconds);
        }
    }
}
