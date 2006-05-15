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
#ifndef _IFCHANGES_H_
#define _IFCHANGES_H_

#include <glib.h>
#include <vector>
#include "glibclient.h"

enum IFChangeOperation
{
	Created = 1,
	Deleted = 2,
	Modified = 3,
	Renamed = 4
};

class GLIBCLIENT_API IFChange
{
	friend class IFChangeList;
private:
	static gchar		*m_pXmlTag;
public:
	IFChangeOperation	m_Operation;
	gchar				*m_pFile;
	gchar				*m_pMetadataFile;

public:
	IFChange(IFChangeOperation	operation, const gchar *pFile, const gchar *pMetadataFile);
	virtual ~IFChange(void);
	gboolean Serialize(FILE *pStream);
	static IFChange* DeSerialize(const gchar** pANames, const gchar **pAValues);
};

class GLIBCLIENT_API IFChangeList
{
private:
	static gchar	*m_ChangeTag;
	static gchar	*m_RootStartTag;
	static gchar	*m_RootEndTag;
	FILE			*m_pWStream;
	FILE			*m_pRStream;
	int				m_Count;

private:
	void WriteHeader();
	void WriteFooter();
	static void StartElement(GMarkupParseContext *pContext, const gchar *pName, const gchar **pANames, const gchar **pAValues, gpointer userData, GError **ppError);

public:
	IFChangeList(gchar *pDataPath);
	virtual ~IFChangeList();
	void Save(gchar *pFile);
	void Restore(gchar *pFile);
};

#endif //_IFCHANGES_H_