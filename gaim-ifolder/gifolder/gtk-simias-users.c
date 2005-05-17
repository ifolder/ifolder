/***********************************************************************
 *  $RCSfile$
 *
 *  Gaim iFolder Plugin: Allows Gaim users to share iFolders.
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
 *  Author: Boyd Timothy <btimothy@novell.com>
 * 
 *  Some code in this file (mostly the saving and reading of the XML files) is
 *  directly based on code found in Gaim's core & plugin files, which is
 *  distributed under the GPL.
 ***********************************************************************/

#include "internal.h"
#include "gtkgaim.h"
#include "gtkblist.h"

#include "account.h"
#include "blist.h"
#include <string.h>
#include <stdio.h>
#include <stdlib.h>
#include "gtkutils.h"

/* Gaim iFolder Plugin Includes */
#include "gtk-simias-users.h"
#include "simias-messages.h"
#include "simias-util.h"

/****************************************************
 * Static Definitions (#defines)                    *
 ****************************************************/

/****************************************************
 * Type declarations                                *
 ****************************************************/
enum
{
	STATUS_ICON_COL,
	DISPLAY_NAME_COL,
	SCREENNAME_COL,
	MACHINE_NAME_COL,
	ACCOUNT_NAME_COL,
	ACCOUNT_PROTO_COL,
	STATUS_COL,
	STATUS_STRING_COL,
	N_COLS
};

/****************************************************
 * Global Variables                                 *
 ****************************************************/
static GtkWidget	*users_window		= NULL;

static GtkWidget	*main_vbox			= NULL;

static GtkWidget	*users_tree_view	= NULL;
static GtkListStore	*users_list_store	= NULL;

static GtkWidget	*toolbar			= NULL;
static GtkToolItem	*tbb_invite			= NULL;
static GtkToolItem	*tbb_cancel_invite	= NULL;
static GtkToolItem	*tbb_share_ifolders	= NULL;
static GtkToolItem	*tbb_disable		= NULL;

static GHashTable	*iters_ht			= NULL;

/****************************************************
 * Forward Declarations                             *
 ****************************************************/
 
static void create_widgets();
static GtkWidget * create_toolbar();

static void user_selection_changed_cb(GtkTreeView *treeview, gpointer user_data);
static void update_actions();

static void reload_users_list_store();
static gboolean clear_iters_ht_cb(gpointer key, gpointer value, gpointer user_data);
static void add_user_to_store(SimiasUser *simias_user);
static void remove_user_from_store(SimiasUser *simias_user);
static void update_user_status(SimiasUser *simias_user);

static GdkPixbuf * get_status_icon(GaimAccount *account, SIMIAS_USER_STATE state);
static GdkPixbuf * overlay_status_emblem(GdkPixbuf *orig, SIMIAS_USER_STATE state);
static char * get_user_state_string(SIMIAS_USER_STATE state);
static void users_window_closed(GtkWidget *w, GdkEvent *ev, gpointer d);

static void invite_cb(GtkWidget *w, GdkEvent *ev, gpointer d);
static void cancel_invite_cb(GtkWidget *w, GdkEvent *ev, gpointer d);
static void share_ifolders_cb(GtkWidget *w, GdkEvent *ev, gpointer d);
static void disable_cb(GtkWidget *w, GdkEvent *ev, gpointer d);

static void simias_users_event_listener(SIMIAS_USERS_EVENT_TYPE type, SimiasUser *simias_user);

/****************************************************
 * Function Implementations                         *
 ****************************************************/

