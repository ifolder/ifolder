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

#include "buddy-profile.h"

/* Gaim iFolder Includes */
#include "gaim-domain.h"
#include "simias-messages.h"
#include "simias-util.h"

#include <glib.h>
#include <gtk/gtk.h>
#include <stdlib.h>
#include <string.h>

/* Gaim Includes */
#include "plugin.h"
#include "util.h"
#include "prpl.h"
#include "notify.h"
#include "gtknotify.h"
#include "blist.h"
#include "account.h"

/* Oscar Protocol Includes */
#include "aim.h"

static GHashTable *requested_userinfos = NULL;

/* Static Forward Declarations */
static GHashTable *get_my_hash_table();
static void add_buddy_to_store(GaimConnection *gc, const char *sn);
static gboolean expecting_buddy_userinfo(GaimConnection *gc, const char *sn);
static void remove_buddy_from_store(GaimConnection *gc, const char *sn);
static gboolean buddy_has_ifolder_plugin(const char *info_text);
/* Returned char * must be freed */
static char * get_buddy_munge(GaimConnection *gc, const char *sn);
static gboolean set_buddy_profile_on_timeout(gpointer data);

/**
 * Replaces the host with the one Gaim determines to be public.
 * The returned string must be freed.
 */
static char * convert_url_to_public(const char *start_url);

static char *get_my_profile_string(GaimAccount *account);

static gboolean parse_encoded_profile(GaimBuddy *buddy, const char *profile, char **machineName, char **userID, char **simiasURL);

/* Returned string must be freed */
void
simias_get_buddy_profile(GaimBuddy *buddy)
{
	GaimPlugin *oscar_plugin;
	GaimPluginProtocolInfo *prpl_info;
	GaimConnection *gc;
	const char *prpl_id;
	
	g_print("simias_get_buddy_profile() entered\n");

	/* Only do anything if this is an AOL (prpl-oscar) buddy */
	prpl_id = gaim_account_get_protocol_id(buddy->account);
	if (!prpl_id || strcmp(prpl_id, "prpl-oscar")) {
		return;
	}
	
	oscar_plugin = gaim_find_prpl("prpl-oscar");
	if (!oscar_plugin) {
		g_print("gaim_find_prpl(\"prpl-oscar\") returned NULL\n");
		return;
	}

	g_print("got oscar_plugin\n");
	
	prpl_info = GAIM_PLUGIN_PROTOCOL_INFO(oscar_plugin);
	if (!prpl_info) {
		g_print("GAIM_PLUGIN_PROTOCOL_INFO(oscar_plugin) returned NULL\n");
		return;
	}

	g_print("got prpl_info.\n");
	
	gc = gaim_account_get_connection(buddy->account);
	if (!gc) {
		g_print("gaim_account_get_connection(account) returned NULL\n");
		return;
	}
	
	/* Hijack the UI opts so we can read user profiles */
	gaim_notify_set_ui_ops(simias_notify_get_ui_ops());
	
	/* Add the buddy to our expecting list so that we can intercept the callback */
	add_buddy_to_store(gc, buddy->name);
	
	/* Inform the protocol to go fetch the user's profile */
	prpl_info->get_info(gc, buddy->name);
}

void
simias_set_account_profile_foreach(gpointer data, gpointer user_data)
{
	GaimAccount *account = (GaimAccount*)data;
	char *my_profile_string;
	if (account)
	{
		my_profile_string = get_my_profile_string(account);
		if (my_profile_string)
		{
			simias_set_buddy_profile(account, my_profile_string);
			free(my_profile_string);
		}
		else
		{
			fprintf(stderr, "Couldn't create my_profile_string.  You likely need to start iFolder and sign back into AIM.\n");
		}
	}
}

static gboolean
set_buddy_profile_on_timeout(gpointer data)
{
	GaimAccount *account = (GaimAccount*)data;
	char *my_profile_string;
	if (account)
	{
		my_profile_string = get_my_profile_string(account);
		if (my_profile_string)
		{
			simias_set_buddy_profile(account, my_profile_string);
			free(my_profile_string);
		}
		else
		{
			fprintf(stderr, "Couldn't create my_profile_string.  You likely need to start iFolder and sign back into AIM.\n");
		}
	}

	/* Tell g_timeout_add() to not call us again repeatedly */
	return FALSE;
}

