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

#include "ifolder-client.h"
#include "ifolder-private.h"
#include "ifolder-errors.h"
#include "ifolder-user-prefs.h"

#ifndef _
#define _
#endif

gboolean
ifolder_user_pref_exists(const gchar *key, GError **error)
{
	GError *err = NULL;
	gboolean b_pref_exists = FALSE;

	if (key == NULL)
	{
		g_set_error(error, IFOLDER_ERROR, IFOLDER_ERR_INVALID_PARAMETER, "ifolder_user_pref_exists() was called with a null key.");
		return false;
	}
	
	GKeyFile *key_file = ifolder_client_get_config_key_file(&err);
	if (err)
	{
		if (error != NULL)
			*error = err;
		else
		{
			g_debug("An error occurred calling ifolder_client_get_config_key_file: %s", err->message);
			g_clear_error(&err);
		}
		return false;
	}
	
	g_assert(key_file != NULL);

	if (!g_key_file_has_group(key_file, IFOLDER_USER_PREFS_GROUP_NAME))
		return false;

	b_pref_exists = g_key_file_has_key(key_file, IFOLDER_USER_PREFS_GROUP_NAME, key, &err);
	if (err)
	{
		if (error != NULL)
			*error = err;
		else
		{
			g_debug("Error calling g_key_file_has_key('%s'): %s", key, err->message);
			g_clear_error(&err);
		}
		return false;
	}
	
	return b_pref_exists;
}

void
ifolder_user_pref_delete(const gchar *key, GError **error)
{
	GKeyFile *key_file;
	GError *err = NULL;
	
	if (key == NULL)
	{
		g_set_error(error, IFOLDER_ERROR, IFOLDER_ERR_INVALID_PARAMETER, "ifolder_user_pref_delete() was called with a null key.");
		return;
	}
	
	key_file = ifolder_client_get_config_key_file(&err);
	if (err)
	{
		if (error != NULL)
			*error = err;
		else
		{
			g_debug("An error occurred calling ifolder_client_get_config_key_file: %s", err->message);
			g_clear_error(&err);
		}
		return;
	}
	
	g_assert(key_file != NULL);
	
	g_key_file_remove_key(key_file, IFOLDER_USER_PREFS_GROUP_NAME, key, &err);
	if (err)
	{
		if (error != NULL)
			*error = err;
		else
		{
			g_debug("Error calling g_key_file_remove_key('%s'): %s", key, err->message);
			g_clear_error(&err);
		}
	}
}

void
ifolder_user_pref_reset(const gchar *key, GError **error)
{
	g_message("FIXME: Implement ifolder_user_pref_reset by adding 'Default' to the end of the key name.");
}

void
ifolder_user_pref_add_string(const gchar *key, const gchar *default_value, GError **error)
{
	GString *default_value_key;
	GKeyFile *key_file;
	GError *err = NULL;

	if (key == NULL || default_value == NULL)
	{
		g_set_error(error, IFOLDER_ERROR, IFOLDER_ERR_INVALID_PARAMETER, "ifolder_user_pref_add_string() was called with a null key or null default_value.");
		return;
	}
	
	key_file = ifolder_client_get_config_key_file(&err);
	if (err)
	{
		if (error != NULL)
			*error = err;
		return;
	}
	
	g_assert(key_file != NULL);
	
	default_value_key = g_string_new(key);
	default_value_key = g_string_append(default_value_key, "Default");
	
	/* Save off the default value */
	g_key_file_set_string(key_file, IFOLDER_USER_PREFS_GROUP_NAME, (const gchar *)default_value_key->str, default_value);
	
	g_string_free(default_value_key, true);

	/* Set the value to the default if it doesn't already exist. */
	if (!g_key_file_has_key(key_file, IFOLDER_USER_PREFS_GROUP_NAME, key, NULL))
		g_key_file_set_string(key_file, IFOLDER_USER_PREFS_GROUP_NAME, key, default_value);
}

