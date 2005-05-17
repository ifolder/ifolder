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

#ifndef _SIMIAS_USERS_H
#define _SIMIAS_USERS_H 1

/**
 * The purpose of this file is to provide user management of the buddies that
 * should be "Enabled" as Simias/iFolder users.
 */

/****************************************************
 * Type Definitions                                 *
 ****************************************************/
typedef enum
{
	USER_INVITED,		/**
						 * We've sent the buddy a request to be enabled as a
						 * Simias user.
						 *
						 * Valid actions:
						 *		1. Resend the "Enable" request (in case the
						 *		   buddy somehow missed it).
						 *		2. Cancel the "Enable" request
						 */

	USER_ENABLED,		/**
						 * The buddy accepted the "Enable" request and is now a
						 * Simias user.  These types of users are loaded into
						 * the hashtable at startup by reading the settings
						 * out of blist.xml.
						 *
						 * Valid actions:
						 *		1. Disable the user as a Simias user (This
						 *		   removes all the Simias information from the
						 *		   user's blist.xml node and marks them back to
						 *		   an "Available" user).
						 */
						
	USER_REJECTED,		/**
						 * The buddy rejected the "Enable" request.
						 *
						 * Valid actions:
						 *		1. Resend the "Enable" request
						 */

	USER_AVAILABLE		/**
						 * The buddy has the plugin installed and enabled but
						 * we've never sent them any "Enable" requests.  This
						 * is the state also that the user is marked when we
						 * manually cancel an "Enable" request.
						 *
						 * Valid actions:
						 *		1. Resend the "Enable" request.
						 */
} SIMIAS_USER_STATE;

typedef struct
{
	char *hashKey;
	char *screenName;
	char *machineName;
	char *accountName;
	char *accountProto;
	SIMIAS_USER_STATE state;

	long timestamp;		/**
						 * This should be updated every time the "object" is
						 * modified.
						 */
} SimiasUser;

/**
 * Users Events
 */
typedef enum
{
	SIMIAS_USERS_EVENT_USER_ADDED,
	SIMIAS_USERS_EVENT_USER_REMOVED,
	SIMIAS_USERS_EVENT_USER_STATUS_CHANGED
} SIMIAS_USERS_EVENT_TYPE;

typedef void (*SimiasUsersEventFunc) (SIMIAS_USERS_EVENT_TYPE type, SimiasUser *simias_user);

/****************************************************
 * Functions                                        *
 ****************************************************/

/**
 * This function should be called once when the plugin loads.
 */
void simias_init_users();

/**
 * This function should be called once when the plugin unloads.
 */
void simias_cleanup_users();

GSList * simias_users_get_all();
SimiasUser * simias_users_get(const char *screenName, const char *machineName,
							  const char *accountName, const char *accountProto);
void simias_users_add(const char *screenName, const char *machineName,
					  const char *accountName, const char *accountProto,
					  SIMIAS_USER_STATE state);
void simias_users_remove(const char *screenName, const char *machineName,
						 const char *accountName, const char *accountProto);
void simias_users_update_status(const char *screenName, const char *machineName,
								const char *accountName, const char *accountProto,
								SIMIAS_USER_STATE state);

void simias_users_register_event_listener(SimiasUsersEventFunc listener_function);
void simias_users_deregister_event_listener(SimiasUsersEventFunc listener_function);

#endif
