using System;
using NUnit.Framework;

namespace ArkSharp.Test.Objects
{
    public class TestObjectPool
    {
        private class TestObject
        {
            public int Value { get; set; }
            public bool IsReset { get; set; }
            public bool IsDisposed { get; set; }
        }

        /// <summary>
        /// 测试目标：验证使用默认构造函数创建对象池后，池的初始状态是否正确
        /// </summary>
        [Test]
        public void CreatePool_WithDefaultConstructor_Success()
        {
            var pool = new ObjectPool<TestObject>();
            Assert.AreEqual(0, pool.Count);
            Assert.IsTrue(pool.IsEmpty);
            Assert.IsFalse(pool.IsFull);
        }

        /// <summary>
        /// 测试目标：验证创建带有容量限制的对象池时，限制值是否正确设置
        /// </summary>
        [Test]
        public void CreatePool_WithLimitCount_Success()
        {
            var pool = new ObjectPool<TestObject>(2);
            Assert.AreEqual(2, pool.LimitCount);
        }

        /// <summary>
        /// 测试目标：验证从空池中分配对象时是否能正确创建新对象
        /// </summary>
        [Test]
        public void Alloc_FromEmptyPool_CreatesNewObject()
        {
            var pool = new ObjectPool<TestObject>();
            var obj = pool.Alloc();

            Assert.IsNotNull(obj);
            Assert.AreEqual(0, pool.Count);
        }

        /// <summary>
        /// 测试目标：验证将对象释放回池中时是否正确增加池中对象计数
        /// </summary>
        [Test]
        public void Release_ToPool_IncreasesCount()
        {
            var pool = new ObjectPool<TestObject>();
            var obj = pool.Alloc();
            pool.Release(obj);

            Assert.AreEqual(1, pool.Count);
            Assert.IsFalse(pool.IsEmpty);
        }

        /// <summary>
        /// 测试目标：验证当池已满时，释放新对象是否会正确调用销毁函数
        /// </summary>
        [Test]
        public void Release_WhenFull_InvokesDisposeFunc()
        {
            var isDisposed = false;
            var pool = new ObjectPool<TestObject>(1)
                .OnDisposeWith(obj => isDisposed = true);

            var obj1 = new TestObject();
            var obj2 = new TestObject();

            pool.Release(obj1);
            pool.Release(obj2);  // 这个对象应该被处理掉而不是加入池中

            Assert.AreEqual(1, pool.Count);
            Assert.IsTrue(isDisposed);
        }

        /// <summary>
        /// 测试目标：验证清理对象池时是否正确移除所有对象并调用销毁函数
        /// </summary>
        [Test]
        public void Clear_RemovesAllObjects()
        {
            bool disposeCalled = false;
            var pool = new ObjectPool<TestObject>()
                .OnDisposeWith(obj => disposeCalled = true);

            pool.Release(new TestObject());
            pool.Release(new TestObject());

            pool.Clear();

            Assert.AreEqual(0, pool.Count);
            Assert.IsTrue(pool.IsEmpty);
            Assert.IsTrue(disposeCalled);
        }

        /// <summary>
        /// 测试目标：验证自定义的创建、重置和销毁函数是否按预期工作
        /// </summary>
        [Test]
        public void CustomFunctions_WorkAsExpected()
        {
            int createCount = 0;
            bool resetCalled = false;
            bool disposeCalled = false;

            var pool = new ObjectPool<TestObject>()
                .CreateWith(() => { createCount++; return new TestObject(); })
                .OnReleaseWith(obj => resetCalled = true)
                .OnDisposeWith(obj => disposeCalled = true);

            var obj = pool.Alloc();
            Assert.AreEqual(1, createCount);

            pool.Release(obj);
            Assert.IsTrue(resetCalled);

            pool.Clear();
            Assert.IsTrue(disposeCalled);
        }

        /// <summary>
        /// 测试目标：验证预热功能是否正确创建指定数量的对象
        /// </summary>
        [Test]
        public void WarmUp_CreatesSpecifiedNumberOfObjects()
        {
            var pool = new ObjectPool<TestObject>(5);
            pool.WarmUp(3);

            Assert.AreEqual(3, pool.Count);
        }

        /// <summary>
        /// 测试目标：验证预热功能是否遵守对象池的容量限制
        /// </summary>
        [Test]
        public void WarmUp_RespectsLimitCount()
        {
            var pool = new ObjectPool<TestObject>(2);
            pool.WarmUp(3); // 尝试预热3个对象，但限制是2个

            Assert.AreEqual(2, pool.Count);
            Assert.IsTrue(pool.IsFull);
        }

        /// <summary>
        /// 测试目标：验证预热功能在 withRelease=false 时不调用 Release 回调函数
        /// </summary>
        [Test]
        public void WarmUp_WithReleaseFalse_DoesNotInvokeReleaseCallback()
        {
            bool releaseCalled = false;
            var pool = new ObjectPool<TestObject>(5)
                .OnReleaseWith(obj => releaseCalled = true);

            pool.WarmUp(3, withRelease: false);

            Assert.AreEqual(3, pool.Count);
            Assert.IsFalse(releaseCalled);
        }

        /// <summary>
        /// 测试目标：验证预热功能在 withRelease=true 时正确调用 Release 回调函数
        /// </summary>
        [Test]
        public void WarmUp_WithReleaseTrue_InvokesReleaseCallback()
        {
            int releaseCallCount = 0;
            var pool = new ObjectPool<TestObject>(5)
                .OnReleaseWith(obj => releaseCallCount++);

            pool.WarmUp(3, withRelease: true);

            Assert.AreEqual(3, pool.Count);
            Assert.AreEqual(3, releaseCallCount);
        }

        /// <summary>
        /// 测试目标：验证预热功能在达到容量限制时，withRelease 参数的行为是否正确
        /// </summary>
        [Test]
        public void WarmUp_WithReleaseTrue_RespectsLimitCount()
        {
            int releaseCallCount = 0;
            var pool = new ObjectPool<TestObject>(2)
                .OnReleaseWith(obj => releaseCallCount++);

            pool.WarmUp(3, withRelease: true); // 尝试预热3个对象，但限制是2个

            Assert.AreEqual(2, pool.Count);
            Assert.AreEqual(2, releaseCallCount); // 只有实际加入池中的对象才会调用 Release 回调
            Assert.IsTrue(pool.IsFull);
        }
    }
}
