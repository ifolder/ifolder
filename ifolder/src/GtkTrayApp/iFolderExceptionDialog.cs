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

using System;
using Gtk;

public class iFolderExceptionDialog : Dialog
{
	private Label details;
	private System.Exception ex;
	private Button dButton;

	public iFolderExceptionDialog(	Gtk.Window parent,  
									System.Exception exception)
		: base()
	{
		this.Title = "iFolder Error";
		this.HasSeparator = true;
		this.BorderWidth = 6;
		this.Resizable = false;
		this.Modal = true;
		this.ex = exception;

		if(parent != null)
			this.TransientFor = parent;

		HBox h = new HBox();
		h.BorderWidth = 6;
		h.Spacing = 12;

		Image i = new Image();
		i.SetFromStock(Gtk.Stock.DialogError, IconSize.Dialog);
		i.SetAlignment(0.5F, 0);
		h.PackStart(i, false, false, 0);

		VBox v = new VBox();
		v.BorderWidth = 6;
		v.Spacing = 12;
		Label l = new Label("<span weight=\"bold\" size=\"larger\">" +
			"An Error occurred in iFolder</span>");
		l.LineWrap = false;
		l.UseMarkup = true;
		l.Selectable = true;
		l.Xalign = 0; l.Yalign = 0;
		v.PackStart(l);

		dButton = new Button("Show Details");
		dButton.Clicked += new EventHandler(ButtonPressed);
		HBox bhbox = new HBox();
		bhbox.PackStart(dButton, false, false, 0);
		v.PackEnd(bhbox, false, false, 0);

		details = new Label(ex.Message);
		details.LineWrap = true;
		details.Selectable = true;
		details.Xalign = 0; details.Yalign = 0;
		v.PackEnd(details);

		h.PackEnd(v);
		h.ShowAll();
		this.VBox.Add(h);

		this.AddButton(Stock.Close, ResponseType.Ok);
	}

	private void ButtonPressed(object o, EventArgs args)
	{
		if(dButton.Label == "Show Details")
		{
			details.Text = ex.ToString();
			dButton.Label = "Hide Details";
		}
		else
		{
			details.Text = ex.Message;
			dButton.Label = "Show Details";
		}
	}
}
