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
 *  Author:
 *		Boyd Timothy <btimothy@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using Gtk;

namespace Novell.iFolder
{
	public class AddAccountWizard : Dialog
	{
		private Notebook	WizardNotebook;

		///
		/// Navigation Buttons
		///
		private Button		NextButton;
		private Button		BackButton;
		private Button		CancelButton;
		private Button		ConnectButton;
		private Button		FinishButton;
		
		///
		/// Introductory Page Widgets
		///
		
		///
		/// Server Information Page Widgets
		///
		private Entry		ServerNameEntry;
		private CheckButton	DefaultServerCheckButton;
		
		///
		/// User Information Page Widgets
		///
		private Entry		UserNameEntry;
		private Entry		PasswordEntry;	// set Visibility = false;
		private CheckButton	RememberPasswordCheckButton;
		
		///
		/// Connect Page Widgets
		///
		private Label		ServerNameVerifyLabel;
		private Label		UserNameVerifyLabel;
		
		///
		/// Accept Certificate Page Widgets
		///
		
		///
		/// Error Page Widgets
		///
		
		///
		/// Summary Page Widgets
		///
		private Label		SummaryMessageLabel;


		public AddAccountWizard(iFolderWebService ifws, SimiasWebService simws) : base()
		{
			this.Title = Util.GS("Account Wizard");
			this.SetDefaultSize(600, 500);
			this.Resizable = true;
			this.HasSeparator = false;
			
			CreateWidgets();
			
			this.VBox.ShowAll();
		}
		
		private void CreateWidgets()
		{
			WizardNotebook = new Notebook();
			this.VBox.PackStart(WizardNotebook, true, true, 0);
			WizardNotebook.ShowTabs = true;	// FIXME: Change this to 'false' once this is working correctly
			
			WizardNotebook.AppendPage(CreateIntroductoryPage(), null);
			WizardNotebook.AppendPage(CreateServerInformationPage(), null);
			WizardNotebook.AppendPage(CreateUserInformationPage(), null);
			WizardNotebook.AppendPage(CreateConnectPage(), null);
			WizardNotebook.AppendPage(CreateAcceptCertPage(), null);
			WizardNotebook.AppendPage(CreateErrorPage(), null);
			WizardNotebook.AppendPage(CreateSummaryPage(), null);
		}
		
		///
		/// Introductory Page
		///
		private Widget CreateIntroductoryPage()
		{
			return new Label("Introductory Page not implemented yet");
		}
		
		///
		/// Server Information Page (1 of 3)
		///
		private Widget CreateServerInformationPage()
		{
			return new Label("Server Information Page not implemented yet");
		}
		
		///
		/// User Information Page (2 of 3)
		///
		private Widget CreateUserInformationPage()
		{
			return new Label("User Information Page not implemented yet");
		}
		
		///
		/// Connect Page (3 of 3)
		///
		private Widget CreateConnectPage()
		{
			return new Label("Connect Page not implemented yet");
		}
		
		///
		/// Accept Certificate Page
		///
		private Widget CreateAcceptCertPage()
		{
			return new Label("Accept Certificate Page not implemented yet");
		}
		
		///
		/// Error Page
		///
		private Widget CreateErrorPage()
		{
			return new Label("Error Page not implemented yet");
		}
		
		///
		/// Summary Page
		///
		private Widget CreateSummaryPage()
		{
			return new Label("Summary Page not implemented yet");
		}
	}
} 