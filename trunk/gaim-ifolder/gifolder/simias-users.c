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

#include "internal.h"
#include "gtkgaim.h"

#include "blist.h"
#include "gtkblist.h"
#include "account.h"
#include <string.h>
#include <stdio.h>
#include <stdlib.h>
#include "gtkutils.h"
#include "notify.h"

#include "debug.h"
#include "signals.h"
#include "util.h"
#include "version.h"

#include "gtkplugin.h"

/* Gaim iFolder Plugin Includes */
#include "simias-users.h"
#include "simias-util.h"


/****************************************************
 * Static Definitions (#defines)                    *
 ****************************************************/

/****************************************************
 * Global Variables                                 *
 ****************************************************/

/**
 * Key: screenname+machineName+accountName+accountProto
 * Value: SimiasUser *
 */
static GHashTable *ht_simias_users = NULL;
static GSList *sl_simias_users = NULL;

static GSList *event_listeners = NULL;

/****************************************************
 * Forward Declarations                             *
 ****************************************************/
static void load_users_from_blist();
static void load_simias_user_cb(gpointer key, gpointer value, gpointer user_data);

static char * create_hash_key(const char *screenName, const char *machineName,
							  const char *accountName, const char *accountProto);

static void key_destroy_cb(gpointer user_data);
static void value_destroy_cb(gpointer user_data);

static void foreach_user_added_cb(gpointer data, gpointer user_data);
static void foreach_user_removed_cb(gpointer data, gpointer user_data);
static void foreach_user_state_changed_cb(gpointer data, gpointer user_data);

static gint simias_user_find_func(gconstpointer a, gconstpointer b);

/****************************************************
 * Function Implementations                         *
 ****************************************************/

void
simias_init_users()
{
	ht_simias_users = g_hash_table_new_full(g_str_hash, g_str_equal,
											key_destroy_cb, value_destroy_cb);

	load_users_from_blist();
}

void
simias_cleanup_users()
{
	g_slist_free(sl_simias_users);

	g_hash_table_destroy(ht_simias_users);
	
	g_slist_free(event_listeners);
}

GSList *
simias_users_get_all()
{
	return sl_simias_users;
}

SimiasUser *
simias_users_get(const char *screenName, const char *machineName,
				 const char *accountName, const char *accountProto)
{
	char *key;
	SimiasUser *simias_user;
	key = create_hash_key(screenName, machineName, accountName, accountProto);

	simias_user = (SimiasUser *) g_hash_table_lookup(ht_simias_users, key);

	free(key);

	return simias_user;
}

void
simias_users_add(const char *screenName, const char *machineName,
				 const char *accountName, const char *accountProto,
				 SIMIAS_USER_STATE state)
{
	SimiasUser *user;
	char *hashKey;
	
	hashKey = create_hash_key(screenName, machineName,
							  accountName, accountProto);

	/**
	 * Check to see if the user already exists and if they do,
	 * just update their status.
	 */
	user = (SimiasUser *) g_hash_table_lookup(ht_simias_users, hashKey);
	if (user)
	{
		free(hashKey);
		user->state = state;
		return;
	}
	
	user = malloc(sizeof(SimiasUser));
	user->hashKey		= hashKey;
	user->screenName	= strdup(screenName);
	user->machineName	= strdup(machineName);
	user->accountName	= strdup(accountName);
	user->accountProto	= strdup(accountProto);
	user->state = state;

	/* Add this new user into the hash table */
	g_hash_table_insert(ht_simias_users, hashKey, user);
	sl_simias_users = g_slist_append(sl_simias_users, user);

	/* Notify user event listeners of the addition */
	g_slist_foreach(event_listeners, foreach_user_added_cb, user);
}

void
simias_users_remove(const char *screenName, const char *machineName,
					const char *accountName, const char *accountProto)
{
	SimiasUser *user;
	char *hashKey;
	GSList *foundUser;
	
	hashKey = create_hash_key(screenName, machineName,
							  accountName, accountProto);

	user = (SimiasUser *) g_hash_table_lookup(ht_simias_users, hashKey);
	if (!user)
	{
		free(hashKey);
		return;
	}

	foundUser = g_slist_find_custom(sl_simias_users, NULL, simias_user_find_func);
	if (foundUser)
		sl_simias_users = g_slist_delete_link(sl_simias_users, foundUser);
	
	g_hash_table_remove(ht_simias_users, hashKey);

	free(hashKey);
}

void
simias_users_update_status(const char *screenName, const char *machineName,
						   const char *accountName, const char *accountProto,
						   SIMIAS_USER_STATE state)
{
	SimiasUser *user;
	char *hashKey;
	
	hashKey = create_hash_key(screenName, machineName,
							  accountName, accountProto);

	user = (SimiasUser *) g_hash_table_lookup(ht_simias_users, hashKey);
	if (user)
		user->state = state;

	free(hashKey);

	/* Notify user event listeners of the update */
	g_slist_foreach(event_listeners, foreach_user_state_changed_cb, user);
}

void
simias_users_register_event_listener(SimiasUsersEventFunc listener_function)
{
	event_listeners = g_slist_append(event_listeners, listener_function);
}

