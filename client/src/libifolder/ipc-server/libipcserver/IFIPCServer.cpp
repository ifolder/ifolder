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
#include <string.h>

#include "ifolder-errors.h"
#include "common/IFNamedPipe.h"
#include "common/IFMessages.h"

#include "IFIPCServer.h"

iFolderIPCServer::iFolderIPCServer(/*QObject *parent*/) :
	/*QThread(parent), */serverNamedPipe(NULL), bExit(false)
{
}

iFolderIPCServer::~iFolderIPCServer()
{
	int err;

	if (serverNamedPipe != NULL)
	{
		err = serverNamedPipe->closePipe();
		if (err != IFOLDER_SUCCESS)
			printf("serverNamedPipe->closePipe() returned %d\n", err); // FIXME: log this to an error log
		
		delete serverNamedPipe;
		serverNamedPipe = NULL;
	}
}

void
iFolderIPCServer::run()
{
	int err;
	char serverNamedPipePath[NAMED_PIPE_PATH_MAX];
	void *message;
	int messageType;

	printf("iFolderIPCServer::run()\n");

	sprintf(serverNamedPipePath, IFOLDER_SERVER_NAMED_PIPE, "boyd");	// FIXME: Determine the user name programmatically
	
	serverNamedPipe = new iFolderNamedPipe(QString(serverNamedPipePath), iFolderNamedPipe::ReadOnly);

	err = serverNamedPipe->openPipe(true);
	if (err != IFOLDER_SUCCESS)
	{
		// FIXME: Log this to the error log
		printf("serverNamedPipe->open(true) returned: %d\n", err);
		return;
	}
	
	while (!bExit)
	{
		err = serverNamedPipe->readMessage(&messageType, &message);
		if (err != IFOLDER_SUCCESS)
		{
			// FIXME: Add the following line to the error log
			printf("serverNamedPipe->readMessage() returned %d\n", err);
			continue;
		}
		
		err = processMessage(messageType, message);
		if (err != IFOLDER_SUCCESS)
			printf("processMessage() returned %d\n", err); // FIXME: log this to an error log		
	}
}

int
iFolderIPCServer::processMessage(int messageType, void *message)
{
	printf("iFolderIPCServer::processMessage() called with message type: %d\n", messageType);
	
	// FIXME: Process the message and delete the memory used by it

	return IFOLDER_SUCCESS;
}

void
iFolderIPCServer::gracefullyExit()
{
	bExit = true;
}

