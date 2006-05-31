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

#ifndef IFOLDER_CHANGE_ENTRY_ITERATOR_H
#define IFOLDER_CHANGE_ENTRY_ITERATOR_H

/* Headers that this header depends on. */
#include "ifolder-types.h"

#ifdef __cplusplus
extern "C"
{
#endif		/* __cplusplus */

G_BEGIN_DECLS

#define IFOLDER_CHANGE_ENTRY_ITERATOR_TYPE				(ifolder_change_entry_iterator_get_type())
#define IFOLDER_CHANGE_ENTRY_ITERATOR(obj)				(G_TYPE_CHECK_INSTANCE_CAST ((obj), IFOLDER_CHANGE_ENTRY_ITERATOR_TYPE, iFolderChangeEntryIterator))
#define IFOLDER_CHANGE_ENTRY_ITERATOR_CLASS(klass)		(G_TYPE_CHECK_CLASS_CAST ((klass), IFOLDER_CHANGE_ENTRY_ITERATOR_TYPE, iFolderChangeEntryIteratorClass))
#define IFOLDER_IS_CHANGE_ENTRY_ITERATOR(obj)			(G_TYPE_CHECK_INSTANCE_TYPE ((obj), IFOLDER_CHANGE_ENTRY_ITERATOR_TYPE))
#define IFOLDER_IS_CHANGE_ENTRY_ITERATOR_CLASS(klass)	(G_TYPE_CHECK_CLASS_TYPE ((klass), IFOLDER_CHANGE_ENTRY_ITERATOR_TYPE))
#define IFOLDER_CHANGE_ENTRY_ITERATOR_GET_CLASS(obj)	(G_TYPE_INSTANCE_GET_CLASS ((obj), IFOLDER_CHANGE_ENTRY_ITERATOR_TYPE, iFolderChangeEntryIteratorClass))

/* GObject support */
GType ifolder_change_entry_iterator_get_type (void) G_GNUC_CONST;

/**
 * Method definitions
 */

/**
 * Get the first/next iFolderChangeEntry * in the iterator.
 * @return Returns NULL if there are no more iFolderChangeEntry * in the list.
 */
iFolderChangeEntry *ifolder_change_entry_iterator_next (iFolderChangeEntryIterator *iterator);

/**
 * Reset the iterator back to the first item.
 */
void ifolder_change_entry_iterator_reset (iFolderChangeEntryIterator *iterator);

/**
 * Returns the total number of items in the iterator
 */
int ifolder_change_entry_iterator_get_count (iFolderChangeEntryIterator *iterator);

G_END_DECLS

#ifdef __cplusplus
}
#endif		/* __cplusplus */

#endif /* IFOLDER_CHANGE_ENTRY_ITERATOR_H */
