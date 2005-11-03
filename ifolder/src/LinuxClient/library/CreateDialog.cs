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
 *  Authors:
 *		Calvin Gaisford <cgaisford@novell.com>
 *		Boyd Timothy <btimothy@novell.com>
 * 
 ***********************************************************************/

using Gtk;
using System;

namespace Novell.iFolder
{
	public class CreateDialog : FileChooserDialog
	{
		private DomainInformation[]	domains;
		private ComboBox			domainComboBox;
		private string				initialPath;
		TextView					descriptionTextView;
		iFolderWebService			ifws;
		private uint				keyReleasedTimeoutID;

		public string iFolderPath
		{
			get
			{
				return this.Filename;
			}
			set
			{
				this.SetCurrentFolder(value);
			}
		}

		public string DomainID
		{
			get
			{
				int activeIndex = domainComboBox.Active;
				if (activeIndex >= 0)
					return domains[0].ID;
				else
					return "0";
			}
		}
		
		public string Description
		{
			get
			{
				return descriptionTextView.Buffer.Text;
			}
			set
			{
				descriptionTextView.Buffer.Text = value;
			}
		}

		///
		/// filteredDomainID: If the main iFolders window is currently
		/// filtering the list of domains, this parameter is used to allow this
		/// dialog to respect the currently selected domain.
		public CreateDialog(Gtk.Window parentWindow, DomainInformation[] domainArray, string filteredDomainID, string initialPath, iFolderWebService ifws)
				: base("", Util.GS("New iFolder..."), parentWindow, FileChooserAction.CreateFolder, Stock.Cancel, ResponseType.Cancel,
                Stock.Ok, ResponseType.Ok)
		{
			domains = domainArray;

			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder24.png"));

			this.initialPath = initialPath;
			
			this.ifws = ifws;
			
			keyReleasedTimeoutID = 0;

			if (this.initialPath != null && this.initialPath.Length > 0)
				this.SetCurrentFolder(this.initialPath);

			// More options expander
			this.ExtraWidget = CreateMoreOptionsExpander(filteredDomainID);

			this.SetResponseSensitive(ResponseType.Ok, false);
		}
		
		private Widget CreateMoreOptionsExpander(string filteredDomainID)
		{
			Expander moreOptionsExpander = new Expander(Util.GS("More options"));

			Table optionsTable = new Table(2, 3, false);
			moreOptionsExpander.Add(optionsTable);
			
			optionsTable.ColumnSpacing = 10;
			optionsTable.RowSpacing = 10;
			optionsTable.SetColSpacing(0, 30);
			
			Label l = new Label(Util.GS("iFolder Server:"));
			l.Xalign = 0;
			optionsTable.Attach(l, 1,2,0,1,
								AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			// Set up Domains
			domainComboBox = ComboBox.NewText();
			optionsTable.Attach(domainComboBox, 2,3,0,1,
								AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
			
			int defaultDomain = 0;
			for (int x = 0; x < domains.Length; x++)
			{
				domainComboBox.AppendText(domains[x].Name);
				if (filteredDomainID != null)
				{
					if (filteredDomainID == domains[x].ID)
						defaultDomain = x;
				}
				else
					defaultDomain = x;
			}
			
			domainComboBox.Active = defaultDomain;

			l = new Label(Util.GS("Description:"));
			l.Xalign = 0;
			l.Yalign = 0;
			optionsTable.Attach(l, 1,2,1,2,
								AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			
			descriptionTextView = new TextView();
			descriptionTextView.LeftMargin = 4;
			descriptionTextView.RightMargin = 4;
			descriptionTextView.Editable = true;
			descriptionTextView.CursorVisible = true;
			descriptionTextView.AcceptsTab = false;
			descriptionTextView.WrapMode = WrapMode.WordChar;
			
			ScrolledWindow sw = new ScrolledWindow();
			sw.ShadowType = ShadowType.EtchedIn;
			sw.Add(descriptionTextView);
			optionsTable.Attach(sw, 2,3,1,2,
								AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
			
			optionsTable.ShowAll();
			
			return moreOptionsExpander;
		}

		protected override void OnSelectionChanged()
		{
			string currentPath = this.Filename;

			try
			{
				if (ifws.CanBeiFolder(currentPath))
					this.SetResponseSensitive(ResponseType.Ok, true);
				else
					this.SetResponseSensitive(ResponseType.Ok, false);
			}
			catch (Exception e)
			{
				this.SetResponseSensitive(ResponseType.Ok, false);
			}
		}
		
		protected override bool OnKeyReleaseEvent(Gdk.EventKey evnt)
		{
			if (descriptionTextView.HasFocus)
				return true;  // Don't do anything here
			
			if (keyReleasedTimeoutID != 0)
			{
				GLib.Source.Remove(keyReleasedTimeoutID);
				keyReleasedTimeoutID = 0;
			}

			// If the user is typing something in the name really quickly
			// delay the ifws.CanBeiFolder() call so that the dialog is
			// more responsive.
			keyReleasedTimeoutID =
				GLib.Timeout.Add(100,
								 new GLib.TimeoutHandler(CheckEnableOkButton));

			return true;
		}
		
		private bool CheckEnableOkButton()
		{
			try
			{
				string currentPath = this.Filename;
				if (ifws.CanBeiFolder(currentPath))
				{
					this.SetResponseSensitive(ResponseType.Ok, true);
					return false;
				}
			}
			catch{}
			this.SetResponseSensitive(ResponseType.Ok, false);
			return false;
		}
	}
}
