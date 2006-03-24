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

#ifndef _IFOLDER_CLIENT_IPC_CLIENT_H
#define _IFOLDER_CLIENT_IPC_CLIENT_H 1

#include "ifolder-client-private.h"

/* account.h */

int ipc_ifolder_accounts_get_all(iFolderEnumeration *account_enum);
int ipc_ifolder_accounts_get_all_active(iFolderEnumeration *account_enum);
int ipc_ifolder_accounts_get_default(iFolderAccount *default_account);
int ipc_ifolder_account_add(const char *host_address, const char *user_name, const char *password, bool make_default, iFolderAccount *account);
int ipc_ifolder_account_remove(iFolderAccount account, bool delete_ifolders_on_server);
int ipc_ifolder_account_login(iFolderAccount account, const char *password);
int ipc_ifolder_account_logout(iFolderAccount account);
int ipc_ifolder_account_activate(iFolderAccount account);
int ipc_ifolder_account_inactivate(iFolderAccount account);
int ipc_ifolder_account_change_host_address(iFolderAccount account, const char *new_host_address);
int ipc_ifolder_account_set_credentials(iFolderAccount account, const char *password, iFolderCredentialType credential_type);
int ipc_ifolder_account_set_default(iFolderAccount *new_default_account);
int ipc_ifolder_account_release(iFolderAccount *account);
int ipc_ifolder_account_get_authenticated_user(iFolderAccount account, iFolderUser *user);
int ipc_ifolder_account_get_user(iFolderAccount account, const char *user_id, iFolderUser *ifolder_user);
int ipc_ifolder_account_get_users(iFolderAccount account, int index, int count, iFolderEnumeration *user_enum);
int ipc_ifolder_account_get_users_by_search(iFolderAccount account, iFolderSearchProperty search_prop, iFolderSearchOperation search_op, const char *pattern, int index, int count, iFolderEnumeration *user_enum);
const char * ipc_ifolder_account_get_id(iFolderAccount account);
const char * ipc_ifolder_account_get_name(iFolderAccount account);
const char * ipc_ifolder_account_get_description(iFolderAccount account);
const char * ipc_ifolder_account_get_version(iFolderAccount account);
const char * ipc_ifolder_account_get_host_address(iFolderAccount account);
const char * ipc_ifolder_account_get_machine_name(iFolderAccount account);
const char * ipc_ifolder_account_get_os_version(iFolderAccount account);
const char * ipc_ifolder_account_get_user_name(iFolderAccount account);
bool ipc_ifolder_account_is_default(iFolderAccount account);
bool ipc_ifolder_account_is_active(iFolderAccount account);

#endif
