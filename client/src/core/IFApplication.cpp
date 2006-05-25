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

#ifndef WIN32
#include <errno.h>
#endif

gchar		*IFApplication::m_pDataPath = NULL;
gboolean	IFApplication::m_Initialized = false;
gchar		IFApplication::SEPARATOR[] = "|";

GQuark	IF_CORE_ERROR_DOMAIN_QUARK = g_quark_from_string(IF_CORE_ERROR_DOMAIN);
	

gboolean IFApplication::Initialize(GError **error)
{
	gchar *pPath = g_build_filename(g_get_home_dir(), ".ifolder3", NULL);
	gboolean status = Initialize(pPath, error);
	g_free(pPath);
	return status;
}

gboolean IFApplication::Initialize(const gchar *pDataPath, GError **error)
{
	// Check to see if we are already initialized.
	if (g_atomic_int_compare_and_exchange(&m_Initialized, false, true))
	{
		m_pDataPath = g_strdup(pDataPath);
		if (g_mkdir_with_parents(m_pDataPath, 0700) == -1)
		{
			// There was an error creating the directory.
			g_set_error(error, g_file_error_quark(), g_file_error_from_errno(errno), "Failed creating path : %s\n", m_pDataPath);
			return false;
		}
		return true;
	}
	// We are already initialized make sure the paths match.
	if (strcmp(pDataPath, m_pDataPath) == 0)
		return true;
	
	// Someone tried to initialize to another path return an error.
	g_set_error(error, IF_CORE_ERROR_DOMAIN_QUARK, IF_ERR_ALREADY_INITIALIZED, "Already initialize to path : %s\n", m_pDataPath);
	return false;
}

gchar* IFApplication::BuildUrlToService(const gchar *pHost, const gchar *pSvcUrl)
{
	gchar** urlparts = g_strsplit(pSvcUrl, "/", 4);
	gchar *pUrl = g_strjoin("/", urlparts[0], urlparts[1], pHost, urlparts[3], NULL);
	g_strfreev(urlparts);
	return pUrl;
}
