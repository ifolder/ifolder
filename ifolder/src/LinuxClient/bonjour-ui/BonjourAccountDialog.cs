/***********************************************************************
 *  $RCSfile: BonjourAccountDialog.cs,v $
 * 
 *  Copyright (C) 2005 Novell, Inc.
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
 *  Authors:
 *		Boyd Timothy <btimothy@novell.com>
 * 
 ***********************************************************************/

using Gtk;
using System;
using Simias.Client;

using Novell.iFolder.Events;
using Novell.iFolder.Controller;

namespace Novell.iFolder
{
	public class BonjourAccountDialog : AccountDialog
	{
		private iFolderData		ifdata;
		private DomainController	domainController;
		private Manager			simiasManager;
		
		private bool				ControlKeyPressed;
		
		///
		/// Global Widgets
		///
		private CheckButton			EnableAccountButton;
		private CheckButton			DefaultAccountButton;

		///
		/// Widgets
		///
		private TextView			AccountDescriptionTextView;
		private Label				QuotaUsedLabel;

		public BonjourAccountDialog(Window parent, DomainInformation curDomain)
			: base(parent, curDomain)
		{
			ifdata = iFolderData.GetData();
			this.simiasManager = Util.GetSimiasManager();
			
			domainController = DomainController.GetDomainController();
			
			SetupDialog();
		}
		
		~BonjourAccountDialog()
		{
			// Unregister fr domain events
			if (domainController != null)
			{
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

		private void SetupDialog()
		{
			this.Title = string.Format("{0} {1}", domain.Name, Util.GS("Properties"));
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder24.png"));
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
			
			notebook.AppendPage(CreateAccountPage(), new Label(Util.GS("Account")));
			
			notebook.ShowAll();
			return notebook;
		}
		
		private Widget CreateGlobalCheckButtons()
		{
			VBox vbox = new VBox(false, 0);

			EnableAccountButton = new CheckButton(Util.GS("_Enable this account"));
			vbox.PackStart(EnableAccountButton, false, false, 0);

			DefaultAccountButton = new CheckButton(Util.GS("Account is _default"));
			vbox.PackStart(DefaultAccountButton, false, false, 0);
			
			return vbox;
		}
		
		private Widget CreateAccountPage()
		{
			VBox vbox = new VBox(false, 0);
			
			Table table = new Table(4, 2, false);
			vbox.PackStart(table, true, true, 0);
			table.ColumnSpacing = 12;
			table.RowSpacing = 6;
			table.BorderWidth = 12;
			
			///
			/// Row 1 (Server Name)
			///
			Label l = new Label(Util.GS("Account Name:"));
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
			l.Xalign = 0;
			
			///
			/// Row 2 (User Name)
			///
			l = new Label(Util.GS("User Name:"));
			table.Attach(l,
						 0,1, 1,2,
						 AttachOptions.Shrink | AttachOptions.Fill,
						 0,0,0);
			l.Xalign = 0;
			
			l = new Label(domain.MemberName);
			table.Attach(l,
						 1,2, 1,2,
						 AttachOptions.Expand | AttachOptions.Fill,
						 0,0,0);
			l.Xalign = 0;
			
			///
			/// Row 3 (Disk Space Used)
			///
			l = new Label(Util.GS("Disk Space Used:"));
			table.Attach(l,
						 0,1, 2,3,
						 AttachOptions.Expand | AttachOptions.Fill,
						 0,0,0);
			l.Xalign = 0;

			QuotaUsedLabel = new Label("");
			table.Attach(QuotaUsedLabel,
						 1,2, 2,3,
						 AttachOptions.Shrink | AttachOptions.Fill,
						 0,0,0);
			QuotaUsedLabel.Xalign = 0;


			///
			/// Row 4 (Description TextView)
			///
			ScrolledWindow sw = new ScrolledWindow();
			table.Attach(sw,
						 0,2, 3,4,
						 AttachOptions.Expand | AttachOptions.Fill,
						 0,0,0);
			sw.ShadowType = Gtk.ShadowType.EtchedIn;
			TextView AccountDescriptionTextView = new TextView();
			if(domain.Description != null)
				AccountDescriptionTextView.Buffer.Text = domain.Description;
			AccountDescriptionTextView.WrapMode = Gtk.WrapMode.Word;
			AccountDescriptionTextView.Editable = false;
			AccountDescriptionTextView.Sensitive = false;
			AccountDescriptionTextView.CursorVisible = false;
			AccountDescriptionTextView.RightMargin = 5;
			AccountDescriptionTextView.LeftMargin = 5;
			sw.Add(AccountDescriptionTextView);

			return vbox;
		}
		
		private void OnRealizeWidget(object o, EventArgs args)
		{
			InitGlobalCheckButtons();
			InitAccountPage();

			// Register for the domain events that could affect this dialog
			if (domainController != null)
			{
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
		}
		
		private void InitGlobalCheckButtons()
		{
			EnableAccountButton.Active = domain.Active;
			
			DefaultAccountButton.Active = domain.IsDefault;
			DefaultAccountButton.Sensitive = !domain.IsDefault;
		}
		
		private void InitAccountPage()
		{
//			ServerAddressEntry.Text = domain.Host;
//			if (domain.Authenticated)
//			{
//				ServerAddressEntry.Sensitive = false;
//				ServerAddressEntry.Editable = false;
//			}
//			else
//			{
//				ServerAddressEntry.Sensitive = true;
//				ServerAddressEntry.Editable = true;
//			}

			DiskSpace ds = ifdata.GetUserDiskSpace(domain.MemberUserID);

			if(ds == null)
			{
				QuotaUsedLabel.Text = Util.GS("N/A");
			}
			else
			{
				int tmpValue;

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
			}
		}
		
		///
		/// Event Handlers
		///
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
		
		public void CloseDialog()
		{
			this.Hide();
			this.Destroy();
		}
	}
}
