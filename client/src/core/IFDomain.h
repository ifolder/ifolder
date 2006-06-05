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
#ifndef _IFDOMAIN_H_
#define _IFDOMAIN_H_

//#include <vector>
#include "stdsoap2.h"
#include "glibclient.h"
#include "simiasDomain_USCOREx0020_USCOREServiceSoapProxy.h"
#include "simiasiFolderWebSoapProxy.h"

#include "Xml.h"

// forward declarations
class IFDomain;
class IFDomainIterator;
class IFDomainList;
class XmlNode;
class XmlTree;
class Domain;
namespace ifweb
{
	class WebService;
	class DomainService;
	class iFolderService;
	class IFServiceManager;
}

//class GLIBCLIENT_API std::vector<IFDomain*>;

class GLIBCLIENT_API IFDomain
{
	friend class ifweb::IFServiceManager;
	friend class ifweb::WebService;
	friend class IFDomainList;
private:
	// Members.
	gint	m_lastError;
	gchar	*m_lastErrorString;
	gint	m_GraceRemaining;
	gint	m_GraceTotal;
	int (*m_Parsehdr)(struct soap *soap, const char *key, const char *val);
public:
	ifweb::DomainService		*m_DS;
	ifweb::iFolderService		*m_iFS;

private:
	// Persist everything below here.
	gchar*		m_UserPassword;
	gchar*		m_POBoxID;
	
public:
	// Properties
	gchar*		m_Name;
	gchar*		m_ID;
	gchar*		m_Version;
	gchar*		m_Description;
	gchar*		m_HomeHost;
	gchar*		m_MasterHost;
	gchar*		m_UserName;
	gchar*		m_UserID;
	gboolean	m_Authenticated; // Not persisted.
	gboolean	m_Active;
	gboolean	m_Default;
	
private:
	IFDomain(const gchar *host);
	virtual ~IFDomain(void);
	gboolean Serialize(FILE *pStream);
	static IFDomain* DeSerialize(XmlTree *tree, GNode *pDNode);
	static int ParseLoginHeader(struct soap *soap, const char *key, const char*val);
	
public:
	static IFDomain* Add(const gchar* userName, const gchar* password, const gchar* host, GError **error);
	gboolean Remove();
	gboolean Login(const gchar *password, GError **error);
	void Logout();
	void GetGraceLimits(gint *pRemaining, gint *pTotal);
	static IFDomainIterator GetDomains();
	static IFDomain* GetDomainByID(const gchar *pID);
	static IFDomain* GetDomainByName(const gchar *pName);
	static IFDomain* GetDefault();
	void SetDefault();
	void SetActive(gboolean state);
};

class GLIBCLIENT_API IFDomainIterator
{
private:
	GArray				*m_List;
	guint				m_Index;
	
public:
	IFDomainIterator(GArray* list) {m_List = list; m_Index = 0; }
	virtual ~IFDomainIterator() {};
	void Reset() {m_Index = 0; }
	IFDomain* Next()
	{
		if (m_Index >= m_List->len)
			return NULL;
		IFDomain *pDomain = g_array_index(m_List, IFDomain*, m_Index);
		m_Index++;
		return pDomain;
	};
};

class IFDomainList
{
	friend class IFDomain;
private:
	// The list of domains should be small so I will
	// use an array to store the list.
	static			IFDomainList* m_Instance;
	gchar			*m_pFileName;
	GArray			*m_List;
	XmlTree			*m_XmlTree;
	static gchar	*EDomains;
	static gfloat	m_Version;
	
	IFDomainList(void);
	virtual ~IFDomainList(void);
	static IFDomainList* Instance();
	static void XmlStart(GMarkupParseContext *pContext, const gchar *pName, const gchar **pANames, const gchar **pAValues, gpointer userData, GError **ppError);
	static void XmlEnd(GMarkupParseContext *pContext, const gchar *pName, gpointer userData, GError **ppError);
	static void XmlText(GMarkupParseContext *pContext, const gchar *text, gsize textLen, gpointer userData, GError **ppError);
	static void XmlError(GMarkupParseContext *pContext, GError *pError, gpointer userData);
	static void Destroy(gpointer data);
	static void Insert(IFDomain *pDomain);
	static gint Count();
	static gboolean Remove(const gchar *id);
	static IFDomainIterator GetIterator();
	static IFDomain* GetDomainByID(const gchar *pID);
	static IFDomain* GetDomainByName(const gchar *pName);
	static IFDomain* GetDefault();
	static void Save();
	static void Restore();

public:
	static int Initialize();
};

#endif //_IFDOMAIN_H_
