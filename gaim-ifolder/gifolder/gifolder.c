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

#include "blist.h"
#include "gtkblist.h"
#include "account.h"
#include <string.h>
#include <stdio.h>
#include <stdlib.h>
#include "conversation.h"
#include "connection.h"
#include "network.h"
#include <time.h>
#include "gtkprefs.h"
#include "gtkutils.h"
#include "notify.h"
#include "xmlnode.h"
#include "prefix.h"

#include "debug.h"
#include "prefs.h"
#include "signals.h"
#include "util.h"
#include "version.h"

#include "gtkplugin.h"

/* Gaim iFolder Plugin Includes */
#include "gifolder.h"
#include "gaim-domain.h"
#include "event-handler.h"
#include "simias-messages.h"
#include "simias-prefs.h"
#include "simias-invitation-store.h"
#include "simias-util.h"


/****************************************************
 * Static Definitions (#defines)                    *
 ****************************************************/
#define IFOLDER_PLUGIN_ID "ifolder"

/****************************************************
 * Global Variables                                 *
 ****************************************************/
static GtkWidget *invitations_dialog;

static GtkWidget *in_inv_tree = NULL;
GtkListStore *in_inv_store = NULL;
static GtkWidget *in_inv_accept_button = NULL;
static GtkWidget *in_inv_reject_button = NULL;

GtkWidget *out_inv_tree = NULL;
GtkListStore *out_inv_store = NULL;
static GtkWidget *out_inv_resend_button = NULL;
static GtkWidget *out_inv_cancel_button = NULL;
static GtkWidget *out_inv_remove_button = NULL;

GtkListStore *trusted_buddies_store = NULL;

SimiasEventClient ec;

/****************************************************
 * Forward Declarations                             *
 ****************************************************/
 
/* Start: UI Functions */
static void buddylist_cb_simulate_share_collection(GaimBlistNode *node,
												gpointer user_data);

static void invitations_dialog_close_button_cb(GtkWidget *widget,
					       int response,
					       gpointer user_data);
static void invitations_dialog_destroy_cb(GtkWidget *widget, GdkEvent *event,
										  gpointer user_data);
static void in_inv_accept_button_cb(GtkWidget *w, GtkTreeView *tree);
static void in_inv_reject_button_cb(GtkWindow *w, GtkTreeView *tree);

static void out_inv_resend_button_cb(GtkWindow *w, GtkTreeView *tree);
static void out_inv_cancel_button_cb(GtkWindow *w, GtkTreeView *tree);
static void out_inv_remove_button_cb(GtkWindow *w, GtkTreeView *tree);

static void init_invitations_window();

static void buddylist_cb_show_invitations_window(GaimBlistNode *node,
												 gpointer user_data);
static void blist_add_context_menu_items_cb(GaimBlistNode *node, GList **menu);
/* End: UI Functions */

/****************************************************
 * Function Implementations                         *
 ****************************************************/

/**
 * FIXME: Remove this function once we've tied in with Simias since invitation
 * requests will be started when an event is received when a member is added to
 * a Simias Gaim collection.  This menu could be easily replaced with one that
 * would open a special dialog that would list all the Collections in the Gaim
 * Domain and ask the user whether they wanted to add a buddy as a member to
 * one or more of these Collections.  Or, it could offer the ability to create
 * a new Collection on the fly right there and add this buddy as a member.  This
 * would be "Icing on the Cake".
 * 
 * This function should prompt the user for a Collection Name and spoof a
 * Simias ID and Collection Type before sending an invitation to the selected
 * buddy.
 */
