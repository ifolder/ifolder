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

#include <ifolder-errors.h>
#include <IFDomain.h>
#include <IFUser.h>
#include <IFiFolder.h>

iFolderDomain::iFolderDomain()
{
}

iFolderDomain::~iFolderDomain()
{
}

QString
iFolderDomain::id()
{
	QString x("Not Implemented");
	return x;
}

QString
iFolderDomain::name()
{
	QString x("Not Implemented");
	return x;
}

QString
iFolderDomain::description()
{
	QString x("Not Implemented");
	return x;
}

QString
iFolderDomain::version()
{
	QString x("Not Implemented");
	return x;
}

QString
iFolderDomain::hostAddress()
{
	QString x("Not Implemented");
	return x;
}

QString
iFolderDomain::machineName()
{
	QString x("Not Implemented");
	return x;
}

QString
iFolderDomain::osVersion()
{
	QString x("Not Implemented");
	return x;
}

QString
iFolderDomain::userName()
{
	QString x("Not Implemented");
	return x;
}

bool
iFolderDomain::isDefault()
{
	return false;
}

bool
iFolderDomain::isActive()
{
	return false;
}
		
void *
iFolderDomain::userData()
{
	return NULL;
}

void
iFolderDomain::setUserData(void *userData)
{
}

int
iFolderDomain::remove(bool deleteiFoldersOnServer)
{
	return IFOLDER_ERROR;
}

int
iFolderDomain::login(const char *password)
{
	return IFOLDER_ERROR;
}

int
iFolderDomain::logout()
{
	return IFOLDER_ERROR;
}

int
iFolderDomain::activate()
{
	return IFOLDER_ERROR;
}

int
iFolderDomain::inactivate()
{
	return IFOLDER_ERROR;
}

int
iFolderDomain::changeHostAddress(const char *newHostAddress)
{
	return IFOLDER_ERROR;
}

int
iFolderDomain::setCredentials(const char *password, CredentialType credentialType)
{
	return IFOLDER_ERROR;
}

int
iFolderDomain::setDefault()
{
	return IFOLDER_ERROR;
}

int
iFolderDomain::getAuthenticatedUser(iFolderUser *retVal)
{
	return IFOLDER_ERROR;
}

int
iFolderDomain::getUser(const char *userID, iFolderUser *retVal)
{
	return IFOLDER_ERROR;
}

int
iFolderDomain::getUsers(int index, int count, QList<iFolderUser> *retVal)
{
	return IFOLDER_ERROR;
}

int
iFolderDomain::getUsersBySearch(SearchProperty searchProp, SearchOperation searchOp, const char *pattern, int index, int count, QList<iFolderUser> *retVal)
{
	return IFOLDER_ERROR;
}

int
iFolderDomain::createiFolder(const char *localPath, const char *description, iFolder *retVal)
{
	return IFOLDER_ERROR;
}

int
iFolderDomain::deleteiFolder(iFolder ifolder)
{
	return IFOLDER_ERROR;
}

int
iFolderDomain::getLocaliFolders(int index, int count, QList<iFolder> *retVal)
{
	return IFOLDER_ERROR;
}

int
iFolderDomain::getRemoteiFolders(int index, int count, QList<iFolder> *retVal)
{
	return IFOLDER_ERROR;
}

int
iFolderDomain::getiFolderByID(const char *id, iFolder *retVal)
{
	return IFOLDER_ERROR;
}

int
iFolderDomain::getiFolderByName(const char *name, iFolder *retVal)
{
	return IFOLDER_ERROR;
}

int
iFolderDomain::getAll(QList<iFolderDomain> *retVal)
{
	return IFOLDER_ERROR;
}

int
iFolderDomain::getAllActive(QList<iFolderDomain> *retVal)
{
	return IFOLDER_ERROR;
}

int
iFolderDomain::getDefault(iFolderDomain *retVal)
{
	return IFOLDER_ERROR;
}

int
iFolderDomain::add(const char *hostAddress, const char *userName, const char *password, bool makeDefault, iFolderDomain *retVal)
{
	return IFOLDER_ERROR;
}

