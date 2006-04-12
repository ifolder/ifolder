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

/**
 * @file ifolder-change-entry.h
 * @brief Change Entry API
 */

typedef void *iFolderChangeEntry;

typedef enum
{
	IFOLDER_CHANGE_ENTRY_TYPE_DIR,	/*!< Directory change */
	IFOLDER_CHANGE_ENTRY_TYPE_FILE	/*!< File change */
} iFolderChangeEntryType;

void ifolder_change_entry_free(iFolderChangeEntry change_entry);

#ifdef __cplusplus
}
#endif		/* __cplusplus */

#endif /*_IFOLDER_C_CHANGE_ENTRY_H_*/
