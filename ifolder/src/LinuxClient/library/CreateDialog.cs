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
	public class CreateDialog : Dialog
	{
		private Entry				pathEntry;
		private DomainInformation[]	domains;
		private OptionMenu			domainOptions;


		public string iFolderPath
		{
			get
			{
				return pathEntry.Text;
			}
		}


		public string DomainID
		{
			get
			{
				return domains[domainOptions.History].ID;
			}
		}


		public CreateDialog(DomainInformation[] domainArray) : base()
		{
			domains = domainArray;

			this.Title = Util.GS("New iFolder");

			this.SetDefaultSize (500, 200);

			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder24.png"));

			VBox dialogBox = new VBox();
			dialogBox.Spacing = 10;
			dialogBox.BorderWidth = 10;
			dialogBox.Homogeneous = false;
			this.VBox.PackStart(dialogBox, true, true, 0);

			Label l = new Label("<span weight=\"bold\" size=\"larger\">" +
						Util.GS("Create a new iFolder</span>"));

			l.LineWrap = false;
			l.UseMarkup = true;
			l.Selectable = false;
			l.Xalign = 0; l.Yalign = 0;
			dialogBox.PackStart(l, false, false, 0);

			VBox serverBox = new VBox();
			dialogBox.PackStart(serverBox, false, true, 0);

			l = new Label(Util.GS("Server:"));
			l.Xalign = 0;
			serverBox.PackStart(l, false, false, 0);


			// Setup Domains
			domainOptions = new OptionMenu();

			int defaultDomain = 0;
			Menu m = new Menu();
			for(int x=0; x < domains.Length; x++)
			{
				m.Append(new MenuItem(domains[x].Name));
				if(domains[x].IsDefault)
					defaultDomain = x;
			}

			domainOptions.Menu = m;
			domainOptions.SetHistory((uint)defaultDomain);
			
			serverBox.PackStart(domainOptions, true, true, 0);

			VBox locBox = new VBox();
			dialogBox.PackEnd(locBox, false, true, 0);

			Label pathLabel = new Label(Util.GS("iFolder Path:"));
			pathLabel.Xalign = 0;
			locBox.PackStart(pathLabel, false, true, 0);

			HBox pathBox = new HBox();
			pathBox.Spacing = 10;
			locBox.PackStart(pathBox, false, true, 0);

			pathEntry = new Entry();
			pathEntry.Changed += new EventHandler(OnPathChanged);
			pathBox.PackStart(pathEntry, true, true, 0);

			Button pathButton = new Button(Stock.Open);
			pathButton.Clicked += new EventHandler(OnChoosePath);
			pathBox.PackEnd(pathButton, false, false, 0);


			this.VBox.ShowAll();

			this.AddButton(Stock.Cancel, ResponseType.Cancel);
			this.AddButton(Stock.Ok, ResponseType.Ok);
			this.SetResponseSensitive(ResponseType.Ok, false);
		}




		private void OnChoosePath(object o, EventArgs args)
		{
			// Switched out to use the compatible file selector
			CompatFileChooserDialog cfcd = new CompatFileChooserDialog(
					Util.GS("Choose a folder..."), this, 
					CompatFileChooserDialog.Action.SelectFolder);

			int rc = cfcd.Run();
			cfcd.Hide();

			if(rc == -5)
			{
				pathEntry.Text = cfcd.Selections[0];
			}
		}




		private void OnPathChanged(object obj, EventArgs args)
		{
			if(pathEntry.Text.Length > 0)
			{
				this.SetResponseSensitive(ResponseType.Ok, true);
			}
			else
			{
				this.SetResponseSensitive(ResponseType.Ok, false);
			}
		}



		private string GetDisplayRights(string rights)
		{
			if(rights == "ReadWrite")
				return Util.GS("Read Write");
			else if(rights == "Admin")
				return Util.GS("Full Control");
			else if(rights == "ReadOnly")
				return Util.GS("Read Only");
			else
				return Util.GS("Unknown");
		}

	}
}
