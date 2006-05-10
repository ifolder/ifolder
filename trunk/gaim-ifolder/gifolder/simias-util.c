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

/* FIXME: Remove signal.h after debugging is complete */
#include <signal.h>

/* Gaim Includes */
#include "internal.h"
#include "network.h"
#include "blist.h"

/* Static global variables */
static char the_public_ip[32];

/* Forward Declarations */
static void slist_string_free_func(gpointer data, gpointer user_data);
static gint slist_string_compare_func(gconstpointer a, gconstpointer b);

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

/**
 * Return the list of machine names for a given GaimBlistNode (GaimBuddy).
 * These machine names are store in blist.xml on a buddy node.
 *
 * The returned GSList will contain char * for each machine name.  The caller
 * should call simias_free_machine_names_list() when the list is no longer
 * needed.
 */
GSList *
simias_get_buddy_machine_names(GaimBlistNode *node)
{
	GSList * machine_names_list = NULL;
	const char *machine_names_setting;
	char *machine_name;
	int comma_pos;
	char *tmp;
	int len;

//raise(2); /* Raise a SIGINT to debug this function */

	/* Read the comma-separated list and split them up into separate strings */
	machine_names_setting = gaim_blist_node_get_string(node, "simias-machine-names");
	if (!machine_names_setting)
		return NULL;

	tmp = (char *) machine_names_setting;

	comma_pos = simias_str_index_of(tmp, ',');
	while (comma_pos > 0)
	{
		machine_name = malloc(sizeof(char) * (comma_pos + 1));
		machine_name[comma_pos] = '\0';	/* NULL terminate */
		strncpy(machine_name, tmp, comma_pos);
		
		machine_names_list = g_slist_append(machine_names_list, machine_name);

		tmp = tmp + comma_pos + 1;
		comma_pos = simias_str_index_of(tmp, ',');
	}
	
	/* There's one more on the list */
	len = strlen(tmp);
	if (len > 0)
	{
		machine_name = malloc(sizeof(char) * (len + 1));
		machine_name[len] = '\0';
		strncpy(machine_name, tmp, len);
		
		machine_names_list = g_slist_append(machine_names_list, machine_name);
	}
	
	return machine_names_list;
}

/**
 * Frees up the memory used by the items and the list.
 */
void
simias_free_machine_names_list(GSList *machine_names_list)
{
	g_slist_foreach(machine_names_list, slist_string_free_func, NULL);
	g_slist_free(machine_names_list);
}

/**
 * Adds on the specified machine name to the buddy's list of machine names
 * or does nothing if the machine name already exists.
 *
 * Returns TRUE if the machine name was added or FALSE if it couldn't be
 * added or already existed.
 */
gboolean
simias_add_buddy_machine_name(GaimBlistNode *node, const char *machine_name)
{
	GSList *machine_names_list = NULL;
	GSList *existing_machine_name;
	int new_setting_len;
	char *new_setting_value;
	const char *machine_names_setting;
	
	machine_names_list = simias_get_buddy_machine_names(node);
	if (machine_names_list)
	{
		/**
		 * Check to see if the machine name already exists.  If it does, do
		 * nothing and return FALSE.
		 */
		existing_machine_name = g_slist_find_custom(machine_names_list,
													machine_name,
													slist_string_compare_func);
		if (existing_machine_name)
		{
			simias_free_machine_names_list(machine_names_list);
			return FALSE;
		}

		simias_free_machine_names_list(machine_names_list);
	}

	/* If the function makes it this far, the machine name doesn't exist */
	machine_names_setting = gaim_blist_node_get_string(node, "simias-machine-names");
	if (machine_names_setting)
	{
		/* Append the new machine_name onto the existing setting */
		new_setting_len = strlen(machine_names_setting) +
						  strlen(machine_name) +
						  2; /* 1 for comma and 1 for trailing '\0' */
		new_setting_value = malloc(sizeof(char) * new_setting_len); 
		sprintf(new_setting_value, "%s,%s", machine_names_setting, machine_name);
		gaim_blist_node_set_string(node, "simias-machine-names", new_setting_value);
		free(new_setting_value);
	}
	else
	{
		gaim_blist_node_set_string(node, "simias-machine-names", machine_name);
	}
	
	return TRUE;
}

/**
 * Removes the specified machine name to the buddy's list of machine names
 * or does nothing if the machine name didn't exists.
 *
 * Returns TRUE if the machine name was removed or FALSE if it couldn't be
 * removed or didn't exist.
 */
gboolean
simias_remove_buddy_machine_name(GaimBlistNode *node, const char *machine_name)
{
	GSList *machine_names_list = NULL;
	GSList *existing_machine_name;
	GSList *cur_machine_name;
	char *old_list;
	char *new_list = NULL;
	int cur_len;
	int new_len;
	int new_machine_name_len;
	gboolean bFirstIteration;
	
	machine_names_list = simias_get_buddy_machine_names(node);
	if (!machine_names_list)
	{
		/* An empty list means there's nothing to remove */
		return FALSE;
	}

	/**
	 * Check to see if the machine name exists.  If it doesn't
	 * free up the list and return FALSE.
	 */
	existing_machine_name = g_slist_find_custom(machine_names_list,
												machine_name,
												slist_string_compare_func);
	if (!existing_machine_name)
	{
		simias_free_machine_names_list(machine_names_list);
		return FALSE;
	}

	/* Remove the machine_name from the list */
	machine_names_list = g_slist_remove_link(machine_names_list, existing_machine_name);
	simias_free_machine_names_list(existing_machine_name);

	/* If the list is now NULL, we've got to remove the machine names setting */
	if (!machine_names_list)
	{
		gaim_blist_node_remove_setting(node, "simias-machine-names");
		return TRUE;
	}

	/* Rebuild the comma-separated setting */
	bFirstIteration = TRUE;
	cur_len = 0;
	cur_machine_name = machine_names_list;
	while (cur_machine_name)
	{
		new_machine_name_len = strlen((char *) cur_machine_name->data);
		new_len = cur_len + new_machine_name_len;

		if (!bFirstIteration)
			new_len += 1;	/* Prepend a comma */

		old_list = new_list;
		new_list = malloc(sizeof(char) * new_len + 1); /* + 1 for trailing '\0' */
		
		if (bFirstIteration)
		{
			sprintf(new_list, "%s", (char *) cur_machine_name->data);
		}
		else
		{
			sprintf(new_list, "%s,%s", old_list, (char *) cur_machine_name->data);
			free(old_list);
		}

		cur_len = new_len;

		bFirstIteration = FALSE;
		cur_machine_name = cur_machine_name->next;
	}	
	
	gaim_blist_node_set_string(node, "simias-machine-names", new_list);
	free(new_list);
	
	simias_free_machine_names_list(machine_names_list);

	return TRUE;
}

static void
slist_string_free_func(gpointer data, gpointer user_data)
{
	char *machine_name;
	
	machine_name = (char *) data;
	
	free(machine_name);
}

static gint
slist_string_compare_func(gconstpointer a, gconstpointer b)
{
	const char *s1;
	const char *s2;
	
	s1 = (const char *) a;
	s2 = (const char *) b;

	return strcmp(s1, s2);
}
