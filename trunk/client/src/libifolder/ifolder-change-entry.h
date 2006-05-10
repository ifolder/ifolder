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

#ifndef _IFOLDER_C_CHANGE_ENTRY_H_
#define _IFOLDER_C_CHANGE_ENTRY_H_

#ifdef __cplusplus
extern "C"
{
#endif		/* __cplusplus */

#include <time.h>

/**
 * @file ifolder-change-entry.h
 * @brief Change Entry API
 */

typedef void *iFolderChangeEntry;

typedef enum
{
	IFOLDER_CHANGE_ENTRY_TYPE_ADD,		/*!< An item was added */
	IFOLDER_CHANGE_ENTRY_TYPE_MODIFY,	/*!< An item was modified */
	IFOLDER_CHANGE_ENTRY_TYPE_DELETE,	/*!< An item was deleted */
	IFOLDER_CHANGE_ENTRY_TYPE_UNKNOWN	/*!< Unknown type */
} iFolderChangeEntryType;


/**
 * @name Change Entry API
 */
/*@{*/

//! Returns the change time of an iFolderChangeEntry.
/**
 * @param change_entry The iFolderChangeEntry.
 * @return The change time of an iFolderChangeEntry.
 */
time_t ifolder_change_entry_get_change_time(const iFolderChangeEntry change_entry);

//! Returns the change type of an iFolderChangeEntry.
/**
 * @param change_entry The iFolderChangeEntry.
 * @return The change type of an iFolderChangeEntry.
 */
iFolderChangeEntryType ifolder_change_entry_get_change_type(const iFolderChangeEntry change_entry);

//! Returns the ID of an iFolderChangeEntry.
/**
 * @param change_entry The iFolderChangeEntry.
 * @return The ID of the iFolderChangeEntry.
 */
const char * ifolder_change_entry_get_id(const iFolderChangeEntry change_entry);

//! Returns the name of an iFolderChangeEntry.
/**
 * @param change_entry The iFolderChangeEntry.
 * @return The name of the iFolderChangeEntry.
 */
const char * ifolder_change_entry_get_name(const iFolderChangeEntry change_entry);

//! Returns the iFolderUser associated with the iFolderChangeEntry.
/**
 * @param change_entry The iFolderChangeEntry.
 * @param user Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_change_entry_get_user(const iFolderChangeEntry change_entry, iFolderUser *user);

//! Returns true if the type of entry is a directory.
/**
 * @param change_entry The iFolderChangeEntry.
 * @return true if the type of entry is a directory.
 */
bool ifolder_change_entry_is_directory(const iFolderChangeEntry change_entry);

//! Free the memory used by an iFolderChangeEntry.
/**
 * @param change_entry The iFolderChangeEntry.
 */
void ifolder_change_entry_free(iFolderChangeEntry change_entry);

/*@}*/

#ifdef __cplusplus
}
#endif		/* __cplusplus */

#endif /*_IFOLDER_C_CHANGE_ENTRY_H_*/
