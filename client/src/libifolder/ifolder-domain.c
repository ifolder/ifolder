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
	parent_class = g_type_class_peek_parent(klass);
	
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
	self->priv->is_default = FALSE;
	self->priv->is_active = FALSE;
	self->priv->user_data = NULL;
}

static void ifolder_domain_finalize(GObject *object)
{
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
									  &info, 0);
	}
	
	return type;
}

iFolderDomain *
ifolder_domain_new (void)
{
	iFolderDomain *domain;
	
	domain = g_object_new (IFOLDER_DOMAIN_TYPE, NULL);
	
	return domain;
}

