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

#include <time.h>
#include "ifolder-types.h"

#ifdef __cplusplus
extern "C"
{
#endif		/* __cplusplus */

G_BEGIN_DECLS

#define IFOLDER_CHANGE_ENTRY_TYPE				(ifolder_change_entry_get_type())
#define IFOLDER_CHANGE_ENTRY(obj)				(G_TYPE_CHECK_INSTANCE_CAST ((obj), IFOLDER_CHANGE_ENTRY_TYPE, iFolderChangeEntry))
#define IFOLDER_CHANGE_ENTRY_CLASS(klass)		(G_TYPE_CHECK_CLASS_CAST ((klass), IFOLDER_CHANGE_ENTRY_TYPE, iFolderChangeEntryClass))
#define IFOLDER_IS_CHANGE_ENTRY(obj)			(G_TYPE_CHECK_INSTANCE_TYPE ((obj), IFOLDER_CHANGE_ENTRY_TYPE))
#define IFOLDER_IS_CHANGE_ENTRY_CLASS(klass)	(G_TYPE_CHECK_CLASS_TYPE ((klass), IFOLDER_CHANGE_ENTRY_TYPE))
#define IFOLDER_CHANGE_ENTRY_GET_CLASS(obj)		(G_TYPE_INSTANCE_GET_CLASS ((obj), IFOLDER_CHANGE_ENTRY_TYPE, iFolderChangeEntryClass))

/* GObject support */
GType ifolder_change_entry_get_type (void) G_GNUC_CONST;

typedef enum
{
	IFOLDER_CHANGE_ENTRY_TYPE_IFOLDER,
	IFOLDER_CHANGE_ENTRY_TYPE_FILE,
	IFOLDER_CHANGE_ENTRY_TYPE_DIRECTORY,
	IFOLDER_CHANGE_ENTRY_TYPE_MEMBER,
	IFOLDER_CHANGE_ENTRY_TYPE_UNKNOWN
} iFolderChangeEntryType;

typedef enum
{
	IFOLDER_CHANGE_ENTRY_ACTION_ADD,		/*!< An item was added */
	IFOLDER_CHANGE_ENTRY_ACTION_MODIFY,		/*!< An item was modified */
	IFOLDER_CHANGE_ENTRY_ACTION_DELETE,		/*!< An item was deleted */
	IFOLDER_CHANGE_ENTRY_ACTION_UNKNOWN		/*!< Unknown type */
} iFolderChangeEntryAction;

/**
 * @name Change Entry API
 */
/*@{*/

//! Returns the change time of an iFolderChangeEntry.
/**
 * @param change_entry The iFolderChangeEntry.
 * @return The change time of an iFolderChangeEntry.
 */
time_t ifolder_change_entry_get_change_time(iFolderChangeEntry *change_entry);

//! Returns the change type of an iFolderChangeEntry.
/**
 * @param change_entry The iFolderChangeEntry.
 * @return The change type of an iFolderChangeEntry.
 */
iFolderChangeEntryType ifolder_change_entry_get_change_type (iFolderChangeEntry *change_entry);

iFolderChangeEntryAction ifolder_change_entry_get_action (iFolderChangeEntry *change_entry);

//! Returns the ID of an iFolderChangeEntry.
/**
 * @param change_entry The iFolderChangeEntry.
 * @return The ID of the iFolderChangeEntry.
 */
const gchar * ifolder_change_entry_get_id(iFolderChangeEntry *change_entry);

//! Returns the name of an iFolderChangeEntry.
/**
 * @param change_entry The iFolderChangeEntry.
 * @return The name of the iFolderChangeEntry.
 */
const gchar * ifolder_change_entry_get_name(iFolderChangeEntry *change_entry);

//! Returns the iFolderUser associated with the iFolderChangeEntry.
/**
 * @param change_entry The iFolderChangeEntry.
 * @param user Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
iFolderUser * ifolder_change_entry_get_user(iFolderChangeEntry *change_entry);

const gchar * ifolder_change_entry_get_user_full_name (iFolderChangeEntry *change_entry);

/*@}*/

G_END_DECLS

#ifdef __cplusplus
}
#endif		/* __cplusplus */

#endif /*_IFOLDER_C_CHANGE_ENTRY_H_*/
