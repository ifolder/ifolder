/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Bruce Getter <bgetter@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Novell.CustomUIControls
{
	/// <summary>
	/// ShellNotifyIcon class.
	/// </summary>
	public class ShellNotifyIcon
	{
		#region Class Members
		internal uint shellRestart;
		private string text;
		private Icon icon;
		private ContextMenu contextMenu;
		private bool visible = false;
		private readonly NotifyMessageLoop messageLoop = null;
		private readonly IntPtr messageLoopHandle = IntPtr.Zero;
  		private IntPtr handle = IntPtr.Zero;
		#endregion

		/// <summary>
		/// Constructs a ShellNotifyIcon object.
		/// </summary>
		/// <param name="handle">The window handle of the form that the notification icon belongs to.</param>
		public ShellNotifyIcon(IntPtr handle)
		{
			this.handle = handle;
			WM_NOTIFY_TRAY += 1;
			uID += 1;
			messageLoop = new NotifyMessageLoop(this);
			messageLoopHandle = messageLoop.Handle;

			messageLoop.Click += new Novell.CustomUIControls.NotifyMessageLoop.ClickDelegate(messageLoop_Click);
			messageLoop.DoubleClick += new Novell.CustomUIControls.NotifyMessageLoop.DoubleClickDelegate(messageLoop_DoubleClick);
			messageLoop.BalloonClick += new Novell.CustomUIControls.NotifyMessageLoop.BalloonClickDelegate(messageLoop_BalloonClick);
			messageLoop.ContextMenuPopup += new Novell.CustomUIControls.NotifyMessageLoop.ContextMenuPopupDelegate(messageLoop_ContextMenuPopup);
		}

        /// <summary>
        /// Destructor
        /// </summary>
		~ ShellNotifyIcon()
		{
			deleteNotifyIcon();
		}

		#region Win32 API
		internal readonly int WM_NOTIFY_TRAY = 0x0400 + 1000;
		internal readonly int uID = 1000;

		// From ShellAPI.h
		private static readonly int NIF_MESSAGE = 0x01;
		private static readonly int NIF_ICON = 0x02;
		private static readonly int NIF_TIP = 0x04;
		private static readonly int NIF_INFO = 0x10;

		private static readonly int NIM_ADD = 0x00;
		private static readonly int NIM_MODIFY = 0x01;
		private static readonly int NIM_DELETE = 0x02;
		//private static readonly int NIM_SETVERSION = 0x04;

		//private static readonly int NOTIFYICON_VERSION = 3;

		[DllImport("shell32.dll")]
		private static extern bool Shell_NotifyIcon(int dwMessage,	ref NOTIFYICONDATA lpData);

		/// <summary>
		/// Puts the thread that created the specified window into the foreground and activates the window.
		/// </summary>
		/// <param name="hwnd">The handle of the window to activate.</param>
		[DllImport("user32.dll")]
		public static extern bool SetForegroundWindow(IntPtr hwnd);

		[DllImport("user32.dll")]
		private static extern uint RegisterWindowMessage(string lpString);

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

		#region Events
		/// <summary>
		/// Click delegate.
		/// </summary>
		public delegate void ClickDelegate(object sender, EventArgs e);
		/// <summary>
		/// Occurs when the user clicks 
		/// (1) the icon in the status notification area of the taskbar, or
		/// (2) a balloon tip associated with the icon.
		/// </summary>
		public event ClickDelegate Click;

		/// <summary>
		/// Double-click delegate.
		/// </summary>
		public delegate void DoubleClickDelegate(object sender, EventArgs e);
		/// <summary>
		/// Occurs when the user double-clicks the icon in the status notification area of the taskbar.
		/// </summary>
		public event DoubleClickDelegate DoubleClick;

		/// <summary>
		/// Balloon click delegate.
		/// </summary>
		public delegate void BalloonClickDelegate(object sender, EventArgs e);
		/// <summary>
		/// Occurs when the user clicks 
		/// (1) the icon in the status area while a balloon tooltip is displayed, or
		/// (2) the balloon tooltip.
		/// </summary>
		public event BalloonClickDelegate BalloonClick;

		/// <summary>
		/// ContextMenu popup delegate.
		/// </summary>
		public delegate void ContextMenuPopupDelegate(object sender, EventArgs e);
		/// <summary>
		/// Occurs before the shortcut menu is displayed.
		/// </summary>
		public event ContextMenuPopupDelegate ContextMenuPopup;
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the shortcut menu for the icon.
		/// </summary>
		public ContextMenu ContextMenu
		{
			get { return contextMenu; }
			set	{ contextMenu = value; }
		}

		/// <summary>
		/// Gets the window handle that the control is bound to.
		/// </summary>
		public IntPtr Handle
		{
			get { return handle; }
		}

		/// <summary>
		/// Gets or sets the current icon.
		/// </summary>
		public Icon Icon
		{
			get { return icon; }
			set
			{
				if (icon == null)
				{
					// Add the notify icon.
					icon = value;
					visible = addNotifyIcon(text);
				}
				else
				{
					// Modify the notify icon.
					icon = value;
					modifyNotifyIcon(NIF_ICON, null, null, null, BalloonType.None);
				}
			}
		}

		/// <summary>
		/// Gets or sets the ToolTip text for the notify icon.
		/// </summary>
		public string Text
		{
			get { return text; }
			set 
			{
				if (icon != null)
				{
					// Modify the notify icon if it exists.
					if (modifyNotifyIcon(NIF_ICON | NIF_TIP, value, null, null, BalloonType.None))
					{
						text = value;
					}
				}
				else
				{
					// The icon hasn't been set yet, just save the text for now.
					text = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the icon is visible in the status notification area of the taskbar.
		/// </summary>
		public bool Visible
		{
			get { return visible; }
			set
			{
				if (!value.Equals(visible) && (icon != null))
				{
					if (value)
					{
						visible = addNotifyIcon(text);
					}
					else
					{
						deleteNotifyIcon();
						visible = false;
					}
				}
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		public void Dispose()
		{
			deleteNotifyIcon();
		}

		/// <summary>
		/// Displays a balloon ToolTip for the notification icon.
		/// </summary>
		/// <param name="InfoTitle">The title for the balloon ToolTip.</param>
		/// <param name="Info">The text for the balloon ToolTip.</param>
		/// <param name="BalloonType">A flag to add an icon to a balloon ToolTip.</param>
		public void DisplayBalloonTooltip(string InfoTitle, string Info, BalloonType BalloonType)
		{
			modifyNotifyIcon(NIF_ICON | NIF_INFO, text, InfoTitle, Info, BalloonType);
		}
		#endregion

		#region Private Methods
        /// <summary>
        /// getNOTIFYICONDATA
        /// </summary>
        private NOTIFYICONDATA getNOTIFYICONDATA(IntPtr iconHwnd, int flags, string tip, string infoTitle, string info, BalloonType balloonType)
		{
			NOTIFYICONDATA data = new NOTIFYICONDATA();

			data.cbSize = Marshal.SizeOf(data);
			data.hwnd = messageLoopHandle;
			data.uID = uID;
			data.uFlags = flags;
			data.uCallbackMessage = WM_NOTIFY_TRAY;
			data.hIcon = iconHwnd;
			data.uTimeoutAndVersion = 10 * 1000;
			data.dwInfoFlags = balloonType;

			data.szTip = tip;
			data.szInfoTitle = infoTitle;
			data.szInfo = info;

			return data;
		}

        /// <summary>
        /// addNotifyIcon
        /// </summary>
        /// <param name="tip">message tool tip</param>
        /// <returns>true on success</returns>
		private bool addNotifyIcon(string tip)
		{
			shellRestart = RegisterWindowMessage("TaskbarCreated");
			NOTIFYICONDATA data = getNOTIFYICONDATA(icon.Handle, NIF_MESSAGE | NIF_ICON | NIF_TIP | NIF_INFO, tip, null, null, BalloonType.None);
			return Shell_NotifyIcon(NIM_ADD, ref data);
		}

		/// <summary>
		/// deleteNotifyIcon
		/// </summary>
		/// <returns>true on success</returns>
        private bool deleteNotifyIcon()
		{
			NOTIFYICONDATA data = getNOTIFYICONDATA(IntPtr.Zero, NIF_MESSAGE | NIF_ICON | NIF_TIP | NIF_INFO, null, null, null, BalloonType.None);
			return Shell_NotifyIcon(NIM_DELETE, ref data);
		}

        /// <summary>
        /// modifyNotifyIcon
        /// </summary>
        /// <returns>true on success</returns>
		private bool modifyNotifyIcon(int flags, string tip, string infoTitle, string info, BalloonType balloonType)
		{
			NOTIFYICONDATA data = getNOTIFYICONDATA(icon.Handle, flags, tip, infoTitle, info, balloonType);
			return Shell_NotifyIcon(NIM_MODIFY, ref data);
		}
		#endregion

		#region Event Handlers
        /// <summary>
        /// event Handler for message loop click event
        /// </summary>
        private void messageLoop_Click(object sender, EventArgs e)
		{
			if (Click != null)
			{
				// Fire the Click event.
				Click(this, e);
			}
		}

        /// <summary>
        /// Event handler for messageloop double clicl event
        /// </summary>
        private void messageLoop_DoubleClick(object sender, EventArgs e)
		{
			if (DoubleClick != null)
			{
				// Fire the DoubleClick event.
				DoubleClick(this, e);
			}
		}

        /// <summary>
        /// Event Handler for message balloon click event
        /// </summary>
        private void messageLoop_BalloonClick(object sender, EventArgs e)
		{
			if (BalloonClick != null)
			{
				// Fire the BalloonClick event.
				BalloonClick(this, e);
			}
		}

        /// <summary>
        /// Event handler for message loop context menu popup event
        /// </summary>
        private void messageLoop_ContextMenuPopup(object sender, EventArgs e)
		{
			if (ContextMenuPopup != null)
			{
				// Fire the ContextMenuPopup event.
				ContextMenuPopup(this, e);
			}
		}
		#endregion
	}

	#region NotifyMessageLoop
    /// <summary>
    /// class NotifyMessageLoop
    /// </summary>
	internal class NotifyMessageLoop : System.Windows.Forms.Form
	{
		private bool balloonClicked = false;
		private bool iconClicked = false;
		private ShellNotifyIcon shellNotifyIcon = null;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="shellNotifyIcon">Shell NotifyIcon object</param>
		internal NotifyMessageLoop(ShellNotifyIcon shellNotifyIcon)
		{
			this.shellNotifyIcon = shellNotifyIcon;
		}

		#region Win32 API
		// From ShellAPI.h
		private const int NIN_BALLOONUSERCLICK = 0x0405;

		// From WinUser.h
		private const int WM_MOUSEMOVE = 0x0200;
		private const int WM_LBUTTONDOWN = 0x0201;
		private const int WM_LBUTTONUP = 0x0202;
		private const int WM_LBUTTONDBLCLK = 0x0203;
		private const int WM_RBUTTONDOWN = 0x0204;
		private const int WM_RBUTTONUP = 0x0205;
		private const int WM_MBUTTONDOWN = 0x0207;

		private const int TPM_RIGHTBUTTON = 0x0002;

		[DllImport("user32.dll")]
		private static extern int TrackPopupMenu(
			IntPtr hMenu,
			int wFlags,
			int x,
			int y,
			int nReserved,
			IntPtr hwnd,
			IntPtr prcRect);
		#endregion

		#region Events
		/// <summary>
		/// Click delegate.
		/// </summary>
		public delegate void ClickDelegate(object sender, EventArgs e);
		/// <summary>
		/// Occurs when the user clicks the icon in the status area.
		/// </summary>
		new public event ClickDelegate Click;

		/// <summary>
		/// Double-click delegate.
		/// </summary>
		public delegate void DoubleClickDelegate(object sender, EventArgs e);
		/// <summary>
		/// Occurs when the user double-clicks the icon in the status notification area of the taskbar.
		/// </summary>
		new public event DoubleClickDelegate DoubleClick;

		/// <summary>
		/// Balloon click delegate.
		/// </summary>
		public delegate void BalloonClickDelegate(object sender, EventArgs e);
		/// <summary>
		/// Occurs when the user clicks 
		/// (1) the icon in the status area while a balloon tooltip is displayed, or
		/// (2) the balloon tooltip.
		/// </summary>
		public event BalloonClickDelegate BalloonClick;

		/// <summary>
		/// ContextMenu popup delegate.
		/// </summary>
		public delegate void ContextMenuPopupDelegate(object sender, EventArgs e);
		/// <summary>
		/// Occurs before the shortcut menu is displayed.
		/// </summary>
		public event ContextMenuPopupDelegate ContextMenuPopup;

/* Implement these if needed.
		/// <summary>
		/// Mouse down delegate.
		/// </summary>
		public delegate void MouseDownDelegate(object sender, MouseEventArgs e);
		/// <summary>
		/// Occurs when the user presses the mouse button while the pointer is over the icon in the 
		/// status notification area of the taskbar.
		/// </summary>
		new public event MouseDownDelegate MouseDown;

		/// <summary>
		/// Mouse move delegate.
		/// </summary>
		public delegate void MouseMoveDelegate(object sender, MouseEventArgs e);
		/// <summary>
		/// Occurs when the user moves the mouse while the pointer is over the icon in the status 
		/// notification area of the taskbar.
		/// </summary>
		new public event MouseMoveDelegate MouseMove;

		/// <summary>
		/// Mouse up delegate.
		/// </summary>
		public delegate void MouseUpDelegate(object sender, MouseEventArgs e);
		/// <summary>
		/// Occurs when the user releases the mouse button while the pointer is over the icon in the 
		/// status notification area of the taskbar.
		/// </summary>
		new public event MouseUpDelegate MouseUp;*/
		#endregion

        /// <summary>
        /// Wnd Proc method
        /// </summary>
        /// <param name="msg">message to the process</param>
		protected override void WndProc(ref Message msg) 
		{
//			System.Diagnostics.Debug.WriteLine(msg.Msg);
			if (msg.Msg == shellNotifyIcon.WM_NOTIFY_TRAY) 
			{
				if ((int)msg.WParam == shellNotifyIcon.uID)
				{
					switch ((int)msg.LParam)
					{
						case NIN_BALLOONUSERCLICK:
							balloonClicked = true;
							if (BalloonClick != null)
							{
								// Fire the BalloonClick event.
								BalloonClick(this, new EventArgs());
								return;
							}
							break;
						case WM_MOUSEMOVE:
							if (!iconClicked)
								balloonClicked = false;
							break;
						case WM_LBUTTONDOWN:
							iconClicked = true;
							break;
						case WM_LBUTTONUP:
							// Don't fire this event if the balloon-click event was just fired.
							if (!balloonClicked)
							{
								if (Click != null)
								{
									// Fire the Click event.
									Click(this, new EventArgs());
									iconClicked = balloonClicked = false;
									return;
								}
							}
							iconClicked = balloonClicked = false;
							break;
						case WM_LBUTTONDBLCLK:
							if (DoubleClick != null)
							{
								// Fire the DoubleClick event.
								DoubleClick(this, new EventArgs());
								return;
							}
							break;
						case WM_RBUTTONDOWN:
							break;
						case WM_RBUTTONUP:
							if ((shellNotifyIcon.ContextMenu != null) && 
								(shellNotifyIcon.ContextMenu.Handle != IntPtr.Zero))
							{
								if (ContextMenuPopup != null)
								{
									// Fire the ContextMenuPopup event.
									ContextMenuPopup(this, new EventArgs());
								}

								// This is a work-around for a known bug ...
								// (call SetForegroundWindow before and after calling TrackPopupMenu).
								ShellNotifyIcon.SetForegroundWindow(shellNotifyIcon.Handle);

								// Display the shortcut menu.
								TrackPopupMenu(
									shellNotifyIcon.ContextMenu.Handle,
									TPM_RIGHTBUTTON,
									System.Windows.Forms.Cursor.Position.X,
									System.Windows.Forms.Cursor.Position.Y,
									0,
									shellNotifyIcon.Handle,
									IntPtr.Zero);

								ShellNotifyIcon.SetForegroundWindow(shellNotifyIcon.Handle);
								return;
							}
							break;
					}
				}
			}
			else if (msg.Msg == shellNotifyIcon.shellRestart)
			{
				// The shell has been restarted, add the icon to the tray.
				if (shellNotifyIcon.Visible)
				{
					shellNotifyIcon.Visible = false;
					shellNotifyIcon.Visible = true;
				}
			}

			base.WndProc(ref msg);
		}
	}
	#endregion
}
