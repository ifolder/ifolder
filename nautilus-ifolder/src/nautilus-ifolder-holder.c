/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Boyd Timothy <btimothy@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/
#include "nautilus-ifolder-holder.h"

///<summary>
/// Create a new iFolder holder
///</summary>
///<param name="id">ID of the new ifolder holder</param>
///<param name="domain_id">ID of the domain to which ifolder belongs</param>
///<param name="unmanaged_path">Path of new ifolder selected</param>
///<param name="name">Name of the folder</param>
///<returns>Password</returns>
iFolderHolder *
ifolder_holder_new (char *id, char *domain_id,
				  char *unmanaged_path, char *name)
{
	iFolderHolder *holder;
	
	if (id == NULL || domain_id == NULL || unmanaged_path == NULL ||
		name == NULL)
		return NULL;
	
	/* Malloc and set contents to 0 */
	holder = g_malloc0 (sizeof(iFolderHolder));
	
	if (!holder) return NULL; /* out of memory error */
	
	holder->id				= strdup (id);
	holder->domain_id		= strdup (domain_id);
	holder->unmanaged_path	= strdup (unmanaged_path);
	holder->name			= strdup (name);
	
	return holder;
}

///<summary>
/// Free the ifolder holder
///</summary>
///<param name="p_holder">Double pointer to iFolderHolder</param>
void
ifolder_holder_free (iFolderHolder **p_holder)
{
	iFolderHolder *holder = (iFolderHolder *)*p_holder;
	if (holder->id)
		free (holder->id);
	if (holder->domain_id)
		free (holder->domain_id);
	if (holder->unmanaged_path)
		free (holder->unmanaged_path);
	if (holder->name)
		free (holder->name);
	
	g_free (holder);
	
	*p_holder = NULL;
}
