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

#include "ifolder-prop-dialog.h"

/*@todo Remove this when gettext is added */
#define _

extern iFolderClient *ifolder_client;

typedef struct _IFAiFolderPropDialogPrivate IFAiFolderPropDialogPrivate;
struct _IFAiFolderPropDialogPrivate 
{
	GtkWidget 		*parentWindow;

	gboolean		realized;
	gboolean		controlKeyPressed;
	
	GtkWidget		*notebook;
	
	/**
	 * General Page
	 */
	GtkWidget		*generalPage;
	
	/**
	 * Sharing Page
	 */
	GtkWidget		*sharingPage;
	
	/**
	 * History Page
	 */
	GtkWidget		*historyPage;
	
	/**
	 * Unsynchronized Items Page
	 */
	GtkWidget		*unsynchronizedItemsPage;
};

static GdkPixbuf *WINDOW_ICON		= NULL;

#define IFA_IFOLDER_PROP_DIALOG_GET_PRIVATE(obj) (G_TYPE_INSTANCE_GET_PRIVATE ((obj), IFA_IFOLDER_PROP_DIALOG_TYPE, IFAiFolderPropDialogPrivate))

static void                 ifa_ifolder_prop_dialog_finalize       (GObject            *object);

G_DEFINE_TYPE (IFAiFolderPropDialog, ifa_ifolder_prop_dialog, GTK_TYPE_DIALOG)

/* Forward Declarations */

static void load_pixbufs (void);

static void                 ifa_ifolder_prop_dialog_get_property   (GObject            *object,
							     guint               prop_id,
							     GValue             *value,
							     GParamSpec         *pspec);
static void                 ifa_ifolder_prop_dialog_set_property   (GObject            *object,
							     guint               prop_id,
							     const GValue       *value,
							     GParamSpec         *pspec);
static void                 ifa_ifolder_prop_dialog_style_set      (GtkWidget          *widget,
			                                     GtkStyle           *previous_style);

static void on_realize (GtkWidget *widget, IFAiFolderPropDialog *ipd);
static void on_hide (GtkWidget *widget, IFAiFolderPropDialog *ipd);
static gboolean on_key_press (GtkWidget *widget, GdkEventKey *key, IFAiFolderPropDialog *ipd);

static GtkWidget * create_notebook (IFAiFolderPropDialog *ipd);
static void create_buttons (IFAiFolderPropDialog *ipd);

static GtkWidget * create_general_page (IFAiFolderPropDialog *ipd);
static GtkWidget * create_sharing_page (IFAiFolderPropDialog *ipd);
static GtkWidget * create_history_page (IFAiFolderPropDialog *ipd);
static GtkWidget * create_unsynchronized_items_page (IFAiFolderPropDialog *ipd);

static void
ifa_ifolder_prop_dialog_class_init (IFAiFolderPropDialogClass *klass)
{
	GObjectClass *object_class;
	GtkWidgetClass *widget_class;
	GtkWindowClass *window_class;
	  
	g_message ("ifa_ifolder_prop_dialog_class_init()");
		
	object_class = (GObjectClass *)klass;
	widget_class = (GtkWidgetClass *)klass;
	window_class = (GtkWindowClass *)klass;
		
	object_class->set_property = ifa_ifolder_prop_dialog_set_property;
	object_class->get_property = ifa_ifolder_prop_dialog_get_property;
	
	object_class->finalize = ifa_ifolder_prop_dialog_finalize;
	
	widget_class->style_set = ifa_ifolder_prop_dialog_style_set;
	
	load_pixbufs ();
	
	g_type_class_add_private (object_class, sizeof (IFAiFolderPropDialogPrivate));
}

