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
#include "ifolder-gtk.h"

#include "ifolder-gtk-internal.h"

#include <stdio.h>
#include <stdbool.h>
#include <string.h>

#include <glib-object.h>

typedef struct _SharingPropertyPageWidgets
{
	GtkWidget *top_level_window;

	GtkWidget *user_tree_view;
	GtkListStore *user_list_store;

	GdkPixbuf *user_pixbuf;
	GdkPixbuf *current_user_pixbuf;
	GdkPixbuf *invited_pixbuf;

	GtkWidget *add_button;
	GtkWidget *remove_button;
	GtkWidget *access_button;
	
	GHashTable *cur_users;
	GHashTable *member_full_names;
	GHashTable *duplicate_members;
} SharingPropertyPageWidgets;

enum
{
	USER_ID_COL,
	USER_NAME_COL,
	USER_FULL_NAME_COL,
	STATE_COL,
	ACCESS_COL,
	N_COLS
};


static void initialize_widgets (GtkWidget *sharing_widget);

static void reload_users_list_store (GtkWidget *sharing_widget);

static void cur_users_ht_destroy_key (gpointer key);
static void cur_users_ht_destroy_value (gpointer value);

static void member_full_names_ht_destroy_key (gpointer key);
static void member_full_names_ht_destroy_value (gpointer value);

static void duplicate_members_ht_destroy_key (gpointer key);
static void duplicate_members_ht_destroy_value (gpointer value);

static void tree_selection_changed_callback (GtkTreeSelection *treeselection,
											 gpointer user_data);
static void add_button_callback (GtkButton *button, gpointer user_data);
static void remove_button_callback (GtkButton *button, gpointer user_data);
static void access_button_callback (GtkButton *button, gpointer user_data);

static void free_widgets_struct (gpointer data);

static gboolean ht_remove (gpointer key, gpointer value, gpointer user_data);

static void user_cell_pixbuf_data_func (GtkTreeViewColumn *tree_column,
										GtkCellRenderer *cell,
										GtkTreeModel *tree_model,
										GtkTreeIter *iter,
										gpointer data);

static void user_cell_text_data_func (GtkTreeViewColumn *tree_column,
									  GtkCellRenderer *cell,
									  GtkTreeModel *tree_model,
									  GtkTreeIter *iter,
									  gpointer data);

static void state_cell_text_data_func (GtkTreeViewColumn *tree_column,
									   GtkCellRenderer *cell,
									   GtkTreeModel *tree_model,
									   GtkTreeIter *iter,
									   gpointer data);

static void access_cell_text_data_func (GtkTreeViewColumn *tree_column,
										GtkCellRenderer *cell,
										GtkTreeModel *tree_model,
										GtkTreeIter *iter,
										gpointer data);


GtkWidget *
ifolder_sharing_property_page_new (const char *ifolder_id)
{
	GtkWidget *vbox;
	SharingPropertyPageWidgets *widgets;
	
	widgets = (SharingPropertyPageWidgets *)
				malloc (sizeof (SharingPropertyPageWidgets));
	memset (widgets, 0, sizeof (SharingPropertyPageWidgets));

	vbox = gtk_vbox_new (false, 10);
	gtk_container_set_border_width (GTK_CONTAINER (vbox), 10);

	// Store the iFFolder ID with our main GtkWidget (vbox) so that callback
	// functions will be able to perform actions on the specified iFolder.
	g_object_set_data_full (G_OBJECT (vbox), "ifolder_id",
							(gpointer) strdup (ifolder_id), ifolder_free_char_data);
	g_object_set_data_full (G_OBJECT (vbox), "widgets",
							widgets, free_widgets_struct);
	
	widgets->cur_users =
		g_hash_table_new_full (g_str_hash, g_str_equal,
							   cur_users_ht_destroy_key,
							   cur_users_ht_destroy_value);

	widgets->member_full_names =
		g_hash_table_new_full (g_str_hash, g_str_equal,
							   cur_users_ht_destroy_key,
							   cur_users_ht_destroy_value);
	
	widgets->duplicate_members =
		g_hash_table_new_full (g_str_hash, g_str_equal,
							   cur_users_ht_destroy_key,
							   cur_users_ht_destroy_value);
	
	initialize_widgets (vbox);
	
	return vbox;
}

