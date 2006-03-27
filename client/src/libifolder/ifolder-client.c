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

#include <stdlib.h>

#include "ifolder-client-private.h"
#include "ifolder-client-internal.h"
#include "ifolder-client-ipc-client.h"

static int initialize_as_trayapp(void);
static int uninitialize_trayapp(void);

static int initialize_as_ipc_client(void);
static int uninitialize_ipc_client(void);

/**
 * This variable is set when ifolder_client_initialize() is called.
 */
static iFolderClient *the_ifolder_client = NULL;

int
ifolder_client_initialize(bool is_tray_app)
{
	if (the_ifolder_client != NULL)
		return IFOLDER_ALREADY_INITIALIZED;
	
	if (is_tray_app)
		return initialize_as_trayapp();
	else
		return initialize_as_ipc_client();
}

int
ifolder_client_uninitialize()
{
	int err;

	if (the_ifolder_client == NULL)
		return IFOLDER_UNINITIALIZED;

	if (the_ifolder_client->is_tray_app)
		err = uninitialize_trayapp();
	else
		err = uninitialize_ipc_client();
	
	if (err != IFOLDER_SUCCESS)
		return err;

	/* FIXME: Protect the following with a mutex */
	free(the_ifolder_client);
	the_ifolder_client = NULL;	
	
	return IFOLDER_SUCCESS;
}


int
initialize_as_trayapp(void)
{
	/* FIXME: Protect this code with a mutex */
	the_ifolder_client = malloc(sizeof(iFolderClient));
	if (the_ifolder_client == NULL)
		return IFOLDER_OUT_OF_MEMORY;
	
	/* FIXME: Set up all of the_ifolder_client's variables/functions */
	the_ifolder_client->is_tray_app = true;
	
	/* account.h APIs */
	the_ifolder_client->ifolder_accounts_get_all				= internal_ifolder_accounts_get_all;
	the_ifolder_client->ifolder_accounts_get_default			= internal_ifolder_accounts_get_default;
	the_ifolder_client->ifolder_account_add						= internal_ifolder_account_add;
	the_ifolder_client->ifolder_account_remove					= internal_ifolder_account_remove;
	the_ifolder_client->ifolder_account_login					= internal_ifolder_account_login;
	the_ifolder_client->ifolder_account_logout					= internal_ifolder_account_logout;
	the_ifolder_client->ifolder_account_activate				= internal_ifolder_account_activate;
	the_ifolder_client->ifolder_account_inactivate				= internal_ifolder_account_inactivate;
	the_ifolder_client->ifolder_account_change_host_address		= internal_ifolder_account_change_host_address;
	the_ifolder_client->ifolder_account_set_default				= internal_ifolder_account_set_default;
	the_ifolder_client->ifolder_account_release					= internal_ifolder_account_release;
	the_ifolder_client->ifolder_account_get_authenticated_user	= internal_ifolder_account_get_authenticated_user;
	the_ifolder_client->ifolder_account_get_user				= internal_ifolder_account_get_user;
	the_ifolder_client->ifolder_account_get_users				= internal_ifolder_account_get_users;
	the_ifolder_client->ifolder_account_get_users_by_search		= internal_ifolder_account_get_users_by_search;
	the_ifolder_client->ifolder_account_get_id					= internal_ifolder_account_get_id;
	the_ifolder_client->ifolder_account_get_name				= internal_ifolder_account_get_name;
	the_ifolder_client->ifolder_account_get_description			= internal_ifolder_account_get_description;
	the_ifolder_client->ifolder_account_get_version				= internal_ifolder_account_get_version;
	the_ifolder_client->ifolder_account_get_host_address		= internal_ifolder_account_get_host_address;
	the_ifolder_client->ifolder_account_get_machine_name		= internal_ifolder_account_get_machine_name;
	the_ifolder_client->ifolder_account_get_os_version			= internal_ifolder_account_get_os_version;
	the_ifolder_client->ifolder_account_get_user_name			= internal_ifolder_account_get_user_name;
	the_ifolder_client->ifolder_account_is_default				= internal_ifolder_account_is_default;
	the_ifolder_client->ifolder_account_is_active				= internal_ifolder_account_is_active;
	
	/* FIXME: Start the services (event server, ipc server, etc.) */
	
	return IFOLDER_SUCCESS;
}

