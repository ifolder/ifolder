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

#include "blist.h"

#ifndef _SIMIAS_MESSAGES_H
#define _SIMIAS_MESSAGES_H 1

#define INVITATION_REQUEST_MSG	"[simias:invitation-request:"
#define INVITATION_DENY_MSG		"[simias:invitation-deny]"
#define INVITATION_ACCEPT_MSG	"[simias:invitation-accept:"
#define INVITATION_COMPLETE_MSG	"[simias:invitation-complete:"

/****************************************************
 * Type Definitions                                 *
 ****************************************************/
typedef enum
{
	UNKNOWN_MSG_TYPE,
	INVITATION_REQUEST,
	INVITATION_DENY,
	INVITATION_ACCEPT,
	INVITATION_COMPLETE
} SIMIAS_MSG_TYPE;

/****************************************************
 * Functions                                        *
 ****************************************************/
int simias_send_msg(GaimBuddy *recipient, char *msg);

/**
 * This function sends a message with the following format:
 *
 * [simias:invitation-request:<Base64Encoded Public Key>:<Base64Encoded Machine Name>]
 */
int simias_send_invitation_request(GaimBuddy *recipient);

/**
 * This function sends a message with the following format:
 *
 * [simias:invitation-deny]
 */
int simias_send_invitation_deny(GaimBuddy *recipient);

/**
 * This function sends a message with the following format:
 *
 * [simias:invitation-accept:<Base64Encoded Public Key>:<Base64Encoded Machine Name>:<Base64Encoded DES key encrypted with the recipient's public key>]
 */
int simias_send_invitation_accept(GaimBuddy *recipient, char *recipientMachineName);

/**
 * This function sends a message with the following format:
 *
 * [simias:invitation-complete:<Base64Encoded Machine Name>:<Base64Encoded DES key encrypted with the recipient's public key>]
 */
int simias_send_invitation_complete(GaimBuddy *recipient, char *recipientMachineName);

gboolean simias_receiving_im_msg_cb(GaimAccount *account, char **sender,
									char **buffer, int *flags, void *data);

#endif
