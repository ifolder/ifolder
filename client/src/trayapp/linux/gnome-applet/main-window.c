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

/*@todo Remove this when gettext is added */
#define _

extern iFolderClient *ifolder_client;

typedef struct _IFAMainWindowPrivate IFAMainWindowPrivate;
struct _IFAMainWindowPrivate 
{
	GtkWidget 	*parentWindow;

	gboolean	realized;
	gboolean	controlKeyPressed;
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

static void close_window (IFAMainWindow *mw);

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
	
	g_message ("ifa_main_window_init()");

	/* Data */
	priv = IFA_MAIN_WINDOW_GET_PRIVATE (mw);
	mw->private_data = priv;
	
	priv->realized = FALSE;

	/* Widgets */
//	gtk_widget_push_composite_child ();
	

//	gtk_widget_pop_composite_child ();
}

static void
ifa_main_window_finalize (GObject *object)
{
	IFAMainWindow *mw = IFA_MAIN_WINDOW (object);
	IFAMainWindowPrivate *priv = IFA_MAIN_WINDOW_GET_PRIVATE (mw);

	g_debug ("IFAMainWindow::ifa_main_window_finalize()");

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

