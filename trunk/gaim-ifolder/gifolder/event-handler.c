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

#include "event-handler.h"

/* Gaim iFolder Includes */
#include "gaim-domain.h"
#include "simias-messages.h"
#include "simias-util.h"
#include "simias-prefs.h"
#include "buddy-profile.h"

#include <glib.h>
#include <gtk/gtk.h>
#include <stdlib.h>
#include <string.h>

#include "blist.h"
#include "util.h"

/**
 * This function is called any time a buddy-sign-on event occurs.  When this
 * happens, we need to do the following:
 *
 * 1. Return if the account is not prpl-oscar (AIM-only functionality for now)
 * 2. Check to see if the buddy has the Gaim iFolder Plugin installed by reading
 *    their profile.  Do nothing if the buddy doesn't have the plugin installed.
 */
void
simias_buddy_signed_on_cb(GaimBuddy *buddy, void *user_data)
{
	const char *prpl_id;

	/* Only do anything if this is an AOL (prpl-oscar) buddy */
	prpl_id = gaim_account_get_protocol_id(buddy->account);
	if (!prpl_id || strcmp(prpl_id, "prpl-oscar")) {
		return;
	}

	simias_get_buddy_profile(buddy);
}

/**
 * This function is called any time a buddy-sign-off event occurs.  When this
 * happens, we need to do the following:
 *
 * 1. Remove the "simias-plugin-enabled" setting to let the Gaim Domain know
 *    that this buddy is no longer online.
 */
void
simias_buddy_signed_off_cb(GaimBuddy *buddy, void *user_data)
{
	const char *pluginEnabled;

	if (gaim_blist_node_get_string(&(buddy->node), "simias-plugin-enabled"))
		gaim_blist_node_remove_setting(&(buddy->node), "simias-plugin-enabled");
}
