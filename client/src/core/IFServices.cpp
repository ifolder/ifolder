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
#include "IFApplication.h"
#include "IFServices.h"
#include "IFDomain.h"

namespace ifweb
{
// IFServiceManager class
DomainService* IFServiceManager::GetDomainService(const gchar* domainID, const gchar *host)
{
	IFDomain *pDomain = IFDomain::GetDomainByID(domainID);
	if (pDomain != NULL)
	{
		// TODO fix host to support multi-server
		DomainService *pService = new DomainService(pDomain);
		IFApplication::BuildUrlToService(host, pService->m_DomainService.endpoint);
		return pService;
	}
	return NULL;
}

DomainService* IFServiceManager::GetDomainService(IFDomain *pDomain, const gchar *host)
{
	if (pDomain != NULL)
	{
		// TODO fix host to support multi-server
		DomainService *pService = new DomainService(pDomain);
		IFApplication::BuildUrlToService(host, pService->m_DomainService.endpoint);
		return pService;
	}
	return NULL;
}

iFolderService* IFServiceManager::GetiFolderService(const gchar* domainID, const gchar *host)
{
	IFDomain *pDomain = IFDomain::GetDomainByID(domainID);
	if (pDomain != NULL)
	{
		// TODO fix host to support multi-server
		host = pDomain->m_MasterHost;
		iFolderService *pService = new iFolderService(pDomain);
		IFApplication::BuildUrlToService(host, pService->m_iFolderService.endpoint);
		return pService;
	}
	return NULL;
}


// DomainService class
DomainInfo* DomainService::GetDomainInfo(const gchar* userID, GError** error)
{
	_ds__GetDomainInfo req;
	_ds__GetDomainInfoResponse resp;
	// Set the input parameters.
	req.userID = (gchar*)userID;

	if (m_DomainService.__ds__GetDomainInfo(&req, &resp) == 0)
	{
		// Get the output parameters.
		return new DomainInfo(resp.GetDomainInfoResult);
	}
	else
	{
		// g_set_error();
		return NULL;
	}
}

HostInfo* DomainService::GetHomeServer(const gchar* userID, GError** error)
{
	_ds__GetHomeServer req;
    _ds__GetHomeServerResponse resp;
	// Set the input parameters.
	req.user = (gchar*)userID;

	if (m_DomainService.__ds__GetHomeServer(&req, &resp) == 0)
	{
		return new HostInfo(resp.GetHomeServerResult);
	}
	else
	{
		// g_set_error();
		return NULL;
	}
}

ProvisionInfo* DomainService::ProvisionUserOnServer(const gchar *userName, const gchar *password, const gchar *ticket, GError **error)
{
	_ds__ProvisionUserOnServer req;
	_ds__ProvisionUserOnServerResponse resp;
	// Set the input parameters.
	req.user = (gchar*)userName;
	req.password = (gchar*)password;
	//req.ticket = ticket;
	m_DomainService.soap->userid = (char*)userName;
	m_DomainService.soap->passwd = (char*)password;
	
	if (m_DomainService.__ds__ProvisionUserOnServer(&req, &resp) == 0)
	{
		// Get the output parameters.
		return new ProvisionInfo(resp.ProvisionUserOnServerResult);
	}
	else
	{
		// g_set_error();
		return NULL;
	}
}

ProvisionInfo* DomainService::ProvisionUser(const gchar* userName, const gchar* password, GError **error)
{
	_ds__ProvisionUser req;
	_ds__ProvisionUserResponse resp;
	// Set the input parameters.
	req.user = (gchar*)userName;
	req.password = (gchar*)password;
	m_DomainService.soap->userid = (char*)userName;
	m_DomainService.soap->passwd = (char*)password;
	
	if (m_DomainService.__ds__ProvisionUser(&req, &resp) == 0)
	{
		// Get the output parameters.
		return new ProvisionInfo(resp.ProvisionUserResult);
	}
	else
	{
		// g_set_error();
		return NULL;
	}
}

void DomainService::CreateMaster()
{
	_ds__CreateMaster req;
	_ds__CreateMasterResponse resp;
	// Set the input parameters.
	req.collectionID;
	req.collectionName;
	req.memberID;
	req.memberName;
	req.memberRights;
	req.rootDirID;
	req.rootDirName;
	req.userID;
	
	// Get the output parameters.
	char *pStatus = resp.CreateMasterResult;
}
gboolean DomainService::RemoveServerCollections(const gchar* domainID, const gchar* userID, GError **error)
{
	_ds__RemoveServerCollections req;
	_ds__RemoveServerCollectionsResponse resp;
	// Set the input parameters.
	req.DomainID = (gchar*)domainID;
	req.UserID = (gchar*)userID;
			
	if (m_DomainService.__ds__RemoveServerCollections(&req, &resp) != 0)
	{
		// g_set_error();
		return false;
	}
	return true;
}

gchar* DomainService::GetDomainID(GError **error)
{
	_ds__GetDomainID req;
	_ds__GetDomainIDResponse resp;
	// Set the input parameters.
	
	if (m_DomainService.__ds__GetDomainID(&req, &resp) == 0)
	{
		// Get the output parameters.
		return g_strdup(resp.GetDomainIDResult);
	}
	else
	{
		// g_set_error();
		return NULL;
	}
}

//
// iFolderService class
//

iFolderIterator* iFolderService::GetiFolders(gint index, gint max, GError **error)
{
	_ifolder__GetiFolders req;
	_ifolder__GetiFoldersResponse resp;
	// Set the input parameters.
	req.index = index;
	req.max_ = max;

	if (m_iFolderService.__ifolder__GetiFolders(&req, &resp) == 0)
	{
		// Get the output parameters.
		return new iFolderIterator(resp.GetiFoldersResult);
	}
	else
	{
		//g_set_error();
		return NULL;
	}
}

iFolder* iFolderService::CreateiFolder(gchar *description, gchar* name, GError **error)
{
	_ifolder__CreateiFolder req;
	_ifolder__CreateiFolderResponse resp;
	// Set the input parameters.
	req.description = description;
	req.name = name;
	
	if (m_iFolderService.__ifolder__CreateiFolder(&req, &resp) == 0)
	{
		// Get the output parameters.
		return new iFolder(resp.CreateiFolderResult);
	}
	else
	{
		//g_set_error();
		return NULL;
	}
}
iFolderIterator* iFolderService::GetiFoldersByName(gint index, gint max, enum ifolder__SearchOperation operation, const gchar* pattern, GError **error)
{
	_ifolder__GetiFoldersByName req;
	_ifolder__GetiFoldersByNameResponse resp;
	// Set the input parameters.
	req.index = index;
	req.max_ = max;
	req.operation = operation;
	req.pattern = (gchar*)pattern;

	if (m_iFolderService.__ifolder__GetiFoldersByName(&req, &resp) == 0)
	{
		// Get the output parameters.
		return new iFolderIterator(resp.GetiFoldersByNameResult);
	}
	else
	{
		//g_set_error();
		return NULL;
	}
}

iFolderIterator* iFolderService::GetiFoldersBySearch(time_t after, gint index, gint max, enum ifolder__SearchOperation operation, const gchar* pattern, enum ifolder__MemberRole role, GError **error)
{
	_ifolder__GetiFoldersBySearch req;
	_ifolder__GetiFoldersBySearchResponse resp;
	// Set the input parameters.
	req.after = after;
	req.index = index;
	req.max_ = max;
	req.operation = operation;
	req.pattern = (gchar*)pattern;
	req.role = role;
	
	if (m_iFolderService.__ifolder__GetiFoldersBySearch(&req, &resp) == 0)
	{
		// Get the output parameters.
		return new iFolderIterator(resp.GetiFoldersBySearchResult);
	}
	else
	{
		//g_set_error();
		return NULL;
	}
}

gboolean iFolderService::RemoveMembership(const gchar *ifolderID, GError **error)
{
	_ifolder__RemoveMembership req;
	_ifolder__RemoveMembershipResponse resp;
	// Set the input parameters.
	req.ifolderID;
	
	if (m_iFolderService.__ifolder__RemoveMembership(&req, &resp) != 0)
	{
		//g_set_error();
		return false;
	}
	return true;
}

UserPolicy* iFolderService::GetAuthenticatedUserPolicy()
{
	_ifolder__GetAuthenticatedUserPolicy req;
	_ifolder__GetAuthenticatedUserPolicyResponse resp;
	// Set the input parameters.
	
	if (m_iFolderService.__ifolder__GetAuthenticatedUserPolicy(&req, &resp) == 0)
	{
		// Get the output parameters.
		return new UserPolicy(resp.GetAuthenticatedUserPolicyResult);
	}
	else
	{
		//g_set_error();
		return NULL;
	}
}

iFolderPolicy* iFolderService::GetiFolderPolicy(gchar *ifolderID, GError **error)
{
	_ifolder__GetiFolderPolicy req;
	_ifolder__GetiFolderPolicyResponse resp;
	// Set the input parameters.
	req.ifolderID = ifolderID;
	
	if (m_iFolderService.__ifolder__GetiFolderPolicy(&req, &resp) == 0)
	{
		// Get the output parameters.
		return new iFolderPolicy(resp.GetiFolderPolicyResult);
	}
	else
	{
		//g_set_error();
		return NULL;
	}
}

gboolean iFolderService::SetiFolderPolicy(iFolderPolicy *pPolicy, GError *error)
{
	gboolean bstatus = true;

	_ifolder__SetiFolderPolicy req;
	_ifolder__SetiFolderPolicyResponse resp;
	// Set the input parameters.
	req.props = pPolicy->To_ifolder__iFolderPolicy();

	// initialize the values.
	if (m_iFolderService.__ifolder__SetiFolderPolicy(&req, &resp) != 0)
	{
		//g_set_error();
		bstatus = false;
	}
	delete req.props;
	return bstatus;
}

iFolderEntry* iFolderService::CreateEntry(const gchar *entryName, const gchar *ifolderID, const gchar *parentID, enum ifolder__iFolderEntryType type, GError **error)
{
	_ifolder__CreateEntry req;
	_ifolder__CreateEntryResponse resp;
	// Set the input parameters.
	req.entryName = (gchar*)entryName;
	req.ifolderID = (gchar*)ifolderID;
	req.parentID = (gchar*)parentID;
	req.type = type;

	if (m_iFolderService.__ifolder__CreateEntry(&req, &resp) == 0)
	{
		// Get the output parameters.
		return new iFolderEntry(resp.CreateEntryResult);
	}
	else
	{
		//g_set_error();
		return NULL;
	}
}

gboolean iFolderService::DeleteEntry(const gchar *entryID, const gchar *ifolderID, GError **error)
{
	_ifolder__DeleteEntry req;
	_ifolder__DeleteEntryResponse resp;
	// Set the input parameters.
	req.entryID = (gchar*)entryID;
	req.ifolderID = (gchar*)ifolderID;

	if (m_iFolderService.__ifolder__DeleteEntry(&req, &resp) != 0)
	{
		//g_set_error();
		return false;
	}
	return true;
}

iFolderEntry* iFolderService::GetEntry(const gchar *entryID, const gchar *ifolderID, GError **error)
{
	_ifolder__GetEntry req;
	_ifolder__GetEntryResponse resp;
	// Set the input parameters.
	req.entryID = (gchar*)entryID;
	req.ifolderID = (gchar*)ifolderID;

	if (m_iFolderService.__ifolder__GetEntry(&req, &resp) == 0)
	{
		// Get the output parameters.
		return new iFolderEntry(resp.GetEntryResult);
	}
	else
	{
		//g_set_error();
		return NULL;
	}
}

iFolderEntry* iFolderService::GetEntryByPath(const gchar *entryPath, const gchar *ifolderID, GError **error)
{
	_ifolder__GetEntryByPath req;
	_ifolder__GetEntryByPathResponse resp;
	// Set the input parameters.
	req.entryPath = (gchar*)entryPath;
	req.ifolderID = (gchar*)ifolderID;

	if (m_iFolderService.__ifolder__GetEntryByPath(&req, &resp) == 0)
	{
		// Get the output parameters.
		return new iFolderEntry(resp.GetEntryByPathResult);
	}
	else
	{
		//g_set_error();
		return NULL;
	}
}

iFolderEntryIterator* iFolderService::GetEntries(const gchar *entryID, const gchar *ifolderID, gint index, gint max, GError **error)
{
	_ifolder__GetEntries req;
	_ifolder__GetEntriesResponse resp;
	// Set the input parameters.
	req.entryID = (gchar*)entryID;
	req.ifolderID = (gchar*)ifolderID;
	req.index = index;
	req.max_ = max;

	if (m_iFolderService.__ifolder__GetEntries(&req, &resp) == 0)
	{
		// Get the output parameters.
		return new iFolderEntryIterator(resp.GetEntriesResult);
	}
	else
	{
		//g_set_error();
		return NULL;
	}
}

iFolderEntryIterator* iFolderService::GetEntriesByName(const gchar *ifolderID, gint index, gint max, enum ifolder__SearchOperation operation, const gchar *parentID, const gchar *pattern, GError **error)
{
	_ifolder__GetEntriesByName req;
	_ifolder__GetEntriesByNameResponse resp;
	// Set the input parameters.
	req.ifolderID = (gchar*)ifolderID;
	req.index = index;
	req.max_ = max;
	req.operation = operation;
	req.parentID = (gchar*)parentID;
	req.pattern = (gchar*)pattern;
	
	if (m_iFolderService.__ifolder__GetEntriesByName(&req, &resp) == 0)
	{
		// Get the output parameters.
		return new iFolderEntryIterator(resp.GetEntriesByNameResult);
	}
	else
	{
		//g_set_error();
		return NULL;
	}
	// Get the output parameters.
	ifolder__iFolderEntrySet *pS = resp.GetEntriesByNameResult;
	pS->Items;
	pS->Total;
}

gchar* iFolderService::GetSetting(gchar *name, GError **error)
{
	_ifolder__GetSetting req;
	_ifolder__GetSettingResponse resp;
	// Set the input parameters.
	req.name = name;

	if (m_iFolderService.__ifolder__GetSetting(&req, &resp) == 0)
	{
		// Get the output parameters.
		return g_strdup(resp.GetSettingResult);
	}
	else
	{
		//g_set_error();
		return NULL;
	}
}

gboolean iFolderService::SetSetting(gchar *name, gchar *value, GError **error)
{
	_ifolder__SetSetting req;
	_ifolder__SetSettingResponse resp;
	// Set the input parameters.
	req.name = name;
	req.value = value;

	if (m_iFolderService.__ifolder__SetSetting(&req, &resp) != 0)
	{
		//g_set_error();
		return false;
	}
	return true;
}

void iFolderService::OpenFileRead()
{
	_ifolder__OpenFileRead req;
	_ifolder__OpenFileReadResponse resp;
	// Set the input parameters.
	req.entryID;
	req.ifolderID;
	
	// Get the output parameters.
	char *pStatus = resp.OpenFileReadResult;
}

void iFolderService::OpenFileWrite()
{
	_ifolder__OpenFileWrite req;
	_ifolder__OpenFileWriteResponse resp;
	// Set the input parameters.
	req.entryID;
	req.ifolderID;
	req.length;
	
	// Get the output parameters.
	char *pStatus = resp.OpenFileWriteResult;
}

void iFolderService::ReadFile()
{
	_ifolder__ReadFile req;
	_ifolder__ReadFileResponse resp;
	// Set the input parameters.
	req.file;
	req.size;
	
	// Get the output parameters.
	xsd__base64Binary *pBuff = resp.ReadFileResult;
	pBuff->__ptr;
	pBuff->__size;
}

void iFolderService::WriteFile()
{
	_ifolder__WriteFile req;
	_ifolder__WriteFileResponse resp;
	// Set the input parameters.
	req.buffer;
	req.file;
	
	// Get the output parameters.
}

void iFolderService::CloseFile()
{
	_ifolder__CloseFile req;
	_ifolder__CloseFileResponse resp;
	// Set the input parameters.
	req.file;
	
	// Get the output parameters.
}

void iFolderService::GetSystem()
{
	_ifolder__GetSystem req;
	_ifolder__GetSystemResponse resp;
	// Set the input parameters.
	
	// Get the output parameters.
	ifolder__iFolderSystem *pS = resp.GetSystemResult;
	pS->Description;
	pS->ID;
	pS->Name;
	pS->Version;
}

iFolderServer* iFolderService::GetHomeServer()
{
	_ifolder__GetHomeServer req;
	_ifolder__GetHomeServerResponse resp;
	// Set the input parameters.

	if (m_iFolderService.__ifolder__GetHomeServer(&req, &resp) == 0)
	{
		// Get the output parameters.
		return new iFolderServer(resp.GetHomeServerResult);
	}
	else
	{
		//g_set_error();
		return NULL;
	}
}

iFolderServerIterator* iFolderService::GetServers()
{
	_ifolder__GetServers req;
	_ifolder__GetServersResponse resp;
	// Set the input parameters.
	
	if (m_iFolderService.__ifolder__GetServers(&req, &resp) == 0)
	{
		// Get the output parameters.
		return new iFolderServerIterator(resp.GetServersResult);
	}
	else
	{
		//g_set_error();
		return NULL;
	}
}

gboolean iFolderService::DeleteiFolder(const gchar *ifolderID, GError **error)
{
	_ifolder__DeleteiFolder req;
	_ifolder__DeleteiFolderResponse resp;
	// Set the input parameters.
	req.ifolderID = (gchar*)ifolderID;

	if (m_iFolderService.__ifolder__DeleteiFolder(&req, &resp) != 0)
	{
		//g_set_error();
		return false;
	}
	return true;
}

iFolder* iFolderService::GetiFolder(const gchar *ifolderID, GError **error)
{
	_ifolder__GetiFolder req;
	_ifolder__GetiFolderResponse resp;
	// Set the input parameters.
	req.ifolderID = (gchar*)ifolderID;

	if (m_iFolderService.__ifolder__GetiFolder(&req, &resp) == 0)
	{
		// Get the output parameters.
		return new iFolder(resp.GetiFolderResult);
	}
	else
	{
		//g_set_error();
		return NULL;
	}
}

iFolderDetails* iFolderService::GetiFolderDetails(const gchar *ifolderID, GError **error)
{
	_ifolder__GetiFolderDetails req;
	_ifolder__GetiFolderDetailsResponse resp;
	// Set the input parameters.
	req.ifolderID = (gchar*)ifolderID;
	
	if (m_iFolderService.__ifolder__GetiFolderDetails(&req, &resp) == 0)
	{
		// Get the output parameters.
		return new iFolderDetails(resp.GetiFolderDetailsResult);
	}
	else
	{
		//g_set_error();
		return NULL;
	}
}

gboolean iFolderService::SetiFolderDescription(const gchar *ifolderID, const gchar *description, GError **error)
{
	_ifolder__SetiFolderDescription req;
	_ifolder__SetiFolderDescriptionResponse resp;
	// Set the input parameters.
	req.description = (gchar*)description;
	req.ifolderID = (gchar*)ifolderID;
	
	if (m_iFolderService.__ifolder__SetiFolderDescription(&req, &resp) != 0)
	{
		//g_set_error();
		return false;
	}
	return true;
}

gboolean iFolderService::PublishiFolder(const gchar *ifolderID, gboolean publish, GError **error)
{
	_ifolder__PublishiFolder req;
	_ifolder__PublishiFolderResponse resp;
	// Set the input parameters.
	req.ifolderID = (gchar*)ifolderID;
	req.publish = publish;

	if (m_iFolderService.__ifolder__PublishiFolder(&req, &resp) != 0)
	{
		//g_set_error();
		return false;
	}
	return true;
}

ChangeEntryIterator* iFolderService::GetChanges(const gchar *ifolderID, const gchar *itemID, gint index, gint max, GError **error)
{
	_ifolder__GetChanges req;
	_ifolder__GetChangesResponse resp;
	// Set the input parameters.
	req.ifolderID = (gchar*)ifolderID;
	req.itemID = (gchar*)itemID;
	req.index = index;
	req.max_ = max;

	if (m_iFolderService.__ifolder__GetChanges(&req, &resp) == 0)
	{
		// Get the output parameters.
		return new ChangeEntryIterator(resp.GetChangesResult);
	}
	else
	{
		//g_set_error();
		return NULL;
	}
}

iFolderUser* iFolderService::GetAuthenticatedUser()
{
	_ifolder__GetAuthenticatedUser req;
	_ifolder__GetAuthenticatedUserResponse resp;
	// Set the input parameters.
	
	if (m_iFolderService.__ifolder__GetAuthenticatedUser(&req, &resp) == 0)
	{
		// Get the output parameters.
		return new iFolderUser(resp.GetAuthenticatedUserResult);
	}
	else
	{
		//g_set_error();
		return NULL;
	}
}

gboolean iFolderService::SetMemberRights(const gchar *ifolderID, const gchar *userID, enum ifolder__Rights rights, GError **error)
{
	_ifolder__SetMemberRights req;
	_ifolder__SetMemberRightsResponse resp;
	// Set the input parameters.
	req.ifolderID = (gchar*)ifolderID;
	req.rights = rights;
	req.userID = (gchar*)userID;

	if (m_iFolderService.__ifolder__SetMemberRights(&req, &resp) != 0)
	{
		//g_set_error();
		return false;
	}
	return true;
}

gboolean iFolderService::AddMember(const gchar *ifolderID, const gchar *userID, enum ifolder__Rights rights, GError **error)
{
	_ifolder__AddMember req;
	_ifolder__AddMemberResponse resp;
	// Set the input parameters.
	req.ifolderID = (gchar*)ifolderID;
	req.rights = rights;
	req.userID = (gchar*)userID;

	if (m_iFolderService.__ifolder__AddMember(&req, &resp) != 0)
	{
		//g_set_error();
		return false;
	}
	return true;
}

gboolean iFolderService::RemoveMember(const gchar *ifolderID, const gchar *userID, GError **error)
{
	_ifolder__RemoveMember req;
	_ifolder__RemoveMemberResponse resp;
	// Set the input parameters.
	req.ifolderID = (gchar*)ifolderID;
	req.userID = (gchar*)userID;

	if (m_iFolderService.__ifolder__RemoveMember(&req, &resp) != 0)
	{
		//g_set_error();
		return false;
	}
	return true;
}

gboolean iFolderService::SetiFolderOwner(const gchar *ifolderID, const gchar *userID, GError **error)
{
	_ifolder__SetiFolderOwner req;
	_ifolder__SetiFolderOwnerResponse resp;
	// Set the input parameters.
	req.ifolderID = (gchar*)ifolderID;
	req.userID = (gchar*)userID;
	
	if (m_iFolderService.__ifolder__SetiFolderOwner(&req, &resp) != 0)
	{
		//g_set_error();
		return false;
	}
	return true;
}

iFolderUserIterator* iFolderService::GetMembers(const gchar* ifolderID, gint index, gint max, GError **error)
{
	_ifolder__GetMembers req;
	_ifolder__GetMembersResponse resp;
	// Set the input parameters.
	req.ifolderID = (gchar*)ifolderID;
	req.index = index;
	req.max_ = max;

	if (m_iFolderService.__ifolder__GetMembers(&req, &resp) == 0)
	{
		// Get the output parameters.
		return new iFolderUserIterator(resp.GetMembersResult);
	}
	else
	{
		//g_set_error();
		return NULL;
	}
}

iFolderUserIterator* iFolderService::GetUsers(gint index, gint max, GError **error)
{
	_ifolder__GetUsers req;
	_ifolder__GetUsersResponse resp;
	// Set the input parameters.
	req.index = index;
	req.max_ = max;

	if (m_iFolderService.__ifolder__GetUsers(&req, &resp) == 0)
	{
		// Get the output parameters.
		return new iFolderUserIterator(resp.GetUsersResult);
	}
	else
	{
		//g_set_error();
		return NULL;
	}
}

iFolderUser* iFolderService::GetUser(const gchar *pUserID, GError **error)
{
	_ifolder__GetUser req;
	_ifolder__GetUserResponse resp;
	// Set the input parameters.
	req.userID = (gchar*)pUserID;

	if (m_iFolderService.__ifolder__GetUser(&req, &resp) == 0)
	{
		// Get the output parameters.
		return new iFolderUser(resp.GetUserResult);
	}
	else
	{
		//g_set_error();
		return NULL;
	}
}

iFolderUserDetails* iFolderService::GetUserDetails(const gchar *pUserID, GError **error)
{
	_ifolder__GetUserDetails req;
	_ifolder__GetUserDetailsResponse resp;
	// Set the input parameters.
	req.userID = (gchar*)pUserID;
	
	if (m_iFolderService.__ifolder__GetUserDetails(&req, &resp) == 0)
	{
		// Get the output parameters.
		return new iFolderUserDetails(resp.GetUserDetailsResult);
	}
	else
	{
		//g_set_error();
		return NULL;
	}
}

iFolderUserIterator* iFolderService::GetUsersBySearch(gint index, gint max, enum ifolder__SearchOperation operation, const gchar* pattern, enum ifolder__SearchProperty property, GError **error)
{
	_ifolder__GetUsersBySearch req;
	_ifolder__GetUsersBySearchResponse resp;
	// Set the input parameters.
	req.index = index;
	req.max_ = max;
	req.operation = operation;
	req.pattern = (gchar*)pattern;
	req.property = property;

	if (m_iFolderService.__ifolder__GetUsersBySearch(&req, &resp) == 0)
	{
		// Get the output parameters.
		return new iFolderUserIterator(resp.GetUsersBySearchResult);
	}
	else
	{
		//g_set_error();
		return NULL;
	}
} //namespace ifweb
}
