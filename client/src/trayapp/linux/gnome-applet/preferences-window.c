/***********************************************************************
 *  iFolder 3 Applet -- Main applet for the iFolder 3 Client
 * 
 *  Copyright (C) 2006 Novell, Inc.
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
 *  Author(s): Boyd Timothy <btimothy@novell.com>
 *
 ***********************************************************************/

#include <stdlib.h>
#include <string.h>
#include <gtk/gtk.h>
#include <gdk/gdkkeysyms.h>

//#if !GTK_CHECK_VERSION(2,6,0)
//#include <gnome.h>
//#endif

//#include <ifolder-client.h>

#include "account-wizard.h"
#include "util.h"
#include "wait-dialog.h"

#include "preferences-window.h"

/*@todo Remove this when gettext is added */
#define _

extern iFolderClient *ifolder_client;

typedef struct _iFolderPrefsAuthReq iFolderPrefsAuthReq;
struct _iFolderPrefsAuthReq
{
	IFAPreferencesWindow	*prefs_window;
	iFolderDomain			*domain;
	gchar					*password;
	gboolean				rememberPassword;
	GtkCellRendererToggle	*cell_renderer;
	GError					*error;
};

static IFAPreferencesWindow *prefsWindow = NULL;

static IFAPreferencesWindow *create_preferences_window();
static void delete_preferences_window(IFAPreferencesWindow *pw);
static gboolean key_press_handler(GtkWidget *widget, GdkEventKey *event, IFAPreferencesWindow *pw);
static gboolean key_release_handler(GtkWidget *widget, GdkEventKey *event, IFAPreferencesWindow *pw);
static gboolean delete_event_handler(GtkWidget *widget, GdkEvent *event, IFAPreferencesWindow *pw);
static GtkWidget *create_general_page(IFAPreferencesWindow *pw);
static GtkWidget *create_accounts_page(IFAPreferencesWindow *pw);
static void help_button_clicked(GtkButton *button, IFAPreferencesWindow *pw);
static void close_button_clicked(GtkButton *button, IFAPreferencesWindow *pw);
static void close_window();
static void notebook_page_switched(GtkNotebook *notebook, GtkNotebookPage *page, guint page_num, IFAPreferencesWindow *pw);
static void leaving_general_page();
static void general_page_realized(GtkWidget *widget, IFAPreferencesWindow *pw);
static void show_conf_button_toggled(GtkToggleButton *togglebutton, IFAPreferencesWindow *pw);
static void notify_sync_errors_button_toggled(GtkToggleButton *togglebutton, IFAPreferencesWindow *pw);

static void on_auto_sync_button(GtkToggleButton *togglebutton, IFAPreferencesWindow *pw);
static void on_sync_interval_changed(GtkSpinButton *spinbutton, IFAPreferencesWindow *pw);
static void on_sync_units_changed(GtkComboBox *widget, IFAPreferencesWindow *pw);

static void accounts_page_realized(GtkWidget *widget, IFAPreferencesWindow *pw);
static void online_cell_toggle_data_func(GtkTreeViewColumn *column, GtkCellRenderer *cell, GtkTreeModel *model, GtkTreeIter *iter, IFAPreferencesWindow *pw);
static void online_toggled(GtkCellRendererToggle *cell_renderer, gchar *path, IFAPreferencesWindow *pw);

static void server_cell_text_data_func(GtkTreeViewColumn *column, GtkCellRenderer *cell, GtkTreeModel *model, GtkTreeIter *iter, IFAPreferencesWindow *pw);
static void name_cell_text_data_func(GtkTreeViewColumn *column, GtkCellRenderer *cell, GtkTreeModel *model, GtkTreeIter *iter, IFAPreferencesWindow *pw);

static void acc_selection_changed (GtkTreeSelection *treeselection, IFAPreferencesWindow *pw);

static void on_add_account(GtkButton *widget, IFAPreferencesWindow *pw);
static void on_remove_account(GtkButton *widget, IFAPreferencesWindow *pw);
static void on_properties_clicked(GtkButton *widget, IFAPreferencesWindow *pw);

static void on_acc_tree_row_activated(GtkTreeView *tree_view, GtkTreePath *path, GtkTreeViewColumn *column, IFAPreferencesWindow *pw);

static void populate_domains(IFAPreferencesWindow *pw);
static void update_widget_sensitivity(IFAPreferencesWindow *pw);

static gboolean g_ifolder_domain_equal(gconstpointer a, gconstpointer b);

static gpointer log_in_thread (iFolderPrefsAuthReq *authReq);
static gboolean log_in_thread_completed(iFolderPrefsAuthReq *authReq);

static gpointer log_out_thread (iFolderPrefsAuthReq *authReq);
static gboolean log_out_thread_completed(iFolderPrefsAuthReq *authReq);

static void domain_added_cb (iFolderClient *client, iFolderDomain *domain, IFAPreferencesWindow *pw);
static void domain_removed_cb (iFolderClient *client, iFolderDomain *domain, IFAPreferencesWindow *pw);

IFAPreferencesWindow *
ifa_get_preferences_window()
{
	if (prefsWindow == NULL)
	{
		prefsWindow = create_preferences_window();
	}

	return prefsWindow;
}

void
ifa_show_preferences_window()
{
	IFAPreferencesWindow *pw = ifa_get_preferences_window();
	if (!pw)
		return;

	gtk_widget_show_all(pw->window);
}

