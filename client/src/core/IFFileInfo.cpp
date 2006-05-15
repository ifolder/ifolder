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
#include <errno.h>
#include "IFFileInfo.h"

//
// IFFileInfo class
//
gchar *IFFileInfo::m_pXmlTag = "file";

IFFileInfo::IFFileInfo(const gchar *pDir, const gchar *pName)
{
	m_pShortName = g_strdup(pName);
	m_pName = g_build_filename(pDir, pName, NULL);
	m_pId = g_strdup("");
	struct stat fstats;
	if (g_stat(m_pName, &fstats) == 0)
	{
		m_Modified = (GTime)fstats.st_mtime;
		m_Size = fstats.st_size;
		m_IsDir = (fstats.st_mode & S_IFDIR) != 0;
	}
	else
	{
		g_debug("%d", errno);
	}
	/*
	UUID uuid;
	UuidCreate(&uuid);
	UuidToString(&uuid, (unsigned char**)&m_pId);
	*/
	m_Version = 0;
}

IFFileInfo::IFFileInfo(const gchar *pName, const gchar *pID, GTime modified, gint64 size, gboolean isDir)
{
	m_pShortName = g_path_get_basename(pName);
	m_pName = g_strdup(pName);
	m_pId = g_strdup(pID);
	m_Modified = modified;
	m_Size = size;
	m_IsDir = isDir;
	m_Version = 0;
}

IFFileInfo::~IFFileInfo(void)
{
	g_free(m_pShortName);
	g_free(m_pName);
	g_free(m_pId);
}

gint IFFileInfo::Compare(IFFileInfo *pInfo1, IFFileInfo *pInfo2)
{
	return strcmp(pInfo1->m_pName, pInfo2->m_pName);
}

void IFFileInfo::Destroy(gpointer pInfo)
{
	IFFileInfo *pI = (IFFileInfo*)pInfo;
	delete pI;
}

gboolean IFFileInfo::Serialize(FILE *pStream)
{
	gchar *name = g_markup_escape_text(m_pName, (gssize)strlen(m_pName));
	fprintf(pStream, "<%s name=\"%s\" id=\"%s\" modified=\"%d\" size=\"%Ld\" isdir=\"%d\"/>\n", m_pXmlTag, name, m_pId, m_Modified, m_Size, m_IsDir);
	g_free(name);
	return true;
}

IFFileInfo* IFFileInfo::DeSerialize(const gchar** pANames, const gchar **pAValues)
{
	GTime modified;
	sscanf(pAValues[2], "%d", &modified);
	gint64 size;
	sscanf(pAValues[3], SCANLL, &size);
	gboolean isDir;
	sscanf(pAValues[4], "%d", &isDir);
	return new IFFileInfo(pAValues[0], pAValues[1], modified, size, isDir);
}

gboolean IFFileInfo::Changed(IFFileInfo *pOther)
{
	if ((m_Modified != pOther->m_Modified) && (m_Size != pOther->m_Size))
		return true;
	return false;
}

//
// IFFileInfoIterator class
//
IFFileInfoIterator::IFFileInfoIterator(GList *pLink)
{ 
	pItem = g_list_first(pLink); 
}
	


//
// IFFIleInfoList class
//

gchar	*IFFileInfoList::m_pSignature = "iFolderList";
gfloat	IFFileInfoList::m_Version = 1.0;


IFFileInfoList::IFFileInfoList(void)
{
	// Set the list to empty.
	m_pList = NULL;
}

IFFileInfoList::~IFFileInfoList(void)
{
	// Delete the IFFileInfo objects.
	GList *pNode = m_pList;
	while (pNode != NULL)
	{
		IFFileInfo *pInfo = (IFFileInfo*)pNode->data;
		delete pInfo;
		pNode = g_list_next(pNode);
	}
	g_list_free(m_pList);
}

void IFFileInfoList::Insert(IFFileInfo *pInfo)
{
	m_pList = g_list_insert_sorted(m_pList, pInfo, &CompareName);
}

void IFFileInfoList::Delete(IFFileInfo *pInfo)
{
	m_pList = g_list_remove(m_pList, pInfo);
}

void IFFileInfoList::Save(gchar *pFName, GTimeVal timeStamp)
{
	FILE *pStream = g_fopen(pFName, "w");
	if (pStream == NULL)
	{
		// This is an error.
		return;
	}
	// Write the header
	fprintf(pStream, "<%s version=\"%f\" count=\"%d\">\n", m_pSignature, m_Version, g_list_length(m_pList));
	// Now we need to save each entry.
	GList *pNode = m_pList;
	while (pNode != NULL)
	{
		IFFileInfo *pInfo = (IFFileInfo*)pNode->data;
		pInfo->Serialize(pStream);
		pNode = g_list_next(pNode);
	}
	fprintf(pStream, "</%s>\n", m_pSignature);
	fclose(pStream);
}

void IFFileInfoList::Restore(gchar *pFName)
{
	const GMarkupParser parser = {StartElement, NULL, NULL, NULL, NULL};
	GMarkupParseContext *pContext = g_markup_parse_context_new(&parser, (GMarkupParseFlags)0, this, NULL);

	gchar *pFileData = NULL;
	gsize fileLength;
	GError *pError = NULL;
	if (!g_file_get_contents(pFName, &pFileData, &fileLength, &pError))
	{
		g_debug(pError->message);
		g_clear_error(&pError);
	}
	if (pFileData != NULL)
	{
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
	}
	g_markup_parse_context_free(pContext);
}

void IFFileInfoList::StartElement(GMarkupParseContext *pContext, const gchar *pName, const gchar **pANames, const gchar **pAValues, gpointer userData, GError **ppError)
{
	if (strcmp(pName, IFFileInfo::m_pXmlTag) == 0)
	{
		IFFileInfoList *pList = (IFFileInfoList*)userData;
		pList->Insert(IFFileInfo::DeSerialize(pANames, pAValues));
	}
}

gint IFFileInfoList::CompareName(gconstpointer a, gconstpointer b)
{
	IFFileInfo *pInfoA = (IFFileInfo*)a;
	IFFileInfo *pInfoB = (IFFileInfo*)b;
	return strcmp(pInfoA->m_pName, pInfoB->m_pName); 
}

IFFileInfoIterator IFFileInfoList::GetIterator()
{
	IFFileInfoIterator iterator(m_pList);
	return iterator;
}

