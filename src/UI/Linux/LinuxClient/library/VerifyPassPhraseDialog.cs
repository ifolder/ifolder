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

namespace Novell.iFolder
{
    /// <summary>
    /// class verify Passphrase Dialog
    /// </summary>
	public class VerifyPassPhraseDialog : Dialog
	{
		private Entry passPhraseEntry;
		private CheckButton savePassPhraseButton;
		private Image				 iFolderBanner;
		private Image				 iFolderScaledBanner;
		private Gdk.Pixbuf			 ScaledPixbuf;

        /// <summary>
        /// Gets the Passphrase
        /// </summary>
		public string PassPhrase
		{
			get
			{
				return passPhraseEntry.Text;
			}
		}

        /// <summary>
        /// Gets the Shuold save Passphrase
        /// </summary>
		public bool ShouldSavePassPhrase
		{
			get
			{
				if (savePassPhraseButton != null)
					return savePassPhraseButton.Active;
				else
					return false;
					
			}
		}

        /// <summary>
        /// Constructor
        /// </summary>
		public VerifyPassPhraseDialog() : base()
 		{
//			FullDialog = true;
			SetupDialog();
		}

        /// <summary>
        /// Setup Dialog
        /// </summary>
		private void SetupDialog()
		{
			this.Title = Util.GS("iFolder PassPhrase");
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder16.png"));
			this.HasSeparator = false;
//			this.BorderWidth = 10;
			this.SetDefaultSize (350, 100);
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
	
			Table loginTable;

			loginTable = new Table(4,2,false);

			loginTable.BorderWidth = 10;
			loginTable.RowSpacing = 10;
			loginTable.ColumnSpacing = 10;
			loginTable.Homogeneous = false;

			Label passPhraseLabel = new Label(Util.GS("Enter Passphrase")+":");
			passPhraseLabel.Xalign = 1;
			loginTable.Attach( passPhraseLabel, 0,1,0,2, AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			passPhraseEntry = new Entry();
			passPhraseEntry.ActivatesDefault = true;
			passPhraseEntry.Visibility = false;
			passPhraseEntry.Changed += new EventHandler(OnPassPhraseChanged);
			loginTable.Attach(passPhraseEntry, 1,2,0,2, AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
				savePassPhraseButton = 
					new CheckButton(Util.GS(
						"_Remember Passphrase"));
				loginTable.Attach(savePassPhraseButton, 1,2,2,4,
						AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
	
			this.VBox.PackStart(loginTable, false, false, 0);
			this.VBox.ShowAll();

			this.AddButton(Stock.Cancel, ResponseType.Cancel);
			this.AddButton(Stock.Ok/*Util.GS("Co_nnect")*/, ResponseType.Ok);
			this.SetResponseSensitive(ResponseType.Ok, false);
			this.DefaultResponse = ResponseType.Ok;
		}

        /// <summary>
        /// Event Handler for Banner Exposed event
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
        /// Event Handler PassPhrase changed
        /// </summary>
        private void OnPassPhraseChanged(object obj, EventArgs args)
		{
			bool enableOK = false;

			if(passPhraseEntry.Text.Length > 0) 
				enableOK = true;
			this.SetResponseSensitive(ResponseType.Ok, enableOK);
		}

/*
		private void OnFieldsChanged(object obj, EventArgs args)
		{
			bool enableOK = false;

			if( FullDialog &&
				(nameEntry.Text.Length > 0) &&
				(passEntry.Text.Length > 0 ) &&
				(serverEntry.Text.Length > 0) )
				enableOK = true;
			else if( (!FullDialog) &&
				(passEntry.Text.Length > 0 ) )
				enableOK = true;

			this.SetResponseSensitive(ResponseType.Ok, enableOK);
		}
*/
	}
}
