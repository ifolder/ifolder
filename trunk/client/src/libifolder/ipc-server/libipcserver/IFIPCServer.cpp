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

IFIPCServer::IFIPCServer() :
	serverNamedPipe(NULL), bExit(false)
//IFIPCServer::IFIPCServer(QObject *parent) :
//	QThread(parent), serverNamedPipe(NULL), bExit(false)
{
}

IFIPCServer::~IFIPCServer()
{
	int err;

	if (serverNamedPipe != NULL)
	{
		err = serverNamedPipe->closePipe();
		if (err != IFOLDER_SUCCESS)
			printf("serverNamedPipe->closePipe() returned %d\n", err); // @todo log this to an error log
		
		delete serverNamedPipe;
		serverNamedPipe = NULL;
	}
}

void
IFIPCServer::run()
{
	int err;
	char serverNamedPipePath[NAMED_PIPE_PATH_MAX];
	void *message;
	uint messageType;

	printf("IFIPCServer::run()\n");

	sprintf(serverNamedPipePath, IFOLDER_SERVER_NAMED_PIPE, "boyd");	// @todo Determine the user name programmatically
	
	serverNamedPipe = new IFNamedPipe(QString(serverNamedPipePath), IFNamedPipe::ReadOnly);

	err = serverNamedPipe->openPipe(true, true);
	if (err != IFOLDER_SUCCESS)
	{
		// @todo Log this to the error log
		printf("serverNamedPipe->openPipe(true) returned: %d\n", err);
		return;
	}
	
	while (!bExit)
	{
		err = serverNamedPipe->readMessage(&messageType, &message);
		if (err != IFOLDER_SUCCESS)
		{
			// @todo Add the following line to the error log
			printf("serverNamedPipe->readMessage() returned %d\n", err);
			err = serverNamedPipe->reset();
			if (err != IFOLDER_SUCCESS)
			{
				printf("IFIPCServer::run(): error resetting server pipe: %d\n", err);
				return;	// @todo Send a message via libifolder to the API consumer so they know there was a bad IPC error
			}

			continue;
		}
		
		if (bExit)
			break;	// gracefully exit

		err = processMessage(messageType, message);
		if (err != IFOLDER_SUCCESS)
		{
			// Close and reopen the pipe to clear the bad out!
			printf("IFIPCServer::run(): processMessage() returned %d\nClosing and re-opening the server pipe...\n", err); // @todo log this to an error log
			err = serverNamedPipe->reset();
			if (err != IFOLDER_SUCCESS)
			{
				printf("IFIPCServer::run(): error resetting server pipe: %d\n", err);
				return;	// @todo Send a message via libifolder to the API consumer so they know there was a bad IPC error
			}
		}
	}
}

void
IFIPCServer::gracefullyExit()
{
	bExit = true;
}

void
IFIPCServer::initHeader(iFolderMessageHeader *header, uint messageType)
{
	header->senderPID = 0; // @todo Get the PID of this process
	header->messageType = messageType;
	memset(header->messageNamedPipePath, '\0', NAMED_PIPE_PATH_MAX);
}

int
IFIPCServer::ipcRespond(QString repsonseNamedPipePath, void *response)
{
	int err;
	iFolderMessageHeader *header;
	IFNamedPipe *responseNamedPipe;

	header = (iFolderMessageHeader *)response;

	// Open the client's named pipe to write the response
	responseNamedPipe = new IFNamedPipe(repsonseNamedPipePath, IFNamedPipe::WriteOnly);
	if (!responseNamedPipe)
		return IFOLDER_ERR_OUT_OF_MEMORY;

printf("IFIPCServer::ipcRespond(): created a pipe to write the response\n");
	
	err = responseNamedPipe->openPipe(true, false);
	if (err != IFOLDER_SUCCESS)
	{
		delete responseNamedPipe;
		printf("IFIPCServer::ipcRespond(): Error opening client's named pipe: %d\n", err);
		return err;
	}
	
printf("IFIPCServer::ipcRespond(): opened client's named pipe for writing\n");

	switch(header->messageType)
	{
		case IFOLDER_MSG_REGISTER_CLIENT_RESPONSE:
			err = responseNamedPipe->writeMessage(response, sizeof(iFolderMessageRegisterClientResponse));
			break;
		case IFOLDER_MSG_UNREGISTER_CLIENT_RESPONSE:
			err = responseNamedPipe->writeMessage(response, sizeof(iFolderMessageUnregisterClientResponse));
			break;
		case IFOLDER_MSG_DOMAIN_ADD_RESPONSE:
			err = responseNamedPipe->writeMessage(response, sizeof(iFolderMessageDomainAddResponse));
			break;
		default:
			delete responseNamedPipe;
			return IFOLDER_ERR_IPC_UNKNOWN_MESSAGE;
			break;
	}
	
	delete responseNamedPipe;	// this will close the named pipe
	
	return err;
}

