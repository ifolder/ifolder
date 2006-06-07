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
#include "simiasDomain_USCOREx0020_USCOREServiceSoapProxy.h"
#include "simiasiFolderWebSoapProxy.h"


namespace ifweb
{
GQuark	IF_SOAP_ERROR = g_quark_from_string("ifweb");

// IFServiceManager class
void IFServiceManager::Initialize(WebService *service, struct soap *soap)
{
	IFDomain *pDomain = service->m_pDomain;
	soap->mode |= SOAP_C_UTFSTRING | SOAP_IO_KEEPALIVE;
	soap->imode |= SOAP_C_UTFSTRING | SOAP_IO_KEEPALIVE;
	soap->omode |= SOAP_C_UTFSTRING | SOAP_IO_KEEPALIVE;
	soap->user = pDomain;
	soap->send_timeout = 30;

	// Set the parse_header callback so that we can get the login status.
	pDomain->m_Parsehdr = soap->fparsehdr;
	soap->fparsehdr = IFDomain::ParseLoginHeader;
	soap->fparsehdr = IFDomain::ParseLoginHeader;

	// Get the cookies.
	if (pDomain->m_Authenticated)
		soap->cookies = soap_copy_cookies(pDomain->m_DS->m_DomainService.soap);
}

DomainService* IFServiceManager::GetDomainService(const gchar* domainID, const gchar *host)
{
	IFDomain *pDomain = IFDomain::GetDomainByID(domainID);
	if (pDomain != NULL)
	{
		// TODO fix host to support multi-server
		DomainService *pService = new DomainService(pDomain);
		pService->m_DomainService.endpoint = IFApplication::BuildUrlToService(host, pService->m_DomainService.endpoint);
		Initialize(pService, pService->m_DomainService.soap);
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
		pService->m_DomainService.endpoint = IFApplication::BuildUrlToService(host, pService->m_DomainService.endpoint);
		Initialize(pService, pService->m_DomainService.soap);
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
		pService->m_iFolderService.endpoint = IFApplication::BuildUrlToService(host, pService->m_iFolderService.endpoint);
		Initialize(pService, pService->m_iFolderService.soap);
		return pService;
	}
	return NULL;
}

iFolderService* IFServiceManager::GetiFolderService(IFDomain *pDomain, const gchar *host)
{
	if (pDomain != NULL)
	{
		// TODO fix host to support multi-server
		iFolderService *pService = new iFolderService(pDomain);
		pService->m_iFolderService.endpoint = IFApplication::BuildUrlToService(host, pService->m_iFolderService.endpoint);
		Initialize(pService, pService->m_iFolderService.soap);
		return pService;
	}
	return NULL;
}

// WebService class

gboolean WebService::HandleError(struct soap *soap, GError **error)
{
	gint serror = soap->error;
	if (serror == 401)
	{
		// We are not logged in log in now.
		if (m_pDomain->m_Authenticated)
		{
			if (m_pDomain->Login(m_pDomain->m_UserPassword, error))
			{
				// We handled the error return true.
				return true;
			}
		}
	}
	g_set_error(error, IF_SOAP_ERROR, serror, "Soap Error");
	return false;
}

// DomainService class

DomainInfo* DomainService::GetDomainInfo(const gchar* userID, GError** error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ds__GetDomainInfo req;
	_ds__GetDomainInfoResponse resp;
	// Set the input parameters.
	req.userID = (gchar*)userID;

	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_DomainService.__ds__GetDomainInfo(&req, &resp) == 0)
		{
			// Get the output parameters.
			return new DomainInfo(resp.GetDomainInfoResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

HostInfo* DomainService::GetHomeServer(const gchar* userID, GError** error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ds__GetHomeServer req;
    _ds__GetHomeServerResponse resp;
	// Set the input parameters.
	req.user = (gchar*)userID;

	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_DomainService.__ds__GetHomeServer(&req, &resp) == 0)
		{
			return new HostInfo(resp.GetHomeServerResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

ProvisionInfo* DomainService::ProvisionUserOnServer(const gchar *userName, const gchar *password, const gchar *ticket, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}
	
	_ds__ProvisionUserOnServer req;
	_ds__ProvisionUserOnServerResponse resp;
	// Set the input parameters.
	req.user = (gchar*)userName;
	req.password = (gchar*)password;
	//req.ticket = ticket;
	m_DomainService.soap->userid = (char*)userName;
	m_DomainService.soap->passwd = (char*)password;
	
	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_DomainService.__ds__ProvisionUserOnServer(&req, &resp) == 0)
		{
			// Get the output parameters.
			return new ProvisionInfo(resp.ProvisionUserOnServerResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

ProvisionInfo* DomainService::ProvisionUser(const gchar* userName, const gchar* password, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}
	
	_ds__ProvisionUser req;
	_ds__ProvisionUserResponse resp;
	// Set the input parameters.
	req.user = (gchar*)userName;
	req.password = (gchar*)password;
	m_DomainService.soap->userid = (char*)userName;
	m_DomainService.soap->passwd = (char*)password;
	
	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_DomainService.__ds__ProvisionUser(&req, &resp) == 0)
		{
			// Get the output parameters.
			return new ProvisionInfo(resp.ProvisionUserResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

void DomainService::CreateMaster()
{
	//if (!m_pDomain->m_Authenticated)
	//{
	//	g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
	//	return;
	//}

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
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return false;
	}
	
	_ds__RemoveServerCollections req;
	_ds__RemoveServerCollectionsResponse resp;
	// Set the input parameters.
	req.DomainID = (gchar*)domainID;
	req.UserID = (gchar*)userID;
			
	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_DomainService.__ds__RemoveServerCollections(&req, &resp) == 0)
		{
			return true;
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return false;
}

gchar* DomainService::GetDomainID(GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ds__GetDomainID req;
	_ds__GetDomainIDResponse resp;
	// Set the input parameters.
	
	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_DomainService.__ds__GetDomainID(&req, &resp) == 0)
		{
			// Get the output parameters.
			return g_strdup(resp.GetDomainIDResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

//
// iFolderService class
//

iFolderIterator* iFolderService::GetiFolders(gint index, gint max, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ifolder__GetiFolders req;
	_ifolder__GetiFoldersResponse resp;
	// Set the input parameters.
	req.index = index;
	req.max_ = max;

	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__GetiFolders(&req, &resp) == 0)
		{
			// Get the output parameters.
			return new iFolderIterator(resp.GetiFoldersResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

iFolder* iFolderService::CreateiFolder(const gchar *description, const gchar* name, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}
	
	_ifolder__CreateiFolder req;
	_ifolder__CreateiFolderResponse resp;
	// Set the input parameters.
	req.description = (gchar*)description;
	req.name = (gchar*)name;
	
	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__CreateiFolder(&req, &resp) == 0)
		{
			// Get the output parameters.
			return new iFolder(resp.CreateiFolderResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

iFolderIterator* iFolderService::GetiFoldersByName(gint index, gint max, enum ifolder__SearchOperation operation, const gchar* pattern, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ifolder__GetiFoldersByName req;
	_ifolder__GetiFoldersByNameResponse resp;
	// Set the input parameters.
	req.index = index;
	req.max_ = max;
	req.operation = operation;
	req.pattern = (gchar*)pattern;

	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__GetiFoldersByName(&req, &resp) == 0)
		{
			// Get the output parameters.
			return new iFolderIterator(resp.GetiFoldersByNameResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

iFolderIterator* iFolderService::GetiFoldersBySearch(time_t after, gint index, gint max, enum ifolder__SearchOperation operation, const gchar* pattern, enum ifolder__MemberRole role, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ifolder__GetiFoldersBySearch req;
	_ifolder__GetiFoldersBySearchResponse resp;
	// Set the input parameters.
	req.after = after;
	req.index = index;
	req.max_ = max;
	req.operation = operation;
	req.pattern = (gchar*)pattern;
	req.role = role;
	
	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__GetiFoldersBySearch(&req, &resp) == 0)
		{
			// Get the output parameters.
			return new iFolderIterator(resp.GetiFoldersBySearchResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

gboolean iFolderService::RemoveMembership(const gchar *ifolderID, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return false;
	}

	_ifolder__RemoveMembership req;
	_ifolder__RemoveMembershipResponse resp;
	// Set the input parameters.
	req.ifolderID;
	
	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__RemoveMembership(&req, &resp) == 0)
		{
			return true;
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return false;
}

UserPolicy* iFolderService::GetAuthenticatedUserPolicy(GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ifolder__GetAuthenticatedUserPolicy req;
	_ifolder__GetAuthenticatedUserPolicyResponse resp;
	// Set the input parameters.
	
	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__GetAuthenticatedUserPolicy(&req, &resp) == 0)
		{
			// Get the output parameters.
			return new UserPolicy(resp.GetAuthenticatedUserPolicyResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

iFolderPolicy* iFolderService::GetiFolderPolicy(gchar *ifolderID, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ifolder__GetiFolderPolicy req;
	_ifolder__GetiFolderPolicyResponse resp;
	// Set the input parameters.
	req.ifolderID = ifolderID;
	
	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__GetiFolderPolicy(&req, &resp) == 0)
		{
			// Get the output parameters.
			return new iFolderPolicy(resp.GetiFolderPolicyResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
}

gboolean iFolderService::SetiFolderPolicy(iFolderPolicy *pPolicy, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return false;
	}

	gboolean bstatus = true;

	_ifolder__SetiFolderPolicy req;
	_ifolder__SetiFolderPolicyResponse resp;
	// Set the input parameters.
	req.props = pPolicy->To_ifolder__iFolderPolicy();

	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__SetiFolderPolicy(&req, &resp) == 0)
		{
			bstatus = true;
			break;
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	bstatus = false;
	delete req.props;
	return bstatus;
}

iFolderEntry* iFolderService::CreateEntry(const gchar *entryName, const gchar *ifolderID, const gchar *parentID, enum ifolder__iFolderEntryType type, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ifolder__CreateEntry req;
	_ifolder__CreateEntryResponse resp;
	// Set the input parameters.
	req.entryName = (gchar*)entryName;
	req.ifolderID = (gchar*)ifolderID;
	req.parentID = (gchar*)parentID;
	req.type = type;

	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__CreateEntry(&req, &resp) == 0)
		{
			// Get the output parameters.
			return new iFolderEntry(resp.CreateEntryResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

gboolean iFolderService::DeleteEntry(const gchar *entryID, const gchar *ifolderID, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return false;
	}

	_ifolder__DeleteEntry req;
	_ifolder__DeleteEntryResponse resp;
	// Set the input parameters.
	req.entryID = (gchar*)entryID;
	req.ifolderID = (gchar*)ifolderID;

	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__DeleteEntry(&req, &resp) == 0)
		{
			return true;
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return false;
}

iFolderEntry* iFolderService::GetEntry(const gchar *entryID, const gchar *ifolderID, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ifolder__GetEntry req;
	_ifolder__GetEntryResponse resp;
	// Set the input parameters.
	req.entryID = (gchar*)entryID;
	req.ifolderID = (gchar*)ifolderID;

	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__GetEntry(&req, &resp) == 0)
		{
			// Get the output parameters.
			return new iFolderEntry(resp.GetEntryResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

iFolderEntry* iFolderService::GetEntryByPath(const gchar *entryPath, const gchar *ifolderID, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ifolder__GetEntryByPath req;
	_ifolder__GetEntryByPathResponse resp;
	// Set the input parameters.
	req.entryPath = (gchar*)entryPath;
	req.ifolderID = (gchar*)ifolderID;

	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__GetEntryByPath(&req, &resp) == 0)
		{
			// Get the output parameters.
			return new iFolderEntry(resp.GetEntryByPathResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

iFolderEntryIterator* iFolderService::GetEntries(const gchar *entryID, const gchar *ifolderID, gint index, gint max, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ifolder__GetEntries req;
	_ifolder__GetEntriesResponse resp;
	// Set the input parameters.
	req.entryID = (gchar*)entryID;
	req.ifolderID = (gchar*)ifolderID;
	req.index = index;
	req.max_ = max;

	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__GetEntries(&req, &resp) == 0)
		{
			// Get the output parameters.
			return new iFolderEntryIterator(resp.GetEntriesResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

iFolderEntryIterator* iFolderService::GetEntriesByName(const gchar *ifolderID, gint index, gint max, enum ifolder__SearchOperation operation, const gchar *parentID, const gchar *pattern, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ifolder__GetEntriesByName req;
	_ifolder__GetEntriesByNameResponse resp;
	// Set the input parameters.
	req.ifolderID = (gchar*)ifolderID;
	req.index = index;
	req.max_ = max;
	req.operation = operation;
	req.parentID = (gchar*)parentID;
	req.pattern = (gchar*)pattern;
	
	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__GetEntriesByName(&req, &resp) == 0)
		{
			// Get the output parameters.
			return new iFolderEntryIterator(resp.GetEntriesByNameResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

gchar* iFolderService::GetSetting(gchar *name, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ifolder__GetSetting req;
	_ifolder__GetSettingResponse resp;
	// Set the input parameters.
	req.name = name;

	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__GetSetting(&req, &resp) == 0)
		{
			// Get the output parameters.
			return g_strdup(resp.GetSettingResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

gboolean iFolderService::SetSetting(gchar *name, gchar *value, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return false;
	}

	_ifolder__SetSetting req;
	_ifolder__SetSettingResponse resp;
	// Set the input parameters.
	req.name = name;
	req.value = value;

	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__SetSetting(&req, &resp) == 0)
		{
			return true;
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return false;
}

void iFolderService::OpenFileRead()
{
	//if (!m_pDomain->m_Authenticated)
	//{
	//	g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
	//	return;
	//}

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
	//if (!m_pDomain->m_Authenticated)
	//{
	//	g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
	//	return;
	//}

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
	//if (!m_pDomain->m_Authenticated)
	//{
	//	g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
	//	return;
	//}

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
	//if (!m_pDomain->m_Authenticated)
	//{
	//	g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
	//	return;
	//}

	_ifolder__WriteFile req;
	_ifolder__WriteFileResponse resp;
	// Set the input parameters.
	req.buffer;
	req.file;
	
	// Get the output parameters.
}

void iFolderService::CloseFile()
{
	//if (!m_pDomain->m_Authenticated)
	//{
	//	g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
	//	return;
	//}

	_ifolder__CloseFile req;
	_ifolder__CloseFileResponse resp;
	// Set the input parameters.
	req.file;
	
	// Get the output parameters.
}

iFolderSystem* iFolderService::GetSystem(GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ifolder__GetSystem req;
	_ifolder__GetSystemResponse resp;
	// Set the input parameters.
	
	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__GetSystem(&req, &resp) == 0)
		{
			return new iFolderSystem(resp.GetSystemResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

iFolderServer* iFolderService::GetHomeServer(GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ifolder__GetHomeServer req;
	_ifolder__GetHomeServerResponse resp;
	// Set the input parameters.

	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__GetHomeServer(&req, &resp) == 0)
		{
			// Get the output parameters.
			return new iFolderServer(resp.GetHomeServerResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

iFolderServerIterator* iFolderService::GetServers(GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ifolder__GetServers req;
	_ifolder__GetServersResponse resp;
	// Set the input parameters.
	
	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__GetServers(&req, &resp) == 0)
		{
			// Get the output parameters.
			return new iFolderServerIterator(resp.GetServersResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

gboolean iFolderService::DeleteiFolder(const gchar *ifolderID, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ifolder__DeleteiFolder req;
	_ifolder__DeleteiFolderResponse resp;
	// Set the input parameters.
	req.ifolderID = (gchar*)ifolderID;

	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__DeleteiFolder(&req, &resp) == 0)
		{
			return true;
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return false;
}

iFolder* iFolderService::GetiFolder(const gchar *ifolderID, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ifolder__GetiFolder req;
	_ifolder__GetiFolderResponse resp;
	// Set the input parameters.
	req.ifolderID = (gchar*)ifolderID;

	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__GetiFolder(&req, &resp) == 0)
		{
			// Get the output parameters.
			return new iFolder(resp.GetiFolderResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

iFolderDetails* iFolderService::GetiFolderDetails(const gchar *ifolderID, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ifolder__GetiFolderDetails req;
	_ifolder__GetiFolderDetailsResponse resp;
	// Set the input parameters.
	req.ifolderID = (gchar*)ifolderID;
	
	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__GetiFolderDetails(&req, &resp) == 0)
		{
			// Get the output parameters.
			return new iFolderDetails(resp.GetiFolderDetailsResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

gboolean iFolderService::SetiFolderDescription(const gchar *ifolderID, const gchar *description, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return false;
	}

	_ifolder__SetiFolderDescription req;
	_ifolder__SetiFolderDescriptionResponse resp;
	// Set the input parameters.
	req.description = (gchar*)description;
	req.ifolderID = (gchar*)ifolderID;
	
	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__SetiFolderDescription(&req, &resp) == 0)
		{
			return true;
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return false;
}

gboolean iFolderService::PublishiFolder(const gchar *ifolderID, gboolean publish, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return false;
	}

	_ifolder__PublishiFolder req;
	_ifolder__PublishiFolderResponse resp;
	// Set the input parameters.
	req.ifolderID = (gchar*)ifolderID;
	req.publish = (publish != 0); // This is to work around a MS C4800 compile error.

	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__PublishiFolder(&req, &resp) == 0)
		{
			return true;
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return false;
}

ChangeEntryIterator* iFolderService::GetChanges(const gchar *ifolderID, const gchar *itemID, gint index, gint max, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ifolder__GetChanges req;
	_ifolder__GetChangesResponse resp;
	// Set the input parameters.
	req.ifolderID = (gchar*)ifolderID;
	req.itemID = (gchar*)itemID;
	req.index = index;
	req.max_ = max;

	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__GetChanges(&req, &resp) == 0)
		{
			// Get the output parameters.
			return new ChangeEntryIterator(resp.GetChangesResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

iFolderUser* iFolderService::GetAuthenticatedUser(GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ifolder__GetAuthenticatedUser req;
	_ifolder__GetAuthenticatedUserResponse resp;
	// Set the input parameters.
	
	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__GetAuthenticatedUser(&req, &resp) == 0)
		{
			// Get the output parameters.
			return new iFolderUser(resp.GetAuthenticatedUserResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

gboolean iFolderService::SetMemberRights(const gchar *ifolderID, const gchar *userID, enum ifolder__Rights rights, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return false;
	}

	_ifolder__SetMemberRights req;
	_ifolder__SetMemberRightsResponse resp;
	// Set the input parameters.
	req.ifolderID = (gchar*)ifolderID;
	req.rights = rights;
	req.userID = (gchar*)userID;

	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__SetMemberRights(&req, &resp) == 0)
		{
			return true;
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return false;
}

gboolean iFolderService::AddMember(const gchar *ifolderID, const gchar *userID, enum ifolder__Rights rights, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return false;
	}

	_ifolder__AddMember req;
	_ifolder__AddMemberResponse resp;
	// Set the input parameters.
	req.ifolderID = (gchar*)ifolderID;
	req.rights = rights;
	req.userID = (gchar*)userID;

	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__AddMember(&req, &resp) == 0)
		{
			return true;
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return false;
}

gboolean iFolderService::RemoveMember(const gchar *ifolderID, const gchar *userID, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return false;
	}

	_ifolder__RemoveMember req;
	_ifolder__RemoveMemberResponse resp;
	// Set the input parameters.
	req.ifolderID = (gchar*)ifolderID;
	req.userID = (gchar*)userID;

	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__RemoveMember(&req, &resp) == 0)
		{
			return true;
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return false;
}

gboolean iFolderService::SetiFolderOwner(const gchar *ifolderID, const gchar *userID, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return false;
	}

	_ifolder__SetiFolderOwner req;
	_ifolder__SetiFolderOwnerResponse resp;
	// Set the input parameters.
	req.ifolderID = (gchar*)ifolderID;
	req.userID = (gchar*)userID;
	
	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__SetiFolderOwner(&req, &resp) == 0)
		{
			return true;
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return false;
}

iFolderUserIterator* iFolderService::GetMembers(const gchar* ifolderID, gint index, gint max, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ifolder__GetMembers req;
	_ifolder__GetMembersResponse resp;
	// Set the input parameters.
	req.ifolderID = (gchar*)ifolderID;
	req.index = index;
	req.max_ = max;

	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__GetMembers(&req, &resp) == 0)
		{
			// Get the output parameters.
			return new iFolderUserIterator(resp.GetMembersResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

iFolderUserIterator* iFolderService::GetUsers(gint index, gint max, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ifolder__GetUsers req;
	_ifolder__GetUsersResponse resp;
	// Set the input parameters.
	req.index = index;
	req.max_ = max;

	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__GetUsers(&req, &resp) == 0)
		{
			// Get the output parameters.
			return new iFolderUserIterator(resp.GetUsersResult);
		}
		else
		{	
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

iFolderUser* iFolderService::GetUser(const gchar *pUserID, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ifolder__GetUser req;
	_ifolder__GetUserResponse resp;
	// Set the input parameters.
	req.userID = (gchar*)pUserID;

	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__GetUser(&req, &resp) == 0)
		{
			// Get the output parameters.
			return new iFolderUser(resp.GetUserResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

iFolderUserDetails* iFolderService::GetUserDetails(const gchar *pUserID, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ifolder__GetUserDetails req;
	_ifolder__GetUserDetailsResponse resp;
	// Set the input parameters.
	req.userID = (gchar*)pUserID;
	
	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__GetUserDetails(&req, &resp) == 0)
		{
			// Get the output parameters.
			return new iFolderUserDetails(resp.GetUserDetailsResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

iFolderUserIterator* iFolderService::GetUsersBySearch(gint index, gint max, enum ifolder__SearchOperation operation, const gchar* pattern, enum ifolder__SearchProperty property, GError **error)
{
	if (!m_pDomain->m_Authenticated)
	{
		g_set_error(error, IF_SOAP_ERROR, IF_ERR_NOT_AUTHENTICATED, "Client is not authenticated");
		return NULL;
	}

	_ifolder__GetUsersBySearch req;
	_ifolder__GetUsersBySearchResponse resp;
	// Set the input parameters.
	req.index = index;
	req.max_ = max;
	req.operation = operation;
	req.pattern = (gchar*)pattern;
	req.property = property;

	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__GetUsersBySearch(&req, &resp) == 0)
		{
			// Get the output parameters.
			return new iFolderUserIterator(resp.GetUsersBySearchResult);
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return NULL;
}

gboolean iFolderService::Login(const gchar *user, const gchar *password, GError **error)
{
	gboolean bStatus = false;
	m_iFolderService.soap->userid = (char*)user;
	m_iFolderService.soap->passwd = (char*)password;

	_ifolder__GetSystem req;
	_ifolder__GetSystemResponse resp;
	// Set the input parameters.

	gboolean retry = true;
	while (retry)
	{
		g_clear_error(error);
		if (m_iFolderService.__ifolder__GetSystem(&req, &resp) == 0)
		{
			return true;
		}
		else
		{
			retry = HandleError(resp.soap, error);
		}
	}
	return false;
}

} //namespace ifweb
