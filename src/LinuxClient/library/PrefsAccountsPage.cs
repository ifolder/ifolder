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
using Simias.Client;
using Simias.Client.Authentication;

namespace Novell.iFolder
{

	/// <summary>
	/// A VBox with the ability to create and manage ifolder accounts
	/// </summary>
	public class PrefsAccountsPage : VBox
	{
		private Gtk.Window				topLevelWindow;
		private iFolderData				ifdata;
		private SimiasWebService		simws;

		private iFolderTreeView		AccTreeView;
		private ListStore			AccTreeStore;

		private Button 				AddButton;
		private Button				RemoveButton;
		private Button				DetailsButton;
		private bool				NewAccountMode;
 
 		private Frame		detailsFrame;
		private Label		nameLabel;
		private Entry		nameEntry;
		private Label		passLabel;
		private Entry		passEntry;
		private Label		serverLabel;
		private Entry		serverEntry;
		private CheckButton	savePasswordButton;
		private CheckButton	autoLoginButton;
		private CheckButton	defaultAccButton;
//		private Button		proxyButton;
		private Button		loginButton;
		private bool		isFirstDomain;

		private string				curDomainPassword;
		private DomainInformation	curDomain;

		/// <summary>
		/// Default constructor for iFolderAccountsPage
		/// </summary>
		public PrefsAccountsPage( Gtk.Window topWindow )
			: base()
		{
			this.topLevelWindow = topWindow;
			this.simws = new SimiasWebService();
			simws.Url = Simias.Client.Manager.LocalServiceUrl.ToString() +
					"/Simias.asmx";

			ifdata = iFolderData.GetData();

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
			
			NewAccountMode = false;

			// Setup the Accounts tree view in a scrolled window
			AccTreeView = new iFolderTreeView();
			ScrolledWindow sw = new ScrolledWindow();
			sw.ShadowType = Gtk.ShadowType.EtchedIn;
			sw.Add(AccTreeView);
			this.PackStart(sw, true, true, 0);

			AccTreeStore = new ListStore(typeof(DomainInformation));
			AccTreeView.Model = AccTreeStore;

			TreeViewColumn NameColumn = new TreeViewColumn();
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
			AccTreeView.Selection.Changed +=
				new EventHandler(AccSelectionChangedHandler);

			// Setup buttons for add/remove/accept/decline
			HButtonBox buttonBox = new HButtonBox();
			buttonBox.Spacing = 10;
			buttonBox.Layout = ButtonBoxStyle.End;
			this.PackStart(buttonBox, false, false, 0);

			AddButton = new Button(Gtk.Stock.Add);
			buttonBox.PackStart(AddButton);
			AddButton.Clicked += new EventHandler(OnAddAccount);

			RemoveButton = new Button(Gtk.Stock.Remove);
			buttonBox.PackStart(RemoveButton);
			RemoveButton.Clicked += new EventHandler(OnRemoveAccount);

			DetailsButton = new Button(Util.GS("_Details"));
			buttonBox.PackStart(DetailsButton);
			DetailsButton.Clicked += new EventHandler(OnDetailsClicked);


			detailsFrame = new Frame(Util.GS("Account Settings"));
			this.PackStart(detailsFrame, false, false, 0);

			VBox vbox = new VBox();
			vbox.BorderWidth = 10;
			detailsFrame.Add(vbox);


			Table loginTable;
			loginTable = new Table(5,2,false);

			loginTable.RowSpacing = 10;
			loginTable.ColumnSpacing = 10;
			loginTable.Homogeneous = false;


			// 1. Server host
			serverLabel = new Label(Util.GS("Server host:"));
			serverLabel.Xalign = 1;
			loginTable.Attach(serverLabel, 0,1,0,1,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			serverEntry = new Entry();
			serverEntry.Changed += new EventHandler(OnFieldsChanged);
			serverEntry.ActivatesDefault = true;
			loginTable.Attach(serverEntry, 1,2,0,1,
					AttachOptions.Fill | AttachOptions.Expand, 0,0,0);


			// 2. User name
			nameLabel = new Label(Util.GS("User name:"));
			nameLabel.Xalign = 1; 
			loginTable.Attach(nameLabel, 0,1,1,2,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			nameEntry = new Entry();
			nameEntry.Changed += new EventHandler(OnFieldsChanged);
			nameEntry.ActivatesDefault = true;
			nameEntry.WidthChars = 35;
			loginTable.Attach(nameEntry, 1,2,1,2, 
					AttachOptions.Fill | AttachOptions.Expand, 0,0,0);


			// 3. Password
			passLabel = new Label(Util.GS("Password:"));
			passLabel.Xalign = 1;
			loginTable.Attach(passLabel, 0,1,2,3,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			passEntry = new Entry();
			passEntry.Changed += new EventHandler(OnFieldsChanged);
			passEntry.Activated += new EventHandler(OnPassEntryActivated);
			passEntry.FocusOutEvent += 
					new FocusOutEventHandler(OnPassEntryFoucusOut);
			passEntry.ActivatesDefault = true;
			passEntry.Visibility = false;
			loginTable.Attach(passEntry, 1,2,2,3,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);


			VBox optBox = new VBox();

			savePasswordButton = 
				new CheckButton(Util.GS(
					"_Remember password"));
			optBox.PackStart(savePasswordButton, false, false,0);
			savePasswordButton.Toggled += 
							new EventHandler(OnSavePasswordToggled);


			autoLoginButton = 
				new CheckButton(Util.GS(
					"_Enable account"));
			optBox.PackStart(autoLoginButton, false, false,0);
			autoLoginButton.Toggled += 
							new EventHandler(OnAutoLoginToggled);

			defaultAccButton = 
				new CheckButton(Util.GS(
					"D_efault account"));
			optBox.PackStart(defaultAccButton, false, false,0);
			defaultAccButton.Toggled += 
							new EventHandler(OnDefAccToggled);

			loginTable.Attach(optBox, 1,2,3,4,
					AttachOptions.Fill | AttachOptions.Expand, 0,0,0);


/*
			HBox proxyBox = new HBox();

			proxyButton =
				new Button(Util.GS("Pro_xy settings"));
			proxyBox.PackStart(proxyButton, false, false, 0);

			loginTable.Attach(proxyBox, 1,2,4,5,
					AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
*/


			vbox.PackStart(loginTable, true, true, 0);


			HButtonBox loginBox = new HButtonBox();
			loginBox.Spacing = 10;
			loginBox.Layout = ButtonBoxStyle.End;

			loginButton =
				new Button(Util.GS("_Activate"));
			loginBox.PackStart(loginButton, false, false, 0);
			loginButton.Clicked += new EventHandler(OnLoginAccount);
			loginButton.CanDefault = true;


			AccTreeView.RowActivated += new RowActivatedHandler(
						OnAccTreeRowActivated);



			vbox.PackStart(loginBox, false, false, 0);

		}




		/// <summary>
		/// Set the Values in the Widgets
		/// </summary>
		private void PopulateWidgets()
		{
			// Read all current domains before letting them create
			// a new ifolder

			isFirstDomain = true;
			DomainInformation[] domains = ifdata.GetDomains();

			foreach(DomainInformation dom in domains)
			{
				// only show Domains that are slaves (not on this machine)
				if(dom.IsSlave)
				{
					isFirstDomain = false;
					AccTreeStore.AppendValues(dom);
				}
			}

 			detailsFrame.Sensitive = false;
			nameEntry.Sensitive = false;
			nameLabel.Sensitive = false;
			passEntry.Sensitive = false;
			passLabel.Sensitive = false;
			serverEntry.Sensitive = false;
			serverLabel.Sensitive = false;

			savePasswordButton.Sensitive = false;
			autoLoginButton.Sensitive = false;
			defaultAccButton.Sensitive = false;
//			proxyButton.Sensitive = false;
			loginButton.Sensitive = false;

			RemoveButton.Sensitive = false;
			DetailsButton.Sensitive = false;
		}




		private void OnRealizeWidget(object o, EventArgs args)
		{
			PopulateWidgets();

			// Select the first item in the TreeView
			if (AccTreeView.Selection != null)
			{
				AccTreeView.Selection.SelectPath(new TreePath("0"));
			}
		}




		private void NameCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			DomainInformation dom = (DomainInformation) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = dom.MemberName;
		}




		private void ServerCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			DomainInformation dom = (DomainInformation) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = dom.Name;
		}




		private void OnAddAccount(object o, EventArgs args)
		{
			if(NewAccountMode == true)
			{
				// This shouldn't be possible but hey, deal with it
				return;
			}
			
			AccTreeView.Selection.UnselectAll();

			NewAccountMode = true;

			AddButton.Sensitive = false;
			RemoveButton.Sensitive = false;
			DetailsButton.Sensitive = false;

			// Set the control states
 			detailsFrame.Sensitive = true;
			nameEntry.Sensitive = true;
			nameEntry.Editable = true;
			nameLabel.Sensitive = true;
			passEntry.Sensitive = true;
			passEntry.Editable = true;
			passLabel.Sensitive = true;
			serverEntry.Sensitive = true;
			serverEntry.Editable = true;
			serverLabel.Sensitive = true; 

			savePasswordButton.Sensitive = true;
			autoLoginButton.Sensitive = false;
			if(isFirstDomain)
				defaultAccButton.Sensitive = false;
			else
				defaultAccButton.Sensitive = true;

//			proxyButton.Sensitive = false;
			loginButton.Sensitive = false; 


			// set the control values
			savePasswordButton.Active = false;
			autoLoginButton.Active = true;
			if(isFirstDomain)
				defaultAccButton.Active = true;
			else
				defaultAccButton.Active = false;

			nameEntry.Text = "";
			serverEntry.Text = "";
			passEntry.Text = "";

			serverEntry.HasFocus = true;
			loginButton.HasDefault = true;
		}



		private void OnRemoveAccount(object o, EventArgs args)
		{
			TreeSelection tSelect = AccTreeView.Selection;

			if(tSelect.CountSelectedRows() == 1)
			{
				RemoveAccountDialog rad = new RemoveAccountDialog();
				rad.TransientFor = topLevelWindow;
				int rc = rad.Run();
				rad.Hide();
				if(rc == -5)
				{
					TreeModel tModel;
					TreeIter iter;

					tSelect.GetSelected(out tModel, out iter);
					DomainInformation dom = 
						(DomainInformation) tModel.GetValue(iter, 0);

					try
					{
						simws.LeaveDomain(dom.ID, !(rad.RemoveFromAll));
					}
					catch(Exception e)
					{
						iFolderExceptionDialog ied = 
							new iFolderExceptionDialog( topLevelWindow, e);
						ied.Run();
						ied.Hide();
						ied.Destroy();
						return;
					}

					ifdata.RemoveDomain(dom.ID);

					AccTreeStore.Remove(ref iter);
					curDomain = null;
					detailsFrame.Sensitive = false;
					nameEntry.Sensitive = false;
					nameEntry.Text = "";
					nameLabel.Sensitive = false;
					passEntry.Sensitive = false;
					passEntry.Text = "";
					passLabel.Sensitive = false;
					serverEntry.Sensitive = false;
					serverLabel.Sensitive = false;
					serverEntry.Text = "";

					savePasswordButton.Sensitive = false;
					autoLoginButton.Sensitive = false;
					defaultAccButton.Sensitive = false;
		//			proxyButton.Sensitive = false;
					loginButton.Sensitive = false;

					RemoveButton.Sensitive = false;
					DetailsButton.Sensitive = false;
				}

				rad.Destroy();
			}
		}


		private void OnAccTreeRowActivated(object o, RowActivatedArgs args)
		{
			if(!NewAccountMode)
				OnDetailsClicked(o, args);
		}


		private void OnDetailsClicked(object o, EventArgs args)
		{
			TreeSelection tSelect = AccTreeView.Selection;

			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				DomainInformation dom = 
						(DomainInformation) tModel.GetValue(iter, 0);

				AccountDialog accDialog = new AccountDialog(dom);
				accDialog.TransientFor = topLevelWindow;
				accDialog.Run();
				accDialog.Hide();
				accDialog.Destroy();
			}
		}




