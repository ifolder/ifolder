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

#include "simias-invitation-store.h"

/* Gaim iFolder Includes */
#include "simias-util.h"
#include "gaim-domain.h"

#include <string.h>
#include <sys/stat.h>
#include <sys/types.h>
#include <unistd.h>

/* Gaim Includes */
#include "internal.h"
#include "gtkblist.h"
#include "debug.h"
#include "util.h"
#include "xmlnode.h"
#include "notify.h"

/* Externs */
extern GtkListStore *in_inv_store;
extern GtkListStore *out_inv_store;
extern GtkListStore *trusted_buddies_store;

/* Non-public Function Declarations */
static gboolean write_invitations_file(FILE *file, GtkListStore *store);
static gboolean invitations_read(GtkListStore *store, const char *filename);
static void load_invitations_from_file(GtkListStore *store, const char *name);

static gboolean write_trusted_buddies_file(FILE *file, GtkListStore *store);
static gboolean trusted_buddies_read(GtkListStore *store, const char *filename);
static void load_trusted_buddies_from_file(GtkListStore *store, const char *name);

/****************************************************
 * Global Variables                                 *
 ****************************************************/
static gboolean in_inv_safe_to_write = FALSE;
static gboolean out_inv_safe_to_write = FALSE;
static gboolean trusted_buddies_safe_to_write = FALSE;

/* Public Function Implementations */
/**
 * This function adds the elements of the Invitation object to the
 * store and also saves the Invitation as one of the items also.
 */
void
simias_add_invitation_to_store(GtkListStore *store, Invitation *invitation)
{
	GtkTreeIter iter;
	char time_str[32];
	char state_str[32];
	GdkPixbuf *invitation_icon = NULL;
	GdkPixbuf *scale;
	int scalesize = 15;
	char *icon_path = NULL;
	char *buddy_name = NULL;
	GaimBuddy *buddy;

g_print("simias_add_invitation_to_store() entered\n");
	if (!strcmp(invitation->collection_type, COLLECTION_TYPE_IFOLDER)) {
		icon_path = g_build_filename(DATADIR, "..", "share", "pixmaps", "gaim", "icons", "ifolder48.png", NULL);
	} else if (!strcmp(invitation->collection_type, COLLECTION_TYPE_GLYPHMARKS)) {
		icon_path = g_build_filename(DATADIR, "..", "share", "pixmaps", "gaim", "icons", "glyphmarks48.png", NULL);
	}

	if (icon_path) {
		g_print("Icon Path: %s\n", icon_path);
		invitation_icon = gdk_pixbuf_new_from_file(icon_path, NULL);
		g_free(icon_path);
	}
	
	if (!invitation_icon) {
		/* Just use a Gaim icon */
		invitation_icon = create_prpl_icon(invitation->gaim_account);
	}

	if (invitation_icon) {
		scale = gdk_pixbuf_scale_simple(invitation_icon, scalesize, scalesize,
										GDK_INTERP_BILINEAR);
		g_object_unref(invitation_icon);
		invitation_icon = scale;
	}	
	
	/* Format the time to a string */
	simias_fill_time_str(time_str, 32, invitation->time);

	/* Format the state string */
	simias_fill_state_str(state_str, invitation->state);
	
	/**
	 * Aquire an iterator.  This appends an empty row in the store and the row
	 * must be filled in with gtk_list_store_set() or gtk_list_store_set_value().
	 */
	gtk_list_store_append(store, &iter);
	
	/**
	 * Try to get an Alias Name for the buddy before reverting to the sceenname.
	 */
	buddy = gaim_find_buddy(invitation->gaim_account, invitation->buddy_name);
	buddy_name = (char *)gaim_buddy_get_alias(buddy);
	if (!buddy_name || !strlen(buddy_name)) {
		buddy_name = invitation->buddy_name;
	}

	/* Set the new row information with the invitation */
	gtk_list_store_set(store, &iter,
		INVITATION_TYPE_ICON_COL,	invitation_icon,
		BUDDY_NAME_COL,			buddy_name,
		TIME_COL,				time_str,
		COLLECTION_NAME_COL,	invitation->collection_name,
		STATE_COL,				state_str,
		INVITATION_PTR,			invitation,
		-1);
		
	/* Save the updates to a file */
g_print("about_to_write 5\n");
	simias_save_invitations(store);

	if (invitation_icon)
		g_object_unref(invitation_icon);
}

