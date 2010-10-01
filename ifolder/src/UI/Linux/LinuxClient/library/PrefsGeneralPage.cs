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
		private Gtk.ComboBox			SyncUnitsComboBox;

		private Gtk.CheckButton			ShowConfirmationButton;
		private Gtk.CheckButton			HideMainWindowButton;
		private int						lastSyncInterval;
		private SyncUnit				currentSyncUnit;

		private NotifyWindow			oneMinuteLimitNotifyWindow;

		private Gtk.CheckButton			HideSyncLogButton;

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
			
//			ClientConfig.SettingChanged += 
//				new GConf.NotifyEventHandler(OnClientConfigChanged);
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
					"_Display confirmation dialog on successful creation of iFolder"));
			appWidgetBox.PackStart(ShowConfirmationButton, false, true, 0);
			ShowConfirmationButton.Toggled += 
						new EventHandler(OnShowConfButton);


/*			ShowNetworkstatusButton =
                                new CheckButton(Util.GS(
                                        "Show _Network Events  messages when iFolder is started"));
                        appWidgetBox.PackStart(ShowNetworkstatusButton, false, true, 0);
                        ShowNetworkstatusButton.Toggled +=
                                                new EventHandler(OnShowNetworkButton); */


			Label strtlabel = new Label("<span style=\"italic\">" + Util.GS("To start up iFolder at login, leave iFolder running when you log out and save your current setup.") + "</span>");
			strtlabel.UseMarkup = true;
			strtlabel.LineWrap = true;
			appWidgetBox.PackStart(strtlabel, false, true, 0);

			HideMainWindowButton=
				new CheckButton(Util.GS("Hide ifolder _main window at startup")); 
			appWidgetBox.PackStart(HideMainWindowButton, false, true, 0);
			HideMainWindowButton.Toggled += 
						new EventHandler(OnHideMainWindowButton);
			
			HideSyncLogButton =
				new CheckButton(Util.GS("Display synchronization _logs")); 
			appWidgetBox.PackStart(HideSyncLogButton, false, true, 0);

			HideSyncLogButton.Toggled += 
						new EventHandler(OnHideSyncLogButton);


			//------------------------------
			// Notifications
			//------------------------------
			// create a section box
			VBox notifySectionBox = new VBox();
			notifySectionBox.Spacing = Util.SectionTitleSpacing;
			this.PackStart(notifySectionBox, true, true, 0);
			Label notifySectionLabel = new Label("<span weight=\"bold\">" +
												Util.GS("Notification") +
												"</span>");
			notifySectionLabel.UseMarkup = true;
			notifySectionLabel.Xalign = 0;
			notifySectionBox.PackStart(notifySectionLabel, false, true, 0);

			// create a hbox to provide spacing
			HBox notifySpacerBox = new HBox();
			notifySectionBox.PackStart(notifySpacerBox, true, true, 0);
			Label notifySpaceLabel = new Label("    "); // four spaces
			notifySpacerBox.PackStart(notifySpaceLabel, false, true, 0);

			// create a vbox to actually place the widgets in for section
			VBox notifyWidgetBox = new VBox();
			notifySpacerBox.PackStart(notifyWidgetBox, true, true, 0);
			notifyWidgetBox.Spacing = 5;

			VBox notificationPreferences = new NotificationPrefsBox (this.topLevelWindow);
			notifyWidgetBox.PackStart (notificationPreferences, true, true, 0);

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

			SyncUnitsComboBox = ComboBox.NewText();
			syncHBox.PackStart(SyncUnitsComboBox, false, false, 0);
			
			SyncUnitsComboBox.AppendText(Util.GS("seconds"));
			SyncUnitsComboBox.AppendText(Util.GS("minutes"));
			SyncUnitsComboBox.AppendText(Util.GS("hours"));
			SyncUnitsComboBox.AppendText(Util.GS("days"));
			
			SyncUnitsComboBox.Active = (int)SyncUnit.Minutes;
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
			if((bool)ClientConfig.Get(ClientConfig.KEY_SHOW_CREATION))
				ShowConfirmationButton.Active = true;
			else
				ShowConfirmationButton.Active = false;

			if((bool)ClientConfig.Get(ClientConfig.KEY_SHOW_SYNC_LOG))
				HideSyncLogButton.Active = true;
			else
				HideSyncLogButton.Active = false;

			if((bool)ClientConfig.Get(ClientConfig.KEY_IFOLDER_WINDOW_HIDE))
				HideMainWindowButton.Active = true;
			else
				HideMainWindowButton.Active = false;

			try
			{
				lastSyncInterval = ifws.GetDefaultSyncInterval();
				if(lastSyncInterval <= 0)
					SyncSpinButton.Value = 0;
				else
				{
					string syncUnitString = (string)
						ClientConfig.Get(ClientConfig.KEY_SYNC_UNIT);
					switch (syncUnitString)
					{
						case "Seconds":
							currentSyncUnit = SyncUnit.Seconds;
							SyncUnitsComboBox.Active = (int)SyncUnit.Seconds;

							// Prevent the user from setting a sync interval less than
							// five seconds.
							SyncSpinButton.Adjustment.Lower = 1;
							break;
						case "Minutes":
							currentSyncUnit = SyncUnit.Minutes;
							SyncUnitsComboBox.Active = (int)SyncUnit.Minutes;
							break;
						case "Hours":
							currentSyncUnit = SyncUnit.Hours;
							SyncUnitsComboBox.Active = (int)SyncUnit.Hours;
							break;
						case "Days":
							currentSyncUnit = SyncUnit.Days;
							SyncUnitsComboBox.Active = (int)SyncUnit.Days;
							break;
						default:
							break;
					}

					SyncSpinButton.Value = CalculateSyncSpinValue(lastSyncInterval, currentSyncUnit);
				}
			}
			catch(Exception)
			{
				lastSyncInterval = -1;
				SyncSpinButton.Value = 0;
			}

			if (lastSyncInterval <= 0)
			{
				AutoSyncCheckButton.Active = false;
				SyncSpinButton.Sensitive = false;
				SyncUnitsComboBox.Sensitive = false;
			}
			else
			{
				AutoSyncCheckButton.Active = true;
				SyncSpinButton.Sensitive = true;
				SyncUnitsComboBox.Sensitive = true;
			}

			AutoSyncCheckButton.Toggled += new EventHandler(OnAutoSyncButton);
			SyncSpinButton.ValueChanged += 
					new EventHandler(OnSyncIntervalChanged);
			SyncUnitsComboBox.Changed +=
					new EventHandler(OnSyncUnitsChanged);
		}




		private void OnRealizeWidget(object o, EventArgs args)
		{
			PopulateWidgets();
		}

		private void OnHideSyncLogButton(object o, EventArgs args)
		{
			if(HideSyncLogButton.Active)
				ClientConfig.Set(ClientConfig.KEY_SHOW_SYNC_LOG, true);
			else
				ClientConfig.Set(ClientConfig.KEY_SHOW_SYNC_LOG, false);
		}


		private void OnHideMainWindowButton(object o, EventArgs args)
		{
			if(HideMainWindowButton.Active)
			{
				ClientConfig.Set(ClientConfig.KEY_IFOLDER_WINDOW_HIDE, true);
			}
			else
			{
				ClientConfig.Set(ClientConfig.KEY_IFOLDER_WINDOW_HIDE, false);
			}
		}

		private void OnShowConfButton(object o, EventArgs args)
		{
			if(ShowConfirmationButton.Active)
				ClientConfig.Set(ClientConfig.KEY_SHOW_CREATION, true);
			else
				ClientConfig.Set(ClientConfig.KEY_SHOW_CREATION, false);
		}

		private void OnAutoSyncButton(object o, EventArgs args)
		{
			if(AutoSyncCheckButton.Active == true)
			{
				SyncSpinButton.Sensitive = true;
				SyncUnitsComboBox.Sensitive = true;

				SyncUnitsComboBox.Changed -= new EventHandler(OnSyncUnitsChanged);
				SyncUnitsComboBox.Active = (int)SyncUnit.Minutes;
				currentSyncUnit = SyncUnit.Minutes;
				SaveSyncUnitConfig();
				SyncUnitsComboBox.Changed += new EventHandler(OnSyncUnitsChanged);

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
				
				try
				{
					ifws.SetDefaultSyncInterval(-1);
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
	
				SyncSpinButton.ValueChanged -= new EventHandler(OnSyncIntervalChanged);
				SyncSpinButton.Value = 0;
				SyncSpinButton.Sensitive = false;
				SyncUnitsComboBox.Sensitive = false;
				SyncSpinButton.ValueChanged += new EventHandler(OnSyncIntervalChanged);
			}
		}




		private void OnSyncIntervalChanged(object o, EventArgs args)
		{
			if ((currentSyncUnit == SyncUnit.Seconds) && (SyncSpinButton.Value < 5) )
			{
			    if (oneMinuteLimitNotifyWindow != null)
			    {
			   	   oneMinuteLimitNotifyWindow.Hide();
				   oneMinuteLimitNotifyWindow.Destroy();
				   oneMinuteLimitNotifyWindow = null;
			    }
			    SyncSpinButton.Value = 5;
			    oneMinuteLimitNotifyWindow =
					    new NotifyWindow(
						    SyncSpinButton, Util.GS("Synchronization Interval Limit"),
						    Util.GS("The synchronization interval cannot be set to less than 5 Seconds.  It will automatically change to 5 seconds."), Gtk.MessageType.Info, 10000);
   			    oneMinuteLimitNotifyWindow.ShowAll();
			}

			int syncSpinValue =
				CalculateActualSyncInterval((int)SyncSpinButton.Value,
											currentSyncUnit);
		
			try
			{
				lastSyncInterval = syncSpinValue;
				if(lastSyncInterval <= 0)
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
											
			currentSyncUnit = (SyncUnit)SyncUnitsComboBox.Active;

			if (currentSyncUnit == SyncUnit.Seconds)
			{

				if (syncSpinValue < 5)
				{
					oneMinuteLimitNotifyWindow =
						new NotifyWindow(
						SyncSpinButton, Util.GS("Synchronization Interval Limit"),
						Util.GS("The synchronization interval cannot be set to less than 5 Seconds.  It will automatically change to 5 seconds."),
						Gtk.MessageType.Info, 10000);
					oneMinuteLimitNotifyWindow.ShowAll();

					SyncSpinButton.ValueChanged -= 
						new EventHandler(OnSyncIntervalChanged);
					SyncSpinButton.Value = 5;
					syncSpinValue = 5;
					SyncSpinButton.ValueChanged += 
						new EventHandler(OnSyncIntervalChanged);
				}
				SyncSpinButton.Adjustment.Lower = 1;
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
				if(lastSyncInterval <= 0)
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
