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
// CSPStoreObject.cpp: implementation of the CSPStoreObject class.
//
//////////////////////////////////////////////////////////////////////

#include "CSPStoreObject.h"

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////
void printflmstring(FLMUNICODE* pString);

CSPStoreObject::CSPStoreObject(PCSStore pStore, FLMUNICODE *pName, FLMUNICODE *pId, FLMUNICODE *pType, FLMINT flmId) :
	m_pStore(pStore),
	m_pvSizeField(0),
	m_ModifiedFlag(CS_MOD_NEW_RECORD),
	m_AddOps(0),
	m_FlmId(flmId)
{
	m_pName = new CSPString(pName, CS_Name_Display_Name);
	m_pId = new CSPString(pId, CS_Name_GUID);
	m_pType = new CSPString(pType, CS_Name_Node_Type);
	m_pRec = new FlmDefaultRec();

	// Set the size needed to return this object.
	m_NeededSize = ObjectMarkupSize;
	
	if (m_pRec != NULL)
	{
		// Set the size;
		setSize();
		// Set the type.
		SetProperty(CS_Name_Node_Type, CSPTypeStringString, pType, 0, USER_PROP_LEVEL, false);
		// Set the GUID.
		SetProperty(CS_Name_GUID, CSPTypeStringString, pId, 0, USER_PROP_LEVEL, false);
		// Set the Display Name.
		SetProperty(CS_Name_Display_Name, CSPTypeStringString, pName, 0, USER_PROP_LEVEL, false);
	}
} // CSPStoreObject::CSPStoreObject()

CSPStoreObject::CSPStoreObject(PCSStore pStore, FLMUNICODE *pName, FLMUNICODE *pId, FLMUNICODE *pType, FlmRecord *pRec) :
	m_pStore(pStore),
	m_pvSizeField(0),
	m_ModifiedFlag(CS_MOD_MODIFIED),
	m_AddOps(0),
	m_FlmId(0)
{
	m_pName = new CSPString(pName, CS_Name_Display_Name);
	m_pId = new CSPString(pId, CS_Name_GUID);
	m_pType = new CSPString(pType, CS_Name_Node_Type);
	
	// Set the size needed to return this object.
	m_NeededSize = ObjectMarkupSize;
	
	m_pRec = pRec;
	if (m_pRec != NULL)
	{
		m_FlmId = m_pRec->getID();
		// Set the size.
		setSize();
		// Set the type.
		SetProperty(CS_Name_Node_Type, CSPTypeStringString, pType, 0, USER_PROP_LEVEL, false);
		// Set the GUID.
		SetProperty(CS_Name_GUID, CSPTypeStringString, pId, 0, USER_PROP_LEVEL, false);
		// Set the Display Name.
		SetProperty(CS_Name_Display_Name, CSPTypeStringString, pName, 0, USER_PROP_LEVEL, false);
	}
} // CSPStoreObject::CSPStoreObject()


CSPStoreObject::CSPStoreObject(PCSStore pStore, FlmRecord *pRec) :
	m_pStore(pStore),
	m_ModifiedFlag(0),
	m_AddOps(0),
	m_FlmId(pRec->getID()),
	m_pRec(pRec)
{
	m_NeededSize = getSize();
	m_pType = (CSPString*)GetProperty(CS_Name_Node_Type);
	m_pId = (CSPString*)GetProperty(CS_Name_GUID);
	m_pName = (CSPString*)GetProperty(CS_Name_Display_Name);
}

CSPStoreObject::~CSPStoreObject()
{
	if (m_pRec != NULL)
	{
		Flush();
		m_pRec->Release();
	}
	if (m_pName)
	{
		delete m_pName;
	}
	if (m_pId)
	{
		delete m_pId;
	}
	if (m_pType)
	{
		delete m_pType;
	}
	
} // CSPStoreObject:~CSPStoreObject()

