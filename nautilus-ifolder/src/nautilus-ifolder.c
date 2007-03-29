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
#include <libnautilus-extension/nautilus-extension-types.h>
#include <libnautilus-extension/nautilus-file-info.h>
#include <libnautilus-extension/nautilus-info-provider.h>
#include <libnautilus-extension/nautilus-menu-provider.h>

#include <gtk/gtk.h>
#include <gdk-pixbuf/gdk-pixbuf.h>
#include <gconf/gconf-client.h>
#include <libintl.h>
#include <string.h>
#include <stdio.h>
#include <unistd.h>
#include <time.h>

#include <libgnomevfs/gnome-vfs-utils.h>

#include <simias-event-client.h>
#include <simiasweb.h>
//#include "simias-internal.h"

#include <ifolder.h>
#include <iFolderStub.h>
#include <iFolder.nsmap>

#include "nautilus-ifolder.h"
#include "nautilus-ifolder-holder.h"
#include "../config.h"

/* Turn this on to see debug messages */
#if DEBUG
#define DEBUG_IFOLDER(args) (g_print("nautilus-ifolder: "), g_printf args)
#else
#define DEBUG_IFOLDER
#endif

#ifdef _
#undef _
#endif 

#define _(STRING) dgettext(GETTEXT_PACKAGE, STRING)

#define IFOLDER_FIFO_NAME ".nautilus-ifolder-fifo"
#define IFOLDER_BUF_SIZE 1024

#define REREAD_SOAP_URL_TIMEOUT 2 /* seconds */

typedef struct {
	GObject parent_slot;
} iFolderNautilus;

typedef struct {
	GObjectClass parent_slot;
} iFolderNautilusClass;

typedef struct {
	GtkWidget	*window;
	gchar		*title;
	gchar		*message;
	gchar		*detail;
} iFolderErrorMessage;

typedef struct {
	GClosure *update_complete;
	NautilusInfoProvider *provider;
	NautilusFileInfo *file;
	int operation_handle;
	gboolean cancelled;
} UpdateHandle;

static GType provider_types[1];

static char *soapURL = NULL;
time_t last_read_of_soap_url = 0;

static void ifolder_extension_register_type (GTypeModule *module);
static void ifolder_nautilus_instance_init (iFolderNautilus *ifn);

static GObjectClass * parent_class = NULL;
static GType ifolder_nautilus_type;

static SimiasEventClient ec;
static gboolean b_nautilus_ifolder_running;

static gint reconnected_id = 0;
static gint disconnected_id = 0;
static GStaticMutex reconnected_mutex = G_STATIC_MUTEX_INIT;
static GStaticMutex disconnected_mutex = G_STATIC_MUTEX_INIT;

/**
 * This hashtable is used to keep a list of all iFolders on the computer and it
 * is around so that we don't have to always call back and forth over the
 * WebService to get the list of iFolders.
 * 
 * When the extension is first loaded (or Simias is started), we build the list
 * of iFolders.  Then, this code listens for create/delete events to keep the
 * list of iFolders up-to-date.
 */
static GHashTable *ifolders_ht;

/**
 * This hashtable is used to track the iFolders that the extension has seen and
 * added an emblem to.  When a SimiasNodeDeleted event occurs and the iFolder is
 * in this list, it will be used to remove the iFolder emblem from the Folder.
 * 
 * Hashtable keys:		iFolder Simias Node ID
 * Hashtable values: 	Nautilus File URI
 */
static GHashTable *seen_ifolders_ht;

/**
 * FIXME: Once nautilus-extension provides nautilus_file_info_get () we
 * can change our implementation to use the functions defined in the internal
 * nautilus API.
 */
extern NautilusFile * nautilus_file_get_existing (const char *uri);
extern NautilusFile * nautilus_file_get (const char *uri);

/**
 * Private function forward declarations
 */
static char * getLocalServiceUrl ();
static void reread_local_service_url ();
static void init_gsoap (struct soap *p_soap);
static void cleanup_gsoap (struct soap *p_soap);
static gchar * get_file_path (NautilusFileInfo *file);
static gboolean is_ifolder (NautilusFileInfo *file);
static gboolean can_be_ifolder (NautilusFileInfo *file);
static gint create_ifolder_in_domain (NautilusFileInfo *file, char *domain_id);
static gint revert_ifolder (NautilusFileInfo *file);
static iFolderHolder * get_ifolder_holder (gchar *ifolder_id);
/*static GSList *get_all_ifolder_paths ();*/

/* Functions for ifolders_ht (GHashTable) */
static void refresh_ifolders_ht ();
static void ifolders_ht_destroy_key (gpointer key);
static void ifolders_ht_destroy_value (gpointer value);

/* Functions for seen_ifolders_ht (GHashTable) */
static void seen_ifolders_ht_destroy_key (gpointer key);
static void seen_ifolders_ht_destroy_value (gpointer value);
static void seen_ifolders_ht_invalidate_ifolder (gpointer key, gpointer value, gpointer user_data);

#define KEY_SHOW_CREATION "/apps/ifolder3/notification/show_created_dialog"

static gboolean show_created_dialog ();

static gboolean sec_reconnected (gpointer user_data);
static gboolean sec_disconnected (gpointer user_data);

static void invalidate_local_path (gpointer key, gpointer value, gpointer user_data);

									   
static void ifolder_help_callback (NautilusMenuItem *item, gpointer user_data);

/**
 * This function should only be called with g_idle_add by the Simias Event
 * Client's Callback function when it connects with the Simias Event Server.
 * 
 * It will rebuild the list of iFolders and invalidate all the previous
 * information that may be cached (so it will be refreshed).
 */
static gboolean
sec_reconnected (gpointer user_data)
{
	/**
	 * Check to make sure sec_disconnected isn't currently running.  If it is,
	 * sec_reconnected shouldn't run because it would start working on
	 * structures that aren't initialized/cleaned up.
	 */
	g_static_mutex_lock (&disconnected_mutex);
	if (disconnected_id != 0) {
		g_static_mutex_unlock (&disconnected_mutex);
		DEBUG_IFOLDER (("sec_reconnected() called and disconnected_id != 0"));
		return TRUE; /* This function will be called again soon. */
	}
	g_static_mutex_unlock (&disconnected_mutex);
	
	/* Rebuild ifolders_ht (GHashTable) to get the latest list */
	refresh_ifolders_ht ();

	/**
	 * Invalidate NautilusFileInfo on every iFolder's unmanaged path so that
	 * Nautilus will update the emblem.
	 */
	g_hash_table_foreach (ifolders_ht, invalidate_local_path, NULL);

	return FALSE; /* Don't have this be called over and overa automatically. */
}

static void
sec_reconnected_idle_removed (gpointer data)
{
	DEBUG_IFOLDER (("sec_reconnected_idle_removed()"));
	g_static_mutex_lock (&reconnected_mutex);
	reconnected_id = 0;
	g_static_mutex_unlock (&reconnected_mutex);
}

static gboolean
sec_disconnected (gpointer user_data)
{
	/**
	 * Check to make sure sec_connected isn't currently running.  If it is,
	 * sec_disconnected shouldn't run because it would free memory on data
	 * structures that are currently being allocated.
	 */
	g_static_mutex_lock (&reconnected_mutex);
	if (reconnected_id != 0) {
		g_static_mutex_unlock (&reconnected_mutex);
		DEBUG_IFOLDER (("sec_disconnected() called and reconnected_id != 0"));
		return TRUE; /* This function will be called again soon. */
	}
	g_static_mutex_unlock (&reconnected_mutex);
	

	/**
	 * Recreate ifolders_ht.  Since we use g_hash_table_new_full() freeing
	 * memory is already handled.
	 */
	g_hash_table_destroy (ifolders_ht);
	ifolders_ht =
		g_hash_table_new_full (g_str_hash, g_str_equal,
							   ifolders_ht_destroy_key,
							   ifolders_ht_destroy_value);

	/**
	 * Iterate through seen_ifolders_ht and invalidate the extension
	 * info so that all iFolder emblems are removed when iFolder is not
	 * running.
	 */
	g_hash_table_foreach (seen_ifolders_ht,
						  seen_ifolders_ht_invalidate_ifolder, NULL);

	/**
	 * Since iFolder is no longer running (or at least the event server
	 * is down, remove all the entries in seen_ifolders_ht.
	 */
	g_hash_table_destroy (seen_ifolders_ht);
	seen_ifolders_ht = 
		g_hash_table_new_full (g_str_hash, g_str_equal,
							   seen_ifolders_ht_destroy_key,
							   seen_ifolders_ht_destroy_value);
	
	return FALSE; /* Don't have this be called over and overa automatically. */
}

