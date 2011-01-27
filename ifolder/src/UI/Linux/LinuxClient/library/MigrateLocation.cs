/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
  *
  *                 $Author: Ramesh Sunder <sramesh@novell.com>
  *                 $Modified by: <Modifier>
  *                 $Mod Date: <Date Modified>
  *                 $Revision: 0.0
  *-----------------------------------------------------------------------------
  * This module is used to:
  *        <Description of the functionality of the file >
  *
  *
  *******************************************************************************/

using Gtk;
using System;

namespace Novell.iFolder
{
    /// <summary>
    /// class MigrateLoaction
    /// </summary>
	public class MigrateLocation : FileChooserDialog
	{
		//never initilized
		//private DomainInformation[]	domains;
		//private ComboBox			domainComboBox;
	//	private ComboBox			security_lvl_ComboBox;
//		private CheckButton			Encryption;
//		private CheckButton 			SSL;
		private string				initialPath;
		private string copyDir;
		iFolderWebService			ifws;
		private uint				keyReleasedTimeoutID;

        /// <summary>
        /// Gets / Sets the iFolder Path
        /// </summary>
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


	// never called, as member inside function is never initilized
        /// <summary>
        /// Gets the Domain ID
        /// </summary>
	/*	public string DomainID
		{
			get
			{
				int activeIndex = domainComboBox.Active;
				if (activeIndex >= 0)
					return domains[activeIndex].ID;
				else
					return "0";
			}
		}*/

		///
		/// filteredDomainID: If the main iFolders window is currently
		/// filtering the list of domains, this parameter is used to allow this
		/// dialog to respect the currently selected domain.
		public MigrateLocation(Gtk.Window parentWindow, string initialPath, iFolderWebService ifws)
				: base("", Util.GS("Select a folder..."), parentWindow, FileChooserAction.SelectFolder, Stock.Cancel, ResponseType.Cancel,
                Stock.Ok, ResponseType.Ok)
		{
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder16.png"));

			this.initialPath = System.IO.Directory.GetCurrentDirectory(); 
		
			this.copyDir = initialPath;	
			this.ifws = ifws;
			
			keyReleasedTimeoutID = 0;

			if (this.initialPath != null && this.initialPath.Length > 0)
				this.SetCurrentFolder(this.initialPath);

			// More options expander
			this.SetResponseSensitive(ResponseType.Ok, false);
		}

        /// <summary>
        /// On Selection Changed
        /// </summary>
		protected override void OnSelectionChanged()
		{
			string currentPath = this.Filename;
			if( copyDir != null)
				currentPath += "/"+copyDir;

			try
			{
				if (ifws.CanBeiFolder(currentPath))
					this.SetResponseSensitive(ResponseType.Ok, true);
				else
					this.SetResponseSensitive(ResponseType.Ok, false);
			}
			catch (Exception)
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
		
        /// <summary>
        /// Check Enable OK Button
        /// </summary>
        /// <returns>true on success</returns>
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
