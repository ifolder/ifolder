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
using Simias.Presence;
using Simias;
using Novell.AddressBook.UI.gtk;

using Gtk;
using Gdk;
using Glade;
using GtkSharp;
using GLib;


namespace Novell.iFolder
{
	public class ReunionBrowser
	{
		[Glade.Widget] internal Gtk.TreeView	ReunionTreeView;
		[Glade.Widget] internal Gtk.Window		RBWindow;

		private ListStore			ReunionTreeStore;
//		private	Store				store;
		private Pixbuf				ifolder_pixbuf;

		public event EventHandler BrowserClosed;

		public ReunionBrowser() 
		{
		}

		private void InitUI()
		{
			Glade.XML gxml = 
					new Glade.XML (Util.GladePath("reunion-browser.glade"), 
					"RBWindow", 
					null);
			gxml.Autoconnect (this);

			// Setup the Collection TreeView
			ReunionTreeStore = new ListStore(typeof(SubscriptionInfo));
			ReunionTreeView.Model = ReunionTreeStore;
			CellRendererPixbuf reunioncrp = new CellRendererPixbuf();
			TreeViewColumn reuniontvc = new TreeViewColumn();
			reuniontvc.PackStart(reunioncrp, false);
			reuniontvc.SetCellDataFunc(reunioncrp, new TreeCellDataFunc(
						ReunionCellPixbufDataFunc));

			CellRendererText reunioncrt = new CellRendererText();
			reuniontvc.PackStart(reunioncrt, false);
			reuniontvc.SetCellDataFunc(reunioncrt, new TreeCellDataFunc(
						ReunionCellTextDataFunc));
			reuniontvc.Title = "iFolders";
			ReunionTreeView.AppendColumn(reuniontvc);
//			ReunionTreeView.Selection.Changed += new EventHandler(
//						on_reunion_selection_changed);

			ifolder_pixbuf = new Pixbuf(Util.ImagesPath("ifolder.png"));
		}


		private void ReunionCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			SubscriptionInfo subInfo = (SubscriptionInfo) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = subInfo.SubscriptionCollectionName;
		}


		private void ReunionCellPixbufDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			((CellRendererPixbuf) cell).Pixbuf = ifolder_pixbuf;
		}

		private void RefreshView()
		{
			ReunionTreeStore.Clear();
			SubscriptionInfo[] slist = PublicSubscription.GetSubscriptions();
			foreach(SubscriptionInfo si in slist)
			{
				ReunionTreeStore.AppendValues(si);
			}
		}

		public void ShowAll()
		{
			if(RBWindow == null)
			{
				InitUI();
				RefreshView();
				RBWindow.ShowAll();
			}
		}

		public void on_properties(object o, EventArgs args) 
		{
			Console.WriteLine("Show Properties");
		}

		public void on_row_activated(object o, EventArgs args)
		{
			TreeSelection tSelect = ReunionTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				SubscriptionInfo si = 
						(SubscriptionInfo) tModel.GetValue(iter, 0);
				if(si != null)
				{
					InvitationAssistant ia = 
						new InvitationAssistant(si);

					ia.ShowAll();	
				}
			}
		}

		public void on_refresh(object o, EventArgs args) 
		{
			RefreshView();
		}

		public void on_close(object o, EventArgs args) 
		{
			RBWindow.Hide();
			RBWindow.Destroy();
			RBWindow = null;
			if(BrowserClosed != null)
			{
				EventArgs e = new EventArgs();
				BrowserClosed(this, e);
			}
		}

		private void on_RBWindow_delete_event (object o, DeleteEventArgs args) 
		{
			if(BrowserClosed != null)
			{
				EventArgs e = new EventArgs();
				BrowserClosed(this, e);
			}
			RBWindow = null;
		}
	}
}
