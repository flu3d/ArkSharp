using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace ArkSharp.Test.Objects
{
	public class TestObjectPoolAsync
	{
		private class TestObject
		{
		}

		private sealed class ValueEqualObject
		{
			public int Value { get; }

			public ValueEqualObject(int value)
			{
				Value = value;
			}

			public override bool Equals(object obj)
			{
				return obj is ValueEqualObject other && other.Value == Value;
			}

			public override int GetHashCode()
			{
				return Value;
			}
		}

		[Test]
		public void Release_DistinctValueEqualObjects_RetainsBothInstances()
		{
			var pool = new ObjectPoolAsync<ValueEqualObject>();
			var obj1 = new ValueEqualObject(1);
			var obj2 = new ValueEqualObject(1);

			pool.Release(obj1);
			pool.Release(obj2);

			Assert.AreEqual(2, pool.Count);
		}

		[Test]
		public void Release_SameInstanceTwice_RetainsOneInstance()
		{
			var pool = new ObjectPoolAsync<TestObject>();
			var obj = new TestObject();

			pool.Release(obj);
			pool.Release(obj);

			Assert.AreEqual(1, pool.Count);
		}

		[Test]
		public void Release_WhenReleaseCallbackThrows_DoesNotRetainObject()
		{
			var pool = new ObjectPoolAsync<TestObject>()
				.OnReleaseWith(_ => throw new InvalidOperationException());

			Assert.Throws<InvalidOperationException>(() => pool.Release(new TestObject()));
			Assert.AreEqual(0, pool.Count);
		}

		[Test]
		public void Release_WhenFull_InvokesDisposeFunc()
		{
			int disposeCount = 0;
			var pool = new ObjectPoolAsync<TestObject>(1)
				.OnDisposeWith(_ => disposeCount++);

			pool.Release(new TestObject());
			pool.Release(new TestObject());

			Assert.AreEqual(1, pool.Count);
			Assert.AreEqual(1, disposeCount);
		}

		[Test]
		public void Clear_WhenDisposeCallbacksThrow_RemovesAllAndThrowsAggregateException()
		{
			var pool = new ObjectPoolAsync<TestObject>()
				.OnDisposeWith(_ => throw new InvalidOperationException());

			pool.Release(new TestObject());
			pool.Release(new TestObject());

			var exception = Assert.Throws<AggregateException>(() => pool.Clear());
			Assert.AreEqual(2, exception.InnerExceptions.Count);
			Assert.AreEqual(0, pool.Count);
		}

		[Test]
		public async Task Clear_WhenDisposeCallbackReleasesObject_PreservesNewlyReleasedObject()
		{
			ObjectPoolAsync<TestObject> pool = null;
			var obj = new TestObject();
			int disposeCount = 0;

			pool = new ObjectPoolAsync<TestObject>()
				.OnDisposeWith(value =>
				{
					disposeCount++;
					if (disposeCount == 1)
						pool.Release(value);
				});

			pool.Release(obj);
			pool.Clear();

			Assert.AreEqual(1, disposeCount);
			Assert.AreEqual(1, pool.Count);
			Assert.AreSame(obj, await pool.Alloc());
		}

		[Test]
		public async Task Alloc_WithCanceledToken_ReturnsNullAndDoesNotConsumePooledObject()
		{
			var pool = new ObjectPoolAsync<TestObject>();
			pool.Release(new TestObject());
			using var cts = new CancellationTokenSource();
			cts.Cancel();

			var obj = await pool.Alloc(cts.Token);

			Assert.IsNull(obj);
			Assert.AreEqual(1, pool.Count);
		}

		[Test]
		public async Task Alloc_WhenFactoryIsCanceled_ReturnsNull()
		{
			using var cts = new CancellationTokenSource();
			var pool = new ObjectPoolAsync<TestObject>(token =>
			{
				cts.Cancel();
				return UniTask.FromCanceled<TestObject>(token);
			});

			var obj = await pool.Alloc(cts.Token);

			Assert.IsNull(obj);
		}

		[Test]
		public async Task WarmUp_WhenCalledRepeatedly_AddsRequestedCount()
		{
			var pool = new ObjectPoolAsync<TestObject>(10);

			await pool.WarmUp(3);
			await pool.WarmUp(3);

			Assert.AreEqual(6, pool.Count);
		}

		[Test]
		public async Task WarmUp_WithReleaseTrue_RespectsLimitCount()
		{
			int releaseCount = 0;
			var pool = new ObjectPoolAsync<TestObject>(2)
				.OnReleaseWith(_ => releaseCount++);

			await pool.WarmUp(3, withRelease: true);

			Assert.AreEqual(2, pool.Count);
			Assert.AreEqual(2, releaseCount);
		}

		[Test]
		public void SetLimitCount_WhenReduced_DoesNotDestroyExistingObjects()
		{
			int disposeCount = 0;
			var pool = new ObjectPoolAsync<TestObject>()
				.OnDisposeWith(_ => disposeCount++);

			pool.Release(new TestObject());
			pool.Release(new TestObject());
			pool.Release(new TestObject());
			pool.SetLimitCount(2);

			Assert.AreEqual(3, pool.Count);
			Assert.AreEqual(0, disposeCount);
			Assert.IsTrue(pool.IsFull);
		}

		[Test]
		public async Task WarmUp_WhenFactoryReturnsNull_ContinuesCreating()
		{
			int createCount = 0;
			var pool = new ObjectPoolAsync<TestObject>(_ =>
			{
				createCount++;
				return UniTask.FromResult(createCount == 1 ? null : new TestObject());
			});

			await pool.WarmUp(3);

			Assert.AreEqual(3, createCount);
			Assert.AreEqual(2, pool.Count);
		}

		[Test]
		public async Task WarmUp_WhenCanceledAfterPartialCompletion_EndsWithoutThrowingAndPreservesCompletedObjects()
		{
			using var cts = new CancellationTokenSource();
			int createCount = 0;
			var pool = new ObjectPoolAsync<TestObject>(_ =>
			{
				createCount++;
				if (createCount == 2)
					cts.Cancel();

				cts.Token.ThrowIfCancellationRequested();
				return UniTask.FromResult(new TestObject());
			});

			await pool.WarmUp(3, token: cts.Token);

			Assert.AreEqual(1, pool.Count);
		}

		[Test]
		public async Task WarmUp_WhenFactoryThrows_PreservesCompletedObjects()
		{
			int createCount = 0;
			var pool = new ObjectPoolAsync<TestObject>(_ =>
			{
				createCount++;
				if (createCount == 2)
					throw new InvalidOperationException();

				return UniTask.FromResult(new TestObject());
			});

			try
			{
				await pool.WarmUp(3);
				Assert.Fail("工厂异常应向上传播。");
			}
			catch (Exception e)
			{
				Assert.IsInstanceOf<InvalidOperationException>(e);
			}

			Assert.AreEqual(1, pool.Count);
		}

		[Test]
		public async Task WarmUp_WhenReleaseCallbackThrows_DisposesCreatedObject()
		{
			int disposeCount = 0;
			var pool = new ObjectPoolAsync<TestObject>()
				.OnReleaseWith(_ => throw new InvalidOperationException())
				.OnDisposeWith(_ => disposeCount++);

			try
			{
				await pool.WarmUp(1, withRelease: true);
				Assert.Fail("归还回调异常应向上传播。");
			}
			catch (Exception e)
			{
				Assert.IsInstanceOf<InvalidOperationException>(e);
			}

			Assert.AreEqual(1, disposeCount);
			Assert.AreEqual(0, pool.Count);
		}

		[Test]
		public async Task WarmUp_WhenReleaseAndDisposeCallbacksThrow_ThrowsAggregateException()
		{
			var pool = new ObjectPoolAsync<TestObject>()
				.OnReleaseWith(_ => throw new InvalidOperationException("release"))
				.OnDisposeWith(_ => throw new ArgumentException("dispose"));

			try
			{
				await pool.WarmUp(1, withRelease: true);
				Assert.Fail("两个回调都失败时应抛出 AggregateException。");
			}
			catch (Exception e)
			{
				Assert.IsInstanceOf<AggregateException>(e);
				Assert.AreEqual(2, ((AggregateException)e).InnerExceptions.Count);
			}
		}
	}
}
