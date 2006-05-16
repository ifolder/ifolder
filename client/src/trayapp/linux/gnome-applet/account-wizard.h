/***********************************************************************
 *  iFolder 3 Applet -- Main applet for the iFolder 3 Client
 * 
 *  Copyright (C) 2006 Novell, Inc.
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
 *  Author(s): Boyd Timothy <btimothy@novell.com>
 *
 ***********************************************************************/

#ifndef _IFOLDER3_ACCOUNT_WIZARD_H_
#define _IFOLDER3_ACCOUNT_WIZARD_H_

#include <stdbool.h>
#include <ifolder-client.h>

#define KEY_USER_PREF_PREFILL_ACCOUNT "PrefillAccount"
#define KEY_USER_PREF_ACCOUNT_SERVER_ADDRESS "ServerAddress"
#define KEY_USER_PREF_ACCOUNT_USER_NAME "UserName"
#define KEY_USER_PREF_ACCOUNT_PASSWORD "Password"
#define KEY_USER_PREF_ACCOUNT_REMEMBER_PASSWORD "RememberPassword"

G_BEGIN_DECLS

typedef struct {
	GtkWidget 		*window;

	GtkWidget		*pageLabel;

	GtkWidget		*notebook;
	GtkWidget		*introductoryPage;
	GtkWidget		*serverInformationPage;
	GtkWidget		*userInformationPage;
	GtkWidget		*connectPage;
	GtkWidget		*summaryPage;

	bool			controlKeyPressed;
	
	GtkWidget		*helpButton;
	GtkWidget		*cancelButton;
	GtkWidget		*backButton;
	GtkWidget		*forwardButton;
	GtkWidget		*connectButton;
	GtkWidget		*finishButton;
	
//	GdkPixbuf		*addAccountPixbuf;
	GtkWidget		*addAccountImage;
	
	/**
	 * Server Information Page Widgets
	 */
	GtkWidget		*serverNameEntry;
	GtkWidget		*makeDefaultLabel;
	GtkWidget		*defaultServerCheckButton;
	
	/**
	 * User Information Page Widgets
	 */
	GtkWidget		*userNameEntry;
	GtkWidget		*passwordEntry;
	GtkWidget		*rememberPasswordCheckButton;
	
	/**
	 * Connect Page Widgets
	 */
	GtkWidget		*serverNameVerifyLabel;
	GtkWidget		*userNameVerifyLabel;
	GtkWidget		*rememberPasswordVerifyLabel;
	GtkWidget		*makeDefaultPromptLabel;
	GtkWidget		*makeDefaultVerifyLabel;
	
	/**
	 * Summary Page Widgets
	 */
	iFolderDomain	*connectedDomain;
	
	/**
	 * Wait Message
	 */
//	iFolderWaitDialog	*waitDialog;
} IFAAccountWizard;

IFAAccountWizard *ifa_account_wizard_new();

G_END_DECLS

#endif /*_IFOLDER3_ACCOUNT_WIZARD_H_*/
