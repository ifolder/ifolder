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

#include <eel/eel-stock-dialogs.h>

#include <gtk/gtk.h>
#include <glib/gi18n-lib.h>
#include <string.h>
#include <stdio.h>
#include <unistd.h>

#include "iFolderClientStub.h"
#include "iFolderClient.nsmap"

#include "nautilus-ifolder.h"

#define IFOLDER_FIFO_NAME ".nautilus-ifolder-fifo"
#define IFOLDER_BUF_SIZE 1024

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

static GType provider_types[1];

static void ifolder_extension_register_type (GTypeModule *module);
static void ifolder_nautilus_instance_init (iFolderNautilus *ifn);

static GObjectClass * parent_class = NULL;
static GType ifolder_nautilus_type;

/**
 * FIXME: Once nautilus-extension provides an API for being able to randomly
 * get a NautilusFileInfo * by specifying a file URI, this code that saves off
 * all the NautilusFileInfo * can be removed/replaced.
 * 
 * seen_nautilus_file_infos will only be a collection of valid NautilusFileInfo
 * objects indexed by the file uri.
 */
static GHashTable *seen_nautilus_file_infos;

/* This function is called when the GObject (NautilusFileInfo) is finalized. */
static void
nautilus_file_info_weak_ref_cb (gpointer user_data, GObject *old_object_loc)
{
	/**
	 * user_data points to the memory address of the file uri which is the
	 * key into the hash table.
	 */
	gchar *uri = (gchar *)user_data;
	g_hash_table_remove (seen_nautilus_file_infos, uri);
	
	/* free the memory used by uri */
	g_free (uri);
}

static gboolean
nautilus_file_info_foreach_remove (gpointer key, 
								   gpointer value, 
								   gpointer user_data)
{
	g_object_weak_unref (G_OBJECT (value), 
						 nautilus_file_info_weak_ref_cb, 
						 user_data);
						 
	/**
	 * key is the file uri and now that we're removing it from the hash table
	 * its memory needs to be freed
	 */
	gchar *uri = (gchar *)key;
	g_free (uri);
						 
	return TRUE;
}

/**
 * This function is intended to be called using g_idle_add by the event system
 * to let Nautilus invalidate the extension info for the given 
 * NautilusFileInfo *.
 */
gboolean
invalidate_ifolder_extension_info (void *user_data)
{
	NautilusFileInfo *file = (NautilusFileInfo *)user_data;
	if (file) {
		nautilus_file_info_invalidate_extension_info (file);
	}
}

/**
 * Named Pipe event implementation
 * 
 * Until a C library for the iFolder event system is provided, this named pipe
 * event implementation will allow us to receive events from the Mono/Gtk
 * iFolder client when a new iFolder is created or when an iFolder is reverted.
 */
 
/**
 * The purpose of this thread/function is to listen/read folder paths sent by
 * the Mono/Gtk iFolder client.  When they are received, they will be looked up
 * in the seen_nautilus_file_infos hash table and if found, the extension info
 * will be invalidated so that the emblem is added or removed accordingly.
 */
static pthread_t named_pipe_thread;

static void *
named_pipe_listener_thread (gpointer user_data)
{
	char fifo_path [IFOLDER_BUF_SIZE];
	char s [IFOLDER_BUF_SIZE];
	char file_path [IFOLDER_BUF_SIZE];
	int num, fd, err;
	
	sprintf (fifo_path, "%s/%s", g_get_home_dir (), IFOLDER_FIFO_NAME);
	err = mknod (fifo_path, S_IFIFO | 0600, 0);
	if (err == -1 && errno != EEXIST) {
		g_print ("Couldn't make the event FIFO\n");
		perror ("mknode");
		return;
	}
	
	/* As long as this extension is running, wait for events */
	for (;;) {
		g_print ("Waiting for an event from iFolder...\n");
		fd = open (fifo_path, O_RDONLY);
		if (fd == -1) {
			perror ("open");
			continue;
		}
		
		g_print ("iFolder Client connected\n");
		
		do {
			if ((num = read (fd, s, IFOLDER_BUF_SIZE)) == -1) {
				perror ("read");
				break;
			} else {
				s [num] = '\0';
				g_printf ("iFolder Client changed folder: %s\n", s);
				sprintf (file_path, "file://%s", s);

				NautilusFileInfo *file = 
					(NautilusFileInfo *)g_hash_table_lookup (
						seen_nautilus_file_infos,
						file_path);
															 
				if (file) {
					/* Let nautilus run this in the main loop */
					g_idle_add (invalidate_ifolder_extension_info, file);
				}
			}
		} while (num > 0);
		
		err = close (fd);
		if (err == -1) {
			perror ("close");
		}
	}
}

