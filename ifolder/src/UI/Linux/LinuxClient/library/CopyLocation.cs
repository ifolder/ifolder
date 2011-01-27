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
    /// class Copy Location
    /// </summary>
	public class CopyLocation : FileChooserDialog
	{
		//never assinged	
		//private DomainInformation[]	domains;
		//private ComboBox			domainComboBox;
	//	private ComboBox			security_lvl_ComboBox;

		//never used
//		private CheckButton			Encryption;
//		private CheckButton 			SSL;
//		private string				initialPath;
//		iFolderWebService			ifws;
//		private Label 				Mesg;


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

	//variable inside function never assinged, means this function never get called
        /// <summary>
        /// Gets / Sets Domain ID
        /// </summary>
	/*
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
        */
		///
		/// filteredDomainID: If the main iFolders window is currently
		/// filtering the list of domains, this parameter is used to allow this
		/// dialog to respect the currently selected domain.             
		public CopyLocation(Gtk.Window parentWindow, string initialPath)
				: base("Choose folder", Util.GS("Select the folder to Download..."), parentWindow, FileChooserAction.SelectFolder, Stock.Cancel, ResponseType.Cancel,
                Stock.Ok, ResponseType.Ok)
		{
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder16.png"));

//			if (this.initialPath != null && this.initialPath.Length > 0)
				this.SetCurrentFolder(initialPath);
			this.SetFilename(initialPath);
			// More options expander
			this.SetResponseSensitive(ResponseType.Ok, false);
		}

		protected override void OnSelectionChanged()
		{

			this.SetResponseSensitive(ResponseType.Ok, true);
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

			return true;
		}

        /// <summary>
        /// Check Enable OK Button
        /// </summary>
        /// <returns></returns>
		/*private bool CheckEnableOkButton()
		{
			this.SetResponseSensitive(ResponseType.Ok, false);
			return false;
		}*/
	}
}
