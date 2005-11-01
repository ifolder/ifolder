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


int simias_get_nodes(SimiasHandle hSimias, 
					SimiasNodeList *hNodeList, 
					const char *collectionID)
{
	int err_code;
	int rc = 0;
	struct _SimiasHandle *_hSimias = (struct _SimiasHandle *)hSimias;

	struct _ns1__GetNodes				message;
	struct _ns1__GetNodesResponse		response;

	// check for a null in type
	if(collectionID == NULL)
		return SIMIAS_ERROR_INVALID_POINTER;

	message.collectionID = collectionID;
	message.type = "";

	err_code = soap_call___ns1__GetNodes(
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
								response.GetNodesResult);
		*hNodeList = (SimiasNodeList *)nl;
	}

	// Free up the tmp resources with this soap call
	soap_end(&(_hSimias->soap));

	return rc;
}




int simias_get_nodes_by_type(SimiasHandle hSimias, 
					SimiasNodeList *hNodeList, 
					const char *collectionID,
					const char *type)
{
	int err_code;
	int rc = 0;
	struct _SimiasHandle *_hSimias = (struct _SimiasHandle *)hSimias;

	struct _ns1__GetNodes				message;
	struct _ns1__GetNodesResponse		response;

	// check for a null in type
	if(collectionID == NULL)
		return SIMIAS_ERROR_INVALID_POINTER;

	if(type == NULL)
		return SIMIAS_ERROR_INVALID_POINTER;

	message.collectionID = collectionID;
	message.type = type;

	err_code = soap_call___ns1__GetNodes(
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
								response.GetNodesResult);
		*hNodeList = (SimiasNodeList *)nl;
	}

	// Free up the tmp resources with this soap call
	soap_end(&(_hSimias->soap));

	return rc;
}



char *simias_node_get_name(SimiasNode hNode)
{
	struct _SimiasNode *_hNode =
		(struct _SimiasNode *)hNode;

	if(_hNode == NULL)
		return NULL;
	
	if(_hNode->node == NULL)
		return NULL;

	return _hNode->name;
}

char *simias_node_get_id(SimiasNode hNode)
{
	struct _SimiasNode *_hNode =
		(struct _SimiasNode *)hNode;

	if(_hNode == NULL)
		return NULL;
	
	if(_hNode->node == NULL)
		return NULL;

	return _hNode->id;
}

char *simias_node_get_type(SimiasNode hNode)
{
	struct _SimiasNode *_hNode =
		(struct _SimiasNode *)hNode;

	if(_hNode == NULL)
		return NULL;
	
	if(_hNode->node == NULL)
		return NULL;

	return _hNode->type;
}



int simias_node_free(SimiasNode *hNode)
{
	struct _SimiasNode *_hNode =
		(struct _SimiasNode *)*hNode;

	if(_hNode != NULL)
	{
		if(_hNode->node != NULL)
		{
			xmlFreeNode(_hNode->node);
		}
		
		if(_hNode->name != NULL)
			xmlFree(_hNode->name);
		
		if(_hNode->id != NULL)
			xmlFree(_hNode->id);
		
		if(_hNode->type != NULL)
			xmlFree(_hNode->type);
			 
		free(_hNode);
	}

	*hNode = NULL;

	return 0;
}


/*******************************************************************
 * Internal functions
 ******************************************************************/

int _simias_node_create(struct _SimiasNode **_hNode, xmlNode *node)
{
	*_hNode = malloc(sizeof(struct _SimiasNode));
	if(*_hNode == NULL)
		return SIMIAS_ERROR_OUT_OF_MEMORY;
		
	memset(*_hNode, 0, sizeof(struct _SimiasNode));
	
	(*_hNode)->node = xmlCopyNode(node, 1);

	(*_hNode)->name = 
			(char *)xmlGetProp((*_hNode)->node, (const xmlChar *)"name");
	(*_hNode)->id = 
			(char *)xmlGetProp((*_hNode)->node, (const xmlChar *)"id");
	(*_hNode)->type = 
			(char *)xmlGetProp((*_hNode)->node, (const xmlChar *)"type");
	
	return 0;
}
