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

using System;
using Gtk;
using Simias.Client;

namespace Novell.iFolder
{
    /// <summary>
    /// class RemoveAccountDialog
    /// </summary>
	public class RemoveAccountDialog : Dialog
	{
		private CheckButton cbutton;

        /// <summary>
        /// Gets the RemoveiFolderFromServer Button status
        /// </summary>
		public bool RemoveiFoldersFromServer
		{
			get
			{
				return cbutton.Active;
			}
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="domainInfo">Domain Information</param>
		public RemoveAccountDialog(DomainInformation domainInfo) : base()
		{
			this.Title = "";
			this.Resizable = false;
			this.HasSeparator = false;
		
			HBox h = new HBox();
			h.BorderWidth = 10;
			h.Spacing = 10;

			Image i = new Image();
			i.SetFromStock(Gtk.Stock.DialogQuestion, IconSize.Dialog);

			i.SetAlignment(0.5F, 0);
			h.PackStart(i, false, false, 0);

			VBox v = new VBox();
			Label l = new Label("<span weight=\"bold\" size=\"larger\">" +
					Util.GS("Remove this iFolder Account?") + "</span>");
			l.LineWrap = true;
			l.UseMarkup = true;
			l.Selectable = false;
			l.Xalign = 0; l.Yalign = 0;
			v.PackStart(l, false, false, 10);
			
			Table table = new Table(3, 2, false);
			
			table.RowSpacing = 0;
			table.ColumnSpacing = 10;
			table.Homogeneous = false;

			//
			// Row: Server
			//
			l = new Label(Util.GS("iFolder account name:"));
			l.Xalign = 1;
			table.Attach(l, 0,1, 0,1,
						 AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			l = new Label(domainInfo.Name);
			l.UseUnderline = false;
			l.Xalign = 0;
			table.Attach(l, 1,2, 0,1,
						 AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			
			//
			// Row: Server host
			//
			l = new Label(Util.GS("Server:"));
			l.Xalign = 1;
			table.Attach(l, 0,1, 1,2,
						 AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			l = new Label(domainInfo.Host);
			l.Xalign = 0;
			table.Attach(l, 1,2, 1,2,
						 AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			
			//
			// Row: Username
			//
			l = new Label(Util.GS("User name:"));
			l.Xalign = 1;
			table.Attach(l, 0,1, 2,3,
						 AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			l = new Label(domainInfo.MemberName);
			l.UseUnderline = false;
			l.Xalign = 0;
			table.Attach(l, 1,2, 2,3,
						 AttachOptions.Fill | AttachOptions.Expand, 0,0,0);

			v.PackEnd(table, true, true, 0);
			h.PackEnd(v);

			this.VBox.PackStart(h, true, true, 0);

			cbutton = new CheckButton(Util.GS("_Remove my iFolders and files from the server"));
			this.VBox.PackStart(cbutton, false, false, 10);


			cbutton.Toggled +=
				new EventHandler(OnRemoveiFoldersToggled);

			// If the account is not logged-in or it's disabled,
			// gray out this checkbox.  This fixes bug #86867.
			if (!domainInfo.Active || !domainInfo.Authenticated)
			{
				cbutton.Sensitive = false;
			}

			this.VBox.ShowAll();

//			this.AddButton(Stock.Help, ResponseType.Help);

			Button noButton = new Button(Stock.No);
			noButton.CanFocus = true;
			noButton.CanDefault = true;
			noButton.ShowAll();

			this.AddActionWidget(noButton, ResponseType.No);
			this.AddButton(Stock.Yes, ResponseType.Yes);

			this.DefaultResponse = ResponseType.No;

			this.FocusChild = noButton;
		}
		
        /// <summary>
        /// Event Handler for Remove iFolders Toggled event
        /// </summary>
        private void OnRemoveiFoldersToggled(object obj, EventArgs args)
		{
			// If the button is being toggled for the first time since this
			// dialog has been opened, warn the user about what this action
			// will do.
			if (cbutton.Active)
			{
				iFolderMsgDialog dialog = new iFolderMsgDialog(
					this,
					iFolderMsgDialog.DialogType.Warning,
					iFolderMsgDialog.ButtonSet.OkCancel,
					"",
					Util.GS("Removing iFolders from Server"),
					Util.GS("Removing iFolders from the server will delete the files stored on the server.  Your files will remain intact on this computer.\n\nIf you've shared any iFolders with other users, they will no longer be able to synchronize them with the server.  Additionally, you will no longer be able to access these iFolders from any other computer."));
				int rc = dialog.Run();
				dialog.Hide();
				dialog.Destroy();

				if ((ResponseType)rc == ResponseType.Cancel)
				{
					// Uncheck the Remove iFolders from Server checkbox
					cbutton.Active = false;
				}				
			}
		}
	}
}
