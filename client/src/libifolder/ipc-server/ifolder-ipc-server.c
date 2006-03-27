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
#include "ifolder-client-ipc-client.h"

static int server_fd = -1;
static char server_pipe_name[PATH_MAX + 1] = {'\0'};

static int client_fd = -1;
static char client_pipe_name[PATH_MAX + 1] = {'\0'};

int
ipc_server_init(void)
{
	ipc_server_get_pipe_name(server_pipe_name);
	
	unlink(server_pipe_name);
	if(mkfifo(server_pipe_name, 0700) == 1)
	{
		fprintf(stderr, "ipc_server_init() error, no FIFO created\n");
		return(ECS_ERR_PIPE);
	}
	
	return(ECS_SUCCESS);
}

int
ipc_server_close(void)
{
	close(server_fd);
	unlink(server_pipe_name);
	
	return IFOLDER_SUCCESS;
}

int
ipc_server_read_request(ipc_message **req_msg)
{
}

int
ipc_server_start_response(const ipc_message *resp_msg)
{
}

int
ipc_server_send_response(const ipc_message *resp_msg)
{
}

int
ipc_server_end_response(void)
{
}

