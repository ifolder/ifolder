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
#include "util.h"

#include "main-window.h"
#include "preferences-window.h"

/*@todo Remove this when gettext is added */
#define _

extern iFolderClient *ifolder_client;

typedef struct _IFAMainWindowPrivate IFAMainWindowPrivate;
struct _IFAMainWindowPrivate 
{
	GtkWidget 		*parentWindow;

	gboolean		realized;
	gboolean		controlKeyPressed;
	
	GtkWidget		*menuBar;
	
	GtkWidget		*mainStatusBar;
	GtkWidget		*syncBar;
	GtkWidget		*createMenuItem;
	GtkWidget		*shareMenuItem;
	GtkWidget		*openMenuItem;
	GtkWidget		*unsynchronizedMenuItem;
	GtkWidget		*syncNowMenuItem;
	GtkWidget		*disconnectMenuItem;
	GtkWidget		*propMenuItem;
	GtkWidget		*closeMenuItem;
	GtkWidget		*quitMenuItem;

	GtkWidget		*accountSettingsMenuItem;
	GtkWidget		*preferencesMenuItem;

	GtkWidget		*refreshMenuItem;
	GtkWidget		*syncLogMenuItem;
	
	GtkWidget		*helpMenuItem;
	GtkWidget		*aboutMenuItem;
	
	/**
	 * Keep track of open windows so that if we're called again for one that is
	 * already open, we'll just present it to the user instead of opening an
	 * additional one.
	 */
	GHashTable		*propDialogs;
	
	/**
	 * iFolder Content Area
	 */
	GtkWidget		*contentEventBox;
	
	/**
	 * Actions Pane
	 */
	GtkWidget		*searchEntry;
	GtkWidget		*cancelSearchButton;
	gulong			searchTimeoutID;
	
	GtkWidget		*createiFolderButton;
	
	/* Buttons for local iFolders */
	GtkWidget		*openButton;
	GtkWidget		*synchronizeNowButton;
	GtkWidget		*shareButton;
	GtkWidget		*fixUnsynchronizedFilesButton;
	GtkWidget		*disconnectiFolderButton;
	GtkWidget		*viewiFolderPropertiesButton;
	
	GtkWidget		*ifoldersScrolledWindow;
	GtkWidget		*ifoldersIconView;
};

static IFAMainWindow *main_window = NULL;

#define IFA_MAIN_WINDOW_GET_PRIVATE(obj) (G_TYPE_INSTANCE_GET_PRIVATE ((obj), IFA_MAIN_WINDOW_TYPE, IFAMainWindowPrivate))

static void                 ifa_main_window_finalize       (GObject            *object);

G_DEFINE_TYPE (IFAMainWindow, ifa_main_window, GTK_TYPE_DIALOG)

/* Forward Declarations */

static GtkWidget * ifa_main_window_new (GtkWindow *parent);

static void                 ifa_main_window_get_property   (GObject            *object,
							     guint               prop_id,
							     GValue             *value,
							     GParamSpec         *pspec);
static void                 ifa_main_window_set_property   (GObject            *object,
							     guint               prop_id,
							     const GValue       *value,
							     GParamSpec         *pspec);
static void                 ifa_main_window_style_set      (GtkWidget          *widget,
			                                     GtkStyle           *previous_style);

static void on_realize (GtkWidget *widget, IFAMainWindow *mw);
static void on_hide (GtkWidget *widget, IFAMainWindow *mw);

static GtkWidget * create_widgets (IFAMainWindow *mw);
static GtkWidget * create_menu (IFAMainWindow *mw);
static GtkWidget * create_content_area (IFAMainWindow *mw);
static GtkWidget * create_status_bar (IFAMainWindow *mw);

/* iFolder Menu Handlers */
static void on_create_ifolder (GtkMenuItem *menuitem, IFAMainWindow *mw);
static void on_open_ifolder (GtkMenuItem *menuitem, IFAMainWindow *mw);
static void on_share_ifolder (GtkMenuItem *menuitem, IFAMainWindow *mw);
static void on_sync_ifolder_now (GtkMenuItem *menuitem, IFAMainWindow *mw);
static void on_disconnect_ifolder (GtkMenuItem *menuitem, IFAMainWindow *mw);
static void on_show_ifolder_properties (GtkMenuItem *menuitem, IFAMainWindow *mw);
static void on_close (GtkMenuItem *menuitem, IFAMainWindow *mw);
static void on_quit (GtkMenuItem *menuitem, IFAMainWindow *mw);

