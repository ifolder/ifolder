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

#include "simias-messages.h"

/* Gaim iFolder Includes */
#include "gifolder.h"
#include "simias-invitation-store.h"
#include "simias-util.h"
#include "simias-invitation-store.h"
#include "simias-prefs.h"

/* Gaim Includes */
#include "internal.h"
#include "network.h"
#include "util.h"

#include <simias.h>

/* Externs */
extern GtkListStore *in_inv_store;
extern GtkListStore *out_inv_store;
extern GtkWidget *out_inv_tree;
extern GtkListStore *trusted_buddies_store;

/**
 * Non-public Functions
 */
static SIMIAS_MSG_TYPE get_possible_simias_msg_type(const char *buffer);

static gboolean handle_invitation_request(GaimAccount *account,
										  const char *sender,
										  const char *buffer);
static gboolean handle_invitation_request_deny(GaimAccount *account,
											   const char *sender,
											   const char *buffer);
static gboolean handle_invitation_request_accept(GaimAccount *account,
												 const char *sender,
												 const char *buffer);
static gboolean handle_ping_request(GaimAccount *account,
									const char *sender,
									const char *buffer);
static gboolean handle_ping_response(GaimAccount *account,
									 const char *sender,
									 const char *buffer);

/**
 * This function takes a generic message and sends it to the specified recipient
 * if they are online.
 * 
 * FIXME: We may have to prevent a "flood" of messages for a given account in case that some IM servers will kick us off for "abuse".
 */
int
simias_send_msg(GaimBuddy *recipient, char *msg)
{
	GaimConnection *conn;

	/**
	 * Make sure the buddy is online.  More information on the "present" field
	 * can be seen here: http://gaim.sourceforge.net/api/struct__GaimBuddy.html
	 */
	if (recipient->present == GAIM_BUDDY_SIGNING_OFF
		|| recipient->present == GAIM_BUDDY_OFFLINE) {
		return recipient->present; /* Buddy is signing off or offline */
	}
	
	conn = gaim_account_get_connection(recipient->account);
	
	if (!conn) {
		return -1; /* Can't send a msg without a connection */
	}

	g_print("Sending message: %s\n", msg);
	return serv_send_im(conn, recipient->name, msg, 0);
}

/**
 * This function will send a message with the following format:
 * 
 * [simias:invitation-request:<ip-address>:<ip-port><collection-id>:<collection-type>:<collection-name>]<Human-readable Invitation String for buddies who don't have the plugin installed/enabled>
 */
int
simias_send_invitation_req(GaimBuddy *recipient, char *collection_id,
							char *collection_type, char *collection_name)
{
	char msg[2048];
	const char *public_ip;
	char *ip_port = "5432";	/* Get the WebService port from Simias */
	
	public_ip = gaim_network_get_my_ip(-1);

	sprintf(msg, "%s%s:%s:%s:%s:%s] %s",
			INVITATION_REQUEST_MSG,
			public_ip,
			ip_port,
			collection_id,
			collection_type,
			collection_name,
			_("I'd like to share an iFolder with you through Gaim but you don't have the iFolder Plugin installed/enabled.  You can download it from http://www.ifolder.com/.  Let me know when you've installed and enabled it and I will re-invite you to share my iFolder."));

	return simias_send_msg(recipient, msg);
}

/**
 * This function will send a message with the following format:
 * 
 * [simias:invitation-request-deny:<collection-id>]
 */
int
simias_send_invitation_deny(GaimBuddy *recipient, char *collection_id)
{
	char msg[2048];
	
	sprintf(msg, "%s%s]", INVITATION_REQUEST_DENY_MSG, collection_id);

	return simias_send_msg(recipient, msg);
}

/**
 * This function will send a message with the following format:
 * 
 * 
 * [simias:invitation-request-accept:<collection-id>:<ip-address>:<ip-port>]
 */
