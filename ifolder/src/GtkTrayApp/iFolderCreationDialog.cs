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

namespace Novell.iFolder
{
	public class iFolderCreationDialog : Dialog
	{
//		private iFolder ifolder;
		private CheckButton cbutton;

		public bool HideDialog
		{
			get
			{
				return cbutton.Active;
			}
		}

		public iFolderCreationDialog(iFolder ifolder) : base()
		{
//			this.ifolder = ifolder;
			this.Title = "iFolder Introduction";
			this.HasSeparator = false;
			this.BorderWidth = 10;
			this.Resizable = false;

			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder.png"));
			Image folderImage = new Image(this.Icon);

			VBox vbox = new VBox();
			vbox.BorderWidth = 10;
			vbox.Spacing = 20;

			Label l = new Label("<span weight=\"bold\" size=\"larger\">" +
						"Congratulations! A new iFolder was created</span>");

			l.LineWrap = false;
			l.UseMarkup = true;
			l.Selectable = false;
			l.Xalign = 0; l.Yalign = 0;
			vbox.PackStart(l, false, false, 0);

			HBox h = new HBox();
//			h.BorderWidth = 10;
			h.Spacing = 12;

			folderImage.SetAlignment(0.5F, 0);
			h.PackStart(folderImage, false, false, 0);

			VBox vbox2 = new VBox();
			vbox2.Spacing = 10;

			l = new Label("To share your iFolder and it's contents with others, right-click the iFolder and select \"Share with...\", or click here now.");

			l.LineWrap = true;
			vbox2.PackStart(l, true, true, 0);

			h.PackEnd(vbox2, true, true, 0);

			vbox.PackStart(h);

			cbutton = new CheckButton("Do not show this message again.");
			vbox.PackStart(cbutton, false, false, 0);
		
			vbox.ShowAll();
			this.VBox.Add(vbox);

			this.AddButton(Stock.Close, ResponseType.Ok);
		}
	}
}
