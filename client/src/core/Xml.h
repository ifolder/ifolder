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
#ifndef _XMLTREE_H_
#define _XMLTREE_H_

#include <glib.h>
#include "glibclient.h"

// Xml Element Tags.
extern gchar *EDomain;
extern gchar *EiFolder;
extern gchar *EName;
extern gchar *EDescription;
extern gchar *EUser;
extern gchar *EHostID;
extern gchar *EMasterHost;
extern gchar *EID;
extern gchar *EPW;
extern gchar *EPOB;
extern gchar *EVersion;
extern gchar *EActive;
extern gchar *EDefault;
extern gchar *EOwner;
extern gchar *EPath;

enum IFXNodeType
{
	IFXElement,
	IFXAttribute,
};

class XmlNode
{
public:
	gchar	*m_Name;
	gchar	*m_Value;

	IFXNodeType m_Type;
	XmlNode(gchar *name, IFXNodeType type) { m_Name = name; m_Value = NULL; m_Type = type; }
	XmlNode(gchar *name, gchar *value, IFXNodeType type) { m_Name = name; m_Value = value; m_Type = type; }
	virtual ~XmlNode() { g_free(m_Name); g_free(m_Value); }
};

class XmlTree
{
public:
	GNode		*m_CurrentNode;
	GNode		*m_RootNode;

	XmlTree();
	virtual ~XmlTree();
	static gboolean FreeXmlNodes(GNode *pNode, gpointer data);
	void StartNode(const gchar *name);
	void EndNode();
	void AddText(const gchar *text, gsize len);
	void AddAttribute(const gchar *name, const gchar *value);
	GNode* FindChild(GNode *parent, gchar* name, IFXNodeType type);
	GNode* FindSibling(GNode *sibling, gchar* name, IFXNodeType type);
};


#endif // _XMLTREE_H_
