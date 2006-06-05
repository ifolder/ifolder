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

IFiFolder::IFiFolder(gchar *pPath) :
	m_RootDir(pPath)
{
}

IFiFolder::~IFiFolder(void)
{
}

IFiFolder* IFiFolder::Create(gchar *pPath)
{
	return new IFiFolder(pPath);
}

gboolean IFiFolder::IsiFolder(gchar *pPath)
{
	return false;
}
	

int IFiFolder::Revert()
{
	return 0;
}
int IFiFolder::Connect()
{
	return 0;
}
int IFiFolder::Sync()
{
	m_RootDir.DetectChanges();
	return 0;
}
int IFiFolder::Stop()
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
