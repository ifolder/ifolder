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

namespace Novell.iFolder
{

	/// <summary>
	/// This is the properties page for sharing an iFolder
	/// </summary>
	public class iFolderPropSharingPage : VBox
	{
		private iFolderWebService	ifws;
		private iFolder				ifolder;

		private Gtk.TreeView		UserTreeView;
		private ListStore			UserTreeStore;
		private Gdk.Pixbuf			UserPixBuf;
		private Gdk.Pixbuf			InvitedPixBuf;
		private Gtk.Window			topLevelWindow;

		private Button 				AddButton;
		private Button				RemoveButton;
		private Button				AccessButton;
		private iFolderUserSelector UserSelector;

		/// <summary>
		/// Default constructor for iFolderPropSharingPage
		/// </summary>
		public iFolderPropSharingPage(	Gtk.Window topWindow,
										iFolder ifolder, 
										iFolderWebService iFolderWS)
			: base()
		{
			this.ifws = iFolderWS;
			this.ifolder = ifolder;
			this.topLevelWindow = topWindow;
			InitializeWidgets();
		}




		/// <summary>
		/// Setup the UI inside the Window
		/// </summary>
		private void InitializeWidgets()
		{
			this.Spacing = 10;
			this.BorderWidth = 10;
			
			// Create the main TreeView and add it to a scrolled
			// window, then add it to the main vbox widget
			UserTreeView = new TreeView();
			ScrolledWindow sw = new ScrolledWindow();
			sw.ShadowType = Gtk.ShadowType.EtchedIn;
			sw.Add(UserTreeView);
			this.PackStart(sw, true, true, 0);


			// Setup the iFolder TreeView
			UserTreeStore = new ListStore(typeof(iFolderUser));
			UserTreeView.Model = UserTreeStore;

			CellRendererPixbuf mcrp = new CellRendererPixbuf();
			TreeViewColumn UserColumn = new TreeViewColumn();
			UserColumn.PackStart(mcrp, false);
			UserColumn.SetCellDataFunc(mcrp,
					new TreeCellDataFunc(UserCellPixbufDataFunc));

			CellRendererText mcrt = new CellRendererText();
			UserColumn.PackStart(mcrt, false);
			UserColumn.SetCellDataFunc(mcrt,
					new TreeCellDataFunc(UserCellTextDataFunc));
			UserColumn.Title = "Users";
			UserTreeView.AppendColumn(UserColumn);
			UserColumn.Resizable = true;

			CellRendererText statecr = new CellRendererText();
//			statecr.Xalign = 1;
			statecr.Xpad = 10;
			TreeViewColumn stateColumn = 
			UserTreeView.AppendColumn("State",
					statecr,
					new TreeCellDataFunc(StateCellTextDataFunc));
//			stateColumn.Alignment = 1;
			stateColumn.Resizable = true;
			stateColumn.MinWidth = 200;

			CellRendererText accesscr = new CellRendererText();
//			accesscr.Xalign = 1;
			accesscr.Xpad = 10;
			TreeViewColumn accessColumn = 
			UserTreeView.AppendColumn("Access",
					accesscr,
					new TreeCellDataFunc(AccessCellTextDataFunc));
//			accessColumn.Alignment = 1;
			accessColumn.Resizable = true;

//			UserTreeView.Selection.Changed +=
//				new EventHandler(OnUserSelectionChanged);

			UserPixBuf = 
					new Gdk.Pixbuf(Util.ImagesPath("ifolderuser.png"));
			InvitedPixBuf = 
					new Gdk.Pixbuf(Util.ImagesPath("inviteduser.png"));

//			CurContactPixBuf = 
//					new Gdk.Pixbuf(Util.ImagesPath("contact_me.png"));


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
			AddButton.Clicked += new EventHandler(OnAddUser);

			RemoveButton = new Button(Gtk.Stock.Remove);
			rightBox.PackStart(RemoveButton);

			AccessButton = new Button("Set Access");
			leftBox.PackStart(AccessButton);

			RefreshUserList();
		}


		private void RefreshUserList()
		{
    		iFolderUser[] userlist =  ifws.GetiFolderUsers(ifolder.ID);
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




		private void UserCellPixbufDataFunc(Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			iFolderUser user = (iFolderUser) tree_model.GetValue(iter,0);
			if(user.State != "Member")
				((CellRendererPixbuf) cell).Pixbuf = InvitedPixBuf;
			else
				((CellRendererPixbuf) cell).Pixbuf = UserPixBuf;
		}




		private void StateCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			iFolderUser user = (iFolderUser) tree_model.GetValue(iter,0);
			if(user.State != "Member")
				((CellRendererText) cell).Text = "Invited User";
			else
				((CellRendererText) cell).Text = "iFolder User";
		}


		private void AccessCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			iFolderUser user = (iFolderUser) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = user.Rights;
		}

		private void OnAddUser(object o, EventArgs args)
		{
			UserSelector = new iFolderUserSelector( topLevelWindow, ifws);

			UserSelector.Response += 
						new ResponseHandler(OnUserSelectorResponse);

			UserSelector.ShowAll();
		}

		private void OnUserSelectorResponse(object o, ResponseArgs args)
		{
			if(UserSelector != null)
			{
				if(args.ResponseId == Gtk.ResponseType.Ok)
				{
					foreach(iFolderUser user in UserSelector.SelectedUsers)
					{
						Console.WriteLine("User: {0}", user.Name);
						try
						{
    						iFolderUser newUser = ifws.InviteUser(
													ifolder.ID,
													user.UserID,
													"ReadWrite");

							UserTreeStore.AppendValues(newUser);
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
							break;
						}
					}
				}

				UserSelector.Hide();
				UserSelector.Destroy();
				UserSelector = null;
			}
		}
	}
}
