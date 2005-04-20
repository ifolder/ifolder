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
using System.Collections;
using Gtk;

using Simias.Client;

namespace Novell.iFolder
{

	/// <summary>
	/// This is the properties dialog for an iFolder.
	/// </summary>
	public class iFolderPropertiesDialog : Dialog
	{
		private iFolderWebService	ifws;
		private SimiasWebService	simws;
		private iFolderWeb		ifolder;
		private Gtk.Notebook		propNoteBook;
//		private Hashtable		ifHash;
		private HBox			ConflictBox;
		private HBox			ConflictHolder;
		private iFolderConflictDialog 	ConflictDialog;
		private iFolderPropSharingPage 	SharingPage;
		private iFolderPropSettingsPage SettingsPage; 

		public int CurrentPage
		{
			set
			{
				if(value <= propNoteBook.NPages)
					propNoteBook.CurrentPage = value;
			}
			get
			{
				return propNoteBook.CurrentPage;
			}
		}
		
		public iFolderWeb iFolder
		{
			get
			{
				return ifolder;
			}
		}



		/// <summary>
		/// Default constructor for iFolderPropertiesDialog
		/// </summary>
		public iFolderPropertiesDialog(	Gtk.Window parent,
										iFolderWeb ifolder, 
										iFolderWebService iFolderWS,
										SimiasWebService SimiasWS)
			: base()
		{
			if(iFolderWS == null)
				throw new ApplicationException("iFolderWebService was null");
			this.ifws = iFolderWS;
			if(SimiasWS == null)
				throw new ApplicationException("SimiasWebService was null");
			this.simws = SimiasWS;

			// Make sure that we have the latest information by forcing this
			// a reread from the server.
			try
			{
				this.ifolder = this.ifws.GetiFolder(ifolder.ID);
			}
			catch(Exception e)
			{
				throw new ApplicationException(
						"Unable to read the iFolder");
			}
			
			this.HasSeparator = false;
			this.Modal = true;
			if(parent != null)
				this.TransientFor = parent;
			this.Title = Util.GS("iFolder Properties");

//			ifHash = new Hashtable();

			InitializeWidgets();
			SetValues();
		}




		/// <summary>
		/// Default constructor for iFolderPropertiesDialog
		/// </summary>
		public iFolderPropertiesDialog(	string ifolderID )
			: base()
		{
			String localServiceUrl =
				Simias.Client.Manager.LocalServiceUrl.ToString();

			this.ifws = new iFolderWebService();
			if(this.ifws == null)
				throw new ApplicationException(
							"Unable to obtain iFolderWebService");
			this.ifws.Url = localServiceUrl + "/iFolder.asmx";
			LocalService.Start(this.ifws);
			
			this.simws = new SimiasWebService();
			if (this.simws == null)
				throw new ApplicationException(
							"Unable to obtain SimiasWebService");
			this.simws.Url = localServiceUrl + "/Simias.asmx";
			LocalService.Start(this.simws);

			try
			{
				this.ifolder = this.ifws.GetiFolder(ifolderID);
			}
			catch(Exception e)
			{
				throw new ApplicationException(
						"Unable to read the iFolder");
			}

			this.HasSeparator = false;
			this.Modal = true;
			this.Title = Util.GS("iFolder Properties");

//			ifHash = new Hashtable();

			InitializeWidgets();
			SetValues();
		}
		
		public void UpdateiFolder(iFolderWeb theiFolder)
		{
			SettingsPage.UpdateiFolder(theiFolder);
			SharingPage.UpdateiFolder(theiFolder);
		}




		/// <summary>
		/// Setup the UI inside the Window
		/// </summary>
		private void InitializeWidgets()
		{
			VBox dialogBox = new VBox();
			this.VBox.PackStart(dialogBox);
			dialogBox.BorderWidth = 10;
			dialogBox.Spacing = 10;

			this.SetDefaultSize (480, 480);
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder24.png"));

			//-----------------------------
			// Create iFolder Conflict
			//-----------------------------
			ConflictHolder = new HBox();
			dialogBox.PackStart(ConflictHolder, false, true, 0);

			//-----------------------------
			// Create iFolder Notebook
			//-----------------------------
			propNoteBook = new Gtk.Notebook();

			SettingsPage = new iFolderPropSettingsPage(this, ifws);

			propNoteBook.AppendPage(SettingsPage, 
								new Label(Util.GS("_General")));

			SharingPage = new iFolderPropSharingPage(this, ifws, simws);

			propNoteBook.AppendPage(SharingPage, 
								new Label(Util.GS("_Sharing")));

			dialogBox.PackStart(propNoteBook);

			this.VBox.ShowAll();

			this.AddButton(Stock.Close, ResponseType.Ok);
			this.AddButton(Stock.Help, ResponseType.Help);
		}




		private void SetValues()
		{
			if(!ifolder.HasConflicts)
			{
				if(ConflictBox != null)
					ConflictBox.Visible = false;
			}
			else
			{
				if(ConflictBox == null)
				{
					ConflictBox = new HBox();
					ConflictBox.Spacing = 5;
					ConflictBox.BorderWidth = 10;

					Gdk.Pixbuf conPix = new Gdk.Pixbuf(
								Util.ImagesPath("conflict24.png"));
					Image conImage = new Image(conPix);

					conImage.SetAlignment(0.5F, 0);
					ConflictBox.PackStart(conImage, false, false, 0);

					Gtk.Label l = new Label("<span weight=\"bold\">" +
								Util.GS("This iFolder contains conflicts.") +
								"</span>");
					l.LineWrap = true;
					l.Xalign = 0;
					l.UseMarkup = true;
					ConflictBox.PackStart(l, true, true, 0);

					Button resButton = new Button(Util.GS("_Resolve conflicts"));
					ConflictBox.PackStart(resButton, false, false, 0);
					resButton.Clicked += new EventHandler(OnResolveConflicts);


					ConflictHolder.PackStart(ConflictBox, false, true, 10);
					ConflictBox.ShowAll();
				}
				else
					ConflictBox.Visible = true;
			}

			UpdateiFolder(ifolder);
		}




		private void OnResolveConflicts(object o, EventArgs args)
		{
			ConflictDialog = new iFolderConflictDialog(
										this,
										ifolder,
										ifws,
										simws);
			ConflictDialog.Response += 
						new ResponseHandler(OnConflictDialogResponse);
			ConflictDialog.ShowAll();
		}



		private void OnConflictDialogResponse(object o, ResponseArgs args)
		{
			if(ConflictDialog != null)
			{
				if (args.ResponseId == ResponseType.Help)
					Util.ShowHelp("conflicts.html", this);
				else
				{
					ConflictDialog.Hide();
					ConflictDialog.Destroy();
					ConflictDialog = null;
				}
			}
			// CRG: TODO
			// At this point, refresh the selected iFolder to see if it
			// has any more conflicts
		}

//		private void OnOpenCurrentiFolder(object o, EventArgs args)
//		{
//				try
//				{
//					Util.OpenInBrowser(ifolder.UnManagedPath);
//				}
//				catch(Exception e)
//				{
//					iFolderMsgDialog dg = new iFolderMsgDialog(
//						this,
//						iFolderMsgDialog.DialogType.Error,
//						iFolderMsgDialog.ButtonSet.Ok,
//						Util.GS("iFolder Error"),
//						Util.GS("Unable to launch File Browser"),
//						Util.GS("iFolder attempted to open the Nautilus File Manager and the Konqueror File Manager and was unable to launch either of them."));
//					dg.Run();
//					dg.Hide();
//					dg.Destroy();
//				}
//		}

	}
}
