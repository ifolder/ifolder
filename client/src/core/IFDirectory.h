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
#ifndef _IFDIRECTORY_H_
#define _IFDIRECTORY_H_

#include <glib.h>
#include "glibclient.h"
#include "IFFileInfo.h"

class GLIBCLIENT_API IFDirectory
{
	IFDirectory *m_piFolder;
	GError		*m_error;
	gchar		*m_pPath;
	gchar		*m_pDataPath;
	gboolean	m_DontDetect;
	gchar		*m_pOldFileListFile;
	gchar		*m_pOldDirListFile;
	gchar		*m_pChangesFile;
	GTimeVal	m_TimeStamp;
	IFFileInfoList	*m_pDirList;
	IFFileInfoList	*m_pFileList;

private:
	IFDirectory(const gchar *pPath, gchar *pDataPath, IFDirectory *piFolder);
	gboolean Initialize(const gchar *pPath, gchar *pDataPath);
	void DetectFileChanges(IFFileInfoList *pOldList, IFFileInfoList *pNewList);
	void DetectDirChanges(IFFileInfoList *pOldList, IFFileInfoList *pNewList);
	void Remove(gchar *path);
	gboolean IsRecursiveLink();
		
public:
	IFDirectory(const gchar *pPath);
	virtual ~IFDirectory(void);

	void DetectChanges();
};

#endif //_IFDIRECTORY_H_