static void
sec_disconnected_idle_removed (gpointer data)
{
	DEBUG_IFOLDER (("sec_disconnected_idle_removed()"));
	g_static_mutex_lock (&disconnected_mutex);
	disconnected_id = 0;
	g_static_mutex_unlock (&disconnected_mutex);
}

/**
 * This function is called by simias_node_created_cb() and is called by
 * g_idle_add() so that it will run in the main thread (and not have
 * concurrency problems modifying ifolders_ht (GHashTable).
 */
static gboolean
add_ifolder (gpointer user_data)
{
	iFolderHolder *holder;
	char *file_uri;
	NautilusFileInfo *file;
	
	holder = (iFolderHolder *)user_data;
	g_hash_table_insert (ifolders_ht,
						 holder->unmanaged_path,
						 holder);
	
	file_uri = gnome_vfs_get_uri_from_local_path (holder->unmanaged_path);
	if (file_uri) {
		/* FIXME: Change the following to be nautilus_file_info_get_existing () once it's available in the Nautilus Extension API */
		file = nautilus_file_get_existing (file_uri);
		if (file) {
			DEBUG_IFOLDER (("Found NautilusFile: %s\n", file_uri));

			nautilus_file_info_invalidate_extension_info (file);
			
			g_object_unref (G_OBJECT(file));
		}

		free (file_uri);
	} else {
		DEBUG_IFOLDER (("add_ifolder: gnome_vfs_get_uri_from_local_path(%s) returned NULL", holder->unmanaged_path));
	}

	return FALSE; /* Don't have this be called again automatically. */
}

static gboolean
find_ifolder_by_id (gpointer key, gpointer value, gpointer user_data)
{
	iFolderHolder *holder;
	char *id_to_find;
	
	holder = (iFolderHolder *)value;
	id_to_find = (char *)user_data;
	
	if (holder->id != NULL && id_to_find != NULL &&
		strcmp(holder->id, id_to_find) == 0)
		return TRUE;
	else
		return FALSE;
}

/**
 * This function is called by simias_node_deleted_cb() and is called by
 * g_idle_add() so that it will run in the main thread (and not have
 * concurrency problems modifying ifolders_ht (GHashTable).
 */
static gboolean
remove_ifolder (gpointer user_data)
{
	char *ifolder_id;
	iFolderHolder *holder;
	gchar *file_uri;
	NautilusFileInfo *file;
	
	ifolder_id = (char *)user_data;
	
	/* FIXME: Decide whether we should change the following line from an O(n) operation to something like O(1) by adding an additional GHashTable to map directly from iFolder ID to iFolderHolder in ifolders_ht. */
	holder = g_hash_table_find (ifolders_ht, find_ifolder_by_id, ifolder_id);

	if (holder != NULL) {
		/* Remove this from ifolders_ht */
		g_hash_table_remove (ifolders_ht, holder->unmanaged_path);
	}
	
	/**
	 * Look in the seen_ifolders_ht (GHashTable) to see if we've ever added an
	 * iFolder emblem onto this folder.  If we haven't, no sense worrying about
	 * it because it wouldn't have an emblem on it anyway.
	 */
	file_uri = (gchar *)g_hash_table_lookup (seen_ifolders_ht, ifolder_id);
	if (file_uri) {
		/**
		 * Get an existing (in memory) NautilusFileInfo object associated with
		 * this file_uri and invalidate the extension information.
		 */
		/* FIXME: Change the following to be nautilus_file_info_get_existing () once it's available in the Nautilus Extension API */
		file = nautilus_file_get_existing (file_uri);
		if (file) {
			nautilus_file_info_invalidate_extension_info (file);
			
			g_object_unref (G_OBJECT(file));
		}
		
		/**
		 * Now that this folder is not an iFolder anymore, we can remove it
		 * from the seen_ifolders_ht (GHashTable).
		 */
		g_hash_table_remove (seen_ifolders_ht, ifolder_id);
	}
	
	free (ifolder_id);

	return FALSE; /* Don't have this be called again automatically. */
}

/**
 * Callback functions for the Simias Event Client
 */
static int
simias_node_created_cb (SimiasNodeEvent *event, void *data)
{
	iFolderHolder *holder;

	DEBUG_IFOLDER (("simias_node_created_cb () entered\n"));
	
	/**
	 * If the extension has ever been asked to provide information about
	 * the folder, it needs to be invalidated so that nautilus will ask
	 * for the information again.  This will allow the iFolder emblem to
	 * appear when a new iFolder is created.
	 */		
	holder = get_ifolder_holder (event->node);
	if (holder != NULL)
		g_idle_add (add_ifolder, holder);
	
	return 0;
}

static int
simias_node_deleted_cb (SimiasNodeEvent *event, void *data)
{
	gchar *file_uri;
	NautilusFile *file;
	
	DEBUG_IFOLDER (("simias_node_deleted_cb () entered\n"));
	
	g_idle_add (remove_ifolder, strdup (event->node));
	
	return 0;
}

static int
ec_state_event_cb (SEC_STATE_EVENT state_event, const char *message, void *data)
{
	SimiasEventClient *ec = (SimiasEventClient *)data;
	int i;
	GSList *ifolder_paths;
	SIMIAS_NODE_TYPE node_type;
	SimiasEventFilter event_filter;
	
	switch (state_event) {
		case SEC_STATE_EVENT_CONNECTED:
			DEBUG_IFOLDER (("Connected event received by SEC\n"));
			
			/* Allow gSOAP to reconnect */
			last_read_of_soap_url = 0;

			/* Register our event handler */
			sec_set_event (*ec, ACTION_NODE_CREATED, true, (SimiasEventFunc)simias_node_created_cb, NULL);
			sec_set_event (*ec, ACTION_NODE_DELETED, true, (SimiasEventFunc)simias_node_deleted_cb, NULL);
			
			/* Specify that we're only interested in changes in Collections (iFolders) */
			node_type = NODE_TYPE_COLLECTION;
			event_filter.type = EVENT_FILTER_NODE_TYPE;
			event_filter.data = &node_type;
			sec_set_filter (*ec, &event_filter);
			
			DEBUG_IFOLDER (("finished registering for simias events\n"));
			
			/**
			 * Build ifolders_ht to contain the most up-to-date list of
			 * iFolders in Simias.
			 */
			g_static_mutex_lock (&reconnected_mutex);
			reconnected_id =
				g_idle_add_full (G_PRIORITY_HIGH_IDLE,
								 sec_reconnected,
								 NULL,
								 sec_reconnected_idle_removed);
			g_static_mutex_unlock (&reconnected_mutex);
			
			break;
		case SEC_STATE_EVENT_DISCONNECTED:
			DEBUG_IFOLDER (("Disconnected event received by SEC\n"));
			
			/**
			 * Clear out all cached information since we've disconnected from
			 * the Event Server.
			 */
			g_static_mutex_lock (&disconnected_mutex);
			disconnected_id =
				g_idle_add_full (G_PRIORITY_HIGH_IDLE,
								 sec_disconnected,
								 NULL,
								 sec_disconnected_idle_removed);
			g_static_mutex_unlock (&disconnected_mutex);

			break;
		case SEC_STATE_EVENT_ERROR:
			if (message) {
				DEBUG_IFOLDER (("Error in Simias Event Client: %s\n", message));
			} else {
				DEBUG_IFOLDER (("An unknown error occurred in Simias Event Client\n"));
			}
			break;
		default:
			DEBUG_IFOLDER (("An unknown Simias Event Client State Event occurred\n"));
	}
	
	return 0;
}

