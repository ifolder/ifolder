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
#include <iostream>
#include <fstream>

#include <QtAlgorithms>

#include "Directory.h"
#include "Application.h"

using namespace std;

namespace iFolderCore
{
	
Directory::Directory(const QString& path) :
	QDir(path)
{
	Initialize(path, Application::DataPath());
}

Directory::Directory(const QString& path, const QString& metadataPath) :
	QDir(path)
{
	Initialize(path, metadataPath);
}

void Directory::Initialize(const QString& path, const QString& metadataPath)
{
	setPath(absolutePath());
	QString metaPath = QDir::cleanPath(metadataPath + "/" + dirName());
	cout << metaPath.toStdString() << " ===== " << absolutePath().toStdString() << endl;
	// If this is a recursive link or our Applicateion area don't recurse.
	cout << "Link Name = " << canonicalPath().toStdString() << QDir::separator().toAscii() << endl;
	if (absolutePath().startsWith(Application::DataPath()) || path.startsWith(canonicalPath() + "/"))
		m_DontDetect = true;
	else 
	{
		m_DontDetect = false;
		m_MetadataDir.setPath(metaPath);
		if (!m_MetadataDir.exists())
			m_MetadataDir.mkpath(metaPath);
//		cout << metaPath.toStdString() << endl;
		m_OldFileListFile.setFileName(m_MetadataDir.absoluteFilePath("files"));
//		cout << m_OldFileListFile.fileName().toStdString() << endl;
		m_OldDirListFile.setFileName(m_MetadataDir.absoluteFilePath("dirs"));
//		cout << m_OldDirListFile.fileName().toStdString() << endl;
		m_ChangesFile.setFileName(m_MetadataDir.absoluteFilePath("changes"));
//		cout << m_ChangesFile.fileName().toStdString() << endl;
	}
}

Directory::~Directory()
{
}

int Directory::DetectChanges()
{
	if (m_DontDetect)
		return 0;
	// Get the current list of files
	SyncFileInfoList fileList = entryInfoList(QDir::Files | QDir::Hidden, QDir::Name);
	// Get the current list of directories
	SyncFileInfoList dirList = entryInfoList(QDir::Dirs | QDir::Hidden,  QDir::Name);

	// Get the old list of files.
	SyncFileInfoList oldFiles;
	oldFiles.Restore(m_OldFileListFile);
	// Get the old list of Dirs
	SyncFileInfoList oldDirs;
	oldDirs.Restore(m_OldDirListFile);

	GetEntryChanges(oldFiles, fileList);
	GetEntryChanges(oldDirs, dirList);
	
	fileList.Save(m_OldFileListFile);
	dirList.Save(m_OldDirListFile);
	m_FileChangeList.Save(m_ChangesFile);
	
	// Now recurse into the directories.
	int i = 0;
	int dirCount = dirList.size();
	cout << "DirCount = " << dirCount << endl;
	while(i < dirCount)
	{
		SyncFileInfo info = dirList.at(i++);
		
		if (info.Name() == "." || info.Name() == "..")
			continue;
		QString path = absoluteFilePath(info.Name());
		cout << "Path = " << path.toStdString() << endl;
		QString metaPath = m_MetadataDir.absolutePath();
		cout << "MetaPath = " << metaPath.toStdString() << endl;
		Directory child(path, metaPath);
		child.DetectChanges();
	}
}

int Directory::GetEntryChanges(SyncFileInfoList& oldList, SyncFileInfoList& newList)
{
	// We need to look for new, deleted and modifed files.
	int i = 0, j = 0;
	int listSize = newList.size(), oldSize = oldList.size();
	while(i < listSize && j < oldSize)
	{
		SyncFileInfo info = newList.at(i);
		SyncFileInfo oldInfo = oldList.at(j);
//		cout << info.Name().toStdString() << " == " << oldInfo.Name().toStdString() << endl;
		int cmpResults = info.Compare(oldInfo);
		if (cmpResults == 0)
		{
			// The names match.
			// Check if the file has been modified.
			if (info.Changed(oldInfo))
			{
				m_FileChangeList.append(info);
			}
			++i;
			++j;
		}
		else if (cmpResults < 0)
		{
			// The file in the file System is new.
			m_FileChangeList.append(info);
			++i;
		}
		else
		{
			// The file in the old list has been deleted.
			m_FileChangeList.append(oldInfo);
			++j;
		}
	}
	while (i < listSize)
	{
		// All of the files remaining in the newList are new.
		m_FileChangeList.append(newList.at(i++));
	}
	while (j < oldSize)
	{
		// All of the files remaining in the oldList have been deleted.
		m_FileChangeList.append(oldList.at(j++));
	}
	return 0;
}

SyncFileInfoList Directory::GetChanges()
{
	return m_FileChangeList;
	/*
	SyncFileInfoList changes;
	// Recurse
	// Get the current list of directories
	SyncFileInfoList dirList = entryInfoList(QDir::Dirs | QDir::Hidden,  QDir::Name);

	// Get the old list of Dirs
	SyncFileInfoList oldDirs;
	oldDirs.Restore(m_OldDirListFile);

	GetEntryChanges(oldFiles, fileList);
	GetEntryChanges(oldDirs, dirList);
	
	fileList.Save(m_OldFileListFile);
	dirList.Save(m_OldDirListFile);
	m_FileChangeList.Save(m_ChangesFile);
	
	// Now recurse into the directories.
	int i = 0;
	int dirCount = dirList.size();
	cout << "DirCount = " << dirCount << endl;
	while(i < dirCount)
	{
		SyncFileInfo info = dirList.at(i++);
		
		if (info.Name() == "." || info.Name() == "..")
			continue;
		QString path = absoluteFilePath(info.Name());
		cout << "Path = " << path.toStdString() << endl;
		QString metaPath = m_MetadataDir.absolutePath();
		cout << "MetaPath = " << metaPath.toStdString() << endl;
		Directory child(path, metaPath);
		child.DetectChanges();
	}
	*/
}

int Directory::SetInSync()
{
	return m_ChangesFile.remove();
}
	
}
