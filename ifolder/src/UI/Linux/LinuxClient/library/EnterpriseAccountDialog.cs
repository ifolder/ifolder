/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
  *
  *                 $Author: Boyd Timothy <btimothy@novell.com> 
  *                 $Modified by: <Modifier>
  *                 $Mod Date: <Date Modified>
  *                 $Revision: 0.0
  *-----------------------------------------------------------------------------
  * This module is used to:
  *        <Description of the functionality of the file >
  *
  *
  *******************************************************************************/

using Gtk;
using System;
using Simias.Client;

using Novell.iFolder.Events;
using Novell.iFolder.Controller;

namespace Novell.iFolder
{
    /// <summary>
    /// class Enteprise Account Dialog
    /// </summary>
	public class EnterpriseAccountDialog : AccountDialog
	{
		private iFolderData			ifdata;
		private DomainController	domainController;
//		private Manager				simiasManager;
		
		private bool				ControlKeyPressed;
	
		///
		/// Global Widgets
		///
		private CheckButton			EnableAccountButton;
		private CheckButton			DefaultAccountButton;

		///
		/// Widgets for the Server Page
		///
		private Entry				ServerAddressEntry;
		private TextView			ServerDescriptionTextView;
		private bool				bServerAddressChanged;
		
		///
		/// Widgets for the Identity Page
		///
		private Entry				PasswordEntry;
		private CheckButton			RememberPasswordButton;
		private bool				bPasswordChanged;
		
		///
		/// Widgets for the Disk Space Page
		///
		private Label				QuotaTotalLabel;
		private Label				QuotaUsedLabel;
		private Label				QuotaAvailableLabel;
		private ProgressBar			QuotaGraph;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">GTK Parent Window</param>
        /// <param name="curDomain">Current Domain</param>
		public EnterpriseAccountDialog(Window parent, DomainInformation curDomain)
			: base(parent, curDomain)
		{
			ifdata = iFolderData.GetData();
//			this.simiasManager = Util.GetSimiasManager();
			
			domainController = DomainController.GetDomainController();
			
			bServerAddressChanged	= false;
			bPasswordChanged		= false;

			SetupDialog();
			
			this.Response +=
				new ResponseHandler(OnDialogResponse);
			
			// FIXME: Figure out if we need to register for when the window closes so that fields will be saved.
			// I believe that the FocusOutEventHandlers should already take care of it, but you never know
		}
		
        /// <summary>
        /// Dummy Destructor
        /// </summary>
		~EnterpriseAccountDialog()
		{
		}

        /// <summary>
        /// Set up Dialog - Customize
        /// </summary>
		private void SetupDialog()
		{
			this.Title = string.Format("{0} {1}", domain.Name, Util.GS("Properties"));
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder16.png"));
			this.HasSeparator = false;
			this.Resizable = false;
			this.Modal = false;
			this.TypeHint = Gdk.WindowTypeHint.Normal;
			this.DefaultResponse = ResponseType.Ok;

			VBox vbox = new VBox(false, 12);
			this.VBox.PackStart(vbox, true, true, 0);
			vbox.BorderWidth = Util.DefaultBorderWidth;
			vbox.PackStart(CreateNotebook(), true, true, 0);
			vbox.PackStart(CreateGlobalCheckButtons(), false, false, 0);

			this.AddButton(Gtk.Stock.Close, ResponseType.Ok);
			
			this.DefaultResponse = ResponseType.Ok;
			
			this.Response += new ResponseHandler(OnAccountDialogResponse);
			
			// Bind ESC and C-w to close the window
			ControlKeyPressed = false;
			KeyPressEvent += new KeyPressEventHandler(KeyPressHandler);
			KeyReleaseEvent += new KeyReleaseEventHandler(KeyReleaseHandler);
			
			this.Realized += new EventHandler(OnRealizeWidget);
		}
		
        /// <summary>
        /// Event Handler for Account Dialog Response event
        /// </summary>
        private void OnAccountDialogResponse(object o, ResponseArgs args)
		{
			// Since there's only one button to respond with, ignore the actual
			// response and just hide and destroy this widget.
			this.Hide();
			this.Destroy();
		}



