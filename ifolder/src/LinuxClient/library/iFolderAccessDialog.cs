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
	public class iFolderAccessDialog : Dialog
	{
		private Gtk.RadioButton		FCButton;
		private Gtk.RadioButton		RWButton;
		private Gtk.RadioButton		ROButton;

		private Gtk.CheckButton		OwnerButton;

		public string Rights
		{
			get
			{
				if(FCButton.Active)
					return "Admin";
				else if(ROButton.Active)
					return "ReadOnly";
				else
					return "ReadWrite";
			}
		}

		public bool EnableOwner
		{
			set
			{
				OwnerButton.Sensitive = value;
			}
		}

		public bool IsOwner
		{
			get
			{
				return OwnerButton.Active;
			}
		}


		public iFolderAccessDialog(Gtk.Window parent, string userName, 
				string rights, bool enableOwner) : base()
 		{
			if(parent != null)
				this.TransientFor = parent;

			if(userName == null)
				this.Title = Util.GS("Access for Multiple Users");
			else
				this.Title = string.Format(Util.GS("Access for {0}"), userName);
			this.HasSeparator = false;
			this.Resizable = false;
			this.Modal = true;
			this.DefaultResponse = ResponseType.Ok;

			this.VBox.Spacing = 10;

			//------------------------------
			// Access
			//------------------------------
			// create a section box
			VBox accSectionBox = new VBox();
			this.VBox.PackStart(accSectionBox, false, true, 0);
			accSectionBox.BorderWidth = 10;
			Label accSectionLabel = new Label("<span weight=\"bold\">" +
												Util.GS("Access") +
												"</span>");
			accSectionLabel.UseMarkup = true;
			accSectionLabel.Xalign = 0;
			accSectionBox.PackStart(accSectionLabel, false, true, 0);

			// create a hbox to provide spacing
			HBox accSpacerBox = new HBox();
			accSectionBox.PackStart(accSpacerBox, false, true, 0);
			Label accSpaceLabel = new Label("    "); // four spaces
			accSpacerBox.PackStart(accSpaceLabel, false, true, 0);


			// create a vbox to actually place the widgets in for section
			VBox accWidgetBox = new VBox();
			accSpacerBox.PackStart(accWidgetBox, false, true, 0);


			FCButton = new RadioButton(Util.GS("Full Control"));
			accWidgetBox.PackStart(FCButton, false, true, 0);

			RWButton = new RadioButton(FCButton, Util.GS("Read/Write"));
			accWidgetBox.PackStart(RWButton, false, true, 0);

			ROButton = new RadioButton(FCButton, Util.GS("Read Only"));
			accWidgetBox.PackStart(ROButton, false, true, 0);

			VBox ownerSectionBox = new VBox();
			this.VBox.PackStart(ownerSectionBox, false, true, 0);
			ownerSectionBox.BorderWidth = 10;

			OwnerButton = new CheckButton(Util.GS("Make this user the owner of the iFolder."));
			ownerSectionBox.PackStart(OwnerButton, false, true, 0);
			if(!enableOwner)
				OwnerButton.Sensitive = false;

			this.VBox.ShowAll();

			this.AddButton(Stock.Cancel, ResponseType.Cancel);
			this.AddButton(Stock.Ok, ResponseType.Ok);


			if(rights == "Admin")
				FCButton.Active = true;
			else if(rights == "ReadOnly")
				ROButton.Active = true;
			else
				RWButton.Active = true;
		}

	}
}
