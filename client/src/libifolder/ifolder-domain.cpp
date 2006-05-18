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

#include <unistd.h> /* FIXME: Remove this when done spoofing things with sleep() */

#include "ifolder-domain.h"
#include "ifolder-private.h"

struct _iFolderDomainPrivate
{
	gboolean dispose_has_run;
	
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

static GObjectClass *parent_class = NULL;

#define IFOLDER_DOMAIN_GET_PRIVATE(o) (G_TYPE_INSTANCE_GET_PRIVATE ((o), IFOLDER_DOMAIN_TYPE, iFolderDomainPrivate))

/**
 * Forward definitions
 */
static void ifolder_domain_finalize(GObject *object);
static void ifolder_domain_dispose(GObject *object);

/**
 * Functions required by GObject
 */
static void
ifolder_domain_class_init(iFolderDomainClass *klass)
{
	GObjectClass *object_class = (GObjectClass *)klass;
	parent_class = G_OBJECT_CLASS(g_type_class_peek_parent(klass));
	
	object_class->finalize = ifolder_domain_finalize;
	object_class->dispose = ifolder_domain_dispose;

	g_type_class_add_private(klass, sizeof(iFolderDomainPrivate));

	/* custom stuff */
}

static void ifolder_domain_init(GTypeInstance *instance, gpointer g_class)
{
	g_message("ifolder_domain_init() called");

	iFolderDomain *self = IFOLDER_DOMAIN(instance);
	self->priv = g_new0(iFolderDomainPrivate, 1);
	
	/**
	 * Initialize all public and private members to reasonable default values.
	 * If you need specific construction properties to complete initialization,
	 * delay initialization completion until the property is set.
	 */
	self->priv->dispose_has_run = FALSE;
	
	self->priv->id = NULL;
	self->priv->name = NULL;
	self->priv->description = NULL;
	self->priv->version = NULL;
	self->priv->host_address = NULL;
	self->priv->machine_name = NULL;
	self->priv->os_version = NULL;
	self->priv->user_name = NULL;
	self->priv->is_authenticated = FALSE;
	self->priv->is_default = FALSE;
	self->priv->is_active = FALSE;
	self->priv->user_data = NULL;
}

static void ifolder_domain_finalize(GObject *object)
{
	g_debug ("ifolder_domain_finalize()");
	
	iFolderDomain *self = IFOLDER_DOMAIN(object);

	/* custom stuff */
	if (self->priv->id)
		g_free(self->priv->id);
	if (self->priv->name)
		g_free(self->priv->name);
	if (self->priv->description)
		g_free(self->priv->description);
	if (self->priv->version)
		g_free(self->priv->version);
	if (self->priv->host_address)
		g_free(self->priv->host_address);
	if (self->priv->machine_name)
		g_free(self->priv->machine_name);
	if (self->priv->os_version)
		g_free(self->priv->os_version);
	if (self->priv->user_name)
		g_free(self->priv->user_name);

	g_free(self->priv);
}

static void ifolder_domain_dispose(GObject *object)
{
	iFolderDomain *self = IFOLDER_DOMAIN(object);
	
	/* if dispose already ran, return */
	if (self->priv->dispose_has_run)
		return;
	
	g_debug ("ifolder_domain_dispose()");
	
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
ifolder_domain_get_type(void)
{
	static GType type = 0;
	if (type == 0)
	{
		static const GTypeInfo info =
		{
			sizeof (iFolderDomainClass),
			NULL,	/* base_init */
			NULL,	/* base_finalize */
			(GClassInitFunc) ifolder_domain_class_init,	/* class_init */
			NULL,	/* class_finalize */
			NULL,	/* class_data */
			sizeof (iFolderDomain),
			0,		/* n_preallocs */
			(GInstanceInitFunc) ifolder_domain_init,	/* instance_init */
		};
		
		type = g_type_register_static(G_TYPE_OBJECT,
									  "iFolderDomainType",
									  &info, (GTypeFlags)0);
	}
	
	return type;
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

	return domain->priv->id;
}

void
ifolder_domain_set_id (iFolderDomain *domain, const gchar *id)
{
	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return;
	}
	
	if (id == NULL)
	{
		if (domain->priv->id != NULL)
		{
			g_free (domain->priv->id);
			domain->priv->id = NULL;
		}
	}
	else
		domain->priv->id = g_strdup (id);
}

const gchar *
ifolder_domain_get_name(iFolderDomain *domain)
{
	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return "";
	}

	return domain->priv->name;
}

