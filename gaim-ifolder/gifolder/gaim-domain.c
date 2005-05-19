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

#include "gaim-domain.h"

/* Gaim Includes */
#include "account.h"
#include "blist.h"
#include "util.h"

#include "simiasgaimStub.h"
#include "simiasgaim.nsmap"

#include <stdlib.h>
#include <stdio.h>
#include <string.h>

#include "simias-util.h"
#include "simias-prefs.h"

#if defined(WIN32)
#define DIR_SEP "\\"
#else
#define DIR_SEP "/"
#endif

/* Global Variables */
static char *the_soap_url = NULL;

/* Forward Declarations of Static Functions */
static char *simias_get_user_profile_dir_path(char *dest_path);
static char *parse_local_service_url(FILE *file);
static char *parse_web_service_password(FILE *file);

/**
 * Retrieves the RSACryptoProvider (public and private keys) for the first time
 * from Simias and stores them as a plugin setting in Gaim.
 */
static int setup_keypair();

/**
 * Retrieves a new Base64 Encoded DES Key from GaimDomainService.asmx and
 * stores them as a plugin setting in Gaim.
 */
static int setup_des_key();

static void init_gsoap (struct soap *p_soap);
static void cleanup_gsoap (struct soap *p_soap);
static char *get_soap_url(gboolean reread_config);

/* Function Implementation */
/**
 * The purpose of this function is to have the Gaim iFolder Plugin call the
 * GaimDomainService (WebService) to wake it up and have it synchronize the
 * memberlist.  This will be called either when the plugin gets a new/updated
 * PO Box URL for a buddy that just signed on or...when the user presses the
 * "Synchronize Now" button from the preferences page.
 */
void
simias_sync_member_list()
{
	char *soap_url;
	struct soap soap;
	struct _ns1__SynchronizeMemberList req;
	struct _ns1__SynchronizeMemberListResponse resp;
	char username[512];
	char password[1024];
	
	soap_url = get_soap_url(TRUE);
	if (!soap_url) {
		return;
	}
	
	init_gsoap(&soap);
	if (simias_get_web_service_credential(username, password) == 0) {
		soap.userid = username;
		soap.passwd = password;
	}
	soap_call___ns1__SynchronizeMemberList(&soap, soap_url, NULL, &req, &resp);
	if (soap.error) {
		cleanup_gsoap(&soap);
		return;
	}
	
	cleanup_gsoap(&soap);
}

void
simias_update_member(const char *account_name, const char *account_prpl_id,
					 const char *buddy_name, const char *machine_name)
{
	char *soap_url;
	struct soap soap;
	struct _ns1__UpdateMember req;
	struct _ns1__UpdateMemberResponse resp;
	char username[512];
	char password[1024];
	
	soap_url = get_soap_url(TRUE);
	if (!soap_url) {
		return;
	}
	
	/* Setup the Request */
	req.AccountName = (char *)account_name;
	req.AccountProtocolID = (char *)account_prpl_id;
	req.BuddyName = (char *)buddy_name;
	req.MachineName = (char *)machine_name;
	
	init_gsoap(&soap);
	if (simias_get_web_service_credential(username, password) == 0) {
		soap.userid = username;
		soap.passwd = password;
	}
	soap_call___ns1__UpdateMember(&soap, soap_url, NULL, &req, &resp);
	if (soap.error) {
		cleanup_gsoap(&soap);
		return;
	}
	
	cleanup_gsoap(&soap);
}

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
int
simias_get_public_key(char **public_key)
{
	const char *existing_public_key;
	int err;
	
	if (gaim_prefs_exists(SIMIAS_PREF_PUBLIC_KEY))
	{
		existing_public_key = gaim_prefs_get_string(SIMIAS_PREF_PUBLIC_KEY);
		if (existing_public_key)
		{
			*public_key = strdup(existing_public_key);
			return 0;
		}
		else
		{
			return -1;
		}
	}


	err = setup_keypair();
	if (err != 0)
	{
		fprintf(stderr, "There was an error retrieving the public/private key pair from Simias.  Is iFolder/Simias running?\n");
		return -2;
	}
	
	/* Now that we've gotten this far, call this method again to return the key */
	return simias_get_public_key(public_key);
}

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
int
simias_get_private_key(char **private_key)
{
	const char *existing_private_key;
	int err;
	
	if (gaim_prefs_exists(SIMIAS_PREF_PRIVATE_KEY))
	{
		existing_private_key = gaim_prefs_get_string(SIMIAS_PREF_PRIVATE_KEY);
		if (existing_private_key)
		{
			*private_key = strdup(existing_private_key);
			return 0;
		}
		else
		{
			return -1;
		}
	}

	err = setup_keypair();
	if (err != 0)
	{
		fprintf(stderr, "There was an error retrieving the public/private key pair from Simias.  Is iFolder/Simias running?\n");
		return -2;
	}
	
	/* Now that we've gotten this far, call this method again to return the key */
	return simias_get_private_key(private_key);
}

