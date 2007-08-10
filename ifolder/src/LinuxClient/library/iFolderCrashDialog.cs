/***********************************************************************
 |  $RCSfile: iFolderCrashDialog.cs,v $
 |
 | Copyright (c) 2007 Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com 
 |
 |  Author: Calvin Gaisford <cgaisford@novell.com>
 | 
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


			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder-error16.png"));
			
			Image crashImage = new Image(Util.ImagesPath("ifolder-error48.png"));

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
