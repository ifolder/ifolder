/***********************************************************************
 |  $RCSfile: iFolderLoginDialog.cs,v $
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
 |  Author: Calvin Gaisford <cgaisford@novell.com>
 | 
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
		private DomainInformation[] domains;
		private string DomainID;
		private CheckButton savePassPhrase;
	
		private Image				 iFolderBanner;
		private Image				 iFolderScaledBanner;
		private Gdk.Pixbuf			 ScaledPixbuf;
		private bool status;

		public DomainInformation[] Domains
		{
			get
			{
				return this.domains;
			}
			set
			{
				this.domains = value;
			}
		}

		public string Domain
		{
			get
			{
				if( domains != null)
					return domains[domainComboBox.Active].ID;
				else 
					return null;
			}
		}

		public string OldPassphrase
		{
			get
			{
				return oldPassPhrase.Text;
			}
		}

		public string NewPassphrase
		{
			get
			{
				return newPassPhrase.Text;
			}
		}

		public bool SavePassphrase
		{
			get
			{
				return savePassPhrase.Active;
			}
		}

		public string RAName
		{
			get
			{
				return recoveryAgentCombo.ActiveText;
			}
		}

		public bool Status
		{
			get
			{
				return status;
			}

		}

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
			/*
			DomainController domainController = DomainController.GetDomainController();
			domains = domainController.GetDomains();	
			for (int x = 0; x < domains.Length; x++)
			{
				domainComboBox.AppendText(domains[x].Name);
			}
			if( domains.Length > 0)
				domainComboBox.Active = 0;
			*/
			// Row 2
			
			lbl = new Label(Util.GS("Enter Present Passphrase")+":");
			table.Attach(lbl, 0,1, 1,2,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			lbl.LineWrap = true;
			lbl.Xalign = 0.0F;
			
			oldPassPhrase = new Entry();
			oldPassPhrase.Visibility = false;
			table.Attach(oldPassPhrase, 1,2, 1,2,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			lbl.MnemonicWidget = oldPassPhrase;
			oldPassPhrase.Changed += new EventHandler(UpdateSensitivity);

			// Row 3	
			lbl = new Label(Util.GS("Enter New Passphrase")+":");
			table.Attach(lbl, 0,1, 2,3,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			lbl.LineWrap = true;
			lbl.Xalign = 0.0F;
			
			newPassPhrase = new Entry();
			newPassPhrase.Visibility = false;
			table.Attach(newPassPhrase, 1,2, 2,3,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			lbl.MnemonicWidget = newPassPhrase;
			newPassPhrase.Changed += new EventHandler(UpdateSensitivity);	

			// Row 4	
			lbl = new Label(Util.GS("Re-type Passphrase")+":");
			table.Attach(lbl, 0,1, 3,4,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			lbl.LineWrap = true;
			lbl.Xalign = 0.0F;
			
			retypePassPhrase = new Entry();
			retypePassPhrase.Visibility = false;
			table.Attach(retypePassPhrase, 1,2, 3,4,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			lbl.MnemonicWidget = retypePassPhrase;			
			retypePassPhrase.Changed += new EventHandler(UpdateSensitivity);		

			// Row 5
			lbl = new Label(Util.GS("Recovery Agent")+":");
			table.Attach(lbl, 0,1, 4,5,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			lbl.LineWrap = true;
			lbl.Xalign = 0.0F;
			recoveryAgentCombo = ComboBox.NewText();
			table.Attach(recoveryAgentCombo, 1,2, 4,5,
					AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
		/*
			DomainController domainController = DomainController.GetDomainController();
			RAList = domainController.GetRAList("f5737f43-d284-4b7b-8647-ecb151aeabdd");
			if( RAList != null)
			{
				//Debug.PrintLine(" no recovery agent present:");
	                        foreach (string raagent in RAList )
				{
					Console.WriteLine("RALIst is {0}", raagent);
        	                    recoveryAgentCombo.AppendText(raagent);
				}
			}

			recoveryAgentCombo.AppendText("First");
		*/

			// Row 6
			savePassPhrase = new CheckButton(Util.GS("Remember Passphrase"));
			table.Attach(savePassPhrase, 1,2,5,6, AttachOptions.Expand|AttachOptions.Fill, 0,0,0);
			
			this.VBox.ShowAll();
		

			this.AddButton(Stock.Cancel, ResponseType.Cancel);
			Button but = (Button)this.AddButton(Util.GS("Reset"), ResponseType.Ok);
			but.Clicked += new EventHandler(OnResetClicked);
			but.Image = new Image(Stock.Undo, Gtk.IconSize.Menu);//new Image(new Gdk.Pixbuf(Util.ImagesPath("ifolder-download16.png")));
			this.SetResponseSensitive(ResponseType.Ok, false);
			this.DefaultResponse = ResponseType.Ok;
			this.Realized += new EventHandler(OnResetPassphraseLoad);	
			domainComboBox.Changed += new EventHandler(OnDomainChangedEvent);
		}

		private void OnResetClicked( object o, EventArgs args)
		{
			Debug.PrintLine("Reset clicked");
			string publicKey = null;
			bool reset = false;
			DomainController domainController = DomainController.GetDomainController();
			if( this.RAName != Util.GS("None") && this.RAName != string.Empty)
			{
				byte [] RACertificateObj = domainController.GetRACertificate(this.Domain, this.RAName);
				if( RACertificateObj != null && RACertificateObj.Length != 0)
				{
					System.Security.Cryptography.X509Certificates.X509Certificate Cert = new System.Security.Cryptography.X509Certificates.X509Certificate(RACertificateObj);
					CertificateDialog dlg = new CertificateDialog(Cert.ToString(true));
					if (!Util.RegisterModalWindow(dlg))
					{
						dlg.Destroy();
						dlg = null;
						return ;
					}
					int res = dlg.Run();
					dlg.Hide();
					dlg.Destroy();
					dlg = null;
					if( res == (int)ResponseType.Ok)
					{
						publicKey = Convert.ToBase64String(Cert.GetPublicKey());
						Debug.PrintLine(String.Format(" The public key is: {0}", publicKey));
						reset = true;
					}
					else
					{
						reset = false;
						Debug.PrintLine("Response type is not ok");
						return;
					}
				}
			}
			else
			{
		                iFolderMsgDialog dg = new iFolderMsgDialog(
		                    this, 
		                    iFolderMsgDialog.DialogType.Warning,
		                    iFolderMsgDialog.ButtonSet.YesNo,
		                    "No Recovery Agent",
		                    Util.GS("Recovery Agent Not Selected"),
		                    Util.GS("There is no Recovery Agent selected. Encrypted data cannot be recovered later, if passphrase is lost. Do you want to continue?"));
				       	int rc = dg.Run();
			    		dg.Hide();
		            	dg.Destroy();
		                if( (ResponseType)rc == ResponseType.Yes )
		                {
					// reset passphrase
					reset = true;
		                }
		                else
		                {
					return;
		                }
			}
			if( reset == true)
			{
				try
				{
					status = domainController.ReSetPassphrase(this.Domain, this.OldPassphrase, this.NewPassphrase, this.RAName, publicKey);
				}
				catch(Exception ex)
				{
				}
			}
			if( status == false)
			{
				 iFolderMsgDialog dialog = new iFolderMsgDialog(
                                                                                                               null,
                                                                                                               iFolderMsgDialog.DialogType.Error,
                                                                                                                iFolderMsgDialog.ButtonSet.None,
                                                                                                                Util.GS("Reset Passphrase"),
                                                                                                                Util.GS("Unable to change the Passphrase"),
                                                                                                                Util.GS("Please try again"));
                               	dialog.Run();
                                dialog.Hide();
       	                        dialog.Destroy();
               	                dialog = null;
			}
		}

		protected bool OnDeleteEvent(object o, EventArgs args)
		{
			return true;
		}


		private void OnResetPassphraseLoad( object o, EventArgs args)
		{
			DomainController domainController = DomainController.GetDomainController();
			domains = domainController.GetLoggedInDomains();
			if( domains == null)
			{
					this.Respond( ResponseType.DeleteEvent);
					return;	
			}
			for (int x = 0; x < domains.Length; x++)
			{
				domainComboBox.AppendText(domains[x].Name);
			}
			if( domains.Length > 0)
				domainComboBox.Active = 0;
			DisplayRAList();
		}

		private void OnDomainChangedEvent( object o, EventArgs args)
		{
			DisplayRAList();
		}
		
		private void DisplayRAList()
		{
			string domainID = this.Domain;
			if( RAList != null)
				for( int i=RAList.Length; i>=0; i--)
					recoveryAgentCombo.RemoveText(i);
			DomainController domController = DomainController.GetDomainController();
			Debug.PrintLine(string.Format("domain id is: {0}", domainID));
			RAList = domController.GetRAList(domainID);
			if( RAList != null)
			{
	            		foreach (string raagent in RAList )
	            		{
        	    			recoveryAgentCombo.AppendText(raagent);
        	    		}	
			}
			else
			{
				Debug.PrintLine("No recovery agent present");
			}
			recoveryAgentCombo.AppendText(Util.GS("None"));
			recoveryAgentCombo.Active = 0;
		}

		private void UpdateSensitivity( object o, EventArgs args)
		{
			if( oldPassPhrase != null && newPassPhrase != null && retypePassPhrase != null)
			{
				if( newPassPhrase.Text.Length >0 && newPassPhrase.Text == retypePassPhrase.Text)
				{
					// Check for validity of passphrase
					if( oldPassPhrase.Text.Length > 0)
					{
						this.SetResponseSensitive( ResponseType.Ok, true);
						return;	
					}
				}
			}
			this.SetResponseSensitive( ResponseType.Ok, false);
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
