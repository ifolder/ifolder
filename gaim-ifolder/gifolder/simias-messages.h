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

#ifndef _SIMIAS_MESSAGES_H
#define _SIMIAS_MESSAGES_H 1

#include <glib.h>

#include "account.h"
#include "blist.h"

#define PING_REQUEST_MSG		"[simias:ping-request:"
#define PING_RESPONSE_MSG		"[simias:ping-response:"

/****************************************************
 * Type Definitions                                 *
 ****************************************************/
typedef enum
{
	PING_REQUEST,
	PING_RESPONSE
} SIMIAS_MSG_TYPE;

/****************************************************
 * Functions                                        *
 ****************************************************/
int simias_send_msg(GaimBuddy *recipient, char *msg);

int simias_send_ping_req(GaimBuddy *recipient);

int simias_send_ping_resp(GaimBuddy *recipient);

gboolean simias_receiving_im_msg_cb(GaimAccount *account, char **sender,
									char **buffer, int *flags, void *data);

#endif