GtkWidget *
simias_gtk_show_users(GtkWidget *parent)
{
	if (users_window == NULL)
	{
		/* This is the first time we've shown the window */
		users_window = gtk_window_new(GTK_WINDOW_TOPLEVEL);
		if (parent)
			gtk_window_set_transient_for(GTK_WINDOW(users_window), GTK_WINDOW(parent));
		gtk_window_set_title(GTK_WINDOW(users_window), _("iFolder Filesharing Users"));
		gtk_window_set_default_size(GTK_WINDOW(users_window), 450, 250);
		gtk_container_border_width(GTK_CONTAINER(users_window), 10);
		
		gtk_window_set_resizable(GTK_WINDOW(users_window), TRUE);
		gtk_window_set_position(GTK_WINDOW(users_window), GTK_WIN_POS_CENTER);
		gtk_window_set_destroy_with_parent(GTK_WINDOW(users_window), TRUE);

		iters_ht = g_hash_table_new(g_str_hash, g_direct_equal);
		
		create_widgets();

		/* Register to listen for events made to the real user hashtable */
		simias_users_register_event_listener(simias_users_event_listener);

		g_signal_connect(G_OBJECT(users_window), "delete_event",
						 G_CALLBACK(users_window_closed), users_window);
	}
	
	gtk_widget_grab_focus(users_window);
	gtk_widget_show_all(users_window);
	update_actions();
	gtk_window_present(GTK_WINDOW(users_window));
	
	return users_window;
}


static void
create_widgets()
{
	GtkCellRenderer *cell_renderer;
	GtkWidget *scrolled_win;
	GtkTreeSelection *tree_sel;
	GtkWidget *hbox;
	GtkWidget *close_button;

	main_vbox = gtk_vbox_new(FALSE, 0);

	gtk_container_add(GTK_CONTAINER(users_window), main_vbox);

	toolbar = create_toolbar();
	gtk_box_pack_start(GTK_BOX(main_vbox), GTK_WIDGET(toolbar), FALSE, FALSE, 0);

	scrolled_win = gtk_scrolled_window_new(NULL, NULL);
	gtk_scrolled_window_set_policy(GTK_SCROLLED_WINDOW(scrolled_win),
								   GTK_POLICY_AUTOMATIC,
								   GTK_POLICY_AUTOMATIC);
	gtk_scrolled_window_set_shadow_type(GTK_SCROLLED_WINDOW(scrolled_win),
										GTK_SHADOW_IN);
	gtk_box_pack_start(GTK_BOX(main_vbox),
					   scrolled_win, TRUE, TRUE, 0);

	users_list_store =
		gtk_list_store_new(N_COLS,
						   GDK_TYPE_PIXBUF, /* User Status Icon */
						   G_TYPE_STRING,	/* Display Name */
						   G_TYPE_STRING,	/* screenname */
						   G_TYPE_STRING,	/* machineName */
						   G_TYPE_STRING,	/* accountName */
						   G_TYPE_STRING,	/* accountProto */
						   G_TYPE_INT,		/* SIMIAS_USER_STATE */
						   G_TYPE_STRING);	/* String for SIMIAS_USER_STATE */

	gtk_tree_sortable_set_sort_column_id(GTK_TREE_SORTABLE(users_list_store),
										 1, GTK_SORT_ASCENDING);
	
	/* Load in the users tree store */
	reload_users_list_store();

	users_tree_view = gtk_tree_view_new_with_model(GTK_TREE_MODEL(users_list_store));
	g_object_unref(users_list_store);	/* Remove our own reference */
	
	g_signal_connect(G_OBJECT(users_tree_view), "cursor-changed",
					 G_CALLBACK(user_selection_changed_cb), NULL);

	/* Single selection mode only */
	tree_sel = gtk_tree_view_get_selection(GTK_TREE_VIEW(users_tree_view));
	gtk_tree_selection_set_mode(tree_sel, GTK_SELECTION_SINGLE);

	/* STATUS_ICON_COL */
	cell_renderer = gtk_cell_renderer_pixbuf_new();
	gtk_tree_view_insert_column_with_attributes(
		GTK_TREE_VIEW(users_tree_view),
		-1, _(""), cell_renderer, "pixbuf", STATUS_ICON_COL, NULL);

	/* DISPLAY_NAME_COL */
	cell_renderer = gtk_cell_renderer_text_new();
	gtk_tree_view_insert_column_with_attributes(
		GTK_TREE_VIEW(users_tree_view),
		-1, _("User"), cell_renderer, "text", DISPLAY_NAME_COL, NULL);

	/* STATUS_STRING_COL */
	gtk_tree_view_insert_column_with_attributes(
		GTK_TREE_VIEW(users_tree_view),
		-1, _("Status"), cell_renderer, "text", STATUS_STRING_COL, NULL);

	gtk_container_add(GTK_CONTAINER(scrolled_win), users_tree_view);
	
	
	/* Buttons */
	hbox = gtk_hbox_new(FALSE, 0);

	close_button = gtk_button_new_from_stock(GTK_STOCK_CLOSE);
	gtk_box_pack_end(GTK_BOX(hbox), close_button, FALSE, FALSE, 0);

	g_signal_connect(G_OBJECT(close_button), "clicked",
					 G_CALLBACK(users_window_closed), NULL);
	
	gtk_box_pack_end(GTK_BOX(main_vbox), hbox, FALSE, FALSE, 10);
}

