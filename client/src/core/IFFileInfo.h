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
#ifndef _IFFILEINFO_H_
#define _IFFILEINFO_H_

#include <glib.h>
#include "glibclient.h"

class GLIBCLIENT_API IFFileInfo
{
	friend class IFFileInfoList;
private:
	static gchar *m_pXmlTag;
public:
	gchar		*m_pShortName;
	gchar		*m_pName;
	GTime		m_Modified;
	gint64		m_Size;
	gboolean	m_IsDir;
	gchar		*m_pId;
	gint64		m_Version;
	
public:
	IFFileInfo(const gchar *pDir, const gchar *pName);
	IFFileInfo(const gchar *pName, const gchar *pID, GTime modified, gint64 size, gboolean isDir);
	virtual ~IFFileInfo(void);
	static gint Compare(IFFileInfo *pInfo1, IFFileInfo *pInfo2);
	static void Destroy(gpointer pInfo);
	gboolean Serialize(FILE *pStream);
	static IFFileInfo* DeSerialize(const gchar** pANames, const gchar **pAValues);
	gboolean Changed(IFFileInfo *pOther);
};

class GLIBCLIENT_API IFFileInfoIterator
{
private:
	GList *pItem;

	
public:
	IFFileInfoIterator(GList *pLink);
	void Reset() {pItem = g_list_first(pItem); }
	IFFileInfo* Next()
	{
		if (pItem == NULL)
			return NULL;
		IFFileInfo *pInfo = (IFFileInfo*)pItem->data;
		pItem = g_list_next(pItem);
		return pInfo;
	};
};

class GLIBCLIENT_API IFFileInfoList
{
private:
	GTimeVal		m_TimeStamp;
	static gchar	*m_pSignature;
	static gfloat	m_Version;
	GList			*m_pList;
	
	static void StartElement(GMarkupParseContext *pContext, const gchar *pName, const gchar **pANames, const gchar **pAValues, gpointer userData, GError **ppError);
	static gint CompareName(gconstpointer a, gconstpointer b);
	
public:
	IFFileInfoList(void);
	virtual ~IFFileInfoList(void);

	void Insert(IFFileInfo *pInfo);
	void Delete(IFFileInfo *pInfo);
	//void Delete(gchar *pName){g_tree_remove(m_pTree, pName);}
	void Save(gchar *pFile, GTimeVal timeStamp);
	void Restore(gchar *pFile);
	IFFileInfoIterator GetIterator();
};


#endif // _IFFILEINFO_H_
