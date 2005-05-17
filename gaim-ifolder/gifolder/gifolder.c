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
#include "simias-users.h"
#include "gtk-simias-users.h"


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
static void buddylist_cb_enable_ifolder_sharing(GaimBlistNode *node, gpointer user_data);
static void buddylist_cb_disable_ifolder_sharing(GaimBlistNode *node, gpointer user_data);

static void show_simias_users_window();
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
	char settingName[1024];
	const char *pluginEnabled;
	const char *machineName;
	const char *desKey;
	
	GaimBlistNodeAction *act;
	GaimBuddy *buddy;

	if (!GAIM_BLIST_NODE_IS_BUDDY(node))
		return;

	buddy = (GaimBuddy *)node;

	/**
	 * Only add on the menu if the buddy is online and they also have the
	 * iFolder plugin enabled.
	 */
	if (GAIM_BUDDY_IS_ONLINE(buddy))
	{
		pluginEnabled =
			gaim_blist_node_get_string(&(buddy->node), "simias-plugin-enabled");
		
		if (pluginEnabled)
		{
			/**
			 * If the "simias-plugin-enabled" setting exists its value will be
			 * the buddy's machine name that they are currently online with.
			 */
			machineName = pluginEnabled;
			sprintf(settingName, "simias-des-key:%s", machineName);
			desKey = gaim_blist_node_get_string(&(buddy->node), settingName);
			if (desKey)
			{
				/* iFolder Sharing with this buddy is enabled */
				act = gaim_blist_node_action_new(_("Disable iFolder File Sharing"),
					buddylist_cb_disable_ifolder_sharing, NULL);
			}
			else
			{
				/* The buddy has the iFolder plugin, but they aren't "enabled" */
				act = gaim_blist_node_action_new(_("Enable iFolder File Sharing"),
					buddylist_cb_enable_ifolder_sharing, NULL);
			}

			*menu = g_list_append(*menu, act);
		}
	}
}

static void
buddylist_cb_enable_ifolder_sharing(GaimBlistNode *node, gpointer user_data)
{
	GaimBuddy *buddy;
	GtkWidget *dialog;
	const char *buddy_alias = NULL;
	const char *machineName;
	int err;
	
	buddy = (GaimBuddy *)node;
	
	err = simias_send_invitation_request(buddy);
	if (err <= 0)
	{
		buddy_alias = gaim_buddy_get_alias(buddy);
		dialog =
			gtk_message_dialog_new(NULL,
									GTK_DIALOG_DESTROY_WITH_PARENT,
									GTK_MESSAGE_ERROR,
									GTK_BUTTONS_OK,
									_("There was an error enabling iFolder Sharing with %s.  Perhaps %s is not online or you do not have iFolder/Simias running?"),
									buddy_alias ? buddy_alias : buddy->name,
									buddy_alias ? buddy_alias : buddy->name);
		gtk_dialog_run(GTK_DIALOG(dialog));
		gtk_widget_destroy(dialog);
	}
	else
	{
		/* FIXME: If a conversation window is open with this buddy, add a little string saying that we just sent an invitation */
		fprintf(stderr, "invitation message sent to %s\n", buddy->name);

		machineName = gaim_blist_node_get_string(&(buddy->node),
												 "simias-plugin-enabled");
		if (machineName)
		{
			simias_users_update_status(
				buddy->name,
				machineName,
				buddy->account->username,
				buddy->account->protocol_id,
				USER_INVITED);
			
			simias_gtk_show_users(NULL);
		}
	}
}

static void
buddylist_cb_disable_ifolder_sharing(GaimBlistNode *node, gpointer user_data)
{
	GaimBuddy *buddy;
	GtkWidget *dialog;
	gint response;
	char settingName[1024];
	const char *pluginEnabled;
	const char *machineName;
	const char *buddy_alias = NULL;

	buddy = (GaimBuddy *)node;
	buddy_alias = gaim_buddy_get_alias(buddy);

	pluginEnabled =
		gaim_blist_node_get_string(&(buddy->node), "simias-plugin-enabled");
		
	if (pluginEnabled)
	{
		machineName = pluginEnabled;

		dialog =
			gtk_message_dialog_new(NULL,
								GTK_DIALOG_DESTROY_WITH_PARENT,
								GTK_MESSAGE_QUESTION,
								GTK_BUTTONS_YES_NO,
								_("Disabling iFolder Sharing will prevent iFolder from synchronizing any iFolders that you may have shared with %s (%s).  You will have to re-enable iFolder Sharing before iFolder will synchronize with %s (%s)."),
								buddy_alias ? buddy_alias : buddy->name,
								machineName,
								buddy_alias ? buddy_alias : buddy->name,
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
		}
	}
	else
	{
		fprintf(stderr, "The buddy is not online anymore!\n");
	}
}

