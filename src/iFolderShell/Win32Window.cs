/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Novell.iFolder.Win32Util
{
	/// <summary>
	/// Encapsulates window functions that aren't in the framework.
	/// NOTE: This class is not thread-safe. 
	/// </summary>
	[ComVisible(false)]
	public class Win32Window
	{
		IntPtr window;

		public const int LR_LOADFROMFILE = 0x10;
		public const int IMAGE_ICON = 1;
		public const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
		public const int SHGFI_ICON = 0x100;
		public const int SHGFI_USEFILEATTRIBUTES = 0x10;

		/// <summary>
		/// Create a Win32Window
		/// </summary>
		/// <param name="window">The window handle</param>
		public Win32Window()
		{
		}

		public IntPtr Window
		{
			set
			{
				window = value;
			}
		}

		/// <summary>
		/// Bring a window to the top
		/// </summary>
		public void BringWindowToTop()
		{
			BringWindowToTop(window);
		}

		/// <summary>
		/// Find a window by name or class
		/// </summary>
		/// <param name="className">Name of the class, or null</param>
		/// <param name="windowName">Name of the window, or null</param>
		/// <returns></returns>
		public static Win32Window FindWindow(string className, string windowName)
		{
			IntPtr window = FindWindowWin32(className, windowName);
			Win32Window win32Window = null;
			if (window != IntPtr.Zero)
			{
				win32Window = new Win32Window();
				win32Window.Window = window;
			}

			return win32Window;
		}

		public static bool ShObjectProperties(IntPtr window, int type, string objectName, string pageName)
		{
			return SHObjectProperties(window, type, objectName, pageName);
		}

		public static IntPtr ShGetFileInfo(string path, int attr, out IFSHFILEINFO fi, int cbfi, int flags)
		{
			return SHGetFileInfo(path, attr, out fi, cbfi, flags);
		}

		public static IntPtr LoadImageFromFile(int hInst, string name, int type, int cx, int cy, int load)
		{
			return LoadImage(hInst, name, type, cx, cy, load);
		}

		[DllImport("user32.dll")]
		static extern bool BringWindowToTop(IntPtr window);
		
		[DllImport("user32.dll", EntryPoint="FindWindow")]
		static extern IntPtr FindWindowWin32(string className, string windowName);

		[DllImport("shell32.dll")]
		static extern bool SHObjectProperties(IntPtr window, int type, [MarshalAs(UnmanagedType.LPWStr)] string lpObject, [MarshalAs(UnmanagedType.LPWStr)] string lpPage);

		[DllImportAttribute("user32.dll")]
		static extern IntPtr LoadImage(int hInst, string name, int type, int cx, int cy, int load);

		[DllImport("shell32.dll")]
		static extern IntPtr SHGetFileInfo([MarshalAs(UnmanagedType.LPWStr)] string path, int attr, out IFSHFILEINFO fi, int cbfi, int flags);
	}
}

[StructLayout(LayoutKind.Sequential)]
[ComVisible(false)]
public struct IFSHFILEINFO
{
	public IntPtr hIcon;
	public int iIcon;
	public int attributes;
	[MarshalAs(UnmanagedType.LPWStr, SizeConst=256)]
	public String displayName;
	[MarshalAs(UnmanagedType.LPWStr, SizeConst=80)]
	public String typeName;
}