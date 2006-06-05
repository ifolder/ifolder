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
	
	GtkWidget		*syncBar;

	GtkWidget		*createMenuItem;
	GtkWidget		*shareMenuItem;
	GtkWidget		*openMenuItem;
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

	guint			mainStatusBarContext;	
	GtkWidget		*mainStatusBar;

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
	GtkWidget		*disconnectiFolderButton;
	GtkWidget		*viewiFolderPropertiesButton;
	
	GtkWidget		*ifoldersScrolledWindow;
	GtkWidget		*ifoldersIconView;
	GtkListStore	*ifoldersListStore;
	GtkTreeModel	*ifoldersListStoreFilter;
};

enum
{
	COL_IFOLDER,
	COL_DISPLAY_NAME,
	COL_PIXBUF,
	NUM_COLS
};

static IFAMainWindow *main_window = NULL;

static GdkPixbuf *WINDOW_ICON		= NULL;
static GdkPixbuf *OK_PIXBUF			= NULL;
static GdkPixbuf *WAIT_PIXBUF		= NULL;
static GdkPixbuf *SYNC_PIXBUF		= NULL;
static GdkPixbuf *WARNING_PIXBUF	= NULL;
static GdkPixbuf *ERROR_PIXBUF		= NULL;

#define IFA_MAIN_WINDOW_GET_PRIVATE(obj) (G_TYPE_INSTANCE_GET_PRIVATE ((obj), IFA_MAIN_WINDOW_TYPE, IFAMainWindowPrivate))

static void                 ifa_main_window_finalize       (GObject            *object);

G_DEFINE_TYPE (IFAMainWindow, ifa_main_window, GTK_TYPE_DIALOG)

/* Forward Declarations */

static GtkWidget * ifa_main_window_new (GtkWindow *parent);

static void load_pixbufs (void);

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
static gboolean on_key_press (GtkWidget *widget, GdkEventKey *key, IFAMainWindow *mw);

static GtkWidget * create_widgets (IFAMainWindow *mw);
static GtkWidget * create_menu (IFAMainWindow *mw);

static GtkWidget * create_content_area (IFAMainWindow *mw);
static GtkWidget * create_actions_pane (IFAMainWindow *mw);
static GtkWidget * create_icon_view_pane (IFAMainWindow *mw);

static GtkListStore * create_store (IFAMainWindow *mw);
static gint sort_func (GtkTreeModel *model, GtkTreeIter *a, GtkTreeIter *b, IFAMainWindow *mw);

static GtkWidget * create_status_bar (IFAMainWindow *mw);

static void update_status (IFAMainWindow *mw, const gchar *message);

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

/* Widget Signal Handlers */
static void on_search_entry_changed (GtkEntry *entry, IFAMainWindow *mw);
static void on_cancel_search_button (GtkButton *widget, IFAMainWindow *mw);

static void add_ifolder_handler (GtkButton *widget, IFAMainWindow *mw);

static void open_ifolder_handler (GtkButton *widget, IFAMainWindow *mw);
static void synchronize_now_handler (GtkButton *widget, IFAMainWindow *mw);
static void share_ifolder_handler (GtkButton *widget, IFAMainWindow *mw);
static void disconnect_ifolder_handler (GtkButton *widget, IFAMainWindow *mw);
static void view_ifolder_properties_handler (GtkButton *widget, IFAMainWindow *mw);

static gboolean ifolder_filter_func (GtkTreeModel *model, GtkTreeIter *iter, GValue *value, gint column, IFAMainWindow *mw);

static void on_ifolder_activated (GtkIconView *iconview, GtkTreePath *path, IFAMainWindow *mw);
static void on_ifolder_selected (GtkIconView *iconview, IFAMainWindow *mw);

static iFolder * get_selected_ifolder (IFAMainWindow *mw);

static void update_sensitivity (IFAMainWindow *mw);
static void update_actions_sensitivity (iFolder *ifolder, IFAMainWindow *mw);
static void update_menu_sensitivity (iFolder *ifolder, IFAMainWindow *mw);