IFAPreferencesWindow *
create_preferences_window()
{
	IFAPreferencesWindow *pw = (IFAPreferencesWindow *)malloc(sizeof(IFAPreferencesWindow));

	pw->window = gtk_window_new(GTK_WINDOW_TOPLEVEL);
	gtk_window_set_title(GTK_WINDOW(pw->window), _("iFolder Preferences"));
	pw->controlKeyPressed = false;
	g_signal_connect(G_OBJECT(pw->window), "key-press-event", G_CALLBACK(key_press_handler), pw);
	g_signal_connect(G_OBJECT(pw->window), "key-release-event", G_CALLBACK(key_release_handler), pw);
	g_signal_connect(G_OBJECT(pw->window), "delete-event", G_CALLBACK(delete_event_handler), pw);

	gtk_window_set_default_size(GTK_WINDOW(pw->window), 480, 550);

	GtkWidget *winBox = gtk_vbox_new(false, 7);
	gtk_container_set_border_width(GTK_CONTAINER(winBox), 7);

	gtk_window_set_position(GTK_WINDOW(pw->window), GTK_WIN_POS_CENTER);
	gtk_container_add(GTK_CONTAINER(pw->window), winBox);

	pw->notebook = gtk_notebook_new();
	gtk_notebook_append_page(GTK_NOTEBOOK(pw->notebook), create_general_page(pw), gtk_label_new(_("General")));
	gtk_notebook_append_page(GTK_NOTEBOOK(pw->notebook), create_accounts_page(pw), gtk_label_new(_("Accounts")));

	g_signal_connect(G_OBJECT(pw->notebook), "switch-page", G_CALLBACK(notebook_page_switched), pw);

	gtk_box_pack_start(GTK_BOX(winBox), pw->notebook, true, true, 0);

	pw->buttonBox = gtk_hbutton_box_new();
	gtk_box_pack_start(GTK_BOX(winBox), pw->buttonBox, false, false, 0);

	pw->helpButton = gtk_button_new_from_stock(GTK_STOCK_HELP);
	gtk_box_pack_start(GTK_BOX(pw->buttonBox), pw->helpButton, false, false, 0);
	g_signal_connect(G_OBJECT(pw->helpButton), "clicked", G_CALLBACK(help_button_clicked), pw);

	pw->closeButton = gtk_button_new_from_stock(GTK_STOCK_CLOSE);
	gtk_box_pack_start(GTK_BOX(pw->buttonBox), pw->closeButton, false, false, 0);
	g_signal_connect(G_OBJECT(pw->closeButton), "clicked", G_CALLBACK(close_button_clicked), pw);
	
	pw->waitDialog = NULL;

	return pw;
}

static gboolean
key_press_handler(GtkWidget *widget, GdkEventKey *event, IFAPreferencesWindow *pw)
{
	gboolean stop_other_handlers = true;

	switch(event->keyval)
	{
		case GDK_Escape:
			close_window();
			break;
		case GDK_Control_L:
		case GDK_Control_R:
			pw->controlKeyPressed = true;
			stop_other_handlers = false;
			break;
		case GDK_W:
		case GDK_w:
			if (pw->controlKeyPressed)
				close_window();
			else
				stop_other_handlers = false;
			break;
		default:
			stop_other_handlers = false;
			break;
	}

	return stop_other_handlers;
}

static gboolean
key_release_handler(GtkWidget *widget, GdkEventKey *event, IFAPreferencesWindow *pw)
{
	gboolean stop_other_handlers = false;

	switch(event->keyval)
	{
		case GDK_Control_L:
		case GDK_Control_R:
			pw->controlKeyPressed = false;
			break;
		default:
			break;
	}

	return stop_other_handlers;
}