/**
 * Retrieves the RSACryptoProvider (public and private keys) for the first time
 * from Simias and stores them as a plugin setting in Gaim.
 */
static int
setup_keypair()
{
	char *soap_url;
	struct soap soap;
	struct _ns1__GetRSACredential req;
	struct _ns1__GetRSACredentialResponse resp;
	char username[512];
	char password[1024];

	/* We need to go get the public/private key from Simias */
	soap_url = get_soap_url(TRUE);
	if (!soap_url) {
		fprintf(stderr, "get_soap_url() returned NULL inside setup_keypair()\n");
		return -1;
	}
	
	init_gsoap(&soap);
	if (simias_get_web_service_credential(username, password) == 0) {
		soap.userid = username;
		soap.passwd = password;
	}
	soap_call___ns1__GetRSACredential(&soap, soap_url, NULL, &req, &resp);
	if (soap.error) {
		fprintf(stderr, "Error calling soap_call___ns1__GetRSACredential() in setup_keypair()\n");
		soap_print_fault(&soap, stderr);
		cleanup_gsoap(&soap);
		return -2;
	}

	if (resp.GetRSACredentialResult != true_)
	{
		fprintf(stderr, "soap_call___ns1__GetRSACredential() returned FALSE\n");
		return -3;
	}
	
	/**
	 * This is the first time we've ever retrieved the PPK from Simias so
	 * store it in the Gaim Plugin Configuration area for future use.
	 */
	gaim_prefs_set_string(SIMIAS_PREF_PUBLIC_KEY, resp.PublicCredential);
	gaim_prefs_set_string(SIMIAS_PREF_PRIVATE_KEY, resp.PrivateCredential);
	gaim_prefs_trigger_callback(SIMIAS_PREF_PUBLIC_KEY);

	fprintf(stderr, "Just added the public/private key pair to prefs.xml\n");
	
	cleanup_gsoap(&soap);

	return 0;
}

/**
 * Uses rsaCryptoXml (.NET XML String for a RSACryptoServiceProvider), to
 * encrypt unencrypted_string.  Returns 0 if successful, in which case
 * encrypted_string will be valid and needs to be freed.  If there was an
 * error, the function returns a negative int and the encrypted_string is
 * invalid and does not need to be freed.
 */
int
simias_rsa_encrypt_string(const char *rsaCryptoXml, const char *unencrypted_string, char **encrypted_string)
{
	char *soap_url;
	struct soap soap;
	struct _ns1__RSAEncryptString req;
	struct _ns1__RSAEncryptStringResponse resp;
	char username[512];
	char password[1024];
	
	soap_url = get_soap_url(TRUE);
	if (!soap_url) {
		return -1;
	}

	/* Setup the Request */
	req.RsaCryptoXml = (char *)rsaCryptoXml;
	req.UnencryptedString = (char *)unencrypted_string;
	
	init_gsoap(&soap);
	if (simias_get_web_service_credential(username, password) == 0) {
		soap.userid = username;
		soap.passwd = password;
	}
	soap_call___ns1__RSAEncryptString(&soap, soap_url, NULL, &req, &resp);
	if (soap.error) {
		fprintf(stderr, "Error calling soap_call___ns1__RSAEncryptString() in simias_encrypt_string()\n");
		soap_print_fault(&soap, stderr);
		cleanup_gsoap(&soap);
		return -2;
	}

	if (resp.RSAEncryptStringResult)
	{
		*encrypted_string = strdup(resp.RSAEncryptStringResult);
	}
	
	cleanup_gsoap(&soap);

	return 0;
}

/**
 * Uses rsaCryptoXml (.NET XML String for a RSACryptoServiceProvider), to
 * decrypt encrypted_string.  Returns 0 if successful, in which case
 * decrypted_string will be valid and needs to be freed.  If there was an
 * error, the function returns a negative int and the decrypted_string is
 * invalid and does not need to be freed.
 */
