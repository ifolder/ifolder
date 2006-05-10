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

#include "../ifolder-errors.h"
#include <IFDomain.h>
#include <IFUser.h>
#include <IFiFolder.h>
#include <IFIPCClient.h>

IFDomain::IFDomain(QString id, QString name, QString description,
					  QString version, QString hostAddress,
					  QString machineName, QString osVersion,
					  QString userName, bool isDefault, bool isActive,
					  void *userData) :
	id(id), name(name), description(description), version(version), hostAddress(hostAddress),
	machineName(machineName), osVersion(osVersion), userName(userName), isDefault(isDefault),
	isActive(isActive), userData(userData)
{
}

IFDomain::~IFDomain()
{
}

QString
IFDomain::getId()
{
	return id;
}

QString
IFDomain::getName()
{
	return name;
}

QString
IFDomain::getDescription()
{
	return description;
}

QString
IFDomain::getVersion()
{
	return version;
}

QString
IFDomain::getHostAddress()
{
	return hostAddress;
}

QString
IFDomain::getMachineName()
{
	return machineName;
}

QString
IFDomain::getOsVersion()
{
	return osVersion;
}

QString
IFDomain::getUserName()
{
	return userName;
}

bool
IFDomain::getIsDefault()
{
	return isDefault;
}

bool
IFDomain::getIsActive()
{
	return isActive;
}
		
void *
IFDomain::getUserData()
{
	return userData;
}

void
IFDomain::setUserData(void *userData)
{
	this->userData = userData;
}

int
IFDomain::add(QString hostAddress, QString userName, QString password, bool makeDefault, IFDomain **retVal)
{
	int err;
	iFolderMessageDomainAddRequest request;
	iFolderMessageDomainAddResponse *response;
	IFDomain *domain;

	printf("IFDomain::add(%s, %s, ********, %s)\n", qPrintable(hostAddress), qPrintable(userName), makeDefault ? "true" : "false");
	
	// @todo Check to see if the client is in a valid state for this call
	
	IFIPCClient::initHeader((iFolderMessageHeader *)&request, IFOLDER_MSG_DOMAIN_ADD_REQUEST);
	
	sprintf(request.hostAddress, qPrintable(hostAddress));
	sprintf(request.userName, qPrintable(userName));
	sprintf(request.password, qPrintable(password));
	request.makeDefault = makeDefault;
	
	err = IFIPCClient::ipcCall(&request, (void **)&response);
	
	if (err == IFOLDER_SUCCESS)
	{
		domain = new IFDomain(QString(response->id), QString(response->name),
								   QString(response->description),
								   QString(response->version), QString(response->hostAddress),
								   QString(response->machineName), QString(response->osVersion),
								   QString(response->userName), response->isDefault,
								   response->isActive, NULL);
		
		free(response);

		if (!domain)
			return IFOLDER_ERR_OUT_OF_MEMORY;
		
		*retVal = domain;
	}

	return err;
}

int
IFDomain::remove(bool deleteiFoldersOnServer)
{
	return IFOLDER_ERROR;
}

int
IFDomain::logIn(QString password)
{
	return IFOLDER_ERROR;
}

int
IFDomain::logOut()
{
	return IFOLDER_ERROR;
}

int
IFDomain::activate()
{
	return IFOLDER_ERROR;
}

int
IFDomain::inactivate()
{
	return IFOLDER_ERROR;
}

int
IFDomain::changeHostAddress(QString newHostAddress)
{
	return IFOLDER_ERROR;
}

int
IFDomain::setCredentials(QString password, CredentialType credentialType)
{
	return IFOLDER_ERROR;
}

int
IFDomain::setDefault()
{
	return IFOLDER_ERROR;
}

int
IFDomain::getAuthenticatedUser(IFUser *retVal)
{
	return IFOLDER_ERROR;
}

int
IFDomain::getUser(QString userID, IFUser *retVal)
{
	return IFOLDER_ERROR;
}

int
IFDomain::getUsers(int index, int count, QList<IFUser> *retVal)
{
	return IFOLDER_ERROR;
}

int
IFDomain::getUsersBySearch(SearchProperty searchProp, SearchOperation searchOp, QString pattern, int index, int count, QList<IFUser> *retVal)
{
	return IFOLDER_ERROR;
}

int
IFDomain::createiFolder(QString localPath, QString description, IFiFolder *retVal)
{
	return IFOLDER_ERROR;
}

int
IFDomain::deleteiFolder(IFiFolder ifolder)
{
	return IFOLDER_ERROR;
}

int
IFDomain::getLocaliFolders(int index, int count, QList<IFiFolder> *retVal)
{
	return IFOLDER_ERROR;
}

int
IFDomain::getRemoteiFolders(int index, int count, QList<IFiFolder> *retVal)
{
	return IFOLDER_ERROR;
}

int
IFDomain::getiFolderByID(QString id, IFiFolder *retVal)
{
	return IFOLDER_ERROR;
}

int
IFDomain::getiFolderByName(QString name, IFiFolder *retVal)
{
	return IFOLDER_ERROR;
}

int
IFDomain::getAll(QList<IFDomain> *retVal)
{
	return IFOLDER_ERROR;
}

int
IFDomain::getAllActive(QList<IFDomain> *retVal)
{
	return IFOLDER_ERROR;
}

int
IFDomain::getDefault(IFDomain *retVal)
{
	return IFOLDER_ERROR;
}