static GtkWidget *
create_general_page(IFAPreferencesWindow *pw)
{
	pw->generalPage = gtk_vbox_new(false, IFA_DEFAULT_SECTION_SPACING);
	gtk_container_set_border_width(GTK_CONTAINER(pw->generalPage), IFA_DEFAULT_BORDER_WIDTH);
	g_signal_connect(G_OBJECT(pw->generalPage), "realize", G_CALLBACK(general_page_realized), pw);

	/**
	 * Application Settings
	 */
	GtkWidget *appSectionBox = gtk_vbox_new(false, IFA_DEFAULT_SECTION_TITLE_SPACING);
	gtk_box_pack_start(GTK_BOX(pw->generalPage), appSectionBox, false, true, 0);
	GtkWidget *appSectionLabel = gtk_label_new(_("<span weight=\"bold\">Application</span>"));
	gtk_label_set_use_markup(GTK_LABEL(appSectionLabel), true);
	gtk_misc_set_alignment(GTK_MISC(appSectionLabel), 0, 0.5);
	gtk_box_pack_start(GTK_BOX(appSectionBox), appSectionLabel, false, true, 0);

	/* create an hbox to provide spacing */
	GtkWidget *appSpacerBox = gtk_hbox_new(false, 0);
	gtk_box_pack_start(GTK_BOX(appSectionBox), appSpacerBox, false, true, 0);
	GtkWidget *appSpaceLabel = gtk_label_new("    ");	// four spaces
	gtk_box_pack_start(GTK_BOX(appSpacerBox), appSpaceLabel, false, true, 0);

	/* create a vbox to actually place the widgets in for the section */
	GtkWidget *appWidgetBox = gtk_vbox_new(false, 5); /* FIXME: Remove hard-coded spacing */
	gtk_box_pack_start(GTK_BOX(appSpacerBox), appWidgetBox, true, true, 0);

	pw->showConfirmationButton = gtk_check_button_new_with_mnemonic(_("_Show a confirmation dialog when creating iFolders"));
	gtk_box_pack_start(GTK_BOX(appWidgetBox), pw->showConfirmationButton, false, true, 0);
	g_signal_connect(G_OBJECT(pw->showConfirmationButton), "toggled", G_CALLBACK(show_conf_button_toggled), pw);

	GtkWidget *startupLabel = gtk_label_new(_("<span style=\"italic\">To start up iFolder at login, leave iFolder running when you log out and save your current setup.</span>"));
	gtk_label_set_use_markup(GTK_LABEL(startupLabel), true);
	gtk_label_set_line_wrap(GTK_LABEL(startupLabel), true);
	gtk_box_pack_start(GTK_BOX(appWidgetBox), startupLabel, false, true, 0);

	/**
	 * Notifications
	 */
	GtkWidget *notifySectionBox = gtk_vbox_new(false, IFA_DEFAULT_SECTION_TITLE_SPACING);
	gtk_box_pack_start(GTK_BOX(pw->generalPage), notifySectionBox, false, true, 0);

	GtkWidget *notifySectionLabel = gtk_label_new(_("<span weight=\"bold\">Notification</span>"));
	gtk_label_set_use_markup(GTK_LABEL(notifySectionLabel), true);
	gtk_misc_set_alignment(GTK_MISC(notifySectionLabel), 0, 0.5);
	gtk_box_pack_start(GTK_BOX(notifySectionBox), notifySectionLabel, false, true, 0);

	/* create an hbox to provide spacing */
	GtkWidget *notifySpacerBox = gtk_hbox_new(false, 0);
	gtk_box_pack_start(GTK_BOX(notifySectionBox), notifySpacerBox, false, true, 0);
	GtkWidget *notifySpaceLabel = gtk_label_new("    "); /* four spaces */
	gtk_box_pack_start(GTK_BOX(notifySpacerBox), notifySpaceLabel, false, true, 0);

	/* create a vbox to actually place the widgets in for the section */
	GtkWidget *notifyWidgetBox = gtk_vbox_new(false, 5); /* FIXME: un-hard-code this and move the value to a #define in util.h */
	gtk_box_pack_start(GTK_BOX(notifySpacerBox), notifyWidgetBox, true, true, 0);

	pw->notifySyncErrorsButton = gtk_check_button_new_with_mnemonic(_("Notify of _synchronization errors"));
	gtk_box_pack_start(GTK_BOX(notifyWidgetBox), pw->notifySyncErrorsButton, false, true, 0);
	g_signal_connect(G_OBJECT(pw->notifySyncErrorsButton), "toggled", G_CALLBACK(notify_sync_errors_button_toggled), pw);
	
	/**
	 * Sync Settings
	 */
	GtkWidget *syncSectionBox = gtk_vbox_new(false, IFA_DEFAULT_SECTION_TITLE_SPACING);
	gtk_box_pack_start(GTK_BOX(pw->generalPage), syncSectionBox, false, true, 0);
	GtkWidget *syncSectionLabel = gtk_label_new(_("<span weight=\"bold\">Synchronization</span>"));
	gtk_label_set_use_markup(GTK_LABEL(syncSectionLabel), true);
	gtk_misc_set_alignment(GTK_MISC(syncSectionLabel), 0, 0.5);
	gtk_box_pack_start(GTK_BOX(syncSectionBox), syncSectionLabel, false, true, 0);
	
	/* create an hbox to provide spacing */
	GtkWidget *syncSpacerBox = gtk_hbox_new(false, 0);
	gtk_box_pack_start(GTK_BOX(syncSectionBox), syncSpacerBox, false, true, 0);
	GtkWidget *syncSpaceLabel = gtk_label_new("    "); /* four spaces */
	gtk_box_pack_start(GTK_BOX(syncSpacerBox), syncSpaceLabel, false, true, 0);

	/* create a vbox to actually place the widgets in for the section */
	GtkWidget *syncWidgetBox = gtk_vbox_new(false, 10); /* FIXME: un-hard-code this and move the value to a #define in util.h */
	gtk_box_pack_start(GTK_BOX(syncSpacerBox), syncWidgetBox, true, true, 0);

	GtkWidget *syncHBox0 = gtk_hbox_new(false, 10);
	gtk_box_pack_start(GTK_BOX(syncWidgetBox), syncHBox0, false, true, 0);
	pw->autoSyncCheckButton = gtk_check_button_new_with_mnemonic(_("Automatically S_ynchronize iFolders"));
	gtk_box_pack_start(GTK_BOX(syncHBox0), pw->autoSyncCheckButton, false, false, 0);
	
	GtkWidget *syncHBox = gtk_hbox_new(false, 10);
	gtk_box_pack_start(GTK_BOX(syncWidgetBox), syncHBox, true, true, 0);

	GtkWidget *spacerLabel = gtk_label_new("  ");
	gtk_box_pack_start(GTK_BOX(syncHBox), spacerLabel, true, true, 0);

	GtkWidget *syncEveryLabel = gtk_label_new(_("Synchronize iFolders Every"));
	gtk_misc_set_alignment(GTK_MISC(syncEveryLabel), 1, 0.5);
	gtk_box_pack_start(GTK_BOX(syncHBox), syncEveryLabel, false, false, 0);
	
	GtkObject *adjustment = gtk_adjustment_new(1, 1, G_MAXINT32, 1, 1, 1);
	pw->syncSpinButton = gtk_spin_button_new(GTK_ADJUSTMENT(adjustment), 1, 0);
	
	gtk_box_pack_start(GTK_BOX(syncHBox), pw->syncSpinButton, false, false, 0);

	pw->syncUnitsComboBox = gtk_combo_box_new_text();
	gtk_box_pack_start(GTK_BOX(syncHBox), pw->syncUnitsComboBox, false, false, 0);
	
	gtk_combo_box_append_text(GTK_COMBO_BOX(pw->syncUnitsComboBox), _("seconds"));
	gtk_combo_box_append_text(GTK_COMBO_BOX(pw->syncUnitsComboBox), _("minutes"));
	gtk_combo_box_append_text(GTK_COMBO_BOX(pw->syncUnitsComboBox), _("hours"));
	gtk_combo_box_append_text(GTK_COMBO_BOX(pw->syncUnitsComboBox), _("days"));
	
	gtk_combo_box_set_active(GTK_COMBO_BOX(pw->syncUnitsComboBox), IFA_SYNC_UNIT_MINUTES);
	pw->currentSyncUnit = IFA_SYNC_UNIT_MINUTES;

	return pw->generalPage;
}