static GtkWidget *
create_toolbar()
{
	GtkWidget *tb;
	
	tb = gtk_toolbar_new();
	gtk_toolbar_set_orientation(GTK_TOOLBAR(tb), GTK_ORIENTATION_HORIZONTAL);
	gtk_toolbar_set_style(GTK_TOOLBAR(tb), GTK_TOOLBAR_BOTH);
	
/*	gtk_toolbar_set_tooltips(GTK_TOOLBAR(tb), TRUE);*/
	
	tbb_invite = gtk_tool_button_new(NULL, _("_Invite"));
	gtk_tool_button_set_use_underline(GTK_TOOL_BUTTON(tbb_invite), TRUE);

	tbb_cancel_invite = gtk_tool_button_new(NULL, _("_Cancel Invitation"));
	gtk_tool_button_set_use_underline(GTK_TOOL_BUTTON(tbb_cancel_invite), TRUE);

	tbb_share_ifolders = gtk_tool_button_new(NULL, _("_Share iFolders"));
	gtk_tool_button_set_use_underline(GTK_TOOL_BUTTON(tbb_share_ifolders), TRUE);

	tbb_disable = gtk_tool_button_new(NULL, _("_Disable"));
	gtk_tool_button_set_use_underline(GTK_TOOL_BUTTON(tbb_disable), TRUE);

	/* Hook up the button handlers */
	g_signal_connect(G_OBJECT(tbb_invite), "clicked",
					 G_CALLBACK(invite_cb), NULL);
	g_signal_connect(G_OBJECT(tbb_cancel_invite), "clicked",
					 G_CALLBACK(cancel_invite_cb), NULL);
	g_signal_connect(G_OBJECT(tbb_share_ifolders), "clicked",
					 G_CALLBACK(share_ifolders_cb), NULL);
	g_signal_connect(G_OBJECT(tbb_disable), "clicked",
					 G_CALLBACK(disable_cb), NULL);

	gtk_toolbar_insert(GTK_TOOLBAR(tb), tbb_invite, 0);
	gtk_toolbar_insert(GTK_TOOLBAR(tb), tbb_cancel_invite, 1);
	gtk_toolbar_insert(GTK_TOOLBAR(tb), tbb_share_ifolders, 2);
	gtk_toolbar_insert(GTK_TOOLBAR(tb), tbb_disable, 3);
	
	return tb;
}

static void
user_selection_changed_cb(GtkTreeView *treeview, gpointer user_data)
{
	g_print("user_selection_changed_cb() entered\n");
	update_actions();
}

