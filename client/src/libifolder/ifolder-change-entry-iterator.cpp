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
#ifdef WIN32
	#include <windows.h>
	#include <fcntl.h>
	#define sleep(x) Sleep((x)*1000)
#endif

#include <IFServices.h>

#include "ifolder-change-entry-iterator.h"
#include "ifolder-change-entry.h"
#include "ifolder-private.h"
#include "ifolder-errors.h"

/* FIXME: Remove the following 3 lines when we start including gettext */
#ifndef _
#define _
#endif

typedef struct _iFolderChangeEntryIteratorPrivate iFolderChangeEntryIteratorPrivate;
struct _iFolderChangeEntryIteratorPrivate
{
	gint						count;
	GSList						*list;
	GSList						*current;
	ifweb::ChangeEntryIterator	*core_iterator;
};

#define IFOLDER_CHANGE_ENTRY_ITERATOR_GET_PRIVATE(o) (G_TYPE_INSTANCE_GET_PRIVATE ((o), IFOLDER_CHANGE_ENTRY_ITERATOR_TYPE, iFolderChangeEntryIteratorPrivate))

/**
 * Forward definitions
 */
static void ifolder_change_entry_iterator_finalize(GObject *object);

G_DEFINE_TYPE (iFolderChangeEntryIterator, ifolder_change_entry_iterator, G_TYPE_OBJECT);


/**
 * Functions required by GObject
 */
static void
ifolder_change_entry_iterator_class_init(iFolderChangeEntryIteratorClass *klass)
{
	GObjectClass *object_class;
	
	object_class = (GObjectClass *)klass;
	
	object_class->finalize = ifolder_change_entry_iterator_finalize;

	g_type_class_add_private(klass, sizeof(iFolderChangeEntryIteratorPrivate));

	/* custom stuff */
}

static void ifolder_change_entry_iterator_init(iFolderChangeEntryIterator *iterator)
{
	iFolderChangeEntryIteratorPrivate *priv;

	g_debug ("ifolder_change_entry_iterator_init() called");

	priv = IFOLDER_CHANGE_ENTRY_ITERATOR_GET_PRIVATE (iterator);
	
	/**
	 * Initialize all public and private members to reasonable default values.
	 * If you need specific construction properties to complete initialization,
	 * delay initialization completion until the property is set.
	 */
	priv->count			= 0;
	priv->list			= NULL;
	priv->current		= NULL;
	priv->core_iterator	= NULL;
}

static void ifolder_change_entry_iterator_finalize(GObject *object)
{
	iFolderChangeEntryIteratorPrivate *priv;
	GSList *cur;
	iFolderChangeEntry *change_entry;

	g_debug (" ***** ifolder_change_entry_iterator_finalize() ***** ");
	
	priv = IFOLDER_CHANGE_ENTRY_ITERATOR_GET_PRIVATE (object);

	/* custom stuff */
	for (cur = priv->list; cur != NULL; cur = g_slist_next (cur))
	{
		change_entry = IFOLDER_CHANGE_ENTRY (cur->data);
		g_object_unref (change_entry);
	}
	
	g_slist_free (priv->list);
	
	delete priv->core_iterator;

	/* Chain up the parent class */
	G_OBJECT_CLASS (ifolder_change_entry_iterator_parent_class)->finalize (object);
}

iFolderChangeEntryIterator *
ifolder_change_entry_iterator_new (ifweb::ChangeEntryIterator *core_iterator)
{
	iFolderChangeEntryIterator *iterator;
	iFolderChangeEntryIteratorPrivate *priv;
	iFolderChangeEntry *change_entry;
	ifweb::ChangeEntry *core_change_entry;
	
	g_return_val_if_fail (core_iterator != NULL, NULL);
	
	iterator = IFOLDER_CHANGE_ENTRY_ITERATOR (g_object_new (IFOLDER_CHANGE_ENTRY_ITERATOR_TYPE, NULL));
	priv = IFOLDER_CHANGE_ENTRY_ITERATOR_GET_PRIVATE (iterator);
	
	priv->core_iterator = core_iterator;
	
	priv->count = priv->core_iterator->m_Count;
	
	if (priv->count > 0)
	{
		for (core_change_entry = priv->core_iterator->Next(); core_change_entry != NULL; core_change_entry = priv->core_iterator->Next())
		{
			change_entry = ifolder_change_entry_new (core_change_entry);
			priv->list = g_slist_prepend (priv->list, change_entry);
		}
	}
	
	priv->current = priv->list;
	
	return iterator;
}

iFolderChangeEntry *
ifolder_change_entry_iterator_next (iFolderChangeEntryIterator *iterator)
{
	iFolderChangeEntryIteratorPrivate *priv;
	
	g_return_val_if_fail (IFOLDER_IS_CHANGE_ENTRY_ITERATOR (iterator), NULL);
	
	priv = IFOLDER_CHANGE_ENTRY_ITERATOR_GET_PRIVATE (iterator);
	
	g_return_val_if_fail (priv->current != NULL, NULL);
	priv->current = g_slist_next (priv->current);
	
	if (priv->current == NULL)
		return NULL;
	
	return IFOLDER_CHANGE_ENTRY (priv->current->data);
}

/**
 * Reset the iterator back to the first item.
 */
void
ifolder_change_entry_iterator_reset (iFolderChangeEntryIterator *iterator)
{
	iFolderChangeEntryIteratorPrivate *priv;
	
	g_return_if_fail (IFOLDER_IS_CHANGE_ENTRY_ITERATOR (iterator));

	priv = IFOLDER_CHANGE_ENTRY_ITERATOR_GET_PRIVATE (iterator);
	
	priv->current = priv->list;	
}

/**
 * Returns the total number of items in the iterator
 */
int
ifolder_change_entry_iterator_get_count (iFolderChangeEntryIterator *iterator)
{
	iFolderChangeEntryIteratorPrivate *priv;
	
	g_return_val_if_fail (IFOLDER_IS_CHANGE_ENTRY_ITERATOR (iterator), -1);

	priv = IFOLDER_CHANGE_ENTRY_ITERATOR_GET_PRIVATE (iterator);
	
	return priv->count;
}

