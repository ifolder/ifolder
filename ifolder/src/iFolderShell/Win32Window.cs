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

		/// <summary>
		/// Load the image from a file.
		/// </summary>
		public const int LR_LOADFROMFILE = 0x10;

		/// <summary>
		/// The image is an icon.
		/// </summary>
		public const int IMAGE_ICON = 1;

		/// <summary>
		/// The file object is a directory.
		/// </summary>
		public const int FILE_ATTRIBUTE_DIRECTORY = 0x10;

		/// <summary>
		/// Get icon.
		/// </summary>
		public const int SHGFI_ICON = 0x100;

		/// <summary>
		/// Use passed file attributes.
		/// </summary>
		public const int SHGFI_USEFILEATTRIBUTES = 0x10;

		/// <summary>
		/// Create a Win32Window
		/// </summary>
		public Win32Window()
		{
		}

		/// <summary>
		/// Set the window handle.
		/// </summary>
		public IntPtr Window
		{
			set
			{
				window = value;
			}
		}

		/// <summary>
		/// Get a long value for this window. See GetWindowLong()
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public int GetWindowLong(int index)
		{
			return GetWindowLong(window, index);
		}

		/// <summary>
		/// Set a long value for this window. See SetWindowLong()
		/// </summary>
		/// <param name="index"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public int SetWindowLong(int index, int value)
		{
			return SetWindowLong(window, index, value);
		}

		const int GWL_EXSTYLE = -20;
		const int WS_EX_TOOLWINDOW = 0x00000080;
		const int WS_EX_APPWINDOW = 0x00040000;

		/// <summary>
		/// Turn this window into a tool window, so it doesn't show up in the Alt-tab list...
		/// </summary>
		public void MakeToolWindow()
		{
			int windowStyle = GetWindowLong(GWL_EXSTYLE);
			SetWindowLong(GWL_EXSTYLE, windowStyle | WS_EX_TOOLWINDOW);
		}

		/// <summary>
		/// Make this a normal window.
		/// </summary>
		public void MakeNormalWindow()
		{
			int windowStyle = GetWindowLong(GWL_EXSTYLE);
			SetWindowLong(GWL_EXSTYLE, windowStyle & ~WS_EX_TOOLWINDOW);
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

		/// <summary>
		/// Display the properties dialog for an object.
		/// </summary>
		/// <param name="window"></param>
		/// <param name="type"></param>
		/// <param name="objectName"></param>
		/// <param name="pageName"></param>
		/// <returns></returns>
		public static bool ShObjectProperties(IntPtr window, int type, string objectName, string pageName)
		{
			return SHObjectProperties(window, type, objectName, pageName);
		}

		/// <summary>
		/// Get fileinfo for a file.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="attr"></param>
		/// <param name="fi"></param>
		/// <param name="cbfi"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public static IntPtr ShGetFileInfo(string path, int attr, out IFSHFILEINFO fi, int cbfi, int flags)
		{
			return SHGetFileInfo(path, attr, out fi, cbfi, flags);
		}

		/// <summary>
		/// An item has changed.
		/// </summary>
		public const int SHCNE_UPDATEITEM = 0x00002000;

		/// <summary>
		/// Path name
		/// </summary>
		public const int SHCNF_PATHW = 0x0005;

		/// <summary>
		/// Notify the shell that something has changed.
		/// </summary>
		/// <param name="wEventId"></param>
		/// <param name="flags"></param>
		/// <param name="dwItem1"></param>
		/// <param name="dwItem2"></param>
		public static void ShChangeNotify(int wEventId, int flags, string dwItem1, IntPtr dwItem2)
		{
			SHChangeNotify(wEventId, flags, dwItem1, dwItem2);
		}

		/// <summary>
		/// Loads an image from a file.
		/// </summary>
		/// <param name="hInst"></param>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="cx"></param>
		/// <param name="cy"></param>
		/// <param name="load"></param>
		/// <returns></returns>
		public static IntPtr LoadImageFromFile(int hInst, string name, int type, int cx, int cy, int load)
		{
			return LoadImage(hInst, name, type, cx, cy, load);
		}

		[DllImport("user32.dll")]
		static extern bool BringWindowToTop(IntPtr window);
		
		[DllImport("user32.dll", EntryPoint="FindWindow")]
		static extern IntPtr FindWindowWin32(string className, string windowName);

		[DllImport("user32.dll")]
		static extern int SetWindowLong(
			IntPtr window,
			int index,
			int value);

		[DllImport("user32.dll")]
		static extern int GetWindowLong(
			IntPtr window,
			int index);
		
		[DllImport("shell32.dll")]
		static extern bool SHObjectProperties(IntPtr window, int type, [MarshalAs(UnmanagedType.LPWStr)] string lpObject, [MarshalAs(UnmanagedType.LPWStr)] string lpPage);

		[DllImportAttribute("user32.dll")]
		static extern IntPtr LoadImage(int hInst, string name, int type, int cx, int cy, int load);

		[DllImport("shell32.dll")]
		static extern IntPtr SHGetFileInfo([MarshalAs(UnmanagedType.LPWStr)] string path, int attr, out IFSHFILEINFO fi, int cbfi, int flags);

		[DllImport("shell32.dll")]
		static extern void SHChangeNotify(int wEventId, int uFlags, [MarshalAs(UnmanagedType.LPWStr)] string dwItem1, IntPtr dwItem2);
	}
}

/// <summary>
/// File info structure.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
[ComVisible(false)]
public struct IFSHFILEINFO
{
	/// <summary>
	/// Handle to an icon.
	/// </summary>
	public IntPtr hIcon;

	/// <summary>
	/// Icon index.
	/// </summary>
	public int iIcon;

	/// <summary>
	/// Flags
	/// </summary>
	public int attributes;

	/// <summary>
	/// Display name or path.
	/// </summary>
	[MarshalAs(UnmanagedType.LPWStr, SizeConst=256)]
	public String displayName;

	/// <summary>
	/// Type name.
	/// </summary>
	[MarshalAs(UnmanagedType.LPWStr, SizeConst=80)]
	public String typeName;
}