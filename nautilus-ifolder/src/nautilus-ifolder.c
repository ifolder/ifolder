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
#include <libnautilus-extension/nautilus-column-provider.h>
#include <libnautilus-extension/nautilus-file-info.h>
#include <libnautilus-extension/nautilus-info-provider.h>
#include <libnautilus-extension/nautilus-menu-provider.h>
#include <libnautilus-extension/nautilus-property-page-provider.h>

#include <gtk/gtk.h>
#include <glib/gi18n-lib.h>
#include <string.h>
#include <stdio.h>

#include "iFolderClientStub.h"
#include "iFolderClient.nsmap"

typedef struct {
	GObject parent_slot;
} iFolderNautilus;

typedef struct {
	GObjectClass parent_slot;
} iFolderNautilusClass;

static GType provider_types[1];

static void ifolder_extension_register_type (GTypeModule *module);
static void ifolder_nautilus_instance_init (iFolderNautilus *ifn);

static GObjectClass * parent_class = NULL;
static GType ifolder_nautilus_type;

/**
 * gSOAP variables
 */
struct soap soap;

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
static gboolean
is_ifolder (NautilusFileInfo *file)
{
	gboolean b_is_ifolder = FALSE;
	gchar *folder_path;
	
	if (!nautilus_file_info_is_directory (file))
		return FALSE;

	folder_path = get_file_path (file);
	if (folder_path != NULL) {
		g_print ("****About to call IsiFolder (");
		g_print (folder_path);
		g_print (")...\n");
		struct _ns1__IsiFolder ns1__IsiFolder;
		struct _ns1__IsiFolderResponse ns1__IsiFolderResponse;
		ns1__IsiFolder.LocalPath = folder_path;
		soap_call___ns1__IsiFolder (&soap, 
									NULL, 
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

		g_free (folder_path);
	}

	return b_is_ifolder;
}

static gboolean
can_be_ifolder (NautilusFileInfo *file)
{
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
		soap_call___ns1__CanBeiFolder (&soap,
									   NULL, 
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
		
		g_free (folder_path);
	}
	
	return b_can_be_ifolder;
}

static gint
create_local_ifolder (NautilusFileInfo *file)
{
	gchar *folder_path;
	
	folder_path = get_file_path (file);
	if (folder_path != NULL) {
		g_print ("****About to call CreateLocaliFolder (");
		g_print (folder_path);
		g_print (")...\n");
		struct _ns1__CreateLocaliFolder ns1__CreateLocaliFolder;
		struct _ns1__CreateLocaliFolderResponse ns1__CreateLocaliFolderResponse;
		ns1__CreateLocaliFolder.Path = folder_path;
		soap_call___ns1__CreateLocaliFolder (&soap, 
											 NULL, 
											 NULL, 
											 &ns1__CreateLocaliFolder, 
											 &ns1__CreateLocaliFolderResponse);
		g_free (folder_path);
		if (soap.error) {
			g_print ("****error calling CreateLocaliFolder***\n");
			soap_print_fault (&soap, stderr);
			return -1;
		} else {
			g_print ("***calling CreateLocaliFolder succeeded***\n");
			struct ns1__iFolderWeb *ifolder = 
				ns1__CreateLocaliFolderResponse.CreateLocaliFolderResult;
			if (ifolder == NULL) {
				g_print ("***The created iFolder is NULL\n");
				return -1;
			} else {
				g_print ("***The created iFolder's ID is: ");
				g_print (ifolder->ID);
				g_print ("\n");
			}
		}
	} else {
		/* Error getting the folder path */
		return -1;
	}
	
	return 0;
}

static gchar *
get_ifolder_id_by_local_path (gchar *path)
{
	gchar *ifolder_id;
	
	ifolder_id = NULL;

	if (path != NULL) {
		g_print ("****About to call GetiFolderByLocalPath (");
		g_print (path);
		g_print (")...\n");
		struct _ns1__GetiFolderByLocalPath ns1__GetiFolderByLocalPath;
		struct _ns1__GetiFolderByLocalPathResponse ns1__GetiFolderByLocalPathResponse;
		ns1__GetiFolderByLocalPath.LocalPath = path;
		soap_call___ns1__GetiFolderByLocalPath (&soap, 
										NULL, 
										NULL, 
										&ns1__GetiFolderByLocalPath, 
										&ns1__GetiFolderByLocalPathResponse);
		if (soap.error) {
			g_print ("****error calling GetiFolderByLocalPath***\n");
			soap_print_fault (&soap, stderr);
			return -1;
		} else {
			g_print ("***calling GetiFolderByLocalPath succeeded***\n");
			struct ns1__iFolderWeb *ifolder = 
				ns1__GetiFolderByLocalPathResponse.GetiFolderByLocalPathResult;
			if (ifolder == NULL) {
				g_print ("***GetiFolderByLocalPath returned NULL\n");
				return -1;
			} else {
				g_print ("***The iFolder's ID is: ");
				g_print (ifolder->ID);
				g_print ("\n");
				ifolder_id = strdup (ifolder->ID);
			}
		}
	}

	return ifolder_id;
}

static gint
revert_ifolder (NautilusFileInfo *file)
{
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
			soap_call___ns1__RevertiFolder (&soap, 
												 NULL, 
												 NULL, 
												 &ns1__RevertiFolder, 
												 &ns1__RevertiFolderResponse);
			g_free (ifolder_id);
			if (soap.error) {
				g_print ("****error calling RevertiFolder***\n");
				soap_print_fault (&soap, stderr);
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
	gchar *ifolder_type = NULL;
	
	if (is_ifolder (file))
	{
		nautilus_file_info_add_emblem (file, "ifolder");
		ifolder_type = _("iFolder");
	}
	else
	{
		ifolder_type = _("not an iFolder");
	}
	
	nautilus_file_info_add_string_attribute (file,
											 "NautilusiFolder::ifolder_type",
											 ifolder_type);
											 
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
static void
create_ifolder_callback (NautilusMenuItem *item, gpointer user_data)
{
	g_print ("Convert to iFolder selected\n");

	GList *files;
	NautilusFileInfo *file;
	gint error;

	files = g_object_get_data (G_OBJECT (item), "files");
	file = NAUTILUS_FILE_INFO (files->data);
//	g_object_unref (G_OBJECT (files->data));
	if (file == NULL)
		return;

	error = create_local_ifolder (file);
	if (error) {
		/* FIXME: Figure out how to let the user know an error happened */
		g_print ("An error occurred creating an iFolder\n");
	} else {
		nautilus_file_info_invalidate_extension_info (file);
	}
}

static void
revert_ifolder_callback (NautilusMenuItem *item, gpointer user_data)
{
	g_print ("Revert to a Normal Folder selected\n");

	GList *files;
	NautilusFileInfo *file;
	gint error;

	files = g_object_get_data (G_OBJECT (item), "files");
	file = NAUTILUS_FILE_INFO (files->data);
//	g_object_unref (G_OBJECT (files->data));
	if (file == NULL)
		return;

	error = revert_ifolder (file);
	if (error) {
		/* FIXME: Figure out how to let the user know an error happened */
		g_print ("An error occurred reverting an iFolder\n");
	} else {
		nautilus_file_info_invalidate_extension_info (file);
	}
}

static void
share_ifolder_callback (NautilusMenuItem *item, gpointer user_data)
{
	g_print ("Share with... selected\n");	
}

static void
ifolder_properties_callback (NautilusMenuItem *item, gpointer user_data)
{
	g_print ("Properties selected\n");	
}

static void
ifolder_help_callback (NautilusMenuItem *item, gpointer user_data)
{
	g_print ("Help... selected\n");	
}
 
static GList *
ifolder_nautilus_get_file_items (NautilusMenuProvider *provider,
								 GtkWidget *window,
								 GList *files)
{
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
		
	items = NULL;

	if (is_ifolder (file)) {
		/* Menu item: Revert to a Normal Folder */
		item = nautilus_menu_item_new ("NautilusiFolder::revert_ifolder",
					_("Revert to a Normal Folder"),
					_("Revert the selected iFolder back to normal folder"),
					NULL);
		g_signal_connect (item, "activate",
					G_CALLBACK (revert_ifolder_callback),
					provider);
		g_object_set_data (G_OBJECT (item),
					"files",
					nautilus_file_info_list_copy (files));
		items = g_list_append (items, item);
		
		/* Menu item: Share with... */
		item = nautilus_menu_item_new ("NautilusiFolder::share_ifolder",
					_("Share with..."),
					_("Share the selected iFolder with another user"),
					NULL);
		g_signal_connect (item, "activate",
					G_CALLBACK (share_ifolder_callback),
					provider);
		g_object_set_data (G_OBJECT (item),
					"files",
					nautilus_file_info_list_copy (files));
		items = g_list_append (items, item);
		
		/* Menu item: Properties */
		item = nautilus_menu_item_new ("NautilusiFolder::ifolder_properties",
					_("Properties"),
					_("View the properties of the selected iFolder"),
					NULL);
		g_signal_connect (item, "activate",
					G_CALLBACK (ifolder_properties_callback),
					provider);
		g_object_set_data (G_OBJECT (item),
					"files",
					nautilus_file_info_list_copy (files));
		items = g_list_append (items, item);
		
		/* Menu item: Help */
		item = nautilus_menu_item_new ("NautilusiFolder::ifolder_help",
					_("Help..."),
					_("View the iFolder help"),
					NULL);
		g_signal_connect (item, "activate",
					G_CALLBACK (ifolder_help_callback),
					provider);
		g_object_set_data (G_OBJECT (item),
					"files",
					nautilus_file_info_list_copy (files));
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
					NULL);
		g_signal_connect (item, "activate",
					G_CALLBACK (create_ifolder_callback),
					provider);
		g_object_set_data (G_OBJECT (item),
					"files",
					nautilus_file_info_list_copy (files));
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

void
nautilus_module_initialize (GTypeModule *module)
{
	g_print ("Initializing nautilus-ifolder extension\n");
	ifolder_extension_register_type (module);
	provider_types[0] = ifolder_nautilus_get_type ();
	
	/* Initialize gSOAP */
	soap_init (&soap);
	soap_set_namespaces (&soap, iFolderClient_namespaces);
}

void
nautilus_module_shutdown (void)
{
	g_print ("Shutting down nautilus-ifolder extension\n");
	
	/* Cleanup gSOAP */
	soap_end (&soap);
}

void
nautilus_module_list_types (const GType **types, int *num_types)
{
	*types = provider_types;
	*num_types = G_N_ELEMENTS (provider_types);
}
