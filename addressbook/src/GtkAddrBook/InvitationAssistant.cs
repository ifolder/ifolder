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
using System.IO;
using System.Drawing;
using Simias.Sync;
using Simias.POBox;
using Simias.Storage;
using Simias;

using Gtk;
using Gdk;
using Glade;
using GtkSharp;
using GLib;

namespace Novell.AddressBook.UI.gtk
{
	/// <summary>
	/// This class represents the Gtk Invitation Assistant.  The UI is loaded
	/// using glade classes from a file named ifolder.glade.  Changes to the
	/// layout of the druid pages should be done using Glade.
	/// </summary>
	public class InvitationAssistant
	{
		/// <summary>
		/// autoloaded widget being the druid
		/// </summary>
		[Glade.Widget] private Gnome.Druid	InvitationDruid = null;

		/// <summary>
		/// autoloaded widget being the druid
		/// </summary>
//		[Glade.Widget] private Gnome.DruidPageStandard	SelectFilePage;

		/// <summary>
		/// autoloaded widget being the druid
		/// </summary>
//		[Glade.Widget] private Gnome.DruidPageStandard	SelectLocationPage;

		/// <summary>
		/// autoloaded widget used to enter invitation path
		/// </summary>
		[Glade.Widget] private Gtk.Entry	InvitationEntry = null;

		/// <summary>
		/// autoloaded widget to browse for invitations
		/// </summary>
		[Glade.Widget] private Gtk.Button OpenInvitationButton = null;

		/// <summary>
		/// autoloaded widget used to enter collection location
		/// </summary>
		[Glade.Widget] private Gtk.Entry CollectionPathEntry = null;

		/// <summary>
		/// autoloaded widget to browse for collection location
		/// </summary>
		[Glade.Widget] private Gtk.Button CollectionBrowseButton = null;

		/// <summary>
		/// autoloaded widget Name label
		/// </summary>
		[Glade.Widget] private Gtk.Label CollectionNameLabel = null;

		/// <summary>
		/// autoloaded widget Type label
		/// </summary>
		[Glade.Widget] private Gtk.Label CollectionTypeLabel = null;

		/// <summary>
		/// autoloaded widget From label
		/// </summary>
		[Glade.Widget] private Gtk.Label CollectionFromLabel = null;

		/// <summary>
		/// autoloaded widget Rights label
		/// </summary>
		[Glade.Widget] private Gtk.Label SelectLabel = null;

		/// <summary>
		/// autoloaded widget Rights label
		/// </summary>
		[Glade.Widget] private Gtk.Label SelectPathLabel = null;

		/// <summary>
		/// autoloaded widget Main Window
		/// </summary>
		[Glade.Widget] private Gtk.Window InviteWindow = null;

		/// <summary>
		/// Storage of the invitation file to process
		/// </summary>
		private string 	inviteFile = null;

		private SubscriptionInfo	subInfo = null;
		private Subscription		subscription = null;
		private Store				store = null;
		private POBox				pobox = null;

		public event EventHandler AssistantClosed;


#region Constructors
		/// <summary>
		/// Constructor for creating a new InvitationAssistant without loading
		/// an invitation.  In this mode, the Load page will be shown.
		/// </summary>
		public InvitationAssistant()
		{
			inviteFile = "";
//			showLoadPage = true;
			Init_Glade();
		}

		/// <summary>
		/// Constructor for creating a new InvitationAssistant.
		/// In this mode, the Load page will not be shown.
		/// </summary>
		/// <param name="inviteFile">Full path to the invitation file</param>
		public InvitationAssistant(string inviteFile)
		{
			this.inviteFile = inviteFile;
//			showLoadPage = false;
			Init_Glade();
		}
#endregion

		/// <summary>
		/// Calls into Glade.XML and loads all pages from the glade
		/// project file invitation-assistant.glade.
		/// </summary>
		private void Init_Glade()
		{
			Glade.XML mainXml = 
				new Glade.XML (Util.GladePath("invitation-assistant.glade"), 
				"InviteWindow", 
				null);
			mainXml.Autoconnect (this);
		}

		/// <summary>
		/// Displays all windows and child widgets.
		/// </summary>
		public void ShowAll()
		{
			if(InviteWindow != null)
			{
				InviteWindow.ShowAll();
			}
		}

		private void on_close(object o, EventArgs args) 
		{
			InviteWindow.Hide();
			InviteWindow.Destroy();
			InviteWindow = null;

			if(AssistantClosed != null)
			{
				EventArgs e = new EventArgs();
				AssistantClosed(this, e);
			}
		}

		/// <summary>
		/// Standard Delete Event handler to quit application and destroy 
		/// all of the windows.
		/// </summary>
		private void on_delete_event(object o, DeleteEventArgs args) 
		{
			args.RetVal = true;
			on_close(o, args);
		}

