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
		[Glade.Widget] Notebook propNotebook;
		[Glade.Widget] Button addSharingButton;

		Gtk.Window win; 
		iFolderManager ifmgr;
		SharingPage	spage;
		NodePropertyPage	nppage;
		Gtk.Widget swidget;
		Gtk.Widget npwidget;
		iFolder ifldr;
		Node node;
/*
		ListStore BrowserTreeStore;
		Pixbuf	FilePixBuf;
		Pixbuf	FolderPixBuf;
		Pixbuf	iFolderPixBuf;
		DirectoryInfo curDirInfo;
*/
		public PropertiesDialog(string path) 
		{
			Glade.XML gxml = new Glade.XML ("ifolder.glade", 
					"Properties", 
					null);
			gxml.Autoconnect (this);

			win = (Gtk.Window) gxml.GetWidget("Properties");
/*
			FilePixBuf = new Pixbuf("file.xpm");
			FolderPixBuf = new Pixbuf("folder.png");
			iFolderPixBuf = new Pixbuf("ifolderfolder.png");
*/
			ifmgr = iFolderManager.Connect();
			if(ifmgr.IsiFolder(path))
			{
				ifldr = ifmgr.GetiFolderByPath(path);

				spage = new SharingPage(win, ifldr);
				swidget = spage.GetWidget();
				propNotebook.AppendPage(swidget, new Label("iFolder Sharing"));

				nppage = new NodePropertyPage(win, ifldr.CurrentNode);
				npwidget = nppage.GetWidget();
				propNotebook.AppendPage(npwidget, new Label("iFolder"));
			}
		}

		public PropertiesDialog(Node node) 
		{
			this.node = node;

			Glade.XML gxml = new Glade.XML ("ifolder.glade", 
					"Properties", 
					null);
			gxml.Autoconnect (this);

			win = (Gtk.Window) gxml.GetWidget("Properties");
/*
			FilePixBuf = new Pixbuf("file.xpm");
			FolderPixBuf = new Pixbuf("folder.png");
			iFolderPixBuf = new Pixbuf("ifolderfolder.png");
*/

			nppage = new NodePropertyPage(win, node);
			npwidget = nppage.GetWidget();
			propNotebook.AppendPage(npwidget, new Label("Node Properties"));
		}

		public void ShowAll()
		{
			ShowAll(0);
		}

		public void ShowAll(int tabnum)
		{
			if(win != null)
			{
				if(propNotebook.NPages >= tabnum)
					propNotebook.CurrentPage = tabnum;
				win.ShowAll();
			}
		}

		private void on_close(object o, EventArgs args) 
		{
			win.Hide();
			win.Destroy();
			win = null;
		}

	}
}
