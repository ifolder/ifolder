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

namespace Novell.iFolder
{
	public class EnterPassPhraseDialog : Dialog
	{



		private Entry PassPhraseEntry;
		private Entry           PassPhraseVerifyEntry;
		private CheckButton savePassPhraseButton;
		private string[] RAList;

		private iFolderTreeView RATreeView;

		private Entry	nameEntry;
		private Entry	passEntry;
		private Entry	serverEntry;
		private CheckButton savePasswordButton;
		private string	DomainID;
		private string  DomainName;
		private string  DomainUserName;
		private bool	FullDialog;

		private Image				 iFolderBanner;
		private Image				 iFolderScaledBanner;
		private Gdk.Pixbuf			 ScaledPixbuf;


		public string PassPhrase
		{
			get
			{
				return PassPhraseEntry.Text;
			}
		}

		public string RetypedPassPhrase
		{
			get
			{
				return PassPhraseVerifyEntry.Text;
			}
		}	
		public string UserName
		{
			get
			{
				if(FullDialog)
					return nameEntry.Text;
				else
					return "";
			}
		}

		public string Password
		{
			get
			{
				return passEntry.Text;
			}
			set
			{
				passEntry.Text = value;
			}
		}

		public string Host
		{
			get
			{
				if(FullDialog)
					return serverEntry.Text;
				else
					return "";
			}
		}

		public string Domain
		{
			get
			{
				if(FullDialog)
					return "";
				else
					return DomainID;
			}
		}
		
		public bool ShouldSavePassword
		{
			get
			{
				if (savePasswordButton != null)
					return savePasswordButton.Active;
				else
					return false;
					
			}
		}

		public EnterPassPhraseDialog(string domain, string domainName,
					string userName) : base()
		{
			DomainID = domain;
			DomainName = domainName;
			DomainUserName = userName;
			FullDialog = false;
			SetupDialog();
		}

		public EnterPassPhraseDialog() : base()
 		{
			FullDialog = true;
			SetupDialog();
		}

		private void SetupDialog()
		{
			this.Title = Util.GS("PassPhrase Manager");
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

			Table table = new Table(7777777, 3, false);
			this.VBox.PackStart(table, false, false, 0);
			table.ColumnSpacing = 6;
			table.RowSpacing = 6;
			table.BorderWidth = 12;

			// Row 1
			Label l = new Label(Util.GS("Enter the Passphrase"));
			table.Attach(l, 0,3, 0,1,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.LineWrap = true;
			l.Xalign = 0.0F;

			// Row 2
			table.Attach(new Label(""), 0,1, 1,2,
				AttachOptions.Fill, 0,12,0); // spacer
			l = new Label(Util.GS("_PassPhrase:"));
			table.Attach(l, 1,2, 1,2,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			l.Xalign = 0.0F;
			PassPhraseEntry = new Entry();
			PassPhraseEntry.Visibility = false;
			PassPhraseEntry.Changed += new EventHandler(OnFieldsChanged);
			table.Attach(PassPhraseEntry, 2,3, 1,2,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.MnemonicWidget = PassPhraseEntry;

			// Row 3
			l = new Label(Util.GS("_Retype PassPhrase:"));
			table.Attach(l, 1,2, 2,3,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			l.Xalign = 0.0F;
			PassPhraseVerifyEntry = new Entry();
			PassPhraseVerifyEntry.Visibility = false;
			PassPhraseVerifyEntry.Changed += new EventHandler(OnFieldsChanged);
			table.Attach(PassPhraseVerifyEntry, 2,3, 2,3,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.MnemonicWidget = PassPhraseVerifyEntry;
			
			// new row 4

			savePassPhraseButton = new CheckButton(Util.GS("Remember Passphrase"));	
			table.Attach(savePassPhraseButton, 2,3, 3,4,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			// Row 4
			l = new Label(Util.GS("Select Recovery Agent"));
			table.Attach(l, 0,3, 4,5,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.LineWrap = true;
			l.Xalign = 0.0F;
			// Row 5
			RATreeView = new iFolderTreeView ();
			ScrolledWindow sw = new ScrolledWindow();
			sw.ShadowType = Gtk.ShadowType.EtchedIn;
			sw.HscrollbarPolicy = Gtk.PolicyType.Automatic;
			sw.VscrollbarPolicy = Gtk.PolicyType.Automatic;
			sw.Add(RATreeView);

			ListStore RATreeStore = new ListStore(typeof(string));
			RATreeView.Model = RATreeStore;
//                      RAList = domainController.GetRAList ();
//                      foreach (string raagent in RAList )
//                          RATreeStore.AppendValues (raagent);

			// RA Name Column
			TreeViewColumn raNameColumn = new TreeViewColumn();
			raNameColumn.Title = Util.GS("Recovery Agents");
			CellRendererText cr = new CellRendererText();
			cr.Xpad = 5;
			raNameColumn.PackStart(cr, false);
			raNameColumn.SetCellDataFunc(cr,
						     new TreeCellDataFunc(RANameCellTextDataFunc));
			raNameColumn.Resizable = true;
			raNameColumn.MinWidth = 250;

			RATreeView.AppendColumn(raNameColumn);

			RATreeView.Selection.Mode = SelectionMode.Single;
 			RATreeStore.AppendValues ("HELLO");
 			RATreeStore.AppendValues ("HELLO1");

 			table.Attach(sw, 0,3, 5,7,
 				AttachOptions.Expand | AttachOptions.Fill, 0,0,0);


			this.VBox.ShowAll();
		

			this.AddButton(Stock.Cancel, ResponseType.Cancel);
			this.AddButton(Util.GS("Co_nnect"), ResponseType.Ok);
			this.SetResponseSensitive(ResponseType.Ok, false);
			this.DefaultResponse = ResponseType.Ok;
		}
		private void RANameCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			string value = (string) tree_model.GetValue(iter, 0);
			((CellRendererText) cell).Text = value;
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

			if( (PassPhraseEntry.Text.Length > 0) &&
				(PassPhraseVerifyEntry.Text.Length > 0 ) )
				enableOK = true;

			this.SetResponseSensitive(ResponseType.Ok, enableOK);
		}
	}
}
