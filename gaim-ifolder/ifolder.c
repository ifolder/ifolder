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
 ***********************************************************************/

#include "internal.h"
#include "gtkgaim.h"

#include "blist.h"
#include "account.h"
#include <string.h>
#include <stdio.h>
#include <stdlib.h>
#include "conversation.h"
#include "connection.h"
#include "network.h"
#include <gtk/gtk.h>
#include <time.h>

#include "debug.h"
#include "prefs.h"
#include "signals.h"
#include "util.h"
#include "version.h"

#include "gtkplugin.h"

/****************************************************
 * Static Definitions (#defines)                    *
 ****************************************************/
#define IFOLDER_PLUGIN_ID "ifolder"

#define IFOLDER_MSG_IP_ADDR_REQ		"[simias:ip_addr_req:"
#define IFOLDER_MSG_IP_ADDR_RESP	"[simias:ip_addr_resp:"
#define IFOLDER_MSG_IP_ADDR_REQ_DENY	"[simias:ip_addr_req_deny]"

#define INVITATION_REQUEST_MSG		"[simias:invitation-request:"
#define INVITATION_REQUEST_DENY_MSG	"[simias:invitation-request-deny:"
#define INVITATION_REQUEST_ACCEPT_MSG	"[simias:invitation-request-accept:"
#define PING_REQUEST_MSG		"[simias:ping-request:"
#define PING_RESPONSE_MSG		"[simias:ping-response:"

/****************************************************
 * Type Definitions                                 *
 ****************************************************/
typedef enum
{
	INVITATION_REQUEST = 1,
	INVITATION_REQUEST_DENY,
	INVITATION_REQUEST_ACCEPT,
	PING_REQUEST,
	PING_RESPONSE
} SIMIAS_MSG_TYPE;


/**
 * The INVITATION_STATE Enumeration:
 * 
 * STATE_INIT: This state should be used when an Invitation is first allocated
 * in memory.
 * 
 * STATE_PENDING: The invitation has been added by Simias but not sent yet.  If
 * an invitation stays in this state for a while it's likely that the buddy is
 * not online.  When a "buddy-sign-on" event occurs for this buddy, the
 * invitation will be sent at that point.
 *
 * STATE_SENT: The invitation has been sent to the buddy but they haven't
 * replied yet.
 *
 * STATE_REJECTED: The buddy replied and rejected the invitation.  This
 * information should be kept around so we don't automatically resend the
 * invitation.
 *
 * STATE_ACCEPTED_PENDING: The buddy replied but Simias is not running so we
 * cannot update Simias.  When Simias returns to an online state, the code
 * should loop through all of these events and sync up this information.
 *
 * STATE_ACCEPTED: The buddy has accepted the invitation and we've informed
 * Simias with the information (IP Address) received from the buddy.
 */
typedef enum
{
	STATE_INIT,
	STATE_PENDING,
	STATE_SENT,
	STATE_REJECTED,
	STATE_ACCEPTED_PENDING,
	STATE_ACCEPTED
} INVITATION_STATE;

/****************************************************
 * Data Structures                                  *
 ****************************************************/
/*
typedef struct
{
	SIMIAS_MSG_TYPE type;
	char ip_addr[16];
} iFolderMessage;
*/

typedef struct
{
	char account_protocol_id[32];
	char account_username[128];
	char buddy_name[128];
	INVITATION_STATE state;
	time_t sent_time;
	char collection_id[64];
	char collection_type[32];
	char collection_name[128];
	char buddy_ip_addr[16];
} Invitation;

/****************************************************
 * Forward Declarations                             *
 ****************************************************/
static int send_msg_to_buddy(GaimBuddy *recipient, char *msg);
static int send_invitation_request_msg(GaimBuddy *recipient,
									   char *collection_id,
									   char *collection_type,
									   char *collection_name);
static int send_invitation_request_deny_msg(GaimBuddy *recipient,
											char *collection_id);
static int send_invitation_request_accept_msg(GaimBuddy *recipient,
											  char *collection_id);
static int send_ping_request_msg(GaimBuddy *recipient);
static int send_ping_response_msg(GaimBuddy *recipient);

static SIMIAS_MSG_TYPE get_possible_simias_msg_type(const char *buffer);

static void sync_buddy_with_simias_roster(gpointer key,
										  gpointer value,
										  gpointer user_data);

static void buddylist_cb_simulate_share_ifolder(GaimBlistNode *node,
												gpointer user_data);

static void buddylist_cb_show_invitations_window(GaimBlistNode *node,
												 gpointer user_data);

static void blist_add_context_menu_items_cb(GaimBlistNode *node, GList **menu);

static gboolean is_valid_ip_part(const char *ip_part);
static int length_of_ip_address(const char *buffer);

