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

#include "debug.h"
#include "prefs.h"
#include "signals.h"
#include "util.h"
#include "version.h"

#include "gtkplugin.h"

#include "buddy-sync.h"

/****************************************************
 * Static Definitions (#defines)                    *
 ****************************************************/
#define IFOLDER_PLUGIN_ID "ifolder"

#define IFOLDER_MSG_IP_ADDR_REQ		"[simias:ip_addr_req:"
#define IFOLDER_MSG_IP_ADDR_RESP	"[simias:ip_addr_resp:"
#define IFOLDER_MSG_IP_ADDR_REQ_DENY	"[simias:ip_addr_req_deny]"

#define INVITATION_REQUEST_MSG		"[simias:invitation-request:"
#define INVITATION_REQUEST_DENY_MSG	"[simias:invitation-request-deny:"
#define INVITATION_REQUEST_ACCEPT_MSG	"[simias:invitation-request-accept:"
#define PING_REQUEST_MSG		"[simias:ping-request:"
#define PING_RESPONSE_MSG		"[simias:ping-response:"

#define TIMESTAMP_FORMAT "%I/%d/%Y %I:%M %p"

#define FILENAME_IN_INVITATIONS "simias-in-invitations.xml"
#define FILENAME_OUT_INVITATIONS "simias-out-invitations.xml"
#define FILENAME_TRUSTED_BUDDIES "simias-trusted-buddies.xml"

#define SIMIAS_PREF_PATH "/plugins/gtk/simias"

#define SIMIAS_PREF_NOTIFY_RECEIVE_NEW_INVITATIONS "/plugins/gtk/simias/notify_receive_new_invitations"
#define SIMIAS_PREF_NOTIFY_RECEIVE_NEW_INVITATIONS_DEF TRUE

#define SIMIAS_PREF_NOTIFY_ACCEPT_INVITATIONS "/plugins/gtk/simias/notify_buddies_accept_invitations"
#define SIMIAS_PREF_NOTIFY_ACCEPT_INVITATIONS_DEF TRUE

#define SIMIAS_PREF_NOTIFY_REJECT_INVITATIONS "/plugins/gtk/simias/notify_buddies_reject_invitations"
#define SIMIAS_PREF_NOTIFY_REJECT_INVITATIONS_DEF TRUE

#define SIMIAS_PREF_NOTIFY_ERRORS "/plugins/gtk/simias/notify_on_errors"
#define SIMIAS_PREF_NOTIFY_ERRORS_DEF FALSE

#define SIMIAS_PREF_REDISCOVER_IP_ADDRS "/plugins/gtk/simias/rediscover_ip_addrs"
#define SIMIAS_PREF_REDISCOVER_IP_ADDRS_DEF TRUE

#define SIMIAS_PREF_SIMIAS_AUTO_START "/plugins/gtk/simias/auto_start_simias"
#define SIMIAS_PREF_SIMIAS_AUTO_START_DEF FALSE

#define COLLECTION_TYPE_IFOLDER "ifolder"
#define COLLECTION_TYPE_GLYPHMARKS "glyphmarks"

/****************************************************
 * Type Definitions                                 *
 ****************************************************/
typedef enum
{
	INVITATION_REQUEST = 1,
	INVITATION_REQUEST_DENY,
	INVITATION_REQUEST_ACCEPT,
	PING_REQUEST,
	PING_RESPONSE
} SIMIAS_MSG_TYPE;


/**
 * The INVITATION_STATE Enumeration:
 * 
 * STATE_NEW: This state is used for incoming invitations and denotes that the
 * user has not accepted or denied the invitation.
 * 
 * STATE_PENDING: The invitation has been added by Simias but not sent yet.  If
 * an invitation stays in this state for a while it's likely that the buddy is
 * not online.  When a "buddy-sign-on" event occurs for this buddy, the
 * invitation will be sent at that point.
 *
 * STATE_SENT: The invitation has been sent to the buddy but they haven't
 * replied yet.
 * 
 * STATE_REJECTED_PENDING: The user rejected an incoming invitation, but the
 * buddy wasn't online to actually send the reply message so it's pending and
 * will be sent as soon as the buddy is online.
 *
 * STATE_REJECTED: The buddy replied and rejected the invitation.  This
 * information should be kept around so we don't automatically resend the
 * invitation.
 *
 * STATE_ACCEPTED_PENDING: The buddy replied but Simias is not running so we
 * cannot update Simias.  When Simias returns to an online state, the code
 * should loop through all of these events and sync up this information.
 *
 * STATE_ACCEPTED: The buddy has accepted the invitation and we've informed
 * Simias with the information (IP Address) received from the buddy.
 * 
 * States used for incoming invitations:
 * 
 * 		STATE_NEW, STATE_REJECTED_PENDING, STATE_ACCEPTED_PENDING
 * 
 * States used for outgoing invitations:
 * 
 * 		STATE_PENDING, STATE_SENT, STATE_REJECTED, STATE_ACCEPTED_PENDING,
 * 		STATE_ACCEPTED
 */
typedef enum
{
	STATE_NEW,
	STATE_PENDING,
	STATE_SENT,
	STATE_REJECTED_PENDING,
	STATE_REJECTED,
	STATE_ACCEPTED_PENDING,
	STATE_ACCEPTED
} INVITATION_STATE;

enum
{
	INVITATION_TYPE_ICON_COL,
	BUDDY_NAME_COL,
	TIME_COL,
	COLLECTION_NAME_COL,
	STATE_COL,
	INVITATION_PTR,
	N_COLS
};

enum
{
	TRUSTED_BUDDY_ICON_COL,
	TRUSTED_BUDDY_NAME_COL,
	TRUSTED_BUDDY_IP_ADDR_COL,
	TRUSTED_BUDDY_IP_PORT_COL,
	GAIM_ACCOUNT_PTR_COL,
	N_TRUSTED_BUDDY_COLS
};

/****************************************************
 * Data Structures                                  *
 ****************************************************/

typedef struct
{
	GaimAccount *gaim_account;
	char buddy_name[128];
	INVITATION_STATE state;
	time_t time;
	char collection_id[64];
	char collection_type[32];
	char collection_name[128];
	char ip_addr[16];
	char ip_port[16];
} Invitation;

/****************************************************
 * Global Variables                                 *
 ****************************************************/
static GtkWidget *invitations_dialog;

static GtkWidget *in_inv_tree = NULL;
static GtkListStore *in_inv_store = NULL;
static GtkWidget *in_inv_accept_button = NULL;
static GtkWidget *in_inv_reject_button = NULL;
static gboolean in_inv_safe_to_write = FALSE;

static GtkWidget *out_inv_tree = NULL;
static GtkListStore *out_inv_store = NULL;
static GtkWidget *out_inv_resend_button = NULL;
static GtkWidget *out_inv_cancel_button = NULL;
static GtkWidget *out_inv_remove_button = NULL;
static gboolean out_inv_safe_to_write = FALSE;

static GtkListStore *trusted_buddies_store = NULL;
static gboolean trusted_buddies_safe_to_write = FALSE;

/****************************************************
 * Forward Declarations                             *
 ****************************************************/
 
/**
 * FIXME: Figure out a good way to separate up these files complete with both
 * header (.h) and implementation files (.c).  The length of this file is
 * getting out of control.  Also, I'm sure it'd be possible to collapse a lot
 * of functions so there isn't so much duplication of code everywhere.
 */
static int send_msg_to_buddy(GaimBuddy *recipient, char *msg);
static int send_invitation_request_msg(GaimBuddy *recipient,
									   char *collection_id,
									   char *collection_type,
									   char *collection_name);
static int send_invitation_request_deny_msg(GaimBuddy *recipient,
											char *collection_id);
static int send_invitation_request_accept_msg(GaimBuddy *recipient,
											  char *collection_id);
