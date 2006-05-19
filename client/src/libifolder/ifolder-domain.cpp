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

#include <unistd.h> /* FIXME: Remove this when done spoofing things with sleep() */

#include "ifolder-domain.h"
#include "ifolder-private.h"

typedef struct _iFolderDomainPrivate iFolderDomainPrivate;
struct _iFolderDomainPrivate
{
	gchar *id;
	gchar *name;
	gchar *description;
	gchar *version;
	gchar *host_address;
	gchar *machine_name;
	gchar *os_version;
	gchar *user_name;
	gboolean is_authenticated;
	gboolean is_default;
	gboolean is_active;
	gpointer user_data;
};

#define IFOLDER_DOMAIN_GET_PRIVATE(o) (G_TYPE_INSTANCE_GET_PRIVATE ((o), IFOLDER_DOMAIN_TYPE, iFolderDomainPrivate))

/**
 * Forward definitions
 */
static void ifolder_domain_finalize(GObject *object);

G_DEFINE_TYPE (iFolderDomain, ifolder_domain, G_TYPE_OBJECT);

/**
 * Functions required by GObject
 */
static void
ifolder_domain_class_init(iFolderDomainClass *klass)
{
	GObjectClass *object_class;
	
	object_class = (GObjectClass *)klass;
	
	object_class->finalize = ifolder_domain_finalize;

	g_type_class_add_private(klass, sizeof(iFolderDomainPrivate));

	/* custom stuff */
}

static void ifolder_domain_init(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	g_message("ifolder_domain_init() called");

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	/**
	 * Initialize all public and private members to reasonable default values.
	 * If you need specific construction properties to complete initialization,
	 * delay initialization completion until the property is set.
	 */
	priv->id = NULL;
	priv->name = NULL;
	priv->description = NULL;
	priv->version = NULL;
	priv->host_address = NULL;
	priv->machine_name = NULL;
	priv->os_version = NULL;
	priv->user_name = NULL;
	priv->is_authenticated = FALSE;
	priv->is_default = FALSE;
	priv->is_active = FALSE;
	priv->user_data = NULL;
}

static void ifolder_domain_finalize(GObject *object)
{
	iFolderDomainPrivate *priv;

	g_message (" ***** ifolder_domain_finalize() ***** ");
	
	priv = IFOLDER_DOMAIN_GET_PRIVATE (object);

	/* custom stuff */
	g_free(priv->id);
	g_free(priv->name);
	g_free(priv->description);
	g_free(priv->version);
	g_free(priv->host_address);
	g_free(priv->machine_name);
	g_free(priv->os_version);
	g_free(priv->user_name);

	/* Chain up the parent class */
	G_OBJECT_CLASS (ifolder_domain_parent_class)->finalize (object);
}

iFolderDomain *
ifolder_domain_new (void)
{
	iFolderDomain *domain;
	
	domain = IFOLDER_DOMAIN(g_object_new (IFOLDER_DOMAIN_TYPE, NULL));
	
	return domain;
}

const gchar *
ifolder_domain_get_id(iFolderDomain *domain)
{
	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return "";
	}

	return IFOLDER_DOMAIN_GET_PRIVATE (domain)->id;
}

void
ifolder_domain_set_id (iFolderDomain *domain, const gchar *id)
{
	iFolderDomainPrivate *priv;

	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return;
	}
	
	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	if (id == NULL)
	{
		g_free (priv->id);
		priv->id = NULL;
	}
	else
		priv->id = g_strdup (id);
}

const gchar *
ifolder_domain_get_name(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return "";
	}

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->name;
}

void
ifolder_domain_set_name (iFolderDomain *domain, const gchar *name)
{
	iFolderDomainPrivate *priv;

	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return;
	}
	
	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	if (name == NULL)
	{
		g_free (priv->name);
		priv->name = NULL;
	}
	else
		priv->name = g_strdup (name);
}

const gchar *
ifolder_domain_get_description(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return "";
	}

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->description;
}

void
ifolder_domain_set_description (iFolderDomain *domain, const gchar *description)
{
	iFolderDomainPrivate *priv;

	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return;
	}
	
	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	if (description == NULL)
	{
		g_free (priv->description);
		priv->description = NULL;
	}
	else
		priv->description = g_strdup (description);
}

