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
static char *parse_web_service_password(FILE *file);

char *
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

static char *
parse_web_service_password(FILE *file)
{
	char line[1024];
	int i;

	if (!fgets(line, sizeof(line), file))
	{
		SIMIAS_DEBUG((stderr, "Password file empty or corrupt\n"));
		return NULL;
	}
	
	/* Remove any newline chars */
	for (i = strlen(line) - 1; i > 0; i--)
	{
		if (line[i] == '\n' || line[i] == '\r')
			line[i] = '\0';
		else
			break;
	}

	return strdup(line);
}

/**
 * This function gets the username and password needed to invoke calls to the
 * local Simias and iFolder WebServices.
 *
 * param: username (char[] that will be filled using sprintf)
 * param: password (char[] that will be filled using sprintf)
 *
 * returns: Returns SIMIAS_SUCCESS (0) if successful or one of the errors
 *          listed above it there's an error.
 */
int
simias_get_web_service_credential(char *username, char *password)
{
	char user_profile_dir[1024];
	char *user;
	char *pw;
	
	user = getenv("USER");
	
	if (!user)
		return -1;


	char simias_password_file_path[1024];
	FILE *simias_password_file;
	
	if (!simias_get_user_profile_dir_path(user_profile_dir)) {
		return SIMIAS_ERROR_NO_USER_PROFILE;
	}

	SIMIAS_DEBUG((stderr, "User Profile Dir: %s\n", user_profile_dir));

	sprintf(simias_password_file_path, "%s%s.local.if",
			user_profile_dir, DIR_SEP);
	
	SIMIAS_DEBUG((stderr, "Simias Password File: %s\n", simias_password_file_path));

	/* Attempt to open the file */
	simias_password_file = fopen(simias_password_file_path, "r");
	if (!simias_password_file) {
		SIMIAS_DEBUG((stderr, "Error opening \"%s\"\n", simias_password_file_path));
		return SIMIAS_ERROR_OPENING_PASSWORD_FILE;
	}

	pw = parse_web_service_password(simias_password_file);

	fclose(simias_password_file);
	
	if (!(password)) {
		SIMIAS_DEBUG((stderr, "Couldn't find the web service password in \"%s\"\n",
					 simias_password_file_path));
		return SIMIAS_ERROR_UNKNOWN;
	}
	
	sprintf(password, "%s", pw);
	free(pw);
	
	sprintf(username, "%s", user);
	
	return SIMIAS_SUCCESS;
}