static void
buddylist_cb_simulate_share_collection(GaimBlistNode *node, gpointer user_data)
{
	/* Prompt the user for an iFolder/Collection Name */
	GtkWidget *dialog;
	GtkWidget *vbox;
	GtkWidget *name_label;
	GtkWidget *name_entry;
	GtkWidget *type_label;
	GtkWidget *type_menu;
	gint response;
	const gchar *name;
	GaimBuddy *buddy;
	int result;
	char collection_type[32];
	Invitation *invitation;
	char guid[128];
	
	GtkListStore *type_store;
	GtkTreeIter iter;
	GtkCellRenderer *renderer;

	GdkPixbuf *icon, *scale;
	int scalesize = 15;
	char *icon_path;

g_print("buddylist_cb_simulate_share_collection() entered\n");
	if (!GAIM_BLIST_NODE_IS_BUDDY(node)) {
		return;
	}

	buddy = (GaimBuddy *)node;

	dialog = gtk_dialog_new_with_buttons("Simulate Adding Member to a Collection",
			NULL,
			GTK_DIALOG_DESTROY_WITH_PARENT,
			GTK_STOCK_OK,
			GTK_RESPONSE_ACCEPT,
			GTK_STOCK_CANCEL,
			GTK_RESPONSE_CANCEL,
			NULL);

	gtk_dialog_set_has_separator(GTK_DIALOG(dialog), FALSE);
	gtk_dialog_set_default_response(GTK_DIALOG(dialog), GTK_RESPONSE_ACCEPT);

	vbox = gtk_vbox_new(FALSE, 10);
	gtk_container_border_width(GTK_CONTAINER(vbox), 10);
	gtk_container_add(GTK_CONTAINER(GTK_DIALOG(dialog)->vbox), vbox);

	/* Collection Name */
	name_label = gtk_label_new(NULL);
	gtk_label_set_markup_with_mnemonic(GTK_LABEL(name_label), _("<b>_Name</b>"));
	gtk_misc_set_alignment(GTK_MISC(name_label), 0, 0.5);
	gtk_box_pack_start(GTK_BOX(vbox), name_label, FALSE, FALSE, 0);

	name_entry = gtk_entry_new();
	gtk_label_set_mnemonic_widget(GTK_LABEL(name_label), name_entry);
	gtk_entry_set_text(GTK_ENTRY(name_entry), "My iFolder");
	gtk_entry_set_activates_default(GTK_ENTRY(name_entry), TRUE);
	gtk_editable_select_region(GTK_EDITABLE(name_entry), 0, -1);
	gtk_widget_grab_focus(name_entry);
	gtk_box_pack_start(GTK_BOX(vbox), name_entry, FALSE, FALSE, 0);
	
	/* Collection Type */
	type_label = gtk_label_new(NULL);
	gtk_label_set_markup_with_mnemonic(GTK_LABEL(type_label), _("<b>_Type</b>"));
	gtk_misc_set_alignment(GTK_MISC(type_label), 0, 0.5);
	gtk_box_pack_start(GTK_BOX(vbox), type_label, FALSE, FALSE, 0);
	
	type_store = gtk_list_store_new(2, GDK_TYPE_PIXBUF, G_TYPE_STRING);

	/* FIXME: Install the icon and load it from the correct place */
	icon_path = g_build_filename(DATADIR, "..", "share", "pixmaps", "gaim", "icons", "ifolder48.png", NULL);
	gaim_debug(GAIM_DEBUG_INFO, "simias", "icon path: %s\n", icon_path);
	icon = gdk_pixbuf_new_from_file(icon_path, NULL);
	g_free(icon_path);
	if (icon) {
		scale = gdk_pixbuf_scale_simple(icon, scalesize, scalesize,
										GDK_INTERP_BILINEAR);
		g_object_unref(icon);
		icon = scale;
	} else {
		gaim_debug(GAIM_DEBUG_ERROR, "simias", "Failed to load the ifolder48.png icon!\n");
	}
	gtk_list_store_append(type_store, &iter);
	gtk_list_store_set(type_store, &iter,
					   0, icon,
					   1, _("iFolder"),
					   -1);
	if (icon) {
		g_object_unref(G_OBJECT(icon));
	}
	
	/* FIXME: Install the icon and load it from the correct place */
	icon_path = g_build_filename(DATADIR, "..", "share", "pixmaps", "gaim", "icons", "glyphmarks48.png", NULL);
	gaim_debug(GAIM_DEBUG_INFO, "simias", "icon path: %s\n", icon_path);
	icon = gdk_pixbuf_new_from_file(icon_path, NULL);
	g_free(icon_path);
	if (icon) {
		scale = gdk_pixbuf_scale_simple(icon, scalesize, scalesize,
										GDK_INTERP_BILINEAR);
		g_object_unref(icon);
		icon = scale;
	} else {
		gaim_debug(GAIM_DEBUG_ERROR, "simias", "Failed to load the glyphmarks48.png icon!\n");
	}
	gtk_list_store_append(type_store, &iter);
	gtk_list_store_set(type_store, &iter,
					   0, icon,
					   1, _("Glyphmarks"),
					   -1);
	if (icon) {
		g_object_unref(G_OBJECT(icon));
	}
	
	type_menu = gtk_combo_box_new_with_model(GTK_TREE_MODEL(type_store));
	gtk_label_set_mnemonic_widget(GTK_LABEL(type_label), type_menu);
	
	renderer = gtk_cell_renderer_pixbuf_new();
	gtk_cell_layout_pack_start(GTK_CELL_LAYOUT(type_menu), renderer, FALSE);
	gtk_cell_layout_set_attributes(GTK_CELL_LAYOUT(type_menu), renderer,
								   "pixbuf", 0, NULL);

	renderer = gtk_cell_renderer_text_new();
	gtk_cell_layout_pack_start(GTK_CELL_LAYOUT(type_menu), renderer, TRUE);
	gtk_cell_layout_set_attributes(GTK_CELL_LAYOUT(type_menu), renderer,
								   "text", 1, NULL);

	/* Select iFolder as the default */
	gtk_combo_box_set_active(GTK_COMBO_BOX(type_menu), 0);
	
	gtk_box_pack_start(GTK_BOX(vbox), type_menu, FALSE, FALSE, 0);
	
	gtk_widget_show_all(vbox);

	response = gtk_dialog_run(GTK_DIALOG(dialog));

	if (response == GTK_RESPONSE_ACCEPT) {
		/**
		 * Check the value of name_entry and if it's not NULL, send an
		 * invitation.
		 */
		name = gtk_entry_get_text(GTK_ENTRY(name_entry));
		
		/* Get the Collection Type */
		switch(gtk_combo_box_get_active(GTK_COMBO_BOX(type_menu))) {
			case 1:
				/* Default to use an iFolder */
				sprintf(collection_type, COLLECTION_TYPE_GLYPHMARKS);
				break;
			case 0:
			default:
				/* Default to use an iFolder */
				sprintf(collection_type, COLLECTION_TYPE_IFOLDER);
		}
		
		if (name && strlen(name) > 0) {

			/* FIXME: Fix this spoofing of a Simias ID to a real Simias ID */
			srand((unsigned) time(NULL)/2);
			sprintf(guid, "{%d-%d}", rand(), rand());
			
			/* Create and fill out the Invitation */
			invitation = malloc(sizeof(Invitation));
			invitation->gaim_account = buddy->account;
			sprintf(invitation->buddy_name, buddy->name);
			invitation->state = STATE_PENDING;
			time(&(invitation->time));
			sprintf(invitation->collection_id, guid);
			sprintf(invitation->collection_type, collection_type);
			sprintf(invitation->collection_name, name);
			sprintf(invitation->ip_addr, "0.0.0.0");
			sprintf(invitation->ip_port, "0");

			result = simias_send_invitation_req(buddy, guid, collection_type,
												(char *)name);
			g_print("simias_send_invitation_req(): %d\n", result);
			if (result > 0) {
				/* The message was sent */
				invitation->state = STATE_SENT;
			}
			
			simias_add_invitation_to_store(out_inv_store, invitation);
		}
	}

	gtk_widget_destroy(dialog);
}

static void
invitations_dialog_close_button_cb(GtkWidget *widget, int response,
				   gpointer user_data)
{
g_print("invitations_dialog_close_button_cb() called\n");
	if (response == GTK_RESPONSE_CLOSE) {
		/* Hide the Invitations Dialog */
		gtk_widget_hide_all(invitations_dialog);
	}
}

static void
invitations_dialog_destroy_cb(GtkWidget *widget, GdkEvent *event,
										  gpointer user_data)
{
g_print("invitations_dialog_destroy_cb() called\n");
	gtk_widget_hide_all(invitations_dialog);
}


