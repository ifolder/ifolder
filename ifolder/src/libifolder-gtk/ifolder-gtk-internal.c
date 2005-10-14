/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
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
 ***********************************************************************/
#include "ifolder-gtk-internal.h"

#include <stdio.h>
#include <unistd.h>
#include <time.h>

#include <simiasweb.h>

#include "iFolderStub.h"
#include "iFolder.nsmap"

/* Turn this on to see debug messages */
#define DEBUG_IFOLDER(args) (g_print("ifolder-gtk: "), g_printf args)

#define REREAD_SOAP_URL_TIMEOUT 2 /* seconds */

static struct soap *static_soap = NULL;
static char *soapURL = NULL;
static time_t last_read_of_soap_url = 0;

static char * getLocalServiceUrl ();
static void reread_local_service_url ();
//static void init_gsoap (struct soap *p_soap);
//static void cleanup_gsoap (struct soap *p_soap);
static void init_gsoap ();
static void cleanup_gsoap ();


void
ifolder_free_char_data (gpointer data)
{
	char *char_data = (char *)data;
	
printf ("ifolder_free_char_data\n");
	if (char_data)
		free (char_data);
}

int 
ifolder_get_all_ifolders (iFolderList **ifolder_list)
{
	return 0;
}

int
ifolder_get (iFolder **ifolder, const char *ifolder_id)
{
	iFolder *tmp_ifolder;

	if (ifolder_id != NULL) {
		DEBUG_IFOLDER (("****About to call GetiFolder (\"%s\")...\n", ifolder_id));
		struct _ns1__GetiFolder ns1__GetiFolder;
		struct _ns1__GetiFolderResponse ns1__GetiFolderResponse;
		ns1__GetiFolder.iFolderID = (char *) ifolder_id;
		init_gsoap ();
		soap_call___ns1__GetiFolder (static_soap,
									 soapURL,
									 NULL,
									 &ns1__GetiFolder,
									 &ns1__GetiFolderResponse);
		if (static_soap->error) {
			DEBUG_IFOLDER (("****error calling GetiFolder***\n"));
			soap_print_fault (static_soap, stderr);
			if (static_soap->error == SOAP_TCP_ERROR) {
				reread_local_service_url ();
			}
			cleanup_gsoap (static_soap);
			return -1;
		} else {
			DEBUG_IFOLDER (("***calling GetiFolder succeeded***\n"));
			struct ns1__iFolderWeb *ifolder_web = 
				ns1__GetiFolderResponse.GetiFolderResult;
			if (ifolder_web == NULL) {
				DEBUG_IFOLDER (("***GetiFolder returned NULL\n"));
				cleanup_gsoap (static_soap);
				return -2;
			} else {
				tmp_ifolder = malloc (sizeof (iFolder));
				if (!tmp_ifolder)
				{
					cleanup_gsoap (static_soap);
					return -2; // FIXME: Return an out of memory error
				}
				memset (tmp_ifolder, 0, sizeof (iFolder));
				
				if (ifolder_web->ID != NULL)
					tmp_ifolder->id = strdup (ifolder_web->ID);
				if (ifolder_web->DomainID != NULL)
					tmp_ifolder->domain_id = strdup (ifolder_web->DomainID);
				if (ifolder_web->ManagedPath != NULL)
					tmp_ifolder->managed_path = strdup (ifolder_web->ManagedPath);
				if (ifolder_web->UnManagedPath != NULL)
					tmp_ifolder->unmanaged_path = strdup (ifolder_web->UnManagedPath);
				if (ifolder_web->Name != NULL)
					tmp_ifolder->name = strdup (ifolder_web->Name);
				if (ifolder_web->Owner != NULL)
					tmp_ifolder->owner = strdup (ifolder_web->Owner);
				if (ifolder_web->OwnerID != NULL)
					tmp_ifolder->owner_id = strdup (ifolder_web->OwnerID);
				if (ifolder_web->Type != NULL)
					tmp_ifolder->type = strdup (ifolder_web->Type);
				if (ifolder_web->Description != NULL)
					tmp_ifolder->description = strdup (ifolder_web->Description);
				if (ifolder_web->State != NULL)
					tmp_ifolder->state = strdup (ifolder_web->State);
				if (ifolder_web->CurrentUserID != NULL)
					tmp_ifolder->current_user_id = strdup (ifolder_web->CurrentUserID);
				if (ifolder_web->CurrentUserRights != NULL)
					tmp_ifolder->current_user_rights = strdup (ifolder_web->CurrentUserRights);
				if (ifolder_web->CollectionID != NULL)
					tmp_ifolder->collection_id = strdup (ifolder_web->CollectionID);
				if (ifolder_web->LastSyncTime != NULL)
					tmp_ifolder->last_sync_time = strdup (ifolder_web->LastSyncTime);
				if (ifolder_web->Role != NULL)
					tmp_ifolder->role = strdup (ifolder_web->Role);

				// int properties
				tmp_ifolder->sync_interval = ifolder_web->SyncInterval;
				tmp_ifolder->effective_sync_interval = ifolder_web->EffectiveSyncInterval;

				// boolean properties
				tmp_ifolder->synchronizable = ifolder_web->Synchronizable ? TRUE : FALSE;
				tmp_ifolder->is_subscription = ifolder_web->IsSubscription ? TRUE : FALSE;
				tmp_ifolder->is_workgroup = ifolder_web->IsWorkgroup ? TRUE : FALSE;
				tmp_ifolder->has_conflicts = ifolder_web->HasConflicts ? TRUE : FALSE;
				
				*ifolder = tmp_ifolder;
			}
		}

		cleanup_gsoap (static_soap);
	}

	return 0;
}

