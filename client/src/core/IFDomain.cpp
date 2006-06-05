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
#include "IFDomain.h"
#include "IFApplication.h"
#include "simias.nsmap"
#include "IFServices.h"

// IFDomain class

IFDomain::IFDomain(const gchar *host)
{
	m_UserPassword = NULL;
	m_POBoxID = NULL;
	m_Name = NULL;
	m_ID = NULL;
	m_Version = NULL;
	m_Description = NULL;
	m_HomeHost = g_strdup(host);
	m_MasterHost = g_strdup(host);
	m_UserName = NULL;
	m_UserID = NULL;
	m_Authenticated = false;
	m_Active = true;
	m_Default = false;
	m_DS = ifweb::IFServiceManager::GetDomainService(this, host);
	m_iFS = ifweb::IFServiceManager::GetiFolderService(this, host);;
}
	
IFDomain::~IFDomain(void)
{
	g_free(m_UserPassword);
	g_free(m_POBoxID);
	
	g_free(m_Name);
	g_free(m_ID);
	g_free(m_Version);
	g_free(m_Description);
	g_free(m_HomeHost);
	g_free(m_MasterHost);
	g_free(m_UserName);
	g_free(m_UserID);
	//if (m_DS != NULL)
		// TODO delete m_DS;
	//if (m_iFS != NULL)
		// TODO delete m_iFS;
}

IFDomain* IFDomain::Add(const gchar* userName, const gchar* password, const gchar* host, GError **error)
{
	gboolean failed = true;
	IFDomain *pDomain = new IFDomain(host);
	pDomain->m_UserName = g_strdup(userName);
	if (!pDomain->Login(password, error))
	{
		// We could not login.
		delete pDomain;
		return NULL;
	}

	ifweb::DomainService* domainService = pDomain->m_DS;

	// Make sure that we do not already belong to this domain.
	pDomain->m_ID = domainService->GetDomainID(error);
	if (pDomain->m_ID == NULL)
	{
		g_warning("Failed to contact host %s", host);
	}
	else
	{
		IFDomain *existingDomain = IFDomain::GetDomainByID(pDomain->m_ID);
		if (existingDomain != NULL)
		{
			// We have already joined this domain.
			delete pDomain;
			return existingDomain;
		}
		/*
		// Get the home server.
		_ds__GetHomeServer hsReq;
		_ds__GetHomeServerResponse hsResp;
		domainService->__ds__GetHomeServer(&hsReq, &hsResp);
		*/

		// Provision the user;
		ifweb::ProvisionInfo *pvInfo = domainService->ProvisionUser(userName, password, error);
		if (pvInfo != NULL)
		{
			pDomain->m_UserID = g_strdup(pvInfo->m_UserID);
			pDomain->m_UserPassword = g_strdup(password);
			pDomain->m_POBoxID = g_strdup(pvInfo->m_POBoxID);
			g_message("provisioned");
			delete pvInfo;

			// Save the cookies.
			ifweb::iFolderService* ifolderService = pDomain->m_iFS;
			ifolderService->m_iFolderService.soap->cookies = soap_copy_cookies(domainService->m_DomainService.soap);
			pDomain->m_Authenticated = true;
			
			// Now get the System Info.
			ifweb::iFolderSystem *sInfo = ifolderService->GetSystem(error);
			if (sInfo != NULL)
			{
				pDomain->m_ID = g_strdup(sInfo->m_ID);
				pDomain->m_Description = g_strdup(sInfo->m_Description);
				pDomain->m_Name = g_strdup(sInfo->m_Name);
				pDomain->m_Version = g_strdup(sInfo->m_Version);
				delete sInfo;
			
				// Add the domain to the list and return and set success.
				if (IFDomainList::Count() == 0)
					pDomain->m_Default = true;
				IFDomainList::Insert(pDomain);
				failed = false;
			}
		}
	}
	if (failed)
	{
		struct soap* soap = domainService->m_DomainService.soap;
		soap_print_fault(soap, stderr);
		soap_print_fault_location(soap, stderr);
		g_set_error(error, IF_CORE_ERROR, soap->error, "Soap Error");
		delete pDomain;
		return NULL;
	}
	return pDomain;
}

int IFDomain::Remove()
{
	IFDomainList::Remove(m_ID);
	return 0;
}

