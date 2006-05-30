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
	gpointer user_data;
	
	IFDomain *core_domain;
	ifweb::iFolderService *ifolder_service;
};

#define IFOLDER_DOMAIN_GET_PRIVATE(o) (G_TYPE_INSTANCE_GET_PRIVATE ((o), IFOLDER_DOMAIN_TYPE, iFolderDomainPrivate))

enum
{
	PROP_ID,
	PROP_NAME,
	PROP_DESCRIPTION,
	PROP_VERSION,
	PROP_MASTER_HOST,
	PROP_HOME_HOST,
	PROP_USER_NAME,
	PROP_USER_ID,
	PROP_AUTHENTICATED,
	PROP_DEFAULT,
	PROP_ACTIVE
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
	 * iFolderDomain:master-host:
	 * 
	 * The master host address of the domain.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_MASTER_HOST,
					g_param_spec_string ("master-host",
						_("Domain master host address"),
						_("The domain's host address of the domain."),
						NULL,
						G_PARAM_READABLE));

	/**
	 * iFolderDomain:home-host:
	 * 
	 * The home host address of the domain.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_HOME_HOST,
					g_param_spec_string ("home-host",
						_("Domain home host address"),
						_("The domain's home address of the domain."),
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
	 * iFolderDomain:user-id:
	 * 
	 * The internal ID of the user who was used to connect to the domain.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_USER_ID,
					g_param_spec_string ("user-id",
						_("Domain user ID"),
						_("The internal ID of the user who was used to connect to the domain."),
						NULL,
						G_PARAM_READABLE));

	/**
	 * iFolderDomain:authenticated:
	 * 
	 * Whether this domain is authenticated.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_AUTHENTICATED,
					g_param_spec_boolean ("authenticated",
						_("Authenticated"),
						_("Whether this domain is authenticated."),
						FALSE,
						G_PARAM_READABLE));

	/**
	 * iFolderDomain:default:
	 * 
	 * Whether this domain is the default domain.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_DEFAULT,
					g_param_spec_boolean ("default",
						_("Default"),
						_("Whether this domain is the default domain."),
						FALSE,
						G_PARAM_READABLE));

	/**
	 * iFolderDomain:active:
	 * 
	 * Whether this domain is active.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_ACTIVE,
					g_param_spec_boolean ("active",
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
	priv->user_data = NULL;
	
	priv->core_domain = NULL;
	priv->ifolder_service = NULL;
}

static void ifolder_domain_finalize(GObject *object)
{
	iFolderDomainPrivate *priv;

	g_debug (" ***** ifolder_domain_finalize() ***** ");
	
	priv = IFOLDER_DOMAIN_GET_PRIVATE (object);

	/* custom stuff */

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
			g_value_set_string (value, priv->core_domain->m_ID);
			break;
		case PROP_NAME:
			g_value_set_string (value, priv->core_domain->m_Name);
			break;
		case PROP_DESCRIPTION:
			g_value_set_string (value, priv->core_domain->m_Description);
			break;
		case PROP_VERSION:
			g_value_set_string (value, priv->core_domain->m_Version);
			break;
		case PROP_MASTER_HOST:
			g_value_set_string (value, priv->core_domain->m_MasterHost);
			break;
		case PROP_HOME_HOST:
			g_value_set_string (value, priv->core_domain->m_HomeHost);
			break;
		case PROP_USER_NAME:
			g_value_set_string (value, priv->core_domain->m_UserName);
			break;
		case PROP_USER_ID:
			g_value_set_string (value, priv->core_domain->m_UserID);
			break;
		case PROP_AUTHENTICATED:
			g_value_set_boolean (value, priv->core_domain->m_Authenticated);
			break;
		case PROP_ACTIVE:
			g_value_set_boolean (value, priv->core_domain->m_Active);
			break;
		case PROP_DEFAULT:
			g_value_set_boolean (value, priv->core_domain->m_Default);
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
ifolder_domain_new (IFDomain *core_domain)
{
	iFolderDomain *domain;
	iFolderDomainPrivate *priv;
	
	g_return_val_if_fail (core_domain != NULL, NULL);
	
	domain = IFOLDER_DOMAIN(g_object_new (IFOLDER_DOMAIN_TYPE, NULL));
	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	priv->core_domain = core_domain;
	
	/* Hook up the iFolderService with this object */
//	priv->ifolder_service = ifweb::IFServiceManager::GetiFolderService (core_domain->m_ID, core_domain->m_MasterHost);
	
	return domain;
}

