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

#include <simias.h>

#include "blist.h"
#include "util.h"

int
on_sec_state_event(SEC_STATE_EVENT state_event, const char *message, void *data)
{
	SimiasEventClient *ec = (SimiasEventClient *)data;
	SIMIAS_NODE_TYPE node_type;
	SimiasEventFilter event_filter;
	
	switch (state_event) {
		case SEC_STATE_EVENT_CONNECTED:
			g_print("Connected to Simias Event Server\n");

			/* Register the event handlers */
			sec_set_event (*ec, ACTION_NODE_CREATED, true,
						   (SimiasEventFunc)on_simias_node_created, ec);
			sec_set_event (*ec, ACTION_NODE_CHANGED, true,
						   (SimiasEventFunc)on_simias_node_changed, ec);
			sec_set_event (*ec, ACTION_NODE_DELETED, true,
						   (SimiasEventFunc)on_simias_node_deleted, ec);
			
			/* Specify that we're only interested in changes in subscriptions */
			node_type = NODE_TYPE_NODE;
			event_filter.type = EVENT_FILTER_NODE_TYPE;
			event_filter.data = &node_type;
			sec_set_filter (*ec, &event_filter);

			break;
		case SEC_STATE_EVENT_DISCONNECTED:
			g_print("Disconnected from Simias Event Server\n");

			break;
		case SEC_STATE_EVENT_ERROR:
			if (message) {
				g_print("Error in Simias Event Client: %s\n", message);
			} else {
				g_print("An unknown error occurred in Simias Event Client\n");
			}
			break;
		default:
			g_print("An unknown Simias Event Client State Event occurred\n");
	}
	
	return 0;
}

/* FIXME: Remove this debug function */
static void print_event(SimiasNodeEvent *event)
{
	g_print("\tEvent Type: %s\n", event->event_type);
	g_print("\tAction: %s\n", event->action);
	g_print("\tTime: %s\n", event->time);
	g_print("\tSource: %s\n", event->source);
	g_print("\tCollection: %s\n", event->collection);
	g_print("\tType: %s\n", event->type);
	g_print("\tEvent ID: %s\n", event->event_id);
	g_print("\tNode: %s\n", event->node);
	g_print("\tFlags: %s\n", event->flags);
	g_print("\tMaster Rev: %s\n", event->master_rev);
	g_print("\tSlave Rev: %s\n", event->slave_rev);
	g_print("\tFile Size: %s\n", event->file_size);
}

int
on_simias_node_created(SimiasNodeEvent *event, void *data)
{
	g_print("on_simias_node_created() entered\n");
	print_event(event);
	
	return 0;
}

int
on_simias_node_changed(SimiasNodeEvent *event, void *data)
{
	g_print("on_simias_node_changed() entered\n");
	print_event(event);
	
	/* FIXME: Figure out if we need to do anything with a changed subscription */
	
	return 0;
}

int
on_simias_node_deleted(SimiasNodeEvent *event, void *data)
{
	g_print("on_simias_node_deleted() entered\n");
	print_event(event);
	
	/* FIXME: Check to see if this is a member/subscription to a collection */
	
	return 0;
}

/**
 * This function is called any time a buddy-sign-on event occurs.  When this
 * happens, we need to do the following:
 *
 * 1. Return if the account is not prpl-oscar (AIM-only functionality for now)
 * 2. Check to see if the buddy has the Gaim iFolder Plugin installed by reading
 *    their profile.  Do nothing if the buddy doesn't have the plugin installed.
 * 3. If the buddy DOES have the Gaim iFolder Plugin installed, send Simias
 *    messages to the buddy to request their POBoxURL.  Create a message handler
 *    that will store the buddy's POBoxURL into blist.xml.
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
