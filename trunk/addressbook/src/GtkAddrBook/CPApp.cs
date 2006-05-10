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
using Simias.Storage;

using Gtk;
using Gnome;
using Gdk;
using GtkSharp;
using Novell.AddressBook.UI.gtk;

namespace CollectionPropertiesViewer
{
	public class CollectionPropertiesApp 
	{
		public static void Main (string[] args)
		{
			Gnome.Program program =
				new Program("collection-properties", "0.10.0", Modules.UI, args);
			Store store = Store.GetStore();

			if(args.Length < 1)
			{
				Console.WriteLine("Usage: ColPropViewer [collectionID]");
				Console.WriteLine("       where collectionID is:");

				foreach(ShallowNode sn in store)
				{
					Collection col = store.GetCollectionByID(sn.ID);
					Console.WriteLine("{0} : {1}", col.Name, col.ID);
				}
			}
			else
			{
				Collection col = store.GetCollectionByID(args[0]);
				if(col != null)
				{
					CollectionProperties cp = new CollectionProperties();
					cp.Collection = col;
					cp.Closed += new EventHandler(on_cp_closed);
					cp.Show();
					program.Run();
				}
			}
		}

		public static void on_cp_closed(object o, EventArgs args) 
		{
			Application.Quit();
		}
	}
}
