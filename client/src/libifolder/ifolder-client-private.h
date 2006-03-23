/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2006 Novell, Inc.
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
 *  Author(s): Boyd Timothy <btimothy@novell.com>
 *
 ***********************************************************************/

#ifndef _IFOLDER_CLIENT_PRIVATE_H
#define _IFOLDER_CLIENT_PRIVATE_H 1

#include <stdlib.h>

#include "ifolder-client.h"

typedef struct _iFolderClient iFolderClient;

struct _iFolderClient
{
	bool is_tray_app;
	
	/* account.h */
	int (*ifolder_account_new)(const char *server_address, iFolderAccount *account);
	int (*ifolder_account_release)(iFolderAccount *account);
};

#endif