static int send_ping_request_msg(GaimBuddy *recipient);
static int send_ping_response_msg(GaimBuddy *recipient);

static SIMIAS_MSG_TYPE get_possible_simias_msg_type(const char *buffer);

static void buddylist_cb_simulate_share_collection(GaimBlistNode *node,
												gpointer user_data);

static void invitations_dialog_close_button_cb(GtkWidget *widget,
					       int response,
					       gpointer user_data);
static void invitations_dialog_destroy_cb(GtkWidget *widget, GdkEvent *event,
										  gpointer user_data);
static void in_inv_accept_button_cb(GtkWidget *w, GtkTreeView *tree);
static void in_inv_reject_button_cb(GtkWindow *w, GtkTreeView *tree);
static void in_inv_sel_changed_cb(GtkTreeSelection *sel, GtkTreeView *tree);

static void out_inv_resend_button_cb(GtkWindow *w, GtkTreeView *tree);
static void out_inv_cancel_button_cb(GtkWindow *w, GtkTreeView *tree);
static void out_inv_remove_button_cb(GtkWindow *w, GtkTreeView *tree);
static void out_inv_sel_changed_cb(GtkTreeSelection *sel, GtkTreeView *tree);

static char * fill_time_str(char *time_str, int buf_len, time_t time);
static char * fill_state_str(char *state_str, INVITATION_STATE state);

static void add_invitation_to_store(GtkListStore *store,
									Invitation *invitation);
static void init_default_prefs();
static gboolean write_invitations_file(FILE *file, GtkListStore *store);
static void simias_invitations_write(GtkListStore *store);
static gboolean invitations_read(GtkListStore *store, const char *filename);
static void load_invitations_from_file(GtkListStore *store, const char *name);
static void init_invitation_stores();
static void add_new_trusted_buddy(GtkListStore *store, GaimBuddy *buddy,
									char *ip_address, char *ip_port);
static gboolean write_trusted_buddies_file(FILE *file, GtkListStore *store);
static void save_trusted_buddies(GtkListStore *store);
static gboolean trusted_buddies_read(GtkListStore *store, const char *filename);
static void load_trusted_buddies_from_file(GtkListStore *store, const char *name);
static void init_trusted_buddies_store();
static void init_invitations_window();
static void show_invitations_window();
static void buddylist_cb_show_invitations_window(GaimBlistNode *node,
												 gpointer user_data);

static void blist_add_context_menu_items_cb(GaimBlistNode *node, GList **menu);

static gboolean is_valid_ip_part(const char *ip_part);
static int length_of_ip_address(const char *buffer);

static gboolean receiving_im_msg_cb(GaimAccount *account, char **sender,
									char **buffer, int *flags, void *data);

static gboolean lookup_collection_in_store(GtkListStore *store,
											char *collection_id,
											GtkTreeIter *iter);

static gboolean lookup_trusted_buddy(GtkListStore *store,
									 GaimBuddy *buddy,
									 GtkTreeIter *iter);

static gboolean handle_invitation_request(GaimAccount *account,
										  const char *sender,
										  const char *buffer);
static gboolean handle_invitation_request_deny(GaimAccount *account,
											   const char *sender,
											   const char *buffer);
static gboolean handle_invitation_request_accept(GaimAccount *account,
												 const char *sender,
												 const char *buffer);
static gboolean handle_ping_request(GaimAccount *account,
									const char *sender,
									const char *buffer);
static gboolean handle_ping_response(GaimAccount *account,
									 const char *sender,
									 const char *buffer);

static void buddy_signed_on_cb(GaimBuddy *buddy, void *user_data);

/****************************************************
 * Function Implementations                         *
 ****************************************************/

/**
 * This function takes a generic message and sends it to the specified recipient
 * if they are online.
 * 
 * FIXME: We may have to prevent a "flood" of messages for a given account in case that some IM servers will kick us off for "abuse".
 */
static int
send_msg_to_buddy(GaimBuddy *recipient, char *msg)
{
	GaimConnection *conn;

	/**
	 * Make sure the buddy is online.  More information on the "present" field
	 * can be seen here: http://gaim.sourceforge.net/api/struct__GaimBuddy.html
	 */
	if (recipient->present == GAIM_BUDDY_SIGNING_OFF
		|| recipient->present == GAIM_BUDDY_OFFLINE) {
		return recipient->present; /* Buddy is signing off or offline */
	}
	
	conn = gaim_account_get_connection(recipient->account);
	
	if (!conn) {
		return -1; /* Can't send a msg without a connection */
	}

	g_print("Sending message: %s\n", msg);
	return serv_send_im(conn, recipient->name, msg, 0);
}

/**
 * This function will send a message with the following format:
 * 
 * [simias:invitation-request:<ip-address>:<ip-port><collection-id>:<collection-type>:<collection-name>]<Human-readable Invitation String for buddies who don't have the plugin installed/enabled>
 */
static int
send_invitation_request_msg(GaimBuddy *recipient, char *collection_id,
							char *collection_type, char *collection_name)
{
	char msg[2048];
	const char *public_ip;
	char *ip_port = "5432";	/* Get the WebService port from Simias */
	
	public_ip = gaim_network_get_my_ip(-1);

	sprintf(msg, "%s%s:%s:%s:%s:%s] %s",
			INVITATION_REQUEST_MSG,
			public_ip,
			ip_port,
			collection_id,
			collection_type,
			collection_name,
			_("I'd like to share an iFolder with you through Gaim but you don't have the iFolder Plugin installed/enabled.  You can download it from http://www.ifolder.com/.  Let me know when you've installed and enabled it and I will re-invite you to share my iFolder."));

	return send_msg_to_buddy(recipient, msg);
}

/**
 * This function will send a message with the following format:
 * 
 * [simias:invitation-request-deny:<collection-id>]
 */
static int
send_invitation_request_deny_msg(GaimBuddy *recipient, char *collection_id)
{
	char msg[2048];
	
	sprintf(msg, "%s%s]", INVITATION_REQUEST_DENY_MSG, collection_id);

	return send_msg_to_buddy(recipient, msg);
}

/**
 * This function will send a message with the following format:
 * 
 * 
 * [simias:invitation-request-accept:<collection-id>:<ip-address>:<ip-port>]
 */
static int
send_invitation_request_accept_msg(GaimBuddy *recipient, char *collection_id)
{
	char msg[2048];
	const char *public_ip;
	char *ip_port = "4321"; /* FIXME: Get WebService port from Simias */
	
	public_ip = gaim_network_get_my_ip(-1);
	
	sprintf(msg, "%s%s:%s:%s]",
			INVITATION_REQUEST_ACCEPT_MSG,
			collection_id,
			public_ip,
			ip_port);

	return send_msg_to_buddy(recipient, msg);
}

/**
 * This function will send a message with the following format:
 * 
 * [simias:ping-request:<ip-address>:<ip-port>] <Human-readable message>
 */
static int
send_ping_request_msg(GaimBuddy *recipient)
{
	char msg[2048];
	const char *public_ip;
	char *ip_port = "1234";	/* FIXME: Get the WebService port from Simias */
	
	public_ip = gaim_network_get_my_ip(-1);
	
	sprintf(msg, "%s%s:%s] %s", PING_REQUEST_MSG, public_ip, ip_port,
		_("You are seeing this message because you do not have the Gaim iFolder Plugin installed/enabled.  Please visit http://www.ifolder.com/ to download the plugin."));

	return send_msg_to_buddy(recipient, msg);
}

/**
 * This function will send a message with the following format:
 * 
 * [simias:ping-response:<ip-address>:<ip-port>]
 */
