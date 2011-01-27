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

using System;
using System.IO;
using System.Collections;
using Gtk;
using Simias.Client;
using Simias.Client.Event;

namespace Novell.iFolder
{

	/// <summary>
	/// This is the main iFolder Window.  This window implements all of the
	/// client code for iFolder.
	/// </summary>
	public class PreferencesWindow : Window
	{
		private iFolderWebService		ifws;

		private Gtk.Notebook			PrefNoteBook;
		private PrefsGeneralPage		generalPage;
		private PrefsAccountsPage		accountsPage;
		//private MigrationPage			migrationPage;
		private bool					ControlKeyPressed;

        /// <summary>
        /// Gets / Sets the Current Page
        /// </summary>
		public int CurrentPage
		{
			set
			{
				if(value <= PrefNoteBook.NPages)
					PrefNoteBook.CurrentPage = value;
			}
			get
			{
				return PrefNoteBook.CurrentPage;
			}
		}


		/// <summary>
		/// Default constructor for iFolderWindow
		/// </summary>
		public PreferencesWindow(iFolderWebService webService, Manager simiasManager)
			: base(Util.GS("iFolder Preferences"))
		{
			if(webService == null)
				throw new ApplicationException("iFolderWebServices was null");

			ifws = webService;

			InitializeWidgets(simiasManager);
			
			// Bind ESC and C-w to close the window
			ControlKeyPressed = false;
			KeyPressEvent += new KeyPressEventHandler(KeyPressHandler);
			KeyReleaseEvent += new KeyReleaseEventHandler(KeyReleaseHandler);
		}




		/// <summary>
		/// Set up the UI inside the Window
		/// </summary>
		private void InitializeWidgets(Manager simiasManager)
		{
			this.SetDefaultSize (480, 550);

			// Create an extra vbox to add the spacing
			EventBox prefsWindow = new EventBox();
			prefsWindow.ModifyBg(StateType.Normal, this.Style.Background(StateType.Normal));
			VBox winBox = new VBox();
			//this.Add (winBox);
			prefsWindow.Add(winBox);
			this.Add (prefsWindow);
			winBox.BorderWidth = 7;
			winBox.Spacing = 7;

			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder16.png"));
			this.WindowPosition = Gtk.WindowPosition.Center;

			//-----------------------------
			// Set up the Notebook (tabs)
			//-----------------------------
			PrefNoteBook = new Notebook();

			generalPage = new PrefsGeneralPage(this, ifws);
			PrefNoteBook.AppendPage( generalPage,
										new Label(Util.GS("General")));

			accountsPage = new PrefsAccountsPage(this);
			PrefNoteBook.AppendPage( accountsPage,
										new Label(Util.GS("Accounts")));

	              /*migrationPage =*/ new MigrationPage(this, ifws); 
//			PrefNoteBook.AppendPage( migrationPage, new Label(Util.GS("Migration")));

			PrefNoteBook.SwitchPage +=
				new SwitchPageHandler(OnSwitchPageEvent);

			winBox.PackStart(PrefNoteBook, true, true, 0);

			HButtonBox buttonBox = new HButtonBox();
			buttonBox.BorderWidth = 10;
			buttonBox.Spacing = 10;
			buttonBox.Layout = ButtonBoxStyle.Edge;
			winBox.PackStart(buttonBox, false, false, 0);

			Button helpButton = new Button(Gtk.Stock.Help);
			buttonBox.PackStart(helpButton);
			helpButton.Clicked += new EventHandler(HelpEventHandler);

			Button closeButton = new Button(Gtk.Stock.Close);
			buttonBox.PackStart(closeButton);
			closeButton.Clicked += new EventHandler(CloseEventHandler);
		}

		void KeyPressHandler(object o, KeyPressEventArgs args)
		{
			args.RetVal = true;
			
			switch(args.Event.Key)
			{
				case Gdk.Key.Escape:
					CloseWindow();
					break;
				case Gdk.Key.Control_L:
				case Gdk.Key.Control_R:
					ControlKeyPressed = true;
					args.RetVal = false;
					break;
				case Gdk.Key.W:
				case Gdk.Key.w:
					if (ControlKeyPressed)
						CloseWindow();
					else
						args.RetVal = false;
					break;
				default:
					args.RetVal = false;
					break;
			}
		}
		
        /// <summary>
        /// Event Handler for Key Release handler event
        /// </summary>
        void KeyReleaseHandler(object o, KeyReleaseEventArgs args)
		{
			args.RetVal = false;
			
			switch(args.Event.Key)
			{
				case Gdk.Key.Control_L:
				case Gdk.Key.Control_R:
					ControlKeyPressed = false;
					break;
				default:
					break;
			}
		}
		
        /// <summary>
        /// Event Handler for Switch Page Event
        /// </summary>
        private void OnSwitchPageEvent(object o, SwitchPageArgs args)
		{
			if (args.PageNum != 0)
				generalPage.LeavingGeneralPage();
/*
			else
			{
				PrefNoteBook.SwitchPage -=
					new SwitchPageHandler(OnSwitchPageEvent);

				CurrentPage = 1;

				if (accountsPage.AllowLeavingAccountsPage())
					CurrentPage = 0;

				PrefNoteBook.SwitchPage +=
					new SwitchPageHandler(OnSwitchPageEvent);
			}
*/
		}

        /// <summary>
        /// Event Handler for Help
        /// </summary>
        private void HelpEventHandler(object o, EventArgs args)
		{
			if (CurrentPage == 0)
			{
				Util.ShowHelp("preferences.html", this);
			}
			else if (CurrentPage == 1)
			{
				Util.ShowHelp("accounts.html", this);
			}
			else if (CurrentPage == 2)
			{
				Util.ShowHelp("settings.html", this);
			}
			else
			{
				Util.ShowHelp(Util.HelpMainPage, this);
			}
		}

		private void CloseEventHandler(object o, EventArgs args)
		{
//			if (CurrentPage == 1)
//				if (!accountsPage.AllowLeavingAccountsPage())
//					return;

			CloseWindow();
		}

        /// <summary>
        /// Close Window
        /// </summary>
		private void CloseWindow()
		{
			if (CurrentPage == 0)
				generalPage.LeavingGeneralPage();

			this.Hide();
			this.Destroy();
		}

		/// <summary>
		/// This should be called anytime the authentication status of a domain
		/// changes.  This will allow the domain status on the Accounts tab to
		/// be updated.
		/// </summary>
		public void UpdateDomainStatus(string domainID)
		{
			accountsPage.UpdateDomainStatus(domainID);
		}

/*		public PrefsAccountsPage GetInstance()
                {
			if(accountsPage != null )
                        	return accountsPage;
			return null;
                } */


		public void ToggelDomain(DomainInformation domainInfo,bool flag)
		{	
			accountsPage.ToggelDomainState(domainInfo, flag);
		}

		public void ShowClientUpgradeMessage()
		{
			accountsPage. ShowClientUpgradeMessageBox();
		}

	}
}
