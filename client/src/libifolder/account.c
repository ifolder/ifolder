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
#include <stdio.h>

int
ifolder_accounts_get_all(iFolderEnumeration *account_enum)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return IFOLDER_UNINITIALIZED;

	return ifolder_client->ifolder_accounts_get_all(account_enum);
}

int
ifolder_accounts_get_all_active(iFolderEnumeration *account_enum)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return IFOLDER_UNINITIALIZED;

	return ifolder_client->ifolder_accounts_get_all_active(account_enum);
}

int
ifolder_accounts_get_default(iFolderAccount *default_account)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return IFOLDER_UNINITIALIZED;

	return ifolder_client->ifolder_accounts_get_default(default_account);
}

int
ifolder_account_add(const char *host_address,
					const char *user_name,
					const char *password,
					bool make_default,
					iFolderAccount *account)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return IFOLDER_UNINITIALIZED;

	printf("ifolder_account_add()\n");

	return ifolder_client->ifolder_account_add(
				host_address,
				user_name,
				password,
				make_default,
				account);
}

int
ifolder_account_remove(iFolderAccount account,
					   bool delete_ifolders_on_server)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return IFOLDER_UNINITIALIZED;

	return ifolder_client->ifolder_account_remove(account, delete_ifolders_on_server);
}

int
ifolder_account_login(iFolderAccount account,
					  const char *password)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return IFOLDER_UNINITIALIZED;

	return ifolder_client->ifolder_account_login(account, password);
}

int
ifolder_account_logout(iFolderAccount account)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return IFOLDER_UNINITIALIZED;

	return ifolder_client->ifolder_account_logout(account);
}

int
ifolder_account_activate(iFolderAccount account)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return IFOLDER_UNINITIALIZED;

	return ifolder_client->ifolder_account_activate(account);
}

int
ifolder_account_inactivate(iFolderAccount account)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return IFOLDER_UNINITIALIZED;

	return ifolder_client->ifolder_account_inactivate(account);
}

int
ifolder_account_change_host_address(iFolderAccount account,
									const char *new_host_address)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return IFOLDER_UNINITIALIZED;

	return ifolder_client->ifolder_account_change_host_address(account, new_host_address);
}

int
ifolder_account_set_credentials(iFolderAccount account,
								const char *password,
								iFolderCredentialType credential_type)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return IFOLDER_UNINITIALIZED;

	return ifolder_client->ifolder_account_set_credentials(account, password, credential_type);
}

int ifolder_account_set_default(iFolderAccount *new_default_account)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return IFOLDER_UNINITIALIZED;

	return ifolder_client->ifolder_account_set_default(new_default_account);
}

int
ifolder_account_release(iFolderAccount *account)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return IFOLDER_UNINITIALIZED;

	return ifolder_client->ifolder_account_release(account);
}

int
ifolder_account_get_authenticated_user(iFolderAccount account, iFolderUser *user)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return IFOLDER_UNINITIALIZED;

	return ifolder_client->ifolder_account_get_authenticated_user(account, user);
}

int
ifolder_account_get_user(iFolderAccount account, const char *user_id, iFolderUser *ifolder_user)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return IFOLDER_UNINITIALIZED;

	return ifolder_client->ifolder_account_get_user(account, user_id, ifolder_user);
}

int
ifolder_account_get_users(iFolderAccount account, int index, int count, iFolderEnumeration *user_enum)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return IFOLDER_UNINITIALIZED;

	return ifolder_client->ifolder_account_get_users(account, index, count, user_enum);
}

int
ifolder_account_get_users_by_search(iFolderAccount account,
										iFolderSearchProperty search_prop,
										iFolderSearchOperation search_op,
										const char *pattern,
										int index,
										int count,
										iFolderEnumeration *user_enum)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return IFOLDER_UNINITIALIZED;

	return ifolder_client->ifolder_account_get_users_by_search(
				account,
				search_prop,
				search_op,
				pattern,
				index,
				count,
				user_enum);
}

const char *
ifolder_account_get_id(iFolderAccount account)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return NULL;

	return ifolder_client->ifolder_account_get_id(account);
}

const char *
ifolder_account_get_name(iFolderAccount account)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return NULL;

	return ifolder_client->ifolder_account_get_name(account);
}

const char *
ifolder_account_get_description(iFolderAccount account)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return NULL;

	return ifolder_client->ifolder_account_get_description(account);
}

const char *
ifolder_account_get_version(iFolderAccount account)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return NULL;

	return ifolder_client->ifolder_account_get_version(account);
}

const char *
ifolder_account_get_host_address(iFolderAccount account)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return NULL;

	return ifolder_client->ifolder_account_get_host_address(account);
}

const char *
ifolder_account_get_machine_name(iFolderAccount account)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return NULL;

	return ifolder_client->ifolder_account_get_machine_name(account);
}

const char *
ifolder_account_get_os_version(iFolderAccount account)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return NULL;

	return ifolder_client->ifolder_account_get_os_version(account);
}

const char *
ifolder_account_get_user_name(iFolderAccount account)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return NULL;

	return ifolder_client->ifolder_account_get_user_name(account);
}

bool
ifolder_account_is_default(iFolderAccount account)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return IFOLDER_UNINITIALIZED;

	return ifolder_client->ifolder_account_is_default(account);
}

bool
ifolder_account_is_active(iFolderAccount account)
{
	iFolderClient *ifolder_client = ifolder_get_client();
	if (ifolder_client == NULL)
		return IFOLDER_UNINITIALIZED;

	return ifolder_client->ifolder_account_is_active(account);
}

void *
ifolder_account_get_user_data(iFolderAccount account)
{
	iFolderAccountObj *account_obj;
	account_obj = (iFolderAccountObj *) account;

	return account_obj->user_data;
}

void
ifolder_account_set_user_data(iFolderAccount account, void *user_data)
{
	iFolderAccountObj *account_obj;
	account_obj = (iFolderAccountObj *) account;
	
	account_obj->user_data = user_data;
}
