/***********************************************************************
 *  $RCSfile$
 *
 *  Gaim iFolder Plugin: Allows Gaim users to share iFolders.
 *  Copyright (C) 2005 Novell, Inc.
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
 *  Some code in this file (mostly the saving and reading of the XML files) is
 *  directly based on code found in Gaim's core & plugin files, which is
 *  distributed under the GPL.
 ***********************************************************************/

#ifndef _SIMIAS_UTIL_H
#define _SIMIAS_UTIL_H 1

#include <glib.h>

/**
 * The behavior of this function is like String.IndexOf();
 */
int simias_str_index_of(char *str, char search_char);

/**
 * Returned value must be freed.
 *
 * Param: str_with_space should be a null-terminated string that
 *        potentially has spaces in it.  All spaces will be
 *        converted to "%20".
 */
char *simias_escape_spaces(char *str_with_space);

/**
 * Does the same thing that gaim_url_parse() does, but works with
 * spaces in the path.  The returned values (host, port, and path)
 * should be freed.
 */
gboolean simias_url_parse(const char *url, char **proto, char **host, char **port, char **path);

/**
 * This calls /sbin/ifconfig and returns the first address
 * that is NOT 127.0.0.*.  The returned string is statically
 * allocated, so if it is needed elsewhere, you should create
 * a copy of it before calling simias_get_public_ip again.  If
 * no public IP address can be determined, NULL will be returned.
 */
const char * simias_get_public_ip();

#endif
