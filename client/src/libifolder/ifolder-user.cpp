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
	gpointer user_data;
	
	ifweb::iFolderUser *core_user;
};

#define IFOLDER_USER_GET_PRIVATE(o) (G_TYPE_INSTANCE_GET_PRIVATE ((o), IFOLDER_USER_TYPE, iFolderUserPrivate))

enum
{
	PROP_ID,
	PROP_USER_NAME,
	PROP_FULL_NAME,
	PROP_FIRST_NAME,
	PROP_LAST_NAME,
	PROP_RIGHTS,
	PROP_ENABLED,
	PROP_OWNER,
	PROP_EMAIL
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
	 * iFolderUser:first-name:
	 * 
	 * The user's first name.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_FIRST_NAME,
					g_param_spec_string ("first-name",
						_("First name"),
						_("The user's first name."),
						NULL,
						G_PARAM_READABLE));

	/**
	 * iFolderUser:last-name:
	 * 
	 * The user's last name.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_LAST_NAME,
					g_param_spec_string ("last-name",
						_("Last name"),
						_("The user's last name."),
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
	 * iFolderUser:enabled:
	 * 
	 * Whether the user's login is enabled.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_ENABLED,
					g_param_spec_boolean ("enabled",
						_("Login enabled"),
						_("Whether the user's login is enabled."),
						TRUE,
						G_PARAM_READABLE));

	/**
	 * iFolderUser:owner:
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

	/**
	 * iFolderUser:email:
	 * 
	 * The user's email address.
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_EMAIL,
					g_param_spec_string ("email",
						_("Email"),
						_("The user's email address."),
						NULL,
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

	priv->user_data = NULL;
	priv->core_user = NULL;
}

static void ifolder_user_finalize(GObject *object)
{
	iFolderUserPrivate *priv;

	g_debug (" ***** ifolder_user_finalize() ***** ");
	
	priv = IFOLDER_USER_GET_PRIVATE (object);

	/* custom stuff */
	delete (priv->core_user);

	/* Chain up the parent class */
	G_OBJECT_CLASS (ifolder_user_parent_class)->finalize (object);
}

static void
ifolder_user_get_property (GObject *object, guint prop_id, GValue *value, GParamSpec *pspec)
{
	iFolderUser *user = IFOLDER_USER (object);
	
	switch (prop_id)
	{
		case PROP_ID:
			g_value_set_string (value, ifolder_user_get_id (user));
			break;
		case PROP_USER_NAME:
			g_value_set_string (value, ifolder_user_get_user_name (user));
			break;
		case PROP_FULL_NAME:
			g_value_set_string (value, ifolder_user_get_full_name (user));
			break;
		case PROP_FIRST_NAME:
			g_value_set_string (value, ifolder_user_get_first_name (user));
			break;
		case PROP_LAST_NAME:
			g_value_set_string (value, ifolder_user_get_last_name (user));
			break;
		case PROP_RIGHTS:
			g_value_set_uint (value, (guint)ifolder_user_get_rights(user));
			break;
		case PROP_ENABLED:
			g_value_set_boolean (value, (guint)ifolder_user_is_login_enabled (user));
			break;
		case PROP_OWNER:
			g_value_set_uint (value, (guint)ifolder_user_is_owner (user));
			break;
		case PROP_EMAIL:
			g_value_set_string (value, ifolder_user_get_email (user));
			break;
		default:
			G_OBJECT_WARN_INVALID_PROPERTY_ID (object, prop_id, pspec);
			break;
	}
}

static void
ifolder_user_set_property (GObject *object, guint prop_id, const GValue *value, GParamSpec *pspec)
{
	switch (prop_id)
	{
		default:
			G_OBJECT_WARN_INVALID_PROPERTY_ID (object, prop_id, pspec);
			break;
	}
}

iFolderUser *
ifolder_user_new (ifweb::iFolderUser *core_user)
{
	iFolderUser *user;
	iFolderUserPrivate *priv;
	
	g_return_val_if_fail (core_user != NULL, NULL);
	
	user = IFOLDER_USER(g_object_new (IFOLDER_USER_TYPE, NULL));
	priv = IFOLDER_USER_GET_PRIVATE (user);
	
	priv->core_user = core_user;
	
	return user;
}

const gchar *
ifolder_user_get_id (iFolderUser *user)
{
	iFolderUserPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_USER (user), "");
	
	priv = IFOLDER_USER_GET_PRIVATE (user);
	
	return priv->core_user->ID;
}

const gchar *
ifolder_user_get_user_name (iFolderUser *user)
{
	iFolderUserPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_USER (user), "");
	
	priv = IFOLDER_USER_GET_PRIVATE (user);
	
	return priv->core_user->UserName;
}

const gchar *
ifolder_user_get_full_name(iFolderUser *user)
{
	iFolderUserPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_USER (user), "");
	
	priv = IFOLDER_USER_GET_PRIVATE (user);
	
	return priv->core_user->FullName;
}

const gchar *
ifolder_user_get_first_name(iFolderUser *user)
{
	iFolderUserPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_USER (user), "");
	
	priv = IFOLDER_USER_GET_PRIVATE (user);
	
	return priv->core_user->FirstName;
}

const gchar *
ifolder_user_get_last_name(iFolderUser *user)
{
	iFolderUserPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_USER (user), "");
	
	priv = IFOLDER_USER_GET_PRIVATE (user);
	
	return priv->core_user->LastName;
}

iFolderMemberRights
ifolder_user_get_rights (iFolderUser *user)
{
	iFolderUserPrivate *priv;
	iFolderMemberRights rights;

	g_return_val_if_fail (IFOLDER_IS_USER (user), IFOLDER_MEMBER_RIGHTS_DENY);
	
	priv = IFOLDER_USER_GET_PRIVATE (user);

	switch (priv->core_user->Rights)
	{
		case ifolder__Rights__ReadOnly:
			rights = IFOLDER_MEMBER_RIGHTS_READ_ONLY;
			break;
		case ifolder__Rights__ReadWrite:
			rights = IFOLDER_MEMBER_RIGHTS_READ_WRITE;
			break;
		case ifolder__Rights__Admin:
			rights = IFOLDER_MEMBER_RIGHTS_ADMIN;
			break;
		case ifolder__Rights__Deny:
		default:
			rights = IFOLDER_MEMBER_RIGHTS_DENY;
			break;
	}

	return rights;
}

gboolean
ifolder_user_is_login_enabled(iFolderUser *user)
{
	iFolderUserPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_USER (user), FALSE);
	
	priv = IFOLDER_USER_GET_PRIVATE (user);
	
	return priv->core_user->Enabled;
}

gboolean
ifolder_user_is_owner(iFolderUser *user)
{
	iFolderUserPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_USER (user), FALSE);
	
	priv = IFOLDER_USER_GET_PRIVATE (user);
	
	return priv->core_user->IsOwner;
}

const gchar *
ifolder_user_get_email (iFolderUser *user)
{
	iFolderUserPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_USER (user), "");
	
	priv = IFOLDER_USER_GET_PRIVATE (user);
	
	return priv->core_user->Email;
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
