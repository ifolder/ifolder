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
		private string				domainID;
		private Gtk.TreeView		UserTreeView;
		private Gtk.ListStore		UserTreeStore;
		private Gdk.Pixbuf			UserPixBuf;

		private Gtk.TreeView		SelTreeView;
		private Gtk.ListStore		SelTreeStore;

		private Gtk.Entry			SearchEntry;
		private Gtk.Button			UserAddButton;
		private Gtk.Button			UserDelButton;
		private uint				searchTimeoutID;
		private Hashtable			selectedUsers;

		public iFolderUser[] SelectedUsers
		{
			get
			{
				ArrayList list = new ArrayList();
				TreeIter iter;

				if(SelTreeStore.GetIterFirst(out iter))
				{
					do
					{
						iFolderUser user = (iFolderUser) 
											SelTreeStore.GetValue(iter,0);
						list.Add(user);
					}
					while(SelTreeStore.IterNext(ref iter));
				}

				return (iFolderUser[]) (list.ToArray(typeof(iFolderUser)));
			}
		}



		/// <summary>
		/// Default constructor for iFolderPropertiesDialog
		/// </summary>
		public iFolderUserSelector(	Gtk.Window parent,
									iFolderWebService iFolderWS,
									string domainID)
			: base()
		{
			this.Title = Util.GS("iFolder User Selector");
			if(iFolderWS == null)
				throw new ApplicationException("iFolderWebServices was null");
			this.ifws = iFolderWS;
			this.domainID = domainID;
			this.HasSeparator = false;
//			this.BorderWidth = 10;
			this.Resizable = true;
			this.Modal = true;
			if(parent != null)
				this.TransientFor = parent;

			InitializeWidgets();

			searchTimeoutID = 0;
			selectedUsers = new Hashtable();
		}




		/// <summary>
		/// Setup the UI inside the Window
		/// </summary>
		private void InitializeWidgets()
		{
			this.SetDefaultSize (500, 400);
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolderuser.png"));

			VBox dialogBox = new VBox();
			dialogBox.Spacing = 10;
			dialogBox.BorderWidth = 10;
			this.VBox.PackStart(dialogBox, true, true, 0);


			//------------------------------
			// Find Entry
			//------------------------------
			Table findTable = new Table(2, 2, false);
			dialogBox.PackStart(findTable, false, false, 0);
			findTable.ColumnSpacing = 20;
			findTable.RowSpacing = 5;

			Label findLabel = new Label(Util.GS("Find:"));
			findLabel.Xalign = 0;
			findTable.Attach(findLabel, 0, 1, 0, 1,
				AttachOptions.Shrink, 0, 0, 0);

			SearchEntry = new Gtk.Entry(Util.GS("<Enter text to find a user>"));
			SearchEntry.SelectRegion(0, -1);
			SearchEntry.CanFocus = true;
			SearchEntry.GrabFocus();
			SearchEntry.Changed += new EventHandler(OnSearchEntryChanged);
			findTable.Attach(SearchEntry, 1, 2, 0, 1,
				AttachOptions.Expand | AttachOptions.Fill, 0, 0, 0);
				
			Label findHelpTextLabel = new Label(Util.GS("(i.e. Full or partial name, or user ID)"));
			findHelpTextLabel.Xalign = 0;
			findTable.Attach(findHelpTextLabel, 1,2,1,2,
				AttachOptions.Expand | AttachOptions.Fill, 0, 0, 0);
			


			//------------------------------
			// Selection Area
			//------------------------------
			HBox selBox = new HBox();
			selBox.Spacing = 10;
			dialogBox.PackStart(selBox, true, true, 0);


			//------------------------------
			// All Users tree
			//------------------------------
			// Create the main TreeView and add it to a scrolled
			// window, then add it to the main vbox widget
			UserTreeView = new TreeView();
			ScrolledWindow sw = new ScrolledWindow();
			sw.ShadowType = Gtk.ShadowType.EtchedIn;
			sw.Add(UserTreeView);
			selBox.PackStart(sw, true, true, 0);

			// Setup the iFolder TreeView
			UserTreeStore = new ListStore(typeof(iFolderUser));
			UserTreeView.Model = UserTreeStore;
			UserTreeStore.SetSortColumnId(0, SortType.Ascending);
			UserTreeStore.SetSortFunc(0, 
					new TreeIterCompareFunc(TreeSortFunc), (System.IntPtr)0, 
					new DestroyNotify(TreeDestroyFunc));


			// Setup Pixbuf and Text Rendering for "iFolder Users" column
			CellRendererPixbuf mcrp = new CellRendererPixbuf();
			TreeViewColumn memberColumn = new TreeViewColumn();
			memberColumn.SortOrder = SortType.Ascending;
			memberColumn.PackStart(mcrp, false);
			memberColumn.SetCellDataFunc(mcrp, new TreeCellDataFunc(
						UserCellPixbufDataFunc));
			CellRendererText mcrt = new CellRendererText();
			memberColumn.PackStart(mcrt, false);
			memberColumn.SetCellDataFunc(mcrt, new TreeCellDataFunc(
						UserCellTextDataFunc));
			memberColumn.Title = Util.GS("iFolder Users");
			memberColumn.Resizable = true;
			UserTreeView.AppendColumn(memberColumn);
			UserTreeView.Selection.Mode = SelectionMode.Multiple;

			UserTreeView.Selection.Changed += new EventHandler(
						OnUserSelectionChanged);

			UserTreeView.RowActivated += new RowActivatedHandler(
						OnUserRowActivated);


			//------------------------------
			// Buttons
			//------------------------------
			VBox btnBox = new VBox();
			btnBox.Spacing = 10;
			selBox.PackStart(btnBox, false, true, 0);

			UserAddButton = new Button(Util.GS("_Add ->"));
			btnBox.PackStart(UserAddButton, false, true, 0);
			UserAddButton.Clicked += new EventHandler(OnAddButtonClicked);

			UserDelButton = new Button(Util.GS("_Remove"));
			btnBox.PackStart(UserDelButton, false, true, 0);
			UserDelButton.Clicked += new EventHandler(OnRemoveButtonClicked);


			//------------------------------
			// Selected Users tree
			//------------------------------
			SelTreeView = new TreeView();
			ScrolledWindow ssw = new ScrolledWindow();
			ssw.ShadowType = Gtk.ShadowType.EtchedIn;
			ssw.Add(SelTreeView);
			selBox.PackStart(ssw, true, true, 0);

			// Setup the iFolder TreeView
			SelTreeStore = new ListStore(typeof(iFolderUser));
			SelTreeView.Model = SelTreeStore;

			// Setup Pixbuf and Text Rendering for "iFolder Users" column
			CellRendererPixbuf smcrp = new CellRendererPixbuf();
			TreeViewColumn selmemberColumn = new TreeViewColumn();
			selmemberColumn.PackStart(smcrp, false);
			selmemberColumn.SetCellDataFunc(smcrp, new TreeCellDataFunc(
						UserCellPixbufDataFunc));
			CellRendererText smcrt = new CellRendererText();
			selmemberColumn.PackStart(smcrt, false);
			selmemberColumn.SetCellDataFunc(smcrt, new TreeCellDataFunc(
						UserCellTextDataFunc));
			selmemberColumn.Title = Util.GS("Selected Users");
			selmemberColumn.Resizable = true;
			SelTreeView.AppendColumn(selmemberColumn);
			SelTreeView.Selection.Mode = SelectionMode.Multiple;

			SelTreeView.Selection.Changed += new EventHandler(
						OnSelUserSelectionChanged);

//			UserTreeView.ButtonPressEvent += new ButtonPressEventHandler(
//						OnUserButtonPressed);

//			UserTreeView.RowActivated += new RowActivatedHandler(
//						OnUserRowActivated);


			UserPixBuf = 
				new Gdk.Pixbuf(Util.ImagesPath("ifolderuser.png"));

			this.AddButton(Stock.Cancel, ResponseType.Cancel);
			this.AddButton(Stock.Ok, ResponseType.Ok);
			this.AddButton(Stock.Help, ResponseType.Help);

			SearchiFolderUsers();
		}




		private void UserCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			iFolderUser user = (iFolderUser) tree_model.GetValue(iter,0);
			if( (user.FN != null) && (user.FN.Length > 0) )
				((CellRendererText) cell).Text = user.FN;
			else
				((CellRendererText) cell).Text = user.Name;
		}




		private void UserCellPixbufDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
