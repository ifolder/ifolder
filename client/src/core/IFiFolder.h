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
#ifndef _IFIFOLDER_H_
#define _IFIFOLDER_H_

#include <glib.h>
#include "glibclient.h"
#include "IFDirectory.h"
#include "IFIterator.h"
#include "Xml.h"

// forward declarations
class IFiFolder;
class IFiFolderList;
class IFDomain;

class GLIBCLIENT_API IFiFolder
{
	friend class IFiFolderList;
private:
	IFDirectory m_RootDir;

	gchar *m_Version;
public:
	// Properties.
	gchar *m_Name;
	gchar *m_ID;
	gchar *m_Path;
	gchar *m_Description;
	gchar *m_Owner;

private:
	gboolean Serialize(FILE *pStream);
	static IFiFolder* DeSerialize(XmlTree *tree, GNode *pDNode);
	
public:
	IFiFolder(const gchar *pPath);
	virtual ~IFiFolder(void);
	static gboolean IsiFolder(gchar *pPath);
	gboolean Sync(GError **error);
	gboolean Stop(GError **error);
};

class IFiFolderList
{
	friend class IFiFolder;
	friend class IFDomain;
private:
	IFDomain		*m_pDomain;
	gchar			*m_pFileName;
	GArray			*m_List;
	static gfloat	m_Version;
	
	IFiFolderList(IFDomain *pDomain);
	virtual ~IFiFolderList(void);
	static void Destroy(gpointer data);
	void Insert(IFiFolder *piFolder);
	gint Count();
	gboolean Remove(const gchar *id);
	IFiFolderIterator GetIterator();
	IFiFolder* GetiFolderByID(const gchar *pID);
	IFiFolder* GetiFolderByName(const gchar *pName);
	void Save();
	void Restore();

public:
	static int Initialize();
};

#endif //_IFIFOLDER_H_