static int
start_simias_event_client ()
{
	if (sec_init (&ec, ec_state_event_cb, &ec) != 0) {
		DEBUG_IFOLDER (("sec_init failed\n"));
		return -1;
	}
	
	if (sec_register (ec) != 0) {
		DEBUG_IFOLDER (("sec_register failed\n"));
		return -1;
	}
	
	DEBUG_IFOLDER (("sec registration complete\n"));

	return 0;
}

/**
 * gSOAP
 */
static void
init_gsoap (struct soap *p_soap)
{
	/* Initialize gSOAP */
	soap_init2 (p_soap, SOAP_C_UTFSTRING, SOAP_C_UTFSTRING);
	soap_set_namespaces (p_soap, iFolder_namespaces);
}

static void
cleanup_gsoap (struct soap *p_soap)
{
	/* Cleanup gSOAP */
	soap_end (p_soap);
}

/**
 * Utility functions
 */
 
/**
 * g_free () must be called on the returned string
 */
static gchar *
get_file_path (NautilusFileInfo *file)
{
	gchar *file_path, *uri;

	file_path = NULL;
	
	if (file) {
		uri = nautilus_file_info_get_uri (file);
		if (uri) {
			file_path = gnome_vfs_get_local_path_from_uri (uri);
			g_free (uri);
		}
	}

	return file_path;
}

static gboolean
is_ifolder (NautilusFileInfo *file)
{
	gboolean b_is_ifolder;
	gchar *folder_path;
	iFolderHolder *holder;
	
	DEBUG_IFOLDER (("is_ifolder()"));

	b_is_ifolder = FALSE;
	
	folder_path = get_file_path (file);
	if (folder_path == NULL)
		return FALSE;
	
	DEBUG_IFOLDER (("is_ifolder: folder_path=%s", folder_path));

	holder = (iFolderHolder *)g_hash_table_lookup (ifolders_ht, folder_path);
	if (holder != NULL)
		b_is_ifolder = TRUE;
	
	g_free (folder_path);
	
	DEBUG_IFOLDER (("is_ifolder() returning: %s", b_is_ifolder ? "TRUE" : "FALSE"));

	return b_is_ifolder;
}

static gboolean
can_be_ifolder (NautilusFileInfo *file)
{
	struct soap soap;
	gchar *folder_path;
	gboolean b_can_be_ifolder = TRUE;
	char username[512];
	char password[1024];
	
	if (!nautilus_file_info_is_directory (file))
		return FALSE;
		
	folder_path = get_file_path (file);
	if (folder_path != NULL) {
		DEBUG_IFOLDER (("****About to call CanBeiFolder (\"%s\")...\n", folder_path));
		struct _ns1__CanBeiFolder ns1__CanBeiFolder;
		struct _ns1__CanBeiFolderResponse ns1__CanBeiFolderResponse;
		ns1__CanBeiFolder.LocalPath = folder_path;
		init_gsoap (&soap);
		if (simias_get_web_service_credential(username, password) == SIMIAS_SUCCESS) {
			soap.userid = username;
			soap.passwd = password;
		}
//printf("Calling iFolderWebService.CanBeiFolder()\n");
		soap_call___ns1__CanBeiFolder (&soap,
									   soapURL, 
									   NULL, 
									   &ns1__CanBeiFolder, 
									   &ns1__CanBeiFolderResponse);
		if (soap.error) {
			DEBUG_IFOLDER (("****error calling CanBeiFolder***\n"));
			soap_print_fault (&soap, stderr);
			if (soap.error == SOAP_TCP_ERROR) {
				reread_local_service_url ();
			}
		} else {
			DEBUG_IFOLDER (("***calling CanBeiFolder succeeded***\n"));
			if (!ns1__CanBeiFolderResponse.CanBeiFolderResult)
				b_can_be_ifolder = FALSE;
		}

		cleanup_gsoap (&soap);
		g_free (folder_path);
	}

	if (b_can_be_ifolder)
		DEBUG_IFOLDER (("can_be_ifolder returning TRUE"));
	else
		DEBUG_IFOLDER (("can_be_ifolder returning FALSE"));
	
	return b_can_be_ifolder;
}

static gint
create_ifolder_in_domain (NautilusFileInfo *file, char *domain_id)
{
	struct soap soap;
	gchar *folder_path;
	char username[512];
	char password[1024];
	
	folder_path = get_file_path (file);
	if (folder_path != NULL) {
		DEBUG_IFOLDER (("****About to call CreateiFolderInDomain (\"%s\", \"%s\")...\n", folder_path, domain_id));
		struct _ns1__CreateiFolderInDomain req;
		struct _ns1__CreateiFolderInDomainResponse resp;
		req.Path = folder_path;
		req.DomainID = domain_id;
		init_gsoap (&soap);
		if (simias_get_web_service_credential(username, password) == SIMIAS_SUCCESS) {
			soap.userid = username;
			soap.passwd = password;
		}
//printf("Calling iFolderWebService.CreateiFolderInDomain()\n");
		soap_call___ns1__CreateiFolderInDomain (&soap, soapURL, NULL, &req, &resp);
		g_free (folder_path);
		if (soap.error) {
			DEBUG_IFOLDER (("****error calling CreateiFolderInDomain***\n"));
			soap_print_fault (&soap, stderr);
			if (soap.error == SOAP_TCP_ERROR) {
				reread_local_service_url ();
			}
			cleanup_gsoap (&soap);
			return -1;
		} else {
			DEBUG_IFOLDER (("***calling CreateiFolderInDomain succeeded***\n"));
			struct ns1__iFolderWeb *ifolder = resp.CreateiFolderInDomainResult;
			if (ifolder == NULL) {
				DEBUG_IFOLDER (("***The created iFolder is NULL\n"));
				cleanup_gsoap (&soap);
				return -1;
			} else {
				DEBUG_IFOLDER (("***The created iFolder's ID is: %s\n", ifolder->ID));
			}
		}

		cleanup_gsoap (&soap);
	} else {
		/* Error getting the folder path */
		return -1;
	}
	
	return 0;
}

static gint
revert_ifolder (NautilusFileInfo *file)
{
	struct soap soap;
	gchar *folder_path;
	char username[512];
	char password[1024];
	iFolderHolder *holder;

	folder_path = get_file_path (file);
	if (folder_path != NULL) {
		holder = (iFolderHolder *)g_hash_table_lookup (ifolders_ht, folder_path);
		g_free (folder_path);
		if (holder != NULL) {
			DEBUG_IFOLDER (("****About to call RevertiFolder ()\n"));
			struct _ns1__RevertiFolder ns1__RevertiFolder;
			struct _ns1__RevertiFolderResponse ns1__RevertiFolderResponse;
			ns1__RevertiFolder.iFolderID = holder->id;
			init_gsoap (&soap);
			if (simias_get_web_service_credential(username, password) == SIMIAS_SUCCESS) {
				soap.userid = username;
				soap.passwd = password;
			}
//printf("Calling iFolderWebService.RevertiFolder()\n");
			soap_call___ns1__RevertiFolder (&soap, 
												 soapURL, 
												 NULL, 
												 &ns1__RevertiFolder, 
												 &ns1__RevertiFolderResponse);
			if (soap.error) {
				DEBUG_IFOLDER (("****error calling RevertiFolder***\n"));
				soap_print_fault (&soap, stderr);
				if (soap.error == SOAP_TCP_ERROR) {
					reread_local_service_url ();
				}
				cleanup_gsoap (&soap);
				return -1;
			} else {
				DEBUG_IFOLDER (("***calling RevertiFolder succeeded***\n"));
				struct ns1__iFolderWeb *ifolder = 
					ns1__RevertiFolderResponse.RevertiFolderResult;
				if (ifolder == NULL) {
					DEBUG_IFOLDER (("***The reverted iFolder is NULL\n"));
					return -1;
				} else {
					DEBUG_IFOLDER (("***The reverted iFolder's ID was: %s\n", ifolder->ID));
				}
			}

			cleanup_gsoap (&soap);
		}
	} else {
		/* Error getting the folder path */
		return -1;
	}
	
	return 0;
}

