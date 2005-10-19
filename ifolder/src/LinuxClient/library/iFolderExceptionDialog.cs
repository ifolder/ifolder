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
	public class iFolderExceptionDialog : Dialog
	{
		private Label details;
		private System.Exception ex;
		private Button dButton;

		public iFolderExceptionDialog(	Gtk.Window parent,  
										System.Exception exception)
			: base()
		{
			this.Title = Util.GS("iFolder Error");
			this.HasSeparator = true;
//			this.BorderWidth = 10;
			this.Resizable = false;
			this.Modal = true;
			this.ex = exception;
	
			if(parent != null)
				this.TransientFor = parent;
	
			HBox h = new HBox();
			h.BorderWidth = 10;
			h.Spacing = 10;
	
			Image i = new Image();
			i.SetFromStock(Gtk.Stock.DialogError, IconSize.Dialog);
			i.SetAlignment(0.5F, 0);
			h.PackStart(i, false, false, 0);
	
			VBox v = new VBox();
			v.BorderWidth = 10;
			v.Spacing = 10;
			Label l = new Label("<span weight=\"bold\" size=\"larger\">" +
				exception.Message + "</span>");
			l.LineWrap = true;
			l.UseMarkup = true;
			l.UseUnderline = false;
			l.Selectable = true;
			l.Xalign = 0; l.Yalign = 0;
			v.PackStart(l);
	
			dButton = new Button(Util.GS("Show Details"));
			dButton.Clicked += new EventHandler(ButtonPressed);
			HBox bhbox = new HBox();
			bhbox.PackStart(dButton, false, false, 0);
			v.PackEnd(bhbox, false, false, 0);
	
			details = new Label(Util.GS("Click \"Show Details\" below to get the full message returned with this error"));
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
			if(dButton.Label == Util.GS("Show Details"))
			{
				details.Text = ex.ToString();
				dButton.Label = Util.GS("Hide Details");
			}
			else
			{
				details.Text = Util.GS("Click \"Show Details\" below to get the full message returned with this error");
				dButton.Label = Util.GS("Show Details");
			}
		}
	}
}