static void
in_inv_accept_button_cb(GtkWidget *w, GtkTreeView *tree)
{
	GtkTreeSelection *sel;
	GtkTreeIter iter;
	GtkTreeModel *model;
	Invitation *invitation;
	GaimBuddy *buddy;
	int send_result;
	GtkWidget *dialog;
	GtkTreeIter tb_iter;
	char time_str[32];
	char state_str[32];

	sel = gtk_tree_view_get_selection(tree);
	if (!gtk_tree_selection_get_selected(sel, &model, &iter)) {
		/**
		 * This shouldn't happen since the button should be disabled when no
		 * items in the list are selected.
		 */
		g_print("in_inv_accept_button_cb() called without an active selection\n");
		return;
	}
	
	/* Extract the Invitation * from the model using iter */
	gtk_tree_model_get(model, &iter,
						INVITATION_PTR, &invitation,
						-1);

	/**
	 * Attempt to send the reply message.  If we get a failure, it could be
	 * because the buddy is not online.  If we want to show presence in the
	 * Invitations Dialog, then we must require that Invitations can only exist
	 * for buddies that are in your buddy list.
	 * 
	 * FIXME: Make a decision about requiring a buddy to be in your buddy list to send/receive simias messages
	 * This decision might have just been made because the send_invitation_msg
	 * stuff requires you to pass in the GaimBuddy * as the first argument.  I
	 * don't think gaim_find_buddy() returns a GaimBuddy if the buddy is not in
	 * your buddy list.  This needs investigation.
	 */
	buddy = gaim_find_buddy(invitation->gaim_account, invitation->buddy_name);
	if (!buddy) {
		dialog = gtk_message_dialog_new(NULL,
					GTK_DIALOG_DESTROY_WITH_PARENT,
					GTK_MESSAGE_ERROR,
					GTK_BUTTONS_CLOSE,
					_("This buddy is not in your buddy list.  If you want to accept this invitation, please add this buddy to your buddy list."));
		gtk_dialog_run(GTK_DIALOG(dialog));
		gtk_widget_destroy(dialog);
		return;
	}
	
	/**
	 * Check to see if this buddy is already trusted.  If not, add a new entry.
	 * If so, update the IP Address and IP Port.
	 */
	if (simias_lookup_trusted_buddy(trusted_buddies_store, buddy, &tb_iter)) {
		/* Update the trusted buddy info */
		gtk_list_store_set(trusted_buddies_store, &tb_iter,
							TRUSTED_BUDDY_IP_ADDR_COL, invitation->ip_addr,
							TRUSTED_BUDDY_IP_PORT_COL, invitation->ip_port,
							-1);

		/* Update the trusted buddies file */
		simias_save_trusted_buddies(trusted_buddies_store);
	} else {
		/* Add a new trusted buddy */
		simias_add_new_trusted_buddy(trusted_buddies_store, buddy, 
								invitation->ip_addr, invitation->ip_port);
	}

	if (buddy->present == GAIM_BUDDY_SIGNING_OFF
		|| buddy->present == GAIM_BUDDY_OFFLINE) {
		/**
		 * Change the state of this invitation to STATE_ACCEPTED_PENDING and it
		 * will be sent when the buddy-signed-on event occurs.
		 */
		invitation->state = STATE_ACCEPTED_PENDING;

		/* Update the "last updated" time */
		time(&(invitation->time));

		/* Format the time to a string */
		simias_fill_time_str(time_str, 32, invitation->time);

		/* Format the state string */
		simias_fill_state_str(state_str, invitation->state);
	
		/* Update the out_inv_store */
		gtk_list_store_set(GTK_LIST_STORE(model), &iter,
			TIME_COL,				time_str,
			STATE_COL,				state_str,
			-1);
		
		/* Save the updates to a file */
g_print("about_to_write 1\n");
		simias_save_invitations(GTK_LIST_STORE(model));

		/* Make sure the buttons are in the correct state */
		simias_in_inv_sel_changed_cb(sel, GTK_TREE_VIEW(in_inv_tree));
	
		return; /* That's about all we can do at this point */
	}

	send_result =
		simias_send_invitation_accept(buddy, invitation->collection_id);
		
	if (send_result <= 0) {
		/* FIXME: Add this message to the PENDING list of messages to be sent and retry when the buddy signs on again or when we login/startup gaim again */
		dialog = gtk_message_dialog_new(NULL,
					GTK_DIALOG_DESTROY_WITH_PARENT,
					GTK_MESSAGE_ERROR,
					GTK_BUTTONS_CLOSE,
					_("There was an error sending this message.  Perhaps the buddy is not online."));
		gtk_dialog_run(GTK_DIALOG(dialog));
		gtk_widget_destroy(dialog);
		return;
	}
	
	/**
	 * If we make it to this point, the reply message was sent and we can now
	 * remove the Invitation from our list.  Don't forget to free the memory
	 * being used by Invitation *!
	 */
	gtk_list_store_remove(GTK_LIST_STORE(model), &iter);
	free(invitation);

	/* Save the updates to a file */
g_print("about_to_write 1.1\n");
	simias_save_invitations(GTK_LIST_STORE(model));

}

/**
 * This function should get the Invitation * at the current selection and
 * send a reply message if the buddy is online.
 */
static void
in_inv_reject_button_cb(GtkWindow *w, GtkTreeView *tree)
{
	GtkTreeSelection *sel;
	GtkTreeIter iter;
	GtkTreeModel *model;
	Invitation *invitation;
	GaimBuddy *buddy;
	int send_result;
	GtkWidget *dialog;
	char time_str[32];
	char state_str[32];

	sel = gtk_tree_view_get_selection(tree);
	if (!gtk_tree_selection_get_selected(sel, &model, &iter)) {
		/**
		 * This shouldn't happen since the button should be disabled when no
		 * items in the list are selected.
		 */
		g_print("in_inv_reject_button_cb() called without an active selection\n");
		return;
	}
	
	/* Extract the Invitation * from the model using iter */
	gtk_tree_model_get(model, &iter,
						INVITATION_PTR, &invitation,
						-1);

	/**
	 * Attempt to send the reply message.  If we get a failure, it could be
	 * because the buddy is not online.  If we want to show presence in the
	 * Invitations Dialog, then we must require that Invitations can only exist
	 * for buddies that are in your buddy list.
	 * 
	 * FIXME: Make a decision about requiring a buddy to be in your buddy list to send/receive simias messages
	 * This decision might have just been made because the send_invitation_msg
	 * stuff requires you to pass in the GaimBuddy * as the first argument.  I
	 * don't think gaim_find_buddy() returns a GaimBuddy if the buddy is not in
	 * your buddy list.  This needs investigation.
	 */
	buddy = gaim_find_buddy(invitation->gaim_account, invitation->buddy_name);
	if (!buddy) {
		dialog = gtk_message_dialog_new(NULL,
					GTK_DIALOG_DESTROY_WITH_PARENT,
					GTK_MESSAGE_ERROR,
					GTK_BUTTONS_CLOSE,
					_("This buddy is not in your buddy list.  If you do not wish to accept this invitation, please remove it from your list."));
		gtk_dialog_run(GTK_DIALOG(dialog));
		gtk_widget_destroy(dialog);
		return;
	}

	if (buddy->present == GAIM_BUDDY_SIGNING_OFF
		|| buddy->present == GAIM_BUDDY_OFFLINE) {
		/**
		 * Change the state of this invitation to STATE_REJECTED_PENDING and it
		 * will be sent when the buddy-signed-on event occurs.
		 */
		invitation->state = STATE_REJECTED_PENDING;

		/* Update the "last updated" time */
		time(&(invitation->time));

		/* Format the time to a string */
		simias_fill_time_str(time_str, 32, invitation->time);

		/* Format the state string */
		simias_fill_state_str(state_str, invitation->state);
	
		/* Update the out_inv_store */
		gtk_list_store_set(GTK_LIST_STORE(model), &iter,
			TIME_COL,				time_str,
			STATE_COL,				state_str,
			-1);

		/* Save the updates to a file */
g_print("about_to_write 2\n");
		simias_save_invitations(GTK_LIST_STORE(model));

		/* Make sure the buttons are in the correct state */
		simias_in_inv_sel_changed_cb(sel, GTK_TREE_VIEW(in_inv_tree));
	
		return; /* That's about all we can do at this point */
	}

	send_result =
		simias_send_invitation_deny(buddy, invitation->collection_id);
		
	if (send_result <= 0) {
		dialog = gtk_message_dialog_new(NULL,
					GTK_DIALOG_DESTROY_WITH_PARENT,
					GTK_MESSAGE_ERROR,
					GTK_BUTTONS_CLOSE,
					_("There was an error sending this message.  Perhaps the buddy is not online."));
		gtk_dialog_run(GTK_DIALOG(dialog));
		gtk_widget_destroy(dialog);
		return;
	}
	
	/**
	 * If we make it to this point, the reply message was sent and we can now
	 * remove the Invitation from our list.  Don't forget to free the memory
	 * being used by Invitation *!
	 */
	gtk_list_store_remove(GTK_LIST_STORE(model), &iter);
	free(invitation);

	/* Save the updates to a file */
	simias_save_invitations(GTK_LIST_STORE(model));
}

