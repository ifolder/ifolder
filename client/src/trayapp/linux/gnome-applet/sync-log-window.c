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

#include "sync-log-window.h"

/*@todo Remove this when gettext is added */
#define _

extern iFolderClient *ifolder_client;

typedef struct _IFASyncLogWindowPrivate IFASyncLogWindowPrivate;
struct _IFASyncLogWindowPrivate 
{
	GtkWidget 		*parentWindow;

	gboolean		realized;
	gboolean		controlKeyPressed;
	
	GtkWidget		*menuBar;
	GtkWidget		*saveMenuItem;
	GtkWidget		*clearMenuItem;
	GtkWidget		*closeMenuItem;
	
	GtkWidget		*helpMenuItem;
	
	GtkWidget		*toolbar;
	GtkTooltips		*tooltips;
	GtkToolItem		*saveToolButton;
	GtkToolItem		*clearToolButton;
	
	GtkTextBuffer	*textBuffer;
	GtkWidget		*textView;
};

static IFASyncLogWindow *sync_log_window = NULL;

static GdkPixbuf *WINDOW_ICON		= NULL;

#define IFA_SYNC_LOG_WINDOW_GET_PRIVATE(obj) (G_TYPE_INSTANCE_GET_PRIVATE ((obj), IFA_SYNC_LOG_WINDOW_TYPE, IFASyncLogWindowPrivate))

static void                 ifa_sync_log_window_finalize       (GObject            *object);

G_DEFINE_TYPE (IFASyncLogWindow, ifa_sync_log_window, GTK_TYPE_WINDOW)

/* Forward Declarations */

static GtkWidget * ifa_sync_log_window_new (GtkWindow *parent);

static void load_pixbufs (void);

static void                 ifa_sync_log_window_get_property   (GObject            *object,
							     guint               prop_id,
							     GValue             *value,
							     GParamSpec         *pspec);
static void                 ifa_sync_log_window_set_property   (GObject            *object,
							     guint               prop_id,
							     const GValue       *value,
							     GParamSpec         *pspec);
static void                 ifa_sync_log_window_style_set      (GtkWidget          *widget,
			                                     GtkStyle           *previous_style);

static void on_realize (GtkWidget *widget, IFASyncLogWindow *slw);
static void on_hide (GtkWidget *widget, IFASyncLogWindow *slw);
static gboolean on_key_press (GtkWidget *widget, GdkEventKey *key, IFASyncLogWindow *slw);

static GtkWidget * create_widgets (IFASyncLogWindow *slw);
static GtkWidget * create_menu (IFASyncLogWindow *slw);
static GtkWidget * create_toolbar (IFASyncLogWindow *slw);
static GtkWidget * create_content_area (IFASyncLogWindow *slw);

/* File Menu Handlers */
static void on_save (GtkMenuItem *menuitem, IFASyncLogWindow *slw);
static void on_clear (GtkMenuItem *menuitem, IFASyncLogWindow *slw);
static void on_close (GtkMenuItem *menuitem, IFASyncLogWindow *slw);
static void on_help (GtkMenuItem *menuitem, IFASyncLogWindow *slw);

/* Toolbar Button Handlers */
static void on_save_toolitem_clicked (GtkToolButton *toolbutton, IFASyncLogWindow *slw);
static void on_clear_toolitem_clicked (GtkToolButton *toolbutton, IFASyncLogWindow *slw);

static void
ifa_sync_log_window_class_init (IFASyncLogWindowClass *klass)
{
	GObjectClass *object_class;
	GtkWidgetClass *widget_class;
	GtkWindowClass *window_class;
	  
	g_message ("ifa_sync_log_window_class_init()");
		
	object_class = (GObjectClass *)klass;
	widget_class = (GtkWidgetClass *)klass;
	window_class = (GtkWindowClass *)klass;
		
	object_class->set_property = ifa_sync_log_window_set_property;
	object_class->get_property = ifa_sync_log_window_get_property;
	
	object_class->finalize = ifa_sync_log_window_finalize;
	
	widget_class->style_set = ifa_sync_log_window_style_set;
	
	load_pixbufs ();
	
	g_type_class_add_private (object_class, sizeof (IFASyncLogWindowPrivate));
}

