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

namespace CustomUIControls
{
	/// <summary>
	/// Summary description for NotifyIconBalloonTip.
	/// </summary>
	public class NotifyIconBalloonTip
	{
		/// <summary>
		/// Constructs a NotifyIconBalloonTip object.
		/// </summary>
		public NotifyIconBalloonTip()
		{
		}

		#region Win32 API
		// from ShellAPI.h
		private const int NIF_MESSAGE = 0x01;
		private const int NIF_ICON = 0x02;
		private const int NIF_TIP = 0x04;
		private const int NIF_STATE = 0x08;
		private const int NIF_INFO = 0x10;

		private const int NIM_ADD = 0x00;
		private const int NIM_MODIFY = 0x01;
		private const int NIM_DELETE = 0x02;
		private const int NIM_SETFOCUS = 0x03;
		private const int NIM_SETVERSION = 0x04;

		[DllImport("shell32.dll")]
		private static extern bool Shell_NotifyIcon (int dwMessage,	ref NOTIFYICONDATA lpData);

		[StructLayout(LayoutKind.Sequential)]
		private struct NOTIFYICONDATA 
		{
			internal int cbSize;
			internal IntPtr hwnd;
			internal int uID;
			internal int uFlags;
			internal int uCallbackMessage;
			internal IntPtr hIcon;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
			internal string szTip;
			internal int dwState;
			internal int dwStateMask;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			internal string szInfo;
			internal int uTimeoutAndVersion;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
			internal string szInfoTitle;
			internal BalloonType dwInfoFlags;
		}
		#endregion

		/// <summary>
		/// Displays a balloon-style tool tip window for the notification tray icon.
		/// </summary>
		/// <param name="hwnd">The window handle for the notification tray icon.</param>
		/// <param name="ID">The ID of the icon.</param>
		/// <param name="type">The type of message to display.</param>
		/// <param name="title">The string to display as the title.</param>
		/// <param name="body">The string to display as the message body.</param>
		public void ShowBalloon(IntPtr hwnd, int ID, BalloonType type, string title, string body)
		{
			NOTIFYICONDATA nData = new NOTIFYICONDATA();

			nData.cbSize = Marshal.SizeOf(nData);
			nData.hwnd = hwnd;
			nData.uID = ID;
			nData.uFlags = NIF_INFO;// | NIF_MESSAGE;
			// TODO: need to investigate why the callback doesn't work ... it may on work with NIM_ADD.
			//nData.uCallbackMessage = 0xbd1;
			nData.uTimeoutAndVersion = 10 * 1000;
			nData.dwInfoFlags = type;

			nData.szInfoTitle = title;
			nData.szInfo = body;

			Shell_NotifyIcon(NIM_MODIFY, ref nData);
		}
	}
}