static int
send_ping_response_msg(GaimBuddy *recipient)
{
	char msg[2048];
	const char *public_ip;
	const char *ip_port = "1234"; /* FIXME: Get the WebService port from Simias */
	
	public_ip = gaim_network_get_my_ip(-1);
	
	sprintf(msg, "%s%s:%s]", PING_RESPONSE_MSG, public_ip, ip_port);

	return send_msg_to_buddy(recipient, msg);
}

/**
 * Parse the first part of the buffer and if it matches the #defined type,
 * return the type, otherwise, return 0 (type unknown).
 * 
 * Note: The buffer passed to this function should be stripped of any HTML.
 */
static SIMIAS_MSG_TYPE
get_possible_simias_msg_type(const char *buffer)
{
	if (strstr(buffer, INVITATION_REQUEST_MSG) == buffer) {
		return INVITATION_REQUEST;
	} else if (strstr(buffer, INVITATION_REQUEST_DENY_MSG) == buffer) {
		return INVITATION_REQUEST_DENY;
	} else if (strstr(buffer, INVITATION_REQUEST_ACCEPT_MSG) == buffer) {
		return INVITATION_REQUEST_ACCEPT;
	} else if (strstr(buffer, PING_REQUEST_MSG) == buffer) {
		return PING_REQUEST;
	} else if (strstr(buffer, PING_RESPONSE_MSG) == buffer) {
		return PING_RESPONSE;
	} else {
		return 0;
	}
}

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
	icon_path = g_build_filename(gaim_user_dir(), "ifolder48.png", NULL);
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
	icon_path = g_build_filename(gaim_user_dir(), "glyphmarks48.png", NULL);
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

			result = send_invitation_request_msg(buddy, guid, collection_type,
												(char *)name);
			g_print("send_invitation_request_msg(): %d\n", result);
			if (result > 0) {
				/* The message was sent */
				invitation->state = STATE_SENT;
			}
			
			add_invitation_to_store(out_inv_store, invitation);
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
	if (lookup_trusted_buddy(trusted_buddies_store, buddy, &tb_iter)) {
		/* Update the trusted buddy info */
		gtk_list_store_set(trusted_buddies_store, &tb_iter,
							TRUSTED_BUDDY_IP_ADDR_COL, invitation->ip_addr,
							TRUSTED_BUDDY_IP_PORT_COL, invitation->ip_port,
							-1);

		/* Update the trusted buddies file */
		save_trusted_buddies(trusted_buddies_store);
	} else {
		/* Add a new trusted buddy */
		add_new_trusted_buddy(trusted_buddies_store, buddy, 
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
		fill_time_str(time_str, 32, invitation->time);

		/* Format the state string */
		fill_state_str(state_str, invitation->state);
	
		/* Update the out_inv_store */
		gtk_list_store_set(GTK_LIST_STORE(model), &iter,
			TIME_COL,				time_str,
			STATE_COL,				state_str,
			-1);
		
		/* Save the updates to a file */
g_print("about_to_write 1\n");
		simias_invitations_write(GTK_LIST_STORE(model));

		/* Make sure the buttons are in the correct state */
		in_inv_sel_changed_cb(sel, GTK_TREE_VIEW(in_inv_tree));
	
		return; /* That's about all we can do at this point */
	}

	send_result =
		send_invitation_request_accept_msg(buddy, invitation->collection_id);
		
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
	simias_invitations_write(GTK_LIST_STORE(model));

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
		fill_time_str(time_str, 32, invitation->time);

		/* Format the state string */
		fill_state_str(state_str, invitation->state);
	
		/* Update the out_inv_store */
		gtk_list_store_set(GTK_LIST_STORE(model), &iter,
			TIME_COL,				time_str,
			STATE_COL,				state_str,
			-1);

		/* Save the updates to a file */
g_print("about_to_write 2\n");
		simias_invitations_write(GTK_LIST_STORE(model));

		/* Make sure the buttons are in the correct state */
		in_inv_sel_changed_cb(sel, GTK_TREE_VIEW(in_inv_tree));
	
		return; /* That's about all we can do at this point */
	}

	send_result =
		send_invitation_request_deny_msg(buddy, invitation->collection_id);
		
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
	simias_invitations_write(GTK_LIST_STORE(model));
}

static void
in_inv_sel_changed_cb(GtkTreeSelection *sel, GtkTreeView *tree)
{
	GtkTreeIter iter;
	GtkTreeModel *model;
	Invitation *invitation;

	/* If nothing is selected, disable the buttons. */
	if (!gtk_tree_selection_get_selected(sel, &model, &iter)) {
		g_print("in_inv_sel_changed_cb() called and nothing is selected.  Disabling buttons...\n");

		/* Disable the buttons */
		gtk_widget_set_sensitive(in_inv_accept_button, FALSE);
		gtk_widget_set_sensitive(in_inv_reject_button, FALSE);
	} else {
		g_print("in_inv_sel_changed_cb() called and something is selected.\n");

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
		fill_time_str(time_str, 32, invitation->time);

		/* Format the state string */
		fill_state_str(state_str, invitation->state);
	
		/* Update the out_inv_store */
		gtk_list_store_set(GTK_LIST_STORE(model), &iter,
			TIME_COL,				time_str,
			STATE_COL,				state_str,
			-1);
		
		/* Save the updates to a file */
g_print("about_to_write 3\n");
		simias_invitations_write(GTK_LIST_STORE(model));

		return; /* That's about all we can do at this point */
	}

	send_result =
		send_invitation_request_msg(buddy, invitation->collection_id,
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
	fill_time_str(time_str, 32, invitation->time);

	/* Format the state string */
	fill_state_str(state_str, invitation->state);

	/* Update the out_inv_store */
	gtk_list_store_set(GTK_LIST_STORE(model), &iter,
		TIME_COL,				time_str,
		STATE_COL,				state_str,
		-1);

	/* Save the updates to a file */
g_print("about_to_write 4\n");
	simias_invitations_write(GTK_LIST_STORE(model));
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
		simias_invitations_write(GTK_LIST_STORE(model));
	} else {
		g_print("out_inv_remove_button_cb() called on an invitation that is not in the STATE_ACCEPTED or STATE_REJECTED state\n");
	}
}