static void
show_simias_users_window()
{
	simias_gtk_show_users(NULL);
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

	gaim_signal_connect(gaim_blist_get_handle(),
				"buddy-signed-off",
				plugin,
				GAIM_CALLBACK(simias_buddy_signed_off_cb),
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

	simias_init_users();
	 
	/**
	 * This could be the very first time that the user enabled the plugin.  If
	 * so, poke the thread service to do a sync.
	 */
	simias_sync_member_list();
	
	/**
	 * If the user is already signed on with their AIM account, we'll need to
	 * set the user profile now so we won't have to make them sign off and sign
	 * back on again.
	 */
	GList *accounts = gaim_accounts_get_all();
	if (accounts)
	{
		g_list_foreach(accounts, simias_set_account_profile_foreach, NULL);
	}

	return TRUE;
}

static gboolean
plugin_unload(GaimPlugin *plugin)
{
	simias_cleanup_users();

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
	GtkWidget *show_users_button;
	GtkWidget *sync_now_button;
	GtkWidget *select;
	char machine_name_str[512];
	ret = gtk_vbox_new(FALSE, 18);
	gtk_container_set_border_width (GTK_CONTAINER (ret), 12);

	/* SECTION: Note */
	vbox = gaim_gtk_make_frame(ret, _("Note"));
	
	label = gtk_label_new("");
	gtk_label_set_line_wrap(GTK_LABEL(label), TRUE);
	gtk_label_set_markup(GTK_LABEL(label), _("The iFolder Plugin for Gaim provides Workgroup (Peer to Peer) file sharing for <b><i>this computer only</i></b>.  Logging into Gaim from another computer will not affect the iFolders configured on this computer.  You will be identified by both your screenname and your computer's machine name."));
	gtk_misc_set_alignment(GTK_MISC(label), 0.0, 0.5);
	gtk_box_pack_start(GTK_BOX(vbox), label, FALSE, FALSE, 0);

	label = gtk_label_new("");
	gtk_label_set_markup(GTK_LABEL(label), _("The Gaim Workgroup will only be available in iFolder (Simias) when you have this plugin enabled <b>AND</b> have a AIM (AOL Instant Messenger) account."));
	gtk_label_set_line_wrap(GTK_LABEL(label), TRUE);
	gtk_misc_set_alignment(GTK_MISC(label), 0.0, 0.5);
	gtk_box_pack_start(GTK_BOX(vbox), label, FALSE, FALSE, 0);
	
	/* SECTION: Identification */
	vbox = gaim_gtk_make_frame(ret, _("Identification"));
	
	sprintf(machine_name_str, "%s: %s",
			_("Machine Name"),
			gaim_prefs_get_string(SIMIAS_PREF_MACHINE_NAME));
	
	label = gtk_label_new("");
	gtk_label_set_markup(GTK_LABEL(label), machine_name_str);
	gtk_misc_set_alignment(GTK_MISC(label), 0.0, 0.5);
	gtk_box_pack_start(GTK_BOX(vbox), label, FALSE, FALSE, 0);
	
	/* SECTION: Gaim Domain Member List */
	vbox = gaim_gtk_make_frame(ret, _("Gaim Domain Member List"));

	hbox = gtk_hbox_new(FALSE, 0);

	select = gaim_gtk_prefs_labeled_spin_button(hbox,
			_("Synchronize every:"), SIMIAS_PREF_SYNC_INTERVAL,
			1, 24 * 60, NULL);
	label = gtk_label_new(_("minutes"));
	gtk_box_pack_end(GTK_BOX(hbox), label, TRUE, TRUE, 4);
	
	gtk_box_pack_start(GTK_BOX(vbox), hbox, FALSE, FALSE, 0);
	
	hbox = gtk_hbox_new(TRUE, 0);
	sync_now_button =
		gtk_button_new_with_mnemonic(_("_Synchronize Now"));
	gtk_box_pack_end(GTK_BOX(hbox),
			sync_now_button, FALSE, FALSE, 50);

	gtk_box_pack_start(GTK_BOX(vbox), hbox, FALSE, FALSE, 0);

	show_users_button =
		gtk_button_new_with_mnemonic(_("iFolder Filesharing _Users"));
	
	gtk_box_pack_end(GTK_BOX(vbox),
					 show_users_button, FALSE, FALSE, 0);

	g_signal_connect(G_OBJECT(sync_now_button), "clicked",
		G_CALLBACK(simias_sync_member_list), NULL);
	g_signal_connect(G_OBJECT(show_users_button), "clicked",
		G_CALLBACK(show_simias_users_window), NULL);

	/* SECTION: Other */
	vbox = gaim_gtk_make_frame(ret, _("Other"));
	
	gaim_gtk_prefs_checkbox(_("_Automatically start iFolder (Simias) if it's not running."),
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


