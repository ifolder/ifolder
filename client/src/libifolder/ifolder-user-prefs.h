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

#define IFOLDER_USER_PREFS_GROUP_NAME "UserPreferences"

//! Returns true if a setting exists for key.
gboolean ifolder_user_pref_exists(const gchar *key, GError **error);

//! Deletes a user preference
void ifolder_user_pref_delete(const gchar *key, GError **error);

//! Reset a user preference to its default value
/**
 * @param error If no default value was set, an error is returned here.
 */
void ifolder_user_pref_reset(const gchar *key, GError **error);

/*@}*/

/**
 * @name string settings
 * @{
 */
void ifolder_user_pref_add_string(const gchar *key, const gchar *default_value, GError **error);
const gchar * ifolder_user_pref_get_string(const gchar *key, const gchar *default_value, GError **error);
void ifolder_user_pref_set_string(const gchar *key, const gchar *value, GError **error);
/*@}*/

/**
 * @name int settings
 * @{
 */
void ifolder_user_pref_add_int(const gchar *key, const gchar *default_value, GError **error);
int ifolder_user_pref_get_int(const gchar *key, int default_value, GError **error);
void ifolder_user_pref_set_int(const gchar *key, int value, GError **error);
/*@}*/

/**
 * @name float settings
 * @{
 */
void ifolder_user_pref_add_float(const gchar *key, const gchar *default_value, GError **error);
float ifolder_user_pref_get_float(const gchar *key, float default_value, GError **error);
void ifolder_user_pref_set_float(const gchar *key, float value, GError **error);
/*@}*/

/**
 * @name long settings
 * @{
 */
void ifolder_user_pref_add_long(const gchar *key, const gchar *default_value, GError **error);
long ifolder_user_pref_get_long(const gchar *key, long default_value, GError **error);
void ifolder_user_pref_set_long(const gchar *key, long value, GError **error);
/*@}*/

/**
 * @name boolean settings
 * @{
 */
void ifolder_user_pref_add_bool(const gchar *key, const gchar *default_value, GError **error);
gboolean ifolder_user_pref_get_bool(const gchar *key, gboolean default_value, GError **error);
void ifolder_user_pref_set_bool(const gchar *key, gboolean value, GError **error);
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
void (*user_pref_added)(const gchar *key, const void *default_value);
 @endeventproto
 @eventdesc
  Emitted when a new user preference is added.
 @param key The user preference key (its name).
 @param default_value The default value of the key if one exists, or NULL otherwise.
@endeventdef

@eventdef user-pref-deleted
 @eventproto
void (*user_pref_deleted)(const gchar *key);
 @endeventproto
 @eventdesc
  Emitted when a user preference is deleted.
 @param key The user preference key (its name).
@endeventdef

@eventdef user-pref-reset
 @eventproto
void (*user_pref_reset)(const gchar *key);
 @endeventproto
 @eventdesc
  Emitted when a user preference is reset.
 @param key The user preference key (its name).
@endeventdef

@eventdef user-pref-modified
 @eventproto
void (*user_pref_modified)(const gchar *key, const void *new_value);
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