static iFolderHolder *
get_ifolder_holder (gchar *ifolder_id)
{
	struct soap soap;
	char username[512];
	char password[1024];
	iFolderHolder *holder;

	holder = NULL;

	if (ifolder_id != NULL) {
		DEBUG_IFOLDER (("****About to call GetiFolder (\"%s\")...\n", ifolder_id));
		struct _ns1__GetiFolder ns1__GetiFolder;
		struct _ns1__GetiFolderResponse ns1__GetiFolderResponse;
		ns1__GetiFolder.iFolderID = ifolder_id;
		init_gsoap (&soap);
		if (simias_get_web_service_credential(username, password) == SIMIAS_SUCCESS) {
			soap.userid = username;
			soap.passwd = password;
		}
//printf("Calling iFolderWebService.GetiFolder()\n");
		soap_call___ns1__GetiFolder (&soap,
									 soapURL,
									 NULL,
									 &ns1__GetiFolder,
									 &ns1__GetiFolderResponse);
		if (soap.error) {
			DEBUG_IFOLDER (("****error calling GetiFolder***\n"));
			soap_print_fault (&soap, stderr);
			if (soap.error == SOAP_TCP_ERROR) {
				reread_local_service_url ();
			}
			cleanup_gsoap (&soap);
			return NULL;
		} else {
			DEBUG_IFOLDER (("***calling GetiFolder succeeded***\n"));
			struct ns1__iFolderWeb *ifolder = 
				ns1__GetiFolderResponse.GetiFolderResult;
			if (ifolder == NULL) {
				DEBUG_IFOLDER (("***GetiFolder returned NULL\n"));
				cleanup_gsoap (&soap);
				return NULL;
			} else {
				/**
				 * If this is a real iFolder and not just a subscription, the
				 * UnManagedPath will NOT be NULL.
				 */
				if (ifolder->UnManagedPath != NULL) {
					DEBUG_IFOLDER (("***The iFolder's Unmanaged Path is: %s\n", ifolder->UnManagedPath));
					holder =
						ifolder_holder_new (ifolder->ID,
											ifolder->DomainID,
											ifolder->UnManagedPath,
											ifolder->Name);
				}
			}
		}

		cleanup_gsoap (&soap);
	}

	return holder;
}

/**
 * Used by sec_reconnect () to invalidate NautilusFileInfo for every known
 * iFolder so that an emblem will be placed on the Nautilus Folder.
 */
static void
invalidate_local_path (gpointer key,
					   gpointer value,
					   gpointer user_data)
{
	gchar *file_path;
	NautilusFileInfo *file;
	char *file_uri;

	file_path = (gchar *)key;

	file_uri = gnome_vfs_get_uri_from_local_path (file_path);
	if (file_uri) {
		file = nautilus_file_get_existing (file_uri);
													 
		if (file) {
			DEBUG_IFOLDER (("invalidate_local_path: %s\n", file_uri));
			
			nautilus_file_info_invalidate_extension_info (file);

			g_object_unref (G_OBJECT(file));
		} else {
			DEBUG_IFOLDER (("\"%s\" existing not found\n", file_uri));
		}

		free (file_uri);
	} else {
		DEBUG_IFOLDER (("invalidate_local_path: gnome_vfs_get_uri_from_local_path (%s) returned NULL", file_path));
	}
}

/**
 * This should only be called by sec_reconnected ().
 */
static void
refresh_ifolders_ht ()
{
	int i;			
	struct soap soap;
	struct _ns1__GetAlliFolders ns1__GetAlliFolders;
	struct _ns1__GetAlliFoldersResponse ns1__GetAlliFoldersResponse;
	char username[512];
	char password[1024];
	iFolderHolder *holder;
	
	DEBUG_IFOLDER (("****About to call GetiFolders ()"));

	init_gsoap (&soap);
	if (simias_get_web_service_credential(username, password) == SIMIAS_SUCCESS) {
		soap.userid = username;
		soap.passwd = password;
	}
//printf("Calling iFolderWebService.GetAlliFolders()\n");
	soap_call___ns1__GetAlliFolders (&soap,
									 soapURL,
									 NULL,
									 &ns1__GetAlliFolders,
									 &ns1__GetAlliFoldersResponse);
	if (soap.error) {
		DEBUG_IFOLDER (("****error calling GetAlliFolders***\n"));
		soap_print_fault (&soap, stderr);
		if (soap.error == SOAP_TCP_ERROR) {
			reread_local_service_url ();
		}
		cleanup_gsoap (&soap);
		return;
	} else {
		DEBUG_IFOLDER (("***calling GetAlliFolders succeeded***\n"));
		struct ns1__ArrayOfIFolderWeb *array_of_ifolders =
			ns1__GetAlliFoldersResponse.GetAlliFoldersResult;
		if (array_of_ifolders == NULL) {
			DEBUG_IFOLDER (("***GetAlliFolders returned NULL\n"));
			cleanup_gsoap (&soap);
			return;
		} else {
			/**
			 * Create a new iFolderHolder struct for every iFolder and add it
			 * to ifolders_ht (GHashTable).
			 */
			DEBUG_IFOLDER (("refresh_ifolders_ht: array_of_ifolders size=%d", array_of_ifolders->__sizeiFolderWeb));
			if (array_of_ifolders->__sizeiFolderWeb > 0) {
				for (i = 0; i < array_of_ifolders->__sizeiFolderWeb; i++) {
					/**
					 * iFolders that are not local iFolders will not have an
					 * UnManagedPath.
					 */
					if (array_of_ifolders->iFolderWeb [i]->UnManagedPath) {
						holder =
							ifolder_holder_new (
								array_of_ifolders->iFolderWeb [i]->ID,
								array_of_ifolders->iFolderWeb [i]->DomainID,
								array_of_ifolders->iFolderWeb [i]->UnManagedPath,
								array_of_ifolders->iFolderWeb [i]->Name);
						if (holder != NULL) {
							g_hash_table_insert (ifolders_ht,
												 holder->unmanaged_path,
												 holder);
							DEBUG_IFOLDER (("refresh_ifolders_ht: added new iFolder=%s", holder->name));
						} else {
							DEBUG_IFOLDER (("ifolder_holder_new returned NULL"));
						}
					}
				}
			}
		}
	}

	DEBUG_IFOLDER (("refresh_ifolders_ht exiting after adding %d iFolders", g_hash_table_size (ifolders_ht)));

	cleanup_gsoap (&soap);
}

/**
 * Nautilus Info Provider Implementation
 */
