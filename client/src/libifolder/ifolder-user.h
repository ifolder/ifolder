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

#ifdef __cplusplus
extern "C"
{
#endif		/* __cplusplus */

#include "ifolder.h"

/**
 * @file ifolder-user.h
 * @brief User API
 */

//! An object that represents a user.
/**
 * You must call ifolder_user_free() to clean up the memory used by an iFolderUser.
 */
typedef void *iFolderUser;

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
const char *ifolder_user_get_id(iFolderUser user);

//! Returns a user's login name.
/**
 * @param user The iFolderUser.
 * @return a user's login name.
 */
const char *ifolder_user_get_user_name(iFolderUser user);

//! Returns a user's full name.
/**
 * @param user The iFolderUser.
 * @return a user's full name.
 */
const char *ifolder_user_get_full_name(iFolderUser user);

//! Returns a user's iFolderMemberRights.
/**
 * @param user The iFolderUser.
 * @return a user's iFolderMemberRights.
 */
iFolderMemberRights ifolder_user_get_rights(iFolderUser user);

//! Returns true if a user's login is enabled.
/**
 * @todo Should we have this call in the Client API?
 * 
 * @param user The iFolderUser.
 * @return true if a user's login is enabled.
 */
bool ifolder_user_is_login_enabled(iFolderUser user);

//! Returns true if a user is an owner in an iFolder/Domain.
/**
 * @todo Should we have this call in the Client API?  What exactly would it
 * mean to the client if the user was the owner of a domain?
 * 
 * @param user The iFolderUser.
 * @return true if a user is an owner in an iFolder/Domain.
 */
bool ifolder_user_is_owner(iFolderUser user);

//! Free the memory used by an iFolderUser.
/**
 * @param user The iFolderUser.
 */
void ifolder_user_free(iFolderUser user);

/*@}*/

#ifdef __cplusplus
}
#endif		/* __cplusplus */

#endif /*_IFOLDER_C_USER_H_*/
