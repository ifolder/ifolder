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

#include "ifolder-user-policy.h"
#include "ifolder-user.h"
#include "ifolder-private.h"
#include "ifolder-errors.h"

/* FIXME: Remove the following 3 lines when we start including gettext */
#ifndef _
#define _
#endif

extern iFolderClient *singleton_client;

typedef struct _iFolderUserPolicyPrivate iFolderUserPolicyPrivate;
struct _iFolderUserPolicyPrivate
{
	iFolderUser *user;
	iFolderSoap::UserPolicy *user_policy;
};

#define IFOLDER_USER_POLICY_GET_PRIVATE(o) (G_TYPE_INSTANCE_GET_PRIVATE ((o), IFOLDER_USER_POLICY_TYPE, iFolderUserPolicyPrivate))

/**
 * Forward definitions
 */
static void ifolder_user_policy_finalize (GObject *object);

G_DEFINE_TYPE (iFolderUserPolicy, ifolder_user_policy, G_TYPE_OBJECT);


/**
 * Functions required by GObject
 */
static void
ifolder_user_policy_class_init(iFolderUserPolicyClass *klass)
{
	GObjectClass *object_class;
	
	object_class = (GObjectClass *)klass;
	
	object_class->finalize = ifolder_user_policy_finalize;
	
	g_type_class_add_private(klass, sizeof(iFolderUserPolicyPrivate));

	/* custom stuff */
}

static void ifolder_user_policy_init(iFolderUserPolicy *user_policy)
{
	iFolderUserPolicyPrivate *priv;

	g_debug("ifolder_user_policy_init()");

	priv = IFOLDER_USER_POLICY_GET_PRIVATE (user_policy);
	
	/**
	 * Initialize all public and private members to reasonable default values.
	 * If you need specific construction properties to complete initialization,
	 * delay initialization completion until the property is set.
	 */
	priv->user = NULL;
	priv->user_policy = NULL;
}

static void ifolder_user_policy_finalize(GObject *object)
{
	iFolderUserPolicyPrivate *priv;

	g_debug("ifolder_user_policy_finalize()");
	
	priv = IFOLDER_USER_POLICY_GET_PRIVATE (object);

	/* custom stuff */
	if (priv->user)
		g_object_unref (priv->user);
	if (priv->user_policy)
		delete (priv->user_policy);

	/* Chain up the parent class */
	G_OBJECT_CLASS (ifolder_user_policy_parent_class)->finalize (object);
}

iFolderUserPolicy *
ifolder_user_policy_new (iFolderUser *user, iFolderSoap::UserPolicy *policy)
{
	iFolderUserPolicy *user_policy;
	iFolderUserPolicyPrivate *priv;

	g_return_val_if_fail (IFOLDER_IS_USER (user), NULL);	
	g_return_val_if_fail (policy != NULL, NULL);
	
	user_policy = IFOLDER_USER_POLICY (g_object_new (IFOLDER_USER_POLICY_TYPE, NULL));
	priv = IFOLDER_USER_POLICY_GET_PRIVATE (user_policy);
	
	g_object_ref (user);
	priv->user = user;
	priv->user_policy = policy;
	
	return user_policy;
}

iFolderUser *
ifolder_user_policy_get_user(iFolderUserPolicy *user_policy)
{
	iFolderUserPolicyPrivate *priv;
	
	g_return_val_if_fail (IFOLDER_IS_USER_POLICY (user_policy), NULL);
	priv = IFOLDER_USER_POLICY_GET_PRIVATE (user_policy);
	
	return priv->user;
}

gboolean
ifolder_user_policy_get_login_enabled(iFolderUserPolicy *user_policy)
{
	iFolderUserPolicyPrivate *priv;
	
	g_return_val_if_fail (IFOLDER_IS_USER_POLICY (user_policy), FALSE);
	priv = IFOLDER_USER_POLICY_GET_PRIVATE (user_policy);
	
	return priv->user_policy->m_LoginEnabled;
}

gint64
ifolder_user_policy_get_space_limit(iFolderUserPolicy *user_policy)
{
	iFolderUserPolicyPrivate *priv;
	
	g_return_val_if_fail (IFOLDER_IS_USER_POLICY (user_policy), -1);
	priv = IFOLDER_USER_POLICY_GET_PRIVATE (user_policy);
	
	return priv->user_policy->m_SpaceLimit;
}

