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
using Simias.Client;

namespace Novell.iFolder
{
	public class RemoveAccountDialog : Dialog
	{
		private CheckButton cbutton;
		private bool bUserWarnedAboutRemovingiFolders;

		public bool RemoveiFoldersFromServer
		{
			get
			{
				return cbutton.Active;
			}
		}

		public RemoveAccountDialog(DomainInformation domainInfo) : base()
		{
			bUserWarnedAboutRemovingiFolders = false;
		
			this.Title = Util.GS("Remove Account");
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
					Util.GS("Remove iFolder Account?") + "</span>");
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
			l = new Label(Util.GS("Server:"));
			l.Xalign = 1;
			table.Attach(l, 0,1, 0,1,
						 AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			l = new Label(domainInfo.Name);
			l.Xalign = 0;
			table.Attach(l, 1,2, 0,1,
						 AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			
			//
			// Row: Server host
			//
			l = new Label(Util.GS("Server host:"));
			l.Xalign = 1;
			table.Attach(l, 0,1, 1,2,
						 AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			l = new Label(domainInfo.Host);
			l.Xalign = 0;
			table.Attach(l, 1,2, 1,2,
						 AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			
			//
			// Row: User name
			//
			l = new Label(Util.GS("User name:"));
			l.Xalign = 1;
			table.Attach(l, 0,1, 2,3,
						 AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			l = new Label(domainInfo.MemberName);
			l.Xalign = 0;
			table.Attach(l, 1,2, 2,3,
						 AttachOptions.Fill | AttachOptions.Expand, 0,0,0);

			v.PackEnd(table, true, true, 0);
						
			h.PackEnd(v);

			this.VBox.PackStart(h, true, true, 0);

			cbutton = new CheckButton(Util.GS("Remove my iFolders and files from the server"));
			this.VBox.PackStart(cbutton, false, false, 10);

			cbutton.Toggled +=
				new EventHandler(OnRemoveiFoldersToggled);

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
		
		private void OnRemoveiFoldersToggled(object obj, EventArgs args)
		{
			// If the button is being toggled for the first time since this
			// dialog has been opened, warn the user about what this action
			// will do.
			if (cbutton.Active && !bUserWarnedAboutRemovingiFolders)
			{
				bUserWarnedAboutRemovingiFolders = true;
				
				iFolderMsgDialog dialog = new iFolderMsgDialog(
					this,
					iFolderMsgDialog.DialogType.Warning,
					iFolderMsgDialog.ButtonSet.Ok,
					Util.GS("Warning"),
					Util.GS("Removing iFolders from Server"),
					Util.GS("Removing iFolders from the server will delete the files stored on the server.  Your files will remain intact on this computer.\n\nIf you've shared any iFolders with other users, they will no longer be able to synchronize them with the server.  Additionally, you will no longer be able to access these iFolders from any other computer."));
//				int rc = dialog.Run();
				dialog.Run();
				dialog.Hide();
				dialog.Destroy();
			}
		}
	}
}
