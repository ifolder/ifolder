/***********************************************************************
 |  $RCSfile: CreateDialog.cs,v $
 |
 | Copyright (c) 2007 Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com 
 |
 |  Authors:
 |		Calvin Gaisford <cgaisford@novell.com>
 |		Boyd Timothy <btimothy@novell.com>
 | 
 ***********************************************************************/

using Gtk;
using System;

namespace Novell.iFolder
{
	public class CreateDialog : FileChooserDialog
	{
		enum SecurityState
		{
			encryption = 1,
			enforceEncryption = 2,
			SSL = 4,
			enforceSSL = 8
		}
		private DomainInformation[]	domains;
		private ComboBox			domainComboBox;
//		private CheckButton			Encryption;
//		private CheckButton 			SSL;
		private RadioButton 			Encryption;
		private RadioButton			SSL;
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

		public bool ssl
		{
			get
			{
				return SSL.Active;
			}
		}

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

		private void OnDomainChangedEvent(System.Object o, EventArgs args)
		{
			int SecurityPolicy = ifws.GetSecurityPolicy(this.DomainID);
			ChangeStatus(SecurityPolicy);
		}
		
		private void ChangeStatus(int SecurityPolicy)
		{
			Encryption.Active = SSL.Active = false;
			Encryption.Sensitive = SSL.Sensitive = false;
			
			if(SecurityPolicy !=0)
			{
				if( (SecurityPolicy & (int)SecurityState.encryption) == (int) SecurityState.encryption)
				{
					if( (SecurityPolicy & (int)SecurityState.enforceEncryption) == (int) SecurityState.enforceEncryption)
						Encryption.Active = true;
					else
					{
						Encryption.Sensitive = true;
						SSL.Sensitive = true;
					}
				}
				else
				{
					SSL.Active = true;
				}
				/*
				if( (SecurityPolicy & (int)SecurityState.SSL) == (int) SecurityState.SSL)
				{
					if( (SecurityPolicy & (int)SecurityState.enforceSSL) == (int) SecurityState.enforceSSL)
						SSL.Active = true;
					else
						SSL.Sensitive = true;
				}
				*/
			}
			else
			{
				SSL.Active = true;
			}
		}

		
		private Widget CreateMoreOptionsExpander(string filteredDomainID)
		{
			Expander moreOptionsExpander = new Expander(Util.GS("More options"));

			Table optionsTable = new Table(4, 3, false);
			moreOptionsExpander.Add(optionsTable);
			
			optionsTable.ColumnSpacing = 10;
			optionsTable.RowSpacing = 10;
			optionsTable.SetColSpacing(0, 30);
			
			Label l = new Label(Util.GS("iFolder Account")+":");
			l.Xalign = 0;
			///Syntax:  Table.Attach(widget, column_start, column_end, row_start, row_end, AttachOptions,.....
			optionsTable.Attach(l, 1,2,0,1,
								AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			Encryption = new RadioButton(Util.GS("Encrypted"));
			optionsTable.Attach(Encryption, 2,3,1,2, AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			SSL = new RadioButton(Encryption, Util.GS("Shared"));
			optionsTable.Attach(SSL, 3,4,1,2, AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

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