/**
 * When Gaim first starts up, load the invitation information from a data file
 * and populate the in_inv_store and out_inv_store.
 */
void
simias_init_invitation_stores()
{
g_print("simias_init_invitation_stores() entered\n");
	in_inv_store = gtk_list_store_new(N_COLS,
					GDK_TYPE_PIXBUF,
					G_TYPE_STRING,
					G_TYPE_STRING,
					G_TYPE_STRING,
					G_TYPE_STRING,
					G_TYPE_POINTER);
	/* Setup sorting by date/time of the message by default */
	gtk_tree_sortable_set_sort_column_id(GTK_TREE_SORTABLE(in_inv_store),
										1, GTK_SORT_ASCENDING);

	/* Load in data from file */
	load_invitations_from_file(in_inv_store, FILENAME_IN_INVITATIONS);
	in_inv_safe_to_write = TRUE;

	out_inv_store = gtk_list_store_new(N_COLS,
					GDK_TYPE_PIXBUF,
					G_TYPE_STRING,
					G_TYPE_STRING,
					G_TYPE_STRING,
					G_TYPE_STRING,
					G_TYPE_POINTER);
	gtk_tree_sortable_set_sort_column_id(GTK_TREE_SORTABLE(out_inv_store),
										1, GTK_SORT_ASCENDING);
	/* Load in data from file */
	load_invitations_from_file(out_inv_store, FILENAME_OUT_INVITATIONS);
	out_inv_safe_to_write = TRUE;
}

void
simias_add_new_trusted_buddy(GtkListStore *store, GaimBuddy *buddy,
						char *ip_address, char *ip_port)
{
	GtkTreeIter iter;
	GdkPixbuf *buddy_icon;

g_print("simias_add_new_trusted_buddy() called: %s (%s:%s)\n", buddy->name, ip_address, ip_port);

	/**
	 * FIXME: Change this icon to be based off of the type of Simias Collection
	 * that is being shared and possibly the state of the invitation too.
	 * Perhaps the invitation state could just be shown as an emblem overlay
	 * of the invitation icon similar to how emblems are overlaid in Nautilus.
	 */
	buddy_icon = create_prpl_icon(buddy->account);
	/**
	 * Aquire an iterator.  This appends an empty row in the store and the row
	 * must be filled in with gtk_list_store_set() or gtk_list_store_set_value().
	 */
	gtk_list_store_append(store, &iter);

	/* Set the new row information with the invitation */
	gtk_list_store_set(store, &iter,
		TRUSTED_BUDDY_ICON_COL,		buddy_icon,
		TRUSTED_BUDDY_NAME_COL,		buddy->name,
		TRUSTED_BUDDY_IP_ADDR_COL,	ip_address,
		TRUSTED_BUDDY_IP_PORT_COL,	ip_port,
		GAIM_ACCOUNT_PTR_COL,		buddy->account,
		-1);

	if (buddy_icon)
		g_object_unref(buddy_icon);

	gaim_blist_node_set_string(&(buddy->node), "simias-ip-addr", ip_address);
	gaim_blist_node_set_string(&(buddy->node), "simias-ip-port", ip_port);

	/* FIXME: Make a call into Simias (if it's up) to update the member info */
	/* Add/Update this trusted buddy in Simias */
	gaim_domain_add_gaim_buddy(gaim_account_get_username(buddy->account),
							   gaim_account_get_protocol_id(buddy->account),
							   buddy->name,
							   gaim_buddy_get_alias(buddy),
							   ip_address,
							   ip_port);

	/* Update the trusted buddies file */
	simias_save_trusted_buddies(trusted_buddies_store);
}

/**
 * When the plugin first loads (when Gaim starts or the user enables this
 * plugin), fill the trusted_buddies_store with the information persited in the
 * trusted_buddies_file.
 */
