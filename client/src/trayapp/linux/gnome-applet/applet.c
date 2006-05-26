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
 *  GNOME Wireless Applet Authors:
 *		Eskil Heyn Olsen <eskil@eskil.dk>
 *		Bastien Nocera <hadess@hadess.net> (Gnome2 port)
 *
 * (C) Copyright 2004-2005 Red Hat, Inc.
 * (C) Copyright 2001, 2002 Free Software Foundation
 *
 ***********************************************************************/

#include <string.h>
#include <gtk/gtk.h>

//#if !GTK_CHECK_VERSION(2,6,0)
//#include <gnome.h>
//#endif

#include "applet.h"
#include "preferences-window.h"

/*@todo Remove this when gettext is added */
#define _

extern iFolderClient *ifolder_client;

static GObject *			ifa_constructor (GType type, guint n_props, GObjectConstructParam *construct_props);
static gboolean			ifa_icons_init (IFApplet *applet);
static void				ifa_icons_free (IFApplet *applet);
static void				ifa_context_menu_update (IFApplet *applet);
static GtkWidget *			ifa_get_instance (IFApplet *applet);
static void				ifa_update_state (IFApplet *applet);
static void				ifa_dropdown_menu_deactivate_cb (GtkWidget *menu, IFApplet *applet);
static G_GNUC_NORETURN void	ifa_destroy (IFApplet *applet);
static void 				ifa_set_icon (IFApplet *applet, GdkPixbuf *applet_icon);

static void on_account_wizard_hide (GtkWidget *widget, IFApplet *applet);


G_DEFINE_TYPE(IFApplet, ifa, EGG_TYPE_TRAY_ICON)

static void ifa_init (IFApplet *applet)
{
//	applet->animation_id = 0;
//	applet->animation_step = 0;

	if (!ifa_icons_init (applet))
		return;
	
	
/*	gtk_window_set_default_icon_from_file (ICONDIR"/IFApplet/wireless-applet.png", NULL); */

	gtk_widget_show (ifa_get_instance (applet));

//	ifa_set_icon(applet, applet->starting_up_icon);
	ifa_set_icon(applet, applet->idle_icon);
}

static void ifa_class_init (IFAppletClass *klass)
{
	GObjectClass *gobject_class = G_OBJECT_CLASS(klass);

	gobject_class->constructor = ifa_constructor;
}

static GObject *ifa_constructor (GType type, guint n_props, GObjectConstructParam *construct_props)
{
	GObject *obj;
	IFApplet *applet;
	IFAppletClass *klass;

	klass = IFL_APPLET_CLASS (g_type_class_peek (type));
	obj = G_OBJECT_CLASS (ifa_parent_class)->constructor (type, n_props, construct_props);
	applet =  IFL_APPLET (obj);

	return obj;
}

static void about_dialog_activate_link_cb (GtkAboutDialog *about,
                                           const gchar *url,
                                           gpointer data)
{
//	gnome_url_show (url, NULL);
}

static void ifa_about_cb (GtkMenuItem *mi, IFApplet *applet)
{
	static const gchar *authors[] =
	{
		"Boyd Timothy <btimothy@novell.com>",
		NULL
	};

	static const gchar *artists[] =
	{
		"Ryan Collier <novell.com>",
		NULL
	};

	static const gchar *documenters[] =
	{
		NULL
	};

//#if !GTK_CHECK_VERSION(2,6,0)
//	GdkPixbuf	*pixbuf;
//	char		*file;
//	GtkWidget	*about_dialog;

//	/* GTK 2.4 and earlier, have to use libgnome for about dialog */
//	file = gnome_program_locate_file (NULL, GNOME_FILE_DOMAIN_PIXMAP, "gnome-networktool.png", FALSE, NULL); /* @todo Change this icon */
//	pixbuf = gdk_pixbuf_new_from_file (file, NULL);
//	g_free (file);

//	about_dialog = gnome_about_new (_("iFolder 3 Applet"),
//	                                VERSION,
//	                                _("Copyright \xc2\xa9 2006 Novell, Inc."),
//	                                _("Notification area applet for iFolder 3."),
//	                                authors,
//	                                documenters,
//	                                _("translator-credits"),
//	                                pixbuf);
//	g_object_unref (pixbuf);

//	gtk_window_set_screen (GTK_WINDOW (about_dialog), gtk_widget_get_screen (GTK_WIDGET (applet)));
//	g_signal_connect (about_dialog, "destroy", G_CALLBACK (gtk_widget_destroyed), &about_dialog);
//	gtk_widget_show (about_dialog);

//#else

	static gboolean been_here = FALSE;
	if (!been_here)
	{
		been_here = TRUE;
		gtk_about_dialog_set_url_hook (about_dialog_activate_link_cb, NULL, NULL);
	}

	/* GTK 2.6 and later code */
	gtk_show_about_dialog (NULL,
	                       "name", _("iFolder 3 Applet"),
	                       "version", VERSION,
	                       "copyright", _("Copyright \xc2\xa9 2006 Novell, Inc."),
	                       "comments", _("Notification area applet for iFolder 3."),
	                       "website", "http://www.ifolder.com/",
	                       "authors", authors,
	                       "artists", artists,
	                       "documenters", documenters,
	                       "translator-credits", _("translator-credits"),
	                       "logo-icon-name", GTK_STOCK_NETWORK, /* @todo Change this to an iFolder icon */
	                       NULL);
//#endif
}


