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

#define SIMIAS_PREF_PATH "/plugins/gtk/simias"

#define SIMIAS_PREF_NOTIFY_RECEIVE_NEW_INVITATIONS "/plugins/gtk/simias/notify_receive_new_invitations"
#define SIMIAS_PREF_NOTIFY_RECEIVE_NEW_INVITATIONS_DEF TRUE

#define SIMIAS_PREF_NOTIFY_ACCEPT_INVITATIONS "/plugins/gtk/simias/notify_buddies_accept_invitations"
#define SIMIAS_PREF_NOTIFY_ACCEPT_INVITATIONS_DEF TRUE

#define SIMIAS_PREF_NOTIFY_REJECT_INVITATIONS "/plugins/gtk/simias/notify_buddies_reject_invitations"
#define SIMIAS_PREF_NOTIFY_REJECT_INVITATIONS_DEF TRUE

#define SIMIAS_PREF_NOTIFY_ERRORS "/plugins/gtk/simias/notify_on_errors"
#define SIMIAS_PREF_NOTIFY_ERRORS_DEF FALSE

#define SIMIAS_PREF_REDISCOVER_IP_ADDRS "/plugins/gtk/simias/rediscover_ip_addrs"
#define SIMIAS_PREF_REDISCOVER_IP_ADDRS_DEF TRUE

#define SIMIAS_PREF_SIMIAS_AUTO_START "/plugins/gtk/simias/auto_start_simias"
#define SIMIAS_PREF_SIMIAS_AUTO_START_DEF FALSE

/* Function Declarations */
void simias_init_default_prefs();

#endif
