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
#include "nautilus-ifolder-holder.h"


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
