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
#include "IFDirectory.h"
#include "IFApplication.h"
#include "IFFileInfo.h"

// Private Methods
gboolean IFDirectory::Initialize(gchar *pPath, gchar *pDataPath)
{
	m_error = NULL;
	if (!g_path_is_absolute(pPath))
	{
		gchar *pCwd = g_get_current_dir();
		m_pPath = g_build_filename(pCwd, pPath, NULL);
		g_free(pCwd);
	}
	else
	{
		m_pPath = g_strdup(pPath);
	}
	gchar *pName = g_path_get_basename(m_pPath);
	m_pDataPath = g_build_filename(pDataPath, pName, NULL);
	g_free(pName);

	// Make sure the data path exists.
	g_mkdir_with_parents(m_pDataPath, 0);

	// Make sure this is not a recursive link or that we are not in our dataPath.
	if (g_str_has_prefix(IFApplication::DataPath(), m_pPath) || IsRecursiveLink())
		m_DontDetect = true;
	else
	{
		m_DontDetect = false;
		m_pOldFileListFile = g_build_filename(m_pDataPath, ".ifFiles", NULL);
		m_pOldDirListFile = g_build_filename(m_pDataPath, ".ifDirs", NULL);
		m_pChangesFile = g_build_filename(m_pDataPath, ".ifChanges", NULL);

		m_pDirList = new IFFileInfoList();
		m_pFileList = new IFFileInfoList();
	}
	
	return true;
}
void IFDirectory::DetectFileChanges(IFFileInfoList *pOldList, IFFileInfoList *pNewList)
{
	// We need to look for new, deleted or modified files.
	IFFileInfoIterator oldIterator = pOldList->GetIterator();
	IFFileInfoIterator newIterator = pNewList->GetIterator();
	IFFileInfo *pNewInfo = newIterator.Next();
	IFFileInfo *pOldInfo = oldIterator.Next();
	while (pNewInfo != NULL && pOldInfo != NULL)
	{
		int cmpResults = IFFileInfo::Compare(pNewInfo, pOldInfo);
		if (cmpResults == 0)
		{
			// The names match.
			// Check if the file has been modified.
			if (pNewInfo->Changed(pOldInfo))
			{
//				changes.Add(newInfo);
				g_debug("File changed: %s", pNewInfo->m_pName);
			}
			pNewInfo = newIterator.Next();
			pOldInfo = oldIterator.Next();
		}
		else if (cmpResults < 0)
		{
			// This is a new file.
//			changes.Add(pNewInfo);
			g_debug("New File: %s", pNewInfo->m_pName);
			pNewInfo = newIterator.Next();
		}
		else
		{
			// The file has been deleted.
//			changes.Add(pOldInfo);
			g_debug("Deleted File: %s", pOldInfo->m_pName);
			pOldInfo = oldIterator.Next();
		}
	}

	while (pNewInfo != NULL)
	{
		// All the files remaining in the new list are new.
//		changes.Add(pNewInfo);
		g_debug("New File: %s", pNewInfo->m_pName);
		pNewInfo = newIterator.Next();
	}

	while (pOldInfo != NULL)
	{
		// All the files remaining in the old list have been deleted.
//		changes.Add(pOldInfo);
		g_debug("Deleted File: %s", pOldInfo->m_pName);
		pOldInfo = oldIterator.Next();
	}
}

void IFDirectory::DetectDirChanges(IFFileInfoList *pOldList, IFFileInfoList *pNewList)
{
	// We need to look for new or deleted directories.
	IFFileInfoIterator oldIterator = pOldList->GetIterator();
	IFFileInfoIterator newIterator = pNewList->GetIterator();
	IFFileInfo *pNewInfo = newIterator.Next();
	IFFileInfo *pOldInfo = oldIterator.Next();
	while (pNewInfo != NULL && pOldInfo != NULL)
	{
		int cmpResults = IFFileInfo::Compare(pNewInfo, pOldInfo);
		if (cmpResults == 0)
		{
			// The names match just continue.
			pNewInfo = newIterator.Next();
			pOldInfo = oldIterator.Next();
		}
		else if (cmpResults < 0)
		{
			// This is a new directory.
//			changes.Add(newInfo);
			g_debug("New Dir: %s", pNewInfo->m_pName);
			pNewInfo = newIterator.Next();
		}
		else
		{
			// The directory has been deleted.
//			changes.Add(oldInfo);
			g_debug("Deleted Dir: %s", pOldInfo->m_pName);
			// Delete the metaData area for this directory.
			gchar *pDataPath = g_build_filename(m_pDataPath, pOldInfo->m_pShortName, NULL);
			Remove(pDataPath);
			g_free(pDataPath);
			pOldInfo = oldIterator.Next();
		}
	}

	while (pNewInfo != NULL)
	{
		// All the directories remaining in the new list are new.
//		changes.Add(fi);
		g_debug("New Dir: %s", pNewInfo->m_pName);
		pNewInfo = newIterator.Next();
	}

	while (pOldInfo != NULL)
	{
		// All the directories remaining in the old list have been deleted.
//		changes.Add(fi);
		g_debug("Deleted Dir: %s", pOldInfo->m_pName);
		// Delete the metaData area for this directory.
		gchar *pDataPath = g_build_filename(m_pDataPath, pOldInfo->m_pShortName, NULL);
		Remove(pDataPath);
		g_free(pDataPath);
		pOldInfo = oldIterator.Next();
	}
}

