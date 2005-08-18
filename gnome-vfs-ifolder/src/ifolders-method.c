/****************************************************************************
 *  $RCSfile$
 * 
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
 ****************************************************************************/

#include <string.h>

#include <libgnomevfs/gnome-vfs.h>
#include <libgnomevfs/gnome-vfs-module.h>

#include <simiasweb.h>
//#include <simias-event-client.h>

#include <iFolderStub.h>
#include <iFolder.nsmap>
//#include <ifolder.h>

#define IFOLDERS_URI "ifolders:"

typedef struct
{
	GNode *gnode;
	gchar *name;
	
} fs_node;

typedef struct
{
	GNode *gnode;
	GNode *current;
} directory_handle;

static char *
get_local_ifolder_url (void)
{
	char *url;
	char result [PATH_MAX];

	if (simias_get_local_service_url(&url) == SIMIAS_SUCCESS)
	{
		g_sprintf(result, "%s/iFolder.asmx", url);
		g_free(url);
	}
	
	return g_strdup(result);
}

static fs_node *
new_root_node (void)
{
	fs_node *root;

	root = g_new0 (fs_node, 1);
	root->name = g_strdup ("iFolders");

	return root;
}

static gboolean
free_root_node (GNode *node)
{
	fs_node *root = node->data;
	
	g_free (root->name);
	g_free (root);
}

static fs_node *
new_ifolder_node (struct ns1__iFolderWeb *ifolder)
{
	fs_node *fsnode;

	fsnode = g_new0 (fs_node, 1);
	fsnode->name = g_strdup (ifolder->Name);

	return fsnode;
}

static gboolean
free_fs_node (GNode *node, gpointer data)
{
	fs_node *fsnode = node->data;
	
	g_free (fsnode->name);
	g_free (fsnode);
}

GNode *
get_all_ifolders (void)
{
	struct soap soap;
	struct _ns1__GetAlliFolders input;
	struct _ns1__GetAlliFoldersResponse output;
	char username[PATH_MAX];
	char password[PATH_MAX];
	char *url;
	int i;
	fs_node *root;

	root = new_root_node ();
	root->gnode = g_node_new (root);
	
	soap_init2 (&soap, SOAP_C_UTFSTRING, SOAP_C_UTFSTRING);
	soap_set_namespaces (&soap, iFolder_namespaces);

	if (simias_get_web_service_credential (username, password) == SIMIAS_SUCCESS)
	{
		soap.userid = username;
		soap.passwd = password;
	}
	
	url = get_local_ifolder_url ();
	
	soap_call___ns1__GetAlliFolders (
		&soap,
		url,
		NULL,
		&input,
		&output);

	if (!soap.error)
	{
		struct ns1__ArrayOfIFolderWeb *ifolders = output.GetAlliFoldersResult;

		if (ifolders != NULL)
		{
			for (i = 0; i < ifolders->__sizeiFolderWeb; i++) 
			{
				struct ns1__iFolderWeb *ifolder = ifolders->iFolderWeb[i];
				
				fs_node *fsnode = new_ifolder_node (ifolder);
				fsnode->gnode = g_node_append_data (root->gnode, fsnode);
			}
		}
	}
	
	soap_end(&soap);

	g_free(url);

	return root->gnode;
}

void
free_ifolders (GNode *root)
{
	g_node_traverse (root,
		G_PRE_ORDER,
		G_TRAVERSE_ALL,
		-1,
		free_fs_node,
		NULL);
	
	g_node_destroy (root);
}

gboolean
create_volume (void)
{
	GnomeVFSVolumeMonitor *monitor;
	GnomeVFSVolume *volume;
	GList *volumes, *l;
	gboolean found;
	char *uri;

	found = FALSE;

	monitor = gnome_vfs_get_volume_monitor ();

	volumes = gnome_vfs_volume_monitor_get_mounted_volumes(monitor);

	for (l = volumes; l != NULL; l = l->next)
	{
		volume = l->data;
		if (gnome_vfs_volume_is_user_visible (volume))
		{
			uri = gnome_vfs_volume_get_activation_uri(volume);

			if (strcmp(uri, IFOLDERS_URI) == 0)
			{
				found = TRUE;
			}
		}
	}

	if (found == FALSE)
	{
		gnome_vfs_connect_to_server(IFOLDERS_URI, "iFolders", "gnome-fs-ifolders");
	}

	g_list_foreach (volumes, (GFunc) gnome_vfs_volume_unref, NULL);
	g_list_free (volumes);

	return found;
}