/* Edit Menu Handlers */
static void on_account_settings_menu_item (GtkMenuItem *menuitem, IFAMainWindow *mw);
static void on_preferences_menu_item (GtkMenuItem *menuitem, IFAMainWindow *mw);

/* View Menu Handlers */
static void on_refresh (GtkMenuItem *menuitem, IFAMainWindow *mw);
static void on_sync_log_menu_item (GtkMenuItem *menuitem, IFAMainWindow *mw);

/* Help Menu Handlers */
static void on_help (GtkMenuItem *menuitem, IFAMainWindow *mw);
static void on_about (GtkMenuItem *menuitem, IFAMainWindow *mw);

static void
ifa_main_window_class_init (IFAMainWindowClass *klass)
{
	GObjectClass *object_class;
	GtkWidgetClass *widget_class;
	GtkWindowClass *window_class;
	  
	g_message ("ifa_main_window_class_init()");
		
	object_class = (GObjectClass *)klass;
	widget_class = (GtkWidgetClass *)klass;
	window_class = (GtkWindowClass *)klass;
		
	object_class->set_property = ifa_main_window_set_property;
	object_class->get_property = ifa_main_window_get_property;
	
	object_class->finalize = ifa_main_window_finalize;
	
	widget_class->style_set = ifa_main_window_style_set;
	
	g_type_class_add_private (object_class, sizeof (IFAMainWindowPrivate));
}

static void
ifa_main_window_init (IFAMainWindow *mw)
{
	IFAMainWindowPrivate *priv;
	GtkWidget *vbox;
	
	g_message ("ifa_main_window_init()");

	/* Data */
	priv = IFA_MAIN_WINDOW_GET_PRIVATE (mw);
	mw->private_data = priv;
	
	priv->realized = FALSE;
	
	priv->propDialogs = g_hash_table_new(g_str_hash, g_str_equal);
	priv->searchTimeoutID = 0;
	
	gtk_window_set_default_size (GTK_WINDOW (mw), 600, 480);
	g_message ("FIXME: set up the icons so we can call gtk_window_set_icon() on the main window");
/*	gtk_window_set_icon (GTK_WINDOW (mw), fixme);*/
	gtk_window_set_position (GTK_WINDOW (mw), GTK_WIN_POS_CENTER);

	vbox = gtk_bin_get_child (GTK_BIN (mw));

	/* Widgets */
	gtk_widget_push_composite_child ();

	gtk_box_pack_start (GTK_BOX (vbox), create_widgets (mw), TRUE, TRUE, 0);
	
	gtk_widget_pop_composite_child ();
}

static void
ifa_main_window_finalize (GObject *object)
{
	IFAMainWindow *mw = IFA_MAIN_WINDOW (object);
	IFAMainWindowPrivate *priv = IFA_MAIN_WINDOW_GET_PRIVATE (mw);

	g_debug ("IFAMainWindow::ifa_main_window_finalize()");

	g_hash_table_destroy (priv->propDialogs);
	
/*
	if (priv->domain_host_modified_cb_id > 0)
		g_signal_handler_disconnect (ifolder_client, priv->domain_host_modified_cb_id);
	if (priv->domain_logged_in_cb_id > 0)
		g_signal_handler_disconnect (ifolder_client, priv->domain_logged_in_cb_id);
	if (priv->domain_logged_out_cb_id > 0)
		g_signal_handler_disconnect (ifolder_client, priv->domain_logged_out_cb_id);
	if (priv->domain_activated_cb_id > 0)
		g_signal_handler_disconnect (ifolder_client, priv->domain_activated_cb_id);
	if (priv->domain_inactivated_cb_id > 0)
		g_signal_handler_disconnect (ifolder_client, priv->domain_inactivated_cb_id);
	if (priv->domain_new_default_cb_id > 0)
		g_signal_handler_disconnect (ifolder_client, priv->domain_new_default_cb_id);
	if (priv->domain_removed_cb_id > 0)
		g_signal_handler_disconnect (ifolder_client, priv->domain_removed_cb_id);
*/
	
	G_OBJECT_CLASS (ifa_main_window_parent_class)->finalize (object);
}

static void
ifa_main_window_set_property (GObject      *object, 
			       guint         prop_id, 
			       const GValue *value, 
			       GParamSpec   *pspec)
{
  IFAMainWindow *mw = IFA_MAIN_WINDOW (object);
  IFAMainWindowPrivate *priv = IFA_MAIN_WINDOW_GET_PRIVATE (mw);

  switch (prop_id) 
    {
    default:
      G_OBJECT_WARN_INVALID_PROPERTY_ID (object, prop_id, pspec);
      break;
    }
}

