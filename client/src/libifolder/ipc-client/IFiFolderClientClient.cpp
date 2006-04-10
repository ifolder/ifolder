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
#include "IFIPCClient.h"
#include "IFiFolderClient.h"

iFolderClient::iFolderClient() :
	bInitialized(false)
{
}

iFolderClient::~iFolderClient()
{
	int err;
	iFolderIPCClient *ipcClient;

	if (ipcClass != NULL)
	{
		ipcClient = (iFolderIPCClient *)ipcClass;
		if (ipcClient->isRunning())
		{
			ipcClient->gracefullyExit();
			if (ipcClient->wait(5000) != true)
			{
				// KILL the thread
				ipcClient->exit(-1);
			}
		}
		
		delete ipcClient;
		ipcClass = NULL;
	}
}

int
iFolderClient::initialize()
{
	int err;
	iFolderIPCClient *ipcClient;

	if (bInitialized)
		return IFOLDER_ERROR_ALREADY_INITIALIZED;

	// FIXME: Initialize the client (i.e., start up the IPC server, etc.)
	ipcClient = new iFolderIPCClient();
	if (!ipcClient)
		return IFOLDER_ERROR_OUT_OF_MEMORY;
	
	ipcClass = ipcClient;	// hold onto the pointer
	
	err = ipcClient->init();
	if (err != IFOLDER_SUCCESS)
	{
		printf("ipcClient->init() failed: %d\n", err);
		return err;
	}

	printf("iFolderClient::initialize(): ipcClient->init() succeeded\n");

	// Start the client thread to listen for events from the IPC server.
	ipcClient->start();
	
	printf("iFolderClient::initialize(): ipcClient->start() called\n");

	err = ipcClient->registerClient();
	if (err != IFOLDER_SUCCESS)
	{
		printf("iFolderClient::initialize(): Error calling ipcClient->registerClient(): %d\n", err);
		delete ipcClient;
		printf("deleted ipcClient\n");
		ipcClass = NULL;
		return err;
	}

	printf("iFolderClient::initialize(): ipcClient->register() succeeded\n");

	bInitialized = true;
	return IFOLDER_SUCCESS;
}

int
iFolderClient::uninitialize()
{
	int err;
	iFolderIPCClient *ipcClient;
	
	if (!bInitialized)
		return IFOLDER_ERROR_NOT_INITIALIZED;

	// FIXME: Uninitialize the client (i.e., stop the IPC server, etc.)
	ipcClient = (iFolderIPCClient *)ipcClass;
	if (ipcClient->isRunning())
	{
		ipcClient->gracefullyExit();
		printf("Waiting 5 seconds for the client thread to finish\n");
		if (ipcClient->wait(5000) != true)
		{
			// kill the thread
			ipcClient->exit(-1);
		}
	}
	
	delete ipcClient;
	ipcClass = NULL;
	
	bInitialized = false;
	return IFOLDER_SUCCESS;
}

int
iFolderClient::startTrayApp(QString trayAppExePath)
{
	return IFOLDER_SUCCESS;
}
