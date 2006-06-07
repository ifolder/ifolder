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
#ifndef _IFITERATOR_H_
#define _IFITERATOR_H_

#include "glibclient.h"

// Forward declarations
class IFDomain;
class IFiFolder;

template <class T>
class GLIBCLIENT_API IFIterator
{
private:
	GArray				*m_List;
	guint				m_Index;
	
public:
	IFIterator(GArray* list) {m_List = list; m_Index = 0; }
	virtual ~IFIterator() {};
	void Reset() {m_Index = 0; }
	T* Next()
	{
		if (m_Index >= m_List->len)
			return NULL;
		T *pEntry = g_array_index(m_List, T*, m_Index);
		m_Index++;
		return pEntry;
	};
};

typedef IFIterator<IFDomain> IFDomainIterator;
typedef IFIterator<IFiFolder> IFiFolderIterator;

#endif // _IFITERATOR_H_
