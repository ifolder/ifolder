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

#ifndef _IFOLDER_C_DOMAIN_H_
#define _IFOLDER_C_DOMAIN_H_

#include "ifolder.h"
#include "ifolder-user.h"
#include "ifolder-enumeration.h"

#include "ifolder-errors.h"

#ifdef __cplusplus
extern "C"
{
#endif		/* __cplusplus */

/**
 * @file ifolder-domain.h
 * @brief Domain API
 * 
 * @link domain_events_page Domain Events @endlink
 */

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

typedef enum
{
	IFOLDER_CREDENTIAL_TYPE_NONE,	/*!< Do not use credentials */
	IFOLDER_CREDENTIAL_TYPE_BASIC	/*!< Use HTTP Basic Authentication */
} iFolderCredentialType;

typedef enum
{
	IFOLDER_SEARCH_PROP_USER_NAME,	/*!< Search by the user name (login name) */
	IFOLDER_SEARCH_PROP_FULL_NAME,	/*!< Search by the user's full name */
	IFOLDER_SEARCH_PROP_FIRST_NAME,	/*!< Search by the user's first name */
	IFOLDER_SEARCH_PROP_LAST_NAME	/*!< Search by the user's last name */
} iFolderSearchProperty;

typedef enum
{
	IFOLDER_SEARCH_OP_BEGINS_WITH,	/*!< Match strings that begin with the search pattern */
	IFOLDER_SEARCH_OP_ENDS_WITH,		/*!< Match strings that end with the search pattern */
	IFOLDER_SEARCH_OP_CONTAINS,		/*!< Match strings that contain the search patter */
	IFOLDER_SEARCH_OP_EQUALS			/*!< Match strings that exactly equal the search pattern */
} iFolderSearchOperation;

/**
 * @name Properties (Getters and Setters)
 * 
 * @{
 */

//! Returns the domain's unique ID.
/**
 * @param domain The domain.
 * @return The unique ID.
 */
const char *ifolder_domain_get_id(const iFolderDomain domain);

//! Returns the domain's name.
/**
 * @param domain The domain.
 * @return The name.
 */
const char *ifolder_domain_get_name(const iFolderDomain domain);

//! Returns the domain's description.
/**
 * @param domain The domain.
 * @return The unique description.
 */
const char *ifolder_domain_get_description(const iFolderDomain domain);

//! Returns the domain's version.
/**
 * @param domain The domain.
 * @return The version.
 */
const char *ifolder_domain_get_version(const iFolderDomain domain);

//! Returns the domain's host address.
/**
 * @param domain The domain.
 * @return The host address.
 */
const char *ifolder_domain_get_host_address(const iFolderDomain domain);

//! Returns the domain's unique ID.
/**
 * @param domain The domain.
 * @return The unique ID.
 */
const char *ifolder_domain_get_machine_name(const iFolderDomain domain);

//! Returns the domain's operating system version.
/**
 * @param domain The domain.
 * @return The operating system version.
 */
const char *ifolder_domain_get_os_version(const iFolderDomain domain);

//! Returns the domain's user name.
/**
 * This is the user name that was used to connect to the domain.
 * 
 * @param domain The domain.
 * @return The user name.
 */
const char *ifolder_domain_get_user_name(const iFolderDomain domain);

//! Returns whether or not this domain is the default.
/**
 * @param domain The domain.
 * @return true if this domain is the default.
 */
bool ifolder_domain_get_is_default(const iFolderDomain domain);

//! Returns whether or not this domain is active.
/**
 * @param domain The domain.
 * @return true if this domain is active.
 */
bool ifolder_domain_get_is_active(const iFolderDomain domain);

//! Returns the domain's custom user data.
/**
 * This is a convenience function for the application developer to attach any
 * type of memory/data to the iFolderDomain.  The user data is not used
 * internally and should not affect the rest of the client.
 * 
 * If this variable is used, ifolder_domain_free() does not free the memory
 * used by user data.  It should be freed by the caller independently.
 * 
 * @param domain The domain.
 * @return The user data.
 */
void *ifolder_domain_get_user_data(const iFolderDomain domain);

//! Sets the domain's custom user data.
/**
 * @param domain The domain.
 * @param user_data the user data to set on the domain object.
 * @see ifolder_domain_get_user_data
 */
void ifolder_domain_set_user_data(const iFolderDomain domain, void *user_data);

/*@}*/

/**
 * @name Domain API
 * @{
 */

//! Add and connect to a new domain.
/**
 * @param host_address The host address to connect to.
 * @param user_name The user name for the domain.
 * @param password The user's password.
 * @param make_default Set to true if this should be marked as the default
 * account.  If there are no other domains on the client, this parameter will
 * be ignored.  The client will ensure there is always a default account if one
 * or more accounts exist.
 * @param domain The newly created iFolderDomain if this function is
 * successful.  If there are errors calling this function, the value of this
 * parameter will not be valid.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_domain_add(const char *host_address, const char *user_name, const char *password, const bool make_default, iFolderDomain *domain);

//! Remove and disconnect from a domain.
/**
 * @param domain The domain.
 * @param delete_ifolders_on_server Set to true if this function should also
 * remove all of the user's iFolders on the server.  If true and the domain
 * cannot communicate with the server, and error will be returned.  If the user
 * still wants to remove the domain, you must call this function again with
 * this parameter set to false.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_domain_remove(const iFolderDomain domain, const bool delete_ifolders_on_server);

//! Log in to a domain.
/**
 * @param domain The domain.
 * @param password The user password.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_domain_log_in(const iFolderDomain domain, const char *password);

//! Log out of a domain.
/**
 * @param domain The domain.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_domain_log_out(const iFolderDomain domain);

//! Activate a domain.
/**
 * If a domain is active, it will be included in the automatic synchronization
 * cycle.  Additionally, the client will periodically check for new available
 * iFolders on this domain.
 * @param domain The domain.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_domain_activate(const iFolderDomain domain);

//! Inactivate a domain.
/**
 * This has the opposite effect of ifolder_domain_activate().
 * @param domain The domain.
 * @return IFOLDER_SUCCESS if the call was successful.
 * @see ifolder_domain_activate()
 */
int ifolder_domain_inactivate(const iFolderDomain domain);

//! Change the host address of a domain.
/**
 * If the address (DNS Name or IP Address) of the iFolder Server that was
 * originally used to add this domain has changed, this function should be
 * called to update the address.
 * 
 * This call will fail if the client is unable to communicate with the iFolder
 * Server specified by new_host_address.
 * 
 * @param domain The domain.
 * @param new_host_address
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_domain_change_host_address(const iFolderDomain domain, const char *new_host_address);

//! Set the credentials for a domain.
/**
 * @param domain The domain.
 * @param password The password to set.
 * @param credential_type The type of credential this is.
 * @return IFOLDER_SUCCESS if the call was successful.
 * @see iFolderCredentialType 
 */
int ifolder_domain_set_credentials(const iFolderDomain domain, const char *password, const iFolderCredentialType credential_type);

//! Set a domain as the default.
/**
 * This call has no effect if the specified domain is the only domain existing
 * on the client.  If there is more than one domain and the specified domain is
 * not the default domain, the specified domain will be marked as the default
 * and the old one will no longer be the default.
 * 
 * @param domain The domain.
 * @return IFOLDER_SUCCESS if the call was successful.
 * @see ifolder_domain_add()
 */
int ifolder_domain_set_default(const iFolderDomain domain);

//! Returns the authenticated iFolderUser object for a domain.
/**
 * The authenticated user is the one that was used to add/connect the domain
 * initially.
 * 
 * @param domain The domain.
 * @param user Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 * @see ifolder_domain_add()
 */
int ifolder_domain_get_authenticated_user(const iFolderDomain domain, iFolderUser *user);

//! Returns the iFolderUserPolicy associated with a domain.
/**
 * @param domain The domain.
 * @param user_policy Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_domain_get_authenticated_user_policy(const iFolderDomain domain, iFolderUserPolicy *user_policy);

//! Checks for a newer version of the iFolder Client on the domain.
/**
 * @param domain The domain.
 * @param new_version Invalid if the call is unsuccessful.  If successful, this
 * will be the version of the newer client.  If no upgrade is available, this
 * will be NULL.
 * @param version_override If not specified or NULL, the version of libifolder
 * will be used.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_domain_check_for_updated_client(const iFolderDomain domain, char **new_version, const char *version_override = NULL);

//! Free the memory used by an iFolderDomain
/**
 * This should be called any time you get an iFolderDomain as a returned
 * item from a function.  You do NOT need to call this when an
 * iFolderDomain is part of an iFolderEnumeration.
 */
void ifolder_domain_free(iFolderDomain domain);

/*@}*/

/**
 * @name Domains API
 * @{
 */

//! Returns all domains configured on the client.
/**
 * If no domains exist, an empty enumeration will be returned.
 * 
 * @param domain_enum An enumeration of iFolderDomain objects.  Invalid if the
 * call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_domain_get_all(iFolderEnumeration *domain_enum);

//! Returns all active domains configured on the client.
/**
 * This function returns a subset of the ifolder_domain_get_all() call.
 * 
 * If no domains exist, an empty enumeration will be returned.
 * 
 * @param domain_enum An enumeration of iFolderDomain objects.  Invalid if the
 * call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_domain_get_all_active(iFolderEnumeration *domain_enum);

//! Returns the default domain.
/**
 * If a no domain exists, domain will be set to NULL.
 * 
 * @param domain The default domain or NULL if no domain exists.  Invalid if
 * the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_domain_get_default(iFolderDomain *domain);

/*@}*/

/**
 * @name User Management
 * @{
 */

//! Returns an iFolderUser object for a given user ID.
/**
 * @param domain The domain.
 * @param user_id The ID of the desired user.
 * @param user Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_domain_get_user(const iFolderDomain domain, const char *user_id, iFolderUser *user);

//! Returns a subset of iFolderUser domain members beginning at the specified index.
/**
 * @param domain The domain.
 * @param index The index of where the user enumeration should begin.  This
 * must be greater than 0.  An empty list will be returned if the index is
 * greater than the total number of users available.
 * @param count The number of iFolderUser objects to return.  This must be
 * at least 1 or greater.
 * @param user_enum Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_domain_get_users(const iFolderDomain domain, const int index, const int count, iFolderEnumeration *user_enum);

//! Returns a subset of iFolderUser domain members by searching, beginning at the specified index.
/**
 * @param domain The domain.
 * @param search_prop The iFolderSearchProperty to use in the search.
 * @param search_op The iFolderSearchOperation to use in the search.
 * @param pattern The pattern to use in the search.
 * @param index The index of where the user enumeration should begin.  This
 * must be greater than 0.  An empty list will be returned if the index is
 * greater than the total number of users available.
 * @param count The number of iFolderUser objects to return.  This must be
 * at least 1 or greater.
 * @param user_enum Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_domain_get_users_by_search(const iFolderDomain domain, const iFolderSearchProperty search_prop, const iFolderSearchOperation search_op, const char *pattern, const int index, const int count, iFolderEnumeration *user_enum);

/*@}*/

/**
 * @name iFolder Management
 * @{
 */

//! Create a new iFolder from a local file system path
/**
 * This will create a new iFolder and connect it with the local file system
 * path for synchronization.
 * 
 * The name of the folder will be taken from the name of the local file system
 * folder/directory specified by local_path.
 * 
 * @param domain The domain.
 * @param local_path The local file system path to a folder/directory.  This
 * must be a path where the user has read/write access and ...
 * @todo List the requirements of a local file system path being able to be converted to an iFolder
 * @param description The description of the iFolder to be created.
 * @param ifolder Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_domain_create_ifolder_from_path(const iFolderDomain domain, const char *local_path, const char *description, iFolder *ifolder);

//! Create a new iFolder on the server only.
/**
 * @param domain The domain.
 * @param name The name of the iFolder to be created.
 * @param description The description of the iFolder to be created.
 * @param ifolder Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_domain_create_ifolder(const iFolderDomain domain, const char *name, const char *description, iFolder *ifolder);

//! Delete an iFolder from a domain (from the server)
/**
 * @param domain The domain.
 * @param ifolder The iFolder to delete.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_domain_delete_ifolder(const iFolderDomain domain, const iFolder ifolder);

//! Connects an iFolder to a local file system path for synchronization.
/**
 * @param domain The domain.
 * @param ifolder The iFolder to connect to a local path.  If successful, the
 * iFolder's type will be changed to #IFOLDER_TYPE_CONNECTED.
 * @param local_path The local file system path where the iFolder should be
 * synchronized.  If this is an existing folder/directory, the contents of the
 * local folder will be synchronized and merged into the iFolder.  If the
 * folder/directory does not already exist, it will be created and the contents
 * of the iFolder will be synchronized to this folder.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_domain_connect_ifolder(const iFolderDomain domain, iFolder ifolder, const char *local_path);

//! Disconnects an iFolder from its local file system path and from syncrhonization.
/**
 * @param domain The domain.
 * @param ifolder The iFolder to connect to a local path.  If successful, the
 * iFolder's type will be changed to #IFOLDER_TYPE_DISCONNECTED.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_domain_disconnect_ifolder(const iFolderDomain domain, iFolder ifolder);

//! Returns a domain's connected iFolders (iFolders configured to synchronize locally).
/**
 * @param domain The domain.
 * @param index The index of where the ifolder enumeration should begin.  This
 * must be greater than 0.  An empty list will be returned if the index is
 * greater than the total number of ifolders.
 * @param count The number of iFolder objects to return.  This must be at least
 * 1 or greater.
 * @param ifolder_enum Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_domain_get_connected_ifolders(const iFolderDomain domain, const int index, const int count, iFolderEnumeration *ifolder_enum);

//! Returns a domain's disconnected iFolders (iFolders on the server).
/**
 * @param domain The domain.
 * @param index The index of where the ifolder enumeration should begin.  This
 * must be greater than 0.  An empty list will be returned if the index is
 * greater than the total number of ifolders.
 * @param count The number of iFolder objects to return.  This must be at least
 * 1 or greater.
 * @param ifolder_enum Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_domain_get_disconnected_ifolders(const iFolderDomain domain, const int index, const int count, iFolderEnumeration *ifolder_enum);

//! Returns an iFolder for the specified iFolder ID.
/**
 * This call may return either a connected or disconnected iFolder.
 * 
 * @param domain The domain.
 * @param id The ID of an iFolder.
 * @param ifolder Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 * @see ifolder_get_type()
 */
int ifolder_domain_get_ifolder_by_id(const iFolderDomain domain, const char *id, iFolder *ifolder);

//! Returns a subset of iFolders for a domain by searching, beginning at the specified index.
/**
 * This call may return both connected and disconnected iFolders.
 * 
 * @param domain The domain.
 * @param search_op The iFolderSearchOperation to use in the search.
 * @param pattern The pattern to use in the search.
 * @param index The index of where the iFolderEnumeration should begin.  This
 * must be greater than 0.  An empty list will be returned if the index is
 * greater than the total number of iFolders available.
 * @param count The maximum number of iFolder objects to return.  This must be
 * at least 1 or greater.
 * @param ifolder_enum Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 * @see ifolder_get_type()
 */
int ifolder_domain_get_ifolders_by_name(const iFolderDomain domain, const iFolderSearchOperation search_op, const char *pattern, const int index, const int count, iFolderEnumeration *ifolder_enum);

/*@}*/

/** @page domain_events_page Domain Events

@events
 @event domain-added
 @event domain-removed
 @event domain-host-modified
 @event domain-logged-in
 @event domain-logged-out
 @event domain-needs-credentials
 @event domain-activated
 @event domain-inactivated
 @event domain-new-default
 @event domain-in-grace-login-period
@endevents

<hr>

@eventdef domain-added
 @eventproto
void (*domain_added)(const iFolderDomain domain);
 @endeventproto
 @eventdesc
  Emitted when an domain is added to the client.
 @param domain The domain that was added.
@endeventdef

@eventdef domain-removed
 @eventproto
void (*domain_removed)(const iFolderDomain domain);
 @endeventproto
 @eventdesc
  Emitted when an domain is removed from the client.
 @param domain The domain that was removed.
@endeventdef

@eventdef domain-host-modified
 @eventproto
void (*domain_host_modified)(const iFolderDomain domain);
 @endeventproto
 @eventdesc
  Emitted when the host address of a domain is modified.
 @param domain The domain that was modified.
@endeventdef

@eventdef domain-logged-in
 @eventproto
void (*domain_logged_in)(const iFolderDomain domain);
 @endeventproto
 @eventdesc
  Emitted when a domain just logged in.
 @param domain The domain that logged in.
@endeventdef

@eventdef domain-logged-out
 @eventproto
void (*domain_logged_out)(const iFolderDomain domain);
 @endeventproto
 @eventdesc
  Emitted when a domain just logged out.
 @param domain The domain that logged out.
@endeventdef

@eventdef domain-needs-credentials
 @eventproto
void (*domain_needs_credentials)(const iFolderDomain domain);
 @endeventproto
 @eventdesc
  Emitted when the client needs credentials for a domain.
 @param domain The domain that needs credentials.
@endeventdef

@eventdef domain-activated
 @eventproto
void (*domain_activated)(const iFolderDomain domain);
 @endeventproto
 @eventdesc
  Emitted when a domain is activated.
 @param domain The domain that was activated.
@endeventdef

@eventdef domain-inactivated
 @eventproto
void (*domain_inactivated)(const iFolderDomain domain);
 @endeventproto
 @eventdesc
  Emitted when a domain is inactivated.
 @param domain The domain that was inactivated.
@endeventdef

@eventdef domain-new-default
 @eventproto
void (*domain_new_default)(const iFolderDomain old_default, const iFolderDomain new_default);
 @endeventproto
 @eventdesc
  Emitted when a domain is marked as the new default.
 @param old_default The old default domain or NULL if the the domain is not available (got deleted or new_default is the first domain to be added).
 @param new_default The new default domain.
@endeventdef

@eventdef domain-in-grace-login-period
 @eventproto
void (*domain_in_grace_login_period)(const iFolderDomain domain, const int remaining);
 @endeventproto
 @eventdesc
  Emitted when a user's account on the domain is in its grace login period.
 @param domain The domain which is in the grace login period.
 @param remaining The number of grace logins remaining.
@endeventdef

*/

#ifdef __cplusplus
}
#endif		/* __cplusplus */

#endif /*_IFOLDER_C_DOMAIN_H_*/