static void populate_store (IFAMainWindow *mw);

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
	
	load_pixbufs ();
	
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
	
	priv->mainStatusBarContext = 1;
	
	gtk_window_set_default_size (GTK_WINDOW (mw), 600, 480);
	gtk_widget_set_size_request (GTK_WIDGET (mw), 600, 480);
	gtk_window_set_resizable (GTK_WINDOW (mw), TRUE);
	
	gtk_window_set_icon (GTK_WINDOW (mw), WINDOW_ICON);
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
load_pixbufs (void)
{
	if (WINDOW_ICON == NULL)
		WINDOW_ICON = ifa_load_pixbuf ("ifolder16.png");
	if (OK_PIXBUF == NULL)
		OK_PIXBUF = ifa_load_pixbuf ("ifolder48.png");
	if (WAIT_PIXBUF == NULL)
		WAIT_PIXBUF = ifa_load_pixbuf ("ifolder-waiting48.png");
	if (SYNC_PIXBUF == NULL)
		SYNC_PIXBUF = ifa_load_pixbuf ("ifolder-sync48.png");
	if (WARNING_PIXBUF == NULL)
		WARNING_PIXBUF = ifa_load_pixbuf ("ifolder-warning48.png");
	if (ERROR_PIXBUF == NULL)
		ERROR_PIXBUF = ifa_load_pixbuf ("ifolder-error48.png");
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
	g_signal_connect (mw, "key-release-event", G_CALLBACK (on_key_release), mw);
	g_signal_connect (mw, "response", G_CALLBACK (on_response), mw);
*/
	g_signal_connect (mw, "realize", G_CALLBACK (on_realize), mw);
	g_signal_connect (mw, "hide", G_CALLBACK (on_hide), mw);
	g_signal_connect (mw, "key-press-event", G_CALLBACK (on_key_press), mw);
	
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
	populate_store (mw);

	priv->realized = TRUE;
}

static void
on_hide (GtkWidget *widget, IFAMainWindow *mw)
{
	g_debug ("IFAMainWindow::on_hide()");

	gtk_widget_destroy (GTK_WIDGET (mw));
	main_window = NULL;
}

static gboolean
on_key_press (GtkWidget *widget, GdkEventKey *key, IFAMainWindow *mw)
{
	switch (key->keyval)
	{
		case GDK_F1:
			on_help (NULL, mw);
			break;
		case GDK_F5:
			on_refresh (NULL, mw);
			break;
		default:
			return FALSE;	/* propogate the event on to other key-press handlers */
			break;
	}
	
	return TRUE; /* don't propogate the event on to other key-press handlers */
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

	/**
	 * Disable menu items that aren't valid if nothing is selected.
	 */
	gtk_widget_set_sensitive (priv->disconnectMenuItem, FALSE);
	gtk_widget_set_sensitive (priv->shareMenuItem, FALSE);
	gtk_widget_set_sensitive (priv->openMenuItem, FALSE);
	gtk_widget_set_sensitive (priv->syncNowMenuItem, FALSE);
	gtk_widget_set_sensitive (priv->propMenuItem, FALSE);
	
	return priv->menuBar;
}

static GtkWidget *
create_content_area (IFAMainWindow *mw)
{
	GtkStyle *default_style;
	GtkWidget *vbox, *hbox;
	IFAMainWindowPrivate *priv = IFA_MAIN_WINDOW_GET_PRIVATE (mw);
	
	priv->contentEventBox = gtk_event_box_new ();
	default_style = gtk_widget_get_style (priv->contentEventBox);
	gtk_widget_modify_bg (priv->contentEventBox, GTK_STATE_NORMAL, &(default_style->bg[GTK_STATE_ACTIVE]));
	
	vbox = gtk_vbox_new (FALSE, 0);
	gtk_container_add (GTK_CONTAINER (priv->contentEventBox), vbox);
	
	hbox = gtk_hbox_new (FALSE, 0);
	gtk_box_pack_start (GTK_BOX (vbox), hbox, TRUE, TRUE, 0);
	
	gtk_box_pack_start (GTK_BOX (hbox), create_actions_pane (mw), FALSE, FALSE, 12);
	gtk_box_pack_start (GTK_BOX (hbox), create_icon_view_pane (mw), TRUE, TRUE, 0);
	
	return priv->contentEventBox;
}

