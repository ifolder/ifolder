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
 *  Authors:
 *		Boyd Timothy <btimothy@novell.com>
 *
 ***********************************************************************/

using System;
using System.IO;
using System.Collections;
using Gtk;

using Simias.Client;
using Simias.Client.Event;

using Novell.iFolder.Events;
using Novell.iFolder.Controller;

namespace Novell.iFolder
{
	public class ChooseRemoteFolderDialog : Dialog
	{
		private iFolderWebService	ifws;
		private SimiasWebService	simws;

		private RemoteFoldersWidget remoteFoldersWidget;
		
		public iFolderHolder SelectedFolder
		{
			get
			{
				return remoteFoldersWidget.SelectedFolder;
			}
		}

		public ChooseRemoteFolderDialog(iFolderWebService ifws, SimiasWebService simws) : base()
		{
			this.Title = "Download a folder...";
			this.SetDefaultSize (600, 500);
			this.Resizable = true;
			this.HasSeparator = false;
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder24.png"));
			
			this.ifws = ifws;
			this.simws = simws;
			
			CreateWidgets();
		}
		
		public void CreateWidgets()
		{
			remoteFoldersWidget = new RemoteFoldersWidget(ifws, simws);
			this.VBox.PackStart(remoteFoldersWidget, true, true, 0);
			
			remoteFoldersWidget.FolderSelected +=
				new FolderSelectedHandler(OnFolderSelected);
				
			this.VBox.ShowAll();
			
			Button cancelButton = new Button(Stock.Cancel);
			cancelButton.CanFocus = true;
			cancelButton.CanDefault = false;
			cancelButton.ShowAll();
			
			this.AddButton(Stock.Cancel, ResponseType.Cancel);
			
			Button downloadButton = new Button(Util.GS("Download"));
			downloadButton.ShowAll();
			this.AddActionWidget(downloadButton, ResponseType.Ok);
			
			this.DefaultResponse = ResponseType.Ok;
			
			this.SetResponseSensitive(ResponseType.Ok, false);
		}
		
		private void OnFolderSelected(object o, FolderSelectedArgs args)
		{
			if (args.Folder == null)
				this.SetResponseSensitive(ResponseType.Ok, false);
			else
				this.SetResponseSensitive(ResponseType.Ok, true);
		}
	}
}