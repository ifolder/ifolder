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
		public iFolderPropSettingsPage(iFolderWebService iFolderWS)
			: base()
		{
			this.ifws = iFolderWS;
			InitializeWidgets();
		}



		public void UpdateiFolder(iFolder ifolder)
		{
			this.ifolder = ifolder;
		}




		/// <summary>
		/// Setup the UI inside the Window
		/// </summary>
		private void InitializeWidgets()
		{
			this.Spacing = 10;
			this.BorderWidth = Util.DefaultBorderWidth;

			//------------------------------
			// Sync Settings
			//------------------------------
			// create a section box
			VBox syncSectionBox = new VBox();
			syncSectionBox.Spacing = Util.SectionTitleSpacing;
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


			Label syncHelpLabel = new Label("This will set the sync interval for the current iFolder.  You can change the default sync setting for all iFolders on the Preferences page of the main window");
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
			srvSectionBox.Spacing = Util.SectionTitleSpacing;
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
			srvTable.ColumnSpacing = 20;
			Label srvNameLabel = new Label("Amount to upload:");
			srvNameLabel.Xalign = 0;
			srvTable.Attach(srvNameLabel, 0,1,0,1,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			Label srvNameValue = new Label("0");
			srvNameValue.Xalign = 0;
			srvTable.Attach(srvNameValue, 1,2,0,1);
			Label usrNameLabel = new Label("Files/Folders to synchronize:");
			usrNameLabel.Xalign = 0;
			srvTable.Attach(usrNameLabel, 0,1,1,2,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			Label usrNameValue = new Label("0");
			usrNameValue.Xalign = 0;
			srvTable.Attach(usrNameValue, 1,2,1,2);



			//------------------------------
			// Disk Space
			//------------------------------
			// create a section box
			VBox diskSectionBox = new VBox();
			diskSectionBox.Spacing = Util.SectionTitleSpacing;
			this.PackStart(diskSectionBox, false, true, 0);
			Label diskSectionLabel = new Label("<span weight=\"bold\">" +
												"Disk Space" +
												"</span>");
			diskSectionLabel.UseMarkup = true;
			diskSectionLabel.Xalign = 0;
			diskSectionBox.PackStart(diskSectionLabel, false, true, 0);

			// create a hbox to provide spacing
			HBox diskSpacerBox = new HBox();
			diskSpacerBox.Spacing = 10;
			diskSectionBox.PackStart(diskSpacerBox, true, true, 0);
			Label diskSpaceLabel = new Label("");
			diskSpacerBox.PackStart(diskSpaceLabel, false, true, 0);


			// create a table to hold the values
			Table diskTable = new Table(4,3,false);
			diskSpacerBox.PackStart(diskTable, true, true, 0);
			diskTable.ColumnSpacing = 20;
			diskTable.RowSpacing = 5;

			Label totalLabel = new Label("Free space:");
			totalLabel.Xalign = 0;
			diskTable.Attach(totalLabel, 0,1,0,1,
					AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
			Label totalValue = new Label("8000");
			totalValue.Xalign = 1;
			diskTable.Attach(totalValue, 1,2,0,1,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			Label totalUnit = new Label("MB");
			diskTable.Attach(totalUnit, 2,3,0,1,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			Label usedLabel = new Label("Used space:");
			usedLabel.Xalign = 0;
			diskTable.Attach(usedLabel, 0,1,1,2,
					AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
			Label usedValue = new Label("500");
			usedValue.Xalign = 1;
			diskTable.Attach(usedValue, 1,2,1,2,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			Label usedUnit = new Label("MB");
			diskTable.Attach(usedUnit, 2,3,1,2,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			Label availLabel = new Label("Total space:");
			availLabel.Xalign = 0;
			diskTable.Attach(availLabel, 0,1,2,3,
					AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
			Label availValue = new Label("7500");
			availValue.Xalign = 1;
			diskTable.Attach(availValue, 1,2,2,3,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			Label availUnit = new Label("MB");
			diskTable.Attach(availUnit, 2,3,2,3,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			CheckButton LimitCheckButton = 
					new CheckButton("Limit size to:");
			diskTable.Attach(LimitCheckButton, 0,1,3,4,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
//			LimitCheckButton.Toggled += new EventHandler(OnAutoSyncButton);

			Entry limitEntry = new Entry();
			diskTable.Attach(limitEntry, 1,2,3,4,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
//			SyncSpinButton.ValueChanged += 
//					new EventHandler(OnSyncIntervalChanged);

			Label limitUnitsLabel = new Label("MB");
			limitUnitsLabel.Xalign = 0;
			diskTable.Attach(limitUnitsLabel, 2,3,3,4,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);



			Frame graphFrame = new Frame();
			graphFrame.Shadow = Gtk.ShadowType.EtchedOut;
			graphFrame.ShadowType = Gtk.ShadowType.EtchedOut;
			diskSpacerBox.PackStart(graphFrame, false, true, 0);
			HBox graphBox = new HBox();
			graphBox.Spacing = 5;
			graphBox.BorderWidth = 5;
			graphFrame.Add(graphBox);

			ProgressBar diskGraph = new ProgressBar();
			graphBox.PackStart(diskGraph, false, true, 0);

			diskGraph.Orientation = Gtk.ProgressBarOrientation.BottomToTop;
//			diskGraph.Text = "%3";
			diskGraph.PulseStep = .10;
			diskGraph.Fraction = .30;

			VBox graphLabelBox = new VBox();
			graphBox.PackStart(graphLabelBox, false, true, 0);

			Label fullLabel = new Label("full");
			fullLabel.Xalign = 0;
			fullLabel.Yalign = 0;
			graphLabelBox.PackStart(fullLabel, true, true, 0);

			Label emptyLabel = new Label("empty");
			emptyLabel.Xalign = 0;
			emptyLabel.Yalign = 1;
			graphLabelBox.PackStart(emptyLabel, true, true, 0);


		}
	}
}
