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
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Russ Young
 *
 ***********************************************************************/
#ifndef _IFConnection_H_
#define _IFConnection_H_

#include <glib.h>

#include "glibclient.h"
#include "simiasDomain_USCOREx0020_USCOREServiceSoapProxy.h"
#include "simiasiFolderWebSoapProxy.h"
#include "IFDomain.h"

// Forward declarations
class IFDomain;
class GLIBCLIENT_API Domain;
class GLIBCLIENT_API iFolderWebSoap;
class GLIBCLIENT_API ifolder__ArrayOfString;

typedef int (*WebCall)(gpointer req, gpointer resp);

namespace ifweb
{
// Forward declarations
class IFServiceManager;
class WebService;
class DomainService;
class iFolderService;
class iFolder;
class iFolderIterator;
class iFolderDetails;
class iFolderUser;
class iFolderUserIterator;
class iFolderUserDetails;
class HostInfo;
class iFolderServer;
class iFolderServerIterator;
class iFolderEntry;
class iFolderEntryIterator;
class ChangeEntry;
class ChangeEntryIterator;
class DomainInfo;
class ProvisionInfo;
class UserPolicy;
class iFolderPolicy;
class iFolderSystem;

class GLIBCLIENT_API IFServiceManager
{
private:
	static GArray connectionList;

	static void Initialize(WebService *service, struct soap *soap);
public:
	static DomainService* GetDomainService(const gchar* domainID, const gchar *host);
	static DomainService* GetDomainService(IFDomain *pDomain, const gchar *host);
	static iFolderService* GetiFolderService(const gchar* domainID, const gchar *host);
	static iFolderService* GetiFolderService(IFDomain *pDomain, const gchar *host);
};

class GLIBCLIENT_API WebService
{
	friend class IFServiceManager;
	friend class IFDomain;
protected:
	IFDomain *m_pDomain;
	WebService(IFDomain *pDomain) : m_pDomain(pDomain) {};
	gboolean HandleError(struct soap *soap, GError **error);
};


class GLIBCLIENT_API DomainService : WebService
{
	friend class IFServiceManager;
	friend class IFDomain;
public:
	Domain		m_DomainService;
private:

