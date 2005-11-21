/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
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
// CSPStoreObject.h: interface for the CSPStoreObject class.
//
//////////////////////////////////////////////////////////////////////

#ifndef _CSPSTOREOBJECT_H_
#define _CSPSTOREOBJECT_H_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "CSPStore.h"

#define CS_MOD_NEW_RECORD		0x00000001
#define CS_MOD_MODIFIED			0x00000002
#define CS_MOD_DELETE_RECORD	0x00000004

#define ObjectMarkupSize		sizeof ("<ObjectList><Object name=\"\" id=\"\" type=\"\"></Object></ObjectList>") -1
#define PropertyMarkupSize		sizeof ("<Property name=\"\" type=\"\"></Property>") -1 
#define PFlagsMarkupSize		sizeof (" flags=\"\"") - 1

class CSPStoreObject  
{
public:
	CSPStoreObject(PCSStore pStore, FLMUNICODE *pName, FLMUNICODE *pId, FLMUNICODE *pType, FLMINT flmId);
	CSPStoreObject(PCSStore pStore, FLMUNICODE *pName, FLMUNICODE *pId, FLMUNICODE *pType, FlmRecord *pRec);
	CSPStoreObject(PCSStore pStore, FlmRecord *pRec);
	virtual ~CSPStoreObject();
	RCODE SetProperties(FLMUNICODE *pProperties);
	RCODE SetProperty(FLMUNICODE *pName, FLMUNICODE *pType, FLMUNICODE *pStringValue, FLMUNICODE *pFlags = 0, FLMINT = USER_PROP_LEVEL, FLMBOOL addMarkup = true);
	RCODE SetPropertyWithId(FLMUINT level, FLMUINT propId, CSP_TYPE cspType, CSPValue *pValue, FLMUINT flags);
	CSPValue * GetProperty(FLMUNICODE *pName);
	CSPValue *GetProperty(void *pvField);
	RCODE Flush();
	void Abort();
	RCODE Delete();
	int ToXML(FLMUNICODE *pBuffer, int nChars, FLMBOOL includeProperties, FLMBOOL includeColId);
	static CSPValue *CreateProperty(FLMUNICODE *pStringValue, FLMUNICODE *pName, CSP_TYPE type);
	int GetXmlSize();

private:
	RCODE setSize();
	FLMUINT getSize();
private:
	PCSStore		m_pStore;
	void			*m_pvSizeField;
	CSPString*		m_pName;
	CSPString*		m_pId;
	CSPString*		m_pType;
	FLMUINT			m_ModifiedFlag;
	FLMINT			m_AddOps;
	FLMINT			m_NeededSize;
	FLMUINT			m_FlmId;

public:
	FlmRecord		*m_pRec;
};

#endif // _CSPSTOREOBJECT_H_
