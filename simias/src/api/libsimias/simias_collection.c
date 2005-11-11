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



int simias_create_collection( SimiasNode *hNode, 
							  const char *domainID, 
							  const char *type)
{


}





int simias_get_collections(SimiasHandle hSimias, SimiasNodeList *hNodeList)
{
	int err_code;
	int rc = 0;
	struct _SimiasHandle *_hSimias = (struct _SimiasHandle *)hSimias;

	struct _ns1__GetCollections				message;
	struct _ns1__GetCollectionsResponse		response;

	err_code = soap_call___ns1__GetCollections(
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
		struct _SimiasNodeList *nl;
		_simias_nodelist_create(&nl, response.GetCollectionsResult);
		*hNodeList = (SimiasNodeList *)nl;
	}

	// Free up the tmp resources with this soap call
	soap_end(&(_hSimias->soap));

	return rc;
}


int simias_get_collections_by_type(SimiasHandle hSimias, 
								   SimiasNodeList *hNodeList, 
								   const char *type)
{
	int err_code;
	int rc = 0;
	struct _SimiasHandle *_hSimias = (struct _SimiasHandle *)hSimias;

	struct _ns1__GetCollectionsByType				message;
	struct _ns1__GetCollectionsByTypeResponse		response;

	// check for a null in type
	if(type == NULL)
		return SIMIAS_ERROR_INVALID_POINTER;

	message.type = type;

	err_code = soap_call___ns1__GetCollectionsByType(
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
		struct _SimiasNodeList *nl;
		_simias_nodelist_create(&nl, response.GetCollectionsByTypeResult);
		*hNodeList = (SimiasNodeList *)nl;
	}

	// Free up the tmp resources with this soap call
	soap_end(&(_hSimias->soap));

	return rc;
}


int simias_get_collections_for_domain(SimiasHandle hSimias, 
									  SimiasNodeList *hNodeList, 
									  const char *domainID)
{
	int err_code;
	int rc = 0;
	struct _SimiasHandle *_hSimias = (struct _SimiasHandle *)hSimias;

	struct _ns1__GetCollectionsInDomain				message;
	struct _ns1__GetCollectionsInDomainResponse		response;

	// check for a null in type
	if(domainID == NULL)
		return SIMIAS_ERROR_INVALID_POINTER;

	message.domainID = domainID;

	err_code = soap_call___ns1__GetCollectionsInDomain(
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
		struct _SimiasNodeList *nl;
		_simias_nodelist_create(&nl, response.GetCollectionsInDomainResult);
		*hNodeList = (SimiasNodeList *)nl;
	}

	// Free up the tmp resources with this soap call
	soap_end(&(_hSimias->soap));

	return rc;
}


int simias_get_collections_for_domain_by_type(SimiasHandle hSimias, 
											  SimiasNodeList *hNodeList, 
											  const char *domainID, 
											  const char *type)
{
	int err_code;
	int rc = 0;
	struct _SimiasHandle *_hSimias = (struct _SimiasHandle *)hSimias;

	struct _ns1__GetCollectionsInDomainByType				message;
	struct _ns1__GetCollectionsInDomainByTypeResponse		response;

	// check for a null in type
	if(domainID == NULL)
		return SIMIAS_ERROR_INVALID_POINTER;

	if(type == NULL)
		return SIMIAS_ERROR_INVALID_POINTER;

	message.domainID = domainID;
	message.type = type;

	err_code = soap_call___ns1__GetCollectionsInDomainByType(
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
		struct _SimiasNodeList *nl;
		_simias_nodelist_create(&nl, 
								response.GetCollectionsInDomainByTypeResult);
		*hNodeList = (SimiasNodeList *)nl;
	}

	// Free up the tmp resources with this soap call
	soap_end(&(_hSimias->soap));

	return rc;
}
