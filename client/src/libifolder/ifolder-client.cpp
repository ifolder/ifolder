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

#include <unistd.h>	/* FIXME: Remove this when finished spoofing work with sleep() */

#include <IFApplication.h>

#include "ifolder-client.h"
#include "ifolder-private.h"
#include "ifolder-errors.h"

#ifndef _
#define _
#endif

typedef struct _iFolderClientPrivate iFolderClientPrivate;
struct _iFolderClientPrivate
{
	gboolean		dispose_has_run;
	
	IFApplication	*ifolder_core_app;
	GKeyFile		*config_key_file;
	
	iFolderDomain 	*default_domain;
	GSList			*domains;
};

#define IFOLDER_CLIENT_GET_PRIVATE(obj) (G_TYPE_INSTANCE_GET_PRIVATE ((obj), IFOLDER_CLIENT_TYPE, iFolderClientPrivate))

enum {
	DOMAIN_ADDED,
	DOMAIN_REMOVED,
	LAST_SIGNAL
};

static iFolderClient *singleton_client = NULL;

static guint ifolder_client_signals[LAST_SIGNAL] = { 0 };

/**
 * Forward definitions
 */
static void ifolder_client_finalize (GObject *object);

G_DEFINE_TYPE (iFolderClient, ifolder_client, G_TYPE_OBJECT);

static void load_domains (iFolderClient *client, GError **error);

/**
 * Functions required by GObject
 */
static void
ifolder_client_class_init(iFolderClientClass *klass)
{
	GObjectClass *object_class;

	object_class = (GObjectClass *)klass;
	
	object_class->finalize = ifolder_client_finalize;
	
	ifolder_client_signals[DOMAIN_ADDED] =
		g_signal_new ("domain-added",
			G_OBJECT_CLASS_TYPE (object_class),
			G_SIGNAL_RUN_LAST,
			G_STRUCT_OFFSET (iFolderClientClass, domain_added),
			NULL, NULL,
			g_cclosure_marshal_VOID__POINTER,
			G_TYPE_NONE,
			1,
			G_TYPE_OBJECT);

	ifolder_client_signals[DOMAIN_REMOVED] =
		g_signal_new ("domain-removed",
			G_OBJECT_CLASS_TYPE (object_class),
			G_SIGNAL_RUN_LAST,
			G_STRUCT_OFFSET (iFolderClientClass, domain_removed),
			NULL, NULL,
			g_cclosure_marshal_VOID__POINTER,
			G_TYPE_NONE,
			1,
			G_TYPE_OBJECT);

	g_type_class_add_private(klass, sizeof(iFolderClientPrivate));

	/* custom stuff */
}

static void ifolder_client_init(iFolderClient *client)
{
	iFolderClientPrivate *priv;

	g_message("ifolder_client_init() called");
	
	priv = IFOLDER_CLIENT_GET_PRIVATE (client);

	/**
	 * Initialize all public and private members to reasonable default values.
	 * If you need specific construction properties to complete initialization,
	 * delay initialization completion until the property is set.
	 */
	priv->ifolder_core_app = NULL;
	priv->config_key_file = g_key_file_new();

	priv->default_domain = NULL;
	priv->domains = NULL;
}

static void ifolder_client_finalize(GObject *object)
{
	GSList *domainListPtr;
	iFolderDomain *domain;
	
	iFolderClient *client = IFOLDER_CLIENT(object);
	iFolderClientPrivate *priv = IFOLDER_CLIENT_GET_PRIVATE (client);

	g_message ("ifolder_client_finalize()");

	/* custom stuff */
	g_key_file_free(priv->config_key_file);

	/* Free up the memory used by the domains */
	domainListPtr = priv->domains;
	while (domainListPtr != NULL)
	{
		domain = (iFolderDomain *)domainListPtr->data;
		priv->domains = g_slist_remove (priv->domains, domain);
		
		g_object_unref (domain);
		domainListPtr = priv->domains;
	}
	
	G_OBJECT_CLASS (ifolder_client_parent_class)->finalize (object);
}

