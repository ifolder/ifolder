/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 * 
 ***********************************************************************/
#include "simias.h"
#include "simias_internal.h"

#include "simiaswebStub.h"
#include "simiasweb.nsmap"

#include <stdlib.h>
#include <stdio.h>
#include <string.h>

struct _SimiasHandle
{
	char	*username;
	char	*password;
	char	*serviceURL;
	bool	connected;
	struct	soap soap;
};

// internal prototypes
void simias_api_init_gsoap(struct _SimiasHandle *handle);
void simias_api_cleanup_gsoap(struct _SimiasHandle *handle);

/**
 * This function will connect
 * 
 */
int simias_handle_init_local(SimiasHandle *handle)
{
	struct _SimiasHandle *sHandle = NULL;
	char	*tmpServiceURL;

	sHandle = (struct _SimiasHandle *) malloc(sizeof(struct _SimiasHandle));
	if(sHandle != NULL)
	{
		bzero((char *) sHandle, sizeof(struct _SimiasHandle));

		sHandle->connected = false;

		simias_api_init_gsoap(sHandle);

		simias_get_local_service_url(&tmpServiceURL);
		if(tmpServiceURL != NULL)
		{
			sHandle->serviceURL = (char *) malloc(strlen(tmpServiceURL) + 25);
			sprintf(sHandle->serviceURL, "%s/SimiasAPI.asmx", tmpServiceURL);
			free(tmpServiceURL);
		}
	}

	*handle = sHandle;

	return 0;
}



/**
 * This function will disconnect and free all memory
 * 
 */
int simias_handle_delete(SimiasHandle *handle)
{
	struct _SimiasHandle *sHandle = (struct _SimiasHandle *)*handle;

	if(sHandle != NULL)
	{
		simias_api_cleanup_gsoap(sHandle);

		if(sHandle->serviceURL != NULL)
			free(sHandle->serviceURL);

		free(sHandle);

		*handle = NULL;
	}
	else
		return -1;

	return 0;
}




int simias_ping(SimiasHandle handle)
{
	int err_code;
	int rc = 0;
	struct _SimiasHandle *sHandle = (struct _SimiasHandle *)handle;

	struct _ns1__Ping 			pingMessage;
	struct _ns1__PingResponse	pingResponse;

	err_code = soap_call___ns1__Ping(
			&(sHandle->soap),
			sHandle->serviceURL,
			NULL, 
			&pingMessage,
			&pingResponse);

	if(err_code != SOAP_OK)
	{
		rc = err_code;
	}

	// Free up the tmp resources with this soap call
	soap_end(&(sHandle->soap));

	return rc;
}




void simias_api_init_gsoap(struct _SimiasHandle *handle)
{
	soap_init2(&(handle->soap), (SOAP_C_UTFSTRING | SOAP_IO_DEFAULT), 
				(SOAP_C_UTFSTRING | SOAP_IO_DEFAULT));

	soap_set_namespaces(&(handle->soap), simiasweb_namespaces);

	handle->username = malloc(1024);
	if(handle->username != NULL)
	{
		memset(handle->username, 0, 1024);
	}
	handle->password = malloc(1024);
	if(handle->password != NULL)
	{
		memset(handle->password, 0, 1024);
	}

	if( (handle->username != NULL) && (handle->password != NULL) )
	{
		if(simias_get_web_service_credential(handle->username, 
											handle->password) == 0)
		{
			handle->soap.userid = handle->username;
			handle->soap.passwd = handle->password;
		}
	}

	// Set the timeout for send and receive to 30 seconds
	handle->soap.recv_timeout = 30;
	handle->soap.send_timeout = 30;
}



void simias_api_cleanup_gsoap(struct _SimiasHandle *handle)
{
	if(handle != NULL)
	{
		if(handle->username != NULL)
		{
			free(handle->username);
			handle->username = NULL;
		}
		if(handle->password != NULL)
		{
			free(handle->password);
			handle->password = NULL;
		}

		soap_done(&(handle->soap));
	}
}
