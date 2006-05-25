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

// IFDomain class

gchar *IFDomain::EDomain = "Domain";
gchar *IFDomain::EName = "name";
gchar *IFDomain::EDescription = "description";
gchar *IFDomain::EUser = "User";
gchar *IFDomain::EHostID = "Host";
gchar *IFDomain::EMasterHost = "master-host";
gchar *IFDomain::EID = "id";
gchar *IFDomain::EPW = "pw";
gchar *IFDomain::EPOB = "pob";

IFDomain::IFDomain()
{
	m_DomainService.soap->mode |= SOAP_C_UTFSTRING | SOAP_IO_KEEPALIVE;
	m_DomainService.soap->imode |= SOAP_C_UTFSTRING | SOAP_IO_KEEPALIVE;
	m_DomainService.soap->omode |= SOAP_C_UTFSTRING | SOAP_IO_KEEPALIVE;
	m_DomainService.soap->user = this;
	
	m_iFolderService.soap->mode |= SOAP_C_UTFSTRING | SOAP_IO_KEEPALIVE;
	m_iFolderService.soap->imode |= SOAP_C_UTFSTRING | SOAP_IO_KEEPALIVE;
	m_iFolderService.soap->omode |= SOAP_C_UTFSTRING | SOAP_IO_KEEPALIVE;
	m_iFolderService.soap->user = this;
	
	// Set the parse_header callback so that we can get the login status.
	m_Parsehdr = m_iFolderService.soap->fparsehdr;
	m_DomainService.soap->fparsehdr = ParseLoginHeader;
	m_iFolderService.soap->fparsehdr = ParseLoginHeader;
	
	m_Name = NULL;
	m_ID = NULL;
	m_Description = NULL;
	m_HomeHost = NULL;
	m_MasterHost = NULL;
	m_POBoxID = NULL;
	m_UserName = NULL;
	m_UserPassword = NULL;
	m_UserID = NULL;
}
	
IFDomain::~IFDomain(void)
{
	g_free(m_Name);
	g_free(m_ID);
	g_free(m_Description);
	g_free(m_HomeHost);
	g_free(m_MasterHost);
	g_free(m_POBoxID);
	g_free(m_UserName);
	g_free(m_UserPassword);
	g_free(m_UserID);
	g_free((gpointer)m_DomainService.endpoint);
	g_free((gpointer)m_iFolderService.endpoint);
}

