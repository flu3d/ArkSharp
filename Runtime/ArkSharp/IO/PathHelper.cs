using System.IO;

namespace ArkSharp
{
	/// <summary>
	/// 路径处理工具箱
	/// </summary>
	public static class PathHelper
	{
		/// <summary>
		/// 统一使用/符号规范化路径
		/// </summary>
		public static string Normalize(string path)
		{
			if (string.IsNullOrEmpty(path))
				return path;

			return path.Replace('\\', '/');
		}

		/// <summary>
		/// 获取全路径并进行规范化
		/// </summary>
		public static string GetFullPath(string path)
		{
			if (string.IsNullOrEmpty(path))
				return path;

			path = Path.GetFullPath(path);
			return Normalize(path);
		}

		/// <summary>
		/// 路径转URL
		/// </summary>
		public static string Path2URL(string path)
		{
			if (string.IsNullOrEmpty(path))
				return path;

			if (IsURL(path))
				return path;

			path = Normalize(path);

			{
				// Use URL instead of Path for Android APK
				// "/data/app/com.xxx.xxx-1/base.apk!assets/" => "jar:file:///data/app/com.xx.xxx-1/base.apk!/assets/"
				int pos = path.IndexOf(".apk!");
				if (pos > 0)
				{
					if (path[pos + 5] != '/')
						path = path.Insert(pos + 5, "/");

					return "jar:file://" + path;
				}
			}

			if (path.StartsWith("/"))
				return "file://" + path;
			else
				return "file:///" + path;
		}

		public static bool IsURL(string path)
		{
			if (string.IsNullOrEmpty(path))
				return false;

			return 
				path.StartsWith("jar:") ||
				path.StartsWith("http:") ||
				path.StartsWith("https:") ||
				path.StartsWith("file:");
		}
	}
}
