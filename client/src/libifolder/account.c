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

#include "ifolder-client-private.h"

extern iFolderClient *the_ifolder_client;

int
ifolder_account_new(const char *server_address, iFolderAccount *account)
{
	/* FIXME: Implement a RETURN_IF_UNINITIALIZED()/CHECK_IFOLDER_CLIENT macro */
	if (the_ifolder_client == NULL)
		return IFOLDER_UNINITIALIZED;

	return the_ifolder_client->ifolder_account_new(server_address, account);
}

int
ifolder_account_release(iFolderAccount *account)
{
	/* FIXME: Implement a RETURN_IF_UNINITIALIZED()/CHECK_IFOLDER_CLIENT macro */
	if (the_ifolder_client == NULL)
		return IFOLDER_UNINITIALIZED;

	return the_ifolder_client->ifolder_account_release(account);
}
