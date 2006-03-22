/***********************************************************************
 *  $RCSfile$
 * 
 * @file account.h Account API
 * @ingroup core
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
#ifndef _IFOLDER_ACCOUNTS_H
/* @cond */
#define _IFOLDER_ACCOUNTS_H 1
/* @endcond */

/*!
 * This represents an iFolder Account.
 *
 * How about a detailed description of the typedef?
 */
typedef void * iFolderAccount;

/**
 * Use this function to create a new iFolderAccount object in memory.
 *
 * Detailed description of ifolder_account_new.
 */
int ifolder_account_new(const char *server_address, iFolderAccount *account);
/**
 * Use this function to free an iFolderAccount object from memory.
 */
int ifolder_account_free(iFolderAccount *account);

/**
 * Get the ID of the @a account.
 *
 * @param account The iFolderAccount to get information from.
 * @returns A char *.  This does not need to be explicitly freed.
 * However, if you want to use this char * somewhere else in your
 * code, you should make a copy of it using something like strdup().
 * The memory used by the char * is freed when you call
 * ifolder_account_release() on the iFolderAccount.
 */
const char * ifolder_account_get_id(iFolderAccount account);

/**
 * Get the name of the @a account.
 *
 * @param account The iFolderAccount to get information from.
 * @returns A char *.  This does not need to be explicitly freed.
 * However, if you want to use this char * somewhere else in your
 * code, you should make a copy of it using something like strdup().
 * The memory used by the char * is freed when you call
 * ifolder_account_release() on the iFolderAccount.
 */
const char * ifolder_account_get_name(iFolderAccount account);

/**
 * Get the description of the @a account.
 *
 * @param account The iFolderAccount to get information from.
 * @returns A char *.  This does not need to be explicitly freed.
 * However, if you want to use this char * somewhere else in your
 * code, you should make a copy of it using something like strdup().
 * The memory used by the char * is freed when you call
 * ifolder_account_release() on the iFolderAccount.
 */
const char * ifolder_account_get_description(iFolderAccount account);

/**
 * Get the version of the server that the @a account is connected to.
 *
 * @param account The iFolderAccount to get information from.
 * @returns A char *.  This does not need to be explicitly freed.
 * However, if you want to use this char * somewhere else in your
 * code, you should make a copy of it using something like strdup().
 * The memory used by the char * is freed when you call
 * ifolder_account_release() on the iFolderAccount.
 */
const char * ifolder_account_get_version(iFolderAccount account);

/**
 * Get the host name of the server that the @a account is connected to.
 *
 * @param account The iFolderAccount to get information from.
 * @returns A char *.  This does not need to be explicitly freed.
 * However, if you want to use this char * somewhere else in your
 * code, you should make a copy of it using something like strdup().
 * The memory used by the char * is freed when you call
 * ifolder_account_release() on the iFolderAccount.
 */
const char * ifolder_account_get_host_name(iFolderAccount account);

/**
 * Get the machine name of the server that the @a account is connected to.
 *
 * @param account The iFolderAccount to get information from.
 * @returns A char *.  This does not need to be explicitly freed.
 * However, if you want to use this char * somewhere else in your
 * code, you should make a copy of it using something like strdup().
 * The memory used by the char * is freed when you call
 * ifolder_account_release() on the iFolderAccount.
 */
const char * ifolder_account_get_machine_name(iFolderAccount account);

/**
 * Get the operating system version of the server that the @a account is connected to.
 *
 * @param account The iFolderAccount to get information from.
 * @returns A char *.  This does not need to be explicitly freed.
 * However, if you want to use this char * somewhere else in your
 * code, you should make a copy of it using something like strdup().
 * The memory used by the char * is freed when you call
 * ifolder_account_release() on the iFolderAccount.
 */
const char * ifolder_account_get_os_version(iFolderAccount account);

/**
 * Get the user name of the @a account.
 *
 * @param account The iFolderAccount to get information from.
 * @returns A char *.  This does not need to be explicitly freed.
 * However, if you want to use this char * somewhere else in your
 * code, you should make a copy of it using something like strdup().
 * The memory used by the char * is freed when you call
 * ifolder_account_release() on the iFolderAccount.
 */
const char * ifolder_account_get_user_name(iFolderAccount account);

int ifolder_account_join(iFolderAccount account);
int ifolder_account_leave(iFolderAccount account);

/**
 * Attempt to log in to the specified account with the specified user_name and password.
 * @param account the iFolderAccount to log in to.
 * @param user_name the user name for this account.
 * @param password the password for this account.
 * @see IFOLDER_SUCCESS
 * @see ifolder_account_logout
 * @return Returns IFOLDER_SUCCESS if the login was successful, otherwise it returns an IFOLDER_ERROR_*.
 */
int ifolder_account_login(iFolderAccount account, const char *user_name, const char *password);
int ifolder_account_logout(iFolderAccount account);

int ifolder_account_get_users(iFolderAccount account, iFolderEnumeration *user_enum);
int ifolder_account_get_users_by_search(iFolderAccount,
										enum iFolderSearchProperty search_property,
										enum iFolderSearchOperation search_operation,
										const char *pattern,
										int index,
										int count,
										iFolderEnumeration *user_enum);

#endif
