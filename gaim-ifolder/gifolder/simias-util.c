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

#include <string.h>

/* Gaim Includes */
#include "internal.h"

char *
simias_fill_time_str(char *time_str, int buf_len, time_t time)
{
	struct tm *time_ptr;
	
	time_ptr = localtime(&time);
	strftime(time_str, buf_len, TIMESTAMP_FORMAT, time_ptr);
	
	return time_str;
}

char *
simias_fill_state_str(char *state_str, INVITATION_STATE state)
{
	switch (state) {
		case STATE_NEW:
			sprintf(state_str, _("New"));
			break;
		case STATE_PENDING:
			sprintf(state_str, _("Pending"));
			break;
		case STATE_SENT:
			sprintf(state_str, _("Sent"));
			break;
		case STATE_REJECTED_PENDING:
			sprintf(state_str, _("Rejected (Pending)"));
			break;
		case STATE_REJECTED:
			sprintf(state_str, _("Rejected"));
			break;
		case STATE_ACCEPTED_PENDING:
			sprintf(state_str, _("Accepted (Pending)"));
			break;
		case STATE_ACCEPTED:
			sprintf(state_str, _("Accepted"));
			break;
		default:
			sprintf(state_str, _("N/A"));
	}
	
	return state_str;
}

/**
 * This function takes a part of an IP Address and validates that it is a number
 * AND that it is a number >= 0 and < 255.
 */
gboolean
simias_is_valid_ip_part(const char *ip_part)
{
	long int num;
	char *error_ptr = NULL;

	g_print("simias_is_valid_ip_part(\"%s\") entered\n", ip_part);
	num = strtol(ip_part, &error_ptr, 10);
	if (error_ptr && error_ptr[0] != '\0') {
		g_print("strtol() (%d) failed because error_ptr was NOT NULL and NOT an empty string\n", (int) num);
		return FALSE;
	}

	if (num >= 0 && num < 255) {
		g_print("simias_is_valid_ip_part() returning TRUE\n");
		return TRUE;
	} else {
		g_print("simias_is_valid_ip_part() returning FALSE\n");
		return FALSE;
	}
}

/**
 * This function takes a buffer and parses for an IP address.
 *
 * The original buffer will be something like:
 *
 *      [simias:<message type>:xxx.xxx.xxx.xxx[':' or ']'<Any number of characters can be here>
 *                             ^
 *
 * This function receives the buffer starting with the character
 * marked with '^' in the above examples.
 *
 * This function returns the length of the IP Address (the char *) before a ':'
 * or ']' character or -1 if the message does not contain a string that
 * resembles an IP Address.
 */
int
simias_length_of_ip_address(const char *buffer)
{
	char *possible_ip_addr;
	char *part1, *part2, *part3, *part4;
	int possible_length;

	g_print("simias_length_of_ip_address() called with: %s\n", buffer);

	/* Buffer must be at least "x.x.x.x]" (8 chars long) */
	if (strlen(buffer) < 8) {
		g_print("Buffer is not long enough to contain an IP Address\n");
		return -1; /* Not a valid message */
	}

	/* The IP Address must be followed by a ':' or ']' */
	possible_ip_addr = strtok((char *)buffer, ":]");
	if (!possible_ip_addr) {
		g_print("Buffer did not contain a ':' or ']' character\n");
		return -1;
	}

	/**
	 * An IP Address has to be at least "x.x.x.x" (7 chars long)
	 * but no longer than "xxx.xxx.xxx.xxx" (15 chars long).
	 */
	possible_length = strlen(possible_ip_addr);
	if (possible_length < 7 || possible_length > 15) {
		g_print("Buffer length was less than 7 or greater than 15\n");
		return -1;
	}

	/* Now verify that all the parts of an IP address exist */
	part1 = strtok(possible_ip_addr, ".");
	if (!part1 || !simias_is_valid_ip_part(part1)) {
		g_print("Part 1 invalid\n");
		return -1;
	}

	part2 = strtok(NULL, ".");
	if (!part2 || !simias_is_valid_ip_part(part2)) {
		g_print("Part 2 invalid\n");
		return -1;
	}

	part3 = strtok(NULL, ".");
	if (!part3 || !simias_is_valid_ip_part(part3)) {
		g_print("Part 3 invalid\n");
		return -1;
	}

	part4 = strtok(NULL, ".");
	if (!part4 || !simias_is_valid_ip_part(part4)) {
		g_print("Part 4 invalid\n");
		return -1;
	}

	g_print("simias_length_of_ip_address() returning: %d\n", possible_length);
	/**
	 * If the code makes it to this point, possible_ip_addr looks
	 * like a valid IP Address (Note: this is not an exhaustive test).
	 */
	return possible_length;
}