static void ifa_set_icon (IFApplet *applet, GdkPixbuf *applet_icon)
{
	g_return_if_fail (applet != NULL);
	g_return_if_fail (applet_icon != NULL);

	gtk_image_set_from_pixbuf (GTK_IMAGE (applet->pixmap), applet_icon);
}


static GdkPixbuf *ifa_get_status_icon (IFApplet *applet)
{
	GdkPixbuf *pixbuf = NULL;

	g_return_val_if_fail (applet != NULL, NULL);

	/* @todo Determine which iFolder icon to show */
	/*
	applet->starting_up_icon;
	applet->shutting_down_icon;
	applet->uploading_icon;
	applet->downloading_icon;
	applet->idle_icon;
	*/
	
	pixbuf = applet->idle_icon;

	return pixbuf;
}


/*
 * ifa_update_state
 *
 * @todo Determine if we need ifa_update_state for iFolder
 */
static void ifa_update_state (IFApplet *applet)
{
	gboolean			show_applet = TRUE;
	GdkPixbuf *		pixbuf = NULL;
	char *			tip = NULL;

//	if (!applet->if_running)
//	{
//		show_applet = FALSE;
//		tip = g_strdup (_("iFolder 3 is not running"));
//		goto done;
//	}

	if (!applet->tooltips)
		applet->tooltips = gtk_tooltips_new ();

	gtk_tooltips_set_tip (applet->tooltips, applet->event_box, tip, NULL);
	g_free (tip);

	/* determine if we should hide the notification icon */
	if (show_applet)
		gtk_widget_show (GTK_WIDGET (applet));
	else
		gtk_widget_hide (GTK_WIDGET (applet));
}


/*
 * ifa_redraw_timeout
 *
 * Called regularly to update the applet's state and icon in the panel
 *
 */
/*
static int ifa_redraw_timeout (IFApplet *applet)
{
	if (!applet->animation_id)
		ifa_update_state (applet);

  	return TRUE;
}
*/


/*
 * show_warning_dialog
 *
 * pop up a warning or error dialog with certain text
 *
 */
static gboolean show_warning_dialog (char *mesg)
{
	GtkWidget	*	dialog;

	dialog = gtk_message_dialog_new (NULL, (GtkDialogFlags)0, GTK_MESSAGE_ERROR, GTK_BUTTONS_OK, mesg, NULL);

	/* Bash focus-stealing prevention in the face */
	gtk_window_set_position (GTK_WINDOW (dialog), GTK_WIN_POS_CENTER_ALWAYS);
	gtk_widget_realize (dialog);
	gdk_x11_window_set_user_time (dialog->window, gtk_get_current_event_time ());
	gtk_window_present (GTK_WINDOW (dialog));

	g_signal_connect_swapped (dialog, "response", G_CALLBACK (gtk_widget_destroy), dialog);
	g_free (mesg);

	return FALSE;
}


/*
 * ifa_schedule_warning_dialog
 *
 * Run a warning dialog in the main event loop.
 *
 */
