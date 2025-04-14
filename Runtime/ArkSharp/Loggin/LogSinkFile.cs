using System;
using System.IO;
using System.Text;

namespace ArkSharp
{
	public static partial class LogSinkExtensions
	{
		/// <summary>
		/// ָ��������־�ļ�
		/// </summary>
		public static ILogSinkGroup ToFile(this ILogSinkGroup sinkGroup, string fileName)
		{
			sinkGroup.Add(new LogSinkFile(fileName));
			return sinkGroup;
		}

		/// <summary>
		/// ָ����־Ŀ¼/ǰ׺������"Ŀ¼/ǰ׺+ʱ���+��չ��"������־�ļ���
		/// </summary>
		public static ILogSinkGroup ToPath(this ILogSinkGroup sinkGroup, string rootPath, string dateTimeFormat = LogSinkFile.DefaultTimeFormat, string ext = LogSinkFile.DefaultFileExt)
		{
			sinkGroup.Add(new LogSinkFile(rootPath, dateTimeFormat, ext));
			return sinkGroup;
		}

		/// <summary>
		/// �����Ĭ����־Ŀ¼���ļ���Ϊʱ���+��չ��
		///   Unity�༭��: ����Ŀ¼/Logs/
		///   Unity����������Application.persistentDataPath/Logs/
		///   ������������ǰ����Ŀ¼/Logs/
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
