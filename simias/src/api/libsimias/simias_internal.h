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
 *  Author: Boyd Timothy <btimothy@novell.com>
 * 
 ***********************************************************************/
#ifndef _SIMIAS_INTERNAL_H
#define _SIMIAS_INTERNAL_H 1

#include <stdbool.h>
#include <stdio.h>
#include <libxml/parser.h>
#include <libxml/tree.h>

#include "simiaswebStub.h"

#define SIMIAS_SUCCESS 0
#define SIMIAS_ERROR_UNKNOWN				-1
#define SIMIAS_ERROR_NO_USER_PROFILE		-101
#define SIMIAS_ERROR_NO_CONFIG_FILE			-102
#define SIMIAS_ERROR_OPENING_CONFIG_FILE	-103
#define SIMIAS_ERROR_IN_SOAP_CALL			-104
#define SIMIAS_ERROR_OUT_OF_MEMORY			-105
#define SIMIAS_ERROR_NO_PASSWORD_FILE		-106
#define SIMIAS_ERROR_OPENING_PASSWORD_FILE	-107

#define SIMIAS_ERROR_INVALID_POINTER		-108
#define SIMIAS_ERROR_INDEX_OUT_OF_RANGE		-109

#define _SIMIAS_ERROR_INVALID_RESULTXML		-201

struct _SimiasHandle
{
	char	*username;
	char	*password;
	char	*serviceURL;
	bool	connected;
	struct	soap soap;
};

struct _SimiasNodeList
{
	int nodeCount;
	struct _SimiasNode **nodeArray;
};

struct _SimiasNode
{
	char *name;
	char *id;
	char *type;

	xmlNode	*node;
};

struct _SimiasProperty
{
	char *name;
	char *type;
	char *value;

	xmlNode *node;
};

/**
 * This function will return the full URL of where Simias is currently running.
 * 
 * param: url    The variable to be filled with the local service URL.
 *               This must be freed if the function is successful.
 * 
 * returns: Returns SIMIAS_SUCCESS (0) if successful or one of the errors listed
 *          above if it's not successful.  If successful, the memory used by
 *          url must be freed.
 */
int simias_get_local_service_url(char **url);

/**
 * This function gets the username and password needed to invoke calls to the
 * local Simias and iFolder WebServices.
 *
 * param: username (char[] that will be filled using sprintf)
 * param: password (char[] that will be filled using sprintf)
 *
 * returns: Returns SIMIAS_SUCCESS (0) if successful or one of the errors
 *          listed above if there's an error.
 */
int simias_get_web_service_credential(char *username, char *password);

/**
 * Returns ~/.local/share/simias in Linux/Mac and the correct location when
 * running in Win32.
 */
char *simias_get_user_profile_dir_path(char *dest_path);

/**
 * Gets the url of the local simias process
 */
int simias_get_local_service_url(char **url);

/**
 * parses out the local service url
 */
static char *parse_local_service_url(FILE *file);

int _simias_node_create(struct _SimiasNode **_hNode, xmlNode *node);

int _simias_nodelist_create(struct _SimiasNodeList **_hNodeList,
										char *resultXML);

int _simias_property_create(struct _SimiasProperty **_hProperty, xmlNode *node);
int _simias_property_get_count(struct _SimiasNode *_hNode);
int _simias_property_extract_property(struct _SimiasNode *_hNode, 
									  struct _SimiasProperty **_hProperty,
									  int index);

#endif	// _SIMIAS_INTERNAL_H
