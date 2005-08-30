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

#include <glib.h>
#include <libgnomevfs/gnome-vfs.h>
#include <libgnomevfs/gnome-vfs-module.h>

#include <simiasweb.h>
//#include <simias-event-client.h>

#include <iFolderStub.h>
#include <iFolder.nsmap>
//#include <ifolder.h>

#define IFOLDER_METHOD "ifolder:"

#ifdef TRUE
#define DEBUG_IFOLDER(x) (g_print("gnome-vfs-ifolder: "), g_printf x)
#else
#define DEBUG_IFOLDER(x) 
#endif

static GMutex *ifolder_lock = NULL;

static GHashTable *ifolders_table = NULL;

static GHashTable *owners_table = NULL;

static GnomeVFSMethod *file_method = NULL;

typedef enum
{
	IFOLDER_URI_ERROR,
	IFOLDER_URI_ROOT,
	IFOLDER_URI_IFOLDERS,
	IFOLDER_URI_OWNERS,
	IFOLDER_URI_IFOLDER,
	IFOLDER_URI_OWNER,
	IFOLDER_URI_FILE
} ifolder_uri_type_t;

typedef struct
{
	gchar *name;
	gchar *path;
	gchar *owner;
} entry_t;

typedef struct
{
	ifolder_uri_type_t uri_type;
	GSList *root;
	GSList *current;
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

static entry_t *
copy_entry (entry_t *entry)
{
	entry_t *result;

	result = g_new0 (entry_t, 1);
	result->name = g_strdup (entry->name);
	result->path = g_strdup (entry->path);
	result->owner = g_strdup (entry->owner);

	return result;
}

static entry_t *
new_ifolder_entry (struct ns1__iFolderWeb *ifolder)
{
	entry_t *entry;

	entry = g_new0 (entry_t, 1);
	entry->name = g_strdup (ifolder->Name);
	entry->path = g_strdup (ifolder->UnManagedPath);
	entry->owner = g_strdup (ifolder->Owner);

	return entry;
}

static entry_t *
new_name_entry (char *name)
{
	entry_t *entry;

	entry = g_new0 (entry_t, 1);
	entry->name = g_strdup (name);
	entry->path = NULL;
	entry->owner = NULL;
	
	return entry;
}

static void
free_entry (entry_t *entry)
{
	g_free (entry->name);
	g_free (entry->path);
	g_free (entry->owner);
	g_free (entry);
}

static void
free_entry_list (gpointer data, gpointer user_data)
{
	entry_t *entry = (entry_t *) data;

	free_entry (entry);
}

static void
free_entry_table (gpointer key, gpointer value, gpointer user_data)
{
	entry_t *entry = (entry_t *) value;

	free_entry (entry);
}

static void
add_entry_to_list (gpointer key, gpointer value, gpointer user_data)
{
	entry_t *entry = (entry_t *) value;
	GSList **root = (GSList **) user_data;

	*root = g_slist_append (*root, copy_entry (entry));
}

static void
free_ifolders (void)
{
	g_mutex_lock (ifolder_lock);
	
	if (ifolders_table)
	{
		g_hash_table_foreach (ifolders_table, free_entry_table, NULL);
		g_hash_table_destroy (ifolders_table);
		ifolders_table = NULL;
	}

	if (owners_table)
	{
		g_hash_table_foreach (owners_table, free_entry_table, NULL);
		g_hash_table_destroy (owners_table);
		owners_table = NULL;
	}
	
	g_mutex_unlock (ifolder_lock);
}

static GNode *
refresh_ifolders (void)
{
	struct soap soap;
	struct _ns1__GetAlliFolders input;
	struct _ns1__GetAlliFoldersResponse output;
	char username[PATH_MAX];
	char password[PATH_MAX];
	char *url;
	int i;

	free_ifolders ();

	g_mutex_lock (ifolder_lock);
	
	ifolders_table = g_hash_table_new (g_str_hash, g_str_equal);
	owners_table = g_hash_table_new (g_str_hash, g_str_equal);
	
	soap_init2 (&soap, SOAP_C_UTFSTRING, SOAP_C_UTFSTRING);
	soap_set_namespaces (&soap, iFolder_namespaces);

	if (simias_get_web_service_credential (username, password) == SIMIAS_SUCCESS)
	{
		soap.userid = username;
		soap.passwd = password;
	}

	url = get_local_ifolder_url ();

	soap_call___ns1__GetAlliFolders (&soap, url, NULL, &input, &output);

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
					entry_t *ifolder_entry = new_ifolder_entry (ifolder);

					g_hash_table_insert (ifolders_table, ifolder_entry->name, ifolder_entry);

					entry_t *owner_entry = new_name_entry (ifolder->Owner);

					g_hash_table_insert (owners_table, owner_entry->name, owner_entry);
				}
			}
		}
	}

	soap_end(&soap);

	g_free(url);

	g_mutex_unlock (ifolder_lock);
	
	return;
}

