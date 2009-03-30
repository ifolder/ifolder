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
*---------------------------------------------------------------------------*/

// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace Egg {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;

#region Autogenerated code
	public class TrayIcon : Gtk.Plug {

		~TrayIcon()
		{
			Dispose();
		}

		protected TrayIcon(GLib.GType gtype) : base(gtype) {}
		public TrayIcon(IntPtr raw) : base(raw) {}

		[DllImport("libtrayicon")]
		static extern IntPtr egg_tray_icon_new(string name);

		public TrayIcon (string name) : base (IntPtr.Zero)
		{
			if (GetType () != typeof (TrayIcon)) {
				ArrayList vals = new ArrayList();
				ArrayList names = new ArrayList();
				names.Add ("name");
				vals.Add (new GLib.Value (name));
				CreateNativeObject ((string[])names.ToArray (typeof (string)), (GLib.Value[])vals.ToArray (typeof (GLib.Value)));
				return;
			}
			Raw = egg_tray_icon_new(name);
		}

		[DllImport("libtrayicon")]
		static extern IntPtr egg_tray_icon_new_for_screen(IntPtr screen, string name);

		public TrayIcon (Gdk.Screen screen, string name) : base (IntPtr.Zero)
		{
			if (GetType () != typeof (TrayIcon)) {
				ArrayList vals = new ArrayList();
				ArrayList names = new ArrayList();
				names.Add ("screen");
				vals.Add (new GLib.Value (screen));
				names.Add ("name");
				vals.Add (new GLib.Value (name));
				CreateNativeObject ((string[])names.ToArray (typeof (string)), (GLib.Value[])vals.ToArray (typeof (GLib.Value)));
				return;
			}
			Raw = egg_tray_icon_new_for_screen(screen.Handle, name);
		}

		[DllImport("libtrayicon")]
		static extern int egg_tray_icon_get_orientation(IntPtr raw);


		public Gtk.Orientation Orientation {
			get  {
				int raw_ret = egg_tray_icon_get_orientation(Handle);
				Gtk.Orientation ret = (Gtk.Orientation)raw_ret;
				return ret;
			}
		}

		[DllImport("libtrayicon")]
		static extern uint egg_tray_icon_send_message(IntPtr raw, int timeout, string message, int len);

		public uint SendMessage(int timeout, string message) {
			uint raw_ret = egg_tray_icon_send_message(Handle, timeout, message, message.Length);
			uint ret = raw_ret;
			return ret;
		}

		[DllImport("libtrayicon")]
		static extern IntPtr egg_tray_icon_get_type();

		public static new GLib.GType GType { 
			get {
				IntPtr raw_ret = egg_tray_icon_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
		}

		[DllImport("libtrayicon")]
		static extern void egg_tray_icon_cancel_message(IntPtr raw, uint id);

		public void CancelMessage(uint id) {
			egg_tray_icon_cancel_message(Handle, id);
		}


		static TrayIcon ()
		{
			GtkSharp.TrayIcon.ObjectManager.Initialize ();
		}
#endregion
	}
}
