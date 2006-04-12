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

iFolderType ifolder_get_type(const iFolder ifolder);
const char *ifolder_get_id(const iFolder ifolder);
const char *ifolder_get_name(const iFolder ifolder);
const char *ifolder_get_description(const iFolder ifolder);


int ifolder_set_description(const iFolder ifolder, const char *new_description)

int ifolder_get_state(iFolder ifolder, iFolderState *state);
int ifolder_get_items_to_synchronize(iFolder ifolder, int *items);

void ifolder_free(iFolder ifolder);

#ifdef __cplusplus
}
#endif		/* __cplusplus */

#endif /*_IFOLDER_C_H_*/
