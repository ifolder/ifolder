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
		/* FIXME: Call the IsiFolder(folder_path) in iFolder.asmx */
		if (
				(!strcmp (folder_path, "/home/boyd"))
				|| (!strcmp (folder_path, "/home/boyd/ifolder"))
				|| (!strcmp (folder_path, "/tmp"))
				|| (!strcmp (folder_path, "/home/boyd/download"))
			)
		{
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
		/* FIXME: Call CanBeiFolder in iFolder.asmx instead of this hard-coding */
		if ((!strcmp (folder_path, "/"))
			|| (!strcmp (folder_path, "/usr"))
			|| (!strcmp (folder_path, "/bin"))
			|| (!strcmp (folder_path, "/opt"))) {
			b_can_be_ifolder = FALSE;
		}
	}
	
	return b_can_be_ifolder;
}

static gint
create_local_ifolder (NautilusFileInfo *file)
{
	gchar *folder_path;
	
	folder_path = get_file_path (file);
	if (folder_path != NULL) {
		/* FIXME: Call CreateLocaliFolder (folder_path) in iFolder.asmx */
		
		g_free (folder_path);
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
		/* FIXME: Call GetiFolderByLocalPath and do a strdup () on the ID */
		ifolder_id = strdup ("FIXME: iFolder ID goes here");
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
		if (ifolder_id != NULL) {
			/* FIXME: Call RevertiFolder (ifolder_id) in iFolder.asmx */
			
			g_free (ifolder_id);
		}
		
		g_free (folder_path);
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
	g_object_unref (G_OBJECT (files->data));
	if (file == NULL)
		return;

	error = create_local_ifolder (file);
	if (error) {
		/* FIXME: Figure out how to let the user know an error happened */
		g_print ("An error occurred creating an iFolder\n");
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
	g_object_unref (G_OBJECT (files->data));
	if (file == NULL)
		return;

	error = revert_ifolder (file);
	if (error) {
		/* FIXME: Figure out how to let the user know an error happened */
		g_print ("An error occurred reverting an iFolder\n");
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
		
	/**
	 * If the iFolder API says that the current file cannot be an iFolder, don't
	 * add any iFolder context menus.
	 */
	if (!can_be_ifolder (file))
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
}

void
nautilus_module_shutdown (void)
{
	g_print ("Shutting down nautilus-ifolder extension\n");
}

void
nautilus_module_list_types (const GType **types, int *num_types)
{
	*types = provider_types;
	*num_types = G_N_ELEMENTS (provider_types);
}
