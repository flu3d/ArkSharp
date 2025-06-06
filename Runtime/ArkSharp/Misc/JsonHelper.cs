﻿using System;
using Newtonsoft.Json;

namespace ArkSharp
{
	public static class JsonHelper
	{
		/// <summary>
		/// 对象序列化成JSON字符串
		/// </summary>
		public static string ToJson(this object obj, bool indented = false)
		{
			var settings = indented ? _jsonSettingsIdented : _jsonSettingsDefault;
			return JsonConvert.SerializeObject(obj, settings);
		}

		/// <summary>
		/// JSON字符串序列化成对象
		/// </summary>
		public static T ToObject<T>(this string json)
		{
			if (string.IsNullOrEmpty(json))
				return default;

			return JsonConvert.DeserializeObject<T>(json);
		}

		/// <summary>
		/// JSON字符串序列化成对象
		/// </summary>
		public static object ToObject(this string json, Type type)
		{
			if (string.IsNullOrEmpty(json))
				return default;

			return JsonConvert.DeserializeObject(json, type);
		}

		private static readonly JsonSerializerSettings _jsonSettingsDefault = new()
		{
			Formatting = Formatting.None,
			DateFormatString = "yyyyMMdd HH:mm:ss",
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			//ContractResolver = new JsonPropertyContractResolver(ignoreProperties);
		};

		private static readonly JsonSerializerSettings _jsonSettingsIdented = new()
		{
			Formatting = Formatting.Indented,
			DateFormatString = "yyyyMMdd HH:mm:ss",
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			//ContractResolver = new JsonPropertyContractResolver(ignoreProperties);
		};
	}
}