static void
initialize_widgets (GtkWidget *sharing_widget)
{
	SharingPropertyPageWidgets *widgets;
	char *ifolder_id;
	
	GtkTreeSelection *tree_sel;
	
	GtkWidget *scrolled_window;
	
	GtkWidget *button_box;
	GtkWidget *left_box;
	GtkWidget *mid_box;
	GtkWidget *right_box;
	
	GtkTreeViewColumn *user_column, *state_column, *access_column;
	GtkCellRenderer *cell_renderer;
	
	GtkWidget *image;

	widgets = (SharingPropertyPageWidgets *)
			  g_object_get_data (G_OBJECT (sharing_widget), "widgets");
	ifolder_id = g_object_get_data (G_OBJECT (sharing_widget), "ifolder_id");

	scrolled_window = gtk_scrolled_window_new (NULL, NULL);
	gtk_scrolled_window_set_shadow_type (GTK_SCROLLED_WINDOW (scrolled_window),
										 GTK_SHADOW_ETCHED_IN);
	gtk_scrolled_window_set_policy (GTK_SCROLLED_WINDOW (scrolled_window),
									GTK_POLICY_AUTOMATIC,
									GTK_POLICY_AUTOMATIC);

	///
	/// Set up the TreeView
	///
	widgets->user_list_store =
		gtk_list_store_new (N_COLS,
							G_TYPE_STRING,		// User ID
							G_TYPE_STRING,		// User Name
							G_TYPE_STRING,		// User Full Name
							G_TYPE_STRING,		// State
							G_TYPE_STRING);		// Access

	gtk_tree_sortable_set_sort_column_id (GTK_TREE_SORTABLE (widgets->user_list_store),
										  USER_FULL_NAME_COL, GTK_SORT_ASCENDING);

	reload_users_list_store (sharing_widget);

	widgets->user_tree_view = gtk_tree_view_new_with_model (GTK_TREE_MODEL (widgets->user_list_store));
	g_object_unref (widgets->user_list_store);	// Remove our own reference now that the treeview has it
	// Single selection mode only
	tree_sel = gtk_tree_view_get_selection (GTK_TREE_VIEW (widgets->user_tree_view));
	gtk_tree_selection_set_mode (tree_sel, GTK_SELECTION_MULTIPLE);
	g_signal_connect (tree_sel,
					  "changed",
					  G_CALLBACK (tree_selection_changed_callback),
					  (gpointer) sharing_widget);

	// Set up the columns

	// User Name Column
	user_column = gtk_tree_view_column_new ();
	gtk_tree_view_column_set_title (user_column, "User Name");
	gtk_tree_view_column_set_expand (user_column, true);
	gtk_tree_view_column_set_resizable (user_column, true);

	cell_renderer = gtk_cell_renderer_pixbuf_new ();
	gtk_tree_view_column_pack_start (user_column, cell_renderer, false);
	gtk_tree_view_column_set_cell_data_func (user_column, cell_renderer,
											 user_cell_pixbuf_data_func,
											 sharing_widget, NULL);
	
	cell_renderer = gtk_cell_renderer_text_new ();
	gtk_tree_view_column_pack_start (user_column, cell_renderer, true);
	gtk_tree_view_column_set_cell_data_func (user_column, cell_renderer,
											 user_cell_text_data_func,
											 sharing_widget, NULL);

	gtk_tree_view_insert_column (GTK_TREE_VIEW (widgets->user_tree_view),
								 user_column, -1);

	// State Column
	state_column = gtk_tree_view_column_new ();
	gtk_tree_view_column_set_title (state_column, "State");
	gtk_tree_view_column_set_resizable (state_column, true);
	gtk_tree_view_column_set_min_width (state_column, 150);
	gtk_tree_view_column_pack_start (state_column, cell_renderer, true);
	gtk_tree_view_column_set_cell_data_func (state_column, cell_renderer,
											 state_cell_text_data_func,
											 sharing_widget, NULL);
	gtk_tree_view_insert_column (GTK_TREE_VIEW (widgets->user_tree_view),
								 state_column, -1);

//	gtk_tree_view_insert_column_with_attributes (
//		GTK_TREE_VIEW (widgets->user_tree_view),
//		-1, "State", cell_renderer, "text", STATE_COL, NULL);
	
	// Access Column
	access_column = gtk_tree_view_column_new ();
	gtk_tree_view_column_set_title (access_column, "Access");
	gtk_tree_view_column_set_resizable (access_column, true);
	gtk_tree_view_column_pack_start (access_column, cell_renderer, true);
	gtk_tree_view_column_set_cell_data_func (access_column, cell_renderer,
											 access_cell_text_data_func,
											 sharing_widget, NULL);
	gtk_tree_view_insert_column (GTK_TREE_VIEW (widgets->user_tree_view),
								 access_column, -1);

//	gtk_tree_view_insert_column_with_attributes (
//		GTK_TREE_VIEW (widgets->user_tree_view),
//		-1, "Access", cell_renderer, "text", ACCESS_COL, NULL);

	gtk_container_add (GTK_CONTAINER (scrolled_window), widgets->user_tree_view);
	
	gtk_box_pack_start (GTK_BOX (sharing_widget), scrolled_window, true, true, 0);

	///
	/// Set up the Pixbuf Images
	///
	image = gtk_image_new_from_file (IFOLDER_USER_PIXBUF);
	if (image != NULL)
		widgets->user_pixbuf = gtk_image_get_pixbuf (GTK_IMAGE (image));
	image = gtk_image_new_from_file (IFOLDER_OWNER_PIXBUF);
	if (image != NULL)
		widgets->current_user_pixbuf = gtk_image_get_pixbuf (GTK_IMAGE (image));
	image = gtk_image_new_from_file (IFOLDER_INVITED_USER_PIXBUF);
	if (image != NULL)
		widgets->invited_pixbuf = gtk_image_get_pixbuf (GTK_IMAGE (image));

	///
	/// Set up buttons for add/remove/accept
	///
	button_box = gtk_hbox_new (false, 10);
	gtk_box_pack_start (GTK_BOX (sharing_widget), button_box, false, false, 0);
	
	left_box = gtk_hbox_new (false, 10);
	gtk_box_pack_start (GTK_BOX (button_box), left_box, false, false, 0);
	
	mid_box = gtk_hbox_new (false, 10);
	gtk_box_pack_start (GTK_BOX (button_box), mid_box, true, true, 0);
	
	right_box = gtk_hbox_new (false, 10);
	gtk_box_pack_start (GTK_BOX (button_box), right_box, false, false, 0);
	
	widgets->add_button = gtk_button_new_from_stock (GTK_STOCK_ADD);
	gtk_box_pack_start (GTK_BOX (right_box), widgets->add_button, false, false, 0);
	g_signal_connect (widgets->add_button,
					  "clicked",
					  G_CALLBACK (add_button_callback),
					  (gpointer) sharing_widget);

	widgets->remove_button = gtk_button_new_from_stock (GTK_STOCK_REMOVE);
	gtk_box_pack_start (GTK_BOX (right_box), widgets->remove_button, false, false, 0);
	g_signal_connect (widgets->remove_button,
					  "clicked",
					  G_CALLBACK (remove_button_callback),
					  (gpointer) sharing_widget);

	
	widgets->access_button = gtk_button_new_with_mnemonic ("Acc_ess...");
	gtk_box_pack_start (GTK_BOX (left_box), widgets->access_button, false, false, 0);
	g_signal_connect (widgets->access_button,
					  "clicked",
					  G_CALLBACK (access_button_callback),
					  (gpointer) sharing_widget);
}