gint64
ifolder_user_policy_get_effective_space_limit(iFolderUserPolicy *user_policy)
{
	iFolderUserPolicyPrivate *priv;
	
	g_return_val_if_fail (IFOLDER_IS_USER_POLICY (user_policy), -1);
	priv = IFOLDER_USER_POLICY_GET_PRIVATE (user_policy);
	
	return priv->user_policy->m_SpaceLimitEffective;
}

gint64
ifolder_user_policy_get_file_size_limit(iFolderUserPolicy *user_policy)
{
	iFolderUserPolicyPrivate *priv;
	
	g_return_val_if_fail (IFOLDER_IS_USER_POLICY (user_policy), -1);
	priv = IFOLDER_USER_POLICY_GET_PRIVATE (user_policy);
	
	return priv->user_policy->m_FileSizeLimit;
}

gint64
ifolder_user_policy_get_effective_file_size_limit(iFolderUserPolicy *user_policy)
{
	iFolderUserPolicyPrivate *priv;
	
	g_return_val_if_fail (IFOLDER_IS_USER_POLICY (user_policy), -1);
	priv = IFOLDER_USER_POLICY_GET_PRIVATE (user_policy);
	
	return priv->user_policy->m_FileSizeLimitEffective;
}

gint64
ifolder_user_policy_get_space_used(iFolderUserPolicy *user_policy)
{
	iFolderUserPolicyPrivate *priv;
	
	g_return_val_if_fail (IFOLDER_IS_USER_POLICY (user_policy), -1);
	priv = IFOLDER_USER_POLICY_GET_PRIVATE (user_policy);
	
	return priv->user_policy->m_SpaceUsed;
}

gint64
ifolder_user_policy_get_space_available(iFolderUserPolicy *user_policy)
{
	iFolderUserPolicyPrivate *priv;
	
	g_return_val_if_fail (IFOLDER_IS_USER_POLICY (user_policy), -1);
	priv = IFOLDER_USER_POLICY_GET_PRIVATE (user_policy);
	
	return priv->user_policy->m_SpaceAvailable;
}

gint
ifolder_user_policy_get_sync_interval(iFolderUserPolicy *user_policy)
{
	iFolderUserPolicyPrivate *priv;
	
	g_return_val_if_fail (IFOLDER_IS_USER_POLICY (user_policy), -1);
	priv = IFOLDER_USER_POLICY_GET_PRIVATE (user_policy);
	
	return priv->user_policy->m_SyncInterval;
}

gint
ifolder_user_policy_get_effective_sync_interval(iFolderUserPolicy *user_policy)
{
	iFolderUserPolicyPrivate *priv;
	
	g_return_val_if_fail (IFOLDER_IS_USER_POLICY (user_policy), -1);
	priv = IFOLDER_USER_POLICY_GET_PRIVATE (user_policy);
	
	return priv->user_policy->m_SyncIntervalEffective;
}

GArray *
ifolder_user_policy_get_file_type_includes(iFolderUserPolicy *user_policy)
{
	iFolderUserPolicyPrivate *priv;
	
	g_return_val_if_fail (IFOLDER_IS_USER_POLICY (user_policy), NULL);
	priv = IFOLDER_USER_POLICY_GET_PRIVATE (user_policy);
	
	return priv->user_policy->m_FileTypesIncludes;
}

GArray *
ifolder_user_policy_get_effective_file_type_includes(iFolderUserPolicy *user_policy)
{
	iFolderUserPolicyPrivate *priv;
	
	g_return_val_if_fail (IFOLDER_IS_USER_POLICY (user_policy), NULL);
	priv = IFOLDER_USER_POLICY_GET_PRIVATE (user_policy);
	
	return priv->user_policy->m_FileTypesIncludesEffective;
}

GArray *
ifolder_user_policy_get_file_type_excludes(iFolderUserPolicy *user_policy)
{
	iFolderUserPolicyPrivate *priv;
	
	g_return_val_if_fail (IFOLDER_IS_USER_POLICY (user_policy), NULL);
	priv = IFOLDER_USER_POLICY_GET_PRIVATE (user_policy);
	
	return priv->user_policy->m_FileTypesExcludes;
}

GArray *
ifolder_user_policy_get_effective_file_type_excludes(iFolderUserPolicy *user_policy)
{
	iFolderUserPolicyPrivate *priv;
	
	g_return_val_if_fail (IFOLDER_IS_USER_POLICY (user_policy), NULL);
	priv = IFOLDER_USER_POLICY_GET_PRIVATE (user_policy);
	
	return priv->user_policy->m_FileTypesExcludesEffective;
}
