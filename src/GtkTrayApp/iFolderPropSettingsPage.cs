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
		private Gtk.Window			topLevelWindow;
		private iFolderWebService	ifws;
		private iFolder				ifolder;
		private iFolderUser			ifolderUser;
		private DiskSpace			ds;
		private	CheckButton 		AutoSyncCheckButton;
		private SpinButton			SyncSpinButton;
		private Label				SyncUnitsLabel;

		private	Label				UploadValue;
		private Label				FFSyncValue;

		private Label				UsedValue;

		private Table				diskTable;
		private Label				LimitLabel;
		private CheckButton			LimitCheckButton;
		private Label				LimitValue;
		private Entry				LimitEntry;
		private Label				LimitUnit;

		private Label				AvailLabel;
		private Label				AvailValue;
		private Label				AvailUnit;


		private ProgressBar			DiskUsageBar;
		private Frame				DiskUsageFrame;
		private Label				DiskUsageFullLabel;
		private Label				DiskUsageEmptyLabel;


		/// <summary>
		/// Default constructor for iFolderPropSharingPage
		/// </summary>
		public iFolderPropSettingsPage(	Gtk.Window topWindow,
										iFolderWebService iFolderWS)
			: base()
		{
			this.topLevelWindow = topWindow;
			this.ifws = iFolderWS;
			InitializeWidgets();
		}



		public void UpdateiFolder(iFolder ifolder)
		{
			this.ifolder = ifolder;

			if(ifolder.Synchronizable)
			{
				AutoSyncCheckButton.Active = true;
				AutoSyncCheckButton.Sensitive = true;
				SyncSpinButton.Sensitive = true;
				SyncUnitsLabel.Sensitive = true;
				SyncSpinButton.Value = ifolder.SyncInterval;
			}
			else
			{
				AutoSyncCheckButton.Active = false;
				AutoSyncCheckButton.Sensitive = false;
				SyncSpinButton.Sensitive = false;
				SyncUnitsLabel.Sensitive = false;
				SyncSpinButton.Value = ifolder.SyncInterval;
			}

			SyncSize ss = ifws.CalculateSyncSize(ifolder.ID);
			UploadValue.Text = string.Format("{0}", ss.SyncByteCount);
			FFSyncValue.Text = string.Format("{0}", ss.SyncNodeCount);


			try
			{
				ifolderUser = ifws.GetiFolderUserFromiFolder(
									ifolder.CurrentUserID, ifolder.ID);
				ds = ifws.GetiFolderDiskSpace(ifolder.ID);
			}
			catch(Exception e)
			{
				ifolderUser = null;
				ds = null;
				iFolderExceptionDialog ied = new iFolderExceptionDialog(
													topLevelWindow, e);
				ied.Run();
				ied.Hide();
				ied.Destroy();
			}

			// check for Admin rights
			if( (ifolderUser != null) && (ifolderUser.Rights == "Admin") )
			{
				if(LimitCheckButton == null)
				{
					LimitCheckButton = 
						new CheckButton("Limit size to:");
					LimitCheckButton.Toggled += 
								new EventHandler(OnLimitSizeButton);
					diskTable.Attach(LimitCheckButton, 0,1,1,2,
						AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

					LimitEntry = new Entry();
					LimitEntry.Changed +=
						new EventHandler(OnLimitChanged);
					LimitEntry.Activated += 
						new EventHandler(OnLimitEdited);
					LimitEntry.FocusOutEvent +=
						new FocusOutEventHandler(OnLimitFocusLost);
					LimitEntry.WidthChars = 6;
					LimitEntry.MaxLength = 10;
					LimitEntry.Layout.Alignment = Pango.Alignment.Left;
					diskTable.Attach(LimitEntry, 1,2,1,2,
						AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
					LimitCheckButton.ShowAll();
					LimitEntry.ShowAll();
				}
				else
				{
					LimitCheckButton.Visible = true;
					LimitEntry.Visible = true;
				}

				if(LimitLabel != null)
				{
					LimitLabel.Visible = false;
					LimitValue.Visible = false;
				}
			}
			else
			{
				if(LimitLabel == null)
				{
					LimitLabel = new Label("iFolder limit:");
					LimitLabel.Xalign = 0;
					diskTable.Attach(LimitLabel, 0,1,1,2,
						AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
				
					LimitValue = new Label("0");
					LimitValue.Xalign = 1;
					diskTable.Attach(LimitValue, 1,2,1,2,
						AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
					LimitLabel.ShowAll();
					LimitValue.ShowAll();
				}
				else
				{
					LimitLabel.Visible = true;
					LimitValue.Visible = true;
				}

				if(LimitCheckButton != null)
				{
					LimitCheckButton.Visible = false;
					LimitEntry.Visible = false;
				}
			}

			if(ds != null)
			{
				int tmpValue;

				// there is no limit set, disable controls
				if(ds.Limit == 0)
				{
					LimitUnit.Sensitive = false;
					AvailLabel.Sensitive = false;
					AvailValue.Sensitive = false;
					AvailUnit.Sensitive = false;
					DiskUsageBar.Sensitive = false;
					DiskUsageFrame.Sensitive = false;
					DiskUsageFullLabel.Sensitive = false;
					DiskUsageEmptyLabel.Sensitive = false;

					if(LimitCheckButton != null)
					{
						LimitCheckButton.Active = false; 
						LimitEntry.Sensitive = false;
						LimitEntry.Text = "0";
					}
					if(LimitLabel != null)
					{
						LimitLabel.Sensitive = false;
						LimitValue.Sensitive = false;
						LimitValue.Text = "0";
					}
					AvailValue.Text = "0";
				}
				else
				{
					LimitUnit.Sensitive = true;
					AvailLabel.Sensitive = true;
					AvailValue.Sensitive = true;
					AvailUnit.Sensitive = true;
					DiskUsageBar.Sensitive = true;
					DiskUsageFrame.Sensitive = true;
					DiskUsageFullLabel.Sensitive = true;
					DiskUsageEmptyLabel.Sensitive = true;

					if(LimitCheckButton != null)
					{
						LimitCheckButton.Active = true; 
						LimitEntry.Sensitive = true;
						tmpValue = (int)(ds.Limit / (1024 * 1024));
						LimitEntry.Text = string.Format("{0}", tmpValue);
					}
					if(LimitLabel != null)
					{
						LimitLabel.Sensitive = true;
						LimitValue.Sensitive = true;
						tmpValue = (int)(ds.Limit / (1024 * 1024));
						LimitValue.Text = string.Format("{0}", tmpValue);
					}

					tmpValue = (int)(ds.AvailableSpace / (1024 * 1024));
					AvailValue.Text = string.Format("{0}",tmpValue);
				}

				SetGraph(ds.UsedSpace, ds.Limit);

				// Add one because there is no iFolder that is zero
				tmpValue = (int)(ds.UsedSpace / (1024 * 1024)) + 1;
				UsedValue.Text = string.Format("{0}", tmpValue);
			}

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
			syncSpacerBox.Spacing = 10;
			syncSectionBox.PackStart(syncSpacerBox, false, true, 0);
			Label syncSpaceLabel = new Label("");
			syncSpacerBox.PackStart(syncSpaceLabel, false, true, 0);

			// create a vbox to actually place the widgets in for section
			VBox syncWidgetBox = new VBox();
			syncSpacerBox.PackStart(syncWidgetBox, false, true, 0);
			syncWidgetBox.Spacing = 10;


			Label syncHelpLabel = new Label("This will set the sync setting for this iFolder.");
			syncHelpLabel.LineWrap = true;
			syncHelpLabel.Xalign = 0;
			syncWidgetBox.PackStart(syncHelpLabel, false, true, 0);

			HBox syncHBox = new HBox();
			syncWidgetBox.PackStart(syncHBox, false, true, 0);
			syncHBox.Spacing = 10;
			AutoSyncCheckButton = 
					new CheckButton("Sync to host every:");
			AutoSyncCheckButton.Toggled += new EventHandler(OnAutoSyncButton);
			syncHBox.PackStart(AutoSyncCheckButton, false, false, 0);
			SyncSpinButton = new SpinButton(0, 99999, 1);
			SyncSpinButton.ValueChanged += 
					new EventHandler(OnSyncIntervalChanged);
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
			srvSpacerBox.Spacing = 10;
			srvSectionBox.PackStart(srvSpacerBox, true, true, 0);
			Label srvSpaceLabel = new Label("");
			srvSpacerBox.PackStart(srvSpaceLabel, false, true, 0);

			// create a vbox to actually place the widgets in for section
			VBox srvWidgetBox = new VBox();
			srvSpacerBox.PackStart(srvWidgetBox, true, true, 0);

			// create a table to hold the values
			Table srvTable = new Table(2,2,false);
			srvWidgetBox.PackStart(srvTable, true, true, 0);
			srvTable.Homogeneous = false;
			srvTable.ColumnSpacing = 20;
			Label uploadLabel = new Label("Amount to upload:");
			uploadLabel.Xalign = 0;
			srvTable.Attach(uploadLabel, 0,1,0,1,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			UploadValue = new Label("0");
			UploadValue.Xalign = 0;
			srvTable.Attach(UploadValue, 1,2,0,1);
			Label FFSyncLabel = new Label("Files/Folders to synchronize:");
			FFSyncLabel.Xalign = 0;
			srvTable.Attach(FFSyncLabel, 0,1,1,2,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			FFSyncValue = new Label("0");
			FFSyncValue.Xalign = 0;
			srvTable.Attach(FFSyncValue, 1,2,1,2);



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
			diskTable = new Table(3,3,false);
			diskSpacerBox.PackStart(diskTable, true, true, 0);
			diskTable.ColumnSpacing = 20;
			diskTable.RowSpacing = 5;



			Label usedLabel = new Label("iFolder size:");
			usedLabel.Xalign = 0;
			diskTable.Attach(usedLabel, 0,1,0,1,
					AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
			UsedValue = new Label("0");
			UsedValue.Xalign = 1;
			diskTable.Attach(UsedValue, 1,2,0,1,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			Label usedUnit = new Label("MB");
			diskTable.Attach(usedUnit, 2,3,0,1,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);


			LimitUnit = new Label("MB");
			diskTable.Attach(LimitUnit, 2,3,1,2,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);


			AvailLabel = new Label("Available space:");
			AvailLabel.Xalign = 0;
			diskTable.Attach(AvailLabel, 0,1,2,3,
					AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
			AvailValue = new Label("0");
			AvailValue.Xalign = 1;
			diskTable.Attach(AvailValue, 1,2,2,3,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			AvailUnit = new Label("MB");
			diskTable.Attach(AvailUnit, 2,3,2,3,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);


			DiskUsageFrame = new Frame();
			diskSpacerBox.PackStart(DiskUsageFrame, false, true, 0);
			HBox graphBox = new HBox();
			graphBox.Spacing = 5;
			graphBox.BorderWidth = 5;
			DiskUsageFrame.Add(graphBox);

			DiskUsageBar = new ProgressBar();
			graphBox.PackStart(DiskUsageBar, false, true, 0);

			DiskUsageBar.Orientation = Gtk.ProgressBarOrientation.BottomToTop;
			DiskUsageBar.Fraction = 0;

			VBox graphLabelBox = new VBox();
			graphBox.PackStart(graphLabelBox, false, true, 0);

			DiskUsageFullLabel = new Label("full");
			DiskUsageFullLabel.Xalign = 0;
			DiskUsageFullLabel.Yalign = 0;
			graphLabelBox.PackStart(DiskUsageFullLabel, true, true, 0);

			DiskUsageEmptyLabel = new Label("empty");
			DiskUsageEmptyLabel.Xalign = 0;
			DiskUsageEmptyLabel.Yalign = 1;
			graphLabelBox.PackStart(DiskUsageEmptyLabel, true, true, 0);
		}




		private void OnSyncIntervalChanged(object o, EventArgs args)
		{
			try
			{
				ifolder.SyncInterval = (int)SyncSpinButton.Value;
				ifws.SetiFolderSyncInterval(ifolder.ID, ifolder.SyncInterval);
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
			}
		}




		private void OnLimitSizeButton(object o, EventArgs args)
		{
			if(LimitCheckButton.Active == true)
			{
				LimitUnit.Sensitive = true;
				AvailLabel.Sensitive = true;
				AvailValue.Sensitive = true;
				AvailUnit.Sensitive = true;
				LimitEntry.Sensitive = true;
				DiskUsageBar.Sensitive = true;
				DiskUsageFrame.Sensitive = true;
				DiskUsageFullLabel.Sensitive = true;
				DiskUsageEmptyLabel.Sensitive = true;
			}
			else
			{
				LimitUnit.Sensitive = false;
				AvailLabel.Sensitive = false;
				AvailValue.Sensitive = false;
				AvailUnit.Sensitive = false;
				DiskUsageBar.Sensitive = false;
				DiskUsageFrame.Sensitive = false;
				DiskUsageFullLabel.Sensitive = false;
				DiskUsageEmptyLabel.Sensitive = false;

				LimitEntry.Sensitive = false;
				LimitEntry.Text = "0";

				// if the currrent value is not the same as the 
				// read value, we need to save the currrent value
				if(GetCurrentLimit() != ds.Limit)
				{
					SaveLimit();
				}
			}
		}




		private void OnLimitChanged(object o, EventArgs args)
		{
			int tmpValue;

			long sizeLimit = GetCurrentLimit();

			if(sizeLimit == 0)
			{
				AvailValue.Text = "0";
			}
			else
			{
				long result = sizeLimit - ds.UsedSpace;
				if(result < 0)
					tmpValue = 0;
				else
				{
					tmpValue = (int)(result / (1024 * 1024));
				}

				AvailValue.Text = string.Format("{0}",tmpValue);
			}

			SetGraph(ds.UsedSpace, sizeLimit);
		}




		private void OnLimitEdited(object o, EventArgs args)
		{
			SaveLimit();
		}




		private void OnLimitFocusLost(object o, FocusOutEventArgs args)
		{
			SaveLimit();
		}




		private void SaveLimit()
		{
			long sizeLimit = GetCurrentLimit();

	 		try
			{
				ifws.SetiFolderDiskSpaceLimit(ifolder.ID, sizeLimit);
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




		private long GetCurrentLimit()
		{
			long sizeLimit;

			if(LimitEntry.Text.Length == 0)
				sizeLimit = 0;
			else
			{
				try
				{
					sizeLimit = (long)System.UInt64.Parse(LimitEntry.Text);
				}
				catch(Exception e)
				{
					sizeLimit = 0;
				}
			}

			sizeLimit = sizeLimit * 1024 * 1024;
			return sizeLimit;
		}




		private void SetGraph(long usedSpace, long limit)
		{
			if(limit == 0)
			{
				DiskUsageBar.Fraction = 0;
				return;
			}

			if(limit < usedSpace)
				DiskUsageBar.Fraction = 1;
			else
				DiskUsageBar.Fraction = ((double)usedSpace) / 
										((double)limit);
		}

	}
}