//			iFolderUser user = (iFolderUser) tree_model.GetValue(iter,0);
			((CellRendererPixbuf) cell).Pixbuf = UserPixBuf;
		}




		public void OnUserSelectionChanged(object o, EventArgs args)
		{
			TreeSelection tSelect = UserTreeView.Selection;

			if(tSelect.CountSelectedRows() > 0)
			{
				UserAddButton.Sensitive = true;
			}
			else
			{
				UserAddButton.Sensitive = false;
			}
		}




		public void OnSelUserSelectionChanged(object o, EventArgs args)
		{
			TreeSelection tSelect = SelTreeView.Selection;

			if(tSelect.CountSelectedRows() > 0)
			{
				UserDelButton.Sensitive = true;
			}
			else
			{
				UserDelButton.Sensitive = false;
			}
		}




		public void OnSearchEntryChanged(object o, EventArgs args)
		{
			if(searchTimeoutID != 0)
			{
				Gtk.Timeout.Remove(searchTimeoutID);
				searchTimeoutID = 0;
			}

			searchTimeoutID = Gtk.Timeout.Add(500, new Gtk.Function(
						SearchCallback));
		}




		private bool SearchCallback()
		{
			SearchiFolderUsers();
			return false;
		}




		private void SearchiFolderUsers()
		{
			if (this.GdkWindow != null)
			{
				this.GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Watch);
			}

			UserTreeStore.Clear();

			if(SearchEntry.Text.Length > 0 && SearchEntry.Text != Util.GS("<Enter text to find a user>"))
			{
				iFolderUser[] userlist = 
						ifws.SearchForDomainUsers(domainID, SearchEntry.Text);
				foreach(iFolderUser user in userlist)
				{
					if(user != null)
					{
						UserTreeStore.AppendValues(user);
					}
				}
			}
			else
			{
				// Get the first 25 users, this should return none if
				// there are more than 25
				iFolderUser[] userlist = ifws.GetDomainUsers(domainID, 25);
				foreach(iFolderUser user in userlist)
				{
					UserTreeStore.AppendValues(user);
				}
			}

			UserAddButton.Sensitive = false;
			UserDelButton.Sensitive = false;

			if (this.GdkWindow != null)
			{
				this.GdkWindow.Cursor = null; // reset to parent's cursor (default)
			}
		}



		private void OnUserRowActivated(object o, RowActivatedArgs args)
		{
			OnAddButtonClicked(o, args);
		}



		public void OnAddButtonClicked(object o, EventArgs args)
		{
			TreeModel tModel;

			TreeSelection tSelect = UserTreeView.Selection;
			Array treePaths = tSelect.GetSelectedRows(out tModel);
			// remove compiler warning
			if(tModel != null)
				tModel = null;

			foreach(TreePath tPath in treePaths)
			{
				TreeIter iter;

				if(UserTreeStore.GetIter(out iter, tPath))
				{
					iFolderUser user = 
							(iFolderUser) UserTreeStore.GetValue(iter,0);

					if(!selectedUsers.ContainsKey(user.UserID))
					{
						selectedUsers.Add(user.UserID, user);
						SelTreeStore.AppendValues(user);
					}
				}
			}
		}


		public void OnRemoveButtonClicked(object o, EventArgs args)
		{
			TreeModel tModel;
			Queue   iterQueue;

			iterQueue = new Queue();
			TreeSelection tSelect = SelTreeView.Selection;
			Array treePaths = tSelect.GetSelectedRows(out tModel);
			// remove compiler warning
			if(tModel != null)
				tModel = null;

			// We can't remove anything while getting the iters
			// because it will change the paths and we'll remove
			// the wrong stuff.
			foreach(TreePath tPath in treePaths)
			{
				TreeIter iter;

				if(SelTreeStore.GetIter(out iter, tPath))
				{
					iterQueue.Enqueue(iter);
				}
			}

			// Now that we have all of the TreeIters, loop and
			// remove them all
			while(iterQueue.Count > 0)
			{
				TreeIter iter = (TreeIter) iterQueue.Dequeue();
				iFolderUser user = 
						(iFolderUser) SelTreeStore.GetValue(iter,0);
				selectedUsers.Remove(user.UserID);
				SelTreeStore.Remove(ref iter);
			}
		}

		private int TreeSortFunc(TreeModel model, TreeIter a, TreeIter b)
		{
			iFolderUser userA = 
					(iFolderUser) model.GetValue(a,0);
			iFolderUser userB = 
					(iFolderUser) model.GetValue(b,0);
	
			string stringA, stringB;

			if(userA.Surname != null)
				stringA = userA.Surname;
			else
				stringA = userA.Name;

			if(userB.Surname != null)
				stringB = userB.Surname;
			else
				stringB = userB.Name;
				
			return string.Compare(stringA, stringB);
		}

		private void TreeDestroyFunc()
		{
		}
		
	}
}