static void
out_inv_sel_changed_cb(GtkTreeSelection *sel, GtkTreeView *tree)
{
	/**
	 * Disable the buttons if no invitation is selected.
	 */
	GtkTreeIter iter;
	GtkTreeModel *model;
	Invitation *invitation;

	if (!gtk_tree_selection_get_selected(sel, &model, &iter)) {
		g_print("out_inv_sel_changed_cb() called and nothing is selected.  Disabling buttons...\n");

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

static char *
fill_time_str(char *time_str, int buf_len, time_t time)
{
	struct tm *time_ptr;
	
	time_ptr = localtime(&time);
	strftime(time_str, buf_len, TIMESTAMP_FORMAT, time_ptr);
	
	return time_str;
}

static char *
fill_state_str(char *state_str, INVITATION_STATE state)
{
	switch (state) {
		case STATE_NEW:
			sprintf(state_str, _("New"));
			break;
		case STATE_PENDING:
			sprintf(state_str, _("Pending"));
			break;
		case STATE_SENT:
			sprintf(state_str, _("Sent"));
			break;
		case STATE_REJECTED_PENDING:
			sprintf(state_str, _("Rejected (Pending)"));
			break;
		case STATE_REJECTED:
			sprintf(state_str, _("Rejected"));
			break;
		case STATE_ACCEPTED_PENDING:
			sprintf(state_str, _("Accepted (Pending)"));
			break;
		case STATE_ACCEPTED:
			sprintf(state_str, _("Accepted"));
			break;
		default:
			sprintf(state_str, _("N/A"));
	}
	
	return state_str;
}

/**
 * This function adds the elements of the Invitation object to the
 * store and also saves the Invitation as one of the items also.
 */
static void
add_invitation_to_store(GtkListStore *store, Invitation *invitation)
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

g_print("add_invitation_to_store() entered\n");
	if (!strcmp(invitation->collection_type, COLLECTION_TYPE_IFOLDER)) {
		icon_path = g_build_filename(gaim_user_dir(), "ifolder48.png", NULL);
	} else if (!strcmp(invitation->collection_type, COLLECTION_TYPE_GLYPHMARKS)) {
		icon_path = g_build_filename(gaim_user_dir(), "glyphmarks48.png", NULL);
	}

	if (icon_path) {
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
	fill_time_str(time_str, 32, invitation->time);

	/* Format the state string */
	fill_state_str(state_str, invitation->state);
	
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
	simias_invitations_write(store);

	if (invitation_icon)
		g_object_unref(invitation_icon);
}

/**
 * If the given preferences don't exist, create them with a default value.
 */
static void
init_default_prefs()
{
	gaim_prefs_add_none(SIMIAS_PREF_PATH);
	if (!gaim_prefs_exists(SIMIAS_PREF_NOTIFY_RECEIVE_NEW_INVITATIONS)) {
		gaim_prefs_add_bool(SIMIAS_PREF_NOTIFY_RECEIVE_NEW_INVITATIONS,
							SIMIAS_PREF_NOTIFY_RECEIVE_NEW_INVITATIONS_DEF);
	}

	if (!gaim_prefs_exists(SIMIAS_PREF_NOTIFY_ACCEPT_INVITATIONS)) {
		gaim_prefs_add_bool(SIMIAS_PREF_NOTIFY_ACCEPT_INVITATIONS,
							SIMIAS_PREF_NOTIFY_ACCEPT_INVITATIONS_DEF);
	}

	if (!gaim_prefs_exists(SIMIAS_PREF_NOTIFY_REJECT_INVITATIONS)) {
		gaim_prefs_add_bool(SIMIAS_PREF_NOTIFY_REJECT_INVITATIONS,
							SIMIAS_PREF_NOTIFY_REJECT_INVITATIONS_DEF);
	}

	if (!gaim_prefs_exists(SIMIAS_PREF_NOTIFY_ERRORS)) {
		gaim_prefs_add_bool(SIMIAS_PREF_NOTIFY_ERRORS,
							SIMIAS_PREF_NOTIFY_ERRORS_DEF);
	}
	
	if (!gaim_prefs_exists(SIMIAS_PREF_REDISCOVER_IP_ADDRS)) {
		gaim_prefs_add_bool(SIMIAS_PREF_REDISCOVER_IP_ADDRS,
							SIMIAS_PREF_REDISCOVER_IP_ADDRS_DEF);
	}

	if (!gaim_prefs_exists(SIMIAS_PREF_SIMIAS_AUTO_START)) {
		gaim_prefs_add_bool(SIMIAS_PREF_SIMIAS_AUTO_START,
							SIMIAS_PREF_SIMIAS_AUTO_START_DEF);
	}
}

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
		fill_time_str(time_str, 32, invitation->time);
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

/**
 * FIXME: Need to put this function in a thread or something so that the messages
 * to write to the file just queue up the requests and only write if it's been
 * at least 5 seconds or so...so that we're not just writing all the time.
 */
static void
simias_invitations_write(GtkListStore *store)
{
	FILE *file;
	struct stat st;
	char *user_dir = gaim_user_dir();
	char *filename;
	char filename_save[512];
	char *filename_real;
	
	gaim_debug(GAIM_DEBUG_INFO, "simias",
			   "simias_invitations_write() called on: %s\n",
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
			
			add_invitation_to_store(store, invitation);
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

/**
 * When Gaim first starts up, load the invitation information from a data file
 * and populate the in_inv_store and out_inv_store.
 */
static void
init_invitation_stores()
{
g_print("init_invitation_stores() entered\n");
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

static void
add_new_trusted_buddy(GtkListStore *store, GaimBuddy *buddy,
						char *ip_address, char *ip_port)
{
	GtkTreeIter iter;
	GdkPixbuf *buddy_icon;

g_print("add_new_trusted_buddy() called: %s (%s:%s)\n", buddy->name, ip_address, ip_port);

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

	/* Add/Update this trusted buddy in Simias */
	gaim_domain_add_gaim_buddy(gaim_account_get_name(buddy->account),
							   gaim_account_get_protocol_id(buddy->account),
							   buddy->name,
							   gaim_buddy_get_alias(buddy),
							   ip_address,
							   ip_port);

	/* Update the trusted buddies file */
	save_trusted_buddies(trusted_buddies_store);
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

static void
save_trusted_buddies(GtkListStore *store)
{
	FILE *file;
	struct stat st;
	char *user_dir = gaim_user_dir();
	char *filename;
	char filename_save[512];
	char *filename_real;
	
	gaim_debug(GAIM_DEBUG_INFO, "simias",
			   "save_trusted_buddies() called\n");

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

			add_new_trusted_buddy(store,
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

/**
 * When the plugin first loads (when Gaim starts or the user enables this
 * plugin), fill the trusted_buddies_store with the information persited in the
 * trusted_buddies_file.
 */
static void
init_trusted_buddies_store()
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
		G_CALLBACK(in_inv_sel_changed_cb), in_inv_tree);

	g_signal_connect(G_OBJECT(out_inv_resend_button), "clicked",
		G_CALLBACK(out_inv_resend_button_cb), out_inv_tree);
	g_signal_connect(G_OBJECT(out_inv_cancel_button), "clicked",
		G_CALLBACK(out_inv_cancel_button_cb), out_inv_tree);
	g_signal_connect(G_OBJECT(out_inv_remove_button), "clicked",
		G_CALLBACK(out_inv_remove_button_cb), out_inv_tree);
	g_signal_connect(G_OBJECT(out_inv_sel), "changed",
		G_CALLBACK(out_inv_sel_changed_cb), out_inv_tree);
}

/**
 * Show the Invitations Dialog
 */
static void
show_invitations_window()
{
	gtk_widget_show_all(invitations_dialog);
}

/**
 * Open and show the Simias Invitations Window
 */
static void
buddylist_cb_show_invitations_window(GaimBlistNode *node, gpointer user_data)
{
	show_invitations_window();
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

/**
 * This function takes a part of an IP Address and validates that it is a number
 * AND that it is a number >= 0 and < 255.
 */
static gboolean
is_valid_ip_part(const char *ip_part)
{
	long int num;
	char *error_ptr = NULL;

	g_print("is_valid_ip_part(\"%s\") entered\n", ip_part);
	num = strtol(ip_part, &error_ptr, 10);
	if (error_ptr && error_ptr[0] != '\0') {
		g_print("strtol() (%d) failed because error_ptr was NOT NULL and NOT an empty string\n", (int) num);
		return FALSE;
	}

	if (num >= 0 && num < 255) {
		g_print("is_valid_ip_part() returning TRUE\n");
		return TRUE;
	} else {
		g_print("is_valid_ip_part() returning FALSE\n");
		return FALSE;
	}
}

/**
 * This function takes a buffer and parses for an IP address.
 *
 * The original buffer will be something like:
 *
 *      [simias:<message type>:xxx.xxx.xxx.xxx[':' or ']'<Any number of characters can be here>
 *                             ^
 *
 * This function receives the buffer starting with the character
 * marked with '^' in the above examples.
 *
 * This function returns the length of the IP Address (the char *) before a ':'
 * or ']' character or -1 if the message does not contain a string that
 * resembles an IP Address.
 */
static int
length_of_ip_address(const char *buffer)
{
	char *possible_ip_addr;
	char *part1, *part2, *part3, *part4;
	int possible_length;

	g_print("length_of_ip_address() called with: %s\n", buffer);

	/* Buffer must be at least "x.x.x.x]" (8 chars long) */
	if (strlen(buffer) < 8) {
		g_print("Buffer is not long enough to contain an IP Address\n");
		return -1; /* Not a valid message */
	}

	/* The IP Address must be followed by a ':' or ']' */
	possible_ip_addr = strtok((char *)buffer, ":]");
	if (!possible_ip_addr) {
		g_print("Buffer did not contain a ':' or ']' character\n");
		return -1;
	}

	/**
	 * An IP Address has to be at least "x.x.x.x" (7 chars long)
	 * but no longer than "xxx.xxx.xxx.xxx" (15 chars long).
	 */
	possible_length = strlen(possible_ip_addr);
	if (possible_length < 7 || possible_length > 15) {
		g_print("Buffer length was less than 7 or greater than 15\n");
		return -1;
	}

	/* Now verify that all the parts of an IP address exist */
	part1 = strtok(possible_ip_addr, ".");
	if (!part1 || !is_valid_ip_part(part1)) {
		g_print("Part 1 invalid\n");
		return -1;
	}

	part2 = strtok(NULL, ".");
	if (!part2 || !is_valid_ip_part(part2)) {
		g_print("Part 2 invalid\n");
		return -1;
	}

	part3 = strtok(NULL, ".");
	if (!part3 || !is_valid_ip_part(part3)) {
		g_print("Part 3 invalid\n");
		return -1;
	}

	part4 = strtok(NULL, ".");
	if (!part4 || !is_valid_ip_part(part4)) {
		g_print("Part 4 invalid\n");
		return -1;
	}

	g_print("length_of_ip_address() returning: %d\n", possible_length);
	/**
	 * If the code makes it to this point, possible_ip_addr looks
	 * like a valid IP Address (Note: this is not an exhaustive test).
	 */
	return possible_length;
}

/**
 * This function will be called any time Gaim starts receiving an instant
 * message.  This gives our plugin the ability to jump in and check to see if
 * it is a special message sent by another buddy who is also using this plugin.
 * If it IS a special Simias message, we intercept it and prevent it from being
 * displayed to the user.
 */
static gboolean
receiving_im_msg_cb(GaimAccount *account, char **sender, char **buffer,
					int *flags, void *data)
{
	SIMIAS_MSG_TYPE possible_msg_type;
	char *html_stripped_buffer;
	gboolean b_simias_msg_handled;
	
	b_simias_msg_handled = FALSE;

	g_print("Receiving message on account: %s\n",
		gaim_account_get_username(account));
	g_print("Sender: %s\n", *sender);

	/**
	 * Examine this message to see if it's a special Simias Gaim message.  If it
	 * is, extract the message and take the appropriate action.  Also, if it is
	 * one of these special messages, return TRUE from this function so no other
	 * module in Gaim gets the message.
	 */
	html_stripped_buffer = gaim_markup_strip_html(*buffer);
	if (!html_stripped_buffer) {
		/**
		 * Couldn't strip the buffer of HTML.  It's likely that it's not a
		 * special Simias message if this is the case.  Return FALSE to allow
		 * the message to be passed on.
		 */
		return FALSE;
	}

	possible_msg_type = get_possible_simias_msg_type(html_stripped_buffer);

	switch (possible_msg_type) {
		case INVITATION_REQUEST:
			b_simias_msg_handled =
				handle_invitation_request(account, *sender, 
										  html_stripped_buffer);
			break;
		case INVITATION_REQUEST_DENY:
			b_simias_msg_handled = 
				handle_invitation_request_deny(account, *sender,
											   html_stripped_buffer);
			break;
		case INVITATION_REQUEST_ACCEPT:
			b_simias_msg_handled = 
				handle_invitation_request_accept(account, *sender,
												 html_stripped_buffer);
			break;
		case PING_REQUEST:
			b_simias_msg_handled = 
				handle_ping_request(account, *sender, html_stripped_buffer);
			break;
		case PING_RESPONSE:
			b_simias_msg_handled = 
				handle_ping_response(account, *sender, html_stripped_buffer);
			break;
		default:
			break;
	}
	
	g_free(html_stripped_buffer);
	
	if (b_simias_msg_handled) {
		return TRUE;	/* Prevent the message from passing through */
	} else {
		return FALSE;	/* The message wasn't a Simias Message */
	}
}

/**
 * This function walks the GtkListStore and looks for the collection_id inside
 * the G_TYPE_POINTER column that points to an Invitation *.  If a match is
 * found, iter will be returned pointing to this row AND the function will
 * return TRUE.  If no match is found, it will return FALSE;
 */
static gboolean
lookup_collection_in_store(GtkListStore *store, char *collection_id,
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
static gboolean
lookup_trusted_buddy(GtkListStore *store, GaimBuddy *buddy, GtkTreeIter *iter)
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

/**
 * This fuction checks to see if this is a properly formatted Simias Invitation
 * Request and then notifies the user of an incoming invitation.  The user can
 * then accept or deny the request at will.
 * 
 * Ideally, we should not cause a popup window to appear to the user, but just
 * a little notification bubble that would appear for a few seconds and then
 * disappear.  The user should be able to go the Invitations Window to attend to
 * incoming and outgoing invitations.
 * 
 * This is a candidate for a plugin setting (i.e., whether the user wants to be
 * notified/see a popup/bubble about incoming invitations).
 * 
 * Additionally, we could choose to pass the notification process right on to
 * the Simias/iFolder Client and just show incoming and outgoing invitations in
 * the Invitation Window.
 * 
 * If the request message has enough information, perhaps the invitation
 * response messages aren't even necessary and the user can start synchronizing
 * the collection immediately.  The invitation request could just be something
 * that adds an available collection to the user's list of collections.
 */
static gboolean
handle_invitation_request(GaimAccount *account, const char *sender, 
						  const char *buffer)
{
	/**
	 * Since this method is called, we already know that the first part of
	 * the message matches our #define.  So, because of that, we can take
	 * that portion out of the picture and start tokenizing the different
	 * parts.
	 */
	char *sender_ip_address;
	char *sender_ip_port;
	char *collection_id;
	char *collection_type;
	char *collection_name;
	Invitation *invitation;
	GtkTreeIter iter;
	char time_str[32];
	char state_str[32];
	
g_print("handle_invitation_request() entered\n");
	/**
	 * Start parsing the message at this point:
	 * 
	 * 	[simias:invitation-request:<ip-address>:<ip-port>:<collection-id>:<collection-type>:<colletion-name>]
	 *                             ^
	 */
	sender_ip_address = strtok((char *) buffer + strlen(INVITATION_REQUEST_MSG), ":");
	if (!sender_ip_address) {
		g_print("handle_invitation_request() couldn't parse the sender-ip-address\n");
		return FALSE;
	}
	
	sender_ip_port = strtok(NULL, ":");
	if (!sender_ip_port) {
		g_print("handle_invitation_request() couldn't parse the ip-port\n");
		return FALSE;
	}
	
	collection_id = strtok(NULL, ":");
	if (!collection_id) {
		g_print("handle_invitation_request() couldn't parse the collection-id\n");
		return FALSE;
	}

	collection_type = strtok(NULL, ":");
	if (!collection_type) {
		g_print("handle_invitation_request() couldn't parse the collection-type\n");
		return FALSE;
	}

	collection_name = strtok(NULL, "]");
	if (!collection_name) {
		g_print("handle_invitation_request() couldn't parse the collection-name\n");
		return FALSE;
	}
	
	/**
	 * Check to see if we already have this invitation in our list (based on the
	 * collection-id) and if we do, update the invitation received time and
	 * notify the user of an incoming invitation (notify bubble or invitation
	 * window show)
	 */
	if (lookup_collection_in_store(in_inv_store, collection_id, &iter)) {
		/* Extract the Invitation * from the model using iter */
		gtk_tree_model_get(GTK_TREE_MODEL(in_inv_store), &iter,
							INVITATION_PTR, &invitation,
							-1);
		/* Update the invitation time */
		time(&(invitation->time));
		
		/* Format the time to a string */
		fill_time_str(time_str, 32, invitation->time);
		
		invitation->state = STATE_NEW;
		fill_state_str(state_str, invitation->state);
	
		/* Update the out_inv_store */
		gtk_list_store_set(in_inv_store, &iter,
			TIME_COL,				time_str,
			STATE_COL,				state_str,
			-1);

		/* Save the updates to a file */
g_print("about_to_write 7\n");
		simias_invitations_write(in_inv_store);
	} else {
		/**
		 * Construct an Invitation, fill it with information, add it to the
		 * in_inv_store, and show the Invitations Dialog.
		 */
		invitation = malloc(sizeof(Invitation));
		if (!invitation) {
			g_print("out of memory in handle_invitation_request()\n");
			return TRUE; /* The message must be discarded */
		}
	
		invitation->gaim_account = account;
		sprintf(invitation->buddy_name, sender);
		invitation->state = STATE_NEW;
		
		/* Get the current time to store as the received time */
		time(&(invitation->time));
		
		sprintf(invitation->collection_id, collection_id);
		sprintf(invitation->collection_type, collection_type);
		sprintf(invitation->collection_name, collection_name);
		sprintf(invitation->ip_addr, sender_ip_address);
		sprintf(invitation->ip_port, sender_ip_port);
		
		/* Now add this new invitation to the in_inv_store. */
		add_invitation_to_store(in_inv_store, invitation);
	}
	
	/* FIXME: Change this to a tiny bubble notification instead of popping up the big Invitations Dialog */
	/* Check the notify preference */
	if (gaim_prefs_get_bool(SIMIAS_PREF_NOTIFY_RECEIVE_NEW_INVITATIONS)) {
		show_invitations_window();
	}
	
	return TRUE;	/* Message was handled correctly */
}

/**
 * This function checks to see if the buffer is a properly formatted deny
 * message and handles it appropriately.
 * 
 * Don't do anything with this message if any of the following are true:
 * 
 * 		1. There is no "pending invitation" in our outgoing invitation list
 * 		2. If Simias is up and running and we can see that the collection in
 * 		   question doesn't exist.
 * 		3. If Simias is up and running and we can tell that the sender is not
 * 		   member of the collection.
 * 
 * If the denies the invitation, mark the status of the invitation object that's
 * in the outgoing invitation list as rejected.  If the user deletes the
 * invitation from the outgoing invitation list after it's been marked as
 * rejected, ask the user whether they want to remomve that buddy from the
 * actual member list in Simias.
 */
static gboolean
handle_invitation_request_deny(GaimAccount *account,
							   const char *sender,
							   const char *buffer)
{
	GtkTreeIter iter;
	Invitation *invitation = NULL;
	char time_str[32];
	char state_str[32];

	/**
	 * Since this method is called, we already know that the first part of
	 * the message matches our #define.  So, because of that, we can take
	 * that portion out of the picture and start tokenizing the different
	 * parts.
	 */
	char *collection_id;
	
	/**
	 * Start parsing the message at this point:
	 * 
	 * 	[simias:invitation-request-deny:<collection-id>]
	 *                                  ^
	 */
	collection_id = strtok((char *) buffer + strlen(INVITATION_REQUEST_DENY_MSG), "]");
	if (!collection_id) {
		g_print("handle_invitation_request_deny() couldn't parse the collection-id\n");
		return FALSE;
	}
	
	/**
	 * Lookup the collection_id in the current out_inv_store.  If it's not there
	 * we'll just discard this message.  If it IS there, we need to update the
	 * status of the invitation and take more action with Simias.
	 */
	if (!lookup_collection_in_store(out_inv_store, collection_id, &iter)) {
		g_print("handle_invitation_request_deny() couldn't find the collection-id in out_inv_store\n");
		/* FIXME: Before returning from here, we should try to retrieve more information from Simias in case the user deleted this invitation information from Gaim */
		return TRUE; /* Discard the message */
	}
	
	/**
	 * If we get this far, iter now points to the row of data in the store model
	 * that contains the Invitation * corresponding with this message.
	 * 
	 * Update the time and the invitation state in the store/model.
	 */

	/* Extract the Invitation * out of the model */
	gtk_tree_model_get(GTK_TREE_MODEL(out_inv_store), &iter,
						INVITATION_PTR, &invitation,
						-1);

	/**
	 * Double-check to make sure nothing has changed since returning from the
	 * lookup call.  If it did, call this function recursively.  If the row
	 * was actually removed, it should stop processing in the recursive call.
	 */
	if (strcmp(invitation->collection_id, collection_id) != 0) {
		/* The row changed */
		return handle_invitation_request_deny(account, sender, buffer);
	}
	
	/* Update the invitation time */
	time(&(invitation->time));
	
	/* Update the invitation state */
	invitation->state = STATE_REJECTED;
	
	/* Format the time to a string */
	fill_time_str(time_str, 32, invitation->time);

	/* Format the state string */
	fill_state_str(state_str, invitation->state);
	
	/* Update the out_inv_store */
	gtk_list_store_set(out_inv_store, &iter,
		TIME_COL,				time_str,
		STATE_COL,				state_str,
		-1);

	/* Save the updates to a file */
g_print("about_to_write 8\n");
	simias_invitations_write(out_inv_store);

	/* Make sure the buttons are in the correct state */
	out_inv_sel_changed_cb(
		gtk_tree_view_get_selection(GTK_TREE_VIEW(out_inv_tree)),
		GTK_TREE_VIEW(out_inv_tree));
	
	/* FIXME: Add more interaction with Simias as described in the notes of the function */

	/* FIXME: Change this to a tiny bubble notification instead of popping up the big Invitations Dialog */
	if (gaim_prefs_get_bool(SIMIAS_PREF_NOTIFY_REJECT_INVITATIONS)) {
		show_invitations_window();
	}
	
	return TRUE;	/* Message was handled correctly */
}

/**
 * This function checks to see if the buffer is a properly formatted accept
 * message and handles it appropriately.
 * 
 * The conditions mentioned in the "Don't do anything" section in the above
 * function apply here too.
 * 
 * If the message is valid and the user is in the list, update the buddy's IP
 * Address in the Simias Gaim Domain Roster.  If the user accepted, Simias
 * should likely already know about the machine and should have already started
 * the sync process.  If this is not the case, then tell Simias to start syncing
 * the collection with the buddy's machine.
 * 
 * [simias:invitation-request-accept:<collection-id>:<ip-address>:<ip-port>]
 */
static gboolean
handle_invitation_request_accept(GaimAccount *account,
								 const char *sender,
								 const char *buffer)
{
	GtkTreeIter iter;
	Invitation *invitation;
	char time_str[32];
	char state_str[32];
	GaimBuddy *buddy;
	GtkTreeIter tb_iter;
	
	/**
	 * Since this method is called, we already know that the first part of
	 * the message matches our #define.  So, because of that, we can take
	 * that portion out of the picture and start tokenizing the different
	 * parts.
	 */
	char *collection_id;
	char *ip_address;
	char *ip_port;
	
	/**
	 * Start parsing the message at this point:
	 * 
	 * 	[simias:invitation-request-accept:<collection-id>:<ip-address>:<ip-port>]
	 *                                    ^
	 */
	collection_id = strtok(
				(char *) buffer + strlen(INVITATION_REQUEST_ACCEPT_MSG), ":");
	if (!collection_id) {
		g_print("handle_invitation_request_accept() couldn't parse the collection-id\n");
		return FALSE;
	}

	ip_address = strtok(NULL, ":");
	if (!ip_address) {
		g_print("handle_invitation_request_accept() couldn't parse the ip-address\n");
		return FALSE;
	}
	
	ip_port = strtok(NULL, "]");
	if (!ip_port) {
		g_print("handle_invitation_request_accept() couldn't parse the ip-port\n");
		return FALSE;
	}
	
	/**
	 * Lookup the collection_id in the current out_inv_store.  If it's not there
	 * we'll just discard this message.  If it IS there, we need to update the
	 * status of the invitation and take more action with Simias.
	 */
	if (!lookup_collection_in_store(out_inv_store, collection_id, &iter)) {
		g_print("handle_invitation_request_accept() couldn't find the collection-id in out_inv_store\n");
		/* FIXME: Before returning from here, we should try to retrieve more information from Simias in case the user deleted this invitation information from Gaim */
		return TRUE;	/* Allow the message to be discarded */
	}
	
	/**
	 * If we get this far, iter now points to the row of data in the store model
	 * that contains the Invitation * corresponding with this message.
	 * 
	 * Update the time and the invitation state in the store/model.
	 */

	/* Extract the Invitation * out of the model */
	gtk_tree_model_get(GTK_TREE_MODEL(out_inv_store), &iter,
						INVITATION_PTR, &invitation,
						-1);
						
	/**
	 * Double-check to make sure nothing has changed since returning from the
	 * lookup call.  If it did, call this function recursively.  If the row
	 * was actually removed, it should stop processing in the recursive call.
	 */
	if (strcmp(invitation->collection_id, collection_id) != 0) {
		/* The row changed */
		return handle_invitation_request_deny(account, sender, buffer);
	}
	
	/* Update the invitation time */
	time(&(invitation->time));
	
	/* Update the invitation state */
	invitation->state = STATE_ACCEPTED_PENDING;
	
	/* Format the time to a string */
	fill_time_str(time_str, 32, invitation->time);

	/* Format the state string */
	fill_state_str(state_str, invitation->state);
	
	/* Update the out_inv_store */
	gtk_list_store_set(out_inv_store, &iter,
		TIME_COL,				time_str,
		STATE_COL,				state_str,
		-1);

	/* Save the updates to a file */
g_print("about_to_write 9\n");
	simias_invitations_write(out_inv_store);

	/* Make sure the buttons are in the correct state */
	out_inv_sel_changed_cb(
		gtk_tree_view_get_selection(GTK_TREE_VIEW(out_inv_tree)),
		GTK_TREE_VIEW(out_inv_tree));

	buddy = gaim_find_buddy(account, sender);

	/**
	 * Add the buddy to the list of trusted buddies if the buddy is not already
	 * there.  If the buddy IS there, just update their IP Address and IP Port.
	 */
	if (lookup_trusted_buddy(trusted_buddies_store, buddy, &tb_iter)) {
		/* Update the trusted buddy info */
		gtk_list_store_set(trusted_buddies_store, &tb_iter,
							TRUSTED_BUDDY_IP_ADDR_COL, ip_address,
							TRUSTED_BUDDY_IP_PORT_COL, ip_port,
							-1);

		/* Update the trusted buddies file */
		save_trusted_buddies(trusted_buddies_store);
	} else {
		/* Add a new trusted buddy */
		add_new_trusted_buddy(trusted_buddies_store, buddy, ip_address, ip_port);
	}

	/* FIXME: Add more interaction with Simias as described in the notes of the function */

	/* FIXME: Change this to a tiny bubble notification instead of popping up the big Invitations Dialog */
	if (gaim_prefs_get_bool(SIMIAS_PREF_NOTIFY_ACCEPT_INVITATIONS)) {
		show_invitations_window();
	}
	
	return TRUE;	/* Message was handled correctly */
}

/**
 * This function checks to see if this is a valid ping request and then handles
 * it.  If we can tell that we've never established any type of connection with
 * the sender before, don't send a reply.  Otherwise, reply with our IP Address.
 */
static gboolean
handle_ping_request(GaimAccount *account, const char *sender,
					const char *buffer)
{
	GtkTreeIter iter;
	char *ip_address;
	char *ip_port;
	int send_result;
	
g_print("handle_ping_request() %s -> %s entered\n",
		sender, gaim_account_get_username(account));
	/**
	 * Since this method is called, we already know that the first part of
	 * the message matches our #define.  So, because of that, we can take
	 * that portion out of the picture and start tokenizing the different
	 * parts.
	 */

	/**
	 * Start parsing the message at this point:
	 * 
	 * 	[simias:ping-request:<ip-address>:<ip-port>]
	 *                       ^
	 */
	ip_address = strtok((char *) buffer + strlen(PING_REQUEST_MSG), ":");
	if (!ip_address) {
		g_print("handle_ping_request() couldn't parse the ip-address\n");
		return FALSE;
	}

	ip_port = strtok(NULL, "]");
	if (!ip_port) {
		g_print("handle_ping_request() couldn't parse the ip-port\n");
		return FALSE;
	}

	/**
	 * Now check in our trusted_buddies_store to see if we have ever accepted an
	 * invitation to share collections with this buddy.  If we have, we can send
	 * a ping-reply message.  If not, we will just drop this message and do
	 * nothing about it (perhaps log it so we could tell that we received it as
	 * a security measure).
	 */
	if (!lookup_trusted_buddy(trusted_buddies_store,
							gaim_find_buddy(account, sender), &iter)) {
		g_print("Received a [simias:ping-request] from an untrusted buddy: %s (%s:%s)\n",
				sender, ip_address, ip_port);
		return TRUE;
	}

	/**
	 * If we get this far, iter now points to the row of data in the store model
	 * that contains the trusted GaimBuddy *.
	 * 
	 * Since we received the sender's IP Address and IP Port update it in our
	 * own record.
	 */
	gtk_list_store_set(trusted_buddies_store, &iter,
						TRUSTED_BUDDY_IP_ADDR_COL, ip_address,
						TRUSTED_BUDDY_IP_PORT_COL, ip_port,
						-1);

	/* Update the trusted buddies file */
	save_trusted_buddies(trusted_buddies_store);

	/* Send a ping-response message */
	send_result = send_ping_response_msg(gaim_find_buddy(account, sender));
	if (send_result <= 0) {
		g_print("handle_ping_request() couldn't send ping response: %d\n", send_result);
	}
	
	return TRUE;
}

/**
 * This function checks to see if the buffer is a properly formatted ping
 * response and then handles it correctly.
 * 
 * If the message is valid, update the buddy's IP Address in the Simias Gaim
 * Domain Roster.
 */
static gboolean
handle_ping_response(GaimAccount *account, const char *sender, 
					 const char *buffer)
{
	GtkTreeIter iter;
	char *ip_address;
	char *ip_port;
	
g_print("handle_ping_response() %s -> %s) entered\n",
		sender, gaim_account_get_username(account));
	/**
	 * Since this method is called, we already know that the first part of
	 * the message matches our #define.  So, because of that, we can take
	 * that portion out of the picture and start tokenizing the different
	 * parts.
	 */

	/**
	 * Start parsing the message at this point:
	 * 
	 * 	[simias:ping-response:<ip-address>:<ip-port>]
	 *                        ^
	 */
	ip_address = strtok((char *) buffer + strlen(PING_RESPONSE_MSG), ":");
	if (!ip_address) {
		g_print("handle_ping_response() couldn't parse the ip-address\n");
		return FALSE;
	}

	ip_port = strtok(NULL, "]");
	if (!ip_port) {
		g_print("handle_ping_response() couldn't parse the ip-port\n");
		return FALSE;
	}

	/**
	 * Now check in our trusted_buddies_store to make sure we trust this buddy.
	 * If we can't find the buddy, there's no since trusting this ping-response
	 * message because.  If we do find the buddy, go ahead and update the
	 * IP Address and IP Port in the trusted_buddies_store.
	 */
	if (!lookup_trusted_buddy(trusted_buddies_store,
							gaim_find_buddy(account, sender), &iter)) {
		g_print("Received a [simias:ping-response] from an untrusted buddy: %s (%s:%s)\n",
				sender, ip_address, ip_port);
		return TRUE;
	}

	/**
	 * If we get this far, iter now points to the row of data in the store model
	 * that contains the trusted GaimBuddy *.
	 */
	gtk_list_store_set(trusted_buddies_store, &iter,
						TRUSTED_BUDDY_IP_ADDR_COL, ip_address,
						TRUSTED_BUDDY_IP_PORT_COL, ip_port,
						-1);

	/* Update the trusted buddies file */
	save_trusted_buddies(trusted_buddies_store);

	return TRUE;
}

/**
 * This function is called any time a buddy-sign-on event occurs.  When this
 * happens, we need to do the following:
 * 
 * 	1. Send out any invitations that are for this buddy that are in the
 *	   STATE_PENDING state.
 *  2. If we have an IP Address for this buddy in the Simias Gaim Domain Roster,
 * 	   send out a [simias:ping-request] message so their IP Address will be
 * 	   updated if it's changed.
 *  3. Send out any pending accept or reject messages for this buddy.
 */
static void
buddy_signed_on_cb(GaimBuddy *buddy, void *user_data)
{
	GtkTreeIter iter;
	int send_result;
	char time_str[32];
	char state_str[32];

	Invitation *invitation;
	gboolean valid;
	gboolean b_in_store_updated = FALSE;
	gboolean b_out_store_updated = FALSE;
	
	valid = gtk_tree_model_get_iter_first(GTK_TREE_MODEL(out_inv_store), &iter);
	while (valid) {
		/* Extract the Invitation * out of the model */
		gtk_tree_model_get(GTK_TREE_MODEL(out_inv_store), &iter,
					INVITATION_PTR, &invitation,
					-1);
					
		/**
		 * Make sure all the following conditions exist before sending the
		 * message:
		 * 	- The invitation state is STATE_PENDING 
		 * 	- The current invitation is intended for this buddy
		 * 	- The buddy is not signing or signed off
		 */
		if (invitation->state == STATE_PENDING
			&& buddy->account == invitation->gaim_account
			&& strcmp(buddy->name, invitation->buddy_name) == 0
			&& buddy->present != GAIM_BUDDY_SIGNING_OFF
			&& buddy->present != GAIM_BUDDY_OFFLINE) {
			send_result = send_invitation_request_msg(
					buddy,
					invitation->collection_id,
					invitation->collection_type,
					invitation->collection_name);
		
			if (send_result > 0) {
				/* Update the invitation time and resend the invitation */
				time(&(invitation->time));

				/* Update the invitation state */
				invitation->state = STATE_SENT;
	
				/* Format the time to a string */
				fill_time_str(time_str, 32, invitation->time);

				/* Format the state string */
				fill_state_str(state_str, invitation->state);

				/* Update the out_inv_store */
				gtk_list_store_set(
					GTK_LIST_STORE(out_inv_store),
					&iter,
					TIME_COL, time_str,
					STATE_COL, state_str,
					-1);

				b_out_store_updated = TRUE;
			}
		}

		valid = gtk_tree_model_iter_next(GTK_TREE_MODEL(out_inv_store), &iter);
	}

	if (gaim_prefs_get_bool(SIMIAS_PREF_REDISCOVER_IP_ADDRS)) {
		if (lookup_trusted_buddy(trusted_buddies_store, buddy, &iter)) {
			/* Send a ping-request message */
			send_result = send_ping_request_msg(buddy);
			if (send_result <= 0) {
				g_print("buddy_signed_on_cb() couldn't send a ping reqest: %d\n", send_result);
			}
		}
	}

	valid = gtk_tree_model_get_iter_first(GTK_TREE_MODEL(in_inv_store), &iter);
	while (valid) {
		/* Extract the Invitation * out of the model */
		gtk_tree_model_get(GTK_TREE_MODEL(in_inv_store), &iter,
					INVITATION_PTR, &invitation,
					-1);

		/**
		 * Make sure all the following conditions exist before sending the
		 * message:
		 * 	- The current invitation is intended for this buddy
		 * 	- The buddy is not signing or signed off
		 */
		if (buddy->account == invitation->gaim_account
			&& strcmp(buddy->name, invitation->buddy_name) == 0
			&& buddy->present != GAIM_BUDDY_SIGNING_OFF
			&& buddy->present != GAIM_BUDDY_OFFLINE) {
			/* Use send_result = 1 to know if a send failed */
			send_result = 1;

			if (invitation->state == STATE_ACCEPTED_PENDING) {
				send_result = send_invitation_request_accept_msg(buddy,
													invitation->collection_id);
			} else if (invitation->state == STATE_REJECTED_PENDING) {
				send_result = send_invitation_request_deny_msg(buddy,
													invitation->collection_id);
			}

			if (send_result <= 0) {
				g_print("Error sending deny message in buddy_signed_on_cb()\n");
				/**
				 * Update the time stamp of the invitation so the user has some
				 * idea that the message was updated.
				 */
				time(&(invitation->time));

				/* Format the time to a string */
				fill_time_str(time_str, 32, invitation->time);

				/* Update the out_inv_store */
				gtk_list_store_set(in_inv_store, &iter,
					TIME_COL, time_str,
					-1);

				b_in_store_updated = TRUE;
			} else {
				/**
				 * The message was sent successfully and so we can remove the
				 * invitation from the in_inv_store.
				 */
				gtk_list_store_remove(in_inv_store, &iter);
				free(invitation);

				b_in_store_updated = TRUE;
			}
		}

		valid = gtk_tree_model_iter_next(GTK_TREE_MODEL(in_inv_store), &iter);
	}

	if (b_in_store_updated) {
		/* Save the updates to a file */
		simias_invitations_write(in_inv_store);
	}
	if (b_out_store_updated) {
		/* Save the updates to a file */
		simias_invitations_write(out_inv_store);
	}
}

static gboolean
plugin_load(GaimPlugin *plugin)
{
	GaimBuddyList *blist;

	blist = gaim_get_blist();
	if (blist) {
		g_hash_table_foreach(blist->buddies,
							 sync_buddy_with_simias_roster,
							 NULL);
	} else {
		g_print("gaim_get_blist() returned NULL\n");
	}

	gaim_signal_connect(gaim_blist_get_handle(),
				"blist-node-extended-menu",
				plugin,
				GAIM_CALLBACK(blist_add_context_menu_items_cb),
				NULL);

	gaim_signal_connect(gaim_conversations_get_handle(),
				"receiving-im-msg",
				plugin,
				GAIM_CALLBACK(receiving_im_msg_cb),
				NULL);

	gaim_signal_connect(gaim_blist_get_handle(),
				"buddy-signed-on",
				plugin,
				GAIM_CALLBACK(buddy_signed_on_cb),
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
	 * would be added in the buddy_signed_on_cb() function because that event is
	 * emitted when you add a new buddy and the buddy is signed-on.
	 * 
	 * Once this event is created and emitted, connect to it in this function
	 * and implement the callback function.
	 */

	/* Make sure the preferences are created if they don't exist */
	init_default_prefs();

	/* Load up the GtkListStore's for incoming and outgoing invitations */
	init_invitation_stores();
	
	/* Load up the GtkListStore for trusted buddies */
	init_trusted_buddies_store();

	/* Load, but don't show the Invitations Window */
	init_invitations_window();

	return TRUE;
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
		G_CALLBACK(show_invitations_window), NULL);

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
	NULL, /* FIXME: Add a plugin-unload function to store information */
	NULL,
	&ui_info,
	NULL,
	NULL
};

static void
init_plugin(GaimPlugin *plugin)
{
	/* FIXME: Possibly move the prefs to this area instead of plugin_load() */
}

GAIM_INIT_PLUGIN(ifolder, init_plugin, info)
