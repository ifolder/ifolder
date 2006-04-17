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
#include "IFClient.h"

IFClient::IFClient() :
	bInitialized(false)
{
}

IFClient::~IFClient()
{
	int err;
	IFIPCClient *ipcClient;

	if (ipcClass != NULL)
	{
		ipcClient = (IFIPCClient *)ipcClass;
		if (ipcClient->isRunning())
		{
printf("Terminating the client thread\n");
			ipcClient->terminate();
			ipcClient->wait();
		}
		
		delete ipcClient;
		ipcClass = NULL;
	}
}

int
IFClient::initialize()
{
	int err;
	IFIPCClient *ipcClient;

	if (bInitialized)
		return IFOLDER_ERR_ALREADY_INITIALIZED;

	// @todo Initialize the client (i.e., start up the IPC server, etc.)
	ipcClient = new IFIPCClient();
	if (!ipcClient)
		return IFOLDER_ERR_OUT_OF_MEMORY;
	
	ipcClass = ipcClient;	// hold onto the pointer
	
	err = ipcClient->init();
	if (err != IFOLDER_SUCCESS)
	{
		printf("ipcClient->init() failed: %d\n", err);
		return err;
	}

	printf("IFClient::initialize(): ipcClient->init() succeeded\n");

	// Start the client thread to listen for events from the IPC server.
	ipcClient->start();
	
	printf("IFClient::initialize(): ipcClient->start() called\n");

	err = ipcClient->registerClient();
	if (err != IFOLDER_SUCCESS)
	{
		printf("IFClient::initialize(): Error calling ipcClient->registerClient(): %d\n", err);
		if (ipcClient->isRunning())
		{
printf("Terminating the client thread\n");
			ipcClient->terminate();
			ipcClient->wait();
		}
		delete ipcClient;
		printf("deleted ipcClient\n");
		ipcClass = NULL;
		return err;
	}

	printf("IFClient::initialize(): ipcClient->register() succeeded\n");

	bInitialized = true;
	return IFOLDER_SUCCESS;
}

int
IFClient::uninitialize()
{
	int err;
	IFIPCClient *ipcClient;
	
	if (!bInitialized)
		return IFOLDER_ERR_NOT_INITIALIZED;

	// @todo Uninitialize the client (i.e., stop the IPC server, etc.)
	ipcClient = (IFIPCClient *)ipcClass;
	if (ipcClient->isRunning())
	{
printf("Terminating the client thread\n");
		ipcClient->terminate(); // kill the thread
		ipcClient->wait();
	}
	
	delete ipcClient;
	ipcClass = NULL;
	
	bInitialized = false;
	return IFOLDER_SUCCESS;
}

