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

#include "IFClient.h"

static IFClient *ifolderClient = NULL;

int
ifolder_client_initialize(const char *data_path)
{
	if (ifolderClient != NULL)
		return ifolderClient->initialize(data_path);
	
	// It is null, so new one up.
	ifolderClient = new IFClient();
	if (ifolderClient == NULL)
		return IFOLDER_ERR_OUT_OF_MEMORY;

	return ifolderClient->initialize(data_path);
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
	
	return IFOLDER_ERR_NOT_INITIALIZED;
}

iFolderClientState
ifolder_client_get_state(void)
{
	if (ifolderClient == NULL)
		return IFOLDER_ERR_NOT_INITIALIZED;
	
	return ifolderClient->getState();
}

int
ifolder_client_run_client_update(const iFolderDomain domain)
{
	IFDomain *ifDomain;

	if (ifolderClient == NULL)
		return IFOLDER_ERR_NOT_INITIALIZED;
		
	ifDomain = (IFDomain *)domain;
	
	return ifolderClient->runClientUpdate(ifDomain);
}

int
ifolder_client_start_synchronization(void)
{
	if (ifolderClient == NULL)
		return IFOLDER_ERR_NOT_INITIALIZED;
	
	return ifolderClient->startSynchronization();
}

int
ifolder_client_stop_synchronization(void)
{
	if (ifolderClient == NULL)
		return IFOLDER_ERR_NOT_INITIALIZED;

	return ifolderClient->stopSynchronization();
}

int
ifolder_client_resume_synchronization(void)
{
	if (ifolderClient == NULL)
		return IFOLDER_ERR_NOT_INITIALIZED;

	return ifolderClient->resumeSynchronization();
}