void
simias_init_trusted_buddies_store()
{
	/**
	 * When this information is stored into a file, we need to save the
	 * following information so that we can restore the store:
	 * 
	 * Account Protocol, Account Username, Buddy Name, IP Address, IP Port
	 */
	trusted_buddies_store = gtk_list_store_new(N_TRUSTED_BUDDY_COLS,
			GDK_TYPE_PIXBUF,	/* Put an icon for the account here */
			G_TYPE_STRING,		/* TRUSTED_BUDDY_NAME_COL */
			G_TYPE_STRING,		/* TRUSTED_BUDDY_IP_ADDR_COL */
			G_TYPE_STRING,		/* TRUSTED_BUDDY_IP_PORT_COL */
			G_TYPE_POINTER);	/* GaimAccount * */
	
	/* Load in the data from a config file */
	load_trusted_buddies_from_file(trusted_buddies_store, FILENAME_TRUSTED_BUDDIES);
}

void
simias_save_trusted_buddies(GtkListStore *store)
{
	FILE *file;
	struct stat st;
	char *user_dir = gaim_user_dir();
	char *filename;
	char filename_save[512];
	char *filename_real;
	
	gaim_debug(GAIM_DEBUG_INFO, "simias",
			   "simias_save_trusted_buddies() called\n");

	if (!trusted_buddies_safe_to_write) {
		gaim_debug(GAIM_DEBUG_WARNING, "simias",
				   "Tried to write to the trusted buddies file before reading it!\n");
		return;
	}
	
	file = fopen(user_dir, "r");
	if (!file)
		mkdir(user_dir, S_IRUSR | S_IWUSR | S_IXUSR);
	else
		fclose(file);
	
	sprintf(filename_save, "%s.save", FILENAME_TRUSTED_BUDDIES);
		
	filename = g_build_filename(user_dir, filename_save, NULL);

	if ((file = fopen(filename, "w"))) {
		write_trusted_buddies_file(file, store);
		fclose(file);
		chmod(filename, S_IRUSR | S_IWUSR);
	} else {
		gaim_debug(GAIM_DEBUG_ERROR, "simias", "Unable to write %s\n",
				   filename);
		g_free(filename);
		return;
	}

	if (stat(filename, &st) || (st.st_size == 0)) {
		gaim_debug_error("simias", "Failed to save trusted buddies file\n");
		unlink(filename);
		g_free(filename);
		return;
	}

	filename_real = g_build_filename(user_dir, FILENAME_TRUSTED_BUDDIES, NULL);

	if (rename(filename, filename_real) < 0)
		gaim_debug(GAIM_DEBUG_ERROR, "simias",
				   "Error renaming %s to %s\n", filename, filename_real);

	g_free(filename);
	g_free(filename_real);
}

/**
 * FIXME: Need to put this function in a thread or something so that the messages
 * to write to the file just queue up the requests and only write if it's been
 * at least 5 seconds or so...so that we're not just writing all the time.
 */
void
simias_save_invitations(GtkListStore *store)
{
	FILE *file;
	struct stat st;
	char *user_dir = gaim_user_dir();
	char *filename;
	char filename_save[512];
	char *filename_real;
	
	gaim_debug(GAIM_DEBUG_INFO, "simias",
			   "simias_save_invitations() called on: %s\n",
			   (store == in_inv_store ? "Incoming Invitations" :
			   		store == out_inv_store ? "Outgoing Invitations" :
			   			"Unknown Invitation Store"));

	if (store == in_inv_store && !in_inv_safe_to_write) {
		gaim_debug(GAIM_DEBUG_WARNING, "simias",
				   "Tried to write to the incoming invitations file before reading it!\n");
		return;
	} else if (store == out_inv_store && !out_inv_safe_to_write) {
		gaim_debug(GAIM_DEBUG_WARNING, "simias",
				   "Tried to write to the outgoing invitations file before reading it!\n");
		return;
	}
	
	file = fopen(user_dir, "r");
	if (!file)
		mkdir(user_dir, S_IRUSR | S_IWUSR | S_IXUSR);
	else
		fclose(file);
	
	if (store == in_inv_store) {
		sprintf(filename_save, "%s.save", FILENAME_IN_INVITATIONS);
	} else if (store == out_inv_store) {
		sprintf(filename_save, "%s.save", FILENAME_OUT_INVITATIONS);
	} else {
		sprintf(filename_save, "simias-unknown-invitations.xml.save");
	}
		
	filename = g_build_filename(user_dir, filename_save, NULL);

	if ((file = fopen(filename, "w"))) {
		write_invitations_file(file, store);
		fclose(file);
		chmod(filename, S_IRUSR | S_IWUSR);
	} else {
		gaim_debug(GAIM_DEBUG_ERROR, "simias", "Unable to write %s\n",
				   filename);
		g_free(filename);
		return;
	}

	if (stat(filename, &st) || (st.st_size == 0)) {
		gaim_debug_error("simias", "Failed to save invitations file\n");
		unlink(filename);
		g_free(filename);
		return;
	}

	filename_real = g_build_filename(user_dir,
						(store == in_inv_store ?
							FILENAME_IN_INVITATIONS :
								store == out_inv_store ?
									FILENAME_OUT_INVITATIONS :
										"simias-unknown-invitations.xml"),
						NULL);

	if (rename(filename, filename_real) < 0)
		gaim_debug(GAIM_DEBUG_ERROR, "simias",
				   "Error renaming %s to %s\n", filename, filename_real);

	g_free(filename);
	g_free(filename_real);
}