int
simias_rsa_decrypt_string(const char *rsaCryptoXml, const char *encrypted_string, char **decrypted_string)
{
	char *soap_url;
	struct soap soap;
	struct _ns1__RSADecryptString req;
	struct _ns1__RSADecryptStringResponse resp;
	char username[512];
	char password[1024];
	
	soap_url = get_soap_url(TRUE);
	if (!soap_url) {
		return -1;
	}

	/* Setup the Request */
	req.RsaCryptoXml = (char *)rsaCryptoXml;
	req.EncryptedString = (char *)encrypted_string;
	
	init_gsoap(&soap);
	if (simias_get_web_service_credential(username, password) == 0) {
		soap.userid = username;
		soap.passwd = password;
	}
	soap_call___ns1__RSADecryptString(&soap, soap_url, NULL, &req, &resp);
	if (soap.error) {
		fprintf(stderr, "Error calling soap_call___ns1__RSADecryptString() in simias_decrypt_string()\n");
		soap_print_fault(&soap, stderr);
		cleanup_gsoap(&soap);
		return -2;
	}

	if (resp.RSADecryptStringResult)
	{
		*decrypted_string = strdup(resp.RSADecryptStringResult);
	}
	
	cleanup_gsoap(&soap);

	return 0;
}

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
int simias_get_des_key(char **des_key)
{
	const char *existing_des_key;
	int err;
	
	if (gaim_prefs_exists(SIMIAS_PREF_DES_KEY))
	{
		existing_des_key = gaim_prefs_get_string(SIMIAS_PREF_DES_KEY);
		if (existing_des_key)
		{
			*des_key = strdup(existing_des_key);
			return 0;
		}
		else
		{
			return -1;
		}
	}

	err = setup_des_key();
	if (err != 0)
	{
		fprintf(stderr, "There was an error getting a new DES key from GaimDomainService.asmx.  Is iFolder/Simias running?\n");
		return -2;
	}
	
	/* Now that we've gotten this far, call this method again to return the key */
	return simias_get_des_key(des_key);
}

/**
 * Retrieves a new Base64 Encoded DES Key from GaimDomainService.asmx and
 * stores them as a plugin setting in Gaim.
 */
static int
setup_des_key()
{
	char *soap_url;
	struct soap soap;
	struct _ns1__GenerateDESKey req;
	struct _ns1__GenerateDESKeyResponse resp;
	char username[512];
	char password[1024];

	/* We need to go get the public/private key from Simias */
	soap_url = get_soap_url(TRUE);
	if (!soap_url) {
		fprintf(stderr, "get_soap_url() returned NULL inside setup_des_key()\n");
		return -1;
	}
	
	init_gsoap(&soap);
	if (simias_get_web_service_credential(username, password) == 0) {
		soap.userid = username;
		soap.passwd = password;
	}
	soap_call___ns1__GenerateDESKey(&soap, soap_url, NULL, &req, &resp);
	if (soap.error) {
		fprintf(stderr, "Error calling soap_call___ns1__GetRSACredential() in setup_keypair()\n");
		soap_print_fault(&soap, stderr);
		cleanup_gsoap(&soap);
		return -2;
	}

	if (!resp.GenerateDESKeyResult)
	{
		fprintf(stderr, "soap_call___ns1__GenerateDESKey() returned NULL\n");
		return -3;
	}
	
	/**
	 * This is the first time we've ever retrieved the DES Key so
	 * store it in the Gaim Plugin Configuration area for future use.
	 */
	gaim_prefs_set_string(SIMIAS_PREF_DES_KEY, resp.GenerateDESKeyResult);
	gaim_prefs_trigger_callback(SIMIAS_PREF_DES_KEY);
	
	cleanup_gsoap(&soap);

	return 0;
}


/**
 * Uses desKey to encrypt unencrypted_string.  The encrypted_string is returned
 * Base64 encoded.
 *
 * Returns 0 if successful, in which case encrypted_string will be valid and
 * needs to be freed.  If there was an error, the function returns a negative
 * int and the encrypted_string is invalid and does not need to be freed.
 */