int
ifolder_free (iFolder **ifolder)
{
	iFolder *ifl = (*ifolder);
	if (ifl == NULL)
		return -1;	// FIXME: return null argument error

	if (ifl->id)
		free (ifl->id);
	if (ifl->domain_id)
		free (ifl->domain_id);
	if (ifl->managed_path)
		free (ifl->managed_path);
	if (ifl->unmanaged_path)
		free (ifl->unmanaged_path);
	if (ifl->name)
		free (ifl->name);
	if (ifl->owner)
		free (ifl->owner);
	if (ifl->owner_id)
		free (ifl->owner_id);
	if (ifl->type)
		free (ifl->type);
	if (ifl->description)
		free (ifl->description);
	if (ifl->state)
		free (ifl->state);
	if (ifl->current_user_id)
		free (ifl->current_user_id);
	if (ifl->current_user_rights)
		free (ifl->current_user_rights);
	if (ifl->collection_id)
		free (ifl->collection_id);
	if (ifl->last_sync_time)
		free (ifl->last_sync_time);
	if (ifl->role)
		free (ifl->role);

	free (ifl);

	*ifolder = NULL;

	return 0;
}

int
ifolder_free_list (iFolderList **ifolder_list)
{
	return 0;
}

int
ifolder_get_default_sync_interval (int *sync_interval)
{
	DEBUG_IFOLDER (("****About to call GetDefaultSyncInterval ()...\n"));
	struct _ns1__GetDefaultSyncInterval ns1__GetDefaultSyncInterval;
	struct _ns1__GetDefaultSyncIntervalResponse ns1__GetDefaultSyncIntervalResponse;

	init_gsoap ();
	soap_call___ns1__GetDefaultSyncInterval (static_soap, 
									soapURL, 
									NULL, 
									&ns1__GetDefaultSyncInterval, 
									&ns1__GetDefaultSyncIntervalResponse);
	if (static_soap->error) {
		DEBUG_IFOLDER (("****error calling GetDefaultSyncInterval***\n"));
		if (static_soap->error == SOAP_TCP_ERROR) {
			reread_local_service_url ();
		}
		soap_print_fault (static_soap, stderr);
		cleanup_gsoap (static_soap);
		return -1;
	} else {
		DEBUG_IFOLDER (("***calling GetDefaultSyncInterval succeeded***\n"));
		*sync_interval = ns1__GetDefaultSyncIntervalResponse.GetDefaultSyncIntervalResult;
	}

	cleanup_gsoap (static_soap);

	return 0;
}

