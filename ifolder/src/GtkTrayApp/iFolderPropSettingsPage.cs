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

			// Main Sync Box
//			Frame SyncFrame = new Frame("Synchronization");
//			this.PackStart(SyncFrame, true, true, 0);

			VBox vbox = new VBox();
			vbox.Spacing = 10;
			vbox.BorderWidth = 10;

			AutoSyncCheckButton = new CheckButton("Automatic Sync");
			vbox.PackStart(AutoSyncCheckButton, false, false, 0);



			// Sync To Host Box
			Frame SyncToHostFrame = new Frame("Synchronize to host");
			vbox.PackStart(SyncToHostFrame, true, true, 0);

			VBox syncVBox = new VBox();
			syncVBox.Spacing = 10;
			syncVBox.BorderWidth = 10;

			Label syncHelp = new Label("This sets the value for how often the host will be contacted to perform a sync.");
			syncHelp.LineWrap = true;
			syncVBox.PackStart(syncHelp, false, false, 0);

			HBox syncHBox = new HBox();
			syncHBox.Spacing = 10;

			Label syncLabel = new Label("Sync to host every:");
			syncHBox.PackStart(syncLabel, false, false, 0);

			SpinButton syncSpinButton = new SpinButton(0, 99999, 1);
			syncHBox.PackStart(syncSpinButton, false, false, 0);

			Label syncValue = new Label("seconds");
			syncValue.Xalign = 0;
			syncHBox.PackEnd(syncValue, true, true, 0);
			syncVBox.PackEnd(syncHBox, false, false, 0);
			SyncToHostFrame.Add(syncVBox);


			// Statistics Box
			Frame StatisticsFrame = new Frame("Statistics");

			Table statsTable = new Table(2,2,false);
			statsTable.BorderWidth = 10;
			statsTable.RowSpacing = 10;
			statsTable.ColumnSpacing = 10;

			Label amountLabel = new Label("Amount to upload:");
			amountLabel.Xalign = 0;
			statsTable.Attach(amountLabel, 0,1,0,1);

			Label amountValue = new Label("0");
			amountValue.Xalign = 1;
			statsTable.Attach(amountValue, 1,2,0,1);

			Label itemsLabel = new Label("Items to synchronize:");
			itemsLabel.Xalign = 0;
			statsTable.Attach(itemsLabel, 0,1,1,2);

			Label itemsValue = new Label("0");
			itemsValue.Xalign = 1;
			statsTable.Attach(itemsValue, 1,2,1,2);

			StatisticsFrame.Add(statsTable);

			vbox.PackStart(StatisticsFrame, true, true, 0);


			this.PackStart(vbox, true, true, 0);
//			SyncFrame.Add(vbox);

		}
	}
}
