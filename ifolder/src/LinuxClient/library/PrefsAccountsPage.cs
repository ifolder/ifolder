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
using System.Collections;
using Gtk;
using Simias.Client.Event;

namespace Novell.iFolder
{

	/// <summary>
	/// A VBox with the ability to create and manage ifolder accounts
	/// </summary>
	public class PrefsAccountsPage : VBox
	{
		private Gtk.Window				topLevelWindow;
		private iFolderWebService		ifws;

		private iFolderTreeView		AccTreeView;
		private ListStore			AccTreeStore;
		private Gdk.Pixbuf			UserPixBuf;

		private Button 				AddButton;
		private Button				RemoveButton;
		private Button				DetailsButton;


		/// <summary>
		/// Default constructor for iFolderAccountsPage
		/// </summary>
		public PrefsAccountsPage(	Gtk.Window topWindow,
									iFolderWebService webService)
			: base()
		{
			this.topLevelWindow = topWindow;
			this.ifws = webService;
			InitializeWidgets();
			this.Realized += new EventHandler(OnRealizeWidget);
		}



		/// <summary>
		/// Setup the widgets
		/// </summary>
		/// <returns>
		/// Widget to display
		/// </returns>
		private void InitializeWidgets()
		{
			this.Spacing = 10;
			this.BorderWidth = 10;
			
			// Create the main TreeView and add it to a scrolled
			// window, then add it to the main vbox widget
			AccTreeView = new iFolderTreeView();
			ScrolledWindow sw = new ScrolledWindow();
			sw.ShadowType = Gtk.ShadowType.EtchedIn;
			sw.Add(AccTreeView);
			this.PackStart(sw, true, true, 0);


			// Setup the iFolder TreeView
			AccTreeStore = new ListStore(typeof(DomainWeb));
			AccTreeView.Model = AccTreeStore;

			CellRendererPixbuf ncrp = new CellRendererPixbuf();
			TreeViewColumn NameColumn = new TreeViewColumn();
			NameColumn.PackStart(ncrp, false);
			NameColumn.SetCellDataFunc(ncrp,
					new TreeCellDataFunc(NameCellPixbufDataFunc));

			CellRendererText ncrt = new CellRendererText();
			NameColumn.PackStart(ncrt, false);
			NameColumn.SetCellDataFunc(ncrt,
					new TreeCellDataFunc(NameCellTextDataFunc));

			NameColumn.Title = Util.GS("User Name");
			AccTreeView.AppendColumn(NameColumn);
			NameColumn.Resizable = true;

			CellRendererText servercr = new CellRendererText();
			servercr.Xpad = 5;
			TreeViewColumn serverColumn = 
			AccTreeView.AppendColumn(Util.GS("Server"),
					servercr,
					new TreeCellDataFunc(ServerCellTextDataFunc));
			serverColumn.Resizable = true;
			serverColumn.MinWidth = 150;

			AccTreeView.Selection.Mode = SelectionMode.Single;
//			AccTreeView.Selection.Changed +=
//				new EventHandler(OnAccSelectionChanged);

//			AccTreeView.ButtonPressEvent += new ButtonPressEventHandler(
//						OnAccTreeViewButtonPressed);

			UserPixBuf = 
					new Gdk.Pixbuf(Util.ImagesPath("ifolderuser.png"));

			// Setup buttons for add/remove/accept/decline
			HBox buttonBox = new HBox();
			buttonBox.Spacing = 10;
			this.PackStart(buttonBox, false, false, 0);

			HBox leftBox = new HBox();
			leftBox.Spacing = 10;
			buttonBox.PackStart(leftBox, false, false, 0);
			HBox midBox = new HBox();
			midBox.Spacing = 10;
			buttonBox.PackStart(midBox, true, true, 0);
			HBox rightBox = new HBox();
			rightBox.Spacing = 10;
			buttonBox.PackStart(rightBox, false, false, 0);

			AddButton = new Button(Gtk.Stock.Add);
			rightBox.PackStart(AddButton);
			AddButton.Clicked += new EventHandler(OnAddAccount);

			RemoveButton = new Button(Gtk.Stock.Remove);
			rightBox.PackStart(RemoveButton);
			RemoveButton.Clicked += new EventHandler(OnRemoveAccount);

			DetailsButton = new Button(Util.GS("_Details"));
			leftBox.PackStart(DetailsButton);
			DetailsButton.Clicked += new EventHandler(OnDetailsClicked);
		}