iFolderClient *
ifolder_client_initialize(const gchar *data_path, GError **error)
{
	iFolderClient *client;
	iFolderClientPrivate *priv;
	gboolean b_initialized = FALSE;
	GError *err = NULL;
	gchar *config_key_file_path = NULL;

	if (singleton_client != NULL)
	{
		g_set_error(error,
					IFOLDER_CLIENT_ERROR,
					IFOLDER_ERR_ALREADY_INITIALIZED,
					_("The iFolder Client is already initialized."));
		return NULL;
	}

	client = ifolder_client_new();
	priv = IFOLDER_CLIENT_GET_PRIVATE (client);
	
	g_message("FIXME: Implement ifolder_client_initialize()");
	
	/* Load the configuration file */
	g_key_file_load_from_data_dirs(priv->config_key_file,
									IFOLDER_DEFAULT_CONFIG_FILE_NAME,
									&config_key_file_path,
									(GKeyFileFlags)(G_KEY_FILE_KEEP_COMMENTS | G_KEY_FILE_KEEP_TRANSLATIONS),
									&err);

	if (!err)
	{
		if (config_key_file_path)
		{
			g_debug("Loaded config file: %s", config_key_file_path);
			g_message("FIXME: Figure out if we should free config_key_file_path or if it's freed when GKeyFile is freed.");
		}
	}
	else
	{
		fprintf(stderr, "Could not find a user config file.  Using default settings.\n");
		g_clear_error(&err);
	}
	
	if (data_path == NULL)
		b_initialized = IFApplication::Initialize();
	else
		b_initialized = IFApplication::Initialize(data_path);
	
	if (!b_initialized)
	{
		g_object_unref(client);
		g_set_error(error,
					IFOLDER_CLIENT_ERROR,
					IFOLDER_ERR_INITIALIZE,
					_("Error initializing iFolder Client's core synchronization engine."));
		return NULL;
	}
	
	load_domains (client, &err);
	if (err != NULL)
	{
//		IFApplication::Uninitialize();
		g_object_unref (client);
		if (error != NULL)
			*error = err;
		
		return NULL;
	}

	/* If everything was initialized correctly, set the singleton instance and return. */
	singleton_client = client;
	
	return client;
}

void
ifolder_client_uninitialize(iFolderClient *client, GError **error)
{
	g_message("FIXME: Implement ifolder_client_uninitialize()");

	if (singleton_client == NULL)
	{
		g_set_error(error,
					IFOLDER_CLIENT_ERROR,
					IFOLDER_ERR_NOT_INITIALIZED,
					_("The iFolder Client is not initialized."));
		return;
	}
	
	g_message("FIXME: Implement ifolder_client_uninitialize()");

	g_object_unref (client);
	
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
	iFolderClientPrivate *priv;
	
	g_debug ("ifolder_client_get_all_domains()");
	if (client == NULL)
	{
		g_set_error (error,
					 IFOLDER_ERROR,
					 IFOLDER_ERR_INVALID_PARAMETER,
					 _("ifolder_client_get_all_domains() called with a NULL iFolderClient parameter."));
		return NULL;
	}

	priv = IFOLDER_CLIENT_GET_PRIVATE (client);

	g_debug ("# of domains: %d", g_slist_length (priv->domains));
	
	return g_slist_copy (priv->domains);
}

GSList *
ifolder_client_get_all_active_domains(iFolderClient *client, GError **error)
{
	GSList *active_domains = NULL;
	GSList *domainListPtr;
	iFolderClientPrivate *priv;
	
	if (client == NULL)
	{
		g_set_error (error,
					 IFOLDER_ERROR,
					 IFOLDER_ERR_INVALID_PARAMETER,
					 _("ifolder_client_get_all_active_domains() called with a NULL iFolderClient parameter."));
		return NULL;
	}
	
	priv = IFOLDER_CLIENT_GET_PRIVATE (client);

	for (domainListPtr = priv->domains; domainListPtr != NULL; domainListPtr = g_slist_next (domainListPtr))
	{
		if (ifolder_domain_is_active (IFOLDER_DOMAIN (domainListPtr->data)))
			active_domains = g_slist_prepend (active_domains, domainListPtr->data);
	}

	return active_domains;
}

iFolderDomain *
ifolder_client_get_default_domain(iFolderClient *client, GError **error)
{
	iFolderClientPrivate *priv;
	
	if (client == NULL)
	{
		g_set_error (error,
					 IFOLDER_ERROR,
					 IFOLDER_ERR_INVALID_PARAMETER,
					 _("ifolder_client_get_default_domain() called with a NULL iFolderClient parameter."));
		return NULL;
	}
	
	priv = IFOLDER_CLIENT_GET_PRIVATE (client);
	
	return priv->default_domain;
}

