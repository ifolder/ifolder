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

namespace Novell.iFolder
{
	using System;
	using System.IO;
	using System.Drawing;
	using Simias.Storage;
	using Simias.Sync;
	using Simias;

	using Gtk;
	using Gdk;
	using Glade;
	using GtkSharp;
	using GLib;

	public class ConflictResolver 
	{
		[Glade.Widget] internal Gtk.Dialog 		ConflictDialog = null;

		public Gtk.Window TransientFor
		{
			set
			{
				if(ConflictDialog != null)
					ConflictDialog.TransientFor = value;
			}
		}

		public ConflictResolver() 
		{
		}

		public void InitGlade()
		{
			Glade.XML gxml = new Glade.XML (Util.GladePath(
					"conflict-resolver.glade"), 
					"ConflictDialog", 
					null);
			gxml.Autoconnect (this);
		}

		public int Run()
		{
			InitGlade();

			int rc = 0;
			if(ConflictDialog != null)
			{
				while(rc == 0)
				{
					rc = ConflictDialog.Run();
					if(rc == -11) // help
					{
						rc = 0;
						Util.ShowHelp("front.html", null);
					}
				}

				ConflictDialog.Hide();
				ConflictDialog.Destroy();
				ConflictDialog = null;
			}
			return rc;
		}

		public void on_open(object o, EventArgs args)
		{
		}

		public void on_auto(object o, EventArgs args)
		{
		}

		public void on_resolve(object o, EventArgs args)
		{
		}

		public void on_refresh(object o, EventArgs args)
		{
		}
	}
}
