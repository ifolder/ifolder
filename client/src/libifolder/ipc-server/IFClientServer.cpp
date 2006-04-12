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
#include "libipcserver/IFIPCServer.h"
#include "IFClient.h"

IFClient::IFClient() :
	bInitialized(false), ipcClass(NULL)
{
}

IFClient::~IFClient()
{
	int err;
	IFIPCServer *ipcServer;

	if (ipcClass != NULL)
	{
		ipcServer = (IFIPCServer *)ipcClass;
		if (ipcServer->isRunning())
		{
			ipcServer->gracefullyExit();
			if (ipcServer->wait(5000) != true)
			{
				// KILL the thread
				ipcServer->exit(-1);
			}
		}
		
		delete ipcServer;
		ipcClass = NULL;
	}
}

int
IFClient::initialize()
{
	int err;
	IFIPCServer *ipcServer;

	if (bInitialized)
		return IFOLDER_ERROR_ALREADY_INITIALIZED;

	// @todo Initialize the client (i.e., start up the IPC server, etc.)
	ipcServer = new IFIPCServer();
//	ipcServer = new IFIPCServer(NULL);
	if (!ipcServer)
		return IFOLDER_ERROR_OUT_OF_MEMORY;

	ipcClass = ipcServer; // Hold onto this pointer

//	connect(ipcServer, SIGNAL(finished()), ipcServer, SLOT(deleteLater()));
	ipcServer->start();

	bInitialized = true;
	return IFOLDER_SUCCESS;
}

int
IFClient::uninitialize()
{
	int err;
	IFIPCServer *ipcServer;

	if (!bInitialized)
		return IFOLDER_ERROR_NOT_INITIALIZED;

	// @todo Uninitialize the client (i.e., stop the IPC server, etc.)
	ipcServer = (IFIPCServer *)ipcClass;
	if (ipcServer->isRunning())
	{
		ipcServer->gracefullyExit();
		if (ipcServer->wait(5000) != true)
		{
			// KILL the thread
			ipcServer->exit(-1);
		}
	}
	
	delete ipcServer;
	ipcClass = NULL;

	bInitialized = false;
	return IFOLDER_SUCCESS;
}

