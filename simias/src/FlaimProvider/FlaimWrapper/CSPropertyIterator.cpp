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
#include "CSPropertyIterator.h"

CSPPropertyIterator::CSPPropertyIterator(CSPStoreObject *pObject) :
	m_pObject(pObject),
	m_pvField(0)
{
	// Skip Level 0 and the first three properties Name, Id, Type.
	m_pvField = m_pObject->m_pRec->root();
	m_pvField = m_pObject->m_pRec->next(m_pvField);
	m_pvField = m_pObject->m_pRec->nextSibling(m_pvField);
	m_pvField = m_pObject->m_pRec->nextSibling(m_pvField);
	m_pvField = m_pObject->m_pRec->nextSibling(m_pvField);
} // CSPPropertyIterator::CSPPropertyIterator()

CSPPropertyIterator::~CSPPropertyIterator(void)
{
} // CSPPropertyIterator::~CSPPropertyIterator()

CSPValue *CSPPropertyIterator::Next() 
{
	CSPValue *pValue = 0;

	if (m_pvField != 0)
	{
		pValue = m_pObject->GetProperty(m_pvField);
		m_pvField = m_pObject->m_pRec->nextSibling(m_pvField);
	}

	return (pValue);
} // CSPPropertyIterator::Next()