void
simias_set_buddy_profile(GaimAccount *account, const char *profile_str)
{
	GaimConnection *gc;
	GaimConnectionState conn_state;
	GaimPlugin *oscar_plugin;
	GaimPluginProtocolInfo *prpl_info;
	const char *prpl_id;

	g_print("simias_set_buddy_profile() entered\n");
	
	/* Only do anything if this is an AOL (prpl-oscar) buddy */
	prpl_id = gaim_account_get_protocol_id(account);
	if (!prpl_id || strcmp(prpl_id, "prpl-oscar")) {
		g_print("returning because prpl_id = %s\n", prpl_id);
		return;
	}

	gc = gaim_account_get_connection(account);
	if (!gc) {
		g_print("gaim_account_get_connection(account) returned NULL\n");
		return;
	}
	
	/* Make sure we're connected */	
	conn_state = gaim_connection_get_state(gc);
	if (conn_state == GAIM_DISCONNECTED)
	{
		/* This account is offline, so don't do anything. */
		g_print("This account is offline, so the profile cannot be set.\n");
		return;
	}
	else if (conn_state == GAIM_CONNECTING)
	{
		/**
		 * Use g_timeout_add() to try setting the profile in a couple seconds
		 * when hopefully the connection will be connected.
		 */
		g_timeout_add(2000, set_buddy_profile_on_timeout, account);
	}
	else if (conn_state != GAIM_CONNECTED)
	{
		g_print("A connection for the account is in an unknown state, so we can't set the profile.\n");
		return;
	}
	
	oscar_plugin = gaim_find_prpl("prpl-oscar");
	if (!oscar_plugin) {
		g_print("gaim_find_prpl(\"prpl-oscar\") returned NULL\n");
		return;
	}

	g_print("got oscar_plugin\n");
	
	prpl_info = GAIM_PLUGIN_PROTOCOL_INFO(oscar_plugin);
	if (!prpl_info) {
		g_print("GAIM_PLUGIN_PROTOCOL_INFO(oscar_plugin) returned NULL\n");
		return;
	}

	g_print("got prpl_info.\n");
	
	g_print("About to call prpl_info->set_info()...\n");

	prpl_info->set_info(gc, profile_str);
	
	g_print("Called prpl_info->set_info()\n");
}

void
simias_account_connecting_cb(GaimAccount *account)
{
	g_print("account-connecting\n");
}

void
simias_account_setting_info_cb(GaimAccount *account, const char *new_info)
{
	g_print("account-setting-info: %s\n", new_info);
}

void
simias_account_set_info_cb(GaimAccount *account, const char *new_info)
{
	g_print("account-set-info: %s\n", new_info);
}

void
simias_connection_signing_on_cb(GaimConnection *gc)
{
	g_print("signing-on\n");
}

void
simias_connection_signed_on_cb(GaimConnection *gc)
{
	GaimAccount *account;
	char *my_profile_string;

	fprintf(stderr, "signed-on\n");
	
	account = gaim_connection_get_account(gc);
	if (!account) return;
	
	/* FIXME: Read the user's existing profile so we can add to it instead of just overwrite it */
	
	my_profile_string = get_my_profile_string(account);
	if (my_profile_string)
	{
		simias_set_buddy_profile(account, my_profile_string);
		free(my_profile_string);
	}
	else
	{
		fprintf(stderr, "Couldn't create my_profile_string.  You likely need to start iFolder and sign back into AIM.\n");
	}
}

static void *
simias_notify_message(GaimNotifyMsgType type, const char *title,
		      const char *primary, const char *secondary,
		      GCallback cb, void *user_data)
{
	/* Pass the function onto the Gtk handler */
	GaimNotifyUiOps *gtk_ui_ops = gaim_gtk_notify_get_ui_ops();
	return gtk_ui_ops->notify_message(type, title, primary, secondary, cb, user_data);
}

static void *
simias_notify_email(const char *subject, const char *from,
		    const char *to, const char *url,
		    GCallback cb, void *user_data)
{
	/* Pass the function onto the Gtk handler */
	GaimNotifyUiOps *gtk_ui_ops = gaim_gtk_notify_get_ui_ops();
	return gtk_ui_ops->notify_email(subject, from, to, url, cb, user_data);
}

static void *
simias_notify_emails(size_t count, gboolean detailed, const char **subjects,
		     const char **froms, const char **tos, const char **urls,
		     GCallback cb, void *user_data)
{
	/* Pass the function onto the Gtk handler */
	GaimNotifyUiOps *gtk_ui_ops = gaim_gtk_notify_get_ui_ops();
	return gtk_ui_ops->notify_emails(count, detailed, subjects, froms, tos, urls,
			   cb, user_data);
}

