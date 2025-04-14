#pragma warning disable SYSLIB0014

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Cysharp.Threading.Tasks;

#if UNITY_5_3_OR_NEWER
using UnityEngine.Networking;
#else
using System.Net;
#endif

namespace ArkSharp
{
	public static partial class WebHelper
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async UniTask<string> Get(string url)
		{
			var bytes = await FetchBytes(url, false, null);
			return Encoding.UTF8.GetString(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async UniTask<string> Post(string url, string postData = null)
		{
			var bytes = await FetchBytes(url, true, postData);
			return Encoding.UTF8.GetString(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UniTask<byte[]> GetBytes(string url) => FetchBytes(url, false, null);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UniTask<byte[]> PostBytes(string url, string postData = null) => FetchBytes(url, true, postData);

#if UNITY_5_3_OR_NEWER
		public static async UniTask<byte[]> FetchBytes(string url, bool postMode, string postData = null)
		{
			using (var www = postMode ? UnityWebRequest.PostWwwForm(url, postData ?? "") : UnityWebRequest.Get(url))
			{
				var req = await www.SendWebRequest();
				return req.downloadHandler.data;
			}
		}
#else
		public static UniTask<byte[]> FetchBytes(string url, bool postMode, string postData = null)
		{
			using (var www = new CustomWebClient { Encoding = Encoding.UTF8, Proxy = null })
			{
				var req = new UniTaskCompletionSource<byte[]>();

				if (postMode)
				{
					www.UploadDataCompleted += (sender, e) => {
						//req.SetProgress(1);
						if (e.Error != null)
							req.TrySetException(e.Error);
						else
							req.TrySetResult(e.Result);
					};
					//www.UploadProgressChanged += (sender, e) => req.SetProgress((float)e.ProgressPercentage / 100);
					www.UploadDataAsync(new Uri(url), www.Encoding.GetBytes(postData ?? ""));
				}
				else
				{
					www.DownloadDataCompleted += (sender, e) => {
						//req.SetProgress(1);
						if (e.Error != null)
							req.TrySetException(e.Error);
						else
							req.TrySetResult(e.Result);
					};
					//www.DownloadProgressChanged += (sender, e) => req.SetProgress((float)e.ProgressPercentage / 100);
					www.DownloadDataAsync(new Uri(url));
				}

				return req.Task;
			}
		}

		class CustomWebClient : WebClient
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