int
ifolder_get_objects_to_sync (int *objects_to_sync, const char *ifolder_id)
{
	if (ifolder_id != NULL) {
		DEBUG_IFOLDER (("****About to call CalculateSyncSize (\"%s\")...\n", ifolder_id));
		struct _ns1__CalculateSyncSize ns1__CalculateSyncSize;
		struct _ns1__CalculateSyncSizeResponse ns1__CalculateSyncSizeResponse;
		ns1__CalculateSyncSize.iFolderID = (char *) ifolder_id;
		init_gsoap ();
		soap_call___ns1__CalculateSyncSize (static_soap,
									 soapURL,
									 NULL,
									 &ns1__CalculateSyncSize,
									 &ns1__CalculateSyncSizeResponse);
		if (static_soap->error) {
			DEBUG_IFOLDER (("****error calling CalculateSyncSize***\n"));
			soap_print_fault (static_soap, stderr);
			if (static_soap->error == SOAP_TCP_ERROR) {
				reread_local_service_url ();
			}
			cleanup_gsoap (static_soap);
			return -1;
		} else {
			DEBUG_IFOLDER (("***calling CalculateSyncSize succeeded***\n"));
			struct ns1__SyncSize *sync_size = 
				ns1__CalculateSyncSizeResponse.CalculateSyncSizeResult;
			
			*objects_to_sync = (int) sync_size->SyncNodeCount;
		}

		cleanup_gsoap (static_soap);
	}

	return 0;
}

int
ifolder_get_disk_space (iFolderDiskSpace **disk_space, const char *ifolder_id)
{
	if (ifolder_id != NULL) {
		DEBUG_IFOLDER (("****About to call GetiFolderDiskSpace (\"%s\")...\n", ifolder_id));
		struct _ns1__GetiFolderDiskSpace ns1__GetiFolderDiskSpace;
		struct _ns1__GetiFolderDiskSpaceResponse ns1__GetiFolderDiskSpaceResponse;
		ns1__GetiFolderDiskSpace.iFolderID = (char *) ifolder_id;
		init_gsoap ();
		soap_call___ns1__GetiFolderDiskSpace (static_soap,
									 soapURL,
									 NULL,
									 &ns1__GetiFolderDiskSpace,
									 &ns1__GetiFolderDiskSpaceResponse);
		if (static_soap->error) {
			DEBUG_IFOLDER (("****error calling GetiFolderDiskSpace***\n"));
			soap_print_fault (static_soap, stderr);
			if (static_soap->error == SOAP_TCP_ERROR) {
				reread_local_service_url ();
			}
			cleanup_gsoap (static_soap);
			return -1;
		} else {
			DEBUG_IFOLDER (("***calling GetiFolderDiskSpace succeeded***\n"));
			struct ns1__DiskSpace *ret_disk_space = 
				ns1__GetiFolderDiskSpaceResponse.GetiFolderDiskSpaceResult;
			
			*disk_space = malloc (sizeof (iFolderDiskSpace));
			if (*disk_space == NULL)
			{
				cleanup_gsoap (static_soap);
				return -2; // FIXME: Change this to an out of memory error
			}

			((iFolderDiskSpace *)*disk_space)->available_space = ret_disk_space->AvailableSpace;
			((iFolderDiskSpace *)*disk_space)->limit = ret_disk_space->Limit;
			((iFolderDiskSpace *)*disk_space)->used_space = ret_disk_space->UsedSpace;
		}

		cleanup_gsoap (static_soap);
	}

	return 0;
}


int
ifolder_sync_now (const char *ifolder_id)
{
	if (ifolder_id != NULL) {
		DEBUG_IFOLDER (("****About to call SynciFolderNow (\"%s\")...\n", ifolder_id));
		struct _ns1__SynciFolderNow ns1__SynciFolderNow;
		struct _ns1__SynciFolderNowResponse ns1__SynciFolderNowResponse;
		ns1__SynciFolderNow.iFolderID = (char *) ifolder_id;
		init_gsoap ();
		soap_call___ns1__SynciFolderNow (static_soap,
									 soapURL,
									 NULL,
									 &ns1__SynciFolderNow,
									 &ns1__SynciFolderNowResponse);
		if (static_soap->error) {
			DEBUG_IFOLDER (("****error calling SynciFolderNow***\n"));
			soap_print_fault (static_soap, stderr);
			if (static_soap->error == SOAP_TCP_ERROR) {
				reread_local_service_url ();
			}
			cleanup_gsoap (static_soap);
			return -1;
		} else {
			DEBUG_IFOLDER (("***calling SynciFolderNow succeeded***\n"));
		}

		cleanup_gsoap (static_soap);
	}

	return 0;
}

