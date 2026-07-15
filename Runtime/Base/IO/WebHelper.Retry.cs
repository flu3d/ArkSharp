using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace ArkSharp
{
	public static partial class WebHelper
	{
		public const int DefaultMaxAttemptCount = 3;
		public const int DefaultRetryIntervalMs = 500;
		public const int DefaultRetryIntervalNextFrame = 0;

		/// <summary>
		/// 用 GET 方法执行可重试的 Web 请求
		/// </summary>
		/// <param name="maxAttemptCount">最大尝试次数，至少为 1</param>
		/// <param name="retryIntervalMs">重试等待间隔，0 表示等待下一帧</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async UniTask<string> GetWithRetry(string url, int maxAttemptCount = DefaultMaxAttemptCount, int retryIntervalMs = DefaultRetryIntervalMs)
			=> (await FetchWithRetry(url, false, null, maxAttemptCount, retryIntervalMs)).Text;

		/// <summary>
		/// 用 POST 方法执行可重试的 Web 请求
		/// </summary>
		/// <param name="maxAttemptCount">最大尝试次数，至少为 1</param>
		/// <param name="retryIntervalMs">重试等待间隔，0 表示等待下一帧</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async UniTask<string> PostWithRetry(string url, string postData = null, int maxAttemptCount = DefaultMaxAttemptCount, int retryIntervalMs = DefaultRetryIntervalMs)
			=> (await FetchWithRetry(url, true, postData, maxAttemptCount, retryIntervalMs)).Text;

		public static async UniTask<WebResult> FetchWithRetry(string url, bool postMode, string postData = null, int maxAttemptCount = DefaultMaxAttemptCount, int retryIntervalMs = DefaultRetryIntervalMs)
		{
			if (maxAttemptCount < 1)
				maxAttemptCount = 1;

			WebResult result = default;

			for (int i = 0; i < maxAttemptCount; i++)
			{
				result = await Fetch(url, postMode, postData);
				if (result.IsSuccess || i + 1 >= maxAttemptCount)
					return result;

				// 重试间隔
				if (retryIntervalMs > 0)
				{
#if UNITY_5_3_OR_NEWER
					await UniTask.Delay(retryIntervalMs, true);
#else
					await Task.Delay(retryIntervalMs);
#endif
				}
				else
				{
#if UNITY_5_3_OR_NEWER
					await UniTask.Yield();
#else
					await Task.Yield();
#endif
				}
			}

			return result;
		}
	}
}