int
uninitialize_trayapp(void)
{
	/* FIXME: Shut down the services (event server, ipc server, etc.) */
	return IFOLDER_SUCCESS;
}

int
initialize_as_ipc_client(void)
{
	/* FIXME: Protect this code with a mutex */
	the_ifolder_client = malloc(sizeof(iFolderClient));
	if (the_ifolder_client == NULL)
		return IFOLDER_OUT_OF_MEMORY;

	/* FIXME: Set up all of the_ifolder_client's variables/functions */
	the_ifolder_client->is_tray_app = false;

	/* account.h APIs */
	the_ifolder_client->ifolder_accounts_get_all				= ipc_ifolder_accounts_get_all;
	the_ifolder_client->ifolder_accounts_get_default			= ipc_ifolder_accounts_get_default;
	the_ifolder_client->ifolder_account_add						= ipc_ifolder_account_add;
	the_ifolder_client->ifolder_account_remove					= ipc_ifolder_account_remove;
	the_ifolder_client->ifolder_account_login					= ipc_ifolder_account_login;
	the_ifolder_client->ifolder_account_logout					= ipc_ifolder_account_logout;
	the_ifolder_client->ifolder_account_activate				= ipc_ifolder_account_activate;
	the_ifolder_client->ifolder_account_inactivate				= ipc_ifolder_account_inactivate;
	the_ifolder_client->ifolder_account_change_host_address		= ipc_ifolder_account_change_host_address;
	the_ifolder_client->ifolder_account_set_default				= ipc_ifolder_account_set_default;
	the_ifolder_client->ifolder_account_release					= ipc_ifolder_account_release;
	the_ifolder_client->ifolder_account_get_authenticated_user	= ipc_ifolder_account_get_authenticated_user;
	the_ifolder_client->ifolder_account_get_user				= ipc_ifolder_account_get_user;
	the_ifolder_client->ifolder_account_get_users				= ipc_ifolder_account_get_users;
	the_ifolder_client->ifolder_account_get_users_by_search		= ipc_ifolder_account_get_users_by_search;
	the_ifolder_client->ifolder_account_get_id					= ipc_ifolder_account_get_id;
	the_ifolder_client->ifolder_account_get_name				= ipc_ifolder_account_get_name;
	the_ifolder_client->ifolder_account_get_description			= ipc_ifolder_account_get_description;
	the_ifolder_client->ifolder_account_get_version				= ipc_ifolder_account_get_version;
	the_ifolder_client->ifolder_account_get_host_address		= ipc_ifolder_account_get_host_address;
	the_ifolder_client->ifolder_account_get_machine_name		= ipc_ifolder_account_get_machine_name;
	the_ifolder_client->ifolder_account_get_os_version			= ipc_ifolder_account_get_os_version;
	the_ifolder_client->ifolder_account_get_user_name			= ipc_ifolder_account_get_user_name;
	the_ifolder_client->ifolder_account_is_default				= ipc_ifolder_account_is_default;
	the_ifolder_client->ifolder_account_is_active				= ipc_ifolder_account_is_active;

	/* FIXME: Start the services (register ipc client with ipc server, etc.) */
	
	return IFOLDER_SUCCESS;
}

int
uninitialize_ipc_client(void)
{
	/* FIXME: Shut down the services (ipc client, etc.) */
	return IFOLDER_SUCCESS;
}

iFolderClient *
ifolder_get_client(void)
{
	return the_ifolder_client;
}