void ifa_schedule_warning_dialog (IFApplet *applet, const char *msg)
{
	char *lcl_msg;

	g_return_if_fail (applet != NULL);
	g_return_if_fail (msg != NULL);

	lcl_msg = g_strdup (msg);
	g_idle_add ((GSourceFunc) show_warning_dialog, lcl_msg);
}


/*
 * ifa_menu_item_activate
 *
 * Signal function called when user clicks on a menu item
 *
 */
static void ifa_menu_item_activate (GtkMenuItem *item, gpointer user_data)
{
	IFApplet	*applet = (IFApplet *)user_data;
	char				*tag;

	g_return_if_fail (item != NULL);
	g_return_if_fail (applet != NULL);

//	if (!(tag = g_object_get_data (G_OBJECT (item), "device")))
//		return;

//	if ((tag = g_object_get_data (G_OBJECT (item), "network")))
//		net = network_device_get_wireless_network_by_essid (dev, tag);

//	ifa_dbus_set_device (applet->connection, dev, net ? wireless_network_get_essid (net) : NULL, NULL);
//	network_device_unref (dev);

//	nmi_dbus_signal_user_interface_activated (applet->connection);
}


/*
 * ifa_menu_add_separator_item
 *
 */
static void ifa_menu_add_separator_item (GtkWidget *menu)
{
	GtkWidget	*menu_item;
	menu_item = gtk_separator_menu_item_new ();
	gtk_menu_shell_append (GTK_MENU_SHELL (menu), menu_item);
	gtk_widget_show (menu_item);
}


/*
 * ifa_menu_add_text_item
 *
 * Add a non-clickable text item to a menu
 *
 */
static void ifa_menu_add_text_item (GtkWidget *menu, char *text)
{
	GtkWidget		*menu_item;

	g_return_if_fail (text != NULL);
	g_return_if_fail (menu != NULL);

	menu_item = gtk_menu_item_new_with_label (text);
	gtk_widget_set_sensitive (menu_item, FALSE);

	gtk_menu_shell_append (GTK_MENU_SHELL (menu), menu_item);
	gtk_widget_show (menu_item);
}


/*
 * ifa_menu_item_data_free
 *
 * Frees the "network" data tag on a menu item we've created
 *
 */
static void ifa_menu_item_data_free (GtkWidget *menu_item, gpointer data)
{
	char	*tag;
	GtkMenu	*menu;

	g_return_if_fail (menu_item != NULL);
	g_return_if_fail (data != NULL);

	if ((tag = (char *)g_object_get_data (G_OBJECT (menu_item), "network")))
	{
		g_object_set_data (G_OBJECT (menu_item), "network", NULL);
		g_free (tag);
	}

	if ((tag = (char *)g_object_get_data (G_OBJECT (menu_item), "nm-item-data")))
	{
		g_object_set_data (G_OBJECT (menu_item), "nm-item-data", NULL);
		g_free (tag);
	}

	if ((tag = (char *)g_object_get_data (G_OBJECT (menu_item), "device")))
	{
		g_object_set_data (G_OBJECT (menu_item), "device", NULL);
		g_free (tag);
	}

	if ((tag = (char *)g_object_get_data (G_OBJECT (menu_item), "disconnect")))
	{
		g_object_set_data (G_OBJECT (menu_item), "disconnect", NULL);
		g_free (tag);
	}

	if ((menu = GTK_MENU (gtk_menu_item_get_submenu (GTK_MENU_ITEM (menu_item)))))
		gtk_container_foreach (GTK_CONTAINER (menu), ifa_menu_item_data_free, menu);

	gtk_widget_destroy (menu_item);
}


/*
 * ifa_dispose_menu_items
 *
 * Destroy the menu and each of its items data tags
 *
 */
static void ifa_dropdown_menu_clear (GtkWidget *menu)
{
	g_return_if_fail (menu != NULL);

	/* Free the "network" data on each menu item, and destroy the item */
	gtk_container_foreach (GTK_CONTAINER (menu), ifa_menu_item_data_free, menu);
}


/*
 * ifa_dropdown_menu_populate
 *
 * Set up our networks menu from scratch
 *
 */
