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

namespace Novell.iFolder
{

	/// <summary>
	/// This is the properties dialog for an iFolder.
	/// </summary>
	public class iFolderPropertiesDialog : Dialog
	{
		private iFolderWebService	ifws;
		private iFolder				ifolder;
		private Gtk.Notebook		propNoteBook;

		/// <summary>
		/// Default constructor for iFolderPropertiesDialog
		/// </summary>
		public iFolderPropertiesDialog(	Gtk.Window parent,
										iFolder ifolder, 
										iFolderWebService iFolderWS)
			: base()
		{
			this.Title = "iFolder \"" + ifolder.Name +"\" Properties";
			if(iFolderWS == null)
				throw new ApplicationException("iFolderWebServices was null");
			this.ifws = iFolderWS;
			this.ifolder = ifolder;
			this.HasSeparator = false;
			this.BorderWidth = 6;
//			this.Resizable = false;
			this.Modal = true;
			if(parent != null)
				this.TransientFor = parent;

			InitializeWidgets();
		}




		/// <summary>
		/// Setup the UI inside the Window
		/// </summary>
		private void InitializeWidgets()
		{
			this.SetDefaultSize (200, 400);
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder.png"));

			propNoteBook = new Gtk.Notebook();

			iFolderPropSettingsPage settingsPage = 
				new iFolderPropSettingsPage(ifolder, ifws);

			propNoteBook.AppendPage(settingsPage, new Label("Settings"));

			iFolderPropSharingPage sharingPage = 
				new iFolderPropSharingPage(this, ifolder, ifws);

			propNoteBook.AppendPage(sharingPage, new Label("Sharing"));

			this.VBox.PackStart(propNoteBook);
			propNoteBook.ShowAll();

			this.AddButton(Stock.Close, ResponseType.Ok);
			this.AddButton(Stock.Help, ResponseType.Help);
		}
	}
}