	DomainService(IFDomain *pDomain) : WebService(pDomain) {};
	virtual ~DomainService() { g_free((gchar*)m_DomainService.endpoint); };
	
public:
	DomainInfo* GetDomainInfo(const gchar* userID, GError** error);
	HostInfo* GetHomeServer(const gchar* userID, GError** error);
	ProvisionInfo* ProvisionUserOnServer(const gchar *userName, const gchar *password, const gchar *ticket, GError **error);
	ProvisionInfo* ProvisionUser(const gchar* userName, const gchar* password, GError **error);
	void CreateMaster();
	gboolean RemoveServerCollections(const gchar* domainID, const gchar* userID, GError **error);
	gchar* GetDomainID(GError **error);
};

class GLIBCLIENT_API iFolderService : WebService
{
	friend class IFServiceManager;
	friend class IFDomain;
public:
	iFolderWebSoap	m_iFolderService;
private:
	iFolderService(IFDomain *pDomain) : WebService(pDomain) {};
	~iFolderService() { g_free((gchar*)m_iFolderService.endpoint); };
public:
	iFolderIterator* GetiFolders(gint index, gint max, GError **error);
	iFolder* CreateiFolder(gchar *description, gchar* name, GError **error);
	iFolderIterator* GetiFoldersByName(gint index, gint max, enum ifolder__SearchOperation operation, const gchar* pattern, GError **error);
	iFolderIterator* GetiFoldersBySearch(time_t after, gint index, gint max, enum ifolder__SearchOperation operation, const gchar* pattern, enum ifolder__MemberRole role, GError **error);
	gboolean RemoveMembership(const gchar *ifolderID, GError **error);
	UserPolicy* GetAuthenticatedUserPolicy(GError **error);
	iFolderPolicy* GetiFolderPolicy(gchar *ifolderID, GError **error);
	gboolean SetiFolderPolicy(iFolderPolicy *pPolicy, GError **error);
	iFolderEntry* CreateEntry(const gchar *entryName, const gchar *ifolderID, const gchar *parentID, enum ifolder__iFolderEntryType type, GError **error);
	gboolean DeleteEntry(const gchar *entryID, const gchar *ifolderID, GError **error);
	iFolderEntry* GetEntry(const gchar *entryID, const gchar *ifolderID, GError **error);
	iFolderEntry* GetEntryByPath(const gchar *entryPath, const gchar *ifolderID, GError **error);
	iFolderEntryIterator* GetEntries(const gchar *entryID, const gchar *ifolderID, gint index, gint max, GError **error);
	iFolderEntryIterator* GetEntriesByName(const gchar *ifolderID, gint index, gint max, enum ifolder__SearchOperation, const gchar *parentID, const gchar *pattern, GError **error);
	gchar* GetSetting(gchar *name, GError **error);
	gboolean SetSetting(gchar *name, gchar *value, GError **error);
	void OpenFileRead();
	void OpenFileWrite();
	void ReadFile();
	void WriteFile();
	void CloseFile();
	iFolderSystem* GetSystem(GError **error);
	iFolderServer* GetHomeServer(GError **error);
	iFolderServerIterator* GetServers(GError **error);
	gboolean DeleteiFolder(const gchar *ifolderID, GError **error);
	iFolder* GetiFolder(const gchar *ifolderID, GError **error);
	iFolderDetails* GetiFolderDetails(const gchar *ifolderID, GError **error);
	gboolean SetiFolderDescription(const gchar *ifolderID, const gchar *description, GError **error);
	gboolean PublishiFolder(const gchar *ifolderID, gboolean publish, GError **error);
	ChangeEntryIterator* GetChanges(const gchar *ifolderID, const gchar *itemID, gint index, gint max, GError **error);
	iFolderUser* GetAuthenticatedUser(GError **error);
	gboolean SetMemberRights(const gchar *ifolderID, const gchar *userID, enum ifolder__Rights rights, GError **error);
	gboolean AddMember(const gchar *ifolderID, const gchar *userID, enum ifolder__Rights rights, GError **error);
	gboolean RemoveMember(const gchar *ifolderID, const gchar *userID, GError **error);
	gboolean SetiFolderOwner(const gchar *ifolderID, const gchar *userID, GError **error);
	iFolderUserIterator* GetMembers(const gchar* ifolderID, gint index, gint max, GError **error);
	iFolderUserIterator* GetUsers(gint index, gint max, GError **error);
	iFolderUser* GetUser(const gchar *pUserID, GError **error);
	iFolderUserDetails* GetUserDetails(const gchar *pUserID, GError **error);
	iFolderUserIterator* GetUsersBySearch(gint index, gint max, enum ifolder__SearchOperation operation, const gchar* pattern, enum ifolder__SearchProperty property, GError **error);
	gboolean Login(const gchar *user, const gchar *password, GError **error);
};


class GLIBCLIENT_API iFolderUser
{
public:
	gchar *ID;	/* optional element of type xsd:string */
	gchar *UserName;	/* optional element of type xsd:string */
	gchar *FullName;	/* optional element of type xsd:string */
	gchar *FirstName;	/* optional element of type xsd:string */
	gchar *LastName;	/* optional element of type xsd:string */
	enum ifolder__Rights Rights;	/* required element of type ifolder:Rights */
	gboolean Enabled;	/* required element of type xsd:boolean */
	gboolean IsOwner;	/* required element of type xsd:boolean */
	gchar *Email;	/* optional element of type xsd:string */

public:
	iFolderUser(ifolder__iFolderUser *pUser)
	{
		ID = g_strdup(pUser->ID);
		UserName = g_strdup(pUser->UserName);
		FullName = g_strdup(pUser->FullName);
		FirstName = g_strdup(pUser->FirstName);
		LastName = g_strdup(pUser->LastName);
		Rights = pUser->Rights;
		Enabled = pUser->Enabled;
		IsOwner = pUser->IsOwner;
		Email = g_strdup(pUser->Email);
	}

	virtual ~iFolderUser()
	{
		g_free(ID);
		g_free(UserName);
		g_free(FullName);
		g_free(FirstName);
		g_free(LastName);
		g_free(Email);
	}
};

class GLIBCLIENT_API iFolderUserDetails : public iFolderUser
{
public:
	gint SyncIntervalEffective;	/* required element of type xsd:int */
	time_t LastLogin;	/* required element of type xsd:dateTime */
	gchar *LdapContext;	/* optional element of type xsd:string */
	gint OwnediFolderCount;	/* required element of type xsd:int */
	gint SharediFolderCount;	/* required element of type xsd:int */
public:
	iFolderUserDetails(ifolder__iFolderUserDetails *pDetails) : iFolderUser(pDetails)
	{
		SyncIntervalEffective = pDetails->SyncIntervalEffective;
		LastLogin = pDetails->LastLogin;
		LdapContext = g_strdup(pDetails->LdapContext);
		OwnediFolderCount = pDetails->OwnediFolderCount;
		SharediFolderCount = pDetails->SharediFolderCount;
	}
	virtual ~iFolderUserDetails()
	{
		g_free(LdapContext);
	}
};

class GLIBCLIENT_API iFolderUserIterator
{
public:
	gint	m_Total;
	gint	m_Count;

private:
	iFolderUser		**m_Items;
	gint			m_Current;

public:

