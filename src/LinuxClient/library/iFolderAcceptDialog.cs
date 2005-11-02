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
		private iFolderWeb	ifolder;
		private string		initialPath;
		private Label		previewPath;
		FileChooserWidget	fileChooserWidget;
		

		public new string Path
		{
			get
			{
				return fileChooserWidget.CurrentFolder;
				// FIXME: Replace the above line with fileChooserWidget.Filename once iFolder allows users to name their local iFolder
			}
		}

		public iFolderAcceptDialog(iFolderWeb ifolder, string initialPath) : base()
		{
			this.Title = "";
			this.SetDefaultSize (600, 500);

			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder24.png"));

			this.ifolder = ifolder;
			this.initialPath = initialPath;

//			this.BorderWidth = 10;
			VBox dialogBox = new VBox();
			dialogBox.Spacing = 10;
			dialogBox.BorderWidth = 10;
			dialogBox.Homogeneous = false;
			this.VBox.PackStart(dialogBox, true, true, 0);


			Label l = new Label("<span weight=\"bold\" size=\"larger\">" +
						Util.GS("Set Up iFolder: ") + ifolder.Name + "</span>");

			l.LineWrap = false;
			l.UseMarkup = true;
			l.UseUnderline = false;
			l.Selectable = false;
			l.Xalign = 0; l.Yalign = 0;
			dialogBox.PackStart(l, false, false, 0);


			VBox detailBox = new VBox();
			dialogBox.PackStart(detailBox, false, false, 0);

			l = new Label(Util.GS("Details:"));
			l.Xalign = 0;
			detailBox.PackStart(l, false, false, 0);

			TextView tv = new TextView();
			tv.LeftMargin = 10;
			tv.RightMargin = 10;
			tv.Editable = false;
			tv.CursorVisible = false;
			TextBuffer buffer = tv.Buffer;
			buffer.Text = string.Format(Util.GS("Name: {0}\nShared by: {1}\nAccess: {2}"), ifolder.Name, ifolder.Owner, GetDisplayRights(ifolder.CurrentUserRights));

			ScrolledWindow sw = new ScrolledWindow();
			sw.ShadowType = Gtk.ShadowType.EtchedIn;
			sw.Add(tv);
			detailBox.PackStart(sw, false, false, 0);


			l = new Label(Util.GS("The iFolder will be set up here:"));

			l.LineWrap = false;
			l.Xalign = 0; l.Yalign = 1;
			dialogBox.PackStart(l, false, false, 0);

			previewPath = new Label();
			previewPath.Xalign = 0; previewPath.Yalign = 0;
			previewPath.Wrap = true;
			dialogBox.PackStart(previewPath, false, false, 0);

			fileChooserWidget =
				new FileChooserWidget(FileChooserAction.SelectFolder, "");
// FIXME: Remove the above line and replace it with the line below once iFolder allows users to name their local iFolders
//				new FileChooserWidget(FileChooserAction.CreateFolder, "");
			fileChooserWidget.SelectMultiple = false;
			fileChooserWidget.LocalOnly = true;
			fileChooserWidget.CurrentName = ifolder.Name;
			
			if (this.initialPath != null && this.initialPath.Length > 0)
				fileChooserWidget.SetCurrentFolder(this.initialPath);
			
			fileChooserWidget.SelectionChanged +=
				new EventHandler(OnFileChooserSelectionChanged);
			
			dialogBox.PackStart(fileChooserWidget, true, true, 0);

			this.VBox.ShowAll();

			this.AddButton(Stock.Cancel, ResponseType.Cancel);
			this.AddButton(Stock.Ok, ResponseType.Ok);
			if (this.initialPath != null && this.initialPath.Length > 0)
				this.SetResponseSensitive(ResponseType.Ok, true);
			else
				this.SetResponseSensitive(ResponseType.Ok, false);
		}
		
		
		private void OnFileChooserSelectionChanged(object sender, EventArgs args)
		{
			previewPath.Text = System.IO.Path.Combine(fileChooserWidget.Filename, ifolder.Name);
		}

		private string GetDisplayRights(string rights)
		{
			if(rights == "ReadWrite")
				return Util.GS("Read/Write");
			else if(rights == "Admin")
				return Util.GS("Full Control");
			else if(rights == "ReadOnly")
				return Util.GS("Read Only");
			else
				return Util.GS("Unknown");
		}

	}
}