int
simias_send_invitation_accept(GaimBuddy *recipient, char *collection_id)
{
	char msg[2048];
	const char *public_ip;
	char *ip_port = "4321"; /* FIXME: Get WebService port from Simias */
	
	public_ip = gaim_network_get_my_ip(-1);
	
	sprintf(msg, "%s%s:%s:%s]",
			INVITATION_REQUEST_ACCEPT_MSG,
			collection_id,
			public_ip,
			ip_port);

	return simias_send_msg(recipient, msg);
}

/**
 * Replaces the host with the one Gaim determines to be public.
 * The returned string must be freed.
 */
static char *
convert_url_to_public(const char *start_url)
{
	char new_url[1024];
	char *host = NULL;
	int port;
	char *path = NULL;
	char *user = NULL;
	char *pass = NULL;
	const char *public_ip;
	
	if (gaim_url_parse(start_url, &host, &port, &path, &user, &pass)) {
		
		public_ip = gaim_network_get_my_ip(-1);
		
		if (path) {
			sprintf(new_url, "http://%s:%d/%s", public_ip, port, path);
		}
		
		if (host) free(host);
		if (path) free(path);
		if (user) free(user);
		if (pass) free(pass);
		
		return strdup(new_url);
	}
	
	return NULL;
}

/**
 * This function will send a message with the following format:
 * 
 * [simias:ping-request:<simias-url>]
 */
int
simias_send_ping_req(GaimBuddy *recipient)
{
	char msg[2048];
	char *simias_service_url;
	char *public_url;
	
	if (simias_get_local_service_url(&simias_service_url)) {
		/* There was an error! */
		return -1;
	}
	
	public_url = convert_url_to_public(simias_service_url);
	if (public_url) {
		sprintf(msg, "%s%s]", PING_REQUEST_MSG, public_url);
		free(public_url);
	} else {
		sprintf(msg, "%s%s]", PING_REQUEST_MSG, simias_service_url);
	}
	free(simias_service_url);

	return simias_send_msg(recipient, msg);
}

/**
 * This function will send a message with the following format:
 * 
 * [simias:ping-response:<simias-url>]
 */
int
simias_send_ping_resp(GaimBuddy *recipient)
{
	char msg[2048];
	char *simias_service_url;
	char *public_url;

	if (simias_get_local_service_url(&simias_service_url)) {
		/* There was an error! */
		return -1;
	}

	public_url = convert_url_to_public(simias_service_url);
	if (public_url) {
		sprintf(msg, "%s%s]", PING_RESPONSE_MSG, public_url);
		free(public_url);
	} else {
		sprintf(msg, "%s%s]", PING_RESPONSE_MSG, simias_service_url);
	}
	free(simias_service_url);

	return simias_send_msg(recipient, msg);
}

/**
 * This function will be called any time Gaim starts receiving an instant
 * message.  This gives our plugin the ability to jump in and check to see if
 * it is a special message sent by another buddy who is also using this plugin.
 * If it IS a special Simias message, we intercept it and prevent it from being
 * displayed to the user.
 */
