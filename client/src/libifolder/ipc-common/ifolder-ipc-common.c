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
 *  Author(s):
 *		Calvin Gaisford <cgaisford@novell.com>
 *		Boyd Timothy <btimothy@novell.com>
 *
 ***********************************************************************/

#include "ifolder-ipc-common.h"
#include "errors.h"

int
ipc_server_get_pipe_name(char *server_pipe_name)
{
	/* FIXME: Make the port dynamic or configurable, or both */
	snprintf(server_pipe_name, PATH_MAX, IPC_SERVER_PIPE_STR, 4747);
}

int
ipc_alloc_message(ipc_message **message)
{
	ipc_message *tmp_msg;
	
	tmp_msg = malloc(sizeof(ipc_message));
	if (tmp_msg == NULL)
		return IFOLDER_OUT_OF_MEMORY;
	
	memset(tmp_msg, 0, sizeof(ipc_message));
	
	*message = tmp_msg;
	
	return IFOLDER_SUCCESS;
}

int
ipc_alloc_message_data(ipc_message *message)
{
	message->data = malloc(message->message_header.data_size);
	if (message->data == NULL)
		return IFOLDER_OUT_OF_MEMORY;
	
	return IFOLDER_SUCCESS;
}

int
ipc_free_message(ipc_message *message)
{
	if (message == NULL)
		return IFOLDER_NULL_PARAMETER;
	
	if (message->data != NULL)
		free(message->data);
	free(message);
	
	return IFOLDER_SUCCESS;
}

int
ipc_write_message(int fd, const ipc_message *message)
{
	int write_bytes;
	
	if (fd == -1)
		return (IFOLDER_ERROR_PIPE_INVALID);
	
	// Send the header portion of the message
	write_bytes = write(fd, message, sizeof(ipc_message_header));
	if (write_bytes != sizeof(ecs_message_header))
		return IFOLDER_ERROR_PIPE_WRITE;
	
	// Send the data portion of the message
	if (message->message_header.data_size > 0)
	{
		write_bytes = write(fd, message->data, message->message_header.data_size);
		if (write_bytes != message->message_header.data_size)
			return IFOLDER_ERROR_PIPE_WRITE;
	}
	
	return IFOLDER_SUCCESS;
}

#endif
