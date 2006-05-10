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
using System.Drawing;

using Gtk;
using Gdk;
using Glade;
using GtkSharp;
using GLib;

namespace Novell.AddressBook.UI.gtk
{
	public class BookEditor
	{
		[Glade.Widget] internal Gtk.Entry beName;
		[Glade.Widget] internal Gtk.Button okButton;

		Gtk.Dialog 		beDlg = null;
		private string name;

		public string Name
		{
			get
			{
				return name;
			}

			set
			{
				name = value;
			}
		}

		public Gtk.Window TransientFor
		{
			set
			{
				if(beDlg != null)
					beDlg.TransientFor = value;
			}
		}

		public BookEditor() 
		{
			Glade.XML gxml = new Glade.XML (Util.GladePath("contact-browser.glade"),
					"BookEditor", null);
			gxml.Autoconnect (this);

			beDlg = (Gtk.Dialog) gxml.GetWidget("BookEditor");
		}

		public int Run()
		{
			int rc = 0;
			if(beDlg != null)
			{
				rc = beDlg.Run();
				name = beName.Text;
				beDlg.Hide();
				beDlg.Destroy();
				beDlg = null;
			}
			return rc;
		}

		public void on_beName_changed(object o, EventArgs args)
		{
			if(beName.Text.Length > 0)
				okButton.Sensitive = true;
			else
				okButton.Sensitive = false;
		}
	}
}