static GnomeVFSResult
do_open (GnomeVFSMethod			*method,
		 GnomeVFSMethodHandle	**method_handle,
		 GnomeVFSURI			*uri,
		 GnomeVFSOpenMode		mode,
		 GnomeVFSContext		*context)
{
	return GNOME_VFS_OK;
}

static GnomeVFSResult
do_read (GnomeVFSMethod			*method,
		 GnomeVFSMethodHandle	*method_handle,
		 gpointer				buffer,
		 GnomeVFSFileSize		bytes,
		 GnomeVFSFileSize		*bytes_read,
		 GnomeVFSContext		*context)
{
	return GNOME_VFS_OK;
}

static GnomeVFSResult
do_close (GnomeVFSMethod		*method,
		  GnomeVFSMethodHandle	*method_handle,
		  GnomeVFSContext		*context)
{
	return GNOME_VFS_OK;
}

static GnomeVFSResult
do_open_directory (GnomeVFSMethod			*method,
				   GnomeVFSMethodHandle		**method_handle,
				   GnomeVFSURI				*uri,
				   GnomeVFSFileInfoOptions	options,
				   GnomeVFSContext			*context)
{
	directory_handle *handle;

	handle = g_new0 (directory_handle, 1);

	handle->gnode = get_all_ifolders();
	handle->current = handle->gnode->children;

	*method_handle = (GnomeVFSMethodHandle *) handle;
	
	return GNOME_VFS_OK;
}

static GnomeVFSResult
do_read_directory (GnomeVFSMethod			*method,
				   GnomeVFSMethodHandle		*method_handle,
				   GnomeVFSFileInfo			*file_info,
				   GnomeVFSContext			*context)
{
	directory_handle *handle = (directory_handle *) method_handle;
	fs_node *node;
	
	if (!handle->current)
	{
		return GNOME_VFS_ERROR_EOF;
	}

	node = handle->current->data;
	
	file_info->name = g_strdup (node->name);
	file_info->type = GNOME_VFS_FILE_TYPE_DIRECTORY;
	file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_TYPE;
	file_info->mime_type = g_strdup ("x-directory/normal");
	file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_MIME_TYPE;

	handle->current = handle->current->next;
	
	return GNOME_VFS_OK;
}

static GnomeVFSResult
do_close_directory (GnomeVFSMethod			*method,
					GnomeVFSMethodHandle	*method_handle,
					GnomeVFSContext			*context)
{
	directory_handle *handle = (directory_handle *) method_handle;

	free_ifolders (handle->gnode);
	
	g_free (handle);
	
	return GNOME_VFS_OK;
}

static GnomeVFSResult
do_get_file_info (GnomeVFSMethod			*method,
				  GnomeVFSURI				*uri,
				  GnomeVFSFileInfo			*file_info,
				  GnomeVFSFileInfoOptions	options,
				  GnomeVFSContext			*context)
{
	file_info->name = g_strdup ("iFolders");
	file_info->type = GNOME_VFS_FILE_TYPE_DIRECTORY;
	file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_TYPE;
	file_info->mime_type = g_strdup ("x-directory/normal");
	file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_MIME_TYPE;
	
	return GNOME_VFS_OK;
}

static gboolean
do_is_local (GnomeVFSMethod		*method,
			 const GnomeVFSURI	*uri)
{
	return TRUE;
}

static GnomeVFSMethod method =
{
	sizeof (GnomeVFSMethod),
	do_open,					/* open */
	NULL,						/* create */
	do_close,					/* close */
	do_read,					/* read */
	NULL,						/* write */
	NULL,						/* seek */
	NULL,						/* tell */
	NULL,						/* truncate_handle */ 
	do_open_directory,			/* open_directory */
	do_close_directory,			/* close_directory */
	do_read_directory,			/* read_directory */     
	do_get_file_info,			/* get_file_info */
	NULL,						/* get_file_info_from_handle */
	do_is_local,				/* is_local */
	NULL,						/* make_directory */
	NULL,						/* remove_directory */
	NULL,						/* move */
	NULL,						/* unlink */
	NULL,						/* check_same_fs */
	NULL,						/* set_file_info */
	NULL,						/* truncate */
	NULL,						/* find_directory */
	NULL,						/* create_symbolic_link */
	NULL,						/* monitor_add */
	NULL,						/* monitor_cancel */
	NULL,						/* file_control */
	NULL						/* forget_cache */
};

GnomeVFSMethod *vfs_module_init(const char *method_name, const char *args)
{
	create_volume();

	return &method;
}

void vfs_module_shutdown (GnomeVFSMethod* method)
{
}