static void
ifa_main_window_get_property (GObject    *object, 
			       guint       prop_id, 
			       GValue     *value, 
			       GParamSpec *pspec)
{
  IFAMainWindow *mw = IFA_MAIN_WINDOW (object);
  IFAMainWindowPrivate *priv = IFA_MAIN_WINDOW_GET_PRIVATE (mw);
	
  switch (prop_id) 
    {
    default:
      G_OBJECT_WARN_INVALID_PROPERTY_ID (object, prop_id, pspec);
      break;
    }
}

static void
dialog_style_set (GtkWidget *widget,
		  GtkStyle *previous_style,
		  gpointer data)
{
/*
  GtkDialog *dialog;

  dialog = GTK_DIALOG (widget);
*/

  /* Override the style properties with HIG-compliant spacings.  Ugh.
   * http://developer.gnome.org/projects/gup/hig/1.0/layout.html#layout-dialogs
   * http://developer.gnome.org/projects/gup/hig/1.0/windows.html#alert-spacing
   */

/*
  gtk_box_set_spacing (GTK_BOX (dialog->vbox), 12);

  gtk_container_set_border_width (GTK_CONTAINER (dialog->action_area), 0);
  gtk_box_set_spacing (GTK_BOX (dialog->action_area), 6);
*/
}

static void
ifa_main_window_style_set (GtkWidget *widget,
			    GtkStyle  *previous_style)
{
/*
  if (GTK_WIDGET_CLASS (ifa_main_window_parent_class)->style_set)
    GTK_WIDGET_CLASS (ifa_main_window_parent_class)->style_set (widget, previous_style);

  dialog_style_set (widget, previous_style, NULL);
*/
}

GtkWidget *
ifa_main_window_new (GtkWindow *parent)
{
	IFAMainWindowPrivate *priv;
	g_message ("ifa_main_window_new()...calling g_object_new()");
	IFAMainWindow *mw = IFA_MAIN_WINDOW (g_object_new (IFA_MAIN_WINDOW_TYPE, NULL));
	
	g_message ("done calling g_object_new()");
	
	priv = IFA_MAIN_WINDOW_GET_PRIVATE(mw);

	if (parent != NULL)
	{
		gtk_window_set_transient_for (GTK_WINDOW (mw), parent);
		gtk_window_set_destroy_with_parent (GTK_WINDOW (mw), TRUE);
		gtk_window_set_modal (GTK_WINDOW (mw), TRUE);
	}
	
	gtk_window_set_title (GTK_WINDOW (mw), _("My iFolders"));
	
	gtk_window_set_resizable (GTK_WINDOW (mw), FALSE);
	gtk_window_set_modal (GTK_WINDOW (mw), FALSE);
	gtk_window_set_type_hint (GTK_WINDOW (mw), GDK_WINDOW_TYPE_HINT_NORMAL);

/*
	g_signal_connect (mw, "response", G_CALLBACK (on_mw_response), mw);
	g_signal_connect (mw, "key-press-event", G_CALLBACK (on_key_press), mw);
	g_signal_connect (mw, "key-release-event", G_CALLBACK (on_key_release), mw);
	g_signal_connect (mw, "response", G_CALLBACK (on_response), mw);
*/
	g_signal_connect (mw, "realize", G_CALLBACK (on_realize), mw);
	g_signal_connect (mw, "hide", G_CALLBACK (on_hide), mw);
	
	return GTK_WIDGET (mw);
}

IFAMainWindow *
ifa_get_main_window ()
{
	return main_window;
}

void
ifa_show_main_window()
{
	IFAMainWindowPrivate *priv;

	if (main_window == NULL)
		main_window = IFA_MAIN_WINDOW (ifa_main_window_new (NULL));
	
	if (!main_window)
		return;
	
	priv = IFA_MAIN_WINDOW_GET_PRIVATE (main_window);
	
	if (priv->realized)
		gtk_window_present (GTK_WINDOW (main_window));
	else
		gtk_widget_show_all (GTK_WIDGET (main_window));
}

void
ifa_hide_main_window()
{
	if (main_window == NULL)
		return;
	
	gtk_widget_hide (GTK_WIDGET (main_window));
}

