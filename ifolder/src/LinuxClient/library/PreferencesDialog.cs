/***********************************************************************
 *  $RCSfile$
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


using System;
using System.IO;
using System.Collections;
using Gtk;
using Simias.Client.Event;

namespace Novell.iFolder
{

	/// <summary>
	/// This is the main iFolder Window.  This window implements all of the
	/// client code for iFolder.
	/// </summary>
	public class PreferencesDialog : Dialog
	{
		private iFolderWebService		ifws;

		private Gtk.Notebook			PrefNoteBook;

		private Gtk.CheckButton			AutoSyncCheckButton;
		private Gtk.SpinButton			SyncSpinButton;
		private Gtk.Label				SyncUnitsLabel;

		private Gtk.CheckButton			ShowConfirmationButton; 
		private Gtk.CheckButton			NotifyUsersButton; 
		private Gtk.CheckButton			NotifyCollisionsButton; 
		private Gtk.CheckButton			NotifyiFoldersButton; 
		private Gtk.CheckButton			UseProxyButton; 
		private Gtk.Entry				ProxyHostEntry;
		private Gtk.SpinButton			ProxyPortSpinButton;
		private Gtk.Label				ProxyHostLabel;
		private Gtk.Label				ProxyPortLabel;


		/// <summary>
		/// Default constructor for iFolderWindow
		/// </summary>
		public PreferencesDialog(iFolderWebService webService)
			: base()
		{
			if(webService == null)
				throw new ApplicationException("iFolderWebServices was null");

			ifws = webService;

			this.HasSeparator = false;
//			this.Modal = false;
			this.Title = Util.GS("iFolder Preferences");

			CreateWidgets();
			PopulateWidgets();
		}




		/// <summary>
		/// Setup the UI inside the Window
		/// </summary>
		private void CreateWidgets()
		{
//			this.SetDefaultSize (400, 480);
//			this.DeleteEvent += new DeleteEventHandler (WindowDelete);
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder.png"));
			this.WindowPosition = Gtk.WindowPosition.Center;

			//-----------------------------
			// Setup the Notebook (tabs)
			//-----------------------------
			PrefNoteBook = new Notebook();

			VBox vbox = new VBox();
			Image genImage = new Image(new Gdk.Pixbuf(
							Util.ImagesPath("prefs-general24.png") ));
			Label genLabel = new Label( Util.GS("_General") );
			vbox.PackStart(genImage, true, true, 0);
			vbox.PackStart(genLabel, false, false, 0);

			PrefNoteBook.AppendPage( CreateGeneralPage(), vbox);
										//new Label(Util.GS("_General")));

			vbox.ShowAll();

			vbox = new VBox();
			Image accImage = new Image(new Gdk.Pixbuf(
							Util.ImagesPath("prefs-accounts24.png") ));
			Label accLabel = new Label( Util.GS("_Accounts") );
			vbox.PackStart(accImage, true, true, 0);
			vbox.PackStart(accLabel, false, false, 0);

			PrefNoteBook.AppendPage( CreateAccountsPage(), vbox);
									//	new Label(Util.GS("_Accounts")));
			vbox.ShowAll();

			this.VBox.PackStart(PrefNoteBook, true, true, 0);

			this.Realized += new EventHandler(OnRealizeWidget);

			this.AddButton(Stock.Close, ResponseType.Ok);
			this.AddButton(Stock.Help, ResponseType.Help);
			this.Response += 
					new ResponseHandler(DialogResponseHandler);
		}




		private void DialogResponseHandler(object o, ResponseArgs args)
		{
			switch(args.ResponseId)
			{
				case Gtk.ResponseType.Help:
					Util.ShowHelp("front.html", this);
					break;
				default:
				{
					this.Hide();
					this.Destroy();
					break;
				}
			}
		}




		/// <summary>
		/// Set the Values in the Widgets
		/// </summary>
		private void PopulateWidgets()
		{
			//------------------------------
			// Setup all of the default values
			//------------------------------
			if(ClientConfig.Get(ClientConfig.KEY_SHOW_CREATION, "true")
										== "true")
				ShowConfirmationButton.Active = true;
			else
				ShowConfirmationButton.Active = false;

			if(ClientConfig.Get(ClientConfig.KEY_NOTIFY_USERS, "true")
										== "true")
				NotifyUsersButton.Active = true;
			else
				NotifyUsersButton.Active = false;

			if(ClientConfig.Get(ClientConfig.KEY_NOTIFY_COLLISIONS, "true")
										== "true")
				NotifyCollisionsButton.Active = true;
			else
				NotifyCollisionsButton.Active = false;

			if(ClientConfig.Get(ClientConfig.KEY_NOTIFY_IFOLDERS, "true")
										== "true")
				NotifyiFoldersButton.Active = true;
			else
				NotifyiFoldersButton.Active = false;


//			SyncSpinButton.Value = ifSettings.DefaultSyncInterval;
			SyncSpinButton.Value = 0;

			if(SyncSpinButton.Value == 0)
			{
				AutoSyncCheckButton.Active = false;
				SyncSpinButton.Sensitive = false;
				SyncUnitsLabel.Sensitive = false; 
			}
			else
			{
				AutoSyncCheckButton.Active = true;
				SyncSpinButton.Sensitive = true;
				SyncUnitsLabel.Sensitive = true;
			}

			if(AutoSyncCheckButton.Active == true)
			{
				SyncSpinButton.Sensitive = true;
				SyncUnitsLabel.Sensitive = true;
			}
			else
			{
				SyncSpinButton.Sensitive = false;
				SyncUnitsLabel.Sensitive = false;
			}


/*			if(ifSettings.UseProxy)
			{
				ProxyHostEntry.Sensitive = true;
				ProxyPortSpinButton.Sensitive = true;
				ProxyHostLabel.Sensitive = true;
				ProxyPortLabel.Sensitive = true;
				UseProxyButton.Active = true; 
				ProxyHostEntry.Text = ifSettings.ProxyHost;
				ProxyPortSpinButton.Value = ifSettings.ProxyPort;
			}
			else
			{
				ProxyHostEntry.Sensitive = false;
				ProxyPortSpinButton.Sensitive = false;
				ProxyHostLabel.Sensitive = false;
				ProxyPortLabel.Sensitive = false;
				UseProxyButton.Active = false; 
			}
*/
		}




		/// <summary>
		/// Creates the General Page
		/// </summary>
		/// <returns>
		/// Widget to display
		/// </returns>
		private Widget CreateGeneralPage()
		{
			// Create a new VBox and place 10 pixels between
			// each item in the vBox
			VBox vbox = new VBox();
			vbox.Spacing = Util.SectionSpacing;
			vbox.BorderWidth = Util.DefaultBorderWidth;

			//------------------------------
			// Application Settings
			//------------------------------
			// create a section box
			VBox appSectionBox = new VBox();
			appSectionBox.Spacing = Util.SectionTitleSpacing;
			vbox.PackStart(appSectionBox, false, true, 0);
			Label appSectionLabel = new Label("<span weight=\"bold\">" +
												Util.GS("Application") +
												"</span>");
			appSectionLabel.UseMarkup = true;
			appSectionLabel.Xalign = 0;
			appSectionBox.PackStart(appSectionLabel, false, true, 0);

			// create a hbox to provide spacing
			HBox appSpacerBox = new HBox();
			appSectionBox.PackStart(appSpacerBox, false, true, 0);
			Label appSpaceLabel = new Label("    "); // four spaces
			appSpacerBox.PackStart(appSpaceLabel, false, true, 0);

			// create a vbox to actually place the widgets in for section
			VBox appWidgetBox = new VBox();
			appSpacerBox.PackStart(appWidgetBox, false, true, 0);
			appWidgetBox.Spacing = Util.SectionTitleSpacing;


			ShowConfirmationButton = 
				new CheckButton(Util.GS(
					"_Show Confirmation dialog when creating iFolders"));
			appWidgetBox.PackStart(ShowConfirmationButton, false, true, 0);
			ShowConfirmationButton.Toggled += 
						new EventHandler(OnShowConfButton);

			NotifyiFoldersButton =
				new CheckButton(Util.GS("_Notify of shared iFolders")); 
			appWidgetBox.PackStart(NotifyiFoldersButton, false, true, 0);

			NotifyiFoldersButton.Toggled += 
						new EventHandler(OnNotifyiFoldersButton);

			NotifyCollisionsButton =
				new CheckButton(Util.GS("Notify of _collisions")); 
			appWidgetBox.PackStart(NotifyCollisionsButton, false, true, 0);
			NotifyCollisionsButton.Toggled += 
						new EventHandler(OnNotifyCollisionsButton);

			NotifyUsersButton =
				new CheckButton(Util.GS("Notify when a _user joins")); 
			appWidgetBox.PackStart(NotifyUsersButton, false, true, 0);

			NotifyUsersButton.Toggled += 
						new EventHandler(OnNotifyUsersButton);

			
			Label strtlabel = new Label("<span style=\"italic\">" + Util.GS("To startup iFolder at login, leave iFolder running when you log out and save your current setup.") + "</span>");
			strtlabel.UseMarkup = true;
			strtlabel.LineWrap = true;
			appWidgetBox.PackStart(strtlabel, false, true, 0);



			//------------------------------
			// Sync Settings
			//------------------------------
			// create a section box
			VBox syncSectionBox = new VBox();
			syncSectionBox.Spacing = Util.SectionTitleSpacing;
			vbox.PackStart(syncSectionBox, false, true, 0);
			Label syncSectionLabel = new Label("<span weight=\"bold\">" +
												Util.GS("Synchronization") +
												"</span>");
			syncSectionLabel.UseMarkup = true;
			syncSectionLabel.Xalign = 0;
			syncSectionBox.PackStart(syncSectionLabel, false, true, 0);

			// create a hbox to provide spacing
			HBox syncSpacerBox = new HBox();
			syncSectionBox.PackStart(syncSpacerBox, false, true, 0);
			Label syncSpaceLabel = new Label("    "); // four spaces
			syncSpacerBox.PackStart(syncSpaceLabel, false, true, 0);

			// create a vbox to actually place the widgets in for section
			VBox syncWidgetBox = new VBox();
			syncSpacerBox.PackStart(syncWidgetBox, false, true, 0);
			syncWidgetBox.Spacing = 10;


			Label syncHelpLabel = new Label(Util.GS("Specify the default Sync interval for synchronizing your iFolders with the host.  To specify a different Sync interval for an individual iFolder, use the iFolder's Properties dialog."));
			syncHelpLabel.LineWrap = true;
			syncHelpLabel.Xalign = 0;
			syncWidgetBox.PackStart(syncHelpLabel, false, true, 0);

			HBox syncHBox = new HBox();
			syncWidgetBox.PackStart(syncHBox, false, true, 0);
			syncHBox.Spacing = 10;
			AutoSyncCheckButton = 
					new CheckButton(Util.GS("Sync to host _every:"));
			AutoSyncCheckButton.Toggled += new EventHandler(OnAutoSyncButton);
			syncHBox.PackStart(AutoSyncCheckButton, false, false, 0);
			SyncSpinButton = new SpinButton(0, 99999, 5);
			SyncSpinButton.ValueChanged += 
					new EventHandler(OnSyncIntervalChanged);

			syncHBox.PackStart(SyncSpinButton, false, false, 0);
			SyncUnitsLabel = new Label(Util.GS("seconds"));
			SyncUnitsLabel.Xalign = 0;
			syncHBox.PackEnd(SyncUnitsLabel, true, true, 0);



			//------------------------------
			// Proxy Frame
			//------------------------------
			// create a section box
			VBox proxySectionBox = new VBox();
			proxySectionBox.Spacing = Util.SectionTitleSpacing;
			vbox.PackStart(proxySectionBox, true, true, 0);
			Label proxySectionLabel = new Label("<span weight=\"bold\">" +
												Util.GS("Proxy") +
												"</span>");
			proxySectionLabel.UseMarkup = true;
			proxySectionLabel.Xalign = 0;
			proxySectionBox.PackStart(proxySectionLabel, false, true, 0);

			// create a hbox to provide spacing
			HBox proxySpacerBox = new HBox();
			proxySectionBox.PackStart(proxySpacerBox, false, true, 0);
			Label proxySpaceLabel = new Label("    "); // four spaces
			proxySpacerBox.PackStart(proxySpaceLabel, false, true, 0);

			// create a vbox to actually place the widgets in for section
			VBox proxyWidgetBox = new VBox();
			proxySpacerBox.PackStart(proxyWidgetBox, true, true, 0);
			proxyWidgetBox.Spacing = 5;


			UseProxyButton = 
				new CheckButton(Util.GS("Use this proxy server to sync iFolders with the host"));
			proxyWidgetBox.PackStart(UseProxyButton, false, true, 0);
			UseProxyButton.Toggled += new EventHandler(OnUseProxyButton);


			HBox pSettingBox = new HBox();
			pSettingBox.Spacing = 10;
			proxyWidgetBox.PackStart(pSettingBox, true, true, 0);

			ProxyHostLabel = new Label(Util.GS("Proxy host:"));
			pSettingBox.PackStart(ProxyHostLabel, false, true, 0);
			ProxyHostEntry = new Entry();
			ProxyHostEntry.Changed += new EventHandler(OnProxySettingsChanged);

			pSettingBox.PackStart(ProxyHostEntry, true, true, 0);
			ProxyPortLabel = new Label(Util.GS("Port:"));
			pSettingBox.PackStart(ProxyPortLabel, false, true, 0);
			ProxyPortSpinButton = new SpinButton(0, 99999, 1);

			ProxyPortSpinButton.ValueChanged += 
					new EventHandler(OnProxySettingsChanged);
			pSettingBox.PackStart(ProxyPortSpinButton, false, true, 0);


			// Disable all proxy stuff right now
			proxySectionLabel.Sensitive = false;
			UseProxyButton.Sensitive = false;
			ProxyHostLabel.Sensitive = false;
			ProxyHostEntry.Sensitive = false;
			ProxyPortSpinButton.Sensitive = false;
			ProxyPortLabel.Sensitive = false;

			return vbox;
		}




		/// <summary>
		/// Creates the Accounts Page
		/// </summary>
		/// <returns>
		/// Widget to display
		/// </returns>
		private Widget CreateAccountsPage()
		{
			// Create a new VBox and place 10 pixels between
			// each item in the vBox
			VBox vbox = new VBox();
			vbox.Spacing = Util.SectionSpacing;
			vbox.BorderWidth = Util.DefaultBorderWidth;


			Label tmpLabel = new Label("<span weight=\"bold\">" +
												Util.GS("Account stuff") +
												"</span>");

			vbox.PackStart(tmpLabel, false, true, 0);

			return vbox;
		}




		private void OnRealizeWidget(object o, EventArgs args)
		{
//			RefreshiFolderTreeView(o, args);
		}



		private void CloseEventHandler(object o, EventArgs args)
		{
			CloseWindow();
		}



		private void CloseWindow()
		{
			this.Hide();
			this.Destroy();
		}



		private void OnUseProxyButton(object o, EventArgs args)
		{
			if(UseProxyButton.Active == true)
			{
				ProxyHostEntry.Sensitive = true;
				ProxyPortSpinButton.Sensitive = true;
				ProxyHostLabel.Sensitive = true;
				ProxyPortLabel.Sensitive = true;
			}
			else
			{
				ProxyHostEntry.Sensitive = false;
				ProxyPortSpinButton.Sensitive = false;
				ProxyHostLabel.Sensitive = false;
				ProxyPortLabel.Sensitive = false;
			}
		}


		private void OnNotifyUsersButton(object o, EventArgs args)
		{
			if(NotifyUsersButton.Active)
				ClientConfig.Set(ClientConfig.KEY_NOTIFY_USERS, "true");
			else
				ClientConfig.Set(ClientConfig.KEY_NOTIFY_USERS, "false");
		}

		private void OnNotifyCollisionsButton(object o, EventArgs args)
		{
			if(NotifyCollisionsButton.Active)
				ClientConfig.Set(ClientConfig.KEY_NOTIFY_COLLISIONS, "true");
			else
				ClientConfig.Set(ClientConfig.KEY_NOTIFY_COLLISIONS, "false");
		}

		private void OnNotifyiFoldersButton(object o, EventArgs args)
		{
			if(NotifyiFoldersButton.Active)
				ClientConfig.Set(ClientConfig.KEY_NOTIFY_IFOLDERS, "true");
			else
				ClientConfig.Set(ClientConfig.KEY_NOTIFY_IFOLDERS, "false");
		}


		private void OnShowConfButton(object o, EventArgs args)
		{
			if(ShowConfirmationButton.Active)
				ClientConfig.Set(ClientConfig.KEY_SHOW_CREATION, "true");
			else
				ClientConfig.Set(ClientConfig.KEY_SHOW_CREATION, "false");
		}




		private void OnProxySettingsChanged(object o, EventArgs args)
		{
			Console.WriteLine("Save ProxySettings here");
			// Save the settings here?
		}




		private void OnAutoSyncButton(object o, EventArgs args)
		{
			if(AutoSyncCheckButton.Active == true)
			{
				SyncSpinButton.Sensitive = true;
				SyncUnitsLabel.Sensitive = true;
			}
			else
			{
				SyncSpinButton.Sensitive = false;
				SyncUnitsLabel.Sensitive = false;
				SyncSpinButton.Value = 0;

/*				try
				{
					ifSettings.DefaultSyncInterval = (int)SyncSpinButton.Value;
					ifws.SetDefaultSyncInterval(
									ifSettings.DefaultSyncInterval);
				}
				catch(Exception e)
				{
					iFolderExceptionDialog ied = new iFolderExceptionDialog(
													this, e);
					ied.Run();
					ied.Hide();
					ied.Destroy();
					return;
				}
*/
			}
		}




		private void OnSyncIntervalChanged(object o, EventArgs args)
		{
/*			if(SyncSpinButton.Value != ifSettings.DefaultSyncInterval)
			{
				try
				{
					ifSettings.DefaultSyncInterval = (int)SyncSpinButton.Value;
					ifws.SetDefaultSyncInterval(
										ifSettings.DefaultSyncInterval);
				}
				catch(Exception e)
				{
					iFolderExceptionDialog ied = new iFolderExceptionDialog(
														this, e);
					ied.Run();
					ied.Hide();
					ied.Destroy();
					return;
				}
			}
*/
		}




		public void HelpEventHandler(object o, EventArgs args)
		{
			Util.ShowHelp("front.html", this);
		}


	}
}