void
simias_in_inv_sel_changed_cb(GtkTreeSelection *sel, GtkTreeView *tree)
{
	GtkTreeIter iter;
	GtkTreeModel *model;
	Invitation *invitation;

	/* If nothing is selected, disable the buttons. */
	if (!gtk_tree_selection_get_selected(sel, &model, &iter)) {
		g_print("simias_in_inv_sel_changed_cb() called and nothing is selected.  Disabling buttons...\n");

		/* Disable the buttons */
		gtk_widget_set_sensitive(in_inv_accept_button, FALSE);
		gtk_widget_set_sensitive(in_inv_reject_button, FALSE);
	} else {
		g_print("simias_in_inv_sel_changed_cb() called and something is selected.\n");

		/**
		 * If the state of the selected invitation is either
		 * STATE_ACCEPTED_PENDING or STATE_REJECTED_PENDING, the buttons should
		 * be disabled, otherwise, enable the buttons.
		 */

		/* Extract the Invitation * from the model using iter */
		gtk_tree_model_get(model, &iter,
							INVITATION_PTR, &invitation,
							-1);
		if (invitation->state == STATE_ACCEPTED_PENDING
			|| invitation->state == STATE_REJECTED_PENDING) {
			/* Disable the buttons */
			gtk_widget_set_sensitive(in_inv_accept_button, FALSE);
			gtk_widget_set_sensitive(in_inv_reject_button, FALSE);
		} else {
			/* Enable the buttons */
			gtk_widget_set_sensitive(in_inv_accept_button, TRUE);
			gtk_widget_set_sensitive(in_inv_reject_button, TRUE);
		}
	}
}

static void
out_inv_resend_button_cb(GtkWindow *w, GtkTreeView *tree)
{
	GtkTreeSelection *sel;
	GtkTreeIter iter;
	GtkTreeModel *model;
	Invitation *invitation;
	GaimBuddy *buddy;
	int send_result;
	GtkWidget *dialog;
	char time_str[32];
	char state_str[32];

	sel = gtk_tree_view_get_selection(tree);
	if (!gtk_tree_selection_get_selected(sel, &model, &iter)) {
		/**
		 * This shouldn't happen since the button should be disabled when no
		 * items in the list are selected.
		 */
		g_print("out_inv_resend_button_cb() called without an active selection\n");
		return;
	}
	
	/* Extract the Invitation * from the model using iter */
	gtk_tree_model_get(model, &iter,
						INVITATION_PTR, &invitation,
						-1);

	buddy = gaim_find_buddy(invitation->gaim_account, invitation->buddy_name);
	if (!buddy) {
		dialog = gtk_message_dialog_new(NULL,
					GTK_DIALOG_DESTROY_WITH_PARENT,
					GTK_MESSAGE_ERROR,
					GTK_BUTTONS_CLOSE,
					_("This buddy is not in your buddy list.  If you do not wish to accept this invitation, please remove it from your list."));
		gtk_dialog_run(GTK_DIALOG(dialog));
		gtk_widget_destroy(dialog);
		return;
	}
	
	/**
	 * Check to see if the buddy is online.  If the buddy is not online, we need
	 * to mark this invitation as PENDING and when the sign-on event occurs, we
	 * need to loop through all the PENDING events and send them out.
	 */
	if (buddy->present == 0) {
		/* Mark the invitation as PENDING */

		/* Update the invitation time and resend the invitation */
		time(&(invitation->time));

		/* Update the invitation state */
		invitation->state = STATE_PENDING;
	
		/* Format the time to a string */
		simias_fill_time_str(time_str, 32, invitation->time);

		/* Format the state string */
		simias_fill_state_str(state_str, invitation->state);
	
		/* Update the out_inv_store */
		gtk_list_store_set(GTK_LIST_STORE(model), &iter,
			TIME_COL,				time_str,
			STATE_COL,				state_str,
			-1);
		
		/* Save the updates to a file */
g_print("about_to_write 3\n");
		simias_save_invitations(GTK_LIST_STORE(model));

		return; /* That's about all we can do at this point */
	}

	send_result =
		simias_send_invitation_req(buddy, invitation->collection_id,
									invitation->collection_type,
									invitation->collection_name);
		
	if (send_result <= 0) {
		dialog = gtk_message_dialog_new(NULL,
					GTK_DIALOG_DESTROY_WITH_PARENT,
					GTK_MESSAGE_ERROR,
					GTK_BUTTONS_CLOSE,
					_("There was an error sending this message.  Perhaps the buddy is not online."));
		gtk_dialog_run(GTK_DIALOG(dialog));
		gtk_widget_destroy(dialog);
		return;
	}

	/**
	 * If we make it to this point, we have sent the message and we can now
	 * update the timestamp and invitation state.
	 */	

	/* Update the invitation time and resend the invitation */
	time(&(invitation->time));

	/* Update the invitation state */
	invitation->state = STATE_SENT;
	
	/* Format the time to a string */
	simias_fill_time_str(time_str, 32, invitation->time);

	/* Format the state string */
	simias_fill_state_str(state_str, invitation->state);

	/* Update the out_inv_store */
	gtk_list_store_set(GTK_LIST_STORE(model), &iter,
		TIME_COL,				time_str,
		STATE_COL,				state_str,
		-1);

	/* Save the updates to a file */
g_print("about_to_write 4\n");
	simias_save_invitations(GTK_LIST_STORE(model));
}