static void
on_realize (GtkWidget *widget, IFAMainWindow *mw)
{
	IFAMainWindowPrivate *priv;
	
	g_debug ("IFAMainWindow::on_realize()");
	
	priv = IFA_MAIN_WINDOW_GET_PRIVATE (mw);
	
	/* Register for domain events that could affect this window */
	
	/* Widget signal handlers */

	priv->realized = TRUE;
}

static void
on_hide (GtkWidget *widget, IFAMainWindow *mw)
{
	g_debug ("IFAMainWindow::on_hide()");

	gtk_widget_destroy (GTK_WIDGET (mw));
	main_window = NULL;
}

static GtkWidget *
create_widgets (IFAMainWindow *mw)
{
	GtkWidget *vbox;
	
	vbox = gtk_vbox_new (FALSE, 0);
	
	/* Create the menu bar */
	gtk_box_pack_start (GTK_BOX (vbox), create_menu (mw), FALSE, FALSE, 0);
	
	/* Create the main content area */
	gtk_box_pack_start (GTK_BOX (vbox), create_content_area (mw), TRUE, TRUE, 0);
	
	/* Create the status bar */
	gtk_box_pack_start (GTK_BOX (vbox), create_status_bar (mw), FALSE, FALSE, 0);
	
	return vbox;
}