static void ifa_dropdown_menu_populate (GtkWidget *menu, IFApplet *applet)
{
	g_return_if_fail (menu != NULL);
	g_return_if_fail (applet != NULL);

//	if (!applet->if_running)
//		ifa_menu_add_text_item (menu, _("iFolder 3 is not running..."));
//	else
//		ifa_menu_add_devices (menu, applet);
}


/*
 * ifa_dropdown_menu_show_cb
 *
 * Pop up the wireless networks menu
 *
 */
static void ifa_dropdown_menu_show_cb (GtkWidget *menu, IFApplet *applet)
{
	g_return_if_fail (menu != NULL);
	g_return_if_fail (applet != NULL);

	if (!applet->tooltips)
		applet->tooltips = gtk_tooltips_new ();
	gtk_tooltips_set_tip (applet->tooltips, applet->event_box, NULL, NULL);

	if (applet->dropdown_menu && (menu == applet->dropdown_menu))
	{
		ifa_dropdown_menu_clear (applet->dropdown_menu);
		ifa_dropdown_menu_populate (applet->dropdown_menu, applet);
		gtk_widget_show_all (applet->dropdown_menu);
	}
}

/*
 * ifa_context_menu_update
 *
 */
static void ifa_context_menu_update (IFApplet *applet)
{
	guint domain_count;
	
	g_return_if_fail (applet != NULL);
	
	domain_count = ifolder_client_get_domain_count (ifolder_client);
	
	gtk_widget_set_sensitive (applet->show_ifolders_item, domain_count > 0 ? TRUE : FALSE);
	gtk_widget_set_sensitive (applet->show_sync_log_item, domain_count > 0 ? TRUE : FALSE);
}

/*
 * ifa_dropdown_menu_create
 *
 * Create the applet's dropdown menu
 *
 */
static GtkWidget *ifa_dropdown_menu_create (GtkMenuItem *parent, IFApplet *applet)
{
	GtkWidget	*menu;

	g_return_val_if_fail (parent != NULL, NULL);
	g_return_val_if_fail (applet != NULL, NULL);

	menu = gtk_menu_new ();
	gtk_container_set_border_width (GTK_CONTAINER (menu), 0);
	gtk_menu_item_set_submenu (GTK_MENU_ITEM (parent), menu);
	g_signal_connect (menu, "show", G_CALLBACK (ifa_dropdown_menu_show_cb), applet);

	return menu;
}


static void
ifa_show_ifolders_cb(GtkMenuItem *mi, IFApplet *applet)
{
	/* @todo Add code to show the main iFolder GUI window */
}

static void
ifa_show_account_settings_cb(GtkMenuItem *mi, IFApplet *applet)
{
	ifa_show_preferences_window (1);
}

static void
ifa_show_sync_log_cb(GtkMenuItem *mi, IFApplet *applet)
{
	g_message ("FIXME: Implement ifa_show_sync_log_cb()");
}

static void
ifa_preferences_cb(GtkMenuItem *mi, IFApplet *applet)
{
	/* @todo Add code to show the preferences */
	ifa_show_preferences_window (0);
}

static void
ifa_help_cb(GtkMenuItem *mi, IFApplet *applet)
{
	/* @todo Add code to open the help documentation */
}

static void
ifa_quit_cb(GtkMenuItem *mi, IFApplet *applet)
{
	int err;

	ifa_set_icon(applet, applet->shutting_down_icon);

	/* FIXME: Add code to stop all the services and quit the program */

	ifa_destroy(applet);
}


/*
 * ifa_context_menu_create
 *
 * Generate the contextual popup menu.
 *
 */