static GtkWidget *
create_actions_pane (IFAMainWindow *mw)
{
	GtkWidget *actionsVBox, *vbox, *l, *searchHBox, *spacerHBox, *hbox, *ifolderTasks;
	gchar *tmpStr;
	GtkStyle *default_style;
	GtkWidget *image;
	IFAMainWindowPrivate *priv = IFA_MAIN_WINDOW_GET_PRIVATE (mw);
	
	actionsVBox = gtk_vbox_new (FALSE, 0);
	
	gtk_widget_set_size_request (actionsVBox, 175, -1);
	
	/* Spacer */
	l = gtk_label_new ("<span size=\"small\"></span>");
	gtk_box_pack_start (GTK_BOX (actionsVBox), l, FALSE, FALSE, 0);
	gtk_label_set_use_markup (GTK_LABEL (l), TRUE);
	
	/* Filter */
	tmpStr = g_markup_printf_escaped ("<span size=\"large\">%s</span>", _("Filter"));
	l = gtk_label_new (tmpStr);
	g_free (tmpStr);
	gtk_box_pack_start (GTK_BOX (actionsVBox), l, FALSE, FALSE, 0);
	gtk_label_set_use_markup (GTK_LABEL (l), TRUE);
	gtk_misc_set_alignment (GTK_MISC (l), 0, 0.5);
	default_style = gtk_widget_get_style (l);
	gtk_widget_modify_fg (l, GTK_STATE_NORMAL, &(default_style->base[GTK_STATE_SELECTED]));
	
	searchHBox = gtk_hbox_new (FALSE, 4);
	gtk_box_pack_start (GTK_BOX (actionsVBox), searchHBox, FALSE, FALSE, 0);
	
	priv->searchEntry = gtk_entry_new ();
	gtk_box_pack_start (GTK_BOX (searchHBox), priv->searchEntry, TRUE, TRUE, 0);
	gtk_entry_select_region (GTK_ENTRY (priv->searchEntry), 0, -1);
	g_signal_connect (priv->searchEntry, "changed", G_CALLBACK (on_search_entry_changed), mw);
	
	image = gtk_image_new_from_stock (GTK_STOCK_STOP, GTK_ICON_SIZE_MENU);
	gtk_misc_set_alignment (GTK_MISC (image), 0.5, 0);
	
	priv->cancelSearchButton = gtk_button_new ();
	gtk_button_set_image (GTK_BUTTON (priv->cancelSearchButton), image);
	gtk_box_pack_start (GTK_BOX (searchHBox), priv->cancelSearchButton, FALSE, FALSE, 0);
	gtk_button_set_relief (GTK_BUTTON (priv->cancelSearchButton), GTK_RELIEF_NONE);
	gtk_widget_set_sensitive (priv->cancelSearchButton, FALSE);
	g_signal_connect (priv->cancelSearchButton, "clicked", G_CALLBACK (on_cancel_search_button), mw);

	/* Spacer */
	l = gtk_label_new ("<span size=\"small\"></span>");
	gtk_box_pack_start (GTK_BOX (actionsVBox), l, FALSE, FALSE, 0);
	gtk_label_set_use_markup (GTK_LABEL (l), TRUE);

	/* Application Actions */
	tmpStr = g_markup_printf_escaped ("<span size=\"large\">%s</span>", _("General Actions"));
	l = gtk_label_new (tmpStr);
	g_free (tmpStr);
	gtk_box_pack_start (GTK_BOX (actionsVBox), l, FALSE, FALSE, 0);
	gtk_label_set_use_markup (GTK_LABEL (l), TRUE);
	gtk_misc_set_alignment (GTK_MISC (l), 0, 0.5);
	default_style = gtk_widget_get_style (l);
	gtk_widget_modify_fg (l, GTK_STATE_NORMAL, &(default_style->base[GTK_STATE_SELECTED]));

	spacerHBox = gtk_hbox_new (FALSE, 0);
	gtk_box_pack_start (GTK_BOX (actionsVBox), spacerHBox, FALSE, FALSE, 0);
	gtk_box_pack_start (GTK_BOX (spacerHBox), gtk_label_new (""), FALSE, FALSE, 4);
	
	vbox = gtk_vbox_new (FALSE, 0);
	gtk_box_pack_start (GTK_BOX (spacerHBox), vbox, TRUE, TRUE, 0);
	
	/* New iFolder Button */
	hbox = gtk_hbox_new (FALSE, 0);
	priv->createiFolderButton = gtk_button_new ();
	gtk_container_add (GTK_CONTAINER (priv->createiFolderButton), hbox);
	gtk_box_pack_start (GTK_BOX (vbox), priv->createiFolderButton, FALSE, FALSE, 0);
	gtk_button_set_relief (GTK_BUTTON (priv->createiFolderButton), GTK_RELIEF_NONE);
	
	tmpStr = g_markup_printf_escaped ("<span>%s</span>", _("New iFolder..."));
	l = gtk_label_new (tmpStr);
	g_free (tmpStr);
	gtk_box_pack_start (GTK_BOX (hbox), l, FALSE, FALSE, 4);
	gtk_label_set_use_markup (GTK_LABEL (l), TRUE);
	gtk_label_set_use_underline (GTK_LABEL (l), FALSE);
	gtk_misc_set_alignment (GTK_MISC (l), 0, 0.5);
	
	g_signal_connect (priv->createiFolderButton, "clicked", G_CALLBACK (add_ifolder_handler), mw);
	
	/* Spacer */
	l = gtk_label_new ("<span size=\"small\"></span>");
	gtk_box_pack_start (GTK_BOX (actionsVBox), l, FALSE, FALSE, 0);
	gtk_label_set_use_markup (GTK_LABEL (l), TRUE);
	
	/* iFolder Actions */
	ifolderTasks = gtk_vbox_new (FALSE, 0);
	gtk_box_pack_start (GTK_BOX (actionsVBox), ifolderTasks, FALSE, FALSE, 0);

	tmpStr = g_markup_printf_escaped ("<span size=\"large\">%s</span>", _("iFolder Actions"));
	l = gtk_label_new (tmpStr);
	g_free (tmpStr);
	gtk_box_pack_start (GTK_BOX (ifolderTasks), l, FALSE, FALSE, 0);
	gtk_label_set_use_markup (GTK_LABEL (l), TRUE);
	gtk_misc_set_alignment (GTK_MISC (l), 0, 0.5);
	default_style = gtk_widget_get_style (l);
	gtk_widget_modify_fg (l, GTK_STATE_NORMAL, &(default_style->base[GTK_STATE_SELECTED]));

	spacerHBox = gtk_hbox_new (FALSE, 0);
	gtk_box_pack_start (GTK_BOX (ifolderTasks), spacerHBox, FALSE, FALSE, 0);
	gtk_box_pack_start (GTK_BOX (spacerHBox), gtk_label_new (""), FALSE, FALSE, 4);
	
	vbox = gtk_vbox_new (FALSE, 0);
	gtk_box_pack_start (GTK_BOX (spacerHBox), vbox, TRUE, TRUE, 0);
	
	/* Open iFolder Button */
	hbox = gtk_hbox_new (FALSE, 0);
	priv->openButton = gtk_button_new ();
	gtk_container_add (GTK_CONTAINER (priv->openButton), hbox);
	gtk_box_pack_start (GTK_BOX (vbox), priv->openButton, FALSE, FALSE, 0);
	gtk_button_set_relief (GTK_BUTTON (priv->openButton), GTK_RELIEF_NONE);
	
	tmpStr = g_markup_printf_escaped ("<span>%s</span>", _("Open..."));
	l = gtk_label_new (tmpStr);
	g_free (tmpStr);
	gtk_box_pack_start (GTK_BOX (hbox), l, FALSE, FALSE, 4);
	gtk_label_set_use_markup (GTK_LABEL (l), TRUE);
	gtk_label_set_use_underline (GTK_LABEL (l), FALSE);
	gtk_misc_set_alignment (GTK_MISC (l), 0, 0.5);
	
	gtk_widget_set_sensitive (priv->openButton, FALSE);
	g_signal_connect (priv->openButton, "clicked", G_CALLBACK (open_ifolder_handler), mw);

	/* Synchronize Now Button */
	hbox = gtk_hbox_new (FALSE, 0);
	priv->synchronizeNowButton = gtk_button_new ();
	gtk_container_add (GTK_CONTAINER (priv->synchronizeNowButton), hbox);
	gtk_box_pack_start (GTK_BOX (vbox), priv->synchronizeNowButton, FALSE, FALSE, 0);
	gtk_button_set_relief (GTK_BUTTON (priv->synchronizeNowButton), GTK_RELIEF_NONE);
	
	tmpStr = g_markup_printf_escaped ("<span>%s</span>", _("Synchronize now"));
	l = gtk_label_new (tmpStr);
	g_free (tmpStr);
	gtk_box_pack_start (GTK_BOX (hbox), l, FALSE, FALSE, 4);
	gtk_label_set_use_markup (GTK_LABEL (l), TRUE);
	gtk_label_set_use_underline (GTK_LABEL (l), FALSE);
	gtk_misc_set_alignment (GTK_MISC (l), 0, 0.5);
	
	gtk_widget_set_sensitive (priv->synchronizeNowButton, FALSE);
	g_signal_connect (priv->synchronizeNowButton, "clicked", G_CALLBACK (synchronize_now_handler), mw);

	/* Share Button */
	hbox = gtk_hbox_new (FALSE, 0);
	priv->shareButton = gtk_button_new ();
	gtk_container_add (GTK_CONTAINER (priv->shareButton), hbox);
	gtk_box_pack_start (GTK_BOX (vbox), priv->shareButton, FALSE, FALSE, 0);
	gtk_button_set_relief (GTK_BUTTON (priv->shareButton), GTK_RELIEF_NONE);
	
	tmpStr = g_markup_printf_escaped ("<span>%s</span>", _("Share with..."));
	l = gtk_label_new (tmpStr);
	g_free (tmpStr);
	gtk_box_pack_start (GTK_BOX (hbox), l, FALSE, FALSE, 4);
	gtk_label_set_use_markup (GTK_LABEL (l), TRUE);
	gtk_label_set_use_underline (GTK_LABEL (l), FALSE);
	gtk_misc_set_alignment (GTK_MISC (l), 0, 0.5);
	
	gtk_widget_set_sensitive (priv->shareButton, FALSE);
	g_signal_connect (priv->shareButton, "clicked", G_CALLBACK (share_ifolder_handler), mw);

	/* Share Button */
	hbox = gtk_hbox_new (FALSE, 0);
	priv->disconnectiFolderButton = gtk_button_new ();
	gtk_container_add (GTK_CONTAINER (priv->disconnectiFolderButton), hbox);
	gtk_box_pack_start (GTK_BOX (vbox), priv->disconnectiFolderButton, FALSE, FALSE, 0);
	gtk_button_set_relief (GTK_BUTTON (priv->disconnectiFolderButton), GTK_RELIEF_NONE);
	
	tmpStr = g_markup_printf_escaped ("<span>%s</span>", _("Disconnect iFolder..."));
	l = gtk_label_new (tmpStr);
	g_free (tmpStr);
	gtk_box_pack_start (GTK_BOX (hbox), l, FALSE, FALSE, 4);
	gtk_label_set_use_markup (GTK_LABEL (l), TRUE);
	gtk_label_set_use_underline (GTK_LABEL (l), FALSE);
	gtk_misc_set_alignment (GTK_MISC (l), 0, 0.5);
	
	gtk_widget_set_sensitive (priv->disconnectiFolderButton, FALSE);
	g_signal_connect (priv->disconnectiFolderButton, "clicked", G_CALLBACK (disconnect_ifolder_handler), mw);

	/* Properties Button */
	hbox = gtk_hbox_new (FALSE, 0);
	priv->viewiFolderPropertiesButton = gtk_button_new ();
	gtk_container_add (GTK_CONTAINER (priv->viewiFolderPropertiesButton), hbox);
	gtk_box_pack_start (GTK_BOX (vbox), priv->viewiFolderPropertiesButton, FALSE, FALSE, 0);
	gtk_button_set_relief (GTK_BUTTON (priv->viewiFolderPropertiesButton), GTK_RELIEF_NONE);
	
	tmpStr = g_markup_printf_escaped ("<span>%s</span>", _("Properties..."));
	l = gtk_label_new (tmpStr);
	g_free (tmpStr);
	gtk_box_pack_start (GTK_BOX (hbox), l, FALSE, FALSE, 4);
	gtk_label_set_use_markup (GTK_LABEL (l), TRUE);
	gtk_label_set_use_underline (GTK_LABEL (l), FALSE);
	gtk_misc_set_alignment (GTK_MISC (l), 0, 0.5);
	
	gtk_widget_set_sensitive (priv->viewiFolderPropertiesButton, FALSE);
	g_signal_connect (priv->viewiFolderPropertiesButton, "clicked", G_CALLBACK (view_ifolder_properties_handler), mw);

	return actionsVBox;
}