const gchar *
ifolder_domain_get_id(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), "");

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);

	return priv->core_domain->m_ID;
}

const gchar *
ifolder_domain_get_name(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), "");

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->core_domain->m_Name;
}

const gchar *
ifolder_domain_get_description(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), "");

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->core_domain->m_Description;
}

const gchar *
ifolder_domain_get_version(iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), "");

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->core_domain->m_Version;
}

const gchar *
ifolder_domain_get_master_host (iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), "");

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->core_domain->m_MasterHost;
}

const gchar *
ifolder_domain_get_home_host (iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), "");

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->core_domain->m_HomeHost;
}

const gchar *
ifolder_domain_get_user_name (iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), "");

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->core_domain->m_UserName;
}

const gchar *
ifolder_domain_get_user_id (iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), "");

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->core_domain->m_UserID;
}

gboolean
ifolder_domain_is_authenticated (iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), FALSE);

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->core_domain->m_Authenticated;
}

gboolean
ifolder_domain_is_default (iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), FALSE);

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->core_domain->m_Default;
}

gboolean
ifolder_domain_is_active (iFolderDomain *domain)
{
	iFolderDomainPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), FALSE);

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	return priv->core_domain->m_Active;
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
ifolder_domain_log_in(iFolderDomain *domain, const char *password, gboolean remember_password, GError **error)
{
	iFolderDomainPrivate *priv;

	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	g_message ("FIXME: Do something to remember the password.");
	if (priv->core_domain->Login (password, error) != 0)
		return;

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
	
	g_signal_emit (singleton_client, ifolder_client_signals[DOMAIN_LOGGED_OUT], 0, domain);
}

void
ifolder_domain_activate(iFolderDomain *domain, GError **error)
{
	iFolderDomainPrivate *priv;

	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	if (priv->core_domain->m_Active) return; /* Don't do work that you don't need to do */

	g_message("FIXME: Implement ifolder_domain_activate to call IFDomain");
	
	g_signal_emit (singleton_client, ifolder_client_signals[DOMAIN_ACTIVATED], 0, domain);
}

void
ifolder_domain_inactivate(iFolderDomain *domain, GError **error)
{
	iFolderDomainPrivate *priv;

	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	if (!priv->core_domain->m_Active) return; /* Don't do work that you don't need to do */

	g_message("FIXME: Implement ifolder_domain_inactivate to call IFDomain");
	
	g_signal_emit (singleton_client, ifolder_client_signals [DOMAIN_INACTIVATED], 0, domain);
}

void
ifolder_domain_change_host_address(iFolderDomain *domain, const char *new_host_address, GError **error)
{
	g_return_if_fail (IFOLDER_IS_DOMAIN (domain));
	g_message("FIXME: Implement ifolder_domain_change_host_address");
	
	g_signal_emit (singleton_client, ifolder_client_signals [DOMAIN_HOST_MODIFIED], 0, domain);
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
	
	g_signal_emit (singleton_client, ifolder_client_signals [DOMAIN_NEW_DEFAULT], 0, domain);
}

iFolderUser *
ifolder_domain_get_authenticated_user(iFolderDomain *domain, GError **error)
{
	iFolderDomainPrivate *priv;
	iFolderUser *user;

	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), NULL);

	priv = IFOLDER_DOMAIN_GET_PRIVATE (domain);
	
	
	
	if (priv->core_domain->m_Active) return NULL; /* Don't do work that you don't need to do */

	g_message("FIXME: Implement ifolder_domain_activate to call IFDomain");
	
	g_signal_emit (singleton_client, ifolder_client_signals[DOMAIN_ACTIVATED], 0, domain);



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

/*
iFolderUser *
ifolder_domain_get_user_by_id (iFolderDomain *domain, const gchar *user_id, GError **error)
{
	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), NULL);
	g_return_val_if_fail (user_id != NULL);
	
	g_message ("FIXME: Implement ifolder_domain_get_user_by_id()");
	
	return NULL;
}
*/

iFolderUser *
ifolder_domain_get_user(iFolderDomain *domain, const gchar *user_name, GError **error)
{
	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), NULL);
	g_message("FIXME: Implement ifolder_domain_get_user()");
	
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
