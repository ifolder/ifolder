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

#include "simias-prefs.h"

/* Gaim Includes */
#include "prefs.h"

/**
 * If the given preferences don't exist, create them with a default value.
 */
void
simias_init_default_prefs()
{
	gaim_prefs_add_none(SIMIAS_PREF_PATH);
	if (!gaim_prefs_exists(SIMIAS_PREF_NOTIFY_RECEIVE_NEW_INVITATIONS)) {
		gaim_prefs_add_bool(SIMIAS_PREF_NOTIFY_RECEIVE_NEW_INVITATIONS,
							SIMIAS_PREF_NOTIFY_RECEIVE_NEW_INVITATIONS_DEF);
	}

	if (!gaim_prefs_exists(SIMIAS_PREF_NOTIFY_ACCEPT_INVITATIONS)) {
		gaim_prefs_add_bool(SIMIAS_PREF_NOTIFY_ACCEPT_INVITATIONS,
							SIMIAS_PREF_NOTIFY_ACCEPT_INVITATIONS_DEF);
	}

	if (!gaim_prefs_exists(SIMIAS_PREF_NOTIFY_REJECT_INVITATIONS)) {
		gaim_prefs_add_bool(SIMIAS_PREF_NOTIFY_REJECT_INVITATIONS,
							SIMIAS_PREF_NOTIFY_REJECT_INVITATIONS_DEF);
	}

	if (!gaim_prefs_exists(SIMIAS_PREF_NOTIFY_ERRORS)) {
		gaim_prefs_add_bool(SIMIAS_PREF_NOTIFY_ERRORS,
							SIMIAS_PREF_NOTIFY_ERRORS_DEF);
	}
	
	if (!gaim_prefs_exists(SIMIAS_PREF_REDISCOVER_IP_ADDRS)) {
		gaim_prefs_add_bool(SIMIAS_PREF_REDISCOVER_IP_ADDRS,
							SIMIAS_PREF_REDISCOVER_IP_ADDRS_DEF);
	}

	if (!gaim_prefs_exists(SIMIAS_PREF_SIMIAS_AUTO_START)) {
		gaim_prefs_add_bool(SIMIAS_PREF_SIMIAS_AUTO_START,
							SIMIAS_PREF_SIMIAS_AUTO_START_DEF);
	}
}
