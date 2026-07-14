using System;
using System.IO;
using System.Text;

namespace ArkSharp
{
	public static partial class LogSinkExtensions
	{
		/// <summary>
		/// 指定具体日志文件
		/// </summary>
		public static ILogSinkGroup ToFile(this ILogSinkGroup sinkGroup, string fileName)
		{
			sinkGroup.Add(new LogSinkFile(fileName));
			return sinkGroup;
		}

		/// <summary>
		/// 指定日志目录/前缀，允许按"目录/前缀+时间戳+扩展名"定制日志文件名
		/// </summary>
		public static ILogSinkGroup ToPath(this ILogSinkGroup sinkGroup, string rootPath, string dateTimeFormat = LogSinkFile.DefaultTimeFormat, string ext = LogSinkFile.DefaultFileExt)
		{
			sinkGroup.Add(new LogSinkFile(rootPath, dateTimeFormat, ext));
			return sinkGroup;
		}

		/// <summary>
		/// 输出到默认日志目录，文件名为时间戳+扩展名
		///   Unity编辑器: 工程目录/Logs/
		///   Unity发布环境：Application.persistentDataPath/Logs/
		///   其他环境：当前工作目录/Logs/
		/// </summary>
		public static ILogSinkGroup ToDefaultPath(this ILogSinkGroup sinkGroup)
		{
			string rootPath;

#if UNITY_EDITOR
			rootPath = UnityEngine.Application.dataPath + "/../Logs/";
#elif UNITY_5_3_OR_NEWER
			rootPath = UnityEngine.Application.persistentDataPath + "/../Logs/";
#else
			rootPath = "Logs/";
#endif

			return sinkGroup.ToPath(Path.GetFullPath(rootPath));
		}
	}

	public class LogSinkFile : ILogSink
	{
		public const string DefaultTimeFormat = "yyyyMMdd_HHmmss";
		public const string DefaultFileExt = ".log";

		public LogSinkFile(string fileName)
		{
			OpenFile(fileName);
		}

		public LogSinkFile(string rootPath, string dateTimeFormat, string ext)
		{
			string fileName = rootPath + DateTime.Now.ToString(dateTimeFormat) + ext;
			OpenFile(fileName);
		}

		private void OpenFile(string fileName)
		{
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(fileName));

				_stream = File.Open(fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
				_writer = new StreamWriter(_stream, Encoding.UTF8);
				_writer.NewLine = "\n";
			}
			catch (Exception)
			{
				// Ignore errors
			}
		}

		public void Dispose()
		{
			try
			{
				if (_writer != null)
				{
					_writer.Close();
					_writer = null;
				}

				if (_stream != null)
				{
					_stream.Close();
					_stream = null;
				}
			}
			catch (Exception)
			{
				// ignore errors
			}
		}

		public void Write(LogLevel level, string message)
		{
			if (_writer == null || _stream == null)
				return;

			try
			{
				_writer.WriteLine(message);
				_writer.Flush();
				_stream.Flush();
			}
			catch (Exception)
			{
				// ignore errors
			}
		}

		private FileStream _stream;
		private StreamWriter _writer;
	}
}