static GtkWidget *
create_menu (IFAMainWindow *mw)
{
	GtkAccelGroup *accelGroup;
	GtkWidget *ifolderMenu;
	GtkWidget *ifolderMenuItem;
	
	GtkWidget *editMenu;
	GtkWidget *editMenuItem;
	
	GtkWidget *viewMenu;
	GtkWidget *viewMenuItem;
	
	GtkWidget *helpMenu;
	GtkWidget *helpMenuItem0;

	IFAMainWindowPrivate *priv = IFA_MAIN_WINDOW_GET_PRIVATE (mw);
	
	priv->menuBar = gtk_menu_bar_new ();
	
	accelGroup = gtk_accel_group_new ();
	gtk_window_add_accel_group (GTK_WINDOW (mw), accelGroup);
	
	/**
	 * iFolder Menu
	 */
	ifolderMenu = gtk_menu_new ();
	
	/* New iFolder */	
	priv->createMenuItem = gtk_image_menu_item_new_from_stock (GTK_STOCK_NEW, accelGroup);
	gtk_menu_shell_append (GTK_MENU_SHELL (ifolderMenu), priv->createMenuItem);
	g_signal_connect (priv->createMenuItem, "activate", G_CALLBACK (on_create_ifolder), mw);
/*
	priv->createMenuItem = gtk_image_menu_item_new_with_mnemonic (_("_New iFolder"));
	g_message ("FIXME: call gtk_image_menu_item_set_image (priv->createMenuItem, image)");
	gtk_menu_shell_append (GTK_MENU_SHELL (ifolderMenu), priv->createMenuItem);
	gtk_widget_add_accelerator (priv->createMenuItem, "activate", accelGroup, GDK_N, GDK_CONTROL_MASK, GTK_ACCEL_VISIBLE);
	g_signal_connect (priv->createMenuItem, "activate", G_CALLBACK (on_create_ifolder), mw);
*/


	/* Separator */
	gtk_menu_shell_append (GTK_MENU_SHELL (ifolderMenu), gtk_separator_menu_item_new ());

	
	/* Open */
	priv->openMenuItem = gtk_image_menu_item_new_from_stock (GTK_STOCK_OPEN, accelGroup);
	gtk_menu_shell_append (GTK_MENU_SHELL (ifolderMenu), priv->openMenuItem);
	g_signal_connect (priv->openMenuItem, "activate", G_CALLBACK (on_open_ifolder), mw);
	
	/* Share with... */
	priv->shareMenuItem = gtk_image_menu_item_new_with_mnemonic (_("Share _with..."));
	g_message ("FIXME: call gtk_image_menu_item_set_image (priv->shareMenuItem, image)");
	gtk_menu_shell_append (GTK_MENU_SHELL (ifolderMenu), priv->shareMenuItem);
	g_signal_connect (priv->shareMenuItem, "activate", G_CALLBACK (on_share_ifolder), mw);
	
	/* Synchronize now */
	priv->syncNowMenuItem = gtk_image_menu_item_new_with_mnemonic (_("_Synchronize now"));
	g_message ("FIXME: call gtk_image_menu_item_set_image (priv->syncNowMenuItem, image)");
	gtk_menu_shell_append (GTK_MENU_SHELL (ifolderMenu), priv->syncNowMenuItem);
	g_signal_connect (priv->syncNowMenuItem, "activate", G_CALLBACK (on_sync_ifolder_now), mw);
	
	/* Disconnect iFolder */
	priv->disconnectMenuItem = gtk_image_menu_item_new_with_mnemonic (_("_Disconnect"));
	g_message ("FIXME: call gtk_image_menu_item_set_image (priv->disconnectMenuItem, image)");
	gtk_menu_shell_append (GTK_MENU_SHELL (ifolderMenu), priv->disconnectMenuItem);
	g_signal_connect (priv->disconnectMenuItem, "activate", G_CALLBACK (on_disconnect_ifolder), mw);
	
	/* Properties */
	priv->propMenuItem = gtk_image_menu_item_new_from_stock (GTK_STOCK_PROPERTIES, accelGroup);
	gtk_menu_shell_append (GTK_MENU_SHELL (ifolderMenu), priv->propMenuItem);
	g_signal_connect (priv->propMenuItem, "activate", G_CALLBACK (on_show_ifolder_properties), mw);


	/* Separator */
	gtk_menu_shell_append (GTK_MENU_SHELL (ifolderMenu), gtk_separator_menu_item_new ());

	
	/* Close */
	priv->closeMenuItem = gtk_image_menu_item_new_from_stock (GTK_STOCK_CLOSE, accelGroup);
	gtk_menu_shell_append (GTK_MENU_SHELL (ifolderMenu), priv->closeMenuItem);
	g_signal_connect (priv->closeMenuItem, "activate", G_CALLBACK (on_close), mw);
	
	/* Quit */
	priv->quitMenuItem = gtk_image_menu_item_new_from_stock (GTK_STOCK_QUIT, accelGroup);
	gtk_menu_shell_append (GTK_MENU_SHELL (ifolderMenu), priv->quitMenuItem);
	g_signal_connect (priv->quitMenuItem, "activate", G_CALLBACK (on_quit), mw);

	ifolderMenuItem = gtk_menu_item_new_with_mnemonic (_("i_Folder"));
	gtk_menu_item_set_submenu (GTK_MENU_ITEM (ifolderMenuItem), ifolderMenu);
	gtk_menu_shell_append (GTK_MENU_SHELL (priv->menuBar), ifolderMenuItem);


	/**
	 * Edit Menu
	 */
	editMenu = gtk_menu_new ();
	
	/* Account Settings... */	
	priv->accountSettingsMenuItem = gtk_image_menu_item_new_with_mnemonic (_("_Account Settings..."));
	g_message ("FIXME: call gtk_image_menu_item_set_image (priv->accountSettingsMenuItem, image)");
	gtk_menu_shell_append (GTK_MENU_SHELL (editMenu), priv->accountSettingsMenuItem);
	g_signal_connect (priv->accountSettingsMenuItem, "activate", G_CALLBACK (on_account_settings_menu_item), mw);
	
	/* Preferences */
	priv->preferencesMenuItem = gtk_image_menu_item_new_from_stock (GTK_STOCK_PREFERENCES, accelGroup);
	gtk_menu_shell_append (GTK_MENU_SHELL (editMenu), priv->preferencesMenuItem);
	g_signal_connect (priv->preferencesMenuItem, "activate", G_CALLBACK (on_preferences_menu_item), mw);

	editMenuItem = gtk_menu_item_new_with_mnemonic (_("_Edit"));
	gtk_menu_item_set_submenu (GTK_MENU_ITEM (editMenuItem), editMenu);
	gtk_menu_shell_append (GTK_MENU_SHELL (priv->menuBar), editMenuItem);


	/**
	 * View Menu
	 */
	viewMenu = gtk_menu_new ();
	
	/* Refresh */
	priv->refreshMenuItem = gtk_image_menu_item_new_from_stock (GTK_STOCK_REFRESH, accelGroup);
	gtk_menu_shell_append (GTK_MENU_SHELL (viewMenu), priv->refreshMenuItem);
	g_signal_connect (priv->refreshMenuItem, "activate", G_CALLBACK (on_refresh), mw);
	
	/* Synchronization Log */
	priv->syncLogMenuItem = gtk_image_menu_item_new_with_mnemonic (_("Synchronization Log"));
	g_message ("FIXME: call gtk_image_menu_item_set_image (priv->syncLogMenuItem, image)");
	gtk_menu_shell_append (GTK_MENU_SHELL (viewMenu), priv->syncLogMenuItem);
	g_signal_connect (priv->syncLogMenuItem, "activate", G_CALLBACK (on_sync_log_menu_item), mw);
	
	viewMenuItem = gtk_menu_item_new_with_mnemonic (_("_View"));
	gtk_menu_item_set_submenu (GTK_MENU_ITEM (viewMenuItem), viewMenu);
	gtk_menu_shell_append (GTK_MENU_SHELL (priv->menuBar), viewMenuItem);
	
	/**
	 * Help Menu
	 */
	helpMenu = gtk_menu_new ();

	/* Help */
	priv->helpMenuItem = gtk_image_menu_item_new_from_stock (GTK_STOCK_HELP, accelGroup);
	gtk_menu_shell_append (GTK_MENU_SHELL (helpMenu), priv->helpMenuItem);
	g_signal_connect (priv->helpMenuItem, "activate", G_CALLBACK (on_help), mw);
	
	/* About */
	priv->helpMenuItem = gtk_image_menu_item_new_from_stock (GTK_STOCK_ABOUT, accelGroup);
	gtk_menu_shell_append (GTK_MENU_SHELL (helpMenu), priv->helpMenuItem);
	g_signal_connect (priv->helpMenuItem, "activate", G_CALLBACK (on_about), mw);
	
	helpMenuItem0 = gtk_menu_item_new_with_mnemonic (_("_Help"));
	gtk_menu_item_set_submenu (GTK_MENU_ITEM (helpMenuItem0), helpMenu);
	gtk_menu_shell_append (GTK_MENU_SHELL (priv->menuBar), helpMenuItem0);


	/*

	GtkWidget		*;
	GtkWidget		*helpMenuItem;
	GtkWidget		*aboutMenuItem;
	*/
	
	return priv->menuBar;
}

