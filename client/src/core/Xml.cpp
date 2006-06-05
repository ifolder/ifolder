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

#include <string.h>
#include "Xml.h"


// Xml Element Tags.
gchar *EDomain = "Domain";
gchar *EiFolder = "iFolder";
gchar *EName = "name";
gchar *EDescription = "description";
gchar *EUser = "User";
gchar *EHostID = "Host";
gchar *EMasterHost = "master-host";
gchar *EID = "id";
gchar *EPW = "pw";
gchar *EPOB = "pob";
gchar *EVersion = "version";
gchar *EActive = "active";
gchar *EDefault = "default";
gchar *EOwner = "owner";
gchar *EPath = "path";



// XmlTree Class
XmlTree::XmlTree()
{
	m_CurrentNode = m_RootNode = NULL;
}

XmlTree::~XmlTree()
{
	// Free the XmlNodes.
	g_node_traverse(m_RootNode, G_IN_ORDER, G_TRAVERSE_ALL, -1, FreeXmlNodes, NULL);

	g_node_destroy(m_RootNode);
}

gboolean XmlTree::FreeXmlNodes(GNode *pNode, gpointer data)
{
	XmlNode *xNode = (XmlNode*)pNode->data;
	delete xNode;
	return false;
}

void XmlTree::StartNode(const gchar *name)
{
	GNode *newNode = g_node_new(new XmlNode(g_strdup(name), IFXElement));
	if (m_CurrentNode)
	{
		g_node_append(m_CurrentNode, newNode);
		m_CurrentNode = newNode;
	}
	else
	{
		m_CurrentNode = m_RootNode = newNode;
	}
}

void XmlTree::EndNode()
{
	if (m_CurrentNode)
		m_CurrentNode = m_CurrentNode->parent;
}

void XmlTree::AddText(const gchar *text, gsize len)
{
	if (m_CurrentNode)
	{
		XmlNode *pNode = (XmlNode*)m_CurrentNode->data;
		gchar *pValue = g_strstrip(g_strndup(text, len));
		pNode->m_Value = pValue;
	}
}

void XmlTree::AddAttribute(const gchar *name, const gchar *value)
{
	if (m_CurrentNode)
	{
		GNode *newNode = g_node_new(new XmlNode(g_strdup(name), g_strdup(value), IFXAttribute));
		g_node_append(m_CurrentNode, newNode);
	}
}

GNode* XmlTree::FindChild(GNode *parent, gchar* name, IFXNodeType type)
{
	if (parent == NULL)
		parent = m_RootNode;

    GNode *gnode = g_node_first_child(parent);
	while (gnode != NULL)
	{
		XmlNode* xNode = (XmlNode*)gnode->data;
		if (xNode->m_Type == type)
		{
			if (strcmp(xNode->m_Name, name) == 0)
			{
				break;
			}
		}
		gnode = g_node_next_sibling(gnode);
	}
	return gnode;
}

GNode* XmlTree::FindSibling(GNode *sibling, gchar* name, IFXNodeType type)
{
	if (sibling == NULL)
		return NULL;

    GNode *gnode = g_node_next_sibling(sibling);
	while (gnode != NULL)
	{
		XmlNode* xNode = (XmlNode*)gnode->data;
		if (xNode->m_Type == type)
		{
			if (strcmp(xNode->m_Name, name) == 0)
			{
				break;
			}
		}
		gnode = g_node_next_sibling(gnode);
	}
	return gnode;
}
