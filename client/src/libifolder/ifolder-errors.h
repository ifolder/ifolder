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

#ifdef __cplusplus
extern "C"
{
#endif		/* __cplusplus */

/**
 * @file ifolder-errors.h
 * @brief Error Codes
 */

#define IFOLDER_SUCCESS	0
#define IFOLDER_ERROR		-1

#define IFOLDER_ERROR_ALREADY_INITIALIZED	-101
#define IFOLDER_ERROR_NOT_INITIALIZED		-102

#define IFOLDER_ERROR_OUT_OF_MEMORY	-201

/* IPC ERROR CODES */
#define IFOLDER_ERROR_IPC_CREATE			-301
#define IFOLDER_ERROR_IPC_READ			-302
#define IFOLDER_ERROR_IPC_WRITE			-302
#define IFOLDER_ERROR_IPC_INVALID			-303
#define IFOLDER_ERROR_IPC_NO_SERVER		-304
#define IFOLDER_ERROR_IPC_CLOSE			-305
#define IFOLDER_ERROR_IPC_INVALID_MESSAGE	-306
#define IFOLDER_ERROR_IPC_UNKNOWN_MESSAGE	-307
#define IFOLDER_ERROR_IPC_INVALID_STATE	-308

//! The current user did not have sufficient rights to perform an operation.
#define IFOLDER_ERR_INSUFFICIENT_RIGHTS		-401

#ifdef __cplusplus
}
#endif

#endif /*_IFOLDER_ERRORS_H_*/

