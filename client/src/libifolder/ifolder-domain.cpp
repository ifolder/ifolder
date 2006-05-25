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

#include <IFDomain.h>

#include "ifolder-domain.h"
#include "ifolder-private.h"
#include "ifolder-errors.h"

/* FIXME: Remove the following 3 lines when we start including gettext */
#ifndef _
#define _
#endif

extern iFolderClient *singleton_client;
extern guint ifolder_client_signals[LAST_SIGNAL];

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
	
	IFDomain *core_domain;
};

#define IFOLDER_DOMAIN_GET_PRIVATE(o) (G_TYPE_INSTANCE_GET_PRIVATE ((o), IFOLDER_DOMAIN_TYPE, iFolderDomainPrivate))

enum
{
	PROP_ID,
	PROP_NAME,
	PROP_DESCRIPTION,
	PROP_VERSION,
	PROP_HOST_ADDRESS,
	PROP_MACHINE_NAME,
	PROP_OS_VERSION,
	PROP_USER_NAME,
	PROP_IS_AUTHENTICATED,
	PROP_IS_DEFAULT,
	PROP_IS_ACTIVE
};

/**
 * Forward definitions
 */
static void ifolder_domain_finalize(GObject *object);
static void ifolder_domain_get_property (GObject *object, guint prop_id, GValue *value, GParamSpec *pspec);
//static void ifolder_domain_set_property (GObject *object, guint prop_id, const GValue *value, GParamSpec *pspec);

G_DEFINE_TYPE (iFolderDomain, ifolder_domain, G_TYPE_OBJECT);


/**
 * Functions required by GObject
 */
static void
ifolder_domain_class_init(iFolderDomainClass *klass)
{
	GObjectClass *object_class;
	
	object_class = (GObjectClass *)klass;
	
	object_class->get_property = ifolder_domain_get_property;
//	object_class->set_property = ifolder_domain_set_property;
	
	object_class->finalize = ifolder_domain_finalize;
	
	/**
	 * iFolderDomain:id:
	 * 
	 * The internal ID of the domain.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_ID,
					g_param_spec_string ("id",
						_("Domain ID"),
						_("The internal ID of the domain."),
						NULL,
						G_PARAM_READABLE));

	/**
	 * iFolderDomain:name:
	 * 
	 * The system name of the domain.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_NAME,
					g_param_spec_string ("name",
						_("Domain name"),
						_("The name of the domain (sometimes referred to as the \"System Name\""),
						NULL,
						G_PARAM_READABLE));

	/**
	 * iFolderDomain:description:
	 * 
	 * The description of the domain.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_DESCRIPTION,
					g_param_spec_string ("description",
						_("Domain description"),
						_("The description of the domain."),
						NULL,
						G_PARAM_READABLE));

	/**
	 * iFolderDomain:version:
	 * 
	 * The version of the domain.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_VERSION,
					g_param_spec_string ("version",
						_("Domain version"),
						_("The version of the domain."),
						NULL,
						G_PARAM_READABLE));

	/**
	 * iFolderDomain:host-address:
	 * 
	 * The host address of the domain.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_HOST_ADDRESS,
					g_param_spec_string ("host-address",
						_("Domain host address"),
						_("The host address of the domain."),
						NULL,
						G_PARAM_READABLE));

	/**
	 * iFolderDomain:machine-name:
	 * 
	 * The machine name of the domain.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_MACHINE_NAME,
					g_param_spec_string ("machine-name",
						_("Domain machine name"),
						_("The machine name of the domain."),
						NULL,
						G_PARAM_READABLE));

	/**
	 * iFolderDomain:os-version:
	 * 
	 * The operating system version of the domain.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_OS_VERSION,
					g_param_spec_string ("os-version",
						_("Domain operating system version"),
						_("The operating system version of the domain."),
						NULL,
						G_PARAM_READABLE));

	/**
	 * iFolderDomain:user-name:
	 * 
	 * The user name of the domain.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_USER_NAME,
					g_param_spec_string ("user-name",
						_("Domain user name"),
						_("The name of the user who was used to connect to the domain."),
						NULL,
						G_PARAM_READABLE));

	/**
	 * iFolderDomain:is-authenticated:
	 * 
	 * Whether this domain is authenticated.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_IS_AUTHENTICATED,
					g_param_spec_boolean ("is-authenticated",
						_("Authenticated"),
						_("Whether this domain is authenticated."),
						FALSE,
						G_PARAM_READABLE));

	/**
	 * iFolderDomain:is-default:
	 * 
	 * Whether this domain is the default domain.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_IS_DEFAULT,
					g_param_spec_boolean ("is-default",
						_("Default"),
						_("Whether this domain is the default domain."),
						FALSE,
						G_PARAM_READABLE));

	/**
	 * iFolderDomain:is-active:
	 * 
	 * Whether this domain is active.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_IS_ACTIVE,
					g_param_spec_boolean ("is-active",
						_("Active"),
						_("Whether this domain is active."),
						FALSE,
						G_PARAM_READABLE));

	g_type_class_add_private(klass, sizeof(iFolderDomainPrivate));

	/* custom stuff */
}