static void
reload_users_list_store (GtkWidget *sharing_widget)
{
	SharingPropertyPageWidgets *widgets;
	char *ifolder_id;

	iFolderUser *user;
	iFolderUserList *user_list, *item;
	
	int rc;
	
	GtkTreeIter *iter;
	
	char *key;
	char *value;

	widgets = (SharingPropertyPageWidgets *)
			  g_object_get_data (G_OBJECT (sharing_widget), "widgets");
	ifolder_id = g_object_get_data (G_OBJECT (sharing_widget), "ifolder_id");


	// 1. Clear out the old list
	// 2. Get the user list
	// 3. Add the users in

	g_hash_table_foreach_remove (widgets->cur_users, ht_remove, NULL);
	g_hash_table_foreach_remove (widgets->member_full_names, ht_remove, NULL);
	g_hash_table_foreach_remove (widgets->duplicate_members, ht_remove, NULL);
	
	gtk_list_store_clear (widgets->user_list_store);

	rc = ifolder_get_members (&user_list, ifolder_id);
	if (rc != 0) {
		printf ("ifolder_get_members() returned %d\n", rc);
		return;
	}
	
	item = user_list;
	
	while (item) {
		user = (iFolderUser *) item->ifolder_user;
		
		if (g_hash_table_lookup (widgets->member_full_names, user->full_name)) {
			// This is a duplicate username
			key = strdup (user->full_name);
			value = key;

			g_hash_table_insert (widgets->duplicate_members, key, value);
		}
		else {
			key = strdup (user->full_name);
			value = key;
			g_hash_table_insert (widgets->member_full_names, key, value);
		}

		iter = malloc (sizeof (GtkTreeIter));
		gtk_list_store_append (widgets->user_list_store, iter);
		gtk_list_store_set (widgets->user_list_store, iter,
			USER_ID_COL,		user->id,
			USER_NAME_COL,		user->name,
			USER_FULL_NAME_COL, user->full_name,
			STATE_COL,			user->state,
			ACCESS_COL,			user->rights,
			-1);
		
		// Store off the GtkTreeIter in cur_users GHashTable
		g_hash_table_insert (widgets->cur_users, strdup (user->id), iter);

		item = item->next;
	}
	
	ifolder_free_user_list (&user_list);
}


