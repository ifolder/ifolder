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

#include "simiasweb.nsmap"

#include <stdlib.h>
#include <stdio.h>
#include <string.h>

// internal prototypes
void _simias_init_gsoap(struct _SimiasHandle *hSimias);


/**
 * This function will connect
 * 
 */
int simias_init_local(SimiasHandle *hSimias)
{
	struct _SimiasHandle *_hSimias = NULL;
	char	*tmpServiceURL = NULL;
	int		rc = SIMIAS_SUCCESS;

	 /* this initialize the library and check potential ABI mismatches
	 * between the version it was compiled for and the actual shared
	 * library used.*/
	LIBXML_TEST_VERSION

	_hSimias = (struct _SimiasHandle *) malloc(sizeof(struct _SimiasHandle));
	if(_hSimias != NULL)
	{

		bzero((char *) _hSimias, sizeof(struct _SimiasHandle));

		_hSimias->connected = false;

		_simias_init_gsoap(_hSimias);

		simias_get_local_service_url(&tmpServiceURL);
		if(tmpServiceURL != NULL)
		{
			_hSimias->serviceURL = (char *) malloc(strlen(tmpServiceURL) + 25);
			sprintf(_hSimias->serviceURL, "%s/SimiasAPI.asmx", tmpServiceURL);
			free(tmpServiceURL);
		}
	}

	*hSimias = _hSimias;

	return 0;
}



/**
 * This function will disconnect and free all memory
 * 
 */
int simias_free(SimiasHandle *hSimias)
{
	struct _SimiasHandle *_hSimias = (struct _SimiasHandle *)*hSimias;

    xmlCleanupParser();	

	if(_hSimias != NULL)
	{
		soap_done(&(_hSimias->soap));

		if(_hSimias->username != NULL)
		{
			free(_hSimias->username);
			_hSimias->username = NULL;
		}
		if(_hSimias->password != NULL)
		{
			free(_hSimias->password);
			_hSimias->password = NULL;
		}

		if(_hSimias->serviceURL != NULL)
			free(_hSimias->serviceURL);

		free(_hSimias);

		*hSimias = NULL;
	}
	else
		return -1;

	return 0;
}




int simias_ping(SimiasHandle hSimias)
{
	int err_code;
	int rc = 0;
	struct _SimiasHandle *_hSimias = (struct _SimiasHandle *)hSimias;

	struct _ns1__Ping 			pingMessage;
	struct _ns1__PingResponse	pingResponse;

	err_code = soap_call___ns1__Ping(
			&(_hSimias->soap),
			_hSimias->serviceURL,
			NULL, 
			&pingMessage,
			&pingResponse);

	if(err_code != SOAP_OK)
	{
		rc = err_code;
	}

	// Free up the tmp resources with this soap call
	soap_end(&(_hSimias->soap));

	return rc;
}




void _simias_init_gsoap(struct _SimiasHandle *_hSimias)
{
	soap_init2(&(_hSimias->soap), (SOAP_C_UTFSTRING | SOAP_IO_DEFAULT), 
				(SOAP_C_UTFSTRING | SOAP_IO_DEFAULT));

	soap_set_namespaces(&(_hSimias->soap), simiasweb_namespaces);

	_hSimias->username = malloc(1024);
	if(_hSimias->username != NULL)
	{
		memset(_hSimias->username, 0, 1024);
	}
	_hSimias->password = malloc(1024);
	if(_hSimias->password != NULL)
	{
		memset(_hSimias->password, 0, 1024);
	}

	if( (_hSimias->username != NULL) && (_hSimias->password != NULL) )
	{
		if(simias_get_web_service_credential(_hSimias->username, 
											_hSimias->password) == 0)
		{
			_hSimias->soap.userid = _hSimias->username;
			_hSimias->soap.passwd = _hSimias->password;
		}
	}

	// Set the timeout for send and receive to 30 seconds
	_hSimias->soap.recv_timeout = 30;
	_hSimias->soap.send_timeout = 30;
}


