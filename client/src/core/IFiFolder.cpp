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
#include "IFiFolder.h"
#include "IFDomain.h"

IFiFolder::IFiFolder(const gchar *pPath) :
	m_RootDir(pPath)
{
	m_Path = g_strdup(pPath);
}

IFiFolder::~IFiFolder(void)
{
	g_free(m_Description);
	g_free(m_ID);
	g_free(m_Name);
	g_free(m_Owner);
	g_free(m_Path);
		
}

gboolean IFiFolder::IsiFolder(gchar *pPath)
{
	return false;
}
	
gboolean IFiFolder::Sync(GError **error)
{
	m_RootDir.DetectChanges();
	return 0;
}

gboolean IFiFolder::Stop(GError **error)
{
	return 0;
}

gboolean IFiFolder::Serialize(FILE *pStream)
{
	gchar *value;
	// <iFolder>
	fprintf(pStream, "<%s %s=\"%s\" %s=\"%s\">\n", EiFolder, EID, m_ID, EVersion, m_Version);
	// <name>
	value = g_markup_escape_text(m_Name, (gssize)strlen(m_Name));
	fprintf(pStream, "<%s>%s</%s>\n", EName, value, EName);
	g_free(value);
	// <description>
	value = g_markup_escape_text(m_Description, (gssize)strlen(m_Description));
	fprintf(pStream, "<%s>%s</%s>\n", EDescription, value, EDescription);
	g_free(value);
	// <owner>
	value = g_markup_escape_text(m_Owner, (gssize)strlen(m_Owner));
	fprintf(pStream, "<%s>%s</%s>\n", EOwner, value, EOwner);
	g_free(value);
	// <path>
	value = g_markup_escape_text(m_Path, (gssize)strlen(m_Path));
	fprintf(pStream, "<%s>%s</%s>\n", EPath, value, EPath);
	g_free(value);
	// </iFolder>
	fprintf(pStream, "</%s>\n", EiFolder);
	return true;
}

IFiFolder* IFiFolder::DeSerialize(XmlTree *tree, GNode *pDNode)
{
	IFiFolder *piFolder; 
	GNode *gnode = pDNode;
	GNode *tnode;

	// path Get the path so that we can construct the IFDomain object.
	tnode = tree->FindChild(gnode, EPath, IFXElement);
	if (tnode != NULL)
	{
		gchar *pPath = ((XmlNode*)tnode->data)->m_Value;
		piFolder = new IFiFolder(pPath);
	}
	// id
	tnode = tree->FindChild(gnode, EID, IFXAttribute);
	if (tnode != NULL)
		piFolder->m_ID = g_strdup(((XmlNode*)tnode->data)->m_Value);
	// version
	tnode = tree->FindChild(gnode, EVersion, IFXAttribute);
	if (tnode != NULL)
		piFolder->m_Version = g_strdup(((XmlNode*)tnode->data)->m_Value);
	// name
	tnode = tree->FindChild(gnode, EName, IFXElement);
	if (tnode != NULL)
		piFolder->m_Name = g_strdup(((XmlNode*)tnode->data)->m_Value);
	// description
	tnode = tree->FindChild(gnode, EDescription, IFXElement);
	if (tnode != NULL)
		piFolder->m_Description = g_strdup(((XmlNode*)tnode->data)->m_Value);
	// owner
	tnode = tree->FindChild(gnode, EOwner, IFXElement);
	if (tnode != NULL)
		piFolder->m_Owner = g_strdup(((XmlNode*)tnode->data)->m_Value);
	return piFolder;
}


// IFiFolderList class
gfloat	IFiFolderList::m_Version = 1.0;
	
IFiFolderList::IFiFolderList(IFDomain *pDomain) :
	m_pDomain(pDomain)
{
	m_List = g_array_new(true, true, sizeof(gpointer));
	m_pFileName = g_build_filename(pDomain->m_DataPath, "iFolders.xml", NULL);
	Restore();
}

IFiFolderList::~IFiFolderList(void)
{
	// free the resources.
	IFiFolderIterator dIter = GetIterator();
	IFiFolder* piFolder;
	while ((piFolder = dIter.Next()) != NULL)
	{
		delete piFolder;
	}
	g_array_free(m_List, true);
}

int IFiFolderList::Initialize()
{
	return 0;
}

void IFiFolderList::Insert(IFiFolder *piFolder)
{
	g_array_append_val(m_List, piFolder);
	Save();
}

gint IFiFolderList::Count()
{
	return m_List->len;
}

gboolean IFiFolderList::Remove(const gchar *id)
{
	int i = 0;
	IFiFolder* piFolder;
	while ((piFolder = g_array_index(m_List, IFiFolder*, i)) != NULL)
	{
		if (strcmp(piFolder->m_ID, id) == 0)
			break;
		++i;
	}
	if (piFolder != NULL)
	{
		g_array_remove_index(m_List, i);
		delete piFolder;
		Save();
		return true;
	}
	return false;
}

void IFiFolderList::Save()
{
	FILE *pStream = g_fopen(m_pFileName, "w");
	if (pStream == NULL)
	{
		// This is an error.
		return;
	}
	// Write the header
	fprintf(pStream, "<%s version=\"%f\" count=\"%d\">\n", EiFolders, m_Version, m_List->len);
	
	// Now we need to save each entry.
	IFiFolderIterator dIter = GetIterator();
	IFiFolder *piFolder;
	while ((piFolder = dIter.Next()) != NULL)
	{
		piFolder->Serialize(pStream);
	}
	fprintf(pStream, "</%s>\n", EiFolders);
	fclose(pStream);
}

void IFiFolderList::Restore()
{
	gchar *pFileData = NULL;
	gsize fileLength;
	GError *pError = NULL;
	if (!g_file_get_contents(m_pFileName, &pFileData, &fileLength, &pError))
	{
		g_debug(pError->message);
		g_clear_error(&pError);
	}
	if (pFileData != NULL)
	{
		XmlTree *xmlTree = new XmlTree();
		if (xmlTree->Parse(pFileData, fileLength, &pError))
		{
			GNode* xNode = xmlTree->FindChild(NULL, EiFolder, IFXElement);
			while (xNode != NULL)
			{
				Insert(IFiFolder::DeSerialize(xmlTree, xNode));
				xNode = xmlTree->FindSibling(xNode, EiFolder, IFXElement);
			}
		}
		else
		{
			g_debug(pError->message);
			g_clear_error(&pError);
		}
		delete xmlTree;
		g_free(pFileData);
	}
}

IFiFolderIterator IFiFolderList::GetIterator()
{
	IFiFolderIterator iterator(m_List);
	return iterator;
}

IFiFolder* IFiFolderList::GetiFolderByID(const gchar *pID)
{
	IFiFolderIterator dIter = GetIterator();
	IFiFolder *piFolder;
	while ((piFolder = dIter.Next()) != NULL)
	{
		if (strcmp(piFolder->m_ID, pID) == 0)
			break;
	}
	return piFolder;
}

IFiFolder* IFiFolderList::GetiFolderByName(const gchar *pName)
{
	IFiFolderIterator dIter = GetIterator();
	IFiFolder *piFolder;
	while ((piFolder = dIter.Next()) != NULL)
	{
		if (strcmp(piFolder->m_Name, pName) == 0)
			break;
	}
	return piFolder;
}