const gchar *
ifolder_domain_get_version(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return "";
	}

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->version;
}

void
ifolder_domain_set_version (iFolderDomain *domain, const gchar *version)
{
	iFolderDomainPrivate *priv;

	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return;
	}
	
	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	if (version == NULL)
	{
		g_free (priv->version);
		priv->version = NULL;
	}
	else
		priv->version = g_strdup (version);
}

const gchar *
ifolder_domain_get_host_address(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return "";
	}

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->host_address;
}

void
ifolder_domain_set_host_address (iFolderDomain *domain, const gchar *host_address)
{
	iFolderDomainPrivate *priv;

	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return;
	}
	
	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	if (host_address == NULL)
	{
		g_free (priv->host_address);
		priv->host_address = NULL;
	}
	else
		priv->host_address = g_strdup (host_address);
}

const gchar *
ifolder_domain_get_machine_name(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return "";
	}

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->machine_name;
}

void
ifolder_domain_set_machine_name (iFolderDomain *domain, const gchar *machine_name)
{
	iFolderDomainPrivate *priv;

	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return;
	}
	
	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	if (machine_name == NULL)
	{
		g_free (priv->machine_name);
		priv->machine_name = NULL;
	}
	else
		priv->machine_name = g_strdup (machine_name);
}

const gchar *
ifolder_domain_get_os_version(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return "";
	}

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->os_version;
}

void
ifolder_domain_set_os_version (iFolderDomain *domain, const gchar *os_version)
{
	iFolderDomainPrivate *priv;

	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return;
	}
	
	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	if (os_version == NULL)
	{
		g_free (priv->os_version);
		priv->os_version = NULL;
	}
	else
		priv->os_version = g_strdup (os_version);
}

const gchar *
ifolder_domain_get_user_name(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return "";
	}

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->user_name;
}

void
ifolder_domain_set_user_name (iFolderDomain *domain, const gchar *user_name)
{
	iFolderDomainPrivate *priv;

	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return;
	}
	
	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	if (user_name == NULL)
	{
		g_free (priv->user_name);
		priv->user_name = NULL;
	}
	else
		priv->user_name = g_strdup (user_name);
}

gboolean
ifolder_domain_is_authenticated(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return false;
	}

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->is_authenticated;
}

void
ifolder_domain_set_is_authenticated (iFolderDomain *domain, gboolean is_authenticated)
{
	iFolderDomainPrivate *priv;

	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return;
	}
	
	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	priv->is_authenticated = is_authenticated;
}

gboolean
ifolder_domain_is_default(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return false;
	}

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->is_default;
}

void
ifolder_domain_set_is_default (iFolderDomain *domain, gboolean is_default)
{
	iFolderDomainPrivate *priv;

	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return;
	}
	
	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	priv->is_default = is_default;
}

gboolean
ifolder_domain_is_active(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return false;
	}

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->is_active;	
}

void
ifolder_domain_set_is_active (iFolderDomain *domain, gboolean is_active)
{
	iFolderDomainPrivate *priv;

	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return;
	}
	
	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	priv->is_active = is_active;
}

gpointer
ifolder_domain_get_user_data(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return NULL;
	}

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->user_data;
}

void
ifolder_domain_set_user_data(iFolderDomain *domain, gpointer user_data)
{
	iFolderDomainPrivate *priv;

	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return;
	}

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	priv->user_data = user_data;
}

void
ifolder_domain_log_in(iFolderDomain *domain, const char *password, gboolean remember_password, GError **error)
{
	g_message("FIXME: Implement ifolder_domain_login");
	sleep (3); /* FIXME: Remove this spoofed work */
}

void
ifolder_domain_log_out(iFolderDomain *domain, GError **error)
{
	g_message("FIXME: Implement ifolder_domain_log_out");
	sleep (3); /* FIXME: Remove this spoofed work */
}

void
ifolder_domain_activate(iFolderDomain *domain, GError **error)
{
	g_message("FIXME: Implement ifolder_domain_login");
}

void
ifolder_domain_inactivate(iFolderDomain *domain, GError **error)
{
	g_message("FIXME: Implement ifolder_domain_login");
}

