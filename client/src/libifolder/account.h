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

//! Accounts API
/**
 *  @file account.h
 *
 *  FIXME: Add documentation for account.h
 */

#ifndef _IFOLDER_ACCOUNTS_H
/* @cond */
#define _IFOLDER_ACCOUNTS_H 1
/* @endcond */

#include "user.h"
#include "enumeration.h"

//! An object that represents an iFolder Account.
/**
 * FIXME: Add detailed documentation for iFolderAccount.
 */
typedef void * iFolderAccount;

//! An enum used by ifolder_account_get_users_by_search().
/**
 * View the ifolder_account_get_users_by_search() documentation for more details.
 */
typedef enum
{
	IFOLDER_SEARCH_PROPERTY_USER_NAME,	/*!< Search on the user name property. */
	IFOLDER_SEARCH_PROPERTY_NAME,		/*!< Search on the full name property. */
	IFOLDER_SEARCH_PROPERTY_FIRST_NAME,	/*!< Search on the first name property. */
	IFOLDER_SEARCH_PROPERTY_LAST_NAME	/*!< Search on the last name property. */
} iFolderSearchProperty;

//! An enum used by ifolder_account_get_users_by_search().
/**
 * View the ifolder_account_get_users_by_search() documentation for more details.
 */
typedef enum
{
	IFOLDER_SEARCH_OP_BEGINS_WITH,	/*!< Match the beginning of the specified property. */
	IFOLDER_SEARCH_OP_ENDS_WITH,	/*!< Match the end of the specified property. */
	IFOLDER_SEARCH_OP_CONTAINS,		/*!< Match any part of the specified property. */
	IFOLDER_SEARCH_OP_EQUALS		/*!< Match the entire specified property. */
} iFolderSearchOperation;

/**
 * Use this function to create a new iFolderAccount object in memory.
 *
 * Detailed description of ifolder_account_new.
 */
int ifolder_account_new(const char *server_address, iFolderAccount *account);
/**
 * Use this function to free an iFolderAccount object from memory.
 */
int ifolder_account_release(iFolderAccount *account);

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

int ifolder_account_get_authenticated_user(iFolderAccount account, iFolderUser *user);

/**
 * Get an @a iFolderUser object from a specified @a user_id.
 *
 * @param account FIXME: Add documentation.
 * @param user_id A user id (usually the user's login name).
 * @param ifolder_user If successful, this variable is set to the
 * @a iFolderUser object that represents the specified user.
 * @returns Returns IFOLDER_SUCESS if there were no errors.
 */
int ifolder_get_user(iFolderAccount account, const char *user_id, iFolderUser *ifolder_user);

//! Get an enumeration of users for the specified iFolderAccount.
/**
 * FIXME: Add detailed documentation on ifolder_account_get_users.
 *
 * @param account The iFolderAccount to search for users on.
 * @param index The index of the first iFolderChangeEntry to return.
 * @param count The maximum number of iFolderChangeEntry objects
 * @param user_enum If successful, this will contain an enumeration of
 * iFolderUser objects.  Make sure to call ifolder_enumeration_release()
 * when you are finished using the data in the enumeration.  You don't
 * need to call ifolder_user_release on each item in the enumeration.
 * This is done automatically in ifolder_enumeration_release().
 * @returns IFOLDER_SUCCESS if the call was successful.
 * @see ifolder_account_get_users_by_search
 */
int ifolder_account_get_users(iFolderAccount account, int index, int count, iFolderEnumeration *user_enum);

//! Get an enumeration of users for the specified iFolderAccount.
/**
 * FIXME: Add detailed documentation on ifolder_account_get_users.
 *
 * @param account The iFolderAccount to search for users on.
 * @param search_prop The user property to search.
 * @param search_op The type of search operation to use.
 * @param pattern The pattern to match in the search.
 * @param index The index of the first iFolderChangeEntry to return.
 * @param count The maximum number of iFolderChangeEntry objects
 * @param user_enum If successful, this will contain an enumeration of
 * iFolderUser objects.  Make sure to call ifolder_enumeration_release()
 * when you are finished using the data in the enumeration.  You don't
 * need to call ifolder_user_release on each item in the enumeration.
 * This is done automatically in ifolder_enumeration_release().
 * @returns IFOLDER_SUCCESS if the call was successful.
 * @see ifolder_account_get_users
 */
int ifolder_account_get_users_by_search(iFolderAccount account,
										iFolderSearchProperty search_prop,
										iFolderSearchOperation search_op,
										const char *pattern,
										int index,
										int count,
										iFolderEnumeration *user_enum);

#endif
