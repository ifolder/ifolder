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
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Author(s): Boyd Timothy <btimothy@novell.com>
 *
 ***********************************************************************/

#include <stdio.h>
#include "../ifolder-client.h"

static bool b_initialized = false;

IFOLDER_API int
ifolder_client_initialize(const char *data_path)
{
	if (b_initialized)
		return IFOLDER_ERR_ALREADY_INITIALIZED;

	// FIXME: Implement ifolder_client_initialize

	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int
ifolder_client_uninitialize(void)
{
	if (!b_initialized)
		return IFOLDER_ERR_NOT_INITIALIZED;

	// FIXME: Implement ifolder_client_uninitialize

	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API iFolderClientState
ifolder_client_get_state(void)
{
	if (!b_initialized)
		return IFOLDER_CLIENT_STATE_UNKNOWN;

	// FIXME: Implement ifolder_client_get_state

	return IFOLDER_CLIENT_STATE_UNKNOWN;
}

IFOLDER_API int
ifolder_client_run_client_update(const iFolderDomain domain)
{
//	iFolderDomainObj *domain = (iFolderDomainObj *)domain;

	if (!b_initialized)
		return IFOLDER_ERR_NOT_INITIALIZED;
		
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int
ifolder_client_start_synchronization(void)
{
	if (!b_initialized)
		return IFOLDER_ERR_NOT_INITIALIZED;
	
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int
ifolder_client_stop_synchronization(void)
{
	if (!b_initialized)
		return IFOLDER_ERR_NOT_INITIALIZED;
	
	return IFOLDER_UNIMPLEMENTED;
}

IFOLDER_API int
ifolder_client_resume_synchronization(void)
{
	if (!b_initialized)
		return IFOLDER_ERR_NOT_INITIALIZED;
	
	return IFOLDER_UNIMPLEMENTED;
}