static NautilusOperationResult
ifolder_nautilus_update_file_info (NautilusInfoProvider		*provider,
								   NautilusFileInfo			*file,
								   GClosure					*update_complete,
								   NautilusOperationHandle	**handle)
{
	gchar *file_uri;
	gchar *file_path;
	iFolderHolder *holder;
	DEBUG_IFOLDER (("ifolder_nautilus_update_file_info called\n"));
	
	/* Don't do anything if the specified file is not a directory. */
	if (!nautilus_file_info_is_directory (file))
		return NAUTILUS_OPERATION_COMPLETE;
	
	if (is_ifolder_running ()) {
		file_path = get_file_path (file);
		if (file_path) {
			holder = (iFolderHolder *)g_hash_table_lookup (ifolders_ht, file_path);
			g_free (file_path);
			if (holder) {
				nautilus_file_info_add_emblem (file, "ifolder");

				file_uri = nautilus_file_info_get_uri (file);
				if (file_uri) {
					/**
					 * Store the file_uri into a hashtable with the key being
					 * the iFolder Simias Node ID.  This is needed because when
					 * we get a SimiasNodeDeleted event, the iFolder in Simias
					 * no longer has any path information.  This hash table is
					 * the only way we'll be able to cause Nautilus to
					 * invalidate our information so that the iFolder emblem
					 * will be removed.
					 */
					DEBUG_IFOLDER (("Adding iFolder to Hashtable: %s = %s\n", holder->id, file_uri));
					
					/**
					 * The memory for ifolder_id and file_uri are freed when the
					 * hashtable item is removed from the hashtable.  If
					 * there's already an existing item, it is freed because
					 * we supplied a value_destroyed_func & key_destroyed_func.
					 */
					g_hash_table_insert (seen_ifolders_ht,
										 strdup(holder->id),	/* key */
										 file_uri);		/* value */
				}
			}
		}
	} else {
		DEBUG_IFOLDER (("*** iFolder is NOT running\n"));
	}

	return NAUTILUS_OPERATION_COMPLETE;
}

static void
ifolder_nautilus_info_provider_iface_init (NautilusInfoProviderIface *iface)
{
	iface->update_file_info	= ifolder_nautilus_update_file_info;
}

/**
 * Functions for ifolders_ht (GHashTable)
 */
static void
ifolders_ht_destroy_key (gpointer key)
{
	/**
	 * We use the iFolderHolder->unmanaged_path, which is freed inside
	 * ifolders_ht_destroy_value () so we don't need to free anything here.
	 */
}

static void
ifolders_ht_destroy_value (gpointer value)
{
	iFolderHolder *holder = (iFolderHolder *)value;
	
	ifolder_holder_free (&holder);
}

/**
 * Functions for seen_ifolders_ht (GHashTable)
 */
/**
 * This function gets called when the entry is being removed and this allows us
 * to cleanup the memory being used by the ifolder_id.
 */
static void
seen_ifolders_ht_destroy_key (gpointer key)
{
	char *ifolder_id = (char *)key;
	
	free (ifolder_id);
}

/**
 * This function gets called when the entry is being removed and this allows us
 * to cleanup the memory being used by the file_uri.
 */
static void
seen_ifolders_ht_destroy_value (gpointer value)
{
	gchar *file_uri = (gchar *)value;
	
	g_free (file_uri);
}

static void
seen_ifolders_ht_invalidate_ifolder (gpointer key, gpointer value, gpointer user_data)
{
	gchar *file_uri;
	NautilusFileInfo *file;
	
	file_uri = (gchar *)value;
	file = nautilus_file_get_existing (file_uri);

	if (file) {
		DEBUG_IFOLDER (("seen_ifolders_ht_invalidate_ifolder: %s\n", file_uri));

		nautilus_file_info_invalidate_extension_info (file);

		g_object_unref (G_OBJECT(file));
	}
}

static
gboolean show_created_dialog ()
{
	GConfClient *client;
	gboolean b_show_created_dialog;
	
	client = gconf_client_get_default();
	if (client == NULL)
		return TRUE;
	
	b_show_created_dialog = gconf_client_get_bool(client, KEY_SHOW_CREATION, NULL);
	
	g_object_unref(G_OBJECT(client));
	
	return b_show_created_dialog;
}

/**
 * Nautilus Menu Provider Implementation
 */

static gboolean
show_ifolder_error_message (void *user_data)
{
	DEBUG_IFOLDER (("*** show_ifolder_error_message () called\n"));
	iFolderErrorMessage *errMsg = (iFolderErrorMessage *)user_data;
	GtkDialog *message_dialog;

	message_dialog = gtk_message_dialog_new(GTK_WINDOW(errMsg->window), 
						GTK_DIALOG_MODAL | GTK_DIALOG_DESTROY_WITH_PARENT, 
						GTK_MESSAGE_ERROR, 
						GTK_BUTTONS_OK, 
						errMsg->message);

	gtk_dialog_run (message_dialog);
	gtk_object_destroy (GTK_OBJECT (message_dialog));
	
	free (errMsg);
	
	return FALSE;
}

static void
creation_dialog_button_callback (GtkDialog *dialog,
								 gint response_type,
								 gpointer data)
{
	GConfClient *client;

	switch (response_type) {
		case GTK_RESPONSE_HELP:
			/* Launch the iFolder Help. */
			g_object_set_data_full (G_OBJECT (dialog), "parent_window",
									g_object_ref (dialog), g_object_unref);
			/**
			 * To use the function that's already available, we have to pass in
			 * the dialog as a NautilusMenuItem * and have the parent_window set.
			 */
			ifolder_help_callback ((NautilusMenuItem *)dialog, NULL);
			break;
		case GTK_RESPONSE_CLOSE:
			if (gtk_toggle_button_get_active (GTK_TOGGLE_BUTTON (data))) {
				/* Save off the setting to NOT show this dialog again */
				client = gconf_client_get_default();
				if (client != NULL)
				{
					gconf_client_set_bool(client, KEY_SHOW_CREATION, FALSE, NULL);
					g_object_unref(G_OBJECT(client));
				}
			}
			
			gtk_widget_destroy (GTK_WIDGET (dialog));
			break;
		default:
			/* Do nothing since the dialog was cancelled */
			break;
	}
}

static gboolean
show_ifolder_created_dialog (void *user_data)
{
	DEBUG_IFOLDER (("*** show_ifolder_created_dialog () called\n"));
	GtkWidget *creation_dialog, *label, *check_button, *vbox, *vbox2, *hbox,
			  *cb_alignment, *folder_image;
	GtkWindow *parent_window;
	gint response;
	parent_window = GTK_WINDOW (user_data);

	creation_dialog = gtk_dialog_new_with_buttons (
						_("iFolder Created"),
						parent_window,
						GTK_DIALOG_MODAL | GTK_DIALOG_DESTROY_WITH_PARENT,
						GTK_STOCK_CLOSE,	/* Close button */
						GTK_RESPONSE_CLOSE,
						GTK_STOCK_HELP,		/* Help button */
						GTK_RESPONSE_HELP,
						NULL);
						
	gtk_dialog_set_has_separator (GTK_DIALOG (creation_dialog), FALSE);
	gtk_window_set_resizable (GTK_WINDOW (creation_dialog), FALSE);
	
	gtk_window_set_icon_from_file (GTK_WINDOW (creation_dialog),
								   IFOLDER_IMAGE_IFOLDER,
								   NULL);
	folder_image = gtk_image_new_from_file (IFOLDER_IMAGE_BIG_IFOLDER);

	vbox = gtk_vbox_new (FALSE, 10);
	gtk_container_border_width (GTK_CONTAINER (vbox), 10);

	hbox = gtk_hbox_new (FALSE, 12);

	gtk_misc_set_alignment (GTK_MISC (folder_image), 0.5, 0);
	gtk_box_pack_start (GTK_BOX (hbox), folder_image, FALSE, FALSE, 0);
	
	vbox2 = gtk_vbox_new (FALSE, 10);
	
	label = gtk_label_new (_("The folder you selected is now an iFolder.  To learn more about using iFolder and sharing iFolders with other users, see \"Managing iFolders\" in iFolder Help."));
	gtk_label_set_line_wrap		(GTK_LABEL (label), TRUE);
	gtk_misc_set_alignment (GTK_MISC (label), 0, 0.5);
	gtk_box_pack_start (GTK_BOX (vbox2), label, TRUE, TRUE, 0);
	
	gtk_box_pack_end (GTK_BOX (hbox), vbox2, TRUE, TRUE, 0);
	
	gtk_box_pack_start (GTK_BOX (vbox), hbox, FALSE, FALSE, 0);

	cb_alignment = gtk_alignment_new (1, 1, 1, 0);
	check_button = gtk_check_button_new_with_mnemonic (_("Do not show this message again."));
	gtk_container_add (GTK_CONTAINER (cb_alignment), check_button);
	gtk_box_pack_start (GTK_BOX (vbox), cb_alignment, TRUE, TRUE, 0);

	gtk_container_add (GTK_CONTAINER (GTK_DIALOG (creation_dialog)->vbox),
					   vbox);
					   
	/* Hook up the signal callbacks for the buttons */
	g_signal_connect (creation_dialog,
					  "response",
					  G_CALLBACK (creation_dialog_button_callback),
					  check_button);
			   
	gtk_widget_show_all (creation_dialog);
	
	return FALSE;
}