static void ifolder_domain_init(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	g_debug ("ifolder_domain_init() called");

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
	
	priv->core_domain = NULL;
}

static void ifolder_domain_finalize(GObject *object)
{
	iFolderDomainPrivate *priv;

	g_debug (" ***** ifolder_domain_finalize() ***** ");
	
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

static void
ifolder_domain_get_property (GObject *object, guint prop_id, GValue *value, GParamSpec *pspec)
{
	iFolderDomain *domain = IFOLDER_DOMAIN (object);
	iFolderDomainPrivate *priv = IFOLDER_DOMAIN_GET_PRIVATE (object);
	
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
		case PROP_VERSION:
			g_value_set_string (value, priv->version);
			break;
		case PROP_HOST_ADDRESS:
			g_value_set_string (value, priv->host_address);
			break;
		case PROP_MACHINE_NAME:
			g_value_set_string (value, priv->machine_name);
			break;
		case PROP_OS_VERSION:
			g_value_set_string (value, priv->os_version);
			break;
		case PROP_USER_NAME:
			g_value_set_string (value, priv->user_name);
			break;
		case PROP_IS_AUTHENTICATED:
			g_value_set_boolean (value, priv->is_authenticated);
			break;
		case PROP_IS_DEFAULT:
			g_value_set_boolean (value, priv->is_default);
			break;
		case PROP_IS_ACTIVE:
			g_value_set_boolean (value, priv->is_active);
			break;
		default:
			G_OBJECT_WARN_INVALID_PROPERTY_ID (object, prop_id, pspec);
			break;
	}
}

//static void
//ifolder_domain_set_property (GObject *object, guint prop_id, const GValue *value, GParamSpec *pspec)
//{
//}

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
	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), "");

	return IFOLDER_DOMAIN_GET_PRIVATE (domain)->id;
}

void
ifolder_domain_set_id (iFolderDomain *domain, const gchar *id)
{
	iFolderDomainPrivate *priv;

	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));
	
	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	if (id == NULL)
	{
		g_free (priv->id);
		priv->id = NULL;
	}
	else
		priv->id = g_strdup (id);
	
	g_object_notify (G_OBJECT (domain), "id");
}

const gchar *
ifolder_domain_get_name(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), "");

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->name;
}

void
ifolder_domain_set_name (iFolderDomain *domain, const gchar *name)
{
	iFolderDomainPrivate *priv;

	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));
	
	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	if (name == NULL)
	{
		g_free (priv->name);
		priv->name = NULL;
	}
	else
		priv->name = g_strdup (name);
	
	g_object_notify (G_OBJECT (domain), "name");
}

const gchar *
ifolder_domain_get_description(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), "");

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->description;
}

void
ifolder_domain_set_description (iFolderDomain *domain, const gchar *description)
{
	iFolderDomainPrivate *priv;

	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));
	
	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	if (description == NULL)
	{
		g_free (priv->description);
		priv->description = NULL;
	}
	else
		priv->description = g_strdup (description);

	g_object_notify (G_OBJECT (domain), "description");
}

const gchar *
ifolder_domain_get_version(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), "");

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->version;
}

void
ifolder_domain_set_version (iFolderDomain *domain, const gchar *version)
{
	iFolderDomainPrivate *priv;

	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));
	
	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	if (version == NULL)
	{
		g_free (priv->version);
		priv->version = NULL;
	}
	else
		priv->version = g_strdup (version);

	g_object_notify (G_OBJECT (domain), "version");
}

const gchar *
ifolder_domain_get_host_address(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), "");

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->host_address;
}

void
ifolder_domain_set_host_address (iFolderDomain *domain, const gchar *host_address)
{
	iFolderDomainPrivate *priv;

	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));
	
	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	if (host_address == NULL)
	{
		g_free (priv->host_address);
		priv->host_address = NULL;
	}
	else
		priv->host_address = g_strdup (host_address);

	g_object_notify (G_OBJECT (domain), "host-address");
}

const gchar *
ifolder_domain_get_machine_name(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), "");

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->machine_name;
}

void
ifolder_domain_set_machine_name (iFolderDomain *domain, const gchar *machine_name)
{
	iFolderDomainPrivate *priv;

	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));
	
	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	if (machine_name == NULL)
	{
		g_free (priv->machine_name);
		priv->machine_name = NULL;
	}
	else
		priv->machine_name = g_strdup (machine_name);

	g_object_notify (G_OBJECT (domain), "machine-name");
}

