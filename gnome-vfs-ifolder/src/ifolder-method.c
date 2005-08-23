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

#define IFOLDER_METHOD "ifolder:"

static GnomeVFSMethod *file_method = NULL;

typedef struct
{
	GNode *gnode;
	gchar *name;
	char *path;

} fs_node;

typedef struct
{
	GNode *gnode;
	GNode *children;
	GnomeVFSMethodHandle *method_handle;
} dir_handle;

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
	fsnode->path = g_strdup (ifolder->UnManagedPath);

	return fsnode;
}

static gboolean
free_fs_node (GNode *node, gpointer data)
{
	fs_node *fsnode = node->data;

	g_free (fsnode->name);
	g_free (fsnode->path);
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

				if (ifolder->UnManagedPath)
				{
					fs_node *fsnode = new_ifolder_node (ifolder);
					fsnode->gnode = g_node_append_data (root->gnode, fsnode);
				}
			}
		}
	}

	soap_end(&soap);

	g_free(url);

	return root->gnode;
}

static fs_node *
get_ifolder (const char *name)
{
	fs_node *result = NULL;
	GNode *root = get_all_ifolders ();
	GNode *current = root->children;

	while (current)
	{
		fs_node *tmp = (fs_node *) current->data;

		if (!strcmp(name, tmp->name))
		{
			result = tmp;
			break;
		}

		current = current->next;
	}

	return result;
}

static GnomeVFSURI *
get_file_uri (const GnomeVFSURI *ifolder_uri)
{
	GnomeVFSURI *file_uri = NULL;
	const gchar *path;

	path = gnome_vfs_uri_get_path (ifolder_uri);

	path = gnome_vfs_unescape_string (path, NULL);

	if (path && strcmp(path, "/"))
	{
		char *s;
		char *name;
		char *p;
		fs_node *ifolder;

		if (path[0] == '/')
		{
			++path;
		}

		name = g_strdup (path);

		if (name[strlen (name) - 1] == '/')
		{
			name[strlen (name) - 1] = '\0';
		}

		p = strchr(name, '/');

		if (p)
		{
			*p++ = '\0';
		}

		ifolder = get_ifolder (name);


		if (ifolder)
		{
			s = g_strconcat ("file://", ifolder->path, "/", p, NULL);

			if (s[strlen (s) - 1] == '/')
			{
				s[strlen (s) - 1] = '\0';
			}

			file_uri = gnome_vfs_uri_new (s);

			g_free (s);
		}

		g_free(name);
	}

	return file_uri;
}

void
free_dir (GNode *root)
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

			if (strcmp(uri, IFOLDER_METHOD) == 0)
			{
				found = TRUE;
			}
		}
	}

	if (found == FALSE)
	{
		gnome_vfs_connect_to_server(IFOLDER_METHOD, "iFolders", "gnome-fs-ifolders");
	}

	g_list_foreach (volumes, (GFunc) gnome_vfs_volume_unref, NULL);
	g_list_free (volumes);

	return found;
}

static GnomeVFSResult
do_open (GnomeVFSMethod         *method,
		 GnomeVFSMethodHandle   **method_handle,
		 GnomeVFSURI            *uri,
		 GnomeVFSOpenMode       mode,
		 GnomeVFSContext        *context)
{
	GnomeVFSResult result;
	GnomeVFSURI *file_uri;

	file_uri = get_file_uri (uri);

	result = (* file_method->open) (method, method_handle, file_uri, mode, context);

	gnome_vfs_uri_unref (file_uri);

	return result;
}

static GnomeVFSResult
do_create (GnomeVFSMethod *method,
		   GnomeVFSMethodHandle **method_handle,
		   GnomeVFSURI *uri,
		   GnomeVFSOpenMode mode,
		   gboolean exclusive,
		   guint perm,
		   GnomeVFSContext *context)
{
	GnomeVFSResult result;
	GnomeVFSURI *file_uri;

	file_uri = get_file_uri (uri);

	result = (* file_method->create) (method, method_handle, file_uri, mode, exclusive, perm, context);

	gnome_vfs_uri_unref (file_uri);

	return result;
}

static GnomeVFSResult
do_close (GnomeVFSMethod        *method,
		  GnomeVFSMethodHandle  *method_handle,
		  GnomeVFSContext       *context)
{
	GnomeVFSResult result;

	result = (* file_method->close) (method, method_handle, context);

	return result;
}

