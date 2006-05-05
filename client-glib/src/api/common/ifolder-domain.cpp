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

#include <stdio.h>
#include "../ifolder-client.h"

IFOLDER_API const char *ifolder_domain_get_id(const iFolderDomain domain)
{
	return NULL;
}

IFOLDER_API const char *ifolder_domain_get_name(const iFolderDomain domain)
{
	return NULL;
}

IFOLDER_API const char *ifolder_domain_get_description(const iFolderDomain domain)
{
	return NULL;
}

IFOLDER_API const char *ifolder_domain_get_version(const iFolderDomain domain)
{
	return NULL;
}

IFOLDER_API const char *ifolder_domain_get_host_address(const iFolderDomain domain)
{
	return NULL;
}

IFOLDER_API const char *ifolder_domain_get_machine_name(const iFolderDomain domain)
{
	return NULL;
}

IFOLDER_API const char *ifolder_domain_get_os_version(const iFolderDomain domain)
{
	return NULL;
}

IFOLDER_API const char *ifolder_domain_get_user_name(const iFolderDomain domain)
{
	return NULL;
}

IFOLDER_API bool ifolder_domain_get_is_default(const iFolderDomain domain)
{
	return false;
}

IFOLDER_API bool ifolder_domain_get_is_active(const iFolderDomain domain)
{
	return false;
}

IFOLDER_API void *ifolder_domain_get_user_data(const iFolderDomain domain)
{
	return NULL;
}

IFOLDER_API void ifolder_domain_set_user_data(const iFolderDomain domain, void *user_data)
{
}

IFOLDER_API int ifolder_domain_add(const char *host_address, const char *user_name, const char *password, const bool make_default, iFolderDomain *domain)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_remove(const iFolderDomain domain, const bool delete_ifolders_on_server)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_log_in(const iFolderDomain domain, const char *password)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_log_out(const iFolderDomain domain)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_activate(const iFolderDomain domain)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_inactivate(const iFolderDomain domain)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_change_host_address(const iFolderDomain domain, const char *new_host_address)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_set_credentials(const iFolderDomain domain, const char *password, const iFolderCredentialType credential_type)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_set_default(const iFolderDomain domain)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_get_authenticated_user(const iFolderDomain domain, iFolderUser *user)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_get_authenticated_user_policy(const iFolderDomain domain, iFolderUserPolicy *user_policy)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_check_for_updated_client(const iFolderDomain domain, char **new_version, const char *version_override)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API void ifolder_domain_free(iFolderDomain domain)
{
}

IFOLDER_API int ifolder_domain_get_all(iFolderEnumeration *domain_enum)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_get_all_active(iFolderEnumeration *domain_enum)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_get_default(iFolderDomain *domain)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_get_user(const iFolderDomain domain, const char *user_id, iFolderUser *user)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_get_users(const iFolderDomain domain, const int index, const int count, iFolderEnumeration *user_enum)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_get_users_by_search(const iFolderDomain domain, const iFolderSearchProperty search_prop, const iFolderSearchOperation search_op, const char *pattern, const int index, const int count, iFolderEnumeration *user_enum)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_create_ifolder_from_path(const iFolderDomain domain, const char *local_path, const char *description, iFolder *ifolder)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_create_ifolder(const iFolderDomain domain, const char *name, const char *description, iFolder *ifolder)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_delete_ifolder(const iFolderDomain domain, const iFolder ifolder)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_connect_ifolder(const iFolderDomain domain, iFolder ifolder, const char *local_path)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_disconnect_ifolder(const iFolderDomain domain, iFolder ifolder)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_get_all_ifolders(const iFolderDomain domain, const int index, const int count, iFolderEnumeration *ifolder_enum)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_get_connected_ifolders(const iFolderDomain domain, const int index, const int count, iFolderEnumeration *ifolder_enum)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_get_disconnected_ifolders(const iFolderDomain domain, const int index, const int count, iFolderEnumeration *ifolder_enum)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_get_ifolder_by_id(const iFolderDomain domain, const char *id, iFolder *ifolder)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_domain_get_ifolders_by_name(const iFolderDomain domain, const iFolderSearchOperation search_op, const char *pattern, const int index, const int count, iFolderEnumeration *ifolder_enum)
{
	return IFOLDER_UNIMPLEMENTED;
}