gboolean
simias_receiving_im_msg_cb(GaimAccount *account, char **sender, char **buffer,
					int *flags, void *data)
{
	SIMIAS_MSG_TYPE possible_msg_type;
	char *html_stripped_buffer;
	gboolean b_simias_msg_handled;
	
	b_simias_msg_handled = FALSE;

	g_print("Receiving message on account: %s\n",
		gaim_account_get_username(account));
	g_print("Sender: %s\n", *sender);

	/**
	 * Examine this message to see if it's a special Simias Gaim message.  If it
	 * is, extract the message and take the appropriate action.  Also, if it is
	 * one of these special messages, return TRUE from this function so no other
	 * module in Gaim gets the message.
	 */
	html_stripped_buffer = gaim_markup_strip_html(*buffer);
	if (!html_stripped_buffer) {
		/**
		 * Couldn't strip the buffer of HTML.  It's likely that it's not a
		 * special Simias message if this is the case.  Return FALSE to allow
		 * the message to be passed on.
		 */
		return FALSE;
	}

	possible_msg_type = get_possible_simias_msg_type(html_stripped_buffer);

	switch (possible_msg_type) {
		case INVITATION_REQUEST:
			b_simias_msg_handled =
				handle_invitation_request(account, *sender, 
										  html_stripped_buffer);
			break;
		case INVITATION_REQUEST_DENY:
			b_simias_msg_handled = 
				handle_invitation_request_deny(account, *sender,
											   html_stripped_buffer);
			break;
		case INVITATION_REQUEST_ACCEPT:
			b_simias_msg_handled = 
				handle_invitation_request_accept(account, *sender,
												 html_stripped_buffer);
			break;
		case PING_REQUEST:
			b_simias_msg_handled = 
				handle_ping_request(account, *sender, html_stripped_buffer);
			break;
		case PING_RESPONSE:
			b_simias_msg_handled = 
				handle_ping_response(account, *sender, html_stripped_buffer);
			break;
		default:
			break;
	}
	
	g_free(html_stripped_buffer);
	
	if (b_simias_msg_handled) {
		return TRUE;	/* Prevent the message from passing through */
	} else {
		return FALSE;	/* The message wasn't a Simias Message */
	}
}

/**
 * Non-public Functions
 */
 
/**
 * Parse the first part of the buffer and if it matches the #defined type,
 * return the type, otherwise, return 0 (type unknown).
 * 
 * Note: The buffer passed to this function should be stripped of any HTML.
 */
static SIMIAS_MSG_TYPE
get_possible_simias_msg_type(const char *buffer)
{
	if (strstr(buffer, INVITATION_REQUEST_MSG) == buffer) {
		return INVITATION_REQUEST;
	} else if (strstr(buffer, INVITATION_REQUEST_DENY_MSG) == buffer) {
		return INVITATION_REQUEST_DENY;
	} else if (strstr(buffer, INVITATION_REQUEST_ACCEPT_MSG) == buffer) {
		return INVITATION_REQUEST_ACCEPT;
	} else if (strstr(buffer, PING_REQUEST_MSG) == buffer) {
		return PING_REQUEST;
	} else if (strstr(buffer, PING_RESPONSE_MSG) == buffer) {
		return PING_RESPONSE;
	} else {
		return 0;
	}
}

/**
 * This fuction checks to see if this is a properly formatted Simias Invitation
 * Request and then notifies the user of an incoming invitation.  The user can
 * then accept or deny the request at will.
 * 
 * Ideally, we should not cause a popup window to appear to the user, but just
 * a little notification bubble that would appear for a few seconds and then
 * disappear.  The user should be able to go the Invitations Window to attend to
 * incoming and outgoing invitations.
 * 
 * This is a candidate for a plugin setting (i.e., whether the user wants to be
 * notified/see a popup/bubble about incoming invitations).
 * 
 * Additionally, we could choose to pass the notification process right on to
 * the Simias/iFolder Client and just show incoming and outgoing invitations in
 * the Invitation Window.
 * 
 * If the request message has enough information, perhaps the invitation
 * response messages aren't even necessary and the user can start synchronizing
 * the collection immediately.  The invitation request could just be something
 * that adds an available collection to the user's list of collections.
 */
