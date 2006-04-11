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

#ifndef _IFOLDER_NAMED_PIPE_H_
#define _IFOLDER_NAMED_PIPE_H_

#include <QString>

//! Name for the IPC Server Named Pipe
/**
 * %s should be set to the current user name (login name)
 */
#define IFOLDER_SERVER_NAMED_PIPE	"/tmp/ifolder3_server_pipe_%s"

//! Name for the IPC Client's Named Pipe
/**
 * %d should be set to the process id of the client
 */
#define IFOLDER_CLIENT_NAMED_PIPE	"/tmp/ifolder3_client_pipe_%d"

//! Name for all other IPC Named Pipes
/**
 * You should use this name and generate a dynamic int for %d for pipes to
 * open anywhere else.
 */
#define IFOLDER_GENERIC_NAMED_PIPE "/tmp/ifolder3_pipe_%s"

//! An abstraction for named pipes
/**
 * Named pipes are used as the IPC mechanism in the iFolder Client and this
 * class provides an abstraction to the innards of how they work since they are
 * different on the three platforms we need to support.
 */
class iFolderNamedPipe
{
	public:
		//! Specifies the type of named pipe to use
		/**
		 * Used by the iFolderNamedPipe construtor to specify which kind of
		 * named pipe to use.  It is, by design, a one-way pipe.
		 */
		enum PermissionType
		{
			ReadOnly,	/*!< A one-way named pipe that can be read from */
			WriteOnly	/*!< A one-way named pipe that can be written to */
		};

		/**
		 * @param pathName the absolute path to the named pipe
		 */
		iFolderNamedPipe(QString pathName, PermissionType permissionType);
		virtual ~iFolderNamedPipe();
		
		//! Get the path of the named pipe
		QString path();
		
		//! Open up the named pipe for reading or writing
		/**
		 * Opens the named pipe in the mode specified by the constructor.
		 * 
		 * @param createIfNeeded if true, this will create the named pipe if
		 * it does not exist.  This is false by default.
		 * @return a file descriptor to read or write to or -1 if an error
		 * occurs.
		 */
		int openPipe(bool block, bool createIfNeeded);
		
		//! Force the named pipe to be closed
		/**
		 * This is called in the destructor if not called explicitly.
		 */
		int closePipe();
		
		//! Write a message to the named pipe
		/**
		 * @param the message to write
		 * @param messageSize the size of the buffer to message
		 * @return IFOLDER_SUCCESS if a message was successfully written or
		 * an error from ifolder-errors.h. If the named pipe hasn't been
		 * initialized (you haven't called open()), IFOLDER_ERROR_IPC_INVALID
		 * will be returned.
		 */
		int writeMessage(const void *message, size_t messageSize);
		
		//! Read a message from the named pipe
		/**
		 * @param messageTypeReturn the type of message that was read is passed
		 * back in this parameter.
		 * @param messageReturn the message is passed back in this parameter
		 * and must be freed when you are finished using it.
		 * @return IFOLDER_SUCCESS if a message was successfully read,
		 * or an error from ifolder-errors.h.
		 */
		int readMessage(uint *messageTypeReturn, void **messageReturn);

		static iFolderNamedPipe *createServerNamedPipeForWriting();
		//! Create a named pipe by process id
		static iFolderNamedPipe *createNamedPipeByPid(PermissionType permissionType);
		static iFolderNamedPipe *createUniqueNamedPipe(PermissionType permissionType);

		//! Close and reopen the pipe to clear out a bad message
		int reset();
		
		//! Used by open() to create a named pipe when needed.
		/**
		 * This can also be called explicitly to force a create.
		 */
		int create();
	private:
		
		//! Read data from the named pipe
		/**
		 * @param buffer the buffer to fill with data
		 * @param bytesToRead the number of bytes to read.  Make sure that
		 * buffer is large enough to store this many bytes.
		 * @return the number of bytes read or an ERROR message (negative
		 * number).  If the named pipe hasn't been initialized (you haven't
		 * called open()), IFOLDER_ERROR_IPC_INVALID will be returned.
		 */
		int readPipe(void *buffer, size_t bytesToRead);

		//! Used internally to calculate the size of a message
		/**
		 * After the iFolderMessageHeader portion is read from a pipe, there
		 * may be additional data to be read depending on the message type.
		 * 
		 * @param messageType read from the iFolderMessageHeader
		 * @return size of the specified messageType
		 */
		static int calculateMessageSize(int messageType);
		
		QString myPathName;
		PermissionType myPermissionType;
		int myFileDescriptor;
};

#endif /*_IFOLDER_NAMED_PIPE_H_*/
