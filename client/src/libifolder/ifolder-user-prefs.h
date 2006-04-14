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
 * 
 * @link user_prefs_events_page User Preferences Events @endlink
 */

/**
 * @name User Preferences API
 * @{
 */

//! Returns true if a setting exists for key.
bool ifolder_user_pref_exists(const char *key);

//! Deletes a user preference
/**
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_user_pref_delete(const char *key);

//! Reset a user preference to its default value
/**
 * If no default value was set, an error is returned.
 * 
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_user_pref_reset(const char *key);

/*@}*/

/**
 * @name string settings
 * @{
 */
int ifolder_user_pref_add_string(const char *key, const char *default_value);
const char * ifolder_user_pref_get_string(const char *key, const char *default_value = NULL);
int ifolder_user_pref_set_string(const char *key, const char *value);
/*@}*/

/**
 * @name int settings
 * @{
 */
int ifolder_user_pref_add_int(const char *key, const char *default_value);
int ifolder_user_pref_get_int(const char *key, int default_value = 0);
int ifolder_user_pref_set_int(const char *key, int value);
/*@}*/

/**
 * @name float settings
 * @{
 */
int ifolder_user_pref_add_float(const char *key, const char *default_value);
float ifolder_user_pref_get_float(const char *key, float default_value = 0.0);
int ifolder_user_pref_set_float(const char *key, float value);
/*@}*/

/**
 * @name long settings
 * @{
 */
int ifolder_user_pref_add_long(const char *key, const char *default_value);
long ifolder_user_pref_get_long(const char *key, long default_value = 0);
int ifolder_user_pref_set_long(const char *key, long value);
/*@}*/

/**
 * @name boolean settings
 * @{
 */
int ifolder_user_pref_add_bool(const char *key, const char *default_value);
bool ifolder_user_pref_get_bool(const char *key, bool default_value = false);
int ifolder_user_pref_set_bool(const char *key, bool value);
/*@}*/

/** @page user_prefs_events_page User Preferences Events

@events
 @event user-pref-added
 @event user-pref-deleted
 @event user-pref-reset
 @event user-pref-modified
@endevents

<hr>

@eventdef user-pref-added
 @eventproto
void (*user_pref_added)(const char *key, const void *default_value);
 @endeventproto
 @eventdesc
  Emitted when a new user preference is added.
 @param key The user preference key (its name).
 @param default_value The default value of the key if one exists, or NULL otherwise.
@endeventdef

@eventdef user-pref-deleted
 @eventproto
void (*user_pref_deleted)(const char *key);
 @endeventproto
 @eventdesc
  Emitted when a user preference is deleted.
 @param key The user preference key (its name).
@endeventdef

@eventdef user-pref-reset
 @eventproto
void (*user_pref_reset)(const char *key);
 @endeventproto
 @eventdesc
  Emitted when a user preference is reset.
 @param key The user preference key (its name).
@endeventdef

@eventdef user-pref-modified
 @eventproto
void (*user_pref_modified)(const char *key, const void *new_value);
 @endeventproto
 @eventdesc
  Emitted when a user preference is modified.
 @param key The user preference key (its name).
 @param new_value The new value of the preference.
@endeventdef

*/

#ifdef __cplusplus
}
#endif		/* __cplusplus */

#endif /*_IFOLDER_C_USER_PREFS_H_*/