static GtkWidget *ifa_context_menu_create (IFApplet *applet)
{
	GtkWidget	*menu;
//	GtkWidget	*menu_item;
	GtkWidget	*image;
	guint		domain_count;

	g_return_val_if_fail (applet != NULL, NULL);
	
	domain_count = ifolder_client_get_domain_count (ifolder_client);

	menu = gtk_menu_new ();

	/* 'Show iFolders' item */
	applet->show_ifolders_item = gtk_menu_item_new_with_mnemonic(_("i_Folders"));
	g_signal_connect(G_OBJECT(applet->show_ifolders_item), "activate", G_CALLBACK(ifa_show_ifolders_cb), applet);
	gtk_menu_shell_append(GTK_MENU_SHELL (menu), applet->show_ifolders_item);

	/* 'Account Settings...' item */
	applet->account_settings_item = gtk_menu_item_new_with_mnemonic(_("Account _Settings..."));
	g_signal_connect(G_OBJECT(applet->account_settings_item), "activate", G_CALLBACK(ifa_show_account_settings_cb), applet);
	gtk_menu_shell_append(GTK_MENU_SHELL (menu), applet->account_settings_item);

	/* 'Synchronization Log' item */
	applet->show_sync_log_item = gtk_image_menu_item_new_with_mnemonic(_("Synchronization _Log"));
	g_signal_connect(G_OBJECT(applet->show_sync_log_item), "activate", G_CALLBACK(ifa_show_sync_log_cb), applet);
	gtk_menu_shell_append(GTK_MENU_SHELL (menu), applet->show_sync_log_item);

	/* Separator */
	ifa_menu_add_separator_item (menu);

	/* 'Preferences...' item */
	applet->preferences_item = gtk_image_menu_item_new_with_mnemonic(_("_Preferences"));
	g_signal_connect(G_OBJECT(applet->preferences_item), "activate", G_CALLBACK(ifa_preferences_cb), applet);
	image = gtk_image_new_from_stock (GTK_STOCK_PREFERENCES, GTK_ICON_SIZE_MENU);
	gtk_image_menu_item_set_image (GTK_IMAGE_MENU_ITEM (applet->preferences_item), image);
	gtk_menu_shell_append(GTK_MENU_SHELL (menu), applet->preferences_item);

	/* Help item */
	applet->help_item = gtk_image_menu_item_new_with_mnemonic (_("_Help"));
	g_signal_connect (G_OBJECT (applet->help_item), "activate", G_CALLBACK (ifa_help_cb), applet);
	image = gtk_image_new_from_stock (GTK_STOCK_HELP, GTK_ICON_SIZE_MENU);
	gtk_image_menu_item_set_image (GTK_IMAGE_MENU_ITEM (applet->help_item), image);
	gtk_menu_shell_append (GTK_MENU_SHELL (menu), applet->help_item);
//	gtk_widget_set_sensitive (GTK_WIDGET (applet->help_item), FALSE);

	/* About item */
	applet->about_item = gtk_image_menu_item_new_with_mnemonic (_("_About"));
	g_signal_connect (G_OBJECT (applet->about_item), "activate", G_CALLBACK (ifa_about_cb), applet);
	image = gtk_image_new_from_stock (GTK_STOCK_ABOUT, GTK_ICON_SIZE_MENU);
	gtk_image_menu_item_set_image (GTK_IMAGE_MENU_ITEM (applet->about_item), image);
	gtk_menu_shell_append (GTK_MENU_SHELL (menu), applet->about_item);

	/* Separator */
	ifa_menu_add_separator_item (menu);

	/* 'Quit' item */
	applet->quit_item = gtk_image_menu_item_new_with_mnemonic(_("_Quit"));
	g_signal_connect(G_OBJECT(applet->quit_item), "activate", G_CALLBACK(ifa_quit_cb), applet);
	image = gtk_image_new_from_stock (GTK_STOCK_STOP, GTK_ICON_SIZE_MENU);
	gtk_image_menu_item_set_image (GTK_IMAGE_MENU_ITEM (applet->quit_item), image);
	gtk_menu_shell_append(GTK_MENU_SHELL (menu), applet->quit_item);

	gtk_widget_show_all (menu);

	return menu;
}


/*
 * ifa_theme_change_cb
 *
 * Destroy the popdown menu when the theme changes
 *
 */
static void ifa_theme_change_cb (IFApplet *applet)
{
	g_return_if_fail (applet != NULL);

	if (applet->dropdown_menu)
		ifa_dropdown_menu_clear (applet->dropdown_menu);

	if (applet->top_menu_item)
	{
		gtk_menu_item_remove_submenu (GTK_MENU_ITEM (applet->top_menu_item));
		applet->dropdown_menu = ifa_dropdown_menu_create (GTK_MENU_ITEM (applet->top_menu_item), applet);
		g_signal_connect (applet->dropdown_menu, "deactivate", G_CALLBACK (ifa_dropdown_menu_deactivate_cb), applet);
	}
}