iFolderDomain *
ifolder_client_add_domain(iFolderClient *client, const gchar *host_address, const gchar *user_name, const gchar *password, gboolean remember_password, gboolean make_default, GError **error)
{
	iFolderDomain *domain;
	GError *err = NULL;
	gchar *tmpStr;
	iFolderClientPrivate *priv;
	
	if (client == NULL)
	{
		g_set_error (error,
					 IFOLDER_ERROR,
					 IFOLDER_ERR_INVALID_PARAMETER,
					 _("ifolder_client_add_domain() called with a NULL iFolderClient parameter."));
		return NULL;
	}
	
	priv = IFOLDER_CLIENT_GET_PRIVATE (client);

	g_message("FIXME: Implement ifolder_client_add_domain()");
	
	/* FIXME: Call the core code to add a domain.  If successful, create an iFolderDomain object and add it to the iFolderClientPrivate->domains list */

	domain = ifolder_domain_new ();

	tmpStr = g_strdup_printf ("ID: %s", host_address);
	ifolder_domain_set_id (domain, tmpStr);
	g_free (tmpStr);

	tmpStr = g_strdup_printf ("Name: %s", host_address);
	ifolder_domain_set_name (domain, tmpStr);
	g_free (tmpStr);

	tmpStr = g_strdup_printf ("Description: %s", host_address);
	ifolder_domain_set_description (domain, tmpStr);
	g_free (tmpStr);

	tmpStr = g_strdup_printf ("Version: %s", host_address);
	ifolder_domain_set_version (domain, tmpStr);
	g_free (tmpStr);

	ifolder_domain_set_host_address (domain, host_address);

	tmpStr = g_strdup_printf ("Machine Name: %s", host_address);
	ifolder_domain_set_machine_name (domain, tmpStr);
	g_free (tmpStr);

	tmpStr = g_strdup_printf ("OS Version: %s", host_address);
	ifolder_domain_set_os_version (domain, tmpStr);
	g_free (tmpStr);

	tmpStr = g_strdup_printf ("User Name: %s", host_address);
	ifolder_domain_set_user_name (domain, tmpStr);
	g_free (tmpStr);

	g_debug ("FIXME: If the core domain_add() call doesn't automatically log the account in, do so now!");

	ifolder_domain_set_is_authenticated (domain, TRUE);

	ifolder_domain_set_is_default (domain, FALSE);

	ifolder_domain_set_is_active (domain, TRUE);
	
	priv->domains = g_slist_prepend (priv->domains, domain);

	sleep (3); /* FIXME: Remove this sleep() call that simulates work */
	
	g_signal_emit (client, ifolder_client_signals[DOMAIN_ADDED], 0, domain);

	return domain;
}

void
ifolder_client_remove_domain(iFolderClient *client, iFolderDomain *domain, GError **error)
{
	iFolderClientPrivate *priv;
	GSList *foundNode;
	
	if (client == NULL)
	{
		g_set_error (error,
					 IFOLDER_ERROR,
					 IFOLDER_ERR_INVALID_PARAMETER,
					 _("ifolder_client_remove_domain() called with a NULL iFolderClient parameter."));
		return;
	}
	
	if (domain == NULL)
	{
		g_set_error (error,
					 IFOLDER_ERROR,
					 IFOLDER_ERR_INVALID_PARAMETER,
					 _("ifolder_client_remove_domain() called with a NULL iFolderDomain parameter."));
		return;
	}
	
	priv = IFOLDER_CLIENT_GET_PRIVATE (client);

	g_message("FIXME: Implement ifolder_client_remove_domain()");
	
	foundNode = g_slist_find (priv->domains, domain);
	if (foundNode == NULL)
	{
		g_set_error (error,
					 IFOLDER_ERROR,
					 IFOLDER_ERR_INVALID_PARAMETER,
					 _("ifolder_client_remove_domain() called with an iFolderDomain object that the client does not know about."));
		return;
	}
	
	/* FIXME: Call the core to remove the domain.  If successful, remove it from iFolderClientPrivate->domains */
	priv->domains = g_slist_remove (priv->domains, domain);
	
	g_signal_emit (client, ifolder_client_signals[DOMAIN_REMOVED], 0, domain);
	
	g_object_unref (domain); /* FIXME: Figure out if this will really delete the iFolderDomain object */
}

/**
 * Private functions
 */
iFolderClient *
ifolder_client_new (void)
{
	iFolderClient *client;
	
	client = IFOLDER_CLIENT(g_object_new (IFOLDER_CLIENT_TYPE, NULL));
	
	return client;
}

GKeyFile *
ifolder_client_get_config_key_file (GError **error)
{
	iFolderClientPrivate *priv;

	if (singleton_client == NULL)
	{
		g_set_error(error,
					IFOLDER_CLIENT_ERROR,
					IFOLDER_ERR_NOT_INITIALIZED,
					_("The iFolder Client is not initialized."));
		return NULL;
	}
	
	priv = IFOLDER_CLIENT_GET_PRIVATE (singleton_client);
	
	return priv->config_key_file;
}

static void
load_domains (iFolderClient *client, GError **error)
{
	g_message ("FIXME: Implement load_domains() in ifolder-client.cpp");
}