static GtkWidget *
create_accounts_page(IFAPreferencesWindow *pw)
{
	GtkTreeSelection *selection;
	
	pw->accountsPage = gtk_vbox_new(false, IFA_DEFAULT_SECTION_SPACING);
	gtk_container_set_border_width(GTK_CONTAINER(pw->accountsPage), IFA_DEFAULT_BORDER_WIDTH);
	g_signal_connect(G_OBJECT(pw->accountsPage), "realize", G_CALLBACK(accounts_page_realized), pw);
	
//	pw->curDomains = g_hash_table_new(g_direct_hash, g_ifolder_domain_equal);
//	pw->removedDomains = g_hash_table_new(g_str_hash, g_str_equal);
	pw->detailsDialogs = g_hash_table_new(g_direct_hash, g_str_equal);

//	pw->curDomains = g_hash_table_new_full(g_str_hash, g_str_equal);
//	pw->removedDomains = g_hash_table_new_full(g_str_hash, g_str_equal);
//	pw->detailsDialogs = g_hash_table_new_full(g_str_hash, g_str_equal);

	/* FIXME: Register for domain events */
	
	/* Set up the Accounts Tree View in a scrolled window */
	pw->accTreeView = gtk_tree_view_new();
	selection = gtk_tree_view_get_selection (GTK_TREE_VIEW (pw->accTreeView));
	gtk_tree_selection_set_mode (selection, GTK_SELECTION_SINGLE); /* FIXME: Implement multiple selection at a later point if desired so users can highlight multiple accounts and remove them. */
	
	GtkWidget *sw = gtk_scrolled_window_new(NULL, NULL);
	gtk_scrolled_window_set_shadow_type(GTK_SCROLLED_WINDOW(sw), GTK_SHADOW_ETCHED_IN);
	gtk_scrolled_window_set_policy(GTK_SCROLLED_WINDOW(sw), GTK_POLICY_AUTOMATIC, GTK_POLICY_AUTOMATIC);
	gtk_container_add(GTK_CONTAINER(sw), pw->accTreeView);
	gtk_box_pack_start(GTK_BOX(pw->accountsPage), sw, true, true, 0);
	
	pw->accTreeStore = gtk_list_store_new(1, G_TYPE_POINTER);
	gtk_tree_view_set_model(GTK_TREE_VIEW(pw->accTreeView), GTK_TREE_MODEL(pw->accTreeStore));
	
	/* Online Column */
	GtkTreeViewColumn *onlineColumn = gtk_tree_view_column_new();
	gtk_tree_view_column_set_title(onlineColumn, _("Online"));
	pw->onlineToggleButton = gtk_cell_renderer_toggle_new();
	g_object_set(G_OBJECT(pw->onlineToggleButton), "xpad", 5, "xalign", 0.5F, NULL);
	gtk_tree_view_column_pack_start(onlineColumn, pw->onlineToggleButton, false);
	gtk_tree_view_column_set_cell_data_func(onlineColumn,
											pw->onlineToggleButton,
											(GtkTreeCellDataFunc)online_cell_toggle_data_func,
											pw,
											NULL);
	
	g_signal_connect(G_OBJECT(pw->onlineToggleButton), "toggled", G_CALLBACK(online_toggled), pw);
	gtk_tree_view_append_column(GTK_TREE_VIEW(pw->accTreeView), onlineColumn);
	
	/* Server Column */
	GtkTreeViewColumn *serverColumn = gtk_tree_view_column_new();
	gtk_tree_view_column_set_title(serverColumn, _("Name"));
	GtkCellRenderer *servercr = gtk_cell_renderer_text_new();
	g_object_set(G_OBJECT(servercr), "xpad", 5, NULL);
	gtk_tree_view_column_pack_start(serverColumn, servercr, true);
	gtk_tree_view_column_set_cell_data_func(serverColumn,
											servercr,
											(GtkTreeCellDataFunc)server_cell_text_data_func,
											pw,
											NULL);
	gtk_tree_view_column_set_resizable(serverColumn, true);
	gtk_tree_view_column_set_min_width(serverColumn, 150);
	gtk_tree_view_append_column(GTK_TREE_VIEW(pw->accTreeView), serverColumn);
	
	/* User Name Column */
	GtkTreeViewColumn *nameColumn = gtk_tree_view_column_new();
	gtk_tree_view_column_set_title(nameColumn, _("User Name"));
	GtkCellRenderer *ncrt = gtk_cell_renderer_text_new();
	g_object_set(G_OBJECT(ncrt), "xpad", 5, NULL);
	gtk_tree_view_column_pack_start(nameColumn, ncrt, true);
	gtk_tree_view_column_set_cell_data_func(nameColumn,
											ncrt,
											(GtkTreeCellDataFunc)name_cell_text_data_func,
											pw,
											NULL);
	gtk_tree_view_column_set_resizable(nameColumn, true);
	gtk_tree_view_column_set_min_width(nameColumn, 150);
	gtk_tree_view_append_column(GTK_TREE_VIEW(pw->accTreeView), nameColumn);
	
	/* Set up buttons for add/remove/details */
	GtkWidget *buttonBox = gtk_hbutton_box_new();
	gtk_box_set_spacing(GTK_BOX(buttonBox), 10); /* FIXME: Add this to not be hard-coded */
	gtk_button_box_set_layout(GTK_BUTTON_BOX(buttonBox), GTK_BUTTONBOX_END);
	gtk_box_pack_start(GTK_BOX(pw->accountsPage), buttonBox, false, false, 0);
	
	pw->addButton = gtk_button_new_from_stock(GTK_STOCK_ADD);
	gtk_box_pack_start(GTK_BOX(buttonBox), pw->addButton, false, false, 0);
	g_signal_connect(G_OBJECT(pw->addButton), "clicked", G_CALLBACK(on_add_account), pw);

	pw->removeButton = gtk_button_new_from_stock(GTK_STOCK_REMOVE);
	gtk_box_pack_start(GTK_BOX(buttonBox), pw->removeButton, false, false, 0);
	g_signal_connect(G_OBJECT(pw->removeButton), "clicked", G_CALLBACK(on_remove_account), pw);

	pw->propertiesButton = gtk_button_new_from_stock(GTK_STOCK_PROPERTIES);
	gtk_box_pack_start(GTK_BOX(buttonBox), pw->propertiesButton, false, false, 0);
	g_signal_connect(G_OBJECT(pw->propertiesButton), "clicked", G_CALLBACK(on_properties_clicked), pw);
	
	g_signal_connect (selection, "changed", G_CALLBACK (acc_selection_changed), pw);
	g_signal_connect(G_OBJECT(pw->accTreeView), "row-activated", G_CALLBACK(on_acc_tree_row_activated), pw);
	
	return pw->accountsPage;
}