static GtkWidget *
create_icon_view_pane (IFAMainWindow *mw)
{
	IFAMainWindowPrivate *priv = IFA_MAIN_WINDOW_GET_PRIVATE (mw);
	
	priv->ifoldersScrolledWindow = gtk_scrolled_window_new (NULL, NULL);
	gtk_scrolled_window_set_policy (GTK_SCROLLED_WINDOW (priv->ifoldersScrolledWindow), GTK_POLICY_AUTOMATIC, GTK_POLICY_AUTOMATIC);

	priv->ifoldersListStore = create_store (mw);	
	priv->ifoldersListStoreFilter = gtk_tree_model_filter_new (GTK_TREE_MODEL (priv->ifoldersListStore), NULL);
	gtk_tree_model_filter_set_visible_func (GTK_TREE_MODEL_FILTER (priv->ifoldersListStoreFilter), (GtkTreeModelFilterVisibleFunc) ifolder_filter_func, mw, NULL);
	
	priv->ifoldersIconView = gtk_icon_view_new_with_model (priv->ifoldersListStoreFilter);
	gtk_container_add (GTK_CONTAINER (priv->ifoldersScrolledWindow), priv->ifoldersIconView);
	
	gtk_icon_view_set_selection_mode (GTK_ICON_VIEW (priv->ifoldersIconView), GTK_SELECTION_SINGLE);
	gtk_icon_view_set_orientation (GTK_ICON_VIEW (priv->ifoldersIconView), GTK_ORIENTATION_HORIZONTAL);
//	gtk_icon_view_set_orientation (GTK_ICON_VIEW (priv->ifoldersIconView), GTK_ORIENTATION_VERTICAL);
	
	gtk_icon_view_set_markup_column (GTK_ICON_VIEW (priv->ifoldersIconView), COL_DISPLAY_NAME);
//	gtk_icon_view_set_text_column (GTK_ICON_VIEW (priv->ifoldersIconView), COL_DISPLAY_NAME);
	gtk_icon_view_set_pixbuf_column (GTK_ICON_VIEW (priv->ifoldersIconView), COL_PIXBUF);
	
	gtk_icon_view_set_item_width (GTK_ICON_VIEW (priv->ifoldersIconView), 250);
	gtk_icon_view_set_spacing (GTK_ICON_VIEW (priv->ifoldersIconView), 8);
	gtk_icon_view_set_margin (GTK_ICON_VIEW (priv->ifoldersIconView), 12);
	
	g_signal_connect (priv->ifoldersIconView, "item-activated", G_CALLBACK (on_ifolder_activated), mw);
	g_signal_connect (priv->ifoldersIconView, "selection-changed", G_CALLBACK (on_ifolder_selected), mw);
	
	return priv->ifoldersScrolledWindow;
}