		public void AccSelectionChangedHandler(object o, EventArgs args)
		{
			if(NewAccountMode)
			{
				if( (nameEntry.Text.Length > 0) ||
					(passEntry.Text.Length > 0 ) ||
					(serverEntry.Text.Length > 0) )
				{
					iFolderMsgDialog dialog = new iFolderMsgDialog(
						topLevelWindow,
						iFolderMsgDialog.DialogType.Question,
						iFolderMsgDialog.ButtonSet.YesNo,
						Util.GS("iFolder Confirmation"),
						Util.GS("Lose current settings?"),
						Util.GS("You are currently creating a new account.  By selecing a different account you cancel the operation.  Do you wish to continue and loose the current settings?"));
					int rc = dialog.Run();
					dialog.Hide();
					dialog.Destroy();
					if(rc == -8)
					{
						NewAccountMode = false;
					}
					else
					{
						// disconnect the handler so we can unselect and
						// not get called again
						AccTreeView.Selection.Changed -=
							new EventHandler(AccSelectionChangedHandler);
						AccTreeView.Selection.UnselectAll();
						// hook'r back up
						AccTreeView.Selection.Changed +=
							new EventHandler(AccSelectionChangedHandler);
	
						return;
					}
				}
				else
				{
					// There isn't any data in the fields so just let them
					// go ahead and quit
					NewAccountMode = false;
				}
			}
			else
			{
				// LAME LAME LAME
				// This Retarted event is called before the other widget
				// looses it's focus so we have to deal with it here
				// This is a hack to save the password
				if( (curDomainPassword != passEntry.Text) && 
						(savePasswordButton.Active == true ) )
				{
					SavePasswordNow();
				}
			}


			TreeSelection tSelect = AccTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				DomainInformation dom = 
						(DomainInformation) tModel.GetValue(iter, 0);

				curDomain = dom;

				// Set the control states
				AddButton.Sensitive = true;
				RemoveButton.Sensitive = true;
				DetailsButton.Sensitive = true;

 				detailsFrame.Sensitive = true;
				serverEntry.Sensitive = true;
				serverEntry.Editable = false;
				serverLabel.Sensitive = false;
				nameEntry.Editable = false;
				nameEntry.Sensitive = true;
				nameLabel.Sensitive = false;

				passEntry.Sensitive = true;
				passEntry.Editable = true;
				passLabel.Sensitive = true;

				savePasswordButton.Sensitive = true;
				autoLoginButton.Sensitive = true;

//				proxyButton.Sensitive = false;
				loginButton.Sensitive = false;


				// set the control values
				try
				{
					string userID;
					string credentials;
					CredentialType credType = simws.GetDomainCredentials(
						dom.ID, out userID, out credentials);
					if( (credentials != null) &&
						(credType == CredentialType.Basic) )
					{
						curDomainPassword = credentials;
						passEntry.Text = credentials;
						savePasswordButton.Active = true;
					}
					else
					{
						throw new Exception("Invalid Creds");
					}
				}
				catch(Exception e)
				{
					curDomainPassword = "";
					passEntry.Text = "";
					savePasswordButton.Active = false;
				}

				autoLoginButton.Active = dom.Active;
				defaultAccButton.Active = dom.IsDefault;
				defaultAccButton.Sensitive = !dom.IsDefault;

				if(dom.MemberName != null)
					nameEntry.Text = dom.MemberName;
				else
					nameEntry.Text = "";
				if(dom.Host != null)
					serverEntry.Text = dom.Host;
				else
					serverEntry.Text = "";
			}
		}

