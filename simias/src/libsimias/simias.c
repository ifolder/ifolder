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
#include "simias.h"

#include <stdio.h>
#include <string.h>

int
simias_get_local_service_url(char **url)
{
	char the_url[1024];
	int b_found_url;
	
	b_found_url = 0;
	
#if defined(OSX)

#elif defined(WIN32)

#else
	sprintf(the_url, "http://127.0.0.1:42227/simias10/boyd");
	b_found_url = 1;
#endif

	if (b_found_url) {
		*url = strdup(the_url);
		return SIMIAS_SUCCESS;
	}

	return SIMIAS_ERROR_UNKNOWN;
}
