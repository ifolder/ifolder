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

//  <Property name="NodeCreate" type="DateTime">632643783391302280</Property>

char *simias_property_get_name(SimiasProperty *hProp)
{
	struct _SimiasProperty *_hProp =
		(struct _SimiasProperty *)*hProp;

	if(_hProp == NULL)
		return NULL;
	
	if(_hProp->node == NULL)
		return NULL;

	return _hProp->name;
}

char *simias_property_get_value_as_string(SimiasProperty *hProp)
{
	struct _SimiasProperty *_hProp =
		(struct _SimiasProperty *)*hProp;

	if(_hProp == NULL)
		return NULL;
	
	if(_hProp->node == NULL)
		return NULL;

	return _hProp->value;
}

char *simias_property_get_type(SimiasProperty *hProp)
{
	struct _SimiasProperty *_hProp =
		(struct _SimiasProperty *)*hProp;

	if(_hProp == NULL)
		return NULL;
	
	if(_hProp->node == NULL)
		return NULL;

	return _hProp->type;
}


int simias_property_get_count(SimiasNode hNode)
{
	struct _SimiasNode *_hNode =
			(struct _SimiasNode *)hNode;

	if(_hNode == NULL)
		return SIMIAS_ERROR_INVALID_POINTER;

	return _simias_property_get_count(_hNode);
}


int simias_property_extract_property(SimiasNode hNode, 
									  SimiasProperty *hProp,
									  int index)
{
	int rc = 0;
	struct _SimiasProperty *_hProp;
	struct _SimiasNode *_hNode = (struct _SimiasNode *)hNode;

	if(_hNode == NULL)
		return SIMIAS_ERROR_INVALID_POINTER;

	rc = _simias_property_extract_property(_hNode, &_hProp, index);
	if(rc)
	{
		*hProp = NULL;
		return rc;
	}

	*hProp = _hProp;
	return rc;
}



int simias_property_free(SimiasProperty *hProp)
{
	struct _SimiasProperty *_hProp =
		(struct _SimiasProperty *)*hProp;

	if(_hProp != NULL)
	{
		if(_hProp->node != NULL)
		{
			xmlFreeNode(_hProp->node);
		}

		if(_hProp->name != NULL)
			xmlFree(_hProp->name);
		
		if(_hProp->value != NULL)
			xmlFree(_hProp->value);
		
		if(_hProp->type != NULL)
			xmlFree(_hProp->type);
			 
		free(_hProp);
	}

	*hProp = NULL;

	return 0;
}


/*******************************************************************
 * Internal functions
 ******************************************************************/
int _simias_property_get_count(struct _SimiasNode *_hNode)
{
	int nodeCounter = 0;
	xmlNode *cur_node;

	if(_hNode == NULL)
		return nodeCounter;

	if(_hNode->node == NULL)
		return nodeCounter;

	for(cur_node = _hNode->node->children; cur_node; cur_node = cur_node->next)
	{
		if (cur_node->type == XML_ELEMENT_NODE)
		{
			nodeCounter++;
		}
	}

	return nodeCounter;
}


int _simias_property_extract_property(struct _SimiasNode *_hNode, 
									  struct _SimiasProperty **_hProperty,
									  int index)
{
	int rc = 0;
	int nodeCounter = 0;
	xmlNode *cur_node;

	if(_hNode == NULL)
		return SIMIAS_ERROR_INVALID_POINTER;

	if(_hNode->node == NULL)
		return SIMIAS_ERROR_INVALID_POINTER;

	for(cur_node = _hNode->node->children; cur_node; cur_node = cur_node->next)
	{
		if (cur_node->type == XML_ELEMENT_NODE)
		{
			if(nodeCounter == index)
			{
				rc = _simias_property_create(_hProperty,
											 cur_node);
				return rc;
			}
			nodeCounter++;
		}
	}

	return SIMIAS_ERROR_INDEX_OUT_OF_RANGE;

}


int _simias_property_create(struct _SimiasProperty **_hProperty, xmlNode *node)
{
	*_hProperty = malloc(sizeof(struct _SimiasProperty));
	if(*_hProperty == NULL)
		return SIMIAS_ERROR_OUT_OF_MEMORY;
		
	memset(*_hProperty, 0, sizeof(struct _SimiasProperty));
	
	(*_hProperty)->node = xmlCopyNode(node, 1);

	(*_hProperty)->name = 
			(char *)xmlGetProp((*_hProperty)->node, (const xmlChar *)"name");
	(*_hProperty)->type = 
			(char *)xmlGetProp((*_hProperty)->node, (const xmlChar *)"type");
	(*_hProperty)->value = 
			(char *)xmlNodeGetContent((*_hProperty)->node);
	
	return 0;
}