int
IFIPCServer::processMessage(uint messageType, void *message)
{
	int err;

	printf("IFIPCServer::processMessage() called with message type: %d\n", messageType);
	
	// The message handlers should delete the memory used by the message
	switch(messageType)
	{
		case IFOLDER_MSG_REGISTER_CLIENT_REQUEST:
			err = handleRegisterClientRequest((iFolderMessageRegisterClientRequest *)message);
			break;
		case IFOLDER_MSG_UNREGISTER_CLIENT_REQUEST:
			err = handleUnregisterClientRequest((iFolderMessageUnregisterClientRequest *)message);
			break;
		case IFOLDER_MSG_DOMAIN_ADD_REQUEST:
			err = handleDomainAddRequest((iFolderMessageDomainAddRequest *)message);
			break;
		default:
			break;
	}

	return err;
}

int
IFIPCServer::handleRegisterClientRequest(iFolderMessageRegisterClientRequest *message)
{
	int err;
	iFolderMessageRegisterClientResponse response;

	printf("IFIPCServer::handleRegisterClientRequest()\n");
	printf("\t%s\n", message->clientNamedPipe);

	err = IFOLDER_SUCCESS; // @todo Really register the client so that messages/events can be sent back to it to its named pipe

	initHeader((iFolderMessageHeader *)&response, IFOLDER_MSG_REGISTER_CLIENT_RESPONSE);
	response.returnCode = err;
	
	err = ipcRespond(QString(message->header.messageNamedPipePath), &response);
	
	free(message);

	return err;
}

int
IFIPCServer::handleUnregisterClientRequest(iFolderMessageUnregisterClientRequest *message)
{
	int err;
	iFolderMessageUnregisterClientResponse response;

	printf("IFIPCServer::handleUnregisterClientRequest()\n");

	err = IFOLDER_SUCCESS; // @todo Really unregister the client so that messages/events can be sent back to it to its named pipe

	initHeader((iFolderMessageHeader *)&response, IFOLDER_MSG_UNREGISTER_CLIENT_RESPONSE);
	response.returnCode = err;
	
	err = ipcRespond(QString(message->header.messageNamedPipePath), &response);
	
	free(message);

	return err;
}

int
IFIPCServer::handleDomainAddRequest(iFolderMessageDomainAddRequest *message)
{
	int err;
	iFolderMessageDomainAddResponse response;

	printf("IFIPCServer::handleDomainAddRequest()\n");
	printf("\t%s, %s, ********, %s\n",
			message->hostAddress,
			message->userName,
			message->makeDefault ? "true" : "false");

	err = IFOLDER_SUCCESS; // @todo Add a new domain and collect the return value

	initHeader((iFolderMessageHeader *)&response, IFOLDER_MSG_DOMAIN_ADD_RESPONSE);
	response.returnCode = err;
	
	sprintf(response.id, "<myid>");
	sprintf(response.name, "The Domain Name");
	sprintf(response.description, "The Domain Description");
	sprintf(response.version, "The Domain Version");
	sprintf(response.hostAddress, message->hostAddress);
	sprintf(response.machineName, "The Machine Name");
	sprintf(response.osVersion, "Linux, DUH!");
	sprintf(response.userName, message->userName);
	response.isDefault = true;
	response.isActive = true;
	
	err = ipcRespond(QString(message->header.messageNamedPipePath), &response);
	
	free(message);

	return err;
}
