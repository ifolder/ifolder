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

#ifndef _IFOLDER_DOMAIN_H_
#define _IFOLDER_DOMAIN_H_

#include <QString>

#include "IFDomain.h"
#include "IFiFolder.h"
#include "IFUser.h"

/**
 * @file IFDomain.h
 * @brief Domain API
 */

class IFDomain
{
	public:
		virtual ~IFDomain();
		
		/**
		 * Enumerations
		 */
		enum CredentialType {Basic, None};
		enum SearchProperty {UserName, FullName, FirstName, LastName};
		enum SearchOperation {BeginsWith, EndsWith, Contains, Equals};
		
		/**
		 * Properties
		 */
		QString getId();
		QString getName();
		QString getDescription();
		QString getVersion();
		QString getHostAddress();
		QString getMachineName();
		QString getOsVersion();
		QString getUserName();
		bool getIsDefault();
		bool getIsActive();
		
		/**
		 * User Data
		 */
		void *getUserData();
		void setUserData(void *userData);
		
		/**
		 * Methods/Actions
		 */
		static int add(QString hostAddress, QString userName, QString password, bool makeDefault, IFDomain **retVal);
		int remove(bool deleteiFoldersOnServer);
		int logIn(QString password);
		int logOut();

		int activate();
		int inactivate();
		int changeHostAddress(QString newHostAddress);
		int setCredentials(QString password, CredentialType credentialType);
		int setDefault();
		int getAuthenticatedUser(IFUser *retVal);
		int getUser(QString userID, IFUser *retVal);
		int getUsers(int index, int count, QList<IFUser> *retVal);
		int getUsersBySearch(SearchProperty searchProp, SearchOperation searchOp, QString pattern, int index, int count, QList<IFUser> *retVal);

		int createiFolder(QString localPath, QString description, IFiFolder *retVal);
		int deleteiFolder(IFiFolder ifolder);
		int getLocaliFolders(int index, int count, QList<IFiFolder> *retVal);
		int getRemoteiFolders(int index, int count, QList<IFiFolder> *retVal);
		int getiFolderByID(QString id, IFiFolder *retVal);
		int getiFolderByName(QString name, IFiFolder *retVal);
		
		/**
		 * Static Methods
		 */
		static int getAll(QList<IFDomain> *retVal);
		static int getAllActive(QList<IFDomain> *retVal);
		static int getDefault(IFDomain *retVal);
	
	private:
		IFDomain(QString id, QString name, QString description,
					  QString version, QString hostAddress,
					  QString machineName, QString osVersion,
					  QString userName, bool isDefault, bool isActive,
					  void *userData);

		QString id;
		QString name;
		QString description;
		QString version;
		QString hostAddress;
		QString machineName;
		QString osVersion;
		QString userName;
		bool isDefault;
		bool isActive;
		void *userData;
};

#endif /*_IFOLDER_DOMAIN_H_*/