		/// <summary>
		/// Creates the tabbed notebook
		/// </summary>
		/// <returns>
		/// Widget to display
		/// </returns>
		private Widget CreateNotebook()
		{
			Notebook notebook = new Notebook();
			notebook.AppendPage(CreateServerPage(), new Label(Util.GS("Account")));
			notebook.AppendPage(CreateIdentityPage(), new Label(Util.GS("Identity")));
			notebook.AppendPage(CreateDiskSpacePage(), new Label(Util.GS("Disk Space")));
		   	
			notebook.ShowAll();
			return notebook;
		}
		
        /// <summary>
        /// Create Global Check Buttons
        /// </summary>
        /// <returns>Widget</returns>
		private Widget CreateGlobalCheckButtons()
		{
			VBox vbox = new VBox(false, 0);

			EnableAccountButton = new CheckButton(Util.GS("_Automatically connect"));
			vbox.PackStart(EnableAccountButton, false, false, 0);

			DefaultAccountButton = new CheckButton(Util.GS("Account is _default"));
			vbox.PackStart(DefaultAccountButton, false, false, 0);
			
			return vbox;
		}
		
		/// <summary>
		/// Create Server Page
		/// </summary>
		/// <returns>Widget</returns>
        private Widget CreateServerPage()
		{
			VBox vbox = new VBox(false, 0);
			
			Table table = new Table(6, 2, false);
			vbox.PackStart(table, true, true, 0);
			table.ColumnSpacing = 12;
			table.RowSpacing = 6;
			table.BorderWidth = 12;
			
			///
			/// Row 1 (Server Name)
			///
			Label l = new Label(Util.GS("Name:"));
			table.Attach(l,
						 0,1, 0,1,
						 AttachOptions.Shrink | AttachOptions.Fill,
						 0,0,0);
			l.Xalign = 0;
			
			l = new Label(domain.Name);
			table.Attach(l,
						 1,2, 0,1,
						 AttachOptions.Expand | AttachOptions.Fill,
						 0,0,0);
			l.UseUnderline = false;
			l.Xalign = 0;
			
			///
			/// Row 2 (Server Address)
			///
			l = new Label(Util.GS("Address:"));
			table.Attach(l,
						 0,1, 1,2,
						 AttachOptions.Expand | AttachOptions.Fill,
						 0,0,0);
			l.Xalign = 0;
			l.Yalign = 0;

			ServerAddressEntry = new Entry();
			table.Attach(ServerAddressEntry,
						 1,2, 1,2,
						 AttachOptions.Shrink | AttachOptions.Fill,
						 0,0,0);
			l.UseUnderline = false;
			l.MnemonicWidget = ServerAddressEntry;
			
			///
			/// Row 3 (Hint about changing the server address)
			///
			l = new Label(Util.GS("(You can change the address if you are offline)"));
			table.Attach(l,
						 1,2, 2,3,
						 AttachOptions.Shrink | AttachOptions.Fill,
						 0,0,0);
			l.LineWrap = true;
			l.Wrap = true;
			l.Xalign = 0;
			l.Sensitive = false;
			Requisition req = ServerAddressEntry.SizeRequest();
			l.WidthRequest = req.Width;

			///
			/// Row 4 (Description TextView)
			///
			ScrolledWindow sw = new ScrolledWindow();
			table.Attach(sw,
						 0,2, 3,4,
						 AttachOptions.Expand | AttachOptions.Fill,
						 0,0,0);
			sw.ShadowType = Gtk.ShadowType.EtchedIn;
			ServerDescriptionTextView = new TextView();
			if(domain.Description != null)
				ServerDescriptionTextView.Buffer.Text = domain.Description;
			ServerDescriptionTextView.WrapMode = Gtk.WrapMode.Word;
			ServerDescriptionTextView.Editable = false;
			ServerDescriptionTextView.Sensitive = false;
			ServerDescriptionTextView.CursorVisible = false;
			ServerDescriptionTextView.RightMargin = 5;
			ServerDescriptionTextView.LeftMargin = 5;
			sw.Add(ServerDescriptionTextView);

			return vbox;
		}
		