static void
out_inv_cancel_button_cb(GtkWindow *w, GtkTreeView *tree)
{
	g_print("FIXME: Implement out_inv_cancel_button_cb()\n");
}

static void
out_inv_remove_button_cb(GtkWindow *w, GtkTreeView *tree)
{
	GtkTreeSelection *sel;
	GtkTreeIter iter;
	GtkTreeModel *model;
	Invitation *invitation;

	sel = gtk_tree_view_get_selection(tree);
	if (!gtk_tree_selection_get_selected(sel, &model, &iter)) {
		/**
		 * This shouldn't happen since the button should be disabled when no
		 * items in the list are selected.
		 */
		g_print("out_inv_remove_button_cb() called without an active selection\n");
		return;
	}
	
	/* Extract the Invitation * from the model using iter */
	gtk_tree_model_get(model, &iter,
						INVITATION_PTR, &invitation,
						-1);

	/**
	 * Verify that the state of the selected invitation is STATE_ACCEPTED or
	 * STATE_REJECTED.
	 */
	if (invitation->state == STATE_ACCEPTED
		|| invitation->state == STATE_REJECTED) {
		gtk_list_store_remove(GTK_LIST_STORE(model), &iter);
		free(invitation);

		/* Save the updates to a file */
		simias_save_invitations(GTK_LIST_STORE(model));
	} else {
		g_print("out_inv_remove_button_cb() called on an invitation that is not in the STATE_ACCEPTED or STATE_REJECTED state\n");
	}
}

void
simias_out_inv_sel_changed_cb(GtkTreeSelection *sel, GtkTreeView *tree)
{
	/**
	 * Disable the buttons if no invitation is selected.
	 */
	GtkTreeIter iter;
	GtkTreeModel *model;
	Invitation *invitation;

	if (!gtk_tree_selection_get_selected(sel, &model, &iter)) {
		g_print("simias_out_inv_sel_changed_cb() called and nothing is selected.  Disabling buttons...\n");

		/* Disable the buttons */
		gtk_widget_set_sensitive(out_inv_resend_button, FALSE);
		gtk_widget_set_sensitive(out_inv_cancel_button, FALSE);
		gtk_widget_set_sensitive(out_inv_remove_button, FALSE);
		return;
	}
	
	/* Extract the Invitation * from the model using iter */
	gtk_tree_model_get(model, &iter,
						INVITATION_PTR, &invitation,
						-1);
	/**
	 * The remove button should only be enabled if the selected invitation is in
	 * the STATE_ACCEPTED or STATE_REJECTED state.
	 */
	if (invitation->state == STATE_ACCEPTED
		|| invitation->state == STATE_REJECTED) {
		gtk_widget_set_sensitive(out_inv_remove_button, TRUE);
	} else {
		gtk_widget_set_sensitive(out_inv_remove_button, FALSE);
	}
	 
	/**
	 * The cancel button can be enabled for invitations that are in the
	 * following states: STATE_PENDING, STATE_SENT, or STATE_ACCEPTED_PENDING.
	 */
	if (invitation->state == STATE_PENDING || invitation->state == STATE_SENT
		|| invitation->state == STATE_ACCEPTED_PENDING) {
		gtk_widget_set_sensitive(out_inv_cancel_button, TRUE);
	} else {
		gtk_widget_set_sensitive(out_inv_cancel_button, FALSE);
	}
	 
	/**
	 * The resend button can be enabled for invitations that are in the
	 * following states: STATE_PENDING, STATE_SENT, STATE_REJECTED,
	 * STATE_ACCEPTED_PENDING, or STATE_ACCEPTED.  Basically you'll be able to
	 * resend an invitation regardless of the invitation state.  This is because
	 * a buddy could move to a different computer and request that you resend
	 * the invitation to share the Collection.
	 */
	if (invitation->state == STATE_PENDING || invitation->state == STATE_SENT
		|| invitation->state == STATE_REJECTED
		|| invitation->state == STATE_ACCEPTED_PENDING
		|| invitation->state == STATE_ACCEPTED) {
		gtk_widget_set_sensitive(out_inv_resend_button, TRUE);
	} else {
		gtk_widget_set_sensitive(out_inv_resend_button, FALSE);
	}
}

/**
 * Setup the Simias Invitations Window
 *
 * If the Gaim iFolder Plugin is enabled, this window will be created and
 * initially hidden so that when users open and close the window, it won't
 * take as long to do.
 */