static void
update_actions()
{
	GtkTreeSelection *selection;
	GtkTreeModel *model;
	GtkTreeIter iter;
	GValue gVal = { 0 };
	SIMIAS_USER_STATE status;
	
	gchar *screenName;
	gchar *accountName;
	gchar *accountProto;
	
	GaimAccount *account;
	GaimBuddy *buddy;

	gtk_widget_set_sensitive(GTK_WIDGET(tbb_invite), FALSE);
	gtk_widget_set_sensitive(GTK_WIDGET(tbb_cancel_invite), FALSE);
	gtk_widget_set_sensitive(GTK_WIDGET(tbb_share_ifolders), FALSE);
	gtk_widget_set_sensitive(GTK_WIDGET(tbb_disable), FALSE);

	gtk_widget_show(GTK_WIDGET(tbb_invite));
	gtk_widget_hide(GTK_WIDGET(tbb_cancel_invite));

	selection = gtk_tree_view_get_selection(GTK_TREE_VIEW(users_tree_view));
	if (!selection)
		return;

	if (gtk_tree_selection_get_selected(selection, &model, &iter))
	{
		gtk_tree_model_get_value(model, &iter, SCREENNAME_COL, &gVal);
		screenName = g_value_dup_string(&gVal);
		g_value_unset(&gVal);

		gtk_tree_model_get_value(model, &iter, ACCOUNT_NAME_COL, &gVal);
		accountName = g_value_dup_string(&gVal);
		g_value_unset(&gVal);

		gtk_tree_model_get_value(model, &iter, ACCOUNT_PROTO_COL, &gVal);
		accountProto = g_value_dup_string(&gVal);
		g_value_unset(&gVal);

		account = gaim_accounts_find(accountName, accountProto);
		buddy = gaim_find_buddy(account, screenName);
		
		if (buddy->present == 0)
		{
			/* The buddy is offline */
			g_print("%s is not online\n", buddy->alias ? buddy->alias : screenName);
			g_free(screenName);
			g_free(accountName);
			g_free(accountProto);
			return;
		}

		g_free(screenName);
		g_free(accountName);
		g_free(accountProto);

		gtk_tree_model_get_value(model, &iter, STATUS_COL, &gVal);
		status = g_value_get_int(&gVal);
		g_value_unset(&gVal);
		g_print("User status: %s\n", get_user_state_string(status));

		switch (status)
		{
			case USER_INVITED:
				gtk_widget_set_sensitive(GTK_WIDGET(tbb_cancel_invite), TRUE);
				gtk_widget_hide(GTK_WIDGET(tbb_invite));
				gtk_widget_show(GTK_WIDGET(tbb_cancel_invite));
				break;
			case USER_ENABLED:
				gtk_widget_set_sensitive(GTK_WIDGET(tbb_share_ifolders), TRUE);
				gtk_widget_set_sensitive(GTK_WIDGET(tbb_disable), TRUE);
				break;
			case USER_REJECTED:
				gtk_widget_set_sensitive(GTK_WIDGET(tbb_invite), TRUE);
				break;
			case USER_AVAILABLE:
			default:
				gtk_widget_set_sensitive(GTK_WIDGET(tbb_invite), TRUE);
				break;
		}
	}
	else
	{
		g_print("Nothing's selected\n");
	}
}

static void
reload_users_list_store()
{
	GSList *simias_users;
	SimiasUser *simias_user;
	
	printf("reload_users_list_store() entered\n");
	
	/*
	 *		1. Clear the old list
	 *		2. Get the user list from simias-users.c
	 *		3. Add the users in.
	 */

	g_hash_table_foreach_remove(iters_ht, clear_iters_ht_cb, NULL);

	gtk_list_store_clear(users_list_store);
	
	simias_users = simias_users_get_all();

	while (simias_users)
	{
		simias_user = (SimiasUser *) simias_users->data;
		add_user_to_store(simias_user);
		simias_users = g_slist_next(simias_users);
	}
}

static gboolean
clear_iters_ht_cb(gpointer key, gpointer value, gpointer user_data)
{
	/* Cleanup the memory used by the value (a GtkTreeIter) */
	free((GtkTreeIter *) value);

	return TRUE;
}

