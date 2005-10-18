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
		private Gtk.OptionMenu			SyncUnitsOptionMenu;

		private Gtk.CheckButton			ShowConfirmationButton; 
		private Gtk.CheckButton			NotifyUsersButton; 
		private Gtk.CheckButton			NotifyCollisionsButton; 
		private Gtk.CheckButton			NotifyiFoldersButton; 
//		private Gtk.CheckButton			NotifySyncErrorsButton;
		private int						lastSyncInterval;
		private SyncUnit				currentSyncUnit;

		private NotifyWindow			oneMinuteLimitNotifyWindow;

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
		
		
		
		public void LeavingGeneralPage()
		{
			if (oneMinuteLimitNotifyWindow != null)
			{
				oneMinuteLimitNotifyWindow.Hide();
				oneMinuteLimitNotifyWindow.Destroy();
				oneMinuteLimitNotifyWindow = null;
			}
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
				new CheckButton(Util.GS("Notify of conflic_ts")); 
			notifyWidgetBox.PackStart(NotifyCollisionsButton, false, true, 0);
			NotifyCollisionsButton.Toggled += 
						new EventHandler(OnNotifyCollisionsButton);

			NotifyUsersButton =
				new CheckButton(Util.GS("Notify when a _user joins")); 
			notifyWidgetBox.PackStart(NotifyUsersButton, false, true, 0);

			NotifyUsersButton.Toggled += 
						new EventHandler(OnNotifyUsersButton);
			
//			NotifySyncErrorsButton =
//				new CheckButton(Util.GS("Notify of _synchronization errors"));
//			notifyWidgetBox.PackStart(NotifySyncErrorsButton, false, true, 0);
//			NotifySyncErrorsButton.Toggled +=
//						new EventHandler(OnNotifySyncErrorsButton);

			

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

			HBox syncHBox0 = new HBox();
			syncWidgetBox.PackStart(syncHBox0, false, true, 0);
			syncHBox0.Spacing = 10;
			AutoSyncCheckButton =
				new CheckButton(Util.GS("Automatically S_ynchronize iFolders"));
			syncHBox0.PackStart(AutoSyncCheckButton, false, false, 0);

			HBox syncHBox = new HBox();
			syncHBox.Spacing = 10;
			syncWidgetBox.PackStart(syncHBox, true, true, 0);

			Label spacerLabel = new Label("  ");
			syncHBox.PackStart(spacerLabel, true, true, 0);

			Label syncEveryLabel = new Label(Util.GS("Synchronize iFolders Every"));
			syncEveryLabel.Xalign = 1;
			syncHBox.PackStart(syncEveryLabel, false, false, 0);
			
			SyncSpinButton = new SpinButton(1, Int32.MaxValue, 1);

			syncHBox.PackStart(SyncSpinButton, false, false, 0);

			SyncUnitsOptionMenu = new OptionMenu();
			syncHBox.PackStart(SyncUnitsOptionMenu, false, false, 0);

			Menu m = new Menu();
			m.Append(new MenuItem(Util.GS("seconds")));
			m.Append(new MenuItem(Util.GS("minutes")));
			m.Append(new MenuItem(Util.GS("hours")));
			m.Append(new MenuItem(Util.GS("days")));
			
			SyncUnitsOptionMenu.Menu = m;
			SyncUnitsOptionMenu.SetHistory((int)SyncUnit.Minutes);
			currentSyncUnit = SyncUnit.Minutes;
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

//			if(ClientConfig.Get(ClientConfig.KEY_NOTIFY_SYNC_ERRORS, "true")
//										== "true")
//				NotifySyncErrorsButton.Active = true;
//			else
//				NotifySyncErrorsButton.Active = false;

			try
			{
				lastSyncInterval = ifws.GetDefaultSyncInterval();
				if(lastSyncInterval == -1)
					SyncSpinButton.Value = 0;
				else
				{
					string syncUnitString =
						ClientConfig.Get(ClientConfig.KEY_SYNC_UNIT, "Minutes");
					switch (syncUnitString)
					{
						case "Seconds":
							currentSyncUnit = SyncUnit.Seconds;
							SyncUnitsOptionMenu.SetHistory((int)SyncUnit.Seconds);

							// Prevent the user from setting a sync interval less than
							// one minute.
							SyncSpinButton.Adjustment.Lower = 60;
							break;
						case "Minutes":
							currentSyncUnit = SyncUnit.Minutes;
							SyncUnitsOptionMenu.SetHistory((int)SyncUnit.Minutes);
							break;
						case "Hours":
							currentSyncUnit = SyncUnit.Hours;
							SyncUnitsOptionMenu.SetHistory((int)SyncUnit.Hours);
							break;
						case "Days":
							currentSyncUnit = SyncUnit.Days;
							SyncUnitsOptionMenu.SetHistory((int)SyncUnit.Days);
							break;
						default:
							break;
					}

					SyncSpinButton.Value = CalculateSyncSpinValue(lastSyncInterval, currentSyncUnit);
				}
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
				SyncUnitsOptionMenu.Sensitive = false;
			}
			else
			{
				AutoSyncCheckButton.Active = true;
				SyncSpinButton.Sensitive = true;
				SyncUnitsOptionMenu.Sensitive = true;
			}

			AutoSyncCheckButton.Toggled += new EventHandler(OnAutoSyncButton);
			SyncSpinButton.ValueChanged += 
					new EventHandler(OnSyncIntervalChanged);
			SyncUnitsOptionMenu.Changed +=
					new EventHandler(OnSyncUnitsChanged);
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


//		private void OnNotifySyncErrorsButton(object o, EventArgs args)
//		{
//			if (NotifySyncErrorsButton.Active)
//				ClientConfig.Set(ClientConfig.KEY_NOTIFY_SYNC_ERRORS, "true");
//			else
//				ClientConfig.Set(ClientConfig.KEY_NOTIFY_SYNC_ERRORS, "false");
//		}

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
				SyncUnitsOptionMenu.Sensitive = true;

				SyncUnitsOptionMenu.Changed -= new EventHandler(OnSyncUnitsChanged);
				SyncUnitsOptionMenu.SetHistory((int)SyncUnit.Minutes);
				currentSyncUnit = SyncUnit.Minutes;
				SaveSyncUnitConfig();
				SyncUnitsOptionMenu.Changed += new EventHandler(OnSyncUnitsChanged);

				SyncSpinButton.Value = 5;
			}
			else
			{
				if (oneMinuteLimitNotifyWindow != null)
				{
					oneMinuteLimitNotifyWindow.Hide();
					oneMinuteLimitNotifyWindow.Destroy();
					oneMinuteLimitNotifyWindow = null;
				}
	
				SyncSpinButton.Sensitive = false;
				SyncUnitsOptionMenu.Sensitive = false;
				SyncSpinButton.Value = 0;
			}
		}




		private void OnSyncIntervalChanged(object o, EventArgs args)
		{
			if (oneMinuteLimitNotifyWindow != null)
			{
				oneMinuteLimitNotifyWindow.Hide();
				oneMinuteLimitNotifyWindow.Destroy();
				oneMinuteLimitNotifyWindow = null;
			}

			int syncSpinValue =
				CalculateActualSyncInterval((int)SyncSpinButton.Value,
											currentSyncUnit);
		
			try
			{
				lastSyncInterval = syncSpinValue;
				if(lastSyncInterval == 0)
				{
					ifws.SetDefaultSyncInterval(-1);
				}
				else
				{
					ifws.SetDefaultSyncInterval(lastSyncInterval);
				}
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
		
		private void OnSyncUnitsChanged(object o, EventArgs args)
		{
			if (oneMinuteLimitNotifyWindow != null)
			{
				oneMinuteLimitNotifyWindow.Hide();
				oneMinuteLimitNotifyWindow.Destroy();
				oneMinuteLimitNotifyWindow = null;
			}

			int syncSpinValue = (int)SyncSpinButton.Value;
											
			currentSyncUnit = (SyncUnit)SyncUnitsOptionMenu.History;

			if (currentSyncUnit == SyncUnit.Seconds)
			{
				// Prevent the user from setting a sync interval less than
				// one minute.
				SyncSpinButton.Adjustment.Lower = 60;

				if (syncSpinValue < 60)
				{
					oneMinuteLimitNotifyWindow =
						new NotifyWindow(
						SyncSpinButton, Util.GS("Synchronization Interval Limit"),
						Util.GS("The synchronization interval cannot be set to less than one minute.  It was automatically changed to 60 seconds."),
						Gtk.MessageType.Info, 10000);
					oneMinuteLimitNotifyWindow.ShowAll();

					SyncSpinButton.ValueChanged -= 
						new EventHandler(OnSyncIntervalChanged);
					SyncSpinButton.Value = 60;
					syncSpinValue = 60;
					SyncSpinButton.ValueChanged += 
						new EventHandler(OnSyncIntervalChanged);
				}
			}
			else
			{
				SyncSpinButton.Adjustment.Lower = 1;
			}

			int syncInterval =
				CalculateActualSyncInterval(syncSpinValue,
											currentSyncUnit);

			try
			{
				lastSyncInterval = syncInterval;
				if(lastSyncInterval == 0)
				{
					ifws.SetDefaultSyncInterval(-1);
				}
				else
				{
					ifws.SetDefaultSyncInterval(lastSyncInterval);
				}

				SaveSyncUnitConfig();
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

		private int CalculateSyncSpinValue(int syncInterval, SyncUnit syncUnit)
		{
			int convertedInterval;

			switch (syncUnit)
			{
				case SyncUnit.Seconds:
					convertedInterval = syncInterval; // No conversion
					break;
				case SyncUnit.Minutes:
					convertedInterval = syncInterval / 60;
					break;
				case SyncUnit.Hours:
					convertedInterval = syncInterval / 60 / 60;
					break;
				case SyncUnit.Days:
					convertedInterval = syncInterval / 60 / 60 / 24;
					break;
				default:
					convertedInterval = 0;
					break;
			}

			return convertedInterval;
		}
		
		private int CalculateActualSyncInterval(int spinValue, SyncUnit syncUnit)
		{
			int actualSyncInterval;
			
			switch (syncUnit)
			{
				case SyncUnit.Seconds:
					actualSyncInterval = spinValue; // No conversion
					break;
				case SyncUnit.Minutes:
					actualSyncInterval = spinValue * 60;
					break;
				case SyncUnit.Hours:
					actualSyncInterval = spinValue * 60 * 60;
					break;
				case SyncUnit.Days:
					actualSyncInterval = spinValue * 60 * 60 * 24;
					break;
				default:
					actualSyncInterval = 0;
					break;
			}

			return actualSyncInterval;
		}
		
		private void SaveSyncUnitConfig()
		{
			string syncUnitString;
			switch (currentSyncUnit)
			{
				case SyncUnit.Seconds:
					syncUnitString = "Seconds";
					break;
				case SyncUnit.Hours:
					syncUnitString = "Hours";
					break;
				case SyncUnit.Days:
					syncUnitString = "Days";
					break;
				case SyncUnit.Minutes:
				default:
					syncUnitString = "Minutes";
					break;
			}
			
			ClientConfig.Set(ClientConfig.KEY_SYNC_UNIT, syncUnitString);
		}
	}
	
	public enum SyncUnit
	{
		Seconds = 0,
		Minutes,
		Hours,
		Days
	}
}
