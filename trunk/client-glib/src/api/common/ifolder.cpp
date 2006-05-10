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

#include "../ifolder-client.h"

IFOLDER_API iFolderType
ifolder_get_type(const iFolder ifolder)
{
	return IFOLDER_TYPE_DISCONNECTED;
}

IFOLDER_API const char *ifolder_get_id(const iFolder ifolder)
{
	return NULL;
}

IFOLDER_API const char *ifolder_get_name(const iFolder ifolder)
{
	return NULL;
}

IFOLDER_API const char *ifolder_get_description(const iFolder ifolder)
{
	return NULL;
}

IFOLDER_API int ifolder_set_description(const iFolder ifolder, const char *new_description)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_get_owner(const iFolder ifolder, iFolderUser *owner)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_get_domain(const iFolder ifolder, iFolderDomain *domain)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_get_size(const iFolder ifolder, long *size)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_get_file_count(const iFolder ifolder, int *file_count)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_get_directory_count(const iFolder ifolder, int *directory_count)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_get_rights(const iFolder ifolder, iFolderMemberRights *rights)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_get_last_modified(const iFolder ifolder, time_t *last_modified)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API bool ifolder_is_published(const iFolder ifolder)
{
	return false;
}

IFOLDER_API bool ifolder_is_enabled(const iFolder ifolder)
{
	return false;
}

IFOLDER_API int ifolder_get_member_count(const iFolder ifolder, int *member_count)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_get_state(const iFolder ifolder, iFolderState *state)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_get_items_to_synchronize(const iFolder ifolder, int *items_to_sync)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_start_synchronization(const iFolder ifolder, bool sync_now)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_stop_synchronization(const iFolder ifolder)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_resume_synchronization(const iFolder ifolder, bool sync_now)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_get_members(const iFolder ifolder, const int index, const int count, iFolderEnumeration *user_enum)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_set_member_rights(const iFolder ifolder, const iFolderUser member, const iFolderMemberRights rights)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_add_member(const iFolder ifolder, const iFolderUser member, const iFolderMemberRights rights)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_remove_member(const iFolder ifolder, const iFolderUser member)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_set_owner(const iFolder ifolder, const iFolderUser member)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_get_change_entries(const iFolder ifolder, const int index, const int count, iFolderEnumeration *change_entry_enum)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_publish(const iFolder ifolder)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int ifolder_unpublish(const iFolder ifolder)
{
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API void ifolder_free(iFolder ifolder)
{
}