        /// <summary>
        /// Create Identity Page
        /// </summary>
        /// <returns>Widget</returns>
		private Widget CreateIdentityPage()
		{
			VBox vbox = new VBox(false, 0);
			
			Table table = new Table(3, 2, false);
			vbox.PackStart(table, true, true, 0);
			table.ColumnSpacing = 12;
			table.RowSpacing = 6;
			table.BorderWidth = 12;
			
			///
			/// Row 1 (User Name)
			///
			Label l = new Label(Util.GS("User name:"));
			table.Attach(l,
						 0,1, 0,1,
						 AttachOptions.Shrink | AttachOptions.Fill,
						 0,0,0);
			l.Xalign = 0;
			
			l = new Label(domain.MemberName);
			table.Attach(l,
						 1,2, 0,1,
						 AttachOptions.Expand | AttachOptions.Fill,
						 0,0,0);
			l.UseUnderline = false;
			l.Xalign = 0;
			
			///
			/// Row 2 (User Password)
			///
			l = new Label(Util.GS("_Password:"));
			table.Attach(l,
						 0,1, 1,2,
						 AttachOptions.Shrink | AttachOptions.Fill,
						 0,0,0);
			l.Xalign = 0;

			PasswordEntry = new Entry();
			table.Attach(PasswordEntry,
						 1,2, 1,2,
						 AttachOptions.Shrink | AttachOptions.Fill,
						 0,0,0);
			PasswordEntry.Visibility = false;
			l.MnemonicWidget = PasswordEntry;

			///
			/// Row 3 (Remember Password CheckButton)
			///
			RememberPasswordButton =
				new CheckButton(Util.GS("_Remember password"));
			table.Attach(RememberPasswordButton,
						 1,2, 2,3,
						 AttachOptions.Expand | AttachOptions.Fill,
						 0,0,0);
			
			return vbox;
		}
		
        /// <summary>
        /// Create Disk Space Page
        /// </summary>
        /// <returns>Widget</returns>
		private Widget CreateDiskSpacePage()
		{
			VBox vbox = new VBox(false, 0);
			
			Table table = new Table(3, 3, false);
			vbox.PackStart(table, true, true, 0);
			table.ColumnSpacing = 12;
			table.RowSpacing = 6;
			table.BorderWidth = 12;
			
			///
			/// Row 1 (Quota)
			///
			Label l = new Label(Util.GS("Quota:"));
			table.Attach(l,
						 0,1, 0,1,
						 AttachOptions.Expand | AttachOptions.Fill,
						 0,0,0);
			l.Xalign = 0;

			QuotaTotalLabel = new Label("");
			table.Attach(QuotaTotalLabel,
						 1,2, 0,1,
						 AttachOptions.Shrink | AttachOptions.Fill,
						 0,0,0);
			QuotaTotalLabel.Xalign = 0;
			
			///
			/// Row 2 (Used)
			///
			l = new Label(Util.GS("Used:"));
			table.Attach(l,
						 0,1, 1,2,
						 AttachOptions.Expand | AttachOptions.Fill,
						 0,0,0);
			l.Xalign = 0;

			QuotaUsedLabel = new Label("");
			table.Attach(QuotaUsedLabel,
						 1,2, 1,2,
						 AttachOptions.Shrink | AttachOptions.Fill,
						 0,0,0);
			QuotaUsedLabel.Xalign = 0;

			///			
			/// Row 3 (Available)
			///
			l = new Label(Util.GS("Available:"));
			table.Attach(l,
						 0,1, 2,3,
						 AttachOptions.Expand | AttachOptions.Fill,
						 0,0,0);
			l.Xalign = 0;

			QuotaAvailableLabel = new Label("");
			table.Attach(QuotaAvailableLabel,
						 1,2, 2,3,
						 AttachOptions.Shrink | AttachOptions.Fill,
						 0,0,0);
			QuotaAvailableLabel.Xalign = 0;

			///
			/// Disk Space Graph
			///
			Frame graphFrame = new Frame();
			table.Attach(graphFrame,
						 2,3, 0,3,
						 AttachOptions.Shrink | AttachOptions.Fill,
						 0,0,0);
			graphFrame.Shadow = Gtk.ShadowType.EtchedOut;
			graphFrame.ShadowType = Gtk.ShadowType.EtchedOut;
			HBox graphBox = new HBox();
			graphBox.Spacing = 5;
			graphBox.BorderWidth = 5;
			graphFrame.Add(graphBox);

			QuotaGraph = new ProgressBar();
			graphBox.PackStart(QuotaGraph, false, true, 0);

			QuotaGraph.Orientation = Gtk.ProgressBarOrientation.BottomToTop;
			QuotaGraph.PulseStep = .10;
			QuotaGraph.Fraction = 0;

			VBox graphLabelBox = new VBox();
			graphBox.PackStart(graphLabelBox, false, true, 0);

			Label fullLabel = new Label(Util.GS("full"));
			fullLabel.Xalign = 0;
			fullLabel.Yalign = 0;
			graphLabelBox.PackStart(fullLabel, true, true, 0);

			Label emptyLabel = new Label(Util.GS("empty"));
			emptyLabel.Xalign = 0;
			emptyLabel.Yalign = 1;
			graphLabelBox.PackStart(emptyLabel, true, true, 0);

			return vbox;
		}
		