static GtkListStore *
create_store (IFAMainWindow *mw)
{
	GtkListStore *store;
	
	store = gtk_list_store_new (NUM_COLS, G_TYPE_POINTER, G_TYPE_STRING, GDK_TYPE_PIXBUF);
	
	gtk_tree_sortable_set_default_sort_func (GTK_TREE_SORTABLE (store), (GtkTreeIterCompareFunc)sort_func, mw, NULL);
	gtk_tree_sortable_set_sort_column_id (GTK_TREE_SORTABLE (store), GTK_TREE_SORTABLE_DEFAULT_SORT_COLUMN_ID, GTK_SORT_ASCENDING);
	
	return store;
}

static gint
sort_func (GtkTreeModel *model, GtkTreeIter *a, GtkTreeIter *b, IFAMainWindow *mw)
{
	iFolder *ifolder_a, *ifolder_b;
	const gchar *name_a, *name_b;
	
	gtk_tree_model_get (model, a, 0, &ifolder_a, -1);
	gtk_tree_model_get (model, b, 0, &ifolder_b, -1);
	
	name_a = ifolder_get_name (ifolder_a);
	name_b = ifolder_get_name (ifolder_b);
	
	return g_utf8_collate (name_a, name_b);
}

static GtkWidget *
create_status_bar (IFAMainWindow *mw)
{
	IFAMainWindowPrivate *priv = IFA_MAIN_WINDOW_GET_PRIVATE (mw);
	
	priv->mainStatusBar = gtk_statusbar_new ();
	gtk_statusbar_set_has_resize_grip (GTK_STATUSBAR (priv->mainStatusBar), TRUE);
	update_status (mw, _("Idle..."));
	
	return priv->mainStatusBar;
}

