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
#ifndef __NAUTILUS_IFOLDER_HOLDER_H_
#define __NAUTILUS_IFOLDER_HOLDER_H_

#include <glib.h>

G_BEGIN_DECLS

typedef struct {
	char	*id;
	char	*domain_id;
	char	*unmanaged_path;
	char	*name;
	
} iFolderHolder;

iFolderHolder *ifolder_holder_new (char *id, char *domain_id,
								 char *unmanaged_path, char *name);
void ifolder_holder_free (iFolderHolder **p_holder);

G_END_DECLS

#endif /* __NAUTILUS_IFOLDER_HOLDER_H_ */
