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
#include "buddy-profile.h"

/* Gaim Includes */
#include "internal.h"
#include "util.h"
#include "connection.h"

/**
 * Non-public Functions
 */
static int send_ping(GaimBuddy *recipient, const char *ping_type);
static SIMIAS_MSG_TYPE get_possible_simias_msg_type(const char *buffer);

static gboolean handle_invitation_request(GaimAccount *account,
										  const char *sender,
										  const char *buffer);
static gboolean handle_invitation_deny(GaimAccount *account,
									   const char *sender,
									   const char *buffer);
static gboolean handle_invitation_accept(GaimAccount *account,
										 const char *sender,
										 const char *buffer);
static gboolean handle_invitation_complete(GaimAccount *account,
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

fprintf(stderr, "Sending message: %s\n", msg);
	return serv_send_im(conn, recipient->name, msg, 0);
}

/**
 * This function sends a message with the following format:
 *
 * [simias:invitation-request:<Base64Encoded Public Key>]
 */
int
simias_send_invitation_request(GaimBuddy *recipient)
{
	char *public_key;
	char msg[4096];
	char *base64Key;
	
	if (simias_get_public_key(&public_key) != 0)
	{
		/* FIXME: Prompt the user to make sure iFolder is up and running...or kick Simias to start before continuing. */
		fprintf(stderr, "Error getting public key.  Maybe iFolder is not running?\n");
		return -1;
	}
	
	/* Base64Encode the public key so it gets sent nicely (not in XML) */
	base64Key = gaim_base64_encode(publicKey, strlen(publicKey));
	free(public_key);
	
	sprintf(msg, "%s%s]", INVITATION_REQUEST_MSG, base64Key);
	free(base64Key);
	
	return simias_send_msg(recipient, msg);
}

/**
 * This function sends a message with the following format:
 *
 * [simias:invitation-deny]
 */
int
simias_send_invitation_deny(GaimBuddy *recipient)
{
	return simias_send_msg(recipient, INVITATION_DENY_MSG);
}

/**
 * This function sends a message with the following format:
 *
 * [simias:invitation-accept:<Base64Encoded Public Key>:<Base64Encoded DES key encrypted with the recipient's public key>]
 */
int
simias_send_invitation_accept(GaimBuddy *recipient, char *recipientBase64PublicKey)
{
	char msg[4096];
	char *public_key;
	char *base64PublicKey;
	char *desKey;
	char *buddyPublicKey;
	int buddyPublicKeyLen;
	char settingName[4096];
	char *encryptedDESKey;
	int err;

	err = simias_get_public_key(&public_key);
	if (err != 0)
	{
		/* FIXME: Prompt the user to make sure iFolder is up and running...or kick Simias to start before continuing. */
		fprintf(stderr, "simias_get_public_key() returned an error (%d).  Maybe iFolder is not running?\n", err);
		return -1;
	}
	
	/* Base64Encode the public key so it gets sent nicely (not in XML) */
	base64PublicKey = gaim_base64_encode(publicKey, strlen(publicKey));
	free(public_key);

	err = simias_get_des_key(&desKey);
	if (err != 0)
	{
		free(base64PublicKey);
		fprintf(stderr, "simias_get_des_key() returned an error (%d).  Maybe iFolder is not running?\n", err);
		return -2;
	}

	gaim_base64_decode(recipientBase64PublicKey, &buddyPublicKey, &buddyPublicKeyLen);

	err = simias_rsa_encrypt_string(buddyPublicKey, desKey, &encryptedDESKey);
	free(buddyPublicKey);
	free(desKey);
	if (err != 0)
	{
		free(base64PublicKey);
		fprintf(stderr, "simias_rsa_encrypt_string() had an error (%d) in simias_send_invitation_accept()\n", err);
		return -3;
	}

	sprintf(msg, "%s%s:%s]", INVITATION_ACCEPT_MSG,
							 base64PublicKey,
							 encryptedDESKey);
	free(base64PublicKey);
	free(encryptedDESKey);

	return simias_send_msg(recipient, msg);
}

/**
 * This function sends a message with the following format:
 *
 * [simias:invitation-complete:<Base64Encoded symmetric key encrypted with the recipient's public key>]
 */
