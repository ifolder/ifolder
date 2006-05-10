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
#include "ifolder-change-entry.h"
#include "ifolder-user.h"

/**
 * @file ifolder.h
 * @brief iFolder API (API for individual iFolders)
 * 
 * @link ifolder_events_page iFolder Events @endlink
 * 
 * @section creating Creating New iFolders
 * 
 * @li ifolder_domain_create_ifolder_from_path()
 * @li ifolder_domain_create_ifolder()
 * 
 * @section existing_ifolders Accessing Existing iFolders
 * 
 * @li ifolder_domain_get_all_ifolders()
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

typedef enum
{
	IFOLDER_SYNC_DIRECTION_UPLOAD,			/*!< Uploading */
	IFOLDER_SYNC_DIRECTION_DOWNLOAD			/*!< Downloading */
} iFolderSyncDirection;

typedef enum
{
	IFOLDER_FILE_TYPE_FILE,					/*!< A file */
	IFOLDER_FILE_TYPE_DIRECTORY				/*!< A directory */
} iFolderFileType;

typedef enum
{
	IFOLDER_SYNC_FAILURE_UPDATE_CONFLICT,		/*!< Update conflict */
	IFOLDER_SYNC_FAILURE_FILE_NAME_CONFLICT,	/*!< File name conflict */
	IFOLDER_SYNC_FAILURE_POLICY,				/*!< Policy prevented synchronization */
	IFOLDER_SYNC_FAILURE_ACCESS,				/*!< Insufficient rights prevented synchronization */
	IFOLDER_SYNC_FAILURE_LOCKED,				/*!< Locked iFolder prevented synchronization */
	IFOLDER_SYNC_FAILURE_POLICY_QUOTA,		/*!< Full iFolder prevented synchronization */
	IFOLDER_SYNC_FAILURE_POLICY_SIZE,			/*!< File size restriction prevented synchronization */
	IFOLDER_SYNC_FAILURE_POLICY_TYPE,			/*!< File type restriction prevented synchronization */
	IFOLDER_SYNC_FAILURE_DISK_FULL,			/*!< Server/Client has insufficient disk space */
	IFOLDER_SYNC_FAILURE_READ_ONLY,			/*!< Read-only ifolder prevented synchronization */
	IFOLDER_SYNC_FAILURE_SERVER_BUSY,			/*!< Server is busy */
	IFOLDER_SYNC_FAILURE_CLIENT_ERROR,		/*!< Client sent bad data to the server that prevented synchronization */
	IFOLDER_SYNC_FAILURE_IN_USE,				/*!< A file is in use and could not be synchronized */
	IFOLDER_SYNC_FAILURE_SERVER_FAILURE,		/*!< Updating the metadata for a file failed */
	IFOLDER_SYNC_FAILURE_UNKNOWN				/*!< Unknown sync failure */
} iFolderSyncFailureType;

/**
 * @name Properties (Getters and Setters)
 * 
 * @{
 */

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
int ifolder_set_description(const iFolder ifolder, const char *new_description);

int ifolder_get_owner(const iFolder ifolder, iFolderUser *owner);
int ifolder_get_domain(const iFolder ifolder, iFolderDomain *domain);
int ifolder_get_size(const iFolder ifolder, long *size);

int ifolder_get_file_count(const iFolder ifolder, int *file_count);
int ifolder_get_directory_count(const iFolder ifolder, int *directory_count);

//! Returns the current user's member rights of an iFolder
/**
 * @param ifolder The iFolder.
 * @param rights Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_get_rights(const iFolder ifolder, iFolderMemberRights *rights);
int ifolder_get_last_modified(const iFolder ifolder, time_t *last_modified);

//! Returns true if an iFolder is published.
/**
 * @todo Add documentation here to specify what it means to have an iFolder be
 * published.
 * 
 * @param ifolder The iFolder.
 * @return true if the iFolder is published.
 */

bool ifolder_is_published(const iFolder ifolder);

//! Returns true if the iFolder is enabled.
/**
 * @todo Add documentation here to specify what it means to have an iFolder be
 * enabled.  Maybe this should be named "Locked" if it means
 * 
 * @param ifolder The iFolder.
 * @return true if the iFolder is enabled.
 */
bool ifolder_is_enabled(const iFolder ifolder);

//! Return the number of members an iFolder has.
/**
 * @param ifolder The iFolder.
 * @param member_count Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_get_member_count(const iFolder ifolder, int *member_count);

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
 * @param items_to_sync Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_get_items_to_synchronize(const iFolder ifolder, int *items_to_sync);

/*@}*/

/**
 * @name Synchronization Control
 * 
 * @todo Review these functions with Calvin and Russ.
 * 
 * @{
 */

//! Start synchronizing an iFolder.
/**
 * @param ifolder The iFolder.
 * @param sync_now Set to true if the synchronization of the iFolder should
 * begin immediately.  If false, the iFolder will be added to the standard
 * synchronization queue.  libifolder will only synchronize one iFolder at a
 * time.  If another iFolder is currently being synchronized, it will be
 * paused until this iFolder has synchronized.
 * 
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_start_synchronization(const iFolder ifolder, bool sync_now = false);

//! Stop synchronizing an iFolder.
/**
 * Immediately abort the synchronization process for the iFolder.  Other
 * iFolders on the synchronization queue will not be stopped.
 * 
 * @param ifolder The iFolder.
 * @return IFOLDER_SUCCESS if the call was successful.
 * 
 * @see ifolder_client_stop_synchronization()
 */