static void
ifa_sync_log_window_init (IFASyncLogWindow *slw)
{
	IFASyncLogWindowPrivate *priv;
	
	g_message ("ifa_sync_log_window_init()");

	/* Data */
	priv = IFA_SYNC_LOG_WINDOW_GET_PRIVATE (slw);
	slw->private_data = priv;
	
	priv->realized = FALSE;
	
	gtk_window_set_default_size (GTK_WINDOW (slw), 500, 400);
//	gtk_widget_set_size_request (GTK_WIDGET (slw), 500, 400);
	
	gtk_window_set_icon (GTK_WINDOW (slw), WINDOW_ICON);
	gtk_window_set_position (GTK_WINDOW (slw), GTK_WIN_POS_CENTER);

	/* Widgets */
	gtk_widget_push_composite_child ();

	gtk_container_add (GTK_CONTAINER (slw), create_widgets (slw));
	
	gtk_widget_pop_composite_child ();
}

static void
ifa_sync_log_window_finalize (GObject *object)
{
	IFASyncLogWindow *slw = IFA_SYNC_LOG_WINDOW (object);
	IFASyncLogWindowPrivate *priv = IFA_SYNC_LOG_WINDOW_GET_PRIVATE (slw);

	g_debug ("IFASyncLogWindow::ifa_sync_log_window_finalize()");

	
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
	
	G_OBJECT_CLASS (ifa_sync_log_window_parent_class)->finalize (object);
}

static void
ifa_sync_log_window_set_property (GObject      *object, 
			       guint         prop_id, 
			       const GValue *value, 
			       GParamSpec   *pspec)
{
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
}

