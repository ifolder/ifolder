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

	using Gtk;
	using Gdk;
	using Gnome;
	using Glade;
	using GtkSharp;
	using GLib;

	public class PropertiesDialog 
	{
		[Glade.Widget] Notebook propNoteBook = null;

		Gtk.Dialog dialog; 
		SharingPage	spage;
		SettingsPage settingsPage;
		NodePropertyPage	nppage;
		Gtk.Widget swidget;
		Gtk.Widget npwidget;
		iFolder ifldr;
		Node node;
		int activeTag = 0;
		string path;

		public Gtk.Window TransientFor
		{
			set
			{
				if(dialog != null)
					dialog.TransientFor = value;
			}
		}

		public string iFolderPath
		{
			get
			{
				return path;
			}

			set
			{
				path = value;
			}
		}

		public iFolder iFolder
		{
			get
			{
				return ifldr;
			}

			set
			{
				ifldr = value;
			}
		}

		public Node Node
		{
			get
			{
				return node;
			}

			set
			{
				node = value;
			}
		}

		public int ActiveTag
		{
			set
			{
				activeTag = value;
			}
		}

		public PropertiesDialog() 
		{
			InitGlade();
		}

		public void InitGlade()
		{
			Glade.XML gxml = 
					new Glade.XML (Util.GladePath("properties-dialog.glade"), 
					"PropertiesDialog", 
					null);
			gxml.Autoconnect (this);

			dialog = (Gtk.Dialog) gxml.GetWidget("PropertiesDialog");
		}


		public int Run()
		{
			int rc = 0;
			if(dialog != null)
			{
				if(ifldr == null)
				{
					iFolderManager ifmgr;

					ifmgr = iFolderManager.Connect();

					if(ifmgr.IsiFolder(path))
					{
						ifldr = ifmgr.GetiFolderByPath(path);
					}
				}

				if(ifldr != null)
				{
					settingsPage = new SettingsPage();
					settingsPage.iFolder = ifldr;
					propNoteBook.AppendPage(settingsPage.GetWidget(), 
							new Label("Settings"));

					spage = new SharingPage(ifldr);
					swidget = spage.GetWidget();
					propNoteBook.AppendPage(swidget, 
							new Label("Sharing"));

					nppage = new NodePropertyPage(ifldr.CurrentNode);
					npwidget = nppage.GetWidget();
					propNoteBook.AppendPage(npwidget, new Label("Advanced"));
	
					dialog.Icon = 
							new Pixbuf(Util.ImagesPath("ifolderfolder.png"));
				}
				else if(node != null)
				{
					nppage = new NodePropertyPage(this.node);
					npwidget = nppage.GetWidget();
					propNoteBook.AppendPage(npwidget, 
							new Label("Advanced"));
				}

				if(propNoteBook.NPages >= activeTag)
					propNoteBook.CurrentPage = activeTag;

				rc = dialog.Run();

				dialog.Hide();
				dialog.Destroy();
				dialog = null;
			}

			return rc;
		}
	}
}