/**
 * This function walks the GtkListStore and looks for the collection_id inside
 * the G_TYPE_POINTER column that points to an Invitation *.  If a match is
 * found, iter will be returned pointing to this row AND the function will
 * return TRUE.  If no match is found, it will return FALSE;
 */
gboolean
simias_lookup_collection_in_store(GtkListStore *store, char *collection_id,
							GtkTreeIter *iter)
{
	Invitation *invitation;
	gboolean valid;
	
	valid = gtk_tree_model_get_iter_first(GTK_TREE_MODEL(store), iter);
	while (valid) {
		/* Extract the Invitation * out of the model */
		gtk_tree_model_get(GTK_TREE_MODEL(store), iter,
							INVITATION_PTR, &invitation,
							-1);
							
		/* Check to see if the collection IDs match */
		if (strcmp(invitation->collection_id, collection_id) == 0) {
			/* We've found our match! */
			return TRUE;
		}

		valid = gtk_tree_model_iter_next(GTK_TREE_MODEL(store), iter);
	}
	
	return FALSE; /* No match was found */
}

/**
 * This function returns TRUE if the buddy exists in the store and FALSE
 * otherwise.
 * 
 * If TRUE is returned, iter will be set to point to the row in the store where
 * the buddy exists.
 */
gboolean
simias_lookup_trusted_buddy(GtkListStore *store, GaimBuddy *buddy, GtkTreeIter *iter)
{
	gboolean valid;
	GaimAccount *store_account;
	GaimBuddy *store_buddy;
	gchar *store_buddy_name;
	
g_print("lookup_trusted_buddy() entered\n");
	valid = gtk_tree_model_get_iter_first(GTK_TREE_MODEL(store), iter);
	while (valid) {
		/* Extract the Invitation * out of the model */
		gtk_tree_model_get(GTK_TREE_MODEL(store), iter,
							TRUSTED_BUDDY_NAME_COL, &store_buddy_name,
							GAIM_ACCOUNT_PTR_COL, &store_account,
							-1);

		if (!store_buddy_name) {
			g_print("store_buddy_name is NULL inside lookup_trusted_buddy()\n");
			continue;
		}
		
		if (!store_account) {
			g_print("store_account is NULL inside lookup_trusted_buddy()\n");
			continue;
		}
		
		store_buddy = gaim_find_buddy(store_account, store_buddy_name);
		if (strcmp(store_buddy->name, buddy->name) == 0) {
			/* We've found our match! */
			return TRUE;
		}

		valid = gtk_tree_model_iter_next(GTK_TREE_MODEL(store), iter);
	}
	
	return FALSE; /* No match was found */
}

/* Non-public Function Implementations */

