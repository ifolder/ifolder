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
 * [simias:invitation-request:<Base64Encoded Public Key>:<Base64Encoded Machine Name>]
 */
int
simias_send_invitation_request(GaimBuddy *recipient)
{
	char *public_key;
	char msg[4096];
	char *base64Key;
	int err;

	char *machineName;
	char *base64MachineName;

	err = simias_get_public_key(&public_key);
	if (err != 0)
	{
		/* FIXME: Prompt the user to make sure iFolder is up and running...or kick Simias to start before continuing. */
		fprintf(stderr, "Error getting public key (%d).  Maybe iFolder is not running?\n", err);
		return -1;
	}
	
	/* Base64Encode the public key so it gets sent nicely (not in XML) */
	base64Key = gaim_base64_encode(publicKey, strlen(publicKey));
	free(public_key);

	err = simias_get_machine_name(&machineName);
	if (err != 0)
	{
		free(base64Key);
		fprintf(stderr, "Error (%d) calling simias_get_machine_name() in simias_send_invitation_request().  Perhaps iFolder/Simias is not running?\n", err);
		return -2;
	}

	/* Base64Encode the machine name so it gets sent nicely */
	base64MachineName = gaim_base64_encode(machineName, strlen(machineName));
	free(machineName);
	
	sprintf(msg, "%s%s:%s]", INVITATION_REQUEST_MSG, base64Key, base64MachineName);
	free(base64Key);
	free(base64MachineName);
	
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
 * [simias:invitation-accept:<Base64Encoded Public Key>:<Base64Encoded Machine Name>:<Base64Encoded DES key encrypted with the recipient's public key>]
 */
int
simias_send_invitation_accept(GaimBuddy *recipient, char *recipientMachineName)
{
	char msg[4096];
	char *public_key;
	char *base64PublicKey;

	char *machineName;
	char *base64MachineName;

	char *desKey;

	char *recipientBase64PublicKey;
	char *recipientPublicKey;
	int recipientPublicKeyLen;
	char settingName[1024];
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

	err = simias_get_machine_name(&machineName);
	if (err != 0)
	{
		free(base64PublicKey);
		fprintf(stderr, "Error (%d) calling simias_get_machine_name() in simias_send_invitation_accept().  Perhaps iFolder/Simias is not running?\n", err);
		return -2;
	}

	/* Base64Encode the machine name so it gets sent nicely */
	base64MachineName = gaim_base64_encode(machineName, strlen(machineName));
	free(machineName);
	
	err = simias_get_des_key(&desKey);
	if (err != 0)
	{
		free(base64PublicKey);
		free(base64MachineName);
		fprintf(stderr, "simias_get_des_key() returned an error (%d).  Maybe iFolder is not running?\n", err);
		return -3;
	}

	sprintf(settingName, "simias-public-key:%s", recipientMachineName);
	recipientBase64PublicKey = gaim_blist_node_get_string(&(recipient->node), settingName);
	if (!recipientBase64PublicKey)
	{
		free(base64PublicKey);
		free(base64MachineName);
		free(desKey);
		fprintf(stderr, "Could not get the buddy's public key from blist.xml in simias_send_invitation_accept().\n");
		return -4;
	}

	gaim_base64_decode(recipientBase64PublicKey, &recipientPublicKey, &recipientPublicKeyLen);

	err = simias_rsa_encrypt_string(recipientPublicKey, desKey, &encryptedDESKey);
	free(recipientPublicKey);
	free(desKey);
	if (err != 0)
	{
		free(base64PublicKey);
		free(base64MachineName);
		fprintf(stderr, "simias_rsa_encrypt_string() had an error (%d) in simias_send_invitation_accept()\n", err);
		return -3;
	}

	sprintf(msg, "%s%s:%s:%s]", INVITATION_ACCEPT_MSG,
								base64PublicKey,
								base64MachineName,
								encryptedDESKey);
	free(base64PublicKey);
	free(base64MachineName);
	free(encryptedDESKey);

	return simias_send_msg(recipient, msg);
}

/**
 * This function sends a message with the following format:
 *
 * [simias:invitation-complete:<Base64Encoded Machine Name>:<Base64Encoded DES key encrypted with the recipient's public key>]
 */
int
simias_send_invitation_complete(GaimBuddy *recipient, char *recipientMachineName);
{
	char msg[4096];
	char *machineName;
	char *base64MachineName;

	char *desKey;

	char *recipientBase64PublicKey;
	char *recipientPublicKey;
	int recipientPublicKeyLen;
	char settingName[1024];
	char *encryptedDESKey;
	int err;

	err = simias_get_machine_name(&machineName);
	if (err != 0)
	{
		fprintf(stderr, "Error (%d) calling simias_get_machine_name() in simias_send_invitation_complete().  Perhaps iFolder/Simias is not running?\n", err);
		return -1;
	}

	/* Base64Encode the machine name so it gets sent nicely */
	base64MachineName = gaim_base64_encode(machineName, strlen(machineName));
	free(machineName);
	
	err = simias_get_des_key(&desKey);
	if (err != 0)
	{
		free(base64MachineName);
		fprintf(stderr, "simias_get_des_key() returned an error (%d) in simias_send_invitation_complete().  Maybe iFolder is not running?\n", err);
		return -2;
	}

	sprintf(settingName, "simias-public-key:%s", recipientMachineName);
	recipientBase64PublicKey = gaim_blist_node_get_string(&(recipient->node), settingName);
	if (!recipientBase64PublicKey)
	{
		free(base64MachineName);
		free(desKey);
		fprintf(stderr, "Could not get the buddy's public key from blist.xml in simias_send_invitation_complete().\n");
		return -3;
	}

	gaim_base64_decode(recipientBase64PublicKey, &recipientPublicKey, &recipientPublicKeyLen);

	err = simias_rsa_encrypt_string(recipientPublicKey, desKey, &encryptedDESKey);
	free(recipientPublicKey);
	free(desKey);
	if (err != 0)
	{
		free(base64MachineName);
		fprintf(stderr, "simias_rsa_encrypt_string() had an error (%d) in simias_send_invitation_complete()\n", err);
		return -4;
	}

	sprintf(msg, "%s%s:%s]", INVITATION_COMPLETE_MSG,
							 base64MachineName,
							 encryptedDESKey);
	free(base64MachineName);
	free(encryptedDESKey);

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
		case INVITATION_COMPLETE: 
			b_simias_msg_handled =
				handle_invitation_complete(account, *sender, html_stripped_buffer);
			if (!b_simias_msg_handled) {
				fprintf(stderr, "Error in invitation complete message\n");
				return TRUE; /* Prevent the message from passing through */
			}
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
 * Static Functions
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
	} else if (strstr(buffer, INVITATION_COMPLETE_MSG) == buffer) {
		return INVITATION_COMPLETE;
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
	char *base64MachineName;
	char *machineName;
	int machineNameLen;
	int colonPos;
	int closeBracketPos;
	char *tmp;
	int err;
	GtkWidget *accept_dialog;
	gint response;
	char settingName[1024];
	const char *buddy_alias = NULL;

	/* FIXME: Add some type of UI callback to handle the invitation request */
	
	fprintf(stderr, "handle_invitation_request() %s -> %s entered\n",
					sender, gaim_account_get_username(account));
		
	/**
	 * Start parsing the message at this point:
	 * 
	 *  [simias:invitation-request:<Base64Encoded Public Key>:<Base64Encoded Machine Name>]
	 *                             ^
	 */
	tmp = (char *)buffer + strlen(INVITATION_REQUEST_MSG);
	colonPos = simias_str_index_of(tmp, ':');
	if (colonPos <= 0)
	{
		fprintf(stderr, "handle_invitation_request() couldn't parse the public key\n");
		return FALSE;
	}

	base64Key = malloc(sizeof(char) * (colonPos + 1));
	memset(base64Key, '\0', colonPos + 1);
	strncpy(base64Key, tmp, colonPos);
	
	/* Parse the machine name */
	tmp = tmp + colonPos + 1;
	closeBracketPos = simias_str_index_of(tmp, "]");
	if (closeBracketPos <= 0)
	{
		free(base64Key);
		fprintf(stderr, "handle_invitation_request() couldn't parse the machine name\n");
		return FALSE;
	}
	
	base64MachineName = malloc(sizeof(char) * (closeBracketPos + 1));
	memset(base64MachineName, '\0', closeBracketPos + 1);
	strncpy(base64MachineName, tmp, closeBracketPos);
	
	gaim_base64_decode(base64MachineName, &machineName, &machineNameLen);
	free(base64MachineName);

	buddy = gaim_find_buddy(account, sender);
	buddy_alias = gaim_buddy_get_alias(buddy);

	/* Prompt the user to accept/reject the invitation */
	/* FIXME: Call the registered UI Callback for handling the invitation request */
	accept_dialog =
		gtk_message_dialog_new(NULL,
							GTK_DIALOG_DESTROY_WITH_PARENT | GTK_DIALOG_NO_SEPARATOR,
							GTK_MESSAGE_QUESTION,
							GTK_BUTTONS_YES_NO,
							_("%s (%s) would like to share files with you through iFolder.  Would you like to participate?",
							buddy_alias ? buddy_alias : sender,
							machineName);
	response = gtk_dialog_run(GTK_DIALOG(accept_dialog));

	if (response == GTK_RESPONSE_YES)
	{
		sprintf(settingName, "simias-public-key:%s", machineName);
		/* Save the buddy's public key in blist.xml */
		gaim_blist_node_set_string(&(buddy->node),
								   settingName,
								   base64Key);

		/* Send an accept message */
		err = simias_send_invitation_accept(buddy, machineName);
		if (err <= 0)
		{
			/* FIXME: Call the registered UI Handler's error handler */
			fprintf(stderr, "Error sending accept invitation message\n");
		}
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
	g_free(machineName);

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
	char *base64MachineName;
	char *machineName;
	int machineNameLen;
	char *encryptedDESKey;
	char *privateKey;
	char *decryptedDESKey;
	int colonPos;
	int closeBracketPos;
	char *tmp;
	char settingName[1024];
	GtkWidget *dialog;
	const char *buddy_alias = NULL;
	int err;

	fprintf(stderr, "handle_invitation_accept() %s -> %s entered\n",
		sender, gaim_account_get_username(account));
		
	buddy = gaim_find_buddy(account, sender);

	/**
	 * Start parsing the message at this point:
	 * 
	 * [simias:invitation-accept:<Base64Encoded Public Key>:<Base64Encoded Machine Name>:<Base64Encoded DES key encrypted with the recipient's public key>]
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

	tmp = tmp + colonPos + 1;
	colonPos = simias_str_index_of(tmp, ':');
	if (colonPos <= 0)
	{
		free(base64PublicKey);
		fprintf(stderr, "handle_invitation_request() couldn't parse the base64 encoded machine name\n");
		return FALSE;
	}
	base64MachineName = malloc(sizeof(char) * (colonPos + 1));
	memset(base64MachineName, '\0', colonPos + 1);
	strncpy(base64MachineName, tmp, colonPos);

	gaim_base64_decode(base64MachineName, &machineName, &machineNameLen);
	free(base64MachineName);

	/* Save the buddy's public key in blist.xml */
	sprintf(settingName, "simias-public-key:%s", machineName);
	gaim_blist_node_set_string(&(buddy->node),
							   settingName,
							   base64PublicKey);
	free(base64PublicKey);

	tmp = tmp + colonPos + 1;
	closeBracketPos = simias_str_index_of(tmp, ']');
	if (closeBracketPos <= 0)
	{
		g_free(machineName);
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
		g_free(machineName);
		free(encryptedDESKey);
		fprintf(stderr, "Couldn't get our private key.  Maybe iFolder is not running?\n");
		return FALSE;
	}

	err = simias_rsa_decrypt_string(privateKey, encryptedDESKey, &decryptedDESKey);
	free(privateKey);
	free(encryptedDESKey);
	if (err != 0)
	{
		g_free(machineName);
		fprintf(stderr, "simias_des_decrypt_string() returned an error (%d) in handle_invitation_accept()\n", err);
		return FALSE;
	}

	/* Save the user's DES key in blist.xml */
	sprintf(settingName, "simias-des-key:%s", machineName);
	gaim_blist_node_set_string(&(buddy->node),
							   settingName,
							   decryptedDESKey);
	free(decryptedDESKey);

	/**
	 * Now that we have the buddy's DES key, we should be able to read their
	 * encrypted profile.  Start the asynchronous call to read it.
	 */
	simias_get_buddy_profile(buddy);

	/* Send an invitation confirmation */
	err = simias_send_invitation_complete(buddy, machineName);
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
								_("%s (%s) has accepted your request to enable iFolder file sharing.  To share files through iFolder, use the iFolder Client, create an iFolder in the Gaim Workgroup Domain, and add %s to the iFolder's member list."),
								buddy_alias ? buddy_alias : sender,
								machineName,
								buddy_alias ? buddy_alias : sender,
								machineName);
	gtk_dialog_run(GTK_DIALOG(dialog));
	gtk_widget_destroy(dialog);
	
	g_free(machineName);

	return TRUE;
}

static gboolean
handle_invitation_complete(GaimAccount *account,
						   const char *sender,
						   const char *buffer)
{
	GaimBuddy *buddy;
	char *base64MachineName;
	char *machineName;
	int machineNameLen;
	char *encryptedDESKey;
	char *privateKey;
	char *decryptedDESKey;
	int colonPos;
	int closeBracketPos;
	char *tmp;
	char settingName[1024];
	GtkWidget *dialog;
	const char *buddy_alias = NULL;
	int err;

	fprintf(stderr, "handle_invitation_complete() %s -> %s entered\n",
		sender, gaim_account_get_username(account));
		
	buddy = gaim_find_buddy(account, sender);

	/**
	 * Start parsing the message at this point:
	 * 
	 *  [simias:invitation-complete:<Base64Encoded Machine Name>:<Base64Encoded DES key encrypted with the recipient's public key>]
	 *                              ^
	 */
	tmp = (char *)buffer + strlen(INVITATION_COMPLETE_MSG);
	colonPos = simias_str_index_of(tmp, ':');
	if (colonPos <= 0)
	{
		fprintf(stderr, "handle_invitation_complete() couldn't parse the base64 encoded machine name\n");
		return FALSE;
	}
	base64MachineName = malloc(sizeof(char) * (colonPos + 1));
	memset(base64MachineName, '\0', colonPos + 1);
	strncpy(base64MachineName, tmp, colonPos);

	gaim_base64_decode(base64MachineName, &machineName, &machineNameLen);
	free(base64MachineName);

	tmp = tmp + colonPos + 1;
	closeBracketPos = simias_str_index_of(tmp, ']');
	if (closeBracketPos <= 0)
	{
		g_free(machineName);
		fprintf(stderr, "handle_invitation_complete() couldn't parse the encrypted DES key\n");
		return FALSE;
	}
	encryptedDESKey = malloc(sizeof(char) * (closeBracketPos + 1));
	memset(encryptedDESKey, '\0', closeBracketPos + 1);
	strncpy(encryptedDESKey, tmp, closeBracketPos);

	/* Decrypt the DES key with our private key */
	err = simias_get_private_key(&privateKey);
	if (err != 0)
	{
		g_free(machineName);
		free(encryptedDESKey);
		fprintf(stderr, "Couldn't get our private key in handle_invitation_complete().  Maybe iFolder is not running?\n");
		return FALSE;
	}

	err = simias_rsa_decrypt_string(privateKey, encryptedDESKey, &decryptedDESKey);
	free(privateKey);
	free(encryptedDESKey);
	if (err != 0)
	{
		g_free(machineName);
		fprintf(stderr, "simias_des_decrypt_string() returned an error (%d) in handle_invitation_accept()\n", err);
		return FALSE;
	}

	/* Save the user's DES key in blist.xml */
	sprintf(settingName, "simias-des-key:%s", machineName);
	g_free(machineName);
	gaim_blist_node_set_string(&(buddy->node),
							   settingName,
							   decryptedDESKey);
	free(decryptedDESKey);

	/**
	 * Now that we have the buddy's DES key, we should be able to read their
	 * encrypted profile.  Start the asynchronous call to read it.
	 */
	simias_get_buddy_profile(buddy);
	
	return TRUE;
}