static void
update_status (IFAMainWindow *mw, const gchar *message)
{
	IFAMainWindowPrivate *priv = IFA_MAIN_WINDOW_GET_PRIVATE (mw);

	gtk_statusbar_pop (GTK_STATUSBAR (priv->mainStatusBar), priv->mainStatusBarContext);
	gtk_statusbar_push (GTK_STATUSBAR (priv->mainStatusBar), priv->mainStatusBarContext, message);
}

static void
on_create_ifolder (GtkMenuItem *menuitem, IFAMainWindow *mw)
{
	add_ifolder_handler (NULL, mw);
}

static void
on_open_ifolder (GtkMenuItem *menuitem, IFAMainWindow *mw)
{
	open_ifolder_handler (NULL, mw);
}

static void
on_share_ifolder (GtkMenuItem *menuitem, IFAMainWindow *mw)
{
	share_ifolder_handler (NULL, mw);
}

static void
on_sync_ifolder_now (GtkMenuItem *menuitem, IFAMainWindow *mw)
{
	synchronize_now_handler (NULL, mw);
}

static void
on_disconnect_ifolder (GtkMenuItem *menuitem, IFAMainWindow *mw)
{
	disconnect_ifolder_handler (NULL, mw);
}

static void
on_show_ifolder_properties (GtkMenuItem *menuitem, IFAMainWindow *mw)
{
	view_ifolder_properties_handler (NULL, mw);
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
	IFAMainWindowPrivate *priv = IFA_MAIN_WINDOW_GET_PRIVATE (mw);
	
	g_debug ("IFAMainWindow::on_refresh()");

	/* Clear out the store and rebuild it */
	gtk_list_store_clear (priv->ifoldersListStore);
	
	populate_store (mw);
}

static void
on_sync_log_menu_item (GtkMenuItem *menuitem, IFAMainWindow *mw)
{
	g_message ("FIXME: Implement IFAMainWindow::on_sync_log_menu_item()");
}

static void
on_help (GtkMenuItem *menuitem, IFAMainWindow *mw)
{
	ifa_show_help (IFA_HELP_MAIN_PAGE);
}