const gchar *
ifolder_domain_get_os_version(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), "");

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->os_version;
}

void
ifolder_domain_set_os_version (iFolderDomain *domain, const gchar *os_version)
{
	iFolderDomainPrivate *priv;

	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));
	
	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	if (os_version == NULL)
	{
		g_free (priv->os_version);
		priv->os_version = NULL;
	}
	else
		priv->os_version = g_strdup (os_version);

	g_object_notify (G_OBJECT (domain), "os-version");
}

const gchar *
ifolder_domain_get_user_name(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), "");

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->user_name;
}

void
ifolder_domain_set_user_name (iFolderDomain *domain, const gchar *user_name)
{
	iFolderDomainPrivate *priv;

	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));
	
	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	if (user_name == NULL)
	{
		g_free (priv->user_name);
		priv->user_name = NULL;
	}
	else
		priv->user_name = g_strdup (user_name);

	g_object_notify (G_OBJECT (domain), "user-name");
}

gboolean
ifolder_domain_is_authenticated(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), FALSE);

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->is_authenticated;
}

void
ifolder_domain_set_is_authenticated (iFolderDomain *domain, gboolean is_authenticated)
{
	iFolderDomainPrivate *priv;

	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));
	
	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	priv->is_authenticated = is_authenticated;

	g_object_notify (G_OBJECT (domain), "is-authenticated");
}

gboolean
ifolder_domain_is_default(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), FALSE);

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->is_default;
}

void
ifolder_domain_set_is_default (iFolderDomain *domain, gboolean is_default)
{
	iFolderDomainPrivate *priv;

	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));
	
	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	priv->is_default = is_default;

	g_object_notify (G_OBJECT (domain), "is-default");
}

gboolean
ifolder_domain_is_active(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), FALSE);

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->is_active;	
}

void
ifolder_domain_set_is_active (iFolderDomain *domain, gboolean is_active)
{
	iFolderDomainPrivate *priv;

	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));
	
	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	priv->is_active = is_active;

	g_object_notify (G_OBJECT (domain), "is-active");
}

gpointer
ifolder_domain_get_user_data(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), NULL);

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->user_data;
}

void
ifolder_domain_set_user_data(iFolderDomain *domain, gpointer user_data)
{
	iFolderDomainPrivate *priv;

	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	priv->user_data = user_data;
}

IFDomain *
ifolder_domain_get_core_domain (iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), NULL);

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->core_domain;
}

void
ifolder_domain_set_core_domain (iFolderDomain *domain, IFDomain *core_domain)
{
	iFolderDomainPrivate *priv;

	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	priv->core_domain = core_domain;
}

void
ifolder_domain_log_in(iFolderDomain *domain, const char *password, gboolean remember_password, GError **error)
{
	iFolderDomainPrivate *priv;

	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	if (priv->core_domain->Login (password, error) != 0)
		return;

	priv->is_authenticated = TRUE;

	g_signal_emit (singleton_client, ifolder_client_signals[DOMAIN_LOGGED_IN], 0, domain);
}

void
ifolder_domain_log_out(iFolderDomain *domain, GError **error)
{
	iFolderDomainPrivate *priv;

	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	if (priv->core_domain->Logout () != 0)
	{
		g_set_error (error,
					 IFOLDER_ERROR,
					 IFOLDER_ERR_UNKNOWN,
					 _("FIXME: IFDomain.Logout() should return a GError so we know what's going on."));
		return;
	}
	
	priv->is_authenticated = FALSE;

	g_signal_emit (singleton_client, ifolder_client_signals[DOMAIN_LOGGED_OUT], 0, domain);
}

void
ifolder_domain_activate(iFolderDomain *domain, GError **error)
{
	iFolderDomainPrivate *priv;

	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	if (priv->is_active) return; /* Don't do work that you don't need to do */

	g_message("FIXME: Implement ifolder_domain_activate to call IFDomain");
	
	priv->is_active = TRUE;
	
	g_signal_emit (singleton_client, ifolder_client_signals[DOMAIN_ACTIVATED], 0, domain);
}

void
ifolder_domain_inactivate(iFolderDomain *domain, GError **error)
{
	iFolderDomainPrivate *priv;

	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	if (!priv->is_active) return; /* Don't do work that you don't need to do */

	g_message("FIXME: Implement ifolder_domain_inactivate to call IFDomain");
	
	priv->is_active = FALSE;
	
	g_signal_emit (singleton_client, ifolder_client_signals[DOMAIN_INACTIVATED], 0, domain);
}

