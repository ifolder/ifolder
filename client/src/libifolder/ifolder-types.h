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

#ifndef IFOLDER_TYPES_H
#define IFOLDER_TYPES_H

#include <glib.h>
#include <glib-object.h>

/**
 * Type macros
 */
typedef struct _iFolderClient iFolderClient;
typedef struct _iFolderClientClass iFolderClientClass;

typedef struct _iFolderDomain iFolderDomain;
typedef struct _iFolderDomainClass iFolderDomainClass;

struct _iFolderClient
{
	GObject parent;
	
	/* instance members */
	
	/* private */
	gpointer private_data;
};

struct _iFolderClientClass
{
	GObjectClass parent;
	
	void (* domain_added) (iFolderClient *client, iFolderDomain *domain, gpointer user_data);
	void (* domain_removed) (iFolderClient *client, iFolderDomain *domain, gpointer user_data);
	void (* domain_logged_in) (iFolderClient *client, iFolderDomain *domain, gpointer user_data);
	void (* domain_logged_out) (iFolderClient *client, iFolderDomain *domain, gpointer user_data);
	
	void (* domain_host_modified) (iFolderClient *client, iFolderDomain *domain, gpointer user_data);
	void (* domain_activated) (iFolderClient *client, iFolderDomain *domain, gpointer user_data);
	void (* domain_inactivated) (iFolderClient *client, iFolderDomain *domain, gpointer user_data);
	void (* domain_new_default) (iFolderClient *client, iFolderDomain *domain, gpointer user_data);
	
	/* class members */
};

struct _iFolderDomain
{
	GObject parent;

	/* instance members */

	/* private */
	gpointer private_data;
};

struct _iFolderDomainClass
{
	GObjectClass parent;

	/* class members */
};

/**
 * iFolder
 */
typedef struct _iFolder iFolder;
typedef struct _iFolderClass iFolderClass;
typedef struct _iFolderPrivate iFolderPrivate;

struct _iFolder
{
	GObject parent;

	/* instance members */

	/* private */
	iFolderPrivate *priv;
};

struct _iFolderClass
{
	GObjectClass parent;

	/* class members */
};

/**
 * iFolderUser
 */
typedef struct _iFolderUser iFolderUser;
typedef struct _iFolderUserClass iFolderUserClass;
typedef struct _iFolderUserPrivate iFolderUserPrivate;

struct _iFolderUser
{
	GObject parent;

	/* instance members */

	/* private */
	iFolderUserPrivate *priv;
};

struct _iFolderUserClass
{
	GObjectClass parent;

	/* class members */
};

/**
 * iFolderUserPolicy
 */
typedef struct _iFolderUserPolicy iFolderUserPolicy;
typedef struct _iFolderUserPolicyClass iFolderUserPolicyClass;
typedef struct _iFolderUserPolicyPrivate iFolderUserPolicyPrivate;

struct _iFolderUserPolicy
{
	GObject parent;

	/* instance members */

	/* private */
	iFolderUserPolicyPrivate *priv;
};

struct _iFolderUserPolicyClass
{
	GObjectClass parent;

	/* class members */
};

#endif /* IFOLDER_TYPES_H */
