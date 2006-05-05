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

#include <sys/types.h>
#include <sys/stat.h>
#include <sys/uio.h>
#include <unistd.h>
#include <fcntl.h>
#include <string.h>
#include <stdio.h>
#include <errno.h>

#include <QUuid>
#include <QString>

#include "ifolder-errors.h"
#include "IFMessages.h"

#include "IFNamedPipe.h"

IFNamedPipe::IFNamedPipe(QString pathName, PermissionType permissionType) :
	myPathName(pathName), myPermissionType(permissionType), myFileDescriptor(-1)
{
	printf("IFNamedPipe::IFNamedPipe(%s, %d)\n", qPrintable(pathName), permissionType);
}

IFNamedPipe::~IFNamedPipe()
{
	int err;
printf("IFNamedPipe::~IFNamedPipe(): %s\n", qPrintable(myPathName));
	if (myFileDescriptor != -1)
	{
		err = closePipe();
		if (err != IFOLDER_SUCCESS)
		{
			// @todo Figure a way to log this error to a log file
printf("IFNamedPipe::~IFNamedPipe(): Error closing pipe\n");
		}
	}
}

QString
IFNamedPipe::path()
{
	return myPathName;
}

int
IFNamedPipe::openPipe(bool block, bool createIfNeeded)
{
	int err;

	if (myPermissionType == WriteOnly)
	{
		if (createIfNeeded)
		{
			err = create();	// Create a pipe for writing
			if (err != IFOLDER_SUCCESS)
				return err;
		}
		
printf("IFNamedPipe::openingPipe in write only mode\n");
		if (block)
			err = open(qPrintable(myPathName), O_WRONLY);
		else
			err = open(qPrintable(myPathName), O_WRONLY | O_NONBLOCK);
		if (err == -1)
		{
			printf("IFNamedPipe::openPipe(): open() returned errno: %d\n", errno);
			perror(qPrintable(myPathName));
			return IFOLDER_ERR_IPC_CREATE;	// @todo Read errno and return a less generic error
		}
		
		myFileDescriptor = err;
	}
	else
	{
		if (createIfNeeded)
		{
			err = create();	// Create a pipe for reading
			if (err != IFOLDER_SUCCESS)
				return err;
		}
		
printf("IFNamedPipe::openingPipe in read only mode\n");
		err = open(qPrintable(myPathName), O_RDONLY);
		if (err == -1)
		{
			printf("IFNamedPipe::openPipe(): open() returned errno: %d\n", errno);
			perror(qPrintable(myPathName));
			return IFOLDER_ERR_IPC_CREATE;	// @todo Read errno and return a less generic error
		}
		
		myFileDescriptor = err;
	}
	
	return IFOLDER_SUCCESS;
}

int
IFNamedPipe::closePipe()
{
	int err;

	if (myFileDescriptor == -1)
		return IFOLDER_SUCCESS;
	
	err = close(myFileDescriptor);
	if (err != 0)
		return IFOLDER_ERR_IPC_CLOSE;	// @todo Read errno and return a less generic error
	
	if (myPermissionType == ReadOnly)
	{
		// Delete the file
		do
		{
			err = unlink(qPrintable(myPathName));
			if (err != 0)
				printf("unlink() returned -1 in IFNamedPipe::close()\n");//: %d\n", errno);
		} while (err != -1);
	}
	
	return IFOLDER_SUCCESS;
}

int
IFNamedPipe::writeMessage(const void *message, size_t messageSize)
{
	ssize_t bytesWritten;
	
	if (myFileDescriptor == -1)
		return IFOLDER_ERR_IPC_INVALID;
	
	bytesWritten = write(myFileDescriptor, message, messageSize);
	if (bytesWritten == -1)
	{
		printf("write returned -1\n");
		perror(qPrintable(myPathName));
		return IFOLDER_ERR_IPC_WRITE; // @todo Read errno for a non-generic error
	}

	if (bytesWritten != messageSize)
	{
		printf("Entire message not sent!\n");
		return IFOLDER_ERR_IPC_WRITE;
	}
	
	return IFOLDER_SUCCESS;
}

