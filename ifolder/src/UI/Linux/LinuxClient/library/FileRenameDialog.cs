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
    /// class File Rename Dialog
    /// </summary>
	public class FileRenameDialog : Dialog
	{
		private Entry	nameEntry;
		private string	fileName;

        /// <summary>
        /// Gets / Sets the File Name
        /// </summary>
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">GTK Parent Dialog</param>
		public FileRenameDialog(Gtk.Window parent) : base()
		{
			this.TransientFor = parent;
			SetupDialog();
		}

        /// <summary>
        /// Set up Dialog - Customize
        /// </summary>
		private void SetupDialog()
		{
			this.Title = Util.GS("Rename file");
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder16.png"));
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

        /// <summary>
        /// Event Handler on Name Changed event
        /// </summary>
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
