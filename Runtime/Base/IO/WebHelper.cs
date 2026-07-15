using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Cysharp.Threading.Tasks;

#if UNITY_5_3_OR_NEWER
using UnityEngine.Networking;
#else
using System.Net.Http;
#endif

namespace ArkSharp
{
	public readonly struct WebResult
	{
		private static readonly byte[] UTF8_BOM = Encoding.UTF8.GetPreamble();

		public readonly byte[] Bytes;
		public readonly string Error;

		public bool IsSuccess => string.IsNullOrEmpty(Error);
		public string Text
		{
			get
			{
				if (Bytes == null)
					return null;
				if (Bytes.Length <= 0)
					return string.Empty;

				var offset = Bytes.AsSpan().StartsWith(UTF8_BOM) ? UTF8_BOM.Length : 0;
				return Encoding.UTF8.GetString(Bytes, offset, Bytes.Length - offset);
			}
		}

		public WebResult(byte[] bytes, string error)
		{
			Bytes = bytes;
			Error = error;
		}
	}

	public static partial class WebHelper
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async UniTask<string> Get(string url)
		{
			return (await Fetch(url, false, null)).Text;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async UniTask<string> Post(string url, string postData = null)
		{
			return (await Fetch(url, true, postData)).Text;
		}

#if UNITY_5_3_OR_NEWER
		public static async UniTask<WebResult> Fetch(string url, bool postMode, string postData = null)
		{
			try
			{
				using (var www = postMode ? UnityWebRequest.PostWwwForm(url, postData ?? "") : UnityWebRequest.Get(url))
				{
					await www.SendWebRequest();
					if (www.result != UnityWebRequest.Result.Success)
						return new WebResult(null, string.IsNullOrEmpty(www.error) ? $"Request failed: {www.result}." : www.error);

					return new WebResult(www.downloadHandler.data, null);
				}
			}
			catch (Exception e)
			{
				return new WebResult(null, e.Message);
			}
		}
#else
		private static readonly HttpClient _httpClient = new(new SocketsHttpHandler {
			AllowAutoRedirect = true,
			UseCookies = false,
			PooledConnectionLifetime = TimeSpan.FromMinutes(2),
		});

		public static async UniTask<WebResult> Fetch(string url, bool postMode, string postData = null)
		{
			try
			{
				var uri = new Uri(url);
				if (uri.IsFile)
					return new WebResult(await File.ReadAllBytesAsync(uri.LocalPath), null);

				using (var request = postMode
					? new HttpRequestMessage(HttpMethod.Post, uri) { Content = new StringContent(postData ?? "", Encoding.UTF8, "application/x-www-form-urlencoded") }
					: new HttpRequestMessage(HttpMethod.Get, uri))
				using (var response = await _httpClient.SendAsync(request))
				{
					if (!response.IsSuccessStatusCode)
						return new WebResult(null, $"{(int)response.StatusCode} {response.ReasonPhrase}");

					return new WebResult(await response.Content.ReadAsByteArrayAsync(), null);
				}
			}
			catch (Exception e)
			{
				return new WebResult(null, e.Message);
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

				if (string.IsNullOrEmpty(kv.Key))
					throw new ArgumentException("Query string key cannot be null or empty.", nameof(kv.Key));

				s.AppendFormat("{0}={1}", kv.Key, Uri.EscapeDataString(kv.Value.NullToEmpty()));
			}

			return s.ToString();
		}
	}
}