static void
on_about (GtkMenuItem *menuitem, IFAMainWindow *mw)
{
	ifa_show_about ();
}

static void
on_search_entry_changed (GtkEntry *entry, IFAMainWindow *mw)
{
	g_message ("FIXME: Implement IFAMainWindow::on_search_entry_changed()");
}

static void
on_cancel_search_button (GtkButton *widget, IFAMainWindow *mw)
{
	g_message ("FIXME: Implement IFAMainWindow::on_cancel_search_button()");
}

static void
add_ifolder_handler (GtkButton *widget, IFAMainWindow *mw)
{
	g_message ("FIXME: Implement IFAMainWindow::add_ifolder_handler()");
}

static void
open_ifolder_handler (GtkButton *widget, IFAMainWindow *mw)
{
	g_message ("FIXME: Implement IFAMainWindow::open_ifolder_handler()");
}

static void
synchronize_now_handler (GtkButton *widget, IFAMainWindow *mw)
{
	g_message ("FIXME: Implement IFAMainWindow::synchronize_now_handler()");
}

static void
share_ifolder_handler (GtkButton *widget, IFAMainWindow *mw)
{
	g_message ("FIXME: Implement IFAMainWindow::share_ifolder_handler()");
}

static void
disconnect_ifolder_handler (GtkButton *widget, IFAMainWindow *mw)
{
	g_message ("FIXME: Implement IFAMainWindow::disconnect_ifolder_handler()");
}

static void
view_ifolder_properties_handler (GtkButton *widget, IFAMainWindow *mw)
{
	g_message ("FIXME: Implement IFAMainWindow::view_ifolder_properties_handler()");
}

static gboolean
ifolder_filter_func (GtkTreeModel *model, GtkTreeIter *iter, GValue *value, gint column, IFAMainWindow *mw)
{
	g_message ("FIXME: Implement IFAMainWindow::ifolder_filter_func()");
	return TRUE;
}

static void
on_ifolder_activated (GtkIconView *iconview, GtkTreePath *path, IFAMainWindow *mw)
{
	open_ifolder_handler (NULL, mw);
}

static void
on_ifolder_selected (GtkIconView *iconview, IFAMainWindow *mw)
{
	update_sensitivity (mw);
}

static iFolder *
get_selected_ifolder (IFAMainWindow *mw)
{
	IFAMainWindowPrivate *priv;
	iFolder *ifolder = NULL;
	GList *selected_items;
	GtkTreePath *path;
	GtkTreeIter iter;
	
	priv = IFA_MAIN_WINDOW_GET_PRIVATE (mw);
	
	selected_items = gtk_icon_view_get_selected_items (GTK_ICON_VIEW (priv->ifoldersIconView));
	if (selected_items)
	{
		if (g_list_length (selected_items) != 1)
			goto one_item_not_selected;
		
		path = (GtkTreePath *)selected_items->data;
		if (gtk_tree_model_get_iter (priv->ifoldersListStoreFilter, &iter, path))
			gtk_tree_model_get (priv->ifoldersListStoreFilter, &iter, 0, &ifolder, -1);
		
one_item_not_selected:
		/* Free the list */
		g_list_foreach (selected_items, (GFunc)gtk_tree_path_free, NULL);
		g_list_free (selected_items);
	}
	
	return ifolder;
}

static void
update_sensitivity (IFAMainWindow *mw)
{
	iFolder *ifolder;
	
	ifolder = get_selected_ifolder (mw);
	update_actions_sensitivity (ifolder, mw);
	update_menu_sensitivity (ifolder, mw);
}

static void
update_actions_sensitivity (iFolder *ifolder, IFAMainWindow *mw)
{
	IFAMainWindowPrivate *priv = IFA_MAIN_WINDOW_GET_PRIVATE (mw);
	
	if (ifolder == NULL)
	{
		gtk_widget_set_sensitive (priv->openButton, FALSE);
		gtk_widget_set_sensitive (priv->synchronizeNowButton, FALSE);
		gtk_widget_set_sensitive (priv->shareButton, FALSE);
		gtk_widget_set_sensitive (priv->disconnectiFolderButton, FALSE);
		gtk_widget_set_sensitive (priv->viewiFolderPropertiesButton, FALSE);
	}
	else
	{
		gtk_widget_set_sensitive (priv->openButton, TRUE);
		gtk_widget_set_sensitive (priv->synchronizeNowButton, TRUE);
		gtk_widget_set_sensitive (priv->shareButton, TRUE);
		gtk_widget_set_sensitive (priv->disconnectiFolderButton, TRUE);
		gtk_widget_set_sensitive (priv->viewiFolderPropertiesButton, TRUE);
	}
}

