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

#ifndef _IFOLDER_ERRORS_H_
#define _IFOLDER_ERRORS_H_

#include <glib.h>

/**
 * @file ifolder-errors.h
 * @brief Error Codes
 */

/**
 * @name General Client Errors
 * @{
 */

typedef enum
{
	IFOLDER_SUCCESS,					/*!< An operation succeeded */
	IFOLDER_ERR_UNKNOWN,				/*!< If this is returned, the API implementors did not do a very good job of implementing the API.  Please refrain from using this. */
	IFOLDER_ERR_OUT_OF_MEMORY			/*!< Failed to allocate memory. */

} iFolderError;
#define IFOLDER_ERROR g_markup_error_quark()


typedef enum
{
	IFOLDER_ERR_ALREADY_INITIALIZED,	/*!< Returned if the client is attempted to be initialized multiple times. */
	IFOLDER_ERR_NOT_INITIALIZED,		/*!< Returned if an operation is attempted on the library without initializing the client. */
} iFolderClientError;
#define IFOLDER_CLIENT_ERROR g_markup_error_quark()


//! A required function parameter was invalid.
#define IFOLDER_ERR_INVALID_PARAMETER		-104

/*@}*/

/**
 * @name User Management Errors
 * @{
 */

//! The current user did not have sufficient rights to perform an operation.
#define IFOLDER_ERR_INSUFFICIENT_RIGHTS	-201

/*@}*/

/* IPC ERROR CODES */
/**
 * @name IPC Errors
 * 
 * libifolder uses IPC to allow multiple processes to communicate with the main
 * synchronization process (usually run inside of a TrayApp).  The following
 * errors may occur inside of the IPC mechanism.
 * 
 * @{
 */

//! Could not create a named pipe.
#define IFOLDER_ERR_IPC_CREATE			-301

//! Could not read from a named pipe.
#define IFOLDER_ERR_IPC_READ			-302

//! Could not write to a named pipe.
#define IFOLDER_ERR_IPC_WRITE			-302

//! @todo Determine what IFOLDER_ERR_IPC_INVALID means.
#define IFOLDER_ERR_IPC_INVALID			-303

//! The main iFolder Client/Process (IPC Server) is not running.
#define IFOLDER_ERR_IPC_NO_SERVER		-304

//! Could not close a named pipe.
#define IFOLDER_ERR_IPC_CLOSE			-305

//! An IPC message was formatted incorrectly.
#define IFOLDER_ERR_IPC_INVALID_MESSAGE	-306

//! An unknown message was read from a named pipe.
#define IFOLDER_ERR_IPC_UNKNOWN_MESSAGE	-307

//! An operation was attempted with the IPC mechanism not ready.
#define IFOLDER_ERR_IPC_INVALID_STATE	-308

/*@}*/

/**
 * @name Event Errors
 * @{
 */

//! An event operation was attempted on an event that has not been added into the event system.
#define IFOLDER_ERR_EVENT_DOES_NOT_EXIST	-401

//! Returned if you attempt to modify a built-in libifolder event.
#define IFOLDER_ERR_EVENT_INVALID_ACCESS	-402

/*@}*/

#endif /*_IFOLDER_ERRORS_H_*/

