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
using System.Collections;

namespace Novell.iFolder
{

	/// <summary>
	/// This is the properties dialog for an iFolder.
	/// </summary>
	public class iFolderUserSelector : Dialog
	{
		private iFolderWebService	ifws;
		private Gtk.TreeView		UserTreeView;
		private Gtk.ListStore		UserTreeStore;
		private Gdk.Pixbuf			UserPixBuf;

		public iFolderUser[] SelectedUsers
		{
			get
			{
				ArrayList list = new ArrayList();
				TreeModel tModel;

				TreeSelection tSelect = UserTreeView.Selection;
				Array treePaths = tSelect.GetSelectedRows(out tModel);

				foreach(TreePath tPath in treePaths)
				{
					TreeIter iter;

					if(tModel.GetIter(out iter, tPath))
					{
						iFolderUser user = (iFolderUser) 
											tModel.GetValue(iter,0);
						list.Add(user);
					}
				}
				return (iFolderUser[]) (list.ToArray(typeof(iFolderUser)));
			}
		}



		/// <summary>
		/// Default constructor for iFolderPropertiesDialog
		/// </summary>
		public iFolderUserSelector(	Gtk.Window parent,
										iFolderWebService iFolderWS)
			: base()
		{
			this.Title = "iFolder User Selector";
			if(iFolderWS == null)
				throw new ApplicationException("iFolderWebServices was null");
			this.ifws = iFolderWS;
			this.HasSeparator = false;
			this.BorderWidth = 6;
			this.Resizable = true;
			this.Modal = true;
			if(parent != null)
				this.TransientFor = parent;

			InitializeWidgets();
		}




		/// <summary>
		/// Setup the UI inside the Window
		/// </summary>
		private void InitializeWidgets()
		{
			this.SetDefaultSize (300, 400);
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("contact.png"));


			// Create the main TreeView and add it to a scrolled
			// window, then add it to the main vbox widget
			UserTreeView = new TreeView();
			ScrolledWindow sw = new ScrolledWindow();
			sw.Add(UserTreeView);
			this.VBox.PackStart(sw, true, true, 0);


			// Setup the iFolder TreeView
			UserTreeStore = new ListStore(typeof(iFolderUser));
			UserTreeView.Model = UserTreeStore;

			// Setup Pixbuf and Text Rendering for "iFolder Users" column
			CellRendererPixbuf mcrp = new CellRendererPixbuf();
			TreeViewColumn memberColumn = new TreeViewColumn();
			memberColumn.PackStart(mcrp, false);
			memberColumn.SetCellDataFunc(mcrp, new TreeCellDataFunc(
						UserCellPixbufDataFunc));
			CellRendererText mcrt = new CellRendererText();
			memberColumn.PackStart(mcrt, false);
			memberColumn.SetCellDataFunc(mcrt, new TreeCellDataFunc(
						UserCellTextDataFunc));
			memberColumn.Title = "iFolder Users";
			memberColumn.Resizable = true;
			UserTreeView.AppendColumn(memberColumn);
			UserTreeView.Selection.Mode = SelectionMode.Multiple;


//			UserTreeView.Selection.Changed += new EventHandler(
//						OnUserSelectionChanged);

//			UserTreeView.ButtonPressEvent += new ButtonPressEventHandler(
//						OnUserButtonPressed);

//			UserTreeView.RowActivated += new RowActivatedHandler(
//						OnUserRowActivated);

			UserPixBuf = 
				new Gdk.Pixbuf(Util.ImagesPath("ifolderuser.png"));

			this.AddButton(Stock.Cancel, ResponseType.Cancel);
			this.AddButton(Stock.Ok, ResponseType.Ok);
			this.AddButton(Stock.Help, ResponseType.Help);

			RefreshUserList();
		}




		private void RefreshUserList()
		{
			iFolderUser[] userlist = ifws.GetAlliFolderUsers();
			foreach(iFolderUser user in userlist)
			{
				UserTreeStore.AppendValues(user);
			}
		}




		private void UserCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			iFolderUser user = (iFolderUser) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = user.Name;
		}




		private void UserCellPixbufDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
//			iFolderUser user = (iFolderUser) tree_model.GetValue(iter,0);
			((CellRendererPixbuf) cell).Pixbuf = UserPixBuf;
		}





	}
}