static void
help_button_clicked(GtkButton *button, IFAPreferencesWindow *pw)
{
	switch(gtk_notebook_get_current_page(GTK_NOTEBOOK(pw->notebook)))
	{
		case 0:
			ifa_show_help(IFA_HELP_PREFERENCES_PAGE);
			break;
		case 1:
			ifa_show_help(IFA_HELP_ACCOUNTS_PAGE);
			break;
		default:
			ifa_show_help(IFA_HELP_MAIN_PAGE);
			break;
	}
}

static void
close_button_clicked(GtkButton *button, IFAPreferencesWindow *pw)
{
	close_window();
}

static gboolean
delete_event_handler(GtkWidget *widget, GdkEvent *event, IFAPreferencesWindow *pw)
{
	g_message("delete_event_handler()");
	
	close_window();
	
	return TRUE; /* stop other handlers from being invoked */
}

static void
close_window()
{
	if (!prefsWindow)
		return;

	if (gtk_notebook_get_current_page(GTK_NOTEBOOK(prefsWindow->notebook)) == 0)
		leaving_general_page();
	
	/* Disconnect signal handlers */
	g_signal_handler_disconnect (ifolder_client, prefsWindow->domain_added_cb_id);
	g_signal_handler_disconnect (ifolder_client, prefsWindow->domain_removed_cb_id);

	gtk_widget_hide(prefsWindow->window);
	gtk_widget_destroy(prefsWindow->window);

	delete_preferences_window(prefsWindow);
}

static void
delete_preferences_window(IFAPreferencesWindow *pw)
{
	g_hash_table_destroy (pw->detailsDialogs);

	free(pw);
	prefsWindow = NULL;
}

static void
notebook_page_switched(GtkNotebook *notebook, GtkNotebookPage *page, guint page_num, IFAPreferencesWindow *pw)
{
	if (page_num != 0)
		leaving_general_page();
}

static void
leaving_general_page()
{
}

/**
 * Set the values of all the widgets
 */
static void
general_page_realized(GtkWidget *widget, IFAPreferencesWindow *pw)
{
	int err;

	g_message("Implement general_page_realized()");
/* FIXME: Implement general_page_realized()
	gtk_toggle_button_set_active(GTK_TOGGLE_BUTTON(pw->showConfirmationButton)),
				     ifolder_user_pref_get_bool(IFOLDER_PREF_SHOW_CREATION));

	gtk_toggle_button_set_active(GTK_TOGGLE_BUTTON(pw->notifySyncErrorsButton)),
				     ifolder_user_pref_get_bool(IFOLDER_PREF_SHOW_CREATION));
*/
//	err = ifolder_client_get_default_sync_interval(&(pw->lastSyncInterval));
//	if (err == IFOLDER_SUCCESS)
//	{
//		if (pw->lastSyncInterval <= 0)
//			gtk_spin_button_set_value(GTK_SPIN_BUTTON(pw->syncSpinButton), 0);
//		else
//		{
//			char * syncUnitString = ifolder_user_pref_get_string(IFOLDER_PREF_SYNC_UNIT);
//			if (strcmp(syncUnitString, "Seconds") == 0)
//			{
//				pw->currentSyncUnit = IFA_SYNC_UNIT_SECONDS;
//				
//				/* prevent the user from setting a sync interval less than one minute */
//				gtk_combo_box_set_range(GTK_SPIN_BUTTON(pw->syncSpinButton), 60, G_MAXINT32);
//			}
//			else if (strcmp(syncUnitString, "Minutes") == 0)
//			{
//				pw->currentSyncUnit = IFA_SYNC_UNIT_MINUTES;
//			}
//			else if (strcmp(syncUnitString, "Hours") == 0)
//			{
//				pw->currentSyncUnit = IFA_SYNC_UNIT_HOURS;
//			}
//			else if (strcmp(syncUnitString, "Days") == 0)
//			{
//				pw->currentSyncUnit = IFA_SYNC_UNIT_DAYS;
//			}
//	
//			gtk_combo_box_set_active(GTK_COMBO_BOX(pw->syncUnitsComboBox), pw->currentSyncUnit);
//			
//			gtk_spin_button_set_value(GTK_SPIN_BUTTON(pw->syncSpinButton), calculate_sync_spin_value(pw->lastSyncInterval, pw->currentSyncUnit));
//			
//			/* FIXME: figure out if we need to free syncUnitString */
//		}
//	}
//	else
//	{
//		pw->lastSyncInterval = -1;
//		gtk_spin_button_set_value(GTK_SPIN_BUTTON(pw->syncSpinButton), 0);
//	}
	
	if (pw->lastSyncInterval <= 0)
	{
		gtk_toggle_button_set_active(GTK_TOGGLE_BUTTON(pw->autoSyncCheckButton), false);
		gtk_widget_set_sensitive(pw->syncSpinButton, false);
		gtk_widget_set_sensitive(pw->syncUnitsComboBox, false);
	}
	else
	{
		gtk_toggle_button_set_active(GTK_TOGGLE_BUTTON(pw->autoSyncCheckButton), true);
		gtk_widget_set_sensitive(pw->syncSpinButton, true);
		gtk_widget_set_sensitive(pw->syncUnitsComboBox, true);
	}
	
	g_signal_connect(G_OBJECT(pw->autoSyncCheckButton), "toggled", G_CALLBACK(on_auto_sync_button), pw);
	g_signal_connect(G_OBJECT(pw->syncSpinButton), "value-changed", G_CALLBACK(on_sync_interval_changed), pw);
	g_signal_connect(G_OBJECT(pw->syncUnitsComboBox), "changed", G_CALLBACK(on_sync_units_changed), pw);
}