static void *
simias_notify_formatted(const char *title, const char *primary,
			const char *secondary, const char *text,
			GCallback cb, void *user_data)
{
	/* Pass the function onto the Gtk handler */
	GaimNotifyUiOps *gtk_ui_ops = gaim_gtk_notify_get_ui_ops();
	return gtk_ui_ops->notify_formatted(title, primary, secondary, text, cb, user_data);
}

static void *
simias_notify_userinfo(GaimConnection *gc, const char *who, const char *title,
			const char *primary, const char *secondary,
			const char *text, GCallback cb, void *user_data)
{
	GaimAccount *account;
	GaimBuddy *buddy;
	char *machineName;
	char *userID;
	char *simiasURL;
	char settingName[1024];
	
	/**
	 * If we're expecting this function to be called on a particular buddy,
	 * the buddy will be listed in our requested_userinfos store.
	 */
	if (expecting_buddy_userinfo(gc, who)) {
		/* Remove buddy from requested_userinfos store */
		remove_buddy_from_store(gc, who);

		if (!buddy_has_ifolder_plugin(text)) {
			return NULL;
		}

		account = gaim_connection_get_account(gc);
		buddy = gaim_find_buddy(account, who);
		
		if (parse_encoded_profile(buddy, text, &machineName, &userID, &simiasURL))
		{
			/* Update the blist.xml with the information */
			sprintf(settingName, "simias-user-id:%s", machineName);
			gaim_blist_node_set_string(&(buddy->node), settingName, userID);
			
			sprintf(settingName, "simias-url:%s", machineName);
			gaim_blist_node_set_string(&(buddy->node), settingName, simiasURL);

			free(machineName);
			free(userID);
			free(simiasURL);
		}
	} else {
		/* Pass this on to the default handler */
		GaimNotifyUiOps *gtk_ui_ops = gaim_gtk_notify_get_ui_ops();
		return gtk_ui_ops->notify_userinfo(gc, who, title, primary, secondary,
										   text, cb, user_data);
	}

	return NULL;
}

static void *
simias_notify_uri(const char *uri)
{
	/* Pass the function onto the Gtk handler */
	GaimNotifyUiOps *gtk_ui_ops = gaim_gtk_notify_get_ui_ops();
	return gtk_ui_ops->notify_uri(uri);
}

static void
simias_close_notify(GaimNotifyType type, void *ui_handle)
{
	/* Pass the function onto the Gtk handler */
	GaimNotifyUiOps *gtk_ui_ops = gaim_gtk_notify_get_ui_ops();
	gtk_ui_ops->close_notify(type, ui_handle);

	return;
}

static GaimNotifyUiOps ops =
{
	simias_notify_message,
	simias_notify_email,
	simias_notify_emails,
	simias_notify_formatted,
	simias_notify_userinfo,
	simias_notify_uri,
	simias_close_notify
};

GaimNotifyUiOps *
simias_notify_get_ui_ops() 
{
	return &ops;
}

/* Static Functions */

/* Static Forward Declarations */
static GHashTable *
get_my_hash_table()
{
	if (!requested_userinfos) {
		requested_userinfos = g_hash_table_new(g_str_hash, g_str_equal);
	}
	
	return requested_userinfos;
}

static void
add_buddy_to_store(GaimConnection *gc, const char *sn)
{
	char *munge = get_buddy_munge(gc, sn);
	
	/* Check to see if this buddy exists already in the hashtable */
	if (g_hash_table_lookup(get_my_hash_table(), munge)) {
		/* Do nothing */
		free(munge);
		return;
	}
	
	/* Buddy is NOT already in the hashtable */
	g_hash_table_insert(get_my_hash_table(), munge, munge);
}

static gboolean
expecting_buddy_userinfo(GaimConnection *gc, const char *sn)
{
	gboolean b_found_buddy = FALSE;
	char *munge = get_buddy_munge(gc, sn);
	
	if (g_hash_table_lookup(get_my_hash_table(), munge)) {
		b_found_buddy = TRUE;
	}

	free(munge);

	return b_found_buddy;
}

