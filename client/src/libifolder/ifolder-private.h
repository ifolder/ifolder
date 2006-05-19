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

#include <IFDomain.h>

#include "ifolder-types.h"

iFolderClient *ifolder_client_new (void);

/**
 * iFolderDomain Functions
 */
iFolderDomain *ifolder_domain_new (void);
void ifolder_domain_set_id (iFolderDomain *domain, const gchar *id);
void ifolder_domain_set_name (iFolderDomain *domain, const gchar *name);
void ifolder_domain_set_description (iFolderDomain *domain, const gchar *description);
void ifolder_domain_set_version (iFolderDomain *domain, const gchar *version);
void ifolder_domain_set_host_address (iFolderDomain *domain, const gchar *host_address);
void ifolder_domain_set_machine_name (iFolderDomain *domain, const gchar *machine_name);
void ifolder_domain_set_os_version (iFolderDomain *domain, const gchar *os_version);
void ifolder_domain_set_user_name (iFolderDomain *domain, const gchar *user_name);
void ifolder_domain_set_is_authenticated (iFolderDomain *domain, gboolean is_authenticated);
void ifolder_domain_set_is_default (iFolderDomain *domain, gboolean is_default);
void ifolder_domain_set_is_active (iFolderDomain *domain, gboolean is_active);
void ifolder_domain_set_core_domain (iFolderDomain *domain, IFDomain *core_domain);

iFolder *ifolder_new (void);
iFolderUser *ifolder_user_new (void);
iFolderUserPolicy *ifolder_user_policy_new (void);

GKeyFile *ifolder_client_get_config_key_file (GError **error = NULL);