static void
show_conf_button_toggled(GtkToggleButton *togglebutton, IFAPreferencesWindow *pw)
{
	g_message("Implement show_conf_button_toggled()");
/* FIXME: Implement show_conf_button_toggled()
	ifolder_user_pref_set_bool(IFOLDER_PREF_SHOW_CREATION,
				   gtk_toggle_button_get_active(toggleButton));
*/
}

static void
notify_sync_errors_button_toggled(GtkToggleButton *togglebutton, IFAPreferencesWindow *pw)
{
	g_message("FIXME: Implement notify_sync_errors_button_toggled()");
}

static void
on_auto_sync_button(GtkToggleButton *togglebutton, IFAPreferencesWindow *pw)
{
	g_message("FIXME: Implement on_auto_sync_button()");
}

static void
on_sync_interval_changed(GtkSpinButton *spinbutton, IFAPreferencesWindow *pw)
{
	g_message("FIXME: Implement on_sync_interval_changed()");
}

static void
on_sync_units_changed(GtkComboBox *widget, IFAPreferencesWindow *pw)
{
	g_message("FIXME: Implement on_sync_units_changed()");
}

static void
accounts_page_realized(GtkWidget *widget, IFAPreferencesWindow *pw)
{
	g_message ("accounts_page_realized()");

	populate_domains(pw);
	update_widget_sensitivity(pw);
	
	pw->domain_added_cb_id = g_signal_connect (ifolder_client, "domain-added", G_CALLBACK (domain_added_cb), pw);
	pw->domain_removed_cb_id = g_signal_connect (ifolder_client, "domain-removed", G_CALLBACK (domain_removed_cb), pw);
}

static void
online_cell_toggle_data_func(GtkTreeViewColumn *column, GtkCellRenderer *cell, GtkTreeModel *model, GtkTreeIter *iter, IFAPreferencesWindow *pw)
{
	g_message ("online_cell_toggle_data_func()");

	iFolderDomain *domain;
	gtk_tree_model_get (model, iter, 0, &domain, -1);
//	g_object_unref (domain);	/* gtk_tree_model_get() appears to increment the ref count */
	
	if (ifolder_domain_is_authenticated (domain))
		gtk_cell_renderer_toggle_set_active (GTK_CELL_RENDERER_TOGGLE (cell), TRUE);
	else
		gtk_cell_renderer_toggle_set_active (GTK_CELL_RENDERER_TOGGLE (cell), FALSE);
}

static void
online_toggled(GtkCellRendererToggle *cell_renderer, gchar *path, IFAPreferencesWindow *pw)
{
	iFolderDomain *domain;
	GValue val = { 0 };
	GtkTreeIter iter;
	GError *err = NULL;
	GtkWidget *errDialog;
	iFolderPrefsAuthReq *authReq;
	
	if (!gtk_tree_model_get_iter_from_string (GTK_TREE_MODEL (pw->accTreeStore), &iter, path))
	{
		g_debug ("online_toggled() could not get a GtkTreeIter");
		return;
	}
	
	gtk_tree_model_get (GTK_TREE_MODEL (pw->accTreeStore), &iter, 0, &domain, -1);
//	g_object_unref (domain);	/* gtk_tree_model_get() appears to increment the ref count */
	
	// Disable the ability for the user to toggle the checkbox during this operation
	memset (&val, 0, sizeof(GValue));
	g_value_init (&val, G_TYPE_BOOLEAN);
	g_value_set_boolean (&val, FALSE);
	g_object_set_property (G_OBJECT (cell_renderer), "activatable", &val);
	
	authReq = (iFolderPrefsAuthReq *)malloc (sizeof (iFolderPrefsAuthReq));
	authReq->prefs_window = pw;
	authReq->domain = domain;
	authReq->password = NULL;
	authReq->cell_renderer = cell_renderer;
	authReq->error = NULL;
	
	if (ifolder_domain_is_authenticated (domain))
	{
		if (pw->waitDialog == NULL)
		{
			pw->waitDialog = ifa_wait_dialog_new (GTK_WINDOW (pw->window), NULL, IFA_WAIT_DIALOG_NONE, _("Disconnecting..."), _("Disconnecting..."), _("Please wait for your iFolder account to disconnect."));
			gtk_widget_show_all (pw->waitDialog);
		}
	
		g_thread_create((GThreadFunc)log_out_thread, authReq, TRUE, NULL);
	}
	else
	{
		if (pw->waitDialog == NULL)
		{
			pw->waitDialog = ifa_wait_dialog_new (GTK_WINDOW (pw->window), NULL, IFA_WAIT_DIALOG_NONE, _("Connecting..."), _("Connecting..."), _("Please wait for your iFolder account to connect."));
			gtk_widget_show_all (pw->waitDialog);
		}
		
/*
		g_message("FIXME: Retrieve the password");
		if (password == NULL)
		{
			// Prompt the user for the password
			// Read the password and the "remember password" value
		}
		authReq->password = g_strdup (password);
		authReq->rememberPassword = FALSE;
*/
	
		g_thread_create((GThreadFunc)log_in_thread, authReq, TRUE, NULL);
	}
}

static void
server_cell_text_data_func(GtkTreeViewColumn *column, GtkCellRenderer *cell, GtkTreeModel *model, GtkTreeIter *iter, IFAPreferencesWindow *pw)
{
	iFolderDomain *domain;
	GValue val = { 0 };
	gtk_tree_model_get (model, iter, 0, &domain, -1);
//	g_object_unref (domain);	/* gtk_tree_model_get() appears to increment the ref count */
	
	memset (&val, 0, sizeof(GValue));
	g_value_init (&val, G_TYPE_STRING);
	g_value_set_string(&val, ifolder_domain_get_name (domain));
	
	g_object_set_property (G_OBJECT (cell), "text", &val);
}