static void
remove_buddy_from_store(GaimConnection *gc, const char *sn)
{
	char *munge = get_buddy_munge(gc, sn);
	char *found_munge = NULL;

	found_munge = g_hash_table_lookup(get_my_hash_table(), munge);
	if (found_munge) {
		g_hash_table_remove(get_my_hash_table(), found_munge);
		free(found_munge);
	}

	free(munge);
}

static gboolean
buddy_has_ifolder_plugin(const char *info_text)
{
	if (strstr(info_text, SIMIAS_PLUGIN_INSTALLED_ID)) {
		return TRUE;
	}
	
	return FALSE;
}

/* Returned char * must be freed */
static char *
get_buddy_munge(GaimConnection *gc, const char *sn)
{
	char munge[1024];
	GaimAccount *account;
	const char *username;
	const char *prpl_id;
	
	account = gaim_connection_get_account(gc);
	username = gaim_account_get_username(account);
	prpl_id = gaim_account_get_protocol_id(account);
	
	sprintf(munge, "%s:%s:%s", username, prpl_id, sn);
	
	return strdup(munge);
}

/**
 * Replaces the host with the one Gaim determines to be public.
 * The returned string must be freed.
 */
static char *
convert_url_to_public(const char *start_url)
{
	char new_url[1024];
	const char *public_ip = NULL;
	char *proto = NULL;
	char *host = NULL;
	char *port = NULL;
	char *path = NULL;

	if (simias_url_parse(start_url, &proto, &host, &port, &path)) {
	
		public_ip = simias_get_public_ip();
		if (!public_ip)
		{
			fprintf(stderr, "Couldn't determin public IP address\n");
			return NULL;
		}

		if (path) {
			sprintf(new_url, "%s://%s:%s/%s", proto, public_ip, port, path);
		}
		
		if (proto) free(proto);
		if (host) free(host);
		if (port) free(port);
		if (path) free(path);
		
		return strdup(new_url);
	}
	
	return NULL;
}

/**
 * This should add the following string to the end of the existing user
 * profile:
 *
 * [simias:plugin-installed:<Base64 encoded machine name>:<Simias UserID and URL encoded with DES key>]
 */
static char *
get_my_profile_string(GaimAccount *account)
{
	char my_profile[4096];
	char *machineName;
	char *userID;
	char *simiasURL;
	char *escapedURL;
	char *publicURL;
	char *base64MachineName;
	char *desKey;
	char unencryptedString[2048];
	char *encryptedString;
	int err;
	
	/**
	 * FIXME: Figure out how to read the existing profile instead of just replacing the entire thing.
	 */

	err = simias_get_user_info(&machineName, &userID, &simiasURL);
	if (err != 0)
	{
		/* None of the returns are valid */
		fprintf(stderr, "simias_get_user_info() returned an error (%d) in get_my_profile_string().  Is iFolder/Simias running?\n", err);
		return NULL;
	}

	/* Escape any spaces in the URL */
	escapedURL = simias_escape_spaces(simiasURL);
	free(simiasURL);
	
	/* Convert simiasURL to a public URL */
	publicURL = convert_url_to_public(escapedURL);
	free(escapedURL);
	if (!publicURL)
	{
		free(machineName);
		free(userID);
		fprintf(stderr, "convert_url_to_public() returned NULL in get_my_profile_string()\n");
		return NULL;
	}

	err = simias_get_des_key(&desKey);
	if (err != 0)
	{
		free(machineName);
		free(userID);
		free(publicURL);
		fprintf(stderr, "simias_get_des_key() returned an error (%d) in get_my_profile_string().  Is iFolder/Simias running?\n", err);
		return NULL;
	}
	
	sprintf(unencryptedString, "%s:%s", userID, publicURL);
	free(userID);
	free(publicURL);
	err = simias_des_encrypt_string(desKey, unencryptedString, &encryptedString);
	free(desKey);
	if (err != 0)
	{
		free(machineName);
		fprintf(stderr, "simias_des_encrypt_string() return an error (%d) in get_my_profile_string().  Is iFolder/Simias running?\n", err);
		return NULL;
	}
	
	base64MachineName = gaim_base64_encode(machineName, strlen(machineName));
	free(machineName);

	sprintf(my_profile, "%s%s:%s]",
						SIMIAS_PLUGIN_INSTALLED_ID,
						base64MachineName,
						encryptedString);
	
	free(encryptedString);
	g_free(base64MachineName);
	
	return strdup(my_profile);
}

