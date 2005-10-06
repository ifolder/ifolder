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


int __simias_nodelist_get_check_struct(SimiasNodeList hNodeList, 
										struct _SimiasNodeList **_hNodeList);

int simias_nodelist_get_node(SimiasNodeList hNodeList, 
								SimiasNode *hNode, 
								int index)
{
	struct _SimiasNodeList *_hNodeList;

	int rc = __simias_nodelist_get_check_struct(hNodeList, &_hNodeList);
	if(rc)
		return rc;

	if(index > _hNodeList->nodeCount)
		return SIMIAS_ERROR_INDEX_OUT_OF_RANGE;
		
	*hNode = (SimiasNode *)_hNodeList->nodeArray[index];

	return 0;
}


int simias_nodelist_extract_node(SimiasNodeList hNodeList, 
								SimiasNode *hNode, 
								int index)
{
	struct _SimiasNodeList *_hNodeList;

	int rc = __simias_nodelist_get_check_struct(hNodeList, &_hNodeList);
	if(rc)
		return rc;

	if(index > _hNodeList->nodeCount)
		return SIMIAS_ERROR_INDEX_OUT_OF_RANGE;
		
	*hNode = (SimiasNode *)_hNodeList->nodeArray[index];
	_hNodeList->nodeArray[index] = NULL;

	return 0;
}


int simias_nodelist_get_node_count(SimiasNodeList hNodeList, int *count)
{
	struct _SimiasNodeList *_hNodeList;

	*count = 0;

	int rc = __simias_nodelist_get_check_struct(hNodeList, &_hNodeList);
	if(rc)
		return rc;

	*count = _hNodeList->nodeCount;
		
	return 0;
}


int simias_nodelist_free(SimiasNodeList *hNodeList)
{
	struct _SimiasNodeList *_hNodeList =
		(struct _SimiasNodeList *)*hNodeList;

	if(_hNodeList != NULL)
	{
		if((_hNodeList->nodeCount > 0) && (_hNodeList->nodeArray != NULL) )
		{
			int counter;
			
			for(counter = 0; counter < _hNodeList->nodeCount; counter++)
			{
				struct _SimiasNode *sNode = _hNodeList->nodeArray[counter];
				if(sNode != NULL)
				{
					if(sNode->node != NULL)
						xmlFreeNode(sNode->node);
					
					free(sNode);
				}
			}
			
			free(_hNodeList->nodeArray);
		}

		free(_hNodeList);
	}

	*hNodeList = NULL;

	return 0;
}



/*******************************************************************
 * Internal functions
 ******************************************************************/
int __simias_nodelist_get_check_struct(SimiasNodeList hNodeList, 
										struct _SimiasNodeList **_hNodeList)
{
	*_hNodeList = (struct _SimiasNodeList *)hNodeList;	

	if(*_hNodeList == NULL)
		return SIMIAS_ERROR_INVALID_POINTER;

	if((*_hNodeList)->nodeArray == NULL)
		return SIMIAS_ERROR_INVALID_POINTER;

	return 0;
}


int _simias_nodelist_create(struct _SimiasNodeList **_hNodeList, 
										char *resultXML)
{
	int rc = 0;
	xmlNode *root_element, *cur_node;
	xmlDoc*	doc;
	int nodeCounter, nodeCount = 0;
	
	*_hNodeList = NULL;

	doc = xmlReadMemory(resultXML,strlen(resultXML),"result.xml",NULL, 0);

	if(doc == NULL)
	{
		rc = _SIMIAS_ERROR_INVALID_RESULTXML;
		return rc;
	}

	/*Get the root element node */
	root_element = xmlDocGetRootElement(doc);

	nodeCounter = 0;
	for(cur_node = root_element->children; cur_node; cur_node = cur_node->next)
	{
		if (cur_node->type == XML_ELEMENT_NODE)
		{
			nodeCounter++;
		}
	}

	nodeCount = nodeCounter;
	// Allocate the NodeList and alloc the number of nodes contained.
	struct _SimiasNodeList *nl = malloc(sizeof(struct _SimiasNodeList));
	if(nl == NULL)
	{
		rc = SIMIAS_ERROR_OUT_OF_MEMORY;
		xmlFreeDoc(doc);		
		return rc;
	}
	
	// Zero out structure
	memset(nl, 0, sizeof(struct _SimiasNodeList));
	
	if(nodeCount > 0)
	{
		// Allocate the nodelist structure and fill it out
		nl->nodeArray = malloc(nodeCount * sizeof(struct _SimiasNode *));
		if(nl->nodeArray == NULL)
		{
			rc = SIMIAS_ERROR_OUT_OF_MEMORY;
			xmlFreeDoc(doc);
			simias_nodelist_free((SimiasNodeList *)&nl);				
			return rc;
		}

		nl->nodeCount = nodeCount;

		nodeCounter = 0;
		for(cur_node = root_element->children; cur_node; cur_node = cur_node->next)
		{
			if (cur_node->type == XML_ELEMENT_NODE)
			{
				rc = _simias_node_create(&(nl->nodeArray[nodeCounter]),
												cur_node);	
				if(rc)
				{
					xmlFreeDoc(doc);
					simias_nodelist_free((SimiasNodeList *)&nl);				
					return rc;					
				}
				nodeCounter++;
			}
		}
	}

	*_hNodeList = nl;
	xmlFreeDoc(doc);

	return rc;
}


