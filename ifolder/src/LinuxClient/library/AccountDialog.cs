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


using Gtk;
using System;

namespace Novell.iFolder
{
	public class AccountDialog : Dialog
	{
		private iFolderData			ifdata;
		private DomainInformation	domain;


		public AccountDialog(DomainInformation curDomain, Manager simiasManager)
			: base()
		{
			domain = curDomain;
			ifdata = iFolderData.GetData(simiasManager);
			SetupDialog();
		}



		private void SetupDialog()
		{
			this.Title = Util.GS("Details");
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder24.png"));
			this.HasSeparator = false;

			this.Resizable = false;
			this.Modal = true;
			this.DefaultResponse = ResponseType.Ok;

			CreateWidgets();

			this.AddButton(Util.GS("Close"), ResponseType.Ok);
			
			this.DefaultResponse = ResponseType.Ok;
		}



		/// <summary>
		/// Creates the Enterprise Page
		/// </summary>
		/// <returns>
		/// Widget to display
		/// </returns>
		private void CreateWidgets()
		{
			VBox vbox = new VBox();
			vbox.Spacing = Util.SectionSpacing;
			vbox.BorderWidth = Util.DefaultBorderWidth;
			this.VBox.PackStart(vbox, true, true, 0);

			//------------------------------
			// Server Information
			//------------------------------
			// create a section box
			VBox srvSectionBox = new VBox();
			srvSectionBox.Spacing = Util.SectionTitleSpacing;
			vbox.PackStart(srvSectionBox, false, true, 0);
			Label srvSectionLabel = new Label("<span weight=\"bold\">" +
												Util.GS("System Information") +
												"</span>");
			srvSectionLabel.UseMarkup = true;
			srvSectionLabel.Xalign = 0;
			srvSectionBox.PackStart(srvSectionLabel, false, true, 0);

			// create a hbox to provide spacing
			HBox srvSpacerBox = new HBox();
			srvSpacerBox.Spacing = 10;
			srvSectionBox.PackStart(srvSpacerBox, false, true, 0);
			Label srvSpaceLabel = new Label("");
			srvSpacerBox.PackStart(srvSpaceLabel, false, true, 0);

			// create a vbox to actually place the widgets in for section
			VBox srvWidgetBox = new VBox();
			srvSpacerBox.PackStart(srvWidgetBox, true, true, 0);

			// create a table to hold the values
			Table srvTable = new Table(2,2,false);
			srvWidgetBox.PackStart(srvTable, true, true, 0);
			srvTable.ColumnSpacing = 20;
			srvTable.RowSpacing = 5;

//			Label usrNameLabel = new Label(Util.GS("Username:"));
//			usrNameLabel.Xalign = 0;
//			srvTable.Attach(usrNameLabel, 0,1,0,1,
//					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
//			Label usrNameValue = new Label(domain.MemberName);
//			usrNameValue.Xalign = 0;
//			srvTable.Attach(usrNameValue, 1,2,0,1);


			Label srvNameLabel = new Label(Util.GS("Name:"));
			srvNameLabel.Xalign = 0;
			srvTable.Attach(srvNameLabel, 0,1,0,1,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			Label srvNameValue = new Label(domain.Name);
			srvNameValue.Xalign = 0;
			srvTable.Attach(srvNameValue, 1,2,0,1);

			Label srvDescLabel = new Label(Util.GS("Description:"));
			srvDescLabel.Xalign = 0;
			srvDescLabel.Yalign = 0;
			srvTable.Attach(srvDescLabel, 0,1,1,2,
					AttachOptions.Shrink | AttachOptions.Fill, 
					AttachOptions.Fill,0,0);

			ScrolledWindow sw = new ScrolledWindow();
			sw.ShadowType = Gtk.ShadowType.EtchedIn;
			TextView srvDescValue = new TextView();
			if(domain.Description != null)
				srvDescValue.Buffer.Text = domain.Description;
			srvDescValue.WrapMode = Gtk.WrapMode.Word;
			srvDescValue.Editable = false;
			srvDescValue.CursorVisible = false;
			srvDescValue.RightMargin = 5;
			srvDescValue.LeftMargin = 5;
			sw.Add(srvDescValue);
			srvTable.Attach(sw, 1,2,1,2,
					AttachOptions.Expand | AttachOptions.Fill , 0,0,0);



			//------------------------------
			// Disk Space
			//------------------------------
			// create a section box
			VBox diskSectionBox = new VBox();
			diskSectionBox.Spacing = Util.SectionTitleSpacing;
			vbox.PackStart(diskSectionBox, false, true, 0);
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
			Table diskTable = new Table(3,3,false);
			diskSpacerBox.PackStart(diskTable, true, true, 0);
			diskTable.ColumnSpacing = 20;
			diskTable.RowSpacing = 5;

			Label totalLabel = new Label(Util.GS("Quota:"));
			totalLabel.Xalign = 0;
			diskTable.Attach(totalLabel, 0,1,0,1,
					AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
			Label totalValue = new Label("0");
			totalValue.Xalign = 1;
			diskTable.Attach(totalValue, 1,2,0,1,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			Label totalUnit = new Label(Util.GS("MB"));
			diskTable.Attach(totalUnit, 2,3,0,1,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			Label usedLabel = new Label(Util.GS("Used:"));
			usedLabel.Xalign = 0;
			diskTable.Attach(usedLabel, 0,1,1,2,
					AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
			Label usedValue = new Label("0");
			usedValue.Xalign = 1;
			diskTable.Attach(usedValue, 1,2,1,2,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			Label usedUnit = new Label(Util.GS("MB"));
			diskTable.Attach(usedUnit, 2,3,1,2,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			Label availLabel = new Label(Util.GS("Available:"));
			availLabel.Xalign = 0;
			diskTable.Attach(availLabel, 0,1,2,3,
					AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
			Label availValue = new Label("0");
			availValue.Xalign = 1;
			diskTable.Attach(availValue, 1,2,2,3,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			Label availUnit = new Label(Util.GS("MB"));
			diskTable.Attach(availUnit, 2,3,2,3,
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
			diskGraph.Fraction = 0;

			VBox graphLabelBox = new VBox();
			graphBox.PackStart(graphLabelBox, false, true, 0);

			Label fullLabel = new Label(Util.GS("full"));
			fullLabel.Xalign = 0;
			fullLabel.Yalign = 0;
			graphLabelBox.PackStart(fullLabel, true, true, 0);

			Label emptyLabel = new Label(Util.GS("empty"));
			emptyLabel.Xalign = 0;
			emptyLabel.Yalign = 1;
			graphLabelBox.PackStart(emptyLabel, true, true, 0);


			DiskSpace ds = ifdata.GetUserDiskSpace(domain.MemberUserID);

			if(ds == null)
			{
				totalValue.Text = Util.GS("N/A");
				totalUnit.Text = "";
				availValue.Text = Util.GS("N/A");
				availUnit.Text = "";
				usedValue.Text = Util.GS("N/A");
				usedUnit.Text = "";
				diskGraph.Fraction = 0;
			}
			else
			{
				int tmpValue;

				if(ds.Limit == 0)
				{
					totalValue.Text = Util.GS("N/A");
					totalUnit.Text = "";
				}
				else
				{
					tmpValue = (int)(ds.Limit / (1024 * 1024));
					totalValue.Text = string.Format("{0}", tmpValue);
					totalUnit.Text = Util.GS("MB");
				}

				if(ds.AvailableSpace == 0)
				{
					availValue.Text = Util.GS("N/A");
					availUnit.Text = "";
				}
				else
				{
					tmpValue = (int)(ds.AvailableSpace / (1024 * 1024));
					availValue.Text = string.Format("{0}",tmpValue);
					availUnit.Text = Util.GS("MB");
				}

				if(ds.UsedSpace == 0)
				{
					usedValue.Text = Util.GS("N/A");
					usedUnit.Text = "";
				}
				else
				{
					tmpValue = (int)(ds.UsedSpace / (1024 * 1024)) + 1;
					usedValue.Text = string.Format("{0}", tmpValue);
					usedUnit.Text = Util.GS("MB");
				}

				if(ds.Limit == 0)
				{
					diskGraph.Fraction = 0;
				}
				else
				{
					if(ds.Limit < ds.UsedSpace)
						diskGraph.Fraction = 1;
					else
						diskGraph.Fraction = ((double)ds.UsedSpace) / 
												((double)ds.Limit);
				}
			}
			vbox.ShowAll();

		}




	}
}
