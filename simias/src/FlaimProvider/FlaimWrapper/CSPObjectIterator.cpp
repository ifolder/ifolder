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
#include "CSPObjectIterator.h"

CSPObjectIterator::CSPObjectIterator(CSPStore *pStore, HFCURSOR cursor, int count, FLMBOOL includeColId) :
	m_pStore(pStore),
	m_Count(count),
	m_Index(0),
	m_pRecords(0),
	m_includeColId(includeColId)
{
	if (m_Count)
	{
		RCODE rc;
		m_pRecords = new FLMUINT[m_Count];
		if (m_pRecords)
		{
			int i;
			for (i = 0; i < count; ++i)
			{
				rc = FlmCursorNextDRN(cursor, &m_pRecords[i]);
				if (RC_BAD(rc))
				{
					m_Count = 0;
					break;
				}
			}
		}
	}
}

CSPObjectIterator::~CSPObjectIterator(void)
{
	if (m_pRecords)
	{
		delete [] m_pRecords;
	}
}

int CSPObjectIterator::NextXml(FLMUNICODE *pOriginalBuffer, int nChars)
{
	RCODE rc = FERR_OK;
	int charsWritten = nChars;
	int len = 0;
	FlmRecord	*pRec = 0;
	FLMUNICODE* pBuffer = pOriginalBuffer;
	int endTagLen = f_unilen((FLMUNICODE*)XmlObjectListEndString) + 1;

	if (m_Index < m_Count)
	{
		if ((len = flmstrcpy(pBuffer, (FLMUNICODE*)XmlObectListString, nChars)) != -1)
		{
			nChars -= len + endTagLen;
			pBuffer += len;
			
			while (RC_OK(rc) && m_Index < m_Count)
			{
				rc = FlmRecordRetrieve(m_pStore->GetDB(), FLM_DATA_CONTAINER, m_pRecords[m_Index], FO_EXACT, &pRec, 0);
				if (RC_OK(rc) && pRec)
				{
					CSPStoreObject *pObject = new CSPStoreObject(m_pStore, pRec);
					if (pObject)
					{
						if ((len = pObject->ToXML(pBuffer, nChars, false, m_includeColId)) != -1)
						{
							nChars -= len;
							pBuffer += len;
							m_Index++;
						}
						else
						{
							rc = FERR_MEM;
						}
						delete pObject;
						pRec = 0;
					}
				}
				else if (rc == FERR_NOT_FOUND)
				{
					m_Index++;
					rc = FERR_OK;
				}
				else if (RC_BAD(rc))
				{
					m_Index++;
					rc = FERR_OK;
				}
			}
			if ((len = flmstrcpy(pBuffer, (FLMUNICODE*)XmlObjectListEndString, nChars + endTagLen)) != -1)
			{
				nChars++;
			}
		}
	}
	return (len != -1 ? charsWritten - nChars : 0);
}

bool CSPObjectIterator::SetIndex(IndexOrigin origin, int offset)
{
	int newOffset = -1;
	switch (origin)
	{
	case CUR:
		newOffset += offset;
		break;
	case END:
		newOffset = m_Count + offset;
		break;
	case SET:
		newOffset = offset;
		break;
	}

	if (newOffset <= m_Count && newOffset >= 0)
	{
			m_Index = newOffset;
			return true;
	}
	else
		return false;
}



