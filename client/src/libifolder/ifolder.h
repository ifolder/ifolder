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

#ifndef _IFOLDER_C_H_
#define _IFOLDER_C_H_

#ifdef __cplusplus
extern "C"
{
#endif		/* __cplusplus */

#include <time.h>

/**
 * @file ifolder.h
 * @brief iFolder API (API for individual iFolders)
 * 
 * @section creating Creating New iFolders
 * 
 * @li ifolder_domain_create_ifolder_from_path()
 * @li ifolder_domain_create_ifolder()
 * 
 * @section existing_ifolders Accessing Existing iFolders
 * 
 * @li ifolder_domain_get_connected_ifolders()
 * @li ifolder_domain_get_disconnected_ifolders()
 * @li ifolder_domain_get_ifolder_by_id()
 * 
 * @section connect_disconnect Connecting and Disconnecting iFolders
 * 
 * @li ifolder_domain_connect_ifolder()
 * @li ifolder_domain_disconnect_ifolder()
 * 
 * @section ifolder_types Types of iFolders
 * 
 * @subsection connected Connected iFolders
 * 
 * Connected iFolders (#IFOLDER_TYPE_CONNECTED) have been connected to a local
 * file system path for synchronization.  To disconnect an iFolder from a
 * local file system path (which will prevent any further synchronization to
 * this computer), call ifolder_domain_disconnect_ifolder().
 * 
 * @subsection disconnected Disconnected iFolders
 * 
 * Disconnected iFolders (#IFOLDER_TYPE_DISCONNECTED) have not been connected to
 * a local file system path and ONLY exist on the server.  To connect an
 * iFolder to a local file system path, call ifolder_domain_connect_ifolder().
 */

//! An object that represents a single iFolder.
/**
 * You must call ifolder_free() to clean up the memory used by an iFolder.
 */
typedef void *iFolder;

//! Defines the two types of iFolders
typedef enum
{
	IFOLDER_TYPE_CONNECTED,				/*!< An iFolder that is connected to a local file system path for synchronization */
	IFOLDER_TYPE_DISCONNECTED			/*!< An iFolder that exists on the server */
} iFolderType;

typedef enum
{
	IFOLDER_STATE_SYNC_WAIT,				/*!< The iFolder has never synchronized since the client has been started */
	IFOLDER_STATE_SYNC_PREPARE,			/*!< The iFolder is preparing to synchronize by checking for changes in the local file system */
	IFOLDER_STATE_SYNC_UPLOADING,		/*!< The iFolder is actively uploading files */
	IFOLDER_STATE_SYNC_DOWNLOADING,		/*!< The iFolder is actively downloading files */
	IFOLDER_STATE_SYNC_INCOMPLETE,		/*!< The synchronization process has paused */
	IFOLDER_STATE_SYNC_FAILED,			/*!< The iFolder failed to synchronize */
	IFOLDER_STATE_SYNC_SUCCESS,			/*!< The iFolder synchronized successfully */
	
	IFOLDER_STATE_DOMAIN_INACTIVE,		/*!< The iFolder's domain is marked as inactive */
	IFOLDER_STATE_DOMAIN_LOGGED_OUT,		/*!< The iFolder's domain is logged out */
	IFOLDER_STATE_DOMAIN_UNAVAILABLE,	/*!< The iFolder's domain is unavailable */
	IFOLDER_STATE_DOMAIN_BUSY,			/*!< The iFolder's domain is too busy to synchronize */
	
	IFOLDER_STATE_UNKNOWN				/*!< The state of the iFolder could not be determined */
} iFolderState;

/**
 * Users that are added as members of an iFolder are assigned one of these
 * rights.  The owner/creator of the iFolder is set up automatically with
 * #IFOLDER_MEMBER_RIGHTS_ADMIN.
 * 
 * @todo Make sure all the capabilities of #IFOLDER_MEMBER_RIGHTS_ADMIN are
 * documented.
 * @todo Find out what #IFOLDER_MEMBER_RIGHTS_DENY is even for.  Can a user be
 * added as a member of an iFolder with deny rights or should the user just be
 * deleted as a member of the iFolder?
 */
typedef enum
{
	IFOLDER_MEMBER_RIGHTS_DENY,			/*!< No access */
	IFOLDER_MEMBER_RIGHTS_READ_ONLY,		/*!< Read only access */
	IFOLDER_MEMBER_RIGHTS_READ_WRITE,	/*!< Read and write access */
	IFOLDER_MEMBER_RIGHTS_ADMIN			/*!< Administrator access (can add other users, change ownership, modify user rights) */
} iFolderMemberRights;

/**
 * @name Properties (Getters and Setters)
 */
/*@{*/

//! Returns the type of an iFolder
/**
 * @param ifolder The iFolder.
 * @return The iFolder type.
 */
iFolderType ifolder_get_type(const iFolder ifolder);
const char *ifolder_get_id(const iFolder ifolder);
const char *ifolder_get_name(const iFolder ifolder);
const char *ifolder_get_description(const iFolder ifolder);

//! Set the description of an iFolder
/**
 * @param ifolder The iFolder.
 * @param new_description The new description.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_set_description(const iFolder ifolder, const char *new_description)

int ifolder_get_owner(const iFolder ifolder, iFolderUser *owner);
int ifolder_get_domain(const iFolder ifolder, iFolderDomain *domain);
int ifolder_get_size(const iFolder ifolder, long *size);

//! Returns the current user's member rights of an iFolder
/**
 * @param ifolder The iFolder.
 * @param rights Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_get_rights(const iFolder ifolder, iFolderMemberRights *rights);
int ifolder_get_last_modified(const iFolder ifolder, time_t *last_modified);

//! Returns true if an iFolder is published
/**
 * @todo Add documentation here to specify what it means to have an iFolder be
 * published.
 * 
 * @param ifolder The iFolder.
 */

bool ifolder_is_published(const iFolder ifolder);

//! Return the current state of an iFolder.
/**
 * @param ifolder The iFolder.
 * @param state Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_get_state(const iFolder ifolder, iFolderState *state);

//! Return the number of items an iFolder has left to synchronize.
/**
 * @todo If an iFolder has never synchronized, we don't have a snapshot of the
 * number of files left to synchronize.  When this is the case, do we want to
 * return some sort of an error, like IFOLDER_ERR_NEVER_SYNCHRONIZED, which
 * would let the user know they've got to call a ifolder_check_local_changes()
 * or ifolder_sync_now().  Or, do we want to add a parameter on this function
 * "bool force_local_dredge" that will dredge the ifolder for changes if
 * needed?
 * 
 * @param ifolder The iFolder.
 * @param user Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_get_items_to_sync(const iFolder ifolder, int *items_to_sync);

/*@}*/

/**
 * @name Member Management
 */
/*@{*/

//! Returns a subset of members of an iFolder beginning at the specified index.
/**
 * @param ifolder The iFolder.
 * @param index The index of where the user enumeration should begin.  This
 * must be greater than 0.  An empty list will be returned if the index is
 * greater than the total number of users available.
 * @param count The number of iFolderUser objects to return.  This must be
 * at least 1 or greater.
 * @param user_enum Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_get_members(const iFolder ifolder, const int index, const int count, iFolderEnumeration *user_enum);

//! Set a member's rights to an iFolder.
/**
 * The current user can only call this function on an iFolder to which they are
 * an owner or administrator.
 * 
 * @param ifolder The iFolder.
 * @param member The member of the iFolder.
 * @param rights The rights to assign to the member.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_set_member_rights(const iFolder ifolder, const iFolderUser member, const iFolderMemberRights rights);

//! Add a new member to an iFolder.
/**
 * The current user can only call this function on an iFolder to which they are
 * an owner or administrator.
 * 
 * @param ifolder The iFolder.
 * @param member The member to add to the iFolder.
 * @param rights The rights to assign to the member.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_add_member(const iFolder ifolder, const iFolderUser member, const iFolderMemberRights rights);

//! Remove a member from an iFolder.
/**
 * The current user can only call this function on an iFolder to which they are
 * an owner or administrator.
 * 
 * @param ifolder The iFolder.
 * @param member The member to remove from the iFolder.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_remove_member(const iFolder ifolder, const iFolderUser member);

//! Set a new owner of an iFolder.
/**
 * @todo Document the conditions for being able to set a new owner of an
 * iFolder.  Can the owner choose ANY member as the new owner?  Can a member
 * with admin rights choose any member as the new owner?
 * 
 * @param ifolder The iFolder.
 * @param member The member to set as the new owner of the iFolder.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_set_owner(const iFolder ifolder, const iFolderUser member);

/*@}*/

/**
 * @name Other Functions
 */
/*@{*/

//! Publish an iFolder
/**
 * @param ifolder The iFolder.
 * @return IFOLDER_SUCCESS if the call was successful.
 * @see ifolder_is_published()
 */
int ifolder_publish(const iFolder ifolder);

//! Un-publish an iFolder
/**
 * @param ifolder The iFolder.
 * @return IFOLDER_SUCCESS if the call was successful.
 * @see ifolder_is_published()
 */
int ifolder_unpublish(const iFolder ifolder);

void ifolder_free(iFolder ifolder);

/*@}*/

#ifdef __cplusplus
}
#endif		/* __cplusplus */

#endif /*_IFOLDER_C_H_*/
