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

using Gtk;
using Gnome;
using Gdk;
using GtkSharp;
using Novell.AddressBook.UI.gtk;

namespace POBoxViewer
{
	public class POBoxViewerApp 
	{
		public static void Main (string[] args)
		{
			Gnome.Program program =
				new Program("pobox-viewer", "0.10.0", Modules.UI, args);

			POBoxViewer poBox = new POBoxViewer();
			poBox.ViewerClosed += new EventHandler(on_po_closed);
			poBox.ShowAll();
			program.Run();
		}

		public static void on_po_closed(object o, EventArgs args) 
		{
			Application.Quit();
		}
	}
}