RCODE CSPStoreObject::SetProperties(FLMUNICODE *pProperties)
{
	RCODE	rc = FERR_OK;
	while (*pProperties)
	{
		FLMUNICODE* pName = 0;
		FLMUNICODE* pType = 0;
		FLMUNICODE* pValue = 0;
		FLMUNICODE* pFlags = 0;
		FLMUNICODE* pEnd;

		if ((pName = flmstrstr(pProperties, (FLMUNICODE*)L"name=\"")) != 0)
		{
			pName += 6;
			if ((pType = flmstrstr(pName, (FLMUNICODE*)L"type=\"")) != 0)
			{
				pType += 6;
				if ((pValue = flmstrstr(pType, (FLMUNICODE*)L">")) != 0)
				{
					// Null the string at the end of the tag so the
					// flag search does not go to far.
					*pValue = L'\0';
					if ((pFlags = flmstrstr(pType, (FLMUNICODE*)L"flags=\"")) != 0)
					{
						pFlags += 7;
					}
					// Restore the character.
					*pValue = L'>';

					pValue += 1;
					if ((pEnd = flmstrstr(pName, (FLMUNICODE*)L"\"")) != 0)
					{
						*pEnd = 0;
						if ((pEnd = flmstrstr(pType, (FLMUNICODE*)L"\"")) != 0)
						{
							*pEnd = 0;

							if (pFlags && (pEnd = flmstrstr(pFlags, (FLMUNICODE*)L"\"")) != 0)
							{
								*pEnd = 0;
							}

							if ((pEnd = flmstrstr(pValue, (FLMUNICODE*)XmlPropertyEndString)) != 0)
							{
								pProperties = pEnd + f_unilen((FLMUNICODE*)XmlPropertyEndString);
								*pEnd = 0;
								rc = SetProperty(pName, pType, pValue, pFlags);
							}
						}
					}
				}
			}
		}
		if (pName == 0 || pType == 0 || pValue == 0)
		{
			pProperties = 0;
		}
	}

	return (rc);
} // CSPStoreObject::SetProperties()


RCODE CSPStoreObject::SetProperty(FLMUNICODE *pName, FLMUNICODE *pType, FLMUNICODE *pStringValue, FLMUNICODE *pFlags, FLMINT level, FLMBOOL addMarkup)
{
	RCODE		rc = FERR_OK;
	CSP_TYPE	cspType = CSP_Type_Undefined;
	CSPValue	*pValue = NULL;
	FLMUINT		propId;
	FLMUINT		flags = 0;
	
	cspType = CSPStore::StringToType(pType);
	pValue = CreateProperty(pStringValue, pName, cspType);
	
	if (pValue)
	{
		rc = m_pStore->NameToId(pName, &propId);
		if (RC_BAD(rc))
		{
			rc = m_pStore->RegisterField(pName, pValue->GetFlaimType(), &propId);
			if (RC_OK(rc))
			{
				// Add an index for this field.
				rc = m_pStore->AddIndex(pName, propId);
			}
		}

		if (RC_OK(rc))
		{
			if (pFlags != 0)
			{
				flags = (FLMUINT)CSUtil::funitoll(pFlags);
			}

			rc = SetPropertyWithId(level, propId, cspType, pValue, flags);
			if (RC_OK(rc))
			{
				if (addMarkup)
				{
					m_NeededSize += PropertyMarkupSize;
					m_NeededSize += f_unilen(pValue->m_pName);
					m_NeededSize += f_unilen(pValue->m_pType);
				}
				if (pFlags != 0)
				{
					m_NeededSize += PFlagsMarkupSize;
					m_NeededSize += f_unilen(pFlags);
				}
				m_NeededSize += pValue->StringSize();
			}
		}
		delete pValue;
	}

	return (rc);
} // CSPStoreObject::setProperty()


CSPValue *CSPStoreObject::CreateProperty(FLMUNICODE *pStringValue, FLMUNICODE *pName, CSP_TYPE type)
{
	CSPValue		*pValue = NULL;

	// now create the correct property based on the type.
	switch (type)
	{
	case CSP_Type_String:
		pValue = new CSPString(pStringValue, pName);
		break;
	case CSP_Type_I1:
		pValue = new CSPI1(pStringValue, pName);
		break;
	case CSP_Type_UI1:
		pValue = new CSPUI1(pStringValue, pName);
		break;
	case CSP_Type_I2:
		pValue = new CSPI2(pStringValue, pName);
		break;
	case CSP_Type_UI2:
		pValue = new CSPUI2(pStringValue, pName);
		break;
	case CSP_Type_I4:
		pValue = new CSPI4(pStringValue, pName);
		break;
	case CSP_Type_UI4:
		pValue = new CSPUI4(pStringValue, pName);
		break;
	case CSP_Type_I8:
		pValue = new CSPI8(pStringValue, pName);
		break;
	case CSP_Type_UI8:
		pValue = new CSPUI8(pStringValue, pName);
		break;
	case CSP_Type_Char:
		pValue = new CSPChar(pStringValue, pName);
		break;
	case CSP_Type_Float:
		pValue = new CSPFloat(pStringValue, pName);
		break;
	case CSP_Type_Bool:
		pValue = new CSPBool(pStringValue, pName);
		break;
	case CSP_Type_DateTime:
		pValue = new CSPDTime(pStringValue, pName);
		break;
	case CSP_Type_Uri:
		pValue = new CSPUri(pStringValue, pName);
		break;
	case CSP_Type_Xml:
		pValue = new CSPXml(pStringValue, pName);
		break;
	case CSP_Type_TimeSpan:
		pValue = new CSPTimeSpan(pStringValue, pName);
		break;
	case CSP_Type_Relationship:
		pValue = new CSPRelationship(pStringValue, pName);
		break;
	case CSP_Type_Int:
	case CSP_Type_Max:
	case CSP_Type_Undefined:
		break;
	}

	return (pValue);
} // CSPStoreObject::CreateProperty()