static void
ifa_ifolder_prop_dialog_init (IFAiFolderPropDialog *ipd)
{
	IFAiFolderPropDialogPrivate *priv;
	GtkWidget *vbox;
	
	g_message ("ifa_ifolder_prop_dialog_init()");

	/* Data */
	priv = IFA_IFOLDER_PROP_DIALOG_GET_PRIVATE (ipd);
	ipd->private_data = priv;
	
	priv->realized = FALSE;
	
//	gtk_window_set_default_size (GTK_WINDOW (ipd), 500, 400);
//	gtk_widget_set_size_request (GTK_WIDGET (ipd), 500, 400);
	
	gtk_window_set_icon (GTK_WINDOW (ipd), WINDOW_ICON);
	gtk_window_set_position (GTK_WINDOW (ipd), GTK_WIN_POS_CENTER_ON_PARENT);

	/* Widgets */
	gtk_widget_push_composite_child ();
	
	vbox = gtk_vbox_new (FALSE, 12);
	gtk_box_pack_start (GTK_BOX (GTK_DIALOG (ipd)->vbox), vbox, TRUE, TRUE, 0);
	gtk_container_set_border_width (GTK_CONTAINER (vbox), IFA_DEFAULT_BORDER_WIDTH);

	gtk_box_pack_start (GTK_BOX (vbox), create_notebook (ipd), FALSE, FALSE, 0);

	gtk_widget_show (vbox);
	
	gtk_widget_pop_composite_child ();

	create_buttons (ipd);
}

static void
ifa_ifolder_prop_dialog_finalize (GObject *object)
{
	IFAiFolderPropDialog *ipd = IFA_IFOLDER_PROP_DIALOG (object);
	IFAiFolderPropDialogPrivate *priv = IFA_IFOLDER_PROP_DIALOG_GET_PRIVATE (ipd);

	g_debug ("IFAiFolderPropDialog::ifa_ifolder_prop_dialog_finalize()");

	
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
	
	G_OBJECT_CLASS (ifa_ifolder_prop_dialog_parent_class)->finalize (object);
}

static void
ifa_ifolder_prop_dialog_set_property (GObject      *object, 
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
ifa_ifolder_prop_dialog_get_property (GObject    *object, 
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
ifa_ifolder_prop_dialog_style_set (GtkWidget *widget,
			    GtkStyle  *previous_style)
{
/*
  if (GTK_WIDGET_CLASS (ifa_ifolder_prop_dialog_parent_class)->style_set)
    GTK_WIDGET_CLASS (ifa_ifolder_prop_dialog_parent_class)->style_set (widget, previous_style);

  dialog_style_set (widget, previous_style, NULL);
*/
}

GtkWidget *
ifa_ifolder_prop_dialog_new (GtkWindow *parent, iFolder *ifolder)
{
	IFAiFolderPropDialogPrivate *priv;
	gchar *tmpStr;
	
	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), NULL);

	g_message ("ifa_ifolder_prop_dialog_new()...calling g_object_new()");
	IFAiFolderPropDialog *ipd = IFA_IFOLDER_PROP_DIALOG (g_object_new (IFA_IFOLDER_PROP_DIALOG_TYPE, NULL));
	
	g_message ("done calling g_object_new()");
	
	priv = IFA_IFOLDER_PROP_DIALOG_GET_PRIVATE(ipd);

	if (parent != NULL)
	{
		gtk_window_set_transient_for (GTK_WINDOW (ipd), parent);
		gtk_window_set_destroy_with_parent (GTK_WINDOW (ipd), TRUE);
		gtk_window_set_modal (GTK_WINDOW (ipd), TRUE);
	}

	tmpStr = g_markup_printf_escaped ("%s %s", ifolder_get_name (ifolder), _("Properties"));
	gtk_window_set_title (GTK_WINDOW (ipd), tmpStr);
	g_free (tmpStr);
	
	gtk_dialog_set_has_separator (GTK_DIALOG (ipd), FALSE);
	gtk_window_set_resizable (GTK_WINDOW (ipd), FALSE);
	gtk_dialog_set_default_response (GTK_DIALOG (ipd), GTK_RESPONSE_OK);
	gtk_window_set_modal (GTK_WINDOW (ipd), TRUE);
	gtk_window_set_type_hint (GTK_WINDOW (ipd), GDK_WINDOW_TYPE_HINT_NORMAL);

/*
	g_signal_connect (ipd, "response", G_CALLBACK (on_ipd_response), ipd);
	g_signal_connect (ipd, "key-release-event", G_CALLBACK (on_key_release), ipd);
	g_signal_connect (ipd, "response", G_CALLBACK (on_response), ipd);
*/
	g_signal_connect (ipd, "realize", G_CALLBACK (on_realize), ipd);
	g_signal_connect (ipd, "hide", G_CALLBACK (on_hide), ipd);
	g_signal_connect (ipd, "key-press-event", G_CALLBACK (on_key_press), ipd);
	
	return GTK_WIDGET (ipd);
}