static void
add_user_to_store(SimiasUser *simias_user)
{
	GtkTreeIter *iter;
	GdkPixbuf *user_icon;
	GaimAccount *gaim_account;
	char displayName[2048];

	printf("add_user_to_store() entered\n");

	gaim_account = gaim_accounts_find(simias_user->accountName,
									  simias_user->accountProto);
	if (!gaim_account)
	{
		printf("gaim_accounts_find() couldn't find account for %s/%s\n",
			   simias_user->accountName, simias_user->accountProto);
		return;
	}

	user_icon = get_status_icon(gaim_account, simias_user->state);

	sprintf(displayName, "%s (%s)",
			simias_user->screenName, /*FIXME: Change this to be the Buddy Alias if one exists */
			simias_user->machineName);

	iter = malloc(sizeof(GtkTreeIter));
	gtk_list_store_append(users_list_store, iter);
	gtk_list_store_set(users_list_store, iter,
		STATUS_ICON_COL, user_icon,		/* User Status Icon */
		DISPLAY_NAME_COL, displayName,
		SCREENNAME_COL, simias_user->screenName,
		MACHINE_NAME_COL, simias_user->machineName,
		ACCOUNT_NAME_COL, simias_user->accountName,
		ACCOUNT_PROTO_COL, simias_user->accountProto,
		STATUS_COL, simias_user->state,
		STATUS_STRING_COL, get_user_state_string(simias_user->state),
		-1);

	/* Store off the GtkTreeIter in iters_ht */
	g_hash_table_insert(iters_ht, simias_user->hashKey, iter);

	if (user_icon)
		g_object_unref(user_icon);
}

static void
remove_user_from_store(SimiasUser *simias_user)
{
	GtkTreeIter *iter;
	
	iter = (GtkTreeIter *) g_hash_table_lookup(iters_ht, simias_user->hashKey);
	if (iter)
	{
		gtk_list_store_remove(users_list_store, iter);
	
		g_hash_table_remove(iters_ht, simias_user->hashKey);
		free(iter);
	}
}

static void
update_user_status(SimiasUser *simias_user)
{
	GdkPixbuf *status_icon;
	GtkTreeIter *iter;
	GaimAccount *gaim_account;
	
	iter = (GtkTreeIter *) g_hash_table_lookup(iters_ht, simias_user->hashKey);
	if (!iter)
	{
		/* Somehow we don't have this user in our list so let's add it */
		add_user_to_store(simias_user);
	}
	else
	{
		gaim_account = gaim_accounts_find(simias_user->accountName,
										  simias_user->accountProto);
		if (!gaim_account)
		{
			printf("gaim_accounts_find() couldn't find account for %s/%s\n",
				   simias_user->accountName, simias_user->accountProto);
			return;
		}

		status_icon = get_status_icon(gaim_account, simias_user->state);
		
		gtk_list_store_set(users_list_store, iter,
			STATUS_ICON_COL, status_icon,
			STATUS_COL, simias_user->state,
			STATUS_STRING_COL, get_user_state_string(simias_user->state),
			-1);
		
		if (status_icon)
			g_object_unref(status_icon);
	}
}

static GdkPixbuf *
get_status_icon(GaimAccount *account, SIMIAS_USER_STATE state)
{
	GdkPixbuf *status_icon;
	GdkPixbuf *scale = NULL;
	GdkPixbuf *overlaid_icon;
	int scalesize = 15;
	status_icon = create_prpl_icon(account);
	if (status_icon)
	{
		scale = gdk_pixbuf_scale_simple(status_icon, scalesize, scalesize,
										GDK_INTERP_BILINEAR);
		g_object_unref(status_icon);
		status_icon = scale;

		overlaid_icon = overlay_status_emblem(status_icon, state);
		if (overlaid_icon)
		{
			g_object_unref(status_icon);
			status_icon = overlaid_icon;
		}
	}

	return status_icon;
}

static GdkPixbuf *
overlay_status_emblem(GdkPixbuf *orig, SIMIAS_USER_STATE state)
{
	GdkPixbuf *emblem = NULL;
	char *filename;

	printf("overlay_status_emblem() entered\n");
	
	if (orig == NULL)
	{
		printf("overlay_status_emblem() called with NULL GdkPixbuf!\n");
		return NULL;
	}

	switch (state)
	{
		case USER_INVITED:
			filename = g_build_filename(DATADIR, "pixmaps", "gaim", "status", "default", "status-invited.png", NULL);
			break;
		case USER_ENABLED:
			filename = g_build_filename(DATADIR, "pixmaps", "gaim", "status", "default", "status-enabled.png", NULL);
			break;
		case USER_REJECTED:
			filename = g_build_filename(DATADIR, "pixmaps", "gaim", "status", "default", "status-rejected.png", NULL);
			break;
		case USER_AVAILABLE:
		default:
			filename = g_build_filename(DATADIR, "pixmaps", "gaim", "status", "default", "status-canceled.png", NULL);
			break;
	}

	emblem = gdk_pixbuf_new_from_file(filename, NULL);
	g_free(filename);

	if (!emblem)
		return NULL;

	gdk_pixbuf_composite(
		emblem, /* source */
		orig,	/* destination */
		5,		/* dest_x */
		5,		/* dest_y */
		10,		/* dest_width */
		10,		/* dest_height */
		5,		/* offset_x */
		5,		/* offset_y */
		.6,		/* scale_x */
		.6,		/* scale_y */
		GDK_INTERP_BILINEAR,
		255);	/* overall_alpha */
	g_object_unref(emblem);

	return orig;
}

