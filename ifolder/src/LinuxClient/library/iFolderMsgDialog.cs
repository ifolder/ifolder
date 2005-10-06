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

namespace Novell.iFolder
{

public class iFolderMsgDialog : Dialog
{
	private Button showDetailsButton;
	private ScrolledWindow showDetailsScrolledWindow;

	public enum DialogType : int
	{
		Error = 1,
		Info,
		Question,
		Warning
	}

	public enum ButtonSet : int
	{
		Ok = 1,
		OkCancel,
		YesNo
	}

	
	public iFolderMsgDialog(	Gtk.Window parent,
								DialogType type,
								ButtonSet buttonSet,
								string title, 
								string statement, 
								string secondaryStatement)
		: base()
	{
		Init(parent, type, buttonSet, title, statement, secondaryStatement, null);
	}

	///
	/// This dialog adds on the "details" parameter.  This will add on
	/// a "Show Details" button that will show the extended details in a text
	/// area.
	///
	public iFolderMsgDialog(	Gtk.Window parent,
								DialogType type,
								ButtonSet buttonSet,
								string title, 
								string statement, 
								string secondaryStatement,
								string details)
		: base()
	{
		Init(parent, type, buttonSet, title, statement, secondaryStatement, details);
	}
	
	internal void Init(Gtk.Window parent,
					   DialogType type,
					   ButtonSet buttonSet,
					   string title,
					   string statement,
					   string secondaryStatement,
					   string details)
	{
		this.Title = title;
		this.HasSeparator = false;
//		this.BorderWidth = 10;
		this.Resizable = false;
		this.Modal = true;
		if(parent != null)
			this.TransientFor = parent;

		HBox h = new HBox();
		h.BorderWidth = 10;
		h.Spacing = 10;

		Image i = new Image();
		switch(type)
		{
			case DialogType.Error:
				i.SetFromStock(Gtk.Stock.DialogError, IconSize.Dialog);
				break;
			case DialogType.Question:
				i.SetFromStock(Gtk.Stock.DialogQuestion, IconSize.Dialog);
				break;
			case DialogType.Warning:
				i.SetFromStock(Gtk.Stock.DialogWarning, IconSize.Dialog);
				break;
			default:
			case DialogType.Info:
				i.SetFromStock(Gtk.Stock.DialogInfo, IconSize.Dialog);
				break;
		}
		i.SetAlignment(0.5F, 0);
		h.PackStart(i, false, false, 0);

		VBox v = new VBox();
		v.Spacing = 10;
		Label l = new Label();
		l.LineWrap = true;
		l.UseMarkup = true;
		l.Selectable = false;
		l.CanFocus = false;
		l.Xalign = 0; l.Yalign = 0;
		l.Markup = "<span weight=\"bold\" size=\"larger\">" + statement + "</span>";
		v.PackStart(l);

		l = new Label(secondaryStatement);
		l.LineWrap = true;
		l.Selectable = false;
		l.CanFocus = false;
		l.Xalign = 0; l.Yalign = 0;
		v.PackStart(l, true, true, 8);
		
		if (details != null)
		{
			showDetailsButton = new Button(Util.GS("Show _Details"));
			showDetailsButton.Clicked += new EventHandler(ShowDetailsButtonPressed);
			
			HBox detailsButtonBox = new HBox();
			detailsButtonBox.PackEnd(showDetailsButton, false, false, 0);

			v.PackStart(detailsButtonBox, false, false, 4);

			TextView textView = new TextView();
			textView.Editable = false;
			textView.WrapMode = WrapMode.Char;
			TextBuffer textBuffer = textView.Buffer;
			
			textBuffer.Text = details;
			
			showDetailsScrolledWindow = new ScrolledWindow();
			showDetailsScrolledWindow.AddWithViewport(textView);
			
			showDetailsScrolledWindow.Visible = false;
			
			v.PackEnd(showDetailsScrolledWindow, false, false, 4);
		}
		else
		{
			showDetailsButton = null;
			showDetailsScrolledWindow = null;
		}

		h.PackEnd(v);
		h.ShowAll();
		
		if (details != null)
			showDetailsScrolledWindow.Visible = false;

		this.VBox.Add(h);
		
		switch(buttonSet)
		{
			default:
			case ButtonSet.Ok:
				this.AddButton(Stock.Ok, ResponseType.Ok);
				break;
			case ButtonSet.OkCancel:
				this.AddButton(Stock.Cancel, ResponseType.Cancel);
				this.AddButton(Stock.Ok, ResponseType.Ok);
				break;
			case ButtonSet.YesNo:
				this.AddButton(Stock.No, ResponseType.No);
				this.AddButton(Stock.Yes, ResponseType.Yes);
				break;
		}
	}
	
	private void ShowDetailsButtonPressed(object o, EventArgs args)
	{
		if (showDetailsButton.Label == Util.GS("Show _Details"))
		{
			showDetailsButton.Label = Util.GS("Hide _Details");
			showDetailsScrolledWindow.Visible = true;
		}
		else
		{
			showDetailsButton.Label = Util.GS("Show _Details");
			showDetailsScrolledWindow.Visible = false;
		}
	}
}

}