RCODE CSPStoreObject::SetPropertyWithId(FLMUINT level, FLMUINT propId, CSP_TYPE cspType, CSPValue *pValue, FLMUINT flags)
{
	RCODE		rc = FERR_OK;;
	void		*pvField = 0;
	void		*pvTypeField = 0;
	void		*pvFlagsField = 0;

	rc = m_pRec->insertLast(level, propId, pValue->GetFlaimType(), &pvField);
	if (RC_OK(rc))
	{
		rc = pValue->ToFlaim(m_pRec, pvField);
		if (RC_OK(rc))
		{
			rc = m_pRec->insert(
				pvField, INSERT_FIRST_CHILD, CS_Id_Property_Type, FLM_NUMBER_TYPE, &pvTypeField);
			if (RC_OK(rc))
			{
				rc = m_pRec->setUINT(pvTypeField, cspType);
				if (RC_OK(rc))
				{
					if (flags != 0)
					{
						rc = m_pRec->insert(
							pvField, INSERT_LAST_CHILD, CS_Id_PFlags, FLM_NUMBER_TYPE, &pvFlagsField);
						if (RC_OK(rc))
						{
							rc = m_pRec->setUINT(pvFlagsField, flags);
						}
					}

					if (RC_OK(rc))
					{
						m_ModifiedFlag |= CS_MOD_MODIFIED;
					}
				}
			}
        }
	}

	return (rc);
} // CSPStoreObject::SetPropertyWithId()


RCODE CSPStoreObject::Flush()
{
	int		rc = FERR_OK;
	HFDB		hFlaim = m_pStore->GetDB();

	if (m_ModifiedFlag)
	{
		if (m_ModifiedFlag & CS_MOD_DELETE_RECORD)
		{
			rc = FlmRecordDelete(
				hFlaim,
				FLM_DATA_CONTAINER,
				m_FlmId,
				0); //FLM_AUTO_TRANS | 255);
		}
		else 
		{
			rc = setSize();
			if (RC_OK(rc))
			{
				if (m_ModifiedFlag & CS_MOD_NEW_RECORD)
				{
					// Get a record Id this is a new record.
					if (m_FlmId == 0)
					{
						rc = FlmReserveNextDrn(hFlaim, FLM_DATA_CONTAINER, &m_FlmId);
					}
				
					if (RC_OK(rc))
					{
						// Write the record.
						rc = FlmRecordAdd(
							hFlaim,
							FLM_DATA_CONTAINER,
							&m_FlmId,
							m_pRec,
							0); //FLM_AUTO_TRANS | 255);
					}
				}
				else if (m_ModifiedFlag & CS_MOD_MODIFIED)
				{
					rc = FlmRecordModify(
						hFlaim,
						FLM_DATA_CONTAINER,
						m_FlmId,
						m_pRec,
						0); //FLM_AUTO_TRANS | 255);
				}
			}
		}
		if (RC_OK(rc))
		{
			m_ModifiedFlag = 0;
		}
	}

	return (rc);
} // CSPStoreObject::Flush()


RCODE CSPStoreObject::setSize()
{
	RCODE		rc = FERR_OK;;
	
	if (m_pvSizeField == 0)
	{
		rc = m_pRec->insertLast(0, CS_Id_Size, FLM_NUMBER_TYPE, &m_pvSizeField);
	}
	if (m_pvSizeField != 0)
	{
		rc = m_pRec->setUINT(m_pvSizeField, m_NeededSize);
	}
	return (rc);
} // CSPStoreObject::setSize()

FLMUINT CSPStoreObject::getSize()
{
	void		*pvField;
	FLMUINT		size = 0;
	
	pvField = m_pRec->find(m_pRec->root(), CS_Id_Size);
	if (pvField)
	{
		m_pRec->getUINT(pvField, &size);
	}

	return (size);
} // CSPStoreObject::getSize()


