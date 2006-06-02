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
#include "ifolder-user.h"
#include "ifolder-domain.h"
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
	gpointer user_data;
	iFolderDomain *domain;
	gboolean connected;
	iFolderState state;
	
	gpointer core_ifolder;
	ifweb::iFolderService *ifolder_service;
};

#define IFOLDER_GET_PRIVATE(o) (G_TYPE_INSTANCE_GET_PRIVATE ((o), IFOLDER_TYPE, iFolderPrivate))

enum
{
	PROP_ID,
	PROP_NAME,
	PROP_DESCRIPTION,
	PROP_LOCAL_PATH,
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
	 * iFolder:local-path:
	 * 
	 * The local path of the iFolder (only valid for a connected iFolder)..
	 * 
	 * Since: 3.5
	 */
	g_object_class_install_property (object_class,
					PROP_LOCAL_PATH,
					g_param_spec_string ("local-path",
					_("iFolder Local Path"),
					_(""),
					NULL,
					G_PARAM_READABLE));

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
						(guint) IFOLDER_STATE_DISCONNECTED,
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
	priv->user_data			= NULL;
	priv->domain			= NULL;
	priv->connected			= FALSE;
	priv->state				= IFOLDER_STATE_UNKNOWN;
	priv->core_ifolder		= NULL;
	priv->ifolder_service	= NULL;
}

static void ifolder_finalize(GObject *object)
{
	iFolderPrivate *priv;

	g_debug (" ***** ifolder_finalize() ***** ");
	
	priv = IFOLDER_GET_PRIVATE (object);

	/* custom stuff */
	if (priv->connected)
		delete ((IFiFolder *)priv->core_ifolder);
	else
		delete ((ifweb::iFolder *)priv->core_ifolder);
		
	
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
			g_value_set_string (value, ifolder_get_id (ifolder));
			break;
		case PROP_NAME:
			g_value_set_string (value, ifolder_get_name (ifolder));
			break;
		case PROP_DESCRIPTION:
			g_value_set_string (value, ifolder_get_description (ifolder));
			break;
		case PROP_LOCAL_PATH:
			g_value_set_string (value, ifolder_get_local_path (ifolder));
			break;
		case PROP_STATE:
			g_value_set_uint (value, (guint)ifolder_get_state (ifolder, NULL));
			break;
		default:
			G_OBJECT_WARN_INVALID_PROPERTY_ID (object, prop_id, pspec);
			break;
	}
}

static void
ifolder_set_property (GObject *object, guint prop_id, const GValue *value, GParamSpec *pspec)
{
	G_OBJECT_WARN_INVALID_PROPERTY_ID (object, prop_id, pspec);
}

iFolder *
ifolder_new_disconnected (iFolderDomain *domain, ifweb::iFolder *core_ifolder)
{
	iFolder *ifolder;
	iFolderPrivate *priv;

	g_debug ("iFolder::ifolder_new_disconnected");
	
	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), NULL);
	g_return_val_if_fail (core_ifolder != NULL, NULL);
	
	ifolder = IFOLDER(g_object_new (IFOLDER_TYPE, NULL));
	
	priv = IFOLDER_GET_PRIVATE (ifolder);

	priv->domain = domain;
	priv->connected = FALSE;
	priv->state = IFOLDER_STATE_DISCONNECTED;
	priv->core_ifolder = core_ifolder;
	
	/* Hook up the iFolderService with this object */
	priv->ifolder_service = ifweb::IFServiceManager::GetiFolderService (ifolder_domain_get_id (domain), ifolder_domain_get_master_host (domain));
	
	return ifolder;
}