void
ifolder_domain_change_host_address(iFolderDomain *domain, const char *new_host_address, GError **error)
{
	g_message("FIXME: Implement ifolder_domain_login");
}

void
ifolder_domain_set_credentials(iFolderDomain *domain, const char *password, const iFolderCredentialType credential_type, GError **error)
{
	g_message("FIXME: Implement ifolder_domain_login");
}

void
ifolder_domain_set_default(iFolderDomain *domain, GError **error)
{
	g_message("FIXME: Implement ifolder_domain_login");
}

iFolderUser *
ifolder_domain_get_authenticated_user(iFolderDomain *domain, GError **error)
{
	g_message("FIXME: Implement ifolder_domain_login");
	
	return NULL;
}

iFolderUserPolicy *
ifolder_domain_get_authenticated_user_policy(iFolderDomain *domain, GError **error)
{
	g_message("FIXME: Implement ifolder_domain_login");
	
	return NULL;
}

gboolean
ifolder_domain_check_for_updated_client(iFolderDomain *domain, char **new_version, const char *version_override, GError **error)
{
	g_message("FIXME: Implement ifolder_domain_login");

	return FALSE;
}

iFolderUser *
ifolder_domain_get_user(iFolderDomain *domain, const char *user_name, GError **error)
{
	g_message("FIXME: Implement ifolder_domain_login");
	
	return NULL;
}

GSList *
ifolder_domain_get_users(iFolderDomain *domain, const int index, const int count, GError **error)
{
	g_message("FIXME: Implement ifolder_domain_login");
	
	return g_slist_alloc();
}

GSList *
ifolder_domain_get_users_by_search(iFolderDomain *domain, const iFolderSearchProperty search_prop, const iFolderSearchOperation search_op, const char *pattern, const int index, const int count, GError **error)
{
	g_message("FIXME: Implement ifolder_domain_login");
	
	return g_slist_alloc();
}

iFolder *
ifolder_domain_create_ifolder_from_path(iFolderDomain *domain, const char *local_path, const char *description, GError **error)
{
	g_message("FIXME: Implement ifolder_domain_login");
	
	return NULL;
}

iFolder *
ifolder_domain_create_ifolder(iFolderDomain *domain, const char *name, const char *description, GError **error)
{
	g_message("FIXME: Implement ifolder_domain_login");
	
	return NULL;
}

void
ifolder_domain_delete_ifolder(iFolderDomain *domain, iFolder *ifolder, GError **error)
{
	g_message("FIXME: Implement ifolder_domain_login");
}

void
ifolder_domain_connect_ifolder(iFolderDomain *domain, iFolder *ifolder, const char *local_path, GError **error)
{
	g_message("FIXME: Implement ifolder_domain_login");
}

void
ifolder_domain_disconnect_ifolder(iFolderDomain *domain, iFolder *ifolder, GError **error)
{
	g_message("FIXME: Implement ifolder_domain_login");
}

GSList *
ifolder_domain_get_all_ifolders(iFolderDomain *domain, const int index, const int count, GError **error)
{
	g_message("FIXME: Implement ifolder_domain_login");
	
	return g_slist_alloc();
}

GSList *
ifolder_domain_get_connected_ifolders(iFolderDomain *domain, const int index, const int count, GError **error)
{
	g_message("FIXME: Implement ifolder_domain_login");
	
	return g_slist_alloc();
}

GSList *
ifolder_domain_get_disconnected_ifolders(iFolderDomain *domain, const int index, const int count, GError **error)
{
	g_message("FIXME: Implement ifolder_domain_login");
	
	return g_slist_alloc();
}

iFolder *
ifolder_domain_get_ifolder_by_id(iFolderDomain *domain, const char *id, GError **error)
{
	g_message("FIXME: Implement ifolder_domain_login");
	
	return NULL;
}

GSList *
ifolder_domain_get_ifolders_by_name(iFolderDomain *domain, const iFolderSearchOperation search_op, const char *pattern, const int index, const int count, GError **error)
{
	g_message("FIXME: Implement ifolder_domain_login");
	
	return g_slist_alloc();
}