/*
 * ifa_menu_position_func
 *
 * Position main dropdown menu, adapted from netapplet
 *
 */
static void ifa_menu_position_func (GtkMenu *menu G_GNUC_UNUSED, int *x, int *y, gboolean *push_in, gpointer user_data)
{
	int screen_w, screen_h, button_x, button_y, panel_w, panel_h;
	GtkRequisition requisition;
	GdkScreen *screen;
	IFApplet *applet = (IFApplet *)user_data;

	screen = gtk_widget_get_screen (applet->event_box);
	screen_w = gdk_screen_get_width (screen);
	screen_h = gdk_screen_get_height (screen);

	gdk_window_get_origin (applet->event_box->window, &button_x, &button_y);
	gtk_window_get_size (GTK_WINDOW (gtk_widget_get_toplevel (applet->event_box)), &panel_w, &panel_h);

	*x = button_x;

	/* Check to see if we would be placing the menu off of the end of the screen. */
	gtk_widget_size_request (GTK_WIDGET (menu), &requisition);
	if (button_y + panel_h + requisition.height >= screen_h)
		*y = button_y - requisition.height;
	else
		*y = button_y + panel_h;

	*push_in = TRUE;
}

/**
 * Handle left/right-clicks for the iFolder Applet Icon
 */
static gboolean ifa_button_press_cb (GtkWidget *widget, GdkEventButton *event, IFApplet *applet)
{
	g_return_val_if_fail (applet != NULL, FALSE);

	switch (event->button)
	{
		case 1:
			
			/**
			 * If there are 0 domains, pop open the account wizard.
			 */
			if (ifolder_client_get_domain_count (ifolder_client) == 0)
			{
				/* Pop open the account wizard. */
				if (applet->account_wizard)
					gtk_window_present (GTK_WINDOW (applet->account_wizard->window));
				else
				{
					applet->account_wizard = ifa_account_wizard_new ();
					g_signal_connect (applet->account_wizard->window, "hide", G_CALLBACK (on_account_wizard_hide), applet);
					gtk_widget_show_all (GTK_WIDGET (applet->account_wizard->window));
				}
			}
			else
			{
				g_message ("FIXME: When the main iFolder Window is implemented, pop it open here");
				/* Open the main iFolder Window. */
//				ifa_show_ifolders_window ();
			}
			
			return TRUE;
		case 3:
			if (applet->account_wizard)
				gtk_window_present (GTK_WINDOW (applet->account_wizard->window));
			else
			{
				ifa_context_menu_update (applet);
				gtk_menu_popup (GTK_MENU (applet->context_menu), NULL, NULL, ifa_menu_position_func, applet, event->button, event->time);
			}
			return TRUE;
		default:
			g_signal_stop_emission_by_name (widget, "button_press_event");
			return FALSE;
	}

	return FALSE;
}

/*
 * ifa_toplevel_menu_button_press_cb
 *
 * Handle left/right-clicks for the dropdown and context popup menus
 *
 */
static gboolean ifa_toplevel_menu_button_press_cb (GtkWidget *widget, GdkEventButton *event, IFApplet *applet)
{
	g_return_val_if_fail (applet != NULL, FALSE);

	switch (event->button)
	{
		case 1:
			gtk_widget_set_state (applet->event_box, GTK_STATE_SELECTED);
			gtk_menu_popup (GTK_MENU (applet->dropdown_menu), NULL, NULL, ifa_menu_position_func, applet, event->button, event->time);
			return TRUE;
		case 3:
			ifa_context_menu_update (applet);
			gtk_menu_popup (GTK_MENU (applet->context_menu), NULL, NULL, ifa_menu_position_func, applet, event->button, event->time);
			return TRUE;
		default:
			g_signal_stop_emission_by_name (widget, "button_press_event");
			return FALSE;
	}

	return FALSE;
}


/*
 * ifa_toplevel_menu_button_press_cb
 *
 * Handle left-unclick on the dropdown menu.
 *
 */
static void ifa_dropdown_menu_deactivate_cb (GtkWidget *menu, IFApplet *applet)
{

	g_return_if_fail (applet != NULL);

	gtk_widget_set_state (applet->event_box, GTK_STATE_NORMAL);
}