	iFolderUserIterator(ifolder__iFolderUserSet *pSet)
	{
		m_Current = 0;
		m_Count = pSet->Items->__sizeiFolderUser;
		m_Total = pSet->Total;
		m_Items = new (iFolderUser(*[m_Count]));
		ifolder__iFolderUser **items = pSet->Items->iFolderUser;
		for (int i = 0; i < m_Count; ++i)
		{
			m_Items[i] = new iFolderUser(items[i]);
		}
	}

	~iFolderUserIterator()
	{
		for (int i = 0; i < m_Count; ++i)
		{
			delete m_Items[i];
		}
		delete [] m_Items;
	}

	iFolderUser* Next()
	{
		if (m_Current < m_Count)
			return m_Items[m_Current++];
		return NULL;
	}

	void Reset() { m_Current = 0; }
};


class GLIBCLIENT_API iFolder
{
public:
	gchar *ID;	/* optional element of type xsd:string */
	gchar *Name;	/* optional element of type xsd:string */
	gchar *Description;	/* optional element of type xsd:string */
	gchar *OwnerID;	/* optional element of type xsd:string */
	gchar *OwnerUserName;	/* optional element of type xsd:string */
	gchar *OwnerFullName;	/* optional element of type xsd:string */
	gchar *DomainID;	/* optional element of type xsd:string */
	LONG64 Size;	/* required element of type xsd:long */
	gboolean IsOwner;	/* required element of type xsd:boolean */
	enum ifolder__Rights Rights;	/* required element of type ifolder:Rights */
	time_t Created;	/* required element of type xsd:dateTime */
	time_t LastModified;	/* required element of type xsd:dateTime */
	gboolean Published;	/* required element of type xsd:boolean */
	gboolean Enabled;	/* required element of type xsd:boolean */
	gint MemberCount;	/* required element of type xsd:int */
	
public:
    iFolder(ifolder__iFolder *piFolder)
	{
		ID = g_strdup(piFolder->ID);
		Name = g_strdup(piFolder->Name);
		Description = g_strdup(piFolder->Description);
		OwnerID = g_strdup(piFolder->OwnerID);
		OwnerUserName = g_strdup(piFolder->OwnerUserName);
		OwnerFullName = g_strdup(piFolder->OwnerFullName);
		DomainID = g_strdup(piFolder->DomainID);
		Size = piFolder->Size;
		IsOwner = piFolder->IsOwner;
		Rights = piFolder->Rights;
		Created = piFolder->Created;
		LastModified = piFolder->LastModified;
		Published = piFolder->Published;
		Enabled = piFolder->Enabled;
		MemberCount = piFolder->MemberCount;
	}

	virtual ~iFolder()
	{
		g_free(ID);
		g_free(Name);
		g_free(Description);
		g_free(OwnerID);
		g_free(OwnerUserName);
		g_free(OwnerFullName);
		g_free(DomainID);
	}
};

class GLIBCLIENT_API iFolderIterator
{
public:
	gint	m_Total;
	gint	m_Count;

private:
	iFolder			**m_Items;
	gint			m_Current;

public:

	iFolderIterator(ifolder__iFolderSet *pSet)
	{
		m_Current = 0;
		m_Count = pSet->Items->__sizeiFolder;
		m_Total = pSet->Total;
		m_Items = new (iFolder(*[m_Count]));
		ifolder__iFolder **items = pSet->Items->iFolder;
		for (int i = 0; i < m_Count; ++i)
		{
			m_Items[i] = new iFolder(items[i]);
		}
	}

	~iFolderIterator()
	{
		for (int i = 0; i < m_Count; ++i)
		{
			delete m_Items[i];
		}
		delete [] m_Items;
	}

	iFolder* Next()
	{
		if (m_Current < m_Count)
			return m_Items[m_Current++];
		return NULL;
	}

	void Reset() { m_Current = 0; }
};

class GLIBCLIENT_API iFolderDetails : public iFolder
{
public:
	gint FileCount;	/* required element of type xsd:int */
	gint DirectoryCount;	/* required element of type xsd:int */
	gchar *ManagedPath;	/* optional element of type xsd:string */
	gchar *UnManagedPath;	/* optional element of type xsd:string */
public:
	iFolderDetails(ifolder__iFolderDetails *pDetails) : iFolder(pDetails)
	{
		FileCount = pDetails->FileCount;
		DirectoryCount = pDetails->DirectoryCount;
		ManagedPath = g_strdup(pDetails->ManagedPath);
		UnManagedPath = g_strdup(pDetails->UnManagedPath);
	}