static void
update_menu_sensitivity (iFolder *ifolder, IFAMainWindow *mw)
{
	IFAMainWindowPrivate *priv = IFA_MAIN_WINDOW_GET_PRIVATE (mw);
	
	if (ifolder == NULL)
	{
		gtk_widget_set_sensitive (priv->disconnectMenuItem, FALSE);
		gtk_widget_set_sensitive (priv->shareMenuItem, FALSE);
		gtk_widget_set_sensitive (priv->openMenuItem, FALSE);
		gtk_widget_set_sensitive (priv->syncNowMenuItem, FALSE);
		gtk_widget_set_sensitive (priv->propMenuItem, FALSE);
	}
	else
	{
		gtk_widget_set_sensitive (priv->disconnectMenuItem, TRUE);
		gtk_widget_set_sensitive (priv->shareMenuItem, TRUE);
		gtk_widget_set_sensitive (priv->openMenuItem, TRUE);
		gtk_widget_set_sensitive (priv->syncNowMenuItem, TRUE);
		gtk_widget_set_sensitive (priv->propMenuItem, TRUE);
	}
}

static void
populate_store (IFAMainWindow *mw)
{
	GSList *domains, *domainPtr;
	GSList *ifolders, *ifolderPtr;
	iFolderDomain *domain;
	iFolder *ifolder;
	GError *err = NULL;
	GtkWidget *errDialog;
	guint index;
	GtkTreeIter iter;
	gchar *tmpStr;
	

	IFAMainWindowPrivate *priv = IFA_MAIN_WINDOW_GET_PRIVATE (mw);

	g_debug ("FIXME: IMPLEMENT IFAMainWindow::populate_store()");

	domains = ifolder_client_get_all_domains (ifolder_client, &err);
	if (err)
	{
		errDialog = gtk_message_dialog_new (GTK_WINDOW (mw), GTK_DIALOG_DESTROY_WITH_PARENT, GTK_MESSAGE_ERROR, GTK_BUTTONS_CLOSE, err->message);
		g_clear_error (&err);
		gtk_dialog_run (GTK_DIALOG (errDialog));
		gtk_widget_destroy (errDialog);
		return;
	}
	
	if (domains == NULL)
		return;

g_debug ("# of domains: %d", g_slist_length (domains));
	
	for (domainPtr = domains; domainPtr != NULL; domainPtr = g_slist_next (domainPtr))
	{
		domain = IFOLDER_DOMAIN (domainPtr->data);
		
		index = 0;
		ifolders = ifolder_domain_get_connected_ifolders (domain, index, 10, &err);
		if (err)
		{
			errDialog = gtk_message_dialog_new (GTK_WINDOW (mw), GTK_DIALOG_DESTROY_WITH_PARENT, GTK_MESSAGE_ERROR, GTK_BUTTONS_CLOSE, err->message);
			g_clear_error (&err);
			gtk_dialog_run (GTK_DIALOG (errDialog));
			gtk_widget_destroy (errDialog);
			continue;
		}
		index = g_slist_length (ifolders);
g_debug ("# of ifolders in '%s': %d", ifolder_domain_get_name (domain), index);

		while (ifolders != NULL)
		{
			for (ifolderPtr = ifolders; ifolderPtr != NULL; ifolderPtr = g_slist_next (ifolderPtr))
			{
				ifolder = IFOLDER (ifolderPtr->data);
				
				tmpStr = g_markup_printf_escaped ("<span size=\"large\">%s</span>\n<span foreground=\"#CCCCCC\">%s\n%s</span>",
												  ifolder_get_name (ifolder),
												  ifolder_get_local_path (ifolder),
												  "FIXME: need to figure out how to get the status here");

				gtk_list_store_append (priv->ifoldersListStore, &iter);
				gtk_list_store_set (priv->ifoldersListStore, &iter,
									COL_IFOLDER, ifolder,
									COL_DISPLAY_NAME, tmpStr,
									COL_PIXBUF, OK_PIXBUF,
									-1);

				g_free (tmpStr);
			}
			
			g_slist_free (ifolders);

			ifolders = ifolder_domain_get_connected_ifolders (domain, index, 10, &err);
			if (err)
			{
				errDialog = gtk_message_dialog_new (GTK_WINDOW (mw), GTK_DIALOG_DESTROY_WITH_PARENT, GTK_MESSAGE_ERROR, GTK_BUTTONS_CLOSE, err->message);
				g_clear_error (&err);
				gtk_dialog_run (GTK_DIALOG (errDialog));
				gtk_widget_destroy (errDialog);
				break;
			}
			index = g_slist_length (ifolders);
		}
	}
	
	g_slist_free (domains);
}
