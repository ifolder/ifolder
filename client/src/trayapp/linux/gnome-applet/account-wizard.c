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

/*@todo Remove this when gettext is added */
#define _

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
static void forward_button_clicked(GtkButton *button, IFAAccountWizard *aw);
static void connect_button_clicked(GtkButton *button, IFAAccountWizard *aw);
static void finish_button_clicked(GtkButton *button, IFAAccountWizard *aw);

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
	g_message("FIXME: Implement account-wizard->waitDialog");
//	aw->waitDialog = NULL;

	gtk_container_add(GTK_CONTAINER(aw->window), create_widgets(aw));
	
	aw->controlKeyPressed = false;
	g_signal_connect(G_OBJECT(aw->window), "key-press-event", G_CALLBACK(key_press_handler), aw);
	g_signal_connect(G_OBJECT(aw->window), "key-release-event", G_CALLBACK(key_release_handler), aw);
	g_signal_connect(G_OBJECT(aw->window), "delete-event", G_CALLBACK(delete_event_handler), aw);

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
	g_object_unref(G_OBJECT(addAccountPixbuf));
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
	

	return table;
}

static GtkWidget *
create_user_information_page(IFAAccountWizard *aw)
{
	GtkWidget *vbox = gtk_vbox_new(false, 0);
	
	return vbox;
}

static GtkWidget *
create_connect_page(IFAAccountWizard *aw)
{
	GtkWidget *vbox = gtk_vbox_new(false, 0);
	
	return vbox;
}

static GtkWidget *
create_summary_page(IFAAccountWizard *aw)
{
	GtkWidget *vbox = gtk_vbox_new(false, 0);
	
	return vbox;
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
	g_signal_connect(G_OBJECT(aw->backButton), "clicked", G_CALLBACK(back_button_clicked), aw);

	aw->forwardButton = gtk_button_new_from_stock(GTK_STOCK_GO_FORWARD);
	gtk_box_pack_start(GTK_BOX(buttonBox), aw->forwardButton, false, false, 0);
	g_signal_connect(G_OBJECT(aw->forwardButton), "clicked", G_CALLBACK(forward_button_clicked), aw);

	aw->connectButton = gtk_button_new_with_mnemonic(_("Co_nnect"));
	gtk_box_pack_start(GTK_BOX(buttonBox), aw->connectButton, false, false, 0);
	gtk_widget_set_no_show_all(aw->connectButton, true);
	g_signal_connect(G_OBJECT(aw->connectButton), "clicked", G_CALLBACK(connect_button_clicked), aw);

	aw->finishButton = gtk_button_new_with_mnemonic(_("_Finish"));
	gtk_box_pack_start(GTK_BOX(buttonBox), aw->finishButton, false, false, 0);
	gtk_widget_set_no_show_all(aw->finishButton, true);
	g_signal_connect(G_OBJECT(aw->finishButton), "clicked", G_CALLBACK(finish_button_clicked), aw);
	
	return buttonBox;
}

static gboolean
key_press_handler(GtkWidget *widget, GdkEventKey *event, IFAAccountWizard *aw)
{
	gboolean stop_other_handlers = true;
	g_message("Key pressed inside the preferences window");

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
	g_message("delete_event_handler()");
	
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
	g_message("FIXME: Implement delete_account_wizard (free memory of non-Gtk items)");

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
		case START_PAGE:
			g_message("Current page: START_PAGE");
			gtk_widget_set_sensitive(aw->backButton, false);
			gtk_widget_set_sensitive(aw->forwardButton, true);
			
			gtk_widget_show(aw->backButton);
			gtk_widget_show(aw->forwardButton);
			gtk_widget_hide(aw->connectButton);
			gtk_widget_hide(aw->finishButton);
			break;
		case SERVER_PAGE:
			g_message("Current page: SERVER_PAGE");
			gtk_widget_set_sensitive(aw->backButton, true);
			gtk_widget_set_sensitive(aw->forwardButton, false);
			
			gtk_widget_show(aw->backButton);
			gtk_widget_show(aw->forwardButton);
			gtk_widget_hide(aw->connectButton);
			gtk_widget_hide(aw->finishButton);
			break;
		case USER_PAGE:
			g_message("Current page: USER_PAGE");
			gtk_widget_set_sensitive(aw->backButton, true);
			gtk_widget_set_sensitive(aw->forwardButton, false);
			
			gtk_widget_show(aw->backButton);
			gtk_widget_show(aw->forwardButton);
			gtk_widget_hide(aw->connectButton);
			gtk_widget_hide(aw->finishButton);
			break;
		case CONNECT_PAGE:
			g_message("Current page: CONNECT_PAGE");
			gtk_widget_set_sensitive(aw->backButton, true);
			gtk_widget_set_sensitive(aw->connectButton, true);
			
			gtk_widget_show(aw->backButton);
			gtk_widget_hide(aw->forwardButton);
			gtk_widget_show(aw->connectButton);
			gtk_widget_hide(aw->finishButton);
			break;
		case SUMMARY_PAGE:
			g_message("Current page: SUMMARY_PAGE");
			gtk_widget_set_sensitive(aw->backButton, false);
			gtk_widget_set_sensitive(aw->finishButton, true);
			
			gtk_widget_show(aw->backButton);
			gtk_widget_hide(aw->forwardButton);
			gtk_widget_hide(aw->connectButton);
			gtk_widget_show(aw->finishButton);
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
	g_message("FIXME: Implement connect_button_clicked()");
	
	/**
	 * 		if (connect is successful)
	 * 			gtk_notebook_next_page(GTK_NOTEBOOK(aw->notebook));
	 */
}

static void
finish_button_clicked(GtkButton *button, IFAAccountWizard *aw)
{
	close_window(aw);
	g_message("FIXME: Cause the main ifolder window to show");
}