	virtual ~iFolderDetails()
	{
		g_free(ManagedPath);
		g_free(UnManagedPath);
	}
};

class GLIBCLIENT_API HostInfo
{
public:
	gchar		*m_ID;	/* optional element of type xsd:string */
	gchar		*m_PublicAddress;	/* optional element of type xsd:string */
	gchar		*m_PrivateAddress;	/* optional element of type xsd:string */
	gchar		*m_PublicKey;	/* optional element of type xsd:string */
	gboolean	m_Master;	/* required element of type xsd:boolean */
public:
	HostInfo(ds__HostInfo *pInfo)
	{
		m_ID = g_strdup(pInfo->ID);
		m_PublicAddress = g_strdup(pInfo->PublicAddress);
		m_PrivateAddress = g_strdup(pInfo->PrivateAddress);
		m_PublicKey = g_strdup(pInfo->PublicKey);
		m_Master = pInfo->Master;
	}

	~HostInfo()
	{
		g_free(m_ID);
		g_free(m_PublicAddress);
		g_free(m_PrivateAddress);
		g_free(m_PublicKey);
	}
};

class GLIBCLIENT_API iFolderServer
{
public:
	gchar *m_Version;	/* optional element of type xsd:string */
	gchar *m_HostName;	/* optional element of type xsd:string */
	gchar *m_MachineName;	/* optional element of type xsd:string */
	gchar *m_OSVersion;	/* optional element of type xsd:string */
	gchar *m_UserName;	/* optional element of type xsd:string */
	gchar *m_ClrVersion;	/* optional element of type xsd:string */
public:
	iFolderServer(ifolder__iFolderServer *server)
	{
		m_Version = g_strdup(server->Version);
		m_HostName = g_strdup(server->HostName);
		m_MachineName = g_strdup(server->MachineName);
		m_OSVersion = g_strdup(server->OSVersion);
		m_UserName = g_strdup(server->UserName);
		m_ClrVersion = g_strdup(server->ClrVersion);
	}

	virtual ~iFolderServer()
	{
		g_free(m_Version);
		g_free(m_HostName);
		g_free(m_MachineName);
		g_free(m_OSVersion);
		g_free(m_UserName);
		g_free(m_ClrVersion);
	}
};

class GLIBCLIENT_API iFolderServerIterator
{
public:
	gint	m_Total;
	gint	m_Count;

private:
	iFolderServer	**m_Items;
	gint			m_Current;

public:

	iFolderServerIterator(ifolder__ArrayOfIFolderServer *pSet)
	{
		m_Current = 0;
		m_Count = pSet->__sizeiFolderServer;
		m_Total = m_Count;
		m_Items = new (iFolderServer(*[m_Count]));
		ifolder__iFolderServer **items = pSet->iFolderServer;
		for (int i = 0; i < m_Count; ++i)
		{
			m_Items[i] = new iFolderServer(items[i]);
		}
	}

	~iFolderServerIterator()
	{
		for (int i = 0; i < m_Count; ++i)
		{
			delete m_Items[i];
		}
		delete [] m_Items;
	}

	iFolderServer* Next()
	{
		if (m_Current < m_Count)
			return m_Items[m_Current++];
		return NULL;
	}