/**
 * If this function returns NON-NULL, it contains a char * from the process
 * executed by popen and should be freed.  The char * only contains the first
 * line of output from the executed process.
 */
static void *
ifolder_dialog_thread (gpointer user_data)
{
	NautilusMenuItem *item;
	FILE *output;
	char readBuffer [1024];
	char *args = (char *)user_data;
	char *return_str = NULL;

	item = (NautilusMenuItem *)user_data;
	args = g_object_get_data (G_OBJECT (item), "ifolder_args");
	
	memset (readBuffer, '\0', sizeof (readBuffer));

	output = popen (args, "r");
	if (output == NULL) {
		/* error calling mono nautilus-ifolder.exe */
		DEBUG_IFOLDER (("Error calling: %s\n", args));
		free (args);
		iFolderErrorMessage *errMsg = malloc (sizeof (iFolderErrorMessage));
		errMsg->window = g_object_get_data (G_OBJECT (item), "parent_window");
		errMsg->title	= _("");
		errMsg->message	= _("Error opening dialog.");
		errMsg->detail	= _("Sorry, unable to open the window to perform the specified action.");
		g_idle_add (show_ifolder_error_message, errMsg);
		g_object_unref(item);
		return;
	}
	
	if (fgets (readBuffer, 1024, output) != NULL) {
		return_str = strdup (readBuffer);
		DEBUG_IFOLDER (("*** 1st line of STDOUT from popen: %s\n", return_str));
	}

	free (args);
	pclose (output);
	
	g_object_unref(item);
	return (void *)return_str;
}

static void *
create_ifolder_thread (gpointer user_data)
{
	NautilusMenuItem *item;
	GList *files;
	NautilusFileInfo *file;
	gint error;
	char *domain_id;
	
	item = (NautilusMenuItem *)user_data;
	files = g_object_get_data (G_OBJECT (item), "files");
	domain_id = (char *)g_object_get_data (G_OBJECT (item), "domain_id");
	file = NAUTILUS_FILE_INFO (files->data);

	error = create_ifolder_in_domain (file, domain_id);
	if (error) {
		DEBUG_IFOLDER (("An error occurred creating an iFolder\n"));
		iFolderErrorMessage *errMsg = malloc (sizeof (iFolderErrorMessage));
		errMsg->window = g_object_get_data (G_OBJECT (item), "parent_window");
		errMsg->title	= _("");
		errMsg->message	= _("The folder could not be converted.");
		errMsg->detail	= _("Sorry, unable to convert the specified folder into an iFolder.");
		g_idle_add (show_ifolder_error_message, errMsg);
	} else {
		if (show_created_dialog ()) {
			g_idle_add (show_ifolder_created_dialog,
						g_object_get_data (G_OBJECT (item), "parent_window"));
		}
	}

	free (domain_id);
	g_object_unref(item);
}

static void
create_ifolder_callback (NautilusMenuItem *item, gpointer user_data)
{
	pthread_t thread;

	/* Prompt the user for what domain to use */
	GtkWidget *dialog;
	GtkWidget *vbox;
	GtkWidget *path_label;
	GtkWidget *path_entry;
	GtkWidget *domain_label;
	GtkWidget *domain_menu;
	gint response;
	int result;
	char domain_name[1024];
	char guid[512];
	
	GtkListStore *domain_store;
	GtkTreeIter iter;
	GtkCellRenderer *renderer;
	char *domain_id;

	int default_domain_idx = 0;
	
	SimiasDomainInfo **domainsA;
	SimiasDomainInfo *domain;
	int i;
	
	GtkWidget *parent_window;

	GList *files;
	NautilusFileInfo *file;
	gchar *file_path;

	parent_window = g_object_get_data (G_OBJECT (item), "parent_window");
	
	/**
	 * Make the call to get the list of domains and only continue if the call
	 * is successful (no sense wasting time/effort dealing with a dialog if
	 * there's nothing to fill it with.
	 */

	if (simias_get_domains(false, &domainsA) != SIMIAS_SUCCESS) {
		/* FIXME: Display an error to the user that we couldn't get the list of domains */
		return;
	}
	
	/**
	 * If we made it this far, we have a list of domains and can fill the
	 * GtkListStore with them so they're ready to be displayed in the dialog.
	 */
	domain_store = gtk_list_store_new(2, G_TYPE_STRING, G_TYPE_STRING);
	/*                                    Domain Name    Domain ID            */
	/*                                                   (Not shown)          */

	i = 0;
	domain = domainsA[i];

	if (!domain) {
		iFolderErrorMessage *errMsg = malloc (sizeof (iFolderErrorMessage));
		errMsg->window = parent_window;
		errMsg->title	= _("Create iFolder");
		errMsg->message	= _("No iFolder Domains");
		errMsg->detail	= _("A new iFolder cannot be created because you have not attached to any iFolder domains.");
		g_idle_add (show_ifolder_error_message, errMsg);
		return;
	}

	while (domain) {
		gtk_list_store_append(domain_store, &iter);
		gtk_list_store_set(domain_store, &iter,
						   0, strdup(domain->name),
						   1, strdup(domain->id),
						   -1);
		
		if (domain->is_default) {
			default_domain_idx = i;
		}

		domain = domainsA[++i];
	}
	
	/* Cleanup the memory used by domainsA */
	simias_free_domains(&domainsA);

	dialog = gtk_dialog_new_with_buttons(_("Convert to an iFolder"),
			GTK_WINDOW(parent_window),
			GTK_DIALOG_DESTROY_WITH_PARENT,
			GTK_STOCK_CANCEL,
			GTK_RESPONSE_CANCEL,
			GTK_STOCK_OK,
			GTK_RESPONSE_ACCEPT,
			NULL);

	gtk_dialog_set_has_separator(GTK_DIALOG(dialog), FALSE);
	gtk_dialog_set_default_response(GTK_DIALOG(dialog), GTK_RESPONSE_ACCEPT);

	vbox = gtk_vbox_new(FALSE, 10);
	gtk_container_border_width(GTK_CONTAINER(vbox), 10);
	gtk_container_add(GTK_CONTAINER(GTK_DIALOG(dialog)->vbox), vbox);

	/* Domain drop-down list */
	domain_label = gtk_label_new(NULL);
	gtk_label_set_markup_with_mnemonic(GTK_LABEL(domain_label), _("<b>_Account:</b>"));
	gtk_misc_set_alignment(GTK_MISC(domain_label), 0, 0.5);
	gtk_box_pack_start(GTK_BOX(vbox), domain_label, FALSE, FALSE, 0);
	
	domain_menu = gtk_combo_box_new_with_model(GTK_TREE_MODEL(domain_store));
	gtk_label_set_mnemonic_widget(GTK_LABEL(domain_label), domain_menu);

	/* Only show the Domain Name in the drop-down */	
	renderer = gtk_cell_renderer_text_new();
	gtk_cell_layout_pack_start(GTK_CELL_LAYOUT(domain_menu), renderer, TRUE);
	gtk_cell_layout_set_attributes(GTK_CELL_LAYOUT(domain_menu), renderer,
								   "text", 0, NULL);

	/* Select the first in the list or the default */
	gtk_combo_box_set_active(GTK_COMBO_BOX(domain_menu), default_domain_idx);
	
	gtk_box_pack_start(GTK_BOX(vbox), domain_menu, FALSE, FALSE, 0);
	
	/* Folder Path */
	path_label = gtk_label_new(NULL);
	gtk_label_set_markup_with_mnemonic(GTK_LABEL(path_label), _("<b>_Location:</b>"));
	gtk_misc_set_alignment(GTK_MISC(path_label), 0, 0.5);
	gtk_box_pack_start(GTK_BOX(vbox), path_label, FALSE, FALSE, 0);

	/* Get the Folder's Path to fill in the path_entry */
	files = g_object_get_data (G_OBJECT (item), "files");
	file = NAUTILUS_FILE_INFO (files->data);
	file_path = get_file_path (file);
	
	path_entry = gtk_label_new(file_path);
	gtk_misc_set_alignment(GTK_MISC(path_entry), 0, 0.5);

	g_free(file_path);
	
	gtk_box_pack_start(GTK_BOX(vbox), path_entry, FALSE, FALSE, 0);
	
	gtk_widget_show_all(vbox);

	response = gtk_dialog_run(GTK_DIALOG(dialog));

	if (response == GTK_RESPONSE_ACCEPT) {
		/**
		 * Get the Default ID of the Domain that was selected and make the call
		 * to create the iFolder.
		 */
		if (!gtk_tree_model_iter_nth_child(GTK_TREE_MODEL(domain_store),
										   &iter, NULL,
										   gtk_combo_box_get_active(
										   		GTK_COMBO_BOX(domain_menu)))) {
			/* FIXME: Let the user know nothing was selected */
			gtk_widget_destroy(dialog);
			return;
		}

		gtk_tree_model_get(GTK_TREE_MODEL(domain_store), &iter,
							1, &domain_id,
							-1);
		
		if (!domain_id) {
			/* FIXME: Let the user know there was some type of an error */
			gtk_widget_destroy(dialog);
			return;
		}
		
		/**
		 * Make the call to create the iFolder.  The create_ifolder_thread will
		 * have to free the memory being used by the domain_id char *.
		 */
		g_object_set_data (G_OBJECT (item), "domain_id", domain_id);

		/**
		 * Increment the reference count on the NautilusMenuItem * so
		 * it isn't just destroyed without our knowledge.  The
		 * create_ifolder_thread() will unref the object.
		 */
		g_object_ref(item);
		pthread_create (&thread, 
						NULL, 
						create_ifolder_thread,
						item);
	}

	gtk_widget_destroy(dialog);
}

