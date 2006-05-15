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

#include "ifolder-client.h"
#include "ifolder-private.h"
#include "ifolder-errors.h"

#ifndef _
#define _
#endif

struct _iFolderClientPrivate
{
	gboolean dispose_has_run;
	
	GSList *domains;
};

static GObjectClass *parent_class = NULL;
static iFolderClient *singleton_client = NULL;

#define IFOLDER_CLIENT_GET_PRIVATE(o) (G_TYPE_INSTANCE_GET_PRIVATE ((o), IFOLDER_CLIENT_TYPE, iFolderClientPrivate))

/**
 * Forward definitions
 */
static void ifolder_client_finalize(GObject *object);
static void ifolder_client_dispose(GObject *object);

/**
 * Functions required by GObject
 */
static void
ifolder_client_class_init(iFolderClientClass *klass)
{
	GObjectClass *object_class = (GObjectClass *)klass;
	parent_class = g_type_class_peek_parent(klass);
	
	object_class->finalize = ifolder_client_finalize;
	object_class->dispose = ifolder_client_dispose;

	g_type_class_add_private(klass, sizeof(iFolderClientPrivate));

	/* custom stuff */
}

static void ifolder_client_init(GTypeInstance *instance, gpointer g_class)
{
	g_message("ifolder_client_init() called");

	iFolderClient *self = IFOLDER_CLIENT(instance);
	self->priv = g_new0(iFolderClientPrivate, 1);
	
	/**
	 * Initialize all public and private members to reasonable default values.
	 * If you need specific construction properties to complete initialization,
	 * delay initialization completion until the property is set.
	 */
	self->priv->dispose_has_run = FALSE;
	
	self->priv->domains = g_slist_alloc();
}

static void ifolder_client_finalize(GObject *object)
{
	iFolderClient *self = IFOLDER_CLIENT(object);

	/* custom stuff */
	g_slist_free(self->priv->domains);

	g_free(self->priv);
}

static void ifolder_client_dispose(GObject *object)
{
	iFolderClient *self = IFOLDER_CLIENT(object);
	
	/* if dispose already ran, return */
	if (self->priv->dispose_has_run)
		return;
	
	/* make sure dispose does not run twice */
	self->priv->dispose_has_run = TRUE;

	/**
	 * In dispose, you are supposed to free all types referenced from this
	 * object which might themselves hold a reference to self.  Generally,
	 * the most simple solution is to unref all members on which you own a
	 * reference.
	 */
	
	/* Chain up the parent class */
	G_OBJECT_CLASS(parent_class)->dispose(object);
}

GType
ifolder_client_get_type(void)
{
	static GType type = 0;
	if (type == 0)
	{
		static const GTypeInfo info =
		{
			sizeof (iFolderClientClass),
			NULL,	/* base_init */
			NULL,	/* base_finalize */
			(GClassInitFunc) ifolder_client_class_init,	/* class_init */
			NULL,	/* class_finalize */
			NULL,	/* class_data */
			sizeof (iFolderClient),
			0,		/* n_preallocs */
			(GInstanceInitFunc) ifolder_client_init,	/* instance_init */
		};
		
		type = g_type_register_static(G_TYPE_OBJECT,
									  "iFolderClientType",
									  &info, 0);
	}
	
	return type;
}

iFolderClient *
ifolder_client_new (void)
{
	iFolderClient *client;
	
	client = g_object_new (IFOLDER_CLIENT_TYPE, NULL);
	
	return client;
}

iFolderClient *
ifolder_client_initialize(const char *data_path, GError **error)
{
	iFolderClient *client;

	if (singleton_client != NULL)
	{
		g_set_error(error,
					IFOLDER_CLIENT_ERROR,
					IFOLDER_ERR_ALREADY_INITIALIZED,
					_("The iFolder Client is already initialized."));
		return NULL;
	}

	client = ifolder_client_new();
	
	g_message("FIXME: Implement ifolder_client_initialize()");
	
	/* If everything was initialized correctly, set the singleton instance and return. */
	singleton_client = client;
	
	return client;
}

void
ifolder_client_uninitialize(iFolderClient *client, GError *error)
{
	if (singleton_client == NULL)
	{
		g_set_error(error,
					IFOLDER_CLIENT_ERROR,
					IFOLDER_ERR_NOT_INITIALIZED,
					_("The iFolder Client is not initialized."));
		return;
	}
	
	g_message("FIXME: Implement ifolder_client_uninitialize()");
	
	g_free(singleton_client);
	
	singleton_client = NULL;
}

iFolderClientState
ifolder_client_get_state(iFolderClient *client)
{
	g_message("FIXME: Implement ifolder_client_get_state()");
	return IFOLDER_CLIENT_STATE_INITIALIZING;
}

void
ifolder_client_start_synchronization(iFolderClient *client, GError **error)
{
	g_message("FIXME: Implement ifolder_client_start_synchronization()");
}

void
ifolder_client_stop_synchronization(iFolderClient *client, GError **error)
{
	g_message("FIXME: Implement ifolder_client_stop_synchronization()");
}

void
ifolder_client_resume_synchronization(iFolderClient *client, GError **error)
{
	g_message("FIXME: Implement ifolder_client_resume_synchronization()");
}

GSList *
ifolder_client_get_all_domains(iFolderClient *client, GError **error)
{
	g_message("FIXME: Implement ifolder_client_get_all_domains()");
	return g_slist_alloc();
}

GSList *
ifolder_client_get_all_active_domains(iFolderClient *client, GError **error)
{
	g_message("FIXME: Implement ifolder_client_get_all_active_domains()");
	return g_slist_alloc();
}

iFolderDomain *
ifolder_client_get_default_domain(iFolderClient *client, GError **error)
{
	g_message("FIXME: Implement ifolder_client_get_default_domain()");
	return NULL;
}

iFolderDomain *
ifolder_client_add_domain(iFolderClient *client, const gchar *host_address, const gchar *user_name, const gchar *password, gboolean make_default, GError **error)
{
	g_message("FIXME: Implement ifolder_client_add_domain()");
	return NULL;
}

void
ifolder_client_remove_domain(iFolderClient *client, iFolderDomain *domain, GError **error)
{
	g_message("FIXME: Implement ifolder_client_remove_domain()");
}