	void Reset() { m_Current = 0; }
};

class GLIBCLIENT_API UserPolicy
{
public:
	gchar		*m_UserID;	/* optional element of type xsd:string */
	gboolean	m_LoginEnabled;	/* required element of type xsd:boolean */
	gint64		m_SpaceLimit;	/* required element of type xsd:long */
	gint64		m_SpaceLimitEffective;	/* required element of type xsd:long */
	gint64		m_FileSizeLimit;	/* required element of type xsd:long */
	gint64		m_FileSizeLimitEffective;	/* required element of type xsd:long */
	gint64		m_SpaceUsed;	/* required element of type xsd:long */
	gint64		m_SpaceAvailable;	/* required element of type xsd:long */
	gint		m_SyncInterval;	/* required element of type xsd:int */
	gint		m_SyncIntervalEffective;	/* required element of type xsd:int */
	GArray		*m_FileTypesIncludes;
	GArray		*m_FileTypesIncludesEffective;
	GArray		*m_FileTypesExcludes;
	GArray		*m_FileTypesExcludesEffective;
public:
	UserPolicy(ifolder__UserPolicy *userPolicy)
	{
		m_UserID = g_strdup(userPolicy->UserID);
		m_LoginEnabled = userPolicy->LoginEnabled;
		m_SpaceLimit = userPolicy->SpaceLimit;
		m_SpaceLimitEffective = userPolicy->SpaceLimitEffective;
		m_FileSizeLimit = userPolicy->FileSizeLimit;
		m_FileSizeLimitEffective = userPolicy->FileSizeLimitEffective;
		m_SpaceUsed = userPolicy->SpaceUsed;
		m_SpaceAvailable = userPolicy->SpaceAvailable;
		m_SyncInterval = userPolicy->SyncInterval;
		m_SyncIntervalEffective = userPolicy->SyncIntervalEffective;
		int count;
		int i;
		count = userPolicy->FileTypesIncludes->__sizestring;
		m_FileTypesIncludes = g_array_sized_new(true, true, sizeof(gchar*), count);
		for (i = 0; i < count; ++i)
		{
			gchar *ftString = g_strdup(userPolicy->FileTypesIncludes->string[i]);
			g_array_insert_val(m_FileTypesIncludes, i, ftString);
		}
		
		count = userPolicy->FileTypesIncludesEffective->__sizestring;
		m_FileTypesIncludesEffective = g_array_sized_new(true, true, sizeof(gchar*), count);
		for (i = 0; i < count; ++i)
		{
			gchar *ftString = g_strdup(userPolicy->FileTypesIncludesEffective->string[i]);
			g_array_insert_val(m_FileTypesIncludesEffective, i, ftString);
		}

		count = userPolicy->FileTypesExcludes->__sizestring;
		m_FileTypesExcludes = g_array_sized_new(true, true, sizeof(gchar*), count);
		for (i = 0; i < count; ++i)
		{
			gchar *ftString = g_strdup(userPolicy->FileTypesExcludes->string[i]);
			g_array_insert_val(m_FileTypesExcludes, i, ftString);
		}

		count = userPolicy->FileTypesExcludesEffective->__sizestring;
		m_FileTypesExcludesEffective = g_array_sized_new(true, true, sizeof(gchar*), count);
		m_FileTypesExcludesEffective = g_array_sized_new(true, true, sizeof(gchar*), count);
		for (i = 0; i < count; ++i)
		{
			gchar *ftString = g_strdup(userPolicy->FileTypesExcludesEffective->string[i]);
			g_array_insert_val(m_FileTypesExcludesEffective, i, ftString);
		}
	}
	virtual ~UserPolicy()
	{ 
		g_free(m_UserID);
		int i;
		int count;
		count = m_FileTypesIncludes->len;
		for (i = 0; i < count; ++i)
		{
			g_free(g_array_index(m_FileTypesIncludes, gchar*, i));
		}
		g_array_free(m_FileTypesIncludes, true);

		count = m_FileTypesIncludesEffective->len;
		for (i = 0; i < count; ++i)
		{
			g_free(g_array_index(m_FileTypesIncludesEffective, gchar*, i));
		}
		g_array_free(m_FileTypesIncludesEffective, true);

		count = m_FileTypesExcludes->len;
		for (i = 0; i < count; ++i)
		{
			g_free(g_array_index(m_FileTypesExcludes, gchar*, i));
		}
		g_array_free(m_FileTypesExcludes, true);

		count = m_FileTypesExcludesEffective->len;
		for (i = 0; i < count; ++i)
		{
			g_free(g_array_index(m_FileTypesExcludesEffective, gchar*, i));
		}
		g_array_free(m_FileTypesExcludesEffective, true);
	}
};

class GLIBCLIENT_API iFolderPolicy
{
private:
	ifolder__ArrayOfString m_FileTypesIncludesArray;
	ifolder__ArrayOfString m_FileTypesIncludesEffectiveArray;
	ifolder__ArrayOfString m_FileTypesExcludesArray;
	ifolder__ArrayOfString m_FileTypesExcludesEffectiveArray;

public:
	gchar *m_iFolderID;	/* optional element of type xsd:string */
	gboolean m_Locked;	/* required element of type xsd:boolean */
	gint64 m_SpaceLimit;	/* required element of type xsd:long */
	gint64 m_SpaceLimitEffective;	/* required element of type xsd:long */
	gint64 m_SpaceUsed;	/* required element of type xsd:long */
	gint64 m_SpaceAvailable;	/* required element of type xsd:long */
	gint m_SyncInterval;	/* required element of type xsd:int */
	gint m_SyncIntervalEffective;	/* required element of type xsd:int */
	gint64 m_FileSizeLimit;	/* required element of type xsd:long */
	gint64 m_FileSizeLimitEffective;	/* required element of type xsd:long */
	GArray *m_FileTypesIncludes;	/* optional element of type ifolder:ArrayOfString */
	GArray *m_FileTypesIncludesEffective;	/* optional element of type ifolder:ArrayOfString */
	GArray *m_FileTypesExcludes;	/* optional element of type ifolder:ArrayOfString */
	GArray *m_FileTypesExcludesEffective;	/* optional element of type ifolder:ArrayOfString */
	
public:
	iFolderPolicy(ifolder__iFolderPolicy *pPolicy)
	{
		m_iFolderID = g_strdup(pPolicy->iFolderID);
		m_Locked = pPolicy->Locked;
		m_SpaceLimit = pPolicy->SpaceLimit;
		m_SpaceLimitEffective = pPolicy->SpaceLimitEffective;
		m_SpaceUsed = pPolicy->SpaceUsed;
		m_SpaceAvailable = pPolicy->SpaceAvailable;
		m_SyncInterval = pPolicy->SyncInterval;
		m_SyncIntervalEffective = pPolicy->SyncIntervalEffective;
		m_FileSizeLimit = pPolicy->FileSizeLimit;
		m_FileSizeLimitEffective = pPolicy->FileSizeLimitEffective;

		int count;
		int i;
		count = pPolicy->FileTypesIncludes->__sizestring;
		m_FileTypesIncludes = g_array_sized_new(true, true, sizeof(gchar*), count);
		for (i = 0; i < count; ++i)
		{
			gchar *ftString = g_strdup(pPolicy->FileTypesIncludes->string[i]);
			g_array_insert_val(m_FileTypesIncludes, i, ftString);
		}
		
		count = pPolicy->FileTypesIncludesEffective->__sizestring;
		m_FileTypesIncludesEffective = g_array_sized_new(true, true, sizeof(gchar*), count);
		for (i = 0; i < count; ++i)
		{
			gchar *ftString = g_strdup(pPolicy->FileTypesIncludesEffective->string[i]);
			g_array_insert_val(m_FileTypesIncludesEffective, i, ftString);
		}

		count = pPolicy->FileTypesExcludes->__sizestring;
		m_FileTypesExcludes = g_array_sized_new(true, true, sizeof(gchar*), count);
		for (i = 0; i < count; ++i)
		{
			gchar *ftString = g_strdup(pPolicy->FileTypesExcludes->string[i]);
			g_array_insert_val(m_FileTypesExcludes, i, ftString);
		}

		count = pPolicy->FileTypesExcludesEffective->__sizestring;
		m_FileTypesExcludesEffective = g_array_sized_new(true, true, sizeof(gchar*), count);
		m_FileTypesExcludesEffective = g_array_sized_new(true, true, sizeof(gchar*), count);
		for (i = 0; i < count; ++i)
		{
			gchar *ftString = g_strdup(pPolicy->FileTypesExcludesEffective->string[i]);
			g_array_insert_val(m_FileTypesExcludesEffective, i, ftString);
		}
	}
	virtual ~iFolderPolicy()
	{
		g_free(m_iFolderID);
	
		int i;
		int count;
		count = m_FileTypesIncludes->len;
		for (i = 0; i < count; ++i)
		{
			g_free(g_array_index(m_FileTypesIncludes, gchar*, i));
		}
		g_array_free(m_FileTypesIncludes, true);

		count = m_FileTypesIncludesEffective->len;
		for (i = 0; i < count; ++i)
		{
			g_free(g_array_index(m_FileTypesIncludesEffective, gchar*, i));
		}
		g_array_free(m_FileTypesIncludesEffective, true);

		count = m_FileTypesExcludes->len;
		for (i = 0; i < count; ++i)
		{
			g_free(g_array_index(m_FileTypesExcludes, gchar*, i));
		}
		g_array_free(m_FileTypesExcludes, true);

		count = m_FileTypesExcludesEffective->len;
		for (i = 0; i < count; ++i)
		{
			g_free(g_array_index(m_FileTypesExcludesEffective, gchar*, i));
		}
		g_array_free(m_FileTypesExcludesEffective, true);
	}