int
IFNamedPipe::readMessage(uint *messageTypeReturn, void **messageReturn)
{
	int bytesRead;
	int messageSize;
	int extraMessageSize;
	void *message;

	iFolderMessageHeader header;

//readAgain:
	
	bytesRead = readPipe(&header, sizeof(iFolderMessageHeader));
	if (bytesRead <= 0)
	{
		if (bytesRead == 0)
		{
//			printf("Read 0 bytes...jumping back to earlier in the function\n");
//			goto readAgain;	// For some reason, the server ends up reading 0 bytes once after each message
			printf("IFNamedPipe::readMessage(header): 0 bytes read\n");
			return IFOLDER_ERR_IPC_READ;
		}

		return bytesRead;	// Error occurred
	}

	messageSize = IFNamedPipe::calculateMessageSize(header.messageType);
	if (messageSize < sizeof(iFolderMessageHeader))
		return IFOLDER_ERR_IPC_INVALID_MESSAGE;
	
	extraMessageSize = messageSize - sizeof(iFolderMessageHeader);
	
	message = malloc(messageSize);
	if (!message)
	{
		// @todo Determine whether we need to read off the remaining bytes before returning the error value
		return IFOLDER_ERR_OUT_OF_MEMORY;
	}
	
	// Copy the iFolderMessageHeader into the message buffer
	memcpy(message, &header, sizeof(iFolderMessageHeader));
	
	if (extraMessageSize > 0)
	{
		// Read the remaining bytes directly into the message buffer
		bytesRead = readPipe((unsigned char *)message + sizeof(iFolderMessageHeader), extraMessageSize);
		if (bytesRead <= 0)
		{
			free(message);
			if (bytesRead == 0)
			{
				printf("IFNamedPipe::readMessage(extra message): 0 bytes read\n");
				return IFOLDER_ERR_IPC_READ;
			}

			return bytesRead;	// Error occurred
		}
	}
	
	*messageTypeReturn = header.messageType;
	*messageReturn = message;
	
	return IFOLDER_SUCCESS;
}

int
IFNamedPipe::readPipe(void *buffer, size_t bytesToRead)
{
	ssize_t bytesRead;
	
	if (myFileDescriptor == -1)
		return IFOLDER_ERR_IPC_INVALID;
	
	bytesRead = read(myFileDescriptor, buffer, bytesToRead);
	if (bytesRead == -1)
	{
		perror(qPrintable(myPathName));
		return IFOLDER_ERR_IPC_READ; // @todo Read errno for a non-generic error
	}
	
	return (int) bytesRead;
}

IFNamedPipe *
IFNamedPipe::createServerNamedPipeForWriting()
{
	char serverNamedPipePath[NAMED_PIPE_PATH_MAX];
	IFNamedPipe *namedPipe;

	sprintf(serverNamedPipePath, IFOLDER_SERVER_NAMED_PIPE, "boyd");	// @todo Determine the user name programmatically
	
	namedPipe = new IFNamedPipe(QString(serverNamedPipePath), WriteOnly);
	
	return namedPipe;
}

IFNamedPipe *
IFNamedPipe::createNamedPipeByPid(PermissionType permissionType)
{
	pid_t mypid;

	IFNamedPipe *namedPipe;
	char namedPipePath[NAMED_PIPE_PATH_MAX];
	
	printf("IFNamedPipe::createNamedPipeByPid()\n");
	
	mypid = getpid();
	sprintf(namedPipePath, IFOLDER_CLIENT_NAMED_PIPE, mypid);

	namedPipe = new IFNamedPipe(QString(namedPipePath), permissionType);
	
	return namedPipe;	
}

