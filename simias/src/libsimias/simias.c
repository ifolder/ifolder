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

#include <stdlib.h>
#include <stdio.h>
#include <string.h>

#ifdef DEBUG
#define SIMIAS_DEBUG(args) (printf("libsimias: "), printf args)
#else
#define SIMIAS_DEBUG
#endif

#if defined(WIN32)
#define DIR_SEP "\\"
#else
#define DIR_SEP "/"
#endif

/* Foward Declarations */
static char *simias_get_user_profile_dir_path(char *dest_path);
static char *parse_local_service_url(FILE *file);

/* Function Implementations */
int
simias_get_local_service_url(char **url)
{
	char user_profile_dir[1024];
	char simias_config_file_path[1024];
	FILE *simias_conf_file;
	
	if (!simias_get_user_profile_dir_path(user_profile_dir)) {
		return SIMIAS_ERROR_NO_USER_PROFILE;
	}

	SIMIAS_DEBUG((stderr, "User Profile Dir: %s\n", user_profile_dir));

	sprintf(simias_config_file_path, "%s%sSimias.config",
			user_profile_dir, DIR_SEP);
	
	SIMIAS_DEBUG((stderr, "Simias Config File: %s\n", simias_config_file_path));

	/* Attempt to open the file */
	simias_conf_file = fopen(simias_config_file_path, "r");
	if (!simias_conf_file) {
		SIMIAS_DEBUG((stderr, "Error opening \"%s\"\n", simias_config_file_path));
		return SIMIAS_ERROR_OPENING_CONFIG_FILE;
	}

	*url = parse_local_service_url(simias_conf_file);

	fclose(simias_conf_file);
	
	if (!(*url)) {
		SIMIAS_DEBUG((stderr, "Couldn't find Local Service URL in \"%s\"\n",
					 simias_config_file_path));
		return SIMIAS_ERROR_UNKNOWN;
	}
	
	return SIMIAS_SUCCESS;
}

static char *
simias_get_user_profile_dir_path(char *dest_path)
{
#if defined(WIN32)
	char *user_profile;
	/* Build the configuration file path. */
	user_profile = getenv("USERPROFILE");
	if (user_profile == NULL || strlen(user_profile) <= 0) {
		SIMIAS_DEBUG((stderr, "Could not get the USERPROFILE directory\n"));
		return NULL;
	}

	sprintf (dest_path, user_profile);
#else
	char *home_dir;
	char dot_local_path[1024];
	char dot_local_share_path[1024];
	char dot_local_share_simias_path[1024];
	
	home_dir = getenv ("HOME");
	if (home_dir == NULL || strlen(home_dir) <= 0) {
		SIMIAS_DEBUG((stderr, "Could not get the HOME directory\n"));
		return NULL;
	}
	
	sprintf (dot_local_share_simias_path, "%s%s", home_dir, "/.local/share/simias");
	sprintf (dest_path, dot_local_share_simias_path);
#endif

	return dest_path;
}

/**
 * Parse through the file looking for the following line:
 * 
 * 	<setting name="WebServiceUri" value="http://127.0.0.1:12345/simias10/username"/>
 * 
 * Return a strdup of the URL inside "value" (the caller must free the char *
 * when finished with it).
 */
static char *
parse_local_service_url(FILE *file)
{
	long file_size;
	char *buffer;
	char *setting_idx;
	char *value_idx;
	char *start_quote_idx;
	char *uri;
	int b_uri_found;
	
	b_uri_found = 0;
	
	/* Determine the file size */
	fseek(file, 0, SEEK_END);
	file_size = ftell(file);
	rewind(file);
	
	/* Allocate memory to suck in the whole file into the buffer */
	buffer = (char *) malloc(file_size);
	if (!buffer) {
		SIMIAS_DEBUG((stderr, "Couldn't allocate memory to read Simias.config\n"));
		return NULL;
	}
	
	/* Read the contents of the file into the buffer */
	fread(buffer, 1, file_size, file);
	
	/* Now parse for the URL */
	/* Look for "WebServiceUri" */
	setting_idx = strstr(buffer, "WebServiceUri");
	if (setting_idx) {
		value_idx = strstr(setting_idx, "value");
		if (value_idx) {
			start_quote_idx = strstr(value_idx, "\"");
			if (start_quote_idx) {
				uri = strtok(start_quote_idx + 1, "\"");
				if (uri) {
					b_uri_found = 1;
				}
			}
		}
	}
	
	/* Free up buffer memory */
	free(buffer);
	
	if (!b_uri_found) {
		return NULL;
	}

	return strdup(uri);
}
