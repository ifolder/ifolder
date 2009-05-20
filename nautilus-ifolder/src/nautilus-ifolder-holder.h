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
