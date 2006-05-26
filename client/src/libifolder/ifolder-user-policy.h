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

#ifndef _IFOLDER_C_USER_POLICY_H_
#define _IFOLDER_C_USER_POLICY_H_

#include "ifolder-types.h"
//#include "ifolder-user.h"

#ifdef __cplusplus
extern "C"
{
#endif		/* __cplusplus */

G_BEGIN_DECLS

#define IFOLDER_USER_POLICY_TYPE				(ifolder_user_policy_get_type())
#define IFOLDER_USER_POLICY(obj)				(G_TYPE_CHECK_INSTANCE_CAST ((obj), IFOLDER_USER_POLICY_TYPE, iFolderUserPolicy))
#define IFOLDER_USER_POLICY_CLASS(klass)		(G_TYPE_CHECK_CLASS_CAST ((klass), IFOLDER_USER_POLICY_TYPE, iFolderUserPolicyClass))
#define IFOLDER_IS_USER_POLICY(obj)				(G_TYPE_CHECK_INSTANCE_TYPE ((obj), IFOLDER_USER_POLICY_TYPE))
#define IFOLDER_IS_USER_POLICY_CLASS(klass)		(G_TYPE_CHECK_CLASS_TYPE ((klass), IFOLDER_USER_POLICY_TYPE))
#define IFOLDER_USER_POLICY_GET_CLASS(obj)		(G_TYPE_INSTANCE_GET_CLASS ((obj), IFOLDER_USER_POLICY_TYPE, iFolderUserPolicyClass))

/* GObject support */
GType ifolder_user_policy_get_type (void) G_GNUC_CONST;

/**
 * @name User Policy API
 */
/*@{*/

//! Returns the iFolderUserPolicy associated with an iFolderUserPolicy.
/**
 * @param user_policy The iFolderUserPolicy.
 * @param user Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
iFolderUser * ifolder_user_policy_get_user(iFolderUserPolicy *user_policy);

//! Returns whether a user's login is enabled.
/**
 * @param user_policy The iFolderUserPolicy.
 * @param login_enabled Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
gboolean ifolder_user_policy_get_login_enabled(iFolderUserPolicy *user_policy);

//! Returns a user's disk space limit (bytes).
/**
 * @todo I assume this returns the disk space limit in bytes.  Verify this.
 * @param user_policy The iFolderUserPolicy.
 * @param space_limit Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 * @see ifolder_user_policy_get_effective_space_limit()
 */
gint64 ifolder_user_policy_get_space_limit(iFolderUserPolicy *user_policy);

//! Returns a user's effective disk space limit (bytes).
/**
 * @todo Figure out the difference between this and
 * ifolder_user_policy_get_space_limit().  Update the documentation as needed.
 * 
 * @param user_policy The iFolderUserPolicy.
 * @param space_limit Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 * @see ifolder_user_policy_get_space_limit()
 */
gint64 ifolder_user_policy_get_effective_space_limit(iFolderUserPolicy *user_policy);

//! Returns a user's file size limit (bytes).
/**
 * @param user_policy The iFolderUserPolicy.
 * @param file_size_limit Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 * @see ifolder_user_policy_get_effective_file_size_limit()
 */
gint64 ifolder_user_policy_get_file_size_limit(iFolderUserPolicy *user_policy);

//! Returns a user's file size limit (bytes).
/**
 * @param user_policy The iFolderUserPolicy.
 * @param file_size_limit Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 * @see ifolder_user_policy_get_file_size_limit()
 */
gint64 ifolder_user_policy_get_effective_file_size_limit(iFolderUserPolicy *user_policy);

//! Returns the space used (bytes) by a user.
/**
 * @param user_policy The iFolderUserPolicy.
 * @param space_used Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
gint64 ifolder_user_policy_get_space_used(iFolderUserPolicy *user_policy);

//! Returns the space available (bytes) to a user.
/**
 * @param user_policy The iFolderUserPolicy.
 * @param space_available Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
gint64 ifolder_user_policy_get_space_available(iFolderUserPolicy *user_policy);

//! Returns the synchronization interval.
/**
 * @param user_policy The iFolderUserPolicy.
 * @param sync_interval Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
gint ifolder_user_policy_get_sync_interval(iFolderUserPolicy *user_policy);

//! Returns the effective synchronization interval.
/**
 * @param user_policy The iFolderUserPolicy.
 * @param sync_interval Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
gint ifolder_user_policy_get_effective_sync_interval(iFolderUserPolicy *user_policy);

//! Returns an enumeration of file types to be included.
/**
 * @param user_policy The iFolderUserPolicy.
 * @param char_ptr_enum Invalid if the call is unsuccessful.
 * @return A GArray of gchar *
 */
GArray * ifolder_user_policy_get_file_type_includes(iFolderUserPolicy *user_policy);

//! Returns an enumeration of effective file types to be included.
/**
 * @param user_policy The iFolderUserPolicy.
 * @param char_ptr_enum Invalid if the call is unsuccessful.
 * @return A GArray of gchar *
 */
GArray * ifolder_user_policy_get_effective_file_type_includes(iFolderUserPolicy *user_policy);

//! Returns an enumeration of file types to be excluded.
/**
 * @param user_policy The iFolderUserPolicy.
 * @param char_ptr_enum Invalid if the call is unsuccessful.
 * @return A GArray of gchar *
 */
GArray * ifolder_user_policy_get_file_type_excludes(iFolderUserPolicy *user_policy);

//! Returns an enumeration of effective file types to be excluded.
/**
 * @param user_policy The iFolderUserPolicy.
 * @param char_ptr_enum Invalid if the call is unsuccessful.
 * @return A GArray of gchar *
 */
GArray * ifolder_user_policy_get_effective_file_type_excludes(iFolderUserPolicy *user_policy);

/*@}*/

G_END_DECLS

#ifdef __cplusplus
}
#endif		/* __cplusplus */

#endif /*_IFOLDER_C_USER_POLICY_H_*/