static void
init_invitations_window()
{
	GtkWidget *vbox;

	GtkWidget *in_inv_vbox;
	GtkWidget *in_inv_label;
	GtkWidget *in_inv_hbox;
	GtkWidget *in_inv_buttons_vbox;
	GtkWidget *in_inv_scrolled_win;
	GtkCellRenderer *in_inv_renderer;
	GtkTreeSelection *in_inv_sel;

	GtkWidget *out_inv_vbox;
	GtkWidget *out_inv_label;
	GtkWidget *out_inv_hbox;
	GtkWidget *out_inv_buttons_vbox;
	GtkWidget *out_inv_scrolled_win;
	GtkCellRenderer *out_inv_renderer;
	GtkTreeSelection *out_inv_sel;

	invitations_dialog = gtk_dialog_new_with_buttons(
				_("Simias Collection Invitations"),
				GTK_WINDOW(GAIM_GTK_BLIST(gaim_get_blist())),
				GTK_DIALOG_DESTROY_WITH_PARENT,
				GTK_STOCK_CLOSE,
				GTK_RESPONSE_CLOSE,
				NULL);
					
	/* Setup the properties of the window */
	/* FIXME: Make it so that this dialog isn't "top-most" to the gaim windows */
	gtk_dialog_set_has_separator(GTK_DIALOG(invitations_dialog), FALSE);
	gtk_window_set_resizable(GTK_WINDOW(invitations_dialog), TRUE);
	gtk_window_set_default_size(GTK_WINDOW(invitations_dialog), 600, 500);

	vbox = gtk_vbox_new(FALSE, 10);
	gtk_container_border_width(GTK_CONTAINER(vbox), 10);
	gtk_container_add(GTK_CONTAINER(GTK_DIALOG(invitations_dialog)->vbox),
			  vbox);
	
	/*****************************
	 * The Incoming Messages VBox
	 *****************************/
	in_inv_vbox = gtk_vbox_new(FALSE, 10);
	gtk_box_pack_start(GTK_BOX(vbox), in_inv_vbox, TRUE, TRUE, 0);

	in_inv_label = gtk_label_new(NULL);
	gtk_label_set_markup_with_mnemonic(GTK_LABEL(in_inv_label),
		_("<span size=\"larger\" weight=\"bold\">_Incoming Invitations</span>"));
	gtk_misc_set_alignment(GTK_MISC(in_inv_label), 0, 0.5);
	gtk_box_pack_start(GTK_BOX(in_inv_vbox),
			   in_inv_label, FALSE, FALSE, 0);

	in_inv_hbox = gtk_hbox_new(FALSE, 10);
	gtk_box_pack_start(GTK_BOX(in_inv_vbox), in_inv_hbox, TRUE, TRUE, 0);

	in_inv_scrolled_win = gtk_scrolled_window_new(NULL, NULL);
	gtk_scrolled_window_set_policy(GTK_SCROLLED_WINDOW(in_inv_scrolled_win),
		GTK_POLICY_AUTOMATIC, GTK_POLICY_AUTOMATIC);
	gtk_scrolled_window_set_shadow_type(
		GTK_SCROLLED_WINDOW(in_inv_scrolled_win), GTK_SHADOW_IN);
	gtk_box_pack_start(GTK_BOX(in_inv_hbox),
			   in_inv_scrolled_win, TRUE, TRUE, 0);

	/* Tree View Control Here */
	in_inv_tree = gtk_tree_view_new_with_model(GTK_TREE_MODEL(in_inv_store));
	gtk_label_set_mnemonic_widget(GTK_LABEL(in_inv_label), in_inv_tree);
	
	
	/**
	 * Now that the tree view holds a reference, we can get rid of our own
	 * reference.
	 */
	g_object_unref(G_OBJECT(in_inv_store));

	/* Create a cell renderer for a pixbuf */
	in_inv_renderer = gtk_cell_renderer_pixbuf_new();

	/* INVITATION_TYPE_ICON_COL */
	gtk_tree_view_insert_column_with_attributes(
		GTK_TREE_VIEW(in_inv_tree),
		-1, _("Type"), in_inv_renderer, "pixbuf", INVITATION_TYPE_ICON_COL, NULL);

	/* Create a cell renderer for text */
	in_inv_renderer = gtk_cell_renderer_text_new();

	/* BUDDY_NAME_COL */
	gtk_tree_view_insert_column_with_attributes(
		GTK_TREE_VIEW(in_inv_tree),
		-1, _("Buddy"), in_inv_renderer, "text", BUDDY_NAME_COL, NULL);

	/* TIME_COL */
	gtk_tree_view_insert_column_with_attributes(
		GTK_TREE_VIEW(in_inv_tree),
		-1, _("Sent/Received"), in_inv_renderer, "text", TIME_COL, NULL);

	/* COLLECTION_NAME_COL */
	gtk_tree_view_insert_column_with_attributes(
		GTK_TREE_VIEW(in_inv_tree),
		-1, _("Collection"), in_inv_renderer, "text", COLLECTION_NAME_COL, NULL);
		
	/* STATE_COL */
	gtk_tree_view_insert_column_with_attributes(
		GTK_TREE_VIEW(in_inv_tree),
		-1, _("State"), in_inv_renderer, "text", STATE_COL, NULL);

	gtk_container_add(GTK_CONTAINER(in_inv_scrolled_win), in_inv_tree);

	in_inv_buttons_vbox = gtk_vbox_new(FALSE, 10);
	gtk_box_pack_end(GTK_BOX(in_inv_hbox),
			in_inv_buttons_vbox, FALSE, FALSE, 0);

	in_inv_reject_button = gtk_button_new_with_mnemonic(_("_Reject"));
	gtk_widget_set_sensitive(in_inv_reject_button, FALSE);
	gtk_box_pack_end(GTK_BOX(in_inv_buttons_vbox),
			in_inv_reject_button, FALSE, FALSE, 0);

	in_inv_accept_button = gtk_button_new_with_mnemonic(_("_Accept"));
	gtk_widget_set_sensitive(in_inv_accept_button, FALSE);
	gtk_box_pack_end(GTK_BOX(in_inv_buttons_vbox),
			in_inv_accept_button, FALSE, FALSE, 0);
	
	in_inv_sel = gtk_tree_view_get_selection(GTK_TREE_VIEW(in_inv_tree));
	gtk_tree_selection_set_mode(in_inv_sel, GTK_SELECTION_SINGLE);
	

	/*****************************
	 * The Outgoing Messages VBox
	 *****************************/
	out_inv_vbox = gtk_vbox_new(FALSE, 10);
	gtk_box_pack_end(GTK_BOX(vbox), out_inv_vbox, TRUE, TRUE, 0);

	out_inv_label = gtk_label_new(NULL);
	gtk_label_set_markup_with_mnemonic(GTK_LABEL(out_inv_label),
		_("<span size=\"larger\" weight=\"bold\">_Outgoing Invitations</span>"));
	gtk_misc_set_alignment(GTK_MISC(out_inv_label), 0, 0.5);
	gtk_box_pack_start(GTK_BOX(out_inv_vbox),
			   out_inv_label, FALSE, FALSE, 0);

	out_inv_hbox = gtk_hbox_new(FALSE, 10);
	gtk_box_pack_start(GTK_BOX(out_inv_vbox), out_inv_hbox, TRUE, TRUE, 0);

	out_inv_scrolled_win = gtk_scrolled_window_new(NULL, NULL);
	gtk_scrolled_window_set_policy(GTK_SCROLLED_WINDOW(out_inv_scrolled_win),
		GTK_POLICY_AUTOMATIC, GTK_POLICY_AUTOMATIC);
	gtk_scrolled_window_set_shadow_type(
		GTK_SCROLLED_WINDOW(out_inv_scrolled_win), GTK_SHADOW_IN);
	gtk_box_pack_start(GTK_BOX(out_inv_hbox),
			   out_inv_scrolled_win, TRUE, TRUE, 0);

	/* Tree View Control Here */
	out_inv_tree = gtk_tree_view_new_with_model(GTK_TREE_MODEL(out_inv_store));
	gtk_label_set_mnemonic_widget(GTK_LABEL(out_inv_label), out_inv_tree);

	
	/**
	 * Now that the tree view holds a reference, we can get rid of our own
	 * reference.
	 */
	g_object_unref(G_OBJECT(out_inv_store));

	/* Create a cell renderer for a pixbuf */
	out_inv_renderer = gtk_cell_renderer_pixbuf_new();

	/* INVITATION_TYPE_ICON_COL */
	gtk_tree_view_insert_column_with_attributes(
		GTK_TREE_VIEW(out_inv_tree),
		-1, _("Type"), out_inv_renderer, "pixbuf", INVITATION_TYPE_ICON_COL, NULL);

	/* Create a cell renderer for text */
	out_inv_renderer = gtk_cell_renderer_text_new();

	/* BUDDY_NAME_COL */
	gtk_tree_view_insert_column_with_attributes(
		GTK_TREE_VIEW(out_inv_tree),
		-1, _("Buddy"), out_inv_renderer, "text", BUDDY_NAME_COL, NULL);

	/* TIME_COL */
	gtk_tree_view_insert_column_with_attributes(
		GTK_TREE_VIEW(out_inv_tree),
		-1, _("Sent/Received"), out_inv_renderer, "text", TIME_COL, NULL);

	/* COLLECTION_NAME_COL */
	gtk_tree_view_insert_column_with_attributes(
		GTK_TREE_VIEW(out_inv_tree),
		-1, _("Collection"), out_inv_renderer, "text", COLLECTION_NAME_COL, NULL);

	/* STATE_COL */
	gtk_tree_view_insert_column_with_attributes(
		GTK_TREE_VIEW(out_inv_tree),
		-1, _("State"), out_inv_renderer, "text", STATE_COL, NULL);

	gtk_container_add(GTK_CONTAINER(out_inv_scrolled_win), out_inv_tree);

	out_inv_buttons_vbox = gtk_vbox_new(FALSE, 10);
	gtk_box_pack_end(GTK_BOX(out_inv_hbox),
			out_inv_buttons_vbox, FALSE, FALSE, 0);

	out_inv_remove_button = gtk_button_new_with_mnemonic(_("Re_move"));
	gtk_widget_set_sensitive(out_inv_remove_button, FALSE);
	gtk_box_pack_end(GTK_BOX(out_inv_buttons_vbox),
			out_inv_remove_button, FALSE, FALSE, 0);

	out_inv_cancel_button = gtk_button_new_with_mnemonic(_("Ca_ncel"));
	gtk_widget_set_sensitive(out_inv_cancel_button, FALSE);
	gtk_box_pack_end(GTK_BOX(out_inv_buttons_vbox),
			out_inv_cancel_button, FALSE, FALSE, 0);

	out_inv_resend_button = gtk_button_new_with_mnemonic(_("Re_send"));
	gtk_widget_set_sensitive(out_inv_resend_button, FALSE);
	gtk_box_pack_end(GTK_BOX(out_inv_buttons_vbox),
			out_inv_resend_button, FALSE, FALSE, 0);

	out_inv_sel = gtk_tree_view_get_selection(GTK_TREE_VIEW(out_inv_tree));
	gtk_tree_selection_set_mode(out_inv_sel, GTK_SELECTION_SINGLE);
	


	/*******************
	 * Signal Callbacks
	 *******************/
	g_signal_connect(invitations_dialog, "response",
		G_CALLBACK(invitations_dialog_close_button_cb),
		NULL);
	/* FIXME: Figure out how which one of the following two events is the one that's allowing the dialog to be hidden and remove the other one for cleanliness :). */
	g_signal_connect(invitations_dialog, "destroy-event",
		G_CALLBACK(invitations_dialog_destroy_cb),
		NULL);
	g_signal_connect(invitations_dialog, "delete-event",
		G_CALLBACK(invitations_dialog_destroy_cb),
		NULL);

	g_signal_connect(G_OBJECT(in_inv_accept_button), "clicked",
		G_CALLBACK(in_inv_accept_button_cb), in_inv_tree);
	g_signal_connect(G_OBJECT(in_inv_reject_button), "clicked",
		G_CALLBACK(in_inv_reject_button_cb), in_inv_tree);
	g_signal_connect(G_OBJECT(in_inv_sel), "changed",
		G_CALLBACK(simias_in_inv_sel_changed_cb), in_inv_tree);

	g_signal_connect(G_OBJECT(out_inv_resend_button), "clicked",
		G_CALLBACK(out_inv_resend_button_cb), out_inv_tree);
	g_signal_connect(G_OBJECT(out_inv_cancel_button), "clicked",
		G_CALLBACK(out_inv_cancel_button_cb), out_inv_tree);
	g_signal_connect(G_OBJECT(out_inv_remove_button), "clicked",
		G_CALLBACK(out_inv_remove_button_cb), out_inv_tree);
	g_signal_connect(G_OBJECT(out_inv_sel), "changed",
		G_CALLBACK(simias_out_inv_sel_changed_cb), out_inv_tree);
}

