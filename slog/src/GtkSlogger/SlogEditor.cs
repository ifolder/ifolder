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

namespace SloggerApplication
{
	public class SlogEditor
	{
		[Glade.Widget] internal Gtk.Dialog NewSlogDialog;
		[Glade.Widget] internal Gtk.Entry NameEntry;
		[Glade.Widget] internal Gtk.Button OKButton;

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
				if(NewSlogDialog != null)
					NewSlogDialog.TransientFor = value;
			}
		}

		public SlogEditor() 
		{
			Glade.XML gxml = new Glade.XML (Util.GladePath("slogger.glade"),
					"NewSlogDialog", null);
			gxml.Autoconnect (this);
		}

		public int Run()
		{
			int rc = 0;
			if(NewSlogDialog != null)
			{
				rc = NewSlogDialog.Run();
				name = NameEntry.Text;
				NewSlogDialog.Hide();
				NewSlogDialog.Destroy();
				NewSlogDialog = null;
			}
			return rc;
		}

		public void on_name_changed(object o, EventArgs args)
		{
			if(NameEntry.Text.Length > 0)
				OKButton.Sensitive = true;
			else
				OKButton.Sensitive = false;
		}
	}
}
