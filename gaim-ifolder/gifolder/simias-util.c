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

#include "simias-util.h"

#include <stdio.h>
#include <string.h>

/* Gaim Includes */
#include "internal.h"
#include "network.h"

/* Static global variables */
static char the_public_ip[32];

/**
 * The behavior of this function is like String.IndexOf();
 */
int
simias_str_index_of(char *str, char search_char)
{
	int i;
	int length;

	if (!str)
		return -1;

	length = strlen(str);
	for (i = 0; i < length; i++)
	{
		if (str[i] == search_char)
			return i;
	}

	return -1;
}

/**
 * Returned value must be freed.
 *
 * Param: str_with_space should be a null-terminated string that
 *        potentially has spaces in it.  All spaces will be
 *        converted to "%20".
 */
char *
simias_escape_spaces(char *str_with_space)
{
	char escapedString[1024];
	int i;
	int j = 0;

	for (i = 0; str_with_space[i] != '\0'; i++)
	{
		if (str_with_space[i] == ' ')
		{
			escapedString[j++] = '%';
			escapedString[j++] = '2';
			escapedString[j++] = '0';
		}
		else
		{
			escapedString[j++] = str_with_space[i];
		}
	}

	escapedString[j] = '\0';

	return strdup(escapedString);
}

/**
 * Does the same thing that gaim_url_parse() does, but works with
 * spaces in the path.  The returned values (host, port, and path)
 * should be freed.
 */
gboolean
simias_url_parse(const char *url, char **proto, char **host, char **port, char **path)
{
	char *tmp_save;
	char *tmp;
	int colonPos;
	int slashPos;

	*proto = NULL;
	*host = NULL;
	*port = NULL;
	*path = NULL;

	// Host
	tmp = strdup(url);
	tmp_save = tmp;
	colonPos = simias_str_index_of(tmp, ':');
	if (colonPos <= 0)
	{
fprintf(stderr, "Couldn't parse the protocol in simias_url_parse()\n");
		free(tmp_save);
		return FALSE;
	}
	else
	{
		*proto = malloc(sizeof(char) * (colonPos + 1));
		memset(*proto, '\0', colonPos + 1);
		strncpy(*proto, tmp, colonPos);
	}

	// Parse the host and port
	tmp = tmp + colonPos + 3;
	colonPos = simias_str_index_of(tmp, ':');
	if (colonPos <= 0)
	{
		// There must not be a port specified
		slashPos = simias_str_index_of(tmp, '/');
		if (slashPos <= 0)
		{
fprintf(stderr, "Couldn't parse the hostname in simias_url_parse()\n");
			free(*host);
			free(tmp_save);
			return FALSE;
		}
		else
		{
			*host = malloc(sizeof(char) * (slashPos + 1));
			memset(*host, '\0', slashPos + 1);
			strncpy(*host, tmp, slashPos);

			*port = strdup("80");

			tmp = tmp + slashPos + 1;
		}
	}
	else
	{
		// Parse the host and port
		*host = malloc(sizeof(char) * (colonPos + 1));
		memset(*host, '\0', colonPos + 1);
		strncpy(*host, tmp, colonPos);

		tmp = tmp + colonPos + 1;
		slashPos = simias_str_index_of(tmp, '/');
		if (slashPos <= 0)
		{
fprintf(stderr, "Couldn't parse the port in simias_url_parse()\n");
			free(*proto);
			free(*host);
			free(tmp_save);
			return FALSE;
		}
		else
		{
			*port = malloc(sizeof(char) * (slashPos + 1));
			memset(*port, '\0', slashPos + 1);
			strncpy(*port, tmp, slashPos);

			tmp = tmp + slashPos + 1;
		}
	}

	// Parse the path
	*path = strdup((const char *)tmp);

	free(tmp_save);
	return TRUE;
}

/**
 * This calls /sbin/ifconfig and returns the first address
 * that is NOT 127.0.0.*.  The returned string is statically
 * allocated, so if it is needed elsewhere, you should create
 * a copy of it before calling simias_get_public_ip again.  If
 * no public IP address can be determined, NULL will be returned.
 */
const char * simias_get_public_ip()
{
#ifdef _WIN32
	return gaim_network_get_my_ip(-1);
#else
	FILE *output;
	char line[32];
	int i;
	char *public_ip = NULL;

	output = popen("/sbin/ifconfig |grep 'inet ' |cut -f2 -d':' |cut -f1 -d' '", "r");
	if (!output)
	{
		fprintf(stderr, "popen() returned NULL\n");
		return NULL;
	}

	while (fgets(line, sizeof(line), output))
	{
		/* Remove any newline chars */
		for (i = strlen(line) - 1; i > 0; i--)
		{
			if (line[i] == '\n' || line[i] == '\r')
				line[i] = '\0';
			else
				break;
		}

		if (strncmp(line, "127.0.0.", 8) == 0)
			continue;
		else
		{
			sprintf(the_public_ip, line);
			public_ip = the_public_ip;
		}
	}

	pclose(output);

	return public_ip;
#endif
}
