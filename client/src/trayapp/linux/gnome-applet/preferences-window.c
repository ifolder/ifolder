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

#include <string.h>
#include <gtk/gtk.h>
#include <gdk/gdkkeysyms.h>

//#if !GTK_CHECK_VERSION(2,6,0)
//#include <gnome.h>
//#endif

#include <ifolder-client.h>

#include "preferences-window.h"
#include "util.h"

/*@todo Remove this when gettext is added */
#define _

static IFAPreferencesWindow *prefsWindow = NULL;

static IFAPreferencesWindow *create_preferences_window();
static void delete_preferences_window(IFAPreferencesWindow **pw);
static gboolean key_press_handler(GtkWidget *widget, GdkEventKey *event, IFAPreferencesWindow *pw);
static gboolean key_release_handler(GtkWidget *widget, GdkEventKey *event, IFAPreferencesWindow *pw);
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
	IFAPreferencesWindow *pw = malloc(sizeof(IFAPreferencesWindow));

	pw->window = gtk_window_new(GTK_WINDOW_TOPLEVEL);
	gtk_window_set_title(GTK_WINDOW(pw->window), _("iFolder Preferences"));
	pw->controlKeyPressed = false;
	g_signal_connect(G_OBJECT(pw->window), "key-press-event", G_CALLBACK(key_press_handler), pw);
	g_signal_connect(G_OBJECT(pw->window), "key-release-event", G_CALLBACK(key_release_handler), pw);

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

	return pw;
}

static gboolean
key_press_handler(GtkWidget *widget, GdkEventKey *event, IFAPreferencesWindow *pw)
{
	gboolean stop_other_handlers = true;
	g_message("Key pressed inside the preferences window");

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
	
	GtkObject *adjustment = gtk_adjustment_new(1, 1, 5000, 1, 1, 1); /* FIXME: Replace 5000 with Int32.MaxValue */
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
	return gtk_label_new("Accounts");
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

static void
close_window()
{
	if (!prefsWindow)
		return;

	if (gtk_notebook_get_current_page(GTK_NOTEBOOK(prefsWindow->notebook)) == 0)
		leaving_general_page();

	gtk_widget_hide(prefsWindow->window);
	gtk_widget_destroy(prefsWindow->window);

	delete_preferences_window(&prefsWindow);
}

static void
delete_preferences_window(IFAPreferencesWindow **pw)
{
	/* FIXME: Free memory used by structure members */
	free(*pw);
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
//				gtk_combo_box_set_range(GTK_SPIN_BUTTON(pw->syncSpinButton), 60, 5000); /* FIXME: Set max to Int32.MaxValue */
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