		private void OnLoginAccount(object o, EventArgs args)
		{
			iFolderMsgDialog dg;
			
			if (NewAccountMode)
			{
				try
				{
					DomainInformation domainInfo = simws.ConnectToDomain(nameEntry.Text, passEntry.Text, serverEntry.Text);
					switch (domainInfo.StatusCode)
					{
						case StatusCodes.Success:
						case StatusCodes.SuccessInGrace:
							// Set the credentials in the current process.
							DomainAuthentication domainAuth = new DomainAuthentication("iFolder", domainInfo.ID, passEntry.Text);
							Status authStatus = domainAuth.Authenticate();
		
							switch (authStatus.statusCode)
							{
								case StatusCodes.Success:
								case StatusCodes.SuccessInGrace:
									ifdata.RefreshDomains();
									//	AddDomain(domainInfo);
									NewAccountMode = false;
									TreeIter iter = AccTreeStore.AppendValues(domainInfo);
									TreeSelection sel = AccTreeView.Selection;
									curDomain = domainInfo;
									
									if (savePasswordButton.Active)
									{
										SavePasswordNow();
									}
									
									if (defaultAccButton.Active)
									{
										if (ifdata.SetDefaultDomain(curDomain))
										{
											defaultAccButton.Sensitive = false;
										}
									}
									
									sel.SelectIter(iter);
									
									if (authStatus.RemainingGraceLogins < authStatus.TotalGraceLogins)
									{
										dg = new iFolderMsgDialog(
											topLevelWindow,
											iFolderMsgDialog.DialogType.Error,
											iFolderMsgDialog.ButtonSet.Ok,
											Util.GS("iFolder Error"),
											Util.GS("Expired Password"),
											string.Format(Util.GS("Your password has expired.  You have {0} grace logins remaining."), authStatus.RemainingGraceLogins));
										dg.Run();
										dg.Hide();
										dg.Destroy();
									}
								
									break;
								case StatusCodes.InvalidCredentials:
								case StatusCodes.InvalidPassword:
									dg = new iFolderMsgDialog(
										topLevelWindow,
										iFolderMsgDialog.DialogType.Error,
										iFolderMsgDialog.ButtonSet.Ok,
										Util.GS("iFolder Error"),
										Util.GS("Unable to Connect to iFolder Server"),
										Util.GS("The user name or password is invalid.  Please try again."));
									dg.Run();
									dg.Hide();
									dg.Destroy();
									break;
								case StatusCodes.AccountDisabled:
									dg = new iFolderMsgDialog(
										topLevelWindow,
										iFolderMsgDialog.DialogType.Error,
										iFolderMsgDialog.ButtonSet.Ok,
										Util.GS("iFolder Error"),
										Util.GS("Unable to Connect to iFolder Server"),
										Util.GS("The user account is disabled.  Please contact your network administrator for assistance."));
									dg.Run();
									dg.Hide();
									dg.Destroy();
									break;
								case StatusCodes.AccountLockout:
									dg = new iFolderMsgDialog(
										topLevelWindow,
										iFolderMsgDialog.DialogType.Error,
										iFolderMsgDialog.ButtonSet.Ok,
										Util.GS("iFolder Error"),
										Util.GS("Unable to Connect to iFolder Server"),
										Util.GS("The user account has been locked out.  Please contact your network administrator for assistance."));
									dg.Run();
									dg.Hide();
									dg.Destroy();
									break;
								default:
									dg = new iFolderMsgDialog(
										topLevelWindow,
										iFolderMsgDialog.DialogType.Error,
										iFolderMsgDialog.ButtonSet.Ok,
										Util.GS("iFolder Error"),
										Util.GS("Unable to Connect to iFolder Server"),
										Util.GS("An error was encountered while connecting to the iFolder server.  Please verify the information entered and try again.  If the problem persists, please contact your network administrator."));
									dg.Run();
									dg.Hide();
									dg.Destroy();
									break;
							}
							break;
						case StatusCodes.InvalidCredentials:
						case StatusCodes.InvalidPassword:
						case StatusCodes.UnknownUser:
							dg = new iFolderMsgDialog(
								topLevelWindow,
								iFolderMsgDialog.DialogType.Error,
								iFolderMsgDialog.ButtonSet.Ok,
								Util.GS("iFolder Error"),
								Util.GS("Unable to Connect to iFolder Server"),
								Util.GS("The user name or password is invalid.  Please try again."));
							dg.Run();
							dg.Hide();
							dg.Destroy();
							break;
						case StatusCodes.AccountDisabled:
							dg = new iFolderMsgDialog(
								topLevelWindow,
								iFolderMsgDialog.DialogType.Error,
								iFolderMsgDialog.ButtonSet.Ok,
								Util.GS("iFolder Error"),
								Util.GS("Unable to Connect to iFolder Server"),
								Util.GS("The user account is disabled.  Please contact your network administrator for assistance."));
							dg.Run();
							dg.Hide();
							dg.Destroy();
							break;
						case StatusCodes.AccountLockout:
							dg = new iFolderMsgDialog(
								topLevelWindow,
								iFolderMsgDialog.DialogType.Error,
								iFolderMsgDialog.ButtonSet.Ok,
								Util.GS("iFolder Error"),
								Util.GS("Unable to Connect to iFolder Server"),
								Util.GS("The user account has been locked out.  Please contact your network administrator for assistance."));
							dg.Run();
							dg.Hide();
							dg.Destroy();
							break;
						default:
							dg = new iFolderMsgDialog(
								topLevelWindow,
								iFolderMsgDialog.DialogType.Error,
								iFolderMsgDialog.ButtonSet.Ok,
								Util.GS("iFolder Error"),
								Util.GS("Unable to Connect to iFolder Server"),
								Util.GS("An error was encountered while connecting to the iFolder server.  Please verify the information entered and try again.  If the problem persists, please contact your network administrator."));
							dg.Run();
							dg.Hide();
							dg.Destroy();
							break;
					}
				}
				catch (Exception ex)
				{
					dg = new iFolderMsgDialog(
						topLevelWindow,
						iFolderMsgDialog.DialogType.Error,
						iFolderMsgDialog.ButtonSet.Ok,
						Util.GS("iFolder Error"),
						Util.GS("Unable to Connect to iFolder Server"),
						Util.GS("An error was encountered while connecting to the iFolder server.  Please verify the information entered and try again.  If the problem persists, please contact your network administrator."));
					dg.Run();
					dg.Hide();
					dg.Destroy();
				}
			}
		}


