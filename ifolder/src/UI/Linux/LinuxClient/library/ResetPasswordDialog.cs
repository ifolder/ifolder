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
*                 $Author: Anil Kumar <kuanil@novell.com>
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
using Novell.iFolder.Controller;

namespace Novell.iFolder
{
	public class ResetPasswordDialog : Dialog
	{

		private ComboBox domainComboBox;
		private Entry oldPassword;
		private Entry newPassword;
		private Entry confirmPassword;
		private DomainInformation[] domains;
		private CheckButton savePassword;
		private iFolderWebService ifws;
		private SimiasWebService simws;	
		private Image				 iFolderBanner;
		private Image				 iFolderScaledBanner;
		private Gdk.Pixbuf			 ScaledPixbuf;
		private bool status;
		private int passwordChangeStatus;

		enum ResetPasswordStatus
		{
			IncorrectOldPassword =1,
			FailedToResetPassword =2,
			LoginDisabled=3,
			UserAccountExpired=4,
			UserCannotChangePassword=5,
			UserPasswordExpired=6,
			MinimumPasswordLengthExcceded=7,
			UserNotFoundInSimias=8,
			NoLoggedInDomainsPasswordText=9,
			NotSupportedServerOld=10,
		};




		public int PasswordChangeStatus
		{
			get
			{
				return this.passwordChangeStatus;
			}
		}

		public DomainInformation[] Domains
		{
			get
			{
				return this.domains;
			}
			set
			{
				this.domains = value;
			}
		}

		public string DomainID
                {
                        get
                        {
                                int activeIndex = domainComboBox.Active;
                                if (activeIndex >= 0)
                                        return domains[activeIndex].ID;
                                else
                                        return null;
                        }
                }


		public string Domain
		{
			get
			{
				if( domains != null)
					return domains[domainComboBox.Active].ID;
				else 
					return null;
			}
		}

		public string OldPassword
		{
			get
			{
				return oldPassword.Text;
			}
		}

		public string NewPassword
		{
			get
			{
				return newPassword.Text;
			}
		}

		public bool SavePassword
		{
			get
			{
				return savePassword.Active;
			}
		}

		public bool Status
		{
			get
			{
				return status;
			}

		}

		public ResetPasswordDialog(SimiasWebService simiasws, iFolderWebService ifws)
		{
			this.ifws = ifws;
			this.simws = simiasws;
			SetupDialog();
		}
		
