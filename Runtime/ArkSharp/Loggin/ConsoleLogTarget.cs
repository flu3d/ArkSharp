#if UNITY_STANDALONE_WIN || !UNITY_5_3_OR_NEWER

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ArkSharp
{
	public class ConsoleLogTarget : ILogTarget
	{
		public ConsoleLogTarget(int codePage = 0)
		{
			try
			{
				var mainWindow = GetActiveWindow();

				const int ATTACH_PARENT_PROCESS = -1;
				if (!AttachConsole(ATTACH_PARENT_PROCESS))
					AllocConsole();
				
				_oldOutput = Console.Out;

				_newOutput = new StreamWriter(Console.OpenStandardOutput(), Encoding.GetEncoding(codePage));
				_newOutput.AutoFlush = true;

				Console.SetOut(_newOutput);

				SendWindowToBack(GetConsoleWindow(), mainWindow);
			}
			catch (Exception)
			{
				_newOutput = null;
			}
		}

		public void Close()
		{
			lock (_syncObj)
			{
				try
				{
					if (_oldOutput != null)
						Console.SetOut(_oldOutput);

					FreeConsole();
				}
				catch (Exception)
				{
					// ignore errors
				}

				_oldOutput = null;
				_newOutput = null;
			}
		}

		public void Write(LogLevel level, string message)
		{
			lock (_syncObj)
			{
				switch (level)
				{
					case LogLevel.Trace:
					case LogLevel.Info: Console.ForegroundColor = ConsoleColor.Gray; break;
					case LogLevel.Warn: Console.ForegroundColor = ConsoleColor.Yellow; break;
					case LogLevel.Error: Console.ForegroundColor = ConsoleColor.Red; break;
					default:
						return;
				}

				Console.WriteLine(message);
				Console.ResetColor();
			}
		}

		private TextWriter _oldOutput = null;
		private StreamWriter _newOutput = null;
		private readonly object _syncObj = new object();

		private static void SendWindowToBack(IntPtr window, IntPtr oldWindow)
		{
			const short SWP_NOSIZE = 1;
			const short SWP_NOMOVE = 2;
			const short SWP_NOACTIVATE = 0x10;

			SetWindowPos(window, oldWindow.ToInt32(), 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool AttachConsole(int dwProcessId);

		[DllImport("Kernel32.dll", SetLastError = true)]
		private static extern bool AllocConsole();

		[DllImport("Kernel32.dll", SetLastError = true)]
		private static extern bool FreeConsole();

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr GetConsoleWindow();

		[DllImport("User32.dll", SetLastError = true)]
		private static extern IntPtr GetActiveWindow();

		[DllImport("User32.dll", SetLastError = true)]
		private static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
	}
}

#endif
