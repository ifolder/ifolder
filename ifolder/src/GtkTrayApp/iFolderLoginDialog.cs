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
 *			Code based on examples from the book "Mono A Develper's Notebook"
 				by Edd Dumbill and Niel M. Bornstein
 * 
 ***********************************************************************/


using Gtk;
using System;

namespace Novell.iFolder
{
	public class iFolderLoginDialog : Dialog
	{
		private Entry	nameEntry;
		private Entry	passEntry;
		private Entry	serverEntry;


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


		public iFolderLoginDialog() : base()
 		{
			this.Title = "iFolder Login";
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder.png"));
			this.HasSeparator = false;
//			this.BorderWidth = 10;
			this.Resizable = false;
			this.Modal = true;
			this.DefaultResponse = ResponseType.Ok;

			Image iFolderImage = new Image(
					new Gdk.Pixbuf(Util.ImagesPath("ifolder-banner.png")));
			this.VBox.PackStart (iFolderImage, false, false, 0);
	
			Table loginTable = new Table(3,2,false);
			loginTable.BorderWidth = 10;
			loginTable.RowSpacing = 10;
			loginTable.ColumnSpacing = 10;
			loginTable.Homogeneous = false;
	
			Label nameLabel = new Label("User Name:");
			nameLabel.Xalign = 0;
			loginTable.Attach(nameLabel, 0,1,0,1,
					AttachOptions.Shrink, 0,0,0);
	
			nameEntry = new Entry();
			nameEntry.Changed += new EventHandler(OnFieldsChanged);
			nameEntry.ActivatesDefault = true;
			loginTable.Attach(nameEntry, 1,2,0,1, 
					AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
	
			Label passLabel = new Label("Password:");
			passLabel.Xalign = 0;
			loginTable.Attach(passLabel, 0,1,1,2,
					AttachOptions.Shrink, 0,0,0);
	
			passEntry = new Entry();
			passEntry.Changed += new EventHandler(OnFieldsChanged);
			passEntry.ActivatesDefault = true;
			passEntry.Visibility = false;
			loginTable.Attach(passEntry, 1,2,1,2,
					AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
	
			Label serverLabel = new Label("Server Host:");
			serverLabel.Xalign = 0;
			loginTable.Attach(serverLabel, 0,1,2,3,
					AttachOptions.Shrink, 0,0,0);
	
			serverEntry = new Entry();
			serverEntry.Changed += new EventHandler(OnFieldsChanged);
			serverEntry.ActivatesDefault = true;
			loginTable.Attach(serverEntry, 1,2,2,3,
					AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
	
			this.VBox.PackStart(loginTable, false, false, 0);
			this.VBox.ShowAll();

			this.AddButton(Stock.Cancel, ResponseType.Cancel);
			this.AddButton("_Login", ResponseType.Ok);
			this.SetResponseSensitive(ResponseType.Ok, false);
		}


		private void OnFieldsChanged(object obj, EventArgs args)
		{
			if(	(nameEntry.Text.Length > 0) &&
				(passEntry.Text.Length > 0 ) &&
				(serverEntry.Text.Length > 0) )
			{
				this.SetResponseSensitive(ResponseType.Ok, true);
			}
			else
			{
				this.SetResponseSensitive(ResponseType.Ok, false);
			}
		}
	}
}
