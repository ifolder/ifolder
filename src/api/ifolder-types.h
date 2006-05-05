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

#ifndef _IFOLDER_C_TYPES_H_
#define _IFOLDER_C_TYPES_H_

//! An object that represents a single iFolder.
/**
 * You must call ifolder_free() to clean up the memory used by an iFolder.
 */
typedef void *iFolder;

typedef void *iFolderChangeEntry;

//! An object that represents an iFolder Domain.
/**
 * Most of the functions require an iFolderDomain to be passed-in.
 * 
 * Create a new iFolderDomain by calling ifolder_domain_add().  Get a list of
 * existing iFolderDomain objects by calling ifolder_domain_get_all(),
 * ifolder_domain_get_all_active(), or ifolder_domain_get_default().
 * 
 * You must call ifolder_domain_free() after successfully calling
 * ifolder_domain_add() or ifolder_domain_get_default().
 */
typedef void *iFolderDomain;

typedef void *iFolderEnumeration;

typedef void *iFolderUserPolicy;

//! An object that represents a user.
/**
 * You must call ifolder_user_free() to clean up the memory used by an iFolderUser.
 */
typedef void *iFolderUser;

#endif /*_IFOLDER_C_TYPES_H_*/