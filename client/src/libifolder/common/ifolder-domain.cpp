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
#include "IFDomain.h"

#include "ifolder-domain.h"

const char *
ifolder_domain_get_id(iFolderDomain domain)
{
	return qPrintable(((IFDomain *)domain)->getId());
}

const char *
ifolder_domain_get_name(iFolderDomain domain)
{
	return qPrintable(((IFDomain *)domain)->getName());
}

const char *
ifolder_domain_get_description(iFolderDomain domain)
{
	return qPrintable(((IFDomain *)domain)->getDescription());
}

const char *
ifolder_domain_get_version(iFolderDomain domain)
{
	return qPrintable(((IFDomain *)domain)->getVersion());
}

const char *
ifolder_domain_get_host_address(iFolderDomain domain)
{
	return qPrintable(((IFDomain *)domain)->getHostAddress());
}

const char *
ifolder_domain_get_machine_name(iFolderDomain domain)
{
	return qPrintable(((IFDomain *)domain)->getMachineName());
}

const char *
ifolder_domain_get_os_version(iFolderDomain domain)
{
	return qPrintable(((IFDomain *)domain)->getOsVersion());
}

const char *
ifolder_domain_get_user_name(iFolderDomain domain)
{
	return qPrintable(((IFDomain *)domain)->getUserName());
}

bool 
ifolder_domain_get_is_default(iFolderDomain domain)
{
	return ((IFDomain *)domain)->getIsDefault();
}

bool 
ifolder_domain_get_is_active(iFolderDomain domain)
{
	return ((IFDomain *)domain)->getIsActive();
}

void *
ifolder_domain_get_user_data(iFolderDomain domain)
{
	return ((IFDomain *)domain)->getUserData();
}

void 
ifolder_domain_set_user_data(iFolderDomain domain, void *user_data)
{
	((IFDomain *)domain)->setUserData(user_data);
}

int
ifolder_domain_add(const char *host_address, const char *user_name, const char *password, bool make_default, iFolderDomain *ret_val)
{
	int err;
	IFDomain *domain;
	
	err = IFDomain::add(QString(host_address), QString(user_name), QString(password), make_default, &domain);
	if (err == IFOLDER_SUCCESS)
		*ret_val = (iFolderDomain)domain;

	return err;	
}

int
ifolder_domain_remove(iFolderDomain domain, bool delete_ifolders_on_server)
{
	int err;
	
	err = ((IFDomain *)domain)->remove(delete_ifolders_on_server);
	
	return err;
}

int
ifolder_domain_login(iFolderDomain domain, const char password)
{
	int err;
	
	err = ((IFDomain *)domain)->logIn(QString(password));
	
	return err;
}

int
ifolder_domain_logout(iFolderDomain domain)
{
	int err;
	
	err = ((IFDomain *)domain)->logOut();
	
	return err;
}

void
ifolder_domain_free(iFolderDomain domain)
{
	IFDomain *domainClass;
	
	domainClass = (IFDomain *)domain;
	
	delete domainClass;
}
