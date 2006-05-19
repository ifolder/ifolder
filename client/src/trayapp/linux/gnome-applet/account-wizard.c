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

#include <unistd.h>
#include <stdlib.h>
#include <string.h>
#include <gtk/gtk.h>
#include <gdk/gdkkeysyms.h>

//#if !GTK_CHECK_VERSION(2,6,0)
//#include <gnome.h>
//#endif

#include <ifolder-client.h>

#include "account-wizard.h"
#include "util.h"
#include "wait-dialog.h"

/*@todo Remove this when gettext is added */
#define _

extern iFolderClient *ifolder_client;

typedef struct _iFolderAddDomainThreadResults iFolderAddDomainThreadResults;
struct _iFolderAddDomainThreadResults
{
	IFAAccountWizard	*aw;
	iFolderDomain		*domain;
	GError				*error;
};

enum
{
	START_PAGE = 0,
	SERVER_PAGE,
	USER_PAGE,
	CONNECT_PAGE,
	SUMMARY_PAGE
};

/**
 * Forward definitions
 */
static GtkWidget *create_widgets(IFAAccountWizard *aw);
static GtkWidget *create_title_bar(IFAAccountWizard *aw);
static GtkWidget *create_start_page(IFAAccountWizard *aw);
static GtkWidget *create_server_information_page(IFAAccountWizard *aw);
static GtkWidget *create_user_information_page(IFAAccountWizard *aw);
static GtkWidget *create_connect_page(IFAAccountWizard *aw);
static GtkWidget *create_summary_page(IFAAccountWizard *aw);
static GtkWidget *create_buttons(IFAAccountWizard *aw);

static void close_window(IFAAccountWizard *aw);
static void delete_account_wizard(IFAAccountWizard *aw);

/**
 * Signal Handlers
 */
static gboolean key_press_handler(GtkWidget *widget, GdkEventKey *event, IFAAccountWizard *aw);
static gboolean key_release_handler(GtkWidget *widget, GdkEventKey *event, IFAAccountWizard *aw);
static gboolean delete_event_handler(GtkWidget *widget, GdkEvent *event, IFAAccountWizard *aw);

static void notebook_page_switched(GtkNotebook *notebook, GtkNotebookPage *page, guint page_num, IFAAccountWizard *aw);

static void help_button_clicked(GtkButton *button, IFAAccountWizard *aw);
static void cancel_button_clicked(GtkButton *button, IFAAccountWizard *aw);
static void back_button_clicked(GtkButton *button, IFAAccountWizard *aw);
static void forward_button_realized(GtkWidget *widget, IFAAccountWizard *aw);
static void forward_button_clicked(GtkButton *button, IFAAccountWizard *aw);
static void connect_button_clicked(GtkButton *button, IFAAccountWizard *aw);
static void finish_button_clicked(GtkButton *button, IFAAccountWizard *aw);

static void server_name_changed(GtkEntry *entry, IFAAccountWizard *aw);
static void user_info_changed(GtkEntry *entry, IFAAccountWizard *aw);

static gpointer add_domain_thread(IFAAccountWizard *aw);
static gboolean add_domain_thread_completed(iFolderAddDomainThreadResults *results);

IFAAccountWizard *
ifa_account_wizard_new()
{
	IFAAccountWizard *aw = (IFAAccountWizard *)malloc(sizeof(IFAAccountWizard));
	
	aw->window = gtk_window_new(GTK_WINDOW_TOPLEVEL);
	gtk_window_set_title(GTK_WINDOW(aw->window), _("iFolder Account Assistant"));
	gtk_window_set_resizable(GTK_WINDOW(aw->window), FALSE);
	gtk_window_set_modal(GTK_WINDOW(aw->window), TRUE);
	gtk_window_set_position(GTK_WINDOW(aw->window), GTK_WIN_POS_CENTER);
	
	g_message("FIXME: Add window icon to account-wizard");
	
	aw->connectedDomain = NULL;

	gtk_container_add(GTK_CONTAINER(aw->window), create_widgets(aw));
	
	aw->controlKeyPressed = false;
	g_signal_connect(G_OBJECT(aw->window), "key-press-event", G_CALLBACK(key_press_handler), aw);
	g_signal_connect(G_OBJECT(aw->window), "key-release-event", G_CALLBACK(key_release_handler), aw);
	g_signal_connect(G_OBJECT(aw->window), "delete-event", G_CALLBACK(delete_event_handler), aw);
	
	aw->waitDialog = NULL;
	aw->addDomainThread = NULL;

//	gtk_window_set_default_size(GTK_WINDOW(aw->window), 480, 550);

	return aw;
}

