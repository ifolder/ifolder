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
using Simias;

using Gtk;
using Gdk;
using Gnome;
using Glade;
using GtkSharp;
using GLib;


namespace Novell.iFolder
{

	public class CollectionBrowser
	{
		[Glade.Widget] IconList		BrowserIconList = null;
		//[Glade.Widget] TreeView	BrowserTreeView;

		Gtk.Window nifWindow; 
		Pixbuf	NodePixBuf;
		Pixbuf	LeafNodePixBuf;
		int							curIndex;
		bool						enableDblClick;
		ArrayList					nodeArray;
		Simias.Storage.Node			curNode;
		Store						store;

		public event EventHandler BrowserClosed;

		public CollectionBrowser(Store store) 
		{
			Init();

			this.store = store;

			SetCurrentNode(null);
		}

		public CollectionBrowser() 
		{
			Init();

			Configuration config = new Configuration();
			store = Store.Connect(config);

			SetCurrentNode(null);
		}

		public CollectionBrowser(string storeLocation) 
		{
			Init();

			Configuration config = new Configuration(storeLocation);
			store = Store.Connect(config);

			SetCurrentNode(null);
		}

		private void Init()
		{
			Glade.XML gxml = new Glade.XML ("ifolder.glade", 
					"BrowserWindow", 
					null);
			gxml.Autoconnect (this);

			nifWindow = (Gtk.Window) gxml.GetWidget("BrowserWindow");

			NodePixBuf = new Pixbuf("node.png");
			LeafNodePixBuf = new Pixbuf("leafnode.png");

			nodeArray = new ArrayList();

			nifWindow.Icon = NodePixBuf;
			nifWindow.Title = "Simias Collection Browser";

			curIndex = -1;
			enableDblClick = false;
		}


		private void SetCurrentNode(Simias.Storage.Node nifNode)
		{
			BrowserIconList.Clear();
			nodeArray.Clear();

			curNode = nifNode;

			if(nifNode == null)
			{
				curIndex = -1;
				foreach(Simias.Storage.Node node in store)
				{
					nodeArray.Add(node);
					if(!node.HasChildren)
						BrowserIconList.AppendPixbuf(LeafNodePixBuf,
										null,
										node.Name);
					else
						BrowserIconList.AppendPixbuf(NodePixBuf,
										null,
										node.Name);
				}
			}
			else
			{
				foreach(Simias.Storage.Node node in nifNode)
				{
					nodeArray.Add(node);
					if(!node.HasChildren)
						BrowserIconList.AppendPixbuf(LeafNodePixBuf,
										null,
										node.Name);
					else
						BrowserIconList.AppendPixbuf(NodePixBuf,
										null,
										node.Name);
				}
			}
		}

		public void ShowAll()
		{
			if(nifWindow != null)
				nifWindow.ShowAll();
		}

		private void on_browser_delete_event (object o, DeleteEventArgs args) 
		{
			nifWindow.Hide();
			nifWindow.Destroy();
			nifWindow = null;

			if(BrowserClosed != null)
			{
				EventArgs e = new EventArgs();
				BrowserClosed(this, e);
			}
		}

		public void on_UpButton_clicked(object o, EventArgs args)
		{
			if(curNode != null)
				SetCurrentNode(curNode.GetParent());
		}

		public void on_HomeButton_clicked(object o, EventArgs args)
		{
			SetCurrentNode(null);
		}

		private void on_select_icon(object obj, 
				IconSelectedArgs args)
		{
			if(args.Event != null)
			{
				int idx = args.Num;

				EventButton ev = new EventButton(args.Event.Handle);

				if(	(args.Event.Type == EventType.TwoButtonPress) && 
						(ev.Button == 1) &&
						(enableDblClick) )
				{
					on_open_context_menu(obj, args);
				}

				if(args.Event.Type == EventType.ButtonPress)
				{
					if(idx == curIndex)
						enableDblClick = true;
					else
						enableDblClick = false;

					curIndex = idx;
				}

				if(ev.Button == 3)
				{
					show_context_menu(idx);
				}
			}
		}