int
ifolder_save_disk_space_limit (const char *ifolder_id, long long limit)
{
DEBUG_IFOLDER (("ifolder_save_disk_space_limit: %lld\n", limit));

	if (ifolder_id != NULL) {
		DEBUG_IFOLDER (("****About to call SetiFolderDiskSpaceLimit (\"%s\")...\n", ifolder_id));
		struct _ns1__SetiFolderDiskSpaceLimit ns1__SetiFolderDiskSpaceLimit;
		struct _ns1__SetiFolderDiskSpaceLimitResponse ns1__SetiFolderDiskSpaceLimitResponse;
		ns1__SetiFolderDiskSpaceLimit.iFolderID = (char *) ifolder_id;
		ns1__SetiFolderDiskSpaceLimit.Limit = limit;
		init_gsoap ();

		soap_call___ns1__SetiFolderDiskSpaceLimit (static_soap,
									 soapURL,
									 NULL,
									 &ns1__SetiFolderDiskSpaceLimit,
									 &ns1__SetiFolderDiskSpaceLimitResponse);
		if (static_soap->error) {
			DEBUG_IFOLDER (("****error calling SetiFolderDiskSpaceLimit***\n"));
			soap_print_fault (static_soap, stderr);
			if (static_soap->error == SOAP_TCP_ERROR) {
				reread_local_service_url ();
			}
			cleanup_gsoap (static_soap);
			return -1;
		} else {
			DEBUG_IFOLDER (("***calling SetiFolderDiskSpaceLimit succeeded***\n"));
		}

		cleanup_gsoap (static_soap);
	}

	return 0;
}

int
ifolder_get_members (iFolderUserList **user_list, const char *ifolder_id)
{
	iFolderUserList *list_start, *list_item, *current_item;
	iFolderUser *user;
	struct ns1__iFolderUser *user_web;
	int i;

	list_start = NULL;
	current_item = NULL;

	if (ifolder_id != NULL) {
		DEBUG_IFOLDER (("****About to call GetiFolderUsers (\"%s\")...\n", ifolder_id));
		struct _ns1__GetiFolderUsers ns1__GetiFolderUsers;
		struct _ns1__GetiFolderUsersResponse ns1__GetiFolderUsersResponse;
		ns1__GetiFolderUsers.iFolderID = (char *) ifolder_id;
		init_gsoap ();
		soap_call___ns1__GetiFolderUsers (static_soap,
									 soapURL,
									 NULL,
									 &ns1__GetiFolderUsers,
									 &ns1__GetiFolderUsersResponse);
		if (static_soap->error) {
			DEBUG_IFOLDER (("****error calling GetiFolderUsers***\n"));
			soap_print_fault (static_soap, stderr);
			if (static_soap->error == SOAP_TCP_ERROR) {
				reread_local_service_url ();
			}
			cleanup_gsoap (static_soap);
			return -1;
		} else {
			DEBUG_IFOLDER (("***calling GetiFolderUsers succeeded***\n"));
			struct ns1__ArrayOfIFolderUser *ifolder_users =
				ns1__GetiFolderUsersResponse.GetiFolderUsersResult;
			
			if (ifolder_users->__sizeiFolderUser > 0) {
				for (i = 0; i < ifolder_users->__sizeiFolderUser; i++) {
					user_web = ifolder_users->iFolderUser [i];
					
					user = malloc (sizeof (iFolderUser));
					memset (user, 0, sizeof (iFolderUser));
					if (user_web->Name)
						user->name = strdup (user_web->Name);
					if (user_web->UserID)
						user->user_id = strdup (user_web->UserID);
					if (user_web->Rights)
						user->rights = strdup (user_web->Rights);
					if (user_web->ID)
						user->id = strdup (user_web->ID);
					if (user_web->State)
						user->state = strdup (user_web->State);
					if (user_web->iFolderID)
						user->ifolder_id = strdup (user_web->iFolderID);
					if (user_web->FirstName)
						user->first_name = strdup (user_web->FirstName);
					if (user_web->Surname)
						user->surname = strdup (user_web->Surname);
					if (user_web->FN)
						user->full_name = strdup (user_web->FN);
					user->is_owner = user_web->IsOwner ? TRUE : FALSE;
					
					list_item = malloc (sizeof (iFolderUserList));
					list_item->ifolder_user = user;
					list_item->next = NULL;
					
					if (list_start == NULL) {
						list_start = list_item;
						current_item = list_start;
					} else {
						current_item->next = list_item;
						current_item = list_item;
					}
				}
			}

			*user_list = list_start;
		}

		cleanup_gsoap (static_soap);
	}
	
	return 0;
}

