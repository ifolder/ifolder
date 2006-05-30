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

#ifndef _IFOLDER_C_USER_H_
#define _IFOLDER_C_USER_H_

#include "ifolder-types.h"

#ifdef __cplusplus
extern "C"
{
#endif		/* __cplusplus */

G_BEGIN_DECLS

#define IFOLDER_USER_TYPE				(ifolder_user_get_type())
#define IFOLDER_USER(obj)				(G_TYPE_CHECK_INSTANCE_CAST ((obj), IFOLDER_USER_TYPE, iFolderUser))
#define IFOLDER_USER_CLASS(klass)		(G_TYPE_CHECK_CLASS_CAST ((klass), IFOLDER_USER_TYPE, iFolderUserClass))
#define IFOLDER_IS_USER(obj)			(G_TYPE_CHECK_INSTANCE_TYPE ((obj), IFOLDER_USER_TYPE))
#define IFOLDER_IS_USER_CLASS(klass)	(G_TYPE_CHECK_CLASS_TYPE ((klass), IFOLDER_USER_TYPE))
#define IFOLDER_USER_GET_CLASS(obj)		(G_TYPE_INSTANCE_GET_CLASS ((obj), IFOLDER_USER_TYPE, iFolderUserClass))

/* GObject support */
GType ifolder_user_get_type (void) G_GNUC_CONST;

/**
 * @file ifolder-user.h
 * @brief User API
 */

/*@}*/
/**
 * @name User API
 */
/*@{*/

//! Returns a user's unique ID.
/**
 * @param user The iFolderUser.
 * @return a user's unique ID.
 */
const gchar *ifolder_user_get_id (iFolderUser *user);

//! Returns a user's login name.
/**
 * @param user The iFolderUser.
 * @return a user's login name.
 */
const gchar *ifolder_user_get_user_name (iFolderUser *user);

//! Returns a user's full name.
/**
 * @param user The iFolderUser.
 * @return a user's full name.
 */
const gchar *ifolder_user_get_full_name (iFolderUser *user);

const gchar *ifolder_user_get_first_name (iFolderUser *user);
const gchar *ifolder_user_get_last_name (iFolderUser *user);

//! Returns a user's iFolderMemberRights.
/**
 * @param user The iFolderUser.
 * @return a user's iFolderMemberRights.
 */
iFolderMemberRights ifolder_user_get_rights (iFolderUser *user);

//! Returns true if a user's login is enabled.
/**
 * @todo Should we have this call in the Client API?
 * 
 * @param user The iFolderUser.
 * @return true if a user's login is enabled.
 */
gboolean ifolder_user_is_login_enabled (iFolderUser *user);

//! Returns true if a user is an owner in an iFolder/Domain.
/**
 * @todo Should we have this call in the Client API?  What exactly would it
 * mean to the client if the user was the owner of a domain?
 * 
 * @param user The iFolderUser.
 * @return true if a user is an owner in an iFolder/Domain.
 */
gboolean ifolder_user_is_owner (iFolderUser *user);

const gchar * ifolder_user_get_email (iFolderUser *user);


gpointer ifolder_user_get_user_data (iFolderUser *user);
void ifolder_user_set_user_data (iFolderUser *user, gpointer user_data);

/*@}*/

G_END_DECLS

#ifdef __cplusplus
}
#endif		/* __cplusplus */

#endif /*_IFOLDER_C_USER_H_*/
