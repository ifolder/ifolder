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
	public class RemoveAccountDialog : Dialog
	{
		private CheckButton cbutton;

		public bool RemoveFromAll
		{
			get
			{
				return cbutton.Active;
			}
		}

		public RemoveAccountDialog() : base()
		{
			HBox h = new HBox();
			h.BorderWidth = 10;
			h.Spacing = 10;

			Image i = new Image();
			i.SetFromStock(Gtk.Stock.DialogQuestion, IconSize.Dialog);

			i.SetAlignment(0.5F, 0);
			h.PackStart(i, false, false, 0);

			VBox v = new VBox();
			Label l = new Label("<span weight=\"bold\" size=\"larger\">" +
					"Remove iFolder Account?</span>");
			l.LineWrap = true;
			l.UseMarkup = true;
			l.Selectable = true;
			l.Xalign = 0; l.Yalign = 0;
			v.PackStart(l);

			l = new Label("Removing this account will revert all iFolder in this account to normal folders on you hard drive.  The folders will no longer synchronize with the server.");
			l.LineWrap = true;
			l.Selectable = true;
			l.Xalign = 0; l.Yalign = 0;
			v.PackEnd(l);

			h.PackEnd(v);

//			h.ShowAll();
			this.VBox.PackStart(h, true, true, 0);

			cbutton = new CheckButton(Util.GS("Remove from all workstations"));
			this.VBox.PackStart(cbutton, false, false, 10);
			this.VBox.ShowAll();

			this.AddButton(Stock.Cancel, ResponseType.Cancel);
			this.AddButton(Stock.Ok, ResponseType.Ok);

			this.DefaultResponse = ResponseType.Cancel;

		}
	}
}