static void
ifa_sync_log_window_get_property (GObject    *object, 
			       guint       prop_id, 
			       GValue     *value, 
			       GParamSpec *pspec)
{
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
ifa_sync_log_window_style_set (GtkWidget *widget,
			    GtkStyle  *previous_style)
{
/*
  if (GTK_WIDGET_CLASS (ifa_sync_log_window_parent_class)->style_set)
    GTK_WIDGET_CLASS (ifa_sync_log_window_parent_class)->style_set (widget, previous_style);

  dialog_style_set (widget, previous_style, NULL);
*/
}

GtkWidget *
ifa_sync_log_window_new (GtkWindow *parent)
{
	IFASyncLogWindowPrivate *priv;
	g_message ("ifa_sync_log_window_new()...calling g_object_new()");
	IFASyncLogWindow *slw = IFA_SYNC_LOG_WINDOW (g_object_new (IFA_SYNC_LOG_WINDOW_TYPE, NULL));
	
	g_message ("done calling g_object_new()");
	
	priv = IFA_SYNC_LOG_WINDOW_GET_PRIVATE(slw);

	if (parent != NULL)
	{
		gtk_window_set_transient_for (GTK_WINDOW (slw), parent);
		gtk_window_set_destroy_with_parent (GTK_WINDOW (slw), TRUE);
		gtk_window_set_modal (GTK_WINDOW (slw), FALSE);
	}
	
	gtk_window_set_title (GTK_WINDOW (slw), _("iFolder Synchronization Log"));
	
	gtk_window_set_resizable (GTK_WINDOW (slw), TRUE);
	gtk_window_set_modal (GTK_WINDOW (slw), FALSE);
	gtk_window_set_type_hint (GTK_WINDOW (slw), GDK_WINDOW_TYPE_HINT_NORMAL);

/*
	g_signal_connect (slw, "response", G_CALLBACK (on_slw_response), slw);
	g_signal_connect (slw, "key-release-event", G_CALLBACK (on_key_release), slw);
	g_signal_connect (slw, "response", G_CALLBACK (on_response), slw);
*/
	g_signal_connect (slw, "realize", G_CALLBACK (on_realize), slw);
	g_signal_connect (slw, "hide", G_CALLBACK (on_hide), slw);
	g_signal_connect (slw, "key-press-event", G_CALLBACK (on_key_press), slw);
	
	return GTK_WIDGET (slw);
}

IFASyncLogWindow *
ifa_get_sync_log_window ()
{
	return sync_log_window;
}

void
ifa_show_sync_log_window()
{
	IFASyncLogWindowPrivate *priv;

	if (sync_log_window == NULL)
		sync_log_window = IFA_SYNC_LOG_WINDOW (ifa_sync_log_window_new (NULL));
	
	if (!sync_log_window)
		return;
	
	priv = IFA_SYNC_LOG_WINDOW_GET_PRIVATE (sync_log_window);
	
	if (priv->realized)
		gtk_window_present (GTK_WINDOW (sync_log_window));
	else
		gtk_widget_show_all (GTK_WIDGET (sync_log_window));
}

void
ifa_hide_sync_log_window()
{
	if (sync_log_window == NULL)
		return;
	
	gtk_widget_hide (GTK_WIDGET (sync_log_window));
}

static void
on_realize (GtkWidget *widget, IFASyncLogWindow *slw)
{
	IFASyncLogWindowPrivate *priv;
	
	g_debug ("IFASyncLogWindow::on_realize()");
	
	priv = IFA_SYNC_LOG_WINDOW_GET_PRIVATE (slw);
	
	/* FIXME: Register for domain events that could affect this window */
	
	priv->realized = TRUE;
}

static void
on_hide (GtkWidget *widget, IFASyncLogWindow *slw)
{
	g_debug ("IFASyncLogWindow::on_hide()");

	gtk_widget_destroy (GTK_WIDGET (slw));
	sync_log_window = NULL;
}

static gboolean
on_key_press (GtkWidget *widget, GdkEventKey *key, IFASyncLogWindow *slw)
{
	switch (key->keyval)
	{
		case GDK_F1:
			on_help (NULL, slw);
			break;
		default:
			return FALSE;	/* propogate the event on to other key-press handlers */
			break;
	}
	
	return TRUE; /* don't propogate the event on to other key-press handlers */
}

static GtkWidget *
create_widgets (IFASyncLogWindow *slw)
{
	GtkWidget *vbox;
	
	vbox = gtk_vbox_new (FALSE, 0);
	
	/* Create the menu bar */
	gtk_box_pack_start (GTK_BOX (vbox), create_menu (slw), FALSE, FALSE, 0);
	
	/* Create the toolbar */
	gtk_box_pack_start (GTK_BOX (vbox), create_toolbar (slw), FALSE, FALSE, 0);
	
	/* Create the main content area */
	gtk_box_pack_start (GTK_BOX (vbox), create_content_area (slw), TRUE, TRUE, 0);
	
	return vbox;
}

static GtkWidget *
create_menu (IFASyncLogWindow *slw)
{
	GtkAccelGroup *accelGroup;
	GtkWidget *fileMenu;
	GtkWidget *fileMenuItem;
	
	GtkWidget *helpMenu;
	GtkWidget *helpMenuItem0;

	IFASyncLogWindowPrivate *priv = IFA_SYNC_LOG_WINDOW_GET_PRIVATE (slw);
	
	priv->menuBar = gtk_menu_bar_new ();
	
	accelGroup = gtk_accel_group_new ();
	gtk_window_add_accel_group (GTK_WINDOW (slw), accelGroup);
	
	/**
	 * File Menu
	 */
	fileMenu = gtk_menu_new ();
	
	/* Save */
	priv->saveMenuItem = gtk_image_menu_item_new_from_stock (GTK_STOCK_SAVE, accelGroup);
	gtk_menu_shell_append (GTK_MENU_SHELL (fileMenu), priv->saveMenuItem);
	g_signal_connect (priv->saveMenuItem, "activate", G_CALLBACK (on_save), slw);
	
	/* Clear */
	priv->clearMenuItem = gtk_image_menu_item_new_from_stock (GTK_STOCK_CLEAR, accelGroup);
	gtk_menu_shell_append (GTK_MENU_SHELL (fileMenu), priv->clearMenuItem);
	g_signal_connect (priv->clearMenuItem, "activate", G_CALLBACK (on_clear), slw);

	/* Separator */
	gtk_menu_shell_append (GTK_MENU_SHELL (fileMenu), gtk_separator_menu_item_new ());
	
	/* Close */
	priv->closeMenuItem = gtk_image_menu_item_new_from_stock (GTK_STOCK_CLOSE, accelGroup);
	gtk_menu_shell_append (GTK_MENU_SHELL (fileMenu), priv->closeMenuItem);
	g_signal_connect (priv->closeMenuItem, "activate", G_CALLBACK (on_close), slw);

	fileMenuItem = gtk_menu_item_new_with_mnemonic (_("_File"));
	gtk_menu_item_set_submenu (GTK_MENU_ITEM (fileMenuItem), fileMenu);
	gtk_menu_shell_append (GTK_MENU_SHELL (priv->menuBar), fileMenuItem);

	/**
	 * Help Menu
	 */
	helpMenu = gtk_menu_new ();

	/* Help */
	priv->helpMenuItem = gtk_image_menu_item_new_from_stock (GTK_STOCK_HELP, accelGroup);
	gtk_menu_shell_append (GTK_MENU_SHELL (helpMenu), priv->helpMenuItem);
	g_signal_connect (priv->helpMenuItem, "activate", G_CALLBACK (on_help), slw);
	
	helpMenuItem0 = gtk_menu_item_new_with_mnemonic (_("_Help"));
	gtk_menu_item_set_submenu (GTK_MENU_ITEM (helpMenuItem0), helpMenu);
	gtk_menu_shell_append (GTK_MENU_SHELL (priv->menuBar), helpMenuItem0);

	return priv->menuBar;
}

static GtkWidget *
create_toolbar (IFASyncLogWindow *slw)
{
	IFASyncLogWindowPrivate *priv = IFA_SYNC_LOG_WINDOW_GET_PRIVATE (slw);
	
	priv->toolbar = gtk_toolbar_new ();
	priv->tooltips = gtk_tooltips_new ();
	
	/* Save Button */
	priv->saveToolButton = gtk_tool_button_new_from_stock (GTK_STOCK_SAVE);
	gtk_tool_item_set_tooltip (priv->saveToolButton, priv->tooltips, _("Save the synchronization log"), "Toolbar/Save");
	g_signal_connect (priv->saveToolButton, "clicked", G_CALLBACK (on_save_toolitem_clicked), slw);
	gtk_toolbar_insert (GTK_TOOLBAR (priv->toolbar), priv->saveToolButton, -1);
	
	/* Clear Button */
	priv->clearToolButton = gtk_tool_button_new_from_stock (GTK_STOCK_CLEAR);
	gtk_tool_item_set_tooltip (priv->clearToolButton, priv->tooltips, _("Clear the synchronization log"), "Toolbar/Clear");
	g_signal_connect (priv->clearToolButton, "clicked", G_CALLBACK (on_clear_toolitem_clicked), slw);
	gtk_toolbar_insert (GTK_TOOLBAR (priv->toolbar), priv->clearToolButton, -1);
	
	return priv->toolbar;
}

static GtkWidget *
create_content_area (IFASyncLogWindow *slw)
{
	GtkWidget *sw;
	IFASyncLogWindowPrivate *priv = IFA_SYNC_LOG_WINDOW_GET_PRIVATE (slw);
	
	sw = gtk_scrolled_window_new (NULL, NULL);
	gtk_scrolled_window_set_policy (GTK_SCROLLED_WINDOW (sw), GTK_POLICY_AUTOMATIC, GTK_POLICY_AUTOMATIC);
	
	priv->textBuffer = gtk_text_buffer_new (NULL);
	
	priv->textView = gtk_text_view_new_with_buffer (priv->textBuffer);
	gtk_container_add (GTK_CONTAINER (sw), priv->textView);
	
	gtk_text_view_set_editable (GTK_TEXT_VIEW (priv->textView), FALSE);
	gtk_text_view_set_cursor_visible (GTK_TEXT_VIEW (priv->textView), FALSE);
	gtk_text_view_set_left_margin (GTK_TEXT_VIEW (priv->textView), 12);
	gtk_text_view_set_right_margin (GTK_TEXT_VIEW (priv->textView), 12);
	gtk_text_view_set_pixels_above_lines (GTK_TEXT_VIEW (priv->textView), 12);
	gtk_text_view_set_pixels_below_lines (GTK_TEXT_VIEW (priv->textView), 12);
	gtk_text_view_set_wrap_mode (GTK_TEXT_VIEW (priv->textView), GTK_WRAP_NONE);
	
	return sw;
}

static void
on_save (GtkMenuItem *menuitem, IFASyncLogWindow *slw)
{
	g_message ("FIXME: Implement IFASyncLogWindow::on_save()");
}

static void
on_clear (GtkMenuItem *menuitem, IFASyncLogWindow *slw)
{
	g_message ("FIXME: Implement IFASyncLogWindow::on_clear()");
}

static void
on_close (GtkMenuItem *menuitem, IFASyncLogWindow *slw)
{
	gtk_widget_hide (GTK_WIDGET (slw));
}

static void
on_help (GtkMenuItem *menuitem, IFASyncLogWindow *slw)
{
	ifa_show_help (IFA_HELP_SYNC_LOG_PAGE);
}

static void
on_save_toolitem_clicked (GtkToolButton *toolbutton, IFASyncLogWindow *slw)
{
	on_save (NULL, slw);
}

static void
on_clear_toolitem_clicked (GtkToolButton *toolbutton, IFASyncLogWindow *slw)
{
	on_clear (NULL, slw);
}
