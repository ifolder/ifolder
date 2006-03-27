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

//! iFolder Errors
/**
 *  @file errors.h
 *
 *  FIXME: Add documentation for errors.h
 */

#ifndef _IFOLDER_CLIENT_ERRORS_H
/* @cond */
#define _IFOLDER_CLIENT_ERRORS_H 1
/* @endcond */

/* FIXME: Before we release this code, need to organize the errors in errors.h in a logical fashion. */

//! Indicates a function succeeded.
#define IFOLDER_SUCCESS					0

//! Indicates a general error.
#define IFOLDER_ERROR					-1

//! Indicates the TrayApp (main iFolder client process) is not running.
#define IFOLDER_TRAYAPP_NOT_RUNNING		-100

//! Returned when a second process attempts to register as the TrayApp when one is already running.
#define IFOLDER_TRAYAPP_ALREADY_RUNNING	-101

//! A function was called with libifolder being in an invalid state.
#define IFOLDER_INVALID_STATE			-102

//! A function was called before libifolder was initialized.
#define IFOLDER_UNINITIALIZED			-103

//! A process attempted to call ifolder_client_initialized multiple times
#define IFOLDER_ALREADY_INITIALIZED		-104

//! Got an out of memory error when trying to allocate memory
#define IFOLDER_OUT_OF_MEMORY			-105

//! Used for development to indicate an unimplemented function
#define IFOLDER_UNIMPLEMENTED			-106

//! A NULL parameter was passed into a function where it wasn't expected
#define IFOLDER_NULL_PARAMETER			-107


#define IFOLDER_ERROR_PIPE					-200
#define IFOLDER_ERROR_PIPE_READ				-201
#define IFOLDER_ERROR_PIPE_WRITE			-202
#define IFOLDER_ERROR_PIPE_INVALID			-203
#define IFOLDER_ERROR_IPC_SRV_NOT_RUNNING	-204


#endif
