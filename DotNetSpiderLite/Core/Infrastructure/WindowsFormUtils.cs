﻿using System;
using System.Runtime.InteropServices;

namespace DotNetSpiderLite.Core.Infrastructure
{
	public static class WindowsFormUtils
	{
		[DllImport("User32.dll", EntryPoint = "FindWindow")]
		public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("User32.dll", EntryPoint = "FindWindowEx")]
		public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClassName, string lpWindowName);
		[DllImport("user32.dll", EntryPoint = "SendMessageA")]
		public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

		public const int WmClose = 0x10;
	}
}

