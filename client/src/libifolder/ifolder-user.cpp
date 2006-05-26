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

#include "ifolder-user.h"
#include "ifolder-private.h"
#include "ifolder-errors.h"

/* FIXME: Remove the following 3 lines when we start including gettext */
#ifndef _
#define _
#endif

extern iFolderClient *singleton_client;
extern guint ifolder_client_signals[LAST_SIGNAL];

typedef struct _iFolderUserPrivate iFolderUserPrivate;
struct _iFolderUserPrivate
{
	gchar *id;
	gchar *user_name;
	gchar *full_name;
	iFolderMemberRights rights;
	gboolean login_enabled;
	gboolean owner;
	
	gpointer user_data;
	
//	IFiFolder *core_user;
};

#define IFOLDER_USER_GET_PRIVATE(o) (G_TYPE_INSTANCE_GET_PRIVATE ((o), IFOLDER_USER_TYPE, iFolderUserPrivate))

enum
{
	PROP_ID,
	PROP_USER_NAME,
	PROP_FULL_NAME,
	PROP_RIGHTS,
	PROP_LOGIN_ENABLED,
	PROP_OWNER
};

/**
 * Forward definitions
 */
static void ifolder_user_finalize (GObject *object);
static void ifolder_user_get_property (GObject *object, guint prop_id, GValue *value, GParamSpec *pspec);
static void ifolder_user_set_property (GObject *object, guint prop_id, const GValue *value, GParamSpec *pspec);

G_DEFINE_TYPE (iFolderUser, ifolder_user, G_TYPE_OBJECT);


/**
 * Functions required by GObject
 */
static void
ifolder_user_class_init(iFolderUserClass *klass)
{
	GObjectClass *object_class;
	
	object_class = (GObjectClass *)klass;
	
	object_class->get_property = ifolder_user_get_property;
	object_class->set_property = ifolder_user_set_property;
	
	object_class->finalize = ifolder_user_finalize;
	
	/**
	 * iFolderUser:id:
	 * 
	 * The internal ID of the user.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_ID,
					g_param_spec_string ("id",
						_("User ID"),
						_("The internal ID of the user."),
						NULL,
						G_PARAM_READABLE));

	/**
	 * iFolderUser:user-name:
	 * 
	 * The user name of the user (sometimes referred to as the login name).
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_USER_NAME,
					g_param_spec_string ("user-name",
						_("User name"),
						_("The user name of the user (sometimes referred to as the login name)."),
						NULL,
						G_PARAM_READABLE));

	/**
	 * iFolderUser:full-name:
	 * 
	 * The user's full name.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_FULL_NAME,
					g_param_spec_string ("full-name",
						_("Full name"),
						_("The user's full name."),
						NULL,
						G_PARAM_READABLE));

	/**
	 * iFolderUser:rights:
	 * 
	 * The user's rights.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_RIGHTS,
					g_param_spec_uint ("rights",
						_("Rights"),
						_("The user's rights."),
						0, /* FIXME: Replace this with min value */
						0,
						0,
						G_PARAM_READABLE));

	/**
	 * iFolderUser:login-enabled:
	 * 
	 * Whether the user's login is enabled.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_LOGIN_ENABLED,
					g_param_spec_boolean ("login-enabled",
						_("Login enabled"),
						_("Whether the user's login is enabled."),
						TRUE,
						G_PARAM_READABLE));

	/**
	 * iFolderUser:login-enabled:
	 * 
	 * Whether the user is an owner.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_OWNER,
					g_param_spec_boolean ("owner",
						_("Owner"),
						_("Whether the user is an owner."),
						FALSE,
						G_PARAM_READABLE));

	g_type_class_add_private(klass, sizeof(iFolderUserPrivate));

	/* custom stuff */
}

static void ifolder_user_init(iFolderUser *user)
{
	iFolderUserPrivate *priv;

	g_message("ifolder_user_init() called");

	priv = IFOLDER_USER_GET_PRIVATE (user);
	
	/**
	 * Initialize all public and private members to reasonable default values.
	 * If you need specific construction properties to complete initialization,
	 * delay initialization completion until the property is set.
	 */
	priv->id = NULL;
	priv->user_name = NULL;
	priv->full_name = NULL;
	priv->rights = IFOLDER_MEMBER_RIGHTS_READ_ONLY;
	priv->login_enabled = TRUE;
	priv->owner = FALSE;
	priv->user_data = NULL;
	
//	priv->core_ifolder = NULL;
}

static void ifolder_user_finalize(GObject *object)
{
	iFolderUserPrivate *priv;

	g_debug (" ***** ifolder_user_finalize() ***** ");
	
	priv = IFOLDER_USER_GET_PRIVATE (object);

	/* custom stuff */
	g_free(priv->id);
	g_free(priv->user_name);
	g_free(priv->full_name);

	/* Chain up the parent class */
	G_OBJECT_CLASS (ifolder_user_parent_class)->finalize (object);
}

static void
ifolder_user_get_property (GObject *object, guint prop_id, GValue *value, GParamSpec *pspec)
{
	iFolderUser *user = IFOLDER_USER (object);
	iFolderUserPrivate *priv = IFOLDER_USER_GET_PRIVATE (object);
	
	switch (prop_id)
	{
		case PROP_ID:
			g_value_set_string (value, priv->id);
			break;
		case PROP_USER_NAME:
			g_value_set_string (value, priv->user_name);
			break;
		case PROP_FULL_NAME:
			g_value_set_string (value, priv->full_name);
			break;
		case PROP_RIGHTS:
			g_value_set_uint (value, (guint)priv->rights);
			break;
		case PROP_LOGIN_ENABLED:
			g_value_set_boolean (value, (guint)priv->login_enabled);
			break;
		case PROP_OWNER:
			g_value_set_uint (value, (guint)priv->owner);
			break;
		default:
			G_OBJECT_WARN_INVALID_PROPERTY_ID (object, prop_id, pspec);
			break;
	}
}

