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

namespace Novell.iFolder
{

	/// <summary>
	/// This is the properties dialog for an iFolder.
	/// </summary>
	public class iFolderPropertiesDialog : Dialog
	{
		private iFolderWebService	ifws;
		private iFolder				ifolder;
		private Gtk.Notebook		propNoteBook;
		private Hashtable			ifHash;
		private HBox				ConflictBox;
		private HBox				ConflictHolder;
		private iFolderConflictDialog ConflictDialog;

		private Gtk.Combo			iFolderPickCombo;
		private Gtk.Button			iFolderPickButton;

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



		/// <summary>
		/// Default constructor for iFolderPropertiesDialog
		/// </summary>
		public iFolderPropertiesDialog(	Gtk.Window parent,
										iFolder ifolder, 
										iFolderWebService iFolderWS)
			: base()
		{
			if(iFolderWS == null)
				throw new ApplicationException("iFolderWebServices was null");
			this.ifws = iFolderWS;
			this.ifolder = ifolder;
			this.HasSeparator = false;
			this.Modal = true;
			if(parent != null)
				this.TransientFor = parent;
			this.Title = "iFolder Properties";

			ifHash = new Hashtable();

			InitializeWidgets();
			iFolderPickCombo.Entry.Text = this.ifolder.UnManagedPath;
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

			this.SetDefaultSize (100, 400);
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder.png"));

			//-----------------------------
			// Create iFolder Picker List
			//-----------------------------
			HBox pickBox = new HBox();
			dialogBox.PackStart(pickBox, false, true, 0);
			pickBox.Spacing = 10;
			Label pickLabel = new Label("iFolder:");
			pickLabel.Xalign = 0;
			pickBox.PackStart(pickLabel, false, false, 0);

			iFolderPickCombo = new Combo();
			pickBox.PackStart(iFolderPickCombo, true, true, 0);
			iFolderPickCombo.Entry.Editable = false;

			iFolder[]	iFolderArray;
			try
			{
				iFolderArray = this.ifws.GetAlliFolders();
			}
			catch(Exception e)
			{
				iFolderExceptionDialog ied = new iFolderExceptionDialog(
													this, e);
				ied.Run();
				ied.Hide();
				ied.Destroy();
				return;
			}

			ArrayList ifList = new ArrayList();
			foreach(iFolder ifldr in iFolderArray)
			{
				ifList.Add(ifldr.UnManagedPath);
				ifHash[ifldr.UnManagedPath] = ifldr;
			}

			string[] strList = (string[])ifList.ToArray(typeof(string));
			iFolderPickCombo.PopdownStrings = strList;

			iFolderPickButton = new Button(Stock.Open);
			pickBox.PackStart(iFolderPickButton, false, false, 0);
			iFolderPickButton.Clicked += 
				new EventHandler(OnOpenCurrentiFolder);

			// set the text to blank so we will refresh when we set it
			iFolderPickCombo.Entry.Text = "";
			iFolderPickCombo.Entry.Changed += 
						new EventHandler(OniFolderChanged);


			//-----------------------------
			// Create iFolder Conflict
			//-----------------------------
			ConflictHolder = new HBox();
			dialogBox.PackStart(ConflictHolder, false, true, 0);

			//-----------------------------
			// Create iFolder Notebook
			//-----------------------------
			propNoteBook = new Gtk.Notebook();

			iFolderPropSettingsPage settingsPage = 
				new iFolderPropSettingsPage(ifolder, ifws);

			propNoteBook.AppendPage(settingsPage, new Label("General"));

			iFolderPropSharingPage sharingPage = 
				new iFolderPropSharingPage(this, ifolder, ifws);

			propNoteBook.AppendPage(sharingPage, new Label("Sharing"));

			dialogBox.PackStart(propNoteBook);

			this.VBox.ShowAll();

			this.AddButton(Stock.Close, ResponseType.Ok);
			this.AddButton(Stock.Help, ResponseType.Help);
		}




		private void SetValues()
		{
			string name = ifolder.Name;
			this.Title = string.Format("iFolder Properties for \"{0}\"",name);

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
								Util.ImagesPath("ifolder-collision.png"));
					Image conImage = new Image(conPix);

					conImage.SetAlignment(0.5F, 0);
					ConflictBox.PackStart(conImage, false, false, 0);

					Gtk.Label l = new Label("<span weight=\"bold\">" +
								"This iFolder contains conflicts." +
								"</span>");
					l.LineWrap = true;
					l.Xalign = 0;
					l.UseMarkup = true;
					ConflictBox.PackStart(l, true, true, 0);

					Button resButton = new Button("_Resolve conflicts");
					ConflictBox.PackStart(resButton, false, false, 0);
					resButton.Clicked += new EventHandler(OnResolveConflicts);


					ConflictHolder.PackStart(ConflictBox, false, true, 10);
					ConflictBox.ShowAll();
				}
				else
					ConflictBox.Visible = true;
			}
		}




		private void OniFolderChanged(object o, EventArgs args)
		{
			if(iFolderPickCombo.Entry.Text.Length > 0)
			{
				ifolder = (iFolder)ifHash[iFolderPickCombo.Entry.Text];
				SetValues();
			}
		}




		private void OnResolveConflicts(object o, EventArgs args)
		{
			ConflictDialog = new iFolderConflictDialog(
										this,
										ifolder,
										ifws);
			ConflictDialog.Response += 
						new ResponseHandler(OnConflictDialogResponse);
			ConflictDialog.ShowAll();
		}



		private void OnConflictDialogResponse(object o, ResponseArgs args)
		{
			if(ConflictDialog != null)
			{
				ConflictDialog.Hide();
				ConflictDialog.Destroy();
				ConflictDialog = null;
			}
			// CRG: TODO
			// At this point, refresh the selected iFolder to see if it
			// has any more conflicts
		}

		private void OnOpenCurrentiFolder(object o, EventArgs args)
		{
				try
				{
					Util.OpenInBrowser(ifolder.UnManagedPath);
				}
				catch(Exception e)
				{
					iFolderMsgDialog dg = new iFolderMsgDialog(
						this,
						iFolderMsgDialog.DialogType.Error,
						iFolderMsgDialog.ButtonSet.Ok,
						"iFolder Error",
						"Unable to launch File Browser",
						"iFolder attempted to open the Nautilus File Manager and the Konqueror File Manager and was unable to launch either of them.");
					dg.Run();
					dg.Hide();
					dg.Destroy();
				}
		}

	}
}
