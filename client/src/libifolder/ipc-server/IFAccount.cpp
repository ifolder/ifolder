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
#include <IFAccount.h>
#include <IFUser.h>
#include <IFiFolder.h>

iFolderAccount::iFolderAccount()
{
}

iFolderAccount::~iFolderAccount()
{
}

QString
iFolderAccount::id()
{
	QString x("Not Implemented");
	return x;
}

QString
iFolderAccount::name()
{
	QString x("Not Implemented");
	return x;
}

QString
iFolderAccount::description()
{
	QString x("Not Implemented");
	return x;
}

QString
iFolderAccount::version()
{
	QString x("Not Implemented");
	return x;
}

QString
iFolderAccount::hostAddress()
{
	QString x("Not Implemented");
	return x;
}

QString
iFolderAccount::machineName()
{
	QString x("Not Implemented");
	return x;
}

QString
iFolderAccount::osVersion()
{
	QString x("Not Implemented");
	return x;
}

QString
iFolderAccount::userName()
{
	QString x("Not Implemented");
	return x;
}

bool
iFolderAccount::isDefault()
{
	return false;
}

bool
iFolderAccount::isActive()
{
	return false;
}
		
void *
iFolderAccount::userData()
{
	return NULL;
}

void
iFolderAccount::setUserData(void *userData)
{
}

int
iFolderAccount::remove(bool deleteiFoldersOnServer)
{
	return IFOLDER_ERROR;
}

int
iFolderAccount::login(const char *password)
{
	return IFOLDER_ERROR;
}

int
iFolderAccount::logout()
{
	return IFOLDER_ERROR;
}

int
iFolderAccount::activate()
{
	return IFOLDER_ERROR;
}

int
iFolderAccount::inactivate()
{
	return IFOLDER_ERROR;
}

int
iFolderAccount::changeHostAddress(const char *newHostAddress)
{
	return IFOLDER_ERROR;
}

int
iFolderAccount::setCredentials(const char *password, CredentialType credentialType)
{
	return IFOLDER_ERROR;
}

int
iFolderAccount::setDefault()
{
	return IFOLDER_ERROR;
}

int
iFolderAccount::getAuthenticatedUser(iFolderUser *retVal)
{
	return IFOLDER_ERROR;
}

int
iFolderAccount::getUser(const char *userID, iFolderUser *retVal)
{
	return IFOLDER_ERROR;
}

int
iFolderAccount::getUsers(int index, int count, QList<iFolderUser> *retVal)
{
	return IFOLDER_ERROR;
}

int
iFolderAccount::getUsersBySearch(SearchProperty searchProp, SearchOperation searchOp, const char *pattern, int index, int count, QList<iFolderUser> *retVal)
{
	return IFOLDER_ERROR;
}

int
iFolderAccount::createiFolder(const char *localPath, const char *description, iFolder *retVal)
{
	return IFOLDER_ERROR;
}

int
iFolderAccount::deleteiFolder(iFolder ifolder)
{
	return IFOLDER_ERROR;
}

int
iFolderAccount::getLocaliFolders(int index, int count, QList<iFolder> *retVal)
{
	return IFOLDER_ERROR;
}

int
iFolderAccount::getRemoteiFolders(int index, int count, QList<iFolder> *retVal)
{
	return IFOLDER_ERROR;
}

int
iFolderAccount::getiFolderByID(const char *id, iFolder *retVal)
{
	return IFOLDER_ERROR;
}

int
iFolderAccount::getiFolderByName(const char *name, iFolder *retVal)
{
	return IFOLDER_ERROR;
}

int
iFolderAccount::getAll(QList<iFolderAccount> *retVal)
{
	return IFOLDER_ERROR;
}

int
iFolderAccount::getAllActive(QList<iFolderAccount> *retVal)
{
	return IFOLDER_ERROR;
}

int
iFolderAccount::getDefault(iFolderAccount *retVal)
{
	return IFOLDER_ERROR;
}

int
iFolderAccount::add(const char *hostAddress, const char *userName, const char *password, bool makeDefault, iFolderAccount *retVal)
{
	return IFOLDER_ERROR;
}

