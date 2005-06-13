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
using System.Runtime.InteropServices;

namespace Novell.Win32Util
{
	/// <summary>
	/// Summary description for Win32.
	/// </summary>
	[ComVisible(false)]
	public class Win32
	{
		/// <summary>
		/// 
		/// </summary>
		public const int LVS_REPORT = 0x0001;
		/// <summary>
		/// 
		/// </summary>
		public const int LVS_SHOWSELALWAYS = 0x0008;
		/// <summary>
		/// 
		/// </summary>
		public const int LVS_OWNERDATA = 0x1000;

		/// <summary>
		/// 
		/// </summary>
		public const int LVIF_TEXT = 0x0001;
		/// <summary>
		/// 
		/// </summary>
		public const int LVIF_IMAGE = 0x0002;
		/// <summary>
		/// 
		/// </summary>
		public const int LVIF_STATE = 0x0008;
		/// <summary>
		/// 
		/// </summary>
		public const int LVIF_INDENT = 0x0010;

		/// <summary>
		/// 
		/// </summary>
		public const int WM_DESTROY = 0x0002;
		/// <summary>
		/// 
		/// </summary>
		public const int WM_NOTIFY = 0x004E;

		/// <summary>
		/// 
		/// </summary>
		public const int LVN_FIRST = -100;
		/// <summary>
		/// 
		/// </summary>
		public const int LVN_ITEMCHANGED = LVN_FIRST-1;
		/// <summary>
		/// 
		/// </summary>
		public const int LVN_ODCACHEHINT = LVN_FIRST-13;
		/// <summary>
		/// 
		/// </summary>
		public const int LVN_GETDISPINFOW = LVN_FIRST - 77;
		/// <summary>
		/// 
		/// </summary>
		public const int LVN_ODFINDITEMW = LVN_FIRST-79;

		/// <summary>
		/// 
		/// </summary>
		public const int LVM_FIRST = 0x1000;
		/// <summary>
		/// 
		/// </summary>
		public const int LVM_GETITEMCOUNT = LVM_FIRST + 4;
		/// <summary>
		/// 
		/// </summary>
		public const int LVM_DELETEALLITEMS = LVM_FIRST + 9;
		/// <summary>
		/// 
		/// </summary>
		public const int LVM_GETNEXTITEM = LVM_FIRST + 12;
		/// <summary>
		/// 
		/// </summary>
		public const int LVM_GETITEMRECT = LVM_FIRST + 14;
		/// <summary>
		/// 
		/// </summary>
		public const int LVM_GETTOPINDEX = LVM_FIRST + 39;
		/// <summary>
		/// 
		/// </summary>
		public const int LVM_SETITEMCOUNT = LVM_FIRST + 47;
		/// <summary>
		/// 
		/// </summary>
		public const int LVM_GETSELECTEDCOUNT = LVM_FIRST + 50;
		/// <summary>
		/// 
		/// </summary>
		public const int LVM_GETITEM = LVM_FIRST + 75;

		/// <summary>
		/// 
		/// </summary>
		public const int LVNI_SELECTED = 0x0002;

		/// <summary>
		/// 
		/// </summary>
		public const int LVSICF_NOINVALIDATEALL = 0x00000001;
		/// <summary>
		/// 
		/// </summary>
		public const int LVSICF_NOSCROLL = 0x00000002;