int IFDomain::ParseLoginHeader(struct soap *soap, const char *key, const char*val)
{
	printf("%s : %s\n", key, val);
	IFDomain *pDomain = (IFDomain*)soap->user;
	if (strcmp(key, "Simias-Error") == 0)
	{
		struct _SimiasError_ sError = SimiasErrorToIFError(val);
		pDomain->m_lastError = sError.Code;
		pDomain->m_lastErrorString = sError.pName;
	}
	else if (strcmp(key, "Simias-Grace-Remaining") == 0)
	{
		// We are in grace save the count.
		sscanf(val, "%d", &pDomain->m_GraceRemaining);
	}
	else if (strcmp(key, "Simias-Grace-Total") == 0)
	{
		// Save the grace total count.
		sscanf(val, "%d", &pDomain->m_GraceTotal);
	}
	else
	{
		return pDomain->m_Parsehdr(soap, key, val);
	}
	return SOAP_OK;
}

gboolean IFDomain::Login(const gchar *password, GError **error)
{
	if (m_iFS->Login(m_UserName, password, error))
	{
		// Save the cookies.
		m_DS->m_DomainService.soap->cookies = soap_copy_cookies(m_iFS->m_iFolderService.soap);
		m_Authenticated = true;
		return true;
	}
	return false;
}

void IFDomain::Logout()
{
	this->m_Authenticated = false;
	// Clear the cookies.
	soap_free_cookies(m_DS->m_DomainService.soap);
	soap_free_cookies(m_iFS->m_iFolderService.soap);
}

void IFDomain::GetGraceLimits(gint *pRemaining, gint *pTotal)
{
	*pRemaining = m_GraceRemaining;
	*pTotal = m_GraceTotal;
}

gboolean IFDomain::Serialize(FILE *pStream)
{
	gchar *value;
	// <Domain>
	fprintf(pStream, "<%s %s=\"%s\" %s=\"%s\">\n", EDomain, EID, m_ID, EVersion, m_Version);
	// <name>
	value = g_markup_escape_text(m_Name, (gssize)strlen(m_Name));
	fprintf(pStream, "<%s>%s</%s>\n", EName, value, EName);
	g_free(value);
	// <description>
	value = g_markup_escape_text(m_Description, (gssize)strlen(m_Description));
	fprintf(pStream, "<%s>%s</%s>\n", EDescription, value, EDescription);
	g_free(value);
	// <host>
	value = g_markup_escape_text(m_MasterHost, (gssize)strlen(m_MasterHost));
	fprintf(pStream, "<%s>%s</%s>\n", EMasterHost, value, EMasterHost);
	g_free(value);
	// <active>
	fprintf(pStream, "<%s>%d</%s>\n", EActive, m_Active, EActive);
	// <default>
	fprintf(pStream, "<%s>%d</%s>\n", EDefault, m_Default, EDefault);
	// <User>
	fprintf(pStream, "<%s %s=\"%s\">\n", EUser, EID, m_UserID);
	// <name>
	value = g_markup_escape_text(m_UserName, (gssize)strlen(m_UserName));
	fprintf(pStream, "<%s>%s</%s>\n", EName, value, EName);
	g_free(value);
	// <pw>
	// todo encrypt password.
	value = g_markup_escape_text(m_UserPassword, (gssize)strlen(m_UserPassword));
	fprintf(pStream, "<%s>%s</%s>\n", EPW, value, EPW);
	g_free(value);
	// <pob>
	value = g_markup_escape_text(m_POBoxID, (gssize)strlen(m_POBoxID));
	fprintf(pStream, "<%s>%s</%s>\n", EPOB, value, EPOB);
	g_free(value);
	// </User>
	fprintf(pStream, "</%s>\n", EUser);
	// </Domain>
	fprintf(pStream, "</%s>\n", EDomain);
	return true;
}