void
ifolder_domain_set_name (iFolderDomain *domain, const gchar *name)
{
	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return;
	}
	
	if (name == NULL)
	{
		if (domain->priv->name != NULL)
		{
			g_free (domain->priv->name);
			domain->priv->name = NULL;
		}
	}
	else
		domain->priv->name = g_strdup (name);
}

const gchar *
ifolder_domain_get_description(iFolderDomain *domain)
{
	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return "";
	}

	return domain->priv->description;
}

void
ifolder_domain_set_description (iFolderDomain *domain, const gchar *description)
{
	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return;
	}
	
	if (description == NULL)
	{
		if (domain->priv->description != NULL)
		{
			g_free (domain->priv->description);
			domain->priv->description = NULL;
		}
	}
	else
		domain->priv->description = g_strdup (description);
}

const gchar *
ifolder_domain_get_version(iFolderDomain *domain)
{
	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return "";
	}

	return domain->priv->version;
}

void
ifolder_domain_set_version (iFolderDomain *domain, const gchar *version)
{
	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return;
	}
	
	if (version == NULL)
	{
		if (domain->priv->version != NULL)
		{
			g_free (domain->priv->version);
			domain->priv->version = NULL;
		}
	}
	else
		domain->priv->version = g_strdup (version);
}

const gchar *
ifolder_domain_get_host_address(iFolderDomain *domain)
{
	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return "";
	}

	return domain->priv->host_address;
}

void
ifolder_domain_set_host_address (iFolderDomain *domain, const gchar *host_address)
{
	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return;
	}
	
	if (host_address == NULL)
	{
		if (domain->priv->host_address != NULL)
		{
			g_free (domain->priv->host_address);
			domain->priv->host_address = NULL;
		}
	}
	else
		domain->priv->host_address = g_strdup (host_address);
}

const gchar *
ifolder_domain_get_machine_name(iFolderDomain *domain)
{
	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return "";
	}

	return domain->priv->machine_name;
}

void
ifolder_domain_set_machine_name (iFolderDomain *domain, const gchar *machine_name)
{
	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return;
	}
	
	if (machine_name == NULL)
	{
		if (domain->priv->machine_name != NULL)
		{
			g_free (domain->priv->machine_name);
			domain->priv->machine_name = NULL;
		}
	}
	else
		domain->priv->machine_name = g_strdup (machine_name);
}

const gchar *
ifolder_domain_get_os_version(iFolderDomain *domain)
{
	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return "";
	}

	return domain->priv->os_version;
}

void
ifolder_domain_set_os_version (iFolderDomain *domain, const gchar *os_version)
{
	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return;
	}
	
	if (os_version == NULL)
	{
		if (domain->priv->os_version != NULL)
		{
			g_free (domain->priv->os_version);
			domain->priv->os_version = NULL;
		}
	}
	else
		domain->priv->os_version = g_strdup (os_version);
}

const gchar *
ifolder_domain_get_user_name(iFolderDomain *domain)
{
	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return "";
	}

	return domain->priv->user_name;
}

void
ifolder_domain_set_user_name (iFolderDomain *domain, const gchar *user_name)
{
	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return;
	}
	
	if (user_name == NULL)
	{
		if (domain->priv->user_name != NULL)
		{
			g_free (domain->priv->user_name);
			domain->priv->user_name = NULL;
		}
	}
	else
		domain->priv->user_name = g_strdup (user_name);
}

gboolean
ifolder_domain_is_authenticated(iFolderDomain *domain)
{
	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return false;
	}

	return domain->priv->is_authenticated;
}

void
ifolder_domain_set_is_authenticated (iFolderDomain *domain, gboolean is_authenticated)
{
	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return;
	}
	
	domain->priv->is_authenticated = is_authenticated;
}

gboolean
ifolder_domain_is_default(iFolderDomain *domain)
{
	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return false;
	}

	return domain->priv->is_default;
}

void
ifolder_domain_set_is_default (iFolderDomain *domain, gboolean is_default)
{
	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return;
	}
	
	domain->priv->is_default = is_default;
}

gboolean
ifolder_domain_is_active(iFolderDomain *domain)
{
	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return false;
	}

	return domain->priv->is_active;	
}

void
ifolder_domain_set_is_active (iFolderDomain *domain, gboolean is_active)
{
	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return;
	}
	
	domain->priv->is_active = is_active;
}

gpointer
ifolder_domain_get_user_data(iFolderDomain *domain)
{
	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return NULL;
	}

	return domain->priv->user_data;
}

void
ifolder_domain_set_user_data(iFolderDomain *domain, gpointer user_data)
{
	if (domain == NULL)
	{
		g_critical("domain is NULL!");
		return;
	}

	domain->priv->user_data = user_data;
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