static gboolean receiving_im_msg_cb(GaimAccount *account, char **sender,
									char **buffer, int *flags, void *data);

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

static void buddy_signed_on_cb(GaimBuddy *buddy, void *user_data);

/****************************************************
 * Function Implementations                         *
 ****************************************************/

/**
 * This function takes a generic message and sends it to the specified recipient
 * if they are online.
 */
static int
send_msg_to_buddy(GaimBuddy *recipient, char *msg)
{
	GaimConnection *conn;

	/**
	 * Make sure the buddy is online.  More information on the "present" field
	 * can be seen here: http://gaim.sourceforge.net/api/struct__GaimBuddy.html
	 */
	if (recipient->present == 0) {
		return -1; /* Buddy is offline */
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
 * [simias:invitation-request:<sender-ip-address>:<collection-id>:<collection-type>:<collection-name>]<Human-readable Invitation String for buddies who don't have the plugin installed/enabled>
 */
static int
send_invitation_request_msg(GaimBuddy *recipient, char *collection_id,
							char *collection_type, char *collection_name)
{
	char msg[2048];
	const char *public_ip;
	
	public_ip = gaim_network_get_my_ip(-1);

	/* FIXME: Escape any colons (:) in the Collection name before sending the message */
	
	sprintf(msg, "%s%s:%s:%s:%s] %s",
			INVITATION_REQUEST_MSG,
			public_ip,
			collection_id,
			collection_type,
			collection_name,
			_("I'd like to share an iFolder with you through Gaim but you don't have the iFolder Plugin installed/enabled.  You can download it from http://www.ifolder.com/.  Let me know when you've installed and enabled it and I will re-invite you to share my iFolder."));

	return send_msg_to_buddy(recipient, msg);
}

/**
 * This function will send a message with the following format:
 * 
 * [simias:invitation-request-deny:<collection-id>]
 */
static int
send_invitation_request_deny_msg(GaimBuddy *recipient, char *collection_id)
{
	char msg[2048];
	
	sprintf(msg, "%s%s]", INVITATION_REQUEST_DENY_MSG, collection_id);

	return send_msg_to_buddy(recipient, msg);
}

/**
 * This function will send a message with the following format:
 * 
 * 
 * [simias:invitation-request-accept:<collection-id>:<ip-address>]
 */
static int
send_invitation_request_accept_msg(GaimBuddy *recipient, char *collection_id)
{
	char msg[2048];
	const char *public_ip;
	
	public_ip = gaim_network_get_my_ip(-1);
	
	sprintf(msg, "%s%s:%s]",
			INVITATION_REQUEST_ACCEPT_MSG,
			collection_id,
			public_ip);

	return send_msg_to_buddy(recipient, msg);
}

/**
 * This function will send a message with the following format:
 * 
 * [simias:ping-request:<ip-address>]
 */
static int
send_ping_request_msg(GaimBuddy *recipient)
{
	char msg[2048];
	const char *public_ip;
	
	public_ip = gaim_network_get_my_ip(-1);
	
	sprintf(msg, "%s%s]", PING_REQUEST_MSG, public_ip);

	return send_msg_to_buddy(recipient, msg);
}

/**
 * This function will send a message with the following format:
 * 
 * [simias:ping-response:<ip-address>]
 */
static int
send_ping_response_msg(GaimBuddy *recipient)
{
	char msg[2048];
	const char *public_ip;
	
	public_ip = gaim_network_get_my_ip(-1);
	
	sprintf(msg, "%s%s]", PING_RESPONSE_MSG, public_ip);

	return send_msg_to_buddy(recipient, msg);
}

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
 * This function is used to loop through ALL of the buddies in the buddy list
 * when Gaim first starts up (and Simias is running) or when Gaim has been
 * running and Simias comes online.
 * 
 * The information in Gaim breaks any ties (i.e., it wins on the conflict
 * resolution).  All changes to information about Gaim buddies should be done
 * by the user in Gaim.  The Gaim Roster list in Simias should not be editable
 * in iFolder/The Client Application.
 */
static void
sync_buddy_with_simias_roster(gpointer key, gpointer value, gpointer user_data)
{
	GaimBuddy *buddy = (GaimBuddy *)value;
	
	g_print("FIXME: Implement sync_buddy_with_simias_roster(): %s\n",
			gaim_buddy_get_alias(buddy));
			
	/**
	 * FIXME: Implement PSEUDOCODE in sync_buddy_with_simias_roster
	 * 
	 * PSEUDOCODE:
	 * 
	 * if (gaim buddy already exists in Simias Gaim Roster) {
	 * 		- Update the Simias Gaim Roster with any changes in the Buddy List
	 * 		- The Gaim Buddy List wins any conflicts
	 * 
	 * 		- If we already have an IP Address stored in Simias for this user,
	 * 		  send a [simias:ping-request] message so we can make sure that the
	 * 		  IP Address reflects what is current for any buddies who are
	 * 		  currently online.
	 * 
	 * 		- Check all the collections in Simias which this buddy may have been
	 * 		  added to and we've never sent an invitation for.  Send out a
	 * 		  [simias:invitation-request] message to the buddy for any that
	 * 		  match this condition.
	 * } else {
	 * 		- This is a buddy that has never been added to The Simias Gaim
	 * 		  Roster so add the buddy to the roster.
	 * }
	 */
}

/**
 * FIXME: Remove this function once we've tied in with Simias since invitation
 * requests will be started when an event is received when a member is added to
 * a Simias Gaim collection.  This menu could be easily replaced with one that
 * would open a special dialog that would list all the Collections in the Gaim
 * Domain and ask the user whether they wanted to add a buddy as a member to
 * one or more of these Collections.  Or, it could offer the ability to create
 * a new Collection on the fly right there and add this buddy as a member.  This
 * would be "Icing on the Cake".
 * 
 * This function should prompt the user for a Collection Name and spoof a
 * Simias ID and Collection Type before sending an invitation to the selected
 * buddy.
 */
static void
buddylist_cb_simulate_share_ifolder(GaimBlistNode *node, gpointer user_data)
{
	g_print("FIXME: Implement buddylist_cb_simulate_share_ifolder()\n");
}

/**
 * Open and show the Simias Invitations Window
 */
static void
buddylist_cb_show_invitations_window(GaimBlistNode *node, gpointer user_data)
{
	g_print("FIXME: Implement buddylist_cb_show_invitations_window()\n");
}

/**
 * This function adds extra context menu items to a buddy in the Buddy List that
 * are specific to this plugin.
 */
static void
blist_add_context_menu_items_cb(GaimBlistNode *node, GList **menu)
{
	GaimBlistNodeAction *act;
	GaimBuddy *buddy;

	if (!GAIM_BLIST_NODE_IS_BUDDY(node))
		return;

	buddy = (GaimBuddy *)node;

	act = gaim_blist_node_action_new(_("Share iFolder (Simulation)"),
		buddylist_cb_simulate_share_ifolder, NULL);
	*menu = g_list_append(*menu, act);
	
	act = gaim_blist_node_action_new(_("Simias Invitations"),
		buddylist_cb_show_invitations_window, NULL);
	*menu = g_list_append(*menu, act);
}

/**
 * This function takes a part of an IP Address and validates that it is a number
 * AND that it is a number >= 0 and < 255.
 */
static gboolean
is_valid_ip_part(const char *ip_part)
{
	long int num;
	char *error_ptr = NULL;

	g_print("is_valid_ip_part(\"%s\") entered\n", ip_part);
	num = strtol(ip_part, &error_ptr, 10);
	if (error_ptr && error_ptr[0] != '\0') {
		g_print("strtol() (%d) failed because error_ptr was NOT NULL and NOT an empty string\n", (int) num);
		return FALSE;
	}

	if (num >= 0 && num < 255) {
		g_print("is_valid_ip_part() returning TRUE\n");
		return TRUE;
	} else {
		g_print("is_valid_ip_part() returning FALSE\n");
		return FALSE;
	}
}

/**
 * This function takes a buffer and parses for an IP address.
 *
 * The original buffer will be something like:
 *
 *      [simias:<message type>:xxx.xxx.xxx.xxx[':' or ']'<Any number of characters can be here>
 *                             ^
 *
 * This function receives the buffer starting with the character
 * marked with '^' in the above examples.
 *
 * This function returns the length of the IP Address (the char *) before a ':'
 * or ']' character or -1 if the message does not contain a string that
 * resembles an IP Address.
 */
static int
length_of_ip_address(const char *buffer)
{
	char *possible_ip_addr;
	char *part1, *part2, *part3, *part4;
	int possible_length;

	g_print("length_of_ip_address() called with: %s\n", buffer);

	/* Buffer must be at least "x.x.x.x]" (8 chars long) */
	if (strlen(buffer) < 8) {
		g_print("Buffer is not long enough to contain an IP Address\n");
		return -1; /* Not a valid message */
	}

	/* The IP Address must be followed by a ':' or ']' */
	possible_ip_addr = strtok((char *)buffer, ":]");
	if (!possible_ip_addr) {
		g_print("Buffer did not contain a ':' or ']' character\n");
		return -1;
	}

	/**
	 * An IP Address has to be at least "x.x.x.x" (7 chars long)
	 * but no longer than "xxx.xxx.xxx.xxx" (15 chars long).
	 */
	possible_length = strlen(possible_ip_addr);
	if (possible_length < 7 || possible_length > 15) {
		g_print("Buffer length was less than 7 or greater than 15\n");
		return -1;
	}

	/* Now verify that all the parts of an IP address exist */
	part1 = strtok(possible_ip_addr, ".");
	if (!part1 || !is_valid_ip_part(part1)) {
		g_print("Part 1 invalid\n");
		return -1;
	}

	part2 = strtok(NULL, ".");
	if (!part2 || !is_valid_ip_part(part2)) {
		g_print("Part 2 invalid\n");
		return -1;
	}

	part3 = strtok(NULL, ".");
	if (!part3 || !is_valid_ip_part(part3)) {
		g_print("Part 3 invalid\n");
		return -1;
	}

	part4 = strtok(NULL, ".");
	if (!part4 || !is_valid_ip_part(part4)) {
		g_print("Part 4 invalid\n");
		return -1;
	}

	g_print("length_of_ip_address() returning: %d\n", possible_length);
	/**
	 * If the code makes it to this point, possible_ip_addr looks
	 * like a valid IP Address (Note: this is not an exhaustive test).
	 */
	return possible_length;
}

/**
 * This function will be called any time Gaim starts receiving an instant
 * message.  This gives our plugin the ability to jump in and check to see if
 * it is a special message sent by another buddy who is also using this plugin.
 * If it IS a special Simias message, we intercept it and prevent it from being
 * displayed to the user.
 */
static gboolean
receiving_im_msg_cb(GaimAccount *account, char **sender, char **buffer,
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
	g_print("FIXME: Implement handle_invitation_request()\n");
	return FALSE;
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
	g_print("FIXME: Implement handle_invitation_request_deny()\n");
	return FALSE;
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
 */
static gboolean
handle_invitation_request_accept(GaimAccount *account,
								 const char *sender,
								 const char *buffer)
{
	g_print("FIXME: Implement handle_invitation_request_accept()\n");
	return FALSE;
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
	g_print("FIXME: Implement handle_ping_request()\n");
	return FALSE;
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
	g_print("FIXME: Implement handle_ping_response()\n");
	return FALSE;
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
 */
static void
buddy_signed_on_cb(GaimBuddy *buddy, void *user_data)
{
	g_print("FIXME: Implement buddy_signed_on_cb()\n");
}

static gboolean
plugin_load(GaimPlugin *plugin)
{
	GaimBuddyList *blist;

	blist = gaim_get_blist();
	if (blist) {
		g_hash_table_foreach(blist->buddies,
							 sync_buddy_with_simias_roster,
							 NULL);
	} else {
		g_print("gaim_get_blist() returned NULL\n");
	}

	gaim_signal_connect(gaim_blist_get_handle(),
				"blist-node-extended-menu",
				plugin,
				GAIM_CALLBACK(blist_add_context_menu_items_cb),
				NULL);

	gaim_signal_connect(gaim_conversations_get_handle(),
				"receiving-im-msg",
				plugin,
				GAIM_CALLBACK(receiving_im_msg_cb),
				NULL);

	gaim_signal_connect(gaim_blist_get_handle(),
				"buddy-signed-on",
				plugin,
				GAIM_CALLBACK(buddy_signed_on_cb),
				NULL);
				
	/**
	 * FIXME: Write and submit a patch to Gaim to emit a buddy-added-to-blist
	 * event with a very detailed explanation of why it is needed.
	 * 
	 * We need that type of event because when Gaim first starts up or when
	 * Simias/iFolder is started, we synchronize/add all the buddies in the
	 * Buddy List to the Simias Gaim Domain Roster.  Since this event doesn't
	 * exist, we could potentially miss adding a buddy to the domain roster only
	 * if the buddy is not signed-on.  If the buddy is signed on, the buddy
	 * would be added in the buddy_signed_on_cb() function because that event is
	 * emitted when you add a new buddy and the buddy is signed-on.
	 * 
	 * Once this event is created and emitted, connect to it in this function
	 * and implement the callback function.
	 */

	return TRUE;
}

static GaimPluginInfo info =
{
	GAIM_PLUGIN_MAGIC,
	GAIM_MAJOR_VERSION,
	GAIM_MINOR_VERSION,
	GAIM_PLUGIN_STANDARD,
	GAIM_GTK_PLUGIN_TYPE,
	0,
	NULL,
	GAIM_PRIORITY_DEFAULT,
	IFOLDER_PLUGIN_ID,
	N_("iFolder"),
	VERSION,
	N_("Allows you to share iFolders with your Gaim contacts."),
	N_("Buddies that are in your contact list will automatically be added as contacts in iFolder that you'll be able to share iFolders with."),
	"Boyd Timothy <btimothy@novell.com>",
	"http://www.ifolder.com/",
	plugin_load,
	NULL,
	NULL,
	NULL,
	NULL,
	NULL
};

static void
init_plugin(GaimPlugin *plugin)
{
}

GAIM_INIT_PLUGIN(ifolder, init_plugin, info)