/*
 * ifa_setup_widgets
 *
 * Intialize the applet's widgets and packing, create the initial
 * menu of networks.
 *
 */
static void ifa_setup_widgets (IFApplet *applet)
{
	/* Event box is the main applet widget */
	applet->event_box = gtk_event_box_new ();
	gtk_container_set_border_width (GTK_CONTAINER (applet->event_box), 0);

	applet->top_menu_item = gtk_menu_item_new();
	gtk_widget_set_name (applet->top_menu_item, "ToplevelMenu");
	gtk_container_set_border_width (GTK_CONTAINER (applet->top_menu_item), 0);

	applet->pixmap = gtk_image_new ();
	gtk_container_add (GTK_CONTAINER (applet->event_box), applet->pixmap);
	gtk_container_add (GTK_CONTAINER (applet), applet->event_box);
 	gtk_widget_show_all (GTK_WIDGET (applet));
 
//	applet->dropdown_menu = ifa_dropdown_menu_create (GTK_MENU_ITEM (applet->top_menu_item), applet);
//	g_signal_connect (applet->event_box, "button_press_event", G_CALLBACK (ifa_toplevel_menu_button_press_cb), applet);
	g_signal_connect (applet->event_box, "button_press_event", G_CALLBACK (ifa_button_press_cb), applet);
	
//	g_signal_connect (applet->dropdown_menu, "deactivate", G_CALLBACK (ifa_dropdown_menu_deactivate_cb), applet);

	applet->context_menu = ifa_context_menu_create (applet);
}


/*
 * ifa_destroy
 *
 * Destroy the applet and clean up its data
 *
 */
static void G_GNUC_NORETURN ifa_destroy (IFApplet *applet)
{
	if (applet->dropdown_menu)
		ifa_dropdown_menu_clear (applet->dropdown_menu);
	if (applet->top_menu_item)
		gtk_menu_item_remove_submenu (GTK_MENU_ITEM (applet->top_menu_item));

	ifa_icons_free (applet);

//	if (applet->redraw_timeout_id > 0)
//	{
//		gtk_timeout_remove (applet->redraw_timeout_id);
//		applet->redraw_timeout_id = 0;
//	}

//	if (applet->gconf_client)
//		g_object_unref (G_OBJECT (applet->gconf_client));

//	ifa_free_data_model (applet);

//	gconf_client_notify_remove (applet->gconf_client, applet->gconf_prefs_notify_id);
//	gconf_client_notify_remove (applet->gconf_client, applet->gconf_vpn_notify_id);
//	g_object_unref (G_OBJECT (applet->gconf_client));

//	dbus_method_dispatcher_unref (applet->nmi_methods);

	exit (0);
}


/*
 * ifa_get_instance
 *
 * Create the initial instance of our wireless applet
 *
 */
static GtkWidget * ifa_get_instance (IFApplet *applet)
{
//	gtk_widget_hide (GTK_WIDGET (applet));

	applet->tooltips = NULL;

//	applet->gconf_client = gconf_client_get_default ();
//	if (!applet->gconf_client)
//		return NULL;

//	gconf_client_add_dir (applet->gconf_client, GCONF_PATH_WIRELESS, GCONF_CLIENT_PRELOAD_NONE, NULL);
//	applet->gconf_prefs_notify_id = gconf_client_notify_add (applet->gconf_client, GCONF_PATH_WIRELESS,
//						ifa_gconf_info_notify_callback, applet, NULL, NULL);

//	gconf_client_add_dir (applet->gconf_client, GCONF_PATH_VPN_CONNECTIONS, GCONF_CLIENT_PRELOAD_NONE, NULL);
//	applet->gconf_vpn_notify_id = gconf_client_notify_add (applet->gconf_client, GCONF_PATH_VPN_CONNECTIONS,
//						ifa_gconf_vpn_connections_notify_callback, applet, NULL, NULL);

//	ifa_dbus_init_helper (applet);

	/* Load pixmaps and create applet widgets */
	ifa_setup_widgets (applet);

	g_signal_connect (applet, "destroy", G_CALLBACK (ifa_destroy), NULL);
	g_signal_connect (applet, "style-set", G_CALLBACK (ifa_theme_change_cb), NULL);

	/* Start redraw timeout */
//	applet->redraw_timeout_id = g_timeout_add (1000, (GtkFunction) ifa_redraw_timeout, applet);

	return GTK_WIDGET (applet);
}