/**
 * Show the Invitations Dialog
 */
void
simias_show_invitations_window()
{
	gtk_widget_show_all(invitations_dialog);
}

/**
 * Open and show the Simias Invitations Window
 */
static void
buddylist_cb_show_invitations_window(GaimBlistNode *node, gpointer user_data)
{
	simias_show_invitations_window();
}

/**
 * This function adds extra context menu items to a buddy in the Buddy List that
 * are specific to this plugin.
 */
static void
blist_add_context_menu_items_cb(GaimBlistNode *node, GList **menu)
{
	GaimBlistNodeAction *act;
	GaimBuddy *buddy;

	if (!GAIM_BLIST_NODE_IS_BUDDY(node))
		return;

	buddy = (GaimBuddy *)node;

	act = gaim_blist_node_action_new(_("Simulate Add Member"),
		buddylist_cb_simulate_share_collection, NULL);
	*menu = g_list_append(*menu, act);
	
	act = gaim_blist_node_action_new(_("Simias Invitations"),
		buddylist_cb_show_invitations_window, NULL);
	*menu = g_list_append(*menu, act);
}

static gboolean
plugin_load(GaimPlugin *plugin)
{
	gaim_signal_connect(gaim_blist_get_handle(),
				"blist-node-extended-menu",
				plugin,
				GAIM_CALLBACK(blist_add_context_menu_items_cb),
				NULL);

	gaim_signal_connect(gaim_conversations_get_handle(),
				"receiving-im-msg",
				plugin,
				GAIM_CALLBACK(simias_receiving_im_msg_cb),
				NULL);

	gaim_signal_connect(gaim_blist_get_handle(),
				"buddy-signed-on",
				plugin,
				GAIM_CALLBACK(simias_buddy_signed_on_cb),
				NULL);
				
	/**
	 * FIXME: Write and submit a patch to Gaim to emit a buddy-added-to-blist
	 * event with a very detailed explanation of why it is needed.
	 * 
	 * We need that type of event because when Gaim first starts up or when
	 * Simias/iFolder is started, we synchronize/add all the buddies in the
	 * Buddy List to the Simias Gaim Domain Roster.  Since this event doesn't
	 * exist, we could potentially miss adding a buddy to the domain roster only
	 * if the buddy is not signed-on.  If the buddy is signed on, the buddy
	 * would be added in the simias_buddy_signed_on_cb() function because that event is
	 * emitted when you add a new buddy and the buddy is signed-on.
	 * 
	 * Once this event is created and emitted, connect to it in this function
	 * and implement the callback function.
	 */

	/* Make sure the preferences are created if they don't exist */
	simias_init_default_prefs();

	/* Load up the GtkListStore's for incoming and outgoing invitations */
	simias_init_invitation_stores();
	
	/* Load up the GtkListStore for trusted buddies */
	simias_init_trusted_buddies_store();

	/* Load, but don't show the Invitations Window */
	init_invitations_window();
	
	/* Initialize the Simias Event Client */
	if (sec_init(&ec, on_sec_state_event, &ec)) {
		g_print("Error initializing Simias Event Client\n");
		return FALSE;
	}
	
	g_print("Simias Event Client initialized successfully\n");
		
	if (sec_register(ec)) {
		g_print("Error Registering Simias Event Client\n");
		return FALSE;
	}
	
	g_print("Simias Event Client registered successfully\n");

	return TRUE;
}