int
simias_des_encrypt_string(const char *desKey, const char *unencrypted_string, char **encrypted_string)
{
	char *soap_url;
	struct soap soap;
	struct _ns1__DESEncryptString req;
	struct _ns1__DESEncryptStringResponse resp;
	char username[512];
	char password[1024];

	/* We need to go get the public/private key from Simias */
	soap_url = get_soap_url(TRUE);
	if (!soap_url) {
		fprintf(stderr, "get_soap_url() returned NULL inside simias_des_encrypt_string()\n");
		return -1;
	}
	
	/* Setup the Request */
	req.DESKey = (char *)desKey;
	req.UnencryptedString = (char *)unencrypted_string;
	
	init_gsoap(&soap);
	if (simias_get_web_service_credential(username, password) == 0) {
		soap.userid = username;
		soap.passwd = password;
	}
	soap_call___ns1__DESEncryptString(&soap, soap_url, NULL, &req, &resp);
	if (soap.error) {
		fprintf(stderr, "Error calling soap_call___ns1__DESEncryptString() in simias_des_encrypt_string()\n");
		soap_print_fault(&soap, stderr);
		cleanup_gsoap(&soap);
		return -2;
	}

	if (!resp.DESEncryptStringResult)
	{
		fprintf(stderr, "soap_call___ns1__DESEncryptString() returned NULL\n");
		return -3;
	}
	
	*encrypted_string = strdup(resp.DESEncryptStringResult);
	
	cleanup_gsoap(&soap);

	return 0;
}

/**
 * Uses desKey to decrypt encrypted_string, which is also Base64 encoded.
 *
 * Returns 0 if successful, in which case decrypted_string will be valid and
 * needs to be freed.  If there was an error, the function returns a negative
 * int and the decrypted_string is invalid and does not need to be freed.
 */
int
simias_des_decrypt_string(const char *desKey, const char *encrypted_string, char **decrypted_string)
{
	char *soap_url;
	struct soap soap;
	struct _ns1__DESDecryptString req;
	struct _ns1__DESDecryptStringResponse resp;
	char username[512];
	char password[1024];

	/* We need to go get the public/private key from Simias */
	soap_url = get_soap_url(TRUE);
	if (!soap_url) {
		fprintf(stderr, "get_soap_url() returned NULL inside simias_des_decrypt_string()\n");
		return -1;
	}
	
	/* Setup the Request */
	req.DESKey = (char *)desKey;
	req.EncryptedString = (char *)encrypted_string;
	
	init_gsoap(&soap);
	if (simias_get_web_service_credential(username, password) == 0) {
		soap.userid = username;
		soap.passwd = password;
	}
	soap_call___ns1__DESDecryptString(&soap, soap_url, NULL, &req, &resp);
	if (soap.error) {
		fprintf(stderr, "Error calling soap_call___ns1__DESDecryptString() in simias_des_decrypt_string()\n");
		soap_print_fault(&soap, stderr);
		cleanup_gsoap(&soap);
		return -2;
	}

	if (!resp.DESDecryptStringResult)
	{
		fprintf(stderr, "soap_call___ns1__DESDecryptString() returned NULL\n");
		return -3;
	}
	
	*decrypted_string = strdup(resp.DESDecryptStringResult);
	
	cleanup_gsoap(&soap);

	return 0;
}

/**
 * Gets the machineName, userID, and simiasURL for the current Gaim Domain Owner.
 *
 * This method returns 0 on success.  If success is returned, the machineName,
 * userID, and simiasURL parameters will have had new strings allocated to them
 * and need to be freed.  If there is an error, the output parameters are
 * invalid and do not need to be freed.
 */
int
simias_get_user_info(char **machineName, char **userID, char **simiasURL)
{
	char *soap_url;
	struct soap soap;
	struct _ns1__GetUserInfo req;
	struct _ns1__GetUserInfoResponse resp;
	char username[512];
	char password[1024];
	const char *existing_user_id;

fprintf(stderr, "simias_get_user_info() entered\n");

	soap_url = get_soap_url(TRUE);
	if (!soap_url) {
		return -1;
	}
	
	init_gsoap(&soap);
	if (simias_get_web_service_credential(username, password) == 0) {
		soap.userid = username;
		soap.passwd = password;
	}
	soap_call___ns1__GetUserInfo(&soap, soap_url, NULL, &req, &resp);
	if (soap.error) {
soap_print_fault(&soap, stderr);
		cleanup_gsoap(&soap);
		return -2;
	}

	if (resp.GetUserInfoResult != true_)
	{
		return -3;
	}

fprintf(stderr, "GetUserInfo() returned UserID: %s\n", resp.UserID);

	if (gaim_prefs_exists(SIMIAS_PREF_USER_ID))
	{
fprintf(stderr, "SIMIAS_PREF_USER_ID already exists\n");
		existing_user_id = gaim_prefs_get_string(SIMIAS_PREF_USER_ID);
		if (existing_user_id)
		{
fprintf(stderr, "SIMIAS_PREF_USER_ID = %s\n", existing_user_id);
			*userID = strdup(existing_user_id);
		}
		else
		{
			return -4;
		}
	}
	else
	{
fprintf(stderr, "SIMIAS_PREF_USER_ID does NOT exist\n");
		*userID = strdup(resp.UserID);

		gaim_prefs_add_string(SIMIAS_PREF_USER_ID, *userID);
		gaim_prefs_trigger_callback(SIMIAS_PREF_USER_ID);
	}

	*machineName = strdup(resp.MachineName);
	*simiasURL = strdup(resp.SimiasURL);
	
	cleanup_gsoap(&soap);

	return 0;
}