static GnomeVFSResult
do_read (GnomeVFSMethod         *method,
		 GnomeVFSMethodHandle   *method_handle,
		 gpointer               buffer,
		 GnomeVFSFileSize       bytes,
		 GnomeVFSFileSize       *bytes_read,
		 GnomeVFSContext        *context)
{
	GnomeVFSResult result;

	result = (* file_method->read) (method, method_handle, buffer, bytes, bytes_read, context);

	return result;
}

static GnomeVFSResult
do_write (GnomeVFSMethod *method,
		  GnomeVFSMethodHandle *method_handle,
		  gconstpointer buffer,
		  GnomeVFSFileSize bytes,
		  GnomeVFSFileSize *bytes_written,
		  GnomeVFSContext *context)
{
	GnomeVFSResult result;

	result = (* file_method->write) (method, method_handle, buffer, bytes, bytes_written, context);

	return result;
}

static GnomeVFSResult
do_seek (GnomeVFSMethod *method,
		 GnomeVFSMethodHandle *method_handle,
		 GnomeVFSSeekPosition whence,
		 GnomeVFSFileOffset offset,
		 GnomeVFSContext *context)
{
	GnomeVFSResult result;

	result = (* file_method->seek) (method, method_handle, whence, offset, context);

	return result;
}

static GnomeVFSResult
do_tell (GnomeVFSMethod *method,
		 GnomeVFSMethodHandle *method_handle,
		 GnomeVFSFileOffset *offset_return)
{
	GnomeVFSResult result;

	result = (* file_method->tell) (method, method_handle, offset_return);

	return result;
}


static GnomeVFSResult
do_truncate_handle (GnomeVFSMethod *method,
					GnomeVFSMethodHandle *method_handle,
					GnomeVFSFileSize where,
					GnomeVFSContext *context)
{
	GnomeVFSResult result;

	result = (* file_method->truncate_handle) (method, method_handle, where, context);

	return result;
}

static GnomeVFSResult
do_truncate (GnomeVFSMethod *method,
			 GnomeVFSURI *uri,
			 GnomeVFSFileSize where,
			 GnomeVFSContext *context)
{
	GnomeVFSResult result;
	GnomeVFSURI *file_uri;

	file_uri = get_file_uri (uri);

	result = (* file_method->truncate) (method, file_uri, where, context);

	gnome_vfs_uri_unref (file_uri);

	return result;
}

static GnomeVFSResult
do_open_directory (GnomeVFSMethod           *method,
				   GnomeVFSMethodHandle     **method_handle,
				   GnomeVFSURI              *uri,
				   GnomeVFSFileInfoOptions  options,
				   GnomeVFSContext          *context)
{
	GnomeVFSResult result;
	dir_handle *handle;
	GnomeVFSURI *file_uri;

	file_uri = get_file_uri (uri);

	handle = g_new0 (dir_handle, 1);

	if (!file_uri)
	{
		handle->gnode = get_all_ifolders();
		handle->children = handle->gnode->children;

		result = GNOME_VFS_OK;
	}
	else
	{
		handle->gnode = NULL;

		result = (* file_method->open_directory) (file_method, &(handle->method_handle), file_uri, options, context);

		gnome_vfs_uri_unref (file_uri);
	}

	*method_handle = (GnomeVFSMethodHandle *) handle;

	return result;
}

static GnomeVFSResult
do_read_directory (GnomeVFSMethod           *method,
				   GnomeVFSMethodHandle     *method_handle,
				   GnomeVFSFileInfo         *file_info,
				   GnomeVFSContext          *context)
{
	GnomeVFSResult result;
	dir_handle *handle = (dir_handle *) method_handle;
	fs_node *node;

	if (!handle->gnode)
	{
		result = (* file_method->read_directory) (file_method, handle->method_handle, file_info, context);
	}
	else
	{
		if (!handle->children)
		{
			return GNOME_VFS_ERROR_EOF;
		}

		node = handle->children->data;

		/* TODO: use the real directory file info */
		
		file_info->name = g_strdup (node->name);
		file_info->type = GNOME_VFS_FILE_TYPE_DIRECTORY;
		file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_TYPE;
		//file_info->mime_type = g_strdup ("application/x-ifolder");
		file_info->mime_type = g_strdup ("x-directory/normal");
		file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_MIME_TYPE;

		handle->children = handle->children->next;

		result = GNOME_VFS_OK;
	}

	return result;
}