static char *
get_user_state_string(SIMIAS_USER_STATE state)
{
	switch (state)
	{
		case USER_INVITED:
			return _("Invited");
			break;
		case USER_ENABLED:
			return _("Enabled");
			break;
		case USER_REJECTED:
			return _("Rejected");
			break;
		case USER_AVAILABLE:
		default:
			return _("Available");
			break;
	}
}

static void
users_window_closed(GtkWidget *w, GdkEvent *ev, gpointer d)
{
	printf("users_window_closed() called\n");
	gtk_widget_hide(users_window);
}

static void
invite_cb(GtkWidget *w, GdkEvent *ev, gpointer d)
{
	GtkTreeSelection *selection;
	GtkTreeModel *model;
	GtkTreeIter iter;
	GValue gVal = { 0 };
	
	gchar *screenName;
	gchar *machineName;
	gchar *accountName;
	gchar *accountProto;

	GaimAccount *account;
	GaimBuddy *buddy;
	
	int err;
	
	GtkWidget *dialog;
	
	g_print("invite_cb() called\n");

	selection = gtk_tree_view_get_selection(GTK_TREE_VIEW(users_tree_view));
	if (!selection)
	{
		g_print("A user is not selected\n");
		return;
	}

	if (gtk_tree_selection_get_selected(selection, &model, &iter))
	{
		gtk_tree_model_get_value(model, &iter, SCREENNAME_COL, &gVal);
		screenName = g_value_dup_string(&gVal);
		g_value_unset(&gVal);

		gtk_tree_model_get_value(model, &iter, MACHINE_NAME_COL, &gVal);
		machineName = g_value_dup_string(&gVal);
		g_value_unset(&gVal);

		gtk_tree_model_get_value(model, &iter, ACCOUNT_NAME_COL, &gVal);
		accountName = g_value_dup_string(&gVal);
		g_value_unset(&gVal);

		gtk_tree_model_get_value(model, &iter, ACCOUNT_PROTO_COL, &gVal);
		accountProto = g_value_dup_string(&gVal);
		g_value_unset(&gVal);

		account = gaim_accounts_find(accountName, accountProto);
		buddy = gaim_find_buddy(account, screenName);

		err = simias_send_invitation_request(buddy);
		if (err <= 0)
		{
			dialog =
				gtk_message_dialog_new(GTK_WINDOW(users_window),
										GTK_DIALOG_DESTROY_WITH_PARENT,
										GTK_MESSAGE_ERROR,
										GTK_BUTTONS_OK,
										_("There was an error enabling iFolder Filesharing with %s.  Perhaps %s is not online or you do not have iFolder/Simias running?"),
										buddy->alias ? buddy->alias : buddy->name,
										buddy->alias ? buddy->alias : buddy->name);
			gtk_dialog_run(GTK_DIALOG(dialog));
			gtk_widget_destroy(dialog);
		}
		else
		{
			/* FIXME: If a conversation window is open with this buddy, add a little string saying that we just sent an invitation */
			fprintf(stderr, "invitation message sent to %s\n", buddy->name);
	
			simias_users_update_status(
				screenName,
				machineName,
				accountName,
				accountProto,
				USER_INVITED);

			update_actions();				
		}

		g_free(screenName);
		g_free(machineName);
		g_free(accountName);
		g_free(accountProto);
	}
	else
	{
		g_print("Nothing's selected\n");
	}
}

