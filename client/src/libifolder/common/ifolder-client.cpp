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

#include <stdio.h>
#include "ifolder-errors.h"
#include "ifolder-client.h"

#include "IFiFolderClient.h"

static iFolderClient *ifolderClient = NULL;

int
ifolder_client_initialize(void)
{
	if (ifolderClient != NULL)
		return ifolderClient->initialize();
	
	// It is null, so new one up.
	ifolderClient = new iFolderClient();
	if (ifolderClient == NULL)
		return IFOLDER_ERROR_OUT_OF_MEMORY;

	return ifolderClient->initialize();
}

int
ifolder_client_uninitialize(void)
{
	int err;

	if (ifolderClient != NULL)
	{
		err = ifolderClient->uninitialize();
		if (err == IFOLDER_SUCCESS)
		{
			delete ifolderClient;
			ifolderClient = NULL;
		}

		return err;
	}
	
	return IFOLDER_ERROR_NOT_INITIALIZED;
}

int
ifolder_start_tray_app(const char *tray_app_exe_path)
{
	QString trayAppExePath(tray_app_exe_path);
	if (ifolderClient != NULL)
		return ifolderClient->startTrayApp(trayAppExePath);
	
	return IFOLDER_ERROR_NOT_INITIALIZED;
}