static gboolean
handle_invitation_request(GaimAccount *account, const char *sender, 
						  const char *buffer)
{
	/**
	 * Since this method is called, we already know that the first part of
	 * the message matches our #define.  So, because of that, we can take
	 * that portion out of the picture and start tokenizing the different
	 * parts.
	 */
	char *sender_ip_address;
	char *sender_ip_port;
	char *collection_id;
	char *collection_type;
	char *collection_name;
	Invitation *invitation;
	GtkTreeIter iter;
	char time_str[32];
	char state_str[32];
	
g_print("handle_invitation_request() entered\n");
	/**
	 * Start parsing the message at this point:
	 * 
	 * 	[simias:invitation-request:<ip-address>:<ip-port>:<collection-id>:<collection-type>:<colletion-name>]
	 *                             ^
	 */
	sender_ip_address = strtok((char *) buffer + strlen(INVITATION_REQUEST_MSG), ":");
	if (!sender_ip_address) {
		g_print("handle_invitation_request() couldn't parse the sender-ip-address\n");
		return FALSE;
	}
	
	sender_ip_port = strtok(NULL, ":");
	if (!sender_ip_port) {
		g_print("handle_invitation_request() couldn't parse the ip-port\n");
		return FALSE;
	}
	
	collection_id = strtok(NULL, ":");
	if (!collection_id) {
		g_print("handle_invitation_request() couldn't parse the collection-id\n");
		return FALSE;
	}

	collection_type = strtok(NULL, ":");
	if (!collection_type) {
		g_print("handle_invitation_request() couldn't parse the collection-type\n");
		return FALSE;
	}

	collection_name = strtok(NULL, "]");
	if (!collection_name) {
		g_print("handle_invitation_request() couldn't parse the collection-name\n");
		return FALSE;
	}
	
	/**
	 * Check to see if we already have this invitation in our list (based on the
	 * collection-id) and if we do, update the invitation received time and
	 * notify the user of an incoming invitation (notify bubble or invitation
	 * window show)
	 */
	if (simias_lookup_collection_in_store(in_inv_store, collection_id, &iter)) {
		/* Extract the Invitation * from the model using iter */
		gtk_tree_model_get(GTK_TREE_MODEL(in_inv_store), &iter,
							INVITATION_PTR, &invitation,
							-1);
		/* Update the invitation time */
		time(&(invitation->time));
		
		/* Format the time to a string */
		simias_fill_time_str(time_str, 32, invitation->time);
		
		invitation->state = STATE_NEW;
		simias_fill_state_str(state_str, invitation->state);
	
		/* Update the out_inv_store */
		gtk_list_store_set(in_inv_store, &iter,
			TIME_COL,				time_str,
			STATE_COL,				state_str,
			-1);

		/* Save the updates to a file */
g_print("about_to_write 7\n");
		simias_save_invitations(in_inv_store);
	} else {
		/**
		 * Construct an Invitation, fill it with information, add it to the
		 * in_inv_store, and show the Invitations Dialog.
		 */
		invitation = malloc(sizeof(Invitation));
		if (!invitation) {
			g_print("out of memory in handle_invitation_request()\n");
			return TRUE; /* The message must be discarded */
		}
	
		invitation->gaim_account = account;
		sprintf(invitation->buddy_name, sender);
		invitation->state = STATE_NEW;
		
		/* Get the current time to store as the received time */
		time(&(invitation->time));
		
		sprintf(invitation->collection_id, collection_id);
		sprintf(invitation->collection_type, collection_type);
		sprintf(invitation->collection_name, collection_name);
		sprintf(invitation->ip_addr, sender_ip_address);
		sprintf(invitation->ip_port, sender_ip_port);
		
		/* Now add this new invitation to the in_inv_store. */
		simias_add_invitation_to_store(in_inv_store, invitation);
	}
	
	/* FIXME: Change this to a tiny bubble notification instead of popping up the big Invitations Dialog */
	/* Check the notify preference */
	if (gaim_prefs_get_bool(SIMIAS_PREF_NOTIFY_RECEIVE_NEW_INVITATIONS)) {
		simias_show_invitations_window();
	}
	
	return TRUE;	/* Message was handled correctly */
}

/**
 * This function checks to see if the buffer is a properly formatted deny
 * message and handles it appropriately.
 * 
 * Don't do anything with this message if any of the following are true:
 * 
 * 		1. There is no "pending invitation" in our outgoing invitation list
 * 		2. If Simias is up and running and we can see that the collection in
 * 		   question doesn't exist.
 * 		3. If Simias is up and running and we can tell that the sender is not
 * 		   member of the collection.
 * 
 * If the denies the invitation, mark the status of the invitation object that's
 * in the outgoing invitation list as rejected.  If the user deletes the
 * invitation from the outgoing invitation list after it's been marked as
 * rejected, ask the user whether they want to remomve that buddy from the
 * actual member list in Simias.
 */
