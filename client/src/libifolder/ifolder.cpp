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

#include <IFiFolder.h>

#include "ifolder.h"
#include "ifolder-private.h"
#include "ifolder-errors.h"

/* FIXME: Remove the following 3 lines when we start including gettext */
#ifndef _
#define _
#endif

extern iFolderClient *singleton_client;
extern guint ifolder_client_signals[LAST_SIGNAL];

typedef struct _iFolderPrivate iFolderPrivate;
struct _iFolderPrivate
{
	gchar *id;
	gchar *name;
	gchar *description;
	gpointer user_data;
	
	gboolean is_connected;
	iFolderState state;
	
	IFiFolder *core_ifolder;
};

#define IFOLDER_GET_PRIVATE(o) (G_TYPE_INSTANCE_GET_PRIVATE ((o), IFOLDER_TYPE, iFolderPrivate))

enum
{
	PROP_ID,
	PROP_NAME,
	PROP_DESCRIPTION,
	PROP_STATE
};

/**
 * Forward definitions
 */
static void ifolder_finalize (GObject *object);
static void ifolder_get_property (GObject *object, guint prop_id, GValue *value, GParamSpec *pspec);
static void ifolder_set_property (GObject *object, guint prop_id, const GValue *value, GParamSpec *pspec);

G_DEFINE_TYPE (iFolder, ifolder, G_TYPE_OBJECT);


/**
 * Functions required by GObject
 */
static void
ifolder_class_init(iFolderClass *klass)
{
	GObjectClass *object_class;
	
	object_class = (GObjectClass *)klass;
	
	object_class->get_property = ifolder_get_property;
	object_class->set_property = ifolder_set_property;
	
	object_class->finalize = ifolder_finalize;
	
	/**
	 * iFolder:id:
	 * 
	 * The internal ID of the ifolder.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_ID,
					g_param_spec_string ("id",
						_("iFolder ID"),
						_("The internal ID of the ifolder."),
						NULL,
						G_PARAM_READABLE));

	/**
	 * iFolder:name:
	 * 
	 * The name of the iFolder.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_NAME,
					g_param_spec_string ("name",
						_("iFolder name"),
						_("The name of the iFolder."),
						NULL,
						G_PARAM_READABLE));

	/**
	 * iFolder:description:
	 * 
	 * The description of the iFolder.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_DESCRIPTION,
					g_param_spec_string ("description",
						_("iFolder description"),
						_("The description of the iFolder."),
						NULL,
						(GParamFlags) (G_PARAM_READABLE | G_PARAM_WRITABLE)));

	/**
	 * iFolder:state:
	 * 
	 * The state of the iFolder.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_STATE,
					g_param_spec_uint ("state",
						_("iFolder state."),
						_("The state of the iFolder."),
						(guint) IFOLDER_STATE_SYNC_WAIT,
						(guint) IFOLDER_STATE_UNKNOWN,
						(guint) IFOLDER_STATE_UNKNOWN,
						G_PARAM_READABLE));

	g_type_class_add_private(klass, sizeof(iFolderPrivate));

	/* custom stuff */
}

static void ifolder_init(iFolder *ifolder)
{
	iFolderPrivate *priv;

	g_message("ifolder_init() called");

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	/**
	 * Initialize all public and private members to reasonable default values.
	 * If you need specific construction properties to complete initialization,
	 * delay initialization completion until the property is set.
	 */
	priv->id = NULL;
	priv->name = NULL;
	priv->description = NULL;
	priv->state = IFOLDER_STATE_UNKNOWN;
	priv->user_data = NULL;
	
	priv->core_ifolder = NULL;
}

static void ifolder_finalize(GObject *object)
{
	iFolderPrivate *priv;

	g_debug (" ***** ifolder_finalize() ***** ");
	
	priv = IFOLDER_GET_PRIVATE (object);

	/* custom stuff */
	g_free(priv->id);
	g_free(priv->name);
	g_free(priv->description);

	/* Chain up the parent class */
	G_OBJECT_CLASS (ifolder_parent_class)->finalize (object);
}

