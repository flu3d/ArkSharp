using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Cysharp.Threading.Tasks;

namespace ArkSharp
{
	/// <summary>
	/// 原始文件加载，需要提供完整路径
	/// </summary>
	public static class FileHelper
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async UniTask<string> ReadText(string path)
		{
			var bytes = await ReadBytes(path);
			return Encoding.UTF8.GetString(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async UniTask<byte[]> ReadBytes(string path)
		{
			// Unity 版本使用 UniTask 的线程池封装；.NET 版本不包含该 API。
#if UNITY_5_3_OR_NEWER
			return await UniTask.RunOnThreadPool(() => File.ReadAllBytes(path));
#else
			return await File.ReadAllBytesAsync(path);
#endif
		}
	}
}
