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

//! iFolder Client API
/**
 *  @file ifolder-client.h
 *
 *  This header file contains all the includes you need to write to
 *  libifolder.
 *
 *  In general you should follow these rules:
 *  - Release/free items returned from a function.
 *      - For example, if a function returns a iFolderAccount, you should
 *     release the iFolderAccount object when you are finished with it
 *     by calling ifolder_account_release(&ifolder_account).
 */

#ifndef _IFOLDER_CLIENT_H
/* @cond */
#define _IFOLDER_CLIENT_H 1
/* @endcond */

#include "account.h"
#include "errors.h"
#include "events.h"
#include "ifolder.h"
#include "prefs.h"
#include "user.h"

#include <stdbool.h>

//! Initialize libifolder for consumption.
/**
 * This function MUST be called before using any other
 * function in this API.  It initializes structures and
 * environments correctly.
 *
 * The only process that should register as the "TrayApp"
 * is, obviously, the TrayApp.  All other processes that
 * wish to use the library should pass in @a FALSE in
 * the @a is_tray_app field.
 *
 * @param is_tray_app If you are writing the main TrayApp
 * you should pass in TRUE.  All other processes should
 * pass in TRUE.
 * @returns Returns IFOLDER_SUCCESS if the library was
 * initialized correctly.  If you are not the TrayApp and
 * the TrayApp is not running, IFOLDER_TRAYAPP_NOT_RUNNING
 * will be returned.
 */
int ifolder_client_initialize(bool is_tray_app);

//! Uninitialize the library before your process quits
/**
 * Although libifolder will periodically check all the
 * registered processes and attempt to clean up memory used,
 * you should call this before your process exits.
 *
 * @returns Returns IFOLDER_SUCCESS if libifolder was
 * uninitialized successfully.
 */
int ifolder_client_uninitialize();

//! Start the TrayApp.
/**
 * This function is provided as a convenience for consumers of
 * this API that are NOT the TrayApp.
 *
 * @param trayapp_exe_path The path to the TrayApp's executable.
 * @returns Returns IFOLDER_SUCCESS if the TrayApp process
 * started successfully.  IFOLDER_TRAYAPP_ALREADY_RUNNING will
 * be returned if the TrayApp is ... yeah, already running.
 */
int ifolder_client_start_tray_app(const char *trayapp_exe_path);

#endif