static void *
revert_ifolder_thread (gpointer user_data)
{
	NautilusMenuItem *item;
	GList *files;
	NautilusFileInfo *file;
	gint error;

	item = (NautilusMenuItem *)user_data;
	files = g_object_get_data (G_OBJECT (item), "files");
	file = NAUTILUS_FILE_INFO (files->data);
	
	error = revert_ifolder (file);
	if (error) {
		DEBUG_IFOLDER (("An error occurred reverting an iFolder\n"));
		iFolderErrorMessage *errMsg = malloc (sizeof (iFolderErrorMessage));
		errMsg->window = g_object_get_data (G_OBJECT (item), "parent_window");
		errMsg->title	= _("");
		errMsg->message	= _("The iFolder could not be reverted.");
		errMsg->detail	= _("Sorry, unable to revert the specified iFolder to a normal folder.");
		g_idle_add (show_ifolder_error_message, errMsg);
	}

	g_object_unref(item);
}

static void
revert_ifolder_callback (NautilusMenuItem *item, gpointer user_data)
{
	GtkDialog *message_dialog;
	GtkWidget *window;
	int response;
	pthread_t thread;

	window = g_object_get_data (G_OBJECT (item), "parent_window");

	message_dialog = gtk_message_dialog_new(window, 
						 GTK_DIALOG_MODAL | GTK_DIALOG_DESTROY_WITH_PARENT, 
						 GTK_MESSAGE_QUESTION, GTK_BUTTONS_YES_NO, 
						 "Reverting iFolder");
	gtk_message_dialog_format_secondary_text(message_dialog, "This reverts the iFolder back to a normal folder and leaves the files intact.  The iFolder is then available from the server and must be set up in a different location to synchronize. - Johnny");
	gtk_widget_show_all(message_dialog);
//	gtk_dialog_set_default_response (message_dialog, GTK_RESPONSE_CANCEL);

	response = gtk_dialog_run (message_dialog);
	gtk_object_destroy (GTK_OBJECT (message_dialog));
	
	if (response == GTK_RESPONSE_YES) {
		g_object_ref(item);
		pthread_create (&thread, 
						NULL, 
						revert_ifolder_thread,
						item);
	}
}

static void
share_ifolder_callback (NautilusMenuItem *item, gpointer user_data)
{
	gchar *ifolder_path;
	iFolderHolder *holder;
	GList *files;
	NautilusFileInfo *file;
	pthread_t thread;
	char args [1024];
	memset (args, '\0', sizeof (args));
	
	files = g_object_get_data (G_OBJECT (item), "files");
	file = NAUTILUS_FILE_INFO (files->data);
	if (file == NULL)
		return;
		
	ifolder_path = get_file_path (file);
	if (ifolder_path != NULL) {
		holder = (iFolderHolder *)g_hash_table_lookup (ifolders_ht, ifolder_path);
		if (holder != NULL) {
			sprintf (args, "%s share %s", NAUTILUS_IFOLDER_SH_PATH, holder->id);
		}
		
		g_free (ifolder_path);
	}
	
	if (strlen (args) <= 0)
		return;
		
	g_object_set_data (G_OBJECT (item),
				"ifolder_args",
				strdup(args));
	
	g_object_ref(item);
	pthread_create (&thread, 
					NULL, 
					ifolder_dialog_thread,
					item);
}

static void
ifolder_properties_callback (NautilusMenuItem *item, gpointer user_data)
{
	gchar *ifolder_path;
	iFolderHolder *holder;
	GList *files;
	NautilusFileInfo *file;
	pthread_t thread;
	char args [1024];
	memset (args, '\0', sizeof (args));
	
	files = g_object_get_data (G_OBJECT (item), "files");
	file = NAUTILUS_FILE_INFO (files->data);
	if (file == NULL)
		return;
		
	ifolder_path = get_file_path (file);
	if (ifolder_path != NULL) {
		holder = (iFolderHolder *)g_hash_table_lookup (ifolders_ht, ifolder_path);
		if (holder != NULL) {
			sprintf (args, "%s properties %s", 
					 NAUTILUS_IFOLDER_SH_PATH, holder->id);
		}
		
		g_free (ifolder_path);
	}
	
	if (strlen (args) <= 0)
		return;
		
	g_object_set_data (G_OBJECT (item),
				"ifolder_args",
				strdup(args));
		
	g_object_ref(item);
	pthread_create (&thread, 
					NULL, 
					ifolder_dialog_thread,
					item);
}

static void
ifolder_help_callback (NautilusMenuItem *item, gpointer user_data)
{
	pthread_t thread;
	char args [1024];
	memset (args, '\0', sizeof (args));
	
	sprintf (args, "%s help", NAUTILUS_IFOLDER_SH_PATH);
	
	if (strlen (args) <= 0)
		return;
		
	g_object_set_data (G_OBJECT (item),
				"ifolder_args",
				strdup(args));

	g_object_ref(item);
	pthread_create (&thread, 
					NULL, 
					ifolder_dialog_thread,
					item);
}

