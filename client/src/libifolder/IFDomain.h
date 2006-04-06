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

class iFolderDomain
{
	public:
		virtual ~iFolderDomain();
		
		/**
		 * Enumerations
		 */
		enum CredentialType {Basic, None};
		enum SearchProperty {UserName, FullName, FirstName, LastName};
		enum SearchOperation {BeginsWith, EndsWith, Contains, Equals};
		
		/**
		 * Properties
		 */
		QString id();
		QString name();
		QString description();
		QString version();
		QString hostAddress();
		QString machineName();
		QString osVersion();
		QString userName();
		bool isDefault();
		bool isActive();
		
		/**
		 * User Data
		 */
		void *userData();
		void setUserData(void *userData);
		
		/**
		 * Methods/Actions
		 */
		int remove(bool deleteiFoldersOnServer);
		int login(const char *password);
		int logout();
		int activate();
		int inactivate();
		int changeHostAddress(const char *newHostAddress);
		int setCredentials(const char *password, CredentialType credentialType);
		int setDefault();
		int getAuthenticatedUser(iFolderUser *retVal);
		int getUser(const char *userID, iFolderUser *retVal);
		int getUsers(int index, int count, QList<iFolderUser> *retVal);
		int getUsersBySearch(SearchProperty searchProp, SearchOperation searchOp, const char *pattern, int index, int count, QList<iFolderUser> *retVal);

		int createiFolder(const char *localPath, const char *description, iFolder *retVal);
		int deleteiFolder(iFolder ifolder);
		int getLocaliFolders(int index, int count, QList<iFolder> *retVal);
		int getRemoteiFolders(int index, int count, QList<iFolder> *retVal);
		int getiFolderByID(const char *id, iFolder *retVal);
		int getiFolderByName(const char *name, iFolder *retVal);
		
		/**
		 * Static Methods
		 */
		static int getAll(QList<iFolderDomain> *retVal);
		static int getAllActive(QList<iFolderDomain> *retVal);
		static int getDefault(iFolderDomain *retVal);
		static int add(const char *hostAddress, const char *userName, const char *password, bool makeDefault, iFolderDomain *retVal);
	
	private:
		iFolderDomain();
};

#endif /*_IFOLDER_DOMAIN_H_*/
