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
#ifndef _CSPObjectIterator_H_
#define _CSPObjectIterator_H_

#include "CSPStore.h"

typedef enum
{
	CUR = 0,
	END,
	SET
} IndexOrigin;


class CSPObjectIterator
{
public:
	CSPObjectIterator(HFCURSOR cursor, int count, FLMBOOL includeColId);
	virtual ~CSPObjectIterator(void);
	int NextXml(CSPStore *pStore, FLMUNICODE *pOriginalBuffer, int nChars);
	bool SetIndex(IndexOrigin origin, int offset);

	
private:
	int			m_Count;
	int			m_Index;
	FLMUINT		*m_pRecords;
	FLMBOOL		m_includeColId;
};

#endif // _CSPObjectIterator_H_

