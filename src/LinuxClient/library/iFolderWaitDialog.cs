/***********************************************************************
 *  $RCSfile: iFolderWaitDialog.cs,v $
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
 *  Author: Boyd Timothy <btimothy@novell.com>
 *
 ***********************************************************************/


using System;
using Gtk;

namespace Novell.iFolder
{

public class iFolderWaitDialog : Dialog
{
	private ButtonSet buttonSet;

	public enum ButtonSet : int
	{
		Cancel,
		None
	}

	
	public iFolderWaitDialog(Window parent,
							 Widget waitingWidget,
							 ButtonSet buttonSet,
							 string title,
							 string statement, 
							 string secondaryStatement)
		: base()
	{
		Init(parent, waitingWidget, buttonSet, title, statement, secondaryStatement);
	}

	internal void Init(Gtk.Window parent,
					   Widget waitingWidget,
					   ButtonSet buttonSet,
					   string title,
					   string statement,
					   string secondaryStatement)
	{
		this.Title = title;
		this.HasSeparator = false;
//		this.BorderWidth = 10;
		this.Resizable = false;
		this.Modal = true;
		if(parent != null)
			this.TransientFor = parent;
		
		this.buttonSet = buttonSet;

		HBox h = new HBox();
		h.BorderWidth = 10;
		h.Spacing = 10;

		if (waitingWidget != null)
		{
			h.PackStart(waitingWidget, false, false, 0);
		}

		VBox v = new VBox();
		v.Spacing = 10;
		Label l = new Label();
		l.LineWrap = true;
		l.UseMarkup = true;
		l.Selectable = false;
		l.CanFocus = false;
		l.Xalign = 0; l.Yalign = 0;
		l.Markup = "<span weight=\"bold\" size=\"larger\">" + GLib.Markup.EscapeText(statement) + "</span>";
		v.PackStart(l);

		l = new Label(secondaryStatement);
		l.LineWrap = true;
		l.Selectable = false;
		l.CanFocus = false;
		l.Xalign = 0; l.Yalign = 0;
		v.PackStart(l, true, true, 8);
		
		h.PackEnd(v);
		h.ShowAll();

		this.VBox.Add(h);
		
		switch(buttonSet)
		{
			case ButtonSet.Cancel:
				this.AddButton(Stock.Cancel, ResponseType.Cancel);
				break;
			case ButtonSet.None:
			default:
				break;
		}
	}
	
	protected override bool OnDeleteEvent(Gdk.Event evnt)
	{
		if (buttonSet == ButtonSet.None)
			return true; // Do nothing and don't let this be seen elsewhere
		else
			return false;	// Allow this to send the cancel
	}
	
	protected override bool OnDestroyEvent(Gdk.Event evnt)
	{
		if (buttonSet == ButtonSet.None)
			return true; // Do nothing and don't let this be seen elsewhere
		else
			return false;	// Allow this to send the cancel
	}

//	private void ShowDetailsButtonPressed(object o, EventArgs args)
//	{
//		if (showDetailsButton.Label == Util.GS("Show _Details"))
//		{
//			showDetailsButton.Label = Util.GS("Hide _Details");
//			showDetailsScrolledWindow.Visible = true;
//		}
//		else
//		{
//			showDetailsButton.Label = Util.GS("Show _Details");
//			showDetailsScrolledWindow.Visible = false;
//		}
//	}
}

}