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
	public class iFolderAcceptDialog : Dialog
	{
		private Entry		pathEntry;

		public string Path
		{
			get
			{
				return pathEntry.Text;
			}
		}

		public iFolderAcceptDialog(iFolder ifolder) : base()
		{
			this.Title = "Setup Shared iFolder";
			this.SetDefaultSize (500, 200);

			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder.png"));

			this.BorderWidth = 10;

			this.VBox.Spacing = 10;
			this.VBox.BorderWidth = 10;
			this.VBox.Homogeneous = false;
			Label l = new Label("<span weight=\"bold\" size=\"larger\">" +
						"Setup Shared iFolder: " + ifolder.Name + "</span>");

			l.LineWrap = false;
			l.UseMarkup = true;
			l.Selectable = false;
			l.Xalign = 0; l.Yalign = 0;
			this.VBox.PackStart(l, false, false, 0);

			l = new Label(string.Format("Select the location on your hard drive for this iFolder.  A new folder named \"{0}\" will be created and the iFolder will begin to sync files.", ifolder.Name));

			l.LineWrap = true;
			l.Xalign = 0; l.Yalign = 0;
			this.VBox.PackStart(l, true, true, 0);

			HBox pathBox = new HBox();
			pathBox.Spacing = 10;

			Label pathLabel = new Label("Path:");
			pathBox.PackStart(pathLabel, false, false, 0);

			pathEntry = new Entry();
			pathEntry.Changed += new EventHandler(OnPathChanged);
			pathBox.PackStart(pathEntry, true, true, 0);

			Button pathButton = new Button(Stock.Open);
			pathButton.Clicked += new EventHandler(OnChoosePath);
			pathBox.PackEnd(pathButton, false, false, 0);

			this.VBox.PackEnd(pathBox, false, false, 0);

			this.VBox.ShowAll();

			this.AddButton(Stock.Cancel, ResponseType.Cancel);
			this.AddButton(Stock.Ok, ResponseType.Ok);
			this.SetResponseSensitive(ResponseType.Ok, false);
		}


		private void OnChoosePath(object o, EventArgs args)
		{
			// create a file selection dialog and turn off all of the
			// file operations and controlls
			FileSelection fs = new FileSelection ("Choose a folder...");
			fs.FileList.Parent.Hide();
			fs.SelectionEntry.Hide();
			fs.FileopDelFile.Hide();
			fs.FileopRenFile.Hide();
			fs.TransientFor = this;

			int rc = fs.Run ();
			fs.Hide();
			if(rc == -5)
			{
				pathEntry.Text = fs.Filename;
			}
		}



		private void OnPathChanged(object obj, EventArgs args)
		{
			if(pathEntry.Text.Length > 0)
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
