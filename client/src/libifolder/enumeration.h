/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2006 Novell, Inc.
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
 *  Author(s): Boyd Timothy <btimothy@novell.com>
 *
 *  @file enumeration.h Enumeration API
 ***********************************************************************/
#ifndef _IFOLDER_ENUMERATION_H
/* @cond */
#define _IFOLDER_ENUMERATION_H 1
/* @endcond */

#include <stdbool.h>

/**
 * FIXME: Add iFolderEnumeration documentation.
 */
typedef void * iFolderEnumeration;

/**
 * FIXME: Add ifolder_enumeration_release documentation.
 */
int ifolder_enumeration_release(iFolderEnumeration *ifolder_enum);

/**
 * FIXME: Add ifolder_enumeration_has_more_elements documentation.
 */
int ifolder_enumeration_has_more_elements(iFolderEnumeration ifolder_enum, bool *ret_val);

/**
 * FIXME: Add ifolder_enumeration_next_element documentation.
 */
int ifolder_enumeration_next_element(iFolderEnumeration ifolder_enum, void **ret_val);

#endif
