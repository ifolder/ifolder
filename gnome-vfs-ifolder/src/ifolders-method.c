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

#define IFOLDERS_URI "ifolders:"

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
	return GNOME_VFS_OK;
}

static GnomeVFSResult
do_read_directory (GnomeVFSMethod			*method,
				   GnomeVFSMethodHandle		*method_handle,
				   GnomeVFSFileInfo			*file_info,
				   GnomeVFSContext			*context)
{
	file_info->name = g_strdup ("iFolders");
	file_info->type = GNOME_VFS_FILE_TYPE_DIRECTORY;
	file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_TYPE;
	file_info->mime_type = g_strdup ("x-directory/normal");
	file_info->valid_fields |= GNOME_VFS_FILE_INFO_FIELDS_MIME_TYPE;
	
	return GNOME_VFS_OK;
}

static GnomeVFSResult
do_close_directory (GnomeVFSMethod			*method,
					GnomeVFSMethodHandle	*method_handle,
					GnomeVFSContext			*context)
{
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

static gboolean create_volume(void)
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

