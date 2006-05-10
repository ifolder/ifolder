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

using Gtk;
using Gdk;
using Glade;
using GtkSharp;
using GLib;

namespace Novell.AddressBook.UI.gtk
{
	public class CollectionProperties
	{
		[Glade.Widget] private Gtk.Dialog 	PropDialog = null;
		[Glade.Widget] private Gtk.VBox 	DialogVBox = null;

		private Gtk.Notebook	propNoteBook = null;

		private ContactCollectionSharingPage sharingPage = null;
		private CollectionGeneralPage		generalPage = null;
		private CollectionPropertiesPage	propPage = null;

		private int activeTag = 0;
		private Collection collection;
		private bool runCalled = false;

		public event EventHandler Closed;

		public Gtk.Window TransientFor
		{
			set
			{
				if(PropDialog != null)
					PropDialog.TransientFor = value;
			}
		}

		public int ActiveTag
		{
			set
			{
				activeTag = value;
			}
		}

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

		public CollectionProperties() 
		{
			InitGlade();
		}

		public void InitGlade()
		{
			Glade.XML gxml = 
				new Glade.XML (Util.GladePath("collection-properties.glade"), 
				"PropDialog", 
				null);
			gxml.Autoconnect (this);
		}

		private void InitDialog()
		{
			//Console.WriteLine("Initializing Dialog");
			if(collection != null)
			{
				//Console.WriteLine("Have a valid Collection");

				propNoteBook = new Gtk.Notebook();
				sharingPage = new ContactCollectionSharingPage();
				generalPage = new CollectionGeneralPage();
				propPage = new CollectionPropertiesPage();

				sharingPage.Collection = collection;
				generalPage.Collection = collection;
				propPage.Collection = collection;

				//Console.WriteLine("Adding General Page");
				propNoteBook.AppendPage(generalPage.MainWidget, 
						new Label("General"));
				//Console.WriteLine("Adding Sharing Page");
				propNoteBook.AppendPage(sharingPage.MainWidget, 
						new Label("Sharing"));
				//Console.WriteLine("Adding Prop Page");
				propNoteBook.AppendPage(propPage.MainWidget, 
						new Label("All Properties"));

				//Console.WriteLine("Connecting all of the stuff");

				DialogVBox.PackStart(propNoteBook);
				DialogVBox.ShowAll();
			}
		}

		public int Run()
		{
			int rc = 0;

			InitDialog();

			if(PropDialog != null)
			{
				if(propNoteBook.NPages >= activeTag)
					propNoteBook.CurrentPage = activeTag;

				while(rc == 0)
				{
					runCalled = true;
					rc = PropDialog.Run();
					if(rc == -11) // help
					{
						rc = 0;
						switch(propNoteBook.CurrentPage)
						{
							case 1:
								Util.ShowHelp("bq6lwlu.html", null);
								break;
							case 2:
								Util.ShowHelp("bq6lwlj.html", null);
								break;
							default:
								Util.ShowHelp("front.html", null);
								break;
						}
					}
				}

				on_close(null, null);
			}
			return rc;
		}


		public void Show()
		{
			int rc = 0;

			InitDialog();

			if(PropDialog != null)
			{
				if(propNoteBook.NPages >= activeTag)
					propNoteBook.CurrentPage = activeTag;

				PropDialog.Show();
			}
		}


		public void on_close(object o, EventArgs args)
		{
			//Console.WriteLine("on_close is being called");
			PropDialog.Hide();
			PropDialog.Destroy();
			PropDialog = null;
			if(Closed != null)
			{
				EventArgs e = new EventArgs();
				Closed(this, e);
			}
		}

		public void on_delete_event(object o, DeleteEventArgs args) 
		{
			args.RetVal = true;
			on_close(o, args);
		}

		public void on_close_clicked(object o, EventArgs args) 
		{
			// if Run was called (not Show), we don't want to call on_close
			// because the close button already sends a signal
			// when it is clicked and we respond to it
			if(!runCalled)
			{
				on_close(o, args);
			}
		}
	}
}
