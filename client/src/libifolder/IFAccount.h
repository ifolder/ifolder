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

#ifndef IFOLDERACCOUNT_H_
#define IFOLDERACCOUNT_H_

class iFolderAccount : public QObject
{
	QOBJECT
	
	Q_ENUMS(CredentialType)
	Q_ENUMS(SearchProperty)
	Q_ENUMS(SearchOperation)
	
	Q_PROPERTY(QString id READ id)
	Q_PROPERTY(QString name READ name)
	Q_PROPERTY(QString description READ description)
	Q_PROPERTY(QString version READ version)
	Q_PROPERTY(QString hostAddress READ hostAddress)
	Q_PROPERTY(QString machineName READ machineName)
	Q_PROPERTY(QString osVersion READ osVersion)
	Q_PROPERTY(QString userName READ userName)
	Q_PROPERTY(bool isDefault READ isDefault)
	Q_PROPERTY(bool isActive READ isActive)

	public:
		~iFolderAccount();
		
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
		void remove(bool deleteiFoldersOnServer);
		void login(const char *password);
		void logout();
		void activate();
		void inactivate();
		void changeHostAddress(const char *newHostAddress);
		void setCredentials(const char *password, CredentialType credentialType);
		void setDefault();
		iFolderUser getAuthenticatedUser();
		iFolderUser getUser(const char *userID);
		QList getUsers(int index, int count);
		QList getUsersBySearch(SearchProperty searchProp, SearchOperation searchOp, const char *pattern, int index, int count);

		iFolder createiFolder(const char *localPath, const char *description);
		void deleteiFolder(iFolder ifolder);
		QList getLocaliFolders(int index, int count);
		QList getRemoteiFolders(int index, int count);
		iFolder getiFolderByID(const char *id);
		iFolder getiFolderByName(const char *name);
		
		
		/**
		 * Static Methods
		 */
		static QList getAll();
		static QList getAllActive();
		static iFolderAccount getDefault();
		static iFolderAccount add(const char *hostAddress, const char *userName, const char *password, bool makeDefault);
	
	private:
		iFolderAccount(QObject *parent = 0);
};

#endif /*IFOLDERACCOUNT_H_*/