		#region Send message prototypes

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hWnd"></param>
		/// <param name="msg"></param>
		/// <param name="wParam"></param>
		/// <param name="lParam"></param>
		/// <returns></returns>
		[DllImport("User32.dll",CharSet = CharSet.Auto,SetLastError=true)]
		public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hWnd"></param>
		/// <param name="msg"></param>
		/// <param name="wParam"></param>
		/// <param name="lParam"></param>
		/// <returns></returns>
		[DllImport("User32.dll", SetLastError=true)]
		public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hWnd"></param>
		/// <param name="msg"></param>
		/// <param name="wParam"></param>
		/// <param name="lParam"></param>
		/// <returns></returns>
		[DllImport("User32.dll",CharSet = CharSet.Auto, SetLastError=true)]
		public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hWnd"></param>
		/// <param name="msg"></param>
		/// <param name="wParam"></param>
		/// <param name="lvi"></param>
		/// <returns></returns>
		[DllImport("User32.dll", CharSet=CharSet.Auto, SetLastError=true)]
		public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, ref LVITEM lvi);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hWnd"></param>
		/// <param name="msg"></param>
		/// <param name="wParam"></param>
		/// <param name="prc"></param>
		/// <returns></returns>
		[DllImport("User32.dll", CharSet=CharSet.Auto, SetLastError=true)]
		public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, ref RECT prc);

		#endregion

		#region Structs

		/// <summary>
		/// 
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		[ComVisible(false)]
		public struct LVDISPINFOW 
		{
			/// <summary>
			/// 
			/// </summary>
			public NMHDR hdr;
			/// <summary>
			/// 
			/// </summary>
			public LVITEM item;
		}

		/// <summary>
		/// 
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		[ComVisible(false)]
		public struct LVFINDINFO
		{
			/// <summary>
			/// 
			/// </summary>
			public uint flags;
			/// <summary>
			/// 
			/// </summary>
			[MarshalAs(UnmanagedType.LPWStr)]
			public string psz;
			/// <summary>
			/// 
			/// </summary>
			public IntPtr lParam;
			/// <summary>
			/// 
			/// </summary>
			public POINT pt;
			/// <summary>
			/// 
			/// </summary>
			public uint vkDirection;
		}

		/// <summary>
		/// 
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		[ComVisible(false)]
		public struct POINT
		{
			/// <summary>
			/// 
			/// </summary>
			public int x;
			/// <summary>
			/// 
			/// </summary>
			public int y;
		}

		/// <summary>
		/// 
		/// </summary>
		[StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]
		[ComVisible(false)]
		public struct LVITEM
		{
			/// <summary>
			/// 
			/// </summary>
			public uint mask;
			/// <summary>
			/// 
			/// </summary>
			public int iItem;
			/// <summary>
			/// 
			/// </summary>
			public int iSubItem;
			/// <summary>
			/// 
			/// </summary>
			public uint state;
			/// <summary>
			/// 
			/// </summary>
			public uint stateMask;
			/// <summary>
			/// 
			/// </summary>
			public IntPtr pszText;
			/// <summary>
			/// 
			/// </summary>
			public int cchTextMax;
			/// <summary>
			/// 
			/// </summary>
			public int iImage;
			/// <summary>
			/// 
			/// </summary>
			public IntPtr lParam;
			/// <summary>
			/// 
			/// </summary>
			public int iIndent;
			/// <summary>
			/// 
			/// </summary>
			public int iGroupId;
			/// <summary>
			/// 
			/// </summary>
			public uint cColumns;
			/// <summary>
			/// 
			/// </summary>
			public IntPtr puColumns;
		}

		/// <summary>
		/// 
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		[ComVisible(false)]
		public struct NMHDR
		{
			/// <summary>
			/// 
			/// </summary>
			public IntPtr hwndFrom;
			/// <summary>
			/// 
			/// </summary>
			public int idFrom;
			/// <summary>
			/// 
			/// </summary>
			public int code;
		}

		/// <summary>
		/// 
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		[ComVisible(false)]
		public struct NMLVCACHEHINT
		{
			/// <summary>
			/// 
			/// </summary>
			public NMHDR hdr;
			/// <summary>
			/// 
			/// </summary>
			public int iFrom;
			/// <summary>
			/// 
			/// </summary>
			public int iTo;
		}

		/// <summary>
		/// 
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		[ComVisible(false)]
		public struct NMLVFINDITEM
		{
			/// <summary>
			/// 
			/// </summary>
			public NMHDR hdr;
			/// <summary>
			/// 
			/// </summary>
			public int iStart;
			/// <summary>
			/// 
			/// </summary>
			public LVFINDINFO lvfi;
		}

		/// <summary>
		/// 
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		[ComVisible(false)]
		public struct NMLISTVIEW
		{
			/// <summary>
			/// 
			/// </summary>
			public NMHDR   hdr;
			/// <summary>
			/// 
			/// </summary>
			public int     iItem;
			/// <summary>
			/// 
			/// </summary>
			public int     iSubItem;
			/// <summary>
			/// 
			/// </summary>
			public uint    uNewState;
			/// <summary>
			/// 
			/// </summary>
			public uint    uOldState;
			/// <summary>
			/// 
			/// </summary>
			public uint    uChanged;
			/// <summary>
			/// 
			/// </summary>
			public POINT   ptAction;
			/// <summary>
			/// 
			/// </summary>
			public IntPtr  lParam;
		}

		/// <summary>
		/// 
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		[ComVisible(false)]
		public struct RECT
		{
			/// <summary>
			/// 
			/// </summary>
			public int left;
			/// <summary>
			/// 
			/// </summary>
			public int top;
			/// <summary>
			/// 
			/// </summary>
			public int right;
			/// <summary>
			/// 
			/// </summary>
			public int bottom;
		}

		#endregion
	}
}