void
ifolder_domain_change_host_address(iFolderDomain *domain, const char *new_host_address, GError **error)
{
	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));
	g_message("FIXME: Implement ifolder_domain_change_host_address");
}

void
ifolder_domain_set_credentials(iFolderDomain *domain, const char *password, const iFolderCredentialType credential_type, GError **error)
{
	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));
	g_message("FIXME: Implement ifolder_domain_set_credentials");
}

void
ifolder_domain_set_default(iFolderDomain *domain, GError **error)
{
	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));
	g_message("FIXME: Implement ifolder_domain_set_default");
}

iFolderUser *
ifolder_domain_get_authenticated_user(iFolderDomain *domain, GError **error)
{
	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), NULL);
	g_message("FIXME: Implement ifolder_domain_get_authenticated_user");
	
	return NULL;
}

iFolderUserPolicy *
ifolder_domain_get_authenticated_user_policy(iFolderDomain *domain, GError **error)
{
	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), NULL);
	g_message("FIXME: Implement ifolder_domain_get_authenticated_user_policy");
	
	return NULL;
}

gboolean
ifolder_domain_check_for_updated_client(iFolderDomain *domain, char **new_version, const char *version_override, GError **error)
{
	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), FALSE);
	g_message("FIXME: Implement ifolder_domain_check_for_updated_client");

	return FALSE;
}

iFolderUser *
ifolder_domain_get_user(iFolderDomain *domain, const char *user_name, GError **error)
{
	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), NULL);
	g_message("FIXME: Implement ifolder_domain_get_user");
	
	return NULL;
}

GSList *
ifolder_domain_get_users(iFolderDomain *domain, const int index, const int count, GError **error)
{
	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), NULL);
	g_message("FIXME: Implement ifolder_domain_get_users");
	
	return g_slist_alloc();
}

GSList *
ifolder_domain_get_users_by_search(iFolderDomain *domain, const iFolderSearchProperty search_prop, const iFolderSearchOperation search_op, const char *pattern, const int index, const int count, GError **error)
{
	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), NULL);
	g_message("FIXME: Implement ifolder_domain_get_users_by_search");
	
	return g_slist_alloc();
}

iFolder *
ifolder_domain_create_ifolder_from_path(iFolderDomain *domain, const char *local_path, const char *description, GError **error)
{
	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), NULL);
	g_message("FIXME: Implement ifolder_domain_create_ifolder_from_path");
	
	return NULL;
}

iFolder *
ifolder_domain_create_ifolder(iFolderDomain *domain, const char *name, const char *description, GError **error)
{
	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), NULL);
	g_message("FIXME: Implement ifolder_domain_create_ifolder");
	
	return NULL;
}

void
ifolder_domain_delete_ifolder(iFolderDomain *domain, iFolder *ifolder, GError **error)
{
	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));
	g_message("FIXME: Implement ifolder_domain_delete_ifolder");
}

void
ifolder_domain_connect_ifolder(iFolderDomain *domain, iFolder *ifolder, const char *local_path, GError **error)
{
	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));
	g_message("FIXME: Implement ifolder_domain_connect_ifolder");
}

void
ifolder_domain_disconnect_ifolder(iFolderDomain *domain, iFolder *ifolder, GError **error)
{
	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));
	g_message("FIXME: Implement ifolder_domain_disconnect_ifolder");
}

GSList *
ifolder_domain_get_all_ifolders(iFolderDomain *domain, const int index, const int count, GError **error)
{
	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), NULL);
	g_message("FIXME: Implement ifolder_domain_get_all_ifolders");
	
	return g_slist_alloc();
}

GSList *
ifolder_domain_get_connected_ifolders(iFolderDomain *domain, const int index, const int count, GError **error)
{
	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), NULL);
	g_message("FIXME: Implement ifolder_domain_get_connected_ifolders");
	
	return g_slist_alloc();
}

GSList *
ifolder_domain_get_disconnected_ifolders(iFolderDomain *domain, const int index, const int count, GError **error)
{
	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), NULL);
	g_message("FIXME: Implement ifolder_domain_get_disconnected_ifolders");
	
	return g_slist_alloc();
}

iFolder *
ifolder_domain_get_ifolder_by_id(iFolderDomain *domain, const char *id, GError **error)
{
	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), NULL);
	g_message("FIXME: Implement ifolder_domain_get_ifolder_by_id");
	
	return NULL;
}

GSList *
ifolder_domain_get_ifolders_by_name(iFolderDomain *domain, const iFolderSearchOperation search_op, const char *pattern, const int index, const int count, GError **error)
{
	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), NULL);
	g_message("FIXME: Implement ifolder_domain_get_ifolders_by_name");
	
	return g_slist_alloc();
}
