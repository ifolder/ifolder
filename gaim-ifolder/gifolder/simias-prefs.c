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

#include "internal.h"

#include "simias-prefs.h"

/* Gaim Includes */
#include "prefs.h"

/**
 * Forward Declarations
 */
static char * parse_host_name(char *localhost);

/**
 * If the given preferences don't exist, create them with a default value.
 */
void
simias_init_default_prefs()
{
	char localhost[129];
	char * machine_name;

	gaim_prefs_add_none(SIMIAS_PREF_PATH);

	if (!gaim_prefs_exists(SIMIAS_PREF_MACHINE_NAME)) {
		/* Determine the machine's host name right here */
		if (gethostname(localhost, 128) < 0)
			sprintf(localhost, "UNKNOWN");
			
		machine_name = parse_host_name(localhost);
	
		gaim_prefs_add_string(SIMIAS_PREF_MACHINE_NAME,
							 machine_name);
		free(machine_name);
	}

	if (!gaim_prefs_exists(SIMIAS_PREF_SYNC_INTERVAL)) {
		gaim_prefs_add_int(SIMIAS_PREF_SYNC_INTERVAL,
				      SIMIAS_PREF_SYNC_INTERVAL_DEF);
	}

	if (!gaim_prefs_exists(SIMIAS_PREF_PING_REPLY_TYPE)) {
		gaim_prefs_add_string(SIMIAS_PREF_PING_REPLY_TYPE,
				      SIMIAS_PREF_PING_REPLY_TYPE_DEF);
	}

	if (!gaim_prefs_exists(SIMIAS_PREF_NOTIFY_ERRORS)) {
		gaim_prefs_add_bool(SIMIAS_PREF_NOTIFY_ERRORS,
				      SIMIAS_PREF_NOTIFY_ERRORS_DEF);
	}

	if (!gaim_prefs_exists(SIMIAS_PREF_SIMIAS_AUTO_START)) {
		gaim_prefs_add_bool(SIMIAS_PREF_SIMIAS_AUTO_START,
							SIMIAS_PREF_SIMIAS_AUTO_START_DEF);
	}
}

static char *
parse_host_name(char *localhost)
{
	char * host_name;
	
	host_name = strtok(localhost, ".");
	if (host_name)
		return strdup(host_name);
	else
		return strdup(localhost);
}