iFolder *
ifolder_new_connected (iFolderDomain *domain, IFiFolder *core_ifolder)
{
	iFolder *ifolder;
	iFolderPrivate *priv;
	
	g_debug ("iFolder::ifolder_new_connected");

	g_return_val_if_fail (IFOLDER_IS_DOMAIN (domain), NULL);
	g_return_val_if_fail (core_ifolder != NULL, NULL);

	ifolder = IFOLDER(g_object_new (IFOLDER_TYPE, NULL));

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	priv->domain = domain;
	priv->connected = TRUE;
	priv->state = IFOLDER_STATE_SYNC_WAIT;
	priv->core_ifolder = core_ifolder;
	
	/* Hook up the iFolderService with this object */
	priv->ifolder_service = ifweb::IFServiceManager::GetiFolderService (ifolder_domain_get_id (domain), ifolder_domain_get_master_host (domain));
	
	return ifolder;
}

const gchar *
ifolder_get_id(iFolder *ifolder)
{
	iFolderPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), "");

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	if (!priv->connected)
		return ((ifweb::iFolder *)priv->core_ifolder)->ID;
	
	return NULL;	/* FIXME: Get the ID from IFiFolder */
}

const gchar *
ifolder_get_name(iFolder *ifolder)
{
	iFolderPrivate *priv;

g_debug ("iFolder::ifolder_get_name()");

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), "");

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	if (!priv->connected)
		return ((ifweb::iFolder *)priv->core_ifolder)->Name;
	
	return "Fake Name";	/* FIXME: Get the Name from IFiFolder */
}

const gchar *
ifolder_get_description(iFolder *ifolder)
{
	iFolderPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), "");

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	if (!priv->connected)
		return ((ifweb::iFolder *)priv->core_ifolder)->Description;
	
	return NULL;	/* FIXME: Get the Description from IFiFolder */
}

const gchar *
ifolder_get_local_path (iFolder *ifolder)
{
	iFolderPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), "");

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	if (!priv->connected)
		return NULL;
	
	return "/this/is/a/fake/path";	/* FIXME: Get the local path from IFiFolder */
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

gpointer
ifolder_get_core_ifolder (iFolder *ifolder)
{
	iFolderPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), NULL);

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	return priv->core_ifolder;
}

gboolean
ifolder_is_connected (iFolder *ifolder)
{
	iFolderPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), FALSE);

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	return priv->connected;
}

iFolderUser *
ifolder_get_owner (iFolder *ifolder)
{
	iFolderPrivate *priv;
	iFolderUser *user = NULL;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), NULL);

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	if (!priv->connected)
		user = ifolder_domain_get_user (priv->domain, ((ifweb::iFolder *)priv->core_ifolder)->OwnerID, NULL);
	else
	{
		/* FIXME: Get the OwnerID from IFiFolder */
	}
	
	return user;
}

iFolderDomain *
ifolder_get_domain (iFolder *ifolder)
{
	iFolderPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), NULL);

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	return priv->domain;
}

long
ifolder_get_size (iFolder *ifolder, GError **error)
{
	iFolderPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), -1);

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	if (!priv->connected)
		return ((ifweb::iFolder *)priv->core_ifolder)->Size;
	
	return -1;	/* FIXME: Get the Size from IFiFolder */
}

iFolderMemberRights
ifolder_get_rights (iFolder *ifolder, GError **error)
{
	iFolderPrivate *priv;
	iFolderMemberRights rights = IFOLDER_MEMBER_RIGHTS_DENY;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), IFOLDER_MEMBER_RIGHTS_DENY);

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	if (!priv->connected)
	{
		switch (((ifweb::iFolder *)priv->core_ifolder)->Rights)
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
	}
	
	return rights;	/* FIXME: Get the iFolderMemberRights from IFiFolder */
}

time_t
ifolder_get_created (iFolder *ifolder, GError **error)
{
	iFolderPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), 0);

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	if (!priv->connected)
		return ((ifweb::iFolder *)priv->core_ifolder)->Created;
	
	return 0;	/* FIXME: Get the Created time from IFiFolder */
}

