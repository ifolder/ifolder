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

#include "ifolder-errors.h"
#include "IFiFolderClient.h"

iFolderClient::iFolderClient() :
	bInitialized(false)
{
}

iFolderClient::~iFolderClient()
{
}

int
iFolderClient::initialize()
{
	if (bInitialized)
		return IFOLDER_ERROR_ALREADY_INITIALIZED;

	// FIXME: Initialize the client (i.e., start up the IPC server, etc.)

	bInitialized = true;
	return IFOLDER_SUCCESS;
}

int
iFolderClient::uninitialize()
{
	if (!bInitialized)
		return IFOLDER_ERROR_NOT_INITIALIZED;

	// FIXME: Uninitialize the client (i.e., stop the IPC server, etc.)

	bInitialized = false;
	return IFOLDER_SUCCESS;
}

int
iFolderClient::startTrayApp(QString trayAppExePath)
{
	return IFOLDER_SUCCESS;
}