static GList *
ifolder_nautilus_get_file_items (NautilusMenuProvider *provider,
								 GtkWidget *window,
								 GList *files)
{
	DEBUG_IFOLDER (("ifolder_nautilus_get_file_items called\n"));
	NautilusMenuItem *item;
	NautilusFileInfo *file;
	GList *items;

	/* For some reason, this function is called with files == NULL */
	if (files == NULL)
		return NULL;

	/**
	 * Multiple select on a file/folder is not supported.  If the user has
	 * selected more than one file/folder, don't add any iFolder context menus.
	 */
	if (g_list_length (files) > 1)
		return NULL;

	file = NAUTILUS_FILE_INFO (files->data);

	/**
	 * If the user selected a file (not a directory), don't add any iFolder
	 * context menus.
	 */
	if (!nautilus_file_info_is_directory (file))
		return NULL;
		
	/**
	 * Don't show any iFolder context menus if the iFolder client is not
	 * running
	 */
	if (!is_ifolder_running ())
		return NULL;
		
	items = NULL;
	
	if (is_ifolder (file)) {
DEBUG_IFOLDER (("This is an iFolder"));
		/* Menu item: Revert to a Normal Folder */
		item = nautilus_menu_item_new ("NautilusiFolder::revert_ifolder",
					_("Revert to a Normal Folder"),
					_("Revert the selected iFolder back to normal folder"),
					"stock_undo");
		g_signal_connect (item, "activate",
					G_CALLBACK (revert_ifolder_callback),
					provider);
		g_object_set_data (G_OBJECT (item),
					"files",
					nautilus_file_info_list_copy (files));
		g_object_set_data_full (G_OBJECT (item), "parent_window",
								g_object_ref (window), g_object_unref);
		items = g_list_append (items, item);
		
		/* Menu item: Share with... */
		item = nautilus_menu_item_new ("NautilusiFolder::share_ifolder",
					_("Share iFolder with..."),
					_("Share the selected iFolder with another user"),
					NULL);
		g_signal_connect (item, "activate",
					G_CALLBACK (share_ifolder_callback),
					provider);
		g_object_set_data (G_OBJECT (item),
					"files",
					nautilus_file_info_list_copy (files));
		g_object_set_data_full (G_OBJECT (item), "parent_window",
								g_object_ref (window), g_object_unref);
		items = g_list_append (items, item);
		
		/* Menu item: Properties */
		item = nautilus_menu_item_new ("NautilusiFolder::ifolder_properties",
					_("iFolder Properties"),
					_("View the properties of the selected iFolder"),
					"stock_properties");
		g_signal_connect (item, "activate",
					G_CALLBACK (ifolder_properties_callback),
					provider);
		g_object_set_data (G_OBJECT (item),
					"files",
					nautilus_file_info_list_copy (files));
		g_object_set_data_full (G_OBJECT (item), "parent_window",
								g_object_ref (window), g_object_unref);
		items = g_list_append (items, item);
		
		/* Menu item: Help */
		item = nautilus_menu_item_new ("NautilusiFolder::ifolder_help",
					_("iFolder Help..."),
					_("View the iFolder help"),
					"stock_help");
		g_signal_connect (item, "activate",
					G_CALLBACK (ifolder_help_callback),
					provider);
		g_object_set_data (G_OBJECT (item),
					"files",
					nautilus_file_info_list_copy (files));
		g_object_set_data_full (G_OBJECT (item), "parent_window",
								g_object_ref (window), g_object_unref);
		items = g_list_append (items, item);
	} else {
		/**
		 * If the iFolder API says that the current file cannot be an iFolder,
		 * don't add any iFolder context menus.
		 */
		if (!can_be_ifolder (file))
			return NULL;

		/* Menu item: Convert to an iFolder */
		item = nautilus_menu_item_new ("NautilusiFolder::create_ifolder",
					_("Convert to an iFolder..."),
					_("Convert the selected folder to an iFolder"),
					IFOLDER_IMAGE_IFOLDER);
//					"ifolder-folder");
		g_signal_connect (item, "activate",
					G_CALLBACK (create_ifolder_callback),
					provider);
		g_object_set_data (G_OBJECT (item),
					"files",
					nautilus_file_info_list_copy (files));
		g_object_set_data_full (G_OBJECT (item), "parent_window",
								g_object_ref (window), g_object_unref);
		items = g_list_append (items, item);
	}

	return items;
}
 
static void
ifolder_nautilus_menu_provider_iface_init (NautilusMenuProviderIface *iface)
{
	iface->get_file_items = ifolder_nautilus_get_file_items;
}

static GType
ifolder_nautilus_get_type (void)
{
	return ifolder_nautilus_type;
}


static void
ifolder_nautilus_instance_init (iFolderNautilus *ifn)
{
}

static void
ifolder_nautilus_class_init (iFolderNautilusClass *klass)
{
	parent_class = g_type_class_peek_parent (klass);
}

static void
ifolder_extension_register_type (GTypeModule *module)
{
	static const GTypeInfo info = {
		sizeof (iFolderNautilusClass),
		(GBaseInitFunc) NULL,
		(GBaseFinalizeFunc) NULL,
		(GClassInitFunc) ifolder_nautilus_class_init,
		NULL,
		NULL,
		sizeof (iFolderNautilus),
		0,
		(GInstanceInitFunc) ifolder_nautilus_instance_init,
	};
	
	ifolder_nautilus_type = g_type_module_register_type (module,
														  G_TYPE_OBJECT,
														  "NautilusiFolder",
														  &info, 0);
														  
	/* Nautilus Info Provider Interface */
	static const GInterfaceInfo info_provider_iface_info = 
	{
		(GInterfaceInitFunc)ifolder_nautilus_info_provider_iface_init,
		NULL,
		NULL
	};
	g_type_module_add_interface (module, 
								 ifolder_nautilus_type,
								 NAUTILUS_TYPE_INFO_PROVIDER,
								 &info_provider_iface_info);
								 
	/* Nautilus Menu Provider Interface */
	static const GInterfaceInfo menu_provider_iface_info = 
	{
		(GInterfaceInitFunc)ifolder_nautilus_menu_provider_iface_init,
		NULL,
		NULL
	};
	g_type_module_add_interface (module, 
								 ifolder_nautilus_type,
								 NAUTILUS_TYPE_MENU_PROVIDER,
								 &menu_provider_iface_info);
	/* Nautilus Property Page Interface */
	/* Nautilus Column Provider Interface (we probably won't need this one) */
}

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

void
nautilus_module_initialize (GTypeModule *module)
{
	fprintf(stdout, "Initializing nautilus-ifolder extension\n");

	ifolder_extension_register_type (module);
	bindtextdomain (GETTEXT_PACKAGE, GNOMELOCALEDIR);
	bind_textdomain_codeset (GETTEXT_PACKAGE, "UTF-8");

	provider_types[0] = ifolder_nautilus_get_type ();
	
	b_nautilus_ifolder_running = TRUE;
	
	soapURL = getLocalServiceUrl ();
	
	/* Initialize the seen_ifolders_ht GHashTable */
	seen_ifolders_ht = 
		g_hash_table_new_full (g_str_hash, g_str_equal,
							   seen_ifolders_ht_destroy_key,
							   seen_ifolders_ht_destroy_value);
	
	/* Initialize the ifolders_ht GHashTable */
	ifolders_ht =
		g_hash_table_new_full (g_str_hash, g_str_equal,
							   ifolders_ht_destroy_key,
							   ifolders_ht_destroy_value);
							   
	/* Start the Simias Event Client */
	start_simias_event_client ();
}

void
nautilus_module_shutdown (void)
{
	DEBUG_IFOLDER (("Shutting down nautilus-ifolder extension\n"));

	b_nautilus_ifolder_running = FALSE;

	/* Cleanup soapURL */	
	if (soapURL) {
		free (soapURL);
	}

	/* Cleanup the Simias Event Client */	
	if (sec_deregister (ec) != 0) {
		DEBUG_IFOLDER (("sec_deregister failed\n"));
		return;
	}
	
	/**
	 * Since we called sec_init (), call sec_cleanup () regardless if the event
	 * client is running or not.
	 */
	if (sec_cleanup (&ec) != 0) {
		DEBUG_IFOLDER (("sec_cleanup failed\n"));
		return;
	}
	
	/* Cleanup the seen_ifolders_ht GHashTable */
	g_hash_table_destroy (seen_ifolders_ht);
}

void
nautilus_module_list_types (const GType **types, int *num_types)
{
	*types = provider_types;
	*num_types = G_N_ELEMENTS (provider_types);
}
