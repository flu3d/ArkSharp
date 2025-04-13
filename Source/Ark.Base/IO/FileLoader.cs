using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading.Tasks;

#if UNITY_5_3_OR_NEWER
using UnityEngine.Networking;
#endif

namespace Ark
{
	/// <summary>
	/// 原始文件加载，需要提供完整路径或路径转换器，不支持AssetBundle
	/// </summary>
	public class FileLoader
	{
		public class Request<T> : AsyncOp<T>
		{
			public override T Result => _result;
			public override bool IsCompleted => _completed;
			public string Error { get; private set; }
			public string filePath { get; }

			internal Request(string filePath) => this.filePath = filePath;
			internal void SetResult(T obj) { _result = obj; _completed = true; }
			internal void SetError(string error) { Error = error; _completed = true; }

			private T _result;
			private bool _completed;
		}

		public Request<string> LoadText(string path)
		{
			var req = new Request<string>(path);

			if (_pathResolver != null)
				path = _pathResolver.Invoke(path);

#if UNITY_5_3_OR_NEWER
			// 如果是从jar或其他url读取则还是走www加载
			if (PathHelper.IsURL(path))
				_scheduler.Start(LoadWithUnityWWW(path, req, _wwwReadText));
			else
				_scheduler.Start(LoadWithFileIO(path, req, _fileReadText));
#else
			_scheduler.Start(LoadWithFileIO(path, req, _fileReadText));
#endif

			return req;
		}

		public Request<byte[]> LoadBytes(string path)
		{
			var req = new Request<byte[]>(path);

			if (_pathResolver != null)
				path = _pathResolver.Invoke(path);

#if UNITY_5_3_OR_NEWER
			// 如果是从jar或其他url读取则还是走www加载
			if (PathHelper.IsURL(path))
				_scheduler.Start(LoadWithUnityWWW(path, req, _wwwReadBytes));
			else
				_scheduler.Start(LoadWithFileIO(path, req, _fileReadBytes));
#else
			_scheduler.Start(LoadWithFileIO(path, req, _fileReadBytes));
#endif

			return req;
		}

#if UNITY_5_3_OR_NEWER
		private static Func<DownloadHandler, string> _wwwReadText = handler => handler.text;
		private static Func<DownloadHandler, byte[]> _wwwReadBytes = handler => handler.data;

		private static IEnumerator LoadWithUnityWWW<T>(string rawPath, Request<T> req, Func<DownloadHandler, T> funcReader)
		{
			var url = PathHelper.Path2URL(rawPath);
			using (var www = UnityWebRequest.Get(url))
			{
				var wwwreq = www.SendWebRequest();
				while (!wwwreq.isDone)
					yield return null;

				if (www.result != UnityWebRequest.Result.Success)
					req.SetError(www.error);
				else
					req.SetResult(funcReader(www.downloadHandler));
			}
		}
#endif

		private static Func<string, string> _fileReadText = path => File.ReadAllText(path, Encoding.UTF8);
		private static Func<string, byte[]> _fileReadBytes = path => File.ReadAllBytes(path);

		private static IEnumerator LoadWithFileIO<T>(string rawPath, Request<T> req, Func<string, T> funcReader)
		{
			var task = Task.Run(() => funcReader(rawPath));
			yield return task;

			try
			{
				req.SetResult(task.Result);
			}
			catch (AggregateException ee)
			{
				var s = new StringBuilder();
				foreach (var e in ee.InnerExceptions)
					s.Append($"{e.Message}\n{e.StackTrace}\n");

				req.SetError(s.ToString());
			}
		}

		private readonly CoroutineScheduler _scheduler;
		private Func<string, string> _pathResolver;

		public void SetPathResolver(Func<string, string> resolver) => _pathResolver = resolver;
		public Func<string, string> GetPathResolver() => _pathResolver;

		public FileLoader() => _scheduler = CoroutineScheduler.Default;
		public FileLoader(CoroutineScheduler scheduler) => _scheduler = scheduler ?? CoroutineScheduler.Default;

		public static FileLoader Default => Singleton.Get<FileLoader>();
	}
}
