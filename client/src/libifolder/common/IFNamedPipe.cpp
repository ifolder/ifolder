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

iFolderNamedPipe::iFolderNamedPipe(QString pathName, PermissionType permissionType) :
	myPathName(pathName), myPermissionType(permissionType), myFileDescriptor(-1)
{
	printf("iFolderNamedPipe::iFolderNamedPipe(%s, %d)\n", qPrintable(pathName), permissionType);
}

iFolderNamedPipe::~iFolderNamedPipe()
{
	int err;
printf("iFolderNamedPipe::~iFolderNamedPipe(): %s\n", qPrintable(myPathName));
	if (myFileDescriptor != -1)
	{
		err = close(myFileDescriptor);
		if (err != IFOLDER_SUCCESS)
		{
			// FIXME: Figure a way to log this error to a log file
printf("iFolderNamedPipe::~iFolderNamedPipe(): Error closing pipe\n");
		}
	}
}

QString
iFolderNamedPipe::path()
{
	return myPathName;
}

int
iFolderNamedPipe::openPipe(bool block, bool createIfNeeded)
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
		
printf("iFolderNamedPipe::openingPipe in write only mode\n");
		if (block)
			err = open(qPrintable(myPathName), O_WRONLY);
		else
			err = open(qPrintable(myPathName), O_WRONLY | O_NONBLOCK);
		if (err == -1)
		{
			printf("iFolderNamedPipe::openPipe(): open() returned errno: %d\n", errno);
			perror(qPrintable(myPathName));
			return IFOLDER_ERROR_IPC_CREATE;	// FIXME: Read errno and return a less generic error
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
		
printf("iFolderNamedPipe::openingPipe in read only mode\n");
		err = open(qPrintable(myPathName), O_RDONLY);
		if (err == -1)
		{
			printf("iFolderNamedPipe::openPipe(): open() returned errno: %d\n", errno);
			perror(qPrintable(myPathName));
			return IFOLDER_ERROR_IPC_CREATE;	// FIXME: Read errno and return a less generic error
		}
		
		myFileDescriptor = err;
	}
	
	return IFOLDER_SUCCESS;
}

int
iFolderNamedPipe::closePipe()
{
	int err;

	if (myFileDescriptor == -1)
		return IFOLDER_SUCCESS;
	
	err = close(myFileDescriptor);
	if (err != 0)
		return IFOLDER_ERROR_IPC_CLOSE;	// FIXME: Read errno and return a less generic error
	
	if (myPermissionType == ReadOnly)
	{
		// Delete the file
		err = unlink(qPrintable(myPathName));
		if (err != 0)
			printf("unlink() didn't work in iFolderNamedPipe::close()\n");//: %d\n", errno);
	}
	
	return IFOLDER_SUCCESS;
}

int
iFolderNamedPipe::writeMessage(const void *message, size_t messageSize)
{
	ssize_t bytesWritten;
	
	if (myFileDescriptor == -1)
		return IFOLDER_ERROR_IPC_INVALID;
	
	bytesWritten = write(myFileDescriptor, message, messageSize);
	if (bytesWritten == -1)
	{
		printf("write returned -1\n");
		perror(qPrintable(myPathName));
		return IFOLDER_ERROR_IPC_WRITE; // FIXME: Read errno for a non-generic error
	}

	if (bytesWritten != messageSize)
	{
		printf("Entire message not sent!\n");
		return IFOLDER_ERROR_IPC_WRITE;
	}
	
	return IFOLDER_SUCCESS;
}

int
iFolderNamedPipe::readMessage(uint *messageTypeReturn, void **messageReturn)
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
			printf("iFolderNamedPipe::readMessage(header): 0 bytes read\n");
			return IFOLDER_ERROR_IPC_READ;
		}

		return bytesRead;	// Error occurred
	}

	messageSize = iFolderNamedPipe::calculateMessageSize(header.messageType);
	if (messageSize < sizeof(iFolderMessageHeader))
		return IFOLDER_ERROR_IPC_INVALID_MESSAGE;
	
	extraMessageSize = messageSize - sizeof(iFolderMessageHeader);
	
	message = malloc(messageSize);
	if (!message)
	{
		// FIXME: Determine whether we need to read off the remaining bytes before returning the error value
		return IFOLDER_ERROR_OUT_OF_MEMORY;
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
				printf("iFolderNamedPipe::readMessage(extra message): 0 bytes read\n");
				return IFOLDER_ERROR_IPC_READ;
			}

			return bytesRead;	// Error occurred
		}
	}
	
	*messageTypeReturn = header.messageType;
	*messageReturn = message;
	
	return IFOLDER_SUCCESS;
}

