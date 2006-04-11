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

#include <ifolder-errors.h>
#include <common/IFNamedPipe.h>
#include <common/IFMessages.h>

#include "IFIPCClient.h"

iFolderIPCClient::iFolderIPCClient() :
	clientNamedPipe(NULL), bExit(false), bInitialized(false)
{
}

iFolderIPCClient::~iFolderIPCClient()
{
	int err;

	if (clientNamedPipe != NULL)
	{
		err = clientNamedPipe->closePipe();
		if (err != IFOLDER_SUCCESS)
			printf("clientNamedPipe->closePipe() returned %d\n", err); // FIXME: log this to an error log
		
		delete clientNamedPipe;
		clientNamedPipe = NULL;
	}
	
	err = unregisterClient();
	if (err != IFOLDER_SUCCESS)
		printf("iFolderIPCClient could not unregister: %d\n", err);
}

int
iFolderIPCClient::init()
{
	int err;
	clientNamedPipe = iFolderNamedPipe::createNamedPipeByPid(iFolderNamedPipe::ReadOnly);
	if (!clientNamedPipe)
		return IFOLDER_ERROR_OUT_OF_MEMORY;
	
	err = clientNamedPipe->create();
	if (err != IFOLDER_SUCCESS)
	{
		printf("iFolderIPCClient::init(): could not create client's named pipe: %d\n", err);
		return err;
	}
	
	bInitialized = true;

	printf("iFolderIPCClient::init(): createNamedPipeByPid() returned successfully\n");
	
	return IFOLDER_SUCCESS;
}

int
iFolderIPCClient::registerClient()
{
	int err;
	iFolderMessageRegisterClientRequest request;
	iFolderMessageRegisterClientResponse *response;
	
	if (!bInitialized || !isRunning())
		return IFOLDER_ERROR_IPC_INVALID_STATE;

	printf("iFolderIPCClient::registerClient(%s)\n", qPrintable(clientNamedPipe->path()));
	
	initHeader((iFolderMessageHeader *)&request, IFOLDER_MSG_REGISTER_CLIENT_REQUEST);
	
	sprintf(request.clientNamedPipe, qPrintable(clientNamedPipe->path()));
	
	err = ipcCall(&request, (void **)&response);
	
	if (err == IFOLDER_SUCCESS)
		free(response);
	
	return err;
}

void
iFolderIPCClient::run()
{
	int err;
	char tempNamedPipePath[NAMED_PIPE_PATH_MAX];
	void *message;
	uint messageType;

	printf("iFolderIPCClient::run()\n");

	if (!bInitialized)
	{
		printf("The IPC Client listen thread was started without being initialized!\n");
		return;
	}

	err = clientNamedPipe->openPipe(true, true);
	if (err != IFOLDER_SUCCESS)
	{
		delete clientNamedPipe;
		clientNamedPipe = NULL;
		// FIXME: Log this to the error log
		printf("iFolderIPCClient::run(): clientNamedPipe->openPipe(true) returned: %d\n", err);
		return;
	}
	
	printf("iFolderIPCClient::run(): openPipe succeeded\n");
	
	while (!bExit)
	{
		err = clientNamedPipe->readMessage(&messageType, &message);
		if (err != IFOLDER_SUCCESS)
		{
			// FIXME: Add the following line to the error log
			printf("clientNamedPipe->readMessage() returned %d\n", err);
			err = clientNamedPipe->reset();
			if (err != IFOLDER_SUCCESS)
			{
				printf("iFolderIPCClient::run(): error resetting client pipe: %d\n", err);
				return;	// FIXME: Send a message via libifolder to the API consumer so they know there was a bad IPC error
			}

			continue;
		}
		
		if (bExit)
			break;	// gracefully exit
		
		err = processMessage(messageType, message);
		if (err != IFOLDER_SUCCESS)
		{
			// Close and reopen the pipe to clear the bad out!
			printf("iFolderIPCClient::run(): processMessage() returned %d\nClosing and re-opening the client pipe...\n", err); // FIXME: log this to an error log
			err = clientNamedPipe->reset();
			if (err != IFOLDER_SUCCESS)
			{
				printf("iFolderIPCClient::run(): error resetting client pipe: %d\n", err);
				return;	// FIXME: Send a message via libifolder to the API consumer so they know there was a bad IPC error
			}
		}
	}
}

void
iFolderIPCClient::gracefullyExit()
{
	bExit = true;
}

void
iFolderIPCClient::initHeader(iFolderMessageHeader *header, uint messageType)
{
	header->senderPID = 0; // FIXME: Get the PID of this process
	header->messageType = messageType;
	memset(header->messageNamedPipePath, '\0', NAMED_PIPE_PATH_MAX);
}

