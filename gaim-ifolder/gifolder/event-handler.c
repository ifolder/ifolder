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
#include "simias-invitation-store.h"
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

/* Externs */
extern GtkListStore *in_inv_store;
extern GtkListStore *out_inv_store;
extern GtkListStore *trusted_buddies_store;

static char *po_box_id = NULL;

int
on_sec_state_event(SEC_STATE_EVENT state_event, const char *message, void *data)
{
	SimiasEventClient *ec = (SimiasEventClient *)data;
	SIMIAS_NODE_TYPE node_type;
	SimiasEventFilter event_filter;
	GaimBuddyList *blist;
		
	
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
			
			/**
			 * Get the Gaim POBox ID so we'll know whether we should pay
			 * attention to incoming events or not.
			 */
			po_box_id = gaim_domain_get_po_box_id();
			if (!po_box_id) {
				g_print("gaim_domain_get_po_box_id() returned NULL!\n");
			} else {
				g_print("POBox ID: %s\n", po_box_id);
			}
			
			/* Specify that we're only interested in changes in subscriptions */
			node_type = NODE_TYPE_NODE;
			event_filter.type = EVENT_FILTER_NODE_TYPE;
			event_filter.data = &node_type;
			sec_set_filter (*ec, &event_filter);

			/* Sync Gaim Buddies */
			blist = gaim_get_blist();
			if (blist) {
				g_hash_table_foreach(blist->buddies, 
									 sync_buddy_with_simias_roster,
									 NULL);
			} else {
				g_print("gaim_get_blist() returned NULL\n");
			}

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

gboolean
is_gaim_subscription(SimiasNodeEvent *event)
{
	/* Check to see if this is an invitation/subscription node for Gaim */
	if (!po_box_id) {
		/* Can't do the check */
		return FALSE;
	}
	if (strcmp(po_box_id, event->collection)) {
		/* This is not a subscription in the Gaim POBox */
		return FALSE;
	}

	return TRUE;
}

int
on_simias_node_created(SimiasNodeEvent *event, void *data)
{
	char *service_url;
	int err;
	char *ip_addr = NULL;
	char ip_port[16];
	
	char *ret_host = NULL;
	int   ret_port = 0;
	char *ret_path = NULL;
	char *ret_user = NULL;
	char *ret_passwd = NULL;
	
	g_print("on_simias_node_created() entered\n");
	print_event(event);

	if (!is_gaim_subscription(event)) {
		return 0; /* Ignore this event */
	}
	
	/* Send out an invitation for the collection */
	err = simias_get_local_service_url(&service_url);
	if (err == SIMIAS_SUCCESS) {
		g_print("Service URL: %s\n", service_url);
		/* Parse off the IP Address & Port */
		gaim_url_parse(service_url, &ret_host, &ret_port,
					   &ret_path, &ret_user, &ret_passwd);
		if (ret_host) {
			g_print("ret_host: %s\n", ret_host);
			ip_addr = ret_host;
		} else {
			ip_addr = strdup("Unknown");
		}
		
		g_print("ret port: %d\n", ret_port);
		sprintf(ip_port, "%d", ret_port);
		
		if (ret_path) {
			g_print("ret_path: %s\n", ret_path);
			g_free(ret_path);
		}
		
		if (ret_user) {
			g_print("ret_user: %s\n", ret_user);
			g_free(ret_user);
		}
		
		if (ret_passwd) {
			g_print("ret_passwd: %s\n", ret_passwd);
			g_free(ret_passwd);
		}
		
		/* FIXME: Create and send a new invitation */
		g_print("FIXME: Send a new invitation to the buddy\n");
		if (ip_addr) {
			g_print("\tFrom IP: %s\n", ip_addr);
			g_free(ip_addr);
		}
		
		if (ip_port) {
			g_print("\tFrom IP Port: %s\n", ip_port);
		}
		
		g_free(service_url);
	}
	
	return 0;
}

int
on_simias_node_changed(SimiasNodeEvent *event, void *data)
{
	g_print("on_simias_node_changed() entered\n");
	print_event(event);
	
	if (!is_gaim_subscription(event)) {
		return 0; /* Ignore this event */
	}
	
	/* FIXME: Figure out if we need to do anything with a changed subscription */
	
	return 0;
}

int
on_simias_node_deleted(SimiasNodeEvent *event, void *data)
{
	g_print("on_simias_node_deleted() entered\n");
	print_event(event);
	
	if (!is_gaim_subscription(event)) {
		return 0; /* Ignore this event */
	}
	
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
	char *buddy_profile;
	char *simias_plugin_installed;

	/* Only do anything if this is an AOL (prpl-oscar) buddy */
	prpl_id = gaim_account_get_protocol_id(buddy->account);
	if (!prpl_id || strcmp(prpl_id, "prpl-oscar")) {
		return;
	}

	simias_get_buddy_profile(buddy);
}


/**
 * This function is called any time a buddy-sign-on event occurs.  When this
 * happens, we need to do the following:
 * 
 * 	1. Send out any invitations that are for this buddy that are in the
 *	   STATE_PENDING state.
 *  2. If we have an IP Address for this buddy in the Simias Gaim Domain Roster,
 * 	   send out a [simias:ping-request] message so their IP Address will be
 * 	   updated if it's changed.
 *  3. Send out any pending accept or reject messages for this buddy.
 */
void
simias_buddy_signed_on_cb_old(GaimBuddy *buddy, void *user_data)
{
	GtkTreeIter iter;
	int send_result;
	char time_str[32];
	char state_str[32];

	Invitation *invitation;
	gboolean valid;
	gboolean b_in_store_updated = FALSE;
	gboolean b_out_store_updated = FALSE;
	
	valid = gtk_tree_model_get_iter_first(GTK_TREE_MODEL(out_inv_store), &iter);
	while (valid) {
		/* Extract the Invitation * out of the model */
		gtk_tree_model_get(GTK_TREE_MODEL(out_inv_store), &iter,
					INVITATION_PTR, &invitation,
					-1);
					
		/**
		 * Make sure all the following conditions exist before sending the
		 * message:
		 * 	- The invitation state is STATE_PENDING 
		 * 	- The current invitation is intended for this buddy
		 * 	- The buddy is not signing or signed off
		 */
		if (invitation->state == STATE_PENDING
			&& buddy->account == invitation->gaim_account
			&& strcmp(buddy->name, invitation->buddy_name) == 0
			&& buddy->present != GAIM_BUDDY_SIGNING_OFF
			&& buddy->present != GAIM_BUDDY_OFFLINE) {
			send_result = simias_send_invitation_req(
					buddy,
					invitation->collection_id,
					invitation->collection_type,
					invitation->collection_name);
		
			if (send_result > 0) {
				/* Update the invitation time and resend the invitation */
				time(&(invitation->time));

				/* Update the invitation state */
				invitation->state = STATE_SENT;
	
				/* Format the time to a string */
				simias_fill_time_str(time_str, 32, invitation->time);

				/* Format the state string */
				simias_fill_state_str(state_str, invitation->state);

				/* Update the out_inv_store */
				gtk_list_store_set(
					GTK_LIST_STORE(out_inv_store),
					&iter,
					TIME_COL, time_str,
					STATE_COL, state_str,
					-1);

				b_out_store_updated = TRUE;
			}
		}

		valid = gtk_tree_model_iter_next(GTK_TREE_MODEL(out_inv_store), &iter);
	}

	if (gaim_prefs_get_bool(SIMIAS_PREF_REDISCOVER_IP_ADDRS)) {
		if (simias_lookup_trusted_buddy(trusted_buddies_store, buddy, &iter)) {
			/* Send a ping-request message */
			send_result = simias_send_ping_req(buddy);
			if (send_result <= 0) {
				g_print("simias_buddy_signed_on_cb() couldn't send a ping reqest: %d\n", send_result);
			}
		}
	}

	valid = gtk_tree_model_get_iter_first(GTK_TREE_MODEL(in_inv_store), &iter);
	while (valid) {
		/* Extract the Invitation * out of the model */
		gtk_tree_model_get(GTK_TREE_MODEL(in_inv_store), &iter,
					INVITATION_PTR, &invitation,
					-1);

		/**
		 * Make sure all the following conditions exist before sending the
		 * message:
		 * 	- The current invitation is intended for this buddy
		 * 	- The buddy is not signing or signed off
		 */
		if (buddy->account == invitation->gaim_account
			&& strcmp(buddy->name, invitation->buddy_name) == 0
			&& buddy->present != GAIM_BUDDY_SIGNING_OFF
			&& buddy->present != GAIM_BUDDY_OFFLINE) {
			/* Use send_result = 1 to know if a send failed */
			send_result = 1;

			if (invitation->state == STATE_ACCEPTED_PENDING) {
				send_result = simias_send_invitation_accept(buddy,
													invitation->collection_id);
			} else if (invitation->state == STATE_REJECTED_PENDING) {
				send_result = simias_send_invitation_deny(buddy,
													invitation->collection_id);
			}

			if (send_result <= 0) {
				g_print("Error sending deny message in simias_buddy_signed_on_cb()\n");
				/**
				 * Update the time stamp of the invitation so the user has some
				 * idea that the message was updated.
				 */
				time(&(invitation->time));

				/* Format the time to a string */
				simias_fill_time_str(time_str, 32, invitation->time);

				/* Update the out_inv_store */
				gtk_list_store_set(in_inv_store, &iter,
					TIME_COL, time_str,
					-1);

				b_in_store_updated = TRUE;
			} else {
				/**
				 * The message was sent successfully and so we can remove the
				 * invitation from the in_inv_store.
				 */
				gtk_list_store_remove(in_inv_store, &iter);
				free(invitation);

				b_in_store_updated = TRUE;
			}
		}

		valid = gtk_tree_model_iter_next(GTK_TREE_MODEL(in_inv_store), &iter);
	}

	if (b_in_store_updated) {
		/* Save the updates to a file */
		simias_save_invitations(in_inv_store);
	}
	if (b_out_store_updated) {
		/* Save the updates to a file */
		simias_save_invitations(out_inv_store);
	}
}
