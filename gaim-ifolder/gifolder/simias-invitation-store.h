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

#ifndef _SIMIAS_INVITATION_STORE_H
#define _SIMIAS_INVITATION_STORE_H 1

#include <glib.h>
#include <gtk/gtk.h>

/* Gaim Includes */
#include "blist.h"

#define FILENAME_IN_INVITATIONS "simias-in-invitations.xml"
#define FILENAME_OUT_INVITATIONS "simias-out-invitations.xml"
#define FILENAME_TRUSTED_BUDDIES "simias-trusted-buddies.xml"

#define COLLECTION_TYPE_IFOLDER "ifolder"
#define COLLECTION_TYPE_GLYPHMARKS "glyphmarks"

/* Gaim iFolder Includes */

/****************************************************
 * Enumerations                                     *
 ****************************************************/
enum
{
	INVITATION_TYPE_ICON_COL,
	BUDDY_NAME_COL,
	TIME_COL,
	COLLECTION_NAME_COL,
	STATE_COL,
	INVITATION_PTR,
	N_COLS
};

enum
{
	TRUSTED_BUDDY_ICON_COL,
	TRUSTED_BUDDY_NAME_COL,
	TRUSTED_BUDDY_IP_ADDR_COL,
	TRUSTED_BUDDY_IP_PORT_COL,
	GAIM_ACCOUNT_PTR_COL,
	N_TRUSTED_BUDDY_COLS
};

/**
 * The INVITATION_STATE Enumeration:
 * 
 * STATE_NEW: This state is used for incoming invitations and denotes that the
 * user has not accepted or denied the invitation.
 * 
 * STATE_PENDING: The invitation has been added by Simias but not sent yet.  If
 * an invitation stays in this state for a while it's likely that the buddy is
 * not online.  When a "buddy-sign-on" event occurs for this buddy, the
 * invitation will be sent at that point.
 *
 * STATE_SENT: The invitation has been sent to the buddy but they haven't
 * replied yet.
 * 
 * STATE_REJECTED_PENDING: The user rejected an incoming invitation, but the
 * buddy wasn't online to actually send the reply message so it's pending and
 * will be sent as soon as the buddy is online.
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
 * 
 * States used for incoming invitations:
 * 
 * 		STATE_NEW, STATE_REJECTED_PENDING, STATE_ACCEPTED_PENDING
 * 
 * States used for outgoing invitations:
 * 
 * 		STATE_PENDING, STATE_SENT, STATE_REJECTED, STATE_ACCEPTED_PENDING,
 * 		STATE_ACCEPTED
 */
typedef enum
{
	STATE_NEW,
	STATE_PENDING,
	STATE_SENT,
	STATE_REJECTED_PENDING,
	STATE_REJECTED,
	STATE_ACCEPTED_PENDING,
	STATE_ACCEPTED
} INVITATION_STATE;

/****************************************************
 * Data Structures                                  *
 ****************************************************/
typedef struct
{
	GaimAccount *gaim_account;
	char buddy_name[128];
	INVITATION_STATE state;
	time_t time;
	char collection_id[64];
	char collection_type[32];
	char collection_name[128];
	char ip_addr[16];
	char ip_port[16];
} Invitation;


void simias_add_invitation_to_store(GtkListStore *store,
									Invitation *invitation);
void simias_init_invitation_stores();

void simias_add_new_trusted_buddy(GtkListStore *store, GaimBuddy *buddy,
									char *ip_address, char *ip_port);
void simias_init_trusted_buddies_store();

void simias_save_trusted_buddies(GtkListStore *store);

void simias_save_invitations(GtkListStore *store);

gboolean simias_lookup_collection_in_store(GtkListStore *store,
										   char *collection_id,
										   GtkTreeIter *iter);

gboolean simias_lookup_trusted_buddy(GtkListStore *store,
									 GaimBuddy *buddy,
									 GtkTreeIter *iter);

#endif