int
iFolderIPCClient::ipcCall(void *request, void **response)
{
	int err;
	iFolderMessageHeader *header;
	iFolderNamedPipe *serverNamedPipe;
	iFolderNamedPipe *tempNamedPipe;
	
	header = (iFolderMessageHeader *)request;
	
	// Open the server's named pipe to write the request
	serverNamedPipe = iFolderNamedPipe::createServerNamedPipeForWriting();
	if (!serverNamedPipe)
		return IFOLDER_ERROR_OUT_OF_MEMORY;

printf("iFolderIPCClient::ipcCall(): created server pipe and now will open it\n");
	
	err = serverNamedPipe->openPipe(false, false);
	if (err != IFOLDER_SUCCESS)
	{
		delete serverNamedPipe;
		printf("Error opening server's named pipe: %d\n", err);
		return err;
	}
	
printf("iFolderIPCClient::ipcCall(): opened server's named pipe for writing\n");

	// Create a named pipe to read the response
	tempNamedPipe = iFolderNamedPipe::createUniqueNamedPipe(iFolderNamedPipe::ReadOnly);
	if (!tempNamedPipe)
	{
		delete serverNamedPipe;
		return IFOLDER_ERROR_OUT_OF_MEMORY;
	}

	err = tempNamedPipe->create();
	if (err != IFOLDER_SUCCESS)
	{
		printf("Error creating temporary named pipe\n");
		return err;
	}
	
	sprintf(header->messageNamedPipePath, qPrintable(tempNamedPipe->path()));
	
	switch(header->messageType)
	{
		case IFOLDER_MSG_REGISTER_CLIENT_REQUEST:
			err = ipcCallRegisterClient(serverNamedPipe, tempNamedPipe, (iFolderMessageRegisterClientRequest *)request, (iFolderMessageRegisterClientResponse **)response);
			break;
		case IFOLDER_MSG_UNREGISTER_CLIENT_REQUEST:
			err = ipcCallUnregisterClient(serverNamedPipe, tempNamedPipe, (iFolderMessageUnregisterClientRequest *)request, (iFolderMessageUnregisterClientResponse **)response);
			break;
		default:
			delete serverNamedPipe;
			delete tempNamedPipe;
			return IFOLDER_ERROR_IPC_UNKNOWN_MESSAGE;
			break;
	}
	
	delete serverNamedPipe;
	delete tempNamedPipe;
	
	return err;
}

int
iFolderIPCClient::processMessage(int messageType, void *message)
{
	printf("iFolderIPCClient::processMessage() called with message type: %d\n", messageType);
	
	// FIXME: Process the message and delete the memory used by it

	return IFOLDER_SUCCESS;
}

int
iFolderIPCClient::ipcCallRegisterClient(iFolderNamedPipe *serverNamedPipe, iFolderNamedPipe *tempNamedPipe, iFolderMessageRegisterClientRequest *request, iFolderMessageRegisterClientResponse **response)
{
	int err;
	uint messageType;
	
	void *tempResponse;
	
//	printf("Press any key to send ipc message to server...");
//	getchar();
printf("iFolderIPCClient::ipcCallRegisterClient()\n");

	err = serverNamedPipe->writeMessage(request, sizeof(iFolderMessageRegisterClientRequest));
	if (err != IFOLDER_SUCCESS)
	{
		printf("iFolderIPCClient::ipcCallRegisterClient(): error writing message: %d\n", err);
		return err;
	}
	
printf("serverNamedPipe->writeMessage() called\n");

	err = tempNamedPipe->openPipe(true, false);
	if (err != IFOLDER_SUCCESS)
	{
		printf("Error opening temporary named pipe for iFolderIPCClient::ipcCall(): %d\n", err);
		return err;
	}

printf("tempNamedPipe->openPipe() called\n");
	
	err = tempNamedPipe->readMessage(&messageType, &tempResponse);
	if (err != IFOLDER_SUCCESS)
	{
		printf("iFolderIPCClient::ipcCallRegisterClient(): error reading response: %d\n", err);
		return err;
	}

printf("tempNamedPipe->readMessage() called\n");
	
	if (messageType != IFOLDER_MSG_REGISTER_CLIENT_RESPONSE)
	{
		printf("iFolderIPCClient::ipcCallRegisterClient(): received bad message type: %d\n", messageType);
		return IFOLDER_ERROR_IPC_INVALID_MESSAGE;
	}
	
	*response = (iFolderMessageRegisterClientResponse *)tempResponse;
	
	return IFOLDER_SUCCESS;
}

int
iFolderIPCClient::unregisterClient()
{
	int err;
	iFolderMessageUnregisterClientRequest request;
	iFolderMessageUnregisterClientResponse *response;
	
	initHeader((iFolderMessageHeader *)&request, IFOLDER_MSG_UNREGISTER_CLIENT_REQUEST);
	
	err = ipcCall(&request, (void **)&response);
	
	if (err == IFOLDER_SUCCESS)
		free(response);
	
	return err;
}

int
iFolderIPCClient::ipcCallUnregisterClient(iFolderNamedPipe *serverNamedPipe, iFolderNamedPipe *tempNamedPipe, iFolderMessageUnregisterClientRequest *request, iFolderMessageUnregisterClientResponse **response)
{
	int err;
	uint messageType;
	
	void *tempResponse;

	err = serverNamedPipe->writeMessage(request, sizeof(iFolderMessageUnregisterClientRequest));
	if (err != IFOLDER_SUCCESS)
	{
		printf("iFolderIPCClient::ipcCallUnregisterClient(): error writing message: %d\n", err);
		return err;
	}
	
	err = tempNamedPipe->openPipe(true, false);
	if (err != IFOLDER_SUCCESS)
	{
		delete serverNamedPipe;
		delete tempNamedPipe;
		printf("Error opening temporary named pipe for iFolderIPCClient::ipcCall(): %d\n", err);
		return err;
	}
	
	err = tempNamedPipe->readMessage(&messageType, &tempResponse);
	if (err != IFOLDER_SUCCESS)
	{
		printf("iFolderIPCClient::ipcCallUnregisterClient(): error reading response: %d\n", err);
		return err;
	}
	
	if (messageType != IFOLDER_MSG_UNREGISTER_CLIENT_RESPONSE)
	{
		printf("iFolderIPCClient::ipcCallUnregisterClient(): received bad message type: %d\n", messageType);
		return IFOLDER_ERROR_IPC_INVALID_MESSAGE;
	}
	
	*response = (iFolderMessageUnregisterClientResponse *)tempResponse;
	
	return IFOLDER_SUCCESS;
}

