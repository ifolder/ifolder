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

#include <stdlib.h>
#include <stdio.h>
#include <string.h>

int simias_get_domains(SimiasHandle hSimias, SimiasNodeList *hNodeList)
{
	int err_code;
	int rc = 0;
	struct _SimiasHandle *_hSimias = (struct _SimiasHandle *)hSimias;

	struct _ns1__GetDomains				message;
	struct _ns1__GetDomainsResponse		response;

	err_code = soap_call___ns1__GetDomains(
			&(_hSimias->soap),
			_hSimias->serviceURL,
			NULL, 
			&message,
			&response);

	if(err_code != SOAP_OK)
	{
		rc = err_code;
		*hNodeList = NULL;
	}
	else
	{
		struct _SimiasNodeList *nl = malloc(sizeof(struct _SimiasNodeList));
		nl->result = strdup(response.GetDomainsResult);
		*hNodeList = (SimiasNodeList *)nl;
	}

	// Free up the tmp resources with this soap call
	soap_end(&(_hSimias->soap));

	return rc;
}




