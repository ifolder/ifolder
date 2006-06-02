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

#ifndef IFOLDER_DOMAIN_H
#define IFOLDER_DOMAIN_H

/* Headers that this header depends on. */
#include "ifolder-types.h"

#ifdef __cplusplus
extern "C"
{
#endif		/* __cplusplus */

G_BEGIN_DECLS

#define IFOLDER_DOMAIN_TYPE				(ifolder_domain_get_type())
#define IFOLDER_DOMAIN(obj)				(G_TYPE_CHECK_INSTANCE_CAST ((obj), IFOLDER_DOMAIN_TYPE, iFolderDomain))
#define IFOLDER_DOMAIN_CLASS(klass)		(G_TYPE_CHECK_CLASS_CAST ((klass), IFOLDER_DOMAIN_TYPE, iFolderDomainClass))
#define IFOLDER_IS_DOMAIN(obj)			(G_TYPE_CHECK_INSTANCE_TYPE ((obj), IFOLDER_DOMAIN_TYPE))
#define IFOLDER_IS_DOMAIN_CLASS(klass)	(G_TYPE_CHECK_CLASS_TYPE ((klass), IFOLDER_DOMAIN_TYPE))
#define IFOLDER_DOMAIN_GET_CLASS(obj)	(G_TYPE_INSTANCE_GET_CLASS ((obj), IFOLDER_DOMAIN_TYPE, iFolderDomainClass))

/* GObject support */
GType ifolder_domain_get_type (void) G_GNUC_CONST;

/**
 * Enumerations
 */
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
 * Method definitions
 */

/**
 * @name Properties (Getters and Setters)
 * 
 * @{
 */
const gchar *ifolder_domain_get_id (iFolderDomain *domain);
const gchar *ifolder_domain_get_name (iFolderDomain *domain);
const gchar *ifolder_domain_get_description (iFolderDomain *domain);
const gchar *ifolder_domain_get_version (iFolderDomain *domain);
const gchar *ifolder_domain_get_master_host (iFolderDomain *domain);
const gchar *ifolder_domain_get_home_host (iFolderDomain *domain);
const gchar *ifolder_domain_get_user_name (iFolderDomain *domain);
const gchar *ifolder_domain_get_user_id (iFolderDomain *domain);
gboolean ifolder_domain_is_authenticated (iFolderDomain *domain);
gboolean ifolder_domain_is_default (iFolderDomain *domain);
gboolean ifolder_domain_is_active (iFolderDomain *domain);
gpointer ifolder_domain_get_user_data (iFolderDomain *domain);
void ifolder_domain_set_user_data (iFolderDomain *domain, gpointer user_data);

/*@}*/

/**
 * @name Domain API
 * @{
 */

//! Log in to a domain.
/**
 * @param domain The domain.
 * @param password The user password.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
void ifolder_domain_log_in(iFolderDomain *domain, const char *password, gboolean remember_password, GError **error);

//! Log out of a domain.
/**
 * @param domain The domain.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
void ifolder_domain_log_out(iFolderDomain *domain, GError **error);

//! Activate a domain.
/**
 * If a domain is active, it will be included in the automatic synchronization
 * cycle.  Additionally, the client will periodically check for new available
 * iFolders on this domain.
 * @param domain The domain.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
void ifolder_domain_activate(iFolderDomain *domain, GError **error);

//! Inactivate a domain.
/**
 * This has the opposite effect of ifolder_domain_activate().
 * @param domain The domain.
 * @return IFOLDER_SUCCESS if the call was successful.
 * @see ifolder_domain_activate()
 */
void ifolder_domain_inactivate(iFolderDomain *domain, GError **error);

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
void ifolder_domain_change_host_address(iFolderDomain *domain, const char *new_host_address, GError **error);

//! Set the credentials for a domain.
/**
 * @param domain The domain.
 * @param password The password to set.
 * @param credential_type The type of credential this is.
 * @return IFOLDER_SUCCESS if the call was successful.
 * @see iFolderCredentialType 
 */
void ifolder_domain_set_credentials(iFolderDomain *domain, const char *password, const iFolderCredentialType credential_type, GError **error);

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
void ifolder_domain_set_default(iFolderDomain *domain, GError **error);

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
iFolderUser * ifolder_domain_get_authenticated_user(iFolderDomain *domain, GError **error);

//! Returns the iFolderUserPolicy associated with a domain.
/**
 * @param domain The domain.
 * @param user_policy Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
iFolderUserPolicy * ifolder_domain_get_authenticated_user_policy(iFolderDomain *domain, GError **error);

//! Checks for a newer version of the iFolder Client on the domain.
/**
 * @param domain The domain.
 * @param new_version Invalid if the call is unsuccessful.  If successful, this
 * will be the version of the newer client.  If no upgrade is available, this
 * will be NULL.
 * @param version_override If not specified or NULL, the version of libifolder
 * will be used.
 * @return TRUE if there is a new version of the client available.
 */
gboolean ifolder_domain_check_for_updated_client(iFolderDomain *domain, char **new_version, const char *version_override, GError **error);

/*@}*/

/**
 * @name User Management
 * @{
 */

//! Returns an iFolderUser object for a given user ID.
/**
 * @param domain The domain.
 * @param user_id_or_name The ID or user name of a user.
 * @param user Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
iFolderUser * ifolder_domain_get_user(iFolderDomain *domain, const gchar *user_id_or_name, GError **error);

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
iFolderUserIterator * ifolder_domain_get_users(iFolderDomain *domain, const int index, const int count, GError **error);

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
iFolderUserIterator * ifolder_domain_get_users_by_search(iFolderDomain *domain, const iFolderSearchProperty search_prop, const iFolderSearchOperation search_op, const gchar *pattern, const int index, const int count, GError **error);

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
iFolder * ifolder_domain_create_ifolder_from_path(iFolderDomain *domain, const char *local_path, const char *description, GError **error);

//! Create a new iFolder on the server only.
/**
 * @param domain The domain.
 * @param name The name of the iFolder to be created.
 * @param description The description of the iFolder to be created.
 * @param ifolder Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
iFolder * ifolder_domain_create_ifolder(iFolderDomain *domain, const gchar *name, const gchar *description, GError **error);

//! Delete an iFolder from a domain (from the server)
/**
 * @param domain The domain.
 * @param ifolder The iFolder to delete.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
void ifolder_domain_delete_ifolder(iFolderDomain *domain, iFolder *ifolder, GError **error);

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
void ifolder_domain_connect_ifolder(iFolderDomain *domain, iFolder *ifolder, const char *local_path, GError **error);

//! Disconnects an iFolder from its local file system path and from syncrhonization.
/**
 * @param domain The domain.
 * @param ifolder The iFolder to connect to a local path.  If successful, the
 * iFolder's type will be changed to #IFOLDER_TYPE_DISCONNECTED.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
void ifolder_domain_disconnect_ifolder(iFolderDomain *domain, iFolder *ifolder, GError **error);

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
GSList * ifolder_domain_get_connected_ifolders(iFolderDomain *domain, guint index, guint count, GError **error);

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
GSList * ifolder_domain_get_disconnected_ifolders(iFolderDomain *domain, guint index, guint count, GError **error);

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
iFolder * ifolder_domain_get_ifolder_by_id(iFolderDomain *domain, const char *id, GError **error);

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
GSList * ifolder_domain_get_ifolders_by_name(iFolderDomain *domain, const iFolderSearchOperation search_op, const char *pattern, const int index, const int count, GError **error);

/*@}*/

G_END_DECLS

#ifdef __cplusplus
}
#endif		/* __cplusplus */

#endif /* IFOLDER_DOMAIN_H */