IFDomain* IFDomain::DeSerialize(XmlTree *tree, GNode *pDNode)
{
	IFDomain *pDomain; // = new IFDomain();
	GNode *gnode = pDNode;
	GNode *tnode;

	// host Get the host so that we can construct the IFDomain object.
	tnode = tree->FindChild(gnode, EMasterHost, IFXElement);
	if (tnode != NULL)
	{
		gchar *host = ((XmlNode*)tnode->data)->m_Value;
		pDomain = new IFDomain(host);
	}
	// id
	tnode = tree->FindChild(gnode, EID, IFXAttribute);
	if (tnode != NULL)
		pDomain->m_ID = g_strdup(((XmlNode*)tnode->data)->m_Value);
	// version
	tnode = tree->FindChild(gnode, EVersion, IFXAttribute);
	if (tnode != NULL)
		pDomain->m_Version = g_strdup(((XmlNode*)tnode->data)->m_Value);
	// name
	tnode = tree->FindChild(gnode, EName, IFXElement);
	if (tnode != NULL)
		pDomain->m_Name = g_strdup(((XmlNode*)tnode->data)->m_Value);
	// description
	tnode = tree->FindChild(gnode, EDescription, IFXElement);
	if (tnode != NULL)
		pDomain->m_Description = g_strdup(((XmlNode*)tnode->data)->m_Value);
	
	// active
	tnode = tree->FindChild(gnode, EActive, IFXElement);
	if (tnode != NULL)
		sscanf(((XmlNode*)tnode->data)->m_Value, "%d", &pDomain->m_Active);
	// default
	tnode = tree->FindChild(gnode, EDefault, IFXElement);
	if (tnode != NULL)
		sscanf(((XmlNode*)tnode->data)->m_Value, "%d", &pDomain->m_Default);
	// User
	gnode = tree->FindChild(gnode, EUser, IFXElement);
	if (gnode != NULL)
	{
		// id
		tnode = tree->FindChild(gnode, EID, IFXAttribute);
		if (tnode != NULL)
			pDomain->m_UserID = g_strdup(((XmlNode*)tnode->data)->m_Value);
		// name
		tnode = tree->FindChild(gnode, EName, IFXElement);
		if (tnode != NULL)
			pDomain->m_UserName = g_strdup(((XmlNode*)tnode->data)->m_Value);
		// pw
		tnode = tree->FindChild(gnode, EPW, IFXElement);
		if (tnode != NULL)
			pDomain->m_UserPassword = g_strdup(((XmlNode*)tnode->data)->m_Value);
		// pob
		tnode = tree->FindChild(gnode, EPOB, IFXElement);
		if (tnode != NULL)
			pDomain->m_POBoxID = g_strdup(((XmlNode*)tnode->data)->m_Value);
	}
	return pDomain;
}

IFDomainIterator IFDomain::GetDomains()
{
	return IFDomainList::GetIterator();
}

IFDomain* IFDomain::GetDomainByID(const gchar *pID)
{
	return IFDomainList::GetDomainByID(pID);
}

IFDomain* IFDomain::GetDomainByName(const gchar *pName)
{
	return IFDomainList::GetDomainByName(pName);
}

IFDomain* IFDomain::GetDefault()
{
	return IFDomainList::GetDefault();
}

void IFDomain::SetDefault()
{
	IFDomain *pDefault = GetDefault();
	if (pDefault != NULL)
		pDefault->m_Default = false;
	m_Default = true;
	// Now persist this change.
	IFDomainList::Save();
}
void IFDomain::SetActive(gboolean state)
{
	m_Active = state;
	// Now persist this change.
	IFDomainList::Save();
}

// IFDomainList class
IFDomainList* IFDomainList::m_Instance = NULL;
gchar	*IFDomainList::EDomains = "Domains";
gfloat	IFDomainList::m_Version = 1.0;
	
IFDomainList::IFDomainList(void)
{
	m_List = g_array_new(true, true, sizeof(gpointer));
}

IFDomainList::~IFDomainList(void)
{
	// free the resources.
	IFDomainIterator dIter = GetIterator();
	IFDomain* pDomain;
	while ((pDomain = dIter.Next()) != NULL)
	{
		delete pDomain;
	}
	g_array_free(m_List, true);
}

IFDomainList* IFDomainList::Instance()
{
	if (m_Instance == NULL)
		Initialize();
	return m_Instance;
}

int IFDomainList::Initialize()
{
	m_Instance = new IFDomainList();
	m_Instance->m_pFileName = g_build_filename(IFApplication::DataPath(), "domains.xml", NULL);
	m_Instance->Restore();
	return 0;
}

void IFDomainList::Insert(IFDomain *pDomain)
{
	IFDomainList* list = Instance();
	g_array_append_val(list->m_List, pDomain);
	list->Save();
}

gint IFDomainList::Count()
{
	IFDomainList* list = Instance();
	return list->m_List->len;
}

gboolean IFDomainList::Remove(const gchar *id)
{
	IFDomainList* list = Instance();
	int i = 0;
	IFDomain* pDomain;
	while ((pDomain = g_array_index(list->m_List, IFDomain*, i)) != NULL)
	{
		if (strcmp(pDomain->m_ID, id) == 0)
			break;
		++i;
	}
	if (pDomain != NULL)
	{
		g_array_remove_index(list->m_List, i);
		delete pDomain;
		list->Save();
		return true;
	}
	return false;
}

