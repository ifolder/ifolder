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

#include "IFIPCServer.h"

iFolderIPCServer::iFolderIPCServer() :
	serverNamedPipe(NULL), bExit(false)
//iFolderIPCServer::iFolderIPCServer(QObject *parent) :
//	QThread(parent), serverNamedPipe(NULL), bExit(false)
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
	uint messageType;

	printf("iFolderIPCServer::run()\n");

	sprintf(serverNamedPipePath, IFOLDER_SERVER_NAMED_PIPE, "boyd");	// FIXME: Determine the user name programmatically
	
	serverNamedPipe = new iFolderNamedPipe(QString(serverNamedPipePath), iFolderNamedPipe::ReadOnly);

	err = serverNamedPipe->openPipe(true);
	if (err != IFOLDER_SUCCESS)
	{
		// FIXME: Log this to the error log
		printf("serverNamedPipe->openPipe(true) returned: %d\n", err);
		return;
	}
	
	while (!bExit)
	{
		err = serverNamedPipe->readMessage(&messageType, &message);
		if (err != IFOLDER_SUCCESS)
		{
			// FIXME: Add the following line to the error log
			printf("serverNamedPipe->readMessage() returned %d\n", err);
			err = serverNamedPipe->reset();
			if (err != IFOLDER_SUCCESS)
			{
				printf("iFolderIPCServer::run(): error resetting server pipe: %d\n", err);
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
			printf("iFolderIPCServer::run(): processMessage() returned %d\nClosing and re-opening the server pipe...\n", err); // FIXME: log this to an error log
			err = serverNamedPipe->reset();
			if (err != IFOLDER_SUCCESS)
			{
				printf("iFolderIPCServer::run(): error resetting server pipe: %d\n", err);
				return;	// FIXME: Send a message via libifolder to the API consumer so they know there was a bad IPC error
			}
		}
	}
}

void
iFolderIPCServer::gracefullyExit()
{
	bExit = true;
}

void
iFolderIPCServer::initHeader(iFolderMessageHeader *header, uint messageType)
{
	header->senderPID = 0; // FIXME: Get the PID of this process
	header->messageType = messageType;
	memset(header->messageNamedPipePath, '\0', NAMED_PIPE_PATH_MAX);
}

int
iFolderIPCServer::ipcRespond(QString repsonseNamedPipePath, void *response)
{
	int err;
	iFolderMessageHeader *header;
	iFolderNamedPipe *responseNamedPipe;

	header = (iFolderMessageHeader *)response;

	// Open the client's named pipe to write the response
	responseNamedPipe = new iFolderNamedPipe(repsonseNamedPipePath, iFolderNamedPipe::WriteOnly);
	if (!responseNamedPipe)
		return IFOLDER_ERROR_OUT_OF_MEMORY;

printf("iFolderIPCServer::ipcRespond(): created a pipe to write the response\n");
	
	err = responseNamedPipe->openPipe();
	if (err != IFOLDER_SUCCESS)
	{
		delete responseNamedPipe;
		printf("iFolderIPCServer::ipcRespond(): Error opening client's named pipe: %d\n", err);
		return err;
	}
	
printf("iFolderIPCServer::ipcRespond(): opened client's named pipe for writing\n");

	switch(header->messageType)
	{
		case IFOLDER_MSG_REGISTER_CLIENT_RESPONSE:
			err = responseNamedPipe->writeMessage(response, sizeof(iFolderMessageRegisterClientResponse));
			break;
		case IFOLDER_MSG_UNREGISTER_CLIENT_RESPONSE:
			err = responseNamedPipe->writeMessage(response, sizeof(iFolderMessageUnregisterClientResponse));
			break;
		default:
			delete responseNamedPipe;
			return IFOLDER_ERROR_IPC_UNKNOWN_MESSAGE;
			break;
	}
	
	delete responseNamedPipe;	// this will close the named pipe
	
	return err;
}

int
iFolderIPCServer::processMessage(uint messageType, void *message)
{
	int err;

	printf("iFolderIPCServer::processMessage() called with message type: %d\n", messageType);
	
	// The message handlers should delete the memory used by the message
	switch(messageType)
	{
		case IFOLDER_MSG_REGISTER_CLIENT_REQUEST:
			err = handleRegisterClientRequest((iFolderMessageRegisterClientRequest *)message);
			break;
		case IFOLDER_MSG_UNREGISTER_CLIENT_REQUEST:
			err = handleUnregisterClientRequest((iFolderMessageUnregisterClientRequest *)message);
			break;
		default:
			break;
	}

	return err;
}

int
iFolderIPCServer::handleRegisterClientRequest(iFolderMessageRegisterClientRequest *message)
{
	int err;
	iFolderMessageRegisterClientResponse response;

	printf("iFolderIPCServer::handleRegisterClientRequest()\n");
	printf("\t%s\n", message->clientNamedPipe);

	err = IFOLDER_SUCCESS; // FIXME: Really register the client so that messages/events can be sent back to it to its named pipe

	initHeader((iFolderMessageHeader *)&response, IFOLDER_MSG_REGISTER_CLIENT_RESPONSE);
	response.returnCode = err;
	
	err = ipcRespond(QString(message->header.messageNamedPipePath), &response);
	
	free(message);

	return err;
}

int
iFolderIPCServer::handleUnregisterClientRequest(iFolderMessageUnregisterClientRequest *message)
{
	int err;
	iFolderMessageUnregisterClientResponse response;

	printf("iFolderIPCServer::handleUnregisterClientRequest()\n");

	err = IFOLDER_SUCCESS; // FIXME: Really unregister the client so that messages/events can be sent back to it to its named pipe

	initHeader((iFolderMessageHeader *)&response, IFOLDER_MSG_UNREGISTER_CLIENT_RESPONSE);
	response.returnCode = err;
	
	err = ipcRespond(QString(message->header.messageNamedPipePath), &response);
	
	free(message);

	return err;
}
