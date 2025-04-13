using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ArkSharp
{
	public static class WebLoaderUtils
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static WebLoader.Request Get(this WebLoader loader, string url, int tryCount, int tryIntervalMS = 0)
		{
			if (tryCount <= 1)
				return loader.Get(url);

			var request = new WebLoader.Request();
			loader.Scheduler.Start(TryLoadWeb(request, loader, url, null, tryCount, tryIntervalMS));
			return request;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static WebLoader.Request Post(this WebLoader loader, string url, string postData, int tryCount, float tryIntervalMS = 0)
		{
			if (tryCount <= 1)
				return loader.Post(url, postData);

			var request = new WebLoader.Request();
			loader.Scheduler.Start(TryLoadWeb(request, loader, url, postData, tryCount, tryIntervalMS));
			return request;
		}

		private static IEnumerator TryLoadWeb(WebLoader.Request request, WebLoader loader, string url, string postData, int tryCount, float tryIntervalMS)
		{
			string lastError = null;

			for (int i = 0; i < tryCount; i++)
			{
				var req = (postData == null)
					? loader.Get(url)
					: loader.Post(url, postData);

				do
				{
					yield return null;
					request.SetProgress(req.Progress);

				} while (!req.IsCompleted);

				if (req.Error == null)
				{
					request.SetResult(req.Result);
					yield break;
				}
				else
				{
					lastError = req.Error;
				}

				if (tryIntervalMS > 0)
					yield return new WaitForSecondsRealtime(tryIntervalMS * 0.001f);
			}

			request.SetError(lastError);
		}
	}
}
