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
using Simias.Invite;

using Gtk;
using Gdk;
using Gnome;
using Glade;
using GtkSharp;
using GLib;

namespace Novell.iFolder
{
	/// <summary>
	/// This class represents the Gtk Invitation Wizard.  The UI is loaded
	/// using glade classes from a file named ifolder.glade.  Changes to the
	/// layout of the wizard pages should be done using Glade.
	/// </summary>
	public class InvitationWizard 
	{
		/// <summary>
		/// autoloaded widget used to hold wizard pages
		/// </summary>
		[Glade.Widget] internal HBox WizardBox;

		/// <summary>
		/// autoloaded widget, wizard "back" button
		/// </summary>
		[Glade.Widget] internal Button BackButton;

		/// <summary>
		/// autoloaded widget, wizard "next" button
		/// </summary>
		[Glade.Widget] internal Button ForwardButton;

		/// <summary>
		/// autoloaded widget, wizard "cancel" button
		/// </summary>
		[Glade.Widget] internal Button CancelButton;

		/// <summary>
		/// autoloaded widget used to hold wizard buttons
		/// </summary>
		[Glade.Widget] internal HBox ButtonHBox;

		/// <summary>
		/// Main windows for the wizard
		/// </summary>
		internal Gtk.Window	win; 

		/// <summary>
		/// Welcome page in the Wizard
		/// </summary>
		internal Gtk.Widget	welcomePage;

		/// <summary>
		/// Accept page where the user will decide to accept the invitaion
		/// </summary>
		internal Gtk.Widget	acceptPage;

		/// <summary>
		/// Load page where user is prompted to browse for an invitation if
		/// one was not passed on the command line.
		/// </summary>
		internal Gtk.Widget	loadPage;

		/// <summary>
		/// Load page where user is taken after accepting the invitation.
		/// </summary>
		internal Gtk.Widget	finalPage;

		/// <summary>
		/// Button used to browse for an invitation to load into the wizard
		/// </summary>
		internal Gtk.Button	loadBrowseButton;

		/// <summary>
		/// Entry used to retrieve the path to the invitation to be loaded
		/// </summary>
		internal Gtk.Entry	loadPathEntry;

		/// <summary>
		/// Button used to browse for a folder to place the accepted invitation
		/// </summary>
		internal Gtk.Button	acceptBrowseButton;

		/// <summary>
		/// Entry used to retrieve the path to the folder to place the ifolder
		/// </summary>
		internal Gtk.Entry	acceptPathEntry;

		/// <summary>
		/// Label on the accpet page for the iFolder name
		/// </summary>
		internal Gtk.Label	acceptiFolderName;

		/// <summary>
		/// Label on the accpet page for the sharer's name
		/// </summary>
		internal Gtk.Label	acceptSharerName;

		/// <summary>
		/// Label on the accpet page for the sharer's email
		/// </summary>
		internal Gtk.Label	acceptSharerEmail;

		/// <summary>
		/// Label on the accpet page for the rights
		/// </summary>
		internal Gtk.Label	acceptRights;

		/// <summary>
		/// Label on the final page for the iFolder name
		/// </summary>
		internal Gtk.Label	finaliFolderName;

		/// <summary>
		/// Label on the final page for the sharer's name
		/// </summary>
		internal Gtk.Label	finalSharerName;

		/// <summary>
		/// Label on the final page for the sharer's email
		/// </summary>
		internal Gtk.Label	finalSharerEmail;

		/// <summary>
		/// Label on the final page for the rights
		/// </summary>
		internal Gtk.Label	finalRights;

		/// <summary>
		/// Label on the final page for the iFolder Location 
		/// </summary>
		internal Gtk.Label	finalLocation;

		/// <summary>
		/// Current page of the Wizard
		/// </summary>
		internal int		page;

		/// <summary>
		/// Storage of the invitation file to process
		/// </summary>
		internal string 	inviteFile;

		/// <summary>
		/// Flag used to indicate if the Load Wizard page should be shown
		/// </summary>
		internal bool		showLoadPage;

		/// <summary>
		/// Flag used to indicate if the Load Wizard page should be shown
		/// </summary>
		internal Invitation invitation;

		/// <summary>
		/// Constant used to represent the Wizard's Wecome page
		/// </summary>
		public const int IW_WELCOME_PAGE = 0;

		/// <summary>
		/// Constant used to represent the Wizard's Load page
		/// </summary>
		public const int IW_LOAD_PAGE = 1;

		/// <summary>
		/// Constant used to represent the Wizard's Accept page
		/// </summary>
		public const int IW_ACCEPT_PAGE = 2;

		/// <summary>
		/// Constant used to represent the Wizard's Final page
		/// </summary>
		public const int IW_FINAL_PAGE = 3;

