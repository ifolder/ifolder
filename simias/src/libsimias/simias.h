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
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Author: Boyd Timothy <btimothy@novell.com>
 * 
 ***********************************************************************/
#ifndef _SIMIAS_H
#define _SIMIAS_H 1

#define SIMIAS_SUCCESS 0
#define SIMIAS_ERROR_UNKNOWN		-1
#define SIMIAS_ERROR_NOT_RUNNING	-101
#define SIMIAS_ERROR_NO_CONFIG_FILE	-102

/**
 * This function will return the full URL of where Simias is currently running.
 * 
 * param: url    The variable to be filled with the local service URL.
 *               This must be freed if the function is successful.
 * 
 * returns: Returns SIMIAS_SUCCESS (0) if successful or one of the errors listed
 *          above if it's not successful.  If successful, the memory used by
 *          url must be freed.
 */
int simias_get_local_service_url(char **url);

#endif
