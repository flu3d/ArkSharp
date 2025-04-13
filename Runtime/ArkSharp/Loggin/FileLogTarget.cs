using System;
using System.IO;
using System.Text;

namespace ArkSharp
{
	public class FileLogTarget : ILogTarget
	{
		public FileLogTarget(string prefix, bool withDateTime, string ext = ".log", Encoding encoding = null)
		{
			try
			{
				if (encoding == null)
					encoding = Encoding.UTF8;

				string fileName = $"{prefix}{DateTime.Now:yyyyMMdd_HHmmss}{ext}";
				Directory.CreateDirectory(Path.GetDirectoryName(fileName));

				_stream = File.Open(fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
				_writer = new StreamWriter(_stream, encoding);
				_writer.NewLine = "\n";
			}
			catch (Exception)
			{
				// Ignore errors
			}
		}

		public void Close()
		{
			lock (_syncObj)
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
		}

		public void Write(LogLevel level, string message)
		{
			if (_writer == null || _stream == null)
				return;

			lock (_syncObj)
			{
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
		}

		private FileStream _stream;
		private StreamWriter _writer;
		private readonly object _syncObj = new object();
	}
}
