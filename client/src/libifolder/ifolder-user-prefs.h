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

#ifndef _IFOLDER_C_USER_PREFS_H_
#define _IFOLDER_C_USER_PREFS_H_

#ifdef __cplusplus
extern "C"
{
#endif		/* __cplusplus */

/**
 * @file ifolder-user-prefs.h
 * @brief User Preferences API
 */

//! Returns true if a setting exists for key.
bool ifolder_user_pref_exists(const char *key);

const char * ifolder_user_pref_get_string(const char *key, const char *default_value = NULL);
void ifolder_user_pref_set_string(const char *key, const char *value);

int ifolder_user_pref_get_int(const char *key, int default_value = 0);
void ifolder_user_pref_set_int(const char *key, int value);

float ifolder_user_pref_get_float(const char *key, float default_value = 0.0);
void ifolder_user_pref_set_float(const char *key, float value);

int ifolder_user_pref_get_long(const char *key, long default_value = 0);
void ifolder_user_pref_set_long(const char *key, long value);

bool ifolder_user_pref_get_bool(const char *key, bool default_value = false);
void ifolder_user_pref_set_bool(const char *key, bool value);

#ifdef __cplusplus
}
#endif		/* __cplusplus */

#endif /*_IFOLDER_C_USER_PREFS_H_*/