IFDomain* IFDomain::Add(const gchar* userName, const gchar* password, const gchar* host, GError **error)
{
	gboolean failed = true;
	IFDomain *pDomain = new IFDomain();
	Domain* domainService = &pDomain->m_DomainService;
	iFolderWebSoap* iFolderService = &pDomain->m_iFolderService;

	// build the url to the host;
	domainService->endpoint = IFApplication::BuildUrlToService(host, domainService->endpoint);
	iFolderService->endpoint = IFApplication::BuildUrlToService(host, iFolderService->endpoint);
	pDomain->m_MasterHost = g_strdup(host);
	pDomain->m_HomeHost = g_strdup(host);
	
	// Make sure that we do not already belong to this domain.
	_ds__GetDomainID req;
	_ds__GetDomainIDResponse resp;
	if (domainService->__ds__GetDomainID(&req, &resp) == 0)
	{
		pDomain->m_ID = g_strdup(resp.GetDomainIDResult);
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
		domainService->soap->userid = (char*)userName;
		domainService->soap->passwd = (char*)password;
		_ds__ProvisionUser puReq;
		puReq.user = (char*)userName;
		puReq.password = (char*)password;
		_ds__ProvisionUserResponse puResp;
		ds__ProvisionInfo *pvInfo;
		if (domainService->__ds__ProvisionUser(&puReq, &puResp) == 0)
		{
			pvInfo = puResp.ProvisionUserResult;
			pDomain->m_UserID = g_strdup(pvInfo->UserID);
			pDomain->m_UserName = g_strdup(userName);
			pDomain->m_UserPassword = g_strdup(password);
			pDomain->m_POBoxID = g_strdup(pvInfo->POBoxID);
			printf("provisioned\n");

			// Now get the domain Info.
			_ds__GetDomainInfo diReq;
			_ds__GetDomainInfoResponse diResp;
			diReq.userID = pvInfo->UserID;
			if (domainService->__ds__GetDomainInfo(&diReq, &diResp) == 0)
			{
				ds__DomainInfo *dInfo = diResp.GetDomainInfoResult;
				pDomain->m_ID = g_strdup(dInfo->ID);
				pDomain->m_Description = g_strdup(dInfo->Description);
				pDomain->m_Name = g_strdup(dInfo->Name);

				// Add the domain to the list and return and set success.
				IFDomainList::Insert(pDomain);
				failed = false;
			}
		}
	}
	if (failed)
	{
		soap_print_fault(domainService->soap, stderr);
		soap_print_fault_location(domainService->soap, stderr);
		g_set_error(error, IF_CORE_ERROR_DOMAIN_QUARK, domainService->soap->error, "Soap Error");
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
}

gboolean IFDomain::Login(const gchar *password, GError **error)
{
	gboolean bStatus = false;
	_ifolder__GetSystem req;
	_ifolder__GetSystemResponse resp;
	m_iFolderService.soap->userid = m_UserName;
	m_iFolderService.soap->passwd = (char*)password;
	if (m_iFolderService.__ifolder__GetSystem(&req, &resp) == 0)
	{
		// We are logged in. Make sure we are not in grace.
		bStatus = true;
	}
	else
	{
		// We failed to log in. Get the reason for the failure.
		g_set_error(error, IF_CORE_ERROR_DOMAIN_QUARK, m_lastError, m_lastErrorString);
	}
	return bStatus;
}

int IFDomain::Logout()
{
	return 0;
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
	fprintf(pStream, "<%s %s=\"%s\">\n", EDomain, EID, m_ID);
	// <name>
	value = g_markup_escape_text(m_Name, (gssize)strlen(m_Name));
	fprintf(pStream, "<%s>%s</%s>\n", EName, value, EName);
	g_free(value);
	// <description>
	value = g_markup_escape_text(m_Description, (gssize)strlen(m_Description));
	fprintf(pStream, "<%s>%s</%s>\n", EDescription, value, EDescription);
	g_free(value);
	// <url>
	value = g_markup_escape_text(m_MasterHost, (gssize)strlen(m_MasterHost));
	fprintf(pStream, "<%s>%s</%s>\n", EMasterHost, value, EMasterHost);
	g_free(value);
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

IFDomain* IFDomain::DeSerialize(ParseTree *tree, GNode *pDNode)
{
	IFDomain *pDomain = new IFDomain();
	GNode *gnode = pDNode;
	GNode *tnode;

	// id
	tnode = tree->FindChild(gnode, EID, IFXAttribute);
	if (tnode != NULL)
		pDomain->m_ID = g_strdup(((XmlNode*)tnode->data)->m_Value);
	// name
	tnode = tree->FindChild(gnode, EName, IFXElement);
	if (tnode != NULL)
		pDomain->m_Name = g_strdup(((XmlNode*)tnode->data)->m_Value);
	// description
	tnode = tree->FindChild(gnode, EDescription, IFXElement);
	if (tnode != NULL)
		pDomain->m_Description = g_strdup(((XmlNode*)tnode->data)->m_Value);
	// url
	tnode = tree->FindChild(gnode, EMasterHost, IFXElement);
	if (tnode != NULL)
	{
		pDomain->m_MasterHost = g_strdup(((XmlNode*)tnode->data)->m_Value);
		pDomain->m_DomainService.endpoint = IFApplication::BuildUrlToService(pDomain->m_MasterHost, pDomain->m_DomainService.endpoint);
		pDomain->m_iFolderService.endpoint = IFApplication::BuildUrlToService(pDomain->m_MasterHost, pDomain->m_iFolderService.endpoint);
	
	}
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
	FILE *pStream = g_fopen(m_pFileName, "w");
	if (pStream == NULL)
	{
		// This is an error.
		return;
	}
	// Write the header
	fprintf(pStream, "<%s version=\"%f\" count=\"%d\">\n", EDomains, m_Version, m_List->len);
	
	// Now we need to save each entry.
	IFDomainIterator dIter = GetIterator();
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
	const GMarkupParser parser = {XmlStart, XmlEnd, XmlText, NULL, XmlError};
	GMarkupParseContext *pContext = g_markup_parse_context_new(&parser, (GMarkupParseFlags)0, this, NULL);

	gchar *pFileData = NULL;
	gsize fileLength;
	GError *pError = NULL;
	if (!g_file_get_contents(m_pFileName, &pFileData, &fileLength, &pError))
	{
		g_debug(pError->message);
		g_clear_error(&pError);
	}
	if (pFileData != NULL)
	{
		m_ParseTree = new ParseTree();
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
		GNode* dNode = m_ParseTree->FindChild(NULL, IFDomain::EDomain, IFXElement);
		while (dNode != NULL)
		{
			Insert(IFDomain::DeSerialize(m_ParseTree, dNode));
			dNode = m_ParseTree->FindSibling(dNode, IFDomain::EDomain, IFXElement);
		}
		delete m_ParseTree;
		m_ParseTree = NULL;
	}
	g_markup_parse_context_free(pContext);
}

void IFDomainList::XmlStart(GMarkupParseContext *pContext, const gchar *pName, const gchar **pANames, const gchar **pAValues, gpointer userData, GError **ppError)
{
	IFDomainList *dList = (IFDomainList*)userData;
	dList->m_ParseTree->StartNode(pName);
	int i = 0;
	while (pANames[i] != NULL)
	{
		dList->m_ParseTree->AddAttribute(pANames[i], pAValues[i]);
		i++;
	}
}

void IFDomainList::XmlEnd(GMarkupParseContext *pContext, const gchar *pName, gpointer userData, GError **ppError)
{
	IFDomainList *dList = (IFDomainList*)userData;
	dList->m_ParseTree->EndNode();
}

void IFDomainList::XmlText(GMarkupParseContext *pContext, const gchar *text, gsize textLen, gpointer userData, GError **ppError)
{
	IFDomainList *dList = (IFDomainList*)userData;
	dList->m_ParseTree->AddText(text, textLen);
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

// DomainParseContext Class

ParseTree::ParseTree()
{
	m_CurrentNode = m_RootNode = NULL;
}

ParseTree::~ParseTree()
{
	// Free the XmlNodes.
	g_node_traverse(m_RootNode, G_IN_ORDER, G_TRAVERSE_ALL, -1, FreeXmlNodes, NULL);

	g_node_destroy(m_RootNode);
}

gboolean ParseTree::FreeXmlNodes(GNode *pNode, gpointer data)
{
	XmlNode *xNode = (XmlNode*)pNode->data;
	delete xNode;
	return false;
}

void ParseTree::StartNode(const gchar *name)
{
	GNode *newNode = g_node_new(new XmlNode(g_strdup(name), IFXElement));
	if (m_CurrentNode)
	{
		g_node_append(m_CurrentNode, newNode);
		m_CurrentNode = newNode;
	}
	else
	{
		m_CurrentNode = m_RootNode = newNode;
	}
}

void ParseTree::EndNode()
{
	if (m_CurrentNode)
		m_CurrentNode = m_CurrentNode->parent;
}

void ParseTree::AddText(const gchar *text, gsize len)
{
	if (m_CurrentNode)
	{
		XmlNode *pNode = (XmlNode*)m_CurrentNode->data;
		gchar *pValue = g_strstrip(g_strndup(text, len));
		pNode->m_Value = pValue;
	}
}

void ParseTree::AddAttribute(const gchar *name, const gchar *value)
{
	if (m_CurrentNode)
	{
		GNode *newNode = g_node_new(new XmlNode(g_strdup(name), g_strdup(value), IFXAttribute));
		g_node_append(m_CurrentNode, newNode);
	}
}

GNode* ParseTree::FindChild(GNode *parent, gchar* name, IFXNodeType type)
{
	if (parent == NULL)
		parent = m_RootNode;

    GNode *gnode = g_node_first_child(parent);
	while (gnode != NULL)
	{
		XmlNode* xNode = (XmlNode*)gnode->data;
		if (xNode->m_Type == type)
		{
			if (strcmp(xNode->m_Name, name) == 0)
			{
				break;
			}
		}
		gnode = g_node_next_sibling(gnode);
	}
	return gnode;
}

GNode* ParseTree::FindSibling(GNode *sibling, gchar* name, IFXNodeType type)
{
	if (sibling == NULL)
		return NULL;

    GNode *gnode = g_node_next_sibling(sibling);
	while (gnode != NULL)
	{
		XmlNode* xNode = (XmlNode*)gnode->data;
		if (xNode->m_Type == type)
		{
			if (strcmp(xNode->m_Name, name) == 0)
			{
				break;
			}
		}
		gnode = g_node_next_sibling(gnode);
	}
	return gnode;
}
