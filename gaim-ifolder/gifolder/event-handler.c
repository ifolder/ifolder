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
#include "gaim-domain.h"

#include <glib.h>
#include <stdlib.h>
#include <string.h>

#include <simias/simias.h>

static char *po_box_id = NULL;

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

			/* FIXME: Move the Sync Buddies Call from plugin_load to here */

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
	char *temp;
	
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