        /// <summary>
        /// Event Handler for REalize Widget event
        /// </summary>
        private void OnRealizeWidget(object o, EventArgs args)
		{
			InitGlobalCheckButtons();
			InitServerPage();
			InitIdentityPage();
			InitDiskSpacePage();

			// Register for the domain events that could affect this dialog
			if (domainController != null)
			{
				domainController.DomainHostModified +=
					new DomainHostModifiedEventHandler(OnDomainHostModified);
				domainController.DomainLoggedIn +=
					new DomainLoggedInEventHandler(OnDomainLoggedIn);
				domainController.DomainLoggedOut +=
					new DomainLoggedOutEventHandler(OnDomainLoggedOut);
				domainController.DomainActivated +=
					new DomainActivatedEventHandler(OnDomainActivated);
				domainController.DomainInactivated +=
					new DomainInactivatedEventHandler(OnDomainInactivated);
				domainController.NewDefaultDomain +=
					new DomainNewDefaultEventHandler(OnNewDefaultDomain);
				domainController.DomainDeleted +=
					new DomainDeletedEventHandler(OnDomainDeleted);
			}
			
			// Register Widget EventHandlers
			EnableAccountButton.Toggled +=
				new EventHandler(OnEnableAccountToggled);
			DefaultAccountButton.Toggled +=
				new EventHandler(OnDefaultAccountToggled);

			ServerAddressEntry.Changed +=
				new EventHandler(OnServerAddressChanged);
			ServerAddressEntry.FocusOutEvent +=
				new FocusOutEventHandler(OnServerAddressFocusOut);

			PasswordEntry.Changed +=
				new EventHandler(OnPasswordChanged);
			PasswordEntry.FocusOutEvent +=
				new FocusOutEventHandler(OnPasswordFocusOut);
			RememberPasswordButton.Toggled +=
				new EventHandler(OnRememberPasswordToggled);
			
		}
        /// <summary>
        /// Event Handler for Dialog Response event
        /// </summary>
		private void OnDialogResponse(object o, ResponseArgs args)
		{
			// Unregister for domain events
			if (domainController != null)
			{
				domainController.DomainHostModified -=
					new DomainHostModifiedEventHandler(OnDomainHostModified);
				domainController.DomainLoggedIn -=
					new DomainLoggedInEventHandler(OnDomainLoggedIn);
				domainController.DomainLoggedOut -=
					new DomainLoggedOutEventHandler(OnDomainLoggedOut);
				domainController.DomainActivated -=
					new DomainActivatedEventHandler(OnDomainActivated);
				domainController.DomainInactivated -=
					new DomainInactivatedEventHandler(OnDomainInactivated);
				domainController.NewDefaultDomain -=
					new DomainNewDefaultEventHandler(OnNewDefaultDomain);
				domainController.DomainDeleted -=
					new DomainDeletedEventHandler(OnDomainDeleted);
			}
		}
		
        /// <summary>
        /// Initialize Global Check Buttons
        /// </summary>
		private void InitGlobalCheckButtons()
		{
			EnableAccountButton.Active = domain.Active;
			
			DefaultAccountButton.Active = domain.IsDefault;
			DefaultAccountButton.Sensitive = !domain.IsDefault;
		}
		
        /// <summary>
        /// Initialize Server Page
        /// </summary>
		private void InitServerPage()
		{
			ServerAddressEntry.Text = GetHostUrl(domain.HostUrl);
			if (domain.Authenticated)
			{
				ServerAddressEntry.Sensitive = false;
				ServerAddressEntry.IsEditable = false;
			}
			else
			{
				ServerAddressEntry.Sensitive = true;
				ServerAddressEntry.IsEditable = true;
			}
		}
	
