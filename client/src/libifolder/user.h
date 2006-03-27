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

//! User API
/**
 *  @file user.h
 *
 *  FIXME: Add detailed documentation for user.h
 */

#ifndef _IFOLDER_USER_H
/* @cond */
#define _IFOLDER_USER_H 1
/* @endcond */

/**
 * This object is used to represent an iFolder User.  The functions
 * inside this file require an iFolderUser object.  When you are done
 * using an iFolderUser object, it should be freed with @a ifolder_user_release.
 * @see ifolder_user_release
 */
typedef void * iFolderUser;

/**
 * Release/free the memory used by an @a iFolderUser object.
 *
 * @param ifolder_user A @a pointer to an @a iFolderUser object
 * to be released/freed.
 * @returns Returns IFOLDER_SUCESS if there were no errors.
 */
int ifolder_user_release(iFolderUser *ifolder_user);

#endif