static gboolean
handle_invitation_request_deny(GaimAccount *account,
							   const char *sender,
							   const char *buffer)
{
	GtkTreeIter iter;
	Invitation *invitation = NULL;
	char time_str[32];
	char state_str[32];

	/**
	 * Since this method is called, we already know that the first part of
	 * the message matches our #define.  So, because of that, we can take
	 * that portion out of the picture and start tokenizing the different
	 * parts.
	 */
	char *collection_id;
	
	/**
	 * Start parsing the message at this point:
	 * 
	 * 	[simias:invitation-request-deny:<collection-id>]
	 *                                  ^
	 */
	collection_id = strtok((char *) buffer + strlen(INVITATION_REQUEST_DENY_MSG), "]");
	if (!collection_id) {
		g_print("handle_invitation_request_deny() couldn't parse the collection-id\n");
		return FALSE;
	}
	
	/**
	 * Lookup the collection_id in the current out_inv_store.  If it's not there
	 * we'll just discard this message.  If it IS there, we need to update the
	 * status of the invitation and take more action with Simias.
	 */
	if (!simias_lookup_collection_in_store(out_inv_store, collection_id, &iter)) {
		g_print("handle_invitation_request_deny() couldn't find the collection-id in out_inv_store\n");
		/* FIXME: Before returning from here, we should try to retrieve more information from Simias in case the user deleted this invitation information from Gaim */
		return TRUE; /* Discard the message */
	}
	
	/**
	 * If we get this far, iter now points to the row of data in the store model
	 * that contains the Invitation * corresponding with this message.
	 * 
	 * Update the time and the invitation state in the store/model.
	 */

	/* Extract the Invitation * out of the model */
	gtk_tree_model_get(GTK_TREE_MODEL(out_inv_store), &iter,
						INVITATION_PTR, &invitation,
						-1);

	/**
	 * Double-check to make sure nothing has changed since returning from the
	 * lookup call.  If it did, call this function recursively.  If the row
	 * was actually removed, it should stop processing in the recursive call.
	 */
	if (strcmp(invitation->collection_id, collection_id) != 0) {
		/* The row changed */
		return handle_invitation_request_deny(account, sender, buffer);
	}
	
	/* Update the invitation time */
	time(&(invitation->time));
	
	/* Update the invitation state */
	invitation->state = STATE_REJECTED;
	
	/* Format the time to a string */
	simias_fill_time_str(time_str, 32, invitation->time);

	/* Format the state string */
	simias_fill_state_str(state_str, invitation->state);
	
	/* Update the out_inv_store */
	gtk_list_store_set(out_inv_store, &iter,
		TIME_COL,				time_str,
		STATE_COL,				state_str,
		-1);

	/* Save the updates to a file */
	simias_save_invitations(out_inv_store);

	/* Make sure the buttons are in the correct state */
	simias_out_inv_sel_changed_cb(
		gtk_tree_view_get_selection(GTK_TREE_VIEW(out_inv_tree)),
		GTK_TREE_VIEW(out_inv_tree));
	
	/* FIXME: Add more interaction with Simias as described in the notes of the function */

	/* FIXME: Change this to a tiny bubble notification instead of popping up the big Invitations Dialog */
	if (gaim_prefs_get_bool(SIMIAS_PREF_NOTIFY_REJECT_INVITATIONS)) {
		simias_show_invitations_window();
	}
	
	return TRUE;	/* Message was handled correctly */
}