        /// <summary>
        /// Get the Host URL of Server
        /// </summary>
        /// <param name="hosturl"></param>
        /// <returns></returns>
		private string GetHostUrl(string hosturl)
                {
			UriBuilder serverUri = new UriBuilder(hosturl);	
                        UriBuilder hosturi = new UriBuilder( serverUri.Scheme + Uri.SchemeDelimiter + serverUri.Host + ":" + serverUri.Port );
                        return (hosturi.Uri.ToString()).TrimEnd( new char[] {'/'} );	
                }

	
        /// <summary>
        /// Initialize Identity Page
        /// </summary>
		private void InitIdentityPage()
		{
			try
			{
				string password = domainController.GetDomainPassword(domain.ID);
				if (password != null)
				{
					PasswordEntry.Text = password;
					RememberPasswordButton.Active = true;
				}
				else
				{
					RememberPasswordButton.Active = false;
				}
			}
			catch{}
		}
		
        /// <summary>
        /// Initial;ize Disk Space Page
        /// </summary>
		private void InitDiskSpacePage()
		{
			DiskSpace ds = ifdata.GetUserDiskSpace(domain.MemberUserID);

			if(ds == null)
			{
				QuotaTotalLabel.Text = Util.GS("N/A");
				QuotaUsedLabel.Text = Util.GS("N/A");
				QuotaAvailableLabel.Text = Util.GS("N/A");
				QuotaGraph.Fraction = 0;
			}
			else
			{
				int tmpValue;

				if(ds.Limit == -1)
				{
					QuotaTotalLabel.Text = Util.GS("N/A");
				}
				else
				{
					tmpValue = (int)(ds.Limit / (1024 * 1024));
					QuotaTotalLabel.Text =
						string.Format("{0} {1}", tmpValue, Util.GS("MB"));
				}

				if(ds.UsedSpace == 0)
				{
					QuotaUsedLabel.Text = Util.GS("N/A");
				}
				else
				{
					tmpValue = (int)(ds.UsedSpace / (1024 * 1024)) + 1;
					QuotaUsedLabel.Text =
						string.Format("{0} {1}", tmpValue, Util.GS("MB"));
				}

				if(ds.AvailableSpace == 0)
				{
					QuotaAvailableLabel.Text = Util.GS("N/A");
				}
				else
				{
					tmpValue = (int)(ds.AvailableSpace / (1024 * 1024));
					QuotaAvailableLabel.Text =
						string.Format("{0} {1}",tmpValue, Util.GS("MB"));
				}

				if(ds.Limit == 0)
				{
					QuotaGraph.Fraction = 0;
				}
				else
				{
					if(ds.Limit < ds.UsedSpace)
						QuotaGraph.Fraction = 1;
					else
						QuotaGraph.Fraction = ((double)ds.UsedSpace) / 
												((double)ds.Limit);
				}
			}
		}
		