/**
 * Gets the machineName.
 *
 * This method returns 0 on success.  If success is returned, the machineName
 * will contain the machineName in a newly allocated char * and will need to be
 * freed.  If there is an error machineName will be invalid and does not need
 * to be freed.
 */
int
simias_get_machine_name(char **machineName)
{
	char *soap_url;
	struct soap soap;
	struct _ns1__GetMachineName req;
	struct _ns1__GetMachineNameResponse resp;
	char username[512];
	char password[1024];
	
	soap_url = get_soap_url(TRUE);
	if (!soap_url) {
		return -1;
	}
	
	init_gsoap(&soap);
	if (simias_get_web_service_credential(username, password) == 0) {
		soap.userid = username;
		soap.passwd = password;
	}
	soap_call___ns1__GetMachineName(&soap, soap_url, NULL, &req, &resp);
	if (soap.error) {
soap_print_fault(&soap, stderr);
		cleanup_gsoap(&soap);
		return -2;
	}

	if (!resp.GetMachineNameResult)
	{
		fprintf(stderr, "soap_call___ns1__GetMachineName() returned NULL\n");
		return -3;
	}
	
	*machineName = strdup(resp.GetMachineNameResult);
	
	cleanup_gsoap(&soap);

	return 0;
}

/**
 * Utility functions for gSOAP
 */
static char *
get_soap_url(gboolean reread_config)
{
	char *url;
	char gaim_domain_url[512];
	int err;
/*
	char *ret_host;
	int ret_port;
	char *ret_path;
	char *ret_user;
	char *ret_passwd;
*/	
	if (!reread_config && the_soap_url) {
		return the_soap_url;
	}

	err = simias_get_local_service_url(&url);
	if (!err) {
		sprintf(gaim_domain_url, "%s/GaimDomainService.asmx", url);
		free(url);

		if (the_soap_url)
			free(the_soap_url);
		the_soap_url = simias_escape_spaces(gaim_domain_url);
		return the_soap_url;

		/**
		 * URL Escape the path so that if there's a space in
		 * the username or anything like that, we won't get errors.
		 */
/*
		if (gaim_url_parse(gaim_domain_url, &ret_host, &ret_port,
						   &ret_path, &ret_user, &ret_passwd))
		{
			sprintf(gaim_domain_url, "http://%s:%d/%s", ret_host, ret_port,
									gaim_url_encode(ret_path));
									^
								This doesn't work!  It stops processing if
								it hits a space character.  WEIRD!


fprintf(stderr, "URL: %s\n", gaim_domain_url);
			the_soap_url = strdup(gaim_domain_url);
			return the_soap_url;
		}
*/
	}
	
	return NULL;
}

static char *
simias_get_user_profile_dir_path(char *dest_path)
{
#if defined(WIN32)
	char *user_profile;
	/* Build the configuration file path. */
	user_profile = getenv("USERPROFILE");
	if (user_profile == NULL || strlen(user_profile) <= 0) {
		return NULL;
	}

	sprintf (dest_path, "%s\\Local Settings\\Application Data\\simias", user_profile);
#else
	char *home_dir;
	char dot_local_share_simias_path[1024];
	
	home_dir = getenv ("HOME");
	if (home_dir == NULL || strlen(home_dir) <= 0) {
		return NULL;
	}
	
	sprintf (dot_local_share_simias_path, "%s%s", home_dir, "/.local/share/simias");
	sprintf (dest_path, dot_local_share_simias_path);
#endif

	return dest_path;
}

/**
 * Parse through the file looking for the following line:
 * 
 * 	<setting name="WebServiceUri" value="http://127.0.0.1:12345/simias10/username"/>
 * 
 * Return a strdup of the URL inside "value" (the caller must free the char *
 * when finished with it).
 */
