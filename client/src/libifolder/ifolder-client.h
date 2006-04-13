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

#ifndef _IFOLDER_C_CLIENT_H_
#define _IFOLDER_C_CLIENT_H_

#include "ifolder-errors.h"

#ifdef __cplusplus
extern "C"
{
#endif		/* __cplusplus */

/**
 * @mainpage iFolder 3.6 Client API for C
 * 
 * @section intro_sec Introduction
 * 
 * @todo Add main page documentation for the C API
 * 
 * iFolder is a simple and secure storage solution that can increase your
 * productivity by enabling you to back up, access and manage your personal
 * files-from anywhere, at any time. Once you have installed iFolder, you
 * simply save your files locally-as you have always done-and iFolder
 * automatically updates the files on a network server and delivers them to the
 * other machines you use.
 * 
 * A wiki page is maintained for iFolder at http://www.ifolder.com/
 * 
 * @section install_sec Installation
 * 
 * @subsection step1 Step 1: Get the Source Code
 * 
 * Get the source code from http://www.ifolder.com
 * 
 * @subsection step2 Step 2: Build the Source Code
 * 
 */

/**
 * @file ifolder-client.h
 * @brief Main Client API (start here)
 * 
 * A process can only control only ONE instance of the iFolder Client.  When
 * your program loads, call ifolder_client_initialize().  Before you exit,
 * make sure you call ifolder_client_uninitialize() before your program exits.
 */

/**
 * @todo Do we need any more states than these?
 */
typedef enum
{
	IFOLDER_CLIENT_STATE_INITIALIZING,	/*!< The client is initializing */
	IFOLDER_CLIENT_STATE_STOPPED,		/*!< Synchronization has been stopped for the client */
	IFOLDER_CLIENT_STATE_IDLE,			/*!< The client is idle (in between synchronization cycles) */
	IFOLDER_CLIENT_STATE_PAUSED,			/*!< The client is paused in the middle of a synchronization process */
	IFOLDER_CLIENT_STATE_SYNCHRONIZING,	/*!< The client is actively synchronizing an iFolder */
	IFOLDER_CLIENT_STATE_UNINITIALIZING	/*!< The client is uninitializing */
} iFolderClientState;

/**
 * @name Main Client API
 */
/*@{*/

//! Initialize the iFolder Client.
/**
 * This must be called before using the iFolder Client API.
 * 
 * @param data_path (Optional) The local file system path that should be used
 * to store metadata, control files, etc. for the iFolder Client.  This allows
 * multiple instances of the iFolder Client to run on the same computer by the
 * same user.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_client_initialize(const char *data_path = NULL);

//! Uninitialize the iFolder Client
/**
 * This must be called when you are finished using the iFolder Client (usually
 * just before your process exits).
 * 
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_client_uninitialize(void);

//! Returns the current state of the iFolder Client.
/**
 * @return the current state of the iFolder Client.
 */
iFolderClientState ifolder_client_get_state(void);

/*@}*/

/**
 * @name Synchronization Control
 * 
 * @todo Review these functions with Calvin, Russ, and anyone else who should
 * give input on this.  Trent?
 * 
 * Control synchronization for the entire client by using one of these
 * functions.
 * 
 * To synchronize a single iFolder, use:
 * 
 * @li ifolder_sync_now()
 */
/*@{*/

//! Start synchronization for all iFolders.
/**
 * The main iFolder process should call this function when it first loads to
 * start a synchronization process.  If this method is subsequently called, it
 * will queue a complete synchronization of all iFolders.
 * 
 * If this function is called again before a full synchronization completes,
 * one of the following will be returned:
 * 
 * @li IFOLDER_ERR_FULL_SYNC_ALREADY_RUNNING
 * 
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_client_start_synchronization(void);

//! Stop all synchronization
/**
 * @todo Determine if a call to ifolder_start_sync(x, true) would be allowed
 * when the client is stopped.
 * 
 * Immediately stops all synchronization processes that are running.
 * 
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_client_stop_synchronization(void);

//! Resume synchronization after being paused
/**
 * This will resume synchronization without causing the iFolder that was
 * synchronizing to check for local changes again.  It also does not re-
 * synchronize iFolders that had already synchronized in the previous
 * synchronization cycle.
 * 
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_client_resume_synchronization(void);

/*@}*/

#ifdef __cplusplus
}
#endif		/* __cplusplus */

#endif /*_IFOLDER_C_CLIENT_H_*/