		///
		/// Utility Methods
		///
		private bool SaveServerAddress()
		{
			string serverAddress = ServerAddressEntry.Text;
			string username = domain.MemberName;
			string password = domainController.GetDomainPassword(domain.ID);
			bServerAddressChanged = false;
			bool bHostAddressUpdated = false;

			if (serverAddress == null || serverAddress.Trim().Length == 0)
			{
				// FIXME: Register this window with the Modal Controller
				iFolderMsgDialog dg =
					new iFolderMsgDialog(
						this,
						iFolderMsgDialog.DialogType.Error,
						iFolderMsgDialog.ButtonSet.Ok,
						"",
						Util.GS("Server address cannot be empty"),
						Util.GS("Please enter an address for the server."));
				dg.Run();
				dg.Hide();
				dg.Destroy();
				
				// Set the ServerAddressEntry back to the original value
				ServerAddressEntry.Changed -= new EventHandler(OnServerAddressChanged);
				ServerAddressEntry.Text = GetHostUrl(domain.HostUrl); 
				ServerAddressEntry.Changed += new EventHandler(OnServerAddressChanged);
				
				return bHostAddressUpdated;
			}
			
			// Make sure that we have a password for calling UpdateDomainHostAddress
			if (password == null || password.Trim().Length == 0)
			{
				Entry tempPasswordEntry = new Entry();
				tempPasswordEntry.Visibility = false;
				iFolderMsgDialog dg =
					new iFolderMsgDialog(
						this,
						iFolderMsgDialog.DialogType.Info,
						iFolderMsgDialog.ButtonSet.OkCancel,
						"",
						Util.GS("Please enter your password"),
						Util.GS("Your password is required to change the address of the server."));
				dg.ExtraWidget = tempPasswordEntry;
				tempPasswordEntry.GrabFocus();
				tempPasswordEntry.ActivatesDefault = true;
				dg.TransientFor = this;
				int rc = dg.Run();
				password = tempPasswordEntry.Text;
				dg.Hide();
				dg.Destroy();
				if ((ResponseType)rc == ResponseType.Cancel)
				{
					// Set the ServerAddressEntry back to the original value
					ServerAddressEntry.Changed -= new EventHandler(OnServerAddressChanged);
					ServerAddressEntry.Text = GetHostUrl(domain.HostUrl);
					ServerAddressEntry.Changed += new EventHandler(OnServerAddressChanged);

					return bHostAddressUpdated;
				}

				
				
				if (password == null || password.Trim().Length == 0)
				{
					// Set the ServerAddressEntry back to the original value
					ServerAddressEntry.Changed -= new EventHandler(OnServerAddressChanged);
					ServerAddressEntry.Text = GetHostUrl(domain.HostUrl);
					ServerAddressEntry.Changed += new EventHandler(OnServerAddressChanged);

					return bHostAddressUpdated;
				}
			}
			
			serverAddress = serverAddress.Trim();
			
			Exception hostAddressUpdateException = null;
			try
			{
				if (domainController.UpdateDomainHostAddress(domain.ID, serverAddress, username, password) != null)
				{
					bHostAddressUpdated = true;
				}
			}
			catch(Exception e)
			{
				hostAddressUpdateException = e;
			}
			
			if (!bHostAddressUpdated)
			{
				// FIXME: Register this as a modal window
				iFolderMsgDialog dg = new iFolderMsgDialog(
					this,
					iFolderMsgDialog.DialogType.Error,
					iFolderMsgDialog.ButtonSet.Ok,
					"",
					Util.GS("Unable to modify the server address"),
					Util.GS("An error was encountered while attempting to modify the server address.  Please verify the address and your password are correct."),
					hostAddressUpdateException == null ? null : hostAddressUpdateException.Message);
				dg.Run();
				dg.Hide();
				dg.Destroy();
				
				// Set the ServerAddressEntry back to the original value
				ServerAddressEntry.Changed -= new EventHandler(OnServerAddressChanged);
				ServerAddressEntry.Text = GetHostUrl(domain.HostUrl); 
				ServerAddressEntry.Changed += new EventHandler(OnServerAddressChanged);
			}
			
			return bHostAddressUpdated;
		}
		
        /// <summary>
        /// Save Password
        /// </summary>
        /// <returns>true on success</returns>
		private bool SavePassword()
		{
			string password = PasswordEntry.Text;
			bPasswordChanged = false;
			
			password = password.Trim();
			
			try
			{
				if (password.Length > 0)
				{
					domainController.SetDomainPassword(domain.ID, password);
				}
				else
				{
					domainController.ClearDomainPassword(domain.ID);
				}
			}
			catch(Exception e)
			{
				// FIXME: Register this as a modal window
				iFolderMsgDialog dg = new iFolderMsgDialog(
					this,
					iFolderMsgDialog.DialogType.Error,
					iFolderMsgDialog.ButtonSet.Ok,
					"",
					Util.GS("Unable to modify the password"),
					Util.GS("An error was encountered while attempting to modify the password."),
					e.Message);
				dg.Run();
				dg.Hide();
				dg.Destroy();
				
				// Reset the password to its original value
				PasswordEntry.Changed -= new EventHandler(OnPasswordChanged);
				PasswordEntry.Text = domainController.GetDomainPassword(domain.ID);
				PasswordEntry.Changed += new EventHandler(OnPasswordChanged);
				
				return false;
			}

			return true;
		}
		
		///
		/// Event Handlers
		///
		public void OnDomainHostModified(object sender, DomainEventArgs args)
		{
			// Make sure that the domain event is for the domain this dialog is showing
			if (args.DomainID == domain.ID)
			{
				DomainInformation updatedDomain =
					domainController.GetDomain(args.DomainID);
				if (updatedDomain != null)
				{
					domain = updatedDomain;
					
					ServerAddressEntry.Changed -= new EventHandler(OnServerAddressChanged);
					ServerAddressEntry.Text = GetHostUrl(domain.HostUrl); 
					bServerAddressChanged = false;
					ServerAddressEntry.Changed += new EventHandler(OnServerAddressChanged);
				}
			}
		}