static GtkWidget *
create_widgets(IFAAccountWizard *aw)
{
	GtkWidget *winBox = gtk_vbox_new(false, 0);
	gtk_container_set_border_width(GTK_CONTAINER(winBox), 7);

	/**
	 * Page Title Bar
	 */
	gtk_box_pack_start(GTK_BOX(winBox), create_title_bar(aw), false, false, 0);

	/**
	 * Page Content Area
	 */
	aw->notebook = gtk_notebook_new();
	gtk_box_pack_start(GTK_BOX(winBox), aw->notebook, true, true, 0);
	gtk_notebook_set_show_tabs(GTK_NOTEBOOK(aw->notebook), false);
	g_signal_connect(G_OBJECT(aw->notebook), "switch-page", G_CALLBACK(notebook_page_switched), aw);

	gtk_notebook_append_page(GTK_NOTEBOOK(aw->notebook), create_start_page(aw), NULL);
	gtk_notebook_append_page(GTK_NOTEBOOK(aw->notebook), create_server_information_page(aw), NULL);
	gtk_notebook_append_page(GTK_NOTEBOOK(aw->notebook), create_user_information_page(aw), NULL);
	gtk_notebook_append_page(GTK_NOTEBOOK(aw->notebook), create_connect_page(aw), NULL);
	gtk_notebook_append_page(GTK_NOTEBOOK(aw->notebook), create_summary_page(aw), NULL);
	
	/**
	 * Buttons
	 */
	gtk_box_pack_start(GTK_BOX(winBox), create_buttons(aw), false, false, 0);

	return winBox;
}

static GtkWidget *
create_title_bar(IFAAccountWizard *aw)
{
	GtkWidget *pageTitleHBox = gtk_hbox_new(false, 0);

	aw->pageLabel = gtk_label_new(_("<span weight=\"bold\" size=\"x-large\">Configure an iFolder Account</span>"));
	gtk_box_pack_start(GTK_BOX(pageTitleHBox), aw->pageLabel, true, true, 0);
	gtk_misc_set_alignment(GTK_MISC(aw->pageLabel), 0, 0.5);
	gtk_label_set_use_markup(GTK_LABEL(aw->pageLabel), true);
	g_message("FIXME: Change the wizard page label to white and add a blue-ish (themed) background");
	GdkPixbuf *addAccountPixbuf = ifolder_util_load_pixbuf("ifolder-add-account48.png");
	aw->addAccountImage = gtk_image_new_from_pixbuf(addAccountPixbuf);
//	g_object_unref(G_OBJECT(addAccountPixbuf));
	gtk_box_pack_end(GTK_BOX(pageTitleHBox), aw->addAccountImage, false, false, 0);
	
	return pageTitleHBox;
}

static GtkWidget *
create_start_page(IFAAccountWizard *aw)
{
	GtkStyle *default_style;
	GtkWidget *hbox = gtk_hbox_new(false, 0);
	
	g_message("FIXME: Figure out how to get the blue bar on the side on the welcome page of the account wizard");

	GtkWidget *event_box = gtk_event_box_new();
	gtk_box_pack_start(GTK_BOX(hbox), event_box, false, true, 0);
	default_style = gtk_widget_get_style(event_box);
	gtk_widget_modify_base(event_box, GTK_STATE_NORMAL, &(default_style->dark[GTK_STATE_NORMAL]));
	gtk_widget_set_size_request(event_box, 100, -1);

	GtkWidget *label = gtk_label_new(_("Welcome to the iFolder Account Assistant.\n\nClick \"Foward\" to begin."));
	gtk_box_pack_start(GTK_BOX(hbox), label, true, true, 0);
	gtk_misc_set_alignment(GTK_MISC(label), 0, 0.5);
	
	return hbox;
}