static void
cancel_invite_cb(GtkWidget *w, GdkEvent *ev, gpointer d)
{
	GtkTreeSelection *selection;
	GtkTreeModel *model;
	GtkTreeIter iter;
	GValue gVal = { 0 };
	
	gchar *screenName;
	gchar *machineName;
	gchar *accountName;
	gchar *accountProto;

	GaimAccount *account;
	GaimBuddy *buddy;
	
	int err;
	
	GtkWidget *dialog;
	
	g_print("cancel_invite_cb() called\n");

	selection = gtk_tree_view_get_selection(GTK_TREE_VIEW(users_tree_view));
	if (!selection)
	{
		g_print("A user is not selected\n");
		return;
	}

	if (gtk_tree_selection_get_selected(selection, &model, &iter))
	{
		gtk_tree_model_get_value(model, &iter, SCREENNAME_COL, &gVal);
		screenName = g_value_dup_string(&gVal);
		g_value_unset(&gVal);

		gtk_tree_model_get_value(model, &iter, MACHINE_NAME_COL, &gVal);
		machineName = g_value_dup_string(&gVal);
		g_value_unset(&gVal);

		gtk_tree_model_get_value(model, &iter, ACCOUNT_NAME_COL, &gVal);
		accountName = g_value_dup_string(&gVal);
		g_value_unset(&gVal);

		gtk_tree_model_get_value(model, &iter, ACCOUNT_PROTO_COL, &gVal);
		accountProto = g_value_dup_string(&gVal);
		g_value_unset(&gVal);

		account = gaim_accounts_find(accountName, accountProto);
		buddy = gaim_find_buddy(account, screenName);

		err = simias_send_invitation_cancel(buddy);
		if (err <= 0)
		{
			dialog =
				gtk_message_dialog_new(GTK_WINDOW(users_window),
										GTK_DIALOG_DESTROY_WITH_PARENT,
										GTK_MESSAGE_ERROR,
										GTK_BUTTONS_OK,
										_("There was an error canceling the invitation to %s.  Perhaps %s is not online or you do not have iFolder/Simias running?"),
										buddy->alias ? buddy->alias : buddy->name,
										buddy->alias ? buddy->alias : buddy->name);
			gtk_dialog_run(GTK_DIALOG(dialog));
			gtk_widget_destroy(dialog);
		}
		else
		{
			/* FIXME: If a conversation window is open with this buddy, add a little string saying that we just sent an invitation */
			fprintf(stderr, "invitation message sent to %s\n", buddy->name);
	
			simias_users_update_status(
				screenName,
				machineName,
				accountName,
				accountProto,
				USER_AVAILABLE);

			update_actions();				
		}

		g_free(screenName);
		g_free(machineName);
		g_free(accountName);
		g_free(accountProto);
	}
	else
	{
		g_print("Nothing's selected\n");
	}
}

static void
share_ifolders_cb(GtkWidget *w, GdkEvent *ev, gpointer d)
{
	GtkWidget *dialog;

	g_print("share_ifolders_cb() called\n");

	dialog =
		gtk_message_dialog_new(GTK_WINDOW(users_window),
							GTK_DIALOG_DESTROY_WITH_PARENT,
							GTK_MESSAGE_INFO,
							GTK_BUTTONS_OK,
							_("This is not implemented yet.  When it is, you'll be able to quickly add the selected user as a member of any of your iFolders in the Gaim Domain."));
	gtk_dialog_run(GTK_DIALOG(dialog));
	gtk_widget_destroy(dialog);
}

