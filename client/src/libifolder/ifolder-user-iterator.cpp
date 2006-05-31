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

#include "ifolder-user-iterator.h"
#include "ifolder-user.h"
#include "ifolder-private.h"
#include "ifolder-errors.h"

/* FIXME: Remove the following 3 lines when we start including gettext */
#ifndef _
#define _
#endif

typedef struct _iFolderUserIteratorPrivate iFolderUserIteratorPrivate;
struct _iFolderUserIteratorPrivate
{
	gint						count;
	GSList						*user_list;
	GSList						*current;
	ifweb::iFolderUserIterator	*core_user_iterator;
};

#define IFOLDER_USER_ITERATOR_GET_PRIVATE(o) (G_TYPE_INSTANCE_GET_PRIVATE ((o), IFOLDER_USER_ITERATOR_TYPE, iFolderUserIteratorPrivate))

/**
 * Forward definitions
 */
static void ifolder_user_iterator_finalize(GObject *object);

G_DEFINE_TYPE (iFolderUserIterator, ifolder_user_iterator, G_TYPE_OBJECT);


/**
 * Functions required by GObject
 */
static void
ifolder_user_iterator_class_init(iFolderUserIteratorClass *klass)
{
	GObjectClass *object_class;
	
	object_class = (GObjectClass *)klass;
	
	object_class->finalize = ifolder_user_iterator_finalize;

	g_type_class_add_private(klass, sizeof(iFolderUserIteratorPrivate));

	/* custom stuff */
}

static void ifolder_user_iterator_init(iFolderUserIterator *user_iterator)
{
	iFolderUserIteratorPrivate *priv;

	g_debug ("ifolder_user_iterator_init() called");

	priv = IFOLDER_USER_ITERATOR_GET_PRIVATE (user_iterator);
	
	/**
	 * Initialize all public and private members to reasonable default values.
	 * If you need specific construction properties to complete initialization,
	 * delay initialization completion until the property is set.
	 */
	priv->count					= 0;
	priv->user_list				= NULL;
	priv->current				= NULL;
	priv->core_user_iterator	= NULL;
}

static void ifolder_user_iterator_finalize(GObject *object)
{
	iFolderUserIteratorPrivate *priv;
	GSList *cur;
	iFolderUser *user;

	g_debug (" ***** ifolder_user_iterator_finalize() ***** ");
	
	priv = IFOLDER_USER_ITERATOR_GET_PRIVATE (object);

	/* custom stuff */
	for (cur = priv->user_list; cur != NULL; cur = g_slist_next (cur))
	{
		user = IFOLDER_USER (cur->data);
		g_object_unref (user);
	}
	
	g_slist_free (priv->user_list);
	
	delete priv->core_user_iterator;

	/* Chain up the parent class */
	G_OBJECT_CLASS (ifolder_user_iterator_parent_class)->finalize (object);
}

iFolderUserIterator *
ifolder_user_iterator_new (ifweb::iFolderUserIterator *core_user_iterator)
{
	iFolderUserIterator *user_iterator;
	iFolderUserIteratorPrivate *priv;
	iFolderUser *user;
	ifweb::iFolderUser *core_user;
	
	g_return_val_if_fail (core_user_iterator != NULL, NULL);
	
	user_iterator = IFOLDER_USER_ITERATOR (g_object_new (IFOLDER_USER_ITERATOR_TYPE, NULL));
	priv = IFOLDER_USER_ITERATOR_GET_PRIVATE (user_iterator);
	
	priv->core_user_iterator = core_user_iterator;
	
	priv->count = priv->core_user_iterator->m_Count;
	
	if (priv->count > 0)
	{
		for (core_user = priv->core_user_iterator->Next(); core_user != NULL; core_user = priv->core_user_iterator->Next())
		{
			user = ifolder_user_new_for_iterator (core_user);
			priv->user_list = g_slist_prepend (priv->user_list, user);
		}
	}
	
	priv->current = priv->user_list;
	
	return user_iterator;
}

iFolderUser *
ifolder_user_iterator_next (iFolderUserIterator *iterator)
{
	iFolderUserIteratorPrivate *priv;
	
	g_return_val_if_fail (IFOLDER_IS_USER_ITERATOR (iterator), NULL);
	
	priv = IFOLDER_USER_ITERATOR_GET_PRIVATE (iterator);
	
	g_return_val_if_fail (priv->current != NULL, NULL);
	priv->current = g_slist_next (priv->current);
	
	if (priv->current == NULL)
		return NULL;
	
	return IFOLDER_USER (priv->current->data);
}

/**
 * Reset the iterator back to the first item.
 */
void
ifolder_user_iterator_reset (iFolderUserIterator *iterator)
{
	iFolderUserIteratorPrivate *priv;
	
	g_return_if_fail (IFOLDER_IS_USER_ITERATOR (iterator));

	priv = IFOLDER_USER_ITERATOR_GET_PRIVATE (iterator);
	
	priv->current = priv->user_list;	
}

/**
 * Returns the total number of items in the iterator
 */
int
ifolder_user_iterator_get_count (iFolderUserIterator *iterator)
{
	iFolderUserIteratorPrivate *priv;
	
	g_return_val_if_fail (IFOLDER_IS_USER_ITERATOR (iterator), -1);

	priv = IFOLDER_USER_ITERATOR_GET_PRIVATE (iterator);
	
	return priv->count;
}