static gboolean
write_invitations_file(FILE *file, GtkListStore *store)
{
	GtkTreeIter iter;
	Invitation *invitation;
	gboolean valid;
	char time_str[32];

	gaim_debug(GAIM_DEBUG_INFO, "simias",
			   "write_invitations_file() called on: %s\n",
			   (store == in_inv_store ? "Incoming Invitations" :
			   		store == out_inv_store ? "Outgoing Invitations" :
			   			"Unknown Invitation Store"));

	fprintf(file, "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\n\n");
	fprintf(file, "<simias>\n");
	fprintf(file, "\t<invitations>\n");

	valid = gtk_tree_model_get_iter_first(GTK_TREE_MODEL(store), &iter);
	while (valid) {
		/* Extract the Invitation * out of the model */
		gtk_tree_model_get(GTK_TREE_MODEL(store), &iter,
					INVITATION_PTR, &invitation,
					-1);

		if (!invitation) {
			continue;
		}
		
		fprintf(file, "\t\t<invitation\n");
		
		fprintf(file, "\t\t\taccount-name=\"%s\"\n",
				gaim_account_get_username(invitation->gaim_account));
		fprintf(file, "\t\t\taccount-proto=\"%s\"\n",
				gaim_account_get_protocol_id(invitation->gaim_account));
		fprintf(file, "\t\t\tbuddy-name=\"%s\"\n", invitation->buddy_name);
		fprintf(file, "\t\t\tstate=\"%d\"\n", invitation->state);

		/* Format the time to a string */
		simias_fill_time_str(time_str, 32, invitation->time);
		fprintf(file, "\t\t\ttime=\"%s\"\n", time_str);

		fprintf(file, "\t\t\tcollection-id=\"%s\"\n", invitation->collection_id);
		fprintf(file, "\t\t\tcollection-type=\"%s\"\n", invitation->collection_type);
		fprintf(file, "\t\t\tcollection-name=\"%s\"\n", g_markup_escape_text(invitation->collection_name, -1));
		fprintf(file, "\t\t\tip-addr=\"%s\"\n", invitation->ip_addr);
		fprintf(file, "\t\t\tip-port=\"%s\"\n", invitation->ip_port);
		
		fprintf(file, "\t\t/>\n");

		valid = gtk_tree_model_iter_next(GTK_TREE_MODEL(store), &iter);
	}
	
	fprintf(file, "\t</invitations>\n");
	fprintf(file, "</simias>\n");

	return TRUE;
}

