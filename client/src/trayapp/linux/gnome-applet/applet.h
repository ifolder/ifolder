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

#ifndef _IFOLDER3_GNOME_APPLET_H_
#define _IFOLDER3_GNOME_APPLET_H_

#include <stdlib.h>
#include <config.h>
#include <gtk/gtk.h>
//#include <libgnomeui/libgnomeui>

#include "eggtrayicon.h"
#include "account-wizard.h"

G_BEGIN_DECLS

/*
#define IFOLDER_APPLET_TYPE(ifolder_get_type())
#define IFOLDER_APPLET(obj) (G_TYPE_CHECK_INSTANCE_CAST((obj), IFOLDER_APPLET_TYPE, iFolderApplet))
#define IFOLDER_APPLET_CLASS(klass) (G_TYPE_CHECK_CLASS_CAST(klass), IFOLDER_APPLET_TYPE, iFolderAppletClass))
#define IS_IFOLDER_APPLET(obj) (G_TYPE_CHECK_INSTANCE_TYPE((obj), IFOLDER_APPLET_TYPE))
#define IS_IFOLDER_APPLET_CLASS(klass) (G_TYPE_CHECK_CLASS_TYPE((klass), IFOLDER_APPLET_TYPE))
#define IFOLDER_APPLET_GET_CLASS(obj) (G_TYPE_INSTANCE_GET_CLASS((obj), IFOLDER_APPLET_TYPE, iFolderAppletClass))

typedef struct {
	EggTrayIconClass parent_class;
} iFolderAppletClass;

GType ifolder_applet_get_type(void);

*/

#define IFL_TYPE_APPLET			(ifa_get_type())
#define IFL_APPLET(object)		(G_TYPE_CHECK_INSTANCE_CAST((object), IFL_TYPE_APPLET, IFApplet))
#define IFL_APPLET_CLASS(klass)	(G_TYPE_CHECK_CLASS_CAST((klass), IFL_TYPE_APPLET, IFAppletClass))
#define IFL_IS_APPLET(object)		(G_TYPE_CHECK_INSTANCE_TYPE((object), IFL_TYPE_APPLET))
#define IFL_IS_APPLET_CLASS(klass)	(G_TYPE_CHECK_CLASS_TYPE((klass), IFL_TYPE_APPLET))
#define IFL_APPLET_GET_CLASS(object)(G_TYPE_INSTANCE_GET_CLASS((object), IFL_TYPE_APPLET, IFAppletClass))

typedef struct
{
	EggTrayIconClass	parent_class;
} IFAppletClass; 

typedef struct
{
	EggTrayIcon		parent;
	
	GdkPixbuf *		starting_up_icon;
	GdkPixbuf *		shutting_down_icon;
	GdkPixbuf *		uploading_icon;
	GdkPixbuf *		downloading_icon;
	GdkPixbuf *		idle_icon;
	
	/* Direct UI elements */
	GtkWidget *		pixmap;
	GtkWidget *		top_menu_item;
	GtkWidget *		dropdown_menu;
	GtkWidget *		event_box;
	GtkTooltips *	tooltips;
	GtkWidget *		context_menu;
	
	GtkWidget *		show_ifolders_item;
	GtkWidget *		account_settings_item;
	GtkWidget *		show_sync_log_item;
	GtkWidget *		preferences_item;
	GtkWidget *		help_item;
	GtkWidget *		about_item;
	GtkWidget *		quit_item;

	/* Child Windows */
	IFAAccountWizard *		account_wizard;
	GtkWidget *		ifolders_window;
} IFApplet;

GType ifa_get_type(void);
IFApplet *ifa_new(void);

G_END_DECLS

#endif /*_IFOLDER3_GNOME_APPLET_H_*/
