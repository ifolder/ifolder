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
#ifndef IFOLDER_GTK_INTERNAL_H
#define IFOLDER_GTK_INTERNAL_H 1

#include <stdbool.h>

#include <gtk/gtk.h>

typedef struct _iFolder
{
	char *id;
	char *domain_id;
	char *managed_path;
	char *unmanaged_path;
	char *name;
	char *owner;
	char *owner_id;
	char *type;
	char *description;
	char *state;
	char *current_user_id;
	char *current_user_rights;
	char *collection_id;
	char *last_sync_time;
	char *role;
	int sync_interval;
	int effective_sync_interval;
	bool synchronizable;
	bool is_subscription;
	bool is_workgroup;
	bool has_conflicts;
} iFolder;

typedef struct _iFolderList
{
	struct iFolder *ifolder;
	struct _iFolderList *next;
} iFolderList;

typedef struct _iFolderDiskSpace
{
	long long available_space;
	long long limit;
	long long used_space;
} iFolderDiskSpace;

typedef struct _iFolderUser
{
	char *name;
	char *user_id;
	char *rights;
	char *id;
	char *state;
	char *ifolder_id;
	char *first_name;
	char *surname;
	char *full_name;
	bool is_owner;
} iFolderUser;

typedef struct _iFolderUserList
{
	struct _iFolderUser *ifolder_user;
	struct _iFolderUserList *next;
} iFolderUserList;

void ifolder_free_char_data (gpointer data);

int ifolder_get_all_ifolders (iFolderList **ifolder_list);
int ifolder_get (iFolder **ifolder, const char *ifolder_id);

int ifolder_free (iFolder **ifolder);
int ifolder_free_list (iFolderList **ifolder_list);

int ifolder_get_default_sync_interval (int *sync_interval);

int ifolder_get_objects_to_sync (int *objects_to_sync, const char *ifolder_id);

int ifolder_get_disk_space (iFolderDiskSpace **disk_space, const char *ifolder_id);

int ifolder_sync_now (const char *ifolder_id);

int ifolder_save_disk_space_limit (const char *ifolder_id, long long limit);

int ifolder_get_members (iFolderUserList **user_list, const char *ifolder_id);
int ifolder_free_user (iFolderUser **ifolder_user);
int ifolder_free_user_list (iFolderUserList **user_list);

#endif
