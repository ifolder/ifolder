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

#ifndef _IFOLDER3_PREFERENCES_H_
#define _IFOLDER3_PREFERENCES_H_

#include <stdbool.h>

G_BEGIN_DECLS

typedef enum
{
	IFA_SYNC_UNIT_SECONDS,
	IFA_SYNC_UNIT_MINUTES,
	IFA_SYNC_UNIT_HOURS,
	IFA_SYNC_UNIT_DAYS
} IFASyncUnit;

typedef struct {
	GtkWidget 		*window;

	GtkWidget		*notebook;
	GtkWidget		*generalPage;
	GtkWidget		*accountsPage;
	GtkWidget		*buttonBox;
	GtkWidget		*helpButton;
	GtkWidget		*closeButton;

	bool			controlKeyPressed;

	/**
	 * General Page Widgets
	 */
	GtkWidget		*autoSyncCheckButton;
	GtkWidget		*syncSpinButton;
	GtkWidget		*syncUnitsComboBox;
	GtkWidget		*showConfirmationButton;
	GtkWidget		*notifySyncErrorsButton;
	int				lastSyncInterval;
	IFASyncUnit		currentSyncUnit;

	/**
	 * Accounts Page Widgets
	 */
	GtkTreeView		*accTreeView;
	GtkListStore		*accTreeStore;
	GtkCellRendererCombo	*onlineToggleButton;
	GtkWidget		*addButton;
	GtkWidget		*removeButton;
	GtkWidget		*propertiesButton;
	GHashTable		*curDomains;
	GHashTable		*removedDomains;

	GHashTable		*detailsDialogs;
	GtkWidget		*waitDialog;
} IFAPreferencesWindow;

IFAPreferencesWindow *ifa_get_preferences_window();
void ifa_show_preferences_window();

G_END_DECLS

#endif /*_IFOLDER3_PREFERENCES_H_*/
