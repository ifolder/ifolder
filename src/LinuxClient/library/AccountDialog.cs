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


using Gtk;
using System;

namespace Novell.iFolder
{
	public class AccountDialog : Dialog
	{
		private Entry		nameEntry;
		private Entry		passEntry;
		private Entry		serverEntry;
		private DomainWeb	domain;
		private bool		isNewAccount;
		private Button		savePasswordButton;
		private Button		autoLoginButton;
		private Button		defaultAccButton;


		public string UserName
		{
			get
			{
				return nameEntry.Text;
			}
		}

		public string Password
		{
			get
			{
				return passEntry.Text;
			}
		}

		public string Host
		{
			get
			{
				return serverEntry.Text;
			}
		}

		public DomainWeb Domain
		{
			get
			{
				return domain;
			}
		}

		public AccountDialog(DomainWeb curDomain)
			: base()
		{
			domain = curDomain;
			isNewAccount = false;
			SetupDialog();
		}

		public AccountDialog() 
			: base()
 		{
			domain = null;
			isNewAccount = true;
			SetupDialog();
		}

		private void SetupDialog()
		{
			this.Title = Util.GS("iFolder Account");
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder.png"));
			this.HasSeparator = false;

			this.Resizable = false;
			this.Modal = true;
			this.DefaultResponse = ResponseType.Ok;

			Table loginTable;

			loginTable = new Table(4,2,false);

			loginTable.BorderWidth = 10;
			loginTable.RowSpacing = 10;
			loginTable.ColumnSpacing = 10;
			loginTable.Homogeneous = false;

			Label nameLabel = new Label(Util.GS("User name:"));
			nameLabel.Xalign = 1; 
			loginTable.Attach(nameLabel, 0,1,0,1,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			nameEntry = new Entry();
			nameEntry.Changed += new EventHandler(OnFieldsChanged);
			nameEntry.ActivatesDefault = true;
			nameEntry.WidthChars = 35;
			loginTable.Attach(nameEntry, 1,2,0,1, 
					AttachOptions.Fill | AttachOptions.Expand, 0,0,0);

			if(!isNewAccount)
			{
				if(domain.UserName != null)
					nameEntry.Text = domain.UserName;
				nameEntry.Editable = false;
			}


			Label passLabel = new Label(Util.GS("Password:"));
			passLabel.Xalign = 1;
			loginTable.Attach(passLabel, 0,1,1,2,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			passEntry = new Entry();
			passEntry.Changed += new EventHandler(OnFieldsChanged);
			passEntry.ActivatesDefault = true;
			passEntry.Visibility = false;
			loginTable.Attach(passEntry, 1,2,1,2,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);

			Label serverLabel = new Label(Util.GS("Server host:"));
			serverLabel.Xalign = 1;
			loginTable.Attach(serverLabel, 0,1,2,3,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			serverEntry = new Entry();
			serverEntry.Changed += new EventHandler(OnFieldsChanged);
			serverEntry.ActivatesDefault = true;
			loginTable.Attach(serverEntry, 1,2,2,3,
					AttachOptions.Fill | AttachOptions.Expand, 0,0,0);

			if(!isNewAccount)
			{
				if(domain.Host != null)
					serverEntry.Text = domain.Host;
				serverEntry.Editable = false;
			}

			VBox optBox = new VBox();

			savePasswordButton = 
				new CheckButton(Util.GS(
					"_Remember password"));

			optBox.PackStart(savePasswordButton, false, false,0);

			autoLoginButton = 
				new CheckButton(Util.GS(
					"_Auto Login"));
			optBox.PackStart(autoLoginButton, false, false,0);

			defaultAccButton = 
				new CheckButton(Util.GS(
					"_Default account"));
			optBox.PackStart(defaultAccButton, false, false,0);

			loginTable.Attach(optBox, 1,2,3,4,
					AttachOptions.Fill | AttachOptions.Expand, 0,0,0);


	
			this.VBox.PackStart(loginTable, true, true, 0);
			this.VBox.ShowAll();

			this.AddButton(Stock.Cancel, ResponseType.Cancel);
			if(isNewAccount)
				this.AddButton(Util.GS("Login"), ResponseType.Ok);
			else
				this.AddButton(Util.GS("OK"), ResponseType.Ok);
			
			if(isNewAccount)
				this.SetResponseSensitive(ResponseType.Ok, false);


			this.DefaultResponse = ResponseType.Ok;
		}


		private void OnFieldsChanged(object obj, EventArgs args)
		{
			bool enableOK = false;

			if(!isNewAccount)
				enableOK = true;
			else if( (nameEntry.Text.Length > 0) &&
					(passEntry.Text.Length > 0 ) &&
					(serverEntry.Text.Length > 0) )
				enableOK = true;

			this.SetResponseSensitive(ResponseType.Ok, enableOK);
		}
	}
}
