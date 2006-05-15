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
#include "glibclient.h"
#include "IFChanges.h"

//
// IFChange class
//

gchar	*IFChange::m_pXmlTag = "change";

IFChange::IFChange(IFChangeOperation operation, const gchar *pFile, const gchar *pMetadataFile)
{
	m_Operation = operation;
	m_pFile = g_strdup(pFile);
	m_pMetadataFile = g_strdup(pMetadataFile);
}

IFChange::~IFChange(void)
{
	g_free(m_pFile);
	g_free(m_pMetadataFile);
}

gboolean IFChange::Serialize(FILE *pStream)
{
	gchar *pFile = g_markup_escape_text(m_pFile, (gssize)strlen(m_pFile));
	gchar *pMDFile = g_markup_escape_text(m_pMetadataFile, (gssize)strlen(m_pMetadataFile));
	fprintf(pStream, "<%s operation=\"%d\" file=\"%s\" metafile=\"%s\"/>\n", m_pXmlTag, m_Operation, pFile, pMDFile);
	g_free(pMDFile);
	g_free(pFile);
	return true;
}

IFChange* IFChange::DeSerialize(const gchar** pANames, const gchar **pAValues)
{
	IFChangeOperation operation;
	sscanf(pAValues[0], "%d", &operation);
	return new IFChange(operation, pAValues[1], pAValues[2]);
}

//
// IFChangeList class
//

gchar	*IFChangeList::m_RootStartTag = "<IFChangeList version=\"1.0\" count=\"%010d\">\n";
gchar	*IFChangeList::m_RootEndTag = "</IFChangeList>";

IFChangeList::IFChangeList(gchar *pDataPath)
{
	gchar *pFile = g_build_filename(pDataPath, ".~ifchanges", NULL);
	if (g_file_test(pFile, G_FILE_TEST_EXISTS))
	{
		// Open the file and set the file ptr to the end of the file.
		m_pWStream = g_fopen(pFile, "a+");
		fseek(m_pWStream, (long)strlen(m_RootEndTag), SEEK_END);
		int fd2 = dup(fileno(m_pWStream));
		m_pRStream = fdopen(fd2, "r");
	}
	else
	{
		m_pWStream = g_fopen(pFile, "w");
		WriteHeader();
	}
	g_free(pFile);
}

IFChangeList::~IFChangeList()
{
	WriteHeader();
	WriteFooter();
	fclose(m_pRStream);
	fclose(m_pWStream);
}

void IFChangeList::WriteHeader()
{
	// Write the header
	fseek(m_pWStream, 0, SEEK_SET);
	fprintf(m_pWStream, m_RootStartTag, m_Count);
}

void IFChangeList::WriteFooter()
{
	fseek(m_pWStream, 0, SEEK_END);
	fprintf(m_pWStream, m_RootEndTag);
}

void IFChangeList::Restore(gchar *pFName)
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

void IFChangeList::StartElement(GMarkupParseContext *pContext, const gchar *pName, const gchar **pANames, const gchar **pAValues, gpointer userData, GError **ppError)
{
	if (strcmp(pName, IFChange::m_pXmlTag) == 0)
	{
		IFChange *pChange = IFChange::DeSerialize(pANames, pAValues);
	}
}

