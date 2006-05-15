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
#include "IFApplication.h"

gchar		*IFApplication::m_pDataPath = NULL;
gboolean	IFApplication::m_Initialized = false;
gchar		IFApplication::SEPARATOR[] = "|";
	

gboolean IFApplication::Initialize()
{
	return Initialize(g_build_filename(g_get_home_dir(), ".ifolder3", NULL));
}

gboolean IFApplication::Initialize(const gchar *pDataPath)
{
	// Check to see if we are already initialized.
	if (g_atomic_int_compare_and_exchange(&m_Initialized, false, true))
	{
		m_pDataPath = (gchar *)pDataPath;
		if (g_mkdir_with_parents(m_pDataPath, 0) == -1)
		{
			// There was an error creating the directory.
			return false;
		}
		return true;
	}
	// We are already initialized make sure the paths match.
	if (strcmp(pDataPath, m_pDataPath) == 0)
		return true;
	
	// Someone tried to initialize to another path return an error.
	return false;
}
