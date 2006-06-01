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

#include "util.h"
#include "config.h"

/*@todo Remove this when gettext is added */
#define _

static GtkWidget *singleton_about_dialog = NULL;

static void on_help_dialog_hidden (GtkWidget *widget, gpointer user_data);

void
ifa_show_help(gchar *page_name)
{
	g_message("FIXME: Implement ifa_show_help()");
}

GdkPixbuf *
ifolder_util_load_pixbuf(const gchar *image_name)
{
	g_message("FIXME: Implement ifolder_util_load_pixbuf()");
	return NULL;
}

void
ifa_show_about ()
{
	static const gchar *authors[] =
	{
		"Boyd Timothy <btimothy@novell.com>",
		NULL
	};

	static const gchar *artists[] =
	{
		"Ryan Collier",
		"Jakub Steiner",
		NULL
	};

	static const gchar *documenters[] =
	{
		NULL
	};

	if (singleton_about_dialog == NULL)
	{

		singleton_about_dialog = gtk_about_dialog_new ();
		gtk_about_dialog_set_name (GTK_ABOUT_DIALOG (singleton_about_dialog), _("iFolder 3"));
		gtk_about_dialog_set_version (GTK_ABOUT_DIALOG (singleton_about_dialog), VERSION);
		gtk_about_dialog_set_copyright (GTK_ABOUT_DIALOG (singleton_about_dialog), _("Copyright \xc2\xa9 2006 Novell, Inc."));
		gtk_about_dialog_set_comments (GTK_ABOUT_DIALOG (singleton_about_dialog), _("FIXME: Add comments to the iFolder3 about dialog"));
		gtk_about_dialog_set_license (GTK_ABOUT_DIALOG (singleton_about_dialog), _("FIXME: Add licensing information to the about dialog"));
		gtk_about_dialog_set_wrap_license (GTK_ABOUT_DIALOG (singleton_about_dialog), TRUE);
		gtk_about_dialog_set_website (GTK_ABOUT_DIALOG (singleton_about_dialog), "http://www.ifolder.com/");
		gtk_about_dialog_set_website_label (GTK_ABOUT_DIALOG (singleton_about_dialog), _("http://www.ifolder.com/"));
		gtk_about_dialog_set_authors (GTK_ABOUT_DIALOG (singleton_about_dialog), authors);
		gtk_about_dialog_set_artists (GTK_ABOUT_DIALOG (singleton_about_dialog), artists);
		gtk_about_dialog_set_documenters (GTK_ABOUT_DIALOG (singleton_about_dialog), documenters);
		g_message ("FIXME: Load in the iFolder Logo for the about dialog");
		gtk_about_dialog_set_logo (GTK_ABOUT_DIALOG (singleton_about_dialog), NULL);
		
		g_signal_connect (singleton_about_dialog, "hide", G_CALLBACK (on_help_dialog_hidden), NULL);
		
		gtk_widget_show_all (singleton_about_dialog);
	}
	else
		gtk_window_present (GTK_WINDOW (singleton_about_dialog));
}

static void
on_help_dialog_hidden (GtkWidget *widget, gpointer user_data)
{
	g_debug ("on_help_dialog_hidden()");
	if (singleton_about_dialog != NULL)
	{
		gtk_widget_destroy (singleton_about_dialog);
		singleton_about_dialog = NULL;
	}
}
