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
using System.Collections;
using System.Drawing;
using Simias.Storage;

using Gtk;
using Gdk;
using Glade;
using GtkSharp;
using GLib;

namespace Novell.iFolder
{
	public class SettingsPage
	{
		public iFolder iFolder
		{
			get
			{
				return(ifldr);
			}

			set
			{
				this.ifldr = value;
			}
		}

		[Glade.Widget] internal Gtk.Entry	RefreshEntry;

		Gtk.Table SettingsTable;
		iFolder  ifldr;

		public SettingsPage()
		{
			InitGlade();
		}

		public void InitGlade()
		{
			Glade.XML gxml = new Glade.XML (Util.GladePath("ifolder.glade"), 
					"SettingsTable", null);
			gxml.Autoconnect (this);
			SettingsTable = (Gtk.Table) gxml.GetWidget("SettingsTable");
		}

		public Gtk.Widget GetWidget()
		{
			return SettingsTable;
		}

		private void on_unrealize(object o, EventArgs args) 
		{
			// Close out the contact picker
		}
	}
}
