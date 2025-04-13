#pragma warning disable SYSLIB0014

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

#if UNITY_5_3_OR_NEWER
using UnityEngine.Networking;
#endif

namespace Ark
{
	public class WebLoader
	{
		public class Request : AsyncOp
		{
			public override bool IsCompleted => _completed;
			public byte[] Result => _result;
			public string GetResultAsText() => (_result == null) ? null : Encoding.UTF8.GetString(_result);

			public string Error { get; private set; }
			public float Progress { get; private set; }

			internal void SetProgress(float val) => Progress = val;
			internal void SetResult(byte[] obj) { _result = obj; _completed = true; }
			internal void SetError(string error) { Error = !string.IsNullOrEmpty(error) ? error : "Unknown error"; _completed = true; }

			private byte[] _result;
			private bool _completed;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Request Get(string url) => LoadWeb(url, false, null);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Request Post(string url, string postData) => LoadWeb(url, true, postData ?? "");

		private Request LoadWeb(string url, bool postMode, string postData)
		{
			var result = new Request();

#if UNITY_5_3_OR_NEWER
			_scheduler.Start(LoadWithUnityWWW(result, url, postMode, postData));
#else
			_scheduler.Start(LoadWithWebClient(result, url, postMode, postData));
#endif

			return result;
		}

#if UNITY_5_3_OR_NEWER
		private static IEnumerator LoadWithUnityWWW(Request req, string url, bool postMode, string postData = null)
		{
			using (var www = postMode ? UnityWebRequest.Post(url, postData ?? "") : UnityWebRequest.Get(url))
			{
				var wwwreq = www.SendWebRequest();
				do
				{
					yield return null;
					req.SetProgress(wwwreq.progress);

				} while (!wwwreq.isDone);

				req.SetProgress(1);

				if (www.result != UnityWebRequest.Result.Success)
					req.SetError(www.error);
				else
					req.SetResult(www.downloadHandler.data);
			}
		}
#else
		private static IEnumerator LoadWithWebClient(Request req, string url, bool postMode, string postData = null)
		{
			using (var www = new CustomWebClient { Encoding = Encoding.UTF8, Proxy = null })
			{
				if (postMode)
				{
					www.UploadDataCompleted += (sender, e) => {
						req.SetProgress(1);
						if (e.Error != null)
							req.SetError(e.Error.Message);
						else
							req.SetResult(e.Result);
					};
					www.UploadProgressChanged += (sender, e) => req.SetProgress((float)e.ProgressPercentage / 100);
					www.UploadDataAsync(new Uri(url), www.Encoding.GetBytes(postData ?? ""));
				}
				else
				{
					www.DownloadDataCompleted += (sender, e) => {
						req.SetProgress(1);
						if (e.Error != null)
							req.SetError(e.Error.Message);
						else
							req.SetResult(e.Result);
					};
					www.DownloadProgressChanged += (sender, e) => req.SetProgress((float)e.ProgressPercentage / 100);
					www.DownloadDataAsync(new Uri(url));
				}

				yield return req;
			}
		}

		private class CustomWebClient : WebClient
		{
			protected override WebRequest GetWebRequest(Uri address)
			{
				var request = base.GetWebRequest(address);
				if (request is HttpWebRequest httpRequest)
					httpRequest.AllowAutoRedirect = true;

				return request;
			}
		}
#endif

		private readonly CoroutineScheduler _scheduler;
		public CoroutineScheduler Scheduler => _scheduler;

		public WebLoader() => _scheduler = CoroutineScheduler.Default;
		public WebLoader(CoroutineScheduler scheduler) => _scheduler = scheduler ?? CoroutineScheduler.Default;

		public static WebLoader Default => Singleton.Get<WebLoader>();

		/// <summary>
		/// 辅助方法:参数字典=>查询字符串
		/// </summary>
		public static string BuildQueryString(IReadOnlyDictionary<string, string> fields)
		{
			if (fields == null || fields.Count <= 0)
				return string.Empty;

			var s = new StringBuilder();
			var first = true;

			foreach (var kv in fields)
			{
				if (first)
					first = false;
				else
					s.Append('&');

				s.AppendFormat("{0}={1}", kv.Key, Uri.EscapeDataString(kv.Value));
			}

			return s.ToString();
		}
	}
}