		private void OnFieldsChanged(object obj, EventArgs args)
		{
			if(NewAccountMode)
			{
				if( (nameEntry.Text.Length > 0) &&
					(passEntry.Text.Length > 0 ) &&
					(serverEntry.Text.Length > 0) )
				{
					loginButton.Sensitive = true;
				}
				else
					loginButton.Sensitive = false;
			}
		}


		private void OnSavePasswordToggled(object obj, EventArgs args)
		{
			// If we are creating a new account, do nothing
			// it will be handled at creation time
			if(	(!NewAccountMode) && (savePasswordButton.HasFocus == true) )
			{
				SavePasswordNow();
			}
		}


		private void OnPassEntryActivated(object o, EventArgs args)
		{
			if( (!NewAccountMode) && (curDomainPassword != passEntry.Text) && 
						(savePasswordButton.Active == true ) )
			{
				SavePasswordNow();
			}
		}


		private void OnPassEntryFoucusOut(object o, FocusOutEventArgs args)
		{
			if( (!NewAccountMode) && (curDomainPassword != passEntry.Text) && 
						(savePasswordButton.Active == true ) )
			{
				SavePasswordNow();
			}
		}


		private void SavePasswordNow()
		{
			try
			{
				if( (savePasswordButton.Active == true) &&
						(passEntry.Text.Length > 0) )
				{
					simws.SetDomainCredentials(curDomain.ID, 
							passEntry.Text, CredentialType.Basic);
				}
				else
				{
					simws.SetDomainCredentials(curDomain.ID, null,
							CredentialType.None);
				}
				curDomainPassword = passEntry.Text;
			}
			catch (Exception ex)
			{
				// Ignore this error for now 
			}
		}


		private void OnAutoLoginToggled(object obj, EventArgs args)
		{
			// If we are creating a new account, do nothing
			// it will be handled at creation time
			if(!NewAccountMode)
			{
				if(autoLoginButton.Active != curDomain.Active)
				{
					try
					{
						if(autoLoginButton.Active)
						{
							simws.SetDomainActive(curDomain.ID);
							curDomain.Active = true;
						}
						else
						{
							simws.SetDomainInactive(curDomain.ID);
							curDomain.Active = false;
						}
					}
					catch (Exception ex)
					{
						// Ignore this error for now 
					}
				}
			}
		}

		private void OnDefAccToggled(object obj, EventArgs args)
		{
			// If we are creating a new account, do nothing
			// it will be handled at creation time
			if(!NewAccountMode)
			{
				if( (defaultAccButton.Active != curDomain.IsDefault) &&
					(defaultAccButton.Active == true ) )
				{
					defaultAccButton.Sensitive = 
							ifdata.SetDefaultDomain(curDomain);
				}
			}
		}




	}
}