static entry_t *
get_ifolder (const char *name)
{
	entry_t *result = NULL;

	g_mutex_lock (ifolder_lock);
	
	result = (entry_t *) g_hash_table_lookup (ifolders_table, name);
	
	g_mutex_unlock (ifolder_lock);
	
	return result;
}

static GnomeVFSURI *
parse_ifolder_uri (const GnomeVFSURI *uri, ifolder_uri_type_t *type)
{
	GnomeVFSURI *file_uri = NULL;
	const gchar *path;

	path = gnome_vfs_uri_get_path (uri);

	path = gnome_vfs_unescape_string (path, NULL);

	if (path && strcmp(path, "/"))
	{
		char *s;
		char *domain;
		char *name;
		char *p;
		entry_t *entry;

		if (path[0] == '/')
		{
			++path;
		}

		domain = g_strdup (path);

		if (domain[strlen (domain) - 1] == '/')
		{
			domain[strlen (domain) - 1] = '\0';
		}

		p = strchr(domain, '/');

		if (p)
		{
			*p++ = '\0';
		}

		if (!strcmp("owners", domain))
		{
			if (!p)
			{
				*type = IFOLDER_URI_OWNERS;
				return NULL;
			}
			else
			{
				p = strchr(p, '/');

				if (p)
				{
					*p++ = '\0';
				}

				if (!p)
				{
					*type = IFOLDER_URI_OWNER;
					return NULL;
				}
			}
		}
		else if (!strcmp("ifolders", domain))
		{
			if (!p)
			{
				*type = IFOLDER_URI_IFOLDERS;
				return NULL;
			}
		}
		else
		{
			*type = IFOLDER_URI_ERROR;
			return NULL;
		}
		
		name = p;

		p = strchr(p, '/');
		
		if (p)
		{
			*p++ = '\0';
		}

		entry = get_ifolder (name);
		
		if (entry)
		{
			if (!p)
			{
				*type = IFOLDER_URI_IFOLDER;
			}
			else
			{
				*type = IFOLDER_URI_FILE;
			}
			
			s = g_strconcat ("file://", entry->path, "/", p, NULL);

			if (s[strlen (s) - 1] == '/')
			{
				s[strlen (s) - 1] = '\0';
			}

			file_uri = gnome_vfs_uri_new (s);

			g_free (s);
		}
		else
		{
			*type = IFOLDER_URI_ERROR;
			return NULL;
		}

		g_free(domain);
	}
	else
	{
		*type = IFOLDER_URI_ROOT;
	}

	return file_uri;
}

