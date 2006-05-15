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
#ifndef _IFAPPLICATION_H_
#define _IFAPPLICATION_H_

#include <glib.h>
#include "glibclient.h"

class GLIBCLIENT_API IFApplication
{
private:
	static gchar	*m_pDataPath;
	static gboolean	m_Initialized;

public:
	static gchar SEPARATOR[];
		
public:
	inline static gchar* DataPath()
	{
		return m_pDataPath;
	}

	static gboolean Initialize();
	static gboolean Initialize(gchar *pDataPath);
};
#endif //_IFAPPLICATION_H_