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
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 * 
 ***********************************************************************/

using System;
using System.IO;
using System.Drawing;
using System.Collections;
using Simias.Storage;
using Simias;

using Gtk;
using Gdk;
using Glade;
using GtkSharp;
using GLib;


namespace Novell.iFolder
{
	public class ServerInfoDialog
	{
		[Glade.Widget] private Gtk.Dialog	SIDialog;
		[Glade.Widget] private Gtk.Entry	AddressEntry;
		[Glade.Widget] private Gtk.Entry	NameEntry;
		[Glade.Widget] private Gtk.Entry	PasswordEntry;
		[Glade.Widget] private Gtk.Button	OKButton;
		[Glade.Widget] private Gtk.Button	CancelButton;

		public string Address;
		public string Name;
		public string Password;

		public ServerInfoDialog() 
		{
			InitUI();
		}

		private void InitUI()
		{
			Glade.XML gxml = 
					new Glade.XML (Util.GladePath("server-info.glade"), 
					"SIDialog", 
					null);
			gxml.Autoconnect (this);

			OKButton.Sensitive = false;
		}

		public int Run()
		{
			int rc = 0;
			if(SIDialog != null)
			{
				rc = SIDialog.Run();
				SIDialog.Hide();
				SIDialog.Destroy();
				SIDialog = null;
			}
			return rc;
		}

		private void on_address_changed(object obj, EventArgs args)
		{
			if(	(NameEntry.Text.Length > 0) &&
				(PasswordEntry.Text.Length > 0 ) &&
				(AddressEntry.Text.Length > 0) )
			{
				OKButton.Sensitive = true;
			}
			else
			{
				OKButton.Sensitive = false;
			}
		}

		private void on_name_changed(object obj, EventArgs args)
		{
			if(	(NameEntry.Text.Length > 0) &&
				(PasswordEntry.Text.Length > 0 ) &&
				(AddressEntry.Text.Length > 0) )
			{
				OKButton.Sensitive = true;
			}
			else
			{
				OKButton.Sensitive = false;
			}
		}

		private void on_password_changed(object obj, EventArgs args)
		{
			if(	(NameEntry.Text.Length > 0) &&
				(PasswordEntry.Text.Length > 0 ) &&
				(AddressEntry.Text.Length > 0) )
			{
				OKButton.Sensitive = true;
			}
			else
			{
				OKButton.Sensitive = false;
			}
		}

		private void on_ok_clicked(object obj, EventArgs args)
		{
			Address = AddressEntry.Text;
			Name = NameEntry.Text;
			Password = PasswordEntry.Text;
		}
	}
}
