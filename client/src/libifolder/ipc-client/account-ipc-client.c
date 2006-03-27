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

#include "ifolder-client-ipc-client.h"

int
ipc_ifolder_accounts_get_all(iFolderEnumeration *account_enum)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
ipc_ifolder_accounts_get_all_active(iFolderEnumeration *account_enum)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
ipc_ifolder_accounts_get_default(iFolderAccount *default_account)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
ipc_ifolder_account_add(const char *host_address, const char *user_name, const char *password, bool make_default, iFolderAccount *account)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
ipc_ifolder_account_remove(iFolderAccount account, bool delete_ifolders_on_server)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
ipc_ifolder_account_login(iFolderAccount account, const char *password)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
ipc_ifolder_account_logout(iFolderAccount account)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
ipc_ifolder_account_activate(iFolderAccount account)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
ipc_ifolder_account_inactivate(iFolderAccount account)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
ipc_ifolder_account_change_host_address(iFolderAccount account, const char *new_host_address)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
ipc_ifolder_account_set_credentials(iFolderAccount account, const char *password, iFolderCredentialType credential_type)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
ipc_ifolder_account_set_default(iFolderAccount *new_default_account)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
ipc_ifolder_account_release(iFolderAccount *account)
{
	iFolderAccountObj *account_obj;
	if (account == NULL && *account == NULL)
		return IFOLDER_NULL_PARAMETER;

	account_obj = (iFolderAccountObj *) *account;

	account_obj->reference_count--;
	if (account_obj->reference_count > 0)
		return IFOLDER_SUCCESS;
	
	if (account_obj->id != NULL)
		free(account_obj->id);
	if (account_obj->name != NULL)
		free(account_obj->name);
	if (account_obj->description != NULL)
		free(account_obj->description);
	if (account_obj->version != NULL)
		free(account_obj->version);
	if (account_obj->host_address != NULL)
		free(account_obj->host_address);
	if (account_obj->machine_name != NULL)
		free(account_obj->machine_name);
	if (account_obj->os_version != NULL)
		free(account_obj->os_version);
	if (account_obj->user_name != NULL)
		free(account_obj->user_name);

	free(account_obj);
	
	/* Cleanup the caller's pointer so they aren't tempted to use it. */
	*account = NULL;

	return IFOLDER_SUCCESS;
}

int
ipc_ifolder_account_get_authenticated_user(iFolderAccount account, iFolderUser *user)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
ipc_ifolder_account_get_user(iFolderAccount account, const char *user_id, iFolderUser *ifolder_user)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
ipc_ifolder_account_get_users(iFolderAccount account, int index, int count, iFolderEnumeration *user_enum)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
ipc_ifolder_account_get_users_by_search(iFolderAccount account, iFolderSearchProperty search_prop, iFolderSearchOperation search_op, const char *pattern, int index, int count, iFolderEnumeration *user_enum)
{
	return IFOLDER_UNIMPLEMENTED;
}

const char *
ipc_ifolder_account_get_id(iFolderAccount account)
{
	return NULL;
}

const char *
ipc_ifolder_account_get_name(iFolderAccount account)
{
	return NULL;
}

const char *
ipc_ifolder_account_get_description(iFolderAccount account)
{
	return NULL;
}

const char *
ipc_ifolder_account_get_version(iFolderAccount account)
{
	return NULL;
}

const char *
ipc_ifolder_account_get_host_address(iFolderAccount account)
{
	return NULL;
}

const char *
ipc_ifolder_account_get_machine_name(iFolderAccount account)
{
	return NULL;
}

const char *
ipc_ifolder_account_get_os_version(iFolderAccount account)
{
	return NULL;
}

const char *
ipc_ifolder_account_get_user_name(iFolderAccount account)
{
	return NULL;
}

bool
ipc_ifolder_account_is_default(iFolderAccount account)
{
	return NULL;
}

bool
ipc_ifolder_account_is_active(iFolderAccount account)
{
	return NULL;
}

