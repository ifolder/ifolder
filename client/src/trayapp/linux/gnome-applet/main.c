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
 *  This file was originally developed for the NetworkManager Wireless Appplet
 *  created by Dan Williams <dcbw@redhat.com>.
 * 
 * (C) Copyright 2004-2005 Red Hat, Inc.
 *
 ***********************************************************************/

#include <config.h>
/*#include <string.h>*/
#include <gtk/gtk.h>
#include <libgnomeui/libgnomeui.h>
#include <libgnome/libgnome.h>
/*#include <glib/gi18n-lib.h>*/

#include <ifolder-client.h>

#include "applet.h"

static iFolderClient *ifolder_client = NULL;

/*
static void session_die (GnomeClient *client, gpointer client_data)
{
        gtk_main_quit ();
}

static gboolean session_save (GnomeClient *client, gpointer client_data)
{
        return TRUE;
}
*/

int main (int argc, char *argv[])
{
	GError *err = NULL;
	IFApplet * ifa;
//	GnomeClient *	client = NULL;

	gnome_program_init ("ifolder3-applet", VERSION, LIBGNOMEUI_MODULE,
						argc, argv, 
						GNOME_PARAM_NONE, GNOME_PARAM_NONE);

//	client = gnome_master_client ();
//	gnome_client_set_restart_style (client, GNOME_RESTART_IF_RUNNING);

//	g_signal_connect (client, "save_yourself", G_CALLBACK (session_save), NULL);
//	g_signal_connect (client, "die", G_CALLBACK (session_die), NULL);

//	bindtextdomain (GETTEXT_PACKAGE, NULL);
//	bind_textdomain_codeset (GETTEXT_PACKAGE, "UTF-8");
//	textdomain (GETTEXT_PACKAGE);

	if (argc < 2)
	{
		g_message("FIXME: Determine what the default config directory should be");
		ifolder_client = ifolder_client_initialize(NULL, &err);
	}
	else
	{
		g_message("FIXME: Validate that argv[1] is a valid directory...or at least that the syntax is correct.");
		ifolder_client = ifolder_client_initialize(argv[1], &err);
	}

	if (err)
	{
		fprintf(stderr, "Could not initialize the iFolder Client: %s\n", err->message);
		g_error_free(err);
		return -1;
	}

	if ((ifa = ifa_new ()))
	{
		gtk_widget_show_all (GTK_WIDGET (ifa));
		gtk_main ();
	}
	
	err = NULL;
	ifolder_client_uninitialize(ifolder_client, &err);
	if (err)
	{
		fprintf(stderr, "Could not uninitialize the iFolder Client: %s\n", err->message);
		return -2;
	}

	return 0;
}

