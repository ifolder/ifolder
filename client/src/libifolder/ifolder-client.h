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

//! Initialize the iFolder Client.
/**
 * This must be called before using the iFolder Client API.
 * 
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_client_initialize(void);

//! Uninitialize the iFolder Client
/**
 * This must be called when you are finished using the iFolder Client (usually
 * just before your process exits).
 * 
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_client_uninitialize(void);

#ifdef __cplusplus
}
#endif		/* __cplusplus */

#endif /*_IFOLDER_C_CLIENT_H_*/
