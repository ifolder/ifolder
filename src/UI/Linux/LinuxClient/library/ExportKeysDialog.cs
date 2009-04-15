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
using Novell.iFolder.Controller;

namespace Novell.iFolder
{
    /// <summary>
    /// class Export Keys Dialog
    /// </summary>
	public class ExportKeysDialog : Dialog
	{

		private ComboBox domainCombo;
		private Button BrowseButton;
		private Entry location;
		private Entry email;
		private Entry oldPassPhrase;
		private Entry newPassPhrase;
		private Entry retypePassPhrase;
		private Entry recoveryAgentCombo;
		private string[] RAList;
		private string DomainID;
		private DomainInformation[] domains;
		private CheckButton savePassPhrase;
	
		private Image				 iFolderBanner;
		private Image				 iFolderScaledBanner;
		private Gdk.Pixbuf			 ScaledPixbuf;
		private iFolderWebService 		 ifws;
		private SimiasWebService		 simws; 

        /// <summary>
        /// Gets the FileName
        /// </summary>
		public string FileName
		{
			get
			{
				return this.location.Text;
			}
		}
	
        /// <summary>
        /// Gets the Domain
        /// </summary>
		public string Domain
		{
			get
			{
				int activeIndex = domainCombo.Active;
				if (activeIndex >= 0)
					return domains[activeIndex].ID;
				else
					return null;
			}
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ifws">iFolder WebService</param>
        /// <param name="simws">SimiasWebService</param>
		public ExportKeysDialog(iFolderWebService ifws, SimiasWebService simws) : base()
		{
			this.ifws = ifws;
			this.simws = simws;
			this.DomainID = DomainID;
			SetupDialog();
		}

        /// <summary>
        /// Setup Dialog - Customize
        /// </summary>
		private void SetupDialog()
		{
			this.Title = Util.GS("Export Encrypted Keys");
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder16.png"));
			this.HasSeparator = false;
//			this.BorderWidth = 10;
			this.SetDefaultSize (450, 100);
	//		this.Resizable = false;
			this.Modal = true;
			this.DefaultResponse = ResponseType.Ok;

			//-----------------------------
			// Add iFolderGraphic
			//-----------------------------
			HBox imagebox = new HBox();
			imagebox.Spacing = 0;
			iFolderBanner = new Image(
					new Gdk.Pixbuf(Util.ImagesPath("ifolder-banner.png")));
			imagebox.PackStart(iFolderBanner, false, false, 0);

			ScaledPixbuf = 
				new Gdk.Pixbuf(Util.ImagesPath("ifolder-banner-scaler.png"));
			iFolderScaledBanner = new Image(ScaledPixbuf);
			iFolderScaledBanner.ExposeEvent += 
					new ExposeEventHandler(OnBannerExposed);
			imagebox.PackStart(iFolderScaledBanner, true, true, 0);
			this.VBox.PackStart (imagebox, false, true, 0);

                        ///
                        /// Content
                        ///
                        Table table = new Table(4, 3, false);
                        table.ColumnSpacing = 6;
                        table.RowSpacing = 6;
                        table.BorderWidth = 12;

                        // Row 1
                        Label l = new Label(Util.GS("iFolder account")+":");
                        table.Attach(l, 0,1, 0,1,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        l.LineWrap = true;
			l.Xalign = 0.0F;

                        domainCombo = ComboBox.NewText();
			DomainController domainController = DomainController.GetDomainController();
			domains = domainController.GetDomains();
			for (int x = 0; x < domains.Length; x++)
			{
				domainCombo.AppendText(domains[x].Name);
			}
			if( domains.Length > 0)
				domainCombo.Active = 0;
                        // read domains from domain controller...
                        table.Attach(domainCombo, 1,2,0,1, AttachOptions.Fill|AttachOptions.Expand, 0,0,0);
                        // Row 2
                        l = new Label(Util.GS("File path")+":");
			l.Xalign = 0.0F;
                        table.Attach(l, 0,1, 1,2,
                                AttachOptions.Fill, 0,0,0); // spacer

                        location = new Entry();
			this.location.Changed += new EventHandler(OnFieldsChanged);
                        table.Attach(location, 1,2, 1,2,
                                AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
                        l.MnemonicWidget = location;

                        BrowseButton = new Button(Util.GS("_Browse"));
                        table.Attach(BrowseButton, 2,3, 1,2, AttachOptions.Fill, 0,0,0);
                        BrowseButton.Sensitive = true;
			BrowseButton.Clicked += new EventHandler(OnBrowseButtonClicked);

                        // Row 3
                        l = new Label(Util.GS("Recovery agent")+":");
			l.Xalign = 0.0F;
                        table.Attach(l, 0,1, 2,3,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);

//                        recoveryAgentCombo = ComboBox.NewText();
			recoveryAgentCombo = new Entry();
			recoveryAgentCombo.Sensitive = false;
            table.Attach(recoveryAgentCombo, 1,2,2,3, AttachOptions.Fill|AttachOptions.Expand, 0,0,0);
            // Populate combo box with recovery agents
            // Row 4
            l = new Label(Util.GS("E-Mail ID")+":");
			l.Xalign = 0.0F;
                        table.Attach(l, 0,1, 3,4,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        email = new Entry();
			email.Sensitive = false;
                        table.Attach(email, 1,2,3,4, AttachOptions.Fill|AttachOptions.Expand, 0,0,0);
			this.VBox.PackStart(table, false, false, 0);
			this.VBox.ShowAll();
		

			this.AddButton(Stock.Help, ResponseType.Help);
			this.AddButton(Stock.Cancel, ResponseType.Cancel);
			this.AddButton(Stock.Ok, ResponseType.Ok);
			this.SetResponseSensitive(ResponseType.Ok, false);
			this.DefaultResponse = ResponseType.Ok;
			domainCombo.Changed += new EventHandler(OnDomainChangedEvent);

			UpdateUI();
			DisplayRAName();
		}
		
        /// <summary>
        /// Event Handler for Domain Changed Event
        /// </summary>
        private void OnDomainChangedEvent( object o, EventArgs args)
		{
			UpdateUI();
			DisplayRAName();
		}

		/// <summary>
		/// Update UI
		/// </summary>
        private void UpdateUI()
		{
			if( ifws.GetSecurityPolicy(this.Domain) != 0  && simws.IsPassPhraseSet(this.Domain))
				location.Sensitive = BrowseButton.Sensitive = true; 
			else
				location.Sensitive = BrowseButton.Sensitive = false;   
		}		

        /// <summary>
        /// Display RA Name
        /// </summary>
		private void DisplayRAName()
		{
			string domainID = this.Domain;
			DomainController domController = DomainController.GetDomainController();
			string raName = domController.GetRAName(domainID);
			email.Text = Util.GS("Information Not Available");
			if( raName ==null || raName == "")
			{
				recoveryAgentCombo.Text = Util.GS("Information Not Available");
			}
			else
			{
				//parsing the RAName by '='
				char [] EmailParser = {'='};
				string [] ParsedString = raName.Split(EmailParser);
				string emailID = "";
				if (ParsedString.Length > 1)
				{
					for(int x = 0; x < ParsedString.Length; x++)
					{
						// Iterate through the parsed string to again parse for '@' to get mail-id
						char [] FinalEmailParser = {'@'};
						string [] FinalParsedString = ParsedString[x].Split(FinalEmailParser);
						if(FinalParsedString.Length > 1)
						{
							emailID = ParsedString[x];
							email.Text = emailID;
						}	
					}
				}	
				//Console.WriteLine("email ID :"+emailID);	
				recoveryAgentCombo.Text = raName;
			}	
		}

        /// <summary>
        /// Event Handler for Browse Button Click
        /// </summary>
        private void OnBrowseButtonClicked(object o, EventArgs e)
		{
			//change the message to "Select a file..." instead of "Select a folder..."
			FileChooserDialog filedlg = new FileChooserDialog("", Util.GS("Select a folder..."), this, FileChooserAction.Save, Stock.Cancel, ResponseType.Cancel,Stock.Ok, ResponseType.Ok);
			int res = filedlg.Run();
			string str = filedlg.Filename;
			filedlg.Hide();
			filedlg.Destroy();
			if( res == (int)ResponseType.Ok)
			{
				this.location.Text = str;
			}
			//Otherwise do nothing
		}

        /// <summary>
        /// Event handler for Banner Exposed event
        /// </summary>
        private void OnBannerExposed(object o, ExposeEventArgs args)
		{
			if(args.Event.Count > 0)
				return;

			Gdk.Pixbuf spb = 
				ScaledPixbuf.ScaleSimple(iFolderScaledBanner.Allocation.Width,
										iFolderScaledBanner.Allocation.Height,
										Gdk.InterpType.Nearest);

			Gdk.GC gc = new Gdk.GC(iFolderScaledBanner.GdkWindow);

			spb.RenderToDrawable(iFolderScaledBanner.GdkWindow,
											gc,
											0, 0,
											args.Event.Area.X,
											args.Event.Area.Y,
											args.Event.Area.Width,
											args.Event.Area.Height,
											Gdk.RgbDither.Normal,
											0, 0);
		}

        /// <summary>
        /// Event Handler for Fields Change event
        /// </summary>
        private void OnFieldsChanged(object obj, EventArgs args)
		{
			bool enableOK = false;
			if( this.location.Text.Length > 0)
				enableOK = true;
			this.SetResponseSensitive(ResponseType.Ok, enableOK);
		}
	}
}
