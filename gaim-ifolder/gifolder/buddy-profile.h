/***********************************************************************
 *  $RCSfile$
 *
 *  Gaim iFolder Plugin: Allows Gaim users to share iFolders.
 *  Copyright (C) 2005 Novell, Inc.
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
 *  Author: Boyd Timothy <btimothy@novell.com>
 * 
 *  Some code in this file (mostly the saving and reading of the XML files) is
 *  directly based on code found in Gaim's core & plugin files, which is
 *  distributed under the GPL.
 ***********************************************************************/

#ifndef _GIFOLDER_BUDDY_PROFILE_H
#define _GIFOLDER_BUDDY_PROFILE_H 1

/* Gaim Includes */
#include "blist.h"
#include "account.h"
#include "connection.h"
#include "notify.h"

#define SIMIAS_PLUGIN_INSTALLED_ID "&lt;!--[simias:plugin-installed]--&gt;"

void simias_get_buddy_profile(GaimBuddy *buddy);
void simias_set_buddy_profile(GaimAccount *account, const char *profile_str);

void simias_account_connecting_cb(GaimAccount *account);
void simias_account_setting_info_cb(GaimAccount *account, const char *new_info);
void simias_account_set_info_cb(GaimAccount *account, const char *new_info);

void simias_connection_signing_on_cb(GaimConnection *gc);
void simias_connection_signed_on_cb(GaimConnection *gc);

GaimNotifyUiOps * simias_notify_get_ui_ops();

#endif