void IFDirectory::Remove(gchar *path)
{
	// Delete all children and then delete the path.
	GError *error = NULL;
	GDir *pDir = g_dir_open(path, 0, &error);
	if (pDir == NULL)
	{
		g_debug("Could not access directory: %s -- error = %s\n", path, error->message); 
		g_clear_error(&error);
		return;
	}

	const gchar* pEntry;
	while ((pEntry = g_dir_read_name(pDir)) != NULL)
	{
		gchar *pFullName = g_build_filename(path, pEntry, NULL);
		if (pFullName != NULL)
		{
			IFFileInfo *pFi = new IFFileInfo(path, pEntry);
			if (pFi->m_IsDir)
			{
				// Recurse into directory.
				Remove(pFullName);
			}
			else
			{
				// Delete the file.
				g_remove(pFullName);
			}
			delete pFi;
			g_free(pFullName);
		}
	}

	g_dir_close(pDir);
	
	// Now delete the directory.
	g_rmdir(path);
}

gboolean IFDirectory::IsRecursiveLink()
{
	// assume it's not a recursive link.
	gboolean isRecursive = false;
			
	if (g_file_test(m_pPath, G_FILE_TEST_IS_SYMLINK))
	{
		gchar* linkPath = g_file_read_link(m_pPath, &m_error);
		if (linkPath = NULL)
		{
			// We either could not get the information about the directory or it is not a recursive link
			g_debug("Could not read link: %s -- error = %s", m_pPath, m_error->message);
			g_clear_error(&m_error);
		}
		else
		{
			// Check if the link target is in our path.
			if (g_str_has_prefix(linkPath, m_pPath))
				isRecursive = true;
			g_free(linkPath);
		}
	}
	return isRecursive;
}

IFDirectory::IFDirectory(gchar *pPath, gchar *pDataPath, IFDirectory *piFolder)
{
	Initialize(pPath, pDataPath);
	m_piFolder = piFolder;
}
		
//
// Public Methods
//
IFDirectory::IFDirectory(gchar *pPath)
{
	Initialize(pPath, IFApplication::DataPath());
	m_piFolder = this;
}

IFDirectory::~IFDirectory(void)
{
	if (m_error != NULL)
		g_clear_error(&m_error);
	g_free(m_pPath);
	g_free(m_pDataPath);
	g_free(m_pOldFileListFile);
	g_free(m_pOldDirListFile);
	g_free(m_pChangesFile);
	if (m_pDirList != NULL)
		delete m_pDirList;
	if (m_pFileList != NULL)
		delete m_pFileList;
}

void IFDirectory::DetectChanges()
{
	if (m_DontDetect)
		return;

	g_get_current_time(&m_TimeStamp);

	// Get the current list of files
	GDir *pDir = g_dir_open(m_pPath, 0, &m_error);
	if (pDir == NULL)
	{
		g_debug("Could not access directory: %s -- error = %s\n", m_pPath, m_error->message); 
		g_clear_error(&m_error);
		return;
	}

	const gchar* pEntry;
	while ((pEntry = g_dir_read_name(pDir)) != NULL)
	{
		IFFileInfo *pFi = new IFFileInfo(m_pPath, pEntry);
		if (pFi->m_IsDir)
		{
			// Add to the directory list.
			m_pDirList->Insert(pFi);
			// Recurse into the directory.
			gchar *pFullName = g_build_filename(m_pPath, pEntry, NULL);
			if (pFullName != NULL)
			{
				IFDirectory *pChild = new IFDirectory(pFullName, m_pDataPath, m_piFolder);
				pChild->DetectChanges();
				delete pChild;
				g_free(pFullName);
			}
		}
		else
		{
			// Add to the file list.
			m_pFileList->Insert(pFi);
		}
	}

	g_dir_close(pDir);
		
	// Get the old list of files.
	IFFileInfoList *pOldFiles = new IFFileInfoList();
	pOldFiles->Restore(m_pOldFileListFile);
	// Get the old list of Dirs.
	IFFileInfoList *pOldDirs = new IFFileInfoList();
	pOldDirs->Restore(m_pOldDirListFile);

	// Check for changes.
	//SyncFileInfoList changes = new SyncFileInfoList();
	DetectFileChanges(pOldFiles, m_pFileList);
	DetectDirChanges(pOldDirs, m_pDirList);

	m_pFileList->Save(m_pOldFileListFile, m_TimeStamp);
	m_pDirList->Save(m_pOldDirListFile, m_TimeStamp);
	//changes.Save(".changes", timeStamp);

	delete pOldFiles;
	delete pOldDirs;
}