static void ifa_icons_free (IFApplet *applet)
{
	/*
	gint i,j;

	g_object_unref (applet->no_connection_icon);
	g_object_unref (applet->wired_icon);
	g_object_unref (applet->adhoc_icon);
	g_object_unref (applet->vpn_lock_icon);

	g_object_unref (applet->wireless_00_icon);
	g_object_unref (applet->wireless_25_icon);
	g_object_unref (applet->wireless_50_icon);
	g_object_unref (applet->wireless_75_icon);
	g_object_unref (applet->wireless_100_icon);

	for (i = 0; i < NUM_CONNECTING_STAGES; i++)
		for (j = 0; j < NUM_CONNECTING_FRAMES; j++)
			g_object_unref (applet->network_connecting_icons[i][j]);

	for (i = 0; i < NUM_VPN_CONNECTING_FRAMES; i++)
		g_object_unref (applet->vpn_connecting_icons[i]);
	*/
}

#define ICON_LOAD(x, y)	\
	{		\
		GError *err = NULL; \
		x = gtk_icon_theme_load_icon (icon_theme, y, icon_size, (GtkIconLookupFlags)0, &err); \
		if (x == NULL) { \
			success = FALSE; \
			g_warning ("Icon %s missing: %s", y, err->message); \
			g_error_free (err); \
			goto out; \
		} \
	}

static gboolean
ifa_icons_load_from_disk (IFApplet *applet, GtkIconTheme *icon_theme)
{
	char *	name;
	int		i, j;
	gboolean	success = FALSE;

	/* Assume icons are square */
	gint icon_size = 22;

	ICON_LOAD(applet->starting_up_icon, "ifolder-starting-up");
	ICON_LOAD(applet->shutting_down_icon, "ifolder-shutting-down");
	ICON_LOAD(applet->uploading_icon, "ifolder-uploading");
	ICON_LOAD(applet->downloading_icon, "ifolder-downloading");
	ICON_LOAD(applet->idle_icon, "ifolder-idle");

	success = TRUE;

out:
	if (!success)
	{
		char *msg = g_strdup(_("The iFolder3 applet could not find some required resources.  It cannot continue.\n"));
		show_warning_dialog (msg);
		ifa_icons_free (applet);
	}

	return success;
}

static void ifa_icon_theme_changed (GtkIconTheme *icon_theme, IFApplet *applet)
{
	ifa_icons_free (applet);
	ifa_icons_load_from_disk (applet, icon_theme);
	/* @todo force redraw */
}

static gboolean ifa_icons_init (IFApplet *applet)
{
	GtkIconTheme *icon_theme;
	const gchar *style = " \
		style \"MenuBar\" \
		{ \
			GtkMenuBar::shadow_type = GTK_SHADOW_NONE \
			GtkMenuBar::internal-padding = 0 \
		} \
		style \"MenuItem\" \
		{ \
			xthickness=0 \
			ythickness=0 \
		} \
		class \"GtkMenuBar\" style \"MenuBar\"\
		widget \"*ToplevelMenu*\" style \"MenuItem\"\
		";	

	/* @todo Do we need to worry about other screens? */
	gtk_rc_parse_string (style);

	icon_theme = gtk_icon_theme_get_default ();
	if (!ifa_icons_load_from_disk (applet, icon_theme))
		return FALSE;
	g_signal_connect (icon_theme, "changed", G_CALLBACK (ifa_icon_theme_changed), applet);

	applet->account_wizard = NULL;
	applet->ifolders_window = NULL;

	return TRUE;
}


IFApplet *ifa_new ()
{
	return IFL_APPLET(g_object_new (IFL_TYPE_APPLET, "title", "iFolder 3", NULL));
}

static void
on_account_wizard_hide (GtkWidget *widget, IFApplet *applet)
{
	g_debug ("on_account_wizard_hide()");
	applet->account_wizard = NULL;
	
	/* FIXME: Possibly change the way that the account wizard works so that we can get a Finish response or a cancel response so we know whether to show the main iFolders Window. */
}