		private void SetupDialog()
		{
			this.Title = Util.GS("Change password");
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder16.png"));
			this.HasSeparator = false;
//			this.BorderWidth = 10;
			this.SetDefaultSize (450, 100);
	//		this.Resizable = false;
			this.Modal = true;
			this.DefaultResponse = ResponseType.Ok;

			//-----------------------------
			// Add iFolderGraphic
			//-----------------------------
			HBox imagebox = new HBox();
			imagebox.Spacing = 0;
			iFolderBanner = new Image(
					new Gdk.Pixbuf(Util.ImagesPath("ifolder-banner.png")));
			imagebox.PackStart(iFolderBanner, false, false, 0);

			ScaledPixbuf = 
				new Gdk.Pixbuf(Util.ImagesPath("ifolder-banner-scaler.png"));
			iFolderScaledBanner = new Image(ScaledPixbuf);
			iFolderScaledBanner.ExposeEvent += 
					new ExposeEventHandler(OnBannerExposed);
			imagebox.PackStart(iFolderScaledBanner, true, true, 0);
			this.VBox.PackStart (imagebox, false, true, 0);

			Table table = new Table(6, 2, false);
			this.VBox.PackStart(table, false, false, 0);
			table.ColumnSpacing = 6;
			table.RowSpacing = 6;
			table.BorderWidth = 12;

			// Row 1
			Label lbl = new Label(Util.GS("_iFolder Account")+":");
			table.Attach(lbl, 0,1, 0,1,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			lbl.LineWrap = true;
			lbl.Xalign = 0.0F;
			domainComboBox = ComboBox.NewText();
				table.Attach(domainComboBox, 1,2, 0,1,
					AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
			// Row 2
				
			lbl = new Label(Util.GS("_Current password")+":");
			table.Attach(lbl, 0,1, 1,2,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			lbl.LineWrap = true;
			lbl.Xalign = 0.0F;
			
			oldPassword = new Entry();
			oldPassword.Visibility = false;
			table.Attach(oldPassword, 1,2, 1,2,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			lbl.MnemonicWidget = oldPassword;
			oldPassword.Changed += new EventHandler(UpdateSensitivity);

			// Row 3	
			lbl = new Label(Util.GS("_New password")+":");
			table.Attach(lbl, 0,1, 2,3,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			lbl.LineWrap = true;
			lbl.Xalign = 0.0F;
			
			newPassword = new Entry();
			newPassword.Visibility = false;
			table.Attach(newPassword, 1,2, 2,3,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			lbl.MnemonicWidget = newPassword;
			newPassword.Changed += new EventHandler(UpdateSensitivity);	

			// Row 4	
			lbl = new Label(Util.GS("Confirm new _password")+":");
			table.Attach(lbl, 0,1, 3,4,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			lbl.LineWrap = true;
			lbl.Xalign = 0.0F;
			
			confirmPassword = new Entry();
			confirmPassword.Visibility = false;
			table.Attach(confirmPassword, 1,2, 3,4,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			lbl.MnemonicWidget = confirmPassword;			
			confirmPassword.Changed += new EventHandler(UpdateSensitivity);		

			// Row 5
			
			// Row 6
			savePassword = new CheckButton(Util.GS("_Remember password"));
			table.Attach(savePassword, 1,2,5,6, AttachOptions.Expand|AttachOptions.Fill, 0,0,0);
			
			this.VBox.ShowAll();
		        Button helpbutton = (Button)this.AddButton(Stock.Help, ResponseType.Help); 
			helpbutton.Sensitive = true;
			helpbutton.Clicked += new EventHandler(OnHelpButtonClicked);
			this.AddButton(Stock.Cancel, ResponseType.Cancel);
			Button but = (Button)this.AddButton(Util.GS("Reset"), ResponseType.Ok);
			but.Clicked += new EventHandler(OnResetClicked);
			but.Image = new Image(Stock.Undo, Gtk.IconSize.Menu);//new Image(new Gdk.Pixbuf(Util.ImagesPath("ifolder-download16.png")));
			this.SetResponseSensitive(ResponseType.Ok, false);
			this.DefaultResponse = ResponseType.Ok;
			this.Realized += new EventHandler(OnResetPasswordLoad);	
			domainComboBox.Changed += new EventHandler(OnDomainChangedEvent);
		}

		private void ResetPassword(string domainid, string oldpassword, string newpassword)
		{
			try{
				this.passwordChangeStatus = this.ifws.ChangePassword(domainid, oldpassword, newpassword);
			}
			catch(System.Web.Services.Protocols.SoapException ex)
			{
				if(ex.Message.IndexOf("Server did not recognize the value of HTTP header SOAPAction") != -1)
				{
					this.passwordChangeStatus = (int)ResetPasswordStatus.NotSupportedServerOld;
				}
			}
			catch{ }

			if( this.passwordChangeStatus == 0)
			{
				try
				{
					DomainController domainController = DomainController.GetDomainController();
					domainController.LogoutDomain(domainid);
				}
				catch{ }
				this.status = true;
			}

                                if( this.passwordChangeStatus != 0)
                                {
                                        string Message = Util.GS("Could not change password, ");
                                        switch(this.passwordChangeStatus)
                                        {
                                                case (int)ResetPasswordStatus.IncorrectOldPassword:
                                                        Message += Util.GS("Incorrect old password.");
                                                        break;
                                                case (int)ResetPasswordStatus.FailedToResetPassword:
                                                        Message += Util.GS("Failed to reset password.");
                                                        break;
                                                case (int)ResetPasswordStatus.LoginDisabled:
                                                        Message += Util.GS("Login disabled.");
                                                        break;
                                                case (int)ResetPasswordStatus.UserAccountExpired:
                                                        Message += Util.GS("User account expired.");
                                                        break;
                                                case (int)ResetPasswordStatus.UserCannotChangePassword:
                                                        Message += Util.GS("User can not change password.");
                                                        break;
                                                case (int)ResetPasswordStatus.UserPasswordExpired:
                                                        Message += Util.GS("User password expired.");
                                                        break;
                                               case (int)ResetPasswordStatus.MinimumPasswordLengthExcceded:
                                                        Message += Util.GS("Minimum password length restriction not met.");
                                                        break;
                                                case (int)ResetPasswordStatus.UserNotFoundInSimias:
                                                        Message += Util.GS("User not found in simias.");
                                                        break;
						case (int)ResetPasswordStatus.NoLoggedInDomainsPasswordText:
							Message += Util.GS("For changing password the domain should be connected. Log on to the domain and try.");
							break;
						case (int)ResetPasswordStatus.NotSupportedServerOld:
							Message += Util.GS("This operation is not supported on the current version of the iFolder server. To perform this operation, you must upgrade to the latest version of iFolder server.");
							break;
                                                default:
                                                        Message = "Error while changing the password.";
                                                        break;
                                        }
                                        iFolderMsgDialog dialog = new iFolderMsgDialog(
                                                                                                                null,
                                                                                                                iFolderMsgDialog.DialogType.Error,
                                                                                                                iFolderMsgDialog.ButtonSet.None,
                                                                                                                Util.GS("Change password"),
                                                                                                                Message, null);
                                        dialog.Run();
                                        dialog.Hide();
                                        dialog.Destroy();
                                        dialog = null;
				}
		}
		private void OnHelpButtonClicked(object o, EventArgs args)
		{
			try
			{
				Util.ShowHelp("bkmgmdj.html", this);	
			}
			catch{ }
		}

		private void OnResetClicked( object o, EventArgs args)
		{
			if( newPassword.Text == oldPassword.Text )
			{
				string Message = Util.GS("Old password and new password should not be same.");
				iFolderMsgDialog dialog = new iFolderMsgDialog(
						null,
						iFolderMsgDialog.DialogType.Error,
						iFolderMsgDialog.ButtonSet.None,
						Util.GS("Error changing password"),
						Message, null);
				dialog.Run();
				dialog.Hide();
				dialog.Destroy();
				dialog = null;
				return;
			}
			else if(newPassword.Text != confirmPassword.Text)
			{
				string Message = Util.GS("New password and confirm password do not match.");
				iFolderMsgDialog dialog = new iFolderMsgDialog(
						null,
						iFolderMsgDialog.DialogType.Error,
						iFolderMsgDialog.ButtonSet.None,
						Util.GS("Error changing password"),
						Message, null);
				dialog.Run();
				dialog.Hide();
				dialog.Destroy();
				dialog = null;

				return;
			}
			if (this.GdkWindow != null) 
				this.GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Watch);
			ResetPassword( this.Domain, this.OldPassword, this.NewPassword);
			if (this.GdkWindow != null) 
				this.GdkWindow.Cursor = null;
		}

		protected bool OnDeleteEvent(object o, EventArgs args)
		{
			return true;
		}


		private void OnResetPasswordLoad( object o, EventArgs args)
		{
			DomainController domainController = DomainController.GetDomainController();
			domains = domainController.GetLoggedInDomains();
			if( domains == null)
			{
					this.Respond( ResponseType.DeleteEvent);
					return;	
			}
			 string defaultDomainID = simws.GetDefaultDomainID();
			 int defaultDomain = 0 ;
			for (int x = 0; x < domains.Length; x++)
			{
				domainComboBox.AppendText(domains[x].Name+"-"+domains[x].Host);
				 if(defaultDomainID != null && defaultDomainID == domains[x].ID)
	                                       defaultDomain = x;

			}
			if( domains.Length > 0)
				domainComboBox.Active = defaultDomain;
			oldPassword.Sensitive = newPassword.Sensitive = confirmPassword.Sensitive = savePassword.Sensitive = true;
		}

		private void OnDomainChangedEvent( object o, EventArgs args)
		{
		}

		private void UpdateSensitivity( object o, EventArgs args)
		{
			if( oldPassword != null && newPassword != null && confirmPassword != null)
			{
			//	if( newPassword.Text.Length >0 && newPassword.Text == confirmPassword.Text && newPassword.Text != oldPassword.Text)
				if( newPassword.Text.Length > 0 && confirmPassword.Text.Length > 0 && oldPassword.Text.Length > 0)
				{
					// Check for validity of passphrase
					if( oldPassword.Text.Length > 0)
					{
						this.SetResponseSensitive( ResponseType.Ok, true);
						return;	
					}
				}
			}
			this.SetResponseSensitive( ResponseType.Ok, false);
		}
		private void OnBannerExposed(object o, ExposeEventArgs args)
		{
			if(args.Event.Count > 0)
				return;

			Gdk.Pixbuf spb = 
				ScaledPixbuf.ScaleSimple(iFolderScaledBanner.Allocation.Width,
										iFolderScaledBanner.Allocation.Height,
										Gdk.InterpType.Nearest);

			Gdk.GC gc = new Gdk.GC(iFolderScaledBanner.GdkWindow);

			spb.RenderToDrawable(iFolderScaledBanner.GdkWindow,
											gc,
											0, 0,
											args.Event.Area.X,
											args.Event.Area.Y,
											args.Event.Area.Width,
											args.Event.Area.Height,
											Gdk.RgbDither.Normal,
											0, 0);
		}

		private void OnFieldsChanged(object obj, EventArgs args)
		{
			bool enableOK = false;
			this.SetResponseSensitive(ResponseType.Ok, enableOK);
		}
	}
}