static GtkWidget *
create_content_area (IFAMainWindow *mw)
{
	return gtk_label_new ("FIXME: Implement IFAMainWindow::create_content_area()");
}

static GtkWidget *
create_status_bar (IFAMainWindow *mw)
{
	return gtk_label_new ("FIXME: Implement IFAMainWindow::create_status_bar()");
}

static void
on_create_ifolder (GtkMenuItem *menuitem, IFAMainWindow *mw)
{
	g_message ("FIXME: Implement IFAMainWindow::on_create_ifolder()");
}

static void
on_open_ifolder (GtkMenuItem *menuitem, IFAMainWindow *mw)
{
	g_message ("FIXME: Implement IFAMainWindow::on_open_ifolder()");
}

static void
on_share_ifolder (GtkMenuItem *menuitem, IFAMainWindow *mw)
{
	g_message ("FIXME: Implement IFAMainWindow::on_share_ifolder()");
}

static void
on_sync_ifolder_now (GtkMenuItem *menuitem, IFAMainWindow *mw)
{
	g_message ("FIXME: Implement IFAMainWindow::on_sync_ifolder_now()");
}

static void
on_disconnect_ifolder (GtkMenuItem *menuitem, IFAMainWindow *mw)
{
	g_message ("FIXME: Implement IFAMainWindow::on_disconnect_ifolder()");
}

static void
on_show_ifolder_properties (GtkMenuItem *menuitem, IFAMainWindow *mw)
{
	g_message ("FIXME: Implement IFAMainWindow::on_show_ifolder_properties()");
}

static void
on_close (GtkMenuItem *menuitem, IFAMainWindow *mw)
{
	gtk_widget_hide (GTK_WIDGET (mw));
}

static void
on_quit (GtkMenuItem *menuitem, IFAMainWindow *mw)
{
	ifa_quit_ifolder ();
}

static void
on_account_settings_menu_item (GtkMenuItem *menuitem, IFAMainWindow *mw)
{
	ifa_show_preferences_window(1);
}
	
static void
on_preferences_menu_item (GtkMenuItem *menuitem, IFAMainWindow *mw)
{
	ifa_show_preferences_window(0);
}

static void
on_refresh (GtkMenuItem *menuitem, IFAMainWindow *mw)
{
	g_message ("FIXME: Implement IFAMainWindow::on_refresh()");
}

static void
on_sync_log_menu_item (GtkMenuItem *menuitem, IFAMainWindow *mw)
{
	g_message ("FIXME: Implement IFAMainWindow::on_sync_log_menu_item()");
}

static void
on_help (GtkMenuItem *menuitem, IFAMainWindow *mw)
{
	g_message ("FIXME: Implement IFAMainWindow::on_help()");
}

static void
on_about (GtkMenuItem *menuitem, IFAMainWindow *mw)
{
	ifa_show_about ();
}