static char *
parse_local_service_url(FILE *file)
{
	long file_size;
	char *buffer;
	char *setting_idx;
	char *value_idx;
	char *start_quote_idx;
	int end_quote_idx;
	char uri[1024];
	int b_uri_found;
	
	b_uri_found = 0;
	
	/* Determine the file size */
	fseek(file, 0, SEEK_END);
	file_size = ftell(file);
	rewind(file);
	
	/* Allocate memory to suck in the whole file into the buffer */
	buffer = (char *) malloc(file_size);
	if (!buffer) {
		return NULL;
	}
	
	/* Read the contents of the file into the buffer */
	fread(buffer, 1, file_size, file);
	
	/* Now parse for the URL */
	/* Look for "WebServiceUri" */
	setting_idx = strstr(buffer, "WebServiceUri");
	if (setting_idx) {
		value_idx = strstr(setting_idx, "value");
		if (value_idx) {
			start_quote_idx = strstr(value_idx, "\"");
			if (start_quote_idx) {
				end_quote_idx = simias_str_index_of(start_quote_idx + 1, '\"');
				if (end_quote_idx > 0)
				{
					strncpy(uri, start_quote_idx + 1, end_quote_idx);
					uri[end_quote_idx] = '\0';
					b_uri_found = 1;
				}
			}
		}
	}
	
	/* Free up buffer memory */
	free(buffer);
	
	if (!b_uri_found) {
		return NULL;
	}

	return strdup(uri);
}

int
simias_get_local_service_url(char **url)
{
	char user_profile_dir[1024];
	char simias_config_file_path[1024];
	FILE *simias_conf_file;
	
	if (!simias_get_user_profile_dir_path(user_profile_dir)) {
		return -1;
	}

	sprintf(simias_config_file_path, "%s%sSimias.config",
			user_profile_dir, DIR_SEP);
	
	/* Attempt to open the file */
	simias_conf_file = fopen(simias_config_file_path, "r");
	if (!simias_conf_file) {
		return -2;
	}

	*url = parse_local_service_url(simias_conf_file);

	fclose(simias_conf_file);
	
	if (!(*url)) {
		return -3;
	}
	
	return 0;
}

static char *
parse_web_service_password(FILE *file)
{
	char line[1024];
	int i;

	if (!fgets(line, sizeof(line), file))
	{
		fprintf(stderr, "Password file empty or corrupt\n");
		return NULL;
	}
	
	/* Remove any newline chars */
	for (i = strlen(line) - 1; i > 0; i--)
	{
		if (line[i] == '\n' || line[i] == '\r')
			line[i] = '\0';
		else
			break;
	}

	return strdup(line);
}

/**
 * This function gets the username and password needed to invoke calls to the
 * local Simias and iFolder WebServices.
 *
 * param: username (char[] that will be filled using sprintf)
 * param: password (char[] that will be filled using sprintf)
 *
 * returns: Returns 0 if successful or -1 if there's an error.
 */
int
simias_get_web_service_credential(char *username, char *password)
{
	char user_profile_dir[1024];
	char *user;
	char *pw;
	
#if defined(WIN32)
	user = getenv("USERNAME");
#else
	user = getenv("USER");
#endif

	if (!user)
	{
		fprintf(stderr, "USER is not set in the environment\n");
		return -1;
	}

	char simias_password_file_path[1024];
	FILE *simias_password_file;
	
	if (!simias_get_user_profile_dir_path(user_profile_dir)) {
		return -1;
	}

	sprintf(simias_password_file_path, "%s%s.local.if",
			user_profile_dir, DIR_SEP);
	
	/* Attempt to open the file */
	simias_password_file = fopen(simias_password_file_path, "r");
	if (!simias_password_file) {
		fprintf(stderr, "Error opening \"%s\"\n", simias_password_file_path);
		return -1;
	}

	pw = parse_web_service_password(simias_password_file);

	fclose(simias_password_file);
	
	if (!(password)) {
		fprintf(stderr, "Couldn't find the web service password in \"%s\"\n",
					 simias_password_file_path);
		return -1;
	}
	
	sprintf(password, "%s", pw);
	free(pw);
	
	sprintf(username, "%s", user);
	
	return 0;
}

static void
init_gsoap (struct soap *p_soap)
{
	/* Initialize gSOAP */
	soap_init (p_soap);
	soap_set_namespaces (p_soap, simiasgaim_namespaces);
}

static void
cleanup_gsoap (struct soap *p_soap)
{
	/* Cleanup gSOAP */
	soap_end (p_soap);
}
