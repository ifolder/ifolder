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
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Novell.Win32Util
{
	/// <summary>
	/// Encapsulates window functions that aren't in the framework.
	/// NOTE: This class is not thread-safe. 
	/// </summary>
	[ComVisible(false)]
	public class Win32Window
	{
		private IntPtr handle;

		#region Win32 API
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
		/// An item has changed.
		/// </summary>
		public const int SHCNE_UPDATEITEM = 0x00002000;

		/// <summary>
		/// Path name
		/// </summary>
		public const int SHCNF_PATHW = 0x0005;

		const int WM_USER = 0x0400;

		/// <summary>
		/// Sets the disabled image list for a toolbar.
		/// </summary>
		public static readonly int TB_SETDISABLEDIMAGELIST = WM_USER + 54;

		/// <summary>
		/// Sets the hot image list for a toolbar.
		/// </summary>
		public static readonly int TB_SETHOTIMAGELIST = WM_USER + 52;

		const int GWL_EXSTYLE = -20;
		const int WS_EX_TOOLWINDOW = 0x00000080;
		const int WS_EX_APPWINDOW = 0x00040000;

		const int SW_HIDE = 0;
		const int SW_SHOWNORMAL = 1;
		const int SW_RESTORE = 9;

		[ComVisible(false)]
		private struct ICONINFO
		{
			/// <summary>
			/// Icon or cursor.
			/// </summary>
			public bool fIcon;

			/// <summary>
			/// x-coordinate of cursor hotspot.
			/// </summary>
			public int xHotspot;

			/// <summary>
			/// y-coordinate of cursor hotspot.
			/// </summary>
			public int yHotspot;

			/// <summary>
			/// Icon bitmask bitmap.
			/// </summary>
			public IntPtr hbmMask;

			/// <summary>
			/// Handle to icon color bitmap.
			/// </summary>
			public IntPtr hbmColor;
		}

		[DllImport("user32.dll")]
		static extern bool BringWindowToTop(IntPtr hwnd);

		[DllImport("user32.dll", SetLastError=true)]
		static extern int DestroyIcon(IntPtr hIcon);

		[DllImport("user32.dll", EntryPoint="FindWindow")]
		static extern IntPtr FindWindowWin32(string className, string windowName);

		[DllImport("user32.dll")]
		static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO piconinfo);

		[DllImport("user32.dll")]
		static extern int GetWindowLong(IntPtr hwnd, int index);

		[DllImportAttribute("user32.dll")]
		static extern IntPtr LoadImage(int hInst, string name, int type, int cx, int cy, int load);

		[DllImport("user32.dll")]
		public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		static extern int SetWindowLong(IntPtr hwnd, int index,	int dwNewLong);

		[DllImport("shell32.dll")]
		static extern void SHChangeNotify(int wEventId, int uFlags, [MarshalAs(UnmanagedType.LPWStr)] string dwItem1, IntPtr dwItem2);

		[DllImport("shell32.dll")]
		static extern IntPtr SHGetFileInfo([MarshalAs(UnmanagedType.LPWStr)] string path, int attr, out IFSHFILEINFO fi, int cbfi, int flags);

		[DllImport("shell32.dll")]
		static extern bool SHObjectProperties(IntPtr hwnd, int type, [MarshalAs(UnmanagedType.LPWStr)] string lpObject, [MarshalAs(UnmanagedType.LPWStr)] string lpPage);

		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
		#endregion

		/// <summary>
		/// Create a Win32Window
		/// </summary>
		public Win32Window()
		{
		}

		#region Properties
		/// <summary>
		/// Set the window handle.
		/// </summary>
		public IntPtr Handle
		{
			set
			{
				handle = value;
			}
		}

		public bool Visible
		{
			set
			{
				if (value)
				{
					ShowWindow(handle, SW_SHOWNORMAL | SW_RESTORE);
					SetForegroundWindow(handle);
				}
				else
				{
					ShowWindow(handle, SW_HIDE);
				}
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Get a long value for this window. See GetWindowLong()
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public int GetWindowLong(int index)
		{
			return GetWindowLong(handle, index);
		}

		/// <summary>
		/// Set a long value for this window. See SetWindowLong()
		/// </summary>
		/// <param name="index"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public int SetWindowLong(int index, int dwNewLong)
		{
			return SetWindowLong(handle, index, dwNewLong);
		}

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
		public bool BringWindowToTop()
		{
			return BringWindowToTop(handle);
		}

		/// <summary>
		/// Find a window by name or class
		/// </summary>
		/// <param name="className">Name of the class, or null</param>
		/// <param name="windowName">Name of the window, or null</param>
		/// <returns></returns>
		public static Win32Window FindWindow(string className, string windowName)
		{
			IntPtr handle = FindWindowWin32(className, windowName);
			Win32Window win32Window = null;
			if (handle != IntPtr.Zero)
			{
				win32Window = new Win32Window();
				win32Window.Handle = handle;
			}

			return win32Window;
		}

		/// <summary>
		/// Display the properties dialog for an object.
		/// </summary>
		/// <param name="handle"></param>
		/// <param name="type"></param>
		/// <param name="objectName"></param>
		/// <param name="pageName"></param>
		/// <returns></returns>
		public static bool ShObjectProperties(IntPtr handle, int type, string objectName, string pageName)
		{
			return SHObjectProperties(handle, type, objectName, pageName);
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

		/// <summary>
		/// Gets rid of the shadow on an icon.  Thanks to Mick Doherty (http://dotnetrix.co.uk)
		/// </summary>
		/// <param name="ico"></param>
		/// <returns></returns>
		public static Bitmap IconToAlphaBitmap(Icon ico)
		{
			ICONINFO ii = new ICONINFO();
			GetIconInfo(ico.Handle, out ii);
			Bitmap bmp = Bitmap.FromHbitmap(ii.hbmColor);
			DestroyIcon(ii.hbmColor);
			DestroyIcon(ii.hbmMask);

			if (Bitmap.GetPixelFormatSize(bmp.PixelFormat) < 32)
				return ico.ToBitmap();

			BitmapData bmData;
			Rectangle bmBounds = new Rectangle(0,0,bmp.Width,bmp.Height);

			bmData = bmp.LockBits(bmBounds,ImageLockMode.ReadOnly, bmp.PixelFormat);

			Bitmap dstBitmap=new Bitmap(bmData.Width, bmData.Height, bmData.Stride, PixelFormat.Format32bppArgb, bmData.Scan0);

			bool IsAlphaBitmap = false;

			for (int y=0; y <= bmData.Height-1; y++)
			{
				for (int x=0; x <= bmData.Width-1; x++)
				{
					Color PixelColor = Color.FromArgb(Marshal.ReadInt32(bmData.Scan0, (bmData.Stride * y) + (4 * x)));
					if (PixelColor.A > 0 & PixelColor.A < 255)
					{
						IsAlphaBitmap = true;
						break;
					}
				}
				if (IsAlphaBitmap) break;
			}

			bmp.UnlockBits(bmData);

			if (IsAlphaBitmap==true)
				return new Bitmap(dstBitmap);
			else
				return new Bitmap(ico.ToBitmap());
		}
		#endregion
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