static void
on_realize (GtkWidget *widget, IFAiFolderPropDialog *ipd)
{
	IFAiFolderPropDialogPrivate *priv;
	
	g_debug ("IFAiFolderPropDialog::on_realize()");
	
	priv = IFA_IFOLDER_PROP_DIALOG_GET_PRIVATE (ipd);
	
	/* FIXME: Register for domain events that could affect this window */
	
	priv->realized = TRUE;
}

static void
on_hide (GtkWidget *widget, IFAiFolderPropDialog *ipd)
{
	IFAiFolderPropDialogPrivate *priv = IFA_IFOLDER_PROP_DIALOG_GET_PRIVATE (ipd);
	g_debug ("IFAiFolderPropDialog::on_hide()");

	gtk_widget_destroy (GTK_WIDGET (ipd));
}

static gboolean
on_key_press (GtkWidget *widget, GdkEventKey *key, IFAiFolderPropDialog *ipd)
{
	switch (key->keyval)
	{
		case GDK_F1:
///			on_help (NULL, ipd);
			break;
		default:
			return FALSE;	/* propogate the event on to other key-press handlers */
			break;
	}
	
	return TRUE; /* don't propogate the event on to other key-press handlers */
}

static GtkWidget *
create_notebook (IFAiFolderPropDialog *ipd)
{
	IFAiFolderPropDialogPrivate *priv = IFA_IFOLDER_PROP_DIALOG_GET_PRIVATE (ipd);
	
	priv->notebook = gtk_notebook_new ();
	
	gtk_notebook_append_page (GTK_NOTEBOOK (priv->notebook), create_general_page (ipd), gtk_label_new (_("General")));
	gtk_notebook_append_page (GTK_NOTEBOOK (priv->notebook), create_sharing_page (ipd), gtk_label_new (_("Sharing")));
	gtk_notebook_append_page (GTK_NOTEBOOK (priv->notebook), create_history_page (ipd), gtk_label_new (_("History")));
	gtk_notebook_append_page (GTK_NOTEBOOK (priv->notebook), create_unsynchronized_items_page (ipd), gtk_label_new (_("Unsynchronized Items")));
	gtk_widget_show_all (priv->notebook);
	
	return priv->notebook;
}

static GtkWidget *
create_general_page (IFAiFolderPropDialog *ipd)
{
	GtkWidget *vbox;
	IFAiFolderPropDialogPrivate *priv = IFA_IFOLDER_PROP_DIALOG_GET_PRIVATE (ipd);
	
	vbox = gtk_vbox_new (FALSE, 0);
	gtk_box_pack_start (GTK_BOX (vbox), gtk_label_new ("FIXME: Implement me!"), TRUE, TRUE, 0);

	return vbox;	
}

static GtkWidget *
create_sharing_page (IFAiFolderPropDialog *ipd)
{
	GtkWidget *vbox;
	IFAiFolderPropDialogPrivate *priv = IFA_IFOLDER_PROP_DIALOG_GET_PRIVATE (ipd);
	
	vbox = gtk_vbox_new (FALSE, 0);
	gtk_box_pack_start (GTK_BOX (vbox), gtk_label_new ("FIXME: Implement me!"), TRUE, TRUE, 0);

	return vbox;	
}

static GtkWidget *
create_history_page (IFAiFolderPropDialog *ipd)
{
	GtkWidget *vbox;
	IFAiFolderPropDialogPrivate *priv = IFA_IFOLDER_PROP_DIALOG_GET_PRIVATE (ipd);
	
	vbox = gtk_vbox_new (FALSE, 0);
	gtk_box_pack_start (GTK_BOX (vbox), gtk_label_new ("FIXME: Implement me!"), TRUE, TRUE, 0);

	return vbox;	
}

static GtkWidget *
create_unsynchronized_items_page (IFAiFolderPropDialog *ipd)
{
	GtkWidget *vbox;
	IFAiFolderPropDialogPrivate *priv = IFA_IFOLDER_PROP_DIALOG_GET_PRIVATE (ipd);
	
	vbox = gtk_vbox_new (FALSE, 0);
	gtk_box_pack_start (GTK_BOX (vbox), gtk_label_new ("FIXME: Implement me!"), TRUE, TRUE, 0);

	return vbox;	
}

static void
create_buttons (IFAiFolderPropDialog *ipd)
{
	gtk_dialog_add_button (GTK_DIALOG (ipd), GTK_STOCK_CLOSE, GTK_RESPONSE_OK);
}