		public void OnDomainLoggedIn(object sender, DomainEventArgs args)
		{
			// Make sure that the domain event is for the domain this dialog is showing
			if (args.DomainID == domain.ID)
			{
				// Reset the values to the original
				ServerAddressEntry.Changed -= new EventHandler(OnServerAddressChanged);
				ServerAddressEntry.Text = GetHostUrl(domain.HostUrl); 
				bServerAddressChanged = false;
				ServerAddressEntry.Changed += new EventHandler(OnServerAddressChanged);

				// Prevent the uesr from modifying the address
				ServerAddressEntry.Sensitive = false;
				ServerAddressEntry.IsEditable = false;
			}
		}
		
		public void OnDomainLoggedOut(object sender, DomainEventArgs args)
		{
			// Make sure that the domain event is for the domain this dialog is showing
			if (args.DomainID == domain.ID)
			{
				// Reset the values to the original
				ServerAddressEntry.Changed -= new EventHandler(OnServerAddressChanged);
				ServerAddressEntry.Text = GetHostUrl(domain.HostUrl); 
				bServerAddressChanged = false;
				ServerAddressEntry.Changed += new EventHandler(OnServerAddressChanged);

				// Allow the user to modify the address
				ServerAddressEntry.Sensitive = true;
				ServerAddressEntry.IsEditable = true;
			}
		}
		
		public void OnDomainActivated(object sender, DomainEventArgs args)
		{
			// Make sure that the domain event is for the domain this dialog is showing
			if (args.DomainID == domain.ID)
			{
				EnableAccountButton.Toggled -= new EventHandler(OnEnableAccountToggled);
				EnableAccountButton.Active = true;
				EnableAccountButton.Toggled += new EventHandler(OnEnableAccountToggled);
			}
		}

		public void OnDomainInactivated(object sender, DomainEventArgs args)
		{
			// Make sure that the domain event is for the domain this dialog is showing
			if (args.DomainID == domain.ID)
			{
				EnableAccountButton.Toggled -= new EventHandler(OnEnableAccountToggled);
				EnableAccountButton.Active = false;
				EnableAccountButton.Toggled += new EventHandler(OnEnableAccountToggled);
			}
		}

		public void OnNewDefaultDomain(object sender, NewDefaultDomainEventArgs args)
		{
			if (args.NewDomainID == domain.ID)
			{
				// This domain was just made the new default
				DefaultAccountButton.Toggled -= new EventHandler(OnDefaultAccountToggled);

				DefaultAccountButton.Active = true;
				DefaultAccountButton.Sensitive = false;

				DefaultAccountButton.Toggled += new EventHandler(OnDefaultAccountToggled);
			}
			else if (args.OldDomainID == domain.ID)
			{
				// This domain is no longer the default
				DefaultAccountButton.Toggled -= new EventHandler(OnDefaultAccountToggled);

				DefaultAccountButton.Active = false;
				DefaultAccountButton.Sensitive = true;

				DefaultAccountButton.Toggled += new EventHandler(OnDefaultAccountToggled);
			}
		}
		
		private void OnDomainDeleted(object sender, DomainEventArgs args)
		{
			// Make sure that the domain event is for the domain this dialog is showing
			if (args.DomainID == domain.ID)
			{
				// Close and destroy this window since this domain is no longer valid
				this.Hide();
				this.Destroy();
			}
		}

