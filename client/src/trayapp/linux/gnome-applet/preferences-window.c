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

#include "preferences-window.h"

/*@todo Remove this when gettext is added */
#define _

static IFPreferencesWindow *prefsWindow = NULL;

static IFPreferencesWindow *create_preferences_window();
static void delete_preferences_window(IFPreferencesWindow **pw);
static gboolean key_press_handler(GtkWidget *widget, GdkEventKey *event, IFPreferencesWindow *pw);
static gboolean key_release_handler(GtkWidget *widget, GdkEventKey *event, IFPreferencesWindow *pw);
static GtkWidget *create_general_page(IFPreferencesWindow *pw);
static GtkWidget *create_accounts_page(IFPreferencesWindow *pw);
static void help_button_clicked(GtkButton *button, IFPreferencesWindow *pw);
static void close_button_clicked(GtkButton *button, IFPreferencesWindow *pw);
static void close_window();
static void notebook_page_switched(GtkNotebook *notebook, GtkNotebookPage *page, guint page_num, IFPreferencesWindow *pw);

IFPreferencesWindow *
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
	IFPreferencesWindow *pw = ifa_get_preferences_window();
	if (!pw)
		return;

	gtk_widget_show_all(pw->window);
}

IFPreferencesWindow *
create_preferences_window()
{
	IFPreferencesWindow *pw = malloc(sizeof(IFPreferencesWindow));

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
key_press_handler(GtkWidget *widget, GdkEventKey *event, IFPreferencesWindow *pw)
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
key_release_handler(GtkWidget *widget, GdkEventKey *event, IFPreferencesWindow *pw)
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
create_general_page(IFPreferencesWindow *pw)
{
	return gtk_label_new("General");
}

static GtkWidget *
create_accounts_page(IFPreferencesWindow *pw)
{
	return gtk_label_new("Accounts");
}

static void
help_button_clicked(GtkButton *button, IFPreferencesWindow *pw)
{
}

static void
close_button_clicked(GtkButton *button, IFPreferencesWindow *pw)
{
	close_window();
}

static void
close_window()
{
	if (!prefsWindow)
		return;

	/* FIXME: check leaving general page */
	gtk_widget_hide(prefsWindow->window);
	gtk_widget_destroy(prefsWindow->window);

	delete_preferences_window(&prefsWindow);
}

static void
delete_preferences_window(IFPreferencesWindow **pw)
{
	/* FIXME: Free memory used by structure members */
	free(*pw);
	prefsWindow = NULL;
}

static void
notebook_page_switched(GtkNotebook *notebook, GtkNotebookPage *page, guint page_num, IFPreferencesWindow *pw)
{
	/* FIXME: Implement notebook_page_switched() */
}