static gboolean
plugin_unload(GaimPlugin *plugin)
{
	/* Cleanup/De-register the Simias Event Client */
	if (sec_deregister(ec)) {
		g_print("Simias Event Client deregistration failed!\n");
		return FALSE;
	}
	
	if (sec_cleanup(&ec)) {
		g_print("Simias Event Client cleanup failed!\n");
		return FALSE;
	}
	
	return TRUE; /* Successfully Unloaded */
}

/**
 * Configuration Settings:
 * 
 * [ ] Notify me when:
 *     [ ] I receive a new invitation
 *     [ ] Buddies accept my invitations
 *     [ ] Buddies reject my invitations
 *     [ ] An error occurs
 * 
 * [ ] Automatically start Simias if needed
 */
static GtkWidget *
get_config_frame(GaimPlugin *plugin)
{
	GtkWidget *ret;
	GtkWidget *vbox;
	GtkWidget *label;
	GtkWidget *show_invitations_button;
	ret = gtk_vbox_new(FALSE, 18);
	gtk_container_set_border_width (GTK_CONTAINER (ret), 12);

	/* SECTION: Notify me when */
	vbox = gaim_gtk_make_frame(ret, _("Notify me when:"));
	gaim_gtk_prefs_checkbox(_("I receive a new invitation"),
	                SIMIAS_PREF_NOTIFY_RECEIVE_NEW_INVITATIONS, vbox);
	gaim_gtk_prefs_checkbox(_("Buddies accept my invitations"),
	                SIMIAS_PREF_NOTIFY_ACCEPT_INVITATIONS, vbox);
	gaim_gtk_prefs_checkbox(_("Buddies reject my invitations"),
	                SIMIAS_PREF_NOTIFY_REJECT_INVITATIONS, vbox);
	gaim_gtk_prefs_checkbox(_("An error occurs"),
	                SIMIAS_PREF_NOTIFY_ERRORS, vbox);
	label = gtk_label_new("(Not implemented yet)");
	gtk_box_pack_start(GTK_BOX(vbox), label, FALSE, FALSE, 0);
	
	/* SECTION: Buddy Location */
	vbox = gaim_gtk_make_frame(ret, _("Buddy Location"));
	gaim_gtk_prefs_checkbox(_("Automatically rediscover buddy IP Addresses"),
							SIMIAS_PREF_REDISCOVER_IP_ADDRS, vbox);
	
	/* SECTION: Simias */
	vbox = gaim_gtk_make_frame(ret, _("Simias"));
	gaim_gtk_prefs_checkbox(_("_Automatically start Simias if needed"),
	                SIMIAS_PREF_SIMIAS_AUTO_START, vbox);
	label = gtk_label_new("(Not implemented yet)");
	gtk_box_pack_start(GTK_BOX(vbox), label, FALSE, FALSE, 0);
	
	show_invitations_button =
		gtk_button_new_with_mnemonic(_("Show _Invitations"));
	gtk_box_pack_end(GTK_BOX(ret),
			show_invitations_button, FALSE, FALSE, 0);
	g_signal_connect(G_OBJECT(show_invitations_button), "clicked",
		G_CALLBACK(simias_show_invitations_window), NULL);

	gtk_widget_show_all(ret);
	return ret;
}


static GaimGtkPluginUiInfo ui_info =
{
	get_config_frame
};

static GaimPluginInfo info =
{
	GAIM_PLUGIN_MAGIC,
	GAIM_MAJOR_VERSION,
	GAIM_MINOR_VERSION,
	GAIM_PLUGIN_STANDARD,
	GAIM_GTK_PLUGIN_TYPE,
	0,
	NULL,
	GAIM_PRIORITY_DEFAULT,
	IFOLDER_PLUGIN_ID,
	N_("iFolder"),
	VERSION,
	N_("Allows you to share iFolders with your Gaim contacts."),
	N_("Buddies that are in your contact list will automatically be added as contacts in iFolder that you'll be able to share iFolders with."),
	"Boyd Timothy <btimothy@novell.com>",
	"http://www.ifolder.com/",
	plugin_load,
	plugin_unload,
	NULL,
	&ui_info,
	NULL,
	NULL
};

static void
init_plugin(GaimPlugin *plugin)
{
	/* FIXME: Possibly initialize the prefs in init_plugin() instead of plugin_load() */
}

GAIM_INIT_PLUGIN(ifolder, init_plugin, info)