const gchar *
ifolder_user_pref_get_string(const gchar *key, const gchar *default_value, GError **error)
{
	GKeyFile *key_file;
	GError *err = NULL;
	gchar *value = NULL;
	
	if (key == NULL)
	{
		g_set_error(error, IFOLDER_ERROR, IFOLDER_ERR_INVALID_PARAMETER, "ifolder_user_pref_get_string() was called with a null key.");
		return NULL;
	}

	key_file = ifolder_client_get_config_key_file(&err);
	if (err)
	{
		if (error != NULL)
			*error = err;
		return NULL;
	}
	
	g_assert(key_file != NULL);

	value = g_key_file_get_value(key_file, IFOLDER_USER_PREFS_GROUP_NAME, key, &err);
	if (err)
	{
		if (err->code == G_KEY_FILE_ERROR_KEY_NOT_FOUND)
			value = (gchar *)default_value;
		else if (error != NULL)
			*error = err;
		else
		{
			g_debug("Error calling g_key_file_get_value('%s'): %s", key, err->message);
			g_clear_error(&err);
		}
	}
	
	return value;
}

void
ifolder_user_pref_set_string(const gchar *key, const gchar *value, GError **error)
{
	GKeyFile *key_file;
	GError *err = NULL;
	
	if (key == NULL || value == NULL)
	{
		g_set_error(error, IFOLDER_ERROR, IFOLDER_ERR_INVALID_PARAMETER, "ifolder_user_pref_set_string() was called with a null key or null value.");
		return;
	}

	key_file = ifolder_client_get_config_key_file(&err);
	if (err)
	{
		if (error != NULL)
			*error = err;
		return;
	}
	
	g_assert(key_file != NULL);

	g_key_file_set_string(key_file, IFOLDER_USER_PREFS_GROUP_NAME, key, value);
}

void
ifolder_user_pref_add_int(const gchar *key, const gchar *default_value, GError **error)
{
	g_message("FIXME: Implement ifolder_user_pref_add_int()");
}

int
ifolder_user_pref_get_int(const gchar *key, int default_value, GError **error)
{
	g_message("FIXME: Implement ifolder_user_pref_get_int()");
	
	return -1;
}

void
ifolder_user_pref_set_int(const gchar *key, int value, GError **error)
{
	g_message("FIXME: Implement ifolder_user_pref_set_int()");
}

void
ifolder_user_pref_add_float(const gchar *key, const gchar *default_value, GError **error)
{
	g_message("FIXME: Implement ifolder_user_pref_add_float()");
}

float
ifolder_user_pref_get_float(const gchar *key, float default_value, GError **error)
{
	g_message("FIXME: Implement ifolder_user_pref_get_float()");
	
	return -1.0;
}

void
ifolder_user_pref_set_float(const gchar *key, float value, GError **error)
{
	g_message("FIXME: Implement ifolder_user_pref_set_float()");
}

void
ifolder_user_pref_add_long(const gchar *key, const gchar *default_value, GError **error)
{
	g_message("FIXME: Implement ifolder_user_pref_add_long()");
}

long
ifolder_user_pref_get_long(const gchar *key, long default_value, GError **error)
{
	g_message("FIXME: Implement ifolder_user_pref_get_long()");
	
	return -1;
}

void
ifolder_user_pref_set_long(const gchar *key, long value, GError **error)
{
	g_message("FIXME: Implement ifolder_user_pref_set_long()");
}

void
ifolder_user_pref_add_bool(const gchar *key, const gchar *default_value, GError **error)
{
	g_message("FIXME: Implement ifolder_user_pref_add_bool()");
}

gboolean
ifolder_user_pref_get_bool(const gchar *key, gboolean default_value, GError **error)
{
	g_message("FIXME: Implement ifolder_user_pref_get_bool()");
	
	return false;
}

void
ifolder_user_pref_set_bool(const gchar *key, gboolean value, GError **error)
{
	g_message("FIXME: Implement ifolder_user_pref_set_bool()");
}

