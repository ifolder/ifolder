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
#ifndef _ACCOUNTS_H
#define _ACCOUNTS_H 1

/**
 * This represents an iFolder Account.
 */
typedef void * iFolderAccount;

/**
 * This is returned when a function can return multiple accounts.
 */
typedef void * ArrayOfiFolderAccount;

/**
 * Use this function to create a new iFolderAccount object in memory.
 */
int ifolder_account_new(const char *server_address, iFolderAccount **account);
/**
 * Use this function to free an iFolderAccount object from memory.
 */
int ifolder_account_free(iFolderAccount **account);

int ifolder_account_join(iFolderAccount *account);
int ifolder_account_leave(iFolderAccount *account);
int ifolder_account_login(iFolderAccount *account, const char *user_name, const char *password);
int ifolder_account_logout(iFolderAccount *account);

#endif