static void
name_cell_text_data_func(GtkTreeViewColumn *column, GtkCellRenderer *cell, GtkTreeModel *model, GtkTreeIter *iter, IFAPreferencesWindow *pw)
{
	iFolderDomain *domain;
	GValue val = { 0 };
	gtk_tree_model_get (model, iter, 0, &domain, -1);
//	g_object_unref (domain);	/* gtk_tree_model_get() appears to increment the ref count */
	
	memset (&val, 0, sizeof(GValue));
	g_value_init (&val, G_TYPE_STRING);
	g_value_set_string(&val, ifolder_domain_get_user_name (domain));
	
	g_object_set_property (G_OBJECT (cell), "text", &val);
}

static void
acc_selection_changed(GtkTreeSelection *treeselection, IFAPreferencesWindow *pw)
{
	update_widget_sensitivity (pw);
}

static void
on_add_account(GtkButton *widget, IFAPreferencesWindow *pw)
{
	g_message("FIXME: Implement on_add_account()");
	IFAAccountWizard *account_wizard = ifa_account_wizard_new();
	gtk_window_set_transient_for (GTK_WINDOW (account_wizard->window), GTK_WINDOW (pw->window));
	gtk_widget_show_all(account_wizard->window);
}

static void
on_remove_account(GtkButton *widget, IFAPreferencesWindow *pw)
{
	GtkTreeSelection *selection;
	GtkTreeIter iter;
	iFolderDomain *domain;
	GtkWidget *dialog;
	gint rc;
	GtkWidget *errDialog;
	GError *err = NULL;
	
	selection = gtk_tree_view_get_selection (GTK_TREE_VIEW (pw->accTreeView));
	if (gtk_tree_selection_count_selected_rows (selection) == 1)
	{
		if (gtk_tree_selection_get_selected (selection, NULL, &iter))
		{
			gtk_tree_model_get (GTK_TREE_MODEL (pw->accTreeStore), &iter, 0, &domain, -1);
//			g_object_unref (domain);	/* gtk_tree_model_get() appears to increment the ref count */
			
			dialog =
				gtk_message_dialog_new_with_markup (GTK_WINDOW (pw->window),
													(GtkDialogFlags)(GTK_DIALOG_DESTROY_WITH_PARENT | GTK_DIALOG_MODAL),
													GTK_MESSAGE_QUESTION,
													GTK_BUTTONS_YES_NO,
													_("<span weight=\"bold\" size=\"larger\">Remove this account?</span>\n\nIf you select \"Yes,\" the \"%s\" account will be removed from this computer."),
													ifolder_domain_get_name (domain));
			rc = gtk_dialog_run (GTK_DIALOG (dialog));
			gtk_widget_destroy (dialog);
			
			if (rc != GTK_RESPONSE_YES)
				return;
			
			ifolder_client_remove_domain (ifolder_client, domain, &err);
			if (err)
			{
				errDialog = gtk_message_dialog_new (GTK_WINDOW (pw->window), GTK_DIALOG_DESTROY_WITH_PARENT, GTK_MESSAGE_ERROR, GTK_BUTTONS_CLOSE, err->message);
				g_clear_error (&err);
				gtk_dialog_run (GTK_DIALOG (errDialog));
				gtk_widget_destroy (errDialog);
			}
		}
	}
}

static void
on_properties_clicked(GtkButton *widget, IFAPreferencesWindow *pw)
{
	g_message("FIXME: Implement on_properties_clicked()");
}

static void
on_acc_tree_row_activated(GtkTreeView *tree_view, GtkTreePath *path, GtkTreeViewColumn *column, IFAPreferencesWindow *pw)
{
	g_message("FIXME: Implement on_acc_tree_row_activated()");
	on_properties_clicked (GTK_BUTTON (pw->propertiesButton), pw);
}

static void
populate_domains(IFAPreferencesWindow *pw)
{
	GError *err = NULL;
	GSList *domains;
	GSList *domainListPtr;
	iFolderDomain *domain;
	GtkWidget *errDialog;
	gchar *errMessage;
	GtkTreeIter iter;
	
	g_debug ("Calling ifolder_client_get_all_domains() from populate_domains() in preferences-window.c");
	domains = ifolder_client_get_all_domains (ifolder_client, &err);
	if (err)
	{
		errDialog = gtk_message_dialog_new (GTK_WINDOW (pw->window), GTK_DIALOG_DESTROY_WITH_PARENT, GTK_MESSAGE_ERROR, GTK_BUTTONS_CLOSE, err->message);
		g_clear_error (&err);
		gtk_dialog_run (GTK_DIALOG (errDialog));
		gtk_widget_destroy (errDialog);
		return;
	}
	
	domainListPtr = domains;
	
	for (domainListPtr = domains; domainListPtr != NULL; domainListPtr = g_slist_next (domainListPtr))
	{
		domain = (iFolderDomain *)domainListPtr->data;
		g_debug ("id: %s", ifolder_domain_get_id (domain));
		g_debug ("name: %s", ifolder_domain_get_name (domain));
		g_debug ("user name: %s", ifolder_domain_get_user_name (domain));
		g_debug ("authenticated: %s", ifolder_domain_is_authenticated (domain) ? "true" : "false");
		gtk_list_store_append (pw->accTreeStore, &iter);
		gtk_list_store_set (pw->accTreeStore, &iter, 0, domain, -1); 
	}
	
	g_slist_free (domains);
}

