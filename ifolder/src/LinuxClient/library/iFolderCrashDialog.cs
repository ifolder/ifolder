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
	public class iFolderCrashDialog : Dialog
	{
		public iFolderCrashDialog(System.Exception e) : base()
		{
			this.SetDefaultSize (600, 400);
			this.Title = "";
			this.HasSeparator = false;
//			this.BorderWidth = 10;
			this.Resizable = true;


			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder-crash.png"));
			
			Image crashImage = 
				new Image(new Gdk.PixbufAnimation(Util.ImagesPath("ifolder-crash.gif")));

			VBox vbox = new VBox();
			vbox.BorderWidth = 10;
			vbox.Spacing = 10;

			Label l = new Label("<span weight=\"bold\" size=\"larger\">" +
				Util.GS("iFolder crashed because of an unhandled exception") + 
				"</span>");
			l.LineWrap = false;
			l.UseMarkup = true;
			l.Selectable = false;
			l.Xalign = 0; l.Yalign = 0;
			vbox.PackStart(l, false, false, 0);

			HBox h = new HBox();
			h.BorderWidth = 10;
			h.Spacing = 12;

			crashImage.SetAlignment(0.5F, 0);
			h.PackStart(crashImage, false, false, 0);

			TextView tv = new TextView();
			tv.WrapMode = Gtk.WrapMode.Word;
			tv.Editable = false;


			tv.Buffer.Text = e.ToString();
			ScrolledWindow sw = new ScrolledWindow();
			sw.ShadowType = Gtk.ShadowType.EtchedIn;
			sw.Add(tv);
			h.PackEnd(sw, true, true, 0);

			vbox.PackEnd(h);
			vbox.ShowAll();
			this.VBox.Add(vbox);

			this.AddButton(Stock.Close, ResponseType.Ok);
		}
	}
}
