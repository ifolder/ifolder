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
*  Library General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with this program; if not, write to the Free Software
*  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*
*  Author(s): Russ Young <ryoung@novell.com>
*
***********************************************************************/
#ifndef DIRECTORY_H_
#define DIRECTORY_H_

#include <QString>
#include <QDir>
#include <QFileInfoList>

#include "SyncFileInfo.h"

namespace iFolderCore
{

///
/// Class to detect changes in directory.
///
/// This class is used to detect what has changed in the local filesystem
/// since the last time this was run.
///
class Directory : public QDir
{
private:
	Directory(const QString& path, const QString& metaDataPath);
	void Initialize(const QString& path, const QString& metadataPath);
	
public:
	Directory::Directory(const QString& path);
	virtual ~Directory();
	
	int DetectChanges();
	Sync::SyncFileInfoList GetChanges();
	int SetInSync();
	
private:
	int GetEntryChanges(Sync::SyncFileInfoList& oldList, Sync::SyncFileInfoList& newList);
		
private:
	
	bool					m_DontDetect;
	Sync::SyncFileInfoList	m_FileChangeList;
	Sync::SyncFileInfoList	m_DirChangeList;
	Sync::SyncFileInfoList	m_NewFileList;
	QFile					m_OldFileListFile;
	QFile					m_OldDirListFile;
	QFile					m_ChangesFile;
	QDir					m_MetadataDir;
};

typedef QList<Directory> DirectoryListInfo;
}
#endif /*DIRECTORY_H_*/
