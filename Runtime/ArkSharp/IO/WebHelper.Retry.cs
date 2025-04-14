using System;
using System.Runtime.CompilerServices;
using System.Text;
using Cysharp.Threading.Tasks;

namespace ArkSharp
{
	public static partial class WebHelper
	{
		public const int DefaultTryCount = 3;
		public const int DefaultTryIntervalMs = 500;
		public const int DefaultTryIntervalNextFrame = 0;

		/// <summary>
		/// 用GET方法多次重试执行web请求
		/// </summary>
		/// <param name="tryCount">尝试次数，至少为1</param>
		/// <param name="tryIntervalMS">重试等待间隔，0表示等待下一帧</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UniTask<string> TryGet(string url, int tryCount = DefaultTryCount, int tryIntervalMS = DefaultTryIntervalMs)
			=> TryFetch(url, false, null, tryCount, tryIntervalMS);

		/// <summary>
		/// 用POST方法多次重试执行web请求
		/// </summary>
		/// <param name="tryCount">尝试次数，至少为1</param>
		/// <param name="tryIntervalMS">重试等待间隔，0表示等待下一帧</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UniTask<string> TryPost(string url, string postData = null, int tryCount = DefaultTryCount, int tryIntervalMS = DefaultTryIntervalMs)
			=> TryFetch(url, true, postData, tryCount, tryIntervalMS);

		public static async UniTask<string> TryFetch(string url, bool postMode, string postData = null, int tryCount = DefaultTryCount, int tryIntervalMS = DefaultTryIntervalMs)
		{
			if (tryCount < 1)
				tryCount = 1;

			byte[] bytes = null;

			for (int i = 0; i < tryCount; i++)
			{
				try
				{
					bytes = await FetchBytes(url, postMode, postData);
					break;
				}
				catch (Exception)
				{
					// 如果是最后一次尝试，向外抛出异常
					if (i + 1 >= tryCount)
						throw;
				}

				// 重试间隔
				if (tryIntervalMS > 0)
					await UniTask.Delay(tryIntervalMS, true);
				else
					await UniTask.Yield();
			}

			return Encoding.UTF8.GetString(bytes);
		}
	}
}
