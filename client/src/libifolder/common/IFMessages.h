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

#ifndef _IFOLDER_MESSAGES_H_
#define _IFOLDER_MESSAGES_H_

#ifndef PATH_MAX
#define PATH_MAX 4095
#endif

#define NAMED_PIPE_PATH_MAX PATH_MAX + 1

typedef struct
{
	uint		senderPID;
	uint		requestID;
	uint		responseID;
	uint		messageType;
} iFolderMessageHeader;

//! This is used for simple messages which do not contain any data.
/**
 * An example of a message which does not need any data is a fuction that has
 * "void" as the parameter list.
 */
typedef struct
{
	iFolderMessageHeader header;
} iFolderSimpleMessageRequest;

//! This is used for messages which do not return any data
/**
 * This is a simple response message which contains a return code.
 */
typedef struct
{
	iFolderMessageHeader header;
	int returnCode;
} iFolderSimpleMessageResponse;

#define IFOLDER_MSG_REGISTER_CLIENT_REQUEST			1
#define IFOLDER_MSG_REGISTER_CLIENT_RESPONSE			2
typedef struct
{
	iFolderMessageHeader header;
	char client_named_pipe[NAMED_PIPE_PATH_MAX];
} iFolderMessageRegisterClientRequest;
typedef iFolderSimpleMessageResponse iFolderMessageRegisterClientResponse;

#define IFOLDER_MSG_UNREGISTER_CLIENT_REQUEST			3
#define IFOLDER_MSG_UNREGISTER_CLIENT_RESPONSE			4
typedef iFolderSimpleMessageRequest iFolderMessageUnregisterClientRequest;
typedef iFolderSimpleMessageResponse iFolderMessageUnregisterClientResponse;

#endif /*_IFOLDER_MESSAGES_H_*/

