/***********************************************************************
 *  $RCSfile: MigrateLocation.cs,v $
 * 
 *  Copyright (C) 2006 Novell, Inc.
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
 *		Ramesh Sunder <sramesh@novell.com>
 * 
 ***********************************************************************/

using Gtk;
using System;

namespace Novell.iFolder
{
	public class MigrateLocation : FileChooserDialog
	{
		private DomainInformation[]	domains;
		private ComboBox			domainComboBox;
	//	private ComboBox			security_lvl_ComboBox;
		private CheckButton			Encryption;
		private CheckButton 			SSL;
		private string				initialPath;
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
					return domains[activeIndex].ID;
				else
					return "0";
			}
		}

		///
		/// filteredDomainID: If the main iFolders window is currently
		/// filtering the list of domains, this parameter is used to allow this
		/// dialog to respect the currently selected domain.
		public MigrateLocation(Gtk.Window parentWindow, string initialPath, iFolderWebService ifws)
				: base("", Util.GS("Select a folder..."), parentWindow, FileChooserAction.SelectFolder, Stock.Cancel, ResponseType.Cancel,
                Stock.Ok, ResponseType.Ok)
		{
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder16.png"));

			this.initialPath = initialPath;
			
			this.ifws = ifws;
			
			keyReleasedTimeoutID = 0;

			if (this.initialPath != null && this.initialPath.Length > 0)
				this.SetCurrentFolder(this.initialPath);

			// More options expander
			this.SetResponseSensitive(ResponseType.Ok, false);
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
//			if (descriptionTextView.HasFocus)
//				return true;  // Don't do anything here
			
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