void
ifolder_sharing_property_page_update (GtkWidget *property_page,
									  const char *ifolder_id)
{
//	g_object_set_data_full (G_OBJECT (property_page), "ifolder_id",
//							(gpointer) strdup (ifolder_id), ifolder_free_char_data);

//	reload_users_list_store (property_page);

//	tree_selection_changed_callback (NULL, property_page);
}

static void
cur_users_ht_destroy_key (gpointer key)
{
	char *user_id = (char *)key;
	
	free (user_id);
}

static void
cur_users_ht_destroy_value (gpointer value)
{
	GtkTreeIter *pIter = (GtkTreeIter *)value;
	
	free (pIter);
}

static void
member_full_names_ht_destroy_key (gpointer key)
{
	char *user_fn = (char *)key;
	
	free (user_fn);
}

static void
member_full_names_ht_destroy_value (gpointer value)
{
	// Do nothing
}

static void
duplicate_members_ht_destroy_key (gpointer key)
{
	char *user_fn = (char *)key;
	
	free (user_fn);
}

static void
duplicate_members_ht_destroy_value (gpointer value)
{
	// Do nothing
}

static void
tree_selection_changed_callback (GtkTreeSelection *treeselection,
								 gpointer user_data)
{
	GtkWidget *sharing_widget;
	char *ifolder_id;
	
	sharing_widget = (GtkWidget *)user_data;
	
	ifolder_id = g_object_get_data (G_OBJECT (sharing_widget), "ifolder_id");

	printf ("tree_selection_changed_callback: %s\n", ifolder_id);
	
	// FIXME: Implement tree_selection_changed_callback
}