int
simias_send_invitation_complete(GaimBuddy *recipient, char *recipientBase64PublicKey);
{
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

	fprintf(stderr, "Receiving message on account: %s\n",
		gaim_account_get_username(account));
	fprintf(stderr, "Sender: %s\n", *sender);

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
				handle_invitation_request(account, *sender, html_stripped_buffer);
			if (!b_simias_msg_handled) {
				fprintf(stderr, "Error in invitation request message\n");
				return TRUE; /* Prevent the message from passing through */
			}
			break;
		case INVITATION_DENY:
			b_simias_msg_handled = 
				handle_invitation_deny(account, *sender, html_stripped_buffer);
			if (!b_simias_msg_handled) {
				fprintf(stderr, "Error in invitation deny message\n");
				return TRUE; /* Prevent the message from passing through */
			}
			break;
		case INVITATION_ACCEPT:
			b_simias_msg_handled = 
				handle_invitation_accept(account, *sender, html_stripped_buffer);
			if (!b_simias_msg_handled) {
				fprintf(stderr, "Error in invitation accept message\n");
				return TRUE; /* Prevent the message from passing through */
			}
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
	} else if (strstr(buffer, INVITATION_DENY_MSG) == buffer) {
		return INVITATION_DENY;
	} else if (strstr(buffer, INVITATION_ACCEPT_MSG) == buffer) {
		return INVITATION_ACCEPT;
	} else {
		return UNKNOWN_MSG_TYPE;
	}
}

