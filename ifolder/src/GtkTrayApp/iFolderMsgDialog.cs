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
 *			Code based on examples from the book "Mono A Develper's Notebook"
 				by Edd Dumbill and Niel M. Bornstein
 * 
 ***********************************************************************/


using Gtk;

public class iFolderMsgDialog : Dialog
{
	public iFolderMsgDialog(string stockIcon, string title, string statement, string details)
		: base()
	{
		this.Title = title;
		this.HasSeparator = false;
		this.BorderWidth = 6;
		this.Resizable = false;

		HBox h = new HBox();
		h.BorderWidth = 6;
		h.Spacing = 12;

		Image i = new Image();
		i.SetFromStock(stockIcon, IconSize.Dialog);
		i.SetAlignment(0.5F, 0);
		h.PackStart(i, false, false, 0);

		VBox v = new VBox();
		Label l = new Label("<span weight=\"bold\" size=\"larger\">" +
							statement + "</span>");
		l.LineWrap = true;
		l.UseMarkup = true;
		l.Selectable = true;
		l.Xalign = 0; l.Yalign = 0;
		v.PackStart(l);

		l = new Label(details);
		l.LineWrap = true;
		l.Selectable = true;
		l.Xalign = 0; l.Yalign = 0;
		v.PackEnd(l);

		h.PackEnd(v);
		h.ShowAll();
		this.VBox.Add(h);

		this.AddButton(Stock.Ok, ResponseType.Ok);
	}
}