time_t
ifolder_get_last_modified (iFolder *ifolder, GError **error)
{
	iFolderPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), 0);

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	if (!priv->connected)
		return ((ifweb::iFolder *)priv->core_ifolder)->LastModified;
	
	return 0;	/* FIXME: Get the LastModified time from IFiFolder */
}

gboolean
ifolder_is_published (iFolder *ifolder, GError **error)
{
	iFolderPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), FALSE);

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	if (!priv->connected)
		return ((ifweb::iFolder *)priv->core_ifolder)->Published;
	
	return FALSE;	/* FIXME: Get the Published value from IFiFolder */
}

gboolean
ifolder_is_enabled (iFolder *ifolder, GError **error)
{
	iFolderPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), FALSE);

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	if (!priv->connected)
		return ((ifweb::iFolder *)priv->core_ifolder)->Enabled;
	
	return FALSE;	/* FIXME: Get the Enabled value from IFiFolder */
}

long
ifolder_get_member_count (iFolder *ifolder, GError **error)
{
	iFolderPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), -1);

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	if (!priv->connected)
		return ((ifweb::iFolder *)priv->core_ifolder)->MemberCount;
	
	return -1;	/* FIXME: Get the MemberCount from IFiFolder */
}

iFolderState
ifolder_get_state (iFolder *ifolder, GError **error)
{
	iFolderPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), IFOLDER_STATE_UNKNOWN);

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	return priv->state;
}

long
ifolder_get_items_to_synchronize (iFolder *ifolder, GError **error)
{
	iFolderPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), -1);

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	if (!priv->connected)
	{
		g_set_error (error, IFOLDER_ERROR, IFOLDER_ERR_UNKNOWN, "You cannot call ifolder_get_items_to_synchronize() on a disconnected iFolder.");
		return -1;
	}
	
	return -1; /* FIXME: Get the number of items left to synchronize from IFiFolder */
}

void
ifolder_start_synchronization (iFolder *ifolder, bool sync_now, GError **error)
{
	iFolderPrivate *priv;

	g_return_if_fail (IFOLDER_IS_IFOLDER (ifolder));

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	if (!priv->connected)
	{
		g_set_error (error, IFOLDER_ERROR, IFOLDER_ERR_UNKNOWN, "You cannot call ifolder_start_synchronization() on a disconnected iFolder.");
		return;
	}
	
	g_message ("FIXME: IFiFolder should change to return a GError on IFiFolder::Sync()");
	((IFiFolder *)priv->core_ifolder)->Sync ();
}

void
ifolder_stop_synchronization (iFolder *ifolder, GError **error)
{
	iFolderPrivate *priv;

	g_return_if_fail (IFOLDER_IS_IFOLDER (ifolder));

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	if (!priv->connected)
	{
		g_set_error (error, IFOLDER_ERROR, IFOLDER_ERR_UNKNOWN, "You cannot call ifolder_stop_synchronization() on a disconnected iFolder.");
		return;
	}
	
	g_message ("FIXME: IFiFolder should change to return a GError on IFiFolder::Sync()");
	((IFiFolder *)priv->core_ifolder)->Stop ();
}

void
ifolder_resume_synchronization (iFolder *ifolder, bool sync_now, GError **error)
{
	g_return_if_fail (IFOLDER_IS_IFOLDER (ifolder));
}

iFolderUserIterator *
ifolder_get_members (iFolder *ifolder, int index, int count, GError **error)
{
	iFolderPrivate				*priv;
	iFolderUserIterator			*user_iter;
	ifweb::iFolderUserIterator	*core_user_iter;
	const gchar					*ifolder_id;
	GError						*err = NULL;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), NULL);
	g_return_val_if_fail (index >= 0, NULL);
	g_return_val_if_fail (count > 0, NULL);

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	if (priv->connected)
		ifolder_id = "";
		/** FIXME: Get the ID from IFiFolder
		ifolder_id = ((IFiFolder *)priv->core_ifolder)->ID;
		*/
	else
		ifolder_id = ((ifweb::iFolder *)priv->core_ifolder)->ID;

	core_user_iter = priv->ifolder_service->GetMembers (ifolder_id, index, count, &err);
	if (err)
	{
		g_propagate_error (error, err);
		return NULL;
	}
	
	user_iter = ifolder_user_iterator_new (core_user_iter);

	return user_iter;
}