		public void OnEnableAccountToggled(object o, EventArgs args)
		{
			if (EnableAccountButton.HasFocus)
			{
				if (EnableAccountButton.Active != domain.Active)
				{
					try
					{
						if (EnableAccountButton.Active)
						{
							domainController.ActivateDomain(domain.ID);
						}
						else
						{
							domainController.InactivateDomain(domain.ID);
							
							// Also cause this account to be logged out
//							domainController.LogoutDomain(domain.ID);
//							domain.Authenticated = false;
						}
					}
					catch(Exception e)
					{
						string header;
						string message;
						
						if (EnableAccountButton.Active)
						{
							header = Util.GS("Could not enable this account");
							message = Util.GS("There was an error enabling this account.");
						}
						else
						{
							header = Util.GS("Could not disable this account");
							message = Util.GS("There was an error disabling this account.");
						}
						
						// FIXME: Register this as a modal window
						iFolderMsgDialog dg = new iFolderMsgDialog(
							this,
							iFolderMsgDialog.DialogType.Error,
							iFolderMsgDialog.ButtonSet.Ok,
							"",
							header,
							message,
							e.Message);
						dg.Run();
						dg.Hide();
						dg.Destroy();
						
						// Change the toggle button back to its original value
						EnableAccountButton.Toggled -= new EventHandler(OnEnableAccountToggled);
						EnableAccountButton.Active = !EnableAccountButton.Active;
						EnableAccountButton.Toggled -= new EventHandler(OnEnableAccountToggled);
					}
				}
			}
		}

		public void OnDefaultAccountToggled(object o, EventArgs args)
		{
			if (DefaultAccountButton.HasFocus)
			{
				// The DefaultAccountButton is not sensitive (enabled) if this
				// is the default account, so the only state that needs to be
				// handled here is when a user activates the check button.
				if (!DefaultAccountButton.Active) return;	// This condition should never be hit

				try
				{
					domainController.SetDefaultDomain(domain.ID);
				}
				catch (Exception e)
				{
					// FIXME: Register this as a modal window
					iFolderMsgDialog dg = new iFolderMsgDialog(
						this,
						iFolderMsgDialog.DialogType.Error,
						iFolderMsgDialog.ButtonSet.Ok,
						"",
						Util.GS("Could not make this account the default"),
						Util.GS("There was an error making this account the default."),
						e.Message);
					dg.Run();
					dg.Hide();
					dg.Destroy();
					
					// Change the toggle button back to its original value
					DefaultAccountButton.Toggled -= new EventHandler(OnDefaultAccountToggled);
					DefaultAccountButton.Active = !DefaultAccountButton.Active;
					DefaultAccountButton.Toggled -= new EventHandler(OnDefaultAccountToggled);
				}
			}
		}
		
		private void OnServerAddressChanged(object o, EventArgs args)
		{
			// FIXME: Add in code to check this against the original value
			bServerAddressChanged = true;
		}
		
		private void OnServerAddressFocusOut(object o, FocusOutEventArgs args)
		{
			if (bServerAddressChanged)
				SaveServerAddress();
		}
		
		private void OnPasswordChanged(object o, EventArgs args)
		{
			// FIXME: Add in code to check this against the original value
			bPasswordChanged = true;
		}

		private void OnPasswordFocusOut(object o, FocusOutEventArgs args)
		{
			if (bPasswordChanged)
				SavePassword();
		}
	
		private void OnRememberPasswordToggled(object o, EventArgs args)
                {
                	if (RememberPasswordButton.HasFocus)
                       	{
                        	if(!RememberPasswordButton.Active)
                               	{
                                	PasswordEntry.Text = "";
                               	}
                                SavePassword();
                       }
                }
	
		void KeyPressHandler(object o, KeyPressEventArgs args)
		{
			args.RetVal = true;
			
			switch(args.Event.Key)
			{
				case Gdk.Key.Escape:
					CloseDialog();
					break;
				case Gdk.Key.Control_L:
				case Gdk.Key.Control_R:
					ControlKeyPressed = true;
					args.RetVal = false;
					break;
				case Gdk.Key.W:
				case Gdk.Key.w:
					if (ControlKeyPressed)
						CloseDialog();
					else
						args.RetVal = false;
					break;
				default:
					args.RetVal = false;
					break;
			}
		}
		
		void KeyReleaseHandler(object o, KeyReleaseEventArgs args)
		{
			args.RetVal = false;
			
			switch(args.Event.Key)
			{
				case Gdk.Key.Control_L:
				case Gdk.Key.Control_R:
					ControlKeyPressed = false;
					break;
				default:
					break;
			}
		}
		
        /// <summary>
        /// Close Dialog
        /// </summary>
		public void CloseDialog()
		{
			if (bServerAddressChanged)
				if (!SaveServerAddress())
					return;
				
			if (bPasswordChanged)
				if (!SavePassword())
					return;

			this.Hide();
			this.Destroy();
		}
	}
}