static void
disable_cb(GtkWidget *w, GdkEvent *ev, gpointer d)
{
	GtkTreeSelection *selection;
	GtkTreeModel *model;
	GtkTreeIter iter;
	GValue gVal = { 0 };
	
	gchar *screenName;
	gchar *machineName;
	gchar *accountName;
	gchar *accountProto;

	GaimAccount *account;
	GaimBuddy *buddy;
	
	GtkWidget *dialog;
	gint response;
	
	char settingName[1024];
	
	g_print("disable_cb() called\n");

	selection = gtk_tree_view_get_selection(GTK_TREE_VIEW(users_tree_view));
	if (!selection)
	{
		g_print("A user is not selected\n");
		return;
	}

	if (gtk_tree_selection_get_selected(selection, &model, &iter))
	{
		gtk_tree_model_get_value(model, &iter, SCREENNAME_COL, &gVal);
		screenName = g_value_dup_string(&gVal);
		g_value_unset(&gVal);

		gtk_tree_model_get_value(model, &iter, MACHINE_NAME_COL, &gVal);
		machineName = g_value_dup_string(&gVal);
		g_value_unset(&gVal);

		gtk_tree_model_get_value(model, &iter, ACCOUNT_NAME_COL, &gVal);
		accountName = g_value_dup_string(&gVal);
		g_value_unset(&gVal);

		gtk_tree_model_get_value(model, &iter, ACCOUNT_PROTO_COL, &gVal);
		accountProto = g_value_dup_string(&gVal);
		g_value_unset(&gVal);

		account = gaim_accounts_find(accountName, accountProto);
		buddy = gaim_find_buddy(account, screenName);

		dialog =
			gtk_message_dialog_new(GTK_WINDOW(users_window),
								GTK_DIALOG_DESTROY_WITH_PARENT,
								GTK_MESSAGE_QUESTION,
								GTK_BUTTONS_YES_NO,
								_("Disabling iFolder Sharing will prevent iFolder from synchronizing any iFolders that you may have shared with %s (%s).  You will have to re-enable iFolder Sharing before iFolder will synchronize with %s (%s)."),
								buddy->alias ? buddy->alias : buddy->name,
								machineName,
								buddy->alias ? buddy->alias : buddy->name,
								machineName);
		response = gtk_dialog_run(GTK_DIALOG(dialog));

		gtk_widget_destroy(dialog);

		if (response == GTK_RESPONSE_YES)
		{
			/* Remove all the simias settings for this user's current machine name */

			/* simias-des-key */
			sprintf(settingName, "simias-des-key:%s", machineName);
			if (gaim_blist_node_get_string(&(buddy->node), settingName))
				gaim_blist_node_remove_setting(&(buddy->node), settingName);

			/* simias-user-id */
			sprintf(settingName, "simias-user-id:%s", machineName);
			if (gaim_blist_node_get_string(&(buddy->node), settingName))
				gaim_blist_node_remove_setting(&(buddy->node), settingName);

			/* simias-url */
			sprintf(settingName, "simias-url:%s", machineName);
			if (gaim_blist_node_get_string(&(buddy->node), settingName))
				gaim_blist_node_remove_setting(&(buddy->node), settingName);

			/* simias-public-key */
			sprintf(settingName, "simias-public-key:%s", machineName);
			if (gaim_blist_node_get_string(&(buddy->node), settingName))
				gaim_blist_node_remove_setting(&(buddy->node), settingName);

			/* Remove the machine name from the list */
			simias_remove_buddy_machine_name(&(buddy->node), machineName);
			
			simias_users_update_status(
				screenName,
				machineName,
				accountName,
				accountProto,
				USER_AVAILABLE);

			update_actions();				
		}

		g_free(screenName);
		g_free(machineName);
		g_free(accountName);
		g_free(accountProto);
	}
	else
	{
		g_print("Nothing's selected\n");
	}
}

static void
simias_users_event_listener(SIMIAS_USERS_EVENT_TYPE type, SimiasUser *simias_user)
{
	switch(type)
	{
		case SIMIAS_USERS_EVENT_USER_ADDED:
			printf("Received 'user-added' event\n");
			add_user_to_store(simias_user);
			break;
		case SIMIAS_USERS_EVENT_USER_REMOVED:
			printf("Received 'user-removed' event\n");
			remove_user_from_store(simias_user);
			break;
		case SIMIAS_USERS_EVENT_USER_STATUS_CHANGED:
			printf("Received 'user-status-changed' event\n");
			update_user_status(simias_user);
			break;
		default:
			break;
	}
}