		/// <summary>
		/// Set the Values in the Widgets
		/// </summary>
		private void PopulateWidgets()
		{
			// Read all current domains before letting them create
			// a new ifolder
			try
			{
				DomainWeb[] domains;
				domains = ifws.GetDomains();

				foreach(DomainWeb dom in domains)
				{
					AccTreeStore.AppendValues(dom);
				}
			}
			catch(Exception e)
			{
				iFolderExceptionDialog ied = new iFolderExceptionDialog(
													topLevelWindow, e);
				ied.Run();
				ied.Hide();
				ied.Destroy();
				return;
			}
		}




		private void OnRealizeWidget(object o, EventArgs args)
		{
			PopulateWidgets();
		}




		private void NameCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			DomainWeb dom = (DomainWeb) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = dom.UserName;
		}




		private void NameCellPixbufDataFunc(Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			((CellRendererPixbuf) cell).Pixbuf = UserPixBuf;
		}




		private void ServerCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			DomainWeb dom = (DomainWeb) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = dom.Name;
		}




		private void OnAddAccount(object o, EventArgs args)
		{
			AccountDialog accDialog = new AccountDialog();
			accDialog.TransientFor = topLevelWindow;
			accDialog.Run();
			accDialog.Hide();
			accDialog.Destroy();
		}



		private void OnRemoveAccount(object o, EventArgs args)
		{
			TreeModel tModel;

			TreeSelection tSelect = AccTreeView.Selection;

			if(tSelect.CountSelectedRows() == 1)
			{
				iFolderMsgDialog dialog = new iFolderMsgDialog(
					topLevelWindow,
					iFolderMsgDialog.DialogType.Question,
					iFolderMsgDialog.ButtonSet.YesNo,
					Util.GS("iFolder Confirmation"),
					Util.GS("Remove Selected Account?"),
					Util.GS("This will remove the selected account from the list and you will no longer be able to sync to it"));
				int rc = dialog.Run();
				dialog.Hide();
				dialog.Destroy();
				if(rc == -8)
				{
					Queue   iterQueue;
					Array treePaths = tSelect.GetSelectedRows(out tModel);

					iterQueue = new Queue();
	
					foreach(TreePath tPath in treePaths)
					{
						TreeIter iter;
		
						if(AccTreeStore.GetIter(out iter, tPath))
						{
							iterQueue.Enqueue(iter);
						}
					}

					// Now that we have all of the TreeIters, loop and
					// remove them all
					while(iterQueue.Count > 0)
					{
						TreeIter iter = (TreeIter) iterQueue.Dequeue();

/*						iFolderUser user = 
								(iFolderUser) tModel.GetValue(iter, 0);
		
						try
						{
   			 				ifws.RemoveiFolderUser(ifolder.ID,
													user.UserID);
							AccTreeStore.Remove(ref iter);
							curUsers.Remove(user.UserID);
						}
						catch(Exception e)
						{
							iFolderExceptionDialog ied = 
									new iFolderExceptionDialog(
											topLevelWindow, e);
							ied.Run();
							ied.Hide();
							ied.Destroy();
							ied = null;
						}
*/
					}
				}
			}
		}




		private void OnDetailsClicked(object o, EventArgs args)
		{
			TreeSelection tSelect = AccTreeView.Selection;

			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				DomainWeb dom = 
						(DomainWeb) tModel.GetValue(iter, 0);

				AccountDialog accDialog = new AccountDialog(dom);
				accDialog.TransientFor = topLevelWindow;
				accDialog.Run();
				accDialog.Hide();
				accDialog.Destroy();
			}
		}







	}
}