static GtkWidget *
create_server_information_page(IFAAccountWizard *aw)
{
	GtkWidget *table = gtk_table_new(4, 3, false);
	gtk_container_set_border_width(GTK_CONTAINER(table), 12);
	gtk_table_set_col_spacings(GTK_TABLE(table), 6);
	gtk_table_set_row_spacings(GTK_TABLE(table), 6);
	
	/**
	 * Row 1
	 */
	GtkWidget *l = gtk_label_new(_("Enter the name of your iFolder Server (for example, \"ifolder.example.net\")."));
	gtk_table_attach(GTK_TABLE(table), l, 0,3, 0,1,
					 (GtkAttachOptions)(GTK_FILL | GTK_EXPAND),
					 (GtkAttachOptions)0,
					 0,0);
	gtk_label_set_line_wrap(GTK_LABEL(l), true);
	gtk_misc_set_alignment(GTK_MISC(l), 0, 0.5);
	
	/**
	 * Row 2
	 */
	gtk_table_attach(GTK_TABLE(table), gtk_label_new(""), 0,1, 1,2,
					(GtkAttachOptions)GTK_FILL,
					(GtkAttachOptions)0,
					12,0);	/* spacer */
	l = gtk_label_new_with_mnemonic(_("Server _Address:"));
	gtk_table_attach(GTK_TABLE(table), l, 1,2, 1,2,
					(GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					(GtkAttachOptions)0,
					0,0);
	gtk_misc_set_alignment(GTK_MISC(l), 0, 0.5);
	
	aw->serverNameEntry = gtk_entry_new();
	gtk_table_attach(GTK_TABLE(table), aw->serverNameEntry, 2,3, 1,2,
					(GtkAttachOptions)(GTK_FILL | GTK_EXPAND),
					(GtkAttachOptions)0,
					0,0);
	gtk_label_set_mnemonic_widget(GTK_LABEL(l), aw->serverNameEntry);
	gtk_entry_set_activates_default(GTK_ENTRY(aw->serverNameEntry), true);

	if (ifolder_user_pref_get_bool(KEY_USER_PREF_PREFILL_ACCOUNT))
		gtk_entry_set_text(GTK_ENTRY(aw->serverNameEntry), ifolder_user_pref_get_string(KEY_USER_PREF_ACCOUNT_SERVER_ADDRESS, ""));
	
	g_signal_connect(G_OBJECT(aw->serverNameEntry), "changed", G_CALLBACK(server_name_changed), aw);
	
	/**
	 * Row 3
	 */
	aw->makeDefaultLabel = gtk_label_new(_("Setting this iFolder Server as your default server will allow iFolder to automatically select this server when adding new folders."));
	gtk_table_attach(GTK_TABLE(table), aw->makeDefaultLabel, 0,3, 2,3,
					(GtkAttachOptions)(GTK_FILL | GTK_EXPAND),
					(GtkAttachOptions)0,
					0,0);
	gtk_label_set_line_wrap(GTK_LABEL(aw->makeDefaultLabel), true);
	gtk_misc_set_alignment(GTK_MISC(aw->makeDefaultLabel), 0, 0.5);
	
	/**
	 * Row 4
	 */
	aw->defaultServerCheckButton = gtk_check_button_new_with_mnemonic(_("Make this my _default server"));
	gtk_table_attach(GTK_TABLE(table), aw->defaultServerCheckButton, 1,3, 3,4,
					(GtkAttachOptions)(GTK_FILL | GTK_EXPAND),
					(GtkAttachOptions)0,
					0,0);
	
	g_message("FIXME: If the number of domains/accounts on the machine is 0, check the checkbutton and disable it");

	return table;
}

static GtkWidget *
create_user_information_page(IFAAccountWizard *aw)
{
	GtkWidget *table = gtk_table_new(4, 3, false);
	gtk_container_set_border_width(GTK_CONTAINER(table), 12);
	gtk_table_set_col_spacings(GTK_TABLE(table), 6);
	gtk_table_set_row_spacings(GTK_TABLE(table), 6);
	
	/**
	 * Row 1
	 */
	GtkWidget *l = gtk_label_new(_("Enter your iFolder user name and password (for example, \"jsmith\")."));
	gtk_table_attach(GTK_TABLE(table), l, 0,3, 0,1,
					(GtkAttachOptions)(GTK_FILL | GTK_EXPAND),
					(GtkAttachOptions)0,
					0,0);
	gtk_label_set_line_wrap(GTK_LABEL(l), true);
	gtk_misc_set_alignment(GTK_MISC(l), 0, 0.5);

	/**
	 * Row 2
	 */
	gtk_table_attach(GTK_TABLE(table), gtk_label_new(""), 0,1, 1,2,
					(GtkAttachOptions)GTK_FILL,
					(GtkAttachOptions)0,
					12,0);	/* spacer */
	l = gtk_label_new_with_mnemonic(_("_User Name:"));
	gtk_table_attach(GTK_TABLE(table), l, 1,2, 1,2,
					(GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					(GtkAttachOptions)0,
					0,0);
	gtk_misc_set_alignment(GTK_MISC(l), 0, 0.5);
	aw->userNameEntry = gtk_entry_new();
	gtk_table_attach(GTK_TABLE(table), aw->userNameEntry, 2,3, 1,2,
					(GtkAttachOptions)(GTK_FILL | GTK_EXPAND),
					(GtkAttachOptions)0,
					0,0);
	gtk_label_set_mnemonic_widget(GTK_LABEL(l), aw->userNameEntry);
	gtk_entry_set_activates_default(GTK_ENTRY(aw->userNameEntry), true);

	if (ifolder_user_pref_get_bool(KEY_USER_PREF_PREFILL_ACCOUNT))
		gtk_entry_set_text(GTK_ENTRY(aw->userNameEntry), ifolder_user_pref_get_string(KEY_USER_PREF_ACCOUNT_USER_NAME, ""));

	g_signal_connect(G_OBJECT(aw->userNameEntry), "changed", G_CALLBACK(user_info_changed), aw);

	// Row 3
	l = gtk_label_new_with_mnemonic(_("_Password:"));
	gtk_table_attach(GTK_TABLE(table), l, 1,2, 2,3,
					(GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					(GtkAttachOptions)0,
					0,0);
	gtk_misc_set_alignment(GTK_MISC(l), 0, 0.5);
	aw->passwordEntry = gtk_entry_new();
	gtk_table_attach(GTK_TABLE(table), aw->passwordEntry, 2,3, 2,3,
					(GtkAttachOptions)(GTK_FILL | GTK_EXPAND),
					(GtkAttachOptions)0,
					0,0);
	gtk_label_set_mnemonic_widget(GTK_LABEL(l), aw->passwordEntry);
	gtk_entry_set_activates_default(GTK_ENTRY(aw->passwordEntry), true);
	gtk_entry_set_visibility(GTK_ENTRY(aw->passwordEntry), false);
	if (ifolder_user_pref_get_bool(KEY_USER_PREF_PREFILL_ACCOUNT))
		gtk_entry_set_text(GTK_ENTRY(aw->passwordEntry), ifolder_user_pref_get_string(KEY_USER_PREF_ACCOUNT_PASSWORD, ""));

	g_signal_connect(G_OBJECT(aw->passwordEntry), "changed", G_CALLBACK(user_info_changed), aw);

	// Row 4
	aw->rememberPasswordCheckButton = gtk_check_button_new_with_mnemonic(_("_Remember my password"));
	gtk_table_attach(GTK_TABLE(table), aw->rememberPasswordCheckButton, 2,3, 3,4,
					(GtkAttachOptions)(GTK_FILL | GTK_EXPAND),
					(GtkAttachOptions)0,
					0,0);
	if (ifolder_user_pref_get_bool(KEY_USER_PREF_PREFILL_ACCOUNT))
		gtk_toggle_button_set_active(GTK_TOGGLE_BUTTON(aw->rememberPasswordCheckButton), ifolder_user_pref_get_bool(KEY_USER_PREF_ACCOUNT_REMEMBER_PASSWORD, false));
		
	return table;	
}

static GtkWidget *
create_connect_page(IFAAccountWizard *aw)
{
	GtkWidget *table = gtk_table_new(6, 3, false);
	gtk_container_set_border_width(GTK_CONTAINER(table), 12);
	gtk_table_set_col_spacings(GTK_TABLE(table), 6);
	gtk_table_set_row_spacings(GTK_TABLE(table), 6);

	// Row 1
	GtkWidget *l = gtk_label_new(_("Please verify that the information you've entered is correct."));
	gtk_table_attach(GTK_TABLE(table), l, 0,3, 0,1,
					(GtkAttachOptions)(GTK_FILL | GTK_EXPAND),
					(GtkAttachOptions)0,
					0,0);
	gtk_label_set_line_wrap(GTK_LABEL(l), true);
	gtk_misc_set_alignment(GTK_MISC(l), 0, 0.5);

	// Row 2
	gtk_table_attach(GTK_TABLE(table), gtk_label_new(""), 0,1, 1,2,
					(GtkAttachOptions)GTK_FILL,
					(GtkAttachOptions)0,
					12,0);	/* spacer */
	l = gtk_label_new(_("Server Address:"));
	gtk_table_attach(GTK_TABLE(table), l, 1,2, 1,2,
					(GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					(GtkAttachOptions)0,
					0,0);
	gtk_misc_set_alignment(GTK_MISC(l), 0, 0.5);
	aw->serverNameVerifyLabel = gtk_label_new("");
	gtk_table_attach(GTK_TABLE(table), aw->serverNameVerifyLabel, 2,3, 1,2,
					(GtkAttachOptions)(GTK_FILL | GTK_EXPAND),
					(GtkAttachOptions)0,
					0,0);
	gtk_misc_set_alignment(GTK_MISC(aw->serverNameVerifyLabel), 0, 0.5);

	// Row 3
	l = gtk_label_new(_("User Name:"));
	gtk_table_attach(GTK_TABLE(table), l, 1,2, 2,3,
					(GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					(GtkAttachOptions)0,
					0,0);
	gtk_misc_set_alignment(GTK_MISC(l), 0, 0.5);
	aw->userNameVerifyLabel = gtk_label_new("");
	gtk_table_attach(GTK_TABLE(table), aw->userNameVerifyLabel, 2,3, 2,3,
					(GtkAttachOptions)(GTK_FILL | GTK_EXPAND),
					(GtkAttachOptions)0,
					0,0);
	gtk_misc_set_alignment(GTK_MISC(aw->userNameVerifyLabel), 0, 0.5);
	
	// Row 4
	l = gtk_label_new(_("Remember password:"));
	gtk_table_attach(GTK_TABLE(table), l, 1,2, 3,4,
					(GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					(GtkAttachOptions)0,
					0,0);
	gtk_misc_set_alignment(GTK_MISC(l), 0, 0.5);
	aw->rememberPasswordVerifyLabel = gtk_label_new("");
	gtk_table_attach(GTK_TABLE(table), aw->rememberPasswordVerifyLabel, 2,3, 3,4,
					(GtkAttachOptions)(GTK_FILL | GTK_EXPAND),
					(GtkAttachOptions)0,
					0,0);
	gtk_misc_set_alignment(GTK_MISC(aw->rememberPasswordVerifyLabel), 0, 0.5);
	
	// Row 5
	aw->makeDefaultPromptLabel = gtk_label_new(_("Make default account:"));
	gtk_table_attach(GTK_TABLE(table), aw->makeDefaultPromptLabel, 1,2, 4,5,
					(GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					(GtkAttachOptions)0,
					0,0);
	gtk_misc_set_alignment(GTK_MISC(aw->makeDefaultPromptLabel), 0, 0.5);
	aw->makeDefaultVerifyLabel = gtk_label_new("");
	gtk_table_attach(GTK_TABLE(table), aw->makeDefaultVerifyLabel, 2,3, 4,5,
					(GtkAttachOptions)(GTK_FILL | GTK_EXPAND),
					(GtkAttachOptions)0,
					0,0);
	gtk_misc_set_alignment(GTK_MISC(aw->makeDefaultVerifyLabel), 0, 0.5);
	
	// Row 6
	l = gtk_label_new(_("\n\nClick \"Connect\" to validate your connection with the server."));
	gtk_table_attach(GTK_TABLE(table), l, 0,3, 5,6,
					(GtkAttachOptions)(GTK_FILL | GTK_EXPAND),
					(GtkAttachOptions)0,
					0,0);
	gtk_misc_set_alignment(GTK_MISC(l), 0, 0.5);
	gtk_label_set_line_wrap(GTK_LABEL(l), true);

	return table;	
}

static GtkWidget *
create_summary_page(IFAAccountWizard *aw)
{
	GtkStyle *default_style;
	GtkWidget *hbox = gtk_hbox_new(false, 0);
	
	g_message("FIXME: Figure out how to get the blue bar on the side on the summary page of the account wizard");

	GtkWidget *event_box = gtk_event_box_new();
	gtk_box_pack_start(GTK_BOX(hbox), event_box, false, true, 0);
	default_style = gtk_widget_get_style(event_box);
	gtk_widget_modify_base(event_box, GTK_STATE_NORMAL, &(default_style->dark[GTK_STATE_NORMAL]));
	gtk_widget_set_size_request(event_box, 100, -1);

	aw->summaryPageTextLabel = gtk_label_new("");
	gtk_box_pack_start(GTK_BOX(hbox), aw->summaryPageTextLabel, true, true, 0);
	gtk_misc_set_alignment(GTK_MISC(aw->summaryPageTextLabel), 0, 0.5);
	
	return hbox;
}

static GtkWidget *
create_buttons(IFAAccountWizard *aw)
{
	GtkWidget *buttonBox = gtk_hbutton_box_new();

	aw->helpButton = gtk_button_new_from_stock(GTK_STOCK_HELP);
	gtk_box_pack_start(GTK_BOX(buttonBox), aw->helpButton, false, false, 0);
	g_signal_connect(G_OBJECT(aw->helpButton), "clicked", G_CALLBACK(help_button_clicked), aw);

	aw->cancelButton = gtk_button_new_from_stock(GTK_STOCK_CANCEL);
	gtk_box_pack_start(GTK_BOX(buttonBox), aw->cancelButton, false, false, 0);
	g_signal_connect(G_OBJECT(aw->cancelButton), "clicked", G_CALLBACK(cancel_button_clicked), aw);

	aw->backButton = gtk_button_new_from_stock(GTK_STOCK_GO_BACK);
	gtk_box_pack_start(GTK_BOX(buttonBox), aw->backButton, false, false, 0);
	gtk_widget_set_sensitive(aw->backButton, false);
	g_signal_connect(G_OBJECT(aw->backButton), "clicked", G_CALLBACK(back_button_clicked), aw);

	aw->forwardButton = gtk_button_new_from_stock(GTK_STOCK_GO_FORWARD);
	gtk_box_pack_start(GTK_BOX(buttonBox), aw->forwardButton, false, false, 0);
	GTK_WIDGET_SET_FLAGS(aw->forwardButton, GTK_CAN_DEFAULT);
	g_signal_connect(G_OBJECT(aw->forwardButton), "realize", G_CALLBACK(forward_button_realized), aw);
	g_signal_connect(G_OBJECT(aw->forwardButton), "clicked", G_CALLBACK(forward_button_clicked), aw);

	aw->connectButton = gtk_button_new_with_mnemonic(_("Co_nnect"));
	gtk_box_pack_start(GTK_BOX(buttonBox), aw->connectButton, false, false, 0);
	gtk_widget_set_no_show_all(aw->connectButton, true);
	GTK_WIDGET_SET_FLAGS(aw->connectButton, GTK_CAN_DEFAULT);
	g_signal_connect(G_OBJECT(aw->connectButton), "clicked", G_CALLBACK(connect_button_clicked), aw);

	aw->finishButton = gtk_button_new_with_mnemonic(_("_Finish"));
	gtk_box_pack_start(GTK_BOX(buttonBox), aw->finishButton, false, false, 0);
	gtk_widget_set_no_show_all(aw->finishButton, true);
	GTK_WIDGET_SET_FLAGS(aw->finishButton, GTK_CAN_DEFAULT);
	g_signal_connect(G_OBJECT(aw->finishButton), "clicked", G_CALLBACK(finish_button_clicked), aw);
	
	return buttonBox;
}

static gboolean
key_press_handler(GtkWidget *widget, GdkEventKey *event, IFAAccountWizard *aw)
{
	gboolean stop_other_handlers = true;

	switch(event->keyval)
	{
		case GDK_Escape:
			close_window(aw);
			break;
		case GDK_Control_L:
		case GDK_Control_R:
			aw->controlKeyPressed = true;
			stop_other_handlers = false;
			break;
		case GDK_W:
		case GDK_w:
			if (aw->controlKeyPressed)
				close_window(aw);
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
key_release_handler(GtkWidget *widget, GdkEventKey *event, IFAAccountWizard *aw)
{
	gboolean stop_other_handlers = false;

	switch(event->keyval)
	{
		case GDK_Control_L:
		case GDK_Control_R:
			aw->controlKeyPressed = false;
			break;
		default:
			break;
	}

	return stop_other_handlers;
}

static gboolean
delete_event_handler(GtkWidget *widget, GdkEvent *event, IFAAccountWizard *aw)
{
	close_window(aw);
	
	return TRUE; /* stop other handlers from being invoked */
}

static void
close_window(IFAAccountWizard *aw)
{
	if (!aw)
		return;

	gtk_widget_hide(aw->window);
	gtk_widget_destroy(aw->window);

	delete_account_wizard(aw);
}

static void
delete_account_wizard(IFAAccountWizard *aw)
{
	if (aw->connectedDomain != NULL)
	{
		g_debug ("FIXME: Should we be calling g_object_unref here?");
		g_debug ("account-wizard: g_object_unref (domain)");
		g_object_unref(aw->connectedDomain);
	}
	
	free(aw);
}

static void
notebook_page_switched(GtkNotebook *notebook, GtkNotebookPage *page, guint page_num, IFAAccountWizard *aw)
{
	if (gtk_notebook_get_current_page(notebook) < 0)
	{
		/**
		 * For whatever reason, this method gets called for each page when the
		 * notebook is first displayed.  gtk_notebook_get_current_page()
		 * returns -1 in this case and we can ignore it.
		 */

		return;
	}

	switch(page_num)
	{
		GString *summary_text;
		
		case START_PAGE:
			gtk_window_set_title(GTK_WINDOW(aw->window), _("iFolder Account Assistant"));
			gtk_widget_set_sensitive(aw->backButton, false);
			gtk_widget_set_sensitive(aw->forwardButton, true);
			
			gtk_widget_show(aw->backButton);
			gtk_widget_show(aw->forwardButton);
			gtk_widget_hide(aw->connectButton);
			gtk_widget_hide(aw->finishButton);
			
			gtk_widget_grab_default(aw->forwardButton);
			gtk_widget_grab_focus(aw->forwardButton);

			break;
		case SERVER_PAGE:
			gtk_window_set_title(GTK_WINDOW(aw->window), _("iFolder Account Assistant - (1 of 3)"));
			gtk_widget_set_sensitive(aw->backButton, true);
			gtk_widget_set_sensitive(aw->forwardButton, false);
			
			gtk_widget_show(aw->backButton);
			gtk_widget_show(aw->forwardButton);
			gtk_widget_hide(aw->connectButton);
			gtk_widget_hide(aw->finishButton);
			
			g_message("FIXME: Disable the 'default account' feature if there aren't any accounts set up");
			
			server_name_changed(GTK_ENTRY(aw->serverNameEntry), aw);
			
			gtk_widget_grab_default(aw->forwardButton);
			gtk_widget_grab_focus(aw->serverNameEntry);

			break;
		case USER_PAGE:
			gtk_window_set_title(GTK_WINDOW(aw->window), _("iFolder Account Assistant - (2 of 3)"));
			gtk_widget_set_sensitive(aw->backButton, true);
			gtk_widget_set_sensitive(aw->forwardButton, false);
			
			gtk_widget_show(aw->backButton);
			gtk_widget_show(aw->forwardButton);
			gtk_widget_hide(aw->connectButton);
			gtk_widget_hide(aw->finishButton);
			
			user_info_changed(GTK_ENTRY(aw->userNameEntry), aw);

			gtk_widget_grab_default(aw->forwardButton);
			gtk_widget_grab_focus(aw->userNameEntry);

			break;
		case CONNECT_PAGE:
			gtk_window_set_title(GTK_WINDOW(aw->window), _("iFolder Account Assistant - (3 of 3)"));
			gtk_widget_set_sensitive(aw->backButton, true);
			gtk_widget_set_sensitive(aw->connectButton, true);
			
			gtk_widget_show(aw->backButton);
			gtk_widget_hide(aw->forwardButton);
			gtk_widget_show(aw->connectButton);
			gtk_widget_hide(aw->finishButton);

			gtk_label_set_text(GTK_LABEL(aw->serverNameVerifyLabel), gtk_entry_get_text(GTK_ENTRY(aw->serverNameEntry)));
			gtk_label_set_text(GTK_LABEL(aw->userNameVerifyLabel), gtk_entry_get_text(GTK_ENTRY(aw->userNameEntry)));
			if (gtk_toggle_button_get_active(GTK_TOGGLE_BUTTON(aw->rememberPasswordCheckButton)))
				gtk_label_set_text(GTK_LABEL(aw->rememberPasswordVerifyLabel), _("Yes"));
			else
				gtk_label_set_text(GTK_LABEL(aw->rememberPasswordVerifyLabel), _("No"));
			if (gtk_toggle_button_get_active(GTK_TOGGLE_BUTTON(aw->defaultServerCheckButton)))
				gtk_label_set_text(GTK_LABEL(aw->makeDefaultVerifyLabel), _("Yes"));
			else
				gtk_label_set_text(GTK_LABEL(aw->makeDefaultVerifyLabel), _("No"));
				
			g_message("FIXME: Determine the number accounts that are set up");
//			if (domains.Length > 0)
//			{
				gtk_widget_show(aw->makeDefaultPromptLabel);
				gtk_widget_show(aw->makeDefaultVerifyLabel);
//			}
//			else
//			{
//				gtk_widget_hide(aw->makeDefaultPromptLabel);
//				gtk_widget_hide(aw->makeDefaultVerifyLabel);
//			}
			gtk_widget_grab_default(aw->connectButton);
			gtk_widget_grab_focus(aw->connectButton);
			
			break;
		case SUMMARY_PAGE:
			gtk_window_set_title(GTK_WINDOW(aw->window), _("iFolder Account Assistant"));

			if (aw->connectedDomain == NULL)
				g_critical("domain is NULL!");
			
			summary_text = g_string_new("");
			g_string_printf(summary_text,
							_("Congratulations!  You are now\nconnected.\n\n\tAccount Name: %s\n\tServer Address: %s\n\tUser Name: %s\n\nClick \"Finish\" to close this window."),
							ifolder_domain_get_name(aw->connectedDomain),
							ifolder_domain_get_host_address(aw->connectedDomain),
							gtk_entry_get_text(GTK_ENTRY(aw->userNameEntry)));
			gtk_label_set_text(GTK_LABEL(aw->summaryPageTextLabel), summary_text->str);
			g_string_free(summary_text, true);

			gtk_widget_set_sensitive(aw->cancelButton, false);
			gtk_widget_set_sensitive(aw->backButton, false);
			gtk_widget_set_sensitive(aw->finishButton, true);
			
			gtk_widget_show(aw->backButton);
			gtk_widget_hide(aw->forwardButton);
			gtk_widget_hide(aw->connectButton);
			gtk_widget_show(aw->finishButton);

			gtk_widget_grab_default(aw->finishButton);
			gtk_widget_grab_focus(aw->finishButton);

			break;
		default:
			g_message("Unknown page!");
			break;
	}
}

static void
help_button_clicked(GtkButton *button, IFAAccountWizard *aw)
{
	ifa_show_help(IFA_HELP_ACCOUNTS_PAGE);
}

static void
cancel_button_clicked(GtkButton *button, IFAAccountWizard *aw)
{
	close_window(aw);
}

static void
back_button_clicked(GtkButton *button, IFAAccountWizard *aw)
{
	gtk_notebook_prev_page(GTK_NOTEBOOK(aw->notebook));
}

static void
forward_button_realized(GtkWidget *widget, IFAAccountWizard *aw)
{
	gtk_widget_grab_focus(widget);
}

static void
forward_button_clicked(GtkButton *button, IFAAccountWizard *aw)
{
	/**
	 * We don't have to do any validation before allowing the page to be
	 * switched because the forward button is not enabled unless the fields
	 * are valid.
	 */
	gtk_notebook_next_page(GTK_NOTEBOOK(aw->notebook));
}

static void
connect_button_clicked(GtkButton *button, IFAAccountWizard *aw)
{
	GThread *addDomainThread;
	GtkWidget *errDialog;
	
	/**
	 * 		if (connect is successful)
	 * 			gtk_notebook_next_page(GTK_NOTEBOOK(aw->notebook));
	 */
	
	if (aw->waitDialog == NULL)
	{
		aw->waitDialog = ifa_wait_dialog_new (GTK_WINDOW (aw->window), NULL, IFA_WAIT_DIALOG_NONE, _("Connecting..."), _("Connecting..."), _("Please wait while your iFolder account is connecting."));
		gtk_widget_show_all (aw->waitDialog);
	}
	
	aw->addDomainThread = g_thread_create((GThreadFunc)add_domain_thread, aw, TRUE, NULL);
	if (aw->addDomainThread == NULL)
	{
		errDialog = gtk_message_dialog_new (GTK_WINDOW (aw->window), GTK_DIALOG_DESTROY_WITH_PARENT, GTK_MESSAGE_ERROR, GTK_BUTTONS_CLOSE, _("Error creating thread to log in."));
		gtk_dialog_run (GTK_DIALOG (errDialog));
		gtk_widget_destroy (errDialog);
		
		gtk_widget_destroy (aw->waitDialog);
		aw->waitDialog = NULL;
		
		return;
	}
}

static void
finish_button_clicked(GtkButton *button, IFAAccountWizard *aw)
{
	close_window(aw);
	g_message("FIXME: Cause the main ifolder window to show");
}

static void
server_name_changed(GtkEntry *entry, IFAAccountWizard *aw)
{
	const gchar *current_server_name = gtk_entry_get_text(entry);
	
	g_message("FIXME: Add in the ability to trim whitespace off the front and end of the server address string to validate it's good.");
	
	if (current_server_name != NULL)
	{
		if (strlen(current_server_name) > 0)
			gtk_widget_set_sensitive(aw->forwardButton, true);
		else
			gtk_widget_set_sensitive(aw->forwardButton, false);
	}
	else
		gtk_widget_set_sensitive(aw->forwardButton, false);
}

static void
user_info_changed(GtkEntry *entry, IFAAccountWizard *aw)
{
	const gchar *current_user_name = gtk_entry_get_text(GTK_ENTRY(aw->userNameEntry));
	const gchar *current_password  = gtk_entry_get_text(GTK_ENTRY(aw->passwordEntry));
	
	if (current_user_name != NULL && current_password != NULL)
	{
		if (strlen(current_user_name) > 0 && strlen(current_password) > 0)
			gtk_widget_set_sensitive(aw->forwardButton, true);
		else
			gtk_widget_set_sensitive(aw->forwardButton, false);
	}
	else
		gtk_widget_set_sensitive(aw->forwardButton, false);
}

static gpointer
add_domain_thread(IFAAccountWizard *aw)
{
	iFolderDomain *domain;
	const gchar *host_address;
	const gchar *user_name;
	const gchar *password;
	gboolean b_remember_password;
	gboolean b_make_default;
	GError *err = NULL;
	GtkWidget *errDialog;
	iFolderAddDomainThreadResults *results;
	
	host_address = gtk_entry_get_text (GTK_ENTRY (aw->serverNameEntry));
	user_name = gtk_entry_get_text (GTK_ENTRY (aw->userNameEntry));
	password = gtk_entry_get_text (GTK_ENTRY (aw->passwordEntry));
	b_remember_password = gtk_toggle_button_get_active (GTK_TOGGLE_BUTTON (aw->rememberPasswordCheckButton));
	b_make_default = gtk_toggle_button_get_active (GTK_TOGGLE_BUTTON (aw->defaultServerCheckButton));

	domain = ifolder_client_add_domain (ifolder_client, host_address, user_name, password, b_remember_password, b_make_default, &err);
	
	results = (iFolderAddDomainThreadResults *)malloc (sizeof (iFolderAddDomainThreadResults));
	results->aw = aw;
	results->domain = domain;
	results->error = err;
	
	g_idle_add((GSourceFunc)add_domain_thread_completed, results);

	return NULL;
}

static gboolean
add_domain_thread_completed(iFolderAddDomainThreadResults *results)
{
	GtkWidget *errDialog;
	gint rc;

	g_message ("FIXME: Implement add_domain_thread_completed()");

	if (results->aw->waitDialog != NULL)
	{
		gtk_widget_destroy (results->aw->waitDialog);
		results->aw->waitDialog = NULL;
	}
	
	if (results->error != NULL)
	{
		switch (results->error->code)
		{
			case IFOLDER_AUTH_SUCCESS_IN_GRACE:
				// FIXME: let the user know about their grace logins and then advance them to the summary page
				errDialog = gtk_message_dialog_new (GTK_WINDOW (results->aw->window), GTK_DIALOG_DESTROY_WITH_PARENT, GTK_MESSAGE_WARNING, GTK_BUTTONS_CLOSE, results->error->message);
				gtk_dialog_run (GTK_DIALOG (errDialog));
				gtk_widget_destroy (errDialog);
				
				gtk_notebook_next_page(GTK_NOTEBOOK (results->aw->notebook));
				break;
			case IFOLDER_AUTH_ERR_INVALID_CERTIFICATE:
				errDialog =
					gtk_message_dialog_new_with_markup (GTK_WINDOW (results->aw->window),
														(GtkDialogFlags)(GTK_DIALOG_DESTROY_WITH_PARENT | GTK_DIALOG_MODAL),
														GTK_MESSAGE_QUESTION,
														GTK_BUTTONS_YES_NO,
														_("<span weight=\"bold\" size=\"larger\">FIXME: Invalid Certificate.  Accept?</span>"));
				rc = gtk_dialog_run (GTK_DIALOG (errDialog));
				gtk_widget_destroy (errDialog);
				
				if (rc == GTK_RESPONSE_YES)
				{
					g_message ("FIXME: User selected \"Yes\" so store the certificate and cause login to be called again!");

					connect_button_clicked (GTK_BUTTON (results->aw->connectButton), results->aw);
				}

				break;
			case IFOLDER_AUTH_ERR_UNKNOWN_USER:
			case IFOLDER_AUTH_ERR_AMBIGUOUS_USER:
			case IFOLDER_AUTH_ERR_INVALID_CREDENTIALS:
			case IFOLDER_AUTH_ERR_INVALID_PASSWORD:
			case IFOLDER_AUTH_ERR_ACCOUNT_DISABLED:
			case IFOLDER_AUTH_ERR_ACCOUNT_LOCKOUT:
			case IFOLDER_AUTH_ERR_SIMIAS_LOGIN_DISABLED:
			case IFOLDER_AUTH_ERR_UNKNOWN_DOMAIN:
			case IFOLDER_AUTH_ERR_INTERNAL_EXCEPTION:
			case IFOLDER_AUTH_ERR_TIMEOUT:
			case IFOLDER_AUTH_ERR_UNKNOWN:
			default:
				errDialog = gtk_message_dialog_new (GTK_WINDOW (results->aw->window), GTK_DIALOG_DESTROY_WITH_PARENT, GTK_MESSAGE_ERROR, GTK_BUTTONS_CLOSE, results->error->message);
				gtk_dialog_run (GTK_DIALOG (errDialog));
				gtk_widget_destroy (errDialog);
				break;
		}
		
		g_error_free (results->error);
	}
	else
	{
		if (results->domain == NULL)
		{
			errDialog = gtk_message_dialog_new (GTK_WINDOW (results->aw->window), GTK_DIALOG_DESTROY_WITH_PARENT, GTK_MESSAGE_ERROR, GTK_BUTTONS_CLOSE, _("An unknown problem occurred (domain is NULL without an error being reported)."));
			gtk_dialog_run (GTK_DIALOG (errDialog));
			gtk_widget_destroy (errDialog);
		}
		else
		{
			g_debug ("account-wizard: g_object_ref (domain)");
			g_object_ref (results->domain);
			results->aw->connectedDomain = results->domain;
			
			gtk_notebook_next_page(GTK_NOTEBOOK(results->aw->notebook));
		}
	}
	
	return FALSE;
}