		private Node GetSelectedItem()
		{
			Node node = (Node) nodeArray[curIndex];

			return node;
		}

		private void set_item_pixBuf(int idx, Pixbuf itemPixbuf)
		{
			Gnome.CanvasPixbuf cPixbuf = BrowserIconList.GetIconPixbufItem(idx);
			cPixbuf.Pixbuf = itemPixbuf;
		}

		private void show_context_menu(int idx)
		{
			Node node = (Node) nodeArray[idx];

			Menu trayMenu = new Menu();
			if(node != null)
			{
				MenuItem open_item = new MenuItem ("Open");
				open_item.Activated += new EventHandler(
						on_open_context_menu);
				trayMenu.Append (open_item);

				if(!node.HasChildren)
				{
					MenuItem delete_item = new MenuItem ("Delete");
					delete_item.Activated += new EventHandler(
						on_delete_context_menu);
					trayMenu.Append (delete_item);
				}

				trayMenu.Append(new SeparatorMenuItem());

				MenuItem properties_item = new MenuItem (
						"Properties");
				properties_item.Activated += new EventHandler(
						on_properties_context_menu);
				trayMenu.Append (properties_item);
			}
			else
			{
				MenuItem broken_item = new MenuItem ("Nada Para Fazer");
				trayMenu.Append (broken_item);
			}

			trayMenu.ShowAll();

			trayMenu.Popup(null, null, null, IntPtr.Zero, 3, 
					Gtk.Global.CurrentEventTime);
		}

		public void on_properties_context_menu(object o, EventArgs args)
		{
			Node node = GetSelectedItem();
			if(node != null)
			{
				PropertiesDialog pd = new PropertiesDialog(node);
				pd.ShowAll(1);
			}
		}

		public void on_refresh()
		{
			SetCurrentNode(curNode);
		}

		public void on_delete_context_menu(object o, EventArgs args)
		{
			Node node = GetSelectedItem();

			node.Delete(true);

			on_refresh();
		}

		public void on_open_context_menu(object o, EventArgs args)
		{
			Node node = GetSelectedItem();

			if(node != null)
			{
				if(node.HasChildren)
				{
					SetCurrentNode(node);
				}
				else
				{
					PropertiesDialog pd = new PropertiesDialog(node);
					pd.ShowAll(1);
				}
			}
		}
/*
		public void on_shareifolder_context_menu(object o, EventArgs args)
		{
			DirectoryEntry de = GetSelectedItem();
			if(de != null)
			{
				PropertiesDialog pd = new PropertiesDialog(de.FullName);
				pd.ShowAll(1);
			}
		}

		public void on_makeifolder_context_menu(object o, EventArgs args)
		{
			DirectoryEntry de = GetSelectedItem();
			if(de != null)
			{
				try
				{
					int count;
					iFolder ifldr = ifmgr.CreateiFolder(de.FullName);
					set_item_pixBuf(curIndex, iFolderPixBuf);
				}
				catch(Exception e)
				{
					Console.WriteLine("Failed to create iFolder " + e);
				}
			}
		}

		public void on_unmakeifolder_context_menu(object o, EventArgs args)
		{
			DirectoryEntry de = GetSelectedItem();
			if(de != null)
			{
				try
				{
					ifmgr.DeleteiFolderByName(de.FullName);
					set_item_pixBuf(curIndex, FolderPixBuf);
				}
				catch(Exception e)
				{
					Console.WriteLine("Failed to delete iFolder " + e);
				}
			}
		}

		public void on_open_context_menu(object o, EventArgs args)
		{
			DirectoryEntry de = GetSelectedItem();
			if(de != null)
			{
				if(de.IsDirectory)
				{
					SetCurrentDir(de.GetDirectoryInfo());
				}
			}
		}
*/
	}
}
