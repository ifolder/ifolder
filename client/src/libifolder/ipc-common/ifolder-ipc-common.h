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

#ifndef _IFOLDER_IPC_COMMON_H
#define _IFOLDER_IPC_COMMON_H 1

#include <unistd.h>
#include <stdlib.h>
#include <stdio.h>
#include <fcntl.h>
#include <limits.h>
#include <sys/types.h>
#include <sys/stat.h>

/* FIXME: Determine if we need to put these files elsewhere (especially in Windows) */
#define IPC_SERVER_PIPE_STR		"/tmp/ecs_%d_server_pipe"
#define IPC_CLIENT_PIPE_STR		"/tmp/ecs_%d_pipe"

typedef struct
{
	pid_t			client_pid;
	uint			request_id;
	uint			response_id;
	uint			data_size;
} ipc_message_header;

typedef struct
{
	ipc_message_header	message_header;
	void				*data;
} ipc_message;



int ipc_server_init(void);
int ipc_server_close(void);
int ipc_server_read_request(ipc_message **req_msg);
int ipc_server_start_response(const ipc_message *resp_msg);
int ipc_server_send_response(const ipc_message *resp_msg);
int ipc_server_end_response(void);

int ipc_client_init(void);
int ipc_client_close(void);
int ipc_client_send_request(ipc_message *req_msg);
int ipc_client_start_read_response(void);
int ipc_client_read_response(ipc_message **resp_msg);
int ipc_client_end_read_response(void);

int ipc_server_get_pipe_name(char *server_pipe_name);
int ipc_alloc_message(ipc_message **message);
int ipc_alloc_message_data(ipc_message *message);
int ipc_free_message(ipc_message *message);
int ipc_write_message(int fd, const ipc_message *message);

#endif