/**
 * This function checks to see if the buffer is a properly formatted accept
 * message and handles it appropriately.
 * 
 * The conditions mentioned in the "Don't do anything" section in the above
 * function apply here too.
 * 
 * If the message is valid and the user is in the list, update the buddy's IP
 * Address in the Simias Gaim Domain Roster.  If the user accepted, Simias
 * should likely already know about the machine and should have already started
 * the sync process.  If this is not the case, then tell Simias to start syncing
 * the collection with the buddy's machine.
 * 
 * [simias:invitation-request-accept:<collection-id>:<ip-address>:<ip-port>]
 */
static gboolean
handle_invitation_request_accept(GaimAccount *account,
								 const char *sender,
								 const char *buffer)
{
	GtkTreeIter iter;
	Invitation *invitation;
	char time_str[32];
	char state_str[32];
	GaimBuddy *buddy;
	GtkTreeIter tb_iter;
	
	/**
	 * Since this method is called, we already know that the first part of
	 * the message matches our #define.  So, because of that, we can take
	 * that portion out of the picture and start tokenizing the different
	 * parts.
	 */
	char *collection_id;
	char *ip_address;
	char *ip_port;
	
	/**
	 * Start parsing the message at this point:
	 * 
	 * 	[simias:invitation-request-accept:<collection-id>:<ip-address>:<ip-port>]
	 *                                    ^
	 */
	collection_id = strtok(
				(char *) buffer + strlen(INVITATION_REQUEST_ACCEPT_MSG), ":");
	if (!collection_id) {
		g_print("handle_invitation_request_accept() couldn't parse the collection-id\n");
		return FALSE;
	}

	ip_address = strtok(NULL, ":");
	if (!ip_address) {
		g_print("handle_invitation_request_accept() couldn't parse the ip-address\n");
		return FALSE;
	}
	
	ip_port = strtok(NULL, "]");
	if (!ip_port) {
		g_print("handle_invitation_request_accept() couldn't parse the ip-port\n");
		return FALSE;
	}
	
	/**
	 * Lookup the collection_id in the current out_inv_store.  If it's not there
	 * we'll just discard this message.  If it IS there, we need to update the
	 * status of the invitation and take more action with Simias.
	 */
	if (!simias_lookup_collection_in_store(out_inv_store, collection_id, &iter)) {
		g_print("handle_invitation_request_accept() couldn't find the collection-id in out_inv_store\n");
		/* FIXME: Before returning from here, we should try to retrieve more information from Simias in case the user deleted this invitation information from Gaim */
		return TRUE;	/* Allow the message to be discarded */
	}
	
	/**
	 * If we get this far, iter now points to the row of data in the store model
	 * that contains the Invitation * corresponding with this message.
	 * 
	 * Update the time and the invitation state in the store/model.
	 */

	/* Extract the Invitation * out of the model */
	gtk_tree_model_get(GTK_TREE_MODEL(out_inv_store), &iter,
						INVITATION_PTR, &invitation,
						-1);
						
	/**
	 * Double-check to make sure nothing has changed since returning from the
	 * lookup call.  If it did, call this function recursively.  If the row
	 * was actually removed, it should stop processing in the recursive call.
	 */
	if (strcmp(invitation->collection_id, collection_id) != 0) {
		/* The row changed */
		return handle_invitation_request_deny(account, sender, buffer);
	}
	
	/* Update the invitation time */
	time(&(invitation->time));
	
	/* Update the invitation state */
	invitation->state = STATE_ACCEPTED_PENDING;
	
	/* Format the time to a string */
	simias_fill_time_str(time_str, 32, invitation->time);

	/* Format the state string */
	simias_fill_state_str(state_str, invitation->state);
	
	/* Update the out_inv_store */
	gtk_list_store_set(out_inv_store, &iter,
		TIME_COL,				time_str,
		STATE_COL,				state_str,
		-1);

	/* Save the updates to a file */
g_print("about_to_write 9\n");
	simias_save_invitations(out_inv_store);

	/* Make sure the buttons are in the correct state */
	simias_out_inv_sel_changed_cb(
		gtk_tree_view_get_selection(GTK_TREE_VIEW(out_inv_tree)),
		GTK_TREE_VIEW(out_inv_tree));

	buddy = gaim_find_buddy(account, sender);

	/**
	 * Add the buddy to the list of trusted buddies if the buddy is not already
	 * there.  If the buddy IS there, just update their IP Address and IP Port.
	 */
	if (simias_lookup_trusted_buddy(trusted_buddies_store, buddy, &tb_iter)) {
		/* Update the trusted buddy info */
		gtk_list_store_set(trusted_buddies_store, &tb_iter,
							TRUSTED_BUDDY_IP_ADDR_COL, ip_address,
							TRUSTED_BUDDY_IP_PORT_COL, ip_port,
							-1);

		/* Update the trusted buddies file */
		simias_save_trusted_buddies(trusted_buddies_store);
	} else {
		/* Add a new trusted buddy */
		simias_add_new_trusted_buddy(trusted_buddies_store, buddy, ip_address, ip_port);
	}

	/* FIXME: Add more interaction with Simias as described in the notes of the function */

	/* FIXME: Change this to a tiny bubble notification instead of popping up the big Invitations Dialog */
	if (gaim_prefs_get_bool(SIMIAS_PREF_NOTIFY_ACCEPT_INVITATIONS)) {
		simias_show_invitations_window();
	}
	
	return TRUE;	/* Message was handled correctly */
}

