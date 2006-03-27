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

//! iFolder API
/**
 *  @file ifolder.h
 *
 *  FIXME: Add detailed documentation for ifolder.h
 */

#ifndef _IFOLDER_IFOLDER_H
/* @cond */
#define _IFOLDER_IFOLDER_H 1
/* @endcond */

#include "account.h"
#include "user.h"
#include "enumeration.h"

/**
 * This object is used to represent an iFolder.  The functions
 * inside this file require an iFolder object.  When you are done
 * using an iFolder object, it should be freed with @a ifolder_release.
 * @see ifolder_release
 */
typedef void * iFolder;

/**
 * FIXME: Add iFolderMemberRights documentation
 */
typedef enum
{
	IFOLDER_RIGHTS_DENY,		/*< FIXME: Add IFOLDER_RIGHTS_DENY documentation */
	IFOLDER_RIGHTS_READ_ONLY,	/*< FIXME: Add IFOLDER_RIGHTS_READ_ONLY documentation */
	IFOLDER_RIGHTS_READ_WRITE,	/*< FIXME: Add IFOLDER_RIGHTS_READ_WRITE documentation */
	IFOLDER_RIGHTS_ADMIN		/*< FIXME: Add IFOLDER_RIGHTS_ADMIN documentation */
} iFolderMemberRights;

/**
 * Release/free the memory used by an @a iFolder object.
 *
 * Example:\n
 *     iFolder ifolder;\n
 *     if (ifolder_get_ifolder_by_name("Test iFolder", &ifolder) == IFOLDER_SUCCESS)\n
 *     {\n
 *         // Do something with the iFolder object\n
 *         ifolder_release(&ifolder);\n
 *     }\n
 *
 * @param ifolder A @a pointer to an @a iFolder object
 * to be released/freed.
 * @returns Returns IFOLDER_SUCESS if there were no errors.
 */
int ifolder_release(iFolder *ifolder);

/**
 * Creates a new iFolder on the specified account.
 *
 * @param account The iFolderAccount to create the new iFolder on.
 * @param local_path The local file system path which will be
 * converted to an iFolder and synchronized with the server.
 * @param description A description of the iFolder.  This can be
 * set to NULL to not specify a description.
 * @param ifolder If successful, this will be populated with an
 * iFolder object that represents the newly created iFolder.
 * @returns Returns IFOLDER_SUCCESS if successful.  The following
 * errors may also be returned: IFOLDER_SERVER_ERROR,
 * IFOLDER_SERVER_NOT_CONNECTED, IFOLDER_SERVER_UNAVAILABLE,
 * IFOLDER_SERVER_TIMEOUT, IFOLDER_INVALID_IFOLDER,
 * IFOLDER_ACCESS_DENIED.
 */
int ifolder_create_ifolder(iFolderAccount account, const char *local_path, const char *description, iFolder *ifolder);

/**
 * Delete an iFolder from the specified account.
 *
 * @param ifolder The iFolder to delete from the server.  If
 * successful, the iFolder object should not be used and should
 * be released immediately.
 * @returns Returns IFOLDER_SUCCESS if successful.  The following
 * errors may also be returned: IFOLDER_SERVER_ERROR,
 * IFOLDER_SERVER_NOT_CONNECTED, IFOLDER_SERVER_UNAVAILABLE,
 * IFOLDER_SERVER_TIMEOUT, IFOLDER_SERVER_BUSY,
 * IFOLDER_INVALID_IFOLDER, IFOLDER_ACCESS_DENIED.
 */
int ifolder_delete_ifolder(iFolder ifolder);

/**
 * Return all the iFolders on the specified account.
 *
 * @param account The iFolder Account to get a list of iFolders from
 * @param ifolder_enum If successful, this will contain an enumeration of
 * iFolder objects.  When finished with the enumeration,
 * ifolder_enumeration_release() should be called.
 * @returns An enumeration of iFolder objects.
 * @see ifolder_enumeration_release
 */
int ifolder_get_ifolders(iFolderAccount account, iFolderEnumeration *ifolder_enum);

/**
 * Get an iFolder object from the specified account.
 *
 * @param account The account where the iFolder is believed
 * to exist.
 * @param ifolder_id The ID of an iFolder.
 * @param ifolder If successful, this will be populated with an
 * iFolder object that represents the newly created iFolder.
 * @returns Returns IFOLDER_SUCCESS if successful.  The following
 * errors may also be returned: IFOLDER_SERVER_ERROR,
 * IFOLDER_SERVER_NOT_CONNECTED, IFOLDER_SERVER_UNAVAILABLE,
 * IFOLDER_SERVER_TIMEOUT, IFOLDER_INVALID_IFOLDER,
 * IFOLDER_ACCESS_DENIED.
 */
int ifolder_get_ifolder_by_id(iFolderAccount account, const char *ifolder_id, iFolder *ifolder);

int ifolder_get_ifolder_by_name(iFolderAccount account, const char *ifolder_name, iFolder *ifolder);
int ifolder_publish_ifolder(iFolder ifolder);
int ifolder_unpublish_ifolder(iFolder ifolder);

int ifolder_set_description(iFolder ifolder, const char *new_description);

/**
 * Get an enumeration of users that are members of the specified
 * @a ifolder.
 *
 * @param ifolder The iFolder to get the member list from.
 * @param index FIXME: Add documentation
 * @param count FIXME: Add documentation
 * @param user_enum If successful, this will contain an enumeration
 * of all the users who are members of the @a ifolder.  When you
 * are finished with the enumeration, you should call @a
 * ifolder_enumeration_release.
 * @returns Returns IFOLDER_SUCCESS if successful.
 */
int ifolder_get_members(iFolder ifolder, int index, int count, iFolderEnumeration *user_enum);

int ifolder_set_member_rights(iFolder ifolder, iFolderUser member, iFolderMemberRights rights);
int ifolder_add_member(iFolder ifolder, iFolderUser member, iFolderMemberRights rights);
int ifolder_remove_member(iFolder ifolder, iFolderUser member);
int ifolder_set_owner(iFolder ifolder, iFolderUser new_owner);

/**
 * Get all the changes for the specified iFolder.
 *
 * @param ifolder The iFolder to check for changes.
 * @param index The index of the first iFolderChangeEntry to return.
 * @param count The maximum number of iFolderChangeEntry objects
 * to return.
 * @param changes_enum An enumeration of iFolderChangeEntry objects.
 * @returns IFOLDER_SUCCESS if successful.
 */
int ifolder_get_changes(iFolder ifolder, int index, int count, iFolderEnumeration *changes_enum);

/**
 * Get the changes for a specific file inside of an iFolder.
 *
 * @param ifolder The iFolder to check for changes.
 * @param relative_path This represents the path of the file entry inside of
 * @param index The index of the first iFolderChangeEntry to return.
 * @param count The maximum number of iFolderChangeEntry objects
 * the iFolder.  It should not be an absolute file system path.
 * @param changes_enum An enumeration of iFolderChangeEntry objects.
 * @returns IFOLDER_SUCCESS if successful.
 */
int ifolder_get_file_changes(iFolder ifolder, const char *relative_path, int index, int count, iFolderEnumeration *changes_enum);

#endif
