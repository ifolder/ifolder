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
 ***********************************************************************/

#ifndef _IFOLDER_C_ENUMERATION_H_
#define _IFOLDER_C_ENUMERATION_H_

#include "ifolder-client.h"

#ifdef __cplusplus
extern "C"
{
#endif		/* __cplusplus */

/**
 * @file ifolder-enumeration.h
 * @brief Enumeration API
 */

/**
 * @name Enumeration API
 */
/*@{*/

//! Returns whether there are more items available in the enumeration.
/**
 * @param enumeration The iFolderEnumeration.
 * @return true if there are more items available in the enumeration.
 */
IFOLDER_API bool		ifolder_enumeration_has_more(const iFolderEnumeration enumeration);

//! Return the next item in the enumeration
/**
 * This increments the iterator to the next item in the enumeration.
 * 
 * @param enumeration The iFolderEnumeration.
 * @return This must be cast to the expected object type.  Returns NULL if
 * there was an error or if there are no more objects in the enumeration.
 */
IFOLDER_API void *	ifolder_enumeration_get_next(const iFolderEnumeration enumeration);

//! Reset the enumeration to point to the first item.
/**
 * @param enumeration The iFolderEnumeration.
 */
IFOLDER_API void		ifolder_enumeration_reset(iFolderEnumeration enumeration);

//! Free the memory used by an iFolderEnumration.
/**
 * The memory used by the objects contained inside of the enumeration is also
 * freed.
 * @param enumeration The iFolderEnumeration.
 */
IFOLDER_API void		ifolder_enumeration_free(iFolderEnumeration enumeration);

/*@}*/

#ifdef __cplusplus
}
#endif		/* __cplusplus */

#endif /*_IFOLDER_C_ENUMERATION_H_*/
