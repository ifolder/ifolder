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
#include "simias-util.h"
#include "simias-prefs.h"
#include "gaim-domain.h"

/* Gaim Includes */
#include "internal.h"
#include "network.h"
#include "util.h"

/**
 * Non-public Functions
 */
static int send_ping(GaimBuddy *recipient, const char *ping_type);
static gboolean parse_simias_info(char *buffer, char **machineName, char **userID, char **simiasURL);
static SIMIAS_MSG_TYPE get_possible_simias_msg_type(const char *buffer);

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
fprintf(stderr, "gaim_account_get_connection() returned null\n");
		return -1; /* Can't send a msg without a connection */
	}

	g_print("Sending message: %s\n", msg);
	return serv_send_im(conn, recipient->name, msg, 0);
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
 * This will either send a ping request or a ping response based
 * on the ping_type parameter.
 */
static int
send_ping(GaimBuddy *recipient, const char *ping_type)
{
	char msg[2048];
	char *machineName;
	char *userID;
	char *simias_service_url;
	char *public_url;
	int err;

	/* Get the Gaim Domain User Info */
	err = simias_get_user_info(&machineName, &userID, &simias_service_url); 
	if (err != 0)
	{
		/* There was an error and none of the returns are valid */
fprintf(stderr, "simias_get_user_info() returned: %d\n", err);
		return -23432;
	}

	public_url = convert_url_to_public(simias_service_url);
	if (public_url) {
		sprintf(msg, "%s%s:%s:%s]", ping_type, machineName, userID, public_url);
		free(public_url);
	} else {
		sprintf(msg, "%s%s:%s:%s]", ping_type, machineName, userID, simias_service_url);
	}
	
	free(machineName);
	free(userID);
	free(simias_service_url);

	return simias_send_msg(recipient, msg);
}

/**
 * This function will send a message with the following format:
 * 
 * [simias:ping-request:<sender-machine-name>:<sender-user-id>:<simias-url>]
 */
int
simias_send_ping_req(GaimBuddy *recipient)
{
	return send_ping(recipient, PING_REQUEST_MSG);
}

/**
 * This function will send a message with the following format:
 * 
 * [simias:ping-response:<sender-machine-name>:<sender-user-id>:<simias-url>]
 */
int
simias_send_ping_resp(GaimBuddy *recipient)
{
	return send_ping(recipient, PING_RESPONSE_MSG);
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
	if (strstr(buffer, PING_REQUEST_MSG) == buffer) {
		return PING_REQUEST;
	} else if (strstr(buffer, PING_RESPONSE_MSG) == buffer) {
		return PING_RESPONSE;
	} else {
		return 0;
	}
}

/**
 * This function will parse the machineName,
 * userID, and simiasURL from a ping message.
 *
 * The buffer should contain the part of the simias message
 * AFTER the ping type, as shown here:
 *
 *     [simias:<ping-type>:<sender-machine-name>:<sender-user-id>:<simias-url>]
 *                         ^
 *
 * Returns TRUE if all the fields could be parsed correctly, otherwise FALSE
 * is returned.
 */
static gboolean
parse_simias_info(char *buffer, char **machineName, char **userID, char **simiasURL)
{
	*machineName = strtok(buffer, ":");
	if (!*machineName) {
		fprintf(stderr, "parse_simias_info() couldn't parse the machine name\n");
		return FALSE;
	}

	*userID = strtok(NULL, ":");
	if (!*userID)
	{
		fprintf(stderr, "parse_simias_info() couldn't parse the user id\n");
		return FALSE;
	}

	*simiasURL = strtok(NULL, "]");
	if (!*simiasURL)
	{
		fprintf(stderr, "parse_simias_info() couldn't parse the simias url\n");
		return FALSE;
	}

	return TRUE;
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
	char *machine_name;
	char *user_id;
	char *simias_url;
	int send_result;
	const char *ping_reply_type;
	
fprintf(stderr, "handle_ping_request() %s -> %s entered\n",
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
	 * 	[simias:ping-request:<sender-machine-name>:<sender-user-id>:<simias-url>]
	 *                       ^
	 */
	if (!parse_simias_info((char *) buffer + strlen(PING_REQUEST_MSG),
						  &machine_name, &user_id, &simias_url))
	{
		return FALSE;
	}
	
	ping_reply_type = gaim_prefs_get_string(SIMIAS_PREF_PING_REPLY_TYPE);
	if (ping_reply_type) {
		/**
		 * If the ping reply type preference has been set to only reply to
		 * users who are in the buddy list, return out of this function if the
		 * sender of the ping is not in the buddy list.
		 */
		if (strcmp(ping_reply_type, SIMIAS_PREF_PING_REPLY_TYPE_BLIST) == 0) {
			/* Check to see if the sender is in the buddy list. */
			
			/* FIXME: Not sure that we're even going to get called if the sender isn't in our buddy list.  Need to test this. */
		}
	}

	/* Don't do anything with the URL.  If we got to this point, it's a valid message */
	buddy = gaim_find_buddy(account, sender);	

	/* Send a ping-response message */
	send_result = simias_send_ping_resp(buddy);
	if (send_result <= 0) {
		fprintf(stderr, "handle_ping_request() couldn't send ping response: %d\n", send_result);
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
	char *machine_name;
	char *user_id;
	char *simias_url;
	
	char user_id_setting[512];
	char simias_url_setting[512];
	
fprintf(stderr, "handle_ping_response() %s -> %s entered\n",
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
	 * 	[simias:ping-response:<sender-machine-name>:<sender-user-id>:<simias-url>]
	 *                        ^
	 */
	if (!parse_simias_info((char *) buffer + strlen(PING_RESPONSE_MSG),
						  &machine_name, &user_id, &simias_url))
	{
fprintf(stderr, "couldn't parse simias_info!\n");
		return FALSE;
	}
	
	/* Update the buddy's simias-url in blist.xml */
	buddy = gaim_find_buddy(account, sender);
	sprintf(user_id_setting, "simias-user-id:%s", machine_name);
	sprintf(simias_url_setting, "simias-url:%s", machine_name);
	gaim_blist_node_set_string(&(buddy->node), user_id_setting, user_id);
	gaim_blist_node_set_string(&(buddy->node), simias_url_setting, simias_url);

	/**
	 * Tell the Gaim Domain Sync Thread to go re-read the updated
	 * information about the buddy.
	 */
	simias_update_member(gaim_account_get_username(account),
						 gaim_account_get_protocol_id(account),
						 sender, machine_name);

	return TRUE;
}