static void
update_widget_sensitivity(IFAPreferencesWindow *pw)
{
	GtkTreeSelection *selection;
	GtkTreeIter iter;
	
	selection = gtk_tree_view_get_selection (GTK_TREE_VIEW (pw->accTreeView));
	if (gtk_tree_selection_count_selected_rows (selection) == 1)
	{
//		gtk_widget_set_sensitive (pw->addButton, TRUE);
		gtk_widget_set_sensitive (pw->removeButton, TRUE);
		gtk_widget_set_sensitive (pw->propertiesButton, TRUE);
	}
	else
	{
//		gtk_widget_set_sensitive (pw->addButton, TRUE);
		gtk_widget_set_sensitive (pw->removeButton, FALSE);
		gtk_widget_set_sensitive (pw->propertiesButton, FALSE);
	}
}

static gboolean
g_ifolder_domain_equal(gconstpointer a, gconstpointer b)
{
/*
	if (strcmp(ifolder_domain_get_id(a), ifolder_domain_get_id(b)) == 0)
		return true;
*/

	return false;
}

static gpointer
log_in_thread (iFolderPrefsAuthReq *authReq)
{
	GError *err = NULL;

	ifolder_domain_log_in (authReq->domain, authReq->password, authReq->rememberPassword, &err);
	if (err != NULL)
		authReq->error = err;

	g_idle_add ((GSourceFunc)log_in_thread_completed, authReq);

	return NULL;
}

static gboolean
log_in_thread_completed(iFolderPrefsAuthReq *authReq)
{
	GtkWidget *errDialog;
	GValue val = { 0 };

	g_message ("FIXME: Implement log_in_thread_completed()");

	memset (&val, 0, sizeof(GValue));
	g_value_init (&val, G_TYPE_BOOLEAN);
	g_value_set_boolean (&val, TRUE);
	g_object_set_property (G_OBJECT (authReq->cell_renderer), "activatable", &val);
	
	if (authReq->prefs_window->waitDialog)
	{
		gtk_widget_destroy (authReq->prefs_window->waitDialog);
		authReq->prefs_window->waitDialog = NULL;
	}
	
	if (authReq->error != NULL)
	{
		errDialog = gtk_message_dialog_new (GTK_WINDOW (authReq->prefs_window->window), GTK_DIALOG_DESTROY_WITH_PARENT, GTK_MESSAGE_ERROR, GTK_BUTTONS_CLOSE, authReq->error->message);
		g_error_free (authReq->error);
		gtk_dialog_run (GTK_DIALOG (errDialog));
		gtk_widget_destroy (errDialog);
	}
	else
	{
		g_message ("FIXME: Remove the following line.  It should be set on a log-in event.");
		gtk_cell_renderer_toggle_set_active (GTK_CELL_RENDERER_TOGGLE (authReq->cell_renderer), TRUE);
	}
	
	if (authReq->password != NULL)
		g_free (authReq->password);
	free(authReq);
	return FALSE;
}

static gpointer
log_out_thread (iFolderPrefsAuthReq *authReq)
{
	GError *err = NULL;

	ifolder_domain_log_out (authReq->domain, &err);
	if (err != NULL)
		authReq->error = err;

	g_idle_add ((GSourceFunc)log_out_thread_completed, authReq);
	return NULL;
}

static gboolean
log_out_thread_completed(iFolderPrefsAuthReq *authReq)
{
	GtkWidget *errDialog;
	GValue val = { 0 };

	g_message ("FIXME: Implement log_out_thread_completed()");
	
	memset (&val, 0, sizeof(GValue));
	g_value_init (&val, G_TYPE_BOOLEAN);
	g_value_set_boolean (&val, TRUE);
	g_object_set_property (G_OBJECT (authReq->cell_renderer), "activatable", &val);

	if (authReq->prefs_window->waitDialog)
	{
		gtk_widget_destroy (authReq->prefs_window->waitDialog);
		authReq->prefs_window->waitDialog = NULL;
	}
	
	if (authReq->error != NULL)
	{
		errDialog = gtk_message_dialog_new (GTK_WINDOW (authReq->prefs_window->window), GTK_DIALOG_DESTROY_WITH_PARENT, GTK_MESSAGE_ERROR, GTK_BUTTONS_CLOSE, authReq->error->message);
		g_error_free (authReq->error);
		gtk_dialog_run (GTK_DIALOG (errDialog));
		gtk_widget_destroy (errDialog);
	}
	else
	{
		g_message ("FIXME: Remove the following line.  It should be set on a log-out event.");
		gtk_cell_renderer_toggle_set_active (GTK_CELL_RENDERER_TOGGLE (authReq->cell_renderer), FALSE);
	}

	free(authReq);
	return FALSE;
}

static void
domain_added_cb (iFolderClient *client, iFolderDomain *domain, IFAPreferencesWindow *pw)
{
	GtkTreeIter iter;
	
	g_debug ("id: %s", ifolder_domain_get_id (domain));
	g_debug ("name: %s", ifolder_domain_get_name (domain));
	g_debug ("user name: %s", ifolder_domain_get_user_name (domain));
	g_debug ("authenticated: %s", ifolder_domain_is_authenticated (domain) ? "true" : "false");
	gtk_list_store_insert_with_values (pw->accTreeStore, &iter, 0, 0, domain, -1);
}

static void
domain_removed_cb (iFolderClient *client, iFolderDomain *domain, IFAPreferencesWindow *pw)
{
	GtkTreeIter iter;
	iFolderDomain *tmpDomain;
	
	gtk_tree_model_get_iter_first (GTK_TREE_MODEL (pw->accTreeStore), &iter);
	
	while (gtk_list_store_iter_is_valid (pw->accTreeStore, &iter))
	{
		gtk_tree_model_get (GTK_TREE_MODEL (pw->accTreeStore), &iter, 0, &tmpDomain, -1);
//		g_object_unref (domain);	/* gtk_tree_model_get() appears to increment the ref count */
		
		if (strcmp (ifolder_domain_get_id (tmpDomain), ifolder_domain_get_id (domain)) == 0)
		{
			gtk_list_store_remove (pw->accTreeStore, &iter);
			break;
		}
		
		gtk_tree_model_iter_next (GTK_TREE_MODEL (pw->accTreeStore), &iter);
	}
}