int ifolder_stop_synchronization(const iFolder ifolder);

//! Resume the synchronization of a paused iFolder.
/**
 * This can only be called on iFolders that are in the
 * #IFOLDER_STATE_SYNC_INCOMPLETE state.  libifolder will not recheck for new
 * files on the local machine to synchronize.  If you want to force a recheck
 * of files, you should call ifolder_start_sync().
 * 
 * @param ifolder The iFolder.
 * @param sync_now Set to true if the synchronization of the iFolder should
 * begin immediately.  If false, the iFolder will be added to the standard
 * synchronization queue.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_resume_synchronization(const iFolder ifolder, bool sync_now = false);

/*@}*/

/**
 * @name Member Management
 * 
 * @{
 */

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

//! Returns an iFolderChangeEntry enumeration for an iFolder.
/**
 * @param ifolder The iFolder.
 * @param index The index of where the change entry enumeration should begin.
 * This must be greater than or equal to 0.  An empty list will be returned if
 * the index is greater than the total number of change entries available.
 * @param count The maximum number of iFolderChangeEntry objects to return.
 * This must be at least 1.
 * @param change_entry_enum Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_get_change_entries(const iFolder ifolder, const int index, const int count, iFolderEnumeration *change_entry_enum);

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

//! Free the memory used by an iFolder.
/**
 * @param ifolder The iFolder.
 */
void ifolder_free(iFolder ifolder);

/*@}*/

/** @page ifolder_events_page iFolder Events

@events
 @event ifolder-connected
 @event ifolder-disconnected
 @event ifolder-created
 @event ifolder-deleted
 @event ifolder-state-changed
 @event ifolder-owner-changed
 @event ifolder-published
 @event ifolder-unpublished
 @event ifolder-member-added
 @event ifolder-member-removed
 @event ifolder-member-rights-modified
@endevents

<hr>

@eventdef ifolder-connected
 @eventproto
void (*ifolder_connected)(const iFolder ifolder);
 @endeventproto
 @eventdesc
  Emitted when an iFolder is connected to a local file system path.
 @param ifolder The iFolder that was connected.
@endeventdef

@eventdef ifolder-disconnected
 @eventproto
void (*ifolder_disconnected)(const iFolder ifolder);
 @endeventproto
 @eventdesc
  Emitted when an iFolder is disconnected from a local file system path.
 @param ifolder The iFolder that was disconnected.
@endeventdef

@eventdef ifolder-created
 @eventproto
void (*ifolder_created)(const iFolderDomain domain, const iFolder ifolder);
 @endeventproto
 @eventdesc
  Emitted when an iFolder is created on a server.
 @param domain The domain that the iFolder belongs to.
 @param ifolder The iFolder that was created.
@endeventdef

@eventdef ifolder-deleted
 @eventproto
void (*ifolder_deleted)(const iFolderDomain domain, const iFolder ifolder);
 @endeventproto
 @eventdesc
  Emitted when an iFolder is deleted from a server.
 @param domain The domain that the iFolder belongs to.
 @param ifolder The iFolder that was deleted.
@endeventdef

@eventdef ifolder-state-changed
 @eventproto
void (*ifolder_state_changed)(const iFolder ifolder, const iFolderState new_state);
 @endeventproto
 @eventdesc
  Emitted when the state of an iFolder changes.
 @param ifolder The iFolder whose state was changed.
 @param new_state The new state of the iFolder.
@endeventdef

@eventdef ifolder-owner-changed
 @eventproto
void (*ifolder_owner_changed)(const iFolder ifolder, const iFolderUser old_owner, const iFolderUser new_owner);
 @endeventproto
 @eventdesc
  Emitted when an iFolder's owner changes.
 @param ifolder The iFolder whose owner was changed.
 @param old_owner The old owner.
 @param new_owner The new owner.
@endeventdef

@eventdef ifolder-published
 @eventproto
void (*ifolder_published)(const iFolder ifolder);
 @endeventproto
 @eventdesc
  Emitted when an iFolder is published.
 @param ifolder The iFolder that was published.
@endeventdef

@eventdef ifolder-unpublished
 @eventproto
void (*ifolder_unpublished)(const iFolder ifolder);
 @endeventproto
 @eventdesc
  Emitted when an iFolder is unpublished.
 @param ifolder The iFolder that was unpublished.
@endeventdef

@eventdef ifolder-member-added
 @eventproto
void (*ifolder_member_added)(const iFolder ifolder, const iFolderUser member);
 @endeventproto
 @eventdesc
  Emitted when a new member is added to an iFolder.
 @param ifolder The iFolder.
 @param member The member that was added.
@endeventdef

@eventdef ifolder-member-removed
 @eventproto
void (*ifolder_member_removed)(const iFolder ifolder, const iFolderUser member);
 @endeventproto
 @eventdesc
  Emitted when a member is removed from an iFolder.
 @param ifolder The iFolder.
 @param member The member that was removed.
@endeventdef

@eventdef ifolder-member-rights-modified
 @eventproto
void (*ifolder_member_rights_modified)(const iFolder ifolder, const iFolderUser member, const iFolderMemberRights rights);
 @endeventproto
 @eventdesc
  Emitted when a member is removed from an iFolder.
 @param ifolder The iFolder.
 @param member The member whose rights were modified.
 @param rights The member's new rights.
@endeventdef

*/

#ifdef __cplusplus
}
#endif		/* __cplusplus */

#endif /*_IFOLDER_C_H_*/