/**
 * gSOAP
 */
char *soapURL = NULL;

static void
init_gsoap (struct soap *p_soap)
{
	/* Initialize gSOAP */
	soap_init (p_soap);
	soap_set_namespaces (p_soap, iFolderClient_namespaces);
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
gchar *
get_file_path (NautilusFileInfo *file)
{
	gchar *file_path, *uri;

	file_path = NULL;

	uri = nautilus_file_info_get_uri (file);
	if (!strncmp (uri, "file://", 7))
	{
		file_path = g_strdup (uri + 7);
	}
	
	g_free (uri);
	
	return file_path;
}

/**
 * Calls to iFolder via GSoap
 */
gboolean
is_ifolder_running ()
{
	struct soap soap;
	gboolean b_is_ifolder_running = TRUE;
	int err_code;

	struct _ns1__Ping ns1__Ping;
	struct _ns1__PingResponse ns1__PingResponse;

	init_gsoap (&soap);
	err_code = soap_call___ns1__Ping (&soap,
										soapURL,
						   				NULL,
						   				&ns1__Ping,
						   				&ns1__PingResponse);
						   				
	if (err_code != SOAP_OK || soap.error) {
		b_is_ifolder_running = FALSE;
	}
	
	cleanup_gsoap (&soap);
	
	return b_is_ifolder_running;
}
 
gboolean
is_ifolder (NautilusFileInfo *file)
{
	struct soap soap;
	gboolean b_is_ifolder = FALSE;
	gchar *folder_path;
	
	folder_path = get_file_path (file);
	if (folder_path != NULL) {
		g_print ("****About to call IsiFolder (");
		g_print (folder_path);
		g_print (")...\n");
		struct _ns1__IsiFolder ns1__IsiFolder;
		struct _ns1__IsiFolderResponse ns1__IsiFolderResponse;
		ns1__IsiFolder.LocalPath = folder_path;
		init_gsoap (&soap);
		soap_call___ns1__IsiFolder (&soap, 
									soapURL, 
									NULL, 
									&ns1__IsiFolder, 
									&ns1__IsiFolderResponse);		
		if (soap.error) {
			g_print ("****error calling IsiFolder***\n");
			soap_print_fault (&soap, stderr);
		} else {
			g_print ("***calling IsiFolder succeeded***\n");
			if (ns1__IsiFolderResponse.IsiFolderResult)
				b_is_ifolder = TRUE;
		}

		cleanup_gsoap (&soap);
		g_free (folder_path);
	}

	return b_is_ifolder;
}

gboolean
can_be_ifolder (NautilusFileInfo *file)
{
	struct soap soap;
	gchar *folder_path;
	gboolean b_can_be_ifolder = TRUE;
	
	if (!nautilus_file_info_is_directory (file))
		return FALSE;
		
	folder_path = get_file_path (file);
	if (folder_path != NULL) {
		g_print ("****About to call CanBeiFolder (");
		g_print (folder_path);
		g_print (")...\n");
		struct _ns1__CanBeiFolder ns1__CanBeiFolder;
		struct _ns1__CanBeiFolderResponse ns1__CanBeiFolderResponse;
		ns1__CanBeiFolder.LocalPath = folder_path;
		init_gsoap (&soap);
		soap_call___ns1__CanBeiFolder (&soap,
									   soapURL, 
									   NULL, 
									   &ns1__CanBeiFolder, 
									   &ns1__CanBeiFolderResponse);
		if (soap.error) {
			g_print ("****error calling CanBeiFolder***\n");
			soap_print_fault (&soap, stderr);
		} else {
			g_print ("***calling CanBeiFolder succeeded***\n");
			if (!ns1__CanBeiFolderResponse.CanBeiFolderResult)
				b_can_be_ifolder = FALSE;
		}

		cleanup_gsoap (&soap);
		g_free (folder_path);
	}
	
	return b_can_be_ifolder;
}

gint
create_local_ifolder (NautilusFileInfo *file)
{
	struct soap soap;
	gchar *folder_path;
	
	folder_path = get_file_path (file);
	if (folder_path != NULL) {
		g_print ("****About to call CreateLocaliFolder (");
		g_print (folder_path);
		g_print (")...\n");
		struct _ns1__CreateLocaliFolder ns1__CreateLocaliFolder;
		struct _ns1__CreateLocaliFolderResponse ns1__CreateLocaliFolderResponse;
		ns1__CreateLocaliFolder.Path = folder_path;
		init_gsoap (&soap);
		soap_call___ns1__CreateLocaliFolder (&soap, 
											 soapURL, 
											 NULL, 
											 &ns1__CreateLocaliFolder, 
											 &ns1__CreateLocaliFolderResponse);
		g_free (folder_path);
		if (soap.error) {
			g_print ("****error calling CreateLocaliFolder***\n");
			soap_print_fault (&soap, stderr);
			cleanup_gsoap (&soap);
			return -1;
		} else {
			g_print ("***calling CreateLocaliFolder succeeded***\n");
			struct ns1__iFolderWeb *ifolder = 
				ns1__CreateLocaliFolderResponse.CreateLocaliFolderResult;
			if (ifolder == NULL) {
				g_print ("***The created iFolder is NULL\n");
				cleanup_gsoap (&soap);
				return -1;
			} else {
				g_print ("***The created iFolder's ID is: ");
				g_print (ifolder->ID);
				g_print ("\n");
			}
		}

		cleanup_gsoap (&soap);
	} else {
		/* Error getting the folder path */
		return -1;
	}
	
	return 0;
}

gchar *
get_ifolder_id_by_local_path (gchar *path)
{
	struct soap soap;
	gchar *ifolder_id;
	
	ifolder_id = NULL;

	if (path != NULL) {
		g_print ("****About to call GetiFolderByLocalPath (");
		g_print (path);
		g_print (")...\n");
		struct _ns1__GetiFolderByLocalPath ns1__GetiFolderByLocalPath;
		struct _ns1__GetiFolderByLocalPathResponse ns1__GetiFolderByLocalPathResponse;
		ns1__GetiFolderByLocalPath.LocalPath = path;
		init_gsoap (&soap);
		soap_call___ns1__GetiFolderByLocalPath (&soap, 
										soapURL, 
										NULL, 
										&ns1__GetiFolderByLocalPath, 
										&ns1__GetiFolderByLocalPathResponse);
		if (soap.error) {
			g_print ("****error calling GetiFolderByLocalPath***\n");
			soap_print_fault (&soap, stderr);
			cleanup_gsoap (&soap);
			return NULL;
		} else {
			g_print ("***calling GetiFolderByLocalPath succeeded***\n");
			struct ns1__iFolderWeb *ifolder = 
				ns1__GetiFolderByLocalPathResponse.GetiFolderByLocalPathResult;
			if (ifolder == NULL) {
				g_print ("***GetiFolderByLocalPath returned NULL\n");
				cleanup_gsoap (&soap);
				return NULL;
			} else {
				g_print ("***The iFolder's ID is: ");
				g_print (ifolder->ID);
				g_print ("\n");
				ifolder_id = strdup (ifolder->ID);
			}
		}

		cleanup_gsoap (&soap);
	}

	return ifolder_id;
}

gint
revert_ifolder (NautilusFileInfo *file)
{
	struct soap soap;
	gchar *folder_path;
	gchar *ifolder_id;
	
	folder_path = get_file_path (file);
	if (folder_path != NULL) {
		ifolder_id = get_ifolder_id_by_local_path (folder_path);
		g_free (folder_path);
		if (ifolder_id != NULL) {
			g_print ("****About to call RevertiFolder ()\n");
			struct _ns1__RevertiFolder ns1__RevertiFolder;
			struct _ns1__RevertiFolderResponse ns1__RevertiFolderResponse;
			ns1__RevertiFolder.iFolderID = ifolder_id;
			init_gsoap (&soap);
			soap_call___ns1__RevertiFolder (&soap, 
												 soapURL, 
												 NULL, 
												 &ns1__RevertiFolder, 
												 &ns1__RevertiFolderResponse);
			g_free (ifolder_id);
			if (soap.error) {
				g_print ("****error calling RevertiFolder***\n");
				soap_print_fault (&soap, stderr);
				cleanup_gsoap (&soap);
				return -1;
			} else {
				g_print ("***calling RevertiFolder succeeded***\n");
				struct ns1__iFolderWeb *ifolder = 
					ns1__RevertiFolderResponse.RevertiFolderResult;
				if (ifolder == NULL) {
					g_print ("***The reverted iFolder is NULL\n");
					return -1;
				} else {
					g_print ("***The reverted iFolder's ID was: ");
					g_print (ifolder->ID);
					g_print ("\n");
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

/**
 * Nautilus Info Provider Implementation
 */
static NautilusOperationResult
ifolder_nautilus_update_file_info (NautilusInfoProvider 	*provider,
								   NautilusFileInfo			*file,
								   GClosure					*update_complete,
								   NautilusOperationHandle	**handle)
{
	g_print ("--> ifolder_nautilus_update_file_info called\n");
	
	gchar *uri;
	
	/* Don't do anything if the specified file is not a directory. */
	if (!nautilus_file_info_is_directory (file))
		return NAUTILUS_OPERATION_COMPLETE;
	
	if (is_ifolder_running ()) {
		if (is_ifolder (file))
		{
			nautilus_file_info_add_emblem (file, "ifolder");
		}
	} else {
		g_print ("*** iFolder is NOT running\n");
	}

	/* FIXME: Temporary fix to keep track of NautilusFileInfo * */
	uri = nautilus_file_info_get_uri (file);
	g_object_weak_ref (G_OBJECT (file),
					   nautilus_file_info_weak_ref_cb,
					   uri);
	g_hash_table_insert (seen_nautilus_file_infos,
					 	 uri,	/* key */
					 	 file);	/* value */

	return NAUTILUS_OPERATION_COMPLETE;
}

static void
ifolder_nautilus_info_provider_iface_init (NautilusInfoProviderIface *iface)
{
	iface->update_file_info	= ifolder_nautilus_update_file_info;
}

/**
 * Nautilus Menu Provider Implementation
 */

gboolean
show_ifolder_error_message (void *user_data)
{
	g_print ("*** show_ifolder_error_message () called\n");
	iFolderErrorMessage *errMsg = (iFolderErrorMessage *)user_data;
	GtkDialog *message_dialog;

	message_dialog = eel_show_error_dialog (
						errMsg->message,
						errMsg->detail,
						errMsg->title,
						GTK_WINDOW (errMsg->window));
	gtk_dialog_run (message_dialog);
	gtk_object_destroy (GTK_OBJECT (message_dialog));
	
	free (errMsg);
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
		g_print ("Error calling: ");
		g_print (args);
		g_print ("\n");
		free (args);
		iFolderErrorMessage *errMsg = malloc (sizeof (iFolderErrorMessage));
		errMsg->window = g_object_get_data (G_OBJECT (item), "parent_window");
		errMsg->title	= _("iFolder Error");
		errMsg->message	= _("Error opening dialog.");
		errMsg->detail	= _("Sorry, unable to open the window to perform the specified action.");
		g_idle_add (show_ifolder_error_message, errMsg);
		return;
	}
	
	if (fgets (readBuffer, 1024, output) != NULL) {
		return_str = strdup (readBuffer);
		g_print ("*** 1st line of STDOUT from popen: ");
		g_print (return_str);
		g_print ("\n");
	}

	free (args);
	pclose (output);
	
	return (void *)return_str;
}

static void *
create_ifolder_thread (gpointer user_data)
{
	NautilusMenuItem *item;
	GList *files;
	NautilusFileInfo *file;
	gint error;
	
	item = (NautilusMenuItem *)user_data;
	files = g_object_get_data (G_OBJECT (item), "files");
	file = NAUTILUS_FILE_INFO (files->data);

	error = create_local_ifolder (file);
	if (error) {
		g_print ("An error occurred creating an iFolder\n");
		iFolderErrorMessage *errMsg = malloc (sizeof (iFolderErrorMessage));
		errMsg->window = g_object_get_data (G_OBJECT (item), "parent_window");
		errMsg->title	= _("iFolder Error");
		errMsg->message	= _("The folder could not be converted.");
		errMsg->detail	= _("Sorry, unable to convert the specified folder into an iFolder.");
		g_idle_add (show_ifolder_error_message, errMsg);
	} else {
		nautilus_file_info_invalidate_extension_info (file);
	}
}

static void
create_ifolder_callback (NautilusMenuItem *item, gpointer user_data)
{
	g_print ("Convert to iFolder selected\n");

	pthread_t thread;

	pthread_create (&thread, 
					NULL, 
					create_ifolder_thread,
					item);
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
		g_print ("An error occurred reverting an iFolder\n");
		iFolderErrorMessage *errMsg = malloc (sizeof (iFolderErrorMessage));
		errMsg->window = g_object_get_data (G_OBJECT (item), "parent_window");
		errMsg->title	= _("iFolder Error");
		errMsg->message	= _("The iFolder could not be reverted.");
		errMsg->detail	= _("Sorry, unable to revert the specified iFolder to a normal folder.");
		g_idle_add (show_ifolder_error_message, errMsg);
	} else {
		nautilus_file_info_invalidate_extension_info (file);
	}
}

static void
revert_ifolder_callback (NautilusMenuItem *item, gpointer user_data)
{
	g_print ("Revert to a Normal Folder selected\n");
	GtkDialog *message_dialog;
	GtkWidget *window;
	int response;
	pthread_t thread;

	window = g_object_get_data (G_OBJECT (item), "parent_window");

	message_dialog = eel_show_yes_no_dialog (
						_("Revert this iFolder?"), 
	                    _("This will revert this iFolder back to a normal folder and leave the files intact.  The iFolder will then be available from the server and will need to be setup in a different location in order to sync."),
						_("iFolder Confirmation"), 
						GTK_STOCK_YES,
						GTK_STOCK_NO,
						GTK_WINDOW (window));
	/* FIXME: Figure out why the next call doesn't set the default button to "NO" */
	gtk_dialog_set_default_response (message_dialog, GTK_RESPONSE_CANCEL);
	response = gtk_dialog_run (message_dialog);
	gtk_object_destroy (GTK_OBJECT (message_dialog));
	
	if (response == GTK_RESPONSE_YES) {
		pthread_create (&thread, 
						NULL, 
						revert_ifolder_thread,
						item);
	}
}

static void
share_ifolder_callback (NautilusMenuItem *item, gpointer user_data)
{
	g_print ("Share with... selected\n");
	gchar *ifolder_path;
	gchar *ifolder_id;
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
		ifolder_id = get_ifolder_id_by_local_path (ifolder_path);
		if (ifolder_id != NULL) {
			sprintf (args, "%s share %s", NAUTILUS_IFOLDER_SH_PATH, ifolder_id);
			g_print ("args: ");
			g_print (args);
			g_print ("\n");
			
			g_free (ifolder_id);
		}
		
		g_free (ifolder_path);
	}
	
	if (strlen (args) <= 0)
		return;
		
	g_object_set_data (G_OBJECT (item),
				"ifolder_args",
				strdup(args));
		
	pthread_create (&thread, 
					NULL, 
					ifolder_dialog_thread,
					item);
}

static void
ifolder_properties_callback (NautilusMenuItem *item, gpointer user_data)
{
	g_print ("Properties selected\n");	
	gchar *ifolder_path;
	gchar *ifolder_id;
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
		ifolder_id = get_ifolder_id_by_local_path (ifolder_path);
		if (ifolder_id != NULL) {
			sprintf (args, "%s properties %s", 
					 NAUTILUS_IFOLDER_SH_PATH, ifolder_id);
			g_print ("args: ");
			g_print (args);
			g_print ("\n");
			
			g_free (ifolder_id);
		}
		
		g_free (ifolder_path);
	}
	
	if (strlen (args) <= 0)
		return;
		
	g_object_set_data (G_OBJECT (item),
				"ifolder_args",
				strdup(args));
		
	pthread_create (&thread, 
					NULL, 
					ifolder_dialog_thread,
					item);
}

static void
ifolder_help_callback (NautilusMenuItem *item, gpointer user_data)
{
	g_print ("Help... selected\n");	
	pthread_t thread;
	char args [1024];
	memset (args, '\0', sizeof (args));
	
	sprintf (args, "%s help", NAUTILUS_IFOLDER_SH_PATH);
	g_print ("args: ");
	g_print (args);
	g_print ("\n");
	
	if (strlen (args) <= 0)
		return;
		
	g_object_set_data (G_OBJECT (item),
				"ifolder_args",
				strdup(args));
		
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
	g_print ("--> ifolder_nautilus_get_file_items called\n");
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
					_("Convert to an iFolder"),
					_("Convert the selected folder to an iFolder"),
					"ifolder-folder");
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

static gchar *
getLocalServiceUrl ()
{
	char readBuffer [1024];
	char tmpUrl [1024];
	gchar *localServiceUrl = NULL;
	char args [1024];
	memset (args, '\0', sizeof (args));
	
	memset (readBuffer, '\0', sizeof (readBuffer));
	memset (tmpUrl, '\0', sizeof (tmpUrl));

	FILE *output;
	
	sprintf (args, "%s WebServiceURL", NAUTILUS_IFOLDER_SH_PATH);
	
	output = popen (args, "r");
	if (output == NULL) {
		/* error calling mono nautilus-ifolder.exe */
		g_print ("Error calling 'mono nautilus-ifolder.exe WebServiceURL");
		return NULL;
	}
	
	if (fgets (readBuffer, 1024, output) != NULL) {
		strcpy (tmpUrl, readBuffer);
		strcat (tmpUrl, "/iFolder.asmx");
		localServiceUrl = strdup (tmpUrl);
		g_print ("*** Web Service URL: ");
		g_print (localServiceUrl);
		g_print ("\n");
	}

	pclose (output);	
	
	return localServiceUrl;
}

void
nautilus_module_initialize (GTypeModule *module)
{
	g_print ("Initializing nautilus-ifolder extension\n");
	ifolder_extension_register_type (module);
	provider_types[0] = ifolder_nautilus_get_type ();
	
	soapURL = getLocalServiceUrl ();
	
	/* Initialize the GHashTable */
	seen_nautilus_file_infos = g_hash_table_new (g_str_hash, g_str_equal);
	
	/* Start the named pipe listener thread */
	pthread_create (&named_pipe_thread, 
					NULL, 
					named_pipe_listener_thread,
					NULL);
}

void
nautilus_module_shutdown (void)
{
	g_print ("Shutting down nautilus-ifolder extension\n");

	/* Cleanup soapURL */	
	if (soapURL) {
		free (soapURL);
	}
	
	/* Cleanup the GHashTable */
	g_hash_table_foreach_remove (seen_nautilus_file_infos,
								 nautilus_file_info_foreach_remove,
								 NULL);
	g_hash_table_destroy (seen_nautilus_file_infos);
}

void
nautilus_module_list_types (const GType **types, int *num_types)
{
	*types = provider_types;
	*num_types = G_N_ELEMENTS (provider_types);
}
