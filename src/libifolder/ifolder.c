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
#include <string.h>
#include <stdio.h>
#include <unistd.h>
#include <time.h>

#include <simias.h>

#include "iFolderStub.h"
#include "iFolder.nsmap"

#include "ifolder.h"

/* Global Variables */
static char *the_soap_url = NULL;

static void init_gsoap (struct soap *p_soap);
static void cleanup_gsoap (struct soap *p_soap);
static char *get_soap_url(bool reread_config);

/**         
 * gSOAP
 */
static void init_gsoap (struct soap *p_soap)
{       
	/* Initialize gSOAP */
	soap_init (p_soap);
	soap_set_namespaces (p_soap, iFolder_namespaces);
}   

static void cleanup_gsoap (struct soap *p_soap)
{           
	/* Cleanup gSOAP */
	soap_end (p_soap);
}   


/**         
 * Calls to iFolder via GSoap
 */
bool is_ifolder_running () 
{
	struct soap soap;
	bool isRunning = false;
	int err_code;
	char *soap_url;
	char username[512];
	char password[1024];

	struct _ns1__Ping ns1__Ping;
	struct _ns1__PingResponse ns1__PingResponse;
	
	soap_url = get_soap_url(true);
	if (!soap_url) {
		return false;
	}

	init_gsoap (&soap);
	if (simias_get_web_service_credential(username, password) == SIMIAS_SUCCESS) {
		soap.userid = username;
		soap.passwd = password;
	}
	err_code = soap_call___ns1__Ping (&soap,
			soap_url, //http://<host>:<port>/simias10[/<username>]/iFolder.asmx
			NULL,
			&ns1__Ping,
			&ns1__PingResponse);

	if (err_code == SOAP_OK)
	{
		isRunning = true; 
	}

	cleanup_gsoap (&soap);

	return isRunning;
}

static char *
get_soap_url(bool reread_config)
{
	char *url;
	char ifolder_domain_url[512];
	int err;
	
	if (!reread_config && the_soap_url) {
		return the_soap_url;
	}

	err = simias_get_local_service_url(&url);
	if (err == SIMIAS_SUCCESS) {
		sprintf(ifolder_domain_url, "%s/iFolder.asmx", url);
		free(url);
		if (the_soap_url)
			free(the_soap_url);
		the_soap_url = strdup(ifolder_domain_url);
		/* FIXME: Figure out who and when this should ever be freed */
	} else {
		printf("simias_get_local_service_url() returned: %d\n", err);
	}
	
	return the_soap_url;
}
