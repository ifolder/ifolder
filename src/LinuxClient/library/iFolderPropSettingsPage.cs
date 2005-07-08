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
		private iFolderWeb			ifolder;
		private DiskSpace			ds;
		
		private Table				BasicTable;
		private Label				NameLabel;
		private Label				OwnerLabel;
		private Label				LocationLabel;

		private	Label				LastSuccessfulSync;
		private Label				FFSyncValue;
		private Label				SyncIntervalValue;

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

		private Button				SyncNowButton;

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



		public void UpdateiFolder(iFolderWeb ifolder)
		{
			this.ifolder = ifolder;

			if (ifolder.LastSyncTime == null || ifolder.LastSyncTime == "")
				LastSuccessfulSync.Text = Util.GS("N/A");
			else
				LastSuccessfulSync.Text = ifolder.LastSyncTime;
			FFSyncValue.Text = "0";
			
			int syncInterval = 0;
			if (ifolder.SyncInterval <= 0)
			{
				try
				{
					syncInterval = ifws.GetDefaultSyncInterval();
				}
				catch
				{}
			}
			else
			{
				syncInterval = ifolder.SyncInterval;
			}
			
			// Make sure it's shown in minutes
			if (syncInterval > 0)
			{
				syncInterval = syncInterval / 60;
			}

			SyncIntervalValue.Text = syncInterval + " " + Util.GS("minute(s)");
			
			NameLabel.Markup = string.Format("<span weight=\"bold\">{0}</span>", ifolder.Name);
			OwnerLabel.Markup = string.Format("<span size=\"small\">{0}</span>", ifolder.Owner);
			LocationLabel.Markup = string.Format("<span size=\"small\">{0}</span>", ifolder.UnManagedPath);
			
			try
			{
				SyncSize ss = ifws.CalculateSyncSize(ifolder.ID);
				FFSyncValue.Text = string.Format("{0}", ss.SyncNodeCount);
			}
			catch(Exception e)
			{
				FFSyncValue.Text = Util.GS("N/A");

//				iFolderExceptionDialog ied = new iFolderExceptionDialog(
//													topLevelWindow, e);
//				ied.Run();
//				ied.Hide();
//				ied.Destroy();
			}


			try
			{
				ds = ifws.GetiFolderDiskSpace(ifolder.ID);
			}
			catch(Exception e)
			{
				ds = null;
//				iFolderExceptionDialog ied = new iFolderExceptionDialog(
//													topLevelWindow, e);
//				ied.Run();
//				ied.Hide();
//				ied.Destroy();
			}

			// check for iFolder Owner
			if(ifolder.CurrentUserID == ifolder.OwnerID)
			{
				if(LimitCheckButton == null)
				{
					LimitCheckButton = 
						new CheckButton(Util.GS("_Set Quota:"));
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
					LimitLabel = new Label(Util.GS("Quota:"));
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
				if(ds.UsedSpace == 0)
				{
					UsedValue.Text = "0";
				}
				else
				{
					tmpValue = (int)(ds.UsedSpace / (1024 * 1024)) + 1;
					UsedValue.Text = string.Format("{0}", tmpValue);
				}
			}

		}




		/// <summary>
		/// Set up the UI inside the Window
		/// </summary>
		private void InitializeWidgets()
		{
			this.Spacing = 10;
			this.BorderWidth = Util.DefaultBorderWidth;

			//----------------------------------------
			// Basic information (Name/Owner/Location)
			//----------------------------------------
			HBox basicBox = new HBox();
			basicBox.Spacing = 10;
			this.PackStart(basicBox, false, true, 0);
			
			// ifolder48.png
			Gdk.Pixbuf iFolderPixbuf = new Gdk.Pixbuf(Util.ImagesPath("ifolder48.png"));
			Image iFolderImage = new Image(iFolderPixbuf);
			iFolderImage.SetAlignment(0.5F, 0);
			
			basicBox.PackStart(iFolderImage, false, false, 0);

			VBox basicLabelsBox = new VBox();
			basicLabelsBox.Spacing = 5;
			basicBox.PackStart(basicLabelsBox, false, true, 0);

			NameLabel = new Label("");
			NameLabel.UseMarkup = true;
			NameLabel.Xalign = 0;
			basicLabelsBox.PackStart(NameLabel, false, true, 5);

			// create a table to hold the values
			BasicTable = new Table(2, 2, false);
			basicLabelsBox.PackStart(BasicTable, true, true, 0);
			BasicTable.ColumnSpacing = 5;
			BasicTable.RowSpacing = 5;
			
			Label label = new Label(string.Format("<span size=\"small\">{0}</span>", Util.GS("Owner:")));
			label.UseMarkup = true;
			label.Xalign = 0;
			BasicTable.Attach(label, 0, 1, 0, 1,
					AttachOptions.Shrink | AttachOptions.Fill, 0, 0, 0);
			
			OwnerLabel = new Label("");
			OwnerLabel.UseMarkup = true;
			OwnerLabel.Xalign = 0;
			BasicTable.Attach(OwnerLabel, 1, 2, 0, 1,
					AttachOptions.Expand | AttachOptions.Fill, 0, 0, 0);
			
			label = new Label(string.Format("<span size=\"small\">{0}</span>", Util.GS("Location:")));
			label.UseMarkup = true;
			label.Xalign = 0;
			BasicTable.Attach(label, 0, 1, 1, 2,
					AttachOptions.Shrink | AttachOptions.Fill, 0, 0, 0);
			
			LocationLabel = new Label("");
			LocationLabel.UseMarkup = true;
			LocationLabel.Xalign = 0;
			BasicTable.Attach(LocationLabel, 1, 2, 1, 2,
					AttachOptions.Expand | AttachOptions.Fill, 0, 0, 0);
			

			//------------------------------
			// Disk Space
			//------------------------------
			// create a section box
			VBox diskSectionBox = new VBox();
			diskSectionBox.Spacing = Util.SectionTitleSpacing;
			this.PackStart(diskSectionBox, false, true, 0);
			Label diskSectionLabel = new Label("<span weight=\"bold\">" +
												Util.GS("Disk Space on Server") +
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



			Label usedLabel = new Label(Util.GS("Used:"));
			usedLabel.Xalign = 0;
			diskTable.Attach(usedLabel, 0,1,0,1,
					AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
			UsedValue = new Label("0");
			UsedValue.Xalign = 1;
			diskTable.Attach(UsedValue, 1,2,0,1,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			Label usedUnit = new Label(Util.GS("MB"));
			diskTable.Attach(usedUnit, 2,3,0,1,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);


			LimitUnit = new Label(Util.GS("MB"));
			diskTable.Attach(LimitUnit, 2,3,1,2,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);


			AvailLabel = new Label(Util.GS("Available:"));
			AvailLabel.Xalign = 0;
			diskTable.Attach(AvailLabel, 0,1,2,3,
					AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
			AvailValue = new Label("0");
			AvailValue.Xalign = 1;
			diskTable.Attach(AvailValue, 1,2,2,3,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			AvailUnit = new Label(Util.GS("MB"));
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

			DiskUsageFullLabel = new Label(Util.GS("full"));
			DiskUsageFullLabel.Xalign = 0;
			DiskUsageFullLabel.Yalign = 0;
			graphLabelBox.PackStart(DiskUsageFullLabel, true, true, 0);

			DiskUsageEmptyLabel = new Label(Util.GS("empty"));
			DiskUsageEmptyLabel.Xalign = 0;
			DiskUsageEmptyLabel.Yalign = 1;
			graphLabelBox.PackStart(DiskUsageEmptyLabel, true, true, 0);



			//------------------------------
			// Synchronization Information
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
			syncSpacerBox.Spacing = 10;
			syncSectionBox.PackStart(syncSpacerBox, true, true, 0);
			Label srvSpaceLabel = new Label("");
			syncSpacerBox.PackStart(srvSpaceLabel, false, true, 0);

			// create a vbox to actually place the widgets in for section
			VBox syncWidgetBox = new VBox();
			syncSpacerBox.PackStart(syncWidgetBox, true, true, 0);
			syncWidgetBox.Spacing = 10;

			// create a table to hold the values
			Table syncTable = new Table(3,2,false);
			syncWidgetBox.PackStart(syncTable, true, true, 0);
			syncTable.Homogeneous = false;
			syncTable.ColumnSpacing = 20;
			syncTable.RowSpacing = 5;
			
			Label lastSyncLabel = new Label(Util.GS("Last Successful Synchronization:"));
			lastSyncLabel.Xalign = 0;
			syncTable.Attach(lastSyncLabel, 0,1,0,1,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			LastSuccessfulSync = new Label(Util.GS("N/A"));
			LastSuccessfulSync.Xalign = 0;
			syncTable.Attach(LastSuccessfulSync, 1,2,0,1);
			
			Label FFSyncLabel = 
					new Label(Util.GS("Files/Folders to Synchronize:"));
			FFSyncLabel.Xalign = 0;
			syncTable.Attach(FFSyncLabel, 0,1,1,2,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			FFSyncValue = new Label("0");
			FFSyncValue.Xalign = 0;
			syncTable.Attach(FFSyncValue, 1,2,1,2);
			
			Label SyncIntervalLabel =
				new Label(Util.GS("Automatically Synchronizes Every:"));
			SyncIntervalLabel.Xalign = 0;
			syncTable.Attach(SyncIntervalLabel, 0,1,2,3,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			SyncIntervalValue = new Label("1 minute(s)");
			SyncIntervalValue.Xalign = 0;
			syncTable.Attach(SyncIntervalValue, 1,2,2,3);
			
			HBox rightBox = new HBox();
			rightBox.Spacing = 10;
			syncWidgetBox.PackEnd(rightBox, false, false, 0);
			
			SyncNowButton = new Button(Util.GS("Synchronize _Now"));
			rightBox.PackEnd(SyncNowButton, false, false, 0);
			SyncNowButton.Clicked += new EventHandler(OnSyncNowClicked);
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




		private void OnSyncNowClicked(object o, EventArgs args)
		{
			try
			{
				ifws.SynciFolderNow(ifolder.ID);
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
