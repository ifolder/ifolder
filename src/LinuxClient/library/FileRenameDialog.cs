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
	public class FileRenameDialog : Dialog
	{
		private Entry	nameEntry;
		private string	fileName;

		public string FileName
		{
			get
			{
				return nameEntry.Text;
			}
			set
			{
				fileName = value;
				nameEntry.Text = value;
			}
		}

		public FileRenameDialog(Gtk.Window parent) : base()
		{
			this.TransientFor = parent;
			SetupDialog();
		}


		private void SetupDialog()
		{
			this.Title = Util.GS("Rename file");
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder24.png"));
			this.HasSeparator = false;
//			this.BorderWidth = 10;
			this.SetDefaultSize (450, 100);
			this.Resizable = false;
			this.Modal = true;
			this.DefaultResponse = ResponseType.Ok;

			HBox hbox = new HBox();

			Label nameLabel = new Label(Util.GS("File name:"));
			hbox.PackStart(nameLabel, false, false, 0);

			nameEntry = new Entry();
			nameEntry.Changed += new EventHandler(OnNameChanged);
			nameEntry.ActivatesDefault = true;
			hbox.PackStart(nameEntry, true, true, 0);
	
			this.VBox.PackStart(hbox, true, true, 0);
			hbox.ShowAll();

			this.AddButton(Stock.Cancel, ResponseType.Cancel);
			this.AddButton(Stock.Ok, ResponseType.Ok);
			this.SetResponseSensitive(ResponseType.Ok, false);
			this.DefaultResponse = ResponseType.Ok;
		}


		private void OnNameChanged(object obj, EventArgs args)
		{
			bool enableOK = false;

			if(	(nameEntry.Text.Length > 0) &&
				(nameEntry.Text != fileName) )
				enableOK = true;
			else
				enableOK = false;

			this.SetResponseSensitive(ResponseType.Ok, enableOK);
		}
	}
}