void CSPStoreObject::Abort()
{
	m_ModifiedFlag = 0;
	m_pRec->clear();
} // CSPStoreObject::Abort()


RCODE CSPStoreObject::Delete()
{
	m_ModifiedFlag |= CS_MOD_DELETE_RECORD;
	return (FERR_OK);
} // CSPStoreObject::Delete()



CSPValue * CSPStoreObject::GetProperty(FLMUNICODE *pName)
{
	int			rc = FERR_OK;
	FLMUINT		propId;
	void		*pvField = 0;
	CSPValue	*pValue = NULL;
	
	rc = m_pStore->NameToId(pName, &propId);
	if (RC_OK(rc))
	{
		pvField = m_pRec->find(m_pRec->root(), propId);
		if (pvField)
		{
			pValue = GetProperty(pvField);
		}
	}
	return (pValue);
} // CSPStoreObject::GetProperty()


CSPValue *CSPStoreObject::GetProperty(void *pvField)
{
	int			rc = FERR_OK;
	void		*pvType = 0;
	void		*pvFlags = 0;
	CSP_TYPE	type;
	FLMUINT		flags = 0;
	CSPValue	*pValue = NULL;
	FLMUNICODE name[MAX_PROPERTY_NAME];

	if (m_pStore->IdToName(m_pRec->getFieldID(pvField), name, MAX_PROPERTY_NAME))
	{
		pvFlags = m_pRec->find(pvField, CS_Id_PFlags, 1, SEARCH_TREE);
		if (pvFlags)
		{
			rc = m_pRec->getUINT(pvFlags, &flags);
		}

		pvType = m_pRec->find(pvField, CS_Id_Property_Type, 1, SEARCH_TREE);
		if (pvType)
		{
			rc = m_pRec->getINT(pvType, (FLMINT*)&type);
			if (RC_OK(rc))
			{
				switch(type)
				{
				case CSP_Type_String:
					pValue = new CSPString(m_pRec, pvField, name);
					break;
				case CSP_Type_I1:
					pValue = new CSPI1(m_pRec, pvField, name);
					break;
				case CSP_Type_UI1:
					pValue = new CSPUI1(m_pRec, pvField, name);
					break;
				case CSP_Type_I2:
					pValue = new CSPI2(m_pRec, pvField, name);
					break;
				case CSP_Type_UI2:
					pValue = new CSPUI2(m_pRec, pvField, name);
					break;
				case CSP_Type_I4:
					pValue = new CSPI4(m_pRec, pvField, name);
					break;
				case CSP_Type_UI4:
					pValue = new CSPUI4(m_pRec, pvField, name);
					break;
				case CSP_Type_I8:
					pValue = new CSPI8(m_pRec, pvField, name);
					break;
				case CSP_Type_UI8:
					pValue = new CSPUI8(m_pRec, pvField, name);
					break;
				case CSP_Type_Char:
					pValue = new CSPChar(m_pRec, pvField, name);
					break;
				case CSP_Type_Float:
					pValue = new CSPFloat(m_pRec, pvField, name);
					break;
				case CSP_Type_Bool:
					pValue = new CSPBool(m_pRec, pvField, name);
					break;
				case CSP_Type_DateTime:
					pValue = new CSPDTime(m_pRec, pvField, name);
					break;
				case CSP_Type_Uri:
					pValue = new CSPUri(m_pRec, pvField, name);
					break;
				case CSP_Type_Xml:
					pValue = new CSPXml(m_pRec, pvField, name);
					break;
				case CSP_Type_TimeSpan:
					pValue = new CSPTimeSpan(m_pRec, pvField, name);
					break;
				case CSP_Type_Relationship:
					pValue = new CSPRelationship(m_pRec, pvField, name);
					break;
				case CSP_Type_Int:
				case CSP_Type_Max:
				case CSP_Type_Undefined:
					break;
				}
			}
		}
	}

	if (pValue)
	{
		pValue->SetFlags(flags);
	}

	return (pValue);
} // CSPStoreObject::GetProperty


int flmstrcpy(FLMUNICODE *pDest, FLMUNICODE *pSrc, int size)
{
	int len = 0;
	while (pSrc[len] != L'\0' && len < size)
	{
		pDest[len] = pSrc[len];
		len++;
	}

	if (pSrc[len] == 0)
	{
        pDest[len] = L'\0';
		return (len);
	}
	return (-1);
}

