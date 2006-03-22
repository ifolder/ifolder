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
 *  @file change-entry.h User API
 ***********************************************************************/
#ifndef _IFOLDER_CHANGE_ENTRY_H
/* @cond */
#define _IFOLDER_CHANGE_ENTRY_H 1
/* @endcond */

#include <time.h>

//! An object that represents a change for an iFolder, a file, or a directory.
/**
 * FIXME: Add documentation for iFolderChangeEntry.
 */
typedef void * iFolderChangeEntry;

//! An enum for the type of change.
/**
 * Add documentation for iFolderChangeEntryType
 */
typedef enum
{
	IFOLDER_CHANGE_ENTRY_TYPE_ADD,		/*!< FIXME: Add documentation for IFOLDER_CHANGE_ENTRY_TYPE_ADD */
	IFOLDER_CHANGE_ENTRY_TYPE_MODIFY,	/*!< FIXME: Add documentation for IFOLDER_CHANGE_ENTRY_TYPE_MODIFY */
	IFOLDER_CHANGE_ENTRY_TYPE_DELETE,	/*!< FIXME: Add documentation for IFOLDER_CHANGE_ENTRY_TYPE_DELETE */
	IFOLDER_CHANGE_ENTRY_TYPE_UNKNOWN	/*!< FIXME: Add documentation for IFOLDER_CHANGE_ENTRY_TYPE_UNKNOWN */
} iFolderChangeEntryType;

//! Release/free the memory used by an @a iFolderChangeEntry object.
/**
 * FIXME: Add more documentation for ifolder_change_entry_release.
 *
 * @param change_entry A @a pointer to an @a iFolderChangeEntry object
 * to be released/freed.
 * @returns Returns IFOLDER_SUCESS if there were no errors.
 */
int ifolder_change_entry_release(iFolderChangeEntry *change_entry);

const time_t ifolder_change_entry_get_time(iFolderChangeEntry change_entry);
iFolderChangeEntryType ifolder_change_entry_get_type(iFolderChangeEntry change_entry);
const char * ifolder_change_entry_get_id(iFolderChangeEntry change_entry);
const char * ifolder_change_entry_get_name(iFolderChangeEntry change_entry);
const char * ifolder_change_entry_get_user_id(iFolderChangeEntry change_entry);
const char * ifolder_change_entry_get_user_full_name(iFolderChangeEntry change_entry);

#endif
