/***********************************************************************
 *  $RCSfile$
 *
 *  Gaim iFolder Plugin: Allows Gaim users to share iFolders.
 *  Copyright (C) 2005 Novell, Inc.
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
 *  Author: Boyd Timothy <btimothy@novell.com>
 * 
 *  Some code in this file (mostly the saving and reading of the XML files) is
 *  directly based on code found in Gaim's core & plugin files, which is
 *  distributed under the GPL.
 ***********************************************************************/

#ifndef _GAIM_DOMAIN_H
#define _GAIM_DOMAIN_H 1

/* GLib/Gtk Includes */
#include <glib.h>

int simias_get_local_service_url(char **url);

void simias_sync_member_list();
void simias_update_member(const char *account_name, const char *account_prpl_id,
						  const char *buddy_name, const char *machine_name);

/**
 * Gets the machineName, userID, and simiasURL for the current Gaim Domain Owner.
 *
 * This method returns 0 on success.  If success is returned, the machineName,
 * userID, and simiasURL parameters will have had new strings allocated to them
 * and need to be freed.  If there is an error, the output parameters are
 * invalid and do not need to be freed.
 */
int simias_get_user_info(char **machineName, char **userID, char **simiasURL);

/**
 * Gets the machineName.
 *
 * This method returns 0 on success.  If success is returned, the machineName
 * will contain the machineName in a newly allocated char * and will need to be
 * freed.  If there is an error machineName will be invalid and does not need
 * to be freed.
 */
int simias_get_machine_name(char **machineName);

/**
 * Gets the user public key that should be used.  This function first checks
 * the custom plugin setting to see if we already have a public/private key
 * stored in the Gaim configuration.  If so, it just returns the public key
 * directly from the configuration.  If the public key is not in the Gaim
 * configuration, it will call the GaimDomainService WebService to get the key
 * and will store it in the Gaim configuration for future calls.
 *
 * This method returns 0 on success.  If success is returned, the public_key
 * will have a newly allocated char * that should be freed by the caller.  If
 * there is an error, public_key will be invalid and does not need to be freed.
 */
int simias_get_public_key(char **public_key);

/**
 * Gets the user private key that should be used.  This function first checks
 * the custom plugin setting to see if we already have a public/private key
 * stored in the Gaim configuration.  If so, it just returns the private key
 * directly from the configuration.  If the private key is not in the Gaim
 * configuration, it will call the GaimDomainService WebService to get the key
 * pair and will store it in the Gaim configuration for future calls.
 *
 * This method returns 0 on success.  If success is returned, the private_key
 * will have a newly allocated char * that should be freed by the caller.  If
 * there is an error, private_key will be invalid and does not need to be freed.
 */
int simias_get_private_key(char **private_key);

/**
 * Uses rsaCryptoXml (.NET XML String for a RSACryptoServiceProvider), to
 * encrypt unencrypted_string.  Returns 0 if successful, in which case
 * encrypted_string will be valid and needs to be freed.  If there was an
 * error, the function returns a negative int and the encrypted_string is
 * invalid and does not need to be freed.
 */
int simias_rsa_encrypt_string(const char *rsaCryptoXml, const char *unencrypted_string, char **encrypted_string);

/**
 * Uses rsaCryptoXml (.NET XML String for a RSACryptoServiceProvider), to
 * decrypt encrypted_string.  Returns 0 if successful, in which case
 * decrypted_string will be valid and needs to be freed.  If there was an
 * error, the function returns a negative int and the decrypted_string is
 * invalid and does not need to be freed.
 */
int simias_rsa_decrypt_string(const char *rsaCryptoXml, const char *encrypted_string, char **decrypted_string);

/**
 * Gets the user's Base64 Encoded DES key.  This function first checks the
 * plugin setting in Gaim prefs.xml.  If so, it just returns that key.
 * Otherwise it will call the GaimDomain WebService to generate a DES key that
 * will be stored and used from the Gaim iFolder Plugin.
 *
 * This method returns 0 on success.  If success is returned, the des_key
 * will have a newly allocated char * that should be freed by the caller.  If
 * there is an error, private_key will be invalid and does not need to be freed.
 */
int simias_get_des_key(char **des_key);

/**
 * Uses desKey to encrypt unencrypted_string.  The encrypted_string is returned
 * Base64 encoded.
 *
 * Returns 0 if successful, in which case encrypted_string will be valid and
 * needs to be freed.  If there was an error, the function returns a negative
 * int and the encrypted_string is invalid and does not need to be freed.
 */
int simias_des_encrypt_string(const char *desKey, const char *unencrypted_string, char **encrypted_string);

/**
 * Uses desKey to decrypt encrypted_string, which is also Base64 encoded.
 *
 * Returns 0 if successful, in which case decrypted_string will be valid and
 * needs to be freed.  If there was an error, the function returns a negative
 * int and the decrypted_string is invalid and does not need to be freed.
 */
int simias_des_decrypt_string(const char *desKey, const char *encrypted_string, char **decrypted_string);

#endif