/**
 * The profile string should be in this format:
 *
 * [simias:<Base64Encoded Machine Name>:<String Encoded with DES Key>]
 */
static gboolean
parse_encoded_profile(GaimBuddy *buddy, const char *profile, char **machineName, char **userID, char **simiasURL)
{
	char *tmp;
	int colonPos;
	int closeBracketPos;
	char *base64MachineName;
	int machineNameLength;
	char *encryptedString;
	char *decryptedString;
	char settingName[1024];
	const char *desKey;
	int err;
	
	tmp = strstr(profile, SIMIAS_PLUGIN_INSTALLED_ID);
	if (!tmp) {
		fprintf(stderr, "parse_encoded_profile() called on a profile that doesn't contain \"%s\"\n", SIMIAS_PLUGIN_INSTALLED_ID);
		
		/**
		 * If this buddy is signing on from a new machine or disabled their
		 * iFolder plugin, remove the setting we've stored in our own blist.xml
		 * about this fact.
		 */
		gaim_blist_node_remove_setting(&(buddy->node), "simias-plugin-enabled");
		
		return FALSE;
	}
	
	/**
	 * Now start parsing here:
	 *
	 * [simias:plugin-installed:<Base64 encoded machine name>:<Simias UserID and URL encoded with DES key>]
	 *                          ^
	 */
	tmp = tmp + strlen(SIMIAS_PLUGIN_INSTALLED_ID);
	colonPos = simias_str_index_of(tmp, ':');
	if (colonPos <= 0)
	{
		fprintf(stderr, "parse_encoded_profile() couldn't parse the base64-encoded machine name\n");
		return FALSE;
	}
	
	base64MachineName = malloc(sizeof(char) * (colonPos + 1));
	memset(base64MachineName, '\0', colonPos + 1);
	strncpy(base64MachineName, tmp, colonPos);
	
	gaim_base64_decode(base64MachineName, machineName, &machineNameLength);
	free(base64MachineName);

	/* Use the machine name to mark this buddy with "simias-plugin-enabled" */
	gaim_blist_node_set_string(&(buddy->node), "simias-plugin-enabled", *machineName);

	sprintf(settingName, "simias-des-key:%s", *machineName);
	desKey = gaim_blist_node_get_string(&(buddy->node), settingName);
	if (!desKey)
	{
		/**
		 * We don't have the buddy's DES key so we cannot decode their
		 * encoded profile.
		 */
		free(*machineName);
		fprintf(stderr, "Do not have %s's DES key to decrypt their profile in parse_encoded_profile()\n", buddy->name);
		return FALSE;
	}
	
	/* Advance the parse position past the colon */
	tmp = tmp + colonPos + 1;
	closeBracketPos = simias_str_index_of(tmp, ']');
	if (closeBracketPos <= 0)
	{
		free(*machineName);
		fprintf(stderr, "parse_encoded_profile() couldn't parse the encrypted string\n");
		return FALSE;
	}
	
	encryptedString = malloc(sizeof(char) * (closeBracketPos + 1));
	memset(encryptedString, '\0', closeBracketPos + 1);
	strncpy(encryptedString, tmp, closeBracketPos);
	
	err = simias_des_decrypt_string(desKey, encryptedString, &decryptedString);
	free(encryptedString);
	if (err != 0)
	{
		free(*machineName);
		fprintf(stderr, "simias_des_decrypt_string() returned an error (%d) inside parse_encoded_profile()\n", err);
		return FALSE;
	}
	
	fprintf(stderr, "Decrypted profile string:\n\n%s\n\n", decryptedString);
	
	/* Now we can parse the userID and simiasURL */
	tmp = decryptedString;
	colonPos = simias_str_index_of(tmp, ':');
	if (colonPos <= 0)
	{
		free(*machineName);
		free(decryptedString);
		fprintf(stderr, "parse_encoded_profile() couldn't parse the Simias UserID\n");
		return FALSE;
	}
	
	*userID = malloc(sizeof(char) * (colonPos + 1));
	memset(*userID, '\0', colonPos + 1);
	strncpy(*userID, tmp, colonPos);

	/* Advance the parse position past the colon */
	tmp = tmp + colonPos + 1;
	*simiasURL = malloc(sizeof(char) * strlen(tmp) + 1);
	memset(*simiasURL, '\0', strlen(tmp) + 1);
	strncpy(*simiasURL, tmp, strlen(tmp));

	free(decryptedString);
	
	return TRUE;
}