		private void on_select_file_prepare(object o, EventArgs args)
		{
			// Setup the buttons				back  next   cancl help
			if(InvitationEntry.Text.Length > 0)
				InvitationDruid.SetButtonsSensitive(true, true, true, true);
			else
				InvitationDruid.SetButtonsSensitive(true, false, true, true);
		}

		private void on_InvitationEntry_changed(object o, EventArgs args)
		{
			// Setup the buttons				    back  next  cancl help
			if(InvitationEntry.Text.Length > 0)
				InvitationDruid.SetButtonsSensitive(true, true, true, true);
			else
				InvitationDruid.SetButtonsSensitive(true, false, true, true);
		}

		private void on_OpenInvitationButton_clicked(object o, EventArgs args)
		{
			// create a file selection dialog and turn off all of the
			// file operations and controlls
			FileSelection fs = new FileSelection ("Choose an invitation file");
			fs.ShowFileops = false;
			if(InvitationEntry.Text.Length > 0)
			{
				// add a slash on the end because it's strange
				// the way this file browser handles directories
				fs.Filename = InvitationEntry.Text;
			}

			int rc = fs.Run ();
			fs.Hide ();
			if(rc == -5)
			{
				try
				{
					InitSubscriptionInfo(fs.Filename);
					InvitationEntry.Text = fs.Filename;
				}
				catch(Exception e)
				{
					MessageDialog md = new MessageDialog(InviteWindow,
						DialogFlags.DestroyWithParent | DialogFlags.Modal,
						MessageType.Error,
						ButtonsType.Close,
						"The selected file was not a valid invitation file.");
					md.Run();
					md.Hide();
					InvitationEntry.Text = "";
					InvitationDruid.SetButtonsSensitive(true, false, 
															true, true);
				}
			}
		}

		private void InitSubscriptionInfo(string filename)
		{
			if(store == null)
				store = new Store(new Configuration());
			
			subInfo = new SubscriptionInfo(filename);

			pobox = POBox.GetPOBox(store, subInfo.DomainID);
			Node node = pobox.GetNodeByID(subInfo.SubscriptionID);
			if(node != null)
			{
				subscription = new Subscription(node);
			}
			else
			{
				subscription = new Subscription("Subscription Name",
													subInfo);
				subscription.SubscriptionState = SubscriptionStates.Received;
				pobox.AddMessage(subscription);
			}
		}

		private void on_select_location_prepare(object o, EventArgs args)
		{
			if((subInfo != null) && (subscription != null))
			{
				CollectionNameLabel.Text = 
						subscription.SubscriptionCollectionName;
				if( (subscription.SubscriptionCollectionType != null) && 
						(subscription.SubscriptionCollectionType.Length > 0) )
					CollectionTypeLabel.Text = 
							subscription.SubscriptionCollectionType;
				else
					CollectionTypeLabel.Text = "iFolder";

				CollectionFromLabel.Text = subscription.FromName;

				if(!subInfo.SubscriptionCollectionHasDirNode)
				{
					SelectLabel.Hide();
					SelectPathLabel.Hide();
					CollectionPathEntry.Hide();
					CollectionBrowseButton.Hide();
				}

				// Setup the buttons				back  next   cancl help
				if( (subInfo.SubscriptionCollectionHasDirNode) && 
						(CollectionPathEntry.Text.Length == 0))
					InvitationDruid.SetButtonsSensitive(true, false, 
															true, true);
				else
					InvitationDruid.SetButtonsSensitive(true, true, true, true);
			}
			else
				InvitationDruid.SetButtonsSensitive(true, false, true, true);
		}

		private void on_collection_path_changed(object o, EventArgs args)
		{
			// Setup the buttons				    back  next  cancl help
			if(CollectionPathEntry.Text.Length > 0)
				InvitationDruid.SetButtonsSensitive(true, true, true, true);
			else
				InvitationDruid.SetButtonsSensitive(true, false, true, true);
		}

		private void on_collection_path_browse_clicked(object o, EventArgs args)
		{
			// create a file selection dialog and turn off all of the
			// file operations and controlls
			FileSelection fs = new FileSelection ("Choose a directory");
			fs.FileList.Parent.Hide();
			fs.SelectionEntry.Hide();
			fs.FileopDelFile.Hide();
			fs.FileopRenFile.Hide();
			if(CollectionPathEntry.Text.Length > 0)
			{
				// add a slash on the end because it's strange
				// the way this file browser handles directories
				fs.Filename = CollectionPathEntry.Text;
				fs.Filename += "/";
			}

			int rc = fs.Run ();
			fs.Hide ();
			if(rc == -5)
			{
				CollectionPathEntry.Text = fs.Filename;
			}
		}