static void
add_button_callback (GtkButton *button, gpointer user_data)
{
	GtkWidget *sharing_widget;
	char *ifolder_id;
	
	sharing_widget = (GtkWidget *)user_data;
	
	ifolder_id = g_object_get_data (G_OBJECT (sharing_widget), "ifolder_id");
	
	printf ("add_button_callback: %s\n", ifolder_id);

	// FIXME: Implement add_button_callback
}

static void
remove_button_callback (GtkButton *button, gpointer user_data)
{
	GtkWidget *sharing_widget;
	char *ifolder_id;
	
	sharing_widget = (GtkWidget *)user_data;
	
	ifolder_id = g_object_get_data (G_OBJECT (sharing_widget), "ifolder_id");
	
	printf ("remove_button_callback: %s\n", ifolder_id);

	// FIXME: Implement remove_button_callback
}

static void
access_button_callback (GtkButton *button, gpointer user_data)
{
	GtkWidget *sharing_widget;
	char *ifolder_id;
	
	sharing_widget = (GtkWidget *)user_data;
	
	ifolder_id = g_object_get_data (G_OBJECT (sharing_widget), "ifolder_id");
	
	printf ("access_button_callback: %s\n", ifolder_id);

	// FIXME: Implement access_button_callback
}



static void
free_widgets_struct (gpointer data)
{
	SharingPropertyPageWidgets *widgets;
	
printf ("free_widgets_struct\n");
	widgets = (SharingPropertyPageWidgets *)data;
	if (widgets)
		free (widgets);
}

static gboolean
ht_remove (gpointer key, gpointer value, gpointer user_data)
{
	return TRUE;	// Allow the key/value pair to be removed.
}

static void
user_cell_pixbuf_data_func (GtkTreeViewColumn *tree_column,
							GtkCellRenderer *cell,
							GtkTreeModel *tree_model,
							GtkTreeIter *iter,
							gpointer data)
{
	GtkWidget *sharing_widget = (GtkWidget *) data;
	SharingPropertyPageWidgets *widgets;
	GValue gVal = { 0 };
	gchar *user_access;
	gchar *user_state;

	widgets = (SharingPropertyPageWidgets *)
			  g_object_get_data (G_OBJECT (sharing_widget), "widgets");

	gtk_tree_model_get_value(tree_model, iter, ACCESS_COL, &gVal);
	user_access = g_value_get_string(&gVal);
	
	if (user_access != NULL && strcmp (user_access, "Admin") == 0) {
		g_value_unset(&gVal);
		g_object_set (G_OBJECT (cell), "pixbuf", widgets->current_user_pixbuf, NULL);
	} else {
		g_value_unset(&gVal);
		gtk_tree_model_get_value(tree_model, iter, STATE_COL, &gVal);
		user_state = g_value_get_string(&gVal);
		
		if (user_state == NULL || strcmp (user_state, "Member") != 0) {
			g_object_set (G_OBJECT (cell), "pixbuf", widgets->invited_pixbuf, NULL);
		} else {
			g_object_set (G_OBJECT (cell), "pixbuf", widgets->user_pixbuf, NULL);
		}

		g_value_unset(&gVal);
	}
}

