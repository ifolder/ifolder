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
  *                 $Author: Calvin Gaisford <cgaisford@novell.com>
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

namespace Novell.iFolder
{
    /// <summary>
    /// class iFolderLoginDialog
    /// </summary>
	public class iFolderLoginDialog : Dialog
	{
		private Entry	nameEntry;
		private Entry	passEntry;
		private Entry	serverEntry;
		private CheckButton savePasswordButton;
		private string	DomainID;
		private string  DomainName;
		private string  DomainUserName;
		private bool	FullDialog;

		private Image				 iFolderBanner;
		private Image				 iFolderScaledBanner;
		private Gdk.Pixbuf			 ScaledPixbuf;

        /// <summary>
        /// Gets UserName
        /// </summary>
		public string UserName
		{
			get
			{
				if(FullDialog)
					return nameEntry.Text;
				else
					return "";
			}
		}

        /// <summary>
        /// Gets Password
        /// </summary>
		public string Password
		{
			get
			{
				return passEntry.Text;
			}
			set
			{
				passEntry.Text = value;
			}
		}

        /// <summary>
        /// Gets Host
        /// </summary>
		public string Host
		{
			get
			{
				if(FullDialog)
					return serverEntry.Text;
				else
					return "";
			}
		}

        /// <summary>
        /// Gets Domain
        /// </summary>
		public string Domain
		{
			get
			{
				if(FullDialog)
					return "";
				else
					return DomainID;
			}
		}
		
        /// <summary>
        /// Gets Should Save Password
        /// </summary>
		public bool ShouldSavePassword
		{
			get
			{
				if (savePasswordButton != null)
					return savePasswordButton.Active;
				else
					return false;
					
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="domainName"></param>
        /// <param name="userName"></param>
		public iFolderLoginDialog(string domain, string domainName,
					string userName) : base()
		{
			DomainID = domain;
			DomainName = domainName;
			DomainUserName = userName;
			FullDialog = false;
			SetupDialog();
		}

        /// <summary>
        /// Constructor
        /// </summary>
		public iFolderLoginDialog() : base()
 		{
			FullDialog = true;
			SetupDialog();
		}

        /// <summary>
        /// Setup Dialog
        /// </summary>
		private void SetupDialog()
		{
			this.Title = Util.GS("iFolder Login");
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
	
			Table loginTable;

			loginTable = new Table(4,2,false);

			loginTable.BorderWidth = 10;
			loginTable.RowSpacing = 10;
			loginTable.ColumnSpacing = 10;
			loginTable.Homogeneous = false;
	
			if(FullDialog)
			{
				Label nameLabel = new Label(Util.GS("_User name:"));
				nameLabel.Xalign = 1; 
				loginTable.Attach(nameLabel, 0,1,0,1,
						AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
	
				nameEntry = new Entry();
				nameEntry.Changed += new EventHandler(OnFieldsChanged);
				nameEntry.ActivatesDefault = true;
				loginTable.Attach(nameEntry, 1,2,0,1, 
						AttachOptions.Fill | AttachOptions.Expand, 0,0,0);

				Label passLabel = new Label(Util.GS("_Password:"));
				passLabel.Xalign = 1;
				loginTable.Attach(passLabel, 0,1,1,2,
						AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
	
				passEntry = new Entry();
				passEntry.Changed += new EventHandler(OnFieldsChanged);
				passEntry.ActivatesDefault = true;
				passEntry.Visibility = false;
				loginTable.Attach(passEntry, 1,2,1,2,
					AttachOptions.Fill | AttachOptions.Expand, 0,0,0);

				Label serverLabel = new Label(Util.GS("iFolder server:"));
				serverLabel.Xalign = 1;
				loginTable.Attach(serverLabel, 0,1,2,3,
						AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
	
				serverEntry = new Entry();
				serverEntry.Changed += new EventHandler(OnFieldsChanged);
				serverEntry.ActivatesDefault = true;
				loginTable.Attach(serverEntry, 1,2,2,3,
						AttachOptions.Fill | AttachOptions.Expand, 0,0,0);

				savePasswordButton = 
					new CheckButton(Util.GS(
						"_Remember password"));
				loginTable.Attach(savePasswordButton, 1,2,3,4,
						AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			}
			else
			{
				Label domainLabel = new Label(Util.GS("iFolder Account:"));
				domainLabel.Xalign = 1;
				loginTable.Attach(domainLabel, 0,1,0,1,
						AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);


				Label domainNameLabel = new Label(DomainName);
				domainNameLabel.UseUnderline = false;
				domainNameLabel.Xalign = 0;
				loginTable.Attach(domainNameLabel, 1,2,0,1,
						AttachOptions.Fill | AttachOptions.Expand, 0,0,0);



				Label nameLabel = new Label(Util.GS("User Name")+":");
				nameLabel.Xalign = 1;
				loginTable.Attach(nameLabel, 0,1,1,2,
						AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
	
				Label userNameLabel = new Label(DomainUserName);
				userNameLabel.UseUnderline = false;
				userNameLabel.Xalign = 0;
				loginTable.Attach(userNameLabel, 1,2,1,2, 
						AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
						

				Label passLabel = new Label(Util.GS("Password:"));
				passLabel.Xalign = 1;
				loginTable.Attach(passLabel, 0,1,2,3,
						AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
	
				passEntry = new Entry();
				passEntry.Changed += new EventHandler(OnFieldsChanged);
				passEntry.ActivatesDefault = true;
				passEntry.Visibility = false;
				loginTable.Attach(passEntry, 1,2,2,3,
					AttachOptions.Fill | AttachOptions.Expand, 0,0,0);

				savePasswordButton = 
					new CheckButton(Util.GS(
						"_Remember password"));
				loginTable.Attach(savePasswordButton, 1,2,3,4,
						AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			}

	
			this.VBox.PackStart(loginTable, false, false, 0);
			this.VBox.ShowAll();

			this.AddButton(Stock.Cancel, ResponseType.Cancel);
			this.AddButton(Util.GS("Co_nnect"), ResponseType.Ok);
			this.SetResponseSensitive(ResponseType.Ok, false);
			this.DefaultResponse = ResponseType.Ok;
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
			// Don't check for password... Fix for bug #235354
			if( FullDialog &&
				(nameEntry.Text.Length > 0) &&
			/*	(passEntry.Text.Length > 0 ) &&*/
				(serverEntry.Text.Length > 0) )
				enableOK = true;
			else if( (!FullDialog)// &&
			/*	(passEntry.Text.Length > 0 )*/ )
				enableOK = true;

			this.SetResponseSensitive(ResponseType.Ok, enableOK);
		}
	}
}