/**
 * This function checks to see if this is a valid ping request and then handles
 * it.  If we can tell that we've never established any type of connection with
 * the sender before, don't send a reply.  Otherwise, reply with our IP Address.
 */
static gboolean
handle_ping_request(GaimAccount *account, const char *sender,
					const char *buffer)
{
	GaimBuddy *buddy;
	char *simias_url;
	int send_result;
	
g_print("handle_ping_request() %s -> %s entered\n",
		sender, gaim_account_get_username(account));
		
	/**
	 * Since this method is called, we already know that the first part of
	 * the message matches our #define.  So, because of that, we can take
	 * that portion out of the picture and start tokenizing the different
	 * parts.
	 */

	/**
	 * Start parsing the message at this point:
	 * 
	 * 	[simias:ping-request:<simias-url>]
	 *                           ^
	 */
	simias_url = strtok((char *) buffer + strlen(PING_REQUEST_MSG), "]");
	if (!simias_url) {
		g_print("handle_ping_request() couldn't parse the simias-url\n");
		return FALSE;
	}

	/* Don't do anything with the URL.  If we got to this point, it's a valid message */
	buddy = gaim_find_buddy(account, sender);	

	/* Send a ping-response message */
	send_result = simias_send_ping_resp(buddy);
	if (send_result <= 0) {
		g_print("handle_ping_request() couldn't send ping response: %d\n", send_result);
	}
	
	return TRUE;
}

/**
 * This function checks to see if the buffer is a properly formatted ping
 * response and then handles it correctly.
 * 
 * If the message is valid, update the buddy's IP Address in the Simias Gaim
 * Domain Roster.
 */
static gboolean
handle_ping_response(GaimAccount *account, const char *sender, 
					 const char *buffer)
{
	GaimBuddy *buddy;
	char *simias_url;
	
g_print("handle_ping_response() %s -> %s entered\n",
		sender, gaim_account_get_username(account));
		
	/**
	 * Since this method is called, we already know that the first part of
	 * the message matches our #define.  So, because of that, we can take
	 * that portion out of the picture and start tokenizing the different
	 * parts.
	 */

	/**
	 * Start parsing the message at this point:
	 * 
	 * 	[simias:ping-response:<simias-url>]
	 *                            ^
	 */
	simias_url = strtok((char *) buffer + strlen(PING_RESPONSE_MSG), "]");
	if (!simias_url) {
		g_print("handle_ping_request() couldn't parse the simias-url\n");
		return FALSE;
	}

	/* Update the buddy's simias-url in blist.xml */
	buddy = gaim_find_buddy(account, sender);	
	gaim_blist_node_set_string(&(buddy->node), "simias-url", simias_url);

	return TRUE;
}