	ifolder__iFolderPolicy* To_ifolder__iFolderPolicy()
	{
		ifolder__iFolderPolicy *pPolicy = new ifolder__iFolderPolicy();
		pPolicy->iFolderID = m_iFolderID;
		pPolicy->Locked = (m_Locked != 0); // This is to work around a MS C4800 compile error.
		pPolicy->SpaceLimit = m_SpaceLimit;
		pPolicy->SpaceLimitEffective = m_SpaceLimitEffective;
		pPolicy->SpaceUsed = m_SpaceUsed;
		pPolicy->SpaceAvailable = m_SpaceAvailable;
		pPolicy->SyncInterval = m_SyncInterval;
		pPolicy->SyncIntervalEffective = m_SyncIntervalEffective;
		pPolicy->FileSizeLimit = m_FileSizeLimit;
		pPolicy->FileSizeLimitEffective = m_FileSizeLimitEffective;

		m_FileTypesIncludesArray.__sizestring = m_FileTypesIncludes->len;
		m_FileTypesIncludesArray.string = (char**)m_FileTypesIncludes->data;
		pPolicy->FileTypesIncludes = &m_FileTypesIncludesArray;

		m_FileTypesIncludesEffectiveArray.__sizestring = m_FileTypesIncludesEffective->len;
		m_FileTypesIncludesEffectiveArray.string = (char**)m_FileTypesIncludesEffective->data;
		pPolicy->FileTypesExcludesEffective = &m_FileTypesIncludesEffectiveArray;

		m_FileTypesExcludesArray.__sizestring = m_FileTypesExcludes->len;
		m_FileTypesExcludesArray.string = (char**)m_FileTypesExcludes->data;
		pPolicy->FileTypesExcludes = &m_FileTypesExcludesArray;

		m_FileTypesExcludesEffectiveArray.__sizestring = m_FileTypesExcludesEffective->len;
		m_FileTypesExcludesEffectiveArray.string = (char**)m_FileTypesExcludesEffective->data;
		pPolicy->FileTypesExcludesEffective = &m_FileTypesExcludesEffectiveArray;
		return pPolicy;
	}
};


class GLIBCLIENT_API iFolderEntry
{
public:
	gchar *m_ID;	/* optional element of type xsd:string */
	gchar *m_Name;	/* optional element of type xsd:string */
	gchar *m_Path;	/* optional element of type xsd:string */
	gchar *m_iFolderID;	/* optional element of type xsd:string */
	gchar *m_ParentID;	/* optional element of type xsd:string */
	gboolean m_IsDirectory;	/* required element of type xsd:boolean */
	gboolean m_IsRoot;	/* required element of type xsd:boolean */
	gboolean m_HasChildren;	/* required element of type xsd:boolean */
	time_t m_LastModified;	/* required element of type xsd:dateTime */
	gint64 m_Size;	/* required element of type xsd:long */

public:
	iFolderEntry(ifolder__iFolderEntry *pEntry)
	{
		m_ID = g_strdup(pEntry->ID);
		m_Name = g_strdup(pEntry->Name);
		m_Path = g_strdup(pEntry->Path);
		m_iFolderID = g_strdup(pEntry->iFolderID);
		m_ParentID = g_strdup(pEntry->ParentID);
		m_IsDirectory = pEntry->IsDirectory;
		m_IsRoot = pEntry->IsRoot;
		m_HasChildren = pEntry->HasChildren;
		m_LastModified = pEntry->LastModified;
		m_Size = pEntry->Size;
	}
	virtual ~iFolderEntry()
	{
		g_free(m_ID);
		g_free(m_Name);
		g_free(m_Path);
		g_free(m_iFolderID);
		g_free(m_ParentID);
	}
};

class GLIBCLIENT_API iFolderEntryIterator
{
public:
	gint	m_Total;
	gint	m_Count;

private:
	iFolderEntry	**m_Items;
	gint			m_Current;

public:

	iFolderEntryIterator(ifolder__iFolderEntrySet *pSet)
	{
		m_Current = 0;
		m_Count = pSet->Items->__sizeiFolderEntry;
		m_Total = pSet->Total;
		m_Items = new (iFolderEntry(*[m_Count]));
		ifolder__iFolderEntry **items = pSet->Items->iFolderEntry;
		for (int i = 0; i < m_Count; ++i)
		{
			m_Items[i] = new iFolderEntry(items[i]);
		}
	}

	~iFolderEntryIterator()
	{
		for (int i = 0; i < m_Count; ++i)
		{
			delete m_Items[i];
		}
		delete [] m_Items;
	}

	iFolderEntry* Next()
	{
		if (m_Current < m_Count)
			return m_Items[m_Current++];
		return NULL;
	}

	void Reset() { m_Current = 0; }
};

class GLIBCLIENT_API ChangeEntry
{
public:
	time_t m_Time;	/* required element of type xsd:dateTime */
	enum ifolder__ChangeEntryType m_Type;	/* required element of type ifolder:ChangeEntryType */
	enum ifolder__ChangeEntryAction m_Action;	/* required element of type ifolder:ChangeEntryAction */
	gchar *m_ID;	/* optional element of type xsd:string */
	gchar *m_Name;	/* optional element of type xsd:string */
	gchar *m_UserID;	/* optional element of type xsd:string */
	gchar *m_UserFullName;	/* optional element of type xsd:string */
public:
	ChangeEntry(ifolder__ChangeEntry *change)
	{
		m_Time = change->Time;
		m_Type = change->Type;
		m_Action = change->Action;
		m_ID = g_strdup(change->ID);
		m_Name = g_strdup(change->Name);
		m_UserID = g_strdup(change->UserID);
		m_UserFullName = g_strdup(change->UserFullName);
	}
	virtual ~ChangeEntry()
	{
		g_free(m_ID);
		g_free(m_Name);
		g_free(m_UserID);
		g_free(m_UserFullName);
	}
};

class GLIBCLIENT_API ChangeEntryIterator
{
public:
	gint	m_Total;
	gint	m_Count;

private:
	ChangeEntry		**m_Items;
	gint			m_Current;

public:

