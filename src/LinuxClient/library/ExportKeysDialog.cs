/***********************************************************************
 *  $RCSfile: iFolderLoginDialog.cs,v $
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
using Novell.iFolder.Controller;

namespace Novell.iFolder
{
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
		private CheckButton savePassPhrase;
	
		private Image				 iFolderBanner;
		private Image				 iFolderScaledBanner;
		private Gdk.Pixbuf			 ScaledPixbuf;


		public ExportKeysDialog() : base()
		{
			this.DomainID = DomainID;
			SetupDialog();
		}
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
                        Label l = new Label(Util.GS("iFolder Account")+":");
                        table.Attach(l, 0,1, 0,1,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        l.LineWrap = true;
//			l.Xalign = 0.0F;

                        domainCombo = ComboBox.NewText();
                        // read domains from domain controller...
                        table.Attach(domainCombo, 1,2,0,1, AttachOptions.Fill|AttachOptions.Expand, 0,0,0);
                        // Row 2
                        l = new Label(Util.GS("File Path")+":");
                        table.Attach(l, 0,1, 1,2,
                                AttachOptions.Fill, 0,0,0); // spacer

                        location = new Entry();
                        table.Attach(location, 1,2, 1,2,
                                AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
                        l.MnemonicWidget = location;

                        BrowseButton = new Button("_Browse");
                        table.Attach(BrowseButton, 2,3, 1,2, AttachOptions.Fill, 0,0,0);
                        BrowseButton.Sensitive = false;
//                      BrowseButton.Clicked += new EventHandler(OnBrowseButtonClicked);

                        // Row 3
                        l = new Label(Util.GS("Recovery Agent")+":");
                        table.Attach(l, 0,1, 2,3,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);

//                        recoveryAgentCombo = ComboBox.NewText();
			recoveryAgentCombo = new Entry();
			recoveryAgentCombo.Sensitive = false;
                        table.Attach(recoveryAgentCombo, 1,2,2,3, AttachOptions.Fill|AttachOptions.Expand, 0,0,0);
                        // Populate combo box with recovery agents
                        // Row 4
                        l = new Label(Util.GS("E-Mail ID")+":");
                        table.Attach(l, 0,1, 3,4,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        email = new Entry();
			email.Sensitive = false;
                        table.Attach(email, 1,2,3,4, AttachOptions.Fill|AttachOptions.Expand, 0,0,0);
			this.VBox.PackStart(table, false, false, 0);
			this.VBox.ShowAll();
		

			this.AddButton(Stock.Cancel, ResponseType.Cancel);
			this.AddButton(Stock.Ok, ResponseType.Ok);
			this.SetResponseSensitive(ResponseType.Ok, false);
			this.DefaultResponse = ResponseType.Ok;
		}
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

		private void OnFieldsChanged(object obj, EventArgs args)
		{
			bool enableOK = false;
			this.SetResponseSensitive(ResponseType.Ok, enableOK);
		}
	}
}