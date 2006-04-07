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

#include "ifolder-errors.h"
#include "IFMessages.h"

#include "IFNamedPipe.h"

iFolderNamedPipe::iFolderNamedPipe(QString pathName, PermissionType permissionType) :
	myPermissionType(ReadOnly), myFileDescriptor(-1)
{
}

iFolderNamedPipe::~iFolderNamedPipe()
{
	int err;

	if (myFileDescriptor != -1)
	{
		err = close(myFileDescriptor);
		if (err != IFOLDER_SUCCESS)
		{
			// FIXME: Figure a way to log this error to a log file
		}
	}
}

int
iFolderNamedPipe::openPipe(bool createIfNeeded)
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
		
		err = open(qPrintable(myPathName), O_WRONLY);
		if (err == -1)
		{
			printf("iFolderNamedPipe::openPipe(): open() returned err\n");//%d\n", errno);
			return IFOLDER_ERROR_IPC_CREATE;	// FIXME: Read errno and return a less generic error
		}
		
		myFileDescriptor = err;
	}
	else
	{
		if (createIfNeeded)
			err = create();	// Create a pipe for reading

		err = open(qPrintable(myPathName), O_RDONLY);
		if (err == -1)
			return IFOLDER_ERROR_IPC_CREATE;	// FIXME: Read errno and return a less generic error
		
		myFileDescriptor = err;
	}
	
	return myFileDescriptor;
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
		return IFOLDER_ERROR_IPC_WRITE; // FIXME: Read errno for a non-generic error

	if (bytesWritten != messageSize)
		return IFOLDER_ERROR_IPC_WRITE;
	
	return IFOLDER_SUCCESS;
}

int
iFolderNamedPipe::readMessage(int *messageTypeReturn, void **messageReturn)
{
	int bytesRead;
	int messageSize;
	int extraMessageSize;
	void *message;

	iFolderMessageHeader header;
	
	bytesRead = readPipe(&header, sizeof(iFolderMessageHeader));
	if (bytesRead <= 0)
	{
		if (bytesRead == 0)
			return IFOLDER_ERROR_IPC_READ;

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
				return IFOLDER_ERROR_IPC_READ;

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
		return IFOLDER_ERROR_IPC_READ; // FIXME: Read errno for a non-generic error
	
	return (int) bytesRead;
}

int
iFolderNamedPipe::create()
{
	int err;

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
		
		return IFOLDER_ERROR_IPC_CREATE;
	}
	
	err = mkfifo(qPrintable(myPathName), 0700);
	if (err != 0)
	{
		printf("mkfifo() didn't work\n");//: %d\n", errno);
		
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