static gboolean
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
		gnome_vfs_connect_to_server(IFOLDER_METHOD, "iFolders", "gnome-mime-x-directory-ifolder-place");
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
    ifolder_uri_type_t uri_type;
	
	file_uri = parse_ifolder_uri (uri, &uri_type);

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
    ifolder_uri_type_t uri_type;
	
	file_uri = parse_ifolder_uri (uri, &uri_type);

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
    ifolder_uri_type_t uri_type;
	
	file_uri = parse_ifolder_uri (uri, &uri_type);

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
    ifolder_uri_type_t uri_type;
	const gchar *path;
	char *leaf = NULL;
	
    DEBUG_IFOLDER(("do_open_directory() %s\n", gnome_vfs_uri_to_string (uri, 0)));

	file_uri = parse_ifolder_uri (uri, &uri_type);

	g_mutex_lock (ifolder_lock);

	/* leaf */
	path = gnome_vfs_uri_get_path (uri);

	path = gnome_vfs_unescape_string (path, NULL);

	leaf = strrchr(path, '/');

	if (leaf) *leaf++;
	
	handle = g_new0 (dir_handle, 1);
	handle->uri_type = uri_type;
	
	switch (uri_type)
	{
		case IFOLDER_URI_ROOT:
			handle->root = NULL;
			handle->root = g_slist_append (handle->root, new_name_entry ("ifolders"));
			handle->root = g_slist_append (handle->root, new_name_entry ("owners"));
			handle->current = handle->root;
			result = GNOME_VFS_OK;
			break;

		case IFOLDER_URI_IFOLDERS:
			handle->root = NULL;
            g_hash_table_foreach (ifolders_table, add_entry_to_list, &handle->root);
			handle->current = handle->root;
			result = GNOME_VFS_OK;
			break;

		case IFOLDER_URI_OWNERS:
			handle->root = NULL;
			g_hash_table_foreach (owners_table, add_entry_to_list, &handle->root);
			handle->current = handle->root;
			result = GNOME_VFS_OK;
			break;

		case IFOLDER_URI_OWNER:
			if (leaf)
			{
				GSList *ifolders;
				GSList *current;
				
				ifolders = NULL;
				g_hash_table_foreach (ifolders_table, add_entry_to_list, &ifolders);


				handle->root = NULL;
				current = ifolders;
				
				/* TODO: cleanup */
				while (current)
				{
					entry_t *entry = (entry_t *) current->data;
					
					if (!strcmp (leaf, entry->owner))
					{
						handle->root = g_slist_append (handle->root, copy_entry (entry));
					}

					current = current->next;
				}
				
				g_slist_foreach (ifolders, free_entry_list, NULL);
				g_slist_free (ifolders);

				handle->current = handle->root;
				result = GNOME_VFS_OK;
			}
			else
			{
				result = GNOME_VFS_ERROR_INVALID_URI;
			}
			break;
			
		case IFOLDER_URI_IFOLDER:
		case IFOLDER_URI_FILE:
			result = (* file_method->open_directory) (file_method, &(handle->method_handle), file_uri, options, context);
			break;
			
		default:
			result = GNOME_VFS_ERROR_INVALID_URI;
			break;
	}

	g_mutex_unlock (ifolder_lock);

	gnome_vfs_uri_unref (file_uri);

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
	entry_t *entry;
	
	switch (handle->uri_type)
	{
		case IFOLDER_URI_ROOT:

			if (!handle->current)
			{
				return GNOME_VFS_ERROR_EOF;
			}

			entry = (entry_t *) handle->current->data;

			file_info->name = g_strdup (entry->name);
			file_info->type = GNOME_VFS_FILE_TYPE_DIRECTORY;
			file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_TYPE;

			if (!strcmp(entry->name, "owners"))
			{
				file_info->mime_type = g_strdup ("x-directory/ifolder-users");
			}
			else
			{
				file_info->mime_type = g_strdup ("x-directory/ifolder-ifolders");
			}
			
			file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_MIME_TYPE;
			file_info->permissions =
				GNOME_VFS_PERM_USER_READ |
				GNOME_VFS_PERM_OTHER_READ |
				GNOME_VFS_PERM_GROUP_READ;
			file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_PERMISSIONS;
			
			handle->current = handle->current->next;
			
            result = GNOME_VFS_OK;

			break;

		case IFOLDER_URI_IFOLDERS:
		case IFOLDER_URI_OWNER:

			if (!handle->current)
			{
				return GNOME_VFS_ERROR_EOF;
			}

			entry = (entry_t *) handle->current->data;

			file_info->name = g_strdup (entry->name);
			file_info->type = GNOME_VFS_FILE_TYPE_DIRECTORY;
			file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_TYPE;
			file_info->mime_type = g_strdup ("x-directory/ifolder");
			file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_MIME_TYPE;
			file_info->permissions =
				GNOME_VFS_PERM_USER_READ |
				GNOME_VFS_PERM_OTHER_READ |
				GNOME_VFS_PERM_GROUP_READ;
			file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_PERMISSIONS;
			
			handle->current = handle->current->next;
			
            result = GNOME_VFS_OK;

			break;

		case IFOLDER_URI_OWNERS:

			if (!handle->current)
			{
				return GNOME_VFS_ERROR_EOF;
			}

			entry = (entry_t *) handle->current->data;

			file_info->name = g_strdup (entry->name);
			file_info->type = GNOME_VFS_FILE_TYPE_DIRECTORY;
			file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_TYPE;
			file_info->mime_type = g_strdup ("x-directory/ifolder-user");
			file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_MIME_TYPE;
			file_info->permissions =
				GNOME_VFS_PERM_USER_READ |
				GNOME_VFS_PERM_OTHER_READ |
				GNOME_VFS_PERM_GROUP_READ;
			file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_PERMISSIONS;

			handle->current = handle->current->next;

			result = GNOME_VFS_OK;

			break;

		case IFOLDER_URI_IFOLDER:
		case IFOLDER_URI_FILE:
			result = (* file_method->read_directory) (file_method, handle->method_handle, file_info, context);
			break;

		default:
            result = GNOME_VFS_ERROR_EOF;
			break;
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

    DEBUG_IFOLDER(("do_close_directory()\n"));

	switch (handle->uri_type)
	{
		case IFOLDER_URI_ROOT:
		case IFOLDER_URI_IFOLDERS:
		case IFOLDER_URI_OWNERS:
		case IFOLDER_URI_OWNER:
			g_slist_foreach (handle->root, free_entry_list, NULL);
			g_slist_free (handle->root);
			result = GNOME_VFS_OK;
			break;

		case IFOLDER_URI_IFOLDER:
		case IFOLDER_URI_FILE:
			result = (* file_method->close_directory) (file_method, handle->method_handle, context);
			break;

		default:
            result = GNOME_VFS_OK;
			break;
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
    ifolder_uri_type_t uri_type;
	
	file_uri = parse_ifolder_uri (uri, &uri_type);

	switch (uri_type)
	{
		case IFOLDER_URI_ROOT:
			file_info->type = GNOME_VFS_FILE_TYPE_DIRECTORY;
			file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_TYPE;
			file_info->mime_type = g_strdup ("x-directory/ifolder-place");
			file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_MIME_TYPE;
			file_info->permissions =
				GNOME_VFS_PERM_USER_READ |
				GNOME_VFS_PERM_OTHER_READ |
				GNOME_VFS_PERM_GROUP_READ;
			file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_PERMISSIONS;
	
			result = GNOME_VFS_OK;
			break;

		case IFOLDER_URI_IFOLDERS:
			file_info->type = GNOME_VFS_FILE_TYPE_DIRECTORY;
			file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_TYPE;
			file_info->mime_type = g_strdup ("x-directory/ifolder-ifolders");
			file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_MIME_TYPE;
			file_info->permissions =
				GNOME_VFS_PERM_USER_READ |
				GNOME_VFS_PERM_OTHER_READ |
				GNOME_VFS_PERM_GROUP_READ;
			file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_PERMISSIONS;
	
			result = GNOME_VFS_OK;
			break;
			
		case IFOLDER_URI_OWNERS:
			file_info->type = GNOME_VFS_FILE_TYPE_DIRECTORY;
			file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_TYPE;
			file_info->mime_type = g_strdup ("x-directory/ifolder-users");
			file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_MIME_TYPE;
			file_info->permissions =
				GNOME_VFS_PERM_USER_READ |
				GNOME_VFS_PERM_OTHER_READ |
				GNOME_VFS_PERM_GROUP_READ;
			file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_PERMISSIONS;
	
			result = GNOME_VFS_OK;
			break;

		case IFOLDER_URI_IFOLDER:
			file_info->type = GNOME_VFS_FILE_TYPE_DIRECTORY;
			file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_TYPE;
			file_info->mime_type = g_strdup ("x-directory/ifolder");
			file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_MIME_TYPE;
			file_info->permissions =
				GNOME_VFS_PERM_USER_READ |
				GNOME_VFS_PERM_OTHER_READ |
				GNOME_VFS_PERM_GROUP_READ;
			file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_PERMISSIONS;
	
			result = GNOME_VFS_OK;
			break;

		case IFOLDER_URI_OWNER:
			file_info->type = GNOME_VFS_FILE_TYPE_DIRECTORY;
			file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_TYPE;
			file_info->mime_type = g_strdup ("x-directory/ifolder-user");
			file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_MIME_TYPE;
			file_info->permissions =
				GNOME_VFS_PERM_USER_READ |
				GNOME_VFS_PERM_OTHER_READ |
				GNOME_VFS_PERM_GROUP_READ;
			file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_PERMISSIONS;

			result = GNOME_VFS_OK;
			break;

		case IFOLDER_URI_FILE:
			result = (* file_method->get_file_info) (file_method, file_uri, file_info, options, context);
			break;

		default:
            result = GNOME_VFS_OK;
			break;
	}
	
	gnome_vfs_uri_unref (file_uri);

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
	gboolean result;
	GnomeVFSURI *file_uri;
    ifolder_uri_type_t uri_type;
	
	file_uri = parse_ifolder_uri (uri, &uri_type);

	if (!file_uri)
	{
		result = TRUE;
	}
	else
	{
		result = (* file_method->is_local) (file_method, file_uri);
		
		gnome_vfs_uri_unref (file_uri);
	}

	return result;
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
	ifolder_lock = g_mutex_new ();
	
	refresh_ifolders ();

	create_volume ();

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
	free_ifolders ();

	g_mutex_free (ifolder_lock);
}