static gboolean
invitations_read(GtkListStore *store, const char *filename)
{
	GError *error;
	gchar *contents = NULL;
	gsize length;
	xmlnode *simias, *invitations;
	
	gaim_debug(GAIM_DEBUG_INFO, "simias",
			   "Reading %s\n", filename);
	if (!g_file_get_contents(filename, &contents, &length, &error)) {
		gaim_debug(GAIM_DEBUG_ERROR, "simias",
				   "Error reading invitations file: %s\n", error->message);
		g_error_free(error);
		return FALSE;
	}
	
	simias = xmlnode_from_str(contents, length);
	
	if (!simias) {
		FILE *backup;
		char backup_filename[512];
		char *name;
		gaim_debug(GAIM_DEBUG_ERROR, "simias", "Error parsing %s\n",
				filename);
		sprintf(backup_filename, "%s~", filename);
		name = g_build_filename(gaim_user_dir(), backup_filename, NULL);

		if ((backup = fopen(name, "w"))) {
			fwrite(contents, length, 1, backup);
			fclose(backup);
			chmod(name, S_IRUSR | S_IWUSR);
		} else {
			gaim_debug(GAIM_DEBUG_ERROR, "simias", "Unable to write backup %s\n",
				   name);
		}
		g_free(name);
		g_free(contents);
		return FALSE;
	}
	
	g_free(contents);
	
	invitations = xmlnode_get_child(simias, "invitations");
	if (invitations) {
		xmlnode *invitation_node;
		for (invitation_node = xmlnode_get_child(invitations, "invitation"); invitation_node;
				invitation_node = xmlnode_get_next_twin(invitation_node)) {
			/**
			 * Parse an invitation here and if successful, add it to the
			 * GtkListStore.
			 */
			const char *account_name;
			const char *account_proto;
			const char *buddy_name;
			const char *state_str;
			const char *time_str;
			const char *collection_id;
			const char *collection_type;
			const char *collection_name;
			const char *ip_addr;
			const char *ip_port;
			Invitation *invitation;
			struct tm *time_ptr;
			
			account_name = xmlnode_get_attrib(invitation_node, "account-name");
			account_proto = xmlnode_get_attrib(invitation_node, "account-proto");
			buddy_name = xmlnode_get_attrib(invitation_node, "buddy-name");
			state_str = xmlnode_get_attrib(invitation_node, "state");
			time_str = xmlnode_get_attrib(invitation_node, "time");
			collection_id = xmlnode_get_attrib(invitation_node, "collection-id");
			collection_type = xmlnode_get_attrib(invitation_node, "collection-type");
			collection_name = xmlnode_get_attrib(invitation_node, "collection-name");
			ip_addr = xmlnode_get_attrib(invitation_node, "ip-addr");
			ip_port = xmlnode_get_attrib(invitation_node, "ip-port");
			
			/* Verify we've read enough information in to construct an Invitation */
			if (account_name == NULL || strlen(account_name) == 0) {
				gaim_debug(GAIM_DEBUG_ERROR, "simias",
					"Unable to parse account-name attribute\n");
				xmlnode_free(simias);
				return TRUE;
			} else if (account_proto == NULL || strlen(account_proto) == 0) {
				gaim_debug(GAIM_DEBUG_ERROR, "simias",
					"Unable to parse account-proto attribute\n");
				xmlnode_free(simias);
				return TRUE;
			} else if (buddy_name == NULL || strlen(buddy_name) == 0) {
				gaim_debug(GAIM_DEBUG_ERROR, "simias",
					"Unable to parse buddy-name attribute\n");
				xmlnode_free(simias);
				return TRUE;
			} else if (state_str == NULL || strlen(state_str) == 0) {
				gaim_debug(GAIM_DEBUG_ERROR, "simias",
					"Unable to parse state attribute\n");
				xmlnode_free(simias);
				return TRUE;
			} else if (time_str == NULL || strlen(time_str) == 0) {
				gaim_debug(GAIM_DEBUG_ERROR, "simias",
					"Unable to parse time attribute\n");
				xmlnode_free(simias);
				return TRUE;
			} else if (collection_id == NULL || strlen(collection_id) == 0) {
				gaim_debug(GAIM_DEBUG_ERROR, "simias",
					"Unable to parse collection-id attribute\n");
				xmlnode_free(simias);
				return TRUE;
			} else if (collection_type == NULL || strlen(collection_type) == 0) {
				gaim_debug(GAIM_DEBUG_ERROR, "simias",
					"Unable to parse collection-type attribute\n");
				xmlnode_free(simias);
				return TRUE;
			} else if (collection_name == NULL || strlen(collection_name) == 0) {
				gaim_debug(GAIM_DEBUG_ERROR, "simias",
					"Unable to parse collection-name attribute\n");
				xmlnode_free(simias);
				return TRUE;
			} else if (ip_addr == NULL || strlen(ip_addr) == 0) {
				ip_addr = "0.0.0.0";
			} else if (ip_port == NULL || strlen(ip_port) == 0) {
				ip_port = "0";
			}

			invitation = malloc(sizeof(Invitation));
			invitation->gaim_account =
				gaim_accounts_find(account_name, account_proto);
			if (!invitation->gaim_account) {
				/* The account must not be valid anymore, drop the invitation */
				free(invitation);
				continue; /* On to the next invitation */
			}
			
			sprintf(invitation->buddy_name, buddy_name);
			invitation->state = atoi(state_str);

			/* FIXME: Figure out how to read in the timestamp */
			time_ptr = localtime(&invitation->time);

			sprintf(invitation->collection_id, collection_id);
			sprintf(invitation->collection_type, collection_type);
			sprintf(invitation->collection_name, collection_name);
			sprintf(invitation->ip_addr, ip_addr);
			sprintf(invitation->ip_port, ip_port);
			
			simias_add_invitation_to_store(store, invitation);
		}
	}

	gaim_debug(GAIM_DEBUG_INFO, "simias", "Finished reading %s\n",
			   filename);

	xmlnode_free(simias);
	return TRUE;
}

