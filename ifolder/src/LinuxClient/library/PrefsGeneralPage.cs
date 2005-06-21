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
	public class PrefsGeneralPage : VBox
	{
		private Gtk.Window				topLevelWindow;
		private iFolderWebService		ifws;

		private Gtk.CheckButton			AutoSyncCheckButton;
		private Gtk.SpinButton			SyncSpinButton;
		private Gtk.Label				SyncUnitsLabel;

		private Gtk.CheckButton			ShowConfirmationButton; 
		private Gtk.CheckButton			NotifyUsersButton; 
		private Gtk.CheckButton			NotifyCollisionsButton; 
		private Gtk.CheckButton			NotifyiFoldersButton; 
		private int						lastSyncInterval;


		/// <summary>
		/// Default constructor for iFolderPropSharingPage
		/// </summary>
		public PrefsGeneralPage(	Gtk.Window topWindow,
									iFolderWebService webService)
			: base()
		{
			this.topLevelWindow = topWindow;
			this.ifws = webService;
			InitializeWidgets();
			this.Realized += new EventHandler(OnRealizeWidget);
		}



		/// <summary>
		/// Set up the widgets
		/// </summary>
		/// <returns>
		/// Widget to display
		/// </returns>
		private void InitializeWidgets()
		{
			this.Spacing = Util.SectionSpacing;
			this.BorderWidth = Util.DefaultBorderWidth;

			//------------------------------
			// Application Settings
			//------------------------------
			// create a section box
			VBox appSectionBox = new VBox();
			appSectionBox.Spacing = Util.SectionTitleSpacing;
			this.PackStart(appSectionBox, false, true, 0);
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

			Label strtlabel = new Label("<span style=\"italic\">" + Util.GS("To start up iFolder at login, leave iFolder running when you log out and save your current setup.") + "</span>");
			strtlabel.UseMarkup = true;
			strtlabel.LineWrap = true;
			appWidgetBox.PackStart(strtlabel, false, true, 0);



			//------------------------------
			// Notifications
			//------------------------------
			// create a section box
			VBox notifySectionBox = new VBox();
			notifySectionBox.Spacing = Util.SectionTitleSpacing;
			this.PackStart(notifySectionBox, false, true, 0);
			Label notifySectionLabel = new Label("<span weight=\"bold\">" +
												Util.GS("Notification") +
												"</span>");
			notifySectionLabel.UseMarkup = true;
			notifySectionLabel.Xalign = 0;
			notifySectionBox.PackStart(notifySectionLabel, false, true, 0);

			// create a hbox to provide spacing
			HBox notifySpacerBox = new HBox();
			notifySectionBox.PackStart(notifySpacerBox, false, true, 0);
			Label notifySpaceLabel = new Label("    "); // four spaces
			notifySpacerBox.PackStart(notifySpaceLabel, false, true, 0);

			// create a vbox to actually place the widgets in for section
			VBox notifyWidgetBox = new VBox();
			notifySpacerBox.PackStart(notifyWidgetBox, true, true, 0);
			notifyWidgetBox.Spacing = 5;




			NotifyiFoldersButton =
				new CheckButton(Util.GS("_Notify of shared iFolders")); 
			notifyWidgetBox.PackStart(NotifyiFoldersButton, false, true, 0);

			NotifyiFoldersButton.Toggled += 
						new EventHandler(OnNotifyiFoldersButton);

			NotifyCollisionsButton =
				new CheckButton(Util.GS("Notify of co_llisions")); 
			notifyWidgetBox.PackStart(NotifyCollisionsButton, false, true, 0);
			NotifyCollisionsButton.Toggled += 
						new EventHandler(OnNotifyCollisionsButton);

			NotifyUsersButton =
				new CheckButton(Util.GS("Notify when a _user joins")); 
			notifyWidgetBox.PackStart(NotifyUsersButton, false, true, 0);

			NotifyUsersButton.Toggled += 
						new EventHandler(OnNotifyUsersButton);

			

			//------------------------------
			// Sync Settings
			//------------------------------
			// create a section box
			VBox syncSectionBox = new VBox();
			syncSectionBox.Spacing = Util.SectionTitleSpacing;
			this.PackStart(syncSectionBox, false, true, 0);
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


			HBox syncHBox = new HBox();
			syncWidgetBox.PackStart(syncHBox, false, true, 0);
			syncHBox.Spacing = 10;
			AutoSyncCheckButton = 
					new CheckButton(Util.GS("Synchronize my iFolders _every:"));
			syncHBox.PackStart(AutoSyncCheckButton, false, false, 0);
			SyncSpinButton = new SpinButton(0, 99999, 5);

			syncHBox.PackStart(SyncSpinButton, false, false, 0);
			SyncUnitsLabel = new Label(Util.GS("seconds"));
			SyncUnitsLabel.Xalign = 0;
			syncHBox.PackEnd(SyncUnitsLabel, true, true, 0);
		}




		/// <summary>
		/// Set the Values in the Widgets
		/// </summary>
		private void PopulateWidgets()
		{
			//------------------------------
			// Set up all of the default values
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

			try
			{
				lastSyncInterval = ifws.GetDefaultSyncInterval();
				if(lastSyncInterval == -1)
					SyncSpinButton.Value = 0;
				else
					SyncSpinButton.Value = lastSyncInterval;
			}
			catch(Exception e)
			{
				lastSyncInterval = -1;
				SyncSpinButton.Value = 0;
			}

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

			AutoSyncCheckButton.Toggled += new EventHandler(OnAutoSyncButton);
			SyncSpinButton.ValueChanged += 
					new EventHandler(OnSyncIntervalChanged);
		}




		private void OnRealizeWidget(object o, EventArgs args)
		{
			PopulateWidgets();
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




		private void OnAutoSyncButton(object o, EventArgs args)
		{
			if(AutoSyncCheckButton.Active == true)
			{
				SyncSpinButton.Sensitive = true;
				SyncUnitsLabel.Sensitive = true;
				SyncSpinButton.Value = 60;
			}
			else
			{
				SyncSpinButton.Sensitive = false;
				SyncUnitsLabel.Sensitive = false;
				SyncSpinButton.Value = 0;
			}
		}




		private void OnSyncIntervalChanged(object o, EventArgs args)
		{
			if(SyncSpinButton.Value != lastSyncInterval)
			{
				try
				{
					lastSyncInterval = (int)SyncSpinButton.Value;
					if(lastSyncInterval == 0)
						ifws.SetDefaultSyncInterval(-1);
					else
						ifws.SetDefaultSyncInterval(lastSyncInterval);
				}
				catch(Exception e)
				{
					iFolderExceptionDialog ied = new iFolderExceptionDialog(
														topLevelWindow, e);
					ied.Run();
					ied.Hide();
					ied.Destroy();
					return;
				}
			}

		}
	}
}