static gboolean
handle_invitation_request(GaimAccount *account,
						  const char *sender,
						  const char *buffer)
{
	GaimBuddy *buddy;
	char *base64Key;
	int closeBracketPos;
	char *tmp;
	int err;
	GtkWidget *accept_dialog;
	gint response;
	const char *buddy_alias = NULL;

	/* FIXME: Add some type of UI callback to handle the invitation request */
	
fprintf(stderr, "handle_invitation_request() %s -> %s entered\n",
		sender, gaim_account_get_username(account));
		
	/**
	 * Start parsing the message at this point:
	 * 
	 * 	[simias:invitation-request:<Base64Encoded Public Key>]
	 *                             ^
	 */
	tmp = (char *)buffer + strlen(INVITATION_REQUEST_MSG);
	closeBracketPos = simias_str_index_of(tmp, ']');
	if (colonPos <= 0)
	{
		fprintf(stderr, "handle_invitation_request() couldn't parse the public key\n");
		return FALSE;
	}
	else
	{
		*base64Key = malloc(sizeof(char) * (closeBracketPos + 1));
		memset(*base64Key, '\0', closeBracketPos + 1);
		strncpy(*base64Key, tmp, closeBracketPos);
	}

	buddy = gaim_find_buddy(account, sender);
	buddy_alias = gaim_buddy_get_alias(buddy);

	/* Prompt the user to accept/reject the invitation */
	/* FIXME: Call the registered UI Callback for handling the invitation request */
	accept_dialog =
		gtk_message_dialog_new(NULL,
							GTK_DIALOG_DESTROY_WITH_PARENT | GTK_DIALOG_NO_SEPARATOR,
							GTK_MESSAGE_QUESTION,
							GTK_BUTTONS_YES_NO,
							_("%s would like to share files with you through iFolder.  Would you like to participate?",
							buddy_alias ? buddy_alias : sender);
	response = gtk_dialog_run(GTK_DIALOG(accept_dialog));

	if (response == GTK_RESPONSE_YES)
	{
		/* Send an accept message */
		err = simias_send_invitation_accept(buddy, base64Key);
		if (err <= 0)
		{
			/* FIXME: Call the registered UI Handler's error handler */
			fprintf(stderr, "Error sending accept invitation message\n");
		}

		/**
		 * Add on a temporary setting for public key so the get_info call
		 * will be able to decrypt the buddy info.
		 */
		gaim_blist_node_set_string(&(buddy->node),
								   "simias-temp-public-key",
								   base64Key);

		/* Queue up a call to read the buddy's profile */
		simias_get_buddy_profile(buddy);
	}
	else /* The user denied the invitation */
	{
		/* Send a deny message */
		err = simias_send_invitation_deny(buddy);
		if (err <= 0)
		{
			/* FIXME: Call the registered UI Handler's error handler */
			fprintf(stderr, "Error sending deny invitation message\n");
		}
	}

	gtk_widget_destroy(accept_dialog);

	free(base64Key);

	return TRUE;
}

static gboolean
handle_invitation_deny(GaimAccount *account,
					   const char *sender,
					   const char *buffer)
{
	GtkWidget *dialog;
	GaimBuddy *buddy;
	const char *buddy_alias = NULL;
	
	/* FIXME: Add some type of UI callback to handle the deny invitation */
	
	buddy = gaim_find_buddy(account, sender);
	buddy_alias = gaim_buddy_get_alias(buddy);

	dialog =
		gtk_message_dialog_new(NULL,
								GTK_DIALOG_DESTROY_WITH_PARENT | GTK_DIALOG_NO_SEPARATOR,
								GTK_MESSAGE_INFO,
								GTK_BUTTONS_OK,
								_("%s denied your request to enable iFolder file sharing.",
								buddy_alias ? buddy_alias : sender);
	gtk_dialog_run(GTK_DIALOG(dialog));
	gtk_widget_destroy(dialog);
	
	return TRUE;
}

static gboolean
handle_invitation_accept(GaimAccount *account,
						 const char *sender,
						 const char *buffer)
{
	GaimBuddy *buddy;
	char *base64PublicKey;
	int colonPos;
	char *encryptedDESKey;
	int closeBracketPos;
	char *tmp;
	char *privateKey;
	char *decryptedDESKey;
	GtkWidget *dialog;
	const char *buddy_alias = NULL;
	int err;

fprintf(stderr, "handle_invitation_accept() %s -> %s entered\n",
		sender, gaim_account_get_username(account));
		
	buddy = gaim_find_buddy(account, sender);

	/**
	 * Start parsing the message at this point:
	 * 
	 * [simias:invitation-accept:<Base64Encoded Public Key>:<Base64Encoded DES key encrypted with the recipient's public key>]
	 *                           ^
	 */
	tmp = (char *)buffer + strlen(INVITATION_ACCEPT_MSG);
	colonPos = simias_str_index_of(tmp, ':');
	if (colonPos <= 0)
	{
		fprintf(stderr, "handle_invitation_request() couldn't parse the public key\n");
		return FALSE;
	}
	base64PublicKey = malloc(sizeof(char) * (colonPos + 1));
	memset(base64PublicKey, '\0', colonPos + 1);
	strncpy(base64PublicKey, tmp, colonPos);

	/**
	 * Add on a temporary setting for public key so get_buddy_profile()
	 * will be able to save the public key with the correct machine name.
	 */
	gaim_blist_node_set_string(&(buddy->node),
							   "simias-temp-public-key",
							   base64PublicKey);

	tmp = tmp + colonPos + 1;
	closeBracketPos = simias_str_index_of(tmp, ']');
	if (closeBracketPos <= 0)
	{
		free(base64PublicKey);
		fprintf(stderr, "handle_invitation_request() couldn't parse the encrypted DES key\n");
		return FALSE;
	}
	encryptedDESKey = malloc(sizeof(char) * (closeBracketPos + 1));
	memset(encryptedDESKey, '\0', closeBracketPos + 1);
	strncpy(encryptedDESKey, tmp, closeBracketPos);

	/* Decrypt the DES key with our private key */
	err = simias_get_private_key(&privateKey);
	if (err != 0)
	{
		free(base64PublicKey);
		free(encryptedDESKey);
		fprintf(stderr, "Couldn't get our private key.  Maybe iFolder is not running?\n");
		return FALSE;
	}

	err = simias_des_decrypt_string(privateKey, encryptedDESKey, &decryptedDESKey);
	free(privateKey);
	free(encryptedDESKey);
	if (err != 0)
	{
		free(base64PublicKey);
		fprintf(stderr, "simias_des_decrypt_string() returned an error (%d)\n", err);
		return FALSE;
	}

	/**
	 * Add on a temporary setting for DES key so that get_info has
	 * the necessary information needed to decrypt the buddy profile.
	 */
	gaim_blist_node_set_string(&(buddy->node),
							   "simias-temp-des-key",
							   decryptedDESKey);
	free(decryptedDESKey);

	/* Queue up a call to read the buddy's profile */
	simias_get_buddy_profile(buddy);

	/* Send an invitation confirmation */
	err = simias_send_invitation_complete(buddy, base64PublicKey);
	free(base64PublicKey);
	if (err <= 0)
	{
		fprintf(stderr, "simias_send_invitation_complete() had an error (%d)\n", err);
		/* FIXME: Add UI callback to alert error */
	}

	/* FIXME: Add some type of UI callback to handle the accept invitation */

	/* FIXME: Check the plugin preferences of whether the accept invitation confirmation dialog should be shown. */

	buddy_alias = gaim_buddy_get_alias(buddy);

	dialog =
		gtk_message_dialog_new(NULL,
								GTK_DIALOG_DESTROY_WITH_PARENT | GTK_DIALOG_NO_SEPARATOR,
								GTK_MESSAGE_INFO,
								GTK_BUTTONS_OK,
								_("%s has accepted your request to enable iFolder file sharing.  To share files through iFolder, use the iFolder Client, create an iFolder in the Gaim Workgroup Domain, and add %s to the iFolder's member list."),
								buddy_alias ? buddy_alias : sender,
								buddy_alias ? buddy_alias : sender);
	gtk_dialog_run(GTK_DIALOG(dialog));
	gtk_widget_destroy(dialog);
	
	return TRUE;
}

static gboolean
handle_invitation_complete(GaimAccount *account,
						   const char *sender,
						   const char *buffer)
{
	GaimBuddy *buddy;
	char *base64Key;
	int closeBracketPos;
	char *tmp;
	GtkWidget *dialog;
	const char *buddy_alias = NULL;

fprintf(stderr, "handle_invitation_accept() %s -> %s entered\n",
		sender, gaim_account_get_username(account));
		
	/**
	 * Start parsing the message at this point:
	 * 
	 * 	[simias:invitation-complete:<Base64Encoded Public Key>]
	 *                              ^
	 */
	tmp = (char *)buffer + strlen(INVITATION_ACCEPT_MSG);
	closeBracketPos = simias_str_index_of(tmp, ']');
	if (colonPos <= 0)
	{
		fprintf(stderr, "handle_invitation_request() couldn't parse the public key\n");
		return FALSE;
	}
	else
	{
		*base64Key = malloc(sizeof(char) * (closeBracketPos + 1));
		memset(*base64Key, '\0', closeBracketPos + 1);
		strncpy(*base64Key, tmp, closeBracketPos);
	}

	buddy = gaim_find_buddy(account, sender);

	/**
	 * Add on a temporary setting for public key so the get_info call
	 * will be able to decrypt the buddy info.
	 */
	gaim_blist_node_set_string(&(buddy->node),
							   "simias-temp-public-key",
							   base64Key);
	free(base64Key);

	/* Queue up a call to read the buddy's profile */
	simias_get_buddy_profile(buddy);
	
	/* FIXME: Add some type of UI callback to handle the accept invitation */

	/* FIXME: Check the plugin preferences of whether the accept invitation confirmation dialog should be shown. */

	buddy_alias = gaim_buddy_get_alias(buddy);

	dialog =
		gtk_message_dialog_new(NULL,
								GTK_DIALOG_DESTROY_WITH_PARENT | GTK_DIALOG_NO_SEPARATOR,
								GTK_MESSAGE_INFO,
								GTK_BUTTONS_OK,
								_("%s has accepted your request to enable iFolder file sharing.  To share files through iFolder, use the iFolder Client, create an iFolder in the Gaim Workgroup Domain, and add %s to the iFolder's member list."),
								buddy_alias ? buddy_alias : sender,
								buddy_alias ? buddy_alias : sender);
	gtk_dialog_run(GTK_DIALOG(dialog));
	gtk_widget_destroy(dialog);
	
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
	char *public_key;
	char *machine_name;
	char *user_id;
	char *simias_url;
	
	char public_key_setting[2048];
	char user_id_setting[512];
	char simias_url_setting[512];
	
fprintf(stderr, "handle_ping_response() %s -> %s entered\n",
		sender, gaim_account_get_username(account));
fprintf(stderr, "Message: %s\n", buffer);
		
	/**
	 * Since this method is called, we already know that the first part of
	 * the message matches our #define.  So, because of that, we can take
	 * that portion out of the picture and start tokenizing the different
	 * parts.
	 */

	/**
	 * Start parsing the message at this point:
	 * 
	 * 	[simias:ping-response:<sender-public-key>:<sender-machine-name>:<sender-user-id>:<simias-url>]
	 *                        ^
	 */
	if (!parse_simias_info(buffer + strlen(PING_RESPONSE_MSG),
						  &public_key, &machine_name, &user_id, &simias_url))
	{
fprintf(stderr, "couldn't parse simias_info!\n");
		return FALSE;
	}
	
	/* Update the buddy's simias-url in blist.xml */
	buddy = gaim_find_buddy(account, sender);
	sprintf(public_key_setting, "simias-public-key:%s", machine_name);
	sprintf(user_id_setting, "simias-user-id:%s", machine_name);
	sprintf(simias_url_setting, "simias-url:%s", machine_name);
	gaim_blist_node_set_string(&(buddy->node), public_key_setting, public_key);
	gaim_blist_node_set_string(&(buddy->node), user_id_setting, user_id);
	gaim_blist_node_set_string(&(buddy->node), simias_url_setting, simias_url);
	
	const char *test = gaim_blist_node_get_string(&(buddy->node), public_key_setting);
	char *decode = NULL;
	int decode_len;
	gaim_base64_decode(test, &decode, &decode_len);
	fprintf(stderr, "Decoded string: %s\n", decode);
	g_free(decode);

	/**
	 * Tell the Gaim Domain Sync Thread to go re-read the updated
	 * information about the buddy.
	 */
	simias_update_member(gaim_account_get_username(account),
						 gaim_account_get_protocol_id(account),
						 sender, machine_name);

	free(public_key);
	free(machine_name);
	free(user_id);
	free(simias_url);

	return TRUE;
}