void
simias_users_deregister_event_listener(SimiasUsersEventFunc listener_function)
{
	event_listeners = g_slist_remove(event_listeners, listener_function);
}

/**
 * Walk through all the buddies we find in blist and add iFolder-enabled
 * users into the user list.
 */
static void
load_users_from_blist()
{
	GaimBuddyList *blist;

	blist = gaim_get_blist();
	
	g_hash_table_foreach(blist->buddies, load_simias_user_cb, NULL);
}

static void
load_simias_user_cb(gpointer key, gpointer value, gpointer user_data)
{
	GaimBuddy *buddy;
	GSList *machine_names_list;
	GSList *cur_machine_name;
	char settingName[1024];
	const char *account_username;
	const char *account_prpl_id;
	const char *desKey;

	buddy = (GaimBuddy *) value;
	
	g_print("load_simias_user_cb() entered: %s\n", buddy->name);

	/* Do nothing if the buddy is not on an AOL account */
	account_username = gaim_account_get_username(buddy->account);
	account_prpl_id = gaim_account_get_protocol_id(buddy->account);
	if (strcmp(account_prpl_id, "prpl-oscar") != 0)
	{
		g_print("returning because buddy is not on AOL account\n");
		return;
	}

	machine_names_list = simias_get_buddy_machine_names(&(buddy->node));
	if (!machine_names_list)
	{
		g_print("returning because we don't have any machine names stored for this buddy\n");
		return;	/* Nothing to do */
	}

	/**
	 * Go through each of the machine names and look to see if we've got
	 * the user's DES key.  If we do, that means they're a Simias-Enabled
	 * user and we can add them to the list.
	 */
	cur_machine_name = machine_names_list;
	while (cur_machine_name)
	{
		sprintf(settingName, "simias-des-key:%s", (char *) cur_machine_name->data);
		desKey = gaim_blist_node_get_string(&(buddy->node), settingName);
		if (desKey)
		{
			g_print("adding Simias-enabled buddy\n");
			simias_users_add(buddy->name, (const char *) cur_machine_name->data,
							 account_username, account_prpl_id,
							 USER_ENABLED);
		}
		else
		{
			/* This buddy has the plugin but is not "Enabled" */
			g_print("adding available buddy\n");
			simias_users_add(buddy->name, (const char *) cur_machine_name->data,
							 account_username, account_prpl_id,
							 USER_AVAILABLE);
		}
	
		cur_machine_name = cur_machine_name->next;
	}
	
	simias_free_machine_names_list(machine_names_list);
}

/**
 * Returns a string consisting of:
 *
 *		screenName+machineName+accountName+accountProto
 */
static char *
create_hash_key(const char *screenName, const char *machineName,
				const char *accountName, const char *accountProto)
{
	char *key;
	int len;
	
	len = strlen(screenName) +
		  strlen(machineName) +
		  strlen(accountName) +
		  strlen(accountProto) +
		  4; /* 3 '+' chars and one trailing '\0' */
	
	key = malloc(sizeof(char) * len);
	
	sprintf(key, "%s+%s+%s+%s",
			screenName,
			machineName,
			accountName,
			accountProto);
	
	return key;
}

static void
key_destroy_cb(gpointer user_data)
{
	char *key;
	
	key = (char *) user_data;
	
	free(key);
}

static void
value_destroy_cb(gpointer user_data)
{
	SimiasUser *simias_user;
	
	simias_user = (SimiasUser *) user_data;

	/* Let the listeners know about the user being removed */
	g_slist_foreach(event_listeners, foreach_user_removed_cb, simias_user);

	free(simias_user);
}

static void
foreach_user_added_cb(gpointer data, gpointer user_data)
{
	SimiasUsersEventFunc listener_func;
	SimiasUser *simias_user;
	
	listener_func = (SimiasUsersEventFunc) data;
	simias_user = (SimiasUser *) user_data;

	listener_func(SIMIAS_USERS_EVENT_USER_ADDED, simias_user);
}

static void
foreach_user_removed_cb(gpointer data, gpointer user_data)
{
	SimiasUsersEventFunc listener_func;
	SimiasUser *simias_user;
	
	listener_func = (SimiasUsersEventFunc) data;
	simias_user = (SimiasUser *) user_data;

	listener_func(SIMIAS_USERS_EVENT_USER_REMOVED, simias_user);
}

static void
foreach_user_state_changed_cb(gpointer data, gpointer user_data)
{
	SimiasUsersEventFunc listener_func;
	SimiasUser *simias_user;
	
	listener_func = (SimiasUsersEventFunc) data;
	simias_user = (SimiasUser *) user_data;

	listener_func(SIMIAS_USERS_EVENT_USER_STATUS_CHANGED, simias_user);
}

static gint
simias_user_find_func(gconstpointer a, gconstpointer b)
{
	SimiasUser *u1;
	SimiasUser *u2;
	
	u1 = (SimiasUser *) a;
	u2 = (SimiasUser *) b;
	
	return strcmp(u1->hashKey, u2->hashKey);
}