gboolean
ifolder_set_member_rights (iFolder *ifolder, iFolderUser *member, iFolderMemberRights rights, GError **error)
{
	iFolderPrivate				*priv;
	const gchar					*ifolder_id;
	ifolder__Rights				core_rights;
	gboolean					success;
	GError						*err = NULL;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), FALSE);
	g_return_val_if_fail (IFOLDER_IS_USER (member), FALSE);

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	if (priv->connected)
		ifolder_id = "";
		/** FIXME: Get the ID from IFiFolder
		ifolder_id = ((IFiFolder *)priv->core_ifolder)->ID;
		*/
	else
		ifolder_id = ((ifweb::iFolder *)priv->core_ifolder)->ID;
	
	switch (rights)
	{
		case IFOLDER_MEMBER_RIGHTS_READ_ONLY:
			core_rights = ifolder__Rights__ReadOnly;
			break;
		case IFOLDER_MEMBER_RIGHTS_READ_WRITE:
			core_rights = ifolder__Rights__ReadWrite;
			break;
		case IFOLDER_MEMBER_RIGHTS_ADMIN:
			core_rights = ifolder__Rights__Admin;
			break;
		case IFOLDER_MEMBER_RIGHTS_DENY:
		default:
			core_rights = ifolder__Rights__Deny;
			break;
	}

	success = priv->ifolder_service->SetMemberRights (ifolder_id, ifolder_user_get_id (member), core_rights, &err);
	if (err)
	{
		g_propagate_error (error, err);
		return FALSE;
	}
	
	return success;
}

gboolean
ifolder_add_member (iFolder *ifolder, iFolderUser *member, iFolderMemberRights rights, GError **error)
{
	iFolderPrivate				*priv;
	const gchar					*ifolder_id;
	ifolder__Rights				core_rights;
	gboolean					success;
	GError						*err = NULL;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), FALSE);
	g_return_val_if_fail (IFOLDER_IS_USER (member), FALSE);

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	if (priv->connected)
		ifolder_id = "";
		/** FIXME: Get the ID from IFiFolder
		ifolder_id = ((IFiFolder *)priv->core_ifolder)->ID;
		*/
	else
		ifolder_id = ((ifweb::iFolder *)priv->core_ifolder)->ID;
	
	switch (rights)
	{
		case IFOLDER_MEMBER_RIGHTS_READ_ONLY:
			core_rights = ifolder__Rights__ReadOnly;
			break;
		case IFOLDER_MEMBER_RIGHTS_READ_WRITE:
			core_rights = ifolder__Rights__ReadWrite;
			break;
		case IFOLDER_MEMBER_RIGHTS_ADMIN:
			core_rights = ifolder__Rights__Admin;
			break;
		case IFOLDER_MEMBER_RIGHTS_DENY:
		default:
			core_rights = ifolder__Rights__Deny;
			break;
	}

	
	success = priv->ifolder_service->AddMember (ifolder_id, ifolder_user_get_id (member), core_rights, &err);
	if (err)
	{
		g_propagate_error (error, err);
		return FALSE;
	}
	
	return success;
}

gboolean
ifolder_remove_member (iFolder *ifolder, iFolderUser *member, GError **error)
{
	iFolderPrivate				*priv;
	const gchar					*ifolder_id;
	gboolean					success;
	GError						*err = NULL;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), FALSE);
	g_return_val_if_fail (IFOLDER_IS_USER (member), FALSE);

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	if (priv->connected)
		ifolder_id = "";
		/** FIXME: Get the ID from IFiFolder
		ifolder_id = ((IFiFolder *)priv->core_ifolder)->ID;
		*/
	else
		ifolder_id = ((ifweb::iFolder *)priv->core_ifolder)->ID;
	
	success = priv->ifolder_service->RemoveMember (ifolder_id, ifolder_user_get_id (member), &err);
	if (err)
	{
		g_propagate_error (error, err);
		return FALSE;
	}
	
	return success;
}