static void
user_cell_text_data_func (GtkTreeViewColumn *tree_column,
						  GtkCellRenderer *cell,
						  GtkTreeModel *tree_model,
						  GtkTreeIter *iter,
						  gpointer data)
{
	GtkWidget *sharing_widget = (GtkWidget *) data;
	SharingPropertyPageWidgets *widgets;
	GValue gVal = { 0 };
	gchar *user_name;
	gchar *user_full_name;
	char display_name [2048];

	widgets = (SharingPropertyPageWidgets *)
			  g_object_get_data (G_OBJECT (sharing_widget), "widgets");

	gtk_tree_model_get_value(tree_model, iter, USER_FULL_NAME_COL, &gVal);
	user_full_name = g_value_dup_string(&gVal);
	g_value_unset(&gVal);

	if (g_hash_table_lookup (widgets->duplicate_members, user_full_name)) {
		gtk_tree_model_get_value(tree_model, iter, USER_NAME_COL, &gVal);
		user_name = g_value_get_string(&gVal);
		sprintf (display_name, "%s (%s)", user_full_name, user_name);
		g_value_unset(&gVal);
		g_object_set (G_OBJECT (cell), "text", display_name, NULL);
	} else {
		// Use the unmodified user_full_name
		g_object_set (G_OBJECT (cell), "text", user_full_name, NULL);
	}

	g_free (user_full_name);
}

static void
state_cell_text_data_func (GtkTreeViewColumn *tree_column,
						  GtkCellRenderer *cell,
						  GtkTreeModel *tree_model,
						  GtkTreeIter *iter,
						  gpointer data)
{
	GtkWidget *sharing_widget = (GtkWidget *) data;
	SharingPropertyPageWidgets *widgets;
	GValue gVal = { 0 };
	gchar *user_access;
	gchar *user_state;

	widgets = (SharingPropertyPageWidgets *)
			  g_object_get_data (G_OBJECT (sharing_widget), "widgets");

	gtk_tree_model_get_value(tree_model, iter, ACCESS_COL, &gVal);
	user_access = g_value_get_string(&gVal);
	
	if (user_access != NULL && strcmp (user_access, "Admin") == 0) {
		g_value_unset(&gVal);
		g_object_set (G_OBJECT (cell), "text", "Owner", NULL);
	} else {
		g_value_unset(&gVal);
		gtk_tree_model_get_value(tree_model, iter, STATE_COL, &gVal);
		user_state = g_value_get_string(&gVal);
		
		if (user_state == NULL || strcmp (user_state, "Member") != 0) {
			g_object_set (G_OBJECT (cell), "text", "Invited User", NULL);
		} else {
			g_object_set (G_OBJECT (cell), "text", "iFolder User", NULL);
		}

		g_value_unset(&gVal);
	}
}

static void
access_cell_text_data_func (GtkTreeViewColumn *tree_column,
						  GtkCellRenderer *cell,
						  GtkTreeModel *tree_model,
						  GtkTreeIter *iter,
						  gpointer data)
{
	GtkWidget *sharing_widget = (GtkWidget *) data;
	SharingPropertyPageWidgets *widgets;
	GValue gVal = { 0 };
	gchar *user_access;

	widgets = (SharingPropertyPageWidgets *)
			  g_object_get_data (G_OBJECT (sharing_widget), "widgets");

	gtk_tree_model_get_value(tree_model, iter, ACCESS_COL, &gVal);
	user_access = g_value_get_string(&gVal);
	
	if (user_access != NULL){
		if (strcmp (user_access, "ReadWrite") == 0) {
			g_object_set (G_OBJECT (cell), "text", "Read/Write", NULL);
		} else if (strcmp (user_access, "Admin") == 0) {
			g_object_set (G_OBJECT (cell), "text", "Full Control", NULL);
		} else if (strcmp (user_access, "ReadOnly") == 0) {
			g_object_set (G_OBJECT (cell), "text", "Read Only", NULL);
		} else {
			g_object_set (G_OBJECT (cell), "text", "Unknown", NULL);
		}
	} else {
		g_object_set (G_OBJECT (cell), "text", "Unknown", NULL);
	}

	g_value_unset(&gVal);
}

