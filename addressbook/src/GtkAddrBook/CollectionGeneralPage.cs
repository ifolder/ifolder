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
using Simias.POBox;
using Simias;
using Simias.Sync;
using Simias.Policy;

using Gtk;
using Gdk;
using Glade;
using GtkSharp;
using GLib;


namespace Novell.AddressBook.UI.gtk
{
	public class CollectionGeneralPage
	{
		[Glade.Widget] private Gtk.VBox			GeneralVBox = null;
		[Glade.Widget] private Gtk.CheckButton	AutoSyncCheckButton = null;
		[Glade.Widget] private Gtk.SpinButton	RefreshSpinButton = null;

		private Collection collection;

		public Collection Collection
		{
			get
			{
				return collection;
			}

			set
			{
				collection = value;
			}
		}

		public Widget MainWidget
		{
			get
			{
				if(collection == null)
					return null;

				if(GeneralVBox == null)
					InitGlade();

				return GeneralVBox;
			}
		}

		public CollectionGeneralPage() 
		{
		}

		private void InitGlade()
		{
			Glade.XML gxml = 
				new Glade.XML (Util.GladePath("collection-properties.glade"), 
				"GeneralVBox", 
				null);
			gxml.Autoconnect (this);

			Init_Page();
		}

		private void Init_Page()
		{
			AutoSyncCheckButton.Active = true;
			RefreshSpinButton.Value = new SyncCollection(collection).Interval;
		}

		private void on_AutoSyncCheckButton_toggled(object o, EventArgs args)
		{
			if(AutoSyncCheckButton.Active == true)
				Console.WriteLine("Auto Sync == true");
			else
				Console.WriteLine("Auto Sync == false");
		}

		private void on_RefreshSpinButton_changed(object o, EventArgs args)
		{
			SyncInterval.Set(collection, (int)RefreshSpinButton.Value);
		}
	}
}