static GnomeVFSResult
do_close_directory (GnomeVFSMethod          *method,
					GnomeVFSMethodHandle    *method_handle,
					GnomeVFSContext         *context)
{
	GnomeVFSResult result;
	dir_handle *handle = (dir_handle *) method_handle;

	if (handle->gnode)
	{
		free_dir (handle->gnode);

		result = GNOME_VFS_OK;
	}
	else
	{
		result = (* file_method->close_directory) (file_method, handle->method_handle, context);
	}

	g_free (handle);

	return result;
}

static GnomeVFSResult
do_get_file_info (GnomeVFSMethod            *method,
				  GnomeVFSURI               *uri,
				  GnomeVFSFileInfo          *file_info,
				  GnomeVFSFileInfoOptions   options,
				  GnomeVFSContext           *context)
{
	GnomeVFSResult result;
	GnomeVFSURI *file_uri;

	file_uri = get_file_uri (uri);

	if (!file_uri)
	{
		file_info->name = g_strdup ("iFolders");
		file_info->type = GNOME_VFS_FILE_TYPE_DIRECTORY;
		file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_TYPE;
		file_info->mime_type = g_strdup ("application/x-ifolders");
		//file_info->mime_type = g_strdup ("x-directory/normal");
		file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_MIME_TYPE;
		file_info->permissions =
			GNOME_VFS_PERM_USER_READ |
			GNOME_VFS_PERM_OTHER_READ |
			GNOME_VFS_PERM_GROUP_READ;
		file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_PERMISSIONS;

		result = GNOME_VFS_OK;
	}
	else
	{
		result = (* file_method->get_file_info) (file_method, file_uri, file_info, options, context);

		gnome_vfs_uri_unref (file_uri);
	}

	return result;
}

static GnomeVFSResult
do_get_file_info_from_handle (GnomeVFSMethod *method,
			      GnomeVFSMethodHandle *method_handle,
			      GnomeVFSFileInfo *file_info,
			      GnomeVFSFileInfoOptions options,
			      GnomeVFSContext *context)
{
	GnomeVFSResult result;

	result = (* file_method->get_file_info_from_handle) (method, method_handle, file_info, options, context);

	return result;
}

static gboolean
do_is_local (GnomeVFSMethod     *method,
			 const GnomeVFSURI  *uri)
{
	return TRUE;

	/* too slow - work on ifolder lookup
	gboolean result;
	GnomeVFSURI *file_uri;

	file_uri = get_file_uri (uri);

	if (!file_uri)
	{
		result = TRUE;
	}
	else
	{
		result = (* file_method->is_local) (file_method, file_uri);
		
		gnome_vfs_uri_unref (file_uri);
	}

	return result; */
}

static GnomeVFSMethod method =
{
	sizeof (GnomeVFSMethod),
	do_open,						/* open */
	do_create,						/* create */
	do_close,						/* close */
	do_read,						/* read */
	do_write,						/* write */
	do_seek,						/* seek */
	do_tell,						/* tell */
	do_truncate_handle,				/* truncate_handle */ 
	do_open_directory,				/* open_directory */
	do_close_directory,				/* close_directory */
	do_read_directory,				/* read_directory */     
	do_get_file_info,				/* get_file_info */
	do_get_file_info_from_handle,	/* get_file_info_from_handle */
	do_is_local,					/* is_local */
	NULL,							/* make_directory */
	NULL,							/* remove_directory */
	NULL,							/* move */
	NULL,							/* unlink */
	NULL,							/* check_same_fs */
	NULL,							/* set_file_info */
	do_truncate,					/* truncate */
	NULL,							/* find_directory */
	NULL,							/* create_symbolic_link */
	NULL,							/* monitor_add */
	NULL,							/* monitor_cancel */
	NULL,							/* file_control */
	NULL							/* forget_cache */
};

GnomeVFSMethod *vfs_module_init(const char *method_name, const char *args)
{
	/* ifolders volume */
	create_volume();

	/* file method */
	file_method = gnome_vfs_method_get ("file");

	if (file_method == NULL)
	{
		g_error ("Error getting 'file' method for gnome-vfs");
		return NULL;
	}

	return &method;
}

void vfs_module_shutdown (GnomeVFSMethod* method)
{
}

