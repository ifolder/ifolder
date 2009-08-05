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
  *                 $Author: Calvin Gaisford <cgaisford@novell.com>
  *                          Boyd Timothy <btimothy@novell.com>
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
using System.Collections;

namespace Novell.iFolder
{
        /// <summary>
        /// Enum Security State
        /// </summary>
		public enum SecurityState
		{
			encryption = 1,
			enforceEncryption = 2,
			SSL = 4,
			enforceSSL = 8
		}

    /// <summary>
    /// class Create Dialog 
    /// </summary>
	public class CreateDialog : FileChooserDialog
	{
		private DomainInformation[]	domains;
		private ComboBox			domainComboBox;
//		private CheckButton			Encryption;
//		private CheckButton 			SSL;
		private RadioButton 			Encryption;
		private RadioButton			Regular;
		private CheckButton                     SecureSync;
		private string				initialPath;
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

        /// <summary>
        /// Gets the Domain ID
        /// </summary>
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

        /// <summary>
        /// Gets the SSL
        /// </summary>
		public bool ssl 
		{
			get
			{
				return SecureSync.Active;
			}
		}

        /// <summary>
        /// Gets Encryption ALgorithm
        /// </summary>
		public string EncryptionAlgorithm
		{
			get
			{
				if(Encryption.Active == true)
					return "BlowFish";
				else 
					return null;			
			}			
		}

        /// <summary>
        /// Gets the description
        /// </summary>
		public string Description
		{
			get
			{
//				return descriptionTextView.Buffer.Text;
				return "";
			}
			
//			set
//			{
//				descriptionTextView.Buffer.Text = value;
//			}
		}

		///
		/// filteredDomainID: If the main iFolders window is currently
		/// filtering the list of domains, this parameter is used to allow this
		/// dialog to respect the currently selected domain.
		public CreateDialog(Gtk.Window parentWindow, DomainInformation[] domainArray, string filteredDomainID, string initialPath, iFolderWebService ifws)
				: base("", Util.GS("Select a folder..."), parentWindow, FileChooserAction.SelectFolder, Stock.Cancel, ResponseType.Cancel,
                Stock.Ok, ResponseType.Ok)
		{
			domains = domainArray;

			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder16.png"));

			this.initialPath = initialPath;
			
			this.ifws = ifws;
			
			keyReleasedTimeoutID = 0;

			if (this.initialPath != null && this.initialPath.Length > 0)
				this.SetCurrentFolder(this.initialPath);

			// More options expander
			this.ExtraWidget = CreateMoreOptionsExpander(filteredDomainID);
			domainComboBox.Changed += new EventHandler(OnDomainChangedEvent);

			this.SetResponseSensitive(ResponseType.Ok, false);
		}

        /// <summary>
        /// Event Handler for Domain Changed event
        /// </summary>
        private void OnDomainChangedEvent(System.Object o, EventArgs args)
		{
			int SecurityPolicy = ifws.GetSecurityPolicy(this.DomainID);
			ChangeStatus(SecurityPolicy);
		}
		
        /// <summary>
        /// Change status
        /// </summary>
        /// <param name="SecurityPolicy">Security Policy</param>
		private void ChangeStatus(int SecurityPolicy)
		{
			Encryption.Active = Regular.Active = false;
			Encryption.Sensitive = Regular.Sensitive = false;
			
			if(SecurityPolicy !=0)
			{
				if( (SecurityPolicy & (int)SecurityState.encryption) == (int) SecurityState.encryption)
				{
					if( (SecurityPolicy & (int)SecurityState.enforceEncryption) == (int) SecurityState.enforceEncryption)
						Encryption.Active = true;
					else
					{
						Encryption.Sensitive = true;
						Regular.Sensitive = true;
					}
				}
				else
				{
					Regular.Active = true;
				}
				/*
				if( (SecurityPolicy & (int)SecurityState.SSL) == (int) SecurityState.SSL)
				{
					if( (SecurityPolicy & (int)SecurityState.enforceSSL) == (int) SecurityState.enforceSSL)
						Regular.Active = true;
					else
						Regular.Sensitive = true;
				}
				*/
			}
			else
			{
				Regular.Active = true;
			}
			

			
			if( (this.domains[domainComboBox.Active].HostUrl).StartsWith( System.Uri.UriSchemeHttps ) )
			{
				SecureSync.Active = true;
				SecureSync.Sensitive = false;
			}
			else
			{
				SecureSync.Active = false;
                                SecureSync.Sensitive = true;
			}
			
		}

		/// <summary>
		/// Create More options
		/// </summary>
		/// <param name="filteredDomainID">Domain ID</param>
		/// <returns>Widget</returns>
		private Widget CreateMoreOptionsExpander(string filteredDomainID)
		{
			Expander moreOptionsExpander = new Expander(Util.GS("More options"));

			Table optionsTable = new Table(4, 3, false);
			moreOptionsExpander.Add(optionsTable);
			
			optionsTable.ColumnSpacing = 10;
			optionsTable.RowSpacing = 10;
			optionsTable.SetColSpacing(0, 30);
			
			Label l = new Label(Util.GS("iFolder account")+":");
			l.Xalign = 0;
			///Syntax:  Table.Attach(widget, column_start, column_end, row_start, row_end, AttachOptions,.....
			optionsTable.Attach(l, 1,2,0,1,
								AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			Encryption = new RadioButton(Util.GS("Passphrase Encryption"));
			optionsTable.Attach(Encryption, 2,3,1,2, AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			Regular = new RadioButton(Encryption, Util.GS("Regular"));
			optionsTable.Attach(Regular, 3,4,1,2, AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			SecureSync = new CheckButton(Util.GS("Secure Sync"));
			optionsTable.Attach(SecureSync, 4,5,1,2,
                                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			

			l = new Label(Util.GS("Type")+":");
			l.Xalign = 0;
			optionsTable.Attach(l, 1,2,1,2,
								AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			// Set up Domains
			domainComboBox = ComboBox.NewText();
			optionsTable.Attach(domainComboBox, 2,4,0,1,
								AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
			
			int defaultDomain = 0;
			for (int x = 0; x < domains.Length; x++)
			{
				domainComboBox.AppendText(String.Format(domains[x].Name + " - " + domains[x].Host));
				if (filteredDomainID != null)
				{
					if (filteredDomainID == domains[x].ID)
						defaultDomain = x;
				}
				else
					defaultDomain = x;
			}
			
			domainComboBox.Active = defaultDomain;
			int encr_status = ifws.GetSecurityPolicy(domains[defaultDomain].ID);
			ChangeStatus(encr_status);

/*
			l = new Label(Util.GS("Description:"));
			l.Xalign = 0;
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
*/
			moreOptionsExpander.Expanded = true;			
			optionsTable.ShowAll();
			
			return moreOptionsExpander;
		}

        /// <summary>
        /// OnSelection Changed
        /// </summary>
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
			catch (Exception)
			{
				this.SetResponseSensitive(ResponseType.Ok, false);
			}
		}
		
        /// <summary>
        /// Event handler for Key Release event
        /// </summary>
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
		/// CheckEnable OK Button
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

