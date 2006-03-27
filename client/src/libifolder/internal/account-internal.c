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

#include <string.h>

#include "ifolder-client-internal.h"

/**
 * Create a new account and set properties to default.
 */
static iFolderAccountObj * internal_ifolder_account_new(void);

int
internal_ifolder_accounts_get_all(iFolderEnumeration *account_enum)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
internal_ifolder_accounts_get_all_active(iFolderEnumeration *account_enum)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
internal_ifolder_accounts_get_default(iFolderAccount *default_account)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
internal_ifolder_account_add(const char *host_address, const char *user_name, const char *password, bool make_default, iFolderAccount *account)
{
	iFolderAccountObj *account_obj;
	
	if (host_address == NULL || user_name == NULL || password == NULL || account == NULL)
		return IFOLDER_NULL_PARAMETER;
	
	/* FIXME: Add a real implementation of internal_ifolder_account_add. */
	
	account_obj = internal_ifolder_account_new();
	if (account_obj == NULL)
		return IFOLDER_OUT_OF_MEMORY;
	
	account_obj->id = strdup("FIXME: Get the real ID");
	account_obj->name = strdup("FIXME: Add a real name");
	account_obj->description = strdup("FIXME: Add a real description");
	account_obj->version = strdup("FIXME: Add real version");
	account_obj->host_address = strdup(host_address);
	account_obj->machine_name = strdup("FIXME: Add real machine name");
	account_obj->os_version = strdup("FIXME: Add real os version");
	account_obj->user_name = strdup(user_name);
	account_obj->is_default = make_default;
	account_obj->is_active = true;
	
	*account = (iFolderAccount) account_obj;
	
	return IFOLDER_SUCCESS;
}

int
internal_ifolder_account_remove(iFolderAccount account, bool delete_ifolders_on_server)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
internal_ifolder_account_login(iFolderAccount account, const char *password)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
internal_ifolder_account_logout(iFolderAccount account)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
internal_ifolder_account_activate(iFolderAccount account)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
internal_ifolder_account_inactivate(iFolderAccount account)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
internal_ifolder_account_change_host_address(iFolderAccount account, const char *new_host_address)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
internal_ifolder_account_set_credentials(iFolderAccount account, const char *password, iFolderCredentialType credential_type)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
internal_ifolder_account_set_default(iFolderAccount *new_default_account)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
internal_ifolder_account_release(iFolderAccount *account)
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
internal_ifolder_account_get_authenticated_user(iFolderAccount account, iFolderUser *user)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
internal_ifolder_account_get_user(iFolderAccount account, const char *user_id, iFolderUser *ifolder_user)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
internal_ifolder_account_get_users(iFolderAccount account, int index, int count, iFolderEnumeration *user_enum)
{
	return IFOLDER_UNIMPLEMENTED;
}

int
internal_ifolder_account_get_users_by_search(iFolderAccount account, iFolderSearchProperty search_prop, iFolderSearchOperation search_op, const char *pattern, int index, int count, iFolderEnumeration *user_enum)
{
	return IFOLDER_UNIMPLEMENTED;
}

const char *
internal_ifolder_account_get_id(iFolderAccount account)
{
	iFolderAccountObj *account_obj = (iFolderAccountObj *) account;
	return account_obj->id;
}

const char *
internal_ifolder_account_get_name(iFolderAccount account)
{
	iFolderAccountObj *account_obj = (iFolderAccountObj *) account;
	return account_obj->name;
}

const char *
internal_ifolder_account_get_description(iFolderAccount account)
{
	iFolderAccountObj *account_obj = (iFolderAccountObj *) account;
	return account_obj->description;
}

const char *
internal_ifolder_account_get_version(iFolderAccount account)
{
	iFolderAccountObj *account_obj = (iFolderAccountObj *) account;
	return account_obj->version;
}

const char *
internal_ifolder_account_get_host_address(iFolderAccount account)
{
	iFolderAccountObj *account_obj = (iFolderAccountObj *) account;
	return account_obj->host_address;
}

const char *
internal_ifolder_account_get_machine_name(iFolderAccount account)
{
	iFolderAccountObj *account_obj = (iFolderAccountObj *) account;
	return account_obj->machine_name;
}

const char *
internal_ifolder_account_get_os_version(iFolderAccount account)
{
	iFolderAccountObj *account_obj = (iFolderAccountObj *) account;
	return account_obj->os_version;
}

const char *
internal_ifolder_account_get_user_name(iFolderAccount account)
{
	iFolderAccountObj *account_obj = (iFolderAccountObj *) account;
	return account_obj->user_name;
}

bool
internal_ifolder_account_is_default(iFolderAccount account)
{
	iFolderAccountObj *account_obj = (iFolderAccountObj *) account;
	return account_obj->is_default;
}

bool
internal_ifolder_account_is_active(iFolderAccount account)
{
	iFolderAccountObj *account_obj = (iFolderAccountObj *) account;
	return account_obj->is_active;
}

/**
 * Create a new account and set properties to default.
 */
iFolderAccountObj *
internal_ifolder_account_new(void)
{
	iFolderAccountObj *account_obj;
	
	account_obj = malloc(sizeof(iFolderAccountObj));
	if (account_obj == NULL)
		return NULL;
	
	/* Set up defaults */
	account_obj->id					= NULL;
	account_obj->name				= NULL;
	account_obj->description		= NULL;
	account_obj->version			= NULL;
	account_obj->host_address		= NULL;
	account_obj->machine_name		= NULL;
	account_obj->os_version			= NULL;
	account_obj->user_name			= NULL;
	account_obj->is_default			= false;
	account_obj->is_active			= false;
	account_obj->reference_count	= 1;
	account_obj->user_data			= NULL;
	
	return account_obj;
}