static void
ifolder_get_property (GObject *object, guint prop_id, GValue *value, GParamSpec *pspec)
{
	iFolder *ifolder = IFOLDER (object);
	iFolderPrivate *priv = IFOLDER_GET_PRIVATE (object);
	
	switch (prop_id)
	{
		case PROP_ID:
			g_value_set_string (value, priv->id);
			break;
		case PROP_NAME:
			g_value_set_string (value, priv->name);
			break;
		case PROP_DESCRIPTION:
			g_value_set_string (value, priv->description);
			break;
		case PROP_STATE:
			g_value_set_uint (value, (guint)priv->state);
			break;
		default:
			G_OBJECT_WARN_INVALID_PROPERTY_ID (object, prop_id, pspec);
			break;
	}
}

static void
ifolder_set_property (GObject *object, guint prop_id, const GValue *value, GParamSpec *pspec)
{
	iFolder *ifolder = IFOLDER (object);
	iFolderPrivate *priv = IFOLDER_GET_PRIVATE (object);
	
	switch (prop_id)
	{
		case PROP_DESCRIPTION:
			ifolder_set_description (ifolder, g_value_get_string (value), NULL);
			break;
		default:
			G_OBJECT_WARN_INVALID_PROPERTY_ID (object, prop_id, pspec);
			break;
	}
}

iFolder *
ifolder_new (void)
{
	iFolder *ifolder;
	
	ifolder = IFOLDER(g_object_new (IFOLDER_TYPE, NULL));
	
	return ifolder;
}

const gchar *
ifolder_get_id(iFolder *ifolder)
{
	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), "");

	return IFOLDER_GET_PRIVATE (ifolder)->id;
}

void
ifolder_set_id (iFolder *ifolder, const gchar *id)
{
	iFolderPrivate *priv;

	g_return_if_fail (IFOLDER_IS_IFOLDER (ifolder));
	
	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	if (id == NULL)
	{
		g_free (priv->id);
		priv->id = NULL;
	}
	else
		priv->id = g_strdup (id);
	
	g_object_notify (G_OBJECT (ifolder), "id");
}

const gchar *
ifolder_get_name(iFolder *ifolder)
{
	iFolderPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), "");

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	return priv->name;
}

void
ifolder_set_name (iFolder *ifolder, const gchar *name)
{
	iFolderPrivate *priv;

	g_return_if_fail (IFOLDER_IS_IFOLDER (ifolder));
	
	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	if (name == NULL)
	{
		g_free (priv->name);
		priv->name = NULL;
	}
	else
		priv->name = g_strdup (name);
	
	g_object_notify (G_OBJECT (ifolder), "name");
}

const gchar *
ifolder_get_description(iFolder *ifolder)
{
	iFolderPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), "");

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	return priv->description;
}

void
ifolder_set_description (iFolder *ifolder, const gchar *new_description, GError **error)
{
	iFolderPrivate *priv;

	g_return_if_fail (IFOLDER_IS_IFOLDER (ifolder));
	
	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	g_debug ("FIXME: Call the core to set the description of an iFolder");
	if (new_description == NULL)
	{
		g_free (priv->description);
		priv->description = NULL;
	}
	else
		priv->description = g_strdup (new_description);

	g_object_notify (G_OBJECT (ifolder), "description");
}

gpointer
ifolder_get_user_data(iFolder *ifolder)
{
	iFolderPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), NULL);

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	return priv->user_data;
}

void
ifolder_set_user_data(iFolder *ifolder, gpointer user_data)
{
	iFolderPrivate *priv;

	g_return_if_fail (IFOLDER_IS_IFOLDER (ifolder));

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	priv->user_data = user_data;
}

IFiFolder *
ifolder_get_core_ifolder (iFolder *ifolder)
{
	iFolderPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), NULL);

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	return priv->core_ifolder;
}

void
ifolder_set_core_ifolder (iFolder *ifolder, IFiFolder *core_ifolder)
{
	iFolderPrivate *priv;

	g_return_if_fail (IFOLDER_IS_IFOLDER (ifolder));

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	priv->core_ifolder = core_ifolder;
}

