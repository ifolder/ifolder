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
using Gtk;

namespace Novell.iFolder
{

	/// <summary>
	/// This is the properties page for iFolder settings
	/// </summary>
	public class iFolderPropSettingsPage : VBox
	{
		private iFolderWebService	ifws;
		private iFolder				ifolder;
		private	CheckButton 		AutoSyncCheckButton;
		private SpinButton			SyncSpinButton;
		private Label				SyncUnitsLabel;




		/// <summary>
		/// Default constructor for iFolderPropSharingPage
		/// </summary>
		public iFolderPropSettingsPage(	iFolder ifolder, 
										iFolderWebService iFolderWS)
			: base()
		{
			this.ifws = iFolderWS;
			this.ifolder = ifolder;
			InitializeWidgets();
		}




		/// <summary>
		/// Setup the UI inside the Window
		/// </summary>
		private void InitializeWidgets()
		{
			this.Spacing = 10;
			this.BorderWidth = 10;

			//------------------------------
			// Sync Settings
			//------------------------------
			// create a section box
			VBox syncSectionBox = new VBox();
			this.PackStart(syncSectionBox, false, true, 0);
			Label syncSectionLabel = new Label("<span weight=\"bold\">" +
												"Synchronization" +
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


			Label syncHelpLabel = new Label("This will set the default Sync Settings for all iFolders.  You can change the sync setting for an individual iFolder on an iFolder's property pages");
			syncHelpLabel.LineWrap = true;
			syncHelpLabel.Xalign = 0;
			syncWidgetBox.PackStart(syncHelpLabel, false, true, 0);

			HBox syncHBox = new HBox();
			syncWidgetBox.PackStart(syncHBox, false, true, 0);
			syncHBox.Spacing = 10;
			AutoSyncCheckButton = 
					new CheckButton("Sync to host every:");
//			AutoSyncCheckButton.Toggled += new EventHandler(OnAutoSyncButton);
			syncHBox.PackStart(AutoSyncCheckButton, false, false, 0);
			SyncSpinButton = new SpinButton(0, 99999, 1);
//			SyncSpinButton.ValueChanged += 
//					new EventHandler(OnSyncIntervalChanged);
			syncHBox.PackStart(SyncSpinButton, false, false, 0);
			SyncUnitsLabel = new Label("seconds");
			SyncUnitsLabel.Xalign = 0;
			syncHBox.PackEnd(SyncUnitsLabel, true, true, 0);







			//------------------------------
			// Statistics Information
			//------------------------------
			// create a section box
			VBox srvSectionBox = new VBox();
			this.PackStart(srvSectionBox, false, true, 0);
			Label srvSectionLabel = new Label("<span weight=\"bold\">" +
												"Statistics" +
												"</span>");
			srvSectionLabel.UseMarkup = true;
			srvSectionLabel.Xalign = 0;
			srvSectionBox.PackStart(srvSectionLabel, false, true, 0);

			// create a hbox to provide spacing
			HBox srvSpacerBox = new HBox();
			srvSectionBox.PackStart(srvSpacerBox, true, true, 0);
			Label srvSpaceLabel = new Label("    "); // four spaces
			srvSpacerBox.PackStart(srvSpaceLabel, false, true, 0);

			// create a vbox to actually place the widgets in for section
			VBox srvWidgetBox = new VBox();
			srvSpacerBox.PackStart(srvWidgetBox, true, true, 0);

			// create a table to hold the values
			Table srvTable = new Table(2,2,false);
			srvWidgetBox.PackStart(srvTable, true, true, 0);
			srvTable.Homogeneous = false;
			srvTable.ColumnSpacing = 10;
			Label srvNameLabel = new Label("Amount to upload:");
			srvNameLabel.Xalign = 0;
			srvTable.Attach(srvNameLabel, 0,1,0,1,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			Label srvNameValue = new Label("0");
			srvNameValue.Xalign = 0;
			srvTable.Attach(srvNameValue, 1,2,0,1);
			Label usrNameLabel = new Label("Items to synchronize:");
			usrNameLabel.Xalign = 0;
			srvTable.Attach(usrNameLabel, 0,1,1,2,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			Label usrNameValue = new Label("0");
			usrNameValue.Xalign = 0;
			srvTable.Attach(usrNameValue, 1,2,1,2);

		}
	}
}