static void
load_invitations_from_file(GtkListStore *store, const char *name)
{
	char *user_dir;
	char *filename;
	char *msg;

	gaim_debug(GAIM_DEBUG_INFO, "simias", "load_invitations_from_file() entered\n");
	user_dir = gaim_user_dir();
	
	if (!user_dir) {
		g_print("load_invitations_from_file() got NULL response from gaim_user_dir()\n");
	}
	
	filename = g_build_filename(user_dir, name, NULL);
	
	if (g_file_test(filename, G_FILE_TEST_EXISTS)) {
		if (!invitations_read(store, filename)) {
			msg = g_strdup_printf(_("An error was encountered parsing your "
					"saved invitations file.  It has not been loaded, "
					"and the old file has been moved to %s~."), name);
			gaim_notify_error(NULL, NULL, _("Simias Invitations File Error"), msg);
			g_free(msg);
		}
	} else {
		gaim_debug(GAIM_DEBUG_INFO, "simias", "load_invitations_from_file() file does not exist: %s\n",
					filename);
	}
	
	g_free(filename);
}

static gboolean
write_trusted_buddies_file(FILE *file, GtkListStore *store)
{
	GtkTreeIter iter;
	gboolean valid;
	char *buddy_name;
	char *ip_addr;
	char *ip_port;
	GaimAccount *gaim_account;

	gaim_debug(GAIM_DEBUG_INFO, "simias",
			   "write_trusted_buddies_file() called\n");

	fprintf(file, "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\n\n");
	fprintf(file, "<simias>\n");
	fprintf(file, "\t<buddies>\n");

	valid = gtk_tree_model_get_iter_first(GTK_TREE_MODEL(store), &iter);
	while (valid) {
		/* Extract the buddy information out of the model */
		gtk_tree_model_get(GTK_TREE_MODEL(store), &iter,
					TRUSTED_BUDDY_NAME_COL, &buddy_name,
					TRUSTED_BUDDY_IP_ADDR_COL, &ip_addr,
					TRUSTED_BUDDY_IP_PORT_COL, &ip_port,
					GAIM_ACCOUNT_PTR_COL, &gaim_account,
					-1);

		if (!gaim_account) {
			continue;
		}
		
		fprintf(file, "\t\t<buddy\n");
		
		fprintf(file, "\t\t\taccount-name=\"%s\"\n",
				gaim_account_get_username(gaim_account));
		fprintf(file, "\t\t\taccount-proto=\"%s\"\n",
				gaim_account_get_protocol_id(gaim_account));
		fprintf(file, "\t\t\tbuddy-name=\"%s\"\n", buddy_name);
		fprintf(file, "\t\t\tip-addr=\"%s\"\n", ip_addr);
		fprintf(file, "\t\t\tip-port=\"%s\"\n", ip_port);
		
		fprintf(file, "\t\t/>\n");
		
		/* FIXME: Go through the file and make sure to free all the strings extracted from a model in the same manner as the following three lines */
		g_free(buddy_name);
		g_free(ip_addr);
		g_free(ip_port);

		valid = gtk_tree_model_iter_next(GTK_TREE_MODEL(store), &iter);
	}
	
	fprintf(file, "\t</buddies>\n");
	fprintf(file, "</simias>\n");

	return TRUE;
}