int
ifolder_free_user (iFolderUser **ifolder_user)
{
	iFolderUser *user = *ifolder_user;
	if (user == NULL)
		return -1;	// FIXME: return null argument error

	if (user->name)
		free (user->name);
	if (user->user_id)
		free (user->user_id);
	if (user->rights)
		free (user->rights);
	if (user->id)
		free (user->id);
	if (user->state)
		free (user->state);
	if (user->ifolder_id)
		free (user->ifolder_id);
	if (user->first_name)
		free (user->first_name);
	if (user->surname)
		free (user->surname);
	if (user->full_name)
		free (user->full_name);

	free (user);

	*ifolder_user = NULL;

	return 0;
}

int
ifolder_free_user_list (iFolderUserList **user_list)
{
	iFolderUserList *item, *next_item;
	
	item = (iFolderUserList *) *user_list;

	while (item) {
		ifolder_free_user (&(item->ifolder_user));
		
		next_item = item->next;
		free (item);
		item = next_item;
	}
	
	*user_list = NULL;
	
	return 0;
}

/**
 * gSOAP
 */
static char *
getLocalServiceUrl ()
{
	int err;
	char *url;
	char tmpUrl [1024];

	DEBUG_IFOLDER (("getLocalServiceUrl () attempting to determine soapURL\n"));
	
	err = simias_get_local_service_url(&url);
	if (err != SIMIAS_SUCCESS) {
		DEBUG_IFOLDER(("simias_get_local_service_url() returned NULL!\n"));
		return NULL;
	}
	
	sprintf(tmpUrl, "%s/iFolder.asmx", url);
	free(url);
	return strdup(tmpUrl);
}

static void
reread_local_service_url ()
{
	time_t current_time;

	/**
	 * If iFolder has never been run, the file that contains the Local Service
	 * URL will not exist.  This method will be called any time a TCP connection
	 * error occurs.  Prevent rapid calling of this method (less than
	 * REREAD_SOAP_URL_TIMEOUT seconds).
	 */
	if (time (&current_time) < 
			(last_read_of_soap_url + REREAD_SOAP_URL_TIMEOUT)) {
		return;
	}
	last_read_of_soap_url = current_time;
	
	soapURL = getLocalServiceUrl ();
}

static void
//init_gsoap (struct soap *p_soap)
init_gsoap ()
{
	char username[512];
	char password[1024];
	
	if (static_soap == NULL)
	{
DEBUG_IFOLDER (("Initializing gSOAP\n"));
		static_soap = (struct soap *) malloc (sizeof (struct soap));

		/* Initialize gSOAP */
		soap_init2 (static_soap, SOAP_C_UTFSTRING | SOAP_IO_DEFAULT,
					SOAP_C_UTFSTRING | SOAP_IO_DEFAULT);
		soap_set_namespaces (static_soap, iFolder_namespaces);

		if (simias_get_web_service_credential(username, password) == SIMIAS_SUCCESS) {
			static_soap->userid = username;
			static_soap->passwd = password;
		}
		
		static_soap->recv_timeout = 30;
		static_soap->send_timeout = 30;

		if (!soapURL)
			soapURL = getLocalServiceUrl ();
	}
}

static void
//cleanup_gsoap (struct soap *p_soap)
cleanup_gsoap ()
{
	/* Cleanup gSOAP */
//	soap_end (p_soap);
	if (static_soap != NULL)
	{
		soap_end (static_soap);
	}
}
