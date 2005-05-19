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

#ifndef _SIMIAS_PREFS_H
#define _SIMIAS_PREFS_H 1

#include <gtk/gtk.h>

/* Gaim Includes */
#include "plugin.h"

#define SIMIAS_PREF_PATH "/plugins/simias"

#define SIMIAS_PREF_USER_ID "/plugins/simias/user_id"
#define SIMIAS_PREF_MACHINE_NAME "/plugins/simias/machine_name"

#define SIMIAS_PREF_SYNC_INTERVAL "/plugins/simias/sync_interval"
#define SIMIAS_PREF_SYNC_INTERVAL_DEF 1 /* minute */

#define SIMIAS_PREF_PING_REPLY_TYPE "/plugins/simias/ping_reply_type"
#define SIMIAS_PREF_PING_REPLY_TYPE_BLIST "buddy-list"
#define SIMIAS_PREF_PING_REPLY_TYPE_ANY "any"
#define SIMIAS_PREF_PING_REPLY_TYPE_DEF SIMIAS_PREF_PING_REPLY_TYPE_BLIST

#define SIMIAS_PREF_NOTIFY_ERRORS "/plugins/simias/notify_on_errors"
#define SIMIAS_PREF_NOTIFY_ERRORS_DEF FALSE

#define SIMIAS_PREF_SIMIAS_AUTO_START "/plugins/simias/auto_start_simias"
#define SIMIAS_PREF_SIMIAS_AUTO_START_DEF FALSE

#define SIMIAS_PREF_PUBLIC_KEY "/plugins/simias/public_key"
#define SIMIAS_PREF_PRIVATE_KEY "/plugins/simias/private_key"

#define SIMIAS_PREF_DES_KEY "/plugins/simias/des_key"

#define SIMIAS_PREF_AUTO_PUBLIC_IFOLDER "/plugins/simias/auto_public_ifolder"
#define SIMIAS_PREF_AUTO_PUBLIC_IFOLDER_DEF TRUE

/* Function Declarations */
void simias_init_default_prefs();

#endif