static void
ifolder_user_set_property (GObject *object, guint prop_id, const GValue *value, GParamSpec *pspec)
{
	iFolderUser *user = IFOLDER_USER (object);
	iFolderUserPrivate *priv = IFOLDER_USER_GET_PRIVATE (object);
	
	switch (prop_id)
	{
		default:
			G_OBJECT_WARN_INVALID_PROPERTY_ID (object, prop_id, pspec);
			break;
	}
}

iFolderUser *
ifolder_user_new (void)
{
	iFolderUser *user;
	
	user = IFOLDER_USER(g_object_new (IFOLDER_USER_TYPE, NULL));
	
	return user;
}

const gchar *
ifolder_get_id(iFolderUser *user)
{
	g_return_val_if_fail (IFOLDER_IS_USER (user), "");

	return IFOLDER_USER_GET_PRIVATE (user)->id;
}

void
ifolder_set_id (iFolderUser *user, const gchar *id)
{
	iFolderUserPrivate *priv;

	g_return_if_fail (IFOLDER_IS_USER (user));
	
	priv = IFOLDER_USER_GET_PRIVATE (user);
	
	if (id == NULL)
	{
		g_free (priv->id);
		priv->id = NULL;
	}
	else
		priv->id = g_strdup (id);
	
	g_object_notify (G_OBJECT (user), "id");
}

const gchar *
ifolder_user_get_user_name(iFolderUser *user)
{
	iFolderUserPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_USER (user), "");
	
	return IFOLDER_USER_GET_PRIVATE (user)->user_name;
}

void
ifolder_user_set_user_name(iFolderUser *user, const gchar *user_name)
{
	iFolderUserPrivate *priv;

	g_return_if_fail (IFOLDER_IS_USER (user));
	
	priv = IFOLDER_USER_GET_PRIVATE (user);
	
	if (user_name == NULL)
	{
		g_free (priv->user_name);
		priv->user_name = NULL;
	}
	else
		priv->user_name = g_strdup (user_name);
	
	g_object_notify (G_OBJECT (user), "user-name");
}

const gchar *
ifolder_user_get_full_name(iFolderUser *user)
{
	iFolderUserPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_USER (user), "");
	
	return IFOLDER_USER_GET_PRIVATE (user)->full_name;
}

void
ifolder_user_set_full_name(iFolderUser *user, const gchar *full_name)
{
	iFolderUserPrivate *priv;

	g_return_if_fail (IFOLDER_IS_USER (user));
	
	priv = IFOLDER_USER_GET_PRIVATE (user);
	
	if (full_name == NULL)
	{
		g_free (priv->full_name);
		priv->full_name = NULL;
	}
	else
		priv->full_name = g_strdup (full_name);
	
	g_object_notify (G_OBJECT (user), "full-name");
}

iFolderMemberRights
ifolder_user_get_rights(iFolderUser *user)
{
	iFolderUserPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_USER (user), IFOLDER_MEMBER_RIGHTS_DENY);
	
	return IFOLDER_USER_GET_PRIVATE (user)->rights;
}

void
ifolder_user_set_rights(iFolderUser *user, iFolderMemberRights rights)
{
	iFolderUserPrivate *priv;

	g_return_if_fail (IFOLDER_IS_USER (user));
	
	priv = IFOLDER_USER_GET_PRIVATE (user);
	
	priv->rights = rights;
	
	g_object_notify (G_OBJECT (user), "rights");
}

gboolean
ifolder_user_is_login_enabled(iFolderUser *user)
{
	iFolderUserPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_USER (user), FALSE);
	
	return IFOLDER_USER_GET_PRIVATE (user)->login_enabled;
}

void
ifolder_user_set_is_login_enabled(iFolderUser *user, gboolean is_login_enabled)
{
	iFolderUserPrivate *priv;

	g_return_if_fail (IFOLDER_IS_USER (user));
	
	priv = IFOLDER_USER_GET_PRIVATE (user);
	
	priv->login_enabled = is_login_enabled;
	
	g_object_notify (G_OBJECT (user), "login-enabled");
}

gboolean
ifolder_user_is_owner(iFolderUser *user)
{
	iFolderUserPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_USER (user), FALSE);
	
	return IFOLDER_USER_GET_PRIVATE (user)->owner;
}

void
ifolder_user_set_is_owner(iFolderUser *user, gboolean is_owner)
{
	iFolderUserPrivate *priv;

	g_return_if_fail (IFOLDER_IS_USER (user));
	
	priv = IFOLDER_USER_GET_PRIVATE (user);
	
	priv->owner = is_owner;
	
	g_object_notify (G_OBJECT (user), "owner");
}

gpointer
ifolder_get_user_data(iFolderUser *user)
{
	iFolderUserPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_USER (user), NULL);

	priv = IFOLDER_USER_GET_PRIVATE (user);
	
	return priv->user_data;
}

void
ifolder_set_user_data(iFolderUser *user, gpointer user_data)
{
	iFolderUserPrivate *priv;

	g_return_if_fail (IFOLDER_IS_USER (user));

	priv = IFOLDER_USER_GET_PRIVATE (user);
	
	priv->user_data = user_data;
}