static gboolean
trusted_buddies_read(GtkListStore *store, const char *filename)
{
	GError *error;
	gchar *contents = NULL;
	gsize length;
	xmlnode *simias, *buddies;
	
	gaim_debug(GAIM_DEBUG_INFO, "simias",
			   "Reading %s\n", filename);
	if (!g_file_get_contents(filename, &contents, &length, &error)) {
		gaim_debug(GAIM_DEBUG_ERROR, "simias",
				   "Error reading buddies file: %s\n", error->message);
		g_error_free(error);
		return FALSE;
	}
	
	simias = xmlnode_from_str(contents, length);
	
	if (!simias) {
		FILE *backup;
		char backup_filename[512];
		char *name;
		gaim_debug(GAIM_DEBUG_ERROR, "simias", "Error parsing %s\n",
				filename);
		sprintf(backup_filename, "%s~", filename);
		name = g_build_filename(gaim_user_dir(), backup_filename, NULL);

		if ((backup = fopen(name, "w"))) {
			fwrite(contents, length, 1, backup);
			fclose(backup);
			chmod(name, S_IRUSR | S_IWUSR);
		} else {
			gaim_debug(GAIM_DEBUG_ERROR, "simias", "Unable to write backup %s\n",
				   name);
		}
		g_free(name);
		g_free(contents);
		return FALSE;
	}
	
	g_free(contents);
	
	buddies = xmlnode_get_child(simias, "buddies");
	if (buddies) {
		xmlnode *buddy_node;
		for (buddy_node = xmlnode_get_child(buddies, "buddy"); buddy_node;
				buddy_node = xmlnode_get_next_twin(buddy_node)) {
			/**
			 * Parse an invitation here and if successful, add it to the
			 * GtkListStore.
			 */
			const char *account_name;
			const char *account_proto;
			const char *buddy_name;
			const char *ip_addr;
			const char *ip_port;
			GaimAccount *gaim_account;
			
			account_name = xmlnode_get_attrib(buddy_node, "account-name");
			account_proto = xmlnode_get_attrib(buddy_node, "account-proto");
			buddy_name = xmlnode_get_attrib(buddy_node, "buddy-name");
			ip_addr = xmlnode_get_attrib(buddy_node, "ip-addr");
			ip_port = xmlnode_get_attrib(buddy_node, "ip-port");
			
			/* Verify we've read enough information in to construct an Invitation */
			if (account_name == NULL || strlen(account_name) == 0) {
				gaim_debug(GAIM_DEBUG_ERROR, "simias",
					"Unable to parse account-name attribute\n");
				xmlnode_free(simias);
				return TRUE;
			} else if (account_proto == NULL || strlen(account_proto) == 0) {
				gaim_debug(GAIM_DEBUG_ERROR, "simias",
					"Unable to parse account-proto attribute\n");
				xmlnode_free(simias);
				return TRUE;
			} else if (buddy_name == NULL || strlen(buddy_name) == 0) {
				gaim_debug(GAIM_DEBUG_ERROR, "simias",
					"Unable to parse buddy-name attribute\n");
				xmlnode_free(simias);
				return TRUE;
			} else if (ip_addr == NULL || strlen(ip_addr) == 0) {
				ip_addr = "0.0.0.0";
			} else if (ip_port == NULL || strlen(ip_port) == 0) {
				ip_port = "0";
			}
			
			gaim_account = gaim_accounts_find(account_name, account_proto);
			if (!gaim_account) {
				/* The account must not be valid anymore, drop the invitation */
				continue; /* On to the next invitation */
			}

			simias_add_new_trusted_buddy(store,
								  gaim_find_buddy(gaim_account, buddy_name),
								  (char *) ip_addr, (char *) ip_port);
		}
	}

	gaim_debug(GAIM_DEBUG_INFO, "simias", "Finished reading %s\n",
			   filename);

	xmlnode_free(simias);
	return TRUE;
}

static void
load_trusted_buddies_from_file(GtkListStore *store, const char *name)
{
	char *user_dir;
	char *filename;
	char *msg;

	gaim_debug(GAIM_DEBUG_INFO, "simias", "load_trusted_buddies_from_file() entered\n");
	user_dir = gaim_user_dir();
	
	if (!user_dir) {
		g_print("load_trusted_buddies_from_file() got NULL response from gaim_user_dir()\n");
	}
	
	filename = g_build_filename(user_dir, name, NULL);
	
	if (g_file_test(filename, G_FILE_TEST_EXISTS)) {
		if (!trusted_buddies_read(store, filename)) {
			msg = g_strdup_printf(_("An error was encountered parsing your "
					"trusted buddies file.  It has not been loaded, "
					"and the old file has been moved to %s~."), name);
			gaim_notify_error(NULL, NULL, _("Simias Trusted Buddies File Error"), msg);
			g_free(msg);
		}
	} else {
		gaim_debug(GAIM_DEBUG_INFO, "simias", "load_trusted_buddies_from_file() file does not exist: %s\n",
					filename);
	}
	
	trusted_buddies_safe_to_write = TRUE;
	
	g_free(filename);
}