	ChangeEntryIterator(ifolder__ChangeEntrySet *pSet)
	{
		m_Current = 0;
		m_Count = pSet->Items->__sizeChangeEntry;
		m_Total = pSet->Total;
		m_Items = new (ChangeEntry(*[m_Count]));
		ifolder__ChangeEntry **items = pSet->Items->ChangeEntry;
		for (int i = 0; i < m_Count; ++i)
		{
			m_Items[i] = new ChangeEntry(items[i]);
		}
	}

	~ChangeEntryIterator()
	{
		for (int i = 0; i < m_Count; ++i)
		{
			delete m_Items[i];
		}
		delete [] m_Items;
	}

	ChangeEntry* Next()
	{
		if (m_Current < m_Count)
			return m_Items[m_Current++];
		return NULL;
	}

	void Reset() { m_Current = 0; }
};


class GLIBCLIENT_API DomainInfo
{
public:
	gchar *m_Name;	/* optional element of type xsd:string */
	gchar *m_Description;	/* optional element of type xsd:string */
	gchar *m_ID;	/* optional element of type xsd:string */
	gchar *m_MemberNodeID;	/* optional element of type xsd:string */
	gchar *m_MemberNodeName;	/* optional element of type xsd:string */
	gchar *m_MemberRights;	/* optional element of type xsd:string */
public:
	DomainInfo(ds__DomainInfo *pInfo)
	{
		m_Name = g_strdup(pInfo->Name);
		m_Description = g_strdup(pInfo->Description);
		m_ID = g_strdup(pInfo->ID);
		m_MemberNodeID = g_strdup(pInfo->MemberNodeID);
		m_MemberNodeName = g_strdup(pInfo->MemberNodeName);
		m_MemberRights = g_strdup(pInfo->MemberRights);
	}
	virtual ~DomainInfo()
	{
		g_free(m_Name);
		g_free(m_Description);
		g_free(m_ID);
		g_free(m_MemberNodeID);
		g_free(m_MemberNodeName);
		g_free(m_MemberRights);
	}
};

class GLIBCLIENT_API ProvisionInfo
{
public:
	gchar *m_UserID;	/* optional element of type xsd:string */
	gchar *m_POBoxID;	/* optional element of type xsd:string */
	gchar *m_POBoxName;	/* optional element of type xsd:string */
	gchar *m_MemberNodeID;	/* optional element of type xsd:string */
	gchar *m_MemberNodeName;	/* optional element of type xsd:string */
	gchar *m_MemberRights;	/* optional element of type xsd:string */
public:
	ProvisionInfo(ds__ProvisionInfo *pInfo)
	{
		m_UserID = g_strdup(pInfo->UserID);
		m_POBoxID = g_strdup(pInfo->POBoxID);
		m_POBoxName = g_strdup(pInfo->POBoxName);
		m_MemberNodeID = g_strdup(pInfo->MemberNodeID);
		m_MemberNodeName = g_strdup(pInfo->MemberNodeName);
		m_MemberRights = g_strdup(pInfo->MemberRights);
	}
	virtual ~ProvisionInfo()
	{
		g_free(m_UserID);
		g_free(m_POBoxID);
		g_free(m_POBoxName);
		g_free(m_MemberNodeID);
		g_free(m_MemberNodeName);
		g_free(m_MemberRights);
	}
};

class GLIBCLIENT_API iFolderSystem
{
public:
	gchar *m_ID;	/* optional element of type xsd:string */
	gchar *m_Name;	/* optional element of type xsd:string */
	gchar *m_Version;	/* optional element of type xsd:string */
	gchar *m_Description;	/* optional element of type xsd:string */
public:
	iFolderSystem(ifolder__iFolderSystem *pSystem)
	{
		m_ID = g_strdup(pSystem->ID);
		m_Name = g_strdup(pSystem->Name);
		m_Version = g_strdup(pSystem->Version);
		m_Description = g_strdup(pSystem->Description);
	}
	virtual ~iFolderSystem()
	{
		g_free(m_ID);
		g_free(m_Name);
		g_free(m_Version);
		g_free(m_Description);
	}
};

} // namespace ifweb

#endif //_IFConnection_H_