void IFDomainList::Save()
{
	IFDomainList* list = Instance();
	FILE *pStream = g_fopen(list->m_pFileName, "w");
	if (pStream == NULL)
	{
		// This is an error.
		return;
	}
	// Write the header
	fprintf(pStream, "<%s version=\"%f\" count=\"%d\">\n", EDomains, m_Version, list->m_List->len);
	
	// Now we need to save each entry.
	IFDomainIterator dIter = list->GetIterator();
	IFDomain *pDomain;
	while ((pDomain = dIter.Next()) != NULL)
	{
		pDomain->Serialize(pStream);
	}
	fprintf(pStream, "</%s>\n", EDomains);
	fclose(pStream);
}

void IFDomainList::Restore()
{
	IFDomainList* list = Instance();
	const GMarkupParser parser = {XmlStart, XmlEnd, XmlText, NULL, XmlError};
	GMarkupParseContext *pContext = g_markup_parse_context_new(&parser, (GMarkupParseFlags)0, list, NULL);

	gchar *pFileData = NULL;
	gsize fileLength;
	GError *pError = NULL;
	if (!g_file_get_contents(list->m_pFileName, &pFileData, &fileLength, &pError))
	{
		g_debug(pError->message);
		g_clear_error(&pError);
	}
	if (pFileData != NULL)
	{
		list->m_XmlTree = new XmlTree();
		if (!g_markup_parse_context_parse(pContext, pFileData, fileLength, &pError))
		{
			g_debug(pError->message);
			g_clear_error(&pError);
		}
		if (!g_markup_parse_context_end_parse(pContext, &pError))
		{
			g_debug(pError->message);
			g_clear_error(&pError);
		}
		g_free(pFileData);
		GNode* dNode = list->m_XmlTree->FindChild(NULL, EDomain, IFXElement);
		while (dNode != NULL)
		{
			Insert(IFDomain::DeSerialize(list->m_XmlTree, dNode));
			dNode = list->m_XmlTree->FindSibling(dNode, EDomain, IFXElement);
		}
		delete list->m_XmlTree;
		list->m_XmlTree = NULL;
	}
	g_markup_parse_context_free(pContext);
}

void IFDomainList::XmlStart(GMarkupParseContext *pContext, const gchar *pName, const gchar **pANames, const gchar **pAValues, gpointer userData, GError **ppError)
{
	IFDomainList *dList = (IFDomainList*)userData;
	dList->m_XmlTree->StartNode(pName);
	int i = 0;
	while (pANames[i] != NULL)
	{
		dList->m_XmlTree->AddAttribute(pANames[i], pAValues[i]);
		i++;
	}
}

void IFDomainList::XmlEnd(GMarkupParseContext *pContext, const gchar *pName, gpointer userData, GError **ppError)
{
	IFDomainList *dList = (IFDomainList*)userData;
	dList->m_XmlTree->EndNode();
}

void IFDomainList::XmlText(GMarkupParseContext *pContext, const gchar *text, gsize textLen, gpointer userData, GError **ppError)
{
	IFDomainList *dList = (IFDomainList*)userData;
	dList->m_XmlTree->AddText(text, textLen);
}

void IFDomainList::XmlError(GMarkupParseContext *pContext, GError *pError, gpointer userData)
{
}


IFDomainIterator IFDomainList::GetIterator()
{
	IFDomainIterator iterator(Instance()->m_List);
	return iterator;
}

IFDomain* IFDomainList::GetDomainByID(const gchar *pID)
{
	IFDomainIterator dIter = GetIterator();
	IFDomain *pDomain;
	while ((pDomain = dIter.Next()) != NULL)
	{
		if (strcmp(pDomain->m_ID, pID) == 0)
			break;
	}
	return pDomain;
}

IFDomain* IFDomainList::GetDomainByName(const gchar *pName)
{
	IFDomainIterator dIter = GetIterator();
	IFDomain *pDomain;
	while ((pDomain = dIter.Next()) != NULL)
	{
		if (strcmp(pDomain->m_Name, pName) == 0)
			break;
	}
	return pDomain;
}

IFDomain* IFDomainList::GetDefault()
{
	IFDomainIterator dIter = GetIterator();
	IFDomain *pDomain;
	while ((pDomain = dIter.Next()) != NULL)
	{
		if (pDomain->m_Default != 0)
			break;
	}
	return pDomain;
}