int
iFolderNamedPipe::readPipe(void *buffer, size_t bytesToRead)
{
	ssize_t bytesRead;
	
	if (myFileDescriptor == -1)
		return IFOLDER_ERROR_IPC_INVALID;
	
	bytesRead = read(myFileDescriptor, buffer, bytesToRead);
	if (bytesRead == -1)
	{
		perror(qPrintable(myPathName));
		return IFOLDER_ERROR_IPC_READ; // FIXME: Read errno for a non-generic error
	}
	
	return (int) bytesRead;
}

iFolderNamedPipe *
iFolderNamedPipe::createServerNamedPipeForWriting()
{
	char serverNamedPipePath[NAMED_PIPE_PATH_MAX];
	iFolderNamedPipe *namedPipe;

	sprintf(serverNamedPipePath, IFOLDER_SERVER_NAMED_PIPE, "boyd");	// FIXME: Determine the user name programmatically
	
	namedPipe = new iFolderNamedPipe(QString(serverNamedPipePath), WriteOnly);
	
	return namedPipe;
}

iFolderNamedPipe *
iFolderNamedPipe::createNamedPipeByPid(PermissionType permissionType)
{
	pid_t mypid;

	iFolderNamedPipe *namedPipe;
	char namedPipePath[NAMED_PIPE_PATH_MAX];
	
	printf("iFolderNamedPipe::createNamedPipeByPid()\n");
	
	mypid = getpid();
	sprintf(namedPipePath, IFOLDER_CLIENT_NAMED_PIPE, mypid);

	namedPipe = new iFolderNamedPipe(QString(namedPipePath), permissionType);
	
	return namedPipe;	
}

iFolderNamedPipe *
iFolderNamedPipe::createUniqueNamedPipe(PermissionType permissionType)
{
	iFolderNamedPipe *namedPipe;
	char namedPipePath[NAMED_PIPE_PATH_MAX];

	printf("iFolderNamedPipe::createUniqueNamedPipe()\n");
//	QString namedPipePath;
	QUuid uuid;
	
	uuid = QUuid::createUuid();

	QString trimmedUuid = uuid.toString();
	trimmedUuid = trimmedUuid.right(trimmedUuid.length() - 1);
	trimmedUuid = trimmedUuid.left(trimmedUuid.length() - 1);

	sprintf(namedPipePath, IFOLDER_GENERIC_NAMED_PIPE, qPrintable(trimmedUuid));
	
	namedPipe = new iFolderNamedPipe(QString(namedPipePath), permissionType);
	
	return namedPipe;
}

int
iFolderNamedPipe::reset()
{
	int err;

	printf("iFolderNamedPipe::reset()\n");

	err = closePipe();
	if (err != IFOLDER_SUCCESS)
	{
		printf("iFolderNamedPipe::reset(): could NOT close pipe: %d\n", err);
		return err;
	}
	
	err = openPipe(true, true);
	if (err != IFOLDER_SUCCESS)
	{
		printf("iFolderNamedPipe::reset(): could NOT reopen pipe: %d\n", err);
		return err;
	}
	
	return IFOLDER_SUCCESS;
}

int
iFolderNamedPipe::create()
{
	int err;

printf("iFolderNamedPipe::create(): %s\n", qPrintable(myPathName));

	err = unlink(qPrintable(myPathName));
	if (err != 0)
	{
		printf("unlink() didn't work\n");//: %d\n", errno);
		// FIXME: Add in a complete listing of errors instead of a generic one
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
		
//		return IFOLDER_ERROR_IPC_CREATE;
	}
	
	err = mkfifo(qPrintable(myPathName), 0700);
	if (err != 0)
	{
		printf("mkfifo() didn't work\n");//: %d\n", errno);
		perror(qPrintable(myPathName));
		
		// FIXME: Once again, change this error to not be a generic one
		return IFOLDER_ERROR_IPC_CREATE;
	}
	
	return IFOLDER_SUCCESS;
}

int
iFolderNamedPipe::calculateMessageSize(int messageType)
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
		default:
			break;
	}
	
	return bytesToRead;
}