int flmstrcpyesc(FLMUNICODE *pDest, FLMUNICODE *pSrc, int size)
{
	//return flmstrcpy(pDest, pSrc, size);

	int dlen = 0;
	int slen = 0;
	while (pSrc[slen] != L'\0' && dlen < size)
	{
		switch (pSrc[slen])
		{
			case L'&':
				pDest[dlen++] = L'&';
				pDest[dlen++] = L'a';
				pDest[dlen++] = L'm';
				pDest[dlen++] = L'p';
				pDest[dlen++] = L';';
				break;
			case L'<':
				pDest[dlen++] = L'&';
				pDest[dlen++] = L'l';
				pDest[dlen++] = L't';
				pDest[dlen++] = L';';
				break;
			case L'>':
				pDest[dlen++] = L'&';
				pDest[dlen++] = L'g';
				pDest[dlen++] = L't';
				pDest[dlen++] = L';';
				break;
			case L'\"':
				pDest[dlen++] = L'&';
				pDest[dlen++] = L'q';
				pDest[dlen++] = L'u';
				pDest[dlen++] = L'o';
				pDest[dlen++] = L't';
				pDest[dlen++] = L';';
				break;
			case L'\'':
				pDest[dlen++] = L'&';
				pDest[dlen++] = L'#';
				pDest[dlen++] = L'3';
				pDest[dlen++] = L'9';
				pDest[dlen++] = L';';
				break;
			default:
				pDest[dlen++] = pSrc[slen];
				break;
		}
		slen++;
	}

	if (pSrc[slen] == 0)
	{
		pDest[dlen] = L'\0';
		return (dlen);
	}
	return (-1);
}



int CSPStoreObject::ToXML(FLMUNICODE *pOriginalBuffer, int nChars, FLMBOOL includeProperties, FLMBOOL includeColId)
{
	int charsWritten = nChars;
	int len;
	FLMUNICODE* pBuffer = pOriginalBuffer;

	if ((len = flmstrcpy(pBuffer, (FLMUNICODE*)XmlObectNameString, nChars)) != -1)
	{
		nChars -= len;
		pBuffer += len;
		if ((len = flmstrcpyesc(pBuffer, (FLMUNICODE*)m_pName->SearchVal(), nChars)) != -1)
		{
			nChars -= len;
			pBuffer += len;
			if ((len = flmstrcpy(pBuffer, (FLMUNICODE*)XmlIdString, nChars)) != -1)
			{
				nChars -= len;
				pBuffer += len;
				if ((len = m_pId->ToString(pBuffer, nChars)) != -1)
				{
					nChars -= len;
					pBuffer += len;
					if ((len = flmstrcpy(pBuffer, (FLMUNICODE*)XmlTypeString, nChars)) != -1)
					{
						nChars -= len;
						pBuffer += len;
						if ((len = m_pType->ToString(pBuffer, nChars)) != -1)
						{
							if (includeColId)
							{
								nChars -= len;
								pBuffer += len;
								if ((len = flmstrcpy(pBuffer, (FLMUNICODE*)XmlColIdString, nChars)) != -1)
								{
									nChars -= len;
									pBuffer += len;
									CSPString* colId = (CSPString*)GetProperty(CS_Name_CollectionId);
									len = colId->ToString(pBuffer, nChars);
								}
							}
					
							if (len != -1)
							{
								nChars -= len;
								pBuffer += len;
								if (includeProperties)
								{
									if ((len = flmstrcpy(pBuffer, (FLMUNICODE*)XmlEndTag, nChars)) != -1)
									{
										nChars -= len;
										pBuffer += len;
										// Now Get All of the properties.
										CSPPropertyIterator *pProperties = new CSPPropertyIterator(this);
										CSPValue *pProperty = pProperties->Next();
										while (pProperty != 0 && len)
										{
											if ((len = pProperty->ToXml(pBuffer, nChars)) != -1)
											{
												nChars -= len;
												pBuffer += len;
											}
											delete pProperty;
											pProperty = pProperties->Next();
										}
									
										if ((len = flmstrcpy(pBuffer, (FLMUNICODE*)XmlObjectEndString, nChars)) != -1)
										{
											nChars -= len;
											pBuffer += len;
										}
									}
								}
								else
								{
									if ((len = flmstrcpy(pBuffer, (FLMUNICODE*)XmlEndTagNoChildren, nChars)) != -1)
									{
										nChars -= len;
										pBuffer += len;
									}
								}
							}
						}
					}
				}
			}
		}
	}
	return (len != -1 ? charsWritten - nChars : -1);
} // CSPStoreObject::ToXML()


int CSPStoreObject::GetXmlSize()
{
	return (m_NeededSize);
} // CSPStoreObject::GetXmlSize()

