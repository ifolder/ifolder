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

using Gtk;
using Gdk;
using Gnome;
using Glade;
using GtkSharp;
using GLib;

namespace Novell.iFolder
{
	public class FileBrowser
	{
		[Glade.Widget] IconList BrowserIconList = null;
		//[Glade.Widget] TreeView	BrowserTreeView;
		[Glade.Widget] Gtk.Entry	PathEntry = null;

		Gtk.Window nifWindow; 
		//ListStore BrowserTreeStore;
		Pixbuf	FilePixBuf;
		Pixbuf	FolderPixBuf;
		Pixbuf	iFolderPixBuf;
		DirectoryInfo curDirInfo;
		int		curIndex;
		bool	enableDblClick;
		iFolderManager ifmgr;
		ArrayList	DirEntryArray;

		public event EventHandler BrowserClosed;

		public FileBrowser() 
		{
			Glade.XML gxml = new Glade.XML ("ifolder.glade", 
					"BrowserWindow", 
					null);
			gxml.Autoconnect (this);

			nifWindow = (Gtk.Window) gxml.GetWidget("BrowserWindow");
			nifWindow.Title = "iFolder File Browser";

			DirectoryInfo dirinfo = new DirectoryInfo(
					Environment.GetFolderPath(
						Environment.SpecialFolder.Personal));

			FilePixBuf = new Pixbuf("file.xpm");
			FolderPixBuf = new Pixbuf("folder.png");
			iFolderPixBuf = new Pixbuf("ifolderfolder.png");

			ifmgr = iFolderManager.Connect();
			DirEntryArray = new ArrayList();

			curIndex = -1;
			enableDblClick = false;
			SetCurrentDir(dirinfo);
		}

		private void SetCurrentDir(DirectoryInfo di)
		{
			if(di != null)
			{
				BrowserIconList.Clear();
				DirEntryArray.Clear();

				curDirInfo = di;
				PathEntry.Text = di.FullName;
				if(ifmgr.IsiFolder(di.FullName))
					nifWindow.Icon = iFolderPixBuf;
				else
					nifWindow.Icon = FolderPixBuf;

				try 
				{
					DirectoryInfo[] dirs = di.GetDirectories();
					foreach (DirectoryInfo diNext in dirs) 
					{
						if(diNext.Name[0] != '.')
						{
							DirectoryEntry de = new DirectoryEntry(diNext);
							DirEntryArray.Add(de);
							if(ifmgr.IsiFolder(de.FullName))
								BrowserIconList.AppendPixbuf(iFolderPixBuf,
												null,
												de.Name);
							else
								BrowserIconList.AppendPixbuf(FolderPixBuf,
												null,
												de.Name);
						}
					}

					FileInfo[] files = di.GetFiles();
					foreach (FileInfo fiNext in files) 
					{
						if(fiNext.Name[0] != '.')
						{
							DirectoryEntry de = new DirectoryEntry(fiNext);
							DirEntryArray.Add(de);
							BrowserIconList.AppendPixbuf(FilePixBuf,
												null,
												de.Name);
						}
					}
				} 
				catch (Exception e) 
				{
					Console.WriteLine("The process failed: {0}", 
							e.ToString());
				}
			}
		}

		public void ShowAll()
		{
			if(nifWindow != null)
				nifWindow.ShowAll();
		}

		private void on_close(object o, EventArgs args) 
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

		private void on_browser_delete_event (object o, DeleteEventArgs args) 
		{
			on_close(o, args);
		}

		public void on_UpButton_clicked(object o, EventArgs args)
		{
			SetCurrentDir(curDirInfo.Parent);
		}

		public void on_HomeButton_clicked(object o, EventArgs args)
		{
			DirectoryInfo di;
			di = new DirectoryInfo( 
					Environment.GetFolderPath(
						Environment.SpecialFolder.Personal));
			SetCurrentDir(di);
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
					DirectoryEntry de = (DirectoryEntry) DirEntryArray[idx];
					if(de.IsDirectory)
					{
						SetCurrentDir(de.GetDirectoryInfo());
					}
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

		private DirectoryEntry GetSelectedItem()
		{
			DirectoryEntry de = (DirectoryEntry) DirEntryArray[curIndex];

			return de;
		}

		private void set_item_pixBuf(int idx, Pixbuf itemPixbuf)
		{
			Gnome.CanvasPixbuf cPixbuf = BrowserIconList.GetIconPixbufItem(idx);
			cPixbuf.Pixbuf = itemPixbuf;
		}

		private void show_context_menu(int idx)
		{
			DirectoryEntry de = (DirectoryEntry) DirEntryArray[idx];

			Menu trayMenu = new Menu();
			if(de != null)
			{
				MenuItem open_item = new MenuItem ("Open");
				trayMenu.Append (open_item);
				open_item.Activated += new EventHandler(on_open_context_menu);
				if(de.IsDirectory)
				{
					if(ifmgr.IsiFolder(de.FullName))
					{
						MenuItem share_item = new MenuItem (
								"Share with...");
						trayMenu.Append (share_item);
						share_item.Activated += new EventHandler(
								on_shareifolder_context_menu);

						MenuItem refresh_item = new MenuItem (
								"Refresh iFolder");
						trayMenu.Append (refresh_item);
						refresh_item.Activated += new EventHandler(
								on_refreshifolder_context_menu);

						MenuItem unmake_item = new MenuItem (
								"Revert to a Normal Folder");
						trayMenu.Append (unmake_item);
						unmake_item.Activated += new EventHandler(
								on_unmakeifolder_context_menu);
					}
					else if(ifmgr.CanBeiFolder(de.FullName))
					{
						MenuItem makeifolder_item = new MenuItem (
								"Convert to an iFolder");
						trayMenu.Append (makeifolder_item);
						makeifolder_item.Activated += new EventHandler(
								on_makeifolder_context_menu);
					}
				}

				// Add separator in Menu
				trayMenu.Append(new SeparatorMenuItem());

				// Add properties to all items
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
			DirectoryEntry de = GetSelectedItem();
			if(de != null)
			{
				Console.WriteLine("Show properties for: " + de.FullName);
				PropertiesDialog pd = new PropertiesDialog(de.FullName);
				pd.ShowAll();
			}
		}

		public void on_refreshifolder_context_menu(object o, EventArgs args)
		{	
			DirectoryEntry de = GetSelectedItem();
			if(de != null)
			{
				iFolder ifldr = ifmgr.GetiFolderByPath(de.FullName);
				if(ifldr != null)
					ifldr.Update();
			}
		}

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
					//int count;
					//iFolder ifldr = ifmgr.CreateiFolder(de.FullName);
					ifmgr.CreateiFolder(de.FullName);
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
					ifmgr.DeleteiFolderByPath(de.FullName);
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
	}
}