gboolean
ifolder_set_owner (iFolder *ifolder, iFolderUser *member, GError **error)
{
	iFolderPrivate				*priv;
	const gchar					*ifolder_id;
	gboolean					success;
	GError						*err = NULL;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), FALSE);
	g_return_val_if_fail (IFOLDER_IS_USER (member), FALSE);

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	if (priv->connected)
		ifolder_id = "";
		/** FIXME: Get the ID from IFiFolder
		ifolder_id = ((IFiFolder *)priv->core_ifolder)->ID;
		*/
	else
		ifolder_id = ((ifweb::iFolder *)priv->core_ifolder)->ID;
	
	success = priv->ifolder_service->SetiFolderOwner (ifolder_id, ifolder_user_get_id (member), &err);
	if (err)
	{
		g_propagate_error (error, err);
		return FALSE;
	}
	
	return success;
}

iFolderChangeEntryIterator *
ifolder_get_change_entries (iFolder *ifolder, int index, int count, GError **error)
{
	iFolderPrivate				*priv;
	iFolderChangeEntryIterator	*iter;
	ifweb::ChangeEntryIterator	*core_iter;
	const gchar					*ifolder_id;
	GError						*err = NULL;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), NULL);
	g_return_val_if_fail (index >= 0, NULL);
	g_return_val_if_fail (count > 0, NULL);

	priv = IFOLDER_GET_PRIVATE (ifolder);

	if (priv->connected)
		ifolder_id = "";
		/** FIXME: Get the ID from IFiFolder
		ifolder_id = ((IFiFolder *)priv->core_ifolder)->ID;
		*/
	else
		ifolder_id = ((ifweb::iFolder *)priv->core_ifolder)->ID;

	core_iter = priv->ifolder_service->GetChanges (ifolder_id, ifolder_id, index, count, &err);
	if (err)
	{
		g_propagate_error (error, err);
		return NULL;
	}
	
	iter = ifolder_change_entry_iterator_new (core_iter);

	return iter;
}

gboolean
ifolder_publish (iFolder *ifolder, GError **error)
{
	iFolderPrivate				*priv;
	const gchar					*ifolder_id;
	gboolean					success;
	GError						*err = NULL;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), FALSE);

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	if (priv->connected)
		ifolder_id = "";
		/** FIXME: Get the ID from IFiFolder
		ifolder_id = ((IFiFolder *)priv->core_ifolder)->ID;
		*/
	else
		ifolder_id = ((ifweb::iFolder *)priv->core_ifolder)->ID;
	
	success = priv->ifolder_service->PublishiFolder (ifolder_id, TRUE, &err);
	if (err)
	{
		g_propagate_error (error, err);
		return FALSE;
	}
	
	return success;
}

gboolean
ifolder_unpublish (iFolder *ifolder, GError **error)
{
	iFolderPrivate				*priv;
	const gchar					*ifolder_id;
	gboolean					success;
	GError						*err = NULL;

	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), FALSE);

	priv = IFOLDER_GET_PRIVATE (ifolder);
	
	if (priv->connected)
		ifolder_id = "";
		/** FIXME: Get the ID from IFiFolder
		ifolder_id = ((IFiFolder *)priv->core_ifolder)->ID;
		*/
	else
		ifolder_id = ((ifweb::iFolder *)priv->core_ifolder)->ID;
	
	success = priv->ifolder_service->PublishiFolder (ifolder_id, FALSE, &err);
	if (err)
	{
		g_propagate_error (error, err);
		return FALSE;
	}
	
	return success;
}

