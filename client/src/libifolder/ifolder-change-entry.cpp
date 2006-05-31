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

#include "ifolder-change-entry.h"
#include "ifolder-private.h"
#include "ifolder-errors.h"

/* FIXME: Remove the following 3 lines when we start including gettext */
#ifndef _
#define _
#endif

extern iFolderClient *singleton_client;

typedef struct _iFolderChangeEntryPrivate iFolderChangeEntryPrivate;
struct _iFolderChangeEntryPrivate
{
	ifweb::ChangeEntry *change_entry;
};

#define IFOLDER_CHANGE_ENTRY_GET_PRIVATE(o) (G_TYPE_INSTANCE_GET_PRIVATE ((o), IFOLDER_CHANGE_ENTRY_TYPE, iFolderChangeEntryPrivate))

/**
 * Forward definitions
 */
static void ifolder_change_entry_finalize (GObject *object);

G_DEFINE_TYPE (iFolderChangeEntry, ifolder_change_entry, G_TYPE_OBJECT);

/**
 * Functions required by GObject
 */
static void
ifolder_change_entry_class_init(iFolderChangeEntryClass *klass)
{
	GObjectClass *object_class;
	
	object_class = (GObjectClass *)klass;
	
	object_class->finalize = ifolder_change_entry_finalize;
	
	g_type_class_add_private(klass, sizeof(iFolderChangeEntryPrivate));

	/* custom stuff */
}

static void ifolder_change_entry_init(iFolderChangeEntry *change_entry)
{
	iFolderChangeEntryPrivate *priv;

	g_debug("ifolder_change_entry_init()");

	priv = IFOLDER_CHANGE_ENTRY_GET_PRIVATE (change_entry);
	
	/**
	 * Initialize all public and private members to reasonable default values.
	 * If you need specific construction properties to complete initialization,
	 * delay initialization completion until the property is set.
	 */
	priv->change_entry = NULL;
}

static void ifolder_change_entry_finalize(GObject *object)
{
	iFolderChangeEntryPrivate *priv;

	g_debug("ifolder_change_entry_finalize()");
	
	priv = IFOLDER_CHANGE_ENTRY_GET_PRIVATE (object);

	/* custom stuff */
	if (priv->change_entry)
		delete (priv->change_entry);

	/* Chain up the parent class */
	G_OBJECT_CLASS (ifolder_change_entry_parent_class)->finalize (object);
}

iFolderChangeEntry *
ifolder_change_entry_new (ifweb::ChangeEntry *entry)
{
	iFolderChangeEntry *change_entry;
	iFolderChangeEntryPrivate *priv;

	g_return_val_if_fail (entry != NULL, NULL);
	
	change_entry = IFOLDER_CHANGE_ENTRY (g_object_new (IFOLDER_CHANGE_ENTRY_TYPE, NULL));
	priv = IFOLDER_CHANGE_ENTRY_GET_PRIVATE (change_entry);
	
	priv->change_entry = entry;
	
	return change_entry;
}

time_t
ifolder_change_entry_get_change_time(iFolderChangeEntry *change_entry)
{
	iFolderChangeEntryPrivate *priv;
	
	g_return_val_if_fail (IFOLDER_IS_CHANGE_ENTRY (change_entry), 0);
	priv = IFOLDER_CHANGE_ENTRY_GET_PRIVATE (change_entry);
	
	return priv->change_entry->m_Time;
}

iFolderChangeEntryType
ifolder_change_entry_get_change_type (iFolderChangeEntry *change_entry)
{
	iFolderChangeEntryPrivate *priv;
	iFolderChangeEntryType type;
	
	g_return_val_if_fail (IFOLDER_IS_CHANGE_ENTRY (change_entry), IFOLDER_CHANGE_ENTRY_TYPE_UNKNOWN);
	priv = IFOLDER_CHANGE_ENTRY_GET_PRIVATE (change_entry);
	
	switch (priv->change_entry->m_Type)
	{
		case ifolder__ChangeEntryType__iFolder:
			type = IFOLDER_CHANGE_ENTRY_TYPE_IFOLDER;
			break;
		case ifolder__ChangeEntryType__File:
			type = IFOLDER_CHANGE_ENTRY_TYPE_FILE;
			break;
		case ifolder__ChangeEntryType__Directory:
			type = IFOLDER_CHANGE_ENTRY_TYPE_DIRECTORY;
			break;
		case ifolder__ChangeEntryType__Member:
			type = IFOLDER_CHANGE_ENTRY_TYPE_MEMBER;
			break;
		case ifolder__ChangeEntryType__Unknown:
		default:
			type = IFOLDER_CHANGE_ENTRY_TYPE_UNKNOWN;
			break;
	}
	
	return type;
}

iFolderChangeEntryAction
ifolder_change_entry_get_action (iFolderChangeEntry *change_entry)
{
	iFolderChangeEntryPrivate *priv;
	iFolderChangeEntryAction action;
	
	g_return_val_if_fail (IFOLDER_IS_CHANGE_ENTRY (change_entry), IFOLDER_CHANGE_ENTRY_ACTION_UNKNOWN);
	priv = IFOLDER_CHANGE_ENTRY_GET_PRIVATE (change_entry);
	
	switch (priv->change_entry->m_Action)
	{
		case ifolder__ChangeEntryAction__Add:
			action = IFOLDER_CHANGE_ENTRY_ACTION_ADD;
			break;
		case ifolder__ChangeEntryAction__Modify:
			action = IFOLDER_CHANGE_ENTRY_ACTION_MODIFY;
			break;
		case ifolder__ChangeEntryAction__Delete:
			action = IFOLDER_CHANGE_ENTRY_ACTION_DELETE;
			break;
		case ifolder__ChangeEntryAction__Unknown:
		default:
			action = IFOLDER_CHANGE_ENTRY_ACTION_UNKNOWN;
			break;
	}
	
	return action;
}

const gchar *
ifolder_change_entry_get_id(iFolderChangeEntry *change_entry)
{
	iFolderChangeEntryPrivate *priv;
	
	g_return_val_if_fail (IFOLDER_IS_CHANGE_ENTRY (change_entry), NULL);
	priv = IFOLDER_CHANGE_ENTRY_GET_PRIVATE (change_entry);
	
	return priv->change_entry->m_ID;
}

const gchar *
ifolder_change_entry_get_name(iFolderChangeEntry *change_entry)
{
	iFolderChangeEntryPrivate *priv;
	
	g_return_val_if_fail (IFOLDER_IS_CHANGE_ENTRY (change_entry), NULL);
	priv = IFOLDER_CHANGE_ENTRY_GET_PRIVATE (change_entry);
	
	return priv->change_entry->m_Name;
}

const gchar *
ifolder_change_entry_get_user_id (iFolderChangeEntry *change_entry)
{
	iFolderChangeEntryPrivate *priv;
	
	g_return_val_if_fail (IFOLDER_IS_CHANGE_ENTRY (change_entry), NULL);
	priv = IFOLDER_CHANGE_ENTRY_GET_PRIVATE (change_entry);
	
	return priv->change_entry->m_UserID;
}

const gchar *
ifolder_change_entry_get_user_full_name (iFolderChangeEntry *change_entry)
{
	iFolderChangeEntryPrivate *priv;
	
	g_return_val_if_fail (IFOLDER_IS_CHANGE_ENTRY (change_entry), NULL);
	priv = IFOLDER_CHANGE_ENTRY_GET_PRIVATE (change_entry);
	
	return priv->change_entry->m_UserFullName;
}
