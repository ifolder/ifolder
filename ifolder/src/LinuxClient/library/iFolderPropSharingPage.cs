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
	/// This is the properties page for sharing an iFolder
	/// </summary>
	public class iFolderPropSharingPage : VBox
	{
		private iFolderWebService	ifws;
		private iFolderWeb			ifolder;

		private iFolderTreeView		UserTreeView;
		private ListStore			UserTreeStore;
		private Gdk.Pixbuf			UserPixBuf;
		private Gdk.Pixbuf			CurrentUserPixBuf;
		private Gdk.Pixbuf			InvitedPixBuf;
		private Gtk.Window			topLevelWindow;

		private Button 				AddButton;
		private Button				RemoveButton;
		private Button				AccessButton;
		private iFolderUserSelector UserSelector;
		private Hashtable			curUsers;

		/// <summary>
		/// Default constructor for iFolderPropSharingPage
		/// </summary>
		public iFolderPropSharingPage(	Gtk.Window topWindow,
										iFolderWebService iFolderWS)
			: base()
		{
			this.ifws = iFolderWS;
			this.topLevelWindow = topWindow;
			curUsers = new Hashtable();
			InitializeWidgets();
		}




		public void UpdateiFolder(iFolderWeb ifolder)
		{
			this.ifolder = ifolder;

			RefreshUserList();
			UpdateWidgets();
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
			UserTreeView = new iFolderTreeView();
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
			UserColumn.Title = Util.GS("Users");
			UserTreeView.AppendColumn(UserColumn);
			UserColumn.Resizable = true;

			CellRendererText statecr = new CellRendererText();
			statecr.Xpad = 5;
			TreeViewColumn stateColumn = 
			UserTreeView.AppendColumn(Util.GS("State"),
					statecr,
					new TreeCellDataFunc(StateCellTextDataFunc));
			stateColumn.Resizable = true;
			stateColumn.MinWidth = 150;

			CellRendererText accesscr = new CellRendererText();
			accesscr.Xpad = 5;
			TreeViewColumn accessColumn = 
			UserTreeView.AppendColumn(Util.GS("Access"),
					accesscr,
					new TreeCellDataFunc(AccessCellTextDataFunc));
			accessColumn.Resizable = true;

			UserTreeView.Selection.Mode = SelectionMode.Multiple;
			UserTreeView.Selection.Changed +=
				new EventHandler(OnUserSelectionChanged);

			UserTreeView.ButtonPressEvent += new ButtonPressEventHandler(
						OnUserTreeViewButtonPressed);

			UserPixBuf = 
					new Gdk.Pixbuf(Util.ImagesPath("ifolderuser.png"));
			InvitedPixBuf = 
					new Gdk.Pixbuf(Util.ImagesPath("inviteduser.png"));
			CurrentUserPixBuf = 
					new Gdk.Pixbuf(Util.ImagesPath("currentuser.png"));

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
			RemoveButton.Clicked += new EventHandler(OnRemoveUser);

			AccessButton = new Button(Util.GS("S_et Access"));
			leftBox.PackStart(AccessButton);
			AccessButton.Clicked += new EventHandler(OnAccessClicked);
		}




		private void RefreshUserList()
		{
			curUsers.Clear();
			UserTreeStore.Clear();

    		iFolderUser[] userlist =  ifws.GetiFolderUsers(ifolder.ID);
			foreach(iFolderUser user in userlist)
			{
				if(!curUsers.ContainsKey(user.UserID))
				{
					TreeIter iter = UserTreeStore.AppendValues(user);
					curUsers.Add(user.UserID, iter);
				}
			}
		}
		


		private void UpdateWidgets()
		{
			OnUserSelectionChanged(null, null);
		}




		private void UserCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			iFolderUser user = (iFolderUser) tree_model.GetValue(iter,0);
			if(user.FN != null)
				((CellRendererText) cell).Text = user.FN;
			else
				((CellRendererText) cell).Text = user.Name;
		}




		private void UserCellPixbufDataFunc(Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			iFolderUser user = (iFolderUser) tree_model.GetValue(iter,0);
			if(user.UserID == ifolder.CurrentUserID)
				((CellRendererPixbuf) cell).Pixbuf = CurrentUserPixBuf;
			else if(user.State != "Member")
				((CellRendererPixbuf) cell).Pixbuf = InvitedPixBuf;
			else
				((CellRendererPixbuf) cell).Pixbuf = UserPixBuf;
		}




		private void StateCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			iFolderUser user = (iFolderUser) tree_model.GetValue(iter,0);
			if(ifolder.OwnerID == user.UserID)
				((CellRendererText) cell).Text = Util.GS("Owner");
			else if(user.State != "Member")
				((CellRendererText) cell).Text = Util.GS("Invited User");
			else
				((CellRendererText) cell).Text = Util.GS("iFolder User");
		}




		private void AccessCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			iFolderUser user = (iFolderUser) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = GetDisplayRights(user.Rights);
		}




		private void OnAddUser(object o, EventArgs args)
		{
			UserSelector = new iFolderUserSelector( topLevelWindow, 
													ifws,
													ifolder.DomainID);

			UserSelector.Response += 
						new ResponseHandler(OnUserSelectorResponse);

			UserSelector.ShowAll();
		}



		private void OnRemoveUser(object o, EventArgs args)
		{
			TreeModel tModel;

			TreeSelection tSelect = UserTreeView.Selection;

			if(tSelect.CountSelectedRows() > 0)
			{
				iFolderMsgDialog dialog = new iFolderMsgDialog(
					topLevelWindow,
					iFolderMsgDialog.DialogType.Question,
					iFolderMsgDialog.ButtonSet.YesNo,
					Util.GS("iFolder Confirmation"),
					Util.GS("Remove Selected Users?"),
					Util.GS("This will remove the selected users from this iFolder.  They will no longer be able to sync file to this iFolder."));
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
		
						if(UserTreeStore.GetIter(out iter, tPath))
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
								(iFolderUser) tModel.GetValue(iter, 0);
		
						try
						{
   			 				ifws.RemoveiFolderUser(ifolder.ID,
													user.UserID);
							UserTreeStore.Remove(ref iter);
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
					}
				}
			}
		}




		private void OnAccessClicked(object o, EventArgs args)
		{
			TreeModel tModel;
			iFolderAccessDialog accDialog = null;
			string defaultRights = "ReadWrite";
			string userName = null;
			bool allowOwner = false;

			TreeSelection tSelect = UserTreeView.Selection;

			// only allow the changing of the owner if the current
			// user is the owner and if the selected users are members
			if(tSelect.CountSelectedRows() == 1)
			{
				Array treePaths = tSelect.GetSelectedRows(out tModel);

				foreach(TreePath tPath in treePaths)
				{
					TreeIter iter;

					if(UserTreeStore.GetIter(out iter, tPath))
					{
						iFolderUser user = 
								(iFolderUser) tModel.GetValue(iter, 0);
						userName = user.Name;
						defaultRights = user.Rights;

						if( (ifolder.CurrentUserID == ifolder.OwnerID) &&
							(user.State == "Member") )
							allowOwner = true;
					}
					break;
				}
			}

			accDialog = new iFolderAccessDialog( 
				topLevelWindow, userName, defaultRights, allowOwner);

			int rc = accDialog.Run();
			accDialog.Hide();
			if(rc == -5)
			{
				string newrights = accDialog.Rights;
				string oldOwnerID;

				Array treePaths = tSelect.GetSelectedRows(out tModel);

				foreach(TreePath tPath in treePaths)
				{
					TreeIter iter;

					if(UserTreeStore.GetIter(out iter, tPath))
					{
						iFolderUser user = 
								(iFolderUser) tModel.GetValue(iter,0);

						try
						{
    						ifws.SetUserRights( ifolder.ID,
												user.UserID,
												newrights);
							user.Rights = newrights;
		
							// if the user selected to make this
							// use the owner set that right now
							if(accDialog.IsOwner)
							{
								ifws.ChangeOwner(	ifolder.ID,
													user.UserID,
													"Admin");

								// update the objects here instead of
								// re-reading them, that's expensive!
								oldOwnerID = ifolder.OwnerID;
								user.Rights = "Admin";
								ifolder.Owner = user.Name;
								ifolder.OwnerID = user.UserID;

								// now loop through the users, find
								// the current owner, and set him to
								// not be the owner any more
								TreeIter ownIter;
								if(UserTreeStore.GetIterFirst(out ownIter))
								{
									do
									{
										iFolderUser ownUser = (iFolderUser) 
											UserTreeStore.GetValue(ownIter,0);
										if(oldOwnerID == ownUser.UserID)
										{
											ownUser.Rights = "Admin";
											tModel.SetValue(ownIter, 
																0, ownUser);
											break;
										}
									}
									while(UserTreeStore.IterNext(ref ownIter));
								}

							}

							tModel.SetValue(iter, 0, user);
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
					}
				}
			}
			accDialog.Destroy();
			accDialog = null;
		}




		private void OnUserSelectionChanged(object o, EventArgs args)
		{
			if(ifolder.CurrentUserRights != "Admin")
			{
				AddButton.Sensitive = false;
				RemoveButton.Sensitive = false;
				AccessButton.Sensitive = false;
			}
			else
			{
				if(!ifolder.IsWorkgroup)
				{
					AddButton.Sensitive = true;
				}
				else
				{
					AddButton.Sensitive = false;
				}

				TreeSelection tSelect = UserTreeView.Selection;
				if((tSelect.CountSelectedRows() < 1) || 
					SelectionHasOwnerOrCurrent() )
				{
					RemoveButton.Sensitive = false;
					AccessButton.Sensitive = false;
				}
				else
				{
					RemoveButton.Sensitive = true;
					AccessButton.Sensitive = true;
				}
			}
		}




		private void OnUserSelectorResponse(object o, ResponseArgs args)
		{
			if(UserSelector != null)
			{
				switch(args.ResponseId)
				{
					case Gtk.ResponseType.Ok:
					{
						foreach(iFolderUser user in UserSelector.SelectedUsers)
						{
							if(!curUsers.ContainsKey(user.UserID))
							{
								try
								{
    								iFolderUser newUser = ifws.InviteUser(
															ifolder.ID,
															user.UserID,
															"ReadWrite");
	
									TreeIter iter = 
										UserTreeStore.AppendValues(newUser);
	
									curUsers.Add(newUser.UserID, iter);
	
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
						break;
					}
					case Gtk.ResponseType.Help:
					{
						Util.ShowHelp("front.html", topLevelWindow);
						break;
					}
					case Gtk.ResponseType.Cancel:
					{
						UserSelector.Hide();
						UserSelector.Destroy();
						UserSelector = null;
						break;
					}
				}
			}
		}




		private string GetDisplayRights(string rights)
		{
			if(rights == "ReadWrite")
				return Util.GS("Read Write");
			else if(rights == "Admin")
				return Util.GS("Full Control");
			else if(rights == "ReadOnly")
				return Util.GS("Read Only");
			else
				return Util.GS("Unknown");
		}




		public bool SelectionHasOwnerOrCurrent()
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
					if(user.UserID == ifolder.OwnerID)
						return true;
					if(user.UserID == ifolder.CurrentUserID)
						return true;
				}
			}
			return false;
		}




		public void OnUserTreeViewButtonPressed(object obj, 
											ButtonPressEventArgs args)
		{
			switch(args.Event.Button)
			{
				case 1: // first mouse button
					break;
				case 2: // second mouse button
					break;
				case 3: // third mouse button
				{
					TreePath tPath = null;
					TreeViewColumn tColumn = null;
//					TreeModel tModel;

					if(UserTreeView.GetPathAtPos(	(int)args.Event.X,
													(int)args.Event.Y,
													out tPath,
													out tColumn) == true)
					{
						// User clicked in a column with data
						if(UserTreeView.GetColumn(2) == tColumn)
						{
							TreeSelection tSelect = UserTreeView.Selection;

							if( (ifolder.CurrentUserRights == "Admin") &&
								(tSelect.CountSelectedRows() > 0) )
							{
								Menu rightsMenu = new Menu();
			
								RadioMenuItem adminItem = 
									new RadioMenuItem (Util.GS("Full Control"));
								rightsMenu.Append(adminItem);

								RadioMenuItem rwItem = 
									new RadioMenuItem (adminItem.Group, 
														Util.GS("Read/Write"));
								rightsMenu.Append(rwItem);

								RadioMenuItem roItem = 
									new RadioMenuItem (adminItem.Group, 
														Util.GS("Read Only"));
								rightsMenu.Append(roItem);

								if(SelectionHasOwnerOrCurrent())
								{
									adminItem.Sensitive = false;
									rwItem.Sensitive = false;
									roItem.Sensitive = false;
								}

								// Get the Value of the actual user selected
								TreeIter iter;
		
								if(UserTreeStore.GetIter(out iter, tPath))
								{
									iFolderUser user = (iFolderUser) 
											UserTreeStore.GetValue(iter, 0);
									if(user.Rights == "ReadWrite")
										rwItem.Active = true;
									else if(user.Rights == "Admin")
										adminItem.Active = true;
									else
										roItem.Active = true;
								}

								adminItem.Activated += new EventHandler(
										OnAdminRightsMenu);
								rwItem.Activated += new EventHandler(
										OnRWRightsMenu);
								roItem.Activated += new EventHandler(
										OnRORightsMenu);

								rightsMenu.ShowAll();

								rightsMenu.Popup(null, null, null, 
									IntPtr.Zero, 3, 
									Gtk.Global.CurrentEventTime);
							}
						}
					}
					break;
				}
			}
		}

		private void OnAdminRightsMenu(object o, EventArgs args)
		{
			SetSelectedUserRights("Admin");
		}

		private void OnRWRightsMenu(object o, EventArgs args)
		{
			SetSelectedUserRights("ReadWrite");
		}

		private void OnRORightsMenu(object o, EventArgs args)
		{
			SetSelectedUserRights("ReadOnly");
		}

		private void SetSelectedUserRights(string rights)
		{
			TreeModel tModel;
			
			// User clicked on rights
			TreeSelection tSelect = UserTreeView.Selection;

			Array treePaths = 
					tSelect.GetSelectedRows(out tModel);

			foreach(TreePath tPath in treePaths)
			{
				TreeIter iter;
		
				if(UserTreeStore.GetIter(out iter, tPath))
				{
					iFolderUser user = 
						(iFolderUser) tModel.GetValue(iter, 0);

					try
					{
    					ifws.SetUserRights( ifolder.ID,
											user.UserID,
											rights);
						user.Rights = rights;

						tModel.SetValue(iter, 0, user);
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
				}
			}
		}




	}
}
