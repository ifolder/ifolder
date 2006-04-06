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

#include "applet.h"

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

	if ((ifa = ifa_new ()))
	{
		gtk_widget_show_all (GTK_WIDGET (ifa));
		gtk_main ();
	}

	return 0;
}