		public event EventHandler WizardClosed;

#region Constructors
		/// <summary>
		/// Constructor for creating a new InvitationWizard without loading
		/// an invitation.  In this mode, the Load page will be shown.
		/// </summary>
		public InvitationWizard()
		{
			inviteFile = "";
			showLoadPage = true;
			InitWizardGUI();
		}

		/// <summary>
		/// Constructor for creating a new InvitationWizard.
		/// In this mode, the Load page will not be shown.
		/// </summary>
		/// <param name="inviteFile">Full path to the invitation file</param>
		public InvitationWizard(string inviteFile)
		{
			this.inviteFile = inviteFile;
			showLoadPage = false;
			InitWizardGUI();
		}
#endregion

		/// <summary>
		/// Calls into Glade.XML and loads all wizard pages from the glade
		/// project file ifolder.glade.
		/// </summary>
		private void InitWizardGUI() 
		{
			// --- Main Invitation Wizard ---
			Glade.XML mainXml = new Glade.XML ("ifolder.glade", 
					"InviteWizard", 
					null);
			mainXml.Autoconnect (this);
			win = (Gtk.Window) mainXml.GetWidget("InviteWizard");

			// --- Welcome Page ---
			// This page is used to welcome the user to the 
			Glade.XML welcomeXml = new Glade.XML ("ifolder.glade", 
					"WelcomePage", 
					null);
			welcomePage = welcomeXml.GetWidget("WelcomePage");
			WizardBox.PackEnd(welcomePage);

			// --- Load Page ---
			Glade.XML loadXml = new Glade.XML ("ifolder.glade", 
					"LoadPage", 
					null);
			loadPage = loadXml.GetWidget("LoadPage");
			loadBrowseButton = (Gtk.Button)
				loadXml.GetWidget("LoadBrowseButton");
			loadPathEntry = (Gtk.Entry) loadXml.GetWidget("LoadPathEntry");
			loadBrowseButton.Clicked += new 
				EventHandler(on_load_browse_clicked);
			loadPathEntry.Changed += new EventHandler(on_load_path_changed);


			// Load in and hookup the Accept Wizard Page
			Glade.XML acceptXml = new Glade.XML ("ifolder.glade", 
					"AcceptPage", 
					null);
			acceptPage = acceptXml.GetWidget("AcceptPage");
			acceptBrowseButton = (Gtk.Button)
				acceptXml.GetWidget("AcceptBrowseButton");
			acceptPathEntry = (Gtk.Entry)
				acceptXml.GetWidget("AcceptPathEntry");
			acceptiFolderName = (Gtk.Label)
				acceptXml.GetWidget("AcceptIFName");
			acceptSharerName = (Gtk.Label)
				acceptXml.GetWidget("AcceptIFSender");
			acceptSharerEmail = (Gtk.Label)
				acceptXml.GetWidget("AcceptIFSenderEmail");
			acceptRights = (Gtk.Label)
				acceptXml.GetWidget("AcceptIFRights");

			acceptBrowseButton.Clicked += new
				EventHandler(on_accept_browse_clicked);
			acceptPathEntry.Changed += new
				EventHandler(on_accept_path_changed);

			// Load in and hookup the Final Wizard Page
			Glade.XML finalXml = new Glade.XML ("ifolder.glade", 
					"FinalPage", 
					null);
			finalPage = finalXml.GetWidget("FinalPage");
			finaliFolderName = (Gtk.Label)
				finalXml.GetWidget("FinalIFName");
			finalSharerName = (Gtk.Label)
				finalXml.GetWidget("FinalIFSender");
			finalSharerEmail = (Gtk.Label)
				finalXml.GetWidget("FinalIFSenderEmail");
			finalRights = (Gtk.Label)
				finalXml.GetWidget("FinalIFRights");
			finalLocation = (Gtk.Label)
				finalXml.GetWidget("FinalIFLocation");

			page = IW_WELCOME_PAGE;
			BackButton.Sensitive = false;
		}

		/// <summary>
		/// Displays all windows and child widgets.
		/// </summary>
		public void ShowAll()
		{
			if(win != null)
			{
				win.ShowAll();
			}
		}

		private void on_close(object o, EventArgs args) 
		{
			win.Hide();
			win.Destroy();
			win = null;

			if(WizardClosed != null)
			{
				EventArgs e = new EventArgs();
				WizardClosed(this, e);
			}
		}

		/// <summary>
		/// Standard Delete Event handler to quit application and destroy 
		/// all of the windows.
		/// </summary>
		private void on_main_delete_event(object o, DeleteEventArgs args) 
		{
			args.RetVal = true;
			on_close(o, args);
		}

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
			ButtonHBox.PackEnd(OKButton, true, true, 10);
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
	}
}
