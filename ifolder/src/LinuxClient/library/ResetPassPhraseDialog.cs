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
	public class ResetPassPhraseDialog : Dialog
	{

		private ComboBox domainComboBox;
		private Entry oldPassPhrase;
		private Entry newPassPhrase;
		private Entry retypePassPhrase;
		private ComboBox recoveryAgentCombo;
		private string[] RAList;
		private string DomainID;
		private CheckButton savePassPhrase;
	
		private Image				 iFolderBanner;
		private Image				 iFolderScaledBanner;
		private Gdk.Pixbuf			 ScaledPixbuf;


		public ResetPassPhraseDialog() : base()
		{
			this.DomainID = DomainID;
			SetupDialog();
		}
		private void SetupDialog()
		{
			this.Title = Util.GS("Reset Passphrase");
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

			Table table = new Table(6, 2, false);
			this.VBox.PackStart(table, false, false, 0);
			table.ColumnSpacing = 6;
			table.RowSpacing = 6;
			table.BorderWidth = 12;

			// Row 1
			Label lbl = new Label(Util.GS("iFolder Account")+":");
			table.Attach(lbl, 0,1, 0,1,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			lbl.LineWrap = true;
			lbl.Xalign = 0.0F;
			domainComboBox = ComboBox.NewText();
			table.Attach(domainComboBox, 1,2, 0,1,
					AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
			DomainController domainController = DomainController.GetDomainController();
			DomainInformation[] domains = domainController.GetDomains();	
			for (int x = 0; x < domains.Length; x++)
			{
				domainComboBox.AppendText(domains[x].Name);
			}
			if( domains.Length > 0)
				domainComboBox.Active = 0;

			// Row 2
			
			lbl = new Label(Util.GS("Enter Passphrase")+":");
			table.Attach(lbl, 0,1, 1,2,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			lbl.LineWrap = true;
			lbl.Xalign = 0.0F;
			
			oldPassPhrase = new Entry();
			table.Attach(oldPassPhrase, 1,2, 1,2,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			lbl.MnemonicWidget = oldPassPhrase;

			// Row 3	
			lbl = new Label(Util.GS("Enter New Passphrase")+":");
			table.Attach(lbl, 0,1, 2,3,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			lbl.LineWrap = true;
			lbl.Xalign = 0.0F;
			
			newPassPhrase = new Entry();
			table.Attach(newPassPhrase, 1,2, 2,3,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			lbl.MnemonicWidget = newPassPhrase;	

			// Row 4	
			lbl = new Label(Util.GS("Re-type Passphrase")+":");
			table.Attach(lbl, 0,1, 3,4,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			lbl.LineWrap = true;
			lbl.Xalign = 0.0F;
			
			retypePassPhrase = new Entry();
			table.Attach(retypePassPhrase, 1,2, 3,4,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			lbl.MnemonicWidget = retypePassPhrase;					

			// Row 5
			lbl = new Label(Util.GS("Recovery Agent")+":");
			table.Attach(lbl, 0,1, 4,5,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			lbl.LineWrap = true;
			lbl.Xalign = 0.0F;
			recoveryAgentCombo = ComboBox.NewText();
			table.Attach(recoveryAgentCombo, 1,2, 4,5,
					AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
/*			RAList = domainController.GetRAList(DomainID);
			if( RAList == null)
			{
				Console.WriteLine(" no recovery agent present:");
	                        foreach (string raagent in RAList )
        	                    recoveryAgentCombo.AppendText(raagent);
			}

*/
			recoveryAgentCombo.AppendText("First");

			// Row 6
			savePassPhrase = new CheckButton(Util.GS("Remember Passphrase"));
			table.Attach(savePassPhrase, 1,2,5,6, AttachOptions.Expand|AttachOptions.Fill, 0,0,0);
			
			this.VBox.ShowAll();
		

			this.AddButton(Stock.Cancel, ResponseType.Cancel);
			this.AddButton(Util.GS("Reset"), ResponseType.Ok);
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
