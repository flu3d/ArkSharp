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
		public static async UniTask<string> Get(string path)
		{
			var bytes = await GetBytes(path);
			return Encoding.UTF8.GetString(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UniTask<byte[]> GetBytes(string path)
		{
			// 如果是从jar或其他url读取则还是走web加载
			if (PathHelper.IsURL(path))
				return WebHelper.GetBytes(path);

			// 使用后台线程池加载文件
			return UniTask.RunOnThreadPool(() => File.ReadAllBytes(path));
		}
	}
}
