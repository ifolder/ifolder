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
#include "simias-util.h"
#include "buddy-profile.h"


/****************************************************
 * Static Definitions (#defines)                    *
 ****************************************************/
#define IFOLDER_PLUGIN_ID "ifolder"

/****************************************************
 * Global Variables                                 *
 ****************************************************/

/****************************************************
 * Forward Declarations                             *
 ****************************************************/
 
/* Start: UI Functions */
static void blist_add_context_menu_items_cb(GaimBlistNode *node, GList **menu);
/* End: UI Functions */

/****************************************************
 * Function Implementations                         *
 ****************************************************/

/**
 * This function adds extra context menu items to a buddy in the Buddy List that
 * are specific to this plugin.
 */
static void
blist_add_context_menu_items_cb(GaimBlistNode *node, GList **menu)
{
//	GaimBlistNodeAction *act;
//	GaimBuddy *buddy;

//	if (!GAIM_BLIST_NODE_IS_BUDDY(node))
//		return;

//	buddy = (GaimBuddy *)node;

//	act = gaim_blist_node_action_new(_("Simulate Add Member"),
//		buddylist_cb_simulate_share_collection, NULL);
//	*menu = g_list_append(*menu, act);
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

	gaim_signal_connect(gaim_accounts_get_handle(),
				"account-connecting",
				plugin,
				GAIM_CALLBACK(simias_account_connecting_cb),
				NULL);

	gaim_signal_connect(gaim_accounts_get_handle(),
				"account-setting-info",
				plugin,
				GAIM_CALLBACK(simias_account_setting_info_cb),
				NULL);
				
	gaim_signal_connect(gaim_accounts_get_handle(),
				"account-set-info",
				plugin,
				GAIM_CALLBACK(simias_account_set_info_cb),
				NULL);
				
	gaim_signal_connect(gaim_connections_get_handle(),
				"signing-on",
				plugin,
				GAIM_CALLBACK(simias_connection_signing_on_cb),
				NULL);
				
	gaim_signal_connect(gaim_connections_get_handle(),
				"signed-on",
				plugin,
				GAIM_CALLBACK(simias_connection_signed_on_cb),
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

	/* FIXME: Check to see if an AIM account exists (Since that's the only
	 * protocol that this will work with.  If there's not one, throw a popup
	 * in front of the user to let them know this plugin only works with the
	 * AIM protocol for now.
	 */
	 
	/**
	 * This could be the very first time that the user enabled the plugin.  If
	 * so, poke the thread service to do a sync.
	 */
	simias_sync_member_list();

	return TRUE;
}

static gboolean
plugin_unload(GaimPlugin *plugin)
{
	return TRUE; /* Successfully Unloaded */
}

/**
 * Configuration Settings:
 *
 * Identification
 * 
 *   Machine Name: THE_COMPUTER_NAME
 *
 *     The iFolder Plugin for Gaim provides "Workgroup" (Peer to
 *     peer) file sharing for <b>this</b> computer only.  Logging
 *     into Gaim from another computer will not affect the iFolders
 *     configured on this computer.  You will be identified by both
 *     your <b>screenname</b> and your computer's <b>machine name</b>.
 *
 * Other
 *
 *   [ ] Automatically start iFolder (Simias) if it's not running
 */
static GtkWidget *
simias_get_config_frame(GaimPlugin *plugin)
{
	GtkWidget *ret;
	GtkWidget *vbox;
	GtkWidget *hbox;
	GtkWidget *label;
	GtkWidget *sync_now_button;
	GtkWidget *select;
	char machine_name_str[256];
	char hostname[128];
	ret = gtk_vbox_new(FALSE, 18);
	gtk_container_set_border_width (GTK_CONTAINER (ret), 12);

	vbox = gaim_gtk_make_frame(ret, _("Identification"));
	
	if (gethostname(hostname, 128) < 0)
		sprintf(hostname, "UNKNOWN");
	
	sprintf(machine_name_str, "<b>%s</b>: %s", _("Machine Name"), hostname);
	
	label = gtk_label_new("");
	gtk_label_set_markup(GTK_LABEL(label), machine_name_str);
	gtk_box_pack_start(GTK_BOX(vbox), label, FALSE, FALSE, 0);
	
	hbox = gtk_hbox_new(TRUE, 0);
	label = gtk_label_new("");
	gtk_label_set_linewrap(GTK_LABEL(label), TRUE);
	gtk_label_set_markup(_("The iFolder Plugin for Gaim provides <b>Workgroup</b> (Peer to Peer) file sharing for <b>this</b> computer only.  Logging into Gaim from another computer will not affect the iFolders configured on this computer.  You will be identified by both your <b>screenname</b> and your computer's <b>machine name</b>."));
	gtk_box_pack_start(GTK_BOX(hbox), label, TRUE, TRUE, 0);
	gtk_box_pack_start(GTK_BOX(vbox), hbox, FALSE, FALSE, 0);

	/* SECTION: Other */
	vbox = gaim_gtk_make_frame(ret, _("Other"));
	
	gaim_gtk_prefs_checkbox(_("_Automatically start Simias (iFolder Synchronization Engine) if it's not running"),
	                SIMIAS_PREF_SIMIAS_AUTO_START, vbox);
	label = gtk_label_new("(Not implemented yet)");
	gtk_box_pack_end(GTK_BOX(vbox), label, FALSE, FALSE, 0);

	gtk_widget_show_all(ret);
	return ret;
}

static GaimGtkPluginUiInfo ui_info =
{
	simias_get_config_frame
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
	N_("Provides iFolder peer to peer filesharing through Gaim."),
	N_("Allows you to share iFolders with buddies who also have the Gaim iFolder Plugin installed."),
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
	
	/* Since we're calling stuff in liboscar, make sure it's loaded before us */
	info.dependencies = g_list_append(info.dependencies, "prpl-oscar");

	/* Hijack the UI opts so we can read user profiles */
	gaim_notify_set_ui_ops(simias_notify_get_ui_ops());
}

GAIM_INIT_PLUGIN(ifolder, init_plugin, info)