IFNamedPipe *
IFNamedPipe::createUniqueNamedPipe(PermissionType permissionType)
{
	IFNamedPipe *namedPipe;
	char namedPipePath[NAMED_PIPE_PATH_MAX];

	printf("IFNamedPipe::createUniqueNamedPipe()\n");
//	QString namedPipePath;
	QUuid uuid;
	
	uuid = QUuid::createUuid();

	QString trimmedUuid = uuid.toString();
	trimmedUuid = trimmedUuid.right(trimmedUuid.length() - 1);
	trimmedUuid = trimmedUuid.left(trimmedUuid.length() - 1);

	sprintf(namedPipePath, IFOLDER_GENERIC_NAMED_PIPE, qPrintable(trimmedUuid));
	
	namedPipe = new IFNamedPipe(QString(namedPipePath), permissionType);
	
	return namedPipe;
}

int
IFNamedPipe::reset()
{
	int err;

	printf("IFNamedPipe::reset()\n");

	err = closePipe();
	if (err != IFOLDER_SUCCESS)
	{
		printf("IFNamedPipe::reset(): could NOT close pipe: %d\n", err);
		return err;
	}
	
	err = openPipe(true, true);
	if (err != IFOLDER_SUCCESS)
	{
		printf("IFNamedPipe::reset(): could NOT reopen pipe: %d\n", err);
		return err;
	}
	
	return IFOLDER_SUCCESS;
}

int
IFNamedPipe::create()
{
	int err;

printf("IFNamedPipe::create(): %s\n", qPrintable(myPathName));

	err = unlink(qPrintable(myPathName));
	if (err != 0)
	{
		printf("unlink() didn't work\n");//: %d\n", errno);
		// @todo Add in a complete listing of errors instead of a generic one
		/*
		switch(errno)
		{
			case ENOTDIR:
				break;
			case ENAMETOOLONG:
				break;
			case ENOENT:
				break;
			case EACCES:
				break;
			case ELOOP:
				break;
			case EPERM:
				break;
			case EBUSY:
				break;
			case EIO:
				break;
			case EROFS:
				break;
			case EFAULT:
				break;
			default
				break;
		}
		*/
		
//		return IFOLDER_ERR_IPC_CREATE;
	}
	
	err = mkfifo(qPrintable(myPathName), 0700);
	if (err != 0)
	{
		printf("mkfifo() didn't work\n");//: %d\n", errno);
		perror(qPrintable(myPathName));
		
		// @todo Once again, change this error to not be a generic one
		return IFOLDER_ERR_IPC_CREATE;
	}
	
	return IFOLDER_SUCCESS;
}

int
IFNamedPipe::calculateMessageSize(int messageType)
{
	int bytesToRead = 0;
	switch (messageType)
	{
		case IFOLDER_MSG_REGISTER_CLIENT_REQUEST:
			bytesToRead = sizeof(iFolderMessageRegisterClientRequest);
			break;
		case IFOLDER_MSG_UNREGISTER_CLIENT_REQUEST:
			bytesToRead = sizeof(iFolderSimpleMessageRequest);
			break;
		case IFOLDER_MSG_REGISTER_CLIENT_RESPONSE:
		case IFOLDER_MSG_UNREGISTER_CLIENT_RESPONSE:
			bytesToRead = sizeof(iFolderSimpleMessageResponse);
			break;
		case IFOLDER_MSG_DOMAIN_ADD_REQUEST:
			bytesToRead = sizeof(iFolderMessageDomainAddRequest);
			break;
		case IFOLDER_MSG_DOMAIN_ADD_RESPONSE:
			bytesToRead = sizeof(iFolderMessageDomainAddResponse);
			break;
		case IFOLDER_MSG_DOMAIN_REMOVE_REQUEST:
			bytesToRead = sizeof(iFolderMessageDomainRemoveRequest);
			break;
		case IFOLDER_MSG_DOMAIN_REMOVE_RESPONSE:
			bytesToRead = sizeof(iFolderMessageDomainRemoveResponse);
			break;
		default:
			break;
	}
	
	return bytesToRead;
}