		private void on_select_file_next(object o, EventArgs args)
		{
			// We should be doing the verification here but
			// I can't figure out how to stop the druid from 
			// moving on if the file is invalid
		}

		private void on_druid_finish(object o, EventArgs args)
		{
			try
			{
				subscription.SubscriptionState = SubscriptionStates.Replied;
				subscription.SubscriptionDisposition = 
						SubscriptionDispositions.Accepted;
				Member member = pobox.GetCurrentMember();
				subscription.FromName = member.Name;
				subscription.FromIdentity = member.UserID;
				pobox.Commit(subscription);
			}
			catch(SimiasException ex)
			{
				Console.WriteLine(ex);
			}
			on_close(o, args);
		}

		private void on_druid_cancel(object o, EventArgs args)
		{
			on_close(o, args);
		}

		/*
		/// <summary>
		/// Standard Event handler to respond to a click on the Wizard's
		/// back button.
		/// </summary>
		private void on_back_clicked(object o, EventArgs args) 
		{
		switch(page)
		{
		case IW_LOAD_PAGE:
		{
		WizardBox.Remove(loadPage);
		WizardBox.PackEnd(welcomePage);
		BackButton.Sensitive = false;
		ForwardButton.Sensitive = true;
		page = IW_WELCOME_PAGE;
		break;
		}

		case IW_ACCEPT_PAGE:
		{
		WizardBox.Remove(acceptPage);

		if(showLoadPage)
		{
		MoveToLoadPage(); 
		}
		else
		{
		WizardBox.PackEnd(welcomePage);
		BackButton.Sensitive = false;
		ForwardButton.Sensitive = true;
		page = IW_WELCOME_PAGE;
		}
		break;
		}
		}
		}

		/// <summary>
		/// Standard Event handler to respond to a click on the Wizard's
		/// forward button.
		/// </summary>
		private void on_forward_clicked(object o, EventArgs args) 
		{
		switch(page)
		{
		case IW_WELCOME_PAGE:
		{
		WizardBox.Remove(welcomePage);
		BackButton.Sensitive = true;

		if(showLoadPage)
		{
		MoveToLoadPage(); 
		}
		else
		{
		MoveToAcceptPage(); 
		}
		break;
		}

		case IW_LOAD_PAGE:
		{
		WizardBox.Remove(loadPage);
		inviteFile = loadPathEntry.Text;
		MoveToAcceptPage(); 
		break;
		}

		case IW_ACCEPT_PAGE:
		{
			WizardBox.Remove(acceptPage);
			MoveToFinalPage();
			break;
		}
	}
	}

	/// <summary>
	/// Moves the wizard to the Final page
	/// </summary>
	private void MoveToFinalPage() 
	{
		iFolderManager manager = iFolderManager.Connect();

		if(manager == null)
		{
			Console.WriteLine("Unable to connect to iFolderManager");
			MoveToAcceptPage();
			return;
		}

		Console.WriteLine("Testing path :" + acceptPathEntry.Text);
		if(manager.IsPathIniFolder(acceptPathEntry.Text))
		{
			MessageDialog md = new MessageDialog(win,
					DialogFlags.DestroyWithParent | DialogFlags.Modal,
					MessageType.Error,
					ButtonsType.Close,
					"The location selected for the new iFolder is below an existing iFolder and cannot be used.  Please select a new location.");
			md.Run();
			md.Hide();

			MoveToAcceptPage();
			return;
		}

		Console.WriteLine("Accepting Invitation");
		try
		{
			manager.AcceptInvitation(invitation, acceptPathEntry.Text);
		}
		catch(Exception e)
		{
			Console.WriteLine("Accept failed, path in an existing iFolder");

			MessageDialog md = new MessageDialog(win,
					DialogFlags.DestroyWithParent | DialogFlags.Modal,
					MessageType.Error,
					ButtonsType.Close,
					"Unable to accept the invitation due to an error while processing the invitation.\n" + e);
			md.Run();
			md.Hide();

			// Go back to the Load Page so they can choose another
			// invitation file to load
			MoveToAcceptPage();
			return;
		}

		Console.WriteLine("Displaying Results");

		WizardBox.PackEnd(finalPage);
		BackButton.Sensitive = false;
		ForwardButton.Sensitive = false;
		ButtonHBox.Remove(CancelButton);
		Button OKButton = new Button(Gtk.Stock.Ok);
		OKButton.Clicked += new EventHandler(on_cancel_clicked);
		ButtonHBox.PackEnd(OKButton, true, true, 0);
		ButtonHBox.ShowAll();
		page = IW_FINAL_PAGE;

		finaliFolderName.Text = invitation.CollectionName;
		finalSharerName.Text = invitation.FromName;
		finalSharerEmail.Text = invitation.FromEmail;
		finalRights.Text = invitation.CollectionRights;
		finalLocation.Text = Path.Combine(acceptPathEntry.Text, invitation.CollectionName);
	}

	/// <summary>
	/// Moves the wizard to the Accept page
	/// </summary>
	private void MoveToAcceptPage() 
	{
		invitation = new Invitation();
		try
		{
			invitation.Load(inviteFile);
		}
		catch(Exception e)
		{
			Console.WriteLine("Unable to load file: {0}", inviteFile);

			MessageDialog md = new MessageDialog(win,
					DialogFlags.DestroyWithParent | DialogFlags.Modal,
					MessageType.Error,
					ButtonsType.Close,
					"Unable to open file or file is not an iFolder Invitation:\n" + inviteFile);
			md.Run();
			//Console.WriteLine("Response was: " + result);
			md.Hide();

			// Go back to the Load Page so they can choose another
			// invitation file to load
			MoveToLoadPage();

			return;
		}

		WizardBox.PackEnd(acceptPage);
		BackButton.Sensitive = true;
		if(acceptPathEntry.Text.Length > 0)
			ForwardButton.Sensitive = true;
		else
			ForwardButton.Sensitive = false;

		page = IW_ACCEPT_PAGE;
		acceptiFolderName.Text = invitation.CollectionName;
		acceptSharerName.Text = invitation.FromName;
		acceptSharerEmail.Text = invitation.FromEmail;
		acceptRights.Text = invitation.CollectionRights;
		if(acceptPathEntry.Text.Length < 1)
			acceptPathEntry.Text = Invitation.DefaultRootPath;
	}

	/// <summary>
	/// Moves the wizard to the Load page
	/// </summary>
	private void MoveToLoadPage() 
	{
		showLoadPage = true;

		if(inviteFile.Length > 0)
			loadPathEntry.Text = inviteFile;

		WizardBox.PackEnd(loadPage);

		if(loadPathEntry.Text.Length > 0)
			ForwardButton.Sensitive = true;
		else
			ForwardButton.Sensitive = false;

		page = IW_LOAD_PAGE;
	}

	/// <summary>
	/// Standard Event handler to respond to a click on the Wizard's
	/// cancel button.
	/// </summary>
	private void on_cancel_clicked(object o, EventArgs args) 
	{
		on_close(o, args);
	}

	/// <summary>
	/// Standard Event handler to respond to a click on the browse
	/// button on the Load wizard page.
	/// </summary>
	private void on_load_browse_clicked(object o, EventArgs args) 
	{
		// create a file selection dialog and turn off all of the
		// file operations and controlls
		FileSelection fs = new FileSelection ("Choose an invitation file");
		fs.ShowFileops = false;
		if(loadPathEntry.Text.Length > 0)
		{
			// add a slash on the end because it's strange
			// the way this file browser handles directories
			fs.Filename = loadPathEntry.Text;
		}

		int rc = fs.Run ();
		if(rc == -5)
			loadPathEntry.Text = fs.Filename;
		fs.Hide ();
	}

	/// <summary>
	/// Standard Event handler to respond to a change in the entry
	/// widget for the path to the invitation file to load.
	/// </summary>
	private void on_load_path_changed(object o, EventArgs args) 
	{
		if(loadPathEntry.Text.Length > 0)
			ForwardButton.Sensitive = true;
		else
			ForwardButton.Sensitive = false;
	}

	/// <summary>
	/// Standard Event handler to respond to a click on the browse
	/// button on the Accpet Wizard page.
	/// </summary>
	private void on_accept_browse_clicked(object o, EventArgs args) 
	{
		// create a file selection dialog and turn off all of the
		// file operations and controlls
		FileSelection fs = new FileSelection ("Choose a directory");
		fs.FileList.Parent.Hide();
		fs.SelectionEntry.Hide();
		fs.FileopDelFile.Hide();
		fs.FileopRenFile.Hide();
		if(acceptPathEntry.Text.Length > 0)
		{
			// add a slash on the end because it's strange
			// the way this file browser handles directories
			fs.Filename = acceptPathEntry.Text;
			fs.Filename += "/";
		}

		int rc = fs.Run ();
		if(rc == -5)
			acceptPathEntry.Text = fs.Filename;
		fs.Hide ();
	}

	/// <summary>
	/// Standard Event handler to respond to a change in the text
	/// in the Entry field which holds the path to the folder where
	/// the iFolder is going to be accepted.
	/// </summary>
	private void on_accept_path_changed(object o, EventArgs args) 
	{
		if(acceptPathEntry.Text.Length > 0)
			ForwardButton.Sensitive = true;
		else
			ForwardButton.Sensitive = false;
	}
	*/



	}
}
