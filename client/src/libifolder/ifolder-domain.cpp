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

#include <stdio.h>
#include "ifolder-errors.h"
#include "ifolder-domain.h"

const char *
ifolder_domain_get_id(iFolderDomain domain)
{
	return "";
}

const char *
ifolder_domain_get_name(iFolderDomain domain)
{
	return "";
}

const char *
ifolder_domain_get_description(iFolderDomain domain)
{
	return "";
}

const char *
ifolder_domain_get_version(iFolderDomain domain)
{
	return "";
}

const char *
ifolder_domain_get_host_address(iFolderDomain domain)
{
	return "";
}

const char *
ifolder_domain_get_machine_name(iFolderDomain domain)
{
	return "";
}

const char *
ifolder_domain_get_os_version(iFolderDomain domain)
{
	return "";
}

const char *
ifolder_domain_get_user_name(iFolderDomain domain)
{
	return "";
}

bool 
ifolder_domain_get_is_default(iFolderDomain domain)
{
	return false;
}

bool 
ifolder_domain_get_is_active(iFolderDomain domain)
{
	return false;
}

void *
ifolder_domain_get_user_data(iFolderDomain domain)
{
	return NULL;
}

void 
ifolder_domain_set_user_data(iFolderDomain domain, void *user_data)
{
}

int
ifolder_domain_add(const char *host_address, const char *user_name, const char *password, bool make_default, iFolderDomain *ret_val)
{
	return IFOLDER_SUCCESS;
}

int
ifolder_domain_remove(iFolderDomain domain, bool delete_ifolders_on_server)
{
	return IFOLDER_SUCCESS;
}

int
ifolder_domain_login(iFolderDomain domain, const char password)
{
	return IFOLDER_SUCCESS;
}

int
ifolder_domain_logout(iFolderDomain domain)
{
	return IFOLDER_SUCCESS;
}

void
ifolder_domain_free(iFolderDomain domain)
{
}


int
ifolder_domain_run_client_update(const iFolderDomain domain)
{
	return IFOLDER_SUCCESS;
